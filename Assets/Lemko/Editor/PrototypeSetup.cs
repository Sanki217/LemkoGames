using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace Lemko.Editor
{
    public static class PrototypeSetup
    {
        const string Prefab="Assets/Lemko/Prefabs/Words/ActivityToken.prefab";
        static TMP_FontAsset Font=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset");
        [MenuItem("Lemko/Setup/Complete prototype scenes (preserve existing gameplay)")]
        public static void Apply()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach(var path in Directory.GetFiles("Assets/Lemko/Sprites/Placeholders/Activities","*.png")) {
                var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/')); importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single; importer.spritePixelsPerUnit=256; importer.alphaIsTransparency=true; importer.mipmapEnabled=false; importer.SaveAndReimport();
            }
            ActivityCsvImporter.Import(); MakeToken();
            Directory.CreateDirectory("Docs/Backups/BeforeFullPrototype");
            foreach(ActivityKind kind in Enum.GetValues(typeof(ActivityKind))) {
                string path=SeparateScenesSetup.Folder+"/"+kind+".unity";
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                if(UnityEngine.Object.FindFirstObjectByType<LearningActivity>(FindObjectsInactive.Include)) continue;
                string backup="Docs/Backups/BeforeFullPrototype/"+kind+".unity.txt"; if(!File.Exists(backup)) File.Copy(path,backup);
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if(session.villagePanel) UnityEngine.Object.DestroyImmediate(session.villagePanel); session.placeholderActivity="";
                var canvas=session.hud.transform.parent;
                var panel=Panel(canvas,"Activity UI",Color.clear); panel.GetComponent<Image>().raycastTarget=false;
                var type=typeof(LearningActivity).Assembly.GetType("Lemko."+kind+"Controller");
                var controller=(LearningActivity)panel.AddComponent(type); session.activity=controller; session.shopPanel=panel;
                if(controller is FamilyTreeController) controller.contentScale=.88f;
                controller.definition=AssetDatabase.LoadAssetAtPath<ActivityDefinition>(ActivityCsvImporter.Root+"/"+kind+".asset");
                controller.tokenPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab).GetComponent<ActivityToken>();
                controller.worldRoot=new GameObject(kind+" World").transform; controller.taskRoot=new GameObject("Task Objects").transform; controller.taskRoot.SetParent(controller.worldRoot,false);
                var bg=new GameObject("Editable background",typeof(SpriteRenderer)); bg.transform.SetParent(controller.worldRoot,false); bg.transform.localScale=new Vector3(15.8f,7.3f,1); bg.transform.localPosition=new Vector3(0,-.6f,3);
                var r=bg.GetComponent<SpriteRenderer>(); r.sprite=controller.tokenPrefab.backing.sprite; r.color=new Color(.19f,.32f,.33f); r.sortingOrder=-10;
                controller.prompt=Text(panel.transform,"Prompt",kind.ToString(),new Vector2(1160,70),new Vector2(0,225),29); controller.prompt.raycastTarget=true; controller.prompt.gameObject.AddComponent<HintPointer>();
                controller.feedback=Text(panel.transform,"Feedback","",new Vector2(1100,60),new Vector2(0,-220),20);
                controller.submit=Button(panel.transform,"Submit",new Vector2(180,48),new Vector2(350,-278),null);
                controller.next=Button(panel.transform,"Next",new Vector2(180,48),new Vector2(550,-278),null);
                controller.speak=Button(panel.transform,"Pronunciation",new Vector2(210,48),new Vector2(-475,-278),null);
                if(controller is HouseController house) {
                    house.previousRoom=Button(panel.transform,"< Room",new Vector2(150,45),new Vector2(-370,155),null); house.nextRoom=Button(panel.transform,"Room >",new Vector2(150,45),new Vector2(370,155),null);
                    house.roomLabel=Text(panel.transform,"Room name","",new Vector2(550,45),new Vector2(0,155),23); house.roomLabel.raycastTarget=true; house.roomLabel.gameObject.AddComponent<HintPointer>();
                }
                if(controller is WeatherController weather) {
                    var rail=Rect(panel.transform,"Temperature",new Vector2(400,30),new Vector2(100,-150)); rail.gameObject.AddComponent<Image>().color=Color.gray;
                    var handle=Rect(rail,"Handle",new Vector2(28,38),Vector2.zero); handle.gameObject.AddComponent<Image>().color=Color.white;
                    weather.temperature=rail.gameObject.AddComponent<Slider>(); weather.temperature.handleRect=handle; weather.temperature.targetGraphic=handle.GetComponent<Image>();
                    weather.temperatureLabel=Text(panel.transform,"Degrees","",new Vector2(160,40),new Vector2(400,-150),23);
                }
                session.resultsPanel=Panel(canvas,"Results",new Color(.2f,.33f,.28f)); session.results=Text(session.resultsPanel.transform,"Result text","",new Vector2(1100,280),new Vector2(0,55),32);
                Button(session.resultsPanel.transform,"Play again",new Vector2(250,60),new Vector2(-160,-170),session.RestartActivity); Button(session.resultsPanel.transform,"Village",new Vector2(250,60),new Vector2(160,-170),session.Village);
                session.exhaustedPanel=Panel(canvas,"Zero lives",new Color(.3f,.2f,.2f)); Text(session.exhaustedPanel.transform,"Message","No lives left. Start fresh or keep learning in practice.",new Vector2(1100,180),new Vector2(0,80),30);
                Button(session.exhaustedPanel.transform,"Fresh attempt",new Vector2(300,60),new Vector2(-180,-100),session.FreshAttempt); Button(session.exhaustedPanel.transform,"Unlimited practice",new Vector2(300,60),new Vector2(180,-100),session.StartPractice);
                session.resultsPanel.SetActive(false); session.exhaustedPanel.SetActive(false); session.tooltip.box.SetAsLastSibling();
                EditorSceneManager.SaveScene(scene,path);
            }
            var village=EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath,OpenSceneMode.Single);
            var s=UnityEngine.Object.FindFirstObjectByType<GameSession>(); foreach(var button in s.villagePanel.GetComponentsInChildren<Button>(true)) { var text=button.GetComponentInChildren<TMP_Text>(); text.text=text.text.Replace("Preview scene","Enter"); }
            s.villagePanel.transform.Find("Subtitle").GetComponent<TMP_Text>().text="Eight places to learn - playable prototype";
            EditorSceneManager.SaveScene(village); AssetDatabase.SaveAssets(); ContentValidator.Validate();
            Debug.Log("LEMKO_FULL_PROTOTYPE_SETUP_PASS: seven activity scenes upgraded; Shop preserved; no builds.");
        }
        static void MakeToken()
        {
            if(File.Exists(Prefab)) return;
            var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Lemko/Prefabs/Words/WorldWord.prefab")); PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            go.name="ActivityToken"; var token=go.AddComponent<ActivityToken>(); token.view=go.GetComponent<WordView>();
            var bg=new GameObject("Backing",typeof(SpriteRenderer)); bg.transform.SetParent(go.transform,false); bg.transform.localScale=new Vector3(1.8f,1.65f,1);
            token.view.placeholderSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Lemko/Sprites/Placeholders/Products/sprite_missing.png");
            token.backing=bg.GetComponent<SpriteRenderer>(); token.backing.sprite=token.view.placeholderSprite; token.backing.color=new Color(.96f,.91f,.76f); token.backing.sortingOrder=0;
            var label=new GameObject("Caption",typeof(TextMeshPro)); label.transform.SetParent(go.transform,false); token.caption=label.GetComponent<TextMeshPro>(); token.caption.font=Font; token.caption.fontSize=36; token.caption.enableAutoSizing=true; token.caption.fontSizeMin=1; token.caption.textWrappingMode=TextWrappingModes.NoWrap; token.caption.fontSizeMax=48; token.caption.color=new Color(.1f,.2f,.2f); token.caption.alignment=TextAlignmentOptions.Center; token.caption.rectTransform.sizeDelta=new Vector2(1.7f,1.5f); token.caption.GetComponent<MeshRenderer>().sortingOrder=4;
            PrefabUtility.SaveAsPrefabAsset(go,Prefab); UnityEngine.Object.DestroyImmediate(go);
        }
        static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 position) { var g=new GameObject(name,typeof(RectTransform)); var r=(RectTransform)g.transform; r.SetParent(parent,false); r.sizeDelta=size; r.anchoredPosition=position; return r; }
        static GameObject Panel(Transform parent,string name,Color color) { var r=Rect(parent,name,new Vector2(1280,630),new Vector2(0,-40)); r.gameObject.AddComponent<Image>().color=color; return r.gameObject; }
        static TMP_Text Text(Transform parent,string name,string value,Vector2 size,Vector2 position,int fontSize) { var r=Rect(parent,name,size,position); var t=r.gameObject.AddComponent<TextMeshProUGUI>(); t.font=Font; t.text=value; t.fontSize=fontSize; t.color=Color.white; t.alignment=TextAlignmentOptions.Center; t.raycastTarget=false; return t; }
        static Button Button(Transform parent,string label,Vector2 size,Vector2 position,UnityAction action) { var r=Rect(parent,label,size,position); r.gameObject.AddComponent<Image>().color=new Color(.38f,.54f,.4f); var b=r.gameObject.AddComponent<Button>(); Text(r,"Label",label,size,Vector2.zero,22); if(action!=null) UnityEventTools.AddPersistentListener(b.onClick,action); return b; }
    }
}

