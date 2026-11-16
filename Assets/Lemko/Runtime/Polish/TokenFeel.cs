using UnityEngine;
using UnityEngine.EventSystems;
namespace Lemko
{
    // Makes a game piece feel like a thing: it rises a little under the pointer, gives when pressed, and casts a soft
    // shadow the shape of its card. Added at runtime to every token; presentation only.
    public sealed class TokenFeel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        ActivityToken token; SpriteRenderer shadow;
        static readonly Vector3 Offset=new Vector3(.05f,-.08f,.01f);
        public static void Attach(ActivityToken token)
        {
            if(!token || token.GetComponent<TokenFeel>()) return;
            var feel=token.gameObject.AddComponent<TokenFeel>(); feel.token=token;
        }
        public void OnPointerEnter(PointerEventData e) { Juice.Hover(transform,true); }
        public void OnPointerExit(PointerEventData e) { Juice.Hover(transform,false); }
        public void OnPointerDown(PointerEventData e) { if(e.button==PointerEventData.InputButton.Left) Juice.Squash(transform,.12f); }
        void OnDisable() { Juice.Hover(transform,false); }
        void LateUpdate()
        {
            var card=token ? token.backing : null;
            bool show=card && card.enabled && card.sprite && card.color.a>.2f && card.gameObject.activeInHierarchy;
            if(!shadow) {
                if(!show) return;
                var go=new GameObject("Soft shadow",typeof(SpriteRenderer)); go.transform.SetParent(card.transform,false);
                shadow=go.GetComponent<SpriteRenderer>(); shadow.color=Look.Alpha(Look.Ink,.2f);
            }
            if(shadow.enabled!=show) shadow.enabled=show;
            if(!show) return;
            shadow.transform.localPosition=Offset; shadow.transform.localRotation=Quaternion.identity; shadow.transform.localScale=Vector3.one;
            if(shadow.sprite!=card.sprite) shadow.sprite=card.sprite;
            shadow.drawMode=card.drawMode; if(card.drawMode!=SpriteDrawMode.Simple) shadow.size=card.size;
            shadow.sortingLayerID=card.sortingLayerID; shadow.sortingOrder=card.sortingOrder-1;
        }
    }
}
