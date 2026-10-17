using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Lemko.Editor
{
    public static class WorldShopSetup
    {
        public const string ScenePath = "Assets/Lemko/Scenes/LemkoVillage2D.unity";
        const string PrefabPath = "Assets/Lemko/Prefabs/Words/WorldWord.prefab";
        static void Folder(string path) { Directory.CreateDirectory(path); }
        static void Move(string from, string to) {
            if(!AssetDatabase.IsValidFolder(from) && !File.Exists(from)) return;
            if(File.Exists(to) || AssetDatabase.IsValidFolder(to)) throw new InvalidOperationException("Destination exists: " + to);
            var error = AssetDatabase.MoveAsset(from,to);
            if(!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
        }
        [MenuItem("Lemko/Setup/Organize and create 2D Shop (preserve existing)")]
        public static void Create()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach(var folder in new[]{"Scenes","Prefabs/Words","Art/Sprites/Products","Art/Sprites/Backgrounds","Art/Materials","Audio/Pronunciation","Audio/SFX","Editor","Data","Fonts"}) Folder("Assets/Lemko/" + folder);
            AssetDatabase.Refresh();
            // Move individual assets through Unity so all GUID references survive.
            if(AssetDatabase.IsValidFolder("Assets/Lemko/Generated")) {
                Move("Assets/Lemko/Generated/LemkoVillage.unity","Assets/Lemko/Scenes/LemkoVillageUI.unity");
                Move("Assets/Lemko/Generated/WordView.prefab","Assets/Lemko/Prefabs/Words/UIWord.prefab");
                Move("Assets/Lemko/Generated/LemkoFont.asset","Assets/Lemko/Fonts/LemkoFont.asset");
                foreach(var path in Directory.GetFiles("Assets/Lemko/Generated","*.asset")) Move(path.Replace('\\','/'),"Assets/Lemko/Data/" + Path.GetFileName(path));
                Move("Assets/Lemko/Generated/Vocabulary","Assets/Lemko/Data/Vocabulary");
                // Keep the now-empty legacy folder; never delete unknown user assets.
            }
            foreach(var name in new[]{"StarterSetup","VocabularyImporter","CsvTable","ContentValidator","MilestoneChecks","WorldShopSetup"}) Move("Assets/Editor/"+name+".cs","Assets/Lemko/Editor/"+name+".cs");
            AssetDatabase.SaveAssets();
            if(File.Exists(ScenePath)) { Debug.Log("2D scene preserved: " + ScenePath); return; }
            string carrotPath = "Assets/Lemko/Sprites/Products/sprite_carrot.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(carrotPath);
            if(!importer) throw new InvalidOperationException("Missing supplied carrot: " + carrotPath);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 512; importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false; importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048; importer.SaveAndReimport();
            var carrot = AssetDatabase.LoadAssetAtPath<VocabularyEntry>(VocabularyImporter.Root + "/Vocabulary/demo_carrot.asset");
            if(carrot && !carrot.sprite) { carrot.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(carrotPath); EditorUtility.SetDirty(carrot); }
            var scene = EditorSceneManager.OpenScene(StarterSetup.ScenePath,OpenSceneMode.Single);
            var session = UnityEngine.Object.FindFirstObjectByType<GameSession>(FindObjectsInactive.Include);
            var camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            camera.orthographic = true; camera.orthographicSize = 4.5f; camera.transform.position = new Vector3(0,0,-10); camera.transform.rotation = Quaternion.identity;
            camera.gameObject.tag = "MainCamera"; camera.gameObject.AddComponent<Physics2DRaycaster>(); camera.gameObject.AddComponent<WorldCameraFit>();
            var previousSmoke = session.GetComponent<MilestoneSmoke>(); if(previousSmoke) UnityEngine.Object.DestroyImmediate(previousSmoke);
            session.gameObject.AddComponent<WorldShopSmoke>().session = session;
            foreach(var image in session.shopPanel.GetComponentsInChildren<Image>(true)) {
                if(image.gameObject == session.shopPanel) { image.color = Color.clear; image.raycastTarget = false; }
            }
            session.shopController.productRoot.gameObject.SetActive(false);
            var world = new GameObject("Shop World - move slots to arrange products");
            session.shopController.worldRoot = world.transform;
            session.shopController.productSlots = new Transform[12];
            var placeholder = Placeholder();
            for(int i=0;i<12;i++) {
                var slot = new GameObject("Product Slot " + (i+1)); slot.transform.SetParent(world.transform,false);
                slot.transform.localPosition = new Vector3(-6.1f+(i%4)*2.05f,1.1f-(i/4)*1.75f,0);
                session.shopController.productSlots[i] = slot.transform;
            }
            for(int row=0;row<3;row++) {
                var shelf = new GameObject("Shelf " + (row+1),typeof(SpriteRenderer)); shelf.transform.SetParent(world.transform,false);
                shelf.transform.localPosition = new Vector3(-3.025f,.05f-row*1.75f,1);
                shelf.transform.localScale = new Vector3(8.2f,.12f,1);
                shelf.GetComponent<SpriteRenderer>().sprite = placeholder; shelf.GetComponent<SpriteRenderer>().color = new Color(.55f,.36f,.21f);
            }
            session.shopController.wordPrefab = MakeWord(placeholder);
            world.SetActive(false);
            AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(scene,ScenePath);
            Validate(); Debug.Log("LEMKO_2D_SETUP_OK: " + ScenePath);
        }
        static Sprite Placeholder()
        {
            const string path = "Assets/Lemko/Sprites/Placeholders/Products/sprite_missing.png";
            if(!File.Exists(path)) {
                var texture = new Texture2D(8,8); texture.SetPixels(Enumerable.Repeat(Color.white,64).ToArray()); texture.Apply();
                File.WriteAllBytes(path,texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture); AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType=TextureImporterType.Sprite; importer.spritePixelsPerUnit=8; importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static WordView MakeWord(Sprite placeholder)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath); if(existing) return existing.GetComponent<WordView>();
            var root = new GameObject("WorldWord",typeof(WordView),typeof(BoxCollider2D));
            root.GetComponent<BoxCollider2D>().size = new Vector2(1.8f,1.65f);
            var view = root.GetComponent<WordView>(); view.placeholderSprite = placeholder; view.pictureBounds = new Vector2(1.35f,1.2f);
            var picture = new GameObject("Picture",typeof(SpriteRenderer)); picture.transform.SetParent(root.transform,false);
            view.worldPicture = picture.GetComponent<SpriteRenderer>(); view.worldPicture.sortingOrder = 2; view.worldPicture.sprite = placeholder;
            var label = new GameObject("Word label",typeof(TextMeshPro)); label.transform.SetParent(root.transform,false); label.transform.localPosition = new Vector3(0,-.72f,-.1f);
            var text = label.GetComponent<TextMeshPro>(); text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset");
            text.fontSize = 2.2f; text.alignment = TextAlignmentOptions.Center; text.rectTransform.sizeDelta = new Vector2(1.95f,.5f); text.text = "Word"; text.GetComponent<MeshRenderer>().sortingOrder=3; view.label=text;
            var speaker = new GameObject("Speak",typeof(BoxCollider2D),typeof(WorldPronunciationButton)); speaker.transform.SetParent(root.transform,false); speaker.transform.localPosition = new Vector3(.68f,.63f,-.2f);
            speaker.GetComponent<BoxCollider2D>().size = new Vector2(.55f,.35f); speaker.GetComponent<WorldPronunciationButton>().word=view;
            var speakLabel = new GameObject("Label",typeof(TextMeshPro)); speakLabel.transform.SetParent(speaker.transform,false);
            var st=speakLabel.GetComponent<TextMeshPro>(); st.font=text.font; st.fontSize=1.5f; st.text="Play"; st.alignment=TextAlignmentOptions.Center; st.rectTransform.sizeDelta=new Vector2(.55f,.35f); st.GetComponent<MeshRenderer>().sortingOrder=4;
            view.worldSpeaker=speaker;
            var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath); UnityEngine.Object.DestroyImmediate(root); return prefab.GetComponent<WordView>();
        }
        [MenuItem("Lemko/Validate 2D Shop scene")]
        public static void Validate()
        {
            var session=UnityEngine.Object.FindFirstObjectByType<GameSession>(FindObjectsInactive.Include);
            if(!session || !session.shopController.worldRoot) throw new Exception("Open the 2D scene before validation.");
            if(session.shop.Validate()!=null) throw new Exception(session.shop.Validate());
            if(session.shopController.worldRoot.GetComponentInParent<Canvas>()) throw new Exception("World products must be outside Canvas.");
            if(session.shopController.productSlots.Length < Mathf.Min(session.shop.displayedProductCount, session.shop.products.Count)) throw new Exception("Insufficient product slots.");
            if(!session.shopController.wordPrefab.GetComponent<Collider2D>() || !session.shopController.wordPrefab.worldPicture) throw new Exception("World prefab missing collider or sprite.");
            foreach(var root in session.gameObject.scene.GetRootGameObjects()) foreach(var t in root.GetComponentsInChildren<Transform>(true))
                if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0) throw new Exception("Missing script: " + t.name);
            Debug.Log("LEMKO_2D_VALIDATION_OK");
        }
        [MenuItem("Lemko/Build Windows 2D Shop")]
        public static void Build()
        {
            if(!File.Exists(ScenePath)) Create();
            var report=BuildPipeline.BuildPlayer(new[]{ScenePath},"Builds/Windows2D/LemkoVillage2D.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("2D build failed");
            Debug.Log("LEMKO_2D_BUILD_OK");
        }
        public static void Batch() { Create(); Build(); }
        public static void ValidateBatch()
        {
            var before=File.ReadAllBytes(ScenePath);
            Create();
            if(!before.SequenceEqual(File.ReadAllBytes(ScenePath))) throw new Exception("Setup changed an existing 2D scene");
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single); Validate();
            MilestoneChecks.Run();
            Debug.Log("LEMKO_2D_RERUN_PRESERVED");
        }
    }
}
