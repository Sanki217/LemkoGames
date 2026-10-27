using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Lemko
{
    [Serializable] public sealed class SaveSlotView { public TMP_Text description; public Button save,load; }
    public sealed class GameMenu : MonoBehaviour
    {
        public GameSession session;
        public GameObject panel,confirmation;
        public SaveSlotView[] slots;
        public TMP_Text message,confirmationText,motionLabel;
        public Button confirm,cancel,close,motion,recovery,audioButton,language;
        enum Pending { None,Save,Load,Recovery }
        Pending pending;
        int pendingSlot;
        public bool IsOpen=>panel && panel.activeSelf;
        void Awake() {
            for(int i=0;i<slots.Length;i++) { int slot=i; slots[i].save.onClick.AddListener(()=>RequestSave(slot)); slots[i].load.onClick.AddListener(()=>RequestLoad(slot)); }
            confirm.onClick.AddListener(Confirm); cancel.onClick.AddListener(Cancel); close.onClick.AddListener(Close);
            motion.onClick.AddListener(ToggleMotion); recovery.onClick.AddListener(RequestRecovery);
            audioButton.onClick.AddListener(()=>{ session.ToggleMute(); Refresh(); });
            language.onClick.AddListener(()=>{ session.ToggleLanguage(); Refresh(); });
        }
        void Update() {
            if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame) {
                if(confirmation.activeSelf) Cancel(); else if(IsOpen) Close(); else Open();
            }
        }
        public void Open() {
            if(!session || session.Progress==null) return;
            session.tooltip.Hide(); session.audioService.Stop(); session.StopFeedback();
            if(session.journal) session.journal.Close();
            panel.SetActive(true); transform.SetAsLastSibling(); Cancel();
            message.text="Progress autosaves as you play. Manual slots let you keep a checkpoint.";
            Refresh();
        }
        public void Close() { if(confirmation) confirmation.SetActive(false); pending=Pending.None; if(panel) panel.SetActive(false); }
        void Refresh() {
            for(int i=0;i<slots.Length;i++) { slots[i].description.text=ManualProgressStore.Description(i); slots[i].load.interactable=ManualProgressStore.Load(i,out _,out _); }
            recovery.interactable=ManualProgressStore.HasRecovery;
            motionLabel.text=session.Progress.reducedMotion ? "Motion: reduced" : "Motion: standard";
            audioButton.GetComponentInChildren<TMP_Text>().text=session.Progress.muted ? "Audio: off" : "Audio: on";
            language.GetComponentInChildren<TMP_Text>().text=session.Progress.english ? "Hints: English" : "Hints: Polish";
        }
        void ToggleMotion() { session.ToggleReducedMotion(); Refresh(); }
        void Ask(Pending action,int slot,string text) { pending=action; pendingSlot=slot; confirmationText.text=text; confirmation.SetActive(true); confirmation.transform.SetAsLastSibling(); }
        void RequestSave(int slot) {
            if(ManualProgressStore.Exists(slot)) Ask(Pending.Save,slot,"Replace the checkpoint in Slot "+(slot+1)+" with your current progress?");
            else Save(slot);
        }
        void Save(int slot) { message.text=ManualProgressStore.Save(slot,session.Progress,out var error) ? "Progress saved in Slot "+(slot+1)+"." : error; Refresh(); }
        void RequestLoad(int slot)=>Ask(Pending.Load,slot,"Load Slot "+(slot+1)+" and return to the village?\nYour current round will end. Current progress is kept in a recovery checkpoint.");
        void RequestRecovery()=>Ask(Pending.Recovery,0,"Restore progress from before the last load and return to the village? Your current round will end.");
        void Cancel() { pending=Pending.None; confirmation.SetActive(false); }
        void Confirm() {
            var action=pending; int slot=pendingSlot; Cancel();
            if(action==Pending.Save) { Save(slot); return; }
            if(action!=Pending.Load && action!=Pending.Recovery) return;
            ProgressData data; string error;
            bool valid=action==Pending.Load ? ManualProgressStore.Load(slot,out data,out error) : ManualProgressStore.Recover(out data,out error);
            if(!valid) { message.text=error; return; }
            if(!ManualProgressStore.ProtectCurrent(session.Progress,out error)) { message.text=error; return; }
            Close(); session.RestoreProgress(data);
        }
    }
}
