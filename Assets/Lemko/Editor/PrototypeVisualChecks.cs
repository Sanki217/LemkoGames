using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Lemko.Editor
{
    [InitializeOnLoad]
    public static class PrototypeVisualChecks
    {
        const string Running="Lemko.VisualChecks";
        static readonly string[] scenes={"Village","Shop","Farm","House","FamilyTree","Memory","Numbers","Alphabet","Weather"};
        static int index;
        static double next;
        static PrototypeVisualChecks() { EditorApplication.update+=Tick; }
        public static void RunBatch()
        {
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null) throw new Exception("Graphics required: omit -nographics");
            EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath,OpenSceneMode.Single);
            SessionState.SetBool(Running,true); SessionState.SetFloat("Lemko.VisualDeadline",(float)EditorApplication.timeSinceStartup+180); EditorApplication.EnterPlaymode();
        }
        static void Tick()
        {
            if(!SessionState.GetBool(Running,false)) return;
            try {
                if(EditorApplication.timeSinceStartup>SessionState.GetFloat("Lemko.VisualDeadline",0)) throw new Exception("Capture timed out");
                if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<next) return;
                next=EditorApplication.timeSinceStartup+2;
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>(); if(!session || session.Progress==null) return;
                Capture(scenes[index],1280,720); Capture(scenes[index],960,540);
                if(session.activity is MemoryController memory) { var cards=memory.taskRoot.GetComponentsInChildren<ActivityToken>(); memory.Choose(cards[0]); memory.Choose(cards.First(t=>t!=cards[0] && t.view.Entry==cards[0].view.Entry)); Capture("Memory-revealed",1280,720); Capture("Memory-revealed",960,540); }
                index++;
                if(index<scenes.Length) { session.OpenActivity(scenes[index]); return; }
                Debug.Log("LEMKO_PROTOTYPE_RENDER_PASS: nine scenes captured at 1280x720 and 960x540 with graphics enabled; camera-space UI capture.");
                SessionState.SetBool(Running,false); EditorApplication.Exit(0);
            } catch(Exception e) { Debug.LogException(e); SessionState.SetBool(Running,false); EditorApplication.Exit(1); }
        }
        static void Capture(string name,int width,int height)
        {
            var camera=Camera.main; var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();
            var mode=canvas.renderMode; var oldCamera=canvas.worldCamera; float distance=canvas.planeDistance;
            var rt=new RenderTexture(width,height,24); rt.Create(); camera.targetTexture=rt; camera.aspect=(float)width/height; camera.orthographicSize=4.5f;
            canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=1; Canvas.ForceUpdateCanvases();
            foreach(var text in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)) text.ForceMeshUpdate();
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});
            var old=RenderTexture.active; RenderTexture.active=rt; var image=new Texture2D(width,height,TextureFormat.RGB24,false); image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
            Directory.CreateDirectory("Docs/Validation/PrototypeCaptures"); File.WriteAllBytes("Docs/Validation/PrototypeCaptures/"+name+"-"+width+".png",image.EncodeToPNG());
            RenderTexture.active=old; camera.targetTexture=null; canvas.renderMode=mode; canvas.worldCamera=oldCamera; canvas.planeDistance=distance;
            UnityEngine.Object.DestroyImmediate(image); rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
