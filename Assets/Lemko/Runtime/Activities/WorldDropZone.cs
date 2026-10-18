using System;
using UnityEngine;
namespace Lemko
{
    public sealed class WorldDropZone : MonoBehaviour
    {
        public Func<ActivityToken,bool> dropped;
        public bool Accept(ActivityToken token) => isActiveAndEnabled && dropped!=null && dropped(token);
        void OnDisable() { dropped=null; }
    }
}
