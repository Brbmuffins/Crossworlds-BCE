#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// ClericAnimSetupBuilder — BCE/Heroes/Setup Cleric Animation
///
/// Finds the Cleric prefab and wires the animation/VFX components:
///   • Adds ClericAnimationDriver (if missing)
///   • Adds ClericHealVFX (if missing)
///   • Links AbilityCaster.healVFX → ClericHealVFX
///   • Links ClericAnimationDriver.healVFX → ClericHealVFX
///   • Links ClericAnimationDriver.abilityCaster → AbilityCaster
///
/// After running, open the Cleric prefab in the editor and:
///   1. Confirm ClericAnimationDriver and ClericHealVFX appear in the Inspector
///   2. Assign particle systems to ClericHealVFX.healBurst / healAura
///   3. Ctrl+S
/// </summary>
public static class ClericAnimSetupBuilder
{
    const string ClericPrefabPath = "Assets/Game/Prefabs/Cleric.prefab";

    [MenuItem("BCE/Heroes/Setup Cleric Animation")]
    static void SetupClericAnimation()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ClericPrefabPath);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Cleric Setup",
                $"Cleric prefab not found at:\n{ClericPrefabPath}\n\nCheck the path.", "OK");
            return;
        }

        // Work on the prefab asset directly
        using var scope = new PrefabUtility.EditPrefabContentsScope(ClericPrefabPath);
        var root = scope.prefabContentsRoot;

        // ── ClericHealVFX ─────────────────────────────────────────────────────
        var healVFX = root.GetComponent<ClericHealVFX>();
        if (healVFX == null)
        {
            healVFX = root.AddComponent<ClericHealVFX>();
            Debug.Log("[ClericSetup] Added ClericHealVFX.");
        }

        // ── ClericAnimationDriver ─────────────────────────────────────────────
        var driver = root.GetComponent<ClericAnimationDriver>();
        if (driver == null)
        {
            driver = root.AddComponent<ClericAnimationDriver>();
            Debug.Log("[ClericSetup] Added ClericAnimationDriver.");
        }

        // ── AbilityCaster wiring ──────────────────────────────────────────────
        var caster = root.GetComponent<AbilityCaster>();
        if (caster != null)
        {
            var so = new SerializedObject(caster);
            var vfxProp = so.FindProperty("healVFX");
            if (vfxProp != null)
            {
                vfxProp.objectReferenceValue = healVFX;
                so.ApplyModifiedProperties();
                Debug.Log("[ClericSetup] Linked AbilityCaster.healVFX → ClericHealVFX.");
            }
        }
        else
        {
            Debug.LogWarning("[ClericSetup] AbilityCaster not found on Cleric prefab root.");
        }

        // ── Driver cross-refs ────────────────────────────────────────────────
        var driverSO = new SerializedObject(driver);

        var dHealVFX = driverSO.FindProperty("healVFX");
        if (dHealVFX != null) dHealVFX.objectReferenceValue = healVFX;

        var dCaster = driverSO.FindProperty("abilityCaster");
        if (dCaster != null) dCaster.objectReferenceValue = caster;

        driverSO.ApplyModifiedProperties();
        Debug.Log("[ClericSetup] Linked ClericAnimationDriver refs.");
    }

    [MenuItem("BCE/Heroes/Setup Cleric Animation", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
