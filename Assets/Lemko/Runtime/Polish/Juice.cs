using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Lemko
{
    // The shared "juice kit": motion, sparkles, floating text, scene transitions and optional sound effects.
    // Lives on one object that survives scene loads. Never checks answers, scores or blocks play
    // (the transition cover blocks clicks only while a scene is changing).
    public sealed class Juice : MonoBehaviour
    {
        static Juice instance;
        RectTransform layer; Image cover; AudioSource audioSource;
        readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
        // A particle is a plain square of whole picture pixels; it moves from pixel to pixel and shrinks away at the end.
        sealed class Particle { public RectTransform rect; public Image image; public Vector2 velocity, at, size; public float gravity, life, age; public bool keep; }
        readonly List<Particle> particles=new List<Particle>(); readonly Stack<Particle> pool=new Stack<Particle>();
        bool changingScene;
        public static bool Reduced=>GameSession.ReducedMotion;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset() { instance=null; }
        static Juice Instance
        {
            get {
                if(instance) return instance;
                var go=new GameObject("Lemko Juice",typeof(RectTransform)); DontDestroyOnLoad(go);
                var canvas=go.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=900;
                var scaler=go.AddComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1280,720); scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
                WorldCameraFit.Apply(scaler);
                go.AddComponent<GraphicRaycaster>();
                instance=go.AddComponent<Juice>(); instance.layer=(RectTransform)go.transform;
                instance.audioSource=go.AddComponent<AudioSource>(); instance.audioSource.playOnAwake=false;
                var coverObject=new GameObject("Scene cover",typeof(RectTransform)); coverObject.transform.SetParent(go.transform,false);
                var rect=(RectTransform)coverObject.transform; rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero;
                instance.cover=coverObject.AddComponent<Image>(); instance.cover.color=Look.Alpha(Look.Peach,0); instance.cover.raycastTarget=false;
                // The picture is 1280 x 720 interface units in the middle of the window; whatever lies beyond is a dark frame.
                Frame(go.transform,new Vector2(0,0),new Vector2(.5f,1),Vector2.zero,new Vector2(-640,0)); Frame(go.transform,new Vector2(.5f,0),new Vector2(1,1),new Vector2(640,0),Vector2.zero);
                Frame(go.transform,new Vector2(0,.5f),new Vector2(1,1),new Vector2(0,360),Vector2.zero); Frame(go.transform,new Vector2(0,0),new Vector2(1,.5f),Vector2.zero,new Vector2(0,-360));
                SceneManager.sceneLoaded+=instance.SceneLoaded;
                return instance;
            }
        }
        static void Frame(Transform parent,Vector2 anchorMin,Vector2 anchorMax,Vector2 offsetMin,Vector2 offsetMax)
        {
            var go=new GameObject("Frame",typeof(RectTransform)); go.transform.SetParent(parent,false);
            var rect=(RectTransform)go.transform; rect.anchorMin=anchorMin; rect.anchorMax=anchorMax; rect.offsetMin=offsetMin; rect.offsetMax=offsetMax;
            var image=go.AddComponent<Image>(); image.color=Look.Night; image.raycastTarget=false;
        }
        void OnDestroy() { SceneManager.sceneLoaded-=SceneLoaded; }

        // ---------- motion ----------
        static Motion On(Transform target) { if(!target) return null; var m=target.GetComponent<Motion>(); return m ? m : target.gameObject.AddComponent<Motion>(); }
        static bool Skip(Transform target)=>!target || Reduced || !target.gameObject.activeInHierarchy;
        public static void Pop(Transform target,float delay=0) { if(!Skip(target)) On(target).Play(Motion.Kind.Pop,.3f,0,delay); }
        public static void Punch(Transform target,float amount=.16f,float duration=.3f,float delay=0) { if(!Skip(target)) On(target).Play(Motion.Kind.Punch,duration,amount,delay); }
        public static void Squash(Transform target,float amount=.22f) { if(!Skip(target)) On(target).Play(Motion.Kind.Squash,.38f,amount); }
        public static void Shake(Transform target,float amount) { if(!Skip(target)) On(target).Play(Motion.Kind.Shake,.34f,amount); }
        public static void Hop(Transform target,float height,float delay=0) { if(!Skip(target)) On(target).Play(Motion.Kind.Hop,.34f,height,delay); }
        public static void Drop(Transform target,float height) { if(!Skip(target)) On(target).Play(Motion.Kind.Drop,.5f,height); }
        public static void TravelFrom(Transform target,Vector3 worldFrom,float arc=.35f)
        { if(!Skip(target)) On(target).Play(Motion.Kind.Travel,.32f,arc,0,target.parent ? target.parent.InverseTransformPoint(worldFrom) : worldFrom); }
        // Turns a card over: the swap happens edge-on. With reduced motion the swap is immediate.
        public static void Flip(Transform target,Action swap) { if(Skip(target)) { swap?.Invoke(); return; } On(target).Play(Motion.Kind.Flip,.26f,0,0,default,swap); }
        public static void Bob(Transform target,float amount,float period=1.6f) { if(!Skip(target)) On(target).Play(Motion.Kind.Bob,period,amount,0,default,null,true); }
        public static void Breathe(Transform target,float amount=.06f,float period=1.1f) { if(!Skip(target)) On(target).Play(Motion.Kind.Breathe,period,amount,0,default,null,true); }
        public static void Calm(Transform target) { if(!target) return; var m=target.GetComponent<Motion>(); if(m) { m.Stop(Motion.Kind.Bob); m.Stop(Motion.Kind.Breathe); } }
        public static Coroutine Run(IEnumerator routine)=>Instance.StartCoroutine(routine);
        public static void After(float seconds,Action action) { Instance.StartCoroutine(Later(seconds,action)); }
        static IEnumerator Later(float seconds,Action action) { yield return new WaitForSecondsRealtime(seconds); action?.Invoke(); }

        // ---------- screen positions ----------
        public static Vector2 PointerScreen=>Pointer.current!=null ? Pointer.current.position.ReadValue() : new Vector2(Screen.width*.5f,Screen.height*.5f);
        public static Vector2 WorldToScreen(Vector3 world)=>Camera.main ? (Vector2)Camera.main.WorldToScreenPoint(world) : new Vector2(Screen.width*.5f,Screen.height*.5f);
        public static Vector2 UiToScreen(RectTransform rect)=>RectTransformUtility.WorldToScreenPoint(null,rect.position);
        Vector2 Local(Vector2 screen) { RectTransformUtility.ScreenPointToLocalPointInRectangle(layer,screen,null,out var p); return p; }

        // ---------- particles ----------
        static readonly Color[] Festive={Look.Honey,Look.Brick,Look.Sage,Look.Paper,Look.Good,Look.Sand};
        public static void Sparkle(Vector2 screen,int count=12)
        { if(!Reduced) Instance.Emit(screen,count,new[]{Color.white,Look.Honey,Look.Paper},260,-420,.55f,new Vector2(4,8)); }
        public static void Dust(Vector2 screen)
        { if(!Reduced) Instance.Emit(screen,7,new[]{Look.Sand,Look.Paper},110,60,.45f,new Vector2(4,8)); }
        public static void Confetti()
        {
            if(Reduced) return;
            for(int i=0;i<5;i++) Instance.Emit(new Vector2(Screen.width*(.1f+.2f*i),Screen.height*1.02f),14,Festive,300,-520,2.2f,new Vector2(6,10));
        }
        // One rain drop or snow flake, falling through the play area below the top bar.
        public static void Fall(bool snow)
        {
            if(Reduced) return;
            var j=Instance; int before=j.particles.Count;
            j.Emit(new Vector2(UnityEngine.Random.Range(0f,Screen.width),Screen.height*.86f),1,snow ? new[]{Color.white} : new[]{Look.Water},0,0,snow ? 3.2f : 1.1f,snow ? new Vector2(4,6) : new Vector2(2,2));
            if(j.particles.Count==before) return;
            var p=j.particles[j.particles.Count-1]; p.velocity=snow ? new Vector2(UnityEngine.Random.Range(-25f,25f),-170) : new Vector2(-40,-640); p.keep=true;
            if(!snow) p.rect.sizeDelta=p.size=new Vector2(2,10);   // a drop is one pixel wide and five long
        }
        // size: smallest and largest side of the squares, in interface units (two to a picture pixel).
        void Emit(Vector2 screen,int count,Color[] colors,float speed,float gravity,float life,Vector2 size)
        {
            var origin=Local(screen);
            for(int i=0;i<count && particles.Count<220;i++) {
                var p=pool.Count>0 ? pool.Pop() : Create();
                p.rect.gameObject.SetActive(true); p.image.color=colors[UnityEngine.Random.Range(0,colors.Length)];
                float s=Mathf.Max(Pixels.Ui,Pixels.SnapUi(UnityEngine.Random.Range(size.x,size.y))); p.size=new Vector2(s,s); p.rect.sizeDelta=p.size;
                p.at=origin; p.rect.anchoredPosition=Pixels.SnapUi(origin); p.keep=false;
                float angle=UnityEngine.Random.Range(0f,Mathf.PI*2); p.velocity=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*speed*UnityEngine.Random.Range(.35f,1f);
                p.gravity=gravity; p.life=life*UnityEngine.Random.Range(.7f,1f); p.age=0;
                particles.Add(p);
            }
        }
        Particle Create()
        {
            var go=new GameObject("Spark",typeof(RectTransform)); go.transform.SetParent(layer,false); go.transform.SetAsFirstSibling();
            var image=go.AddComponent<Image>(); image.raycastTarget=false; image.sprite=Look.Square;
            // Held by its corner, so a square of any whole size keeps all four sides on the pixel grid.
            var rect=(RectTransform)go.transform; rect.pivot=Vector2.zero;
            return new Particle { rect=rect, image=image };
        }
        void Update()
        {
            float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);
            for(int i=particles.Count-1;i>=0;i--) {
                var p=particles[i]; p.age+=dt;
                if(p.age>=p.life) { p.rect.gameObject.SetActive(false); pool.Push(p); particles.RemoveAt(i); continue; }
                p.velocity.y+=p.gravity*dt; if(p.gravity!=0) p.velocity*=1-1.2f*dt; p.at+=p.velocity*dt; p.rect.anchoredPosition=Pixels.SnapUi(p.at);
                // No fading: towards the end a square loses a pixel at a time.
                if(!p.keep) { float left=Mathf.Clamp01((p.life-p.age)/(p.life*.35f)); float s=Mathf.Max(Pixels.Ui,Pixels.SnapUi(p.size.x*left)); p.rect.sizeDelta=new Vector2(s,s); }
            }
        }

        // ---------- floating text ----------
        // Rises from a point, then (optionally) flies to a target such as the score, and reports arrival.
        public static void FloatText(string text,Vector2 screen,Color color,TMP_FontAsset font,Vector2? flyTo=null,Action arrived=null)
        {
            if(Reduced || !font) { arrived?.Invoke(); return; }
            Instance.StartCoroutine(Instance.Float(text,screen,color,font,flyTo,arrived));
        }
        IEnumerator Float(string text,Vector2 screen,Color color,TMP_FontAsset font,Vector2? flyTo,Action arrived)
        {
            var go=new GameObject("Floating text",typeof(RectTransform)); go.transform.SetParent(layer,false);
            var label=go.AddComponent<TextMeshProUGUI>(); label.font=font; label.text=text; label.fontSize=34; label.fontStyle=FontStyles.Bold; label.alignment=TextAlignmentOptions.Center;
            label.color=color; label.raycastTarget=false; label.outlineWidth=.22f; label.outlineColor=Look.Ink;
            var rect=(RectTransform)go.transform; rect.sizeDelta=new Vector2(320,60); var start=Local(screen);
            for(float t=0;t<1;t+=Time.unscaledDeltaTime/.5f) { rect.anchoredPosition=start+Vector2.up*(70*(1-(1-t)*(1-t))); rect.localScale=Vector3.one*Mathf.Min(1,t*4+.4f); yield return null; }
            var from=rect.anchoredPosition;
            if(flyTo.HasValue) {
                var to=Local(flyTo.Value);
                for(float t=0;t<1;t+=Time.unscaledDeltaTime/.32f) { rect.anchoredPosition=Vector2.Lerp(from,to,t*t); rect.localScale=Vector3.one*Mathf.Lerp(1,.45f,t); yield return null; }
            } else for(float t=0;t<1;t+=Time.unscaledDeltaTime/.3f) { label.alpha=1-t; yield return null; }
            Destroy(go); arrived?.Invoke();
        }

        // ---------- scene transitions ----------
        public static void Boot() { var j=Instance; j.cover.color=Look.Peach; j.cover.raycastTarget=true; j.StartCoroutine(j.Reveal()); }
        public static void ChangeScene(string sceneName)
        {
            var j=Instance; if(j.changingScene) return;
            j.changingScene=true; j.cover.raycastTarget=true; j.StartCoroutine(j.CoverThenLoad(sceneName));
        }
        IEnumerator CoverThenLoad(string sceneName)
        {
            Sound("whoosh");
            if(!Reduced) for(float t=0;t<1;t+=Time.unscaledDeltaTime/.22f) { cover.color=Look.Alpha(Look.Peach,t); yield return null; }
            cover.color=Look.Peach;
            SceneManager.LoadScene(sceneName,LoadSceneMode.Single);
        }
        void SceneLoaded(Scene scene,LoadSceneMode mode)
        {
            foreach(var p in particles) { p.rect.gameObject.SetActive(false); pool.Push(p); } particles.Clear();
            if(cover.color.a>0) { StopAllCoroutines(); foreach(Transform child in layer) if(child.name=="Floating text") Destroy(child.gameObject); StartCoroutine(Reveal()); }
        }
        IEnumerator Reveal()
        {
            // Two frames let the new scene build and restyle itself behind the cover.
            yield return null; yield return null;
            if(!Reduced) for(float t=0;t<1;t+=Time.unscaledDeltaTime/.3f) { cover.color=Look.Alpha(Look.Peach,1-t); yield return null; }
            cover.color=Look.Alpha(Look.Peach,0); cover.raycastTarget=false; changingScene=false;
        }

        // ---------- optional sound effects ----------
        // Drop clips named click, correct, wrong, flip, match, pickup, drop, heart, star, fanfare, whoosh
        // into Assets/Resources/Sfx and they play; with no clips this stays silent.
        public static void Sound(string id,float pitch=1)
        {
            var j=Instance; if(GameSession.Muted) return;
            if(!j.clips.TryGetValue(id,out var clip)) { clip=Resources.Load<AudioClip>("Sfx/"+id); j.clips[id]=clip; }
            if(!clip) return;
            j.audioSource.pitch=pitch; j.audioSource.PlayOneShot(clip);
        }
    }
}
