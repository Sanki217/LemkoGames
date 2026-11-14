using System.Collections.Generic;
using UnityEngine;
namespace Lemko
{
    // One palette, shared with the illustrations, plus the few interface shapes the game needs.
    // The shapes are pixel art made in code, on the same grid as every picture: 40 pixels per world unit, which is
    // two interface units per pixel. Icons are spelled out pixel by pixel; cards and discs are worked out per pixel.
    // Nothing here is ever smoothed, so a shape must be shown at its own size (or stretched as a nine-slice).
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
        delegate Color Paint(int x,int y);
        // Test hook: lets a tool outside Unity look at the generated pixels.
        public static System.Action<string,int,int,Color[]> Dump;
        static readonly Color Line=new Color(.227f,.086f,.051f,1), Lip=new Color(.8f,.8f,.8f,1);
        // x runs right and y runs up, one call per pixel.
        static Sprite Make(string key,int w,int h,Paint paint,Vector4 border=default)
        {
            if(cache.TryGetValue(key,out var existing) && existing) return existing;
            var texture=new Texture2D(w,h,TextureFormat.RGBA32,false) { name=key, filterMode=FilterMode.Point, wrapMode=TextureWrapMode.Clamp, hideFlags=HideFlags.HideAndDontSave };
            var pixels=new Color[w*h];
            for(int y=0;y<h;y++) for(int x=0;x<w;x++) pixels[y*w+x]=paint(x,y);
            Dump?.Invoke(key,w,h,pixels);
            texture.SetPixels(pixels); texture.Apply(false,true);
            var sprite=Sprite.Create(texture,new Rect(0,0,w,h),new Vector2(.5f,.5f),Pixels.PerUnit,0,SpriteMeshType.FullRect,border);
            sprite.name=key; sprite.hideFlags=HideFlags.HideAndDontSave; cache[key]=sprite; return sprite;
        }
        // A picture spelled out row by row, top row first: '#' is the dark outline, 'o' white (it takes the tint),
        // '+' the darker lip, 'p' paper, 'h' honey, 'b' brick; anything else is see-through.
        static Color Tone(char c)=>c=='#' ? Line : c=='o' ? Color.white : c=='+' ? Lip : c=='p' ? Paper : c=='h' ? Honey : c=='b' ? Brick : Color.clear;
        static Sprite Art(string key,string[] rows)=>Make(key,rows[0].Length,rows.Length,(x,y)=>Tone(rows[rows.Length-1-y][x]));

        // ---------- cards ----------
        static readonly int[] Round={4,2,1,1}, Slight={2,1};   // how far the first rows of a corner are cut in
        static int Inset(int row,int size,int[] cut) { if(row<0 || row>=size) return size; int d=Mathf.Min(row,size-1-row); return d<cut.Length ? cut[d] : 0; }
        // A white card with cut corners, as a nine-slice: tint it with any palette colour.
        static Sprite Card(string key,int size,int border,int[] cut,bool outline,int lip)
            => Make(key,size,size,(x,y)=> {
                int a=Inset(y,size,cut); if(x<a || x>=size-a) return Color.clear;
                int above=Inset(y+1,size,cut), below=Inset(y-1,size,cut);
                bool edge=x==a || x==size-1-a || x<above || x>=size-above || x<below || x>=size-below;
                return edge && outline ? Line : lip>0 && y>=1 && y<=lip ? Lip : Color.white;
            },new Vector4(border,border,border,border));
        // Dark outline, white fill.
        public static Sprite Panel=>Card("lemko_panel",16,6,Round,true,0);
        // The same with a darker lower lip: buttons and the cards the game pieces lie on.
        public static Sprite Button=>Card("lemko_button",16,6,Round,true,2);
        // A small outlined tag for the score row.
        public static Sprite Chip=>Card("lemko_chip",12,4,Slight,true,0);
        // Plain white with softened corners, no outline: slider tracks and other quiet fills.
        public static Sprite Pill=>Card("lemko_pill",8,3,Slight,false,0);
        public static Sprite Square=>Make("lemko_square",2,2,(x,y)=>Color.white);

        // ---------- discs ----------
        // An outlined white disc of the given width in pixels.
        public static Sprite Disc(int size)=>Make("lemko_disc_"+size,size,size,(x,y)=> {
            float r=size*.5f, d=new Vector2(x+.5f-r,y+.5f-r).magnitude;
            return d>r ? Color.clear : d>r-1.3f ? Line : Color.white; });
        public static Sprite Dot=>Disc(10);
        public static Sprite Soft=>Make("lemko_soft",6,6,(x,y)=>new Vector2(x-2.5f,y-2.5f).magnitude>3 ? Color.clear : Color.white);
        public static Sprite Diamond=>Art("lemko_diamond",new[] {
            "..oo..",
            ".oooo.",
            "oooooo",
            "oooooo",
            ".oooo.",
            "..oo.." });

        // ---------- icons ----------
        public static Sprite Heart=>Art("lemko_heart",new[] {
            "............",
            "..##....##..",
            ".#oo#..#oo#.",
            "#oooo##oooo#",
            "#oooooooooo#",
            "#oooooooooo#",
            "#oooooooooo#",
            ".#oooooooo#.",
            "..#oooooo#..",
            "...#oooo#...",
            "....#oo#....",
            ".....##....." });
        public static Sprite Star=>Art("lemko_star",new[] {
            "......##......",
            "......##......",
            ".....#oo#.....",
            ".....#oo#.....",
            "#####oooo#####",
            "#oooooooooooo#",
            ".#oooooooooo#.",
            "..#oooooooo#..",
            "...#oooooo#...",
            "...#oooooo#...",
            "..#ooo##ooo#..",
            "..#oo#..#oo#..",
            ".#o##....##o#.",
            ".##........##." });
        // A big star for the results card, worked out at any size.
        public static Sprite StarOf(int size)=>Make("lemko_star_"+size,size,size,(x,y)=> {
            float d=StarDistance(x+.5f-size*.5f,y+.5f-size*.47f,size*.5f-.5f,size*.22f);
            return d>0 ? Color.clear : d>-1.4f ? Line : Color.white; });
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
        public static Sprite Lantern=>Art("lemko_lantern",new[] {
            "...####...",
            "...#oo#...",
            ".########.",
            ".#oooooo#.",
            "#oooooooo#",
            "#oooooooo#",
            "#oooooooo#",
            "#oooooooo#",
            "#oooooooo#",
            ".#oooooo#.",
            ".########.",
            "...####..." });
        // A chunky "next" arrow pointing right; flip it for "previous".
        public static Sprite Chevron=>Art("lemko_chevron",new[] {
            "ooo.......",
            ".ooo......",
            "..ooo.....",
            "...ooo....",
            "....ooo...",
            ".....ooo..",
            "......ooo.",
            ".......ooo",
            ".......ooo",
            "......ooo.",
            ".....ooo..",
            "....ooo...",
            "...ooo....",
            "..ooo.....",
            ".ooo......",
            "ooo......." });

        // ---------- badges ----------
        // A round paper badge with a little picture in the middle.
        static Sprite Badge(string key,int size,string[] glyph)
            => Make(key,size,size,(x,y)=> {
                float r=size*.5f, d=new Vector2(x+.5f-r,y+.5f-r).magnitude;
                if(d>r) return Color.clear;
                int gx=x-(size-glyph[0].Length)/2, gy=(size-1-y)-(size-glyph.Length)/2;
                if(gx>=0 && gy>=0 && gy<glyph.Length && gx<glyph[0].Length && glyph[gy][gx]!='.') return Tone(glyph[gy][gx]);
                return d>r-1.3f ? Line : Paper; });
        static readonly string[] Loudspeaker={
            "......#.......",
            ".....##...#...",
            "....###....#..",
            "#######..#..#.",
            "#######...#.#.",
            "#######...#.#.",
            "#######...#.#.",
            "#######..#..#.",
            "....###....#..",
            ".....##...#...",
            "......#......." };
        // The "hear this word" button beside a task.
        public static Sprite Speaker=>Badge("lemko_speaker",26,Loudspeaker);
        // The same, small: it sits on the corner of every card that can be heard.
        public static Sprite SpeakerSmall=>Badge("lemko_speaker_small",14,new[] {
            "...#....",
            "..##..#.",
            "####...#",
            "####...#",
            "..##..#.",
            "...#...." });
        // Its partner, "show me what this means": the same badge with a light bulb.
        public static Sprite HintBadge=>Badge("lemko_hint",26,new[] {
            "...####...",
            "..#hhhh#..",
            ".#hhhhhh#.",
            "#hhhhhhhh#",
            "#hhhhhhhh#",
            "#hhhhhhhh#",
            ".#hhhhhh#.",
            "..#hhhh#..",
            "..#hhhh#..",
            "...####...",
            "...#..#...",
            "...####...",
            "....##...." });
    }
}
