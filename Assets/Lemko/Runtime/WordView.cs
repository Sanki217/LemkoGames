using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Lemko
{
    public sealed class WordView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public VocabularyEntry entry;
        public TMP_Text label;
        public Image picture;
        public Button selectButton;
        public Button audioButton;
        public TooltipService tooltip;
        public PronunciationService audioService;
        [Header("Optional scene sprite presentation")]
        public SpriteRenderer worldPicture;
        public SpriteRenderer selectionMarker;
        public Sprite placeholderSprite;
        public GameObject worldSpeaker;
        public Vector2 pictureBounds = new Vector2(1.5f, 1.65f);
        bool submitted;
        Vector3 restingScale; bool hovering;
        GameSession owner;
        public void OnPointerEnter(PointerEventData e) { hovering=true; }
        public void OnPointerExit(PointerEventData e) { hovering=false; }
        void LateUpdate() {
            if(!worldPicture || !worldPicture.gameObject.activeInHierarchy || !owner || owner.Progress==null) return;
            bool lift=hovering && !submitted && owner.CanAnswer && !owner.Progress.reducedMotion;
            worldPicture.transform.localScale=Vector3.Lerp(worldPicture.transform.localScale,restingScale*(lift?1.055f:1),1-Mathf.Exp(-18*Time.unscaledDeltaTime));
        }
        HintPointer hint;
        [SerializeField] Color idleColor = new Color(.39f, .53f, .39f);
        Action<WordView> selected;
        public VocabularyEntry Entry => entry;
        void Awake() { if(selectButton) selectButton.onClick.AddListener(Select); if(audioButton) audioButton.onClick.AddListener(Speak); }
        void Start() { if (entry && selected == null) Bind(entry, tooltip, audioService, null); }
        public void Bind(VocabularyEntry word, TooltipService tips, PronunciationService audio, Action<WordView> onSelect)
        {
            if (tooltip) tooltip.Hide(this);
            entry = word; tooltip = tips; audioService = audio; selected = onSelect;
            submitted = false; hovering=false; owner=FindFirstObjectByType<GameSession>();
            if(selectionMarker) selectionMarker.enabled=false;
            hint=GetComponent<HintPointer>(); if(!hint) hint=gameObject.AddComponent<HintPointer>(); hint.Bind(word,tips);
            if(label) { label.text = word ? word.lemko : "Missing word"; label.gameObject.SetActive(!worldPicture); }
            if (picture) {
                picture.sprite = word ? word.sprite : null;
                picture.color = word && word.sprite ? Color.white : new Color(.75f, .82f, .69f);
            }
            if (worldPicture) {
                worldPicture.sprite = word && word.sprite ? word.sprite : placeholderSprite;
                worldPicture.color = word && word.sprite ? Color.white : idleColor;
                if (worldPicture.sprite) {
                    var size = worldPicture.sprite.bounds.size;
                    float scale = Mathf.Min(pictureBounds.x / size.x, pictureBounds.y / size.y);
                    worldPicture.transform.localScale = Vector3.one * scale;
                }
            }
            if(worldPicture) restingScale=worldPicture.transform.localScale;
            if(selectButton) { selectButton.interactable = word; selectButton.image.color = idleColor; }
            if(audioButton) audioButton.interactable = word && word.pronunciation;
            if(worldSpeaker) worldSpeaker.SetActive(word && word.pronunciation);
        }
        public void Select() { if (entry && !submitted && (!selectButton || selectButton.interactable)) selected?.Invoke(this); }
        public void Speak() { if (entry && audioService) audioService.Play(entry.pronunciation); }
        public void OnPointerClick(PointerEventData e) { if ((!TryGetComponent<WorldDrag>(out var drag) || !drag.BlocksClick) && worldPicture && e.button == PointerEventData.InputButton.Left && (!hint || !hint.BlocksSelection)) Select(); }
        public void SetSelected(bool value)
        {
            if(selectionMarker) selectionMarker.enabled=value;
            if(worldPicture) worldPicture.color=entry && entry.sprite ? Color.white : idleColor;
            if(selectButton) selectButton.image.color=value ? new Color(.2f,.4f,.6f) : idleColor;
        }
        public void Mark(bool correct)
        {
            submitted = true;
            if(selectButton) { selectButton.interactable = false; selectButton.image.color = correct ? new Color(.15f, .45f, .25f) : new Color(.6f, .2f, .15f); }
            if(worldPicture) worldPicture.color = correct ? new Color(.5f,1f,.5f) : new Color(1f,.4f,.4f);
            if(label && !worldPicture) label.text = (correct ? "OK: " : "Try another: ") + entry.lemko;
        }
        void OnDisable() { if (tooltip && hint) tooltip.Hide(hint); selected = null; }
    }
}
