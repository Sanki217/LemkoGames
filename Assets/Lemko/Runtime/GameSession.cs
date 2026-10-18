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
        public TooltipService tooltip;
        public PronunciationService audioService;
        [Header("Scene navigation")]
        public bool useSeparateScenes;
        [Tooltip("Only used in unfinished activity scenes.")]
        public string placeholderActivity;
        static ProgressData sharedProgress;
        static int sharedLives;
        static bool sharedPractice;
        bool navigating;
        public ProgressData Progress => sharedProgress;
        public int Lives => sharedLives;
        public bool Practice => sharedPractice;
        public bool CanAnswer => Practice || Lives > 0;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSession() { sharedProgress=null; sharedLives=0; sharedPractice=false; }
        void Start()
        {
            tooltip.Revealed+=RecordHint;
            if(sharedProgress==null) { sharedProgress=ProgressStore.Load(); sharedLives=shop.startingLives; }
            ApplyPreferences();
            if(activity) RestartActivity();
            else if(useSeparateScenes && SceneManager.GetActiveScene().name=="Shop") OpenShop();
            else ShowVillageOrPlaceholder();
        }
        public void RefreshHud()
        {
            string progress=activity ? activity.ProgressLabel : shopController ? shopController.ProgressLabel : "Village learning";
            hud.text=(Practice ? "Practice - unlimited lives" : "Lives: "+Lives)+"  |  "+progress;
            if(shopController) hud.text+="\nPoints: "+shopController.Points+"  |  Next: x"+shopController.NextMultiplier+"  |  Hints used: "+shopController.HintsUsed;
            if(activity) hud.text+="\nPoints: "+activity.Points+"  |  Next: x"+activity.Multiplier+"  |  Hints used: "+activity.Hints;
        }
        void RecordHint() { if(activity) activity.RecordHint(); else if(shopController) shopController.RecordHint(); }
        public void ToggleMute() { Progress.muted=!Progress.muted; ApplyPreferences(); ProgressStore.Save(Progress); }
        public void ToggleLanguage() { Progress.english=!Progress.english; ApplyPreferences(); tooltip.Hide(); ProgressStore.Save(Progress); }
        void ApplyPreferences()
        {
            tooltip.English=Progress.english; audioService.SetMuted(Progress.muted);
            muteButton.GetComponentInChildren<TMP_Text>().text=Progress.muted ? "Audio: off" : "Audio: on";
            languageButton.GetComponentInChildren<TMP_Text>().text=Progress.english ? "Hints: English" : "Hints: Polish";
        }
        static void Hide(GameObject panel) { if(panel) panel.SetActive(false); }
        void Clear()
        {
            tooltip.Hide(); audioService.Stop(); if(shopController) shopController.Close();
            if(activity) activity.Close();
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
            var correct=shopController.Correct; var mistakes=shopController.Mistakes; int points=shopController.Points;
            int hints=shopController.HintsUsed; Progress.lastShopHints=hints;
            if(complete) Progress.shopCompletions++;
            Progress.lastCorrect=correct; Progress.lastMistakes=mistakes; Progress.lastShopPoints=points;
            Progress.bestShopPoints=Mathf.Max(Progress.bestShopPoints,points); ProgressStore.Save(Progress);
            Clear(); resultsPanel.SetActive(true);
            results.text=(complete ? "Shopping complete!" : "Attempt finished")+"\nPoints: "+points+"  |  Best: "+Progress.bestShopPoints
                +"\nCorrect: "+correct+"  |  Mistakes: "+mistakes+"  |  Hints used: "+hints+"\n"+(Practice ? "Practice session" : "Lives remaining: "+Lives);
            RefreshHud();
        }
        public void RestartActivity() { if(!activity) { OpenShop(); return; } Clear(); if(!CanAnswer) { exhaustedPanel.SetActive(true); RefreshHud(); return; } shopPanel.SetActive(true); activity.Begin(this); RefreshHud(); }
        public void FinishActivity(bool complete)
        {
            if(!activity || !activity.Running) return;
            string id=activity.definition.kind.ToString(); var record=Progress.activities.Find(a=>a.id==id);
            if(record==null) { record=new ActivityResult{id=id}; Progress.activities.Add(record); }
            if(complete) record.completions++; record.lastPoints=activity.Points; record.bestPoints=Mathf.Max(record.bestPoints,activity.Points);
            record.lastHints=activity.Hints; record.lastCorrect=activity.Correct; record.lastMistakes=activity.Mistakes; ProgressStore.Save(Progress);
            Clear(); resultsPanel.SetActive(true); results.text=(complete ? "Session complete!" : "Attempt finished")+"\n"+id+" | Points: "+record.lastPoints+" | Best: "+record.bestPoints+"\nCorrect: "+record.lastCorrect+" | Mistakes: "+record.lastMistakes+"\nHints used: "+record.lastHints; RefreshHud();
        }
        public void FreshAttempt() { sharedLives=shop.startingLives; sharedPractice=false; RestartActivity(); }
        public void StartPractice() { sharedPractice=true; RestartActivity(); }
        void OnApplicationPause(bool paused) { if(paused && Progress!=null) ProgressStore.Save(Progress); }
        void OnApplicationQuit() { if(Progress!=null) ProgressStore.Save(Progress); }
        void OnDestroy() { if(tooltip) { tooltip.Revealed-=RecordHint; tooltip.Hide(); } if(audioService) audioService.Stop(); }
    }
}
