using System.Collections.Generic;
namespace Lemko {
    // Per activity session, shared by pictures, text, copies and explicit letter support.
    public sealed class SessionHintUsage {
        readonly HashSet<string> ids=new HashSet<string>(System.StringComparer.Ordinal);
        public bool Register(string id)=>!string.IsNullOrWhiteSpace(id) && ids.Add(id);
        public void Reset()=>ids.Clear();
    }
}
