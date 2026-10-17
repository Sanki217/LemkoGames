using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Lemko
{
    public sealed class WordView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public VocabularyEntry entry;
        public TMP_Text label;
        public Image picture;
        public Button selectButton;
        public Button audioButton;
        public TooltipService tooltip;
        public PronunciationService audioService;
        [SerializeField] Color idleColor = new Color(.39f, .53f, .39f);
        Action<WordView> selected;
        public VocabularyEntry Entry => entry;
        void Awake() { selectButton.onClick.AddListener(Select); audioButton.onClick.AddListener(Speak); }
        void Start() { if (entry && selected == null) Bind(entry, tooltip, audioService, null); }
        public void Bind(VocabularyEntry word, TooltipService tips, PronunciationService audio, Action<WordView> onSelect)
        {
            if (tooltip) tooltip.Hide(this);
            entry = word; tooltip = tips; audioService = audio; selected = onSelect;
            label.text = word ? word.lemko : "Missing word";
            picture.sprite = word ? word.sprite : null;
            picture.color = word && word.sprite ? Color.white : new Color(.75f, .82f, .69f);
            selectButton.interactable = word;
            selectButton.image.color = idleColor;
            audioButton.interactable = word && word.pronunciation;
        }
        void Select() { if (selectButton.interactable) selected?.Invoke(this); }
        void Speak() { if (entry && audioService) audioService.Play(entry.pronunciation); }
        public void Mark(bool correct)
        {
            selectButton.interactable = false;
            selectButton.image.color = correct ? new Color(.15f, .45f, .25f) : new Color(.6f, .2f, .15f);
            label.text = (correct ? "OK: " : "Try another: ") + entry.lemko;
        }
        public void OnPointerEnter(PointerEventData e) { if (entry && tooltip) tooltip.Show(this, entry, e.position); }
        public void OnPointerExit(PointerEventData e) { if (tooltip) tooltip.Hide(this); }
        void OnDisable() { if (tooltip) tooltip.Hide(this); selected = null; }
    }
}
