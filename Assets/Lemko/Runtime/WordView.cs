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
        float lifted; bool hovering;
        GameSession owner;
        public void OnPointerEnter(PointerEventData e) { hovering=true; }
        public void OnPointerExit(PointerEventData e) { hovering=false; }
        void LateUpdate() {
            if(!worldPicture || !worldPicture.gameObject.activeInHierarchy || !owner || owner.Progress==null) return;
            // Pixel art is never enlarged: a picture under the pointer rises by one pixel instead. Only the change is
            // applied, so anything else that moves the picture (a strolling animal) keeps working.
            float lift=hovering && !submitted && owner.CanAnswer && !owner.Progress.reducedMotion ? Pixels.Unit : 0;
            if(lift!=lifted && !(worldPicture.TryGetComponent<Motion>(out var moving) && moving.enabled)) { worldPicture.transform.localPosition+=Vector3.up*(lift-lifted); lifted=lift; }
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
                picture.color = word && word.sprite ? Color.white : Look.Sand;
            }
            if (worldPicture) {
                worldPicture.sprite = word && word.sprite ? word.sprite : placeholderSprite;
                worldPicture.color = word && word.sprite ? Color.white : Look.Sand;
                // Every picture is shown at its own size: 40 pixels to a unit, like everything else in the game.
                worldPicture.transform.localPosition-=Vector3.up*lifted; lifted=0;
                Pixels.Native(worldPicture.transform);
            }
            if(selectButton) { selectButton.interactable = word; selectButton.image.color = Look.Paper; }
            if(audioButton) audioButton.interactable = word && word.pronunciation;
            if(worldSpeaker) worldSpeaker.SetActive(word && word.pronunciation);
            SpeakerIcon();
        }
        // The speaker used to be the word "Play"; it is a small loudspeaker badge in the same place.
        void SpeakerIcon()
        {
            if(!worldSpeaker || worldSpeaker.transform.Find("Speaker icon")) return;
            var icon=new GameObject("Speaker icon",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            icon.transform.SetParent(worldSpeaker.transform,false); icon.sprite=Look.SpeakerSmall; icon.sortingOrder=20;
            foreach(var text in worldSpeaker.GetComponentsInChildren<TMP_Text>(true)) {
                icon.transform.position=text.transform.position;
                if(text.TryGetComponent<Renderer>(out var mesh)) { icon.sortingLayerID=mesh.sortingLayerID; icon.sortingOrder=mesh.sortingOrder; mesh.enabled=false; }
                text.text="";
            }
            float parent=Mathf.Max(.0001f,Mathf.Abs(worldSpeaker.transform.lossyScale.x));
            icon.transform.localScale=Vector3.one/parent;   // the badge at its own pixel size
        }
        // A picture with no card behind it wears its loudspeaker on its own corner.
        public void SpeakerOnPicture() { if(worldSpeaker) worldSpeaker.transform.localPosition=new Vector3(.5f,.5f,worldSpeaker.transform.localPosition.z); }
        public void Select() { if (entry && !submitted && (!selectButton || selectButton.interactable)) selected?.Invoke(this); }
        public void Speak() { if (entry && audioService) audioService.Play(entry.pronunciation); }
        public void OnPointerClick(PointerEventData e) { if ((!TryGetComponent<WorldDrag>(out var drag) || !drag.BlocksClick) && worldPicture && e.button == PointerEventData.InputButton.Left && (!hint || !hint.BlocksSelection)) Select(); }
        public void SetSelected(bool value)
        {
            if(selectionMarker) selectionMarker.enabled=value;
            if(worldPicture) worldPicture.color=entry && entry.sprite ? Color.white : Look.Sand;
            if(selectButton) selectButton.image.color=value ? Look.Honey : Look.Paper;
        }
        public void Mark(bool correct)
        {
            submitted = true;
            if(selectButton) { selectButton.interactable = false; selectButton.image.color = correct ? Look.GoodSoft : Look.Sand; }
            if(worldPicture) worldPicture.color = correct ? Color.white : Look.Hex(0xE68F4B);   // a wrong pick blushes red
            if(label && !worldPicture) label.text = (correct ? "OK: " : "Try another: ") + entry.lemko;
        }
        void OnDisable() { if (tooltip && hint) tooltip.Hide(hint); selected = null; }
    }
}
