using UnityEngine;

/// <summary>
/// LoginBackgroundFX — Blizzard-style login camera.
///
/// The camera barely moves. You feel like you're STANDING in the world —
/// a slow breath left, right, up, down. 90-second cycle. Imperceptible
/// unless you look for it. Depth comes from the VFX in front of you,
/// not from camera spinning.
///
/// Attach to LoginBG in LoginScene alongside LoginScreenVFX.
/// </summary>
[AddComponentMenu("BCE/UI/Login Background FX")]
public class LoginBackgroundFX : MonoBehaviour
{
    [Header("Camera Starting Position")]
    public Vector3 cameraPos  = new Vector3(0f, 2.5f, -6f); // standing back, looking in
    public Vector3 lookTarget = new Vector3(0f, 1f,  8f);   // portal + circles focal point

    [Header("Drift (Blizzard feel — barely moves)")]
    public float driftX      = 1.2f;   // units left/right over full cycle
    public float driftY      = 0.25f;  // units up/down (breath)
    public float driftPeriod = 90f;    // seconds per full left->right->left cycle

    [Header("Atmosphere")]
    public Color fogColor     = new Color(0.02f, 0.01f, 0.05f);
    public Color ambientColor = new Color(0.04f, 0.03f, 0.09f);

    Camera _cam;
    float  _t;

    void Start()
    {
        _cam = Camera.main;
        if (_cam == null) return;

        _cam.clearFlags      = CameraClearFlags.SolidColor;
        _cam.backgroundColor = new Color(0.01f, 0.01f, 0.03f);
        _cam.fieldOfView     = 55f;
        _cam.nearClipPlane   = 0.1f;
        _cam.farClipPlane    = 200f;

        // Place camera immediately so there's no pop on first frame
        _cam.transform.position = cameraPos;
        _cam.transform.LookAt(lookTarget);

        RenderSettings.fog         = true;
        RenderSettings.fogMode     = FogMode.Exponential;
        RenderSettings.fogDensity  = 0.025f;
        RenderSettings.fogColor    = fogColor;
        RenderSettings.ambientLight = ambientColor;
    }

    void Update()
    {
        if (_cam == null) return;

        _t += Time.deltaTime;

        float cycle = (_t / driftPeriod) * Mathf.PI * 2f;

        // Camera drifts gently — one slow breath per driftPeriod seconds
        float x = cameraPos.x + Mathf.Sin(cycle)        * driftX;
        float y = cameraPos.y + Mathf.Sin(cycle * 1.3f) * driftY;

        _cam.transform.position = new Vector3(x, y, cameraPos.z);

        // Look target trails the camera drift slightly — natural parallax feel
        Vector3 look = new Vector3(
            lookTarget.x + Mathf.Sin(cycle * 0.7f) * 0.4f,
            lookTarget.y,
            lookTarget.z
        );
        _cam.transform.LookAt(look);
    }
}
