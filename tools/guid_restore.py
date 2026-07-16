"""
Repair Unity .meta GUID churn introduced by a commit that regenerated metas.

Restores the pre-churn GUID into the current .meta, but ONLY where provably safe:
  1. The meta's guid actually changed in the churn commit (old != new).
  2. The current on-disk meta still holds the churn guid (nobody changed it since).
  3. The old (pre-churn) meta and the current meta differ ONLY on the guid line
     -> importer settings identical -> internal fileIDs identical -> clean restore.
  4. No current meta already owns the old guid (no collision).
  5. Nothing in the project references the NEW guid (nothing would break).
  6. Something actually references the old guid (i.e. it is genuinely dangling).

Dry-run by default. Pass --apply to write.
"""
import re, subprocess, sys, pathlib, collections

REPO = pathlib.Path(__file__).resolve().parent.parent
BEFORE, AFTER = "31221ddd", "b0aec3c1"
GUID_RE = re.compile(rb"guid: ([a-f0-9]{32})")
REF_EXT = {".unity", ".prefab", ".asset", ".mat", ".controller", ".playable", ".overrideController"}
APPLY = "--apply" in sys.argv


def git_show(rev, path):
    p = subprocess.run(["git", "show", f"{rev}:{path}"], cwd=REPO,
                       capture_output=True)
    return p.stdout if p.returncode == 0 else None


def norm(b):
    return b.replace(b"\r\n", b"\n").lstrip(b"\xef\xbb\xbf")


def guid_of(b):
    m = re.search(rb"^guid: ([a-f0-9]{32})", norm(b), re.M)
    return m.group(1).decode() if m else None


print("Scanning project for guid ownership + references ...")
owned = {}          # guid -> meta path
refs = collections.defaultdict(list)   # guid -> [files referencing]

for f in REPO.rglob("*"):
    if not f.is_file():
        continue
    parts = f.parts
    if "Library" in parts or ".git" in parts or "Temp" in parts:
        continue
    try:
        if f.suffix == ".meta":
            head = f.open("rb").read(200)
            g = guid_of(head)
            if g:
                owned[g] = f
        elif f.suffix in REF_EXT:
            data = f.read_bytes()
            for g in set(GUID_RE.findall(data)):
                refs[g.decode()].append(f)
    except OSError:
        pass

print(f"  owned guids: {len(owned)}   referenced guids: {len(refs)}\n")

changed = subprocess.run(
    ["git", "diff", BEFORE, AFTER, "--name-only", "--", "*.meta"],
    cwd=REPO, capture_output=True, text=True).stdout.split("\n")

restore, skip = [], []
for path in filter(None, changed):
    old_b, new_b = git_show(BEFORE, path), git_show(AFTER, path)
    if not old_b or not new_b:
        continue
    old_g, new_g = guid_of(old_b), guid_of(new_b)
    if not old_g or not new_g or old_g == new_g:
        continue

    cur = REPO / path
    if not cur.exists():
        skip.append((path, old_g, "file gone"));  continue
    cur_b = cur.read_bytes()
    if guid_of(cur_b) != new_g:
        skip.append((path, old_g, "guid moved again since churn")); continue
    if norm(old_b).replace(old_g.encode(), b"") != norm(cur_b).replace(new_g.encode(), b""):
        skip.append((path, old_g, "meta differs beyond guid line")); continue
    if old_g in owned and owned[old_g] != cur:
        skip.append((path, old_g, f"collision: owned by {owned[old_g].name}")); continue
    if refs.get(new_g):
        skip.append((path, old_g, f"NEW guid referenced by {len(refs[new_g])} file(s)")); continue
    if not refs.get(old_g):
        skip.append((path, old_g, "old guid unreferenced (nothing broken)")); continue

    restore.append((path, old_g, new_g, len(refs[old_g])))

print(f"=== WILL RESTORE ({len(restore)}) ===")
for path, old_g, new_g, n in sorted(restore, key=lambda r: -r[3]):
    print(f"  {n:3d} refs  {new_g} -> {old_g}  {path}")

print(f"\n=== SKIPPED ({len(skip)}) ===")
for path, g, why in skip:
    print(f"  [{why}]  {pathlib.Path(path).name}")

if APPLY:
    for path, old_g, new_g, _ in restore:
        f = REPO / path
        f.write_bytes(f.read_bytes().replace(new_g.encode(), old_g.encode()))
    print(f"\nAPPLIED: rewrote {len(restore)} meta file(s).")
else:
    print("\n(dry run - pass --apply to write)")
