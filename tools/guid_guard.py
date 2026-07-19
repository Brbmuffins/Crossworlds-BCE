"""
Fail if a commit re-GUIDs a .meta that something still references.

Unity reassigns a .meta GUID when it hits a duplicate — e.g. an asset folder gets
copied/unzipped into Assets/ alongside a copy that already owns that GUID. The
importer settings stay byte-identical; only the guid line moves. Every scene and
prefab pointing at the old GUID silently dangles, and the person who caused it
usually never opens the affected scene, so it lands on someone else days later.

This has hit main three times (527fb6b7, b0aec3c1, da3711c6). The guard turns a
silent, delayed break into a loud, immediate one attributed to the right commit.

Usage:  python tools/guid_guard.py --base <rev> --head <rev>
Exit 0 = clean, 1 = a referenced GUID was reassigned.

Repair is the sibling script: tools/guid_restore.py (set BEFORE/AFTER, --apply).
"""
import argparse, collections, pathlib, re, subprocess, sys

REPO = pathlib.Path(__file__).resolve().parent.parent
GUID_RE = re.compile(rb"guid: ([a-f0-9]{32})")
REF_EXT = {".unity", ".prefab", ".asset", ".mat", ".controller", ".playable", ".overrideController"}
# Backups/ holds snapshot copies of scenes; a dangling ref in a stale backup is not
# a break worth failing the build over.
SKIP_DIRS = {"Library", ".git", "Temp", "Build", "build", "Logs", "obj", "Backups"}


def git(*args):
    p = subprocess.run(["git", *args], cwd=REPO, capture_output=True)
    return p.stdout if p.returncode == 0 else None


def norm(b):
    return b.replace(b"\r\n", b"\n").lstrip(b"\xef\xbb\xbf")


def guid_of(b):
    m = re.search(rb"^guid: ([a-f0-9]{32})", norm(b), re.M)
    return m.group(1).decode() if m else None


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--base", required=True)
    ap.add_argument("--head", default="HEAD")
    args = ap.parse_args()

    changed = git("diff", args.base, args.head, "--name-only", "--", "*.meta")
    if changed is None:
        print(f"guid-guard: cannot diff {args.base}..{args.head}; skipping.")
        return 0
    changed = [p for p in changed.decode(errors="replace").split("\n") if p]
    if not changed:
        print("guid-guard: no .meta files changed.")
        return 0

    # Which GUIDs were reassigned in this range?
    reassigned = {}  # old_guid -> (path, new_guid)
    for path in changed:
        old_b, new_b = git("show", f"{args.base}:{path}"), git("show", f"{args.head}:{path}")
        if not old_b or not new_b:
            continue  # added or deleted — not a reassignment
        old_g, new_g = guid_of(old_b), guid_of(new_b)
        if old_g and new_g and old_g != new_g:
            reassigned[old_g] = (path, new_g)

    if not reassigned:
        print(f"guid-guard: {len(changed)} .meta file(s) changed, no GUIDs reassigned. OK.")
        return 0

    # Does anything still point at the old GUIDs? One indexed pass — a recursive
    # grep over Assets/ takes minutes.
    refs = collections.defaultdict(list)
    for f in REPO.rglob("*"):
        if f.suffix not in REF_EXT or SKIP_DIRS & set(f.parts):
            continue
        try:
            for g in set(GUID_RE.findall(f.read_bytes())):
                g = g.decode()
                if g in reassigned:
                    refs[g].append(f.relative_to(REPO).as_posix())
        except OSError:
            pass

    broken = {g: v for g, v in reassigned.items() if refs.get(g)}
    if not broken:
        print(f"guid-guard: {len(reassigned)} GUID(s) reassigned, none referenced. OK.")
        return 0

    print("=" * 74)
    print(f"guid-guard FAILED - {len(broken)} reassigned GUID(s) still referenced")
    print("=" * 74)
    for old_g, (path, new_g) in sorted(broken.items(), key=lambda kv: -len(refs[kv[0]])):
        users = refs[old_g]
        print(f"\n  {path}")
        print(f"    {old_g} -> {new_g}")
        print(f"    breaks {len(users)} reference(s):")
        for u in users[:5]:
            print(f"      - {u}")
        if len(users) > 5:
            print(f"      ... and {len(users) - 5} more")
    print(f"""
{'=' * 74}
Unity reassigned these GUIDs — it did NOT reimport the assets (importer settings
are unchanged). The usual cause is a duplicate copy of an asset landing in
Assets/ (an unzipped pack, a re-run of the Tripo import, a folder copied in from
outside Unity). Unity resolves the GUID collision by renumbering one copy; the
scenes referencing the old number then dangle.

To fix:
  1. Remove the duplicate copy from Assets/ (keep one home per asset).
  2. python tools/guid_restore.py   # set BEFORE/AFTER to this range, then --apply
  3. Commit the restored metas.
{'=' * 74}""")
    return 1


if __name__ == "__main__":
    sys.exit(main())
