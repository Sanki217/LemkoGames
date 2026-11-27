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
        static GameSession challengeVisit;
        static bool returningAfterZeroHearts;
        static bool checkingMemoryJournal;
        static double weatherCaptureAt;
        static PronunciationPlayChecks() => EditorApplication.update += Tick;

        public static void Run()
        {
            SessionState.SetBool(Running+".journal",false);
            SessionState.SetBool(Running+".weather",false);
            if (!Environment.GetCommandLineArgs().Contains("-lemkoSmoke")) throw new Exception("Use isolated smoke saves.");
            if(Lang.Translate("in <b>кухня</b>")!="w <b>кухня</b>")
                throw new Exception("House room preposition was not translated");
            EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            SessionState.SetFloat(Running + ".deadline", (float)EditorApplication.timeSinceStartup + 100);
            SessionState.SetBool(Running, true);
            EditorApplication.EnterPlaymode();
        }

        public static void RunMemoryJournal()
        {
            Run();
            SessionState.SetBool(Running+".journal",true);
        }

        public static void RunWeather()
        {
            RunMemoryJournal();
            SessionState.SetBool(Running+".weather",true);
        }

        static void Tick()
        {
            if (!SessionState.GetBool(Running, false)) return;
            try
            {
                if (EditorApplication.timeSinceStartup > SessionState.GetFloat(Running + ".deadline", 0))
                    throw new Exception("Timed out entering " + scenes[index]);
                if (!EditorApplication.isPlaying) return;
                string sceneName=SceneManager.GetActiveScene().name;
                if (sceneName!=scenes[index] && !(index==0 && sceneName=="Shop") && !(checkingMemoryJournal && sceneName=="Village")) return;
                var session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if (!session || session.Progress == null) return;
                var cover = GameObject.Find("Scene cover");
                if (cover && cover.GetComponent<UnityEngine.UI.Image>().raycastTarget) return;
                if(SessionState.GetBool(Running+".weather",false)) {
                    SessionState.SetBool(Running+".weather",false);
                    index=Array.IndexOf(scenes,"Weather");
                    session.OpenActivity("Weather");
                    return;
                }
                if(checkingMemoryJournal && sceneName=="Village") {
                    session.journal.Open();
                    if(session.journal.VisibleCount!=8 || session.Progress.encountered.Count!=8 ||
                        session.Progress.words.Count!=8 || session.Progress.words.Any(w=>w.correct<1 || w.independent!=0))
                        throw new Exception("Completed Memory board has missing or unexposed journal words");
                    do {
                        if(session.journal.rows.Any(r=>r.button.gameObject.activeSelf && r.state.text!="Practised"))
                            throw new Exception("Matched journal word is not marked Practised");
                        if(!session.journal.next.interactable) break;
                        session.journal.Next();
                    } while(true);
                    session.journal.Close();
                    checkingMemoryJournal=false;
                    Debug.Log("Memory journal passed: eight visible words, all practised, full board and association checks.");
                    session.OpenActivity(scenes[++index]);
                    return;
                }
                if (session.introduction && session.introduction.IsOpen)
                {
                    if (session.introduction.instructions.gameObject.activeInHierarchy ||
                        session.introduction.title.transform.parent.Find("Continue cue").gameObject.activeInHierarchy)
                        throw new Exception("Activity introduction still displays the removed instructions");
                    session.introduction.OnPointerClick(new PointerEventData(EventSystem.current));
                    return;
                }
                if (UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(l => l.isActiveAndEnabled) != 1)
                    throw new Exception("Expected one active listener in " + scenes[index]);
                var source = session.audioService.GetComponent<AudioSource>();
                if(index==0 && sceneName=="Shop") {
                    if(returningAfterZeroHearts || !session.shopController.HasRound) return;
                    for(int attempt=0;attempt<2;attempt++) {
                        int lives=session.Lives;
                        for(int i=0;i<lives;i++) session.Wrong();
                        if(!session.exhaustedPanel.activeSelf || session.Lives!=0 || session.Practice)
                            throw new Exception("Missing challenge recovery choices");
                        var card=session.resultsPanel.transform.Find("Result card") as RectTransform;
                        if(!card || card.anchoredPosition.x!=0 || session.contributionReward.gameObject.activeSelf)
                            throw new Exception("Unexpected Shop results layout");
                        string choice=attempt==0 ? "Fresh attempt" : "Unlimited practice";
                        session.exhaustedPanel.transform.Find(choice).GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                        if(session.Lives!=session.shop.startingLives || session.Practice)
                            throw new Exception("Recovery changed mode or failed to refill hearts");
                    }
                    returningAfterZeroHearts=true;
                    return;
                }
                if (scenes[index] == "Village")
                {
                    if (challengeVisit == session) return;
                    if(returningAfterZeroHearts) {
                        if(!SessionStats.VisitStarted || session.Practice) throw new Exception("Return to village lost challenge mode");
                        returningAfterZeroHearts=false;
                        session.NewSession();
                        return;
                    }
                    var title = GameObject.Find("Title screen/Content");
                    if (!title) return;
                    if (title.transform.Find("Places").childCount != 8 || title.transform.Find("Note") ||
                        title.transform.Find("Subtitle") || session.modeButton.gameObject.activeSelf)
                        throw new Exception("Unexpected welcome screen places or footer");
                    bool challenge = ReferenceEquals(challengeVisit, null);
                    title.transform.Find(challenge ? "Learning challenge" : "Relaxed experience")
                        .GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                    if (!SessionStats.VisitStarted || session.Practice == challenge || session.Progress.challengeMode != challenge)
                        throw new Exception("Welcome selection did not apply the mode");
                    var progress = new ProgressData();
                    foreach (var place in VillageStoryProgress.Activities)
                    {
                        VillageStoryProgress.Contribute(progress, place);
                        if (SessionStats.GoalReached(progress) != (progress.story.contributions.Count == 8))
                            throw new Exception("Finale must require all eight places");
                    }
                    Debug.Log("Welcome passed for " + (challenge ? "learning challenge" : "relaxed experience"));
                    if (challenge) {
                        challengeVisit = session;
                        session.ToggleLearningMode();
                        if(session.Practice) throw new Exception("Challenge mode changed after leaving the welcome screen");
                        session.OpenShop(); return;
                    }
                }
                if (session.activity && (session.activity.speak.gameObject.activeInHierarchy || session.activity.speak.transform.parent.Find("Hint")))
                    throw new Exception("Speaker or lightbulb shown in " + scenes[index]);
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
                else if (session.activity is WeatherController weather)
                {
                    if(weatherCaptureAt==0) { weatherCaptureAt=EditorApplication.timeSinceStartup+1; return; }
                    if(EditorApplication.timeSinceStartup<weatherCaptureAt) return;
                    if(weather.temperature.gameObject.activeSelf || weather.temperatureLabel.gameObject.activeSelf ||
                        weather.taskRoot.Find("Observed thermometer"))
                        throw new Exception("Temperature must wait until the second stage");
                    if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null) {
                        PrototypeVisualChecks.Capture("Weather-observation",1280,720);
                    }
                    weather.definition=UnityEngine.Object.Instantiate(weather.definition);
                    weather.definition.tasksPerSession=weather.definition.tasks.Count;
                    session.Progress.reducedMotion=true;
                    session.RestartActivity();
                    int count=weather.SessionTaskCount;
                    for(int task=0;task<count;task++) {
                        var observation=weather.taskRoot.GetComponentsInChildren<SpriteRenderer>().Single(r=>r.name=="Observed weather");
                        var expected=weather.Current.word;
                        int degrees=weather.ObservedTemperature;
                        if(observation.sprite!=expected.sprite || degrees<weather.Current.minTemperature ||
                            degrees>weather.Current.maxTemperature || weather.taskRoot.Find("Outing supplies") ||
                            weather.taskRoot.Find("Outing blanket"))
                            throw new Exception("Invalid weather observation or obsolete outing reward");
                        var choices=weather.taskRoot.GetComponentsInChildren<ActivityToken>().Where(t=>t.Key=="weather").ToArray();
                        if(choices.Length!=3 || choices.Select(t=>t.view.Entry).Distinct().Count()!=3 ||
                            choices.Count(t=>t.view.Entry==expected)!=1 || weather.temperature.interactable ||
                            choices.Any(t=>t.view.Entry.id=="weather_frost" || t.view.Entry.id=="weather_hot"))
                            throw new Exception("Invalid weather choices or automatic temperature");
                        weather.Choose(choices.First(t=>t.view.Entry!=expected).view.Entry);
                        weather.Submit();
                        if(weather.Locked || observation.sprite!=expected.sprite || weather.ObservedTemperature!=degrees)
                            throw new Exception("Choosing a wrong description changed or solved the observed weather");
                        weather.Choose(expected);
                        weather.Submit();
                        if(!weather.TemperatureStage || weather.Locked || weather.Selected ||
                            !weather.temperature.gameObject.activeSelf || weather.temperature.interactable ||
                            Mathf.RoundToInt(weather.temperature.value)!=degrees || source.clip!=expected.pronunciation)
                            throw new Exception("Correct weather must open the automatic temperature stage");
                        var temperatureChoices=weather.taskRoot.GetComponentsInChildren<ActivityToken>();
                        if(temperatureChoices.Length!=3 || temperatureChoices.Count(t=>t.view.Entry==weather.Current.context)!=1)
                            throw new Exception("Invalid temperature descriptions");
                        weather.Choose(temperatureChoices.First(t=>t.view.Entry!=weather.Current.context).view.Entry);
                        weather.Submit();
                        if(weather.Locked) throw new Exception("Wrong temperature description accepted");
                        weather.Choose(weather.Current.context);
                        weather.Submit();
                        if(!weather.Locked || source.clip!=weather.Current.context.pronunciation ||
                            weather.feedback.gameObject.activeSelf)
                            throw new Exception("Temperature answer, pronunciation, or hidden feedback failed");
                        weather.Advance();
                    }
                    if(!session.resultsPanel.activeSelf) throw new Exception("Weather round did not finish");
                    Debug.Log("Weather passed all nine scenarios, separate weather and temperature stages, three choices, automatic temperature, wrong-answer retries, pronunciation, hidden feedback, and round completion.");
                }
                else if (session.activity && session.activity.Current != null)
                {
                    var activity = session.activity;
                    if(activity is FamilyTreeController family && (family.album.gameObject.activeSelf || family.feedback.gameObject.activeSelf))
                        throw new Exception("Family Tree still displays overlapping invitation or feedback text");
                    var clip = activity.Current.word.pronunciation;
                    if (activity.speak.interactable != (clip != null)) throw new Exception("Speaker state differs from clip availability");
                    if (clip)
                    {
                        activity.Speak();
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
                session.Progress.encountered.Clear();
                session.Progress.words.Clear();
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
                    if(cards.Any(t=>!MemoryController.IsEligibleWord(t.view.Entry)) || session.Progress.encountered.Count!=0)
                        throw new Exception("Family card or unseen word recorded when dealing Memory");
                }
                var trayTap = memory.taskRoot.GetComponentsInChildren<EventTrigger>().Single(t => t.name == "Matched pairs tray");
                ExecuteEvents.Execute(trayTap.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
                if (cards.Count(t => t.FaceUp) != 1 || memory.Hints != 1) throw new Exception("Peek did not expose exactly one card");
                if(session.Progress.encountered.Count!=1 || !session.Progress.encountered.Contains(cards.Single(t=>t.FaceUp).view.Entry.id))
                    throw new Exception("Peek did not record exactly the visible word");
                memoryStep++;
                return false;
            }
            if (memory.Evaluating) return false;
            if(memoryStep==3) {
                if(memory.AssociationPending) { memory.Advance(); return false; }
                if(memory.AssociationActive) {
                    memory.ChooseAssociation(memory.AssociationWord);
                    memory.Advance();
                    if(session.resultsPanel.activeSelf) {
                        if(session.SupportedAnswers!=10 || session.IndependentAnswers!=0)
                            throw new Exception("Eight matches and two checks must count as supported practice");
                        checkingMemoryJournal=true;
                        session.Village();
                    }
                    return false;
                }
                var nextPair=cards.Where(t=>!t.FaceUp).GroupBy(t=>t.view.Entry).First().ToArray();
                foreach(var card in nextPair) card.Click();
                return false;
            }
            if (memoryStep == 1)
            {
                if (cards.Any(t => t.FaceUp) || memory.PairsFound != 0) throw new Exception("Peek changed the board");
                if (!SessionState.GetBool(Running+".journal",false) &&
                    !cards.All(t => t.caption.font.sourceFontFile && t.caption.font.faceInfo.familyName == "Nunito"))
                    throw new Exception("Newly dealt cards did not use Nunito");
                pair = cards.GroupBy(t => t.view.Entry).First(g => g.Key.pronunciation).ToArray();
                foreach (var card in pair) card.Click();
                memoryStep++;
                return false;
            }
            if (memory.PairsFound != 1 || source.clip != pair[0].view.Entry.pronunciation)
                throw new Exception("Matched pair did not play its word");
            var word=pair[0].view.Entry;
            var record=session.Progress.words.Single(r=>r.id==word.id);
            if(record.correct!=1 || record.assisted!=1 || record.independent!=0 || session.Progress.words.Count!=1)
                throw new Exception("A match must record supported practice exactly once");
            var tray = memory.taskRoot.Find("Matched pairs tray").GetComponent<SpriteRenderer>();
            if (pair.Any(t => !tray.bounds.Contains(new Vector3(t.transform.position.x, t.transform.position.y, tray.bounds.center.z)))) return false;
            Debug.Log("Memory passed 4/6/8 board sizes, family exclusion, hidden-word exclusion, revealed word recording, supported match, collected pair and pronunciation.");
            memoryStep=3;
            return false;
        }

        static void Finish(int result)
        {
            SessionState.SetBool(Running, false);
            EditorApplication.Exit(result);
        }
    }
}
