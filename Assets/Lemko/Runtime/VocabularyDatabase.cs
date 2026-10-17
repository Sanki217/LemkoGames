using System.Collections.Generic;
using UnityEngine;
namespace Lemko
{
    [CreateAssetMenu(menuName = "Lemko/Vocabulary database")]
    public sealed class VocabularyDatabase : ScriptableObject
    {
        public List<VocabularyEntry> entries = new List<VocabularyEntry>();
    }
}
