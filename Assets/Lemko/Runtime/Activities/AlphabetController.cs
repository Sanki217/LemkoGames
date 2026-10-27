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
            IsRepairTask=enableRepairLabels && (Correct+1)%Mathf.Max(2,repairEvery)==0;
            prompt.text=(IsRepairTask ? "Repair this shelf label: " : "Make a shelf label: ")+Current.word.polish; prompt.GetComponent<HintPointer>().Bind(Current.word,Session.tooltip,HintContent.LemkoSpelling);
            if(hintControl) hintControl.Bind(Current.word,Session.tooltip,HintContent.LemkoSpelling);
            var picture=Token(Current.word,new Vector2(-5.7f,1.2f),"object",t=>Speak());
            picture.GetComponent<HintPointer>().Bind(Current.word,Session.tooltip,HintContent.LemkoSpelling);
            spelling=SpellingRules.Elements(Current.word.lemko); filled=new string[spelling.Count]; tiles.Clear(); used.Clear(); slots.Clear();
            for(int i=0;i<spelling.Count;i++) {
                int index=i; var pos=new Vector2(-(spelling.Count-1)*Spacing*.5f+i*Spacing,-.4f); bool fixedSlot=SpellingRules.Fixed(spelling[i]); filled[i]=fixedSlot ? spelling[i] : "";
                var slot=Token(Current.word,pos,"slot",t=>Remove(index),fixedSlot?spelling[i]:"_",false); StyleLetter(slot,false); slot.GetComponent<HintPointer>().enabled=false; slots.Add(slot);
                if(!fixedSlot) Zone(pos,TileSize+new Vector2(.02f,.04f),t=>Place(int.Parse(t.Key),index));
            }
            var letters=SpellingRules.Letters(Current.word.lemko);
            var extras=SpellingRules.Letters(definition.extraLetters);
            if(extras.Count==0 && definition.alphabetLetters!=null) {
                extras=definition.alphabetLetters.Select(l=>l.lowercase).Where(l=>!letters.Contains(l)).Distinct().ToList();
                Shuffle(extras);
            }
            letters.AddRange(extras.Take(4)); Shuffle(letters);
            for(int i=0;i<letters.Count;i++) { int tile=i; var token=Token(Current.word,new Vector2(-5.5f*Spacing+(i%12)*Spacing,-1.65f-(i/12)*(TileSize.y+.23f)),i.ToString(),t=>PlaceFirst(tile),letters[i],false,true); StyleLetter(token,true); token.GetComponent<HintPointer>().enabled=false; tiles[i]=token; }
            if(IsRepairTask) {
                var positions=Enumerable.Range(0,spelling.Count).Where(i=>!SpellingRules.Fixed(spelling[i])).ToList(); Shuffle(positions);
                foreach(int index in positions.Skip(Mathf.Max(1,missingLetters))) FillCorrect(index);
                if(used.Count>0) Session.MarkSupported(Current.word);
            }
            feedback.text=(IsRepairTask ? "Replace the missing letters. " : "")+"Click or drag a letter. Click a filled slot to undo. Hold the picture or meaning for the Lemko spelling.";
        }
        Vector2 TileSize => Session.presentation ? Session.presentation.letterSize : new Vector2(.72f,.82f);
        float Spacing => Session.presentation ? Session.presentation.letterSpacing : .96f;
        void StyleLetter(ActivityToken token,bool tile) {
            token.transform.localScale=Vector3.one;
            var size=token.backing.sprite.bounds.size;
            token.backing.transform.localScale=new Vector3(TileSize.x/size.x,TileSize.y/size.y,1);
            token.backing.color=Session.presentation ? (tile ? Session.presentation.letterColor : Session.presentation.slotColor) : (tile ? new Color(1,.84f,.48f) : new Color(.75f,.88f,.85f));
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
            if(FillCorrect(index)) { Session.MarkSupported(Current.word); RecordHint(); feedback.text="One letter supplied. You can still move or remove it; finish the label and Submit."; }
        }
        public void PlaceFirst(int tile) { int index=System.Array.FindIndex(filled,s=>s==""); if(index>=0) Place(tile,index); }
        public bool Place(int tile,int index)
        {
            if(!Ready || !tiles.ContainsKey(tile) || used.ContainsValue(tile) || index<0 || index>=filled.Length || filled[index]!="") return false;
            filled[index]=tiles[tile].caption.text; used[index]=tile; tiles[tile].Available(false); slots[index].caption.text=filled[index]; return true;
        }
        public void Remove(int index) { if(!Ready || !used.TryGetValue(index,out int tile)) return; used.Remove(index); filled[index]=""; slots[index].caption.text="_"; tiles[tile].Available(true); }
        public override void Submit() { if(!Ready) return; Answer(AnswerText==string.Concat(spelling),string.Join("|",filled),AnswerText==string.Concat(spelling) ? "Shelf label ready: "+Current.word.lemko : "Check each letter, including repeated letters and accents."); }
    }
}
