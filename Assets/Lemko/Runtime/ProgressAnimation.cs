using UnityEngine;
using UnityEngine.UI;
namespace Lemko {
    public sealed class ProgressAnimation : MonoBehaviour {
        public GameSession session; public Image fill;
        float target,shown; bool initialized;
        void LateUpdate() {
            if(!fill || !session || session.Progress==null) return;
            if(!initialized) { shown=fill.fillAmount; target=shown; initialized=true; }
            if(!Mathf.Approximately(fill.fillAmount,shown)) target=fill.fillAmount;
            shown=session.Progress.reducedMotion ? target : Mathf.MoveTowards(shown,target,Time.unscaledDeltaTime*1.8f);
            fill.fillAmount=shown;
        }
    }
}
