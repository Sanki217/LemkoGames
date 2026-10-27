using UnityEngine;
using UnityEngine.UI;
namespace Lemko
{
    // A vector ring: no texture dependency and no pointer interception.
    public sealed class HintProgressRing : MaskableGraphic
    {
        [Range(0,1)] public float progress;
        public Color trackColor=new Color(.12f,.23f,.21f,.95f);
        public void SetProgress(float value) { value=Mathf.Clamp01(value); if(Mathf.Approximately(progress,value)) return; progress=value; SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper vh) {
            vh.Clear(); var rect=GetPixelAdjustedRect(); float radius=Mathf.Min(rect.width,rect.height)*.5f;
            for(int i=0;i<64;i++) {
                float a=(90-i*360f/64)*Mathf.Deg2Rad, b=(90-(i+1)*360f/64)*Mathf.Deg2Rad;
                var c=i/64f<progress ? color : trackColor; int start=vh.currentVertCount;
                vh.AddVert(rect.center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,c,Vector2.zero);
                vh.AddVert(rect.center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,c,Vector2.zero);
                vh.AddVert(rect.center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*(radius-5),c,Vector2.zero);
                vh.AddVert(rect.center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(radius-5),c,Vector2.zero);
                vh.AddTriangle(start,start+1,start+2); vh.AddTriangle(start,start+2,start+3);
            }
        }
    }
}
