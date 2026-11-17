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
        sealed class Seen { public TMP_Text label; public string source, shown; public bool typed, polish; }
        readonly HashSet<TMP_Text> known=new HashSet<TMP_Text>();
        readonly List<Seen> labels=new List<Seen>();
        void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(Track);
            foreach(var label in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None)) Track(label);
        }
        void OnDisable() => TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(Track);
        void Track(Object changed)
        {
            if(!(changed is TMP_Text label) || !label || !known.Add(label)) return;
            var field=label.GetComponentInParent<TMP_InputField>();
            labels.Add(new Seen { label=label, typed=field && field.textComponent==label });
        }
        void LateUpdate()
        {
            for(int i=labels.Count-1;i>=0;i--) {
                var entry=labels[i]; var text=entry.label;
                if(!text) { known.Remove(text); labels.RemoveAt(i); continue; }
                if(!text.isActiveAndEnabled) continue;
                if(ScenePolish.ActiveFont && text.font!=ScenePolish.ActiveFont) text.font=ScenePolish.ActiveFont;
                if(entry.typed) continue;
                string now=text.text;
                if(entry.shown!=null && now==entry.shown) { if(entry.polish==Lang.Polish) continue; }
                else entry.source=now;
                entry.polish=Lang.Polish;
                entry.shown=Lang.T(entry.source);
                if(entry.shown!=now) text.text=entry.shown;
            }
        }
    }
}
