using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
[AddComponentMenu("BCE/World/Network Vendor")]
public sealed class NetworkVendor : NetworkBehaviour, INPCInteractable
{
    public VendorProfile profile;
    bool _registered;

    public string DisplayName => profile != null && !string.IsNullOrWhiteSpace(profile.displayName)
        ? profile.displayName.Trim() : "Merchant";
    public string PromptText => $"[E] Browse {DisplayName}";

    public override void OnStartServer()
    {
        base.OnStartServer();
        VendorPersistenceService.SyncProfile(profile);
    }

#if UNITY_EDITOR || !UNITY_SERVER
    void Update()
    {
        if (!isClient || NetworkClient.localPlayer == null || profile == null) return;
        float radius = Mathf.Max(1f, profile.interactionRadius);
        bool nearby = (NetworkClient.localPlayer.transform.position - transform.position).sqrMagnitude <= radius * radius;
        if (nearby == _registered) return;
        _registered = nearby;
        if (nearby) NPCInteractionManager.Instance?.RegisterNearby(this);
        else NPCInteractionManager.Instance?.UnregisterNearby(this);
    }

    void OnDisable()
    {
        if (_registered) NPCInteractionManager.Instance?.UnregisterNearby(this);
        _registered = false;
    }

    public void Interact()
    {
        if (_registered && profile != null) VendorShopUI.EnsureInstance().Open(this);
    }

    public void RequestBuy(string itemId, int quantity) => CmdBuy(itemId, Mathf.Clamp(quantity, 1, 99));
    public void RequestSell(int slotIndex, int quantity) => CmdSell(slotIndex, Mathf.Clamp(quantity, 1, 99));
#else
    public void Interact() { }
#endif

    [Command(requiresAuthority = false)]
    void CmdBuy(string itemId, int quantity, NetworkConnectionToClient sender = null)
    {
        if (!ValidateRequest(sender, itemId, quantity)) return;
        VendorPersistenceService.Buy(sender, profile, itemId.Trim(), quantity, SendResult);
    }

    [Command(requiresAuthority = false)]
    void CmdSell(int slotIndex, int quantity, NetworkConnectionToClient sender = null)
    {
        if (profile == null || !profile.buysItems || sender?.identity == null ||
            slotIndex < 0 || slotIndex >= 24 || quantity < 1 || quantity > 99 || !IsNear(sender))
        {
            if (sender != null) TargetVendorResult(sender, false, "Sale rejected.");
            return;
        }
        VendorPersistenceService.Sell(sender, profile, slotIndex, quantity, SendResult);
    }

    [Server]
    bool ValidateRequest(NetworkConnectionToClient sender, string itemId, int quantity)
    {
        if (profile == null || sender?.identity == null || string.IsNullOrWhiteSpace(itemId) ||
            quantity < 1 || quantity > 99 || !IsNear(sender))
        {
            if (sender != null) TargetVendorResult(sender, false, "Purchase rejected.");
            return false;
        }
        return true;
    }

    [Server]
    bool IsNear(NetworkConnectionToClient sender)
    {
        float radius = Mathf.Max(1f, profile.interactionRadius) + 1f;
        return (sender.identity.transform.position - transform.position).sqrMagnitude <= radius * radius;
    }

    [Server]
    void SendResult(NetworkConnectionToClient target, bool success, string message)
    {
        if (target != null) TargetVendorResult(target, success, message ?? "Vendor request completed.");
    }

    [TargetRpc]
    void TargetVendorResult(NetworkConnection target, bool success, string message)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        VendorShopUI.EnsureInstance().CompleteTransaction(success, message);
#endif
    }
}
