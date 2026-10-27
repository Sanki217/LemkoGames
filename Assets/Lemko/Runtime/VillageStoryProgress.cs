using System;
using System.Collections.Generic;
using System.Linq;
namespace Lemko
{
    [Serializable] public sealed class VillageStoryState
    {
        public string id="village_gathering";
        public List<string> contributions=new List<string>();
        public string decoration="";
    }
    public static class VillageStoryProgress
    {
        public static readonly string[] Activities={"Shop","FamilyTree","Farm","House","Memory","Numbers","Alphabet","Weather"};
        public static void Repair(ProgressData data)
        {
            if(data.story==null) data.story=new VillageStoryState();
            if(data.story.contributions==null) data.story.contributions=new List<string>();
            data.story.contributions=data.story.contributions.Where(Activities.Contains).Distinct().ToList();
            if(data.story.decoration!="gold" && data.story.decoration!="blue") data.story.decoration="";
            if(data.words==null) data.words=new List<WordLearningRecord>();
            data.words=data.words.Where(w=>w!=null && !string.IsNullOrWhiteSpace(w.id)).GroupBy(w=>w.id).Select(g=>g.First()).ToList();
            foreach(var word in data.words) { word.correct=Math.Max(0,word.correct); word.assisted=Math.Max(0,word.assisted); word.mistakes=Math.Max(0,word.mistakes); word.independent=Math.Max(0,word.independent); }
        }
        public static bool Contribute(ProgressData data,string activity)
        {
            Repair(data);
            if(!Activities.Contains(activity) || data.story.contributions.Contains(activity)) return false;
            data.story.contributions.Add(activity); return true;
        }
        public static bool Complete(ProgressData data) => Activities.All(data.story.contributions.Contains);
        public static bool ChooseDecoration(ProgressData data,string choice)
        {
            if(!Complete(data) || (choice!="gold" && choice!="blue")) return false;
            data.story.decoration=choice; return true;
        }
    }
    [Serializable] public sealed class WordLearningRecord
    {
        public string id;
        public int correct, assisted, independent, mistakes;
        public string lastIndependentDay="", lastActivity="";
        public bool rememberedOnLaterDay;
    }
    public static class WordLearning
    {
        public static void Record(ProgressData data,string id,string activity,bool correct,bool supported,string utcDay)
        {
            if(string.IsNullOrEmpty(id)) return;
            var word=data.words.Find(w=>w.id==id);
            if(word==null) { word=new WordLearningRecord{id=id}; data.words.Add(word); }
            word.lastActivity=activity;
            if(!correct) { word.mistakes++; return; }
            word.correct++;
            if(supported) word.assisted++;
            else {
                word.independent++;
                if(DateTime.TryParseExact(word.lastIndependentDay,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out var previous) &&
                   DateTime.TryParseExact(utcDay,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out var current) && current>previous) word.rememberedOnLaterDay=true;
                word.lastIndependentDay=utcDay;
            }
        }
    }
}
