using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko
{
    [Serializable] public sealed class HouseRoomArtwork { public VocabularyEntry room; public Sprite background; }
    public sealed class HouseController : LearningActivity
    {
        public Button previousRoom, nextRoom;
        public TMP_Text roomLabel;
        public TMP_Text requestedRoomLabel;
        [HideInInspector] public int slotsPerRoom=3; // Retained for old serialized scenes.
        [Header("Free placement in the room")]
        public Rect placementArea=new Rect(-1.2f,-1.8f,7.4f,3.3f);
        [Min(1)] public int maximumFurniture=8;
        [Range(.2f,1)] public float furnitureScale=.75f;
        public List<HouseRoomArtwork> roomArtwork=new List<HouseRoomArtwork>();
        sealed class PlacedItem { public int id, room; public VocabularyEntry word; public Vector2 position; }
        readonly List<PlacedItem> placed=new List<PlacedItem>();
        int room, nextId;
        public int RoomIndex => room;
        public int PlacedCount => placed.Count;
        public Vector2[] Positions => placed.Where(p=>p.room==room).Select(p=>p.position).ToArray();
        protected override void BuildTask()
        {
            // Keep the room decorations through this session; Close starts a fresh house.
            previousRoom.onClick.RemoveAllListeners(); previousRoom.onClick.AddListener(()=>SwitchRoom(-1));
            nextRoom.onClick.RemoveAllListeners(); nextRoom.onClick.AddListener(()=>SwitchRoom(1));
            prompt.text="Place "+Look.Word(Current.word.lemko);
            if(requestedRoomLabel) { requestedRoomLabel.text="Room: "+Current.context.lemko; requestedRoomLabel.GetComponent<HintPointer>().Bind(Current.context,Session.tooltip); }
            DrawRoom();
        }
        protected override bool HasAnswer => placed.Count>0;
        public void SwitchRoom(int delta) { if(!Ready) return; room=(room+delta+definition.rooms.Count)%definition.rooms.Count; DrawRoom(); Juice.Punch(roomLabel.transform,.2f); Juice.Sound("whoosh",1.3f); }
        void DrawRoom()
        {
            ClearTask(); roomLabel.text=definition.rooms[room].lemko; roomLabel.GetComponent<HintPointer>().Bind(definition.rooms[room],Session.tooltip);
            var artwork=roomArtwork.Find(a=>a.room==definition.rooms[room]);
            var background=new GameObject("Empty room background",typeof(SpriteRenderer)); background.transform.SetParent(taskRoot,false);
            background.transform.localPosition=placementArea.center;
            var renderer=background.GetComponent<SpriteRenderer>(); renderer.sprite=artwork?.background ? artwork.background : tokenPrefab.backing.sprite; renderer.sortingOrder=-3;
            if(renderer.sprite) { var size=renderer.sprite.bounds.size; background.transform.localScale=new Vector3(placementArea.width/size.x,placementArea.height/size.y,1); }
            var drop=new GameObject("Room drop area",typeof(BoxCollider2D),typeof(WorldDropZone)); drop.transform.SetParent(taskRoot,false); drop.transform.localPosition=placementArea.center;
            drop.GetComponent<BoxCollider2D>().size=placementArea.size; drop.GetComponent<WorldDropZone>().droppedAt=Drop;
            for(int i=0;i<definition.pool.Count;i++) Token(definition.pool[i],new Vector2(-5.7f+(i%2)*1.9f,.5f-(i/2)*1.7f),"tray",t=>PlaceAt(t.view.Entry,placementArea.center),draggable:true);
            foreach(var item in placed.Where(p=>p.room==room)) {
                int id=item.id;
                var token=Token(item.word,item.position,"placed_"+id,t=>Remove(id),draggable:true);
                token.transform.localScale=Vector3.one*furnitureScale;
                token.backing.enabled=false;
            }
            feedback.text="Drag to place or move. Tap to remove.";
        }
        bool Drop(ActivityToken token,Vector2 worldPoint)
        {
            if(!Ready || !token || !token.transform.IsChildOf(taskRoot)) return false;
            Vector2 local=taskRoot.InverseTransformPoint(worldPoint);
            if(token.Key.StartsWith("placed_") && int.TryParse(token.Key.Substring(7),out int id)) return Move(id,local);
            return token.Key=="tray" && PlaceAt(token.view.Entry,local);
        }
        public bool PlaceAt(VocabularyEntry word,Vector2 position)
        {
            if(!Ready || !definition.pool.Contains(word) || !placementArea.Contains(position) || placed.Count>=maximumFurniture) return false;
            int id=nextId++; placed.Add(new PlacedItem{id=id,room=room,word=word,position=position}); Redraw(); Land(id); return true;
        }
        // Kept for old button/test callers; no visible slots or snapping remain.
        public bool Place(VocabularyEntry word,int slot) => slot>=0 && slot<slotsPerRoom && PlaceAt(word,placementArea.center+Vector2.right*(slot*.5f));
        public bool Move(int id,Vector2 position) {
            var item=placed.Find(p=>p.id==id && p.room==room);
            if(!Ready || item==null || !placementArea.Contains(position)) return false;
            item.position=position; Redraw(); Land(id); return true;
        }
        public void Remove(int id) { if(!Ready) return; if(placed.RemoveAll(p=>p.id==id && p.room==room)>0) Redraw(); }
        void Redraw() { quietSpawn=true; DrawRoom(); quietSpawn=false; }
        // The piece that was just set down lands with a little squash and dust.
        void Land(int id) {
            var token=tokens.Find(t=>t && t.Key=="placed_"+id); if(!token) return;
            Juice.Squash(token.transform); Juice.Dust(Juice.WorldToScreen(token.transform.position+Vector3.down*.3f)); Juice.Sound("drop");
        }
        public override void Close() { base.Close(); placed.Clear(); room=0; nextId=0; }
        public override void Submit()
        {
            if(!Ready) return;
            bool correct=placed.Any(p=>definition.rooms[p.room]==Current.context && p.word==Current.word);
            string correction=placed.Any(p=>p.word==Current.word) ? "Right piece, wrong room." : "That is not the piece we need.";
            Answer(correct,string.Join("|",placed.Select(p=>p.room+":"+p.word.id).OrderBy(s=>s)),correct?"Just right!":correction);
        }
    }
}
