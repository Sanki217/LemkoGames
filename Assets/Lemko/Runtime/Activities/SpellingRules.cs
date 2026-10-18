using System.Collections.Generic;
using System.Globalization;
using System.Text;
namespace Lemko
{
    public static class SpellingRules
    {
        // NFC only: preserve case, accents and each distinct apostrophe; separators are fixed slots.
        public static List<string> Elements(string text) {
            var result=new List<string>(); var e=StringInfo.GetTextElementEnumerator((text??"").Normalize(NormalizationForm.FormC));
            while(e.MoveNext()) result.Add(e.GetTextElement()); return result;
        }
        public static bool Fixed(string value) => string.IsNullOrWhiteSpace(value) || value=="'" || value=="’" || value=="ʼ" || value=="-";
        public static List<string> Letters(string text) => Elements(text).FindAll(v=>!Fixed(v));
    }
}
