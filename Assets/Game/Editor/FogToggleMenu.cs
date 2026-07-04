#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FogToggleMenu
{
    const string MenuPath = "Rate of Decay/World/Toggle Fog";

    [MenuItem(MenuPath)]
    public static void ToggleFog()
    {
        RenderSettings.fog = !RenderSettings.fog;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        string state = RenderSettings.fog ? "ON" : "OFF";
        Debug.Log($"[Rate of Decay] Fog toggled {state}. Save the scene to keep this setting.");
    }

    [MenuItem(MenuPath, true)]
    public static bool ToggleFogValidate()
    {
        Menu.SetChecked(MenuPath, RenderSettings.fog);
        return true;
    }
}
#endif
