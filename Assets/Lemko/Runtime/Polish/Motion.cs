using System;
using System.Collections.Generic;
using UnityEngine;
namespace Lemko
{
    // Layered, transient motion for one transform, in the soft style of the painted pictures: things squash when they
    // land, stretch when they leap, overshoot a little and settle. Every effect is added on top of the resting
    // position, scale and turn, and game code may keep setting those while an effect plays. Presentation only.
    public sealed class Motion : MonoBehaviour
    {
        public enum Kind { Pop, Punch, Squash, Shake, Hop, Travel, Flip, Bob, Breathe, Drop, Idle, Wobble }
        sealed class Effect { public Kind kind; public float start, duration, amount, phase; public Vector3 from; public bool loop, midDone; public Action mid; }
        readonly List<Effect> effects=new List<Effect>();
        Vector3 restScale, restPosition, lastScale, lastPosition; Quaternion restRotation, lastRotation;
        bool captured;
        // Held (dragged) and hovered pieces lift and lean; both ease in and out instead of jumping.
        bool hovered, held; float lift, lean, leanVelocity; Vector3 previousRest; bool havePrevious;
        public void Play(Kind kind,float duration,float amount=0,float delay=0,Vector3 from=default,Action mid=null,bool loop=false)
        {
            if(loop) effects.RemoveAll(e=>e.kind==kind);
            // A new bounce of the same kind replaces the old one, so quick repeats never add up to a huge jump.
            else if(kind!=Kind.Flip) effects.RemoveAll(e=>e.kind==kind && Time.unscaledTime>=e.start);
            effects.Add(new Effect { kind=kind, start=Time.unscaledTime+delay, duration=Mathf.Max(.01f,duration), amount=amount, from=from, mid=mid, loop=loop,
                phase=loop && kind==Kind.Idle ? UnityEngine.Random.value : 0 });
            enabled=true;
            if(kind==Kind.Pop) LateUpdate(); // hide at once, so nothing shows before its turn
        }
        public void Stop(Kind kind) { effects.RemoveAll(e=>e.kind==kind); }
        public void Hover(bool on) { hovered=on; enabled=true; }
        public void Hold(bool on) { held=on; havePrevious=false; enabled=true; }
        void Capture()
        {
            if(!captured) { restScale=transform.localScale; restPosition=transform.localPosition; restRotation=transform.localRotation; captured=true; return; }
            if(transform.localScale!=lastScale) restScale=transform.localScale;
            if(transform.localPosition!=lastPosition) restPosition=transform.localPosition;
            if(transform.localRotation!=lastRotation) restRotation=transform.localRotation;
        }
        // A decaying wobble: starts at zero, swings out, and settles back by the end.
        static float Spring(float t,float swings=2.5f)=>Mathf.Sin(t*Mathf.PI*swings)*(1-t)*(1-t);
        static float BackOut(float t,float s=2.2f) { t-=1; return t*t*((s+1)*t+s)+1; }
        // A short squash on contact: wide and low, then back.
        static float Bump(float t,float at,float width)=>t<at || t>at+width ? 0 : Mathf.Sin((t-at)/width*Mathf.PI);
        void LateUpdate()
        {
            Capture();
            float dt=Mathf.Min(Time.unscaledDeltaTime,.05f), now=Time.unscaledTime;
            bool hidden=false; var offset=Vector3.zero; float sx=1, sy=1, turn=0;
            for(int i=effects.Count-1;i>=0;i--) {
                var e=effects[i]; float t=(now-e.start)/e.duration;
                if(t<0) { if(e.kind==Kind.Pop) hidden=true; continue; }
                if(t>=1 && !e.loop) { if(e.mid!=null && !e.midDone) { e.midDone=true; e.mid(); } effects.RemoveAt(i); continue; }
                float a=e.amount;
                switch(e.kind) {
                    // Grows out of nothing past its size, then settles with a jelly wobble.
                    case Kind.Pop: { float s=Mathf.Max(0,BackOut(Mathf.Min(1,t*1.6f))); float j=Spring(t,3)*.18f; sx*=s*(1+j); sy*=s*(1-j); break; }
                    // A quick swell that springs back.
                    case Kind.Punch: { float s=Spring(t,2)*Mathf.Clamp(a,0,1)*1.4f; sx*=1+s*.85f; sy*=1+s*1.15f; break; }
                    // Pressed flat, then springs up a little taller and settles.
                    case Kind.Squash: { float s=Spring(t,2.5f)*Mathf.Clamp(a,0,.6f)*1.3f; sx*=1+s; sy*=1-s; break; }
                    // A side to side shake that tilts as it goes.
                    case Kind.Shake: { float w=Mathf.Sin(t*Mathf.PI*7)*(1-t); offset.x+=a*w; turn+=w*5; break; }
                    // Stretches as it leaps, squashes as it lands.
                    case Kind.Hop: {
                        const float air=.72f;
                        if(t<air) { float u=t/air; offset.y+=a*4*u*(1-u); float v=Mathf.Abs(1-2*u); sy*=1+.14f*v*(1-u*.3f); sx*=1-.09f*v; if(u<.12f) { float k=1-u/.12f; sy*=1-.16f*k; sx*=1+.14f*k; } }
                        else { float w=Mathf.Sin((t-air)/(1-air)*Mathf.PI); sy*=1-.16f*w; sx*=1+.14f*w; }
                        break; }
                    case Kind.Travel: {
                        float k=1-Mathf.Pow(1-t,3); var d=(e.from-restPosition)*(1-k)+Vector3.up*(a*Mathf.Sin(t*Mathf.PI));
                        offset+=d; float speed=3*(1-t)*(1-t); float dx=(restPosition.x-e.from.x)*speed;
                        turn+=Mathf.Clamp(-dx*12,-14,14); sx*=1+.08f*speed; sy*=1-.06f*speed;
                        if(t>.8f) { float w=Mathf.Sin((t-.8f)/.2f*Mathf.PI); sx*=1+.1f*w; sy*=1-.1f*w; }
                        break; }
                    // A card turns over: it narrows to its edge, the faces swap there, and it opens again with a little jump.
                    case Kind.Flip:
                        if(t>=.5f && !e.midDone) { e.midDone=true; e.mid?.Invoke(); Capture(); }
                        sx*=Mathf.Max(.02f,Mathf.Abs(Mathf.Cos(t*Mathf.PI))); sy*=1+.08f*Mathf.Sin(t*Mathf.PI); offset.y+=Mathf.Sin(t*Mathf.PI)*Size().y*.12f; break;
                    case Kind.Bob: offset.y+=a*Mathf.Sin(t*Mathf.PI*2); turn+=1.6f*Mathf.Sin(t*Mathf.PI*2+1); break;
                    // A slow breath: a little taller and narrower, then back.
                    case Kind.Breathe: { float b=.5f-.5f*Mathf.Cos(t*Mathf.PI*2); sy*=1+a*b; sx*=1+a*.4f*(1-b); break; }
                    // Falls in, bounces twice and squashes on every landing.
                    case Kind.Drop: {
                        offset.y+=a*(1-BounceOut(t));
                        float squash=.2f*Bump(t,.364f,.12f)+.1f*Bump(t,.727f,.09f)+.05f*Bump(t,.909f,.07f);
                        sx*=1+squash; sy*=1-squash; break; }
                    // Never quite still: a slow, barely-there sway that differs from piece to piece.
                    case Kind.Idle: { float p=(t+e.phase)*Mathf.PI*2; sy*=1+.016f*Mathf.Sin(p); sx*=1-.01f*Mathf.Sin(p); turn+=a*Mathf.Sin(p*.5f+e.phase*6); break; }
                    // A tilt back and forth that dies away, for a gentle "no".
                    case Kind.Wobble: turn+=a*Spring(t,3); break;
                }
            }
            // Lift and lean: a held piece rises, grows a little and leans into the direction it is pulled.
            float target=held ? 1 : hovered ? .45f : 0;
            lift=Mathf.MoveTowards(lift,target,dt*(target>lift ? 7 : 5));
            float leanTarget=0;
            if(held) {
                if(havePrevious && dt>0) leanTarget=Mathf.Clamp(-(restPosition.x-previousRest.x)/dt*(transform is RectTransform ? .02f : 2.2f),-16,16);
                previousRest=restPosition; havePrevious=true;
            }
            // A spring, so a piece swings a little when it stops.
            leanVelocity+=((leanTarget-lean)*180-leanVelocity*16)*dt; lean+=leanVelocity*dt;
            float grow=1+.1f*Smooth(lift); sx*=grow; sy*=grow; turn+=lean;
            transform.localScale=lastScale=hidden ? Vector3.zero : new Vector3(restScale.x*sx,restScale.y*sy,restScale.z);
            transform.localPosition=lastPosition=restPosition+offset;
            transform.localRotation=lastRotation=restRotation*Quaternion.Euler(0,0,turn);
            bool settling=held || hovered || lift>.001f || Mathf.Abs(lean)>.05f || Mathf.Abs(leanVelocity)>.05f;
            if(effects.Count==0 && !settling) { lift=lean=leanVelocity=0; Rest(); captured=false; enabled=false; }
        }
        static float Smooth(float t)=>t*t*(3-2*t);
        void Rest() { transform.localScale=restScale; transform.localPosition=restPosition; transform.localRotation=restRotation; }
        // The size of what this moves, for effects measured against it.
        Vector2 Size()
        {
            if(transform is RectTransform rect) return rect.rect.size;
            var r=GetComponentInChildren<Renderer>(); return r ? (Vector2)r.bounds.size : Vector2.one;
        }
        void OnDisable()
        {
            // Finish cleanly: pending flips still turn their card, and the transform returns to rest.
            foreach(var e in effects) if(e.mid!=null && !e.midDone) { e.midDone=true; e.mid(); }
            effects.Clear(); hovered=held=false; lift=lean=leanVelocity=0;
            if(captured) { Rest(); captured=false; }
        }
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
