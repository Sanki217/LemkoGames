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
        // The fenced pasture drawn in the backdrop: animals put there wander inside it.
        static readonly Rect Pasture=new Rect(-1.8f,-2f,6.8f,2f);
        readonly List<ActivityToken> placed=new List<ActivityToken>();
        // Every task brings a fresh group to the shelter: the animal asked for and five others from the whole farm.
        const int ShelterSize=6;
        readonly List<VocabularyEntry> shelter=new List<VocabularyEntry>();
        public IReadOnlyList<ActivityToken> Placed => placed;
        protected override void BuildTask()
        {
            placed.Clear(); IsRepairTask=QuantityTasks && enableRepairTasks && (Correct+1)%Mathf.Max(2,repairEvery)==0;
            prompt.text=QuantityTasks ? (IsRepairTask ? "Fix the pen: " : "Fill the pen: ")+Look.Word(Current.quantity+" × "+Current.word.lemko) : "Find "+Look.Word(Current.word.lemko);
            counter=QuantityTasks ? Badge(new Vector2(6.3f,.3f),.9f,Look.Honey) : null; if(counter) counter.text="0";
            var zone=Zone(new Vector2(1.5f,-1.3f),new Vector2(8.4f,3.3f),t=>Add(t.view.Entry));
            zone.droppedAt=(t,point)=>Add(t.view.Entry,taskRoot.InverseTransformPoint(point)); zone.GetComponent<SpriteRenderer>().color=Color.clear;
            // The animals wait in the shelter on the left.
            var others=definition.pool.Where(w=>w && w!=Current.word).Distinct().ToList(); Shuffle(others);
            shelter.Clear(); shelter.Add(Current.word); shelter.AddRange(others.Take(ShelterSize-1)); Shuffle(shelter);
            // They stand in the stalls drawn in the backdrop, at full size and without a card behind them.
            for(int i=0;i<shelter.Count;i++) {
                var token=Token(shelter[i],new Vector2(-7f+(i%2)*1.675f,1.4f-(i/2)*1.65f),"tray",t=>Add(t.view.Entry),draggable:true);
                token.backing.enabled=false; token.view.SpeakerOnPicture();
            }
            if(IsRepairTask) {
                // Rotate missing, extra and wrong-animal arrangements; every one can be corrected using the tray.
                int variant=((Correct+1)/Mathf.Max(2,repairEvery)-1)%3;
                int count=variant==1 ? Mathf.Min(12,Current.quantity+1) : Mathf.Max(0,Current.quantity-1);
                if(variant==1 && count==Current.quantity) count=Current.quantity-1;
                for(int i=0;i<count;i++) Add(Current.word);
                if(variant==2) { var other=shelter.FirstOrDefault(w=>w!=Current.word); if(other) Add(other); }
            }
            feedback.text=!QuantityTasks ? "Put the animal in the pasture." : IsRepairTask ? "Something is wrong in this pen." : "Drag or tap animals into the pen. Tap one to remove it.";
        }
        public bool Add(VocabularyEntry word)=>Add(word,null);
        public bool Add(VocabularyEntry word,Vector2? at)
        {
            if(!Ready || !definition.pool.Contains(word) || placed.Count>=12) return false;
            if(!QuantityTasks) { foreach(var old in placed) { old.gameObject.SetActive(false); Destroy(old.gameObject); } placed.Clear(); }
            var spot=at ?? new Vector2(Random.Range(Pasture.xMin,Pasture.xMax),Random.Range(Pasture.yMin,Pasture.yMax));
            spot=new Vector2(Mathf.Clamp(spot.x,Pasture.xMin,Pasture.xMax),Mathf.Clamp(spot.y,Pasture.yMin,Pasture.yMax));
            var token=Token(word,spot,"placed",Remove); placed.Add(token);
            token.backing.enabled=false; if(token.view.worldSpeaker) token.view.worldSpeaker.SetActive(false); Arrange(); Juice.Drop(token.transform,1.1f); Juice.Dust(Juice.WorldToScreen(token.transform.position+Vector3.down*.4f)); Juice.Sound("drop",1+placed.Count*.05f);
            var wander=token.gameObject.AddComponent<AnimalWander>(); wander.session=Session; wander.area=Pasture; wander.picture=token.view.worldPicture; return true;
        }
        public void Remove(ActivityToken token) { if(!Ready || !placed.Remove(token)) return; token.gameObject.SetActive(false); Destroy(token.gameObject); Arrange(); }
        TMP_Text counter;
        protected override bool HasAnswer => placed.Count>0;
        void Arrange() { if(counter) { counter.text=placed.Count.ToString(); Juice.Punch(counter.transform.parent,.25f,.25f); }
        }
        public override void Submit() => Answer(placed.Count==(QuantityTasks ? Current.quantity : 1) && placed.All(t=>t.view.Entry==Current.word),string.Join("|",placed.Select(t=>t.view.Entry.id).OrderBy(s=>s)),placed.Count==(QuantityTasks ? Current.quantity : 1) && placed.All(t=>t.view.Entry==Current.word) ? "The pen is ready!" : QuantityTasks ? (placed.Any(t=>t.view.Entry!=Current.word) ? "One animal is a different kind." : "You have "+placed.Count+", not "+Current.quantity+".") : "That is a different animal.");
    }
}
