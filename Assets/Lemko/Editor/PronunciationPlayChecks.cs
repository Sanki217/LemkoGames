using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Lemko.Editor
{
    [InitializeOnLoad]
    public static class PronunciationPlayChecks
    {
        const string Running = "Lemko.PronunciationCheck";
        static readonly string[] scenes = { "Village", "Shop", "FamilyTree", "Farm", "House", "Memory", "Numbers", "Alphabet", "Weather" };
        static int index;
        static PronunciationPlayChecks() => EditorApplication.update += Tick;

        public static void Run()
        {
            if (!Environment.GetCommandLineArgs().Contains("-lemkoSmoke")) throw new Exception("Use isolated smoke saves.");
            EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            SessionState.SetFloat(Running + ".deadline", (float)EditorApplication.timeSinceStartup + 100);
            SessionState.SetBool(Running, true);
            EditorApplication.EnterPlaymode();
        }

        static void Tick()
        {
            if (!SessionState.GetBool(Running, false)) return;
            try
            {
                if (EditorApplication.timeSinceStartup > SessionState.GetFloat(Running + ".deadline", 0))
                    throw new Exception("Timed out entering " + scenes[index]);
                if (!EditorApplication.isPlaying || SceneManager.GetActiveScene().name != scenes[index]) return;
                var session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if (!session || session.Progress == null) return;
                if (session.introduction && session.introduction.IsOpen)
                {
                    session.introduction.OnPointerClick(new PointerEventData(EventSystem.current));
                    return;
                }
                if (UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(l => l.isActiveAndEnabled) != 1)
                    throw new Exception("Expected one active listener in " + scenes[index]);
                var source = session.audioService.GetComponent<AudioSource>();
                if (scenes[index] == "Shop")
                {
                    var view = session.shopController.worldRoot.GetComponentsInChildren<WordView>().First(v => v.Entry);
                    view.Speak();
                    if (!view.Entry.pronunciation || source.clip != view.Entry.pronunciation || source.clip.samples <= 0)
                        throw new Exception("Shop speaker failed for " + view.Entry.id);
                }
                else if (session.activity && session.activity.Current != null)
                {
                    var activity = session.activity;
                    var clip = activity.Current.word.pronunciation;
                    if (activity.speak.interactable != (clip != null)) throw new Exception("Speaker state differs from clip availability");
                    if (clip)
                    {
                        activity.speak.onClick.Invoke();
                        if (source.clip != clip || clip.samples <= 0) throw new Exception("Incorrect activity recording");
                    }
                }
                Debug.Log("Pronunciation check passed in " + scenes[index]);
                if (++index < scenes.Length) session.OpenActivity(scenes[index]);
                else Finish(0);
            }
            catch (Exception e) { Debug.LogException(e); Finish(1); }
        }

        static void Finish(int result)
        {
            SessionState.SetBool(Running, false);
            EditorApplication.Exit(result);
        }
    }
}
