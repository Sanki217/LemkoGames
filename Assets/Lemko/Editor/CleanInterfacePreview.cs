using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Lemko.Editor {
    // Authoring-only layout preview. Does not enter Play mode, execute a round or read player saves.
    public static class CleanInterfacePreview {
        static GameSession session;
        static int frames;
        public static void Capture() {
            CleanInterfaceSetup.CheckHints();
            EditorSceneManager.OpenScene(SeparateScenesSetup.Folder+"/Shop.unity");
            session=UnityEngine.Object.FindFirstObjectByType<GameSession>();
            session.shopPanel.SetActive(true); session.resultsPanel.SetActive(false); session.exhaustedPanel.SetActive(false); session.menu.panel.SetActive(false);
            session.tooltip.Hide(); var shop=session.shopController; shop.worldRoot.gameObject.SetActive(true);
            string[] ids={"food_cherries","food_pears","food_cream","food_carrot","food_cheese","food_potatoes","food_mint","food_fish"};
            var words=ids.Select(id=>shop.definition.products.FirstOrDefault(w=>w.id==id)).Where(w=>w).ToList();
            foreach(var w in shop.definition.products) if(words.Count<8 && !words.Contains(w)) words.Add(w);
            for(int i=0;i<8;i++) {
                var view=UnityEngine.Object.Instantiate(shop.wordPrefab,shop.productSlots[i]); view.Bind(words[i],session.tooltip,session.audioService,null);
                if(i<2) { view.SetSelected(true); var copy=UnityEngine.Object.Instantiate(shop.wordPrefab,shop.basketRoot); copy.Bind(words[i],session.tooltip,session.audioService,null); copy.transform.localPosition=shop.basketStart+Vector3.right*i*shop.basketSpacing; copy.transform.localScale=Vector3.one*shop.basketItemScale; }
            }
            for(int i=0;i<5;i++) { var row=UnityEngine.Object.Instantiate(shop.listRowPrefab,shop.listRoot); row.gameObject.SetActive(true); row.text=words[i].lemko; }
            shop.shoppingList.text=shop.definition.shoppingListTitle; shop.feedback.text="Choose the products on your list."; shop.basketStatus.text="Basket · 2 items   /   Tap to remove";
            session.hud.text="Shop · 0 / 5\n0 points   •   ×1   •   0 hints"; session.sessionProgress.fillAmount=.4f;
            frames=0; EditorApplication.update+=Tick;
        }
        static void Tick() {
            if(++frames<20) { UnityEditorInternal.InternalEditorUtility.RepaintAllViews(); return; }
            EditorApplication.update-=Tick;
            try {
                string[] args=Environment.GetCommandLineArgs(); int index=Array.IndexOf(args,"-lemkoPreviewFolder"); if(index<0) throw new Exception("Preview folder required");
                string folder=args[index+1]; Directory.CreateDirectory(folder);
                Render(folder+"/shop-1280.png",1280,720); Render(folder+"/shop-960.png",960,540);
                session.menu.panel.SetActive(true); session.menu.transform.SetAsLastSibling();
                Render(folder+"/menu-1280.png",1280,720);
                Debug.Log("LEMKO_UI_AUTHORING_PREVIEW_PASS: Shop 1280/960 and menu. No Play mode, round simulation or real saves."); EditorApplication.Exit(0);
            } catch(Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }
        static void Render(string path,int width,int height) {
            var camera=Camera.main; var canvas=session.hud.GetComponentInParent<Canvas>(); var target=new RenderTexture(width,height,24); target.Create();
            camera.targetTexture=target; camera.aspect=(float)width/height; camera.orthographicSize=4.5f;
            canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=1; canvas.sortingOrder=100;
            Canvas.ForceUpdateCanvases(); foreach(var t in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)) t.ForceMeshUpdate(); Canvas.ForceUpdateCanvases();
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
            var old=RenderTexture.active; RenderTexture.active=target; var image=new Texture2D(width,height,TextureFormat.RGB24,false); image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply(); File.WriteAllBytes(path,image.EncodeToPNG());
            RenderTexture.active=old; camera.targetTexture=null; target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(image);
        }
    }
}
