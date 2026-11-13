using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Lemko {
    public sealed class ActivityIntroduction : MonoBehaviour, IPointerClickHandler {
        public ActivityIntroductionDefinition definition;
        public GameObject panel;
        public TMP_Text title,story,instructions,support;
        public Image illustration;
        public Button repeatButton;
        GameSession owner;
        public void ToggleRepeat() {
            if(!owner || !definition) return;
            var skipped=owner.Progress.skippedIntroductions;
            if(!skipped.Remove(definition.activityId)) skipped.Add(definition.activityId);
            RefreshRepeat();
        }
        void RefreshRepeat() { if(repeatButton) repeatButton.GetComponentInChildren<TMP_Text>().text=owner.Progress.skippedIntroductions.Contains(definition.activityId) ? "Show next time: off" : "Show next time: on"; }
        public bool IsOpen=>panel && panel.activeSelf;
        Action begin;
        bool pending;
        float opened;
        public void Show(GameSession session,Action onContinue) {
            Cancel(); owner=session; begin=onContinue;
            if(repeatButton) { repeatButton.onClick.RemoveListener(ToggleRepeat); repeatButton.onClick.AddListener(ToggleRepeat); RefreshRepeat(); }
            if(EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
            title.text=definition.title; story.text=definition.story;
            instructions.text=definition.activityId=="Farm" && !session.FarmQuantitiesUnlocked && !string.IsNullOrWhiteSpace(definition.beforeNumbers) ? definition.beforeNumbers : definition.instructions;
            support.text="Stuck? Tap the lightbulb, or hold a word or picture, to see what it means.";
            illustration.sprite=definition.illustration; illustration.enabled=definition.illustration;
            session.tooltip.Hide(); session.audioService.Stop();
            panel.SetActive(true); transform.SetAsLastSibling(); opened=Time.unscaledTime;
        }
        public void OnPointerClick(PointerEventData e) { if(e.button==PointerEventData.InputButton.Left) Continue(); }
        void Update() {
            if(IsOpen && Keyboard.current!=null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)) Continue();
        }
        void Continue() {
            if(!IsOpen || pending || Time.unscaledTime-opened<.15f) return;
            pending=true; StartCoroutine(BeginNextFrame());
        }
        IEnumerator BeginNextFrame() {
            // Consume the dismissal click before any interactive game objects exist.
            yield return null;
            var action=begin; begin=null; pending=false; panel.SetActive(false); action?.Invoke();
        }
        public void Cancel() { StopAllCoroutines(); begin=null; pending=false; if(panel) panel.SetActive(false); }
        void OnDisable()=>Cancel();
    }
}
