using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lemko {
    public sealed class ContributionReward : MonoBehaviour {
        public Image illustration;
        public TMP_Text caption;
        public void Show(GameSession session,string id,bool completed) {
            var reward=session.story ? session.story.Find(id) : null;
            illustration.sprite=reward?.illustration; illustration.enabled=illustration.sprite;
            caption.text=completed ? (reward?.title??"Contribution ready") : "Practice complete";
            illustration.color=completed ? Color.white : new Color(.8f,.84f,.88f);
            if(completed) PresentationMotion.Pulse(illustration.transform,session);
        }
    }
}
