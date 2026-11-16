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
        static int memoryStep;
        static ActivityToken[] pair;
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
                var cover = GameObject.Find("Scene cover");
                if (cover && cover.GetComponent<UnityEngine.UI.Image>().raycastTarget) return;
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
                    var requested = session.shopController.listRoot.GetComponentsInChildren<HintPointer>().First(h => h.entry).entry;
                    source.clip = null;
                    session.shopController.worldRoot.GetComponentsInChildren<WordView>().First(v => v.Entry == requested).Select();
                    if (!requested.pronunciation || source.clip != requested.pronunciation)
                        throw new Exception("Correct shop selection did not play the requested word");
                }
                else if (session.activity is MemoryController memory && !CheckMemory(session, memory, source)) return;
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

        static bool CheckMemory(GameSession session, MemoryController memory, AudioSource source)
        {
            if (!memory.Running) return false;
            var cards = memory.taskRoot.GetComponentsInChildren<ActivityToken>();
            if (memoryStep == 0)
            {
                var result = new ActivityResult { id = "Memory" };
                session.Progress.activities.RemoveAll(r => r.id == result.id);
                session.Progress.activities.Add(result);
                for (int round = 0; round < 3; round++)
                {
                    result.completions = round;
                    session.RestartActivity();
                    cards = memory.taskRoot.GetComponentsInChildren<ActivityToken>();
                    if (cards.Length != 2 * (4 + round * 2) || cards.GroupBy(t => t.view.Entry).Any(g => g.Count() != 2))
                        throw new Exception("Invalid board for Memory round " + (round + 1));
                }
                var hint = memory.speak.transform.parent.Find("Hint").GetComponent<UnityEngine.UI.Button>();
                if (!hint.isActiveAndEnabled || !hint.interactable) throw new Exception("Memory hint is unavailable");
                hint.onClick.Invoke();
                if (cards.Count(t => t.FaceUp) != 1 || memory.Hints != 1) throw new Exception("Peek did not expose exactly one card");
                memoryStep++;
                return false;
            }
            if (memory.Evaluating) return false;
            if (memoryStep == 1)
            {
                if (cards.Any(t => t.FaceUp) || memory.PairsFound != 0) throw new Exception("Peek changed the board");
                if (!cards.All(t => t.caption.font.sourceFontFile && t.caption.font.faceInfo.familyName == "Nunito"))
                    throw new Exception("Newly dealt cards did not use Nunito");
                pair = cards.GroupBy(t => t.view.Entry).First(g => g.Key.pronunciation).ToArray();
                foreach (var card in pair) card.Click();
                memoryStep++;
                return false;
            }
            if (memory.PairsFound != 1 || source.clip != pair[0].view.Entry.pronunciation)
                throw new Exception("Matched pair did not play its word");
            var tray = memory.taskRoot.Find("Matched pairs tray").GetComponent<SpriteRenderer>();
            if (pair.Any(t => !tray.bounds.Contains(new Vector3(t.transform.position.x, t.transform.position.y, tray.bounds.center.z)))) return false;
            Debug.Log("Memory passed 4/6/8 board sizes, Nunito cards, peek, collected pair and pronunciation.");
            return true;
        }

        static void Finish(int result)
        {
            SessionState.SetBool(Running, false);
            EditorApplication.Exit(result);
        }
    }
}
