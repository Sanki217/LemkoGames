using System.Collections.Generic;
using System.Linq;
namespace Lemko {
    public static class PracticeSelection {
        public static List<T> Order<T>(IEnumerable<T> shuffled,System.Func<T,VocabularyEntry> word,GameSession session,int length) {
            var all=shuffled.ToList();
            var familiar=all.Where(t=>session.Progress.encountered.Contains(word(t).id)).ToList();
            var fresh=all.Where(t=>!session.Progress.encountered.Contains(word(t).id)).ToList();
            var review=familiar.OrderByDescending(t=>session.ReviewPriority(word(t))).ToList();
            var chosen=new List<T>();
            int newCount=System.Math.Min(2,System.Math.Max(0,length-review.Count));
            if(fresh.Count>0) newCount=System.Math.Max(1,newCount);
            chosen.AddRange(review.Take(System.Math.Max(0,length-newCount)));
            chosen.AddRange(fresh.Take(newCount));
            // Small starter pools must still produce valid, complete sessions.
            chosen.AddRange(all.Where(t=>!chosen.Contains(t)).Take(System.Math.Max(0,length-chosen.Count)));
            chosen.AddRange(all.Where(t=>!chosen.Contains(t)));
            return chosen;
        }
    }
}
