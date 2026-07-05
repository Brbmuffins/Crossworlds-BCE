"""
generate_ability_icons.py — upgrade ability icons via Gemini Imagen
Run locally (not in the Unity sandbox) when you want AI-generated art.

Usage:
    python tools/generate_ability_icons.py

Requires:
    pip install google-generativeai pillow
    GEMINI_API_KEY environment variable set (or paste key below)

Generated PNGs land in:
    Assets/Game/UI/CharacterSelect/AbilityIcons/

After running: BCE → Heroes → Assign Ability Icons in Unity.
"""

import os, pathlib, time
import google.generativeai as genai
from PIL import Image
import io

# ── Key ───────────────────────────────────────────────────────────────────────
API_KEY = os.environ.get("GEMINI_API_KEY", "")
if not API_KEY:
    raise SystemExit("Set GEMINI_API_KEY environment variable.")

genai.configure(api_key=API_KEY)

OUT = pathlib.Path(__file__).parent.parent / "Assets/Game/UI/CharacterSelect/AbilityIcons"
OUT.mkdir(parents=True, exist_ok=True)

STYLE = (
    "square game ability icon, dark fantasy RPG style, "
    "black background, glowing magical effect, highly detailed, "
    "centered composition, 256x256, no text, no watermark. "
)

ABILITIES = {
    "storm-lash":      "electric whip made of lightning crackling with storm energy",
    "ember-surge":     "burst of roaring orange and red fire exploding outward",
    "mind-spike":      "glowing purple psychic spike piercing a bright aura",
    "binding-wave":    "teal energy wave radiating binding chains of light",
    "arcane-ward":     "blue arcane magical barrier shield with glowing runes",
    "battle-hymn":     "golden musical notes radiating from a glowing lute",
    "spirit-redirect": "translucent spirit arrow curving mid-flight in blue mist",
    "counter-blow":    "golden armored fist with a shield deflecting energy",
    "gravity-slam":    "purple gravity well pulling glowing rocks downward",
    "void-maw":        "swirling dark void portal with purple energy tendrils",
    "spirit-wisps":    "three glowing green spirit orbs floating in green mist",
    "divine-spark":    "brilliant golden spark of holy light radiating rays",
    "sacred-aegis":    "radiant gold and white holy shield with a cross",
    "dispel":          "blue magical circle shattering purple enchantment sparkles",
    "temporal-grace":  "glowing gold hourglass with swirling green time energy",
    "silence-ward":    "grey magical barrier with muted sound wave symbols",
}

model = genai.GenerativeModel("gemini-2.0-flash-preview-image-generation")

for slug, desc in ABILITIES.items():
    out_path = OUT / f"{slug}.png"
    print(f"Generating {slug}...", end=" ", flush=True)
    try:
        response = model.generate_content(
            STYLE + desc,
            generation_config={"response_modalities": ["IMAGE"]},
        )
        for part in response.candidates[0].content.parts:
            if part.inline_data and part.inline_data.mime_type.startswith("image"):
                img = Image.open(io.BytesIO(part.inline_data.data))
                img = img.resize((256, 256), Image.LANCZOS)
                img.save(out_path)
                print(f"✓ saved")
                break
        else:
            print("⚠ no image in response")
    except Exception as e:
        print(f"✗ {e}")
    time.sleep(1.5)   # stay under rate limit

print("\nAll done. Re-run BCE → Heroes → Assign Ability Icons in Unity.")
