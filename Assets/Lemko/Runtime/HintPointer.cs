using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace Lemko
{
    // One shared delay policy for world objects and UI list rows.
    public sealed class HintPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public VocabularyEntry entry;
        public TooltipService tooltip;
        public bool BlocksSelection { get; private set; }
        public void Bind(VocabularyEntry word,TooltipService service)
        { if(tooltip) tooltip.Hide(this); entry=word; tooltip=service; BlocksSelection=false; }
        static bool Touch(PointerEventData e) => e is ExtendedPointerEventData p && p.pointerType==UIPointerType.Touch;
        void Request(PointerEventData e) { if(entry && tooltip) tooltip.Show(this,entry,e.position); }
        public void OnPointerEnter(PointerEventData e) { if(!Touch(e)) Request(e); }
        public void OnPointerExit(PointerEventData e) { if(tooltip) tooltip.Hide(this); }
        public void OnPointerDown(PointerEventData e) { BlocksSelection=false; if(Touch(e)) Request(e); }
        public void OnPointerUp(PointerEventData e)
        {
            if(!Touch(e)) return;
            BlocksSelection=tooltip && tooltip.IsVisibleFor(this);
            if(tooltip) tooltip.Hide(this);
        }
        void OnDisable() { if(tooltip) tooltip.Hide(this); BlocksSelection=false; }
    }
}
