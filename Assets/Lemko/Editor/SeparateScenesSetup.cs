using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko.Editor
{
    public static class SeparateScenesSetup
    {
        public static readonly string[] Activities={"Shop","FamilyTree","Farm","House","Memory","Numbers","Alphabet","Weather"};
        public const string Folder="Assets/Lemko/Scenes/Activities";
        public const string VillagePath="Assets/Lemko/Scenes/Village.unity";
        [MenuItem("Lemko/Setup/Create separate activity scenes (preserve existing)")]
        public static void Create()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            var shop=AssetDatabase.LoadAssetAtPath<ShopDefinition>(VocabularyImporter.Root+"/Shop.asset");
            shop.displayedProductCount=8; shop.shoppingListTitle="Што треба купити:"; EditorUtility.SetDirty(shop);
            VocabularyImporter.Import("Content/Shop");
            if(!File.Exists(VillagePath)) MakeVillage();
            string shopPath=Folder+"/Shop.unity";
            if(!File.Exists(shopPath)) MakeShop(shopPath);
            foreach(var name in Activities.Skip(1)) {
                string path=Folder+"/"+name+".unity";
                if(File.Exists(path)) continue;
                var scene=EditorSceneManager.OpenScene(VillagePath,OpenSceneMode.Single);
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                session.placeholderActivity=name=="FamilyTree" ? "Family Tree" : name;
                foreach(var button in session.villagePanel.GetComponentsInChildren<Button>(true)) UnityEngine.Object.DestroyImmediate(button.gameObject);
                session.villagePanel.transform.Find("Title").GetComponent<TMP_Text>().text=session.placeholderActivity.ToUpperInvariant();
                session.villagePanel.transform.Find("Subtitle").GetComponent<TMP_Text>().text="Activity scene ready - gameplay coming later";
                session.villageStatus.text=session.placeholderActivity+"\nUse Village to return.";
                EditorSceneManager.SaveScene(scene,path);
            }
            // Scene registration enables runtime navigation; this does not build a player.
            var paths=new[]{VillagePath}.Concat(Activities.Select(n=>Folder+"/"+n+".unity")).ToArray();
            var old=EditorBuildSettings.scenes;
            EditorBuildSettings.scenes=paths.Select(p=>new EditorBuildSettingsScene(p,true))
                .Concat(old.Where(s=>!paths.Contains(s.path))).ToArray();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(VillagePath,OpenSceneMode.Single);
            ValidateData();
            ContentValidator.Validate();
            Debug.Log("LEMKO_SEPARATE_SCENES_OK: Village + eight activity scenes registered; no builds.");
        }
        static void Prepare(GameSession session)
        {
            session.useSeparateScenes=true; session.placeholderActivity="";
            var smoke=session.GetComponent<WorldShopSmoke>(); if(smoke) UnityEngine.Object.DestroyImmediate(smoke);
            var legacy=session.GetComponent<MilestoneSmoke>(); if(legacy) UnityEngine.Object.DestroyImmediate(legacy);
            session.hud.rectTransform.sizeDelta=new Vector2(660,65);
            session.hud.rectTransform.anchoredPosition=new Vector2(-255,320);
            session.hud.fontSize=20;
        }
        static void MakeVillage()
        {
            var scene=EditorSceneManager.OpenScene(WorldShopSetup.ScenePath,OpenSceneMode.Single);
            var session=UnityEngine.Object.FindFirstObjectByType<GameSession>(); Prepare(session);
            if(session.shopController.worldRoot) UnityEngine.Object.DestroyImmediate(session.shopController.worldRoot.gameObject);
            UnityEngine.Object.DestroyImmediate(session.shopPanel); UnityEngine.Object.DestroyImmediate(session.resultsPanel); UnityEngine.Object.DestroyImmediate(session.exhaustedPanel);
            var buttons=session.villagePanel.GetComponentsInChildren<Button>(true);
            if(buttons.Length!=8) throw new Exception("Village must have eight location buttons.");
            for(int i=0;i<8;i++) {
                var button=buttons[i]; button.onClick=new Button.ButtonClickedEvent(); button.interactable=true;
                UnityEventTools.AddStringPersistentListener(button.onClick,session.OpenActivity,Activities[i]);
                button.GetComponentInChildren<TMP_Text>().text=(i==1 ? "Family Tree" : Activities[i])+ (i==0 ? "\nEnter" : "\nPreview scene");
            }
            session.villagePanel.SetActive(true);
            EditorSceneManager.SaveScene(scene,VillagePath);
        }
        static void MakeShop(string path)
        {
            var scene=EditorSceneManager.OpenScene(WorldShopSetup.ScenePath,OpenSceneMode.Single);
            var session=UnityEngine.Object.FindFirstObjectByType<GameSession>(); Prepare(session);
            UnityEngine.Object.DestroyImmediate(session.villagePanel);
            var background=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="Placeholder Village Background");
            if(background) UnityEngine.Object.DestroyImmediate(background);
            session.shopPanel.SetActive(true); session.shopController.worldRoot.gameObject.SetActive(true);
            EditorSceneManager.SaveScene(scene,path);
        }
        public static void ValidateData()
        {
            var score=new ShopScore();
            Check(score.Apply(SelectionResult.Correct)==100 && score.Points==100,"first correct");
            Check(score.Apply(SelectionResult.Correct)==200 && score.Points==300,"second correct multiplier");
            Check(score.Apply(SelectionResult.Ignored)==0 && score.Points==300 && score.Streak==2,"ignored duplicate");
            score.Apply(SelectionResult.Incorrect); Check(score.Points==300 && score.NextMultiplier==1,"wrong resets multiplier only");
            Check(score.Apply(SelectionResult.Correct)==100 && score.Points==400,"streak restart");
            var old=ProgressStore.Parse("{\"version\":1,\"shopCompletions\":2}"); Check(old.bestShopPoints==0 && old.shopCompletions==2,"old save compatibility");
            var shop=AssetDatabase.LoadAssetAtPath<ShopDefinition>(VocabularyImporter.Root+"/Shop.asset");
            Check(shop.Validate()==null && shop.products.Count==47,"47 word pool");
            Check(shop.products.All(w=>w.id.StartsWith("shop_") && !w.lemko.Contains("[DEMO]")),"real source content");
            var paths=new[]{VillagePath}.Concat(Activities.Select(n=>Folder+"/"+n+".unity"));
            foreach(var path in paths) {
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                var sessions=UnityEngine.Object.FindObjectsByType<GameSession>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                Check(sessions.Length==1 && sessions[0].useSeparateScenes,"one scene session: "+path);
                foreach(var root in scene.GetRootGameObjects()) foreach(var t in root.GetComponentsInChildren<Transform>(true))
                    Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"missing script "+t.name);
            }
            EditorSceneManager.OpenScene(VillagePath,OpenSceneMode.Single);
            Debug.Log("LEMKO_SCORE_DATA_SCENES_CHECKS_PASS");
        }
        static void Check(bool condition,string message) { if(!condition) throw new Exception(message); }
    }
}
