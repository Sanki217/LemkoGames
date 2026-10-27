using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Lemko
{
    public sealed class GameSession : MonoBehaviour
    {
        public ShopDefinition shop;
        public ShopController shopController;
        public LearningActivity activity;
        public GameObject villagePanel, shopPanel, resultsPanel, exhaustedPanel;
        public TMP_Text hud, results, villageStatus;
        public Button muteButton, languageButton;
        [Header("Village learning mode")]
        public Button modeButton;
        public TMP_Text modeDescription;
        [Header("Answer feedback and results recap")]
        public AnswerFeedback answerFeedback;
        public TMP_Text wordRecap;
        public Button nextPlaceButton;
        readonly List<VocabularyEntry> sessionWords=new List<VocabularyEntry>();
        string suggestedActivity, lastResultActivity;
        public TooltipService tooltip;
        public PronunciationService audioService;
        [Header("Scene navigation")]
        public bool useSeparateScenes;
        [Tooltip("Only used in unfinished activity scenes.")]
        public string placeholderActivity;
        [Header("Village gathering")]
        public VillageStoryDefinition story;
        public VillageStoryView storyView;
        public WordJournal journal;
        public GameMenu menu;
        public GamePresentation presentation;
        public TMP_Text storyRequest;
        readonly System.Collections.Generic.HashSet<string> supportedWords=new System.Collections.Generic.HashSet<string>();
        readonly System.Collections.Generic.HashSet<string> correctedWords=new System.Collections.Generic.HashSet<string>();
        public int IndependentAnswers {get;private set;}
        public int SupportedAnswers {get;private set;}
        static ProgressData sharedProgress;
        static int sharedLives;
        static bool sharedPractice;
        bool navigating;
        public ProgressData Progress => sharedProgress;
        public int Lives => sharedLives;
        public bool FarmQuantitiesUnlocked => Progress!=null && Progress.activities.Exists(a=>a.id=="Numbers" && a.completions>0);
        public bool Practice => sharedPractice;
        public bool CanAnswer => (!menu || !menu.IsOpen) && (Practice || Lives > 0);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSession() { sharedProgress=null; sharedLives=0; sharedPractice=false; }
        void Start()
        {
            tooltip.Revealed+=RecordHint;
            if(sharedProgress==null) { sharedProgress=ProgressStore.Load(); sharedLives=shop.startingLives; sharedPractice=!sharedProgress.challengeMode; }
            ApplyPreferences();
            if(activity) RestartActivity();
            else if(useSeparateScenes && SceneManager.GetActiveScene().name=="Shop") OpenShop();
            else ShowVillageOrPlaceholder();
        }
        public void RefreshHud()
        {
            string progress=activity ? activity.ProgressLabel : shopController ? shopController.ProgressLabel : "Village learning";
            hud.text=(Practice ? "Learning - no life limit" : "Challenge lives: "+Lives)+"  |  "+progress;
            if(shopController) hud.text+="\nPoints: "+shopController.Points+"  |  Next: x"+shopController.NextMultiplier+"  |  Hints used: "+shopController.HintsUsed;
            RefreshModeControls();
            if(activity) hud.text+="\nPoints: "+activity.Points+"  |  Next: x"+activity.Multiplier+"  |  Hints used: "+activity.Hints;
        }
        void RefreshModeControls() {
            if(modeButton) modeButton.GetComponentInChildren<TMP_Text>().text=Practice ? "Start challenge" : "Switch to learning";
            if(modeDescription) modeDescription.text=Practice ? "Learning: mistakes never block play." : "Challenge: "+Lives+" shared lives. Hints and rewards stay available.";
        }
        public void ToggleLearningMode() {
            if(SceneManager.GetActiveScene().name!="Village" || Progress==null) return;
            sharedPractice=!sharedPractice;
            if(!sharedPractice) sharedLives=shop.startingLives;
            Progress.challengeMode=!sharedPractice; ProgressStore.Save(Progress); RefreshHud();
        }
        public void PresentFeedback(bool correct) { if(answerFeedback) answerFeedback.Play(correct); }
        public void StopFeedback() { if(answerFeedback) answerFeedback.Cancel(); }
        void ShowRecap(string completedActivity) {
            lastResultActivity=completedActivity;
            suggestedActivity=VillageStoryProgress.Activities.FirstOrDefault(id=>id!=completedActivity && !Progress.story.contributions.Contains(id)) ?? VillageStoryProgress.Activities.FirstOrDefault(id=>!Progress.story.contributions.Contains(id));
            if(nextPlaceButton) {
                nextPlaceButton.gameObject.SetActive(!string.IsNullOrEmpty(suggestedActivity));
                if(!string.IsNullOrEmpty(suggestedActivity)) nextPlaceButton.GetComponentInChildren<TMP_Text>().text="Next: "+(suggestedActivity=="FamilyTree" ? "Family Tree" : suggestedActivity);
            }
            if(!wordRecap) return;
            string words=string.Join("  •  ",sessionWords.Take(3).Select(w=>w.lemko+" — "+w.Translation(Progress.english)));
            wordRecap.text=(sessionWords.Count==0 ? "" : "Words practised: "+words+(sessionWords.Count>3 ? "  (+"+(sessionWords.Count-3)+" more in the journal)" : "")+"\n")+
                (string.IsNullOrEmpty(suggestedActivity) ? "Your gathering preparations are ready. Revisit any place whenever you like." : "Choose another short session whenever you are ready. Your progress is saved.");
        }
        public void OpenSuggestedActivity() { if(resultsPanel && resultsPanel.activeSelf && !string.IsNullOrEmpty(suggestedActivity)) OpenActivity(suggestedActivity); }
        void RecordHint() {
            if(tooltip.RevealedWord) supportedWords.Add(tooltip.RevealedWord.id);
            if(activity) { activity.RecordHint(); if(activity.Current!=null) supportedWords.Add(activity.Current.word.id); }
            else if(shopController) shopController.RecordHint();
        }
        public void BeginLearningSession() { StopFeedback(); sessionWords.Clear(); suggestedActivity=null; supportedWords.Clear(); correctedWords.Clear(); IndependentAnswers=SupportedAnswers=0; RefreshStoryRequest(); }
        public void MarkSupported(VocabularyEntry word) { if(word) supportedWords.Add(word.id); }
        public void RecordAnswer(VocabularyEntry word,bool correct)
        {
            if(!word) return;
            if(!sessionWords.Any(w=>w.id==word.id)) sessionWords.Add(word);
            bool support=supportedWords.Contains(word.id) || correctedWords.Contains(word.id);
            if(!correct) correctedWords.Add(word.id);
            else if(support) SupportedAnswers++; else IndependentAnswers++;
            WordLearning.Record(Progress,word.id,activity?activity.definition.kind.ToString():"Shop",correct,support,System.DateTime.UtcNow.ToString("yyyy-MM-dd"));
            ProgressStore.Save(Progress);
        }
        public int ReviewPriority(VocabularyEntry word)
        {
            var record=Progress.words.Find(w=>w.id==word.id);
            if(record==null) return 0;
            if(record.lastIndependentDay==System.DateTime.UtcNow.ToString("yyyy-MM-dd")) return 0;
            return record.mistakes>0 || record.assisted>0 ? 2 : 1;
        }
        void RefreshStoryRequest() {
            if(!storyRequest || !story) return;
            var contribution=story.Find(activity?activity.definition.kind.ToString():"Shop");
            storyRequest.text=contribution==null ? "" : "Village gathering: "+contribution.request;
        }
        string ContributionResult(string id,bool complete) {
            if(!complete || !story) return "";
            bool added=VillageStoryProgress.Contribute(Progress,id);
            var contribution=story.Find(id);
            string result=added ? contribution?.success??"Contribution complete." : "Your contribution is already ready. Thank you for practising.";
            return "\n"+result+" ("+Progress.story.contributions.Count+"/8)";
        }
        string LearningResult() => "\nWithout help: "+IndependentAnswers+" | With support: "+SupportedAnswers;
        public void ToggleMute() { Progress.muted=!Progress.muted; ApplyPreferences(); ProgressStore.Save(Progress); }
        public void ToggleLanguage() { Progress.english=!Progress.english; ApplyPreferences(); tooltip.Hide(); if(journal) journal.Refresh(); if(resultsPanel && resultsPanel.activeSelf && !string.IsNullOrEmpty(lastResultActivity)) ShowRecap(lastResultActivity); ProgressStore.Save(Progress); }
        public void ToggleReducedMotion() { Progress.reducedMotion=!Progress.reducedMotion; ApplyPreferences(); ProgressStore.Save(Progress); }
        public void RestoreProgress(ProgressData restored) {
            if(restored==null) return;
            Clear(); sharedProgress=restored; sharedLives=shop.startingLives; sharedPractice=!restored.challengeMode;
            ProgressStore.Save(Progress); ApplyPreferences();
            if(SceneManager.GetActiveScene().name=="Village") ShowVillageOrPlaceholder(); else Navigate("Village");
        }
        void ApplyPreferences()
        {
            tooltip.ReducedMotion=Progress.reducedMotion;
            if(answerFeedback) answerFeedback.reducedMotion=Progress.reducedMotion;
            tooltip.English=Progress.english; audioService.SetMuted(Progress.muted);
            muteButton.GetComponentInChildren<TMP_Text>().text=Progress.muted ? "Audio: off" : "Audio: on";
            languageButton.GetComponentInChildren<TMP_Text>().text=Progress.english ? "Hints: English" : "Hints: Polish";
        }
        static void Hide(GameObject panel) { if(panel) panel.SetActive(false); }
        void Clear()
        {
            if(menu) menu.Close(); StopFeedback(); tooltip.Hide(); audioService.Stop(); if(shopController) shopController.Close();
            if(activity) activity.Close(); if(journal) journal.Close();
            Hide(villagePanel); Hide(shopPanel); Hide(resultsPanel); Hide(exhaustedPanel);
        }
        void Navigate(string sceneName)
        {
            if(navigating) return;
            if(!Application.CanStreamedLevelBeLoaded(sceneName)) { Debug.LogError("Scene is not registered: "+sceneName); return; }
            navigating=true; Clear(); ProgressStore.Save(Progress);
            SceneManager.LoadScene(sceneName,LoadSceneMode.Single);
        }
        public void OpenActivity(string sceneName)
        {
            if(sceneName=="Shop") OpenShop();
            else Navigate(sceneName);
        }
        public void Village()
        {
            if(useSeparateScenes && SceneManager.GetActiveScene().name!="Village") { Navigate("Village"); return; }
            ShowVillageOrPlaceholder();
        }
        void ShowVillageOrPlaceholder()
        {
            Clear(); if(villagePanel) villagePanel.SetActive(true);
            if(villageStatus) villageStatus.text=string.IsNullOrEmpty(placeholderActivity)
                ? "Shop completed: "+Progress.shopCompletions+" | Best: "+Progress.bestShopPoints+" points\n"+string.Join("  •  ",Progress.activities.ConvertAll(a=>a.id+": "+a.completions))
                : placeholderActivity+" has its own scene. Gameplay is not implemented yet.\nUse Village to return.";
            if(storyView) storyView.Refresh(this);
            RefreshHud();
        }
        public void OpenShop()
        {
            if(useSeparateScenes && SceneManager.GetActiveScene().name!="Shop") { Navigate("Shop"); return; }
            Clear();
            if(!CanAnswer) { exhaustedPanel.SetActive(true); RefreshHud(); return; }
            shopPanel.SetActive(true); shopController.Begin(this); RefreshHud();
        }
        public void Encounter(VocabularyEntry entry) { if(!Progress.encountered.Contains(entry.id)) Progress.encountered.Add(entry.id); }
        public void Wrong()
        {
            if(!Practice) sharedLives=Mathf.Max(0,Lives-1);
            RefreshHud();
            if(!CanAnswer) { if(activity) FinishActivity(false); else Finish(false); exhaustedPanel.SetActive(true); }
        }
        public void Finish(bool complete)
        {
            if(!shopController || !shopController.HasRound || (complete && !shopController.RoundComplete)) return;
            string contribution=ContributionResult("Shop",complete);
            var correct=shopController.Correct; var mistakes=shopController.Mistakes; int points=shopController.Points;
            int hints=shopController.HintsUsed; Progress.lastShopHints=hints;
            if(complete) Progress.shopCompletions++;
            Progress.lastCorrect=correct; Progress.lastMistakes=mistakes; Progress.lastShopPoints=points;
            Progress.bestShopPoints=Mathf.Max(Progress.bestShopPoints,points); ProgressStore.Save(Progress);
            Clear(); resultsPanel.SetActive(true);
            results.text=(complete ? "Shopping complete!" : "Attempt finished")+"\nPoints: "+points+"  |  Best: "+Progress.bestShopPoints
                +"\nCorrect: "+correct+"  |  Mistakes: "+mistakes+"  |  Hints used: "+hints+"\n"+(Practice ? "Learning session" : "Challenge lives remaining: "+Lives)+LearningResult()+contribution;
            RefreshHud(); ShowRecap("Shop");
        }
        public void RestartActivity() { if(!activity) { OpenShop(); return; } Clear(); if(!CanAnswer) { exhaustedPanel.SetActive(true); RefreshHud(); return; } shopPanel.SetActive(true); activity.Begin(this); RefreshHud(); }
        public void FinishActivity(bool complete)
        {
            if(!activity || !activity.Running) return;
            string memorySummary=activity is MemoryController memory ? memory.PracticeSummary : "";
            string id=activity.definition.kind.ToString(); var record=Progress.activities.Find(a=>a.id==id);
            if(record==null) { record=new ActivityResult{id=id}; Progress.activities.Add(record); }
            bool unlockFarm=complete && id=="Numbers" && !FarmQuantitiesUnlocked;
            string contribution=ContributionResult(id,complete)+(unlockFarm ? "\nAnimal quantity tasks are now available at Farm!" : "");
            if(complete) record.completions++; record.lastPoints=activity.Points; record.bestPoints=Mathf.Max(record.bestPoints,activity.Points);
            record.lastHints=activity.Hints; record.lastCorrect=activity.Correct; record.lastMistakes=activity.Mistakes; ProgressStore.Save(Progress);
            Clear(); resultsPanel.SetActive(true); results.text=(complete ? "Session complete!" : "Attempt finished")+"\n"+id+" | Points: "+record.lastPoints+" | Best: "+record.bestPoints+"\nCorrect: "+record.lastCorrect+" | Mistakes: "+record.lastMistakes+"\n"+(Practice ? "Learning" : "Challenge")+" | Hints used: "+record.lastHints+(id=="Memory" ? "\n"+memorySummary+LearningResult() : LearningResult())+contribution; RefreshHud(); ShowRecap(id);
        }
        public void FreshAttempt() { sharedLives=shop.startingLives; sharedPractice=false; Progress.challengeMode=true; ProgressStore.Save(Progress); RestartActivity(); }
        public void StartPractice() { sharedPractice=true; Progress.challengeMode=false; ProgressStore.Save(Progress); RestartActivity(); }
        void OnApplicationPause(bool paused) { if(paused && Progress!=null) ProgressStore.Save(Progress); }
        void OnApplicationQuit() { if(Progress!=null) ProgressStore.Save(Progress); }
        void OnDestroy() { if(tooltip) { tooltip.Revealed-=RecordHint; tooltip.Hide(); } if(audioService) audioService.Stop(); }
    }
}
