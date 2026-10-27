using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Lemko
{
    public sealed class NumbersController : LearningActivity
    {
        [Header("Ticket numbers (uses supplied forms only)")]
        [Range(1,99)] public int minimumNumber=1;
        [Range(1,99)] public int maximumNumber=20;
        [Header("First session introduction")]
        public bool introduceSmallNumbers=true;
        [Range(2,12)] public int introductoryMaximum=5;
        readonly List<int> issued=new List<int>();
        public int TicketsIssued=>issued.Count;
        public int[] Options {get;private set;}
        protected override List<LearningTask> AvailableTasks()=>definition.tasks.Where(t=>t.quantity>=minimumNumber && t.quantity<=(introduceSmallNumbers && Session && !Session.FarmQuantitiesUnlocked ? Mathf.Min(maximumNumber,introductoryMaximum) : maximumNumber)).ToList();
        protected override string ValidateContent() {
            var error=base.ValidateContent(); if(error!=null) return error;
            if(minimumNumber<1 || maximumNumber>99 || minimumNumber>maximumNumber || AvailableTasks().Select(t=>t.quantity).Distinct().Count()<2)
                return "Ticket range needs at least two supplied number forms within 1–99.";
            return null;
        }
        protected override void BuildTask()
        {
            submit.gameObject.SetActive(false); prompt.text="Issue a gathering ticket marked: "+Current.word.lemko;
            var options=AvailableTasks().Select(t=>t.quantity).Distinct().Where(n=>n!=Current.quantity).ToList(); Shuffle(options);
            options=options.Take(3).ToList(); options.Add(Current.quantity); Shuffle(options); Options=options.ToArray();
            for(int i=0;i<Options.Length;i++) {
                int n=Options[i]; var ticket=Token(Current.word,new Vector2(-4.5f+i*3,0),n.ToString(),t=>Choose(n),n.ToString(),false);
                ticket.GetComponent<HintPointer>().enabled=false; ticket.backing.color=new Color(.95f,.82f,.48f);
            }
            for(int i=0;i<Mathf.Min(5,issued.Count);i++) DrawIssued(issued[issued.Count-Mathf.Min(5,issued.Count)+i],i);
            feedback.text="Read the Lemko request or play its recording, then choose the matching ticket. Tickets ready: "+issued.Count+".";
        }
        void DrawIssued(int number,int position) {
            var token=Token(Current.word,new Vector2(-2.4f+position*1.2f,-1.8f),"issued",null,number.ToString(),false);
            token.transform.localScale=Vector3.one*.45f; token.GetComponent<HintPointer>().enabled=false; token.backing.color=new Color(.55f,.85f,.6f);
        }
        public void Choose(int number) {
            if(!Ready || Options==null || !Options.Contains(number)) return;
            bool correct=number==Current.quantity;
            if(Answer(correct,number.ToString(),correct ? "Ticket "+number+" issued for the gathering." : "That ticket has a different number. Listen again or hold the prompt for a hint.") && correct) {
                issued.Add(number);
                // Keep the preview bounded for longer Inspector-configured sessions.
                foreach(var old in tokens.Where(t=>t && t.Key=="issued").ToArray()) { old.gameObject.SetActive(false); Destroy(old.gameObject); tokens.Remove(old); }
                for(int i=0;i<Mathf.Min(5,issued.Count);i++) DrawIssued(issued[issued.Count-Mathf.Min(5,issued.Count)+i],i);
            }
        }
        public override void Submit() { }
        public override void Close() { base.Close(); issued.Clear(); Options=null; }
    }
}
