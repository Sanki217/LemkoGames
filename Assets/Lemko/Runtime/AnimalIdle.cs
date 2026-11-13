using UnityEngine;
namespace Lemko {
    public sealed class AnimalIdle : MonoBehaviour {
        public GameSession session;
        Vector3 home; float phase;
        void Start() { home=transform.localPosition; phase=Random.Range(0f,5f); }
        void Update() {
            if(!session || session.Progress==null) return;
            float t=(Time.unscaledTime+phase)%5f;
            float lift=!session.Progress.reducedMotion && session.CanAnswer && t<.45f ? Mathf.Sin(t/.45f*Mathf.PI)*.045f : 0;
            transform.localPosition=home+Vector3.up*lift;
        }
        void OnDisable() { transform.localPosition=home; }
    }
}
