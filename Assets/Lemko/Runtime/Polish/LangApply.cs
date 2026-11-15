using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Lemko
{
    // Shows every text in the chosen language. The game keeps writing its texts in English; after everything else
    // has run in a frame, each text that changed is translated (see Lang) before it is drawn. The English original
    // is remembered, so switching the language back and forth changes what is on screen at once.
    [DefaultExecutionOrder(10000)]
    public sealed class LangApply : MonoBehaviour
    {
        sealed class Seen { public string source, shown; public bool typed; }
        readonly Dictionary<TMP_Text,Seen> seen=new Dictionary<TMP_Text,Seen>();
        readonly List<TMP_Text> gone=new List<TMP_Text>();
        bool polish; int sweep;
        void LateUpdate()
        {
            bool switched=polish!=Lang.Polish; polish=Lang.Polish;
            foreach(var text in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)) {
                if(!seen.TryGetValue(text,out var entry)) {
                    // What the player types into a search box is never translated.
                    var field=text.GetComponentInParent<TMP_InputField>();
                    entry=new Seen{ typed=field && field.textComponent==text }; seen[text]=entry;
                }
                if(entry.typed) continue;
                string now=text.text;
                if(entry.shown!=null && now==entry.shown) { if(!switched) continue; }
                else entry.source=now;
                entry.shown=Lang.T(entry.source);
                if(entry.shown!=now) text.text=entry.shown;
            }
            if(++sweep<600) return;
            sweep=0; gone.Clear(); foreach(var text in seen.Keys) if(!text) gone.Add(text);
            foreach(var text in gone) seen.Remove(text);
        }
    }
}
