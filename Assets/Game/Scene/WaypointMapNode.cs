using System;
using UnityEngine;

[Serializable]
public sealed class WaypointMapNode
{
    public string id = "darkwood";
    public string displayName = "DARKWOOD";
    public string subtitle = "";
    public string sceneName = "";

    [Tooltip("Optional named arrival marker to place players at after loading this scene.")]
    public string arrivalSpawnId = "";

    [Tooltip("Face players the same way as the arrival marker after travel.")]
    public bool useArrivalSpawnRotation = true;

    public bool unlocked = true;
    public Vector2 normalizedPosition = new Vector2(0.5f, 0.5f);
    public Vector2 labelOffset = new Vector2(0f, -36f);
    public Color color = Color.white;
    public Sprite icon;

    [TextArea(2, 4)]
    public string description = "";

    public bool CanTravel => unlocked && !string.IsNullOrWhiteSpace(sceneName);
}

[Serializable]
public sealed class WaypointMapConnection
{
    public string fromNodeId;
    public string toNodeId;
    public bool dashed;
    public Color color = new Color(0.55f, 0.5f, 0.44f, 0.65f);
}
