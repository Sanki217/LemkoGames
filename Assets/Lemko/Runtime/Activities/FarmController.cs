using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Lemko
{
    public sealed class FarmController : LearningActivity
    {
        readonly List<ActivityToken> placed=new List<ActivityToken>();
        public IReadOnlyList<ActivityToken> Placed => placed;
        protected override void BuildTask()
        {
            placed.Clear(); prompt.text="Bring "+Current.quantity+" × "+Current.word.lemko+" to the pen";
            Zone(new Vector2(2,-.5f),new Vector2(7,4),t=>Add(t.view.Entry));
            for(int i=0;i<definition.pool.Count;i++) Token(definition.pool[i],new Vector2(-5.8f+(i%2)*2,-.1f-(i/2)*1.8f),"tray",t=>Add(t.view.Entry),draggable:true);
            feedback.text="Drag animals into the pen (or click to add). Click a placed animal to remove it, then Submit.";
        }
        public bool Add(VocabularyEntry word)
        {
            if(!Ready || !definition.pool.Contains(word) || placed.Count>=12) return false;
            var token=Token(word,Vector2.zero,"placed",Remove); placed.Add(token); Arrange(); return true;
        }
        public void Remove(ActivityToken token) { if(!Ready || !placed.Remove(token)) return; token.gameObject.SetActive(false); Destroy(token.gameObject); Arrange(); }
        void Arrange() { for(int i=0;i<placed.Count;i++) { placed[i].transform.localPosition=new Vector2(-.4f+(i%4)*1.55f,.8f-(i/4)*1.35f); placed[i].transform.localScale=Vector3.one*.75f; } }
        public override void Submit() => Answer(placed.Count==Current.quantity && placed.All(t=>t.view.Entry==Current.word),string.Join("|",placed.Select(t=>t.view.Entry.id).OrderBy(s=>s)),"Match both the animal and requested quantity.");
    }
}
