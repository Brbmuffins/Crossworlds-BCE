using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RateOfDecaySceneShortcuts
{
    const string MenuRoot = "Rate of Decay/Scenes/";

    [MenuItem(MenuRoot + "Login Scene", priority = 10)]
    public static void OpenLoginScene() => OpenScene("Assets/Game/Scenes/LoginScene.unity", "Login Scene");

    [MenuItem(MenuRoot + "Character Select", priority = 11)]
    public static void OpenCharacterSelect() => OpenScene("Assets/Game/Scenes/CharacterSelect.unity", "Character Select");

    [MenuItem(MenuRoot + "HUB", priority = 12)]
    public static void OpenHub() => OpenScene("Assets/Game/Scenes/HUB.unity", "HUB");

    [MenuItem(MenuRoot + "Darkwood", priority = 20)]
    public static void OpenDarkwood() => OpenScene("Assets/Game/Scenes/Darkwood.unity", "Darkwood");

    [MenuItem(MenuRoot + "Ashen Wastelands", priority = 21)]
    public static void OpenAshenWastelands() => OpenScene("Assets/Game/Scenes/Ashen Wastelands.unity", "Ashen Wastelands");

    [MenuItem(MenuRoot + "Toujam Basin", priority = 22)]
    public static void OpenToujamBasin() => OpenScene("Assets/Game/Scenes/Toujam Basin.unity", "Toujam Basin");

    [MenuItem(MenuRoot + "Boneyard", priority = 23)]
    public static void OpenBoneyard() => OpenScene("Assets/Game/Scenes/Boneyard.unity", "Boneyard");

    [MenuItem(MenuRoot + "GM Island", priority = 24)]
    public static void OpenGMIsland() => OpenScene("Assets/Game/Scenes/GM Island.unity", "GM Island");

    [MenuItem(MenuRoot + "Void Dungeon", priority = 25)]
    public static void OpenVoidDungeon() => OpenScene("Assets/Game/Scenes/VoidDungeon.unity", "Void Dungeon");

    static void OpenScene(string scenePath, string label)
    {
        if (!File.Exists(scenePath))
        {
            EditorUtility.DisplayDialog("Scene not found", $"{label} was not found at:\n{scenePath}", "OK");
            Debug.LogWarning($"[RateOfDecay] Scene not found: {scenePath}");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    }
}
