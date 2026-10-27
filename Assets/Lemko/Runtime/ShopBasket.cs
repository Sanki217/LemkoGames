using System;
using System.Collections.Generic;
using System.Linq;
namespace Lemko
{
    public sealed class ShopBasket
    {
        readonly HashSet<string> required, available, selected=new HashSet<string>(), rejected=new HashSet<string>();
        public bool Complete {get;private set;}
        public int Mistakes {get;private set;}
        public int Count=>selected.Count;
        public IReadOnlyCollection<string> Selected=>selected;
        public string[] Missing=>required.Except(selected).ToArray();
        public string[] Extra=>selected.Except(required).ToArray();
        public ShopBasket(IEnumerable<string> targets,IEnumerable<string> products)
        {
            required=new HashSet<string>(targets); available=new HashSet<string>(products);
            if(required.Count==0 || !required.IsSubsetOf(available)) throw new ArgumentException("Unsolvable basket.");
        }
        public bool Toggle(string id)
        {
            if(Complete || id==null || !available.Contains(id)) return false;
            if(!selected.Remove(id)) selected.Add(id); return true;
        }
        public bool Contains(string id)=>selected.Contains(id);
        public SelectionResult Submit()
        {
            if(Complete || selected.Count==0) return SelectionResult.Ignored;
            if(required.SetEquals(selected)) { Complete=true; return SelectionResult.Correct; }
            string signature=string.Join("|",selected.OrderBy(s=>s,StringComparer.Ordinal));
            if(!rejected.Add(signature)) return SelectionResult.Ignored;
            Mistakes++; return SelectionResult.Incorrect;
        }
    }
}
