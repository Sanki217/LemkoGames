using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Lemko
{
    public sealed class GameMenu : MonoBehaviour
    {
        public GameSession session;
        public GameObject panel;
        public TMP_Text motionLabel;
        public Button close,motion,audioButton,language;
        public bool IsOpen=>panel && panel.activeSelf;
        void Awake() {
            close.onClick.AddListener(Close);
            motion.onClick.AddListener(ToggleMotion);
            audioButton.onClick.AddListener(()=>{ session.ToggleMute(); Refresh(); });
            language.onClick.AddListener(()=>{ session.ToggleLanguage(); Refresh(); });
        }
        void Update() {
            if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame) {
                if(IsOpen) Close(); else Open();
            }
        }
        public void Open() {
            if(!session || session.Progress==null || (session.introduction && session.introduction.IsOpen)) return;
            session.tooltip.Hide(); session.audioService.Stop(); session.StopFeedback();
            if(session.journal) session.journal.Close();
            panel.SetActive(true); transform.SetAsLastSibling();
            Refresh();
        }
        public void Close() { if(panel) panel.SetActive(false); }
        void Refresh() {
            motionLabel.text=session.Progress.reducedMotion ? "Motion: reduced" : "Motion: standard";
            audioButton.GetComponentInChildren<TMP_Text>().text=session.Progress.muted ? "Audio: off" : "Audio: on";
            language.GetComponentInChildren<TMP_Text>().text=session.Progress.english ? "Hints: English" : "Hints: Polish";
        }
        void ToggleMotion() { session.ToggleReducedMotion(); Refresh(); }

    }
}
