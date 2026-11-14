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
        public HoldTranslationHint hintControl;
        [Header("World layout (editable independently of UI)")]
        public Vector2 contentOffset;
        [Range(.5f,1.5f)] public float contentScale=1;
        public GameSession Session {get;private set;}
        public int Correct {get;private set;}
        public int Mistakes {get;private set;}
        public int Hints {get;private set;}
        public int Points => score.Points;
        public int Multiplier => score.NextMultiplier;
        public int Streak => score.Streak;
        [Header("Challenge mode extras")]
        [Min(1)] public float quickBonusSeconds=8;
        int goldenIndex=-1; float taskStarted;
        // One surprise task per round is worth double in Challenge mode.
        public bool GoldenTask => Running && Session && !Session.Practice && Total>=3 && Correct==goldenIndex;
        // 1 at the start of a task, falling to 0; only meaningful in Challenge mode and never ends a task.
        public float QuickBonus => Running && !Locked && Session && !Session.Practice && Session.CanAnswer ? Mathf.Clamp01(1-(Time.unscaledTime-taskStarted)/quickBonusSeconds) : 0;
        protected virtual bool HasAnswer => true;
        // Set while a board is merely being redrawn, so unchanged pieces do not pop in again.
        protected bool quietSpawn;
        // A small round counter drawn in the world (destroyed with the task).
        protected TMP_Text Badge(Vector2 position,float size,Color color)
        {
            var go=new GameObject("Counter badge",typeof(SpriteRenderer)); go.transform.SetParent(taskRoot,false); go.transform.localPosition=position;
            var disc=go.GetComponent<SpriteRenderer>(); disc.sprite=Look.Dot; disc.color=color; disc.sortingOrder=6; go.transform.localScale=Vector3.one*(size/disc.sprite.bounds.size.x);
            var label=new GameObject("Count",typeof(TextMeshPro)).GetComponent<TextMeshPro>(); label.transform.SetParent(go.transform,false);
            label.font=tokenPrefab.caption.font; label.fontSize=3.2f; label.fontStyle=FontStyles.Bold; label.alignment=TextAlignmentOptions.Center; label.color=Look.Ink; label.sortingOrder=7;
            label.rectTransform.sizeDelta=new Vector2(.6f,.5f); return label;
        }
        void Update() { if(submit && submit.gameObject.activeSelf) submit.interactable=Ready && HasAnswer; }
        public bool Locked {get;private set;}
        public bool Running {get;private set;}
        public LearningTask Current {get;private set;}
        public string ProgressLabel => definition.kind+": "+Correct+"/"+Total;
        protected virtual int Total => definition.tasksPerSession;
        public int SessionTaskCount => Total;
        protected readonly List<ActivityToken> tokens=new List<ActivityToken>();
        ShopScore score=new ShopScore();
        List<LearningTask> order;
        string rejected;
        public void Begin(GameSession session)
        {
            Close(); Session=session; session.BeginLearningSession(); Correct=Mistakes=Hints=0; score=new ShopScore();
            string error=ValidateContent();
            if(error!=null) { prompt.text="Content needs attention"; feedback.text=error; Debug.LogError(error,this); return; }
            order=AvailableTasks(); Shuffle(order);
            order=PracticeSelection.Order(order,t=>t.word,session,Total); Running=true; goldenIndex=Random.Range(0,Mathf.Max(1,Total)); worldRoot.gameObject.SetActive(true); taskRoot.localPosition=contentOffset; taskRoot.localScale=Vector3.one*contentScale;
            submit.onClick.RemoveListener(Submit); submit.onClick.AddListener(Submit);
            next.onClick.RemoveListener(Advance); next.onClick.AddListener(Advance);
            speak.onClick.RemoveListener(Speak); speak.onClick.AddListener(Speak);
            StartTask();
        }
        void StartTask()
        {
            ClearTask(); Locked=false; rejected=null; Current=order.Count>0 ? order[Correct%order.Count] : null;
            next.gameObject.SetActive(false); submit.gameObject.SetActive(true); feedback.text=""; taskStarted=Time.unscaledTime;
            if(Current!=null) { Session.Encounter(Current.word); prompt.text=Current.word.lemko; prompt.GetComponent<HintPointer>().Bind(Current.word,Session.tooltip); }
            if(hintControl) { hintControl.gameObject.SetActive(false); hintControl.Bind(Current?.word,Session.tooltip); }
            speak.interactable=Current!=null && Current.word.pronunciation;
            BuildTask();
            var listen=speak.GetComponentInChildren<TMP_Text>(true); if(listen) listen.text="Listen"; speak.gameObject.SetActive(speak.interactable);
            var check=submit.GetComponentInChildren<TMP_Text>(true); if(check) check.text="Check";
            Session.RefreshHud(); GameEvents.Started();
        }
        protected virtual string ValidateContent()=>definition.Validate();
        protected virtual List<LearningTask> AvailableTasks()=>new List<LearningTask>(definition.tasks);
        protected abstract void BuildTask();
        public abstract void Submit();
        protected bool Ready => Running && !Locked && Session.CanAnswer;
        protected bool Answer(bool correct,string signature,string explanation)
        {
            if(!Ready || (!correct && rejected==signature)) return false;
            if(Current!=null && definition.kind!=ActivityKind.Memory) Session.RecordAnswer(Current.word,correct);
            if(correct) {
                bool golden=GoldenTask; float quick=QuickBonus;
                Correct++; Locked=true; int awarded=score.Apply(SelectionResult.Correct); int bonus=0;
                if(!Session.Practice) { bonus=(golden ? awarded : 0)+Mathf.RoundToInt(awarded*.5f*quick/10f)*10; score.Bonus(bonus); }
                feedback.text=explanation+(bonus>0 ? "  Bonus +"+bonus+"!" : ""); Session.PresentFeedback(true);
                submit.gameObject.SetActive(false); next.gameObject.SetActive(false);
                SessionStats.BestStreak=Mathf.Max(SessionStats.BestStreak,score.Streak);
                GameEvents.Answer(true,awarded+bonus,score.Streak); ContinueSoon();
            }
            else { rejected=signature; Mistakes++; score.Apply(SelectionResult.Incorrect); feedback.text=explanation+(Mistakes>=2 ? " Right-click a word, or tap the lightbulb, for help." : ""); Session.PresentFeedback(false); GameEvents.Answer(false,0,0); Session.Wrong(); }
            Session.RefreshHud(); return true;
        }
        // Moves on by itself after a short celebration; waits while a menu or the help card is open.
        protected void ContinueSoon(float seconds=1.15f) { if(isActiveAndEnabled) StartCoroutine(ContinueAfter(seconds)); }
        System.Collections.IEnumerator ContinueAfter(float seconds) {
            yield return new WaitForSecondsRealtime(seconds);
            while(Running && Session && ((Session.menu && Session.menu.IsOpen) || (Session.introduction && Session.introduction.IsOpen))) yield return null;
            if(Running) Advance();
        }
        public virtual void Advance() { if(!Running || !Locked) return; if(Correct>=Total) Session.FinishActivity(true); else StartTask(); }
        public void RecordHint() { if(Running) { Hints++; Session.RefreshHud(); } }
        public void Speak() { if(Current!=null) Session.audioService.Play(Current.word.pronunciation); }
        protected ActivityToken Token(VocabularyEntry word,Vector2 position,string key,System.Action<ActivityToken> click,string text=null,bool picture=true,bool draggable=false)
        {
            var token=Instantiate(tokenPrefab,taskRoot); token.transform.localPosition=position;
            token.Bind(word,Session,key,click,text,picture); tokens.Add(token); token.backing.color=Look.Paper;
            if(draggable) { var drag=token.gameObject.AddComponent<WorldDrag>(); drag.token=token; }
            if(word) Session.Encounter(word); if(!quietSpawn) GameEvents.Spawned(token); return token;
        }
        protected WorldDropZone Zone(Vector2 position,Vector2 size,System.Func<ActivityToken,bool> accept)
        {
            var go=new GameObject("Placement zone",typeof(BoxCollider2D),typeof(WorldDropZone),typeof(SpriteRenderer)); go.transform.SetParent(taskRoot,false); go.transform.localPosition=position;
            go.GetComponent<BoxCollider2D>().size=size; var renderer=go.GetComponent<SpriteRenderer>(); renderer.sprite=tokenPrefab.backing.sprite; renderer.drawMode=SpriteDrawMode.Sliced; renderer.size=size; renderer.color=Look.Alpha(Look.Sage,.55f); renderer.sortingOrder=-1;
            var zone=go.GetComponent<WorldDropZone>(); zone.dropped=accept; return zone;
        }
        protected void ClearTask() { Session?.StopFeedback(); if(hintControl) hintControl.Cancel(); StopAllCoroutines(); Session?.tooltip.Hide(); foreach(Transform t in taskRoot) { t.gameObject.SetActive(false); Destroy(t.gameObject); } tokens.Clear(); }
        public virtual void Close() { Session?.StopFeedback(); if(hintControl) hintControl.Cancel(); Running=false; Locked=true; StopAllCoroutines(); if(taskRoot) { foreach(Transform t in taskRoot) { t.gameObject.SetActive(false); Destroy(t.gameObject); } } tokens.Clear(); if(worldRoot) worldRoot.gameObject.SetActive(false); if(Session) { Session.tooltip.Hide(); Session.audioService.Stop(); } }
        protected static void Shuffle<T>(IList<T> list) { for(int i=list.Count-1;i>0;i--) { int j=Random.Range(0,i+1); (list[i],list[j])=(list[j],list[i]); } }
        protected virtual void OnDisable() => Close();
    }
}
