using UnityEngine;

namespace Lemko
{
    [CreateAssetMenu(menuName = "Lemko/Vocabulary entry")]
    public sealed class VocabularyEntry : ScriptableObject
    {
        public string id;
        [TextArea] public string lemko;
        public string polish;
        public string english;
        public Sprite sprite;
        public AudioClip pronunciation;
        public string[] tags;
        public bool verified;
        [TextArea] public string notes;
        public string Translation(bool useEnglish) => useEnglish ? english : polish;
    }
}
