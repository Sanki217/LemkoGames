using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
            var subjects=new HashSet<string>();
            pool=candidates.Where(w=>subjects.Add(w.tags?.FirstOrDefault() ?? "")).Take(definition.pairs).ToList();
            if(pool.Count<definition.pairs)
                pool.AddRange(candidates.Except(pool).Take(definition.pairs-pool.Count).ToList());
            boardWords.Clear(); boardWords.AddRange(pool); checkWords.Clear(); checkWords.AddRange(pool.Take(Mathf.Clamp(associationChecks,0,2)));
            var cards=pool.SelectMany(w=>new[]{(w,true),(w,false)}).ToList(); Shuffle(cards);
            for(int i=0;i<cards.Count;i++) {
                var card=cards[i]; var token=Token(card.w,new Vector2(-5.4f+(i%4)*3.5f,1.3f-(i/4)*1.85f),"card_"+i,Choose);
                picture[token]=card.Item2; token.Face(false,card.Item2,card.w.lemko);
            }
            feedback.text="Turn over two cards.";
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
                Juice.Hop(a.transform,.35f); Juice.Hop(b.transform,.35f,.08f); Juice.Sparkle(Juice.WorldToScreen(a.transform.position),8); Juice.Sparkle(Juice.WorldToScreen(b.transform.position),8); Juice.Sound("match",1+pairsFound*.08f);
                Session.audioService.Play(a.view.Entry.pronunciation); pairsFound++; feedback.text="Pair found! "+pairsFound+"/"+definition.pairs; Session.PresentFeedback(true);
                if(pairsFound==definition.pairs) {
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
