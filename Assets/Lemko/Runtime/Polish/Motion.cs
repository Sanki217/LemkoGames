using System;
using System.Collections.Generic;
using UnityEngine;
namespace Lemko
{
    // Layered, transient motion for one transform. Effects multiply the resting scale and add to the resting
    // position, so game code may keep setting scale and position while an effect plays. Presentation only.
    public sealed class Motion : MonoBehaviour
    {
        public enum Kind { Pop, Punch, Squash, Shake, Hop, Travel, Flip, Bob, Breathe, Drop }
        sealed class Effect { public Kind kind; public float start, duration, amount; public Vector3 from; public bool loop, midDone; public Action mid; }
        readonly List<Effect> effects=new List<Effect>();
        Vector3 restScale, restPosition, lastScale, lastPosition;
        bool captured;
        public void Play(Kind kind,float duration,float amount=0,float delay=0,Vector3 from=default,Action mid=null,bool loop=false)
        {
            if(loop) effects.RemoveAll(e=>e.kind==kind);
            effects.Add(new Effect { kind=kind, start=Time.unscaledTime+delay, duration=Mathf.Max(.01f,duration), amount=amount, from=from, mid=mid, loop=loop });
            enabled=true;
            if(kind==Kind.Pop) LateUpdate(); // hide at once, so nothing shows at full size before its turn
        }
        public void Stop(Kind kind) { effects.RemoveAll(e=>e.kind==kind); }
        void Capture()
        {
            if(!captured) { restScale=transform.localScale; restPosition=transform.localPosition; captured=true; return; }
            if(transform.localScale!=lastScale) restScale=transform.localScale;
            if(transform.localPosition!=lastPosition) restPosition=transform.localPosition;
        }
        void LateUpdate()
        {
            Capture();
            var scale=Vector3.one; var offset=Vector3.zero; float now=Time.unscaledTime;
            for(int i=effects.Count-1;i>=0;i--) {
                var e=effects[i]; float t=(now-e.start)/e.duration;
                if(t<0) { if(e.kind==Kind.Pop) scale=Vector3.zero; continue; }
                if(t>=1 && !e.loop) { if(e.mid!=null && !e.midDone) { e.midDone=true; e.mid(); } effects.RemoveAt(i); continue; }
                switch(e.kind) {
                    case Kind.Pop: scale*=BackOut(t); break;
                    case Kind.Punch: scale*=1+e.amount*Mathf.Sin(t*Mathf.PI); break;
                    case Kind.Squash: { float s=e.amount*Mathf.Sin(t*Mathf.PI*2)*(1-t); scale=Vector3.Scale(scale,new Vector3(1+s,1-s,1)); break; }
                    case Kind.Shake: offset.x+=e.amount*Mathf.Sin(t*Mathf.PI*6)*(1-t); break;
                    case Kind.Hop: offset.y+=e.amount*4*t*(1-t); break;
                    case Kind.Travel: { float k=1-Mathf.Pow(1-t,3); offset+=(e.from-restPosition)*(1-k)+Vector3.up*(e.amount*Mathf.Sin(t*Mathf.PI)); scale*=1+.09f*Mathf.Sin(t*Mathf.PI); break; }
                    case Kind.Flip:
                        if(t>=.5f && !e.midDone) { e.midDone=true; e.mid?.Invoke(); Capture(); }
                        scale.x*=Mathf.Max(.02f,Mathf.Abs(Mathf.Cos(t*Mathf.PI))); break;
                    case Kind.Bob: offset.y+=e.amount*Mathf.Sin(t*Mathf.PI*2); break;
                    case Kind.Breathe: scale*=1+e.amount*(.5f+.5f*Mathf.Sin(t*Mathf.PI*2)); break;
                    case Kind.Drop: offset.y+=e.amount*(1-BounceOut(t)); break;
                }
            }
            transform.localScale=lastScale=Vector3.Scale(restScale,scale);
            transform.localPosition=lastPosition=restPosition+offset;
            if(effects.Count==0) { captured=false; enabled=false; }
        }
        void OnDisable()
        {
            // Finish cleanly: pending flips still turn their card, and the transform returns to rest.
            foreach(var e in effects) if(e.mid!=null && !e.midDone) { e.midDone=true; e.mid(); }
            effects.Clear();
            if(captured) { transform.localScale=restScale; transform.localPosition=restPosition; captured=false; }
        }
        static float BackOut(float t) { const float c=1.70158f; t-=1; return 1+(c+1)*t*t*t+c*t*t; }
        static float BounceOut(float t)
        {
            const float n=7.5625f, d=2.75f;
            if(t<1/d) return n*t*t;
            if(t<2/d) { t-=1.5f/d; return n*t*t+.75f; }
            if(t<2.5f/d) { t-=2.25f/d; return n*t*t+.9375f; }
            t-=2.625f/d; return n*t*t+.984375f;
        }
    }
}
