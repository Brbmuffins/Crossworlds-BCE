#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// LoginSceneBuilder — BCE/World/Setup Login Scene
///
/// One click wires up the entire login scene atmosphere:
///   - Camera settings (cinematic dark)
///   - Scene fog + ambient lighting
///   - LoginScreenVFX with all brbmuffins prefabs assigned
///   - LoginBackgroundFX cinematic orbit camera
///   - Directional light (deep blue night)
///
/// Run from: BCE → World → ► Setup Login Scene
/// </summary>
public static class LoginSceneBuilder
{
    [MenuItem("BCE/World/► Setup Login Scene")]
    static void SetupLoginScene()
    {
        // ── Confirm we're in LoginScene ───────────────────────────────────────
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.name.Contains("Login"))
        {
            bool ok = EditorUtility.DisplayDialog(
                "Setup Login Scene",
                $"Active scene is '{scene.name}', not LoginScene.\n\nOpen LoginScene first, then run this.",
                "OK");
            return;
        }

        // ── Load prefabs ──────────────────────────────────────────────────────
        var portalBlue   = Load("Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Portals/Portal blue.prefab");
        var circle1      = Load("Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Magic circles/Magic circle.prefab");
        var circle2      = Load("Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Magic circles/Magic circle 2.prefab");
        var plexus       = Load("Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Character auras/Plexus.prefab");
        var groundFog    = Load("Assets/brbmuffins Technologies/brbmuffins Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/GroundFog.prefab");
        var dustMotes    = Load("Assets/brbmuffins Technologies/brbmuffins Particle Pack/EffectExamples/Misc Effects/Prefabs/DustMotesEffect.prefab");
        var glowingOrbs  = Load("Assets/brbmuffins Dark Arts/brbmuffins Fantasy Pack/Prefabs/Glowing orbs.prefab");
        var lightPillar  = Load("Assets/brbmuffins VFX/brbmuffins Free VFX/Prefab/FX_LightPillar.prefab");
        var magicDoor    = Load("Assets/brbmuffins VFX/brbmuffins Free VFX/Prefab/FX_Magic Door_Gold.prefab");
        var dustLoop     = Load("Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Smoke effects/Dust loop.prefab");

        // ── Scene fog + ambient ───────────────────────────────────────────────
        RenderSettings.fog         = true;
        RenderSettings.fogMode     = FogMode.Exponential;
        RenderSettings.fogDensity  = 0.025f;
        RenderSettings.fogColor    = new Color(0.02f, 0.01f, 0.06f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.04f, 0.03f, 0.09f);

        // ── Directional light (deep blue night) ───────────────────────────────
        var dirGO = GameObject.Find("Directional Light");
        if (dirGO == null) dirGO = new GameObject("Directional Light");
        var dirLight = dirGO.GetComponent<Light>();
        if (dirLight == null) dirLight = dirGO.AddComponent<Light>();
        dirLight.type      = LightType.Directional;
        dirLight.color     = new Color(0.13f, 0.2f, 0.55f);
        dirLight.intensity = 0.18f;
        dirGO.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

        // ── Camera ────────────────────────────────────────────────────────────
        var camGO = GameObject.Find("Main Camera");
        if (camGO == null) camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.GetComponent<Camera>();
        if (cam == null) cam = camGO.AddComponent<Camera>();
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.01f, 0.04f);
        cam.fieldOfView     = 55f;
        cam.nearClipPlane   = 0.1f;
        cam.farClipPlane    = 200f;

        // ── LoginBG object ────────────────────────────────────────────────────
        var bgGO = GameObject.Find("LoginBG") ?? new GameObject("LoginBG");
        bgGO.transform.position = Vector3.zero;

        // LoginBackgroundFX — cinematic orbit camera
        var bgfx = bgGO.GetComponent<LoginBackgroundFX>();
        if (bgfx == null) bgfx = bgGO.AddComponent<LoginBackgroundFX>();

        // LoginScreenVFX — brbmuffins atmosphere
        var vfx = bgGO.GetComponent<LoginScreenVFX>();
        if (vfx == null) vfx = bgGO.AddComponent<LoginScreenVFX>();

        // Assign prefabs to LoginScreenVFX via SerializedObject
        var so = new SerializedObject(vfx);
        SetPrefab(so, "dustMotes",      dustMotes);
        SetPrefab(so, "fireFlies",      dustLoop);       // dust loop works as subtle fireflies
        SetPrefab(so, "groundFog",      groundFog);
        SetPrefab(so, "lightPillar",    lightPillar);
        SetPrefab(so, "lootBeam",       magicDoor);
        SetPrefab(so, "magicCircle",    circle1);
        SetPrefab(so, "magicCircle2",   circle2);
        SetPrefab(so, "plexusAura",     plexus);
        SetPrefab(so, "portalBlue",     portalBlue);
        SetPrefab(so, "glowingOrbs",    glowingOrbs);
        so.ApplyModifiedProperties();

        // Timing tweaks
        vfx.sparksInterval  = 10f;
        vfx.slowRotateSpeed = 4f;

        // ── Mark scene dirty and save ─────────────────────────────────────────
        EditorUtility.SetDirty(bgGO);
        EditorUtility.SetDirty(vfx);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        int assigned = CountAssigned(so);
        EditorUtility.DisplayDialog("Login Scene Setup",
            $"Done!\n\n" +
            $"  Prefabs assigned: {assigned}\n" +
            $"  Fog: on\n" +
            $"  Ambient: deep void blue\n\n" +
            "Press Play to see the result.\n" +
            "Camera orbits the scene slowly — Blizzard style.",
            "Let's go");
    }

    static void SetPrefab(SerializedObject so, string field, GameObject prefab)
    {
        if (prefab == null) return;
        var prop = so.FindProperty(field);
        if (prop != null) prop.objectReferenceValue = prefab;
    }

    static int CountAssigned(SerializedObject so)
    {
        string[] fields = { "dustMotes","fireFlies","groundFog","lightPillar",
                            "lootBeam","magicCircle","magicCircle2","plexusAura",
                            "portalBlue","glowingOrbs" };
        int count = 0;
        foreach (var f in fields)
        {
            var p = so.FindProperty(f);
            if (p != null && p.objectReferenceValue != null) count++;
        }
        return count;
    }

    static GameObject Load(string path)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (go == null) Debug.LogWarning($"[LoginScene] Prefab not found: {path}");
        return go;
    }

    [MenuItem("BCE/World/► Setup Login Scene", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
