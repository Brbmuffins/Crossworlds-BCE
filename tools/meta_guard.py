"""
Enforce Unity's meta invariant in git: every tracked asset has a tracked .meta.

A .meta carries the asset's GUID. Commit an asset without its .meta and every clone
regenerates a *different* GUID, so anything referencing it dangles -- for everyone
except the person who authored it (their local .meta still holds the original).

This is how Darkwood's 8 ogres broke: `Idle.prefab` and `Idle (1).prefab` were tracked
without their metas (ROADMAP 2.5). Same symptom as the duplicate-collision churn in 2.6,
different cause -- and this one is trivially preventable.

Usage:  python tools/meta_guard.py
Exit 0 = clean, 1 = invariant violated.
"""
import pathlib, subprocess, sys

REPO = pathlib.Path(__file__).resolve().parent.parent
# Unity does not create .meta for these; git-only files that live under Assets/.
META_EXEMPT_SUFFIX = (".gitignore", ".gitattributes", ".keep")


def main():
    out = subprocess.run(["git", "ls-files", "Assets/"], cwd=REPO,
                         capture_output=True, text=True, errors="replace")
    if out.returncode != 0:
        print("meta-guard: git ls-files failed")
        return 1

    files = [f for f in out.stdout.split("\n") if f.startswith("Assets/")]
    metas = {f[:-5] for f in files if f.endswith(".meta")}
    assets = [f for f in files
              if not f.endswith(".meta") and not f.endswith(META_EXEMPT_SUFFIX)]

    missing = sorted(a for a in assets if a not in metas)
    # A meta whose asset is absent is only a problem when the path is neither a tracked
    # file nor a real directory (Unity metas folders too, and git tracks no folders).
    orphan = sorted(m for m in metas
                    if m not in set(assets) and not (REPO / m).is_dir())

    print(f"meta-guard: {len(assets)} assets, {len(metas)} metas tracked")

    if not missing and not orphan:
        print("meta-guard: OK - every tracked asset has its .meta.")
        return 0

    print("=" * 74)
    print("meta-guard FAILED")
    print("=" * 74)
    if missing:
        print(f"\n{len(missing)} asset(s) tracked WITHOUT their .meta.")
        print("Every clone regenerates a new GUID for these, dangling every reference:\n")
        for a in missing:
            print(f"    {a}")
            print(f"      -> git add \"{a}.meta\"")
    if orphan:
        print(f"\n{len(orphan)} .meta tracked with no asset and no folder (stale):\n")
        for m in orphan:
            print(f"    {m}.meta")
    print(f"""
{'=' * 74}
Commit the .meta alongside its asset. If a .meta is being ignored, check
.gitignore -- an ignore rule that catches assets but not metas (or vice versa)
silently breaks GUID stability for every other dev.
{'=' * 74}""")
    return 1


if __name__ == "__main__":
    sys.exit(main())
