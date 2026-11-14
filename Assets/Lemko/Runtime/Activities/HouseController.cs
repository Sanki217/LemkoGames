using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko
{
    [Serializable] public sealed class HouseRoomArtwork { public VocabularyEntry room; public Sprite background; }
    // Every room is one decorated picture that fills the screen. Arrows at the sides of the screen lead from room
    // to room and the pieces wait in a bar along the bottom. The task is always "Place X in Z": setting the right
    // piece down in the right room is the answer, so there is nothing to confirm.
    public sealed class HouseController : LearningActivity
    {
        public Button previousRoom, nextRoom;
        public TMP_Text roomLabel;
        public TMP_Text requestedRoomLabel;
        [HideInInspector] public int slotsPerRoom=3; // Retained for old serialized scenes.
        [Header("Free placement in the room")]
        public Rect placementArea=new Rect(-6.3f,-2.2f,12.6f,3.9f);
        [Min(1)] public int maximumFurniture=16;
        [Range(.2f,1)] public float furnitureScale=1;
        [Tooltip("Only used for a room that has no full-screen picture in Resources/Rooms.")]
        public List<HouseRoomArtwork> roomArtwork=new List<HouseRoomArtwork>();

        // The layout lives here rather than in the scene, so the scene file needs no editing.
        static readonly Rect Floor=new Rect(-6.3f,-2.2f,12.6f,3.9f);        // where the middle of a piece may stand
        static readonly Rect DropArea=new Rect(-8f,-2.95f,16f,7.45f);       // everything above the bar
        const float BarTop=-2.95f, BarY=-3.75f, BarStep=1.62f, BarScale=.88f, HeaderY=292;
        const int BarSize=9;
        // Pieces come in four rough sizes once they stand in a room, so a bed is not the size of a spoon.
        static readonly HashSet<string> Long=new HashSet<string> { "furniture_bed","house_sofa","house_bathtub","house_carpet" };
        static readonly HashSet<string> Large=new HashSet<string> { "furniture_wardrobe","furniture_table","house_refrigerator","house_fireplace",
            "house_stove","house_cabinet","house_washing_machine","house_shelf","house_sink" };
        static readonly HashSet<string> Small=new HashSet<string> { "house_mug","house_plate","house_bowl","house_glass","house_jug","house_spoon","house_fork","house_knife",
            "house_soap","house_shampoo","house_toothbrush","house_toothpaste","house_hairbrush","house_comb","house_can","house_bottle","house_pot","house_frying_pan",
            "house_kettle","house_rolling_pin","house_pillow","house_tap","house_shoes","house_game","house_books","house_small_lamp","house_radio","house_clock","house_towel" };
        static float Size(VocabularyEntry word)=>Long.Contains(word.id) ? 2.3f : Large.Contains(word.id) ? 1.9f : Small.Contains(word.id) ? .78f : 1.25f;

        sealed class PlacedItem { public int id, room; public VocabularyEntry word; public Vector2 position; }
        readonly List<PlacedItem> placed=new List<PlacedItem>();
        readonly List<VocabularyEntry> bar=new List<VocabularyEntry>();
        int room, nextId;
        SpriteRenderer backdrop, shelf; Image roomCard; bool arranged; Vector3 laidOut;
        public int RoomIndex => room;
        public int PlacedCount => placed.Count;
        public Vector2[] Positions => placed.Where(p=>p.room==room).Select(p=>p.position).ToArray();

        // A sitting visits the whole house: every room offers a few of its tasks.
        protected override List<LearningTask> AvailableTasks()
        {
            int share=Mathf.CeilToInt((float)Total/Mathf.Max(1,definition.rooms.Count))+1;
            var dealt=new List<LearningTask>();
            foreach(var group in definition.tasks.GroupBy(t=>t.context)) { var tasks=group.ToList(); Shuffle(tasks); dealt.AddRange(tasks.Take(share)); }
            return dealt;
        }
        protected override void BuildTask()
        {
            Arrange();
            // Pieces set down earlier stay where they are for the whole sitting; Close starts a fresh house.
            previousRoom.onClick.RemoveAllListeners(); previousRoom.onClick.AddListener(()=>SwitchRoom(-1));
            nextRoom.onClick.RemoveAllListeners(); nextRoom.onClick.AddListener(()=>SwitchRoom(1));
            prompt.text="Place "+Look.Word(Current.word.lemko);
            if(requestedRoomLabel) { requestedRoomLabel.text="in "+Look.Word(Current.context.lemko); requestedRoomLabel.GetComponent<HintPointer>().Bind(Current.context,Session.tooltip); }
            // The bar offers the piece that is asked for among others from all over the house.
            var others=definition.pool.Where(w=>w && w!=Current.word).Distinct().ToList(); Shuffle(others);
            bar.Clear(); bar.Add(Current.word); bar.AddRange(others.Take(BarSize-1)); Shuffle(bar);
            DrawRoom(true,false);
        }
        protected override bool HasAnswer => placed.Count>0;
        public void SwitchRoom(int delta)
        {
            if(!Ready) return;
            room=(room+delta+definition.rooms.Count)%definition.rooms.Count; feedback.text=""; DrawRoom(false,true);
            Juice.Sound("whoosh",1.3f);
            // The room's own name drifts up as you walk in.
            Juice.FloatText(definition.rooms[room].lemko,new Vector2(Screen.width*.5f,Screen.height*.56f),Look.Paper,prompt.font);
        }

        // ---------- the room and the bar ----------
        void DrawRoom(bool popBar,bool popRoom)
        {
            ClearTask();
            var here=definition.rooms[room];
            roomLabel.text=here.lemko; roomLabel.GetComponent<HintPointer>().Bind(here,Session.tooltip);
            ShowBackdrop(here);
            var drop=new GameObject("Room drop area",typeof(BoxCollider2D),typeof(WorldDropZone)); drop.transform.SetParent(taskRoot,false);
            drop.transform.localPosition=new Vector3(DropArea.center.x,DropArea.center.y,1);   // behind the pieces, so a click reaches the piece first
            drop.GetComponent<BoxCollider2D>().size=DropArea.size; drop.GetComponent<WorldDropZone>().droppedAt=Drop;
            quietSpawn=!popBar;
            for(int i=0;i<bar.Count;i++)
                Bare(Token(bar[i],new Vector2((i-(bar.Count-1)*.5f)*BarStep,BarY),"tray",t=>PlaceAt(t.view.Entry,FreeSpot()),draggable:true),BarScale,40);
            quietSpawn=!popRoom;
            foreach(var item in placed.Where(p=>p.room==room)) {
                // Pieces lower in the room stand in front of those further up.
                Bare(Token(item.word,item.position,"placed_"+item.id,Say,draggable:true),furnitureScale*Size(item.word),4+Mathf.RoundToInt((Floor.yMax-item.position.y)*4));
            }
            quietSpawn=false;
        }
        // A piece in the house is just its picture: no card behind it and no loudspeaker badge.
        static void Bare(ActivityToken token,float scale,int order)
        {
            token.transform.localScale=Vector3.one*scale; token.backing.enabled=false;
            var at=token.transform.localPosition; token.transform.localPosition=new Vector3(at.x,at.y,-.01f*order);   // the piece in front is also the one a click reaches
            if(token.view.worldSpeaker) token.view.worldSpeaker.SetActive(false);
            if(token.view.selectionMarker) token.view.selectionMarker.enabled=false;
            if(token.view.worldPicture) token.view.worldPicture.sortingOrder=order;
        }
        void ShowBackdrop(VocabularyEntry here)
        {
            if(!backdrop) {
                backdrop=Layer("Room backdrop",-6); shelf=Layer("Furniture bar",-2);
                shelf.sprite=Resources.Load<Sprite>("Rooms/furniture_bar");
            }
            var picture=Resources.Load<Sprite>("Rooms/"+here.id);
            if(!picture) { var spare=roomArtwork.Find(a=>a.room==here); if(spare!=null) picture=spare.background; }
            backdrop.sprite=picture;
            // The old shared backdrop only shows when a room has no picture of its own.
            foreach(var old in worldRoot.GetComponentsInChildren<SpriteRenderer>(true)) if(old.name=="Editable background") old.enabled=!picture;
            Fit();
        }
        SpriteRenderer Layer(string name,int order)
        {
            var layer=new GameObject(name,typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            layer.transform.SetParent(worldRoot,false); layer.sortingOrder=order; return layer;
        }
        // The room always fills the window, whatever its shape; the bar keeps its place under the pieces.
        void Fit()
        {
            var view=Camera.main; if(!view) return;
            float height=view.orthographicSize*2, width=height*view.aspect;
            if(backdrop && backdrop.sprite) {
                var size=backdrop.sprite.bounds.size;
                backdrop.transform.position=new Vector3(view.transform.position.x,view.transform.position.y,3);
                backdrop.transform.localScale=Vector3.one*Mathf.Max(width/size.x,height/size.y);
            }
            if(shelf && shelf.sprite) {
                var size=shelf.sprite.bounds.size;
                shelf.transform.position=new Vector3(view.transform.position.x,BarTop-size.y*.5f,2);
                shelf.transform.localScale=new Vector3(Mathf.Max(1,width/size.x),1,1);
            }
        }

        // ---------- the screen furniture: arrows at the sides, the task along the top ----------
        void Arrange()
        {
            placementArea=Floor; maximumFurniture=16; furnitureScale=1;
            if(arranged) return; arranged=true;
            roomLabel.gameObject.SetActive(false);
            foreach(var label in new[] { prompt,requestedRoomLabel }) { label.enableAutoSizing=false; label.fontSize=26; }
            Side(previousRoom,-1); Side(nextRoom,1);
            feedback.rectTransform.anchoredPosition=new Vector2(0,HeaderY-66); feedback.rectTransform.sizeDelta=new Vector2(900,44);
            var card=new GameObject("Room card",typeof(RectTransform),typeof(Image)); var label2=requestedRoomLabel.rectTransform;
            card.transform.SetParent(label2.parent,false); card.transform.SetSiblingIndex(label2.GetSiblingIndex());
            roomCard=card.GetComponent<Image>(); roomCard.raycastTarget=false;
        }
        // One arrow button at the edge of the screen.
        static void Side(Button button,int direction)
        {
            var rect=(RectTransform)button.transform; rect.anchoredPosition=new Vector2(direction*590,60); rect.sizeDelta=new Vector2(64,104);
            foreach(var label in button.GetComponentsInChildren<TMP_Text>(true)) label.text="";
            var arrow=new GameObject("Chevron",typeof(RectTransform),typeof(Image)); arrow.transform.SetParent(rect,false);
            var mark=(RectTransform)arrow.transform; mark.sizeDelta=new Vector2(44,44); mark.anchoredPosition=new Vector2(direction*2,3); mark.localScale=new Vector3(direction,1,1);
            var image=arrow.GetComponent<Image>(); image.sprite=Look.Chevron; image.color=Look.Ink; image.raycastTarget=false;
        }
        void LateUpdate()
        {
            if(!Running || !arranged) return;
            if(submit.gameObject.activeSelf) submit.gameObject.SetActive(false);   // pieces are judged as they are set down
            Fit();
            // "Place X" and "in Z" sit side by side along the top, followed by the loudspeaker and the light bulb.
            float first=prompt.preferredWidth, second=requestedRoomLabel.preferredWidth;
            var a=prompt.rectTransform; var b=requestedRoomLabel.rectTransform; var listen=(RectTransform)speak.transform;
            if(roomCard && roomCard.sprite!=Look.Button) { roomCard.sprite=Look.Button; roomCard.type=Image.Type.Sliced; }
            // Only when the words change, so a shake or a pulse on the task is left alone in between.
            var words=new Vector3(first,second,listen.sizeDelta.x); if(words==laidOut) return; laidOut=words;
            a.sizeDelta=new Vector2(Mathf.Max(first+24,180),56); b.sizeDelta=new Vector2(second+24,56);
            float w1=Mathf.Max(220,first+64), w2=Mathf.Max(150,second+56), tail=14+listen.sizeDelta.x+8+52;
            float x=-(w1+10+w2+tail)*.5f;
            a.anchoredPosition=new Vector2(x+w1*.5f,HeaderY); b.anchoredPosition=new Vector2(x+w1+10+w2*.5f,HeaderY);
            listen.anchoredPosition=new Vector2(x+w1+10+w2+14+listen.sizeDelta.x*.5f,HeaderY);
            if(roomCard) {
                roomCard.color=Look.Paper; roomCard.rectTransform.anchoredPosition=b.anchoredPosition;
                roomCard.rectTransform.sizeDelta=new Vector2(w2,Mathf.Clamp(prompt.preferredHeight+20,54,130));
            }
        }

        // ---------- setting pieces down ----------
        bool Drop(ActivityToken token,Vector2 worldPoint)
        {
            if(!Ready || !token || !token.transform.IsChildOf(taskRoot)) return false;
            Vector2 local=taskRoot.InverseTransformPoint(worldPoint);
            if(token.Key.StartsWith("placed_") && int.TryParse(token.Key.Substring(7),out int id)) return Move(id,local);
            return token.Key=="tray" && PlaceAt(token.view.Entry,local);
        }
        static Vector2 Clamp(Vector2 p)=>new Vector2(Mathf.Clamp(p.x,Floor.xMin,Floor.xMax),Mathf.Clamp(p.y,Floor.yMin,Floor.yMax));
        // Somewhere near the front of the room that is not already taken.
        Vector2 FreeSpot()
        {
            Vector2 best=Floor.center; float widest=-1;
            for(int i=0;i<12;i++) {
                var spot=new Vector2(UnityEngine.Random.Range(Floor.xMin+1,Floor.xMax-1),UnityEngine.Random.Range(Floor.yMin,Floor.yMin+1.6f));
                float gap=float.MaxValue; foreach(var item in placed) if(item.room==room) gap=Mathf.Min(gap,Vector2.Distance(item.position,spot));
                if(gap>widest) { widest=gap; best=spot; }
            }
            return best;
        }
        // Setting a piece down is the answer: the right piece in the right room stays, anything else goes back to the bar.
        public bool PlaceAt(VocabularyEntry word,Vector2 position)
        {
            if(!Ready || !word || !definition.pool.Contains(word)) return false;
            bool piece=word==Current.word, here=definition.rooms[room]==Current.context;
            if(!piece || !here) { Answer(false,room+":"+word.id,piece ? "Right piece, but this is another room." : "That is not the piece we need."); return false; }
            while(placed.Count>=maximumFurniture) placed.RemoveAt(0);   // the oldest piece makes way
            int id=nextId++; placed.Add(new PlacedItem { id=id,room=room,word=word,position=Clamp(position) });
            bar.Remove(word); Redraw(); Land(id);
            Answer(true,room+":"+word.id,"Just right!"); return true;
        }
        // Kept for old button/test callers; no visible slots or snapping remain.
        public bool Place(VocabularyEntry word,int slot) => slot>=0 && slot<slotsPerRoom && PlaceAt(word,placementArea.center+Vector2.right*(slot*.5f));
        public bool Move(int id,Vector2 position) {
            var item=placed.Find(p=>p.id==id && p.room==room);
            if(!Ready || item==null) return false;
            item.position=Clamp(position); Redraw(); Land(id); return true;
        }
        public void Remove(int id) { if(!Ready) return; if(placed.RemoveAll(p=>p.id==id && p.room==room)>0) Redraw(); }
        void Redraw() => DrawRoom(false,false);
        // Tapping a piece that already stands in the room says its name.
        void Say(ActivityToken token)
        {
            if(!token || !token.view.Entry) return;
            Session.audioService.Play(token.view.Entry.pronunciation); Juice.Hop(token.transform,.18f);
        }
        // The piece that was just set down lands with a little squash and dust.
        void Land(int id) {
            var token=tokens.Find(t=>t && t.Key=="placed_"+id); if(!token) return;
            Juice.Squash(token.transform); Juice.Dust(Juice.WorldToScreen(token.transform.position+Vector3.down*.3f)); Juice.Sound("drop");
        }
        public override void Close() { base.Close(); placed.Clear(); bar.Clear(); room=0; nextId=0; }
        // Not on screen any more (pieces are judged as they land); kept for callers that check the stored rooms.
        public override void Submit()
        {
            if(!Ready) return;
            bool correct=placed.Any(p=>definition.rooms[p.room]==Current.context && p.word==Current.word);
            Answer(correct,string.Join("|",placed.Select(p=>p.room+":"+p.word.id).OrderBy(s=>s)),correct ? "Just right!" : "The piece is still waiting in the bar.");
        }
    }
}
