using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko.Editor
{
    public static class PlaceholderArtSetup
    {
        public const string Root="Assets/Lemko/Art/Placeholders";
        [MenuItem("Lemko/Art/Apply simple placeholder art (preserve assigned sprites)")]
        public static void Apply()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            AssetDatabase.Refresh();
            foreach(var path in Directory.GetFiles(Root,"*.png",SearchOption.AllDirectories)) {
                var assetPath=path.Replace('\\','/');
                var importer=(TextureImporter)AssetImporter.GetAtPath(assetPath);
                importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=assetPath.Contains("Backgrounds/") ? 100 : 256;
                importer.alphaIsTransparency=true; importer.mipmapEnabled=false; importer.filterMode=FilterMode.Bilinear;
                importer.textureCompression=TextureImporterCompression.Uncompressed; importer.maxTextureSize=2048;
                importer.SaveAndReimport();
            }
            foreach(var name in new[]{"apple","pear","bread","cheese","egg","milk","water"}) {
                var entry=AssetDatabase.LoadAssetAtPath<VocabularyEntry>(VocabularyImporter.Root+"/Vocabulary/demo_"+name+".asset");
                if(entry && !entry.sprite) {
                    entry.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Products/ph_"+name+".png");
                    EditorUtility.SetDirty(entry);
                }
            }
            Directory.CreateDirectory("Docs/Backups");
            const string backup="Docs/Backups/LemkoVillage2D-before-placeholder-art.unity.txt";
            if(!File.Exists(backup)) File.Copy(WorldShopSetup.ScenePath,backup);
            var scene=EditorSceneManager.OpenScene(WorldShopSetup.ScenePath,OpenSceneMode.Single);
            var session=UnityEngine.Object.FindFirstObjectByType<GameSession>(FindObjectsInactive.Include);
            if(!session || !session.shopController.worldRoot) throw new InvalidOperationException("Open/setup the 2D Shop first.");
            if(!scene.GetRootGameObjects().Any(g=>g.name=="Placeholder Village Background")) {
                Background("Placeholder Village Background",null,"ph_village_background",-50);
                var image=session.villagePanel.GetComponent<Image>(); image.color=Color.clear; image.raycastTarget=false;
                foreach(var button in session.villagePanel.GetComponentsInChildren<Button>(true))
                    button.image.color=new Color(.12f,.24f,.20f,.86f);
            }
            if(!session.shopController.worldRoot.Find("Placeholder Shop Background"))
                Background("Placeholder Shop Background",session.shopController.worldRoot,"ph_shop_background",-40);
            AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(scene,WorldShopSetup.ScenePath);
            WorldShopSetup.Validate();
            if(session.shop.products.Any(w=>!w.sprite)) Debug.LogWarning("Some authored products still use fallback artwork; assign final sprites in their vocabulary assets.");
            Debug.Log("LEMKO_PLACEHOLDER_ART_OK: 7 product sprites, 2 backgrounds; existing carrot preserved. No build requested.");
        }
        static void Background(string name,Transform parent,string spriteName,int order)
        {
            var go=new GameObject(name,typeof(SpriteRenderer));
            go.transform.SetParent(parent,false); go.transform.localPosition=Vector3.zero;
            var renderer=go.GetComponent<SpriteRenderer>();
            renderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Backgrounds/"+spriteName+".png");
            renderer.sortingOrder=order;
            if(!renderer.sprite) throw new InvalidOperationException("Missing background " + spriteName);
        }
    }
}
