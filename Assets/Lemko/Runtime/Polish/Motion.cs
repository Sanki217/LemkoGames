using System;
using System.Collections.Generic;
using UnityEngine;
namespace Lemko
{
    // Layered, transient motion for one transform. The game is pixel art, so nothing is ever stretched: every effect
    // is a small move in whole picture pixels (a hop, a nudge, a shake) added to the resting position, and game code
    // may keep setting the position while an effect plays. Presentation only.
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
            if(kind==Kind.Pop) LateUpdate(); // hide at once, so nothing shows before its turn
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
            // One picture pixel: a fortieth of a unit in the world, two units in the interface.
            float px=transform is RectTransform ? Pixels.Ui : Pixels.Unit;
            bool hidden=false; var offset=Vector3.zero; float now=Time.unscaledTime;
            for(int i=effects.Count-1;i>=0;i--) {
                var e=effects[i]; float t=(now-e.start)/e.duration;
                if(t<0) { if(e.kind==Kind.Pop) hidden=true; continue; }
                if(t>=1 && !e.loop) { if(e.mid!=null && !e.midDone) { e.midDone=true; e.mid(); } effects.RemoveAt(i); continue; }
                switch(e.kind) {
                    // Appears a few pixels up and settles into place.
                    case Kind.Pop: offset.y+=px*Mathf.Round(4*(1-t)*(1-t)); break;
                    // A quick nod upwards: one to four pixels, by strength.
                    case Kind.Punch: offset.y+=px*Mathf.Round(Mathf.Clamp(e.amount*12,1,4)*Mathf.Sin(t*Mathf.PI)); break;
                    // Pressed down by a pixel.
                    case Kind.Squash: offset.y-=px*Mathf.Round(Mathf.Sin(t*Mathf.PI)); break;
                    case Kind.Shake: offset.x+=Whole(e.amount*Mathf.Sin(t*Mathf.PI*6)*(1-t),px); break;
                    case Kind.Hop: offset.y+=Whole(e.amount*4*t*(1-t),px); break;
                    case Kind.Travel: {
                        float k=1-Mathf.Pow(1-t,3); var d=(e.from-restPosition)*(1-k)+Vector3.up*(e.amount*Mathf.Sin(t*Mathf.PI));
                        offset+=new Vector3(Whole(d.x,px),Whole(d.y,px),0); break; }
                    // A card turns over with a little jump; the faces swap at the top of it.
                    case Kind.Flip:
                        if(t>=.5f && !e.midDone) { e.midDone=true; e.mid?.Invoke(); Capture(); }
                        offset.y+=px*Mathf.Round(3*Mathf.Sin(t*Mathf.PI)); break;
                    case Kind.Bob: offset.y+=Whole(e.amount*Mathf.Sin(t*Mathf.PI*2),px); break;
                    // A slow one pixel rise and fall.
                    case Kind.Breathe: offset.y+=px*Mathf.Round(.5f+.5f*Mathf.Sin(t*Mathf.PI*2)); break;
                    case Kind.Drop: offset.y+=Whole(e.amount*(1-BounceOut(t)),px); break;
                }
            }
            transform.localScale=lastScale=hidden ? Vector3.zero : restScale;
            transform.localPosition=lastPosition=restPosition+offset;
            if(effects.Count==0) { captured=false; enabled=false; }
        }
        static float Whole(float distance,float pixel)=>Mathf.Round(distance/pixel)*pixel;
        void OnDisable()
        {
            // Finish cleanly: pending flips still turn their card, and the transform returns to rest.
            foreach(var e in effects) if(e.mid!=null && !e.midDone) { e.midDone=true; e.mid(); }
            effects.Clear();
            if(captured) { transform.localScale=restScale; transform.localPosition=restPosition; captured=false; }
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
