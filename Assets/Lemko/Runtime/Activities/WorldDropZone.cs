using System;
using UnityEngine;
namespace Lemko
{
    public sealed class WorldDropZone : MonoBehaviour
    {
        public Func<ActivityToken,bool> dropped;
        public Func<ActivityToken,Vector2,bool> droppedAt;
        public bool Accept(ActivityToken token) => Accept(token,transform.position);
        public bool Accept(ActivityToken token,Vector2 point) => isActiveAndEnabled && (droppedAt!=null ? droppedAt(token,point) : dropped!=null && dropped(token));
        void OnDisable() { dropped=null; droppedAt=null; }
    }
}
