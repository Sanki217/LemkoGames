using System.Collections;
using UnityEngine;
namespace Lemko {
    // Transient presentation only. Never moves colliders during a pulse or changes answers.
    public sealed class PresentationMotion : MonoBehaviour {
        Vector3 scale,position; bool captured;
        public static void Pulse(Transform target,GameSession session) {
            if(!target || !session || session.Progress.reducedMotion) return;
            var effect=target.GetComponent<PresentationMotion>() ?? target.gameObject.AddComponent<PresentationMotion>();
            effect.ResetMotion(); effect.Capture(); effect.StartCoroutine(effect.Animate(false,Vector3.zero));
        }
        public static void Travel(Transform target,Vector3 from,GameSession session) {
            if(!target || !session || session.Progress.reducedMotion) return;
            var effect=target.GetComponent<PresentationMotion>() ?? target.gameObject.AddComponent<PresentationMotion>();
            effect.ResetMotion(); effect.Capture(); effect.StartCoroutine(effect.Animate(true,target.parent ? target.parent.InverseTransformPoint(from) : from));
        }
        void Capture() { scale=transform.localScale; position=transform.localPosition; captured=true; }
        IEnumerator Animate(bool travel,Vector3 from) {
            for(float elapsed=0;elapsed<.28f;elapsed+=Time.unscaledDeltaTime) {
                float t=Mathf.Clamp01(elapsed/.28f); float wave=Mathf.Sin(t*Mathf.PI);
                transform.localScale=scale*(1+wave*.09f);
                if(travel) transform.localPosition=Vector3.Lerp(from,position,1-Mathf.Pow(1-t,3))+Vector3.up*(wave*.35f);
                yield return null;
            }
            Restore();
        }
        void Restore() { if(!captured) return; transform.localScale=scale; transform.localPosition=position; captured=false; }
        void ResetMotion() { StopAllCoroutines(); Restore(); }
        void OnDisable()=>ResetMotion();
    }
}
