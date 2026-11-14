using System.Collections;
using TMPro;
using UnityEngine;
namespace Lemko
{
    // Presentation only: never scores, checks answers or blocks input.
    public sealed class AnswerFeedback : MonoBehaviour
    {
        public TMP_Text message, hud;
        public bool animate=true;
        public bool reducedMotion {get;set;}
        [Min(.1f)] public float duration=.55f;
        [Range(0,.08f)] public float scaleAmount=.025f;
        public Color successColor=new Color(.65f,1f,.7f);
        public Color correctionColor=new Color(1f,.84f,.55f);
        Color originalColor;
        Vector3 messageScale, hudScale;
        bool captured;
        void Capture() { if(captured || !message || !hud) return; originalColor=message.color; messageScale=message.transform.localScale; hudScale=hud.transform.localScale; captured=true; }
        public void Play(bool correct) {
            Capture(); Cancel(); if(!captured || !message.gameObject.activeInHierarchy) return;
            message.color=correct ? Look.Good : Look.Brick;
            if(animate && !reducedMotion) StartCoroutine(Pulse(correct));
        }
        IEnumerator Pulse(bool correct) {
            float elapsed=0; Color tint=message.color;
            while(elapsed<duration) {
                float t=elapsed/Mathf.Max(.1f,duration);
                // The colour settles back; nothing is enlarged, so the pixels around the text stay put.
                message.color=Color.Lerp(tint,originalColor,t);
                elapsed+=Time.unscaledDeltaTime; yield return null;
            }
            Restore();
        }
        void Restore() { if(!captured) return; if(message) { message.color=originalColor; message.transform.localScale=messageScale; } if(hud) hud.transform.localScale=hudScale; }
        public void Cancel() { StopAllCoroutines(); Restore(); }
        void OnDisable()=>Cancel();
    }
}
