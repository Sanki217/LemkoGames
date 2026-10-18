using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko
{
    public abstract class LearningActivity : MonoBehaviour
    {
        [Header("Content and scene references")]
        public ActivityDefinition definition;
        public Transform worldRoot, taskRoot;
        public ActivityToken tokenPrefab;
        public TMP_Text prompt, feedback;
        public Button submit, next, speak;
        [Header("World layout (editable independently of UI)")]
        public Vector2 contentOffset;
        [Range(.5f,1.5f)] public float contentScale=1;
        public GameSession Session {get;private set;}
        public int Correct {get;private set;}
        public int Mistakes {get;private set;}
        public int Hints {get;private set;}
        public int Points => score.Points;
        public int Multiplier => score.NextMultiplier;
        public bool Locked {get;private set;}
        public bool Running {get;private set;}
        public LearningTask Current {get;private set;}
        public string ProgressLabel => definition.kind+": "+Correct+"/"+Total;
        protected virtual int Total => definition.tasksPerSession;
        protected readonly List<ActivityToken> tokens=new List<ActivityToken>();
        ShopScore score=new ShopScore();
        List<LearningTask> order;
        string rejected;
        public void Begin(GameSession session)
        {
            Close(); Session=session; Correct=Mistakes=Hints=0; score=new ShopScore();
            string error=definition.Validate();
            if(error!=null) { prompt.text="Content needs attention"; feedback.text=error; Debug.LogError(error,this); return; }
            order=new List<LearningTask>(definition.tasks); Shuffle(order); Running=true; worldRoot.gameObject.SetActive(true); taskRoot.localPosition=contentOffset; taskRoot.localScale=Vector3.one*contentScale;
            submit.onClick.RemoveListener(Submit); submit.onClick.AddListener(Submit);
            next.onClick.RemoveListener(Advance); next.onClick.AddListener(Advance);
            speak.onClick.RemoveListener(Speak); speak.onClick.AddListener(Speak);
            StartTask();
        }
        void StartTask()
        {
            ClearTask(); Locked=false; rejected=null; Current=order.Count>0 ? order[Correct%order.Count] : null;
            next.gameObject.SetActive(false); submit.gameObject.SetActive(true); feedback.text="Demo content may be unverified. Hold a word/picture for 3 seconds for a hint.";
            if(Current!=null) { Session.Encounter(Current.word); prompt.text=Current.word.lemko; prompt.GetComponent<HintPointer>().Bind(Current.word,Session.tooltip); }
            speak.interactable=Current!=null && Current.word.pronunciation;
            BuildTask(); Session.RefreshHud();
        }
        protected abstract void BuildTask();
        public abstract void Submit();
        protected bool Ready => Running && !Locked && Session.CanAnswer;
        protected bool Answer(bool correct,string signature,string explanation)
        {
            if(!Ready || (!correct && rejected==signature)) return false;
            if(correct) { Correct++; Locked=true; score.Apply(SelectionResult.Correct); feedback.text="Correct! "+explanation; submit.gameObject.SetActive(false); next.gameObject.SetActive(true); next.GetComponentInChildren<TMP_Text>().text=Correct>=Total ? "Results" : "Next"; }
            else { rejected=signature; Mistakes++; score.Apply(SelectionResult.Incorrect); feedback.text=explanation+" Change your answer and try again."; Session.Wrong(); }
            Session.RefreshHud(); return true;
        }
        public void Advance() { if(!Running || !Locked) return; if(Correct>=Total) Session.FinishActivity(true); else StartTask(); }
        public void RecordHint() { if(Running) { Hints++; Session.RefreshHud(); } }
        public void Speak() { if(Current!=null) Session.audioService.Play(Current.word.pronunciation); }
        protected ActivityToken Token(VocabularyEntry word,Vector2 position,string key,System.Action<ActivityToken> click,string text=null,bool picture=true,bool draggable=false)
        {
            var token=Instantiate(tokenPrefab,taskRoot); token.transform.localPosition=position;
            token.Bind(word,Session,key,click,text,picture); tokens.Add(token);
            if(draggable) { var drag=token.gameObject.AddComponent<WorldDrag>(); drag.token=token; }
            if(word) Session.Encounter(word); return token;
        }
        protected WorldDropZone Zone(Vector2 position,Vector2 size,System.Func<ActivityToken,bool> accept)
        {
            var go=new GameObject("Placement zone",typeof(BoxCollider2D),typeof(WorldDropZone),typeof(SpriteRenderer)); go.transform.SetParent(taskRoot,false); go.transform.localPosition=position;
            go.GetComponent<BoxCollider2D>().size=size; var renderer=go.GetComponent<SpriteRenderer>(); renderer.sprite=tokenPrefab.backing.sprite; renderer.drawMode=SpriteDrawMode.Sliced; renderer.size=size; renderer.color=new Color(.3f,.5f,.35f,.5f); renderer.sortingOrder=-1;
            var zone=go.GetComponent<WorldDropZone>(); zone.dropped=accept; return zone;
        }
        protected void ClearTask() { StopAllCoroutines(); Session?.tooltip.Hide(); foreach(Transform t in taskRoot) { t.gameObject.SetActive(false); Destroy(t.gameObject); } tokens.Clear(); }
        public virtual void Close() { Running=false; Locked=true; StopAllCoroutines(); if(taskRoot) { foreach(Transform t in taskRoot) { t.gameObject.SetActive(false); Destroy(t.gameObject); } } tokens.Clear(); if(worldRoot) worldRoot.gameObject.SetActive(false); if(Session) { Session.tooltip.Hide(); Session.audioService.Stop(); } }
        protected static void Shuffle<T>(IList<T> list) { for(int i=list.Count-1;i>0;i--) { int j=Random.Range(0,i+1); (list[i],list[j])=(list[j],list[i]); } }
        protected virtual void OnDisable() => Close();
    }
}
