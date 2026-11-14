using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace Lemko
{
    // A hint is always asked for on purpose: the right mouse button shows it at once,
    // and on a touch screen a press and hold does. Simply pointing at something never loads one.
    public sealed class HintPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        public VocabularyEntry entry;
        public TooltipService tooltip;
        public HintContent content;
        public bool BlocksSelection { get; private set; }
        public void Bind(VocabularyEntry word,TooltipService service,HintContent kind=HintContent.Translation)
        { if(tooltip) tooltip.Hide(this); entry=word; tooltip=service; content=kind; BlocksSelection=false; }
        static bool Touch(PointerEventData e) => e is ExtendedPointerEventData p && p.pointerType==UIPointerType.Touch;
        public void OnPointerMove(PointerEventData e) { if(tooltip && Touch(e)) tooltip.MovePointer(this,e.position); }
        public void OnPointerEnter(PointerEventData e) { }
        public void OnPointerExit(PointerEventData e) { if(tooltip) tooltip.Hide(this); }
        public void OnPointerDown(PointerEventData e)
        {
            BlocksSelection=false; if(!tooltip) return;
            if(Touch(e)) { if(entry) tooltip.Show(this,entry,e.position,content); }
            else if(e.button==PointerEventData.InputButton.Right && entry && !tooltip.IsVisibleFor(this)) tooltip.RevealNow(this,entry,e.position,content);
            else tooltip.Hide(this);
        }
        public void OnPointerUp(PointerEventData e)
        {
            if(!Touch(e)) return;
            BlocksSelection=tooltip && tooltip.IsVisibleFor(this);
            if(tooltip) tooltip.Hide(this);
        }
        void OnDisable() { if(tooltip) tooltip.Hide(this); BlocksSelection=false; }
    }
}
