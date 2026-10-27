using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko.Editor
{
    public static class AssociationTicketLabelSetup
    {
        [MenuItem("Lemko/Setup/Add association checks, tickets and label support")]
        public static void Apply() {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach(var name in new[]{"Memory","Numbers","Alphabet"}) {
                var scene=EditorSceneManager.OpenScene(SeparateScenesSetup.Folder+"/"+name+".unity");
                var session=Object.FindFirstObjectByType<GameSession>();
                if(session.activity is AlphabetController alphabet && !session.shopPanel.transform.Find("Help one letter")) {
                    var go=new GameObject("Help one letter",typeof(RectTransform),typeof(Image),typeof(Button)); var rect=go.GetComponent<RectTransform>();
                    rect.SetParent(session.shopPanel.transform,false); rect.sizeDelta=new Vector2(240,48); rect.anchoredPosition=new Vector2(90,-278);
                    go.GetComponent<Image>().color=new Color(.30f,.45f,.32f); UnityEventTools.AddPersistentListener(go.GetComponent<Button>().onClick,alphabet.HelpLetter);
                    var label=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI)); label.transform.SetParent(rect,false);
                    var text=label.GetComponent<TextMeshProUGUI>(); text.rectTransform.sizeDelta=rect.sizeDelta; text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset");
                    text.fontSize=18; text.text="Help: one letter"; text.alignment=TextAlignmentOptions.Center; text.raycastTarget=false;
                }
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            Debug.Log("LEMKO_ASSOCIATION_TICKET_LABEL_SETUP_PASS");
        }
    }
}
