using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// PlayerEquipment — server-authoritative networked equipment on the player.
///
/// State: a SyncDictionary of slot → serverItemId, replicated to every client (and to
/// late joiners via the spawn payload). When it changes, each client attaches / removes
/// the item's world model on the matching EquipmentRig mount. This is a cosmetic child
/// spawned LOCALLY on each client (not NetworkServer.Spawn) — cheap, no extra
/// NetworkIdentities, and everyone stays in sync because the driving dictionary is synced.
///
/// Persistence to the auth DB is handled separately by InventoryManager
/// (POST /api/inventory/equip) on the owning client. This component only owns the shared
/// net + visual state.
///
/// Mirror discipline:
///   • All mutations of _equipped happen inside [Server] methods.
///   • Owner client requests changes via [Command]s.
///   • Visual instantiation is compiled out of the dedicated-server build
///     (#if UNITY_EDITOR || !UNITY_SERVER) — the server never needs the models.
/// </summary>
[RequireComponent(typeof(EquipmentRig))]
public class PlayerEquipment : NetworkBehaviour
{
    /// The local player's equipment (owner client only). Null on remote copies and the server.
    public static PlayerEquipment Local { get; private set; }

    // slotInt ((int)EquipmentSlotType) → serverItemId. Server-authoritative.
    readonly SyncDictionary<int, string> _equipped = new SyncDictionary<int, string>();

    EquipmentRig _rig;

    // Client-only: currently instantiated models, per slot.
    readonly Dictionary<int, GameObject> _visuals = new Dictionary<int, GameObject>();

    void Awake() => _rig = GetComponent<EquipmentRig>();

    // ── Server authority ──────────────────────────────────────────────────────
    [Server]
    public void ServerEquip(string serverItemId)
    {
        var catalog = EquipmentCatalog.Instance;
        if (catalog == null) return;

        var slot = catalog.GetSlot(serverItemId);
        if (slot == EquipmentSlotType.None)
        {
            Debug.LogWarning($"[EQUIP] '{serverItemId}' is not in the EquipmentCatalog — ignored.");
            return;
        }
        _equipped[(int)slot] = serverItemId;   // replaces whatever was in that slot
    }

    [Server]
    public void ServerUnequip(EquipmentSlotType slot)
    {
        _equipped.Remove((int)slot);
    }

    // ── Owner → server requests ─────────────────────────────────────────────────
    [Command]
    void CmdEquip(string serverItemId) => ServerEquip(serverItemId);

    [Command]
    void CmdUnequip(int slotInt) => ServerUnequip((EquipmentSlotType)slotInt);

    /// Call on the owning client to equip an item (drives shared visuals for everyone).
    public void RequestEquip(string serverItemId)
    {
        if (!isOwned || string.IsNullOrEmpty(serverItemId)) return;
        CmdEquip(serverItemId);
    }

    public void RequestUnequip(EquipmentSlotType slot)
    {
        if (!isOwned || slot == EquipmentSlotType.None) return;
        CmdUnequip((int)slot);
    }

    // ── Lifecycle ───────────────────────────────────────────────────────────────
    public override void OnStartLocalPlayer()
    {
        Local = this;
#if UNITY_EDITOR || !UNITY_SERVER
        StartCoroutine(ApplyEquippedWhenReady());
#endif
    }

    public override void OnStopLocalPlayer()
    {
        if (Local == this) Local = null;
    }

    public override void OnStartClient()
    {
        _equipped.OnAdd    += OnSlotAdded;
        _equipped.OnSet    += OnSlotSet;
        _equipped.OnRemove += OnSlotRemoved;

        // Render whatever arrived in the spawn payload (already-equipped items).
        foreach (var kv in _equipped)
            RefreshVisual(kv.Key, kv.Value);
    }

    public override void OnStopClient()
    {
        _equipped.OnAdd    -= OnSlotAdded;
        _equipped.OnSet    -= OnSlotSet;
        _equipped.OnRemove -= OnSlotRemoved;

        foreach (var go in _visuals.Values)
            if (go != null) Destroy(go);
        _visuals.Clear();
    }

    // SyncDictionary callbacks. Note: OnSet passes the OLD value, so we read the current
    // value straight out of the dictionary instead of trusting the argument.
    void OnSlotAdded(int slotInt)
    {
        _equipped.TryGetValue(slotInt, out var id);
        RefreshVisual(slotInt, id);
    }

    void OnSlotSet(int slotInt, string _oldValue)
    {
        _equipped.TryGetValue(slotInt, out var id);
        RefreshVisual(slotInt, id);
    }

    void OnSlotRemoved(int slotInt, string _oldValue) => RemoveVisual(slotInt);

    // ── Visuals (client only) ────────────────────────────────────────────────────
    void RefreshVisual(int slotInt, string serverItemId)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        RemoveVisual(slotInt);

        var catalog = EquipmentCatalog.Instance;
        var def = catalog != null ? catalog.Get(serverItemId) : null;
        if (def == null || def.worldModelPrefab == null) return;   // stat-only gear: no model

        var slot  = (EquipmentSlotType)slotInt;
        var mount = _rig != null ? _rig.GetMount(slot) : null;
        if (mount == null)
        {
            Debug.LogWarning($"[EQUIP] No rig mount for slot {slot} on {name} — model not shown.");
            return;
        }

        var go = Instantiate(def.worldModelPrefab, mount);
        go.transform.localPosition = def.attachPosition;
        go.transform.localRotation = Quaternion.Euler(def.attachEuler);
        go.transform.localScale    = def.attachScale;
        go.name = $"Equip_{slot}_{serverItemId}";
        _visuals[slotInt] = go;
#endif
    }

    void RemoveVisual(int slotInt)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        if (_visuals.TryGetValue(slotInt, out var go))
        {
            if (go != null) Destroy(go);
            _visuals.Remove(slotInt);
        }
#endif
    }

#if UNITY_EDITOR || !UNITY_SERVER
    // On spawn, re-apply whatever the DB says is equipped (InventoryManager loads it async).
    IEnumerator ApplyEquippedWhenReady()
    {
        float waited = 0f;
        while (InventoryManager.Instance == null && waited < 10f)
        {
            yield return new WaitForSeconds(0.25f);
            waited += 0.25f;
        }
        // Give the inventory's initial REST load a moment to populate.
        yield return new WaitForSeconds(0.75f);

        var inv = InventoryManager.Instance;
        if (inv == null) yield break;

        foreach (var s in inv.GetSlots())
            if (s.equipped == 1 && !string.IsNullOrEmpty(s.item_id))
                RequestEquip(s.item_id);
    }
#endif
}
