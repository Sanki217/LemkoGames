using System.Collections.Generic;
using UnityEngine;
namespace Lemko
{
    [CreateAssetMenu(menuName = "Lemko/Shop definition")]
    public sealed class ShopDefinition : ScriptableObject
    {
        public List<VocabularyEntry> products = new List<VocabularyEntry>();
        [Range(1, 8)] public int taskCount = 5;
        [Min(1)] public int startingLives = 5;
        public string Validate()
        {
            if (startingLives < 1 || taskCount < 1 || taskCount > 8) return "Invalid life or task count.";
            var ids = new HashSet<string>();
            foreach (var word in products)
                if (!word || string.IsNullOrWhiteSpace(word.id) || string.IsNullOrWhiteSpace(word.lemko) || !ids.Add(word.id))
                    return "Shop contains a missing word, blank ID/text or duplicate ID.";
            if (products.Count < taskCount + 1 || products.Count > 12) return "Shop needs task count + 1 to 12 unique products (including a distractor).";
            return null;
        }
    }
}
