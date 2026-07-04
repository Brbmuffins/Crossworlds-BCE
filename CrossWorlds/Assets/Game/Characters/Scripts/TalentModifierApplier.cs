// COPY TO: Assets/Game/Characters/Scripts/TalentModifierApplier.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TalentModifierApplier — fetches invested talents on scene load and applies
/// stat modifiers to the local player only.
///
/// Modifier types (from /api/talents/:characterId response):
///   damage_pct       → CharacterStats.DamageMultiplier += value
///   cdr_pct          → CharacterStats.CooldownReduction += value
///   heal_pct         → CharacterStats.HealMultiplier += value
///   shield_pct       → CharacterStats.ShieldMultiplier += value
///   cooldown_flat    → CharacterStats.FlatCooldownReduction += value
///   hp_flat          → CharacterStats.BonusMaxHp += value  (Health picks this up)
///   deployable_limit → CharacterStats.DeployableLimit += (int)value
///
/// Call ApplyTalents() again after a respec to zero and re-apply.
/// </summary>
public class TalentModifierApplier : MonoBehaviour
{
    public static TalentModifierApplier Instance { get; private set; }

    // ── Bootstrap ─────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
#if !UNITY_SERVER
        if (Instance != null) return;
        var go = new GameObject("[TalentModifierApplier]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<TalentModifierApplier>();
#endif
    }

    // ── State ─────────────────────────────────────────────────────────────────
    private int _characterId;
    private bool _applied = false;

    string ServerUrl => $"http://{PlayerPrefs.GetString("serverIP", "localhost")}:3000";
    string Token     => PlayerPrefs.GetString("jwt_token", "");

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Start()
    {
#if !UNITY_SERVER
        _characterId = PlayerPrefs.GetInt("SelectedCharacter", 0);
        if (_characterId > 0)
            StartCoroutine(FetchAndApply());
#endif
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Call after respec to zero all modifiers then re-apply.</summary>
    public void ReapplyTalents()
    {
#if !UNITY_SERVER
        ZeroAllModifiers();
        _applied = false;
        StartCoroutine(FetchAndApply());
#endif
    }

    // ── Fetch ─────────────────────────────────────────────────────────────────
    IEnumerator FetchAndApply()
    {
        if (_applied) yield break;

        using var req = UnityWebRequest.Get($"{ServerUrl}/api/talents/{_characterId}");
        req.SetRequestHeader("Authorization", $"Bearer {Token}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[TALENT] Failed to fetch talents: {req.error}");
            yield break;
        }

        var wrapper = JsonUtility.FromJson<TalentListResponse>(req.downloadHandler.text);
        if (wrapper == null || !wrapper.success)
        {
            Debug.LogWarning($"[TALENT] Server returned failure on talent fetch");
            yield break;
        }

        ApplyAll(wrapper.data);
        _applied = true;
        Debug.Log($"[TALENT] Applied {wrapper.data.Length} talent modifiers to local player");
    }

    // ── Apply ─────────────────────────────────────────────────────────────────
    void ApplyAll(TalentData[] talents)
    {
        var stats = CharacterStats.Local;
        if (stats == null)
        {
            Debug.LogWarning("[TALENT] CharacterStats.Local not found — will retry next frame");
            StartCoroutine(RetryApply(talents));
            return;
        }

        foreach (var t in talents)
        {
            if (!t.invested) continue;
            ApplySingle(stats, t.modifier_type, t.modifier_value);
        }
    }

    void ApplySingle(CharacterStats stats, string modifierType, float value)
    {
        switch (modifierType)
        {
            case "damage_pct":
                stats.DamageMultiplier += value;
                break;
            case "cdr_pct":
                stats.CooldownReduction += value;
                break;
            case "heal_pct":
                stats.HealMultiplier += value;
                break;
            case "shield_pct":
                stats.ShieldMultiplier += value;
                break;
            case "cooldown_flat":
                stats.FlatCooldownReduction += value;
                break;
            case "hp_flat":
                stats.BonusMaxHp += value;
                // Notify Health component to recompute max HP
                var health = FindLocalPlayerHealth();
                if (health != null) health.RefreshMaxHp();
                break;
            case "deployable_limit":
                stats.DeployableLimit += Mathf.RoundToInt(value);
                break;
            default:
                Debug.LogWarning($"[TALENT] Unknown modifier_type: {modifierType}");
                break;
        }
    }

    void ZeroAllModifiers()
    {
        var stats = CharacterStats.Local;
        if (stats == null) return;
        stats.DamageMultiplier      = 1f;
        stats.CooldownReduction     = 0f;
        stats.HealMultiplier        = 1f;
        stats.ShieldMultiplier      = 1f;
        stats.FlatCooldownReduction = 0f;
        stats.BonusMaxHp            = 0f;
        stats.DeployableLimit       = stats.BaseDeployableLimit;
    }

    Health FindLocalPlayerHealth()
    {
        // Only call when needed — not per frame
        var identities = FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var id in identities)
            if (id.isLocalPlayer) return id.GetComponent<Health>();
        return null;
    }

    IEnumerator RetryApply(TalentData[] talents)
    {
        yield return new WaitForSeconds(1f);
        ApplyAll(talents);
    }

    // ── JSON Types ────────────────────────────────────────────────────────────
    [System.Serializable] class TalentListResponse { public bool success; public TalentData[] data; }
    [System.Serializable] public class TalentData
    {
        public int    id;
        public string name;
        public string modifier_type;
        public float  modifier_value;
        public bool   invested;
        public int    tier;
        public int    branch;
        public int    prerequisite_talent_id;
    }
}

// ── CharacterStats ────────────────────────────────────────────────────────────
/// <summary>
/// Singleton stat container for the local player.
/// Attach to the local player prefab, or let TalentModifierApplier create it.
/// AbilityCaster reads DamageMultiplier and CooldownReduction each cast.
/// </summary>
public class CharacterStats : MonoBehaviour
{
    public static CharacterStats Local { get; private set; }

    [Header("Multipliers (base = 1.0)")]
    public float DamageMultiplier      = 1f;
    public float HealMultiplier        = 1f;
    public float ShieldMultiplier      = 1f;

    [Header("Cooldown")]
    public float CooldownReduction     = 0f;   // percentage, e.g. 0.15 = 15% CDR
    public float FlatCooldownReduction = 0f;   // seconds shaved per cast

    [Header("HP Bonus")]
    public float BonusMaxHp            = 0f;

    [Header("Deployables")]
    public int   BaseDeployableLimit   = 3;
    public int   DeployableLimit       = 3;

    void Awake()
    {
        var identity = GetComponent<PlayerIdentity>();
        if (identity != null && identity.isLocalPlayer)
            Local = this;
    }

    void OnDestroy()
    {
        if (Local == this) Local = null;
    }

    /// <summary>Apply CDR to a raw cooldown value.</summary>
    public float ApplyCDR(float rawCooldown)
    {
        float reduced = rawCooldown * (1f - CooldownReduction) - FlatCooldownReduction;
        return Mathf.Max(0.5f, reduced); // never below 0.5s
    }
}
