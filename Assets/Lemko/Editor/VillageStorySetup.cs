using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko.Editor
{
    public static class VillageStorySetup
    {
        const string StoryPath="Assets/Lemko/Data/VillageGathering.asset";
        static TMP_FontAsset Font=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset");
        [MenuItem("Lemko/Setup/Apply village gathering milestone")]
        public static void Apply()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach(var path in Directory.GetFiles("Assets/Lemko/Sprites/Placeholders/Rooms","*.png")) {
                var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
                importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single; importer.spritePixelsPerUnit=32; importer.alphaIsTransparency=true; importer.mipmapEnabled=false; importer.SaveAndReimport();
            }
            var story=AssetDatabase.LoadAssetAtPath<VillageStoryDefinition>(StoryPath);
            if(!story) {
                story=ScriptableObject.CreateInstance<VillageStoryDefinition>();
                string[] titles={"Gather supplies","Deliver invitations","Prepare the animals","Prepare the room","Pack the checklist","Prepare tickets","Prepare the labels","Plan the weather"};
                string[] requests={"Collect supplies for the shared table.","Invite the relatives shown in the family tree.","Help finish the animal pens before the gathering.","Prepare furniture in the requested room.","Match the packing checklist pictures and words.","Choose the requested numbers for the tickets.","Check the spelling for our preparations.","Set the requested weather and temperature for our planned day."};
                string[] successes={"The supplies are ready.","The invitations are ready.","The animal pens are ready.","The room is ready.","The packing checklist is ready.","The tickets are ready.","The spelling preparations are ready.","The weather plan is ready."};
                string[] words={"food_bread","family_mother","animal_cow","furniture_table","food_apples","number_3","","weather_sunny"};
                story.contributions=new VillageContribution[8];
                for(int i=0;i<8;i++) story.contributions[i]=new VillageContribution{activityId=VillageStoryProgress.Activities[i],title=titles[i],request=requests[i],success=successes[i],illustration=string.IsNullOrEmpty(words[i])?null:AssetDatabase.LoadAssetAtPath<VocabularyEntry>(VocabularyImporter.Root+"/Vocabulary/"+words[i]+".asset").sprite};
                AssetDatabase.CreateAsset(story,StoryPath);
            }
            foreach(var path in new[]{SeparateScenesSetup.VillagePath}.Concat(SeparateScenesSetup.Activities.Select(n=>SeparateScenesSetup.Folder+"/"+n+".unity"))) {
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                session.story=story;
                if(session.villagePanel && scene.name=="Village") {
                    var view=session.GetComponent<VillageStoryView>();
                    if(!view) {
                        view=session.gameObject.AddComponent<VillageStoryView>(); session.storyView=view;
                        view.title=session.villagePanel.transform.Find("Title").GetComponent<TMP_Text>(); view.title.fontSize=32;
                        view.introduction=session.villagePanel.transform.Find("Subtitle").GetComponent<TMP_Text>(); view.introduction.fontSize=19;
                        view.status=session.villageStatus; view.status.fontSize=18; view.status.rectTransform.anchoredPosition=new Vector2(0,-292); view.status.rectTransform.sizeDelta=new Vector2(1150,55);
                        view.locations=session.villagePanel.GetComponentsInChildren<Button>(true);
                        for(int i=0;i<view.locations.Length;i++) {
                            var r=view.locations[i].GetComponent<RectTransform>(); r.sizeDelta=new Vector2(250,105); r.anchoredPosition=new Vector2(-420+(i%4)*280,65-(i/4)*140);
                            view.locations[i].GetComponentInChildren<TMP_Text>().fontSize=20;
                        }
                        var preview=new GameObject("Gathering preview").transform;
                        view.contributions=new SpriteRenderer[8];
                        for(int i=0;i<8;i++) {
                            if(i==6) continue; // No visible Alphabet signs, by design.
                            var go=new GameObject("Contribution "+VillageStoryProgress.Activities[i],typeof(SpriteRenderer)); go.transform.SetParent(preview,false);
                            go.transform.position=new Vector3(-5.6f+i*1.6f,-2.55f,0); var renderer=go.GetComponent<SpriteRenderer>(); renderer.sprite=story.contributions[i].illustration; renderer.sortingOrder=6;
                            if(renderer.sprite) go.transform.localScale=Vector3.one*(.8f/Mathf.Max(renderer.sprite.bounds.size.x,renderer.sprite.bounds.size.y));
                            view.contributions[i]=renderer; go.SetActive(false);
                        }
                        var cloth=new GameObject("Gathering cloth",typeof(SpriteRenderer)); cloth.transform.SetParent(preview,false); cloth.transform.position=new Vector3(0,-2.55f,1); cloth.transform.localScale=new Vector3(13,.9f,1);
                        view.tablecloth=cloth.GetComponent<SpriteRenderer>(); view.tablecloth.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Lemko/Sprites/Placeholders/Rooms/sprite_room_surface.png"); view.tablecloth.sortingOrder=5; cloth.SetActive(false);
                        view.goldChoice=Choice(session.villagePanel.transform,"Warm gold",new Vector2(-135,-235)); view.blueChoice=Choice(session.villagePanel.transform,"River blue",new Vector2(135,-235));
                        view.goldChoice.gameObject.SetActive(false); view.blueChoice.gameObject.SetActive(false);
                    }
                } else {
                    if(!session.storyRequest) session.storyRequest=Text(session.shopPanel.transform,"Gathering request",new Vector2(1160,28),new Vector2(0,279),16);
                    session.results.fontSize=23; session.results.rectTransform.sizeDelta=new Vector2(1120,350); session.results.rectTransform.anchoredPosition=new Vector2(0,45);
                    if(session.activity is HouseController house && house.roomArtwork.Count==0) foreach(var room in house.definition.rooms)
                        house.roomArtwork.Add(new HouseRoomArtwork{room=room,background=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Lemko/Sprites/Placeholders/Rooms/sprite_empty_"+room.id+".png")});
                }
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath,OpenSceneMode.Single);
            Debug.Log("LEMKO_STORY_SETUP_PASS: eight contributions, nine existing scenes, free House room artwork; no builds.");
        }
        static TMP_Text Text(Transform parent,string name,Vector2 size,Vector2 position,int fontSize)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI)); var rect=(RectTransform)go.transform; rect.SetParent(parent,false); rect.sizeDelta=size; rect.anchoredPosition=position;
            var label=go.GetComponent<TMP_Text>(); label.font=Font; label.fontSize=fontSize; label.color=Color.white; label.alignment=TextAlignmentOptions.Center; label.raycastTarget=false; return label;
        }
        static Button Choice(Transform parent,string name,Vector2 position)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button)); var rect=(RectTransform)go.transform; rect.SetParent(parent,false); rect.sizeDelta=new Vector2(245,34); rect.anchoredPosition=position;
            go.GetComponent<Image>().color=new Color(.2f,.35f,.3f); Text(rect,"Label",rect.sizeDelta,Vector2.zero,18).text=name; return go.GetComponent<Button>();
        }
    }
}
