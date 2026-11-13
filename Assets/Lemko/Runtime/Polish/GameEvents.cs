using System;
using System.Collections.Generic;
using UnityEngine;
namespace Lemko
{
    public sealed class RoundResult
    {
        public string activity; public bool complete, practice; public int points, best, correct, mistakes, hints, stars;
        public string contribution; public List<VocabularyEntry> revisit=new List<VocabularyEntry>(); public string suggested;
    }
    // Announcements from game logic to the presentation layer. Listeners must never change answers.
    public static class GameEvents
    {
        public static event Action<ActivityToken> TokenSpawned;
        public static event Action TaskStarted;
        public static event Action<bool,int,int> Answered; // correct, points awarded, streak after the answer
        public static event Action<RoundResult> RoundFinished;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset() { TokenSpawned=null; TaskStarted=null; Answered=null; RoundFinished=null; }
        public static void Spawned(ActivityToken token)=>TokenSpawned?.Invoke(token);
        public static void Started()=>TaskStarted?.Invoke();
        public static void Answer(bool correct,int awarded,int streak)=>Answered?.Invoke(correct,awarded,streak);
        public static void Finished(RoundResult result)=>RoundFinished?.Invoke(result);
    }
    // Facts about the current sitting only. Nothing here is ever written to disk.
    public static class SessionStats
    {
        public static readonly Dictionary<string,int> Stars=new Dictionary<string,int>();
        public static int BestStreak, Goal, LanternsShown;
        public static bool FinaleShown;
        public static float StartedAt;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset() { Stars.Clear(); BestStreak=0; Goal=0; LanternsShown=0; FinaleShown=false; StartedAt=0; }
        public static int TotalStars { get { int n=0; foreach(var s in Stars.Values) n+=s; return n; } }
        public static int StarsFor(string activity)=>Stars.TryGetValue(activity,out int s) ? s : 0;
        public static int Rate(bool complete,int mistakes,int hints)=>!complete ? 0 : 1+(mistakes<=1 ? 1 : 0)+(hints==0 ? 1 : 0);
        public static void Record(string activity,int stars) { if(stars>StarsFor(activity)) Stars[activity]=stars; }
        public static bool GoalReached(ProgressData progress)=>Goal>0 && progress!=null && progress.story.contributions.Count>=Mathf.Min(Goal,8);
    }
}
