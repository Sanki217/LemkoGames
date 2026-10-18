using System.Linq;
using UnityEngine;
namespace Lemko
{
    public sealed class NumbersController : LearningActivity
    {
        public int[] Options {get;private set;}
        protected override void BuildTask()
        {
            submit.gameObject.SetActive(false); prompt.text="Choose the numeral: "+Current.word.lemko;
            var options=definition.tasks.Select(t=>t.quantity).Distinct().Where(n=>n!=Current.quantity).ToList(); Shuffle(options);
            options=options.Take(3).ToList(); options.Add(Current.quantity); Shuffle(options); Options=options.ToArray();
            for(int i=0;i<Options.Length;i++) { int n=Options[i]; Token(Current.word,new Vector2(-4.5f+i*3,0),n.ToString(),t=>Choose(n),n.ToString(),false).GetComponent<HintPointer>().enabled=false; }
        }
        public void Choose(int number) { if(Options==null || !Options.Contains(number)) return; Answer(number==Current.quantity,number.ToString(),"Listen again or hold the prompt for a hint."); }
        public override void Submit() { }
    }
}
