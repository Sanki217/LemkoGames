using UnityEngine;

namespace Lemko
{
    [CreateAssetMenu(menuName = "Lemko/Vocabulary entry")]
    public sealed class VocabularyEntry : ScriptableObject
    {
        [System.Serializable] public struct LocalizedText { public string language; [TextArea] public string text; }
        public string id;
        [TextArea] public string lemko;
        public string polish;
        public string english;
        public Sprite sprite;
        public AudioClip pronunciation;
        public string[] tags;
        public bool verified;
        [TextArea] public string notes;
        public string spriteId, audioId;
        public LocalizedText[] translations;
        public string Translation(bool useEnglish) => Translation(useEnglish ? "en" : "pl");
        public string Translation(string language)
        {
            if(language=="lemko") return lemko;
            if(language=="pl") return polish;
            if(language=="en") return english;
            if(translations!=null) foreach(var item in translations) if(item.language==language) return item.text;
            return english;
        }
    }
}
