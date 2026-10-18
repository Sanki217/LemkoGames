using UnityEngine;
using UnityEngine.EventSystems;
namespace Lemko
{
    public sealed class WorldDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ActivityToken token;
        Vector3 origin;
        bool dragging;
        HintPointer hint;
        bool restoreHint;
        public void OnBeginDrag(PointerEventData e) { dragging=true; origin=transform.position; hint=GetComponent<HintPointer>(); restoreHint=hint && hint.enabled; if(hint) hint.enabled=false; if(token.view.tooltip) token.view.tooltip.Hide(); }
        public void OnDrag(PointerEventData e) { if(!dragging) return; var p=Camera.main.ScreenToWorldPoint(e.position); transform.position=new Vector3(p.x,p.y,origin.z-1); }
        public void OnEndDrag(PointerEventData e)
        {
            if(!dragging) return;
            var p=Camera.main.ScreenToWorldPoint(e.position); transform.position=origin; dragging=false; if(hint) hint.enabled=restoreHint;
            foreach(var hit in Physics2D.OverlapPointAll(p)) { var zone=hit.GetComponent<WorldDropZone>(); if(zone && zone.Accept(token)) break; }
        }
        void OnDisable() { if(dragging) transform.position=origin; dragging=false; }
    }
}
