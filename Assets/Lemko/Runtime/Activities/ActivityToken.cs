using System;
using TMPro;
using UnityEngine;
namespace Lemko
{
    public sealed class ActivityToken : MonoBehaviour
    {
        public WordView view;
        public TMP_Text caption;
        public SpriteRenderer backing;
        public string Key {get;private set;}
        public bool FaceUp {get;private set;}
        public Action<ActivityToken> clicked;
        public void Bind(VocabularyEntry word,GameSession session,string key,Action<ActivityToken> action,string text=null,bool picture=true)
        {
            Key=key; clicked=action; view.Bind(word,session.tooltip,session.audioService,_=>clicked?.Invoke(this));
            caption.text=text??""; caption.gameObject.SetActive(text!=null);
            view.worldPicture.gameObject.SetActive(picture); if(!picture && view.worldSpeaker) view.worldSpeaker.SetActive(false); FaceUp=true;
        }
        public void Face(bool visible,bool picture,string text)
        {
            FaceUp=visible; view.worldPicture.gameObject.SetActive(visible && picture);
            caption.gameObject.SetActive(!visible || !picture); caption.text=visible ? text : "?";
            var hint=GetComponent<HintPointer>(); hint.enabled=visible;
            if(view.worldSpeaker) view.worldSpeaker.SetActive(visible && view.Entry && view.Entry.pronunciation);
            backing.color=visible ? new Color(.96f,.91f,.76f) : new Color(.22f,.42f,.5f);
        }
        public void Click() => clicked?.Invoke(this);
        public void Available(bool value) { foreach(var r in GetComponentsInChildren<Renderer>()) r.enabled=value; foreach(var c in GetComponentsInChildren<Collider2D>()) c.enabled=value; }
        void OnDisable() { clicked=null; }
    }
}
