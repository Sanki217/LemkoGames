using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko.Editor
{
    public static class GameQualitySetup
    {
        const string Prefab="Assets/Lemko/Prefabs/UI/GameMenu.prefab";
        const string Theme="Assets/Lemko/Data/GamePresentation.asset";
        static TMP_FontAsset Font=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset");
        static GamePresentation style;
        [MenuItem("Lemko/Setup/Add game menu and save slots")]
        public static void Apply() {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            style=AssetDatabase.LoadAssetAtPath<GamePresentation>(Theme);
            if(!style) { style=ScriptableObject.CreateInstance<GamePresentation>(); AssetDatabase.CreateAsset(style,Theme); }
            if(!File.Exists(Prefab)) CreateMenu();
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            foreach(var path in new[]{SeparateScenesSetup.VillagePath}.Concat(SeparateScenesSetup.Activities.Select(n=>SeparateScenesSetup.Folder+"/"+n+".unity"))) {
                var scene=EditorSceneManager.OpenScene(path); var s=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if(!s) throw new Exception("Session missing: "+path);
                if(!s.presentation) s.presentation=style;
                if(!s.menu) {
                    var canvas=s.hud.GetComponentInParent<Canvas>();
                    var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,canvas.transform);
                    s.menu=instance.GetComponent<GameMenu>(); s.menu.session=s;
                    var menuButton=Button(canvas.transform,"Menu",new Vector2(145,44),new Vector2(5,325));
                    UnityEventTools.AddPersistentListener(menuButton.onClick,s.menu.Open);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(s.menu);
                }
                s.tooltip.stillSeconds=.5f; s.tooltip.holdSeconds=1f; s.tooltip.movementTolerance=4;
                if(s.tooltip.waiting) {
                    var cue=s.tooltip.waiting.GetComponentInChildren<TMP_Text>(true); if(cue) cue.text="Hint";
                }
                foreach(var root in scene.GetRootGameObjects()) foreach(var t in root.GetComponentsInChildren<TMP_Text>(true)) {
                    t.text=t.text.Replace("Hold for hint (3s)","Hold for hint").Replace("Hold translation (3s)","Hold for hint").Replace("3 seconds","a moment").Replace("(3s)","");
                }
                if(s.journal) {
                    // Keep the journal shortcut clear of the village introduction.
                    foreach(var b in s.villagePanel.GetComponentsInChildren<Button>(true))
                        for(int i=0;i<b.onClick.GetPersistentEventCount();i++) if(b.onClick.GetPersistentTarget(i)==s.journal && b.onClick.GetPersistentMethodName(i)=="Open")
                            b.GetComponent<RectTransform>().anchoredPosition=new Vector2(-500,-205);
                }
                if(s.menu.slots.Length!=3 || !s.menu.confirmation || !s.menu.audioButton || !s.menu.session) throw new Exception("Menu wiring incomplete: "+path);
                foreach(var root in scene.GetRootGameObjects()) foreach(var t in root.GetComponentsInChildren<Transform>(true))
                    if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0) throw new Exception("Missing script: "+t.name);
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            Debug.Log("LEMKO_GAME_QUALITY_WIRING_PASS: nine scenes, shared menu prefab, three slots, presentation asset, 0.5s + 1s hint timing. No play simulations or builds.");
        }
        public static void CheckSaveData() {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-lemkoSmoke")<0) throw new Exception("Save checks require isolated smoke keys");
            string[] keys={"Lemko.Manual.Smoke.0","Lemko.Manual.Smoke.Recovery"};
            var existed=keys.Select(PlayerPrefs.HasKey).ToArray(); var previous=keys.Select(k=>PlayerPrefs.GetString(k)).ToArray();
            try {
                if(ProgressStore.TryParse("{broken",out _) || ProgressStore.TryParse("{\"version\":99}",out _)) throw new Exception("Invalid save accepted");
                if(!ProgressStore.TryParse("{\"version\":1,\"shopCompletions\":2}",out var old) || old.shopCompletions!=2) throw new Exception("Legacy save migration failed");
                old.reducedMotion=true; old.activities.Add(new ActivityResult{id="Numbers",completions=1});
                if(!ManualProgressStore.Save(0,old,out var error)) throw new Exception(error);
                old.shopCompletions=42;
                if(!ManualProgressStore.Load(0,out var restored,out error) || restored.shopCompletions!=2 || !restored.reducedMotion || restored.activities[0].completions!=1) throw new Exception("Checkpoint round trip failed: "+error);
                if(!ManualProgressStore.ProtectCurrent(old,out error) || !ManualProgressStore.Recover(out restored,out error) || restored.shopCompletions!=42) throw new Exception("Recovery round trip failed");
                PlayerPrefs.SetString(keys[0],"unreadable");
                if(ManualProgressStore.Load(0,out _,out _)) throw new Exception("Corrupt slot accepted");
                Debug.Log("LEMKO_MANUAL_SAVE_DATA_PASS: isolated checkpoint/recovery round trips, legacy migration and invalid data rejection. No gameplay simulation.");
            } finally {
                for(int i=0;i<keys.Length;i++) { if(existed[i]) PlayerPrefs.SetString(keys[i],previous[i]); else PlayerPrefs.DeleteKey(keys[i]); }
                PlayerPrefs.Save();
            }
        }
        static void CreateMenu() {
            Directory.CreateDirectory(Path.GetDirectoryName(Prefab)); AssetDatabase.Refresh();
            var root=Rect(null,"Game menu",new Vector2(1280,720),Vector2.zero); var menu=root.gameObject.AddComponent<GameMenu>();
            var shade=Rect(root,"Modal input blocker",new Vector2(1280,720),Vector2.zero);
            shade.anchorMin=Vector2.zero; shade.anchorMax=Vector2.one; shade.sizeDelta=Vector2.zero;
            shade.gameObject.AddComponent<Image>().color=new Color(0,0,0,.65f); menu.panel=shade.gameObject;
            var panel=Rect(shade,"Menu card",new Vector2(1160,640),Vector2.zero); panel.gameObject.AddComponent<Image>().color=style.panelColor;
            Text(panel,"Title","Your village • save & settings",new Vector2(900,45),new Vector2(-50,278),30);
            Text(panel,"Scope","Save learning progress, results and preferences. Loading returns to the village.",new Vector2(1080,35),new Vector2(0,229),18);
            menu.close=Button(panel,"Resume",new Vector2(140,42),new Vector2(480,278));
            menu.slots=new SaveSlotView[3];
            for(int i=0;i<3;i++) {
                float y=155-i*95;
                var row=Rect(panel,"Slot "+(i+1),new Vector2(1080,82),new Vector2(0,y)); row.gameObject.AddComponent<Image>().color=new Color(.15f,.27f,.23f);
                var title=Text(row,"Slot name","Slot "+(i+1),new Vector2(120,38),new Vector2(-455,0),22); title.color=style.accentColor;
                var description=Text(row,"Description","Empty slot",new Vector2(510,65),new Vector2(-130,0),18); description.alignment=TextAlignmentOptions.MidlineLeft;
                menu.slots[i]=new SaveSlotView{description=description,save=Button(row,"Save",new Vector2(135,44),new Vector2(280,0)),load=Button(row,"Load",new Vector2(135,44),new Vector2(440,0))};
            }
            menu.message=Text(panel,"Status","",new Vector2(1080,60),new Vector2(0,-115),19);
            menu.audioButton=Button(panel,"Audio",new Vector2(250,44),new Vector2(-290,-186));
            menu.language=Button(panel,"Hint language",new Vector2(250,44),new Vector2(0,-186));
            menu.motion=Button(panel,"Motion",new Vector2(250,44),new Vector2(290,-186)); menu.motionLabel=menu.motion.GetComponentInChildren<TMP_Text>();
            menu.recovery=Button(panel,"Recover progress before last load",new Vector2(580,40),new Vector2(0,-245));
            Text(panel,"Save policy","Local to this device/browser. Rounds and challenge lives are not saved; loading starts a fresh attempt.",new Vector2(1080,40),new Vector2(0,-290),16);
            var confirm=Rect(shade,"Confirm progress replacement",new Vector2(1160,640),Vector2.zero); confirm.gameObject.AddComponent<Image>().color=style.panelColor; menu.confirmation=confirm.gameObject;
            menu.confirmationText=Text(confirm,"Question","",new Vector2(970,150),new Vector2(0,50),25);
            menu.confirm=Button(confirm,"Confirm",new Vector2(250,52),new Vector2(-150,-100));
            menu.cancel=Button(confirm,"Cancel",new Vector2(250,52),new Vector2(150,-100));
            confirm.gameObject.SetActive(false); shade.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject,Prefab); UnityEngine.Object.DestroyImmediate(root.gameObject);
        }
        static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 pos) {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); if(parent) r.SetParent(parent,false); r.sizeDelta=size; r.anchoredPosition=pos; return r;
        }
        static TMP_Text Text(Transform parent,string name,string value,Vector2 size,Vector2 pos,int fontSize) {
            var t=Rect(parent,name,size,pos).gameObject.AddComponent<TextMeshProUGUI>(); t.font=Font; t.fontSize=fontSize; t.color=new Color(.95f,.96f,.88f); t.alignment=TextAlignmentOptions.Center; t.raycastTarget=false; t.text=value; return t;
        }
        static Button Button(Transform parent,string name,Vector2 size,Vector2 pos) {
            var r=Rect(parent,name,size,pos); var image=r.gameObject.AddComponent<Image>(); image.color=style.buttonColor;
            var b=r.gameObject.AddComponent<Button>(); var colors=b.colors; colors.highlightedColor=new Color(1.2f,1.2f,1.05f); colors.pressedColor=new Color(.8f,.85f,.75f); colors.selectedColor=colors.highlightedColor; colors.fadeDuration=.12f; b.colors=colors;
            Text(r,"Label",name,size-Vector2.one*8,Vector2.zero,20); return b;
        }
    }
}
