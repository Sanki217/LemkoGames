using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Lemko
{
    public sealed class AlphabetController : LearningActivity
    {
        [Header("Label support")]
        public bool enableRepairLabels=true;
        [Min(2)] public int repairEvery=2;
        [Range(1,4)] public int missingLetters=2;
        public bool IsRepairTask {get;private set;}
        List<string> spelling;
        string[] filled;
        readonly Dictionary<int,ActivityToken> tiles=new Dictionary<int,ActivityToken>();
        readonly Dictionary<int,int> used=new Dictionary<int,int>();
        readonly List<ActivityToken> slots=new List<ActivityToken>();
        public string AnswerText => string.Concat(filled);
        protected override void BuildTask()
        {
            building=true; IsRepairTask=enableRepairLabels && (Correct+1)%Mathf.Max(2,repairEvery)==0;
            prompt.text=(IsRepairTask ? "Fix the label: " : "Spell in Lemko: ")+Look.Word(Current.word.polish); prompt.GetComponent<HintPointer>().Bind(Current.word,Session.tooltip,HintContent.LemkoSpelling);
            if(hintControl) hintControl.Bind(Current.word,Session.tooltip,HintContent.LemkoSpelling);
            var picture=Token(Current.word,new Vector2(-6.85f,.45f),"object",t=>Speak());
            picture.GetComponent<HintPointer>().Bind(Current.word,Session.tooltip,HintContent.LemkoSpelling);
            spelling=SpellingRules.Elements(Current.word.lemko); filled=new string[spelling.Count]; tiles.Clear(); used.Clear(); slots.Clear();
            for(int i=0;i<spelling.Count;i++) {
                // The fields sit on the blackboard drawn in the backdrop; long words close up to fit it.
                float step=Mathf.Min(Spacing,11.2f/spelling.Count);
                int index=i; var pos=new Vector2(BoardCentre.x-(spelling.Count-1)*step*.5f+i*step,BoardCentre.y); bool fixedSlot=SpellingRules.Fixed(spelling[i]); filled[i]=fixedSlot ? spelling[i] : "";
                var slot=Token(Current.word,pos,"slot",t=>Remove(index),fixedSlot?spelling[i]:"_",false); StyleLetter(slot,false); slot.GetComponent<HintPointer>().enabled=false; slots.Add(slot);
                if(!fixedSlot) Zone(pos,TileSize+new Vector2(.02f,.04f),t=>Place(int.Parse(t.Key),index)).GetComponent<SpriteRenderer>().color=Color.clear;
            }
            var letters=SpellingRules.Letters(Current.word.lemko);
            var extras=SpellingRules.Letters(definition.extraLetters);
            if(extras.Count==0 && definition.alphabetLetters!=null) {
                extras=definition.alphabetLetters.Select(l=>l.lowercase).Where(l=>!letters.Contains(l)).Distinct().ToList();
                Shuffle(extras);
            }
            letters.AddRange(extras.Take(4)); Shuffle(letters);
            for(int i=0;i<letters.Count;i++) { int tile=i; var token=Token(Current.word,new Vector2(BoardCentre.x-5.5f*Spacing+(i%12)*Spacing,-1.72f-(i/12)*(TileSize.y+.2f)),i.ToString(),t=>PlaceFirst(tile),letters[i],false,true); StyleLetter(token,true); token.GetComponent<HintPointer>().enabled=false; tiles[i]=token; }
            if(IsRepairTask) {
                var positions=Enumerable.Range(0,spelling.Count).Where(i=>!SpellingRules.Fixed(spelling[i])).ToList(); Shuffle(positions);
                foreach(int index in positions.Skip(Mathf.Max(1,missingLetters))) FillCorrect(index);
                if(used.Count>0) Session.MarkSupported(Current.word);
            }
            feedback.text=(IsRepairTask ? "Add the missing letters. " : "")+"Tap or drag letters. Tap a slot to undo."; building=false;
        }
        static readonly Vector2 BoardCentre=new Vector2(.6f,.42f);
        bool building;
        protected override bool HasAnswer => used.Count>0;
        // Filling the last slot checks the label, exactly as pressing Check would.
        IEnumerator AutoCheck() {
            yield return new WaitForSecondsRealtime(.55f);
            if(Ready && System.Array.TrueForAll(filled,f=>f!="")) Submit();
        }
        Vector2 TileSize => Session.presentation ? Session.presentation.letterSize : new Vector2(.72f,.82f);
        float Spacing => Session.presentation ? Session.presentation.letterSpacing : .96f;
        void StyleLetter(ActivityToken token,bool tile) {
            token.transform.localScale=Vector3.one;
            var size=token.backing.sprite.bounds.size;
            token.backing.transform.localScale=new Vector3(TileSize.x/size.x,TileSize.y/size.y,1);
            // Fields are chalk boxes with chalk letters; the loose tiles stay yellow cards.
            token.backing.color=tile ? Look.Honey : Look.Alpha(Look.Paper,.16f); if(!tile) token.caption.color=Look.Paper;
            token.GetComponent<BoxCollider2D>().size=TileSize;
            token.caption.rectTransform.sizeDelta=TileSize-new Vector2(.07f,.06f);
        }
        bool FillCorrect(int index) {
            Remove(index);
            int tile=tiles.Keys.Where(k=>tiles[k].caption.text==spelling[index] && !used.ContainsValue(k)).DefaultIfEmpty(-1).First();
            if(tile<0) {
                var occupied=used.Where(pair=>tiles[pair.Value].caption.text==spelling[index]).OrderBy(pair=>filled[pair.Key]==spelling[pair.Key]).FirstOrDefault();
                if(!used.ContainsKey(occupied.Key) || tiles[occupied.Value].caption.text!=spelling[index]) return false;
                tile=occupied.Value; Remove(occupied.Key);
            }
            return Place(tile,index);
        }
        public void HelpLetter() {
            if(!Ready) return;
            int index=Enumerable.Range(0,spelling.Count).Where(i=>!SpellingRules.Fixed(spelling[i]) && filled[i]!=spelling[i]).DefaultIfEmpty(-1).First();
            if(index<0) return;
            if(FillCorrect(index)) { Session.MarkSupported(Current.word); Session.RegisterHint(Current.word); feedback.text="One letter added for you."; }
        }
        public void PlaceFirst(int tile) { int index=System.Array.FindIndex(filled,s=>s==""); if(index>=0) Place(tile,index); }
        public bool Place(int tile,int index)
        {
            if(!Ready || !tiles.ContainsKey(tile) || used.ContainsValue(tile) || index<0 || index>=filled.Length || filled[index]!="") return false;
            filled[index]=tiles[tile].caption.text; used[index]=tile; tiles[tile].Available(false); slots[index].caption.text=filled[index]; PresentationMotion.Pulse(slots[index].caption.transform,Session);
            var letter=definition.alphabetLetters?.Find(l=>l.lowercase==filled[index]);
            if(letter?.pronunciation) Session.audioService.Play(letter.pronunciation);
            Juice.Sound("drop",1+used.Count*.06f);
            if(!building && System.Array.TrueForAll(filled,f=>f!="")) StartCoroutine(AutoCheck());
            return true;
        }
        public void Remove(int index) { if(!Ready || !used.TryGetValue(index,out int tile)) return; used.Remove(index); filled[index]=""; slots[index].caption.text="_"; tiles[tile].Available(true); }
        public override void Submit() { if(!Ready) return; bool correct=AnswerText==string.Concat(spelling);
            if(correct) Session.audioService.Play(Current.word.pronunciation);
            if(!Answer(correct,string.Join("|",filled),correct ? "Label ready: "+Current.word.lemko : "Not quite. Check each letter.")) return;
            // Right: the letters do a little wave. Wrong: the whole label shakes (no letter is singled out).
            for(int i=0;i<slots.Count;i++) if(slots[i]) { if(correct) Juice.Hop(slots[i].transform,.28f,i*.05f); else Juice.Shake(slots[i].transform,.08f); } }
    }
}
