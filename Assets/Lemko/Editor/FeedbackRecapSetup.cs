using System;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko.Editor
{
    public static class FeedbackRecapSetup
    {
        [MenuItem("Lemko/Setup/Add answer feedback and session recap")]
        public static void Apply() {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach(string name in SeparateScenesSetup.Activities) {
                var scene=EditorSceneManager.OpenScene(SeparateScenesSetup.Folder+"/"+name+".unity");
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if(!session || !session.resultsPanel) throw new Exception("Missing results panel: "+name);
                if(!session.answerFeedback) {
                    session.answerFeedback=session.GetComponent<AnswerFeedback>();
                    if(!session.answerFeedback) session.answerFeedback=session.gameObject.AddComponent<AnswerFeedback>();
                    session.answerFeedback.message=session.activity ? session.activity.feedback : session.shopController.feedback;
                    session.answerFeedback.hud=session.hud;
                }
                if(!session.wordRecap) {
                    session.wordRecap=Text(session.resultsPanel.transform,"Words and next step",new Vector2(1120,70),new Vector2(0,-260),17);
                    session.wordRecap.enableAutoSizing=true; session.wordRecap.fontSizeMin=14; session.wordRecap.fontSizeMax=17;
                }
                if(!session.nextPlaceButton) {
                    var go=new GameObject("Next place",typeof(RectTransform),typeof(Image),typeof(Button)); var rect=go.GetComponent<RectTransform>();
                    rect.SetParent(session.resultsPanel.transform,false); rect.sizeDelta=new Vector2(230,60); rect.anchoredPosition=new Vector2(460,-170);
                    go.GetComponent<Image>().color=new Color(.30f,.45f,.32f); session.nextPlaceButton=go.GetComponent<Button>();
                    var label=Text(rect,"Label",rect.sizeDelta,Vector2.zero,21); label.text="Next place"; label.enableAutoSizing=true; label.fontSizeMin=16; label.fontSizeMax=21;
                    UnityEventTools.AddPersistentListener(session.nextPlaceButton.onClick,session.OpenSuggestedActivity);
                }
                if(!session.answerFeedback.message || !session.answerFeedback.hud || !session.wordRecap || session.nextPlaceButton.onClick.GetPersistentEventCount()==0) throw new Exception("Incomplete feedback/recap wiring: "+name);
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            Debug.Log("LEMKO_FEEDBACK_RECAP_WIRING_PASS: eight activity scenes, feedback and recap references. No play simulation or builds.");
        }
        static TMP_Text Text(Transform parent,string name,Vector2 size,Vector2 position,int fontSize) {
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI)); var rect=go.GetComponent<RectTransform>(); rect.SetParent(parent,false); rect.sizeDelta=size; rect.anchoredPosition=position;
            var text=go.GetComponent<TextMeshProUGUI>(); text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset"); text.fontSize=fontSize; text.color=Color.white; text.alignment=TextAlignmentOptions.Center; text.raycastTarget=false; return text;
        }
    }
}
