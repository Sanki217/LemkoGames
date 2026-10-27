using UnityEngine;
using UnityEngine.EventSystems;
namespace Lemko
{
    // Explicit mouse/touch hold shares the same timer, reveal event and counter as hover hints.
    public sealed class HoldTranslationHint : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerMoveHandler
    {
        public VocabularyEntry word;
        public TooltipService tooltip;
        public HintContent content;
        public void Bind(VocabularyEntry entry,TooltipService service,HintContent kind=HintContent.Translation) { Cancel(); word=entry; tooltip=service; content=kind; }
        public void OnPointerDown(PointerEventData e) { if(e.button==PointerEventData.InputButton.Left && word && tooltip) tooltip.Show(this,word,e.position,content); }
        public void OnPointerMove(PointerEventData e) { if(tooltip) tooltip.MovePointer(this,e.position); }
        public void OnPointerUp(PointerEventData e)=>Cancel();
        public void OnPointerExit(PointerEventData e)=>Cancel();
        public void Cancel() { if(tooltip) tooltip.Hide(this); }
        void OnDisable()=>Cancel();
    }
}
