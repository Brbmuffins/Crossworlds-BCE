public partial class RodPlayerAuth
{
    public const float DefaultFreeCameraSpeed = 8f;
    public const float MinFreeCameraSpeed = 0.25f;
    public const float MaxFreeCameraSpeed = 100f;

    public bool gmAllowed;
    public bool gmActive;
    public bool gmFlyEnabled;
    public bool gmFreeCameraEnabled;
    public int gmLevel;
    public float gmSpeedMultiplier = 1f;
    public float gmFreeCameraSpeed = DefaultFreeCameraSpeed;
    public string gmPermissions;
}
