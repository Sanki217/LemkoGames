using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko.Editor
{
    public static class WeatherLearningModeSetup
    {
        [MenuItem("Lemko/Setup/Add learning mode and outing preview")]
        public static void Apply() {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach(var path in new[]{SeparateScenesSetup.VillagePath}.Concat(SeparateScenesSetup.Activities.Select(n=>SeparateScenesSetup.Folder+"/"+n+".unity"))) {
                var scene=EditorSceneManager.OpenScene(path); var session=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if(!session) throw new Exception("Session missing: "+path);
                if(scene.name=="Village") {
                    if(!session.modeButton) {
                        var go=new GameObject("Learning mode",typeof(RectTransform),typeof(Image),typeof(Button)); var rect=go.GetComponent<RectTransform>();
                        rect.SetParent(session.villagePanel.transform,false); rect.sizeDelta=new Vector2(250,38); rect.anchoredPosition=new Vector2(460,160);
                        go.GetComponent<Image>().color=new Color(.30f,.45f,.32f); session.modeButton=go.GetComponent<Button>();
                        Text(rect,"Label",rect.sizeDelta,Vector2.zero,19).text="Start challenge";
                        UnityEventTools.AddPersistentListener(session.modeButton.onClick,session.ToggleLearningMode);
                    }
                    if(!session.modeDescription) session.modeDescription=Text(session.villagePanel.transform,"Mode description",new Vector2(780,36),new Vector2(-130,160),17);
                    session.modeDescription.text="Learning: mistakes never block play."; session.modeDescription.color=new Color(.1f,.23f,.2f);
                }
                if(session.exhaustedPanel) {
                    foreach(var button in session.exhaustedPanel.GetComponentsInChildren<Button>(true)) {
                        for(int i=0;i<button.onClick.GetPersistentEventCount();i++) {
                            string method=button.onClick.GetPersistentMethodName(i);
                            if(method=="FreshAttempt") button.GetComponentInChildren<TMP_Text>(true).text="Fresh challenge";
                            if(method=="StartPractice") button.GetComponentInChildren<TMP_Text>(true).text="Continue learning";
                        }
                    }
                    foreach(var text in session.exhaustedPanel.GetComponentsInChildren<TMP_Text>(true))
                        if(!text.GetComponentInParent<Button>()) text.text="Challenge lives used up.\nStart a fresh challenge or continue learning without a life limit.";
                }
                if(session.activity is WeatherController weather && !weather.outingIllustration) {
                    var entry=AssetDatabase.LoadAssetAtPath<VocabularyEntry>("Assets/Lemko/Data/Vocabulary/food_apples.asset");
                    if(entry) weather.outingIllustration=entry.sprite;
                }
                if(session.activity is WeatherController w && (!w.temperature || !w.temperatureLabel || !w.outingIllustration)) throw new Exception("Weather preview or slider reference missing");
                if(scene.name=="Village" && (!session.modeButton || !session.modeDescription || session.modeButton.onClick.GetPersistentEventCount()==0)) throw new Exception("Mode controls not wired");
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            Debug.Log("LEMKO_WEATHER_LEARNING_MODE_WIRING_PASS: nine scenes saved, mode controls and Weather references checked. No play simulation or builds.");
        }
        static TMP_Text Text(Transform parent,string name,Vector2 size,Vector2 position,int fontSize) {
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI)); var rect=go.GetComponent<RectTransform>(); rect.SetParent(parent,false); rect.sizeDelta=size; rect.anchoredPosition=position;
            var text=go.GetComponent<TextMeshProUGUI>(); text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset"); text.fontSize=fontSize; text.color=Color.white; text.alignment=TextAlignmentOptions.Center; text.raycastTarget=false; return text;
        }
    }
}
