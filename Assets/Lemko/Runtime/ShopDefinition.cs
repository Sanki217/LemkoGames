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
        [Tooltip("Number of products displayed from the full vocabulary pool each round.")]
        [Range(2,12)] public int displayedProductCount = 8;
        public string shoppingListTitle = "SHOPPING LIST";
        public string Validate()
        {
            if (startingLives < 1 || taskCount < 1 || taskCount > 8) return "Invalid life or task count.";
            var ids = new HashSet<string>();
            foreach (var word in products)
                if (!word || string.IsNullOrWhiteSpace(word.id) || string.IsNullOrWhiteSpace(word.lemko) || !ids.Add(word.id))
                    return "Shop contains a missing word, blank ID/text or duplicate ID.";
            if (products.Count < taskCount + 1) return "Shop needs at least task count + 1 unique products (including a distractor).";
            if(displayedProductCount < taskCount+1 || displayedProductCount > 12) return "Displayed product count must be between task count + 1 and 12.";
            return null;
        }
    }
}
