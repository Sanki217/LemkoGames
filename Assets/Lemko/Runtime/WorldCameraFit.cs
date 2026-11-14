using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
namespace Lemko
{
    // Shows the 16 x 9 world as pixel art. The world is 640 x 360 picture pixels (40 to a unit). Whenever the window
    // is at least twice that size the picture is enlarged by a whole number, so every picture pixel covers the same
    // square block of screen pixels, and what is left of the window is a dark frame (the Juice layer draws it).
    // The interface follows: one picture pixel is two interface units of the 1280 x 720 canvas.
    // A window too small for that shows the picture fitted to it, as before, with pixels of uneven size.
    [RequireComponent(typeof(Camera))]
    public sealed class WorldCameraFit : MonoBehaviour
    {
        [Tooltip("Minimum visible world area; matches the 1280 x 720 UI reference.")]
        public Vector2 referenceSize = new Vector2(16, 9);
        Camera target; int width, height; Vector3 home; bool placed;
        // Screen pixels per picture pixel while the picture is shown at a whole-number size; 0 while it is fitted.
        public static int Zoom {get; private set;}
        void Awake() { target = GetComponent<Camera>(); }
        void OnDisable() { PixelPerfectRendering.pixelSnapSpacing=0; width=height=0; }
        void LateUpdate()
        {
            if(target.targetTexture) return;   // an editor preview is rendering into a picture of its own size
            int w=Screen.width, h=Screen.height;
            // Sprites are drawn on whole picture pixels wherever they are put.
            PixelPerfectRendering.pixelSnapSpacing=Zoom>0 ? Pixels.Unit : 0;
            if(w==width && h==height) return;
            width=w; height=h;
            if(!placed) { home=transform.position; placed=true; }
            int zoom=Mathf.FloorToInt(Mathf.Min(w/(referenceSize.x*Pixels.PerUnit),h/(referenceSize.y*Pixels.PerUnit)));
            Zoom=zoom>=2 ? zoom : 0;
            if(Zoom>0) {
                float perUnit=Pixels.PerUnit*Zoom;   // screen pixels per world unit
                target.orthographicSize=h/(2f*perUnit);
                // In a window of odd size the middle of the screen is the middle of a pixel: move over by half of one.
                transform.position=home+new Vector3((w&1)*.5f/perUnit,(h&1)*.5f/perUnit,0);
            } else {
                target.orthographicSize=Mathf.Max(referenceSize.y*.5f,referenceSize.x/(2f*target.aspect));
                transform.position=home;
            }
            PixelPerfectRendering.pixelSnapSpacing=Zoom>0 ? Pixels.Unit : 0;
            foreach(var scaler in FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include,FindObjectsSortMode.None)) Apply(scaler);
        }
        // Screen canvases show pictures at two interface units per pixel and scale by the same whole number as the world.
        public static void Apply(CanvasScaler scaler)
        {
            if(!scaler || !scaler.TryGetComponent<Canvas>(out var canvas) || !canvas.isRootCanvas || canvas.renderMode==RenderMode.WorldSpace) return;
            scaler.referencePixelsPerUnit=Pixels.PerUnit*Pixels.Ui;
            if(Zoom>0) { scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize; scaler.scaleFactor=Zoom/Pixels.Ui; }
            else {
                scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1280,720);
                scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            }
        }
    }
}
