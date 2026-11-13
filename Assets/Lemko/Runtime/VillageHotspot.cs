using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Lemko
{
    public sealed class VillageHotspot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public GameSession session;
        public string activityId;
        public TMP_Text label;
        public SpriteRenderer highlight;
        bool ready,hover,recommended;
        public bool Recommended=>recommended;
        public void Recommend(bool value) { recommended=value; if(value) label.text+="\nGo here next"; Draw(); }
        public void Refresh(bool completed) {
            ready=completed; recommended=false;
            string name=activityId=="FamilyTree" ? "Family Tree" : activityId;
            label.text=name+(completed ? "\nReady - revisit" : activityId=="Farm" && !session.FarmQuantitiesUnlocked ? "\nLearn animal names" : "");
            Draw();
        }
        void Draw() { if(highlight) { highlight.enabled=hover || ready || recommended; highlight.color=hover ? Color.white : recommended ? Look.Alpha(Look.Honey,.95f) : Look.Alpha(Look.GoodSoft,.85f); } }
        public void OnPointerEnter(PointerEventData e) { hover=true; Draw(); }
        public void OnPointerExit(PointerEventData e) { hover=false; Draw(); }
        public void OnPointerClick(PointerEventData e) {
            if(e.button==PointerEventData.InputButton.Left && session.villagePanel.activeInHierarchy && (!session.journal || !session.journal.IsOpen)) session.OpenActivity(activityId);
        }
        void OnDisable() { hover=false; Draw(); }
    }
}
