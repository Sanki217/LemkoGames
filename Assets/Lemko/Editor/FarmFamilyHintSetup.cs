using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko.Editor
{
    public static class FarmFamilyHintSetup
    {
        static TMP_FontAsset Font=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset");
        [MenuItem("Lemko/Setup/Add pen repairs, invitations and hold hints")]
        public static void Apply()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string rowPath="Assets/Lemko/Prefabs/Words/ShoppingListRow.prefab";
            var row=PrefabUtility.LoadPrefabContents(rowPath);
            try {
                if(!row.GetComponentInChildren<HoldTranslationHint>(true)) {
                    var control=Control(row.transform,"Hold hint",new Vector2(64,34),Vector2.zero,"? 3s");
                    var rect=control.GetComponent<RectTransform>(); rect.anchorMin=rect.anchorMax=new Vector2(1,.5f); rect.anchoredPosition=new Vector2(-32,0);
                    var text=row.GetComponent<TMP_Text>(); text.margin=new Vector4(text.margin.x,text.margin.y,72,text.margin.w);
                    text.enableAutoSizing=true; text.fontSizeMin=18; text.fontSizeMax=text.fontSize;
                    PrefabUtility.SaveAsPrefabAsset(row,rowPath);
                }
            } finally { PrefabUtility.UnloadPrefabContents(row); }
            foreach(var name in new[]{"Farm","House","FamilyTree","Numbers","Alphabet","Weather"}) {
                var scene=EditorSceneManager.OpenScene(SeparateScenesSetup.Folder+"/"+name+".unity");
                var session=Object.FindFirstObjectByType<GameSession>(); var activity=session.activity;
                if(!activity.hintControl) activity.hintControl=Control(session.shopPanel.transform,"Hold translation hint",new Vector2(250,48),new Vector2(-195,-278),"Hold for hint (3s)");
                if(activity is FamilyTreeController family && !family.album) {
                    family.album=Text(session.shopPanel.transform,"Invitation album",new Vector2(1120,58),new Vector2(0,-160),19);
                    family.album.color=Color.white; family.contentScale=.70f; family.contentOffset=new Vector2(0,.1f);
                }
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            Debug.Log("LEMKO_FARM_FAMILY_HINT_SETUP_PASS: existing scenes and shopping row extended; no builds.");
        }
        public static void RepairAlbumLayout() {
            var scene=EditorSceneManager.OpenScene(SeparateScenesSetup.Folder+"/FamilyTree.unity");
            var family=Object.FindFirstObjectByType<FamilyTreeController>();
            family.album.color=Color.white; family.contentScale=.70f; family.contentOffset=new Vector2(0,.1f);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            Debug.Log("LEMKO_INVITATION_LAYOUT_PASS");
        }
        static HoldTranslationHint Control(Transform parent,string name,Vector2 size,Vector2 position,string label) {
            var rect=Rect(parent,name,size,position); rect.gameObject.AddComponent<Image>().color=new Color(.30f,.45f,.32f);
            Text(rect,"Label",size,Vector2.zero,18).text=label; return rect.gameObject.AddComponent<HoldTranslationHint>();
        }
        static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 position) {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent,false); rect.sizeDelta=size; rect.anchoredPosition=position; return rect;
        }
        static TMP_Text Text(Transform parent,string name,Vector2 size,Vector2 position,int fontSize) {
            var rect=Rect(parent,name,size,position); var text=rect.gameObject.AddComponent<TextMeshProUGUI>(); text.font=Font; text.fontSize=fontSize; text.color=Color.white; text.alignment=TextAlignmentOptions.Center; text.raycastTarget=false; return text;
        }
    }
}
