using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Lemko
{
    [Serializable] public sealed class ActivityResult { public string id; public int completions,lastPoints,bestPoints,lastHints,lastCorrect,lastMistakes; }
    [Serializable] public sealed class ProgressData
    {
        public int version = 2;
        public VillageStoryState story=new VillageStoryState();
        public List<WordLearningRecord> words=new List<WordLearningRecord>();
        public bool challengeMode; // Older saves default to ordinary learning.
        public bool muted;
        public bool reducedMotion;
        public bool english;
        public int shopCompletions;
        public int lastCorrect;
        public int lastMistakes;
        public int lastShopPoints;
        public int bestShopPoints;
        public int lastShopHints;
        public List<ActivityResult> activities=new List<ActivityResult>();
        public List<string> encountered = new List<string>();
    }
    public static class ProgressStore
    {
        static string Key => Environment.GetCommandLineArgs().Contains("-lemkoSmoke") ? "Lemko.Progress.Smoke.v1" : "Lemko.Progress.v1";
        public static ProgressData Parse(string json) => TryParse(json,out var data) ? data : new ProgressData();
        public static bool TryParse(string json,out ProgressData result)
        {
            result=null;
            try
            {
                if(string.IsNullOrWhiteSpace(json)) return false;
                var data = JsonUtility.FromJson<ProgressData>(json);
                if (string.IsNullOrWhiteSpace(json) || !json.Contains("\"version\"") || data == null || (data.version != 1 && data.version != 2) || data.shopCompletions < 0) return false;
                if (data.encountered == null) data.encountered = new List<string>();
                if(data.activities==null) data.activities=new List<ActivityResult>();
                data.activities=data.activities.Where(a=>a!=null && !string.IsNullOrEmpty(a.id) && a.completions>=0).GroupBy(a=>a.id).Select(g=>g.First()).ToList();
                data.encountered=data.encountered.Select(VocabularyIdMigration.Convert).Where(id=>!string.IsNullOrEmpty(id)).Distinct().ToList();
                data.version=2; VillageStoryProgress.Repair(data);
                result=data; return true;
            }
            catch (Exception) { return false; }
        }
        public static ProgressData Load() => Parse(PlayerPrefs.GetString(Key, ""));
        public static void Save(ProgressData data)
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }
}
