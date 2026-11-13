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
        [MenuItem("Lemko/Setup/Add game menu")]
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
                if(!s.menu.close || !s.menu.language || !s.menu.motion || !s.menu.audioButton || !s.menu.session) throw new Exception("Menu wiring incomplete: "+path);
                foreach(var root in scene.GetRootGameObjects()) foreach(var t in root.GetComponentsInChildren<Transform>(true))
                    if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0) throw new Exception("Missing script: "+t.name);
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            Debug.Log("LEMKO_GAME_QUALITY_WIRING_PASS: nine scenes, shared menu prefab, presentation asset, 0.5s + 1s hint timing. No play simulations or builds.");
        }
        static void CreateMenu() {
            Directory.CreateDirectory(Path.GetDirectoryName(Prefab)); AssetDatabase.Refresh();
            var root=Rect(null,"Game menu",new Vector2(1280,720),Vector2.zero); var menu=root.gameObject.AddComponent<GameMenu>();
            var shade=Rect(root,"Modal input blocker",new Vector2(1280,720),Vector2.zero);
            shade.anchorMin=Vector2.zero; shade.anchorMax=Vector2.one; shade.sizeDelta=Vector2.zero;
            shade.gameObject.AddComponent<Image>().color=new Color(0,0,0,.65f); menu.panel=shade.gameObject;
            var panel=Rect(shade,"Menu card",new Vector2(880,220),Vector2.zero); panel.gameObject.AddComponent<Image>().color=style.panelColor;
            Text(panel,"Title","Session settings",new Vector2(520,45),new Vector2(0,58),30);
            menu.close=Button(panel,"Resume",new Vector2(140,42),new Vector2(345,58));
            menu.audioButton=Button(panel,"Audio",new Vector2(250,44),new Vector2(-290,-38));
            menu.language=Button(panel,"Hint language",new Vector2(250,44),new Vector2(0,-38));
            menu.motion=Button(panel,"Motion",new Vector2(250,44),new Vector2(290,-38)); menu.motionLabel=menu.motion.GetComponentInChildren<TMP_Text>();
            shade.gameObject.SetActive(false);
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
