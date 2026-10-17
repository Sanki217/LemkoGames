using System.Collections.Generic;
using System.Linq;
namespace Lemko
{
    public enum SelectionResult { Ignored, Correct, Incorrect }
    public sealed class ShopRound
    {
        readonly HashSet<string> required;
        readonly HashSet<string> available;
        readonly HashSet<string> submitted = new HashSet<string>();
        public int Correct { get; private set; }
        public int Mistakes { get; private set; }
        public int Total => required.Count;
        public bool Complete => Correct == Total;
        public ShopRound(IEnumerable<string> targets, IEnumerable<string> products)
        {
            required = new HashSet<string>(targets);
            available = new HashSet<string>(products);
            if (required.Count == 0 || !required.IsSubsetOf(available)) throw new System.ArgumentException("Unsolvable shopping list.");
        }
        public SelectionResult Select(string id)
        {
            if (Complete || id == null || !available.Contains(id) || !submitted.Add(id)) return SelectionResult.Ignored;
            if (required.Contains(id)) { Correct++; return SelectionResult.Correct; }
            Mistakes++; return SelectionResult.Incorrect;
        }
        public bool IsComplete(string id) => required.Contains(id) && submitted.Contains(id);
    }
}
