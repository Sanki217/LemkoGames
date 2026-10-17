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
            if(sharedProgress==null) { sharedProgress=ProgressStore.Load(); sharedLives=shop.startingLives; }
            ApplyPreferences();
            if(useSeparateScenes && SceneManager.GetActiveScene().name=="Shop") OpenShop();
            else ShowVillageOrPlaceholder();
        }
        public void RefreshHud()
        {
            string progress=shopController ? shopController.ProgressLabel : "Village learning";
            hud.text=(Practice ? "Practice - unlimited lives" : "Lives: "+Lives)+"  |  "+progress;
            if(shopController) hud.text+="\nPoints: "+shopController.Points+"  |  Next correct: x"+shopController.NextMultiplier;
        }
        public void ToggleMute() { Progress.muted=!Progress.muted; ApplyPreferences(); ProgressStore.Save(Progress); }
        public void ToggleLanguage() { Progress.english=!Progress.english; ApplyPreferences(); tooltip.Hide(); ProgressStore.Save(Progress); }
        void ApplyPreferences()
        {
            tooltip.English=Progress.english; audioService.SetMuted(Progress.muted);
            muteButton.GetComponentInChildren<TMP_Text>().text=Progress.muted ? "Audio: off" : "Audio: on";
            languageButton.GetComponentInChildren<TMP_Text>().text=Progress.english ? "Hints: English" : "Hints: Polski";
        }
        static void Hide(GameObject panel) { if(panel) panel.SetActive(false); }
        void Clear()
        {
            tooltip.Hide(); audioService.Stop(); if(shopController) shopController.Close();
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
                ? "Shop completed: "+Progress.shopCompletions+" | Best: "+Progress.bestShopPoints+" points\nOther locations are scene placeholders."
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
            if(!CanAnswer) { Finish(false); exhaustedPanel.SetActive(true); }
        }
        public void Finish(bool complete)
        {
            var correct=shopController.Correct; var mistakes=shopController.Mistakes; int points=shopController.Points;
            if(complete) Progress.shopCompletions++;
            Progress.lastCorrect=correct; Progress.lastMistakes=mistakes; Progress.lastShopPoints=points;
            Progress.bestShopPoints=Mathf.Max(Progress.bestShopPoints,points); ProgressStore.Save(Progress);
            Clear(); resultsPanel.SetActive(true);
            results.text=(complete ? "Shopping complete!" : "Attempt finished")+"\nPoints: "+points+"  |  Best: "+Progress.bestShopPoints
                +"\nCorrect: "+correct+"  |  Mistakes: "+mistakes+"\n"+(Practice ? "Practice session" : "Lives remaining: "+Lives);
            RefreshHud();
        }
        public void FreshAttempt() { sharedLives=shop.startingLives; sharedPractice=false; OpenShop(); }
        public void StartPractice() { sharedPractice=true; OpenShop(); }
        void OnApplicationPause(bool paused) { if(paused && Progress!=null) ProgressStore.Save(Progress); }
        void OnApplicationQuit() { if(Progress!=null) ProgressStore.Save(Progress); }
        void OnDestroy() { if(tooltip) tooltip.Hide(); if(audioService) audioService.Stop(); }
    }
}
