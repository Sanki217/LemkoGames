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
            submit.gameObject.SetActive(false); prompt.text="Deliver an invitation to "+Look.Word(Current.word.lemko);
            foreach(var person in definition.people) {
                var p=person; var token=Token(p.appearance,p.position,p.id,t=>Choose(p.id),p.id==definition.referencePerson ? "YOU" : null);
                Pixels.Card(token.backing,Portrait); token.GetComponent<BoxCollider2D>().size=Portrait;
                if(invited.Contains(p.id)) token.backing.color=Look.GoodSoft;
                if(p.id==definition.referencePerson) {
                    // "YOU" is a small tag on the top edge of the card, where the family line arrives.
                    token.backing.color=Look.Honey;
                    var tag=new GameObject("You tag",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); tag.transform.SetParent(token.transform,false);
                    tag.transform.localPosition=new Vector3(0,Portrait.y*.5f,0); Pixels.Card(tag,new Vector2(.9f,.4f),Look.Chip); tag.color=Look.Honey; tag.sortingOrder=3;
                    token.caption.transform.localPosition=new Vector3(0,Portrait.y*.5f,0); token.caption.rectTransform.sizeDelta=new Vector2(.8f,.3f); token.caption.enableAutoSizing=true; token.caption.fontSizeMin=1; token.caption.fontSizeMax=6;
                }
                if(!string.IsNullOrEmpty(p.parentId)) { var parent=definition.people.Find(x=>x.id==p.parentId); if(parent!=null) Link(parent.position,p.position,Look.Wood,-2); }
                if(!string.IsNullOrEmpty(p.secondParentId)) { var parent=definition.people.Find(x=>x.id==p.secondParentId); if(parent!=null) Link(parent.position,p.position,Look.Wood,-2); }
            }
            RefreshAlbum(); feedback.text="Start from YOU and follow the lines.";
        }
        // Every portrait card is 60 x 60 pixels.
        static readonly Vector2 Portrait=new Vector2(1.5f,1.5f);
        // A family line runs down from the parent, across, and down to the child: three straight bars two pixels thick.
        void Link(Vector2 a,Vector2 b,Color color,int order)
        {
            if(a.y<b.y) { var swap=a; a=b; b=swap; }   // a is the one above
            a=Pixels.Snap(a); b=Pixels.Snap(b);
            float top=a.y-Portrait.y*.5f, bottom=b.y+Portrait.y*.5f, middle=Pixels.Snap((top+bottom)*.5f), half=Pixels.Unit;
            Bar(a.x-half,middle-half,a.x+half,top+half,color,order);
            Bar(Mathf.Min(a.x,b.x)-half,middle-half,Mathf.Max(a.x,b.x)+half,middle+half,color,order);
            Bar(b.x-half,bottom-half,b.x+half,middle+half,color,order);
        }
        // A plain bar between two corners. Its sides are made an even number of pixels (growing down and to the right,
        // behind a card) so that its middle lies on the pixel grid.
        void Bar(float left,float low,float right,float high,Color color,int order)
        {
            float width=Pixels.Even(right-left), height=Pixels.Even(high-low);
            var go=new GameObject("Family connection",typeof(SpriteRenderer)); go.transform.SetParent(taskRoot,false);
            go.transform.localPosition=new Vector3(left+width*.5f,high-height*.5f,0);
            var r=go.GetComponent<SpriteRenderer>(); r.sprite=Look.Square; r.color=color; r.sortingOrder=order;
            var one=r.sprite.bounds.size; go.transform.localScale=new Vector3(width/one.x,height/one.y,1);
        }
        void RefreshAlbum() { if(album) album.text="Invited: "+delivered.Count+" / "+definition.tasksPerSession+"\n"+string.Join("  •  ",delivered); }
        public void Choose(string person) {
            if(!Ready || !definition.people.Exists(p=>p.id==person)) return;
            bool correct=Array.IndexOf(Current.validPeople,person)>=0;
            var chosen=tokens.Find(t=>t && t.Key==person);
            if(!Answer(correct,person,correct ? "Invitation delivered!" : "Not this relative. Follow the lines from YOU." )) return;
            if(!correct) { if(chosen) Juice.Shake(chosen.transform,.14f); return; }
            {
                invited.Add(person); delivered.Add(Current.word.lemko);
                foreach(var token in tokens) if(token.Key==person) { token.backing.color=Look.GoodSoft; Juice.Hop(token.transform,.4f); }
                HighlightPath(person); RefreshAlbum();
            }
        }
        void HighlightPath(string target) {
            var previous=new Dictionary<string,string>(); var queue=new Queue<string>();
            previous[definition.referencePerson]=null; queue.Enqueue(definition.referencePerson);
            while(queue.Count>0 && !previous.ContainsKey(target)) {
                var id=queue.Dequeue();
                foreach(var p in definition.people) {
                    bool adjacent=p.parentId==id || p.secondParentId==id;
                    var current=definition.people.Find(x=>x.id==id);
                    adjacent|=current!=null && (current.parentId==p.id || current.secondParentId==p.id);
                    if(adjacent && !previous.ContainsKey(p.id)) { previous[p.id]=id; queue.Enqueue(p.id); }
                }
            }
            if(!previous.ContainsKey(target)) return;
            for(string id=target; previous[id]!=null; id=previous[id]) {
                var a=definition.people.Find(p=>p.id==id); var b=definition.people.Find(p=>p.id==previous[id]);
                Link(a.position,b.position,Look.Brick,-1);
            }
        }
        public override void Close() { base.Close(); invited.Clear(); delivered.Clear(); if(album) album.text=""; }
        public override void Submit() { }
    }
}
