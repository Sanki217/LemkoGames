using UnityEngine;
using UnityEngine.EventSystems;
namespace Lemko
{
    public sealed class WorldDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ActivityToken token;
        Vector3 origin;
        bool dragging;
        GameObject preview;
        void ClearPreview() { if(preview) Destroy(preview); preview=null; }
        int suppressClickUntil=-1;
        public bool BlocksClick => dragging || Time.frameCount<=suppressClickUntil;
        HintPointer hint;
        bool restoreHint;
        public void OnBeginDrag(PointerEventData e) { if(e.button!=PointerEventData.InputButton.Left) return; dragging=true; origin=transform.position; hint=GetComponent<HintPointer>(); restoreHint=hint && hint.enabled; if(hint) hint.enabled=false; if(token.view.tooltip) token.view.tooltip.Hide(); }
        public void OnDrag(PointerEventData e) { if(!dragging) return; var p=Camera.main.ScreenToWorldPoint(e.position); transform.position=new Vector3(p.x,p.y,origin.z-1);
            ClearPreview();
            foreach(var hit in Physics2D.OverlapPointAll(p)) if(hit.GetComponent<WorldDropZone>()) {
                preview=new GameObject("Placement preview",typeof(SpriteRenderer)); var r=preview.GetComponent<SpriteRenderer>();
                r.sprite=token.view.worldPicture.sprite; r.color=new Color(1,1,1,.4f); r.sortingOrder=10;
                preview.transform.position=new Vector3(p.x,p.y,origin.z); break;
            } }
        public void OnEndDrag(PointerEventData e)
        {
            if(!dragging) return;
            ClearPreview(); suppressClickUntil=Time.frameCount+1;
            var p=Camera.main.ScreenToWorldPoint(e.position); transform.position=origin; dragging=false; if(hint) hint.enabled=restoreHint;
            foreach(var hit in Physics2D.OverlapPointAll(p)) { var zone=hit.GetComponent<WorldDropZone>(); if(zone && zone.Accept(token,p)) break; }
        }
        void OnDisable() { ClearPreview(); if(dragging) transform.position=origin; dragging=false; }
    }
}
