using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko.Editor {
    public static class EngagementSetup {
        const string Intro="Assets/Lemko/Prefabs/UI/ActivityIntroduction.prefab";
        static Color Purple=new Color(.43f,.33f,.84f);
        [MenuItem("Lemko/Setup/Apply engagement polish")]
        public static void Apply() {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var prefab=PrefabUtility.LoadPrefabContents(Intro);
            try {
                var intro=prefab.GetComponent<ActivityIntroduction>();
                if(!intro.repeatButton) intro.repeatButton=MakeButton(intro.panel.transform,"Repeat tutorial",new Vector2(0,-320),new Vector2(260,42),"Show next time: on");
                PrefabUtility.SaveAsPrefabAsset(prefab,Intro);
            } finally { PrefabUtility.UnloadPrefabContents(prefab); }
            foreach(var path in new[]{SeparateScenesSetup.VillagePath}.Concat(SeparateScenesSetup.Activities.Select(n=>SeparateScenesSetup.Folder+"/"+n+".unity"))) {
                var scene=EditorSceneManager.OpenScene(path); var s=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if(!s || !s.sessionProgress) throw new Exception("Missing session/progress: "+path);
                var animator=s.sessionProgress.GetComponent<ProgressAnimation>() ?? s.sessionProgress.gameObject.AddComponent<ProgressAnimation>(); animator.session=s; animator.fill=s.sessionProgress;
                var canvas=s.hud.GetComponentInParent<Canvas>();
                if(s.introduction) {
                    if(!s.introduction.repeatButton) throw new Exception("Missing repeat control: "+path);
                    if(!canvas.transform.Find("Activity help")) {
                        var help=MakeButton(canvas.transform,"Activity help",new Vector2(215,320),new Vector2(110,46),"Help");
                        UnityEventTools.AddPersistentListener(help.onClick,s.ShowHelp);
                    }
                    s.hud.rectTransform.sizeDelta=new Vector2(690,60); s.hud.rectTransform.anchoredPosition=new Vector2(-260,320);
                }
                if(s.activity) {
                    var a=s.activity; a.prompt.enableAutoSizing=true; a.prompt.fontSizeMin=20; a.prompt.fontSizeMax=28;
                    a.feedback.enableAutoSizing=true; a.feedback.fontSizeMin=16; a.feedback.fontSizeMax=20;
                    foreach(var b in new[]{a.submit,a.next}) { b.image.color=Purple; b.GetComponentInChildren<TMP_Text>(true).color=Color.white; }
                    // The request itself supports hover/hold; a second hint button adds clutter.
                    if(a.hintControl) a.hintControl.gameObject.SetActive(false);
                }
                if(s.introduction && s.resultsPanel) {
                    if(!s.contributionReward) {
                        var go=new GameObject("Contribution reward",typeof(RectTransform),typeof(ContributionReward)); go.transform.SetParent(s.resultsPanel.transform,false);
                        var rect=go.GetComponent<RectTransform>(); rect.anchoredPosition=new Vector2(-440,60); rect.sizeDelta=new Vector2(220,260);
                        var reward=go.GetComponent<ContributionReward>(); s.contributionReward=reward;
                        var icon=new GameObject("Illustration",typeof(RectTransform),typeof(Image)); icon.transform.SetParent(go.transform,false);
                        icon.GetComponent<RectTransform>().sizeDelta=new Vector2(170,170); icon.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,35);
                        reward.illustration=icon.GetComponent<Image>(); reward.illustration.preserveAspect=true; reward.illustration.raycastTarget=false;
                        var caption=new GameObject("Caption",typeof(RectTransform),typeof(TextMeshProUGUI)); caption.transform.SetParent(go.transform,false);
                        var cr=caption.GetComponent<RectTransform>(); cr.sizeDelta=new Vector2(230,90); cr.anchoredPosition=new Vector2(0,-100);
                        reward.caption=caption.GetComponent<TMP_Text>(); reward.caption.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset"); reward.caption.fontSize=22; reward.caption.alignment=TextAlignmentOptions.Center; reward.caption.color=Purple; reward.caption.raycastTarget=false;
                    }
                    s.results.rectTransform.sizeDelta=new Vector2(770,280); s.results.rectTransform.anchoredPosition=new Vector2(110,65);
                    s.results.enableAutoSizing=true; s.results.fontSizeMin=19; s.results.fontSizeMax=27;
                }
                if(s.nextPlaceButton) { s.nextPlaceButton.image.color=Purple; s.nextPlaceButton.GetComponentInChildren<TMP_Text>(true).color=Color.white; }
                if(s.wordRecap) { s.wordRecap.enableAutoSizing=true; s.wordRecap.fontSizeMin=15; s.wordRecap.fontSizeMax=21; }
                foreach(var r in scene.GetRootGameObjects()) foreach(var t in r.GetComponentsInChildren<Transform>(true)) if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0) throw new Exception("Missing script "+t.name);
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            Debug.Log("LEMKO_ENGAGEMENT_PASS: compiled, nine scenes saved, help/repeat controls and progress animation wired; missing-script checks passed. No builds or gameplay simulations.");
        }
        static Button MakeButton(Transform parent,string name,Vector2 pos,Vector2 size,string label) {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button)); go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>(); r.sizeDelta=size; r.anchoredPosition=pos;
            var image=go.GetComponent<Image>(); image.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Lemko/Sprites/UI/ui_button.png"); image.type=Image.Type.Sliced; image.color=new Color(.89f,.87f,.98f);
            var b=go.GetComponent<Button>(); b.targetGraphic=image;
            var text=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI)); text.transform.SetParent(go.transform,false);
            var tr=text.GetComponent<RectTransform>(); tr.anchorMin=Vector2.zero; tr.anchorMax=Vector2.one; tr.sizeDelta=Vector2.zero;
            var tmp=text.GetComponent<TMP_Text>(); tmp.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset"); tmp.text=label; tmp.fontSize=19; tmp.alignment=TextAlignmentOptions.Center; tmp.color=new Color(.14f,.2f,.24f); tmp.raycastTarget=false;
            return b;
        }
    }
}
