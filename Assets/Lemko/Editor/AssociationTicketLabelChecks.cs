using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Lemko.Editor
{
    [InitializeOnLoad] public static class AssociationTicketLabelChecks
    {
        const string Key="Lemko.AssociationTicketLabelChecks";
        static int step;
        static double next;
        static AssociationTicketLabelChecks() { EditorApplication.update+=Tick; }
        static void Check(bool condition,string message) { if(!condition) throw new Exception(message); }
        public static void RunBatch() {
            if(!Environment.GetCommandLineArgs().Contains("-lemkoSmoke")) throw new Exception("Use smoke saves");
            EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath); SessionState.SetBool(Key,true);
            SessionState.SetFloat(Key+"Deadline",(float)EditorApplication.timeSinceStartup+160); EditorApplication.EnterPlaymode();
        }
        static void Capture(string name) { if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null) return; PrototypeVisualChecks.Capture(name,1280,720); PrototypeVisualChecks.Capture(name,960,540); }
        static void Tick() {
            if(!SessionState.GetBool(Key,false)) return;
            try {
                if(EditorApplication.timeSinceStartup>SessionState.GetFloat(Key+"Deadline",0)) throw new Exception("Timeout");
                if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<next) return; next=EditorApplication.timeSinceStartup+.8;
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>(); if(!session || session.Progress==null) return;
                if(step==0) session.OpenActivity("Memory");
                else if(step==1) {
                    var memory=(MemoryController)session.activity;
                    if(memory.Evaluating) return;
                    if(!memory.AssociationPending) {
                        var pair=memory.taskRoot.GetComponentsInChildren<ActivityToken>().Where(t=>!t.FaceUp).GroupBy(t=>t.view.Entry).First().ToArray();
                        Check(pair.All(t=>!t.GetComponent<HintPointer>().enabled),"hidden card hints disabled"); memory.Choose(pair[0]); memory.Choose(pair[1]); next=EditorApplication.timeSinceStartup+1.4; return;
                    }
                    Check(!memory.Locked && session.IndependentAnswers==0 && session.SupportedAnswers==0,"matching alone is not recall or completed session"); memory.Advance();
                } else if(step==2) {
                    var memory=(MemoryController)session.activity; Check(memory.AssociationActive,"association phase");
                    var options=memory.taskRoot.GetComponentsInChildren<ActivityToken>().Where(t=>t.Key=="association_option").ToArray();
                    Check(options.Length==memory.definition.pairs && options.All(t=>!t.GetComponent<HintPointer>().enabled),"word options no hidden translation hints");
                    int lives=session.Lives; var wrong=options.First(t=>t.view.Entry!=memory.AssociationWord).view.Entry;
                    memory.ChooseAssociation(wrong); memory.ChooseAssociation(wrong); Check(memory.AssociationMistakes==1 && session.Lives==lives,"association mistakes harmless and duplicate guarded");
                } else if(step==3) {
                    Capture("Memory-association"); var memory=(MemoryController)session.activity;
                    memory.ChooseAssociation(memory.AssociationWord); memory.ChooseAssociation(memory.AssociationWord);
                    Check(memory.AssociationsCorrect==1 && session.SupportedAnswers==1 && session.IndependentAnswers==0,"association practice supported once"); memory.Advance();
                } else if(step==4) {
                    var memory=(MemoryController)session.activity; memory.ChooseAssociation(memory.AssociationWord);
                    Check(memory.Locked && memory.AssociationsCorrect==2 && session.SupportedAnswers==2,"both checks required"); memory.Advance();
                    Check(session.resultsPanel.activeSelf && session.results.text.Contains("Association checks: 2"),"association results survive cleanup");
                    session.RestartActivity(); Check(!memory.AssociationActive && !memory.AssociationPending && memory.AssociationsCorrect==0,"association reset"); session.OpenActivity("Numbers");
                } else if(step==5) {
                    var numbers=(NumbersController)session.activity;
                    Check(numbers.Options.Length==numbers.Options.Distinct().Count() && numbers.Options.Contains(numbers.Current.quantity) && numbers.Options.All(n=>n>=1 && n<=20),"solvable supplied ticket range");
                    var validate=typeof(NumbersController).GetMethod("ValidateContent",BindingFlags.NonPublic|BindingFlags.Instance);
                    numbers.minimumNumber=99; numbers.maximumNumber=1; Check(validate.Invoke(numbers,null)!=null,"inverted range rejected");
                    numbers.maximumNumber=99; Check(validate.Invoke(numbers,null)!=null,"one-form range rejected");
                    numbers.minimumNumber=1; numbers.maximumNumber=20;
                    numbers.Choose(numbers.Current.quantity); numbers.Choose(numbers.Current.quantity); Check(numbers.TicketsIssued==1 && numbers.Correct==1,"ticket issued once");
                } else if(step==6) {
                    Capture("Numbers-ticket-issued"); var numbers=(NumbersController)session.activity; numbers.Advance(); Check(numbers.TicketsIssued==1,"issued preview retained");
                    session.RestartActivity(); Check(numbers.TicketsIssued==0,"ticket reset"); session.OpenActivity("Alphabet");
                } else if(step==7) {
                    var alphabet=(AlphabetController)session.activity; alphabet.HelpLetter();
                    Check(alphabet.Hints==1 && !string.IsNullOrEmpty(alphabet.AnswerText),"letter support available");
                    // Synthetic repeated-letter/apostrophe fixture, never written to a vocabulary asset.
                    var definition=UnityEngine.Object.Instantiate(alphabet.definition); var word=UnityEngine.Object.Instantiate(definition.tasks[0].word); word.lemko="а’ааа"; word.id="smoke_spelling_only"; word.polish="TEST";
                    definition.tasks=new System.Collections.Generic.List<LearningTask>{new LearningTask{id="smoke_spelling",word=word}}; alphabet.definition=definition;
                    word.lemko="аба"; session.RestartActivity();
                    var aTiles=alphabet.taskRoot.GetComponentsInChildren<ActivityToken>().Where(t=>t.GetComponent<WorldDrag>() && t.caption.text=="а").ToArray();
                    alphabet.Place(int.Parse(aTiles[0].Key),2); alphabet.Place(int.Parse(aTiles[1].Key),1); alphabet.HelpLetter();
                    Check(alphabet.AnswerText=="аа","help moves a misplaced tile before disturbing a correct letter"); alphabet.HelpLetter(); Check(alphabet.AnswerText=="аба","occupied matching tiles remain solvable");
                    word.lemko="а’ааа"; session.RestartActivity();
                    for(int i=0;i<4;i++) alphabet.HelpLetter(); Check(alphabet.AnswerText==word.lemko && alphabet.Hints==4,"repeated letters and fixed apostrophe");
                    alphabet.HelpLetter(); Check(alphabet.Hints==4,"finished help does not count again");
                    alphabet.Remove(2); Check(alphabet.AnswerText!=word.lemko,"supported tile removable"); alphabet.HelpLetter();
                    alphabet.Submit(); alphabet.Submit(); Check(alphabet.Correct==1 && session.SupportedAnswers==1 && session.IndependentAnswers==0,"helped spelling supported once"); alphabet.Advance();
                    Check(alphabet.IsRepairTask && alphabet.AnswerText.Count(c=>c=='а')==2,"repair leaves two letters missing");
                    int hints=alphabet.Hints; alphabet.HelpLetter(); alphabet.HelpLetter(); Check(alphabet.AnswerText==word.lemko && alphabet.Hints==hints+2,"repair solvable with distinct tiles");
                    alphabet.Submit(); Check(alphabet.Correct==2 && session.SupportedAnswers==2,"prefilled repair supported");
                    session.Village();
                } else if(step==8) { session.OpenActivity("Alphabet");
                } else if(step==9) {
                    var alphabet=(AlphabetController)session.activity;
                    for(int i=0;i<12 && alphabet.AnswerText!=string.Concat(SpellingRules.Elements(alphabet.Current.word.lemko));i++) alphabet.HelpLetter(); alphabet.Submit(); alphabet.Advance();
                } else if(step==10) {
                    Capture("Alphabet-repair-label"); session.Village();
                } else {
                    Check(!session.activity && !session.tooltip.box.gameObject.activeSelf,"exit cleanup");
                    Debug.Log("LEMKO_ASSOCIATION_TICKET_LABEL_PLAY_PASS: Memory two-check flow/support/duplicates/privacy/reset, supplied ticket range/validation/issuance, repeated-letter help/apostrophes/repair/support and scene cleanup.");
                    SessionState.SetBool(Key,false); EditorApplication.Exit(0); return;
                }
                step++;
            } catch(Exception e) { Debug.LogException(e); SessionState.SetBool(Key,false); EditorApplication.Exit(1); }
        }
    }
}
