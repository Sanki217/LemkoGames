using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Lemko
{
    public sealed class FamilyTreeController : LearningActivity
    {
        [Header("Session invitation album")]
        public TMP_Text album;
        readonly HashSet<string> invited=new HashSet<string>();
        readonly List<string> delivered=new List<string>();
        public int InvitationsDelivered=>delivered.Count;
        protected override void BuildTask()
        {
            submit.gameObject.SetActive(false); prompt.text="Deliver an invitation. From YOU, find: "+Current.word.lemko;
            foreach(var person in definition.people) {
                var p=person; var token=Token(p.appearance,p.position,p.id,t=>Choose(p.id),p.id==definition.referencePerson ? "YOU" : null);
                if(invited.Contains(p.id)) token.backing.color=new Color(.55f,.85f,.6f);
                if(p.id==definition.referencePerson) { token.backing.color=Color.yellow; token.caption.transform.localPosition=new Vector3(0,-.6f,0); token.caption.rectTransform.sizeDelta=new Vector2(1.7f,.4f); token.caption.enableAutoSizing=true; token.caption.fontSizeMin=1; token.caption.fontSizeMax=6; }
                if(!string.IsNullOrEmpty(p.parentId)) { var parent=definition.people.Find(x=>x.id==p.parentId); if(parent!=null) Link(parent.position,p.position); }
                if(!string.IsNullOrEmpty(p.secondParentId)) { var parent=definition.people.Find(x=>x.id==p.secondParentId); if(parent!=null) Link(parent.position,p.position); }
            }
            RefreshAlbum(); feedback.text="Invite the requested relative to our gathering. YOU is highlighted; every valid relative is accepted.";
        }
        void Link(Vector2 a,Vector2 b)
        {
            var go=new GameObject("Family connection",typeof(SpriteRenderer)); go.transform.SetParent(taskRoot,false); go.transform.localPosition=(a+b)/2;
            go.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg);
            go.transform.localScale=new Vector3(Vector2.Distance(a,b),.045f,1); var r=go.GetComponent<SpriteRenderer>(); r.sprite=tokenPrefab.backing.sprite; r.color=Color.white; r.sortingOrder=-2;
        }
        void RefreshAlbum() { if(album) album.text="Invitations delivered: "+delivered.Count+" / "+definition.tasksPerSession+"\n"+string.Join("  •  ",delivered); }
        public void Choose(string person) {
            if(!Ready || !definition.people.Exists(p=>p.id==person)) return;
            bool correct=Array.IndexOf(Current.validPeople,person)>=0;
            if(Answer(correct,person,correct ? "Invitation delivered! This relative can join the gathering." : "Follow the connections from YOU." ) && correct) {
                invited.Add(person); delivered.Add(Current.word.lemko);
                foreach(var token in tokens) if(token.Key==person) token.backing.color=new Color(.55f,.85f,.6f);
                RefreshAlbum();
            }
        }
        public override void Close() { base.Close(); invited.Clear(); delivered.Clear(); if(album) album.text=""; }
        public override void Submit() { }
    }
}
