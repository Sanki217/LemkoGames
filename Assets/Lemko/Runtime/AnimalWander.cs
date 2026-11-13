using UnityEngine;
namespace Lemko {
    // An animal standing in the pasture: it strolls to a nearby spot, rests, and strolls again.
    // Presentation only. With reduced motion the animal stays where it was put.
    public sealed class AnimalWander : MonoBehaviour {
        public GameSession session;
        public Rect area;
        public SpriteRenderer picture;
        Vector2 target; float wait, speed, phase; bool walking; Vector3 pictureHome;
        void Start() { speed=Random.Range(.35f,.6f); wait=Random.Range(.4f,1.8f); phase=Random.Range(0f,6f); if(picture) pictureHome=picture.transform.localPosition; Sort(); }
        // Animals lower on the screen are drawn in front.
        void Sort() { if(picture) picture.sortingOrder=3+Mathf.RoundToInt((area.yMax-transform.localPosition.y)*4); }
        void Update() {
            if(!session || session.Progress==null) return;
            Sort();
            if(session.Progress.reducedMotion || !session.CanAnswer) { Step(0); return; }
            Vector2 at=transform.localPosition;
            if(!walking) {
                Step(0); wait-=Time.unscaledDeltaTime; if(wait>0) return;
                target=new Vector2(Mathf.Clamp(at.x+Random.Range(-2.4f,2.4f),area.xMin,area.xMax),Mathf.Clamp(at.y+Random.Range(-.9f,.9f),area.yMin,area.yMax));
                walking=true; if(picture) picture.flipX=target.x<at.x;
            }
            var next=Vector2.MoveTowards(at,target,speed*Time.unscaledDeltaTime);
            transform.localPosition=new Vector3(next.x,next.y,transform.localPosition.z);
            Step(Mathf.Abs(Mathf.Sin((Time.unscaledTime+phase)*8f))*.04f);
            if(next==target) { walking=false; wait=Random.Range(1.2f,3.5f); }
        }
        void Step(float lift) { if(picture) picture.transform.localPosition=pictureHome+Vector3.up*lift; }
        void OnDisable() { Step(0); }
    }
}
