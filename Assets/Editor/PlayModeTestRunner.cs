using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";
        private const string SentinelLog = "PLAY_MODE_TEST_COMPLETE";

        private static readonly float TestTimeout = 40.0f;

        private static List<string> _capturedLogs = new List<string>();
        private const int MaxCapturedLogs = 200;

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");

            switch (state)
            {
                case "Idle":
                    break;

                case "WaitingForCompile":
                    Debug.Log("[PlayModeTest] Bootstrap compiled. Scheduling Play Mode entry.");
                    EditorApplication.delayCall += () =>
                    {
                        SessionState.SetString(StateKey, "EnteringPlayMode");
                        EditorApplication.isPlaying = true;
                    };
                    break;

                case "EnteringPlayMode":
                    if (EditorApplication.isPlaying)
                    {
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;

                case "InPlayMode":
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;

                case "Done":
                    Debug.Log(SentinelLog);
                    EditorApplication.delayCall += SelfDestruct;
                    break;
            }
        }

        private static int _frameCount = 0;
        private static bool _setupDone = false;
        private static bool _testDone = false;
        private static double _testStartTime = 0;

        private static int _cycleCount = 0;
        private static int _testState = 0; // 0: Login, 1: CharacterSelect, 2: Gameplay, 3: VerifyReturn
        private static float _stateTimer = 0f;
        private static string _lastSceneName = "";

        private static int _esCountLogin = 0;
        private static int _esCountCharSelect = 0;
        private static int _esCountGameplay = 0;
        private static bool _imEnabledLogin = false;
        private static bool _imEnabledCharSelect = false;
        private static bool _imEnabledGameplay = false;

        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < 10) return;

            if (_testDone) return;

            if (!_setupDone)
            {
                _setupDone = true;
                Application.logMessageReceived += OnLogMessage;
                _testStartTime = EditorApplication.timeSinceStartup;
                try
                {
                    Setup();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[PlayModeTest] Setup threw exception: " + e);
                    FinishTest(true, e.Message);
                    return;
                }
                return;
            }

            float elapsed = (float)(EditorApplication.timeSinceStartup - _testStartTime);
            bool timedOut = elapsed >= TestTimeout;

            try
            {
                bool complete = Tick(elapsed);
                if (complete || timedOut)
                {
                    if (timedOut && !complete)
                    {
                        Debug.LogWarning("[PlayModeTest] Test timed out after " + elapsed + "s");
                    }
                    FinishTest(timedOut && !complete, timedOut ? "Test timed out after " + TestTimeout + "s" : null);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[PlayModeTest] Tick threw exception: " + e);
                FinishTest(true, e.Message);
            }
        }

        private static void FinishTest(bool isError, string errorMessage)
        {
            _testDone = true;
            EditorApplication.update -= WaitFramesThenRun;
            Application.logMessageReceived -= OnLogMessage;

            string resultJson;
            try
            {
                resultJson = GetResult();
            }
            catch (System.Exception e)
            {
                resultJson = JsonUtility.ToJson(new TestResult
                {
                    success = false,
                    error = "GetResult() threw: " + e.Message,
                    logs = _capturedLogs.ToArray()
                });
            }

            if (isError && errorMessage != null)
            {
                resultJson = JsonUtility.ToJson(new TestResult
                {
                    success = false,
                    error = errorMessage,
                    logs = _capturedLogs.ToArray()
                });
            }

            SessionState.SetString(ResultKey, resultJson);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static void OnLogMessage(string message, string stackTrace, LogType type)
        {
            if (_capturedLogs.Count >= MaxCapturedLogs) return;
            if (type == LogType.Error || type == LogType.Exception ||
                message.Contains("[Test]") || message.Contains("TEST_RESULT") || message.Contains("EventSystem"))
            {
                _capturedLogs.Add("[" + type + "] " + message);
            }
        }

        private static void SelfDestruct()
        {
            string scriptPath = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(scriptPath) && AssetDatabase.AssetPathExists(scriptPath))
            {
                AssetDatabase.DeleteAsset(scriptPath);
            }
            SessionState.EraseString(StateKey);
            SessionState.EraseString(ScriptPathKey);
        }

        [System.Serializable]
        private class TestResult
        {
            public bool success;
            public string error;
            public string[] logs;
            public int cyclesCompleted;
            public int eventSystemsInLogin;
            public int eventSystemsInCharSelect;
            public int eventSystemsInGameplay;
            public bool inputModuleEnabledInLogin;
            public bool inputModuleEnabledInCharSelect;
            public bool inputModuleEnabledInGameplay;
        }

        private static void Setup()
        {
            Debug.Log("[Test] Setup: Loading LoginScene");
            EditorSceneManager.LoadSceneInPlayMode("Assets/Game/Scenes/LoginScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
            _cycleCount = 0;
            _testState = 0;
            _stateTimer = 0f;
            _lastSceneName = "";
        }

        private static bool Tick(float elapsed)
        {
            _stateTimer += Time.deltaTime;
            Scene activeScene = SceneManager.GetActiveScene();

            if (activeScene.name != _lastSceneName)
            {
                Debug.Log("[Test] Active Scene transitioned to: " + activeScene.name);
                _lastSceneName = activeScene.name;
            }

            switch (_testState)
            {
                case 0:
                    if (activeScene.name == "LoginScene")
                    {
                        var esList = FindEventSystems();
                        _esCountLogin = esList.Count;
                        _imEnabledLogin = false;
                        if (_esCountLogin == 1)
                        {
                            var im = esList[0].GetComponent<InputSystemUIInputModule>();
                            if (im != null && im.enabled)
                            {
                                _imEnabledLogin = true;
                            }
                        }
                        
                        Debug.Log("[Test] Login Scene Check: EventSystems count = " + _esCountLogin + ", UIModuleEnabled = " + _imEnabledLogin);

                        // Look for a Button with text or name containing "HOST"
                        Button hostBtn = FindButtonByKeyword("HOST");
                        if (hostBtn != null && hostBtn.interactable)
                        {
                            Debug.Log("[Test] Found host button: " + hostBtn.gameObject.name + ". Clicking it!");
                            hostBtn.onClick.Invoke();
                            _testState = 1;
                            _stateTimer = 0f;
                        }
                        else
                        {
                            if (_stateTimer > 5f)
                            {
                                Debug.LogError("[Test] Host Button not found or not interactable on Login Screen after 5s!");
                                return true;
                            }
                        }
                    }
                    break;

                case 1:
                    if (activeScene.name == "CharacterSelect")
                    {
                        var esList = FindEventSystems();
                        _esCountCharSelect = esList.Count;
                        _imEnabledCharSelect = false;
                        if (_esCountCharSelect == 1)
                        {
                            var im = esList[0].GetComponent<InputSystemUIInputModule>();
                            if (im != null && im.enabled)
                            {
                                _imEnabledCharSelect = true;
                            }
                        }
                        
                        Debug.Log("[Test] CharacterSelect Scene Check: EventSystems count = " + _esCountCharSelect + ", UIModuleEnabled = " + _imEnabledCharSelect);

                        Button deployBtn = FindButtonByKeyword("DEPLOY");
                        if (deployBtn != null && deployBtn.interactable)
                        {
                            Debug.Log("[Test] Found Deploy button: " + deployBtn.gameObject.name + ". Clicking it!");
                            deployBtn.onClick.Invoke();
                            _testState = 2;
                            _stateTimer = 0f;
                        }
                        else
                        {
                            if (_stateTimer > 5f)
                            {
                                Debug.LogError("[Test] Deploy button not found or not interactable on CharacterSelect after 5s!");
                                return true;
                            }
                        }
                    }
                    break;

                case 2:
                    if (activeScene.name != "LoginScene" && activeScene.name != "CharacterSelect")
                    {
                        var esList = FindEventSystems();
                        _esCountGameplay = esList.Count;
                        _imEnabledGameplay = false;
                        if (_esCountGameplay == 1)
                        {
                            var im = esList[0].GetComponent<InputSystemUIInputModule>();
                            if (im != null && im.enabled)
                            {
                                _imEnabledGameplay = true;
                            }
                        }
                        
                        Debug.Log("[Test] Gameplay Scene (" + activeScene.name + ") Check: EventSystems count = " + _esCountGameplay + ", UIModuleEnabled = " + _imEnabledGameplay);

                        if (_stateTimer < 2.0f) return false;

                        Button logoutBtn = FindButtonByKeyword("LOGOUT");
                        if (logoutBtn != null)
                        {
                            Debug.Log("[Test] Found Logout button: " + logoutBtn.gameObject.name + ". Clicking it!");
                            logoutBtn.onClick.Invoke();
                            _cycleCount++;
                            _testState = 3;
                            _stateTimer = 0f;
                        }
                        else
                        {
                            // Try ESC to open Menu if Logout button is in EscMenu and EscMenu is inactive
                            var esObj = GameObject.Find("EscMenu");
                            if (esObj != null || GameObject.Find("EscMenuCanvas") != null)
                            {
                                // Simulate ESC key or call method directly via reflection/component
                                Debug.Log("[Test] EscMenu found, trying to invoke Logout directly or active menu");
                            }

                            // Let's search EscMenu components
                            var escMenus = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
                            foreach (var em in escMenus)
                            {
                                if (em.GetType().Name == "EscMenu")
                                {
                                    var method = em.GetType().GetMethod("Logout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                                    if (method != null)
                                    {
                                        Debug.Log("[Test] Invoking EscMenu.Logout directly!");
                                        method.Invoke(em, null);
                                        _cycleCount++;
                                        _testState = 3;
                                        _stateTimer = 0f;
                                        return false;
                                    }
                                }
                            }

                            Debug.LogError("[Test] LogoutBtn or EscMenu component not found in gameplay scene " + activeScene.name + "!");
                            return true;
                        }
                    }
                    break;

                case 3:
                    if (activeScene.name == "LoginScene")
                    {
                        if (_stateTimer < 1.5f) return false;

                        var esList = FindEventSystems();
                        int esCount = esList.Count;
                        bool imEnabled = false;
                        if (esCount == 1)
                        {
                            var im = esList[0].GetComponent<InputSystemUIInputModule>();
                            if (im != null && im.enabled)
                            {
                                imEnabled = true;
                            }
                        }

                        Debug.Log("[Test] Returned to LoginScene (Cycle " + _cycleCount + ") check: EventSystems count = " + esCount + ", UIModuleEnabled = " + imEnabled);

                        if (esCount != 1 || !imEnabled)
                        {
                            Debug.LogError("[Test] FAILED: EventSystem count is " + esCount + ", enabled is " + imEnabled + " on return to LoginScene!");
                            return true;
                        }

                        if (_cycleCount >= 3)
                        {
                            Debug.Log("[Test] SUCCESSFULLY COMPLETED 3 FULL CYCLES!");
                            return true;
                        }
                        else
                        {
                            _testState = 0;
                            _stateTimer = 0f;
                        }
                    }
                    break;
            }

            return false;
        }

        private static List<EventSystem> FindEventSystems()
        {
            var result = new List<EventSystem>();
            foreach (var es in Resources.FindObjectsOfTypeAll<EventSystem>())
            {
                if (es.gameObject.activeInHierarchy && es.enabled)
                {
                    result.Add(es);
                }
            }
            return result;
        }

        private static Button FindButtonByKeyword(string keyword)
        {
            foreach (var btn in Resources.FindObjectsOfTypeAll<Button>())
            {
                if (btn.gameObject.name.ToUpper().Contains(keyword))
                    return btn;

                var textComp = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (textComp != null && textComp.text.ToUpper().Contains(keyword))
                    return btn;

                var legacyText = btn.GetComponentInChildren<Text>();
                if (legacyText != null && legacyText.text.ToUpper().Contains(keyword))
                    return btn;
            }
            return null;
        }

        private static string GetResult()
        {
            var result = new TestResult
            {
                success = (_cycleCount >= 3),
                error = _cycleCount >= 3 ? null : "Failed to complete 3 cycles. Cycle count: " + _cycleCount + " state: " + _testState,
                logs = _capturedLogs.ToArray(),
                cyclesCompleted = _cycleCount,
                eventSystemsInLogin = _esCountLogin,
                eventSystemsInCharSelect = _esCountCharSelect,
                eventSystemsInGameplay = _esCountGameplay,
                inputModuleEnabledInLogin = _imEnabledLogin,
                inputModuleEnabledInCharSelect = _imEnabledCharSelect,
                inputModuleEnabledInGameplay = _imEnabledGameplay
            };
            return JsonUtility.ToJson(result);
        }
    }
}
