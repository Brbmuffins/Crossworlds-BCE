using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// ═══════════════════════════════════════════════════════════════════════════
//  RodPositionSaver
//  Added at runtime by RodNetworkManager to the server-side player object.
//  Saves the player's last position AND zone to the auth server DB.
//
//  Fires PATCH /character/position on:
//    - a periodic tick while the player is connected (crash safety)
//    - SaveNow() when the player changes zone (called by ZoneManager, ROADMAP 6.4)
//    - OnDestroy (player object cleaned up on server after disconnect)
//    - OnApplicationQuit (server shutdown)
//
//  ZONE (ROADMAP 6.2): the "map" field used to be the hardcoded literal
//  "GameWorld", so every character in the DB claimed to be in the same
//  nonexistent map and "which zone was I in?" could not be answered on login.
//  It now reports the scene the player object actually lives in — which is the
//  active scene today, and the additively-loaded zone scene after ROADMAP 6.3.
//
//  CRASH SAFETY (ROADMAP 6.2): saving only on OnDestroy/OnApplicationQuit meant
//  a kill -9 or an OOM lost every online player's position. The periodic tick
//  bounds that loss to one interval.
// ═══════════════════════════════════════════════════════════════════════════

public class RodPositionSaver : MonoBehaviour
{
    [HideInInspector] public int    characterId;
    [HideInInspector] public string authServerURL;
    [HideInInspector] public string jwt;

    [Tooltip("Seconds between periodic position saves while connected.")]
    public float saveInterval = 45f;

    bool _saved;          // guards the terminal save only, not periodic ticks
    string _lastZone;

    void Start()
    {
        _lastZone = ResolveZoneForSave();
        StartCoroutine(PeriodicSaveLoop());
    }

    void OnDestroy()    => TrySave();
    void OnApplicationQuit() { TrySave(); }

    /// <summary>
    /// Zone to persist for this player. Prefers the scene the server-side player
    /// object is actually in, and remembers it.
    ///
    /// The fallback matters: during OnDestroy and OnApplicationQuit the scene may
    /// already be tearing down, so gameObject.scene.name can come back empty. Naively
    /// normalizing that would write Hub over everyone's real zone at shutdown — the
    /// one save where being right counts most. So an unusable live value falls back
    /// to the last zone we positively observed, not to Hub.
    /// </summary>
    string ResolveZoneForSave()
    {
        string live = gameObject.scene.name;
        if (SceneNames.IsZone(live))
        {
            _lastZone = live;
            return live;
        }

        return SceneNames.NormalizeZone(_lastZone);
    }

    /// <summary>
    /// Immediate save. Call before moving a player between zones so the DB never
    /// holds the new zone with the old zone's coordinates (or vice versa).
    /// </summary>
    public void SaveNow()
    {
        if (!CanSave()) return;
        _lastZone = ResolveZoneForSave();
        StartCoroutine(PositionSaveRoutine.Save(
            authServerURL, jwt, characterId, transform.position, transform.eulerAngles.y, _lastZone,
            isLogout: false));
    }

    IEnumerator PeriodicSaveLoop()
    {
        // Stagger the first tick so N players who joined together don't all hit
        // the auth server on the same frame.
        yield return new WaitForSeconds(Random.Range(0f, saveInterval));

        while (true)
        {
            yield return new WaitForSeconds(saveInterval);
            if (!CanSave()) continue;

            _lastZone = ResolveZoneForSave();
            yield return PositionSaveRoutine.Save(
                authServerURL, jwt, characterId, transform.position, transform.eulerAngles.y, _lastZone,
                isLogout: false);
        }
    }

    bool CanSave() => characterId > 0 && !string.IsNullOrEmpty(jwt) && jwt != "dev";

    void TrySave()
    {
        if (_saved) return;
        if (!CanSave()) return;
        _saved = true;

        // Can't use coroutines after OnDestroy — hand off to a detached host.
        SavePosition(authServerURL, jwt, characterId,
            transform.position, transform.eulerAngles.y, ResolveZoneForSave(), isLogout: true);
    }

    // Static so it can survive the MonoBehaviour being destroyed
    public static void SavePosition(string url, string jwt, int charId,
        Vector3 pos, float orientation, string zone, bool isLogout)
    {
        // Fire-and-forget via a temporary GameObject coroutine host
        var host = new GameObject("_PositionSaveRequest");
        DontDestroyOnLoad(host);
        host.AddComponent<PositionSaveHost>().Run(url, jwt, charId, pos, orientation, zone, isLogout);
    }
}

// ── Shared request builder ────────────────────────────────────────────────

static class PositionSaveRoutine
{
    public static IEnumerator Save(string url, string jwt, int charId,
        Vector3 pos, float orientation, string zone, bool isLogout)
    {
        // Force invariant culture so floats always serialize with a '.' decimal
        // separator. On a build whose OS locale uses ',' (much of Europe) the old
        // interpolation produced "x":1,234 — invalid JSON — and the save silently failed.
        var ic = System.Globalization.CultureInfo.InvariantCulture;
        string json = "{" +
                      $"\"x\":{pos.x.ToString("F3", ic)}," +
                      $"\"y\":{pos.y.ToString("F3", ic)}," +
                      $"\"z\":{pos.z.ToString("F3", ic)}," +
                      $"\"map\":\"{zone}\"," +
                      $"\"orientation\":{orientation.ToString("F3", ic)}," +
                      $"\"logout\":{(isLogout ? "true" : "false")}}}";

        using var req = new UnityWebRequest($"{url}/character/position", "PATCH");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + jwt);
        req.timeout = 5;

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            Debug.Log($"[RodPositionSaver] Saved char {charId} at {pos} in {zone}");
        else
            Debug.LogWarning($"[RodPositionSaver] Failed to save position: {req.error}");
    }
}

// ── Temporary coroutine host for saves that outlive the player object ─────

class PositionSaveHost : MonoBehaviour
{
    public void Run(string url, string jwt, int charId, Vector3 pos, float orientation, string zone,
        bool isLogout)
    {
        StartCoroutine(RunThenDestroy(url, jwt, charId, pos, orientation, zone, isLogout));
    }

    IEnumerator RunThenDestroy(string url, string jwt, int charId, Vector3 pos, float orientation,
        string zone, bool isLogout)
    {
        yield return PositionSaveRoutine.Save(url, jwt, charId, pos, orientation, zone, isLogout);
        Destroy(gameObject);
    }
}
