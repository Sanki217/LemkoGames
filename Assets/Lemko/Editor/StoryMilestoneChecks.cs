using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Lemko.Editor
{
    [InitializeOnLoad]
    public static class StoryMilestoneChecks
    {
        const string Running="Lemko.StoryChecks";
        static int stage;
        static double next, deadline;
        static StoryMilestoneChecks() { EditorApplication.update+=Tick; }
        static void Check(bool ok,string message) { if(!ok) throw new Exception(message); }
        public static void Data()
        {
            var old=ProgressStore.Parse("{\"version\":1,\"shopCompletions\":4,\"encountered\":[\"food_bread\"]}");
            Check(old.version==2 && old.shopCompletions==4 && old.story.contributions.Count==0,"old save migration preserves progress without retroactive story completion");
            Check(!VillageStoryProgress.ChooseDecoration(old,"gold"),"reward unavailable before completing story");
            foreach(var id in VillageStoryProgress.Activities) { Check(VillageStoryProgress.Contribute(old,id),"first contribution "+id); Check(!VillageStoryProgress.Contribute(old,id),"duplicate contribution "+id); }
            Check(!VillageStoryProgress.Contribute(old,"Unknown"),"unknown contribution rejected");
            Check(VillageStoryProgress.Complete(old) && VillageStoryProgress.ChooseDecoration(old,"blue"),"eight contributions unlock choice");
            Check(!VillageStoryProgress.ChooseDecoration(old,"unknown"),"invalid reward rejected");
            WordLearning.Record(old,"food_bread","Shop",false,false,"2026-10-26");
            WordLearning.Record(old,"food_bread","Shop",true,true,"2026-10-26");
            WordLearning.Record(old,"food_bread","Shop",true,false,"2026-10-26");
            Check(!old.words[0].rememberedOnLaterDay,"same day is not later recall");
            WordLearning.Record(old,"food_bread","Alphabet",true,false,"2026-10-27");
            var restored=ProgressStore.Parse(JsonUtility.ToJson(old));
            Check(restored.story.contributions.Count==8 && restored.story.decoration=="blue" && restored.words[0].rememberedOnLaterDay && restored.words[0].assisted==1,"save roundtrip includes story and learning evidence");
            var corrupt=ProgressStore.Parse("{\"version\":2,\"story\":{\"contributions\":[\"Shop\",\"Shop\",\"Invalid\"],\"decoration\":\"bad\"},\"words\":null}");
            Check(corrupt.story.contributions.SequenceEqual(new[]{"Shop"}) && corrupt.words.Count==0 && corrupt.story.decoration=="","malformed partial save repair");
            var score=new ShopScore(); for(int i=0;i<12;i++) score.Apply(SelectionResult.Correct); Check(score.NextMultiplier==5,"multiplier cap");
            Debug.Log("LEMKO_STORY_DATA_PASS: old saves, eight contributions, duplicates, reward choices, supported versus independent evidence, later-day recall, malformed saves, score cap.");
        }
        public static void SetupAndData() { VillageStorySetup.Apply(); Data(); }
        public static void RunPlay()
        {
            if(!Environment.GetCommandLineArgs().Contains("-lemkoSmoke")) throw new Exception("Use isolated smoke save");
            EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath,OpenSceneMode.Single); SessionState.SetBool(Running,true); EditorApplication.EnterPlaymode();
        }
        public static void RunVisual()
        {
            var houseScene=EditorSceneManager.OpenScene(SeparateScenesSetup.Folder+"/House.unity",OpenSceneMode.Single);
            UnityEngine.Object.FindFirstObjectByType<HouseController>(FindObjectsInactive.Include).roomLabel.color=new Color(.1f,.23f,.2f);
            EditorSceneManager.MarkSceneDirty(houseScene); EditorSceneManager.SaveScene(houseScene);
            var village=EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath,OpenSceneMode.Single);
            var view=UnityEngine.Object.FindFirstObjectByType<VillageStoryView>();
            view.introduction.rectTransform.anchoredPosition=new Vector2(0,210); view.status.color=new Color(.08f,.2f,.17f);
            EditorSceneManager.MarkSceneDirty(village); EditorSceneManager.SaveScene(village); RunPlay();
        }
        static void Capture(string name) {
            if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            PrototypeVisualChecks.Capture(name,960,540); PrototypeVisualChecks.Capture(name,1280,720);
        }
        static void Tick()
        {
            if(!SessionState.GetBool(Running,false) || !EditorApplication.isPlaying) return;
            try {
                if(deadline==0) deadline=EditorApplication.timeSinceStartup+90;
                if(EditorApplication.timeSinceStartup>deadline) throw new Exception("Story play check timeout");
                if(EditorApplication.timeSinceStartup<next) return; next=EditorApplication.timeSinceStartup+.5;
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>(); if(!session || session.Progress==null) return;
                if(stage==0) {
                    session.Progress.story=new VillageStoryState(); session.storyView.Refresh(session);
                    Check(session.storyView && session.storyView.locations.Length==8 && !session.storyView.goldChoice.gameObject.activeSelf,"eight accessible locations and no premature reward");
                    Capture("Story-start");
                    session.OpenActivity("House"); stage++; return;
                }
                if(stage==1) {
                    if(!(session.activity is HouseController house) || !house.Running) return;
                    while(house.definition.rooms[house.RoomIndex]!=house.Current.context) house.SwitchRoom(1);
                    int lives=session.Lives; var point=new Vector2(1.27f,-.38f);
                    Check(house.PlaceAt(house.Current.word,point) && house.Positions[0]==point,"free placement preserves exact position");
                    Check(house.Move(0,new Vector2(4.13f,.24f)),"placed furniture moves");
                    house.SwitchRoom(1); Check(house.Positions.Length==0,"other room empty"); house.SwitchRoom(-1);
                    Check(Vector2.Distance(house.Positions[0],new Vector2(4.13f,.24f))<.001f,"position survives room switch");
                    Check(!house.PlaceAt(house.Current.word,new Vector2(-10,0)) && !house.Move(0,new Vector2(20,0)) && session.Lives==lives,"invalid drops harmless");
                    house.Remove(0);
                    var tray=house.taskRoot.GetComponentsInChildren<ActivityToken>().First(t=>t.Key=="tray" && t.view.Entry==house.Current.word);
                    Physics2D.SyncTransforms(); var pointer=new PointerEventData(EventSystem.current){position=Camera.main.WorldToScreenPoint(house.taskRoot.TransformPoint(point))};
                    var drag=tray.GetComponent<WorldDrag>(); drag.OnBeginDrag(pointer); drag.OnDrag(pointer); drag.OnEndDrag(pointer);
                    Check(house.PlacedCount==1 && Vector2.Distance(house.Positions[0],point)<.01f,"actual drag callback places at pointer");
                    Capture("House-free-placement");
                    house.Submit(); Check(house.Locked,"valid freely positioned answer accepted");
                    house.Submit(); Check(house.Correct==1,"duplicate Submit does not rescore");
                    session.Village(); stage++; return;
                }
                if(stage==2) {
                    if(!session.storyView) return;
                    Check(session.Progress.story.contributions.Count==0,"unfinished activity gives no contribution");
                    foreach(var id in VillageStoryProgress.Activities) VillageStoryProgress.Contribute(session.Progress,id);
                    session.storyView.Refresh(session); Check(session.storyView.goldChoice.gameObject.activeSelf,"completed chapter offers reward");
                    session.storyView.Blue(); Check(session.Progress.story.decoration=="blue","reward choice saved");
                    Check(session.storyView.contributions[6]==null,"Alphabet adds no visible signs");
                    stage=20; return; // Let Canvas rebuild changed button text before the capture.
                }
                if(stage==20) {
                    Capture("Story-complete");
                    session.OpenActivity("House"); stage=3; return;
                }
                if(stage==3) {
                    if(!(session.activity is HouseController house) || !house.Running) return;
                    Check(house.PlacedCount==0,"reenter clears temporary furniture");
                    while(house.Running) {
                        while(house.definition.rooms[house.RoomIndex]!=house.Current.context) house.SwitchRoom(1);
                        Check(house.PlaceAt(house.Current.word,new Vector2(2.3f,-.6f)),"place each session task"); house.Submit(); Check(house.Locked,"solved session task"); house.Advance();
                    }
                    Check(session.resultsPanel.activeSelf && session.Progress.story.contributions.Count==8,"completed replay gives results without extra contribution"); Capture("Story-results");
                    session.Village(); stage++; return;
                }
                if(!session.storyView) return;
                Check(session.Progress.story.contributions.Count==8 && session.Progress.story.decoration=="blue" && session.storyView.tablecloth.gameObject.activeSelf,"chapter and decoration survive navigation");
                Debug.Log("LEMKO_STORY_PLAY_PASS: eight locations, exact-position drag, moving furniture, rooms, invalid drops, duplicate Submit, partial session, reward UI, no Alphabet signs, reentry and navigation persistence.");
                SessionState.SetBool(Running,false); EditorApplication.Exit(0);
            } catch(Exception e) { Debug.LogException(e); SessionState.SetBool(Running,false); EditorApplication.Exit(1); }
        }
    }
}
