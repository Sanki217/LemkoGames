using UnityEngine;
using UnityEngine.UI;
namespace Lemko
{
    // The whole game is pixel art on one grid: the 16 x 9 world is 640 x 360 picture pixels, 40 to a world unit, and
    // every picture is shown at exactly that size, never enlarged or shrunk on its own. The interface is laid out on
    // a 1280 x 720 canvas, so there one picture pixel is two interface units.
    public static class Pixels
    {
        public const int PerUnit=40;            // picture pixels per world unit
        public const float Unit=1f/PerUnit;     // one picture pixel, in world units
        public const float Ui=2f;               // one picture pixel, in interface units
        public static float Snap(float v)=>Mathf.Round(v*PerUnit)/PerUnit;
        public static Vector2 Snap(Vector2 p)=>new Vector2(Snap(p.x),Snap(p.y));
        public static Vector3 Snap(Vector3 p)=>new Vector3(Snap(p.x),Snap(p.y),p.z);
        // A size in world units made of an even number of pixels, so a card centred on the grid keeps its edges on it.
        public static float Even(float v)=>Mathf.Max(2,Mathf.Round(v*PerUnit*.5f)*2)/PerUnit;
        public static Vector2 Even(Vector2 size)=>new Vector2(Even(size.x),Even(size.y));
        public static float SnapUi(float v)=>Mathf.Round(v/Ui)*Ui;
        public static Vector2 SnapUi(Vector2 p)=>new Vector2(SnapUi(p.x),SnapUi(p.y));
        // A width or height in interface units whose half is still a whole pixel (for boxes placed by their middle).
        public static float EvenUi(float v)=>Mathf.Max(2*Ui,Mathf.Round(v/(2*Ui))*2*Ui);
        public static Vector2 EvenUi(Vector2 size)=>new Vector2(EvenUi(size.x),EvenUi(size.y));
        // How big a picture is in the interface when its pixels are the game's pixels.
        public static Vector2 UiSize(Sprite sprite)=>sprite ? sprite.rect.size*Ui : Vector2.zero;
        public static void Native(Image image)
        {
            if(!image || !image.sprite) return;
            image.preserveAspect=false; var size=UiSize(image.sprite); var rect=image.rectTransform;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,size.x); rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,size.y);
            Seat(rect);
        }
        // Moves an interface picture by less than a screen pixel so that its corner lies on one. At a whole-number
        // size an interface unit can be one and a half screen pixels (1080p); a picture that began on a half pixel
        // would show pixels of uneven width.
        static readonly Vector3[] corners=new Vector3[4];
        public static void Seat(RectTransform rect)
        {
            if(!rect) return;
            var canvas=rect.GetComponentInParent<Canvas>(true); if(!canvas || canvas.rootCanvas.renderMode==RenderMode.WorldSpace) return;
            rect.GetWorldCorners(corners);   // on a screen canvas these are screen pixels
            var shift=new Vector3(Mathf.Round(corners[0].x)-corners[0].x,Mathf.Round(corners[0].y)-corners[0].y,0);
            if(shift.sqrMagnitude>1e-6f && shift.sqrMagnitude<1) rect.position+=shift;
        }
        // A world picture at its own size: no scale, no turn, standing on the grid.
        public static void Native(Transform picture)
        {
            if(!picture) return;
            picture.localScale=Vector3.one; picture.localRotation=Quaternion.identity; picture.localPosition=Snap(picture.localPosition);
        }
        // A card behind a picture or a word, as a nine-slice at its own pixel size.
        public static void Card(SpriteRenderer card,Vector2 size,Sprite sprite=null)
        {
            if(!card) return;
            card.sprite=sprite ? sprite : Look.Button; card.drawMode=SpriteDrawMode.Sliced; card.size=Even(size); card.transform.localScale=Vector3.one;
        }
    }
}
