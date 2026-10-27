using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko
{
    public sealed class VillageStoryView : MonoBehaviour
    {
        public TMP_Text title, introduction, status;
        public Button[] locations;
        public VillageHotspot[] worldLocations;
        public SpriteRenderer[] contributions;
        public SpriteRenderer tablecloth;
        public Button goldChoice, blueChoice;
        GameSession session;
        public void Refresh(GameSession owner)
        {
            session=owner; if(!session.story) return;
            var state=session.Progress.story; title.text=session.story.title;
            bool complete=VillageStoryProgress.Complete(session.Progress);
            introduction.text=complete ? "The gathering is ready. Choose a keepsake colour, or revisit any place." : session.story.introduction;
            for(int i=0;i<VillageStoryProgress.Activities.Length;i++) {
                string id=VillageStoryProgress.Activities[i]; bool done=state.contributions.Contains(id);
                var contribution=session.story.Find(id);
                if(worldLocations!=null && i<worldLocations.Length && worldLocations[i]) worldLocations[i].Refresh(done);
                if(i<locations.Length && locations[i]) locations[i].GetComponentInChildren<TMP_Text>().text=(id=="FamilyTree"?"Family Tree":id)+"\n"+(done?"Ready - practise again":contribution?.title??"Help here");
                if(i<contributions.Length && contributions[i]) contributions[i].gameObject.SetActive(done && id!="Alphabet");
            }
            status.text="Gathering preparations: "+state.contributions.Count+" / 8";
            if(!complete) status.text+="\nChoose any unfinished place. Hints and practice count too.";
            else status.text+="\n"+(string.IsNullOrEmpty(state.decoration)?"Choose a colour below. Your progress is saved.":"Your gathering is saved. Come back whenever you like.");
            goldChoice.gameObject.SetActive(complete); blueChoice.gameObject.SetActive(complete);
            goldChoice.onClick.RemoveListener(Gold); goldChoice.onClick.AddListener(Gold);
            blueChoice.onClick.RemoveListener(Blue); blueChoice.onClick.AddListener(Blue);
            if(tablecloth) { tablecloth.gameObject.SetActive(complete); tablecloth.color=state.decoration=="blue" ? new Color(.3f,.65f,.9f) : new Color(1,.75f,.25f); }
        }
        public void Gold()=>Choose("gold");
        public void Blue()=>Choose("blue");
        void Choose(string choice) { if(VillageStoryProgress.ChooseDecoration(session.Progress,choice)) { ProgressStore.Save(session.Progress); Refresh(session); } }
        void OnDisable() { if(goldChoice) goldChoice.onClick.RemoveListener(Gold); if(blueChoice) blueChoice.onClick.RemoveListener(Blue); }
    }
}
