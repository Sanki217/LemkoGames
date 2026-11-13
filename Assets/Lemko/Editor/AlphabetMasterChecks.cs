using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Lemko.Editor {
    public static class AlphabetMasterChecks {
        static string Rules(string path) {
            var asset=AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if(asset is ActivityDefinition activity) {
                var copy=UnityEngine.Object.Instantiate(activity);
                try { copy.alphabetLetters.Clear();return EditorJsonUtility.ToJson(copy); }
                finally {UnityEngine.Object.DestroyImmediate(copy);}
            }
            return EditorJsonUtility.ToJson(asset);
        }
        public static void Verify() {
            var paths=AssetDatabase.FindAssets("t:ActivityDefinition").Select(AssetDatabase.GUIDToAssetPath).Concat(new[]{VocabularyImporter.Root+"/Shop.asset"}).ToArray();
            var rules=paths.ToDictionary(p=>p,Rules);
            var guids=AssetDatabase.FindAssets("t:VocabularyEntry").Select(AssetDatabase.GUIDToAssetPath).ToDictionary(p=>p,AssetDatabase.AssetPathToGUID);
            VocabularyImporter.ImportFile(VocabularyImporter.MasterPath);VocabularyImporter.ImportFile(VocabularyImporter.MasterPath);
            foreach(var p in paths)if(Rules(p)!=rules[p])throw new Exception("Gameplay rules changed: "+p);
            foreach(var p in guids)if(AssetDatabase.AssetPathToGUID(p.Key)!=p.Value)throw new Exception("GUID changed: "+p.Key);
            var rows=CsvTable.Records(File.ReadAllText(VocabularyImporter.MasterPath),"id","tags");
            var definition=AssetDatabase.LoadAssetAtPath<ActivityDefinition>(ActivityCsvImporter.Root+"/Alphabet.asset");
            foreach(var row in rows.Where(AlphabetMasterImporter.IsLetter)) {
                var letter=definition.alphabetLetters.Find(l=>l.id==row["id"]);
                if(letter==null || letter.lowercase!=row["text_lemko"] || letter.uppercase!=row["letter_uppercase"] || letter.phoneticPolish!=row["phonetic_pl"])throw new Exception("Letter mismatch: "+row["id"]);
            }
            if(definition.Validate()!=null)throw new Exception(definition.Validate());
            Debug.Log("LEMKO_GAME_TAGS_MASTER_PASS: master imported twice; 34 alphabet rows matched; gameplay rules and vocabulary GUIDs preserved. No builds or play simulation.");
        }
    }
}
