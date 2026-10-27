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
            submit.gameObject.SetActive(false); speak.interactable=false; prompt.text="Match each picture with its Lemko word";
            picture.Clear(); matched.Clear(); first=null; evaluating=false; pairsFound=0;
            var pool=definition.pool.Distinct().ToList(); Shuffle(pool);
            pool=pool.OrderByDescending(w=>Session.Progress.encountered.Contains(w.id)).Take(definition.pairs).ToList();
            boardWords.Clear(); boardWords.AddRange(pool); checkWords.Clear(); checkWords.AddRange(pool.Take(Mathf.Clamp(associationChecks,0,2)));
            var cards=pool.SelectMany(w=>new[]{(w,true),(w,false)}).ToList(); Shuffle(cards);
            for(int i=0;i<cards.Count;i++) {
                var card=cards[i]; var token=Token(card.w,new Vector2(-5.4f+(i%4)*3.5f,1.3f-(i/4)*1.85f),"card_"+i,Choose);
                picture[token]=card.Item2; token.Face(false,card.Item2,card.w.lemko);
            }
            feedback.text="Ordinary mismatches cost no lives. Face-down cards have no hints or pronunciation.";
        }
        public void Choose(ActivityToken card)
        {
            if(AssociationActive || boardFinished || !Ready || evaluating || !picture.ContainsKey(card) || matched.Contains(card) || first==card) return;
            card.Face(true,picture[card],card.view.Entry.lemko);
            if(!first) { first=card; return; }
            evaluating=true; StartCoroutine(Evaluate(first,card));
        }
        IEnumerator Evaluate(ActivityToken a,ActivityToken b)
        {
            yield return new WaitForSecondsRealtime(definition.revealSeconds);
            if(a.view.Entry==b.view.Entry && picture[a]!=picture[b]) {
                matched.Add(a); matched.Add(b); pairsFound++; feedback.text="Pair found! "+pairsFound+"/"+definition.pairs; Session.PresentFeedback(true);
                if(pairsFound==definition.pairs) {
                    if(checkWords.Count==0) Answer(true,"complete","All pairs found.");
                    else { boardFinished=true; next.gameObject.SetActive(true); next.GetComponentInChildren<TMPro.TMP_Text>().text="Check words"; feedback.text="Packing pairs ready! Now try "+checkWords.Count+" short word checks without card positions."; }
                }
            } else { a.Face(false,picture[a],a.view.Entry.lemko); b.Face(false,picture[b],b.view.Entry.lemko); feedback.text="Try another pair. No life lost."; Session.PresentFeedback(false); }
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
            prompt.text="Packing check "+(AssociationsCorrect+1)+"/"+checkWords.Count+": choose this picture's Lemko word";
            prompt.GetComponent<HintPointer>().Bind(null,Session.tooltip);
            speak.interactable=false;
            Token(AssociationWord,new Vector2(0,1.1f),"association_picture",t=>Session.audioService.Play(AssociationWord.pronunciation));
            var options=new List<VocabularyEntry>(boardWords); Shuffle(options);
            for(int i=0;i<options.Count;i++) {
                var word=options[i]; var token=Token(word,new Vector2(-5.1f+(i%4)*3.4f,-.8f-(i/4)*1.5f),"association_option",t=>ChooseAssociation(word),word.lemko,false);
                token.GetComponent<HintPointer>().enabled=false;
            }
            feedback.text="Use the word, not its old card position. Mistakes here cost no lives.";
        }
        public void ChooseAssociation(VocabularyEntry word) {
            if(!Ready || !AssociationActive || associationSolved || !boardWords.Contains(word)) return;
            bool correct=word==AssociationWord;
            if(!correct && !rejectedChecks.Add(word.id)) return;
            Session.MarkSupported(AssociationWord); Session.RecordAnswer(AssociationWord,correct);
            if(!correct) { AssociationMistakes++; feedback.text="That word belongs to another picture. Try again; no life lost."; Session.PresentFeedback(false); return; }
            AssociationsCorrect++; associationSolved=true;
            if(AssociationsCorrect>=checkWords.Count) Answer(true,"complete","Packing checklist ready. Word associations practised!");
            else { Session.PresentFeedback(true); feedback.text="Correct: "+AssociationWord.lemko+". One more packing check."; next.gameObject.SetActive(true); next.GetComponentInChildren<TMPro.TMP_Text>().text="Next check"; }
        }
        public override void Submit() { }
        public override void Close() { base.Close(); first=null; evaluating=false; boardFinished=AssociationActive=associationSolved=false; AssociationsCorrect=AssociationMistakes=0; AssociationWord=null; boardWords.Clear(); checkWords.Clear(); rejectedChecks.Clear(); picture.Clear(); matched.Clear(); }
    }
}
