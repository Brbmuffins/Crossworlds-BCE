"""
generate_null_architect_textures_gemini.py
-------------------------------------------
Generates 2D texture / sprite assets for the Null Architect boss arena
using the Gemini Imagen API via raw HTTP (no SDK dependency).

Usage (run from repo root):
    python Tools/generate_null_architect_textures_gemini.py

Requires:
    GEMINI_API_KEY environment variable (never hardcode)
    pip install requests pillow

Output:
    Assets/Game/Textures/NullArchitect/
        fog_sheet_01.png
        fog_sheet_02.png
        telegraph_decal_reflect.png
        telegraph_decal_drain.png
        rune_glyph_floor.png
        void_seam_crack.png
        void_texture_tile.png
"""

import os
import base64
import time
import pathlib
import json
import requests
from PIL import Image
import io

# ── Config ────────────────────────────────────────────────────────────────────

API_KEY = os.environ.get("GEMINI_API_KEY")
if not API_KEY:
    raise EnvironmentError("GEMINI_API_KEY environment variable not set.")

BASE_URL    = "https://generativelanguage.googleapis.com/v1beta/models"
IMAGE_MODEL = "imagen-4.0-generate-001"

OUTPUT_ROOT = pathlib.Path("Assets/Game/Textures/NullArchitect")
OUTPUT_ROOT.mkdir(parents=True, exist_ok=True)

# ── Texture definitions ───────────────────────────────────────────────────────

TEXTURES = [
    {
        "filename": "fog_sheet_01.png",
        "prompt": (
            "A soft billowing purple-violet fog texture on pure black background. "
            "Tileable, seamless. Deep violet and indigo hues, translucent wisps. "
            "No figures, no objects — pure atmospheric fog smoke texture. "
            "Flat 2D, high contrast against black, particle sprite sheet style."
        ),
        "negative": "people, objects, faces, text, bright white, green, red",
        "size": (512, 512),
        "role": "Fog particle sheet — VoidParticulateDome ParticleSystem",
    },
    {
        "filename": "fog_sheet_02.png",
        "prompt": (
            "Dense swirling dark purple and deep violet mist fog texture on black. "
            "Seamless tile. Heavier more opaque than sheet 01. "
            "Pure abstract atmospheric — no figures or objects. Particle sprite style."
        ),
        "negative": "people, objects, faces, text, bright colors",
        "size": (512, 512),
        "role": "Secondary fog particle sheet (denser, floor-level emitters)",
    },
    {
        "filename": "telegraph_decal_reflect.png",
        "prompt": (
            "Top-down view of a glowing arcane warning circle on black background. "
            "Cyan-violet energy rings radiating outward from centre. "
            "Sharp high-contrast neon glow lines on dark background. "
            "Fantasy game AoE telegraph indicator style. Square centred. "
            "No text, no figures — pure glowing ring pattern."
        ),
        "negative": "people, text, realistic, blurry, green, red, orange",
        "size": (512, 512),
        "role": "Phase 1 Reflect telegraph floor decal",
    },
    {
        "filename": "telegraph_decal_drain.png",
        "prompt": (
            "Top-down view of a glowing safe-zone circle game AoE indicator. "
            "Solid glowing purple-violet ring on black background. "
            "Inner area clear, ring edge glowing bright purple-white. "
            "Sharp edges, high contrast. No text, no figures."
        ),
        "negative": "people, text, blurry, red, orange, green",
        "size": (512, 512),
        "role": "Phase 3 Void Drain safe-zone floor decal",
    },
    {
        "filename": "rune_glyph_floor.png",
        "prompt": (
            "Top-down view of ancient arcane rune glyphs carved into dark void-stone floor. "
            "Purple glowing rune channels etched into near-black stone. "
            "Central rune circle with radiating arm glyphs, angular and alien. "
            "Glow is bright purple-violet, stone is charcoal-black. "
            "Square tile. High detail. Floor material emissive map style."
        ),
        "negative": "people, figures, text, words, letters, bright background",
        "size": (1024, 1024),
        "role": "Floor rune seam emissive map — M_RuneSeamEmissive material",
    },
    {
        "filename": "void_seam_crack.png",
        "prompt": (
            "A tall vertical crack in dark stone glowing with purple void energy from within. "
            "Stone is charcoal-dark, crack interior blazes violet-white. "
            "Hair-line secondary fractures branch from main crack. "
            "Portrait composition. Black background. Game texture style, high contrast."
        ),
        "negative": "people, text, bright background, green, orange",
        "size": (512, 512),
        "role": "Void seam crack emissive map — M_VoidSeam materials",
    },
    {
        "filename": "void_texture_tile.png",
        "prompt": (
            "Seamless tileable texture of swirling void energy. "
            "Deep space purple-black with subtle violet nebula-like patterns. "
            "Very dark, nearly black with faint purple sheen. "
            "Abstract, no objects or figures. Background skybox tile style."
        ),
        "negative": "people, text, bright colors, green, orange",
        "size": (512, 512),
        "role": "Void background tile — optional skybox or wall material",
    },
]

# ── API call ──────────────────────────────────────────────────────────────────

def generate_image(prompt: str, negative: str, size: tuple) -> Image.Image | None:
    url = f"{BASE_URL}/{IMAGE_MODEL}:predict?key={API_KEY}"
    body = {
        "instances": [{"prompt": prompt}],
        "parameters": {
            "sampleCount": 1,
            "personGeneration": "dont_allow",
            # Imagen 4 dropped negativePrompt; safety uses server defaults here.
        },
    }
    resp = requests.post(url, json=body, timeout=60)
    if resp.status_code != 200:
        print(f"  API error {resp.status_code}: {resp.text[:300]}")
        return None

    data = resp.json()
    predictions = data.get("predictions", [])
    if not predictions:
        print(f"  No predictions returned. Response: {json.dumps(data)[:300]}")
        return None

    # Imagen returns base64-encoded PNG in bytesBase64Encoded
    img_b64 = predictions[0].get("bytesBase64Encoded")
    if not img_b64:
        print(f"  No image bytes in prediction. Keys: {list(predictions[0].keys())}")
        return None

    img_bytes = base64.b64decode(img_b64)
    pil_img = Image.open(io.BytesIO(img_bytes))

    w, h = size
    if pil_img.size != (w, h):
        pil_img = pil_img.resize((w, h), Image.LANCZOS)

    return pil_img


# ── Manifest ──────────────────────────────────────────────────────────────────

REUSED_ASSETS = [
    {"path": "Assets/brbmuffins Dark Arts/brbmuffins Fantasy Pack/Prefabs/Effects normal/Death magic circle.prefab",
     "role": "reflectTelegraphVFX on WorldBossController"},
    {"path": "Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/AoE effects/Smoke AOE explosion.prefab",
     "role": "transitionVFXPrefab on WorldBossController"},
    {"path": "Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/AoE effects/Red energy explosion.prefab",
     "role": "deathVFXPrefab on WorldBossController"},
    {"path": "Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Environment/Crystal effect blue.prefab",
     "role": "voidDrainVFX on WorldBossController"},
    {"path": "Assets/brbmuffins VFX/brbmuffins Free VFX/Prefab/FX_LightPillar.prefab",
     "role": "God-ray pillars (4× in scene)"},
    {"path": "Assets/brbmuffins Trails/brbmuffins Trails VFX/VFX/Particles/VFX_Trail_Void.prefab",
     "role": "Boss ambient trail (attach to boss prefab child)"},
    {"path": "Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Magic circles/Magic circle.prefab",
     "role": "Reflect floor decal"},
    {"path": "Assets/brbmuffins Dark Arts/brbmuffins Fantasy Pack/Prefabs/Glowing orbs.prefab",
     "role": "Ceiling ambient orbs (2× in scene)"},
]

def write_manifest(results: list) -> None:
    manifest_path = OUTPUT_ROOT / "MANIFEST.txt"
    lines = [
        "Null Architect Arena — 2D Texture Manifest",
        "Generated by Tools/generate_null_architect_textures_gemini.py",
        f"Date: {time.strftime('%Y-%m-%d %H:%M:%S')}",
        "",
        "=== GENERATED (Gemini Imagen) ===",
        "",
    ]
    for r in results:
        status = "OK" if r["ok"] else "FAILED"
        lines += [f"[{status}] {r['path']}", f"       Role: {r['role']}", ""]

    lines += ["=== REUSED (in-project brbmuffins) ===", ""]
    for a in REUSED_ASSETS:
        lines += [f"[R] {a['path']}", f"    Role: {a['role']}", ""]

    manifest_path.write_text("\n".join(lines))
    print(f"\nManifest written → {manifest_path}")


# ── Main ──────────────────────────────────────────────────────────────────────

def main():
    print("=== Null Architect Texture Generator (Gemini Imagen — HTTP) ===\n")
    results = []

    for tex in TEXTURES:
        filename = tex["filename"]
        out_path = OUTPUT_ROOT / filename
        print(f"[{filename}]")

        if out_path.exists():
            print(f"  Already exists — skipping.\n")
            results.append({"ok": True, "path": str(out_path), "role": tex["role"]})
            continue

        img = generate_image(tex["prompt"], tex["negative"], tex["size"])
        if img:
            img.save(out_path, "PNG")
            print(f"  Saved: {out_path}\n")
            results.append({"ok": True, "path": str(out_path), "role": tex["role"]})
        else:
            print(f"  FAILED — skipping.\n")
            results.append({"ok": False, "path": str(out_path), "role": tex["role"]})

        time.sleep(1.5)  # brief pause between API calls

    write_manifest(results)
    ok_count = sum(1 for r in results if r["ok"])
    print(f"\nDone. {ok_count}/{len(results)} textures generated.")
    print("Import PNGs in Unity — drag into Assets/Game/Textures/NullArchitect/")


if __name__ == "__main__":
    main()
