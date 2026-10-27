using System;
using UnityEngine;
namespace Lemko
{
    // Manual checkpoints are separate from frequent autosaves and contain no round state.
    public static class ManualProgressStore
    {
        [Serializable] sealed class Snapshot { public string format="Lemko.ManualSave"; public int version=1; public string savedUtc,progressJson; }
        static string Prefix => Array.IndexOf(Environment.GetCommandLineArgs(),"-lemkoSmoke")>=0 ? "Lemko.Manual.Smoke." : "Lemko.Manual.";
        static string Key(int slot) { if(slot<0 || slot>2) throw new ArgumentOutOfRangeException(nameof(slot)); return Prefix+slot; }
        public static bool Exists(int slot)=>PlayerPrefs.HasKey(Key(slot));
        public static bool HasRecovery=>PlayerPrefs.HasKey(Prefix+"Recovery");
        static bool Write(string key,ProgressData data,out string error) {
            error=null;
            try {
                if(data==null) { error="Progress is not ready yet."; return false; }
                string json=JsonUtility.ToJson(data);
                if(!ProgressStore.TryParse(json,out _)) { error="Progress could not be saved."; return false; }
                var snapshot=new Snapshot{savedUtc=DateTime.UtcNow.ToString("O"),progressJson=json};
                PlayerPrefs.SetString(key,JsonUtility.ToJson(snapshot)); PlayerPrefs.Save(); return true;
            } catch(Exception) { error="Saving failed. Your existing progress is still in memory. Please try again."; return false; }
        }
        public static bool Save(int slot,ProgressData data,out string error)=>Write(Key(slot),data,out error);
        public static bool ProtectCurrent(ProgressData data,out string error)=>Write(Prefix+"Recovery",data,out error);
        static bool Read(string key,out ProgressData data,out string date,out string error) {
            data=null; date=""; error=null;
            try {
                if(!PlayerPrefs.HasKey(key)) { error="This slot is empty."; return false; }
                var s=JsonUtility.FromJson<Snapshot>(PlayerPrefs.GetString(key));
                if(s==null || s.format!="Lemko.ManualSave" || s.version!=1 || !ProgressStore.TryParse(s.progressJson,out data)) {
                    data=null; error="This save could not be read. Current progress was not changed."; return false;
                }
                date=DateTime.TryParse(s.savedUtc,null,System.Globalization.DateTimeStyles.RoundtripKind,out var time) ? time.ToLocalTime().ToString("dd MMM yyyy, HH:mm") : "Date unavailable";
                return true;
            } catch(Exception) { data=null; error="This save could not be read. Current progress was not changed."; return false; }
        }
        public static bool Load(int slot,out ProgressData data,out string error)=>Read(Key(slot),out data,out _,out error);
        public static bool Recover(out ProgressData data,out string error)=>Read(Prefix+"Recovery",out data,out _,out error);
        public static string Description(int slot) {
            if(!Exists(slot)) return "Empty slot";
            if(!Read(Key(slot),out var data,out var date,out _)) return "Unreadable save — current progress is safe";
            return date+"\nGathering: "+data.story.contributions.Count+"/8  •  Words encountered: "+data.encountered.Count;
        }
    }
}
