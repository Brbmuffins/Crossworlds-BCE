"""
generate_null_architect_assets_tripo.py
----------------------------------------
Generates 3D set-dressing assets for the Null Architect void-cathedral boss arena
using the Tripo3D API.  Mirrors the pattern of generate_wooden_fence_fbx.py.

Usage (run from repo root):
    python Tools/generate_null_architect_assets_tripo.py

Requires:
    TRIPO_API_KEY environment variable (never hardcode — set in shell or .env)
    pip install requests

Output:
    Assets/Game/3d Assets/NullArchitect/
        broken_cathedral_pillar/    broken_cathedral_pillar.glb
        rune_stone_altar/           rune_stone_altar.glb
        void_crystal_shard/         void_crystal_shard.glb
        altar_fragment_a/           altar_fragment_a.glb
        altar_fragment_b/           altar_fragment_b.glb

All generated assets are standard mesh/material imports.
DO NOT add NetworkIdentity — these are cosmetic set-dressing only.

Asset manifest is written to Assets/Game/3d Assets/NullArchitect/MANIFEST.txt
with source (generated / reused) and intended scene role for each entry.
"""

import os
import time
import json
import pathlib
import requests

# ── Config ───────────────────────────────────────────────────────────────────

API_KEY = os.environ.get("TRIPO_API_KEY")
if not API_KEY:
    raise EnvironmentError(
        "TRIPO_API_KEY environment variable not set.\n"
        "Export it before running: $env:TRIPO_API_KEY='tsk_...'"
    )

BASE_URL    = "https://api.tripo3d.ai/v2/openapi"
OUTPUT_ROOT = pathlib.Path("Assets/Game/3d Assets/NullArchitect")

HEADERS = {
    "Authorization": f"Bearer {API_KEY}",
    "Content-Type":  "application/json",
}

POLL_INTERVAL = 4   # seconds between status checks
POLL_TIMEOUT  = 300 # seconds before giving up on a task

# ── Asset definitions ─────────────────────────────────────────────────────────

ASSETS = [
    {
        "slug":   "broken_cathedral_pillar",
        "prompt": (
            "A broken medieval cathedral pillar made of dark void-stone. "
            "Gothic architecture style, cracked and crumbling at the top, "
            "with glowing purple void energy seeping through the cracks. "
            "The stone is near-black with faint violet iridescence. "
            "Realistic PBR textures, game-ready, low to mid polygon count. "
            "Standalone prop, no background."
        ),
        "negative_prompt": "grass, wood, metal, bright colors, sci-fi, modern",
        "role": "Cathedral pillar set-dressing (cosmetic, no NetworkIdentity)",
        "count": 1,
    },
    {
        "slug":   "rune_stone_altar",
        "prompt": (
            "A dark fantasy rune-stone altar carved from void-stone. "
            "Covered in glowing purple arcane runes that pulse with void energy. "
            "Roughly 1.5 meters tall, cracked and ancient. "
            "The stone is charcoal-black with deep violet emissive rune channels. "
            "Realistic PBR, game-ready prop. No background."
        ),
        "negative_prompt": "grass, wood, bright, sci-fi, bone, skull",
        "role": "Altar set-dressing at boss arena perimeter (cosmetic)",
        "count": 1,
    },
    {
        "slug":   "void_crystal_shard",
        "prompt": (
            "A jagged void crystal shard about 0.5 meters tall. "
            "Deep purple-black translucent crystal, sharp faceted edges, "
            "with inner glow of violet light. "
            "Fantasy dark magic aesthetic, PBR textures, game-ready. "
            "Standalone, no background or base."
        ),
        "negative_prompt": "quartz, white, bright, ice, sci-fi",
        "role": "Void crystal scatter prop (cosmetic, cluster near seam tears)",
        "count": 1,
    },
    {
        "slug":   "altar_fragment_a",
        "prompt": (
            "A large broken stone fragment from a collapsed cathedral altar. "
            "Dark grey-purple void-stone, cracked surfaces, one flat broken face. "
            "Arcane rune engravings partially visible on surface. "
            "PBR game-ready prop, about 1 meter across."
        ),
        "negative_prompt": "complete, pristine, bright, sci-fi, wood",
        "role": "Floor debris near pillars (cosmetic)",
        "count": 1,
    },
    {
        "slug":   "altar_fragment_b",
        "prompt": (
            "A smaller broken piece of carved dark stone from a collapsed gothic arch. "
            "Void-stone: near-black with purple tinge. Broken edges rough and jagged. "
            "Small rune fragment visible on one face. Game-ready PBR prop."
        ),
        "negative_prompt": "complete, bright, sci-fi, wood, metal",
        "role": "Floor debris scatter (cosmetic)",
        "count": 1,
    },
]

# ── API helpers ───────────────────────────────────────────────────────────────

def submit_task(prompt: str, negative_prompt: str) -> str:
    """Submit a text-to-model task and return the task ID."""
    body = {
        "type": "text_to_model",
        "prompt": prompt,
        "negative_prompt": negative_prompt,
        "model_version": "v2.5-20250123",
        "texture": True,
        "pbr": True,
    }
    resp = requests.post(f"{BASE_URL}/task", headers=HEADERS, json=body, timeout=30)
    resp.raise_for_status()
    data = resp.json()
    task_id = data["data"]["task_id"]
    print(f"  Submitted task: {task_id}")
    return task_id


def poll_task(task_id: str) -> dict:
    """Poll until task succeeds or fails. Returns the completed task data."""
    deadline = time.time() + POLL_TIMEOUT
    while time.time() < deadline:
        resp = requests.get(f"{BASE_URL}/task/{task_id}", headers=HEADERS, timeout=15)
        resp.raise_for_status()
        task = resp.json()["data"]
        status = task["status"]

        if status == "success":
            return task
        if status in ("failed", "cancelled", "error"):
            raise RuntimeError(f"Task {task_id} failed with status: {status}\n{task}")

        print(f"  [{task_id}] status={status} progress={task.get('progress', '?')}%")
        time.sleep(POLL_INTERVAL)

    raise TimeoutError(f"Task {task_id} timed out after {POLL_TIMEOUT}s")


def download_glb(task_data: dict, output_path: pathlib.Path) -> None:
    """Download the rendered GLB from the task result."""
    # Tripo stores the model URL under result.model.url
    result = task_data.get("output", task_data.get("result", {}))
    model_url = None

    # Try common key paths. Tripo v2.5 returns these as plain URL strings,
    # older/other shapes return a {"url": ...} dict — handle both.
    for key in ("pbr_model", "model", "rendered_image"):
        val = result.get(key)
        if not val:
            continue
        model_url = val.get("url") if isinstance(val, dict) else val
        if model_url:
            break

    if not model_url:
        print(f"  WARNING: could not find model URL in task result. Keys: {list(result.keys())}")
        print(f"  Full result: {json.dumps(result, indent=2)}")
        return

    print(f"  Downloading from {model_url[:60]}...")
    r = requests.get(model_url, timeout=120, stream=True)
    r.raise_for_status()
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "wb") as f:
        for chunk in r.iter_content(chunk_size=8192):
            f.write(chunk)
    print(f"  Saved: {output_path}")


# ── Manifest ──────────────────────────────────────────────────────────────────

def write_manifest(entries: list[dict]) -> None:
    manifest_path = OUTPUT_ROOT / "MANIFEST.txt"
    manifest_path.parent.mkdir(parents=True, exist_ok=True)
    lines = [
        "Null Architect Arena — Asset Manifest",
        "Generated by Tools/generate_null_architect_assets_tripo.py",
        f"Date: {time.strftime('%Y-%m-%d %H:%M:%S')}",
        "",
        "SOURCE KEY: [G] = Generated (Tripo3D)  [R] = Reused (in-project)",
        "",
    ]
    for e in entries:
        lines.append(f"[{e['source']}] {e['path']}")
        lines.append(f"      Role: {e['role']}")
        lines.append(f"      NetworkIdentity: NONE — cosmetic set-dressing only")
        lines.append("")

    manifest_path.write_text("\n".join(lines))
    print(f"\nManifest written to {manifest_path}")


# ── Reused assets manifest entries ────────────────────────────────────────────

REUSED_ASSETS = [
    {
        "source": "R",
        "path": "Assets/brbmuffins Dark Arts/brbmuffins Fantasy Pack/Prefabs/Effects normal/Death magic circle.prefab",
        "role": "reflectTelegraphVFX on WorldBossController (Phase 1 reflect warning)",
    },
    {
        "source": "R",
        "path": "Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/AoE effects/Smoke AOE explosion.prefab",
        "role": "transitionVFXPrefab on WorldBossController (phase shift burst)",
    },
    {
        "source": "R",
        "path": "Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/AoE effects/Red energy explosion.prefab",
        "role": "deathVFXPrefab on WorldBossController (boss death collapse)",
    },
    {
        "source": "R",
        "path": "Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Environment/Crystal effect blue.prefab",
        "role": "voidDrainVFX on WorldBossController (Phase 3 drain zone — tint purple in Inspector)",
    },
    {
        "source": "R",
        "path": "Assets/brbmuffins VFX/brbmuffins Free VFX/Prefab/FX_LightPillar.prefab",
        "role": "God-ray pillars in NullArchitectRoomBuilder (4 positions, cosmetic)",
    },
    {
        "source": "R",
        "path": "Assets/brbmuffins Trails/brbmuffins Trails VFX/VFX/Particles/VFX_Trail_Void.prefab",
        "role": "Boss ambient trail / shard ambient VFX (attach to boss prefab)",
    },
    {
        "source": "R",
        "path": "Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Magic circles/Magic circle.prefab",
        "role": "Reflect-window floor decal (instantiate under reflectTelegraphVFX, tint cyan)",
    },
    {
        "source": "R",
        "path": "Assets/brbmuffins Dark Arts/brbmuffins Fantasy Pack/Prefabs/Glowing orbs.prefab",
        "role": "Ambient glowing orbs near ceiling rifts (placed by NullArchitectRoomBuilder)",
    },
    {
        "source": "R",
        "path": "Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Slash effects/Charge slash purple.prefab",
        "role": "Reflect pulse outward ring (optional, instantiate in OnDamageTakenServer reflect)",
    },
]


# ── Main ──────────────────────────────────────────────────────────────────────

def main():
    print("=== Null Architect 3D Asset Generator (Tripo3D) ===\n")
    OUTPUT_ROOT.mkdir(parents=True, exist_ok=True)

    manifest_entries = list(REUSED_ASSETS)  # start with reused

    for asset in ASSETS:
        slug = asset["slug"]
        out_dir  = OUTPUT_ROOT / slug
        out_file = out_dir / f"{slug}.glb"

        print(f"\n[{slug}]")

        if out_file.exists():
            print(f"  Already exists — skipping ({out_file})")
            manifest_entries.append({
                "source": "G",
                "path":   str(out_file),
                "role":   asset["role"],
            })
            continue

        try:
            task_id   = submit_task(asset["prompt"], asset["negative_prompt"])
            task_data = poll_task(task_id)
            download_glb(task_data, out_file)
            manifest_entries.append({
                "source": "G",
                "path":   str(out_file),
                "role":   asset["role"],
            })
        except Exception as exc:
            print(f"  ERROR generating {slug}: {exc}")
            manifest_entries.append({
                "source": "G (FAILED)",
                "path":   str(out_file),
                "role":   asset["role"],
            })

    write_manifest(manifest_entries)
    print("\nDone.  Import GLB files in Unity (drag into Project window).")
    print("Do NOT add NetworkIdentity to any generated asset — cosmetic only.")


if __name__ == "__main__":
    main()
