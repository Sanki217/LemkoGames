using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko.Editor {
    public static class ActivityIntroductionSetup {
        const string Folder="Assets/Lemko/Data/Introductions";
        const string Prefab="Assets/Lemko/Prefabs/UI/ActivityIntroduction.prefab";
        static readonly Color Ink=new Color(.14f,.2f,.24f),Purple=new Color(.43f,.33f,.84f);
        static TMP_FontAsset Font=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset");
        [MenuItem("Lemko/Setup/Add activity introduction screens")]
        public static void Apply() {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            if(!File.Exists(Prefab)) MakePrefab();
            string[] titles={"Gather the supplies","Invite the family","Look after the animals","Make the house welcoming","Pack for the gathering","Prepare the tickets","Finish the labels","Plan a day together"};
            string[] stories={
                "The village is preparing a shared meal. Help collect everything on the shopping list.",
                "A gathering feels better with family around. Work out who is being invited, using the person marked 'you' as your reference.",
                "Before the neighbours meet, the farm animals need your attention. Help prepare their pens.",
                "Our neighbours are making space for the gathering. Put the requested furniture in the right room.",
                "We have pictures and Lemko words for the supplies. Pair them up to help everyone pack.",
                "The gathering needs numbered tickets. Read or listen to each request and choose the right number.",
                "Some labels still need to be written or repaired. Help spell the words in Lemko.",
                "Our neighbours have a day out in mind. Match the weather and temperature to each request."};
            string[] instructions={
                "1. Read the Lemko shopping list.\n2. Click or tap products to add them to your basket. Tap again to remove.\n3. Choose Check basket when everything is ready.",
                "1. Find the person marked 'you'.\n2. Read the requested relative in Lemko.\n3. Select a matching person. Some questions accept more than one person.",
                "1. Read the requested animal and quantity.\n2. Drag animals into the pen, or tap to add them. Tap placed animals to remove them.\n3. Repair the pen if needed, then choose Check answer.",
                "1. Read the furniture and room request. Use the arrows to change rooms.\n2. Drag furniture into the room. Drag to move it; tap to remove it.\n3. Choose Check answer from any room — the saved placement is what matters.",
                "1. Turn over two cards: match a picture with its Lemko word.\n2. Keep matching until the board is cleared. Mismatches do not cost lives.\n3. Finish the short picture-to-word checks afterwards.",
                "1. Read the Lemko number, or play its recording when available.\n2. Select the matching numeral to issue a ticket.\n3. Finish the session to unlock animal quantities at the Farm.",
                "1. Look at the picture and its meaning. Hold either for the Lemko spelling.\n2. Click or drag letters into the slots. Tap a filled slot to return its letter.\n3. Finish or repair the word, then choose Check answer.",
                "1. Read the weather request.\n2. Select the weather and move the temperature slider.\n3. Choose Check answer. Adjust your choices until the outing is ready."};
            foreach(var name in SeparateScenesSetup.Activities) {
                int i=Array.IndexOf(SeparateScenesSetup.Activities,name); string path=Folder+"/"+name+".asset";
                var data=AssetDatabase.LoadAssetAtPath<ActivityIntroductionDefinition>(path);
                var scene=EditorSceneManager.OpenScene(SeparateScenesSetup.Folder+"/"+name+".unity");
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if(!data) {
                    data=ScriptableObject.CreateInstance<ActivityIntroductionDefinition>(); data.activityId=name; data.title=titles[i]; data.story=stories[i]; data.instructions=instructions[i];
                    if(name=="Farm") data.beforeNumbers="1. Read the animal name.\n2. Drag or tap the matching animal into the pen. Another choice replaces it.\n3. Choose Check answer. Quantities become available after a Numbers session.";
                    var contribution=session.story ? session.story.Find(name) : null; data.illustration=contribution?.illustration;
                    if(!data.illustration && session.activity && session.activity.definition.pool.Count>0) data.illustration=session.activity.definition.pool[0].sprite;
                    AssetDatabase.CreateAsset(data,path);
                }
                if(!session.introduction) {
                    var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab),session.hud.GetComponentInParent<Canvas>().transform);
                    session.introduction=instance.GetComponent<ActivityIntroduction>(); session.introduction.definition=data;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(session.introduction);
                }
                var intro=session.introduction;
                if(!intro.definition || !intro.panel || !intro.title || !intro.instructions || !intro.support || !intro.illustration) throw new Exception("Introduction reference missing: "+name);
                if(intro.definition.activityId!=name) throw new Exception("Wrong introduction: "+name);
                foreach(var root in scene.GetRootGameObjects()) foreach(var t in root.GetComponentsInChildren<Transform>(true)) if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0) throw new Exception("Missing script: "+t.name);
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            Debug.Log("LEMKO_ACTIVITY_INTRODUCTIONS_PASS: eight entry screens and editable definitions, shared prefab, references checked. No build or gameplay simulation.");
        }
        static void MakePrefab() {
            var root=Rect(null,"Activity introduction",new Vector2(1280,720),Vector2.zero);
            var intro=root.gameObject.AddComponent<ActivityIntroduction>();
            var overlay=Rect(root,"Click or tap anywhere",Vector2.zero,Vector2.zero); overlay.anchorMin=Vector2.zero; overlay.anchorMax=Vector2.one;
            overlay.gameObject.AddComponent<Image>().color=new Color(.956f,.969f,.973f); intro.panel=overlay.gameObject;
            var card=Rect(overlay,"Introduction card",new Vector2(1120,590),Vector2.zero).gameObject.AddComponent<Image>();
            card.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Lemko/Sprites/UI/ui_card.png"); card.type=Image.Type.Sliced; card.raycastTarget=false;
            Text(card.transform,"Eyebrow","A GATHERING FOR THE VILLAGE",new Vector2(990,28),new Vector2(0,242),17).color=Purple;
            intro.title=Text(card.transform,"Activity title","Your next adventure",new Vector2(990,58),new Vector2(0,192),35); intro.title.fontStyle=FontStyles.Bold;
            intro.story=Text(card.transform,"Story","",new Vector2(950,85),new Vector2(0,115),22);
            var illustration=Rect(card.transform,"Illustration",new Vector2(145,145),new Vector2(-405,-25)).gameObject.AddComponent<Image>(); illustration.preserveAspect=true; illustration.raycastTarget=false; intro.illustration=illustration;
            intro.instructions=Text(card.transform,"How to play","",new Vector2(740,192),new Vector2(105,-29),22); intro.instructions.alignment=TextAlignmentOptions.MidlineLeft;
            intro.instructions.enableAutoSizing=true; intro.instructions.fontSizeMin=18; intro.instructions.fontSizeMax=22;
            intro.support=Text(card.transform,"Learning support","",new Vector2(990,55),new Vector2(0,-168),17); intro.support.color=new Color(.38f,.44f,.49f);
            var cue=Text(card.transform,"Continue cue","Click or tap anywhere to play",new Vector2(990,45),new Vector2(0,-239),23); cue.color=Purple; cue.fontStyle=FontStyles.Bold;
            intro.panel.SetActive(false); PrefabUtility.SaveAsPrefabAsset(root.gameObject,Prefab); UnityEngine.Object.DestroyImmediate(root.gameObject);
        }
        static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 pos) { var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); if(parent) r.SetParent(parent,false); r.sizeDelta=size; r.anchoredPosition=pos; return r; }
        static TMP_Text Text(Transform parent,string name,string value,Vector2 size,Vector2 pos,int fontSize) { var t=Rect(parent,name,size,pos).gameObject.AddComponent<TextMeshProUGUI>(); t.font=Font; t.text=value; t.fontSize=fontSize; t.alignment=TextAlignmentOptions.Center; t.color=Ink; t.raycastTarget=false; return t; }
    }
}
