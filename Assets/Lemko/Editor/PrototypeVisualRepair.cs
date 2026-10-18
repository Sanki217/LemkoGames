using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Lemko.Editor
{
    public static class PrototypeVisualRepair
    {
        public static void Apply()
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Lemko/Sprites/Placeholders/Products/sprite_missing.png"); var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType=SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Lemko/Sprites/Placeholders/Products/sprite_missing.png"); if(!sprite) throw new Exception("Fallback sprite missing");
            foreach(var path in new[]{"Assets/Lemko/Prefabs/Words/WorldWord.prefab","Assets/Lemko/Prefabs/Words/ActivityToken.prefab"}) {
                var go=PrefabUtility.LoadPrefabContents(path); go.GetComponent<WordView>().placeholderSprite=sprite;
                var token=go.GetComponent<ActivityToken>(); if(token) { token.backing.sprite=sprite; token.caption.fontSize=36; token.caption.enableAutoSizing=true; token.caption.fontSizeMin=1; token.caption.fontSizeMax=48; token.caption.textWrappingMode=TextWrappingModes.NoWrap; }
                var speaker=go.GetComponent<WordView>().worldSpeaker; if(speaker) { var label=speaker.GetComponentInChildren<TMP_Text>(true); label.fontSize=3; label.enableAutoSizing=true; label.fontSizeMin=.5f; label.fontSizeMax=3; label.textWrappingMode=TextWrappingModes.NoWrap; }
                PrefabUtility.SaveAsPrefabAsset(go,path); PrefabUtility.UnloadPrefabContents(go);
            }
            foreach(ActivityKind kind in Enum.GetValues(typeof(ActivityKind))) {
                var scene=EditorSceneManager.OpenScene(SeparateScenesSetup.Folder+"/"+kind+".unity",OpenSceneMode.Single);
                var a=UnityEngine.Object.FindFirstObjectByType<LearningActivity>(FindObjectsInactive.Include);
                if(a is FamilyTreeController) a.contentScale=.88f;
                a.worldRoot.Find("Editable background").GetComponent<SpriteRenderer>().sprite=sprite;
                // Remove the inherited village artwork from activity scenes; the Village scene is untouched.
                foreach(var root in scene.GetRootGameObjects()) if(root.name=="Placeholder Village Background") UnityEngine.Object.DestroyImmediate(root);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath,OpenSceneMode.Single); Debug.Log("LEMKO_PROTOTYPE_VISUAL_REPAIR_PASS");
        }
    }
}
