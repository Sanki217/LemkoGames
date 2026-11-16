using System.Collections.Generic;
using UnityEngine;
namespace Lemko
{
    // One palette, shared with the illustrations, plus the few interface shapes the game needs.
    // The shapes are drawn in code in the style of the painted pictures: soft round forms with a dark brown outline.
    // Each is described as a distance field in "design pixels" (40 to a world unit, two interface units each) and
    // rendered four times finer with smooth edges, so it keeps its old size on screen but is never jagged.
    public static class Look
    {
        // Every colour here comes from the game's palette; Tools/InkArt draws every picture with the same one.
        public static readonly Color Peach=Hex(0xF6E4B0), Paper=Hex(0xFBF5D8), Ink=Hex(0x3A160D), Mint=Hex(0xE0ECC6), Sand=Hex(0xE6C58F),
            Sage=Hex(0xBFD6A0), Pine=Hex(0x4A7A6C), Brick=Hex(0xD5583A), Honey=Hex(0xF2C85F), Good=Hex(0x628745), Retry=Hex(0xE68F4B),
            GoodSoft=Hex(0xCDCB5D), Muted=Hex(0x66747A), Empty=Hex(0xE6C58F), Grass=Hex(0xA6C47E), Water=Hex(0x5B87A6), Wood=Hex(0x8A4B2C),
            Night=Hex(0x2A2233);
        public static Color Hex(int rgb,float a=1)=>new Color(((rgb>>16)&255)/255f,((rgb>>8)&255)/255f,(rgb&255)/255f,a);
        public static Color Alpha(Color c,float a) { c.a=a; return c; }
        // The Lemko word is the star of every prompt.
        public static string Word(string lemko)=>"<size=150%><b>"+lemko+"</b></size>";

        static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset()=>cache.Clear();
        // Paints one sample; x and y are in design pixels from the lower left corner, x right and y up.
        delegate Color Paint(float x,float y);
        // Test hook: lets a tool outside Unity look at the generated pixels.
        public static System.Action<string,int,int,Color[]> Dump;
        static readonly Color Line=new Color(.227f,.118f,.078f,1), Lip=new Color(.86f,.84f,.8f,1);
        const int Fine=4;            // texture pixels per design pixel
        const float Edge=1f/Fine;    // the width of the smooth edge, in design pixels
        static Sprite Make(string key,int w,int h,Paint paint,Vector4 border=default)
        {
            if(cache.TryGetValue(key,out var existing) && existing) return existing;
            int tw=w*Fine, th=h*Fine;
            var texture=new Texture2D(tw,th,TextureFormat.RGBA32,true) { name=key, filterMode=FilterMode.Bilinear, wrapMode=TextureWrapMode.Clamp, hideFlags=HideFlags.HideAndDontSave };
            var pixels=new Color[tw*th];
            for(int y=0;y<th;y++) for(int x=0;x<tw;x++) pixels[y*tw+x]=paint((x+.5f)/Fine,(y+.5f)/Fine);
            // See-through samples take the outline's colour, so smoothing never brings in a light fringe.
            for(int i=0;i<pixels.Length;i++) if(pixels[i].a<=0) pixels[i]=new Color(Line.r,Line.g,Line.b,0);
            Dump?.Invoke(key,tw,th,pixels);
            texture.SetPixels(pixels); texture.Apply(true,true);
            var sprite=Sprite.Create(texture,new Rect(0,0,tw,th),new Vector2(.5f,.5f),Pixels.PerUnit*Fine,0,SpriteMeshType.FullRect,new Vector4(border.x*Fine,border.y*Fine,border.z*Fine,border.w*Fine));
            sprite.name=key; sprite.hideFlags=HideFlags.HideAndDontSave; cache[key]=sprite; return sprite;
        }

        // ---------- drawing helpers ----------
        // How much of a sample lies inside a shape whose signed distance is d (negative inside).
        static float Cover(float d)=>Mathf.Clamp01(.5f-d/Edge);
        // Lays a colour over what is there, by coverage.
        static Color Over(Color under,Color top,float cover)
        {
            float a=top.a*cover; if(a<=0) return under;
            float outA=a+under.a*(1-a); if(outA<=0) return Color.clear;
            float u=under.a*(1-a);
            return new Color((top.r*a+under.r*u)/outA,(top.g*a+under.g*u)/outA,(top.b*a+under.b*u)/outA,outA);
        }
        // A filled shape with an outline of the given width just inside its edge.
        static Color Outlined(float d,Color fill,float width=1.3f)
        { var c=Over(Color.clear,Line,Cover(d)); return Over(c,fill,Cover(d+width)); }
        static float Circle(Vector2 p,Vector2 c,float r)=>(p-c).magnitude-r;
        static float Box(Vector2 p,Vector2 c,Vector2 half,float r)
        {
            var q=new Vector2(Mathf.Abs(p.x-c.x),Mathf.Abs(p.y-c.y))-half+new Vector2(r,r);
            return new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-r;
        }
        static float Segment(Vector2 p,Vector2 a,Vector2 b)
        { var ab=b-a; float t=Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude); return (p-(a+ab*t)).magnitude; }
        static float Polygon(Vector2 p,Vector2[] v)
        {
            float best=float.MaxValue; bool inside=false;
            for(int i=0,j=v.Length-1;i<v.Length;j=i++) {
                best=Mathf.Min(best,Segment(p,v[j],v[i]));
                if((v[i].y>p.y)!=(v[j].y>p.y) && p.x<(v[j].x-v[i].x)*(p.y-v[i].y)/(v[j].y-v[i].y)+v[i].x) inside=!inside;
            }
            return inside ? -best : best;
        }
        // The heart of the HUD, in a box one unit high (after Inigo Quilez's heart).
        static float HeartShape(Vector2 p)
        {
            p.x=Mathf.Abs(p.x);
            if(p.y+p.x>1) return (p-new Vector2(.25f,.75f)).magnitude-Mathf.Sqrt(2)/4;
            float a=(p-new Vector2(0,1)).sqrMagnitude, m=.5f*Mathf.Max(p.x+p.y,0), b=(p-new Vector2(m,m)).sqrMagnitude;
            return Mathf.Sqrt(Mathf.Min(a,b))*Mathf.Sign(p.x-p.y);
        }

        // ---------- cards ----------
        // A white card with round corners, as a nine-slice: tint it with any palette colour. With a lip, its lower
        // edge is a slightly darker band that follows the curve of the corners, like a thick painted sticker.
        static Sprite Card(string key,int size,int border,float radius,bool outline,float lip)
            => Make(key,size,size,(x,y)=> {
                var p=new Vector2(x,y); var c=new Vector2(size*.5f,size*.5f); var half=c-new Vector2(.25f,.25f);
                float d=Box(p,c,half,radius);
                if(!outline) return Over(Color.clear,Color.white,Cover(d));
                var col=Outlined(d,Lip);
                if(lip>0) { float top=Box(p,c+new Vector2(0,lip*.5f),half-new Vector2(0,lip*.5f),radius); col=Over(col,Color.white,Cover(Mathf.Max(top,d+1.3f))); }
                else col=Over(col,Color.white,Cover(d+1.3f));
                return col;
            },new Vector4(border,border,border,border));
        // Dark outline, white fill.
        public static Sprite Panel=>Card("lemko_panel",16,6,5,true,0);
        // The same with a darker lower lip: buttons and the cards the game pieces lie on.
        public static Sprite Button=>Card("lemko_button",16,6,5,true,1.6f);
        // A small outlined tag for the score row.
        public static Sprite Chip=>Card("lemko_chip",12,5,4.5f,true,0);
        // Plain white with round ends, no outline: slider tracks and other quiet fills.
        public static Sprite Pill=>Card("lemko_pill",8,3,3,false,0);
        public static Sprite Square=>Make("lemko_square",2,2,(x,y)=>Color.white);

        // ---------- discs and soft shapes ----------
        // An outlined white disc of the given width in design pixels.
        public static Sprite Disc(int size)=>Make("lemko_disc_"+size,size,size,(x,y)=>Outlined(Circle(new Vector2(x,y),new Vector2(size*.5f,size*.5f),size*.5f-.3f),Color.white));
        public static Sprite Dot=>Disc(10);
        public static Sprite Soft=>Make("lemko_soft",6,6,(x,y)=>Over(Color.clear,Color.white,Cover(Circle(new Vector2(x,y),new Vector2(3,3),2.8f))));
        public static Sprite Diamond=>Make("lemko_diamond",6,6,(x,y)=>Over(Color.clear,Color.white,Cover((Mathf.Abs(x-3)+Mathf.Abs(y-3)-2.8f)*.7071f)));
        // A round glow that fades to nothing at its edge: motes, snow and the soft part of sparkles.
        public static Sprite Glow=>Make("lemko_glow",16,16,(x,y)=> { float r=Mathf.Clamp01(new Vector2(x-8,y-8).magnitude/8); float a=(1-r)*(1-r); return new Color(1,1,1,a*a*(3-2*a)); });
        // A four-pointed twinkle with a soft halo.
        public static Sprite Twinkle=>Make("lemko_twinkle",16,16,(x,y)=> {
            float u=Mathf.Abs(x-8)/7.6f, v=Mathf.Abs(y-8)/7.6f;
            float star=Mathf.Sqrt(u)+Mathf.Sqrt(v)-1;   // a concave four-point star
            float halo=Mathf.Clamp01(1-new Vector2(u,v).magnitude*1.6f);
            return Over(new Color(1,1,1,halo*halo*.55f),Color.white,Mathf.Clamp01(.5f-star*14)); });
        // A thin ring that widens from where something good happened.
        public static Sprite Ring=>Make("lemko_ring",32,32,(x,y)=>Over(Color.clear,Color.white,Cover(Mathf.Abs(Circle(new Vector2(x,y),new Vector2(16,16),14.5f))-1.2f)));
        // A little rounded slip of paper for confetti.
        public static Sprite Confetto=>Make("lemko_confetto",6,10,(x,y)=>Over(Color.clear,Color.white,Cover(Box(new Vector2(x,y),new Vector2(3,5),new Vector2(2.7f,4.7f),1.4f))));
        // A soft rain streak, round at both ends.
        public static Sprite Streak=>Make("lemko_streak",4,16,(x,y)=> { float d=Segment(new Vector2(x,y),new Vector2(2,2),new Vector2(2,14))-1.4f; return new Color(1,1,1,Cover(d)*Mathf.Lerp(.25f,1,y/16f)); });

        // ---------- icons ----------
        public static Sprite Heart=>Make("lemko_heart",12,12,(x,y)=> Outlined(HeartShape(new Vector2((x-6)/8.6f,(y-1.2f)/8.6f))*8.6f,Color.white,1.2f));
        public static Sprite Star=>StarOf(14);
        // A plump star with round points, worked out at any size.
        public static Sprite StarOf(int size)=>Make("lemko_star_"+size,size,size,(x,y)=> {
            float round=size*.06f;
            float d=StarDistance(x-size*.5f,y-size*.47f,size*.5f-.4f-round,size*.23f)-round;
            return Outlined(d,Color.white,Mathf.Max(1.2f,size*.06f)); });
        static float StarDistance(float x,float y,float outer,float inner)
        {
            // Signed distance to a five-point star, evaluated against its ten edges.
            float best=float.MaxValue; bool inside=false; var p=new Vector2(x,y);
            for(int i=0;i<10;i++) {
                var a=Corner(i,outer,inner); var b=Corner(i+1,outer,inner);
                best=Mathf.Min(best,Segment(p,a,b));
                if((a.y>y)!=(b.y>y) && x<(b.x-a.x)*(y-a.y)/(b.y-a.y)+a.x) inside=!inside;
            }
            return inside ? -best : best;
        }
        static Vector2 Corner(int i,float outer,float inner) { float angle=Mathf.PI/2+i*Mathf.PI/5; float r=i%2==0 ? outer : inner; return new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r; }
        // A round paper lantern with a dark cap and foot.
        public static Sprite Lantern=>Make("lemko_lantern",10,12,(x,y)=> {
            var p=new Vector2(x,y);
            var c=Over(Color.clear,Line,Cover(Box(p,new Vector2(5,10.6f),new Vector2(2.2f,1.1f),.6f)));
            c=Over(c,Line,Cover(Box(p,new Vector2(5,1.3f),new Vector2(2.2f,1.1f),.6f)));
            float body=Box(p,new Vector2(5,5.9f),new Vector2(4.6f,3.9f),3.2f);
            c=Over(c,Line,Cover(body)); c=Over(c,Color.white,Cover(body+1.2f));
            return Over(c,Lip,Cover(Mathf.Abs(x-5)-.35f)*Cover(body+1.2f)*.6f); });
        // A chunky rounded "next" arrow pointing right; flip it for "previous".
        public static Sprite Chevron=>Make("lemko_chevron",10,16,(x,y)=> {
            var p=new Vector2(x,y); float d=Mathf.Min(Segment(p,new Vector2(2,14),new Vector2(8,8)),Segment(p,new Vector2(8,8),new Vector2(2,2)))-1.6f;
            return Over(Color.clear,Color.white,Cover(d)); });

        // ---------- badges ----------
        // A round paper badge with a little picture in the middle; glyph gets coordinates from the badge's centre.
        delegate Color Glyph(Vector2 p,Color under);
        static Sprite Badge(string key,int size,Glyph glyph)
            => Make(key,size,size,(x,y)=> {
                var p=new Vector2(x-size*.5f,y-size*.5f); float d=p.magnitude-(size*.5f-.3f);
                var c=Outlined(d,Paper,Mathf.Max(1,size*.05f));
                c=Over(c,Lip,Cover(d+Mathf.Max(1,size*.05f))*Cover(p.y+size*.3f)*.8f);   // a shaded lower rim
                return glyph(p*(26f/size),c); });
        static Color Loudspeaker(Vector2 p,Color c)
        {
            var cone=new[]{ new Vector2(-6.5f,-2.2f),new Vector2(-2.6f,-2.2f),new Vector2(1.6f,-6f),new Vector2(1.6f,6f),new Vector2(-2.6f,2.2f),new Vector2(-6.5f,2.2f) };
            c=Over(c,Line,Cover(Polygon(p,cone)-.5f));
            float angle=Mathf.Abs(Mathf.Atan2(p.y,p.x-1.6f));
            if(angle<.9f) foreach(float r in new[]{3.6f,6.6f}) c=Over(c,Line,Cover(Mathf.Abs((p-new Vector2(1.6f,0)).magnitude-r)-.75f));
            return c;
        }
        // The "hear this word" button beside a task.
        public static Sprite Speaker=>Badge("lemko_speaker",26,Loudspeaker);
        // The same, small: it sits on the corner of every card that can be heard.
        public static Sprite SpeakerSmall=>Badge("lemko_speaker_small",14,Loudspeaker);
        // Its partner, "show me what this means": the same badge with a light bulb.
        public static Sprite HintBadge=>Badge("lemko_hint",26,(p,c)=> {
            c=Over(c,Line,Cover(Box(p,new Vector2(0,-5.6f),new Vector2(2.4f,1.9f),.8f)));
            float bulb=Circle(p,new Vector2(0,1.4f),5.2f);
            c=Over(c,Line,Cover(bulb)); c=Over(c,Honey,Cover(bulb+1.1f));
            return Over(c,Color.white,Cover(Circle(p,new Vector2(-1.8f,3.2f),1.3f))*.85f); });
    }
}
