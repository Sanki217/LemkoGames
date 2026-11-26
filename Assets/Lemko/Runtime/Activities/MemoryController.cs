using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Lemko
{
    public sealed class MemoryController : LearningActivity
    {
        [Header("After-board association practice")]
        [Range(0,2)] public int associationChecks=2;
        public bool AssociationActive {get;private set;}
        public bool AssociationPending=>boardFinished && !AssociationActive;
        public VocabularyEntry AssociationWord {get;private set;}
        public int AssociationsCorrect {get;private set;}
        public int AssociationMistakes {get;private set;}
        public string PracticeSummary=>"Pairs: "+pairsFound+" | Association checks: "+AssociationsCorrect+" | Check mistakes: "+AssociationMistakes+"\nThese checks are supported practice, not independent recall.";
        bool boardFinished, associationSolved;
        readonly List<VocabularyEntry> boardWords=new List<VocabularyEntry>();
        readonly List<VocabularyEntry> checkWords=new List<VocabularyEntry>();
        readonly HashSet<string> rejectedChecks=new HashSet<string>();
        readonly Dictionary<ActivityToken,bool> picture=new Dictionary<ActivityToken,bool>();
        readonly HashSet<ActivityToken> matched=new HashSet<ActivityToken>();
        ActivityToken first;
        bool evaluating;
        int pairsFound;
        public int PairsFound=>pairsFound;
        public int PairCount=>boardWords.Count;
        public bool CanPeek=>Ready && !boardFinished && !AssociationActive && !evaluating && pairsFound<PairCount;
        public bool Evaluating=>evaluating;
        protected override int Total => 1;
        protected override void BuildTask()
        {
            submit.gameObject.SetActive(false); speak.interactable=false; prompt.text="Match pictures and words";
            picture.Clear(); matched.Clear(); first=null; evaluating=false; pairsFound=0;
            var pool=definition.pool.Distinct().ToList(); Shuffle(pool);
            // Mix subjects so closely related concepts seldom compete on the same board.
            // Rotate in unseen words while keeping a familiar item for review.
            var encountered=new HashSet<string>(Session.Progress.encountered);
            var review=pool.Where(w=>encountered.Contains(w.id)).OrderByDescending(Session.ReviewPriority).Take(1);
            var candidates=review.Concat(pool.Where(w=>!encountered.Contains(w.id))).Concat(pool).Distinct().ToList();
            // Several words share a painted picture (sun, sunny, bright); a board never holds two of them.
            var pictures=new HashSet<Sprite>(); candidates=candidates.Where(w=>!w.sprite || pictures.Add(w.sprite)).ToList();
            var subjects=new HashSet<string>();
            int completed=Session.Progress.activities.Find(r=>r.id=="Memory")?.completions ?? 0;
            int target=Mathf.Min(8,definition.pairs+2*Mathf.Clamp(completed,0,3));
            pool=candidates.Where(w=>subjects.Add(w.tags?.FirstOrDefault() ?? "")).Take(target).ToList();
            if(pool.Count<target)
                pool.AddRange(candidates.Except(pool).Take(target-pool.Count).ToList());
            boardWords.Clear(); boardWords.AddRange(pool); checkWords.Clear(); checkWords.AddRange(pool.Take(Mathf.Clamp(associationChecks,0,2)));
            var cards=pool.SelectMany(w=>new[]{(w,true),(w,false)}).ToList(); Shuffle(cards);
            var tray=new GameObject("Matched pairs tray",typeof(SpriteRenderer));
            tray.transform.SetParent(taskRoot,false); tray.transform.localPosition=new Vector2(0,-2.55f);
            var surface=tray.GetComponent<SpriteRenderer>();
            Pixels.Card(surface,new Vector2(PairCount*1.65f+.3f,1.05f),Look.Panel);
            surface.color=Look.Sage; surface.sortingOrder=-1;
            tray.AddComponent<BoxCollider2D>().size=surface.size;
            var tap=new EventTrigger.Entry { eventID=EventTriggerType.PointerClick };
            tap.callback.AddListener(e=> { if(((PointerEventData)e).button==PointerEventData.InputButton.Left) Peek(); });
            tray.AddComponent<EventTrigger>().triggers.Add(tap);
            float spacing=Mathf.Min(3.5f,14f/PairCount);
            for(int i=0;i<cards.Count;i++) {
                var card=cards[i]; float x=(i%PairCount-(PairCount-1)*.5f)*spacing;
                var token=Token(card.w,new Vector2(x,1f-(i/PairCount)*1.85f),"card_"+i,Choose);
                if(PairCount>6) token.transform.localScale=Vector3.one*.86f;
                picture[token]=card.Item2; token.Face(false,card.Item2,card.w.lemko);
            }
        }
        public void Peek()
        {
            if(!CanPeek) return;
            var hidden=picture.Keys.Where(t=>t!=first && !matched.Contains(t) && !t.FaceUp).ToList();
            if(hidden.Count==0) return;
            var card=first ? hidden.Find(t=>t.view.Entry==first.view.Entry) : null;
            if(!card) card=hidden[Random.Range(0,hidden.Count)];
            evaluating=true;
            Session.RegisterHint(card.view.Entry);
            StartCoroutine(PeekCard(card));
        }
        IEnumerator PeekCard(ActivityToken card)
        {
            card.Face(true,picture[card],card.view.Entry.lemko);
            card.GetComponent<HintPointer>().enabled=false;
            yield return new WaitForSecondsRealtime(1f);
            card.Face(false,picture[card],card.view.Entry.lemko);
            evaluating=false;
        }
        void Collect(ActivityToken card)
        {
            Vector3 from=card.transform.position;
            foreach(var hit in card.GetComponentsInChildren<Collider2D>(true)) hit.enabled=false;
            Juice.Calm(card.transform); Juice.Hover(card.transform,false);
            float slot=(pairsFound-(PairCount-1)*.5f)*1.65f;
            card.transform.localPosition=new Vector2(slot+(picture[card] ? -.40f : .40f),-2.55f);
            card.transform.localScale=Vector3.one*.43f;
            Juice.TravelFrom(card.transform,from);
        }
        public void Choose(ActivityToken card)
        {
            if(AssociationActive || boardFinished || !Ready || evaluating || !picture.ContainsKey(card) || matched.Contains(card) || first==card) return;
            bool showPicture=picture[card]; Juice.Flip(card.transform,()=>card.Face(true,showPicture,card.view.Entry.lemko)); Juice.Sound("flip");
            if(!first) { first=card; return; }
            evaluating=true; StartCoroutine(Evaluate(first,card));
        }
        IEnumerator Evaluate(ActivityToken a,ActivityToken b)
        {
            yield return new WaitForSecondsRealtime(definition.revealSeconds);
            while(Running && !Session.CanAnswer) yield return null;
            if(!Running) yield break;
            if(a.view.Entry==b.view.Entry && picture[a]!=picture[b]) {
                matched.Add(a); matched.Add(b);
                a.Face(true,picture[a],a.view.Entry.lemko); b.Face(true,picture[b],b.view.Entry.lemko);
                a.backing.color=b.backing.color=Look.GoodSoft;
                Juice.Sparkle(Juice.WorldToScreen(a.transform.position),8); Juice.Sparkle(Juice.WorldToScreen(b.transform.position),8); Juice.Sound("match",1+pairsFound*.08f);
                Collect(a); Collect(b);
                Session.audioService.Play(a.view.Entry.pronunciation); pairsFound++; feedback.text="Pair found! "+pairsFound+"/"+PairCount; Session.PresentFeedback(true);
                if(pairsFound==PairCount) {
                    if(checkWords.Count==0) Answer(true,"complete","All pairs found.");
                    else { boardFinished=true; feedback.text="All pairs found! Now "+checkWords.Count+" quick word checks."; ContinueSoon(1.7f); }
                }
            } else {
                bool pa=picture[a], pb=picture[b]; Juice.Shake(a.transform,.1f); Juice.Shake(b.transform,.1f);
                Juice.Flip(a.transform,()=>a.Face(false,pa,a.view.Entry.lemko)); Juice.Flip(b.transform,()=>b.Face(false,pb,b.view.Entry.lemko));
                feedback.text="Not a pair. Try again."; Session.PresentFeedback(false);
            }
            first=null; evaluating=false;
        }
        public override void Advance() {
            if(!Running) return;
            if(AssociationPending || (AssociationActive && associationSolved && !Locked)) { StartAssociation(); return; }
            base.Advance();
        }
        void StartAssociation() {
            ClearTask(); picture.Clear(); matched.Clear(); first=null; evaluating=false;
            AssociationActive=true; associationSolved=false; rejectedChecks.Clear(); AssociationWord=checkWords[AssociationsCorrect];
            next.gameObject.SetActive(false); submit.gameObject.SetActive(false);
            prompt.text="Word check "+(AssociationsCorrect+1)+"/"+checkWords.Count+": which word is this?";
            prompt.GetComponent<HintPointer>().Bind(null,Session.tooltip);
            speak.interactable=false;
            Token(AssociationWord,new Vector2(0,1.1f),"association_picture",t=>Session.audioService.Play(AssociationWord.pronunciation));
            var options=new List<VocabularyEntry>(boardWords); Shuffle(options);
            for(int i=0;i<options.Count;i++) {
                var word=options[i]; var token=Token(word,new Vector2(-5.1f+(i%4)*3.4f,-.8f-(i/4)*1.5f),"association_option",t=>ChooseAssociation(word),word.lemko,false);
                token.GetComponent<HintPointer>().enabled=false;
            }
            feedback.text="";
        }
        public void ChooseAssociation(VocabularyEntry word) {
            if(!Ready || !AssociationActive || associationSolved || !boardWords.Contains(word)) return;
            bool correct=word==AssociationWord;
            if(!correct && !rejectedChecks.Add(word.id)) return;
            Session.MarkSupported(AssociationWord); Session.RecordAnswer(AssociationWord,correct);
            if(!correct) { AssociationMistakes++; feedback.text="That word belongs to another picture."; Session.PresentFeedback(false); GameEvents.Answer(false,0,0); return; }
            AssociationsCorrect++; associationSolved=true;
            if(AssociationsCorrect>=checkWords.Count) Answer(true,"complete","All matched and checked!");
            else { Session.PresentFeedback(true); feedback.text="Yes: "+AssociationWord.lemko; GameEvents.Answer(true,0,0); ContinueSoon(); }
        }
        public override void Submit() { }
        public override void Close() { base.Close(); first=null; evaluating=false; boardFinished=AssociationActive=associationSolved=false; AssociationsCorrect=AssociationMistakes=0; AssociationWord=null; boardWords.Clear(); checkWords.Clear(); rejectedChecks.Clear(); picture.Clear(); matched.Clear(); }
    }
}
