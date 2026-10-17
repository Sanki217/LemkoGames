using System;
using TMPro;
using UnityEngine;
namespace Lemko
{
    public sealed class TooltipService : MonoBehaviour
    {
        public RectTransform box;
        public TMP_Text label;
        public CanvasGroup group;
        [Min(0)] public float holdSeconds = 3f;
        public bool English { get; set; }
        public event Action Revealed;
        UnityEngine.Object owner;
        VocabularyEntry pendingWord;
        Vector2 position;
        float started;
        bool visible;
        public bool IsVisibleFor(UnityEngine.Object requester) => visible && owner==requester;
        public void Show(UnityEngine.Object requester, VocabularyEntry word, Vector2 screenPosition)
        {
            if(owner==requester && pendingWord==word) return;
            Hide(); owner=requester; pendingWord=word; position=screenPosition; started=Time.unscaledTime;
        }
        public void Hide(UnityEngine.Object requester=null)
        {
            if(requester!=null && owner!=requester) return;
            owner=null; pendingWord=null; visible=false;
            if(box) box.gameObject.SetActive(false);
        }
        void Update()
        {
            if(!owner || !pendingWord || (owner is Behaviour b && !b.isActiveAndEnabled)) { Hide(); return; }
            if(!visible && Time.unscaledTime-started>=holdSeconds) Reveal();
            if(visible) group.alpha=Mathf.MoveTowards(group.alpha,1,Time.unscaledDeltaTime*8);
        }
        void Reveal()
        {
            visible=true; label.text=pendingWord.Translation(English);
            box.gameObject.SetActive(true); group.blocksRaycasts=false; group.interactable=false; group.alpha=0;
            var parent=(RectTransform)box.parent;
            var canvas=box.GetComponentInParent<Canvas>();
            var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,position,camera,out var p);
            p+=new Vector2(0,48); var half=box.rect.size*.5f;
            p.x=Mathf.Clamp(p.x,parent.rect.xMin+half.x,parent.rect.xMax-half.x);
            p.y=Mathf.Clamp(p.y,parent.rect.yMin+half.y,parent.rect.yMax-half.y);
            box.anchoredPosition=p; box.SetAsLastSibling(); Revealed?.Invoke();
        }
        void OnDisable() => Hide();
    }
}
