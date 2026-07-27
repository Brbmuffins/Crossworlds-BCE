using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class QuestGiver : MonoBehaviour, INPCInteractable
{
    public List<QuestDefinition> quests = new();
    public Transform markerAnchor;
    [Min(0.5f)] public float interactionRadius = 3f;
    TextMeshPro _marker;
    QuestDefinition _current;
    public string PromptText => _current != null ? $"Press E to discuss {_current.title}" : "Press E to talk";

    void Awake()
    {
        foreach (QuestDefinition quest in quests) QuestLocalRuntime.RegisterDefinition(quest);
        bool hasTrigger = false;
        foreach (Collider collider in GetComponents<Collider>())
            hasTrigger |= collider.isTrigger;
        if (!hasTrigger)
        {
            var sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = interactionRadius;
            sphere.isTrigger = true;
        }
        if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
            BuildMarker();
    }

    void OnEnable() { QuestLocalRuntime.StateChanged += RefreshMarker; RefreshMarker(); }
    void OnDisable()
    {
        QuestLocalRuntime.StateChanged -= RefreshMarker;
        NPCInteractionManager.Instance?.UnregisterNearby(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsLocalPlayer(other)) return;
        RefreshMarker();
        NPCInteractionManager.Instance?.RegisterNearby(this);
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsLocalPlayer(other)) return;
        NPCInteractionManager.Instance?.UnregisterNearby(this);
        QuestLocalDialogue.Hide();
    }

    public void Interact()
    {
        RefreshMarker();
        if (_current != null) QuestLocalDialogue.Show(_current);
    }

    void RefreshMarker()
    {
        _current = ChooseQuest();
        if (_marker == null) return;
        if (_current == null) { _marker.gameObject.SetActive(false); return; }
        LocalQuestStatus status = QuestLocalRuntime.Instance.GetStatus(_current);
        _marker.gameObject.SetActive(status == LocalQuestStatus.Available ||
                                     status == LocalQuestStatus.ReadyToTurnIn);
        _marker.text = "?";
        _marker.color = new Color(1f, 0.82f, 0.08f);
    }

    QuestDefinition ChooseQuest()
    {
        if (QuestLocalRuntime.Instance == null) return null;
        QuestDefinition active = null;
        foreach (QuestDefinition quest in quests)
        {
            if (quest == null) continue;
            QuestLocalRuntime.Instance.Register(quest);
            LocalQuestStatus status = QuestLocalRuntime.Instance.GetStatus(quest);
            if (status == LocalQuestStatus.ReadyToTurnIn) return quest;
            if (status == LocalQuestStatus.Active) active = quest;
            else if (status == LocalQuestStatus.Available && active == null) active = quest;
        }
        return active;
    }

    void BuildMarker()
    {
        Transform existing = null;
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
            if (child.name == "QuestMarker") { existing = child; break; }
        GameObject go = existing != null ? existing.gameObject : new GameObject("QuestMarker");
        if (existing == null)
        {
            go.transform.SetParent(markerAnchor != null ? markerAnchor : transform, false);
        }
        if (markerAnchor != null)
            go.transform.localPosition = Vector3.zero;
        else
            PositionMarkerAboveRenderers(go.transform, gameObject, 0.75f, 2.75f);
        _marker = go.GetComponent<TextMeshPro>() ?? go.AddComponent<TextMeshPro>();
        _marker.text = "?";
        _marker.fontSize = 5f;
        _marker.alignment = TextAlignmentOptions.Center;
        _marker.rectTransform.sizeDelta = new Vector2(1.5f, 1.5f);
        if (go.GetComponent<QuestMarkerBillboard>() == null)
            go.AddComponent<QuestMarkerBillboard>();
    }

    static void PositionMarkerAboveRenderers(
        Transform marker, GameObject owner, float clearance, float fallbackHeight)
    {
        Renderer[] renderers = owner.GetComponentsInChildren<Renderer>();
        bool found = false;
        Bounds bounds = default;
        foreach (Renderer renderer in renderers)
        {
            if (renderer.transform == marker || renderer.transform.IsChildOf(marker)) continue;
            if (!found) { bounds = renderer.bounds; found = true; }
            else bounds.Encapsulate(renderer.bounds);
        }
        marker.position = found
            ? new Vector3(bounds.center.x, bounds.max.y + clearance, bounds.center.z)
            : owner.transform.position + Vector3.up * fallbackHeight;
    }

    static bool IsLocalPlayer(Collider other)
    {
        NetworkIdentity identity = other.GetComponentInParent<NetworkIdentity>();
        return identity != null ? identity.isLocalPlayer : other.CompareTag("Player");
    }

    [Server]
    public static bool ServerPlayerIsNearQuest(
        NetworkConnectionToClient sender, QuestDefinition definition)
    {
        if (sender?.identity == null || definition == null) return false;
        foreach (QuestGiver giver in
                 Object.FindObjectsByType<QuestGiver>(FindObjectsInactive.Exclude))
        {
            if (!giver.quests.Contains(definition) ||
                giver.gameObject.scene != sender.identity.gameObject.scene) continue;
            if (Vector3.Distance(giver.transform.position, sender.identity.transform.position) <=
                giver.interactionRadius + 1.5f) return true;
        }
        return false;
    }
}

[ExecuteAlways]
public sealed class QuestMarkerBillboard : MonoBehaviour
{
    void LateUpdate()
    {
        Camera camera = Camera.main;
        if (camera != null) transform.rotation =
            Quaternion.LookRotation(transform.position - camera.transform.position);
    }
}

public sealed class QuestObjectiveMarker : MonoBehaviour
{
    public QuestDefinition quest;
    public string targetId;
    TextMeshPro _marker;

    void Awake()
    {
        FindMarker();
    }

    public void Configure(QuestDefinition definition, string objectiveTargetId)
    {
        quest = definition;
        targetId = objectiveTargetId;
        FindMarker();
        Refresh();
    }

    void OnEnable()
    {
        QuestLocalRuntime.StateChanged += Refresh;
        Refresh();
    }

    void OnDisable() => QuestLocalRuntime.StateChanged -= Refresh;

    void FindMarker()
    {
        foreach (TextMeshPro text in GetComponentsInChildren<TextMeshPro>(true))
            if (text.gameObject.name == "QuestObjectiveMarker") { _marker = text; break; }
    }

    void Refresh()
    {
        if (_marker == null) return;
        if (!Application.isPlaying)
        {
            _marker.gameObject.SetActive(true);
            return;
        }
        if (quest == null || QuestLocalRuntime.Instance == null)
        {
            _marker.gameObject.SetActive(false);
            return;
        }

        bool incomplete = false;
        if (QuestLocalRuntime.Instance.GetStatus(quest) == LocalQuestStatus.Active)
        {
            for (int i = 0; i < quest.objectives.Count; i++)
            {
                QuestObjectiveDefinition objective = quest.objectives[i];
                if (!string.Equals(objective.targetId, targetId,
                        System.StringComparison.OrdinalIgnoreCase)) continue;
                incomplete = QuestLocalRuntime.Instance.GetProgress(quest, i) <
                             Mathf.Max(1, objective.requiredAmount);
                break;
            }
        }
        _marker.gameObject.SetActive(incomplete);
    }
}

public sealed class QuestEnemyTarget : MonoBehaviour
{
    public string enemyTemplateId;
    Health _health;

    public static QuestEnemyTarget EnsureAttached(GameObject enemyObject)
    {
        if (enemyObject == null) return null;
        return enemyObject.GetComponent<QuestEnemyTarget>() ??
               enemyObject.AddComponent<QuestEnemyTarget>();
    }

    void OnEnable()
    {
        EnemyController enemy = GetComponent<EnemyController>();
        if (string.IsNullOrWhiteSpace(enemyTemplateId) && enemy != null)
            enemyTemplateId = enemy.enemyTemplateId;
        if (string.IsNullOrWhiteSpace(enemyTemplateId))
            enemyTemplateId = gameObject.name;
        enemyTemplateId = QuestTargetId.NormalizeEnemy(enemyTemplateId);
        _health = GetComponent<Health>();
        if (_health != null) _health.onKilledBy.AddListener(ReportDeath);
    }
    void OnDisable() { if (_health != null) _health.onKilledBy.RemoveListener(ReportDeath); }
    [Server]
    void ReportDeath(GameObject killer)
    {
        if (!NetworkServer.active) return;
        NetworkIdentity identity = killer != null ? killer.GetComponentInParent<NetworkIdentity>() : null;
        if (identity?.connectionToClient != null)
            QuestLocalRuntime.ServerReport(identity.connectionToClient,
                QuestObjectiveType.KillEnemy, QuestTargetId.NormalizeEnemy(enemyTemplateId), 1);
    }
}

public sealed class QuestInteractableTarget : MonoBehaviour, INPCInteractable
{
    public string targetId;
    public string promptText = "Press E to interact";
    [Min(0.5f)] public float interactionRadius = 3f;
    public string PromptText => promptText;

    void Awake()
    {
        bool hasTrigger = false;
        foreach (Collider collider in GetComponents<Collider>())
            hasTrigger |= collider.isTrigger;
        if (!hasTrigger)
        {
            var sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = interactionRadius;
            sphere.isTrigger = true;
        }
    }
    void OnTriggerEnter(Collider other) { if (IsLocal(other)) NPCInteractionManager.Instance?.RegisterNearby(this); }
    void OnTriggerExit(Collider other) { if (IsLocal(other)) NPCInteractionManager.Instance?.UnregisterNearby(this); }
    public void Interact() => QuestLocalRuntime.RequestInteraction(targetId);
    static bool IsLocal(Collider other)
    {
        NetworkIdentity identity = other.GetComponentInParent<NetworkIdentity>();
        return identity != null ? identity.isLocalPlayer : other.CompareTag("Player");
    }
}

public sealed class QuestAreaTarget : MonoBehaviour
{
    public string areaId;
    void OnTriggerEnter(Collider other)
    {
        NetworkIdentity identity = other.GetComponentInParent<NetworkIdentity>();
        if (!NetworkServer.active || identity?.connectionToClient == null) return;
        QuestLocalRuntime.ServerReport(identity.connectionToClient,
            QuestObjectiveType.EnterArea, areaId, 1);
    }
}

/// <summary>
/// Repairs discovery bindings created while Quest Forge scripts were recompiling.
/// This also keeps authored discovery quests functional in additive Mirror scenes.
/// </summary>
static class QuestDiscoveryBindingBootstrap
{
    const string AreaPrefix = "QuestArea_";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install() => SceneManager.sceneLoaded += OnSceneLoaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void RepairInitialScene() => RepairLoadedDiscoveryBindings();

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => RepairLoadedDiscoveryBindings();

    static void RepairLoadedDiscoveryBindings()
    {
        foreach (Transform transform in
                 Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
        {
            if (!transform.name.StartsWith(AreaPrefix,
                    System.StringComparison.OrdinalIgnoreCase)) continue;

            string targetId = transform.name.Substring(AreaPrefix.Length);
            if (string.IsNullOrWhiteSpace(targetId)) continue;

            BoxCollider trigger = transform.GetComponent<BoxCollider>();
            bool createdTrigger = trigger == null;
            if (createdTrigger) trigger = transform.gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            if (createdTrigger || trigger.size.sqrMagnitude < 0.01f)
                trigger.size = new Vector3(4f, 3f, 4f);

            QuestAreaTarget area = transform.GetComponent<QuestAreaTarget>() ??
                                   transform.gameObject.AddComponent<QuestAreaTarget>();
            area.areaId = targetId;

            GameObject questObject = transform.parent != null
                ? transform.parent.gameObject : transform.gameObject;
            QuestDefinition quest = FindQuestForTarget(targetId);
            if (quest != null) EnsureMarker(questObject, quest, targetId);
        }
    }

    static QuestDefinition FindQuestForTarget(string targetId)
    {
        foreach (QuestGiver giver in
                 Object.FindObjectsByType<QuestGiver>(FindObjectsInactive.Exclude))
        foreach (QuestDefinition quest in giver.quests)
        {
            if (quest == null) continue;
            QuestLocalRuntime.RegisterDefinition(quest);
            foreach (QuestObjectiveDefinition objective in quest.objectives)
                if (objective.type == QuestObjectiveType.EnterArea &&
                    string.Equals(objective.targetId, targetId,
                        System.StringComparison.OrdinalIgnoreCase))
                    return quest;
        }
        return null;
    }

    static void EnsureMarker(GameObject questObject, QuestDefinition quest, string targetId)
    {
        Transform markerTransform = questObject.transform.Find("QuestObjectiveMarker");
        GameObject markerObject;
        if (markerTransform == null)
        {
            markerObject = new GameObject("QuestObjectiveMarker");
            markerObject.transform.SetParent(questObject.transform, false);
        }
        else markerObject = markerTransform.gameObject;

        PositionMarkerJustAboveObject(markerObject.transform, questObject);
        TextMeshPro marker = markerObject.GetComponent<TextMeshPro>() ??
                             markerObject.AddComponent<TextMeshPro>();
        marker.text = "!";
        marker.fontSize = 5f;
        marker.alignment = TextAlignmentOptions.Center;
        marker.color = new Color(1f, 0.82f, 0.08f);
        marker.rectTransform.sizeDelta = new Vector2(1.5f, 1.5f);
        if (markerObject.GetComponent<QuestMarkerBillboard>() == null)
            markerObject.AddComponent<QuestMarkerBillboard>();

        QuestObjectiveMarker association = questObject.GetComponent<QuestObjectiveMarker>() ??
                                           questObject.AddComponent<QuestObjectiveMarker>();
        markerObject.SetActive(true);
        association.Configure(quest, targetId);
    }

    static void PositionMarkerJustAboveObject(Transform marker, GameObject questObject)
    {
        Renderer[] renderers = questObject.GetComponentsInChildren<Renderer>();
        bool found = false;
        Bounds bounds = default;
        foreach (Renderer renderer in renderers)
        {
            if (renderer.transform == marker || renderer.transform.IsChildOf(marker)) continue;
            if (!found) { bounds = renderer.bounds; found = true; }
            else bounds.Encapsulate(renderer.bounds);
        }
        marker.position = found
            ? new Vector3(bounds.center.x, bounds.max.y + 0.75f, bounds.center.z)
            : questObject.transform.position + Vector3.up * 2.75f;
    }
}
