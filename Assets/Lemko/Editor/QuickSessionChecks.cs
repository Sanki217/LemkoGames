using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lemko.Editor
{
    [InitializeOnLoad]
    public static class QuickSessionChecks
    {
        const string Step = "Lemko.QuickSessionCheck.Step";
        const string Deadline = "Lemko.QuickSessionCheck.Deadline";
        static QuickSessionChecks() => EditorApplication.update += Tick;

        public static void Run()
        {
            EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            SessionState.SetInt(Step, 1);
            SessionState.SetFloat(Deadline, (float)EditorApplication.timeSinceStartup + 100);
            EditorApplication.EnterPlaymode();
        }

        static void Tick()
        {
            int step = SessionState.GetInt(Step, 0);
            if (step == 0) return;
            try
            {
                if (EditorApplication.timeSinceStartup > SessionState.GetFloat(Deadline, 0))
                    throw new Exception("Quick session check timed out at step " + step);
                if (step == 3)
                {
                    if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                    SessionState.SetInt(Step, 4);
                    EditorApplication.EnterPlaymode();
                    return;
                }
                if (!EditorApplication.isPlaying) return;
                var session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if (!session || session.Progress == null) return;
                if (step == 2)
                {
                    if (SceneManager.GetActiveScene().name != "Shop") return;
                    if (!session.Progress.english || !session.Progress.story.contributions.Contains("Farm"))
                        throw new Exception("Scene navigation lost session state.");
                    SessionState.SetInt(Step, 3);
                    EditorApplication.ExitPlaymode();
                    return;
                }
                if (SceneManager.GetActiveScene().name != "Village") return;
                if (session.Progress.english || session.Progress.story.contributions.Count != 0)
                    throw new Exception("New play session inherited progress or preferences.");
                if (step == 4)
                {
                    Debug.Log("LEMKO_QUICK_SESSION_PASS: settings menu, scene navigation and fresh second launch.");
                    SessionState.EraseInt(Step);
                    EditorApplication.Exit(0);
                    return;
                }
                var menu = session.menu;
                menu.Open();
                if (!menu.IsOpen || menu.panel.GetComponentsInChildren<Button>(true).Length != 4)
                    throw new Exception("Settings menu must contain Resume and three preferences.");
                menu.language.onClick.Invoke();
                menu.close.onClick.Invoke();
                if (menu.IsOpen) throw new Exception("Resume did not dismiss the menu.");
                VillageStoryProgress.Contribute(session.Progress, "Farm");
                SessionState.SetInt(Step, 2);
                session.OpenShop();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                SessionState.EraseInt(Step);
                EditorApplication.Exit(1);
            }
        }
    }
}
