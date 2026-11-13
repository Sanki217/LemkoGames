using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Lemko
{
    public sealed class FarmController : LearningActivity
    {
        [Header("Pen repair tasks")]
        public bool enableRepairTasks=true;
        [Min(2)] public int repairEvery=2;
        public bool IsRepairTask {get;private set;}
        public bool QuantityTasks => Session && Session.FarmQuantitiesUnlocked;
        [Range(1,12)] public int maximumQuantity=5;
        protected override List<LearningTask> AvailableTasks() => definition.tasks.Where(t=>!QuantityTasks || t.quantity<=maximumQuantity).GroupBy(t=>QuantityTasks ? t.id : t.word.id).Select(g=>g.First()).ToList();
        protected override string ValidateContent() { var error=base.ValidateContent(); return error ?? (AvailableTasks().Count==0 ? "No Farm tasks in the selected quantity range." : null); }
        readonly List<ActivityToken> placed=new List<ActivityToken>();
        public IReadOnlyList<ActivityToken> Placed => placed;
        protected override void BuildTask()
        {
            placed.Clear(); IsRepairTask=QuantityTasks && enableRepairTasks && (Correct+1)%Mathf.Max(2,repairEvery)==0;
            prompt.text=QuantityTasks ? (IsRepairTask ? "Fix the pen: " : "Fill the pen: ")+Look.Word(Current.quantity+" × "+Current.word.lemko) : "Find "+Look.Word(Current.word.lemko);
            counter=QuantityTasks ? Badge(new Vector2(5.6f,1.7f),.9f,Look.Honey) : null; if(counter) counter.text="0";
            Zone(new Vector2(2,-.5f),new Vector2(7,4),t=>Add(t.view.Entry));
            for(int i=0;i<definition.pool.Count;i++) Token(definition.pool[i],new Vector2(-5.8f+(i%2)*2,-.1f-(i/2)*1.8f),"tray",t=>Add(t.view.Entry),draggable:true);
            if(IsRepairTask) {
                // Rotate missing, extra and wrong-animal arrangements; every one can be corrected using the tray.
                int variant=((Correct+1)/Mathf.Max(2,repairEvery)-1)%3;
                int count=variant==1 ? Mathf.Min(12,Current.quantity+1) : Mathf.Max(0,Current.quantity-1);
                if(variant==1 && count==Current.quantity) count=Current.quantity-1;
                for(int i=0;i<count;i++) Add(Current.word);
                if(variant==2) { var other=definition.pool.FirstOrDefault(w=>w!=Current.word); if(other) Add(other); }
            }
            feedback.text=!QuantityTasks ? "Put the animal in the pen." : IsRepairTask ? "Something is wrong in this pen." : "Drag or tap animals into the pen. Tap one to remove it.";
        }
        public bool Add(VocabularyEntry word)
        {
            if(!Ready || !definition.pool.Contains(word) || placed.Count>=12) return false;
            if(!QuantityTasks) { foreach(var old in placed) { old.gameObject.SetActive(false); Destroy(old.gameObject); } placed.Clear(); }
            var token=Token(word,Vector2.zero,"placed",Remove); placed.Add(token); Arrange(); Juice.Drop(token.transform,1.1f); Juice.Dust(Juice.WorldToScreen(token.transform.position+Vector3.down*.4f)); Juice.Sound("drop",1+placed.Count*.05f);
            var idle=token.view.worldPicture.gameObject.AddComponent<AnimalIdle>(); idle.session=Session; return true;
        }
        public void Remove(ActivityToken token) { if(!Ready || !placed.Remove(token)) return; token.gameObject.SetActive(false); Destroy(token.gameObject); Arrange(); }
        TMP_Text counter;
        protected override bool HasAnswer => placed.Count>0;
        void Arrange() { if(counter) { counter.text=placed.Count.ToString(); Juice.Punch(counter.transform.parent,.25f,.25f); }
            for(int i=0;i<placed.Count;i++) { placed[i].transform.localPosition=new Vector2(-.4f+(i%4)*1.55f,.8f-(i/4)*1.35f); placed[i].transform.localScale=Vector3.one*.75f; } }
        public override void Submit() => Answer(placed.Count==(QuantityTasks ? Current.quantity : 1) && placed.All(t=>t.view.Entry==Current.word),string.Join("|",placed.Select(t=>t.view.Entry.id).OrderBy(s=>s)),placed.Count==(QuantityTasks ? Current.quantity : 1) && placed.All(t=>t.view.Entry==Current.word) ? "The pen is ready!" : QuantityTasks ? (placed.Any(t=>t.view.Entry!=Current.word) ? "One animal is a different kind." : "You have "+placed.Count+", not "+Current.quantity+".") : "That is a different animal.");
    }
}
