using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lemko.Editor
{
    [InitializeOnLoad]
    public static class FamilyRelationshipsChecks
    {
        const string Key="Lemko.FamilyRelationships";
        static ActivityDefinition original, test;
        static int question, answer, round;
        static string previousReference;
        static readonly HashSet<string> words=new HashSet<string>();
        static FamilyRelationshipsChecks() => EditorApplication.update+=Tick;

        public static void Run()
        {
            if(!Environment.GetCommandLineArgs().Contains("-lemkoSmoke")) throw new Exception("Use isolated smoke saves.");
            EditorSceneManager.OpenScene(SeparateScenesSetup.Folder+"/FamilyTree.unity");
            SessionState.SetFloat(Key+".deadline",(float)EditorApplication.timeSinceStartup+180);
            SessionState.SetBool(Key,true);
            EditorApplication.EnterPlaymode();
        }

        static void Check(bool condition,string message) { if(!condition) throw new Exception(message); }

        static void Tick()
        {
            if(!SessionState.GetBool(Key,false)) return;
            try {
                Check(EditorApplication.timeSinceStartup<SessionState.GetFloat(Key+".deadline",0),"Family checks timed out");
                if(!EditorApplication.isPlaying) return;
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if(!session || session.Progress==null) return;
                var cover=GameObject.Find("Scene cover");
                if(cover && cover.GetComponent<UnityEngine.UI.Image>().raycastTarget) return;
                if(session.introduction.IsOpen) {
                    session.introduction.OnPointerClick(new PointerEventData(EventSystem.current));
                    return;
                }
                var family=(FamilyTreeController)session.activity;
                if(!original) {
                    original=family.definition;
                    Check(original.Validate()==null,original.Validate());
                    Check(original.people.Count==15,"Expected expanded family");
                    foreach(string id in new[]{"mother","father"}) {
                        var parent=original.people.Single(p=>p.id==id);
                        Check(!string.IsNullOrEmpty(parent.parentId) && !string.IsNullOrEmpty(parent.secondParentId),"Both parents need two parents");
                    }
                    test=UnityEngine.Object.Instantiate(original);
                    family.definition=test;
                    test.tasksPerSession=1;
                }
                if(question<original.tasks.Count) {
                    var task=original.tasks[question];
                    test.tasks=new List<LearningTask>{task};
                    session.RestartActivity();
                    Check(family.ReferencePerson==task.referencePerson,"Wrong reference person");
                    var tokens=family.taskRoot.GetComponentsInChildren<ActivityToken>().Where(t=>t.gameObject.activeSelf).ToArray();
                    Check(tokens.Length==15,"Missing portrait");
                    Check(tokens.Count(t=>t.caption.gameObject.activeSelf)==1,"Expected one YOU marker");
                    Check(tokens.Single(t=>t.caption.gameObject.activeSelf).Key==family.ReferencePerson,"YOU marker on wrong person");
                    Check(tokens.All(t=>!t.GetComponent<HintPointer>().enabled),"Portrait exposes a fixed relationship hint");
                    int correct=family.Correct;
                    family.Choose(family.ReferencePerson);
                    Check(family.Correct==correct,"Reference person accepted as their own relative");
                    tokens.Single(t=>t.Key==task.validPeople[answer]).view.Select();
                    Check(family.Correct==1 && family.Locked,"Valid relative rejected: "+task.id);
                    Check(session.audioService.GetComponent<AudioSource>().clip==task.word.pronunciation,"Wrong relationship audio");
                    family.Advance();
                    Check(session.resultsPanel.activeSelf,"Round did not finish");
                    if(++answer==task.validPeople.Length) { question++; answer=0; }
                    return;
                }
                if(round<8) {
                    family.definition=original;
                    if(!family.Running) {
                        previousReference=null; words.Clear();
                        session.RestartActivity();
                    }
                    Check(previousReference!=family.ReferencePerson,"Reference repeated consecutively");
                    Check(words.Add(family.Current.word.id),"Relationship repeated within round");
                    previousReference=family.ReferencePerson;
                    family.Choose(family.Current.validPeople[0]);
                    family.Advance();
                    if(!family.Running) round++;
                    return;
                }
                Debug.Log("LEMKO_FAMILY_RELATIONSHIPS_PASS: all 85 questions and every accepted answer, marker placement, wrong reference rejection, pronunciation, results, and eight rounds with changing references.");
                Finish(0);
            } catch(Exception e) { Debug.LogException(e); Finish(1); }
        }

        static void Finish(int code)
        {
            SessionState.SetBool(Key,false);
            EditorApplication.Exit(code);
        }
    }
}
