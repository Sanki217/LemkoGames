using System;
using TMPro;
using UnityEngine;
namespace Lemko
{
    public enum HintContent { Translation, LemkoSpelling }
    public sealed class TooltipService : MonoBehaviour
    {
        public RectTransform box;
        public TMP_Text label;
        public CanvasGroup group;
        public RectTransform waiting;
        public HintProgressRing progressRing;
        HintContent content;
        [Header("Stationary hint timing")]
        [Min(0)] public float stillSeconds=.5f;
        [Tooltip("Ring fill duration after the stationary delay.")]
        [Min(0)] public float holdSeconds=1f;
        [Min(0)] public float movementTolerance=4f;
        public bool ReducedMotion {get;set;}
        public bool English { get; set; }
        public event Action Revealed;
        public VocabularyEntry RevealedWord => visible ? pendingWord : null;
        UnityEngine.Object owner;
        VocabularyEntry pendingWord;
        Vector2 position;
        float started;
        bool visible;
        public bool IsVisibleFor(UnityEngine.Object requester) => visible && owner==requester;
        public void Show(UnityEngine.Object requester, VocabularyEntry word, Vector2 screenPosition, HintContent kind=HintContent.Translation)
        {
            if(owner==requester && pendingWord==word && content==kind) return;
            Hide(); owner=requester; pendingWord=word; position=screenPosition; started=Time.unscaledTime; content=kind;
            if(progressRing) progressRing.SetProgress(0);
        }
        // Immediate reveal for an explicit hint button; counted exactly like a held hint.
        public void RevealNow(UnityEngine.Object requester,VocabularyEntry word,Vector2 screenPosition,HintContent kind=HintContent.Translation)
        {
            if(!word) return;
            Hide(); owner=requester; pendingWord=word; position=screenPosition; started=Time.unscaledTime; content=kind;
            Reveal();
        }
        public void MovePointer(UnityEngine.Object requester,Vector2 screenPosition) {
            if(owner!=requester || (screenPosition-position).sqrMagnitude<=movementTolerance*movementTolerance) return;
            position=screenPosition; started=Time.unscaledTime; visible=false;
            if(box) box.gameObject.SetActive(false);
            if(waiting) waiting.gameObject.SetActive(false);
            if(progressRing) progressRing.SetProgress(0);
        }
        public void Hide(UnityEngine.Object requester=null)
        {
            if(requester!=null && owner!=requester) return;
            owner=null; pendingWord=null; visible=false;
            if(box) box.gameObject.SetActive(false);
            if(waiting) waiting.gameObject.SetActive(false);
        }
        void Update()
        {
            if(!owner || !pendingWord || (owner is Behaviour b && !b.isActiveAndEnabled)) { Hide(); return; }
            if(!visible) {
                float elapsed=Time.unscaledTime-started-stillSeconds;
                if(elapsed>=0) {
                    if(waiting && !waiting.gameObject.activeSelf) { waiting.gameObject.SetActive(true); Place(waiting); }
                    if(progressRing) progressRing.SetProgress(holdSeconds<=0 ? 1 : elapsed/holdSeconds);
                    if(elapsed>=holdSeconds) Reveal();
                }
            }
            if(visible) group.alpha=Mathf.MoveTowards(group.alpha,1,Time.unscaledDeltaTime*8);
        }
        void Reveal()
        {
            visible=true; label.text=content==HintContent.LemkoSpelling ? pendingWord.lemko : pendingWord.Translation(English);
            if(waiting) waiting.gameObject.SetActive(false);
            box.gameObject.SetActive(true); group.blocksRaycasts=false; group.interactable=false; group.alpha=ReducedMotion ? 1 : 0;
            Place(box); Revealed?.Invoke();
        }
        void Place(RectTransform target) {
            var parent=(RectTransform)target.parent;
            var canvas=box.GetComponentInParent<Canvas>();
            var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,position,camera,out var p);
            p+=new Vector2(0,65); var half=target.rect.size*.5f;
            p.x=Mathf.Clamp(p.x,parent.rect.xMin+half.x,parent.rect.xMax-half.x);
            p.y=Mathf.Clamp(p.y,parent.rect.yMin+half.y,parent.rect.yMax-half.y);
            target.anchoredPosition=p; target.SetAsLastSibling();
        }
        void OnDisable() => Hide();
    }
}
