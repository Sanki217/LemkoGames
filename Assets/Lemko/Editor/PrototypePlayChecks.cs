using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Lemko.Editor
{
    [InitializeOnLoad]
    public static class PrototypePlayChecks
    {
        const string Running="Lemko.PrototypeChecks";
        static readonly string[] names={"Farm","House","FamilyTree","Numbers","Alphabet","Weather","Memory"};
        static double next;
        static int activityIndex, stage;
        static bool wrongChecked;
        static PrototypePlayChecks() { EditorApplication.update+=Tick; }
        public static void RunBatch()
        {
            if(!Environment.GetCommandLineArgs().Contains("-lemkoSmoke")) throw new Exception("Test save isolation required");
            EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath,OpenSceneMode.Single);
            SessionState.SetBool(Running,true); SessionState.SetFloat("Lemko.PrototypeDeadline",(float)EditorApplication.timeSinceStartup+240); EditorApplication.EnterPlaymode();
        }
        static void Check(bool condition,string reason) { if(!condition) throw new Exception(reason); }
        static void Tick()
        {
            if(!SessionState.GetBool(Running,false)) return;
            try {
                if(EditorApplication.timeSinceStartup>SessionState.GetFloat("Lemko.PrototypeDeadline",0)) throw new Exception("Prototype checks timed out");
                if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<next) return;
                next=EditorApplication.timeSinceStartup+.25;
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>(); if(!session || session.Progress==null) return;
                if(stage==0) { session.OpenActivity(names[activityIndex]); stage=1; return; }
                var a=session.activity;
                if(stage==1) {
                    if(!a || !a.Running) return;
                    Check(a.definition.Validate()==null,"valid definition "+names[activityIndex]); Check(!a.worldRoot.GetComponentInParent<Canvas>(),"world objects outside Canvas");
                    session.FreshAttempt(); wrongChecked=false; stage=2; return;
                }
                if(stage==2) {
                    if(!wrongChecked) { TestWrong(a,session); wrongChecked=true; next=EditorApplication.timeSinceStartup+1.4; return; }
                    if(a is MemoryController memory) {
                        if(memory.Evaluating) return;
                        if(!memory.Locked) {
                            var cards=a.taskRoot.GetComponentsInChildren<ActivityToken>();
                            var group=cards.Where(t=>!t.FaceUp).GroupBy(t=>t.view.Entry.id).FirstOrDefault(g=>g.Count()==2);
                            Check(group!=null,"remaining Memory pair"); var pair=group.ToArray(); memory.Choose(pair[0]); memory.Choose(pair[1]); next=EditorApplication.timeSinceStartup+1.3; return;
                        }
                    } else Solve(a);
                    Check(a.Locked,"correct answer locks task: "+names[activityIndex]);
                    int correct=a.Correct, mistakes=a.Mistakes, lives=session.Lives;
                    a.Submit(); Check(a.Correct==correct && a.Mistakes==mistakes && session.Lives==lives,"repeated Submit cannot rescore");
                    a.Advance();
                    if(a.Running) return;
                    Check(session.resultsPanel.activeSelf,"results visible");
                    var record=session.Progress.activities.Find(r=>r.id==names[activityIndex]); Check(record!=null && record.completions>0 && record.lastCorrect>0,"completion persisted");
                    var restored=ProgressStore.Parse(JsonUtility.ToJson(session.Progress)); Check(restored.activities.Exists(r=>r.id==record.id && r.lastPoints==record.lastPoints),"save roundtrip");
                    Debug.Log("LEMKO_ACTIVITY_PLAY_PASS: "+names[activityIndex]);
                    session.RestartActivity(); Check(a.Running && a.Correct==0 && a.Hints==0,"reentry resets temporary state");
                    if(a is MemoryController m) { var cards=a.taskRoot.GetComponentsInChildren<ActivityToken>(); m.Choose(cards[0]); m.Choose(cards.First(t=>t.view.Entry!=cards[0].view.Entry)); Check(m.Evaluating,"Memory evaluation pending before exit"); }
                    session.Village(); stage=3; next=EditorApplication.timeSinceStartup+1.4; return;
                }
                if(stage==3) {
                    Check(!a && !session.tooltip.box.gameObject.activeSelf,"exit removes activity and tooltip");
                    activityIndex++; if(activityIndex<names.Length) { stage=0; return; }
                    session.OpenActivity("Farm"); stage=4; return;
                }
                if(stage==4) {
                    if(!a || !a.Running) return;
                    while(session.Lives>0) session.Wrong(); Check(session.exhaustedPanel.activeSelf && !a.Running,"zero lives closes activity");
                    session.OpenActivity("House"); stage=5; return;
                }
                if(stage==5) {
                    if(!a || a.definition.kind!=ActivityKind.House) return;
                    Check(!a.Running && session.exhaustedPanel.activeSelf && session.hud.text.Contains("Lives: 0"),"zero-life scene entry and HUD");
                    session.StartPractice(); a.Submit(); Check(a.Running && session.Practice && session.Lives==0,"practice stays accessible");
                    session.FreshAttempt(); Check(a.Running && session.Lives==session.shop.startingLives && !session.Practice,"fresh activity attempt");
                    session.Village(); stage=6; return;
                }
                Check(!session.activity,"returns to village after recovery");
                Debug.Log("LEMKO_FULL_PROTOTYPE_PLAY_PASS: seven games completed, errors/duplicates, corrections, hidden Memory, save roundtrip, reentry, pending Memory cancellation, zero lives/practice.");
                SessionState.SetBool(Running,false); EditorApplication.Exit(0);
            } catch(Exception e) { Debug.LogException(e); SessionState.SetBool(Running,false); EditorApplication.Exit(1); }
        }
        static void TestWrong(LearningActivity a,GameSession session)
        {
            int lives=session.Lives;
            if(a is MemoryController m) {
                var cards=a.taskRoot.GetComponentsInChildren<ActivityToken>();
                Check(cards.Length==8 && cards.All(t=>!t.FaceUp && !t.GetComponent<HintPointer>().enabled && t.caption.text=="?" && !t.view.worldSpeaker.activeSelf),"Memory hidden answers");
                m.Choose(cards[0]); m.Choose(cards.First(t=>t.view.Entry!=cards[0].view.Entry)); Check(m.Evaluating,"pair evaluation lock");
                var hidden=cards.First(t=>!t.FaceUp); m.Choose(hidden); Check(!hidden.FaceUp,"third card blocked"); Check(session.Lives==lives,"mismatch no life cost"); return;
            }
            if(a is FamilyTreeController family) { family.Choose("you"); family.Choose("you"); }
            else if(a is NumbersController numbers) { int wrong=numbers.Options.First(n=>n!=a.Current.quantity); numbers.Choose(wrong); numbers.Choose(wrong); }
            else { a.Submit(); a.Submit(); }
            Check(session.Lives==lives-1 && a.Mistakes==1,"wrong answer costs exactly one life: "+a.definition.kind);
            if(a is FarmController farm) {
                Check(!farm.Add(session.shop.products[0]),"invalid animal rejected");
                var tray=a.taskRoot.GetComponentsInChildren<ActivityToken>().First(t=>t.Key=="tray"); var drag=tray.GetComponent<WorldDrag>(); var origin=tray.transform.position;
                var pointer=new PointerEventData(EventSystem.current){position=Camera.main.WorldToScreenPoint(new Vector3(2,-.5f,0))};
                drag.OnBeginDrag(pointer); drag.OnDrag(pointer); drag.OnEndDrag(pointer); Check(farm.Placed.Count==1 && tray.transform.position==origin,"world drag creates copy and restores tray"); farm.Remove(farm.Placed[0]);
                pointer.position=Camera.main.WorldToScreenPoint(new Vector3(-7,3,0)); drag.OnBeginDrag(pointer); drag.OnDrag(pointer); drag.OnEndDrag(pointer); Check(farm.Placed.Count==0,"invalid drop does not place");
                Check(session.Lives==lives-1,"invalid edit no life cost");
            }
            if(a is HouseController house) { Check(!house.Place(a.Current.word,-1),"invalid slot rejected"); Check(session.Lives==lives-1,"invalid drop no life cost"); }
        }
        static void Solve(LearningActivity a)
        {
            if(a is FarmController farm) { foreach(var t in farm.Placed.ToArray()) farm.Remove(t); for(int i=0;i<a.Current.quantity+1;i++) farm.Add(a.Current.word); farm.Remove(farm.Placed.Last()); farm.Submit(); }
            else if(a is HouseController house) { while(a.definition.rooms[house.RoomIndex]!=a.Current.context) house.SwitchRoom(1); house.Place(a.Current.word,0); house.Submit(); }
            else if(a is FamilyTreeController family) family.Choose(a.Current.validPeople[0]);
            else if(a is NumbersController numbers) numbers.Choose(a.Current.quantity);
            else if(a is AlphabetController alphabet) {
                var tiles=a.taskRoot.GetComponentsInChildren<ActivityToken>().Where(t=>t.GetComponent<WorldDrag>()).ToList(); var used=new HashSet<string>(); var letters=SpellingRules.Elements(a.Current.word.lemko);
                for(int i=0;i<letters.Count;i++) { if(SpellingRules.Fixed(letters[i])) continue; var tile=tiles.First(t=>t.caption.text==letters[i] && !used.Contains(t.Key)); Check(alphabet.Place(int.Parse(tile.Key),i),"letter placement"); alphabet.Remove(i); Check(alphabet.Place(int.Parse(tile.Key),i),"returned tile can be reused"); used.Add(tile.Key); }
                alphabet.Submit();
            } else if(a is WeatherController weather) { weather.Choose(a.Current.word); weather.temperature.value=a.Current.minTemperature; weather.Submit(); }
        }
    }
}
