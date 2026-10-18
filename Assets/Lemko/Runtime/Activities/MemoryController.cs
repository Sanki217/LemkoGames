using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Lemko
{
    public sealed class MemoryController : LearningActivity
    {
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
            var cards=pool.SelectMany(w=>new[]{(w,true),(w,false)}).ToList(); Shuffle(cards);
            for(int i=0;i<cards.Count;i++) {
                var card=cards[i]; var token=Token(card.w,new Vector2(-5.4f+(i%4)*3.5f,1.3f-(i/4)*1.85f),"card_"+i,Choose);
                picture[token]=card.Item2; token.Face(false,card.Item2,card.w.lemko);
            }
            feedback.text="Ordinary mismatches cost no lives. Face-down cards have no hints or pronunciation.";
        }
        public void Choose(ActivityToken card)
        {
            if(!Ready || evaluating || !picture.ContainsKey(card) || matched.Contains(card) || first==card) return;
            card.Face(true,picture[card],card.view.Entry.lemko);
            if(!first) { first=card; return; }
            evaluating=true; StartCoroutine(Evaluate(first,card));
        }
        IEnumerator Evaluate(ActivityToken a,ActivityToken b)
        {
            yield return new WaitForSecondsRealtime(definition.revealSeconds);
            if(a.view.Entry==b.view.Entry && picture[a]!=picture[b]) {
                matched.Add(a); matched.Add(b); pairsFound++; feedback.text="Pair found! "+pairsFound+"/"+definition.pairs;
                if(pairsFound==definition.pairs) Answer(true,"complete","All pairs found.");
            } else { a.Face(false,picture[a],a.view.Entry.lemko); b.Face(false,picture[b],b.view.Entry.lemko); feedback.text="Try another pair. No life lost."; }
            first=null; evaluating=false;
        }
        public override void Submit() { }
        public override void Close() { base.Close(); first=null; evaluating=false; }
    }
}
