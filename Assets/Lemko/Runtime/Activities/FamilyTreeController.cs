using System;
using UnityEngine;
namespace Lemko
{
    public sealed class FamilyTreeController : LearningActivity
    {
        protected override void BuildTask()
        {
            submit.gameObject.SetActive(false); prompt.text="From YOU, select: "+Current.word.lemko;
            foreach(var person in definition.people) {
                var p=person; var token=Token(p.appearance,p.position,p.id,t=>Choose(p.id),p.id==definition.referencePerson ? "YOU" : null);
                if(p.id==definition.referencePerson) { token.backing.color=Color.yellow; token.caption.transform.localPosition=new Vector3(0,-.6f,0); token.caption.rectTransform.sizeDelta=new Vector2(1.7f,.4f); token.caption.enableAutoSizing=true; token.caption.fontSizeMin=1; token.caption.fontSizeMax=6; }
                if(!string.IsNullOrEmpty(p.parentId)) { var parent=definition.people.Find(x=>x.id==p.parentId); if(parent!=null) Link(parent.position,p.position); }
                if(!string.IsNullOrEmpty(p.secondParentId)) { var parent=definition.people.Find(x=>x.id==p.secondParentId); if(parent!=null) Link(parent.position,p.position); }
            }
            feedback.text="The highlighted YOU is the reference person. Select a relative; every explicitly valid person is accepted.";
        }
        void Link(Vector2 a,Vector2 b)
        {
            var go=new GameObject("Family connection",typeof(SpriteRenderer)); go.transform.SetParent(taskRoot,false); go.transform.localPosition=(a+b)/2;
            go.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg);
            go.transform.localScale=new Vector3(Vector2.Distance(a,b),.045f,1); var r=go.GetComponent<SpriteRenderer>(); r.sprite=tokenPrefab.backing.sprite; r.color=Color.white; r.sortingOrder=-2;
        }
        public void Choose(string person) { if(!definition.people.Exists(p=>p.id==person)) return; Answer(Array.IndexOf(Current.validPeople,person)>=0,person,"Look at the connections to YOU."); }
        public override void Submit() { }
    }
}
