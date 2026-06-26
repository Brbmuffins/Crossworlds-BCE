using System.Collections;
using Mirror;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  RodNetworkAuthenticator
//  Attach to the NetworkManager GameObject alongside RodNetworkManager.
//  Assign this component to NetworkManager.authenticator in the Inspector.
//
//  Flow:
//    1. Client connects → OnClientAuthenticate() sends AuthRequest with JWT.
//    2. Server receives it → validates token is present → ServerAccept().
//    3. Client receives AuthResponse → ClientAccept() → Mirror continues.
//    4. RodNetworkManager.OnClientConnect() fires → sends CreatePlayerMessage.
// ═══════════════════════════════════════════════════════════════════════════

[AddComponentMenu("RoD/Network/Rod Network Authenticator")]
public class RodNetworkAuthenticator : NetworkAuthenticator
{
    [Header("Dev Mode")]
    [Tooltip("Bypasses JWT validation. Use in Editor for local host testing only.")]
    public bool devMode = false;

    // ── Message types ────────────────────────────────────────────────────────

    public struct AuthRequestMessage : NetworkMessage
    {
        public string jwt;
        public string username;
        public int    selectedClass;  // 0=Engineer 1=Guardian 2=Wraith 3=Medic
    }

    public struct AuthResponseMessage : NetworkMessage
    {
        public bool   success;
        public string message;
    }

    // ── Server ───────────────────────────────────────────────────────────────

    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequest, false);
    }

    public override void OnStopServer()
    {
        NetworkServer.UnregisterHandler<AuthRequestMessage>();
    }

    public override void OnServerAuthenticate(NetworkConnectionToClient conn)
    {
        // Wait for the client to send AuthRequestMessage — do nothing here.
    }

    void OnAuthRequest(NetworkConnectionToClient conn, AuthRequestMessage msg)
    {
        // Dev mode: skip all validation, accept anyone.
        if (devMode)
        {
            conn.authenticationData = new RodPlayerAuth
            {
                username      = string.IsNullOrEmpty(msg.username) ? "DevPlayer" : msg.username,
                selectedClass = msg.selectedClass,
                jwt           = "dev"
            };
            conn.Send(new AuthResponseMessage { success = true, message = "Dev mode — accepted." });
            ServerAccept(conn);
            return;
        }

        // Basic gate: must have a token and a username.
        if (string.IsNullOrEmpty(msg.jwt) || string.IsNullOrEmpty(msg.username))
        {
            conn.Send(new AuthResponseMessage
            {
                success = false,
                message = "Missing credentials. Please log in first."
            });
            StartCoroutine(DelayedReject(conn, 1f));
            return;
        }

        // Store auth data on the connection so RodNetworkManager can read it
        // when spawning the player object.
        conn.authenticationData = new RodPlayerAuth
        {
            username      = msg.username,
            selectedClass = msg.selectedClass,
            jwt           = msg.jwt
        };

        conn.Send(new AuthResponseMessage
        {
            success = true,
            message = "Authenticated."
        });

        ServerAccept(conn);
    }

    IEnumerator DelayedReject(NetworkConnectionToClient conn, float delay)
    {
        yield return new WaitForSeconds(delay);
        ServerReject(conn);
    }

    // ── Client ───────────────────────────────────────────────────────────────

    public override void OnStartClient()
    {
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponse, false);
    }

    public override void OnStopClient()
    {
        NetworkClient.UnregisterHandler<AuthResponseMessage>();
    }

    public override void OnClientAuthenticate()
    {
        NetworkClient.Send(new AuthRequestMessage
        {
            jwt           = devMode ? "dev" : PlayerPrefs.GetString("jwt_token", ""),
            username      = PlayerPrefs.GetString("username", "DevPlayer"),
            selectedClass = PlayerPrefs.GetInt("SelectedCharacter", 0)
        });
    }

    void OnAuthResponse(AuthResponseMessage msg)
    {
        if (msg.success)
        {
            ClientAccept();
        }
        else
        {
            Debug.LogError("[RodAuth] Server rejected connection: " + msg.message);
            ClientReject();
        }
    }
}

// ── Auth data stored on each server connection ────────────────────────────
// RodNetworkManager reads this during OnServerAddPlayer to spawn the right prefab.
public class RodPlayerAuth
{
    public string username;
    public int    selectedClass;
    public string jwt;
}
