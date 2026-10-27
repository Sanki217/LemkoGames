using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Lemko.Editor
{
    [InitializeOnLoad] public static class FarmFamilyHintChecks
    {
        const string Key="Lemko.FarmFamilyHints";
        static double next;
        static int step;
        static HoldTranslationHint held;
        static PointerEventData pointer;
        static FarmFamilyHintChecks() { EditorApplication.update+=Tick; }
        static void Check(bool value,string message) { if(!value) throw new Exception(message); }
        public static void RunBatch() {
            if(!Environment.GetCommandLineArgs().Contains("-lemkoSmoke")) throw new Exception("Isolated save required");
            EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath); SessionState.SetBool(Key,true);
            SessionState.SetFloat(Key+"Deadline",(float)EditorApplication.timeSinceStartup+150); EditorApplication.EnterPlaymode();
        }
        static void Capture(string name) { if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null) return; PrototypeVisualChecks.Capture(name,1280,720); PrototypeVisualChecks.Capture(name,960,540); }
        static void Tick() {
            if(!SessionState.GetBool(Key,false)) return;
            try {
                if(EditorApplication.timeSinceStartup>SessionState.GetFloat(Key+"Deadline",0)) throw new Exception("Timeout");
                if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<next) return;
                next=EditorApplication.timeSinceStartup+.8;
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>(); if(!session || session.Progress==null) return;
                if(step==0) session.OpenActivity("Farm");
                else if(step==1) {
                    var farm=(FarmController)session.activity;
                    // Clone only in play mode to test all three repair variants without editing the supplied asset.
                    farm.definition=UnityEngine.Object.Instantiate(farm.definition); farm.definition.tasksPerSession=6;
                    session.FreshAttempt(); Check(!farm.IsRepairTask && farm.Placed.Count==0,"first task starts empty");
                    held=farm.hintControl; pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=new Vector2(300,70)};
                    held.OnPointerDown(pointer); Check(farm.Hints==0,"no instant explicit hint"); next=EditorApplication.timeSinceStartup+1.5;
                } else if(step==2) {
                    Check(session.activity.Hints==0,"hold delay"); held.OnPointerUp(pointer); next=EditorApplication.timeSinceStartup+2;
                } else if(step==3) {
                    Check(session.activity.Hints==0 && !session.tooltip.IsVisibleFor(held),"release cancels pending hint"); held.OnPointerDown(pointer); next=EditorApplication.timeSinceStartup+3.3;
                } else if(step==4) {
                    Check(session.activity.Hints==1 && session.tooltip.IsVisibleFor(held),"explicit hold counts once"); next=EditorApplication.timeSinceStartup+1;
                } else if(step==5) {
                    Check(session.activity.Hints==1,"continued hold no repeat"); held.OnPointerUp(pointer);
                } else if(step==6) {
                    var farm=(FarmController)session.activity;
                    if(farm.IsRepairTask) {
                        Check(!(farm.Placed.Count==farm.Current.quantity && farm.Placed.All(t=>t.view.Entry==farm.Current.word)),"repair always begins unsolved");
                        Check(farm.Mistakes==0,"prepared errors are not player mistakes"); Capture("Farm-repair-"+(farm.Correct+1));
                    }
                    int lives=session.Lives; foreach(var token in farm.Placed.ToArray()) farm.Remove(token);
                    for(int i=0;i<farm.Current.quantity;i++) Check(farm.Add(farm.Current.word),"tray can solve repair");
                    Check(session.Lives==lives,"editing free"); farm.Submit(); farm.Submit(); Check(farm.Locked,"repair submits and locks");
                    if(farm.Correct==1) Check(session.SupportedAnswers==1,"explicit hint marks answer supported");
                    if(farm.Correct<6) { farm.Advance(); return; }
                    farm.Advance(); Check(session.resultsPanel.activeSelf,"six tasks complete"); session.OpenActivity("FamilyTree");
                } else if(step==7) {
                    var family=(FamilyTreeController)session.activity; session.FreshAttempt();
                    Check(family.InvitationsDelivered==0 && family.prompt.text.Contains("invitation"),"invitation context and clean album");
                    int lives=session.Lives; family.Choose("you"); family.Choose("you"); Check(session.Lives==lives-1 && family.InvitationsDelivered==0,"incorrect invitation not delivered");
                    family.Choose(family.Current.validPeople.Last()); family.Choose(family.Current.validPeople.Last());
                    Check(family.InvitationsDelivered==1 && family.Correct==1 && family.album.text.Contains(family.Current.word.lemko),"valid answer delivered once");
                } else if(step==8) {
                    Capture("Family-invitation-album"); var family=(FamilyTreeController)session.activity; family.Advance();
                    Check(family.InvitationsDelivered==1,"album retained between tasks"); family.hintControl.OnPointerDown(pointer); session.RestartActivity();
                    Check(family.InvitationsDelivered==0 && !session.tooltip.IsVisibleFor(family.hintControl),"restart clears album and pending hint"); session.OpenShop();
                } else if(step==9) {
                    held=session.shopController.listRoot.GetComponentInChildren<HoldTranslationHint>();
                    Check(held && held.word,"shop row explicit hint bound"); held.OnPointerDown(pointer); next=EditorApplication.timeSinceStartup+3.3;
                } else if(step==10) {
                    Check(session.shopController.HintsUsed==1 && session.shopController.BasketCount==0,"shop explicit hint without selecting"); Capture("Shop-explicit-hint"); held.OnPointerUp(pointer); session.OpenActivity("Memory");
                } else if(step==11) {
                    Check(!session.activity.hintControl,"no hidden Memory hint control"); session.Village();
                } else {
                    Check(!session.tooltip.box.gameObject.activeSelf,"navigation cleanup"); Debug.Log("LEMKO_FARM_FAMILY_HINT_PLAY_PASS: all repair variants solvable, free edits, six-task session, explicit hold delays/cancellation/count/support, invitations/duplicates/restart, Shop row hints and Memory exclusion.");
                    SessionState.SetBool(Key,false); EditorApplication.Exit(0); return;
                }
                step++;
            } catch(Exception e) { Debug.LogException(e); SessionState.SetBool(Key,false); EditorApplication.Exit(1); }
        }
    }
}
