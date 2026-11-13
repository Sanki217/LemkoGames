using System.Collections.Generic;
using UnityEngine;
namespace Lemko
{
    // One palette, shared with the illustrations, plus the few interface shapes the game needs.
    // Shapes are drawn in code so no scene or texture has to be edited by hand.
    public static class Look
    {
        // Every colour here comes from the game's 31-colour palette; Tools/InkArt draws every picture with the same one.
        public static readonly Color Peach=Hex(0xF6E4B0), Paper=Hex(0xFBF5D8), Ink=Hex(0x3A160D), Mint=Hex(0xE0ECC6), Sand=Hex(0xE6C58F),
            Sage=Hex(0xBFD6A0), Pine=Hex(0x4A7A6C), Brick=Hex(0xD5583A), Honey=Hex(0xF2C85F), Good=Hex(0x628745), Retry=Hex(0xE68F4B),
            GoodSoft=Hex(0xCDCB5D), Muted=Hex(0x66747A), Empty=Hex(0xE6C58F), Grass=Hex(0xA6C47E), Water=Hex(0x5B87A6), Wood=Hex(0x8A4B2C);
        public static Color Hex(int rgb,float a=1)=>new Color(((rgb>>16)&255)/255f,((rgb>>8)&255)/255f,(rgb&255)/255f,a);
        public static Color Alpha(Color c,float a) { c.a=a; return c; }
        // The Lemko word is the star of every prompt.
        public static string Word(string lemko)=>"<size=150%><b>"+lemko+"</b></size>";

        static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset()=>cache.Clear();
        delegate Color Paint(float x,float y);
        // Test hook: lets a tool outside Unity look at the generated pixels.
        public static System.Action<string,int,int,Color[]> Dump;
        static Sprite Make(string key,int w,int h,Paint paint,Vector4 border=default,float pixelsPerUnit=100,bool repeat=false)
        {
            if(cache.TryGetValue(key,out var existing) && existing) return existing;
            var texture=new Texture2D(w,h,TextureFormat.RGBA32,false) { name=key, filterMode=FilterMode.Bilinear, wrapMode=repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp, hideFlags=HideFlags.HideAndDontSave };
            var pixels=new Color[w*h];
            for(int y=0;y<h;y++) for(int x=0;x<w;x++) pixels[y*w+x]=paint(x+.5f,y+.5f);
            Dump?.Invoke(key,w,h,pixels);
            texture.SetPixels(pixels); texture.Apply(false,true);
            var sprite=Sprite.Create(texture,new Rect(0,0,w,h),new Vector2(.5f,.5f),pixelsPerUnit,0,SpriteMeshType.FullRect,border);
            sprite.name=key; sprite.hideFlags=HideFlags.HideAndDontSave; cache[key]=sprite; return sprite;
        }
        static float Cover(float distance)=>Mathf.Clamp01(.5f-distance);
        static float RoundBox(float x,float y,float w,float h,float r)
        {
            float qx=Mathf.Abs(x-w*.5f)-(w*.5f-r), qy=Mathf.Abs(y-h*.5f)-(h*.5f-r);
            return new Vector2(Mathf.Max(qx,0),Mathf.Max(qy,0)).magnitude+Mathf.Min(Mathf.Max(qx,qy),0)-r;
        }
        static Color Over(Color under,Color over,float a) { var c=Color.Lerp(under,over,a); c.a=Mathf.Max(under.a,a*over.a); return c; }
        // White fill with an ink outline and a slightly darker lower lip; tint it with any palette colour.
        static Sprite Card(string key,float lip)
            => Make(key,64,64,(x,y)=> {
                float d=RoundBox(x,y,64,64,16); float inside=Cover(d);
                if(inside<=0) return Color.clear;
                var fill=y<lip+3 && lip>0 ? new Color(.8f,.8f,.8f,1) : Color.white;
                var c=Color.Lerp(fill,new Color(.227f,.086f,.051f,1),1-Cover(d+4.5f));
                c.a=inside; return c;
            },new Vector4(22,22,22,22));
        public static Sprite Panel=>Card("lemko_panel",0);
        public static Sprite Button=>Card("lemko_button",7);
        public static Sprite Pill=>Make("lemko_pill",64,64,(x,y)=> { float a=Cover(RoundBox(x,y,64,64,31)); return new Color(1,1,1,a); },new Vector4(31,31,31,31));
        public static Sprite Dot=>Make("lemko_dot",48,48,(x,y)=> {
            float d=Vector2.Distance(new Vector2(x,y),new Vector2(24,24))-22;
            var c=Color.Lerp(Color.white,new Color(.227f,.086f,.051f,1),1-Cover(d+4.5f)); c.a=Cover(d); return c; });
        public static Sprite Soft=>Make("lemko_soft",64,64,(x,y)=> { float d=Vector2.Distance(new Vector2(x,y),new Vector2(32,32))/32; return new Color(1,1,1,Mathf.Clamp01(1-d)*Mathf.Clamp01(1-d)); });
        public static Sprite Square=>Make("lemko_square",8,8,(x,y)=>Color.white);
        public static Sprite Diamond=>Make("lemko_diamond",32,32,(x,y)=>new Color(1,1,1,Cover(Mathf.Abs(x-16)+Mathf.Abs(y-16)-14)));
        public static Sprite Heart=>Make("lemko_heart",64,64,(x,y)=> {
            float d=HeartDistance(Mathf.Abs(x-32)/50f,(y-7)/50f)*50f;
            var c=Color.Lerp(Color.white,new Color(.227f,.086f,.051f,1),1-Cover(d+4.5f)); c.a=Cover(d); return c; });
        // Signed distance to a heart whose tip is at the origin and whose top is at y = 1 (x already mirrored).
        static float HeartDistance(float x,float y)
        {
            if(x+y>1) return new Vector2(x-.25f,y-.75f).magnitude-Mathf.Sqrt(2)/4;
            float m=.5f*Mathf.Max(x+y,0);
            float a=new Vector2(x,y-1).sqrMagnitude, b=new Vector2(x-m,y-m).sqrMagnitude;
            return Mathf.Sqrt(Mathf.Min(a,b))*Mathf.Sign(x-y);
        }
        public static Sprite Star=>Make("lemko_star",64,64,(x,y)=> {
            float d=StarDistance(x-32,y-31,27,12);
            var c=Color.Lerp(Color.white,new Color(.227f,.086f,.051f,1),1-Cover(d+4.5f)); c.a=Cover(d); return c; });
        static float StarDistance(float x,float y,float outer,float inner)
        {
            // Signed distance to a five-point star, evaluated against its ten edges.
            float best=float.MaxValue; bool inside=false; var p=new Vector2(x,y);
            for(int i=0;i<10;i++) {
                var a=Corner(i,outer,inner); var b=Corner(i+1,outer,inner);
                var ab=b-a; float t=Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude);
                best=Mathf.Min(best,(p-(a+ab*t)).magnitude);
                if((a.y>y)!=(b.y>y) && x<(b.x-a.x)*(y-a.y)/(b.y-a.y)+a.x) inside=!inside;
            }
            return inside ? -best : best;
        }
        static Vector2 Corner(int i,float outer,float inner) { float angle=Mathf.PI/2+i*Mathf.PI/5; float r=i%2==0 ? outer : inner; return new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r; }
        // The scalloped awning from the Shop backdrop, as a strip that tiles along the top bar.
        public static Sprite Awning=>Make("lemko_awning",96,28,(x,y)=> {
            bool second=x>=48; float cx=second ? 72 : 24;
            float d=y>=24 ? Mathf.Abs(x-cx)-24 : Vector2.Distance(new Vector2(x,y),new Vector2(cx,24))-24;
            var fill=second ? Paper : Brick;
            var c=Color.Lerp(fill,Ink,1-Cover(d+3.5f)); c.a=Cover(d); return c;
        },default,100,true);
        public static Sprite Lantern=>Make("lemko_lantern",48,64,(x,y)=> {
            float body=RoundBox(x-8,y-6,32,42,12); float cap=RoundBox(x-16,y-46,16,8,2); float d=Mathf.Min(body,cap);
            var c=Color.Lerp(Color.white,new Color(.227f,.086f,.051f,1),1-Cover(d+4.5f)); c.a=Cover(d); return c; });
        public static Sprite Cloud=>Make("lemko_cloud",128,64,(x,y)=> {
            float d=Mathf.Min(Vector2.Distance(new Vector2(x,y),new Vector2(42,26))-22,Mathf.Min(Vector2.Distance(new Vector2(x,y),new Vector2(70,34))-27,Vector2.Distance(new Vector2(x,y),new Vector2(96,24))-19));
            d=Mathf.Min(d,RoundBox(x-22,y-6,84,22,10));
            return new Color(1,1,1,Cover(d)); });
        // A round paper badge with a little loudspeaker: the "hear this word" button on every card.
        public static Sprite Speaker=>Make("lemko_speaker",64,64,(x,y)=> {
            float d=Vector2.Distance(new Vector2(x,y),new Vector2(32,32))-30;
            var c=Color.Lerp(Paper,Ink,1-Cover(d+4.5f));
            float box=RoundBox(x-15,y-25,11,14,2);
            float t=Mathf.Clamp01((x-24)/11f); float cone=Mathf.Max(Mathf.Abs(y-32)-(7+t*8),Mathf.Max(24-x,x-35));
            var p=new Vector2(x-34,y-32); float r=p.magnitude; bool front=x>39 && Mathf.Abs(p.y)<p.x*1.15f;
            float wave=front ? Mathf.Min(Mathf.Abs(r-11),Mathf.Abs(r-18))-1.6f : 9;
            float glyph=Mathf.Min(Mathf.Min(box,cone),wave);
            c=Color.Lerp(c,Ink,Cover(glyph)); c.a=Cover(d); return c; });
        public static Sprite Bulb=>Make("lemko_bulb",48,48,(x,y)=> {
            float glass=Vector2.Distance(new Vector2(x,y),new Vector2(24,29))-13; float neck=RoundBox(x-18,y-6,12,12,2); float d=Mathf.Min(glass,neck);
            var c=Color.Lerp(Color.white,new Color(.227f,.086f,.051f,1),1-Cover(d+2.5f)); c.a=Cover(d); return c; });
    }
}
