using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Lemko
{
    public sealed class AlphabetController : LearningActivity
    {
        List<string> spelling;
        string[] filled;
        readonly Dictionary<int,ActivityToken> tiles=new Dictionary<int,ActivityToken>();
        readonly Dictionary<int,int> used=new Dictionary<int,int>();
        readonly List<ActivityToken> slots=new List<ActivityToken>();
        public string AnswerText => string.Concat(filled);
        protected override void BuildTask()
        {
            prompt.text="Spell the picture: "+Current.word.polish; prompt.GetComponent<HintPointer>().Bind(Current.word,Session.tooltip);
            Token(Current.word,new Vector2(-5.7f,1),"object",t=>Speak());
            spelling=SpellingRules.Elements(Current.word.lemko); filled=new string[spelling.Count]; tiles.Clear(); used.Clear(); slots.Clear();
            for(int i=0;i<spelling.Count;i++) {
                int index=i; var pos=new Vector2(-4.8f+i*.88f,-.4f); bool fixedSlot=SpellingRules.Fixed(spelling[i]); filled[i]=fixedSlot ? spelling[i] : "";
                var slot=Token(Current.word,pos,"slot",t=>Remove(index),fixedSlot?spelling[i]:"_",false); slot.transform.localScale=Vector3.one*.5f; slot.GetComponent<HintPointer>().enabled=false; slots.Add(slot);
                if(!fixedSlot) Zone(pos,new Vector2(.8f,1),t=>Place(int.Parse(t.Key),index));
            }
            var letters=SpellingRules.Letters(Current.word.lemko); letters.AddRange(SpellingRules.Letters(definition.extraLetters).Take(4)); Shuffle(letters);
            for(int i=0;i<letters.Count;i++) { int tile=i; var token=Token(Current.word,new Vector2(-5.2f+(i%12)*.9f,-1.7f-(i/12)),i.ToString(),t=>PlaceFirst(tile),letters[i],false,true); token.transform.localScale=Vector3.one*.5f; token.GetComponent<HintPointer>().enabled=false; tiles[i]=token; }
            feedback.text="Click letters or drag them into slots. Click a filled slot to return its tile. Spaces and apostrophes are fixed.";
        }
        public void PlaceFirst(int tile) { int index=System.Array.FindIndex(filled,s=>s==""); if(index>=0) Place(tile,index); }
        public bool Place(int tile,int index)
        {
            if(!Ready || !tiles.ContainsKey(tile) || used.ContainsValue(tile) || index<0 || index>=filled.Length || filled[index]!="") return false;
            filled[index]=tiles[tile].caption.text; used[index]=tile; tiles[tile].Available(false); slots[index].caption.text=filled[index]; return true;
        }
        public void Remove(int index) { if(!Ready || !used.TryGetValue(index,out int tile)) return; used.Remove(index); filled[index]=""; slots[index].caption.text="_"; tiles[tile].Available(true); }
        public override void Submit() { if(!Ready) return; Answer(AnswerText==string.Concat(spelling),string.Join("|",filled),"Check each letter, including repeated letters and accents."); }
    }
}
