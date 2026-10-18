using System;
using System.Collections.Generic;
using UnityEngine;
namespace Lemko
{
    public enum ActivityKind { Farm, House, FamilyTree, Memory, Numbers, Alphabet, Weather }
    [Serializable] public sealed class LearningTask
    {
        public string id;
        public VocabularyEntry word;
        public VocabularyEntry context;
        public int quantity=1;
        public int minTemperature, maxTemperature=20;
        public string request;
        public string[] validPeople=Array.Empty<string>();
    }
    [Serializable] public sealed class FamilyPerson
    {
        public string id;
        public VocabularyEntry appearance;
        public Vector2 position;
        public string parentId;
        public string secondParentId;
    }
    [CreateAssetMenu(menuName="Lemko/Activity definition")]
    public sealed class ActivityDefinition : ScriptableObject
    {
        public ActivityKind kind;
        [Range(1,20)] public int tasksPerSession=5;
        [Header("Available words / tray / starter pool")]
        public List<VocabularyEntry> pool=new List<VocabularyEntry>();
        public List<LearningTask> tasks=new List<LearningTask>();
        [Header("House rooms")]
        public List<VocabularyEntry> rooms=new List<VocabularyEntry>();
        [Header("Family tree")]
        public string referencePerson="you";
        public List<FamilyPerson> people=new List<FamilyPerson>();
        [Header("Memory")]
        [Range(2,8)] public int pairs=4;
        [Min(.2f)] public float revealSeconds=1;
        [Header("Alphabet")]
        [Range(1,12)] public int maxLetters=10;
        public string extraLetters="";
        public string Validate()
        {
            if(tasksPerSession<1 || tasksPerSession>20) return "Session must contain 1–20 tasks.";
            if(pool.Exists(w=>!w) || rooms.Exists(w=>!w)) return "Null vocabulary reference.";
            if((kind==ActivityKind.Farm || kind==ActivityKind.House) && pool.Count>4) return "This prototype tray supports at most four distinct items.";
            if(kind==ActivityKind.Weather && pool.Count>6) return "This prototype supports at most six weather options.";
            if(kind==ActivityKind.Memory) return pairs<2 || pairs>6 || new HashSet<VocabularyEntry>(pool).Count<pairs ? "Memory needs 2–6 pairs and enough distinct words." : null;
            if(tasks.Count==0) return "No tasks configured.";
            var ids=new HashSet<string>();
            foreach(var t in tasks) {
                if(t==null || string.IsNullOrWhiteSpace(t.id) || !ids.Add(t.id) || !t.word) return "Missing/duplicate task ID or word.";
                if(kind==ActivityKind.Farm && (t.quantity<1 || t.quantity>12 || !pool.Contains(t.word))) return "Farm requires quantity 1–12 and an available animal.";
                if(kind==ActivityKind.House && (!rooms.Contains(t.context) || !pool.Contains(t.word))) return "House item/room is unavailable.";
                if(kind==ActivityKind.Numbers && (t.quantity<1 || t.quantity>99)) return "Numeral outside 1–99.";
                if(kind==ActivityKind.Weather && (t.minTemperature>t.maxTemperature || t.minTemperature< -30 || t.maxTemperature>50 || !pool.Contains(t.word))) return "Invalid weather or temperature range (-30..50).";
                if(kind==ActivityKind.Alphabet && (SpellingRules.Letters(t.word.lemko).Count==0 || SpellingRules.Letters(t.word.lemko).Count>maxLetters || SpellingRules.Elements(t.word.lemko).Count>12)) return "Alphabet word exceeds configured tile/slot limit (12 total slots).";
                if(kind==ActivityKind.FamilyTree && (t.validPeople==null || t.validPeople.Length==0 || Array.Exists(t.validPeople,p=>!people.Exists(x=>x.id==p)))) return "Unknown family answer person.";
            }
            if(kind==ActivityKind.FamilyTree && (!people.Exists(p=>p.id==referencePerson) || new HashSet<string>(people.ConvertAll(p=>p.id)).Count!=people.Count)) return "Invalid family reference/duplicate person.";
            if(kind==ActivityKind.FamilyTree && people.Exists(p=>!p.appearance || (!string.IsNullOrEmpty(p.parentId) && !people.Exists(x=>x.id==p.parentId)) || (!string.IsNullOrEmpty(p.secondParentId) && !people.Exists(x=>x.id==p.secondParentId)))) return "Family appearance or parent reference missing.";
            if(kind==ActivityKind.Numbers) { var numbers=new HashSet<int>(); var meanings=new Dictionary<VocabularyEntry,int>(); foreach(var t in tasks) { numbers.Add(t.quantity); if(meanings.TryGetValue(t.word,out var n) && n!=t.quantity) return "One number word maps to conflicting numerals."; meanings[t.word]=t.quantity; } if(numbers.Count<2) return "Numbers needs at least two distinct numerals."; }
            return null;
        }
    }
}
