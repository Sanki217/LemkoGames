using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko.Editor
{
    public static class EnglishContentMigration
    {
        [Serializable] public class Entry { public string oldId,id,slug,spriteId,audioId,oldSpritePath; public bool active; }
        [Serializable] public class Entries { public Entry[] entries; }
        const string Sprites="Assets/Lemko/Sprites";
        const string RowPath="Assets/Lemko/Prefabs/Words/ShoppingListRow.prefab";
        [MenuItem("Lemko/Content/Apply English IDs and delayed hints")]
        public static void Apply()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var entries=JsonUtility.FromJson<Entries>(File.ReadAllText("Tools/EnglishContent/entries.json")).entries;
            foreach(var path in new[]{Sprites+"/Products",Sprites+"/Placeholders/Products",Sprites+"/Placeholders/Backgrounds","Assets/Lemko/Audio/Pronunciation","Content/CSV/Archive"}) Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
            // Migration records are authoring data only; runtime uses the generated ID alias map.
            foreach(var entry in entries) {
                var oldPath=VocabularyImporter.Root+"/Vocabulary/"+entry.oldId+".asset";
                var newPath=VocabularyImporter.Root+"/Vocabulary/"+entry.id+".asset";
                string guid=AssetDatabase.AssetPathToGUID(oldPath);
                MoveAsset(oldPath,newPath);
                var word=AssetDatabase.LoadAssetAtPath<VocabularyEntry>(newPath);
                if(word) { word.id=entry.id; EditorUtility.SetDirty(word); }
                if(!string.IsNullOrEmpty(guid) && AssetDatabase.AssetPathToGUID(newPath)!=guid) throw new Exception("Vocabulary GUID changed: "+entry.id);
                string target=(entry.slug=="carrot" ? Sprites+"/Products/" : Sprites+"/Placeholders/Products/")+entry.spriteId+".png";
                if(!string.IsNullOrEmpty(entry.oldSpritePath)) MoveAsset(entry.oldSpritePath,target);
                if(!File.Exists(target)) File.Copy("Tools/EnglishContent/GeneratedSprites/"+entry.spriteId+".png",target);
            }
            foreach(var name in new[]{"ph_shop_background","ph_village_background"})
                MoveAsset("Assets/Lemko/Art/Placeholders/Backgrounds/"+name+".png",Sprites+"/Placeholders/Backgrounds/"+name+".png");
            MoveAsset("Assets/Lemko/Art/Sprites/Products/placeholder.png",Sprites+"/Placeholders/Products/sprite_missing.png");
            AssetDatabase.Refresh();
            foreach(var path in Directory.GetFiles(Sprites,"*.png",SearchOption.AllDirectories)) {
                var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
                importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
                if(path.Contains("sprite_missing")) importer.spritePixelsPerUnit=8;
                else importer.spritePixelsPerUnit=path.Contains("Backgrounds") ? 100 : path.Contains("sprite_carrot") ? 512 : 256;
                importer.alphaIsTransparency=true; importer.mipmapEnabled=false; importer.filterMode=FilterMode.Bilinear;
                importer.textureCompression=TextureImporterCompression.Uncompressed; importer.maxTextureSize=2048; importer.SaveAndReimport();
            }
            Archive("Content/Shop","Content/CSV/Archive/ShopBeforeEnglishIds");
            Archive("Content/Starter","Content/CSV/Archive/Starter");
            Archive("Content/Templates","Content/CSV/Templates");
            Archive("Content/Source","Content/CSV/Source");
            foreach(var file in Directory.GetFiles("Content","*.csv")) Archive(file,"Content/CSV/Archive/"+Path.GetFileName(file));
            VocabularyImporter.Import("Content/CSV");
            MakeRow();
            var prefab=PrefabUtility.LoadPrefabContents("Assets/Lemko/Prefabs/Words/WorldWord.prefab");
            var view=prefab.GetComponent<WordView>(); if(view.label) view.label.gameObject.SetActive(false);
            if(!prefab.GetComponent<HintPointer>()) prefab.AddComponent<HintPointer>();
            PrefabUtility.SaveAsPrefabAsset(prefab,"Assets/Lemko/Prefabs/Words/WorldWord.prefab"); PrefabUtility.UnloadPrefabContents(prefab);
            foreach(var path in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Lemko/Scenes"}).Select(AssetDatabase.GUIDToAssetPath)) {
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>(FindObjectsInactive.Include); if(!session) continue;
                session.tooltip.holdSeconds=3; session.tooltip.Hide();
                session.languageButton.GetComponentInChildren<TMP_Text>().text="Hints: Polish";
                if(session.shopController) {
                    var shop=session.shopController;
                    if(!shop.listRoot) {
                        var go=new GameObject("Shopping List Rows",typeof(RectTransform),typeof(VerticalLayoutGroup));
                        var rect=go.GetComponent<RectTransform>(); rect.SetParent(shop.shoppingList.transform.parent,false);
                        rect.anchoredPosition=new Vector2(430,-20); rect.sizeDelta=new Vector2(360,325);
                        var layout=go.GetComponent<VerticalLayoutGroup>(); layout.spacing=5; layout.childControlWidth=true; layout.childControlHeight=true; layout.childForceExpandHeight=false;
                        shop.listRoot=rect;
                    }
                    shop.listRowPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(RowPath).GetComponent<TMP_Text>();
                    shop.shoppingList.rectTransform.anchoredPosition=new Vector2(430,135); shop.shoppingList.rectTransform.sizeDelta=new Vector2(360,60);
                    shop.shoppingList.text=shop.definition.shoppingListTitle;
                }
                session.hud.fontSize=18;
                EditorSceneManager.SaveScene(scene,path);
            }
            AssetDatabase.SaveAssets();
            Validate(entries);
            EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath,OpenSceneMode.Single);
            Debug.Log("LEMKO_ENGLISH_HINT_MIGRATION_OK: IDs preserved by GUID; 76 pictures; three-second hints; no build.");
        }
        static void MoveAsset(string source,string target)
        {
            if(!File.Exists(source)) return;
            if(File.Exists(target)) throw new Exception("Refusing to overwrite "+target);
            var error=AssetDatabase.MoveAsset(source,target); if(!string.IsNullOrEmpty(error)) throw new Exception(error);
        }
        static void Archive(string source,string target)
        {
            if(!Directory.Exists(source) && !File.Exists(source)) return;
            string root=Path.GetFullPath(".")+Path.DirectorySeparatorChar;
            if(!Path.GetFullPath(source).StartsWith(root,StringComparison.OrdinalIgnoreCase) || !Path.GetFullPath(target).StartsWith(root,StringComparison.OrdinalIgnoreCase)) throw new Exception("Archive path escapes project");
            if(Directory.Exists(target) || File.Exists(target)) throw new Exception("Archive destination exists: "+target);
            if(Directory.Exists(source)) Directory.Move(source,target); else File.Move(source,target);
        }
        static void MakeRow()
        {
            if(File.Exists(RowPath)) return;
            var go=new GameObject("ShoppingListRow",typeof(RectTransform),typeof(TextMeshProUGUI),typeof(LayoutElement),typeof(HintPointer));
            var text=go.GetComponent<TextMeshProUGUI>(); text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset");
            text.fontSize=27; text.color=Color.white; text.alignment=TextAlignmentOptions.MidlineLeft; text.raycastTarget=true;
            var layout=go.GetComponent<LayoutElement>(); layout.preferredHeight=47; layout.minHeight=40;
            PrefabUtility.SaveAsPrefabAsset(go,RowPath); UnityEngine.Object.DestroyImmediate(go);
        }
        static void Validate(Entry[] entries)
        {
            var shop=AssetDatabase.LoadAssetAtPath<ShopDefinition>(VocabularyImporter.Root+"/Shop.asset");
            if(shop.products.Count!=47 || shop.products.Any(w=>!w.sprite || !w.id.StartsWith("food_"))) throw new Exception("Invalid active vocabulary or missing artwork");
            foreach(var entry in entries) {
                string folder=entry.slug=="carrot" ? "/Products/" : "/Placeholders/Products/";
                if(!AssetDatabase.LoadAssetAtPath<Sprite>(Sprites+folder+entry.spriteId+".png")) throw new Exception("Missing sprite "+entry.spriteId);
            }
            var json="{\"version\":1,\"encountered\":[\"shop_chleb\",\"food_bread\"],\"shopCompletions\":3}";
            var data=ProgressStore.Parse(json);
            if(data.encountered.Count!=1 || data.encountered[0]!="food_bread" || data.shopCompletions!=3) throw new Exception("Save migration failed");
            ContentValidator.Validate(); Debug.Log("LEMKO_ENGLISH_DATA_CHECKS_PASS");
        }
    }
}
