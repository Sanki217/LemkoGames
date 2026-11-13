using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Lemko.Editor {
    public static class AlphabetMasterImporter {
        const string Path="Assets/Lemko/Data/Activities/Alphabet.asset";
        public static bool IsLetter(Dictionary<string,string> row)=>
            (row.TryGetValue("letter_uppercase",out var upper) && !string.IsNullOrWhiteSpace(upper)) ||
            (row.TryGetValue("phonetic_pl",out var phonetic) && !string.IsNullOrWhiteSpace(phonetic));
        public static List<AlphabetLetter> Prepare(List<Dictionary<string,string>> rows) {
            var letters=rows.Where(IsLetter).ToList(); if(letters.Count==0) return null;
            var definition=AssetDatabase.LoadAssetAtPath<ActivityDefinition>(Path);
            if(!definition) throw new FormatException("Alphabet activity asset is missing; no data imported.");
            var merged=new List<AlphabetLetter>();
            foreach(var row in letters) {
                string Value(string key)=>row.TryGetValue(key,out var value)?value:"";
                string upper=Value("letter_uppercase"),lower=Value("text_lemko"),phonetic=Value("phonetic_pl");
                if(Value("tags")!="alphabet" || SpellingRules.Elements(upper).Count!=1 || SpellingRules.Elements(lower).Count!=1 || upper.ToLowerInvariant()!=lower || string.IsNullOrWhiteSpace(phonetic))
                    throw new FormatException("Invalid alphabet row: "+row["id"]+". Use matching single upper/lowercase letters and phonetic_pl.");
                var previous=definition.alphabetLetters?.Find(l=>l.id==row["id"]);
                var entry=new AlphabetLetter{id=row["id"],verified=previous!=null && previous.verified};
                merged.Add(entry);
                entry.uppercase=upper;entry.lowercase=lower;entry.phoneticPolish=phonetic;entry.audioId=Value("audio_id");
                entry.pronunciation=VocabularyImporter.Resolve<AudioClip>(entry.audioId,true);
            }
            var pending=UnityEngine.Object.Instantiate(definition);
            try { pending.alphabetLetters=merged; var error=pending.Validate(); if(error!=null)throw new FormatException(error); }
            finally {UnityEngine.Object.DestroyImmediate(pending);}
            return merged;
        }
        public static void Apply(List<AlphabetLetter> letters) {
            if(letters==null)return;
            var definition=AssetDatabase.LoadAssetAtPath<ActivityDefinition>(Path);
            Undo.RecordObject(definition,"Import alphabet from master");definition.alphabetLetters=letters;EditorUtility.SetDirty(definition);
            Debug.Log("Imported "+letters.Count+" alphabet definitions from the master; word pools/tasks preserved.");
        }
    }
}
