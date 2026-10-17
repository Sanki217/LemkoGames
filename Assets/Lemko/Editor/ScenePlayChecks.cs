using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Lemko.Editor
{
    // Editor-only state/navigation validation. No player build is created.
    [InitializeOnLoad]
    public static class ScenePlayChecks
    {
        const string Running="Lemko.SceneChecks.Running";
        static double next;
        static ScenePlayChecks() { EditorApplication.update+=Tick; }
        public static void RunBatch()
        {
            if(!Environment.GetCommandLineArgs().Contains("-lemkoSmoke")) throw new Exception("Use -lemkoSmoke to isolate test saves.");
            EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath,OpenSceneMode.Single);
            SessionState.SetInt("Lemko.TestStep",0); SessionState.SetBool(Running,true);
            SessionState.SetFloat("Lemko.TestDeadline",(float)EditorApplication.timeSinceStartup+90);
            EditorApplication.EnterPlaymode();
        }
        static void Check(bool condition,string message) { if(!condition) throw new Exception(message); }
        static void Tick()
        {
            if(!SessionState.GetBool(Running,false)) return;
            try {
                if(EditorApplication.timeSinceStartup>SessionState.GetFloat("Lemko.TestDeadline",0)) throw new Exception("Play checks timed out");
                if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<next) return;
                next=EditorApplication.timeSinceStartup+.7;
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if(!session || session.Progress==null) return;
                int step=SessionState.GetInt("Lemko.TestStep",0);
                if(step==0) { Check(SceneManager.GetActiveScene().name=="Village","village entry"); session.OpenShop(); }
                else if(step==1) {
                    Check(SceneManager.GetActiveScene().name=="Shop","Shop navigation");
                    var views=session.shopController.worldRoot.GetComponentsInChildren<WordView>();
                    Check(views.Length==8,"eight displayed products from large pool");
                    var targetIds=session.shopController.listRoot.GetComponentsInChildren<HintPointer>().Select(h=>h.entry.id).ToArray(); var correct=views.Where(v=>targetIds.Contains(v.Entry.id)).ToArray();
                    var wrong=views.Except(correct).ToArray(); Check(correct.Length==5 && wrong.Length==3,"solvable round");
                    int lives=session.Lives;
                    wrong[0].Select(); wrong[0].Select(); Check(session.Lives==lives-1,"duplicate wrong selection");
                    for(int i=0;i<3;i++) { correct[i].Select(); correct[i].Select(); }
                    Check(session.shopController.Points==600,"streak gives 100 + 200 + 300");
                    wrong[1].Select(); Check(session.shopController.Points==600 && session.shopController.NextMultiplier==1,"wrong resets streak");
                    correct[3].Select(); correct[4].Select(); Check(session.shopController.Points==900,"score after reset");
                    session.shopController.Complete(); int completions=session.Progress.shopCompletions;
                    session.shopController.Complete(); Check(session.resultsPanel.activeSelf && session.Progress.lastShopPoints==900 && session.Progress.shopCompletions==completions,"results and duplicate completion");
                    SessionState.SetInt("Lemko.ExpectedLives",session.Lives); session.Village();
                }
                else if(step>=2 && step<=8) {
                    string expected=step==2 ? "Village" : SeparateScenesSetup.Activities[step-2];
                    Check(SceneManager.GetActiveScene().name==expected,"placeholder navigation "+expected);
                    Check(session.Lives==SessionState.GetInt("Lemko.ExpectedLives",-1),"lives preserved");
                    Check(UnityEngine.Object.FindObjectsByType<GameSession>(FindObjectsSortMode.None).Length==1,"no duplicated sessions");
                    session.OpenActivity(SeparateScenesSetup.Activities[step-1]);
                }
                else if(step==9) { Check(SceneManager.GetActiveScene().name=="Weather","Weather scene"); session.Village(); }
                else if(step==10) { Check(SceneManager.GetActiveScene().name=="Village","return from activity"); session.OpenShop(); }
                else if(step==11) {
                    Check(session.shopController.Points==0 && session.shopController.NextMultiplier==1,"round score reset on return");
                    while(session.Lives>0) session.Wrong(); Check(session.exhaustedPanel.activeSelf,"zero-life recovery");
                    session.StartPractice(); session.Wrong(); Check(session.Lives==0 && session.Practice,"practice");
                    session.FreshAttempt(); Check(session.Lives==session.shop.startingLives && !session.Practice,"fresh attempt");
                    session.Village();
                }
                else {
                    Check(SceneManager.GetActiveScene().name=="Village" && session.Lives==session.shop.startingLives,"fresh lives retained across scenes");
                    Debug.Log("LEMKO_EDITOR_SCENE_PLAY_CHECKS_PASS: scoring, duplicates, nine scenes, shared lives, practice, fresh attempt.");
                    SessionState.SetBool(Running,false); EditorApplication.Exit(0); return;
                }
                SessionState.SetInt("Lemko.TestStep",step+1);
            } catch(Exception e) { Debug.LogException(e); SessionState.SetBool(Running,false); EditorApplication.Exit(1); }
        }
    }
}
