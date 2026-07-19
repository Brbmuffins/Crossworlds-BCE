#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// BCE/Setup/4z — Fix Class Prefab Asset IDs
///
/// Mirror's NetworkIdentity requires a non-zero _assetId baked into each
/// prefab file so clients can look up the prefab in the spawn registry when
/// the server sends a Spawn message.  The value is computed and written by
/// NetworkIdentity.OnValidate(), which only runs when a prefab is imported
/// or touched in the editor.
///
/// Warden, Ironclad, Shadowblade, and Cleric currently have _assetId: 0 in
/// their .prefab files (visible in raw YAML).  Arcanist has a correct
/// non-zero value.  ChatManager.prefab also has _assetId: 0 which breaks
/// chat for non-host clients (RPC delivery and UI never exist on their end).
/// In built clients, Mirror uses only the serialized value and cannot fall
/// back to a GUID-based computation, so those prefabs fail to spawn for
/// non-host clients — which is why "only Arcanist works" and chat is broken.
///
/// Running this menu item force-reimports all 5 class prefabs so
/// NetworkIdentity.OnValidate() fires and writes the correct _assetId.
/// Commit the resulting .prefab changes; the fix is permanent.
/// </summary>
public static class ClassPrefabFixer
{
    const string PREFABS_DIR = "Assets/Game/Game_Prefabs";

    static readonly string[] ClassNames =
        { "Warden", "Ironclad", "Shadowblade", "Cleric", "Arcanist" };

    // Additional networked prefabs (not class heroes) that also need a valid assetId.
    static readonly string[] ExtraPrefabs =
    {
        "Assets/Game/Networking/ChatManager.prefab",
        "Assets/Game/Networking/RestorationBeacon.prefab",
        "Assets/Game/Game_Prefabs/Muffin Junk/Wisp_Mob.prefab",
        "Assets/Game/Game_Prefabs/Muffin Junk/Enemy_Grunt.prefab",
        "Assets/Game/Game_Prefabs/Muffin Junk/Enemy_Ranged.prefab",
        "Assets/Game/Game_Prefabs/Muffin Junk/Enemy_Elite.prefab",
    };

    [MenuItem("BCE/Setup/4z ▶ Fix Class Prefab Asset IDs", priority = 47)]
    static void FixAssetIds()
    {
        int fixed_ = 0;
        int missing = 0;

        foreach (var name in ClassNames)
        {
            string path = $"{PREFABS_DIR}/{name}.prefab";

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[BCE] Prefab not found: {path}");
                missing++;
                continue;
            }

            var netId = prefab.GetComponent<Mirror.NetworkIdentity>();
            if (netId == null)
            {
                Debug.LogWarning($"[BCE] {name}.prefab has no NetworkIdentity — skipping.");
                missing++;
                continue;
            }

            // Force-reimport so NetworkIdentity.OnValidate() runs and sets _assetId
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            fixed_++;
            Debug.Log($"[BCE] Reimported {name}.prefab — _assetId will be written by Mirror.");
        }

        // Also reimport extra networked prefabs (ChatManager, etc.)
        foreach (var path in ExtraPrefabs)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) { Debug.LogWarning($"[BCE] Extra prefab not found: {path}"); missing++; continue; }
            var netId = prefab.GetComponent<Mirror.NetworkIdentity>();
            if (netId == null) { Debug.LogWarning($"[BCE] {path} has no NetworkIdentity — skipping."); missing++; continue; }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            fixed_++;
            Debug.Log($"[BCE] Reimported {path}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Read back and report what was written
        string report = "";
        foreach (var name in ClassNames)
        {
            string path = $"{PREFABS_DIR}/{name}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) { report += $"  {name}: NOT FOUND\n"; continue; }
            var netId = prefab.GetComponent<Mirror.NetworkIdentity>();
            uint id = netId != null ? netId.assetId : 0;
            string status = id != 0 ? "✅" : "❌ still 0 — check Mirror version";
            report += $"  {name}: assetId={id}  {status}\n";
        }
        foreach (var path in ExtraPrefabs)
        {
            string shortName = System.IO.Path.GetFileNameWithoutExtension(path);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) { report += $"  {shortName}: NOT FOUND\n"; continue; }
            var netId = prefab.GetComponent<Mirror.NetworkIdentity>();
            uint id = netId != null ? netId.assetId : 0;
            string status = id != 0 ? "✅" : "❌ still 0 — check Mirror version";
            report += $"  {shortName}: assetId={id}  {status}\n";
        }

        string msg = fixed_ > 0
            ? $"Force-reimported {fixed_} prefab(s).\n\n{report}\n" +
              "Commit the changed .prefab files — the fix is permanent."
            : $"No prefabs found in {PREFABS_DIR}.";

        if (missing > 0) msg += $"\n\n{missing} prefab(s) were missing or had no NetworkIdentity.";

        EditorUtility.DisplayDialog("Class Prefab Asset IDs", msg, "OK");
    }

    [MenuItem("BCE/Setup/4z ▶ Fix Class Prefab Asset IDs", true)]
    static bool FixAssetIds_Validate() => !Application.isPlaying;
}
#endif
