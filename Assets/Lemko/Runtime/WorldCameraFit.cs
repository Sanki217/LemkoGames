using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
namespace Lemko
{
    // Shows the whole 16 x 9 world, fitted to the window. The art is painted and drawn smooth, so it scales to any
    // size; a window of another shape gets a dark frame around the picture (the Juice layer draws it).
    // The interface follows: the 1280 x 720 canvas has 80 interface units to a world unit.
    [RequireComponent(typeof(Camera))]
    public sealed class WorldCameraFit : MonoBehaviour
    {
        [Tooltip("Minimum visible world area; matches the 1280 x 720 UI reference.")]
        public Vector2 referenceSize = new Vector2(16, 9);
        Camera target; int width, height; Vector3 home; bool placed;
        // Kept for older callers: the picture is always fitted now, so this is always 0.
        public static int Zoom {get; private set;}
        void Awake() { target = GetComponent<Camera>(); }
        void OnDisable() { PixelPerfectRendering.pixelSnapSpacing=0; width=height=0; }
        void LateUpdate()
        {
            if(target.targetTexture) return;   // an editor preview is rendering into a picture of its own size
            PixelPerfectRendering.pixelSnapSpacing=0;
            int w=Screen.width, h=Screen.height;
            if(w==width && h==height) return;
            width=w; height=h;
            if(!placed) { home=transform.position; placed=true; }
            Zoom=0;
            target.orthographicSize=Mathf.Max(referenceSize.y*.5f,referenceSize.x/(2f*target.aspect));
            transform.position=home;
            foreach(var scaler in FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include,FindObjectsSortMode.None)) Apply(scaler);
        }
        // Screen canvases scale with the window, like the world.
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
