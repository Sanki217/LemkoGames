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
        // A particle is a small soft shape (a glow, a twinkle, a slip of confetti) that drifts, turns and fades.
        enum Shape { Glow, Twinkle, Confetto, Streak, Ring, Disc }
        sealed class Particle { public RectTransform rect; public Image image; public Vector2 velocity, at, size; public float gravity, life, age, spin, sway, alpha, grow; public Shape shape; public bool keep; }
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
        // A slow, barely visible sway that keeps a piece alive while it waits; turn is its largest tilt in degrees.
        public static void Idle(Transform target,float turn=1.2f) { if(!Skip(target)) On(target).Play(Motion.Kind.Idle,UnityEngine.Random.Range(2.4f,3.4f),turn,0,default,null,true); }
        // A tilt back and forth that dies away.
        public static void Wobble(Transform target,float degrees=10) { if(!Skip(target)) On(target).Play(Motion.Kind.Wobble,.5f,degrees); }
        // Hovered pieces rise a little; held ones rise more and lean into the pull.
        public static void Hover(Transform target,bool on) { if(target && (!on || !Skip(target))) { var m=on ? On(target) : target.GetComponent<Motion>(); if(m) m.Hover(on); } }
        public static void Hold(Transform target,bool on) { if(target && (!on || !Skip(target))) { var m=on ? On(target) : target.GetComponent<Motion>(); if(m) m.Hold(on); } }
        public static void Calm(Transform target) { if(!target) return; var m=target.GetComponent<Motion>(); if(m) { m.Stop(Motion.Kind.Bob); m.Stop(Motion.Kind.Breathe); m.Stop(Motion.Kind.Idle); } }
        public static Coroutine Run(IEnumerator routine)=>Instance.StartCoroutine(routine);
        public static void After(float seconds,Action action) { Instance.StartCoroutine(Later(seconds,action)); }
        static IEnumerator Later(float seconds,Action action) { yield return new WaitForSecondsRealtime(seconds); action?.Invoke(); }

        // ---------- screen positions ----------
        public static Vector2 PointerScreen=>Pointer.current!=null ? Pointer.current.position.ReadValue() : new Vector2(Screen.width*.5f,Screen.height*.5f);
        public static Vector2 WorldToScreen(Vector3 world)=>Camera.main ? (Vector2)Camera.main.WorldToScreenPoint(world) : new Vector2(Screen.width*.5f,Screen.height*.5f);
        public static Vector2 UiToScreen(RectTransform rect)=>RectTransformUtility.WorldToScreenPoint(null,rect.position);
        Vector2 Local(Vector2 screen) { RectTransformUtility.ScreenPointToLocalPointInRectangle(layer,screen,null,out var p); return p; }

        // ---------- particles ----------
        static readonly Color[] Festive={Look.Honey,Look.Brick,Look.Sage,Look.Paper,Look.Good,Look.Sand,Look.Water};
        static readonly Color Warm=new Color(1,.97f,.86f);
        // A burst of twinkles and soft glows where something went right.
        public static void Sparkle(Vector2 screen,int count=12)
        {
            if(Reduced) return; var j=Instance;
            j.Emit(screen,Mathf.CeilToInt(count*.6f),new[]{Color.white,Look.Honey,Warm},Shape.Twinkle,300,-260,.7f,new Vector2(16,30),spin:220);
            j.Emit(screen,Mathf.CeilToInt(count*.5f),new[]{Look.Alpha(Look.Honey,.8f),Look.Alpha(Warm,.8f)},Shape.Glow,180,-120,.55f,new Vector2(14,26));
            Ring(screen,Look.Alpha(Warm,.9f));
        }
        // A thin ring that widens and fades from a point.
        public static void Ring(Vector2 screen,Color color,float size=150)
        {
            if(Reduced) return; var j=Instance; int before=j.particles.Count;
            j.Emit(screen,1,new[]{color},Shape.Ring,0,0,.45f,new Vector2(24,24));
            if(j.particles.Count>before) { var p=j.particles[j.particles.Count-1]; p.life=.45f; p.grow=size; p.alpha=color.a; }
        }
        // Little puffs: where something lands or is lost.
        public static void Dust(Vector2 screen)
        { if(!Reduced) Instance.Emit(screen,8,new[]{Look.Alpha(Look.Sand,.8f),Look.Alpha(Look.Paper,.8f)},Shape.Glow,130,50,.55f,new Vector2(16,30)); }
        public static void Confetti()
        {
            if(Reduced) return;
            for(int i=0;i<6;i++) Instance.Emit(new Vector2(Screen.width*(.08f+.168f*i),Screen.height*1.03f),14,Festive,Shape.Confetto,320,-480,2.6f,new Vector2(12,20),spin:400,sway:1);
        }
        // One rain drop or snow flake, falling through the play area below the top bar.
        public static void Fall(bool snow)
        {
            if(Reduced) return;
            var j=Instance; int before=j.particles.Count;
            j.Emit(new Vector2(UnityEngine.Random.Range(0f,Screen.width*1.1f),Screen.height*.86f),1,snow ? new[]{Look.Alpha(Color.white,.95f)} : new[]{Look.Alpha(Look.Water,.7f)},snow ? Shape.Glow : Shape.Streak,0,0,snow ? 3.4f : 1.1f,snow ? new Vector2(12,22) : new Vector2(8,8),sway:snow ? 1 : 0);
            if(j.particles.Count==before) return;
            var p=j.particles[j.particles.Count-1]; p.velocity=snow ? new Vector2(UnityEngine.Random.Range(-25f,25f),-150) : new Vector2(-60,-680); p.keep=true;
            if(!snow) { p.rect.sizeDelta=p.size=new Vector2(6,30); p.rect.localRotation=Quaternion.Euler(0,0,-5); }
        }
        // Soft specks of light that drift slowly up through every scene, so the picture is never quite still.
        float nextMote;
        static bool motes;
        public static void Motes(bool on) { motes=on; if(on) Instance.nextMote=0; }
        void Mote()
        {
            int before=particles.Count;
            Emit(new Vector2(UnityEngine.Random.Range(0f,Screen.width),UnityEngine.Random.Range(-.02f,.7f)*Screen.height),1,new[]{Warm,Look.Honey,Color.white},Shape.Glow,0,0,UnityEngine.Random.Range(6f,9f),new Vector2(8,16),sway:1);
            if(particles.Count==before) return;
            var p=particles[particles.Count-1]; p.velocity=new Vector2(UnityEngine.Random.Range(4f,16f),UnityEngine.Random.Range(10f,24f)); p.alpha=UnityEngine.Random.Range(.25f,.5f); p.keep=true; p.image.color=Look.Alpha(p.image.color,0);
        }
        // size: smallest and largest side, in interface units.
        void Emit(Vector2 screen,int count,Color[] colors,Shape shape,float speed,float gravity,float life,Vector2 size,float spin=0,float sway=0)
        {
            var origin=Local(screen);
            for(int i=0;i<count && particles.Count<240;i++) {
                var p=pool.Count>0 ? pool.Pop() : Create();
                p.rect.gameObject.SetActive(true); var color=colors[UnityEngine.Random.Range(0,colors.Length)]; p.image.color=color; p.alpha=color.a;
                p.shape=shape; p.image.sprite=shape==Shape.Twinkle ? Look.Twinkle : shape==Shape.Confetto ? Look.Confetto : shape==Shape.Streak ? Look.Streak : shape==Shape.Ring ? Look.Ring : shape==Shape.Disc ? Look.Soft : Look.Glow;
                float s=UnityEngine.Random.Range(size.x,size.y); p.size=shape==Shape.Confetto ? new Vector2(s*.6f,s) : new Vector2(s,s); p.rect.sizeDelta=p.size;
                p.at=origin; p.rect.anchoredPosition=origin; p.keep=false; p.grow=0;
                float angle=UnityEngine.Random.Range(0f,Mathf.PI*2); p.velocity=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*speed*UnityEngine.Random.Range(.35f,1f);
                p.gravity=gravity; p.life=life*UnityEngine.Random.Range(.7f,1f); p.age=0;
                p.spin=spin*UnityEngine.Random.Range(-1f,1f); p.sway=sway*UnityEngine.Random.Range(.6f,1.4f); p.rect.localRotation=Quaternion.Euler(0,0,UnityEngine.Random.Range(0f,360f)*(spin!=0 ? 1 : 0)); p.rect.localScale=Vector3.one;
                particles.Add(p);
            }
        }
        Particle Create()
        {
            var go=new GameObject("Spark",typeof(RectTransform)); go.transform.SetParent(layer,false); go.transform.SetAsFirstSibling();
            var image=go.AddComponent<Image>(); image.raycastTarget=false; image.sprite=Look.Glow;
            return new Particle { rect=(RectTransform)go.transform, image=image };
        }
        void Update()
        {
            float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);
            if(motes && !Reduced && !changingScene && (nextMote-=dt)<=0) { nextMote=UnityEngine.Random.Range(.35f,.7f); int alive=0; foreach(var q in particles) if(q.keep && q.shape==Shape.Glow && q.gravity==0 && q.velocity.y>0) alive++; if(alive<16) Mote(); }
            for(int i=particles.Count-1;i>=0;i--) {
                var p=particles[i]; p.age+=dt;
                if(p.age>=p.life) { p.rect.gameObject.SetActive(false); pool.Push(p); particles.RemoveAt(i); continue; }
                float t=p.age/p.life;
                p.velocity.y+=p.gravity*dt; if(p.gravity!=0) p.velocity*=1-(p.shape==Shape.Confetto ? 1.6f : 1.2f)*dt; p.at+=p.velocity*dt;
                var at=p.at; if(p.sway!=0) at.x+=Mathf.Sin(p.age*2.2f+i)*14*p.sway;
                p.rect.anchoredPosition=at;
                if(p.spin!=0) p.rect.localRotation=Quaternion.Euler(0,0,p.rect.localEulerAngles.z+p.spin*dt);
                var c=p.image.color;
                switch(p.shape) {
                    case Shape.Ring: { float k=1-(1-t)*(1-t); p.rect.sizeDelta=Vector2.one*Mathf.Lerp(p.size.x,p.grow,k); c.a=p.alpha*(1-t); break; }
                    // Confetti flutters: it turns over and over, showing its edge, as it drifts down.
                    case Shape.Confetto: p.rect.localScale=new Vector3(Mathf.Cos(p.age*9+i),1,1); c.a=p.alpha*Mathf.Clamp01((1-t)*4); break;
                    // Twinkles swell in quickly, then shrink away.
                    case Shape.Twinkle: { float s=t<.2f ? t/.2f : 1-(t-.2f)/.8f; p.rect.localScale=Vector3.one*Mathf.Max(0,s); c.a=p.alpha; break; }
                    default:
                        // Motes and flakes fade in and out; puffs grow a little as they fade.
                        if(p.keep) c.a=p.alpha*Mathf.Clamp01(Mathf.Min(t*5,(1-t)*5));
                        else { c.a=p.alpha*(1-t)*(1-t); p.rect.localScale=Vector3.one*(1+t*.6f); }
                        break;
                }
                p.image.color=c;
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
            for(float t=0;t<1;t+=Time.unscaledDeltaTime/.55f) {
                rect.anchoredPosition=start+Vector2.up*(70*(1-(1-t)*(1-t)));
                // Bursts out larger than its size, then settles with a little squash.
                float u=Mathf.Min(1,t*2.2f)-1, s=u*u*(3.2f*u+2.2f)+1, j=Mathf.Sin(t*Mathf.PI*3)*(1-t)*.12f;
                rect.localScale=new Vector3(s*(1+j),s*(1-j),1); yield return null; }
            var from=rect.anchoredPosition;
            if(flyTo.HasValue) {
                var to=Local(flyTo.Value);
                for(float t=0;t<1;t+=Time.unscaledDeltaTime/.32f) { rect.anchoredPosition=Vector2.Lerp(from,to,t*t)+Vector2.up*Mathf.Sin(t*Mathf.PI)*40; rect.localScale=new Vector3(Mathf.Lerp(1,.45f,t)*(1+t*.2f),Mathf.Lerp(1,.45f,t)*(1-t*.15f),1); yield return null; }
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
            if(!Reduced) for(float t=0;t<1;t+=Time.unscaledDeltaTime/.25f) { cover.color=Look.Alpha(Look.Peach,t*t*(3-2*t)); yield return null; }
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
            if(!Reduced) for(float t=0;t<1;t+=Time.unscaledDeltaTime/.35f) { cover.color=Look.Alpha(Look.Peach,1-t*t*(3-2*t)); yield return null; }
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
