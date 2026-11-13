using System;
using System.Collections.Generic;
namespace Lemko
{
    [Serializable] public sealed class ActivityResult { public string id; public int completions,lastPoints,bestPoints,lastHints,lastCorrect,lastMistakes; }
    [Serializable] public sealed class ProgressData
    {
        public VillageStoryState story=new VillageStoryState();
        public List<WordLearningRecord> words=new List<WordLearningRecord>();
        public bool challengeMode;
        public List<string> skippedIntroductions=new List<string>();
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
}
