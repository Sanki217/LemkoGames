using TMPro;
using UnityEngine;
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
        public ProgressData Progress { get; private set; }
        public int Lives { get; private set; }
        public bool Practice { get; private set; }
        public bool CanAnswer => Practice || Lives > 0;
        void Start()
        {
            Progress = ProgressStore.Load(); Lives = shop.startingLives;
            ApplyPreferences(); Village();
        }
        public void RefreshHud() => hud.text = (Practice ? "Practice - unlimited lives" : "Lives: " + Lives) + "  |  " + shopController.ProgressLabel;
        public void ToggleMute() { Progress.muted = !Progress.muted; ApplyPreferences(); ProgressStore.Save(Progress); }
        public void ToggleLanguage() { Progress.english = !Progress.english; ApplyPreferences(); tooltip.Hide(); ProgressStore.Save(Progress); }
        void ApplyPreferences()
        {
            tooltip.English = Progress.english; audioService.SetMuted(Progress.muted);
            muteButton.GetComponentInChildren<TMP_Text>().text = Progress.muted ? "Audio: off" : "Audio: on";
            languageButton.GetComponentInChildren<TMP_Text>().text = Progress.english ? "Hints: English" : "Hints: Polski";
        }
        void Clear()
        {
            tooltip.Hide(); audioService.Stop(); shopController.Close();
            villagePanel.SetActive(false); shopPanel.SetActive(false); resultsPanel.SetActive(false); exhaustedPanel.SetActive(false);
        }
        public void Village()
        {
            Clear(); villagePanel.SetActive(true);
            villageStatus.text = "Shop completed: " + Progress.shopCompletions + " times. Other locations arrive in later milestones.";
            RefreshHud();
        }
        public void OpenShop()
        {
            Clear();
            if (!CanAnswer) { exhaustedPanel.SetActive(true); return; }
            shopPanel.SetActive(true); shopController.Begin(this); RefreshHud();
        }
        public void Encounter(VocabularyEntry entry)
        {
            if (!Progress.encountered.Contains(entry.id)) Progress.encountered.Add(entry.id);
        }
        public void Wrong()
        {
            if (!Practice) Lives = Mathf.Max(0, Lives - 1);
            RefreshHud();
            if (!CanAnswer) { Finish(false); exhaustedPanel.SetActive(true); }
        }
        public void Finish(bool complete)
        {
            var correct = shopController.Correct; var mistakes = shopController.Mistakes;
            if (complete) Progress.shopCompletions++;
            Progress.lastCorrect = correct; Progress.lastMistakes = mistakes; ProgressStore.Save(Progress);
            Clear(); resultsPanel.SetActive(true);
            results.text = (complete ? "Shopping complete!" : "Attempt finished") + "\nCorrect: " + correct + "  |  Mistakes: " + mistakes + "\n" + (Practice ? "Practice session" : "Lives remaining: " + Lives);
            RefreshHud();
        }
        public void FreshAttempt() { Lives = shop.startingLives; Practice = false; OpenShop(); }
        public void StartPractice() { Practice = true; OpenShop(); }
        void OnApplicationPause(bool paused) { if (paused && Progress != null) ProgressStore.Save(Progress); }
        void OnApplicationQuit() { if (Progress != null) ProgressStore.Save(Progress); }
    }
}
