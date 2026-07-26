using UnityEngine;

/// <summary>Development quest UI. Replace with the final styled UI before release.</summary>
public sealed class QuestLocalDialogue : MonoBehaviour
{
    static QuestLocalDialogue _instance;
    QuestDefinition _quest;
    Rect _window = new Rect(0, 0, 520, 420);
    CursorLockMode _previousCursorLock;
    bool _previousCursorVisible;
    bool _cursorReleased;
    bool _requestPending;
    float _requestStartedAt;
    string _feedback;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null) return;
        var go = new GameObject("[QuestDevelopmentDialogue]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<QuestLocalDialogue>();
    }

    public static void Show(QuestDefinition quest)
    {
        if (_instance == null) Bootstrap();
        _instance._quest = quest;
        _instance._requestPending = false;
        _instance._feedback = "";
        _instance._window.x = (Screen.width - _instance._window.width) * 0.5f;
        _instance._window.y = (Screen.height - _instance._window.height) * 0.5f;
        _instance.ReleaseCursor();
    }

    public static void Hide()
    {
        if (_instance == null) return;
        _instance._quest = null;
        _instance._requestPending = false;
        _instance.RestoreCursor();
    }

    void OnEnable() => QuestLocalRuntime.StateChanged += OnQuestStateChanged;

    void OnDisable()
    {
        QuestLocalRuntime.StateChanged -= OnQuestStateChanged;
        RestoreCursor();
    }

    void Update()
    {
        if (_quest == null || !_requestPending) return;
        if (Time.unscaledTime - _requestStartedAt < 5f) return;
        _requestPending = false;
        _feedback = "The server did not confirm the request. Stay near the quest giver and try again.";
    }

    void OnGUI()
    {
        if (_quest == null || QuestLocalRuntime.Instance == null) return;
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
        {
            Hide();
            Event.current.Use();
            return;
        }
        _window = GUI.ModalWindow(GetInstanceID(), _window, DrawWindow, "QUEST");
    }

    void DrawWindow(int id)
    {
        LocalQuestStatus status = QuestLocalRuntime.Instance.GetStatus(_quest);
        GUILayout.Space(8);
        GUILayout.Label(_quest.title, GUI.skin.box);
        GUILayout.Label($"Status: {GetStatusLabel(status)}");
        GUILayout.Label(status switch
        {
            LocalQuestStatus.Available => _quest.offerText,
            LocalQuestStatus.Active => _quest.activeText,
            LocalQuestStatus.ReadyToTurnIn => _quest.completionText,
            _ => "This quest is complete."
        });
        GUILayout.Space(8);
        for (int i = 0; i < _quest.objectives.Count; i++)
        {
            QuestObjectiveDefinition objective = _quest.objectives[i];
            int progress = QuestLocalRuntime.Instance.GetProgress(_quest, i);
            GUILayout.Label($"{objective.description}  ({progress}/{Mathf.Max(1, objective.requiredAmount)})");
        }
        GUILayout.FlexibleSpace();
        if (!string.IsNullOrWhiteSpace(_feedback))
            GUILayout.Label(_feedback, GUI.skin.box);
        GUILayout.Label($"Rewards: {_quest.goldReward} gold, {_quest.experienceReward} XP");
        GUILayout.BeginHorizontal();
        GUI.enabled = !_requestPending;
        if (status == LocalQuestStatus.Available &&
            GUILayout.Button(_requestPending ? "Accepting..." : "Accept Quest", GUILayout.Height(42)))
        {
            BeginRequest("Sending acceptance request to the server...");
            QuestLocalRuntime.Instance.Accept(_quest);
        }
        if (status == LocalQuestStatus.ReadyToTurnIn &&
            GUILayout.Button(_requestPending ? "Turning In..." : "Turn In Quest", GUILayout.Height(42)))
        {
            BeginRequest("Sending completion request to the server...");
            QuestLocalRuntime.Instance.Complete(_quest);
        }
        GUI.enabled = true;
        if (GUILayout.Button("Close", GUILayout.Height(36))) Hide();
        GUILayout.EndHorizontal();
        GUI.DragWindow(new Rect(0, 0, 10000, 25));
    }

    void BeginRequest(string feedback)
    {
        _requestPending = true;
        _requestStartedAt = Time.unscaledTime;
        _feedback = feedback;
    }

    void OnQuestStateChanged()
    {
        if (_quest == null) return;
        _requestPending = false;
        _feedback = QuestLocalRuntime.Instance.GetStatus(_quest) switch
        {
            LocalQuestStatus.Active => "Quest accepted. Complete the listed objective.",
            LocalQuestStatus.ReadyToTurnIn => "Objective complete. Return here to turn in the quest.",
            LocalQuestStatus.Completed => "Quest complete. Rewards have been granted.",
            _ => ""
        };
    }

    void ReleaseCursor()
    {
        if (!_cursorReleased)
        {
            _previousCursorLock = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            _cursorReleased = true;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RestoreCursor()
    {
        if (!_cursorReleased) return;
        Cursor.lockState = _previousCursorLock;
        Cursor.visible = _previousCursorVisible;
        _cursorReleased = false;
    }

    static string GetStatusLabel(LocalQuestStatus status) => status switch
    {
        LocalQuestStatus.Available => "Available",
        LocalQuestStatus.Active => "In Progress",
        LocalQuestStatus.ReadyToTurnIn => "Ready to Turn In",
        _ => "Completed"
    };
}
