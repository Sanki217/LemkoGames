using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace Lemko.Editor
{
    public static class BasketJournalSetup
    {
        static TMP_FontAsset Font=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset");
        [MenuItem("Lemko/Setup/Add basket and word journal (preserve existing)")]
        public static void Apply()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene=EditorSceneManager.OpenScene(SeparateScenesSetup.Folder+"/Shop.unity");
            var session=Object.FindFirstObjectByType<GameSession>(); var shop=session.shopController;
            if(!shop.basketRoot) { shop.basketRoot=new GameObject("Basket products").transform; shop.basketRoot.SetParent(shop.worldRoot,false); }
            if(!shop.basketStatus) shop.basketStatus=Text(session.shopPanel.transform,"Basket status",new Vector2(720,30),new Vector2(-230,-270),18);
            shop.basketStatus.color=new Color(.12f,.24f,.21f);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            scene=EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath);
            session=Object.FindFirstObjectByType<GameSession>();
            session.villagePanel.GetComponentInParent<Canvas>().sortingOrder=100;
            if(!session.journal) {
                var journal=session.gameObject.AddComponent<WordJournal>(); session.journal=journal; journal.session=session;
                journal.database=AssetDatabase.LoadAssetAtPath<VocabularyDatabase>("Assets/Lemko/Data/VocabularyDatabase.asset");
                Button(session.villagePanel.transform,"Word journal",new Vector2(-500,245),new Vector2(160,38),journal.Open);
                var panel=Rect(session.villagePanel.transform.parent,"Word journal panel",new Vector2(1220,615),new Vector2(0,-32));
                panel.gameObject.AddComponent<Image>().color=new Color(.10f,.19f,.18f); journal.panel=panel.gameObject;
                journal.title=Text(panel,"Title",new Vector2(900,40),new Vector2(-80,275),26);
                Button(panel,"Close",new Vector2(520,275),new Vector2(140,38),journal.Close);
                var input=Rect(panel,"Search",new Vector2(660,40),new Vector2(-250,223)); input.gameObject.AddComponent<Image>().color=new Color(.21f,.31f,.28f);
                var field=input.gameObject.AddComponent<TMP_InputField>(); journal.search=field;
                var area=Rect(input,"Viewport",new Vector2(640,36),Vector2.zero); area.gameObject.AddComponent<RectMask2D>();
                var typed=Text(area,"Text",new Vector2(640,36),Vector2.zero,20); typed.alignment=TextAlignmentOptions.MidlineLeft;
                var placeholder=Text(area,"Placeholder",new Vector2(640,36),Vector2.zero,20); placeholder.text="Search Lemko, Polish or English"; placeholder.color=new Color(.7f,.8f,.75f); placeholder.alignment=TextAlignmentOptions.MidlineLeft;
                field.textViewport=area; field.textComponent=(TextMeshProUGUI)typed; field.placeholder=placeholder; field.fontAsset=Font; field.pointSize=20;
                journal.reviewFilter=Button(panel,"Show words to practise",new Vector2(330,223),new Vector2(510,40),journal.ToggleReview);
                journal.rows=new JournalRow[6];
                for(int i=0;i<6;i++) {
                    var r=Rect(panel,"Word row "+(i+1),new Vector2(1170,53),new Vector2(0,163-i*57));
                    r.gameObject.AddComponent<Image>().color=new Color(.19f,.29f,.26f); var button=r.gameObject.AddComponent<Button>();
                    UnityEventTools.AddIntPersistentListener(button.onClick,journal.SelectRow,i);
                    var icon=Rect(r,"Picture",new Vector2(45,45),new Vector2(-550,0)).gameObject.AddComponent<Image>(); icon.preserveAspect=true; icon.raycastTarget=false;
                    var word=Text(r,"Lemko",new Vector2(310,49),new Vector2(-350,0),22);
                    var meaning=Text(r,"Translation",new Vector2(300,49),new Vector2(-30,0),20);
                    var state=Text(r,"Learning history",new Vector2(390,49),new Vector2(355,0),17);
                    journal.rows[i]=new JournalRow{button=button,picture=icon,word=word,meaning=meaning,state=state};
                }
                journal.previous=Button(panel,"Previous",new Vector2(-220,-168),new Vector2(150,32),journal.Previous);
                journal.next=Button(panel,"Next",new Vector2(220,-168),new Vector2(150,32),journal.Next);
                journal.pageLabel=Text(panel,"Page",new Vector2(200,32),new Vector2(0,-168),19);
                journal.detail=Text(panel,"Details",new Vector2(900,94),new Vector2(-130,-241),18);
                journal.pronunciation=Button(panel,"Pronunciation",new Vector2(470,-235),new Vector2(220,46),journal.Speak);
                panel.gameObject.SetActive(false);
            }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("LEMKO_BASKET_JOURNAL_SETUP_PASS: existing Shop and Village scenes updated; no builds.");
        }
        static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 position) {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent,false); rect.sizeDelta=size; rect.anchoredPosition=position; return rect;
        }
        static TMP_Text Text(Transform parent,string name,Vector2 size,Vector2 position,int fontSize) {
            var rect=Rect(parent,name,size,position); var text=rect.gameObject.AddComponent<TextMeshProUGUI>(); text.font=Font; text.fontSize=fontSize; text.color=Color.white; text.alignment=TextAlignmentOptions.Center; text.raycastTarget=false; return text;
        }
        static Button Button(Transform parent,string name,Vector2 position,Vector2 size,UnityAction action) {
            var rect=Rect(parent,name,size,position); rect.gameObject.AddComponent<Image>().color=new Color(.30f,.45f,.32f); var button=rect.gameObject.AddComponent<Button>();
            Text(rect,"Label",size,Vector2.zero,19).text=name; UnityEventTools.AddPersistentListener(button.onClick,action); return button;
        }
    }
}
