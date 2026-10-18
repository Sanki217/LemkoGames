using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko
{
    public sealed class HouseController : LearningActivity
    {
        public Button previousRoom, nextRoom;
        public TMP_Text roomLabel;
        [Range(1,4)] public int slotsPerRoom=3;
        VocabularyEntry[,] placed;
        int room;
        public int RoomIndex => room;
        protected override void BuildTask()
        {
            placed=new VocabularyEntry[definition.rooms.Count,slotsPerRoom]; room=0;
            previousRoom.onClick.RemoveAllListeners(); previousRoom.onClick.AddListener(()=>SwitchRoom(-1));
            nextRoom.onClick.RemoveAllListeners(); nextRoom.onClick.AddListener(()=>SwitchRoom(1));
            prompt.text="Place "+Current.word.lemko+" in "+Current.context.lemko;
            DrawRoom();
        }
        public void SwitchRoom(int delta) { if(!Ready) return; room=(room+delta+definition.rooms.Count)%definition.rooms.Count; DrawRoom(); }
        void DrawRoom()
        {
            ClearTask(); roomLabel.text=definition.rooms[room].lemko; roomLabel.GetComponent<HintPointer>().Bind(definition.rooms[room],Session.tooltip);
            for(int i=0;i<definition.pool.Count;i++) Token(definition.pool[i],new Vector2(-5.7f+(i%2)*1.9f,.5f-(i/2)*1.7f),"tray",t=>Place(t.view.Entry,0),draggable:true);
            for(int i=0;i<slotsPerRoom;i++) {
                int slot=i; var position=new Vector2(.2f+i*2,-.7f);
                Zone(position,new Vector2(1.8f,2),t=>Place(t.view.Entry,slot));
                if(placed[room,i]) Token(placed[room,i],position,"placed",t=>{ if(Ready) { placed[room,slot]=null; DrawRoom(); } });
            }
            feedback.text="Switch rooms with arrows. Drag furniture into a slot; click placed furniture to remove it. Submit when ready.";
        }
        public bool Place(VocabularyEntry word,int slot) { if(!Ready || slot<0 || slot>=slotsPerRoom || !definition.pool.Contains(word)) return false; placed[room,slot]=word; DrawRoom(); return true; }
        public override void Submit()
        {
            if(!Ready) return; var items=Enumerable.Range(0,slotsPerRoom).Select(i=>placed[room,i]).Where(w=>w).ToArray();
            Answer(definition.rooms[room]==Current.context && items.Length==1 && items[0]==Current.word,room+":"+string.Join("|",items.Select(w=>w.id)),"Choose the requested room and place only the requested item.");
        }
    }
}
