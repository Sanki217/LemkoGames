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
        bool ready,hover;
        public void Refresh(bool completed) {
            ready=completed;
            string name=activityId=="FamilyTree" ? "Family Tree" : activityId;
            label.text=name+(completed ? "\nReady - revisit" : activityId=="Farm" && !session.FarmQuantitiesUnlocked ? "\nLearn animal names" : "");
            Draw();
        }
        void Draw() { if(highlight) { highlight.enabled=hover || ready; highlight.color=hover ? Color.white : new Color(.75f,1,.7f,.8f); } }
        public void OnPointerEnter(PointerEventData e) { hover=true; Draw(); }
        public void OnPointerExit(PointerEventData e) { hover=false; Draw(); }
        public void OnPointerClick(PointerEventData e) {
            if(e.button==PointerEventData.InputButton.Left && session.villagePanel.activeInHierarchy && (!session.journal || !session.journal.IsOpen)) session.OpenActivity(activityId);
        }
        void OnDisable() { hover=false; Draw(); }
    }
}
