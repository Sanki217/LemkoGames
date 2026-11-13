using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
namespace Lemko.Editor
{
    public static class SuppliedContentChecks
    {
        public static void RunBatch()
        {
            var db=AssetDatabase.LoadAssetAtPath<VocabularyDatabase>(VocabularyImporter.Root+"/VocabularyDatabase.asset");
            var before=db.entries.Where(w=>w).ToDictionary(w=>w.id,w=>AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(w)));
            PrototypeDataChecks.RunBatch();
            // Scene checks can unload asset instances; re-resolve the database before comparison.
            db=AssetDatabase.LoadAssetAtPath<VocabularyDatabase>(VocabularyImporter.Root+"/VocabularyDatabase.asset");
            foreach(var pair in before) Check(db.entries.Any(w=>w && w.id==pair.Key && AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(w))==pair.Value),"Preserved GUID: "+pair.Key);
            var importedIds=CsvTable.Records(System.IO.File.ReadAllText("Content/CSV/vocabulary_master.csv"),"id").Where(r=>!AlphabetMasterImporter.IsLetter(r)).Select(r=>r["id"]).ToArray();
            var imported=db.entries.Where(w=>w && importedIds.Contains(w.id)).ToArray();
            Check(imported.Length==importedIds.Length,"All master word rows imported; unrelated legacy assets retained");
            Check(imported.All(w=>!w.lemko.Contains("[DEMO:")),"Demo language replaced in master content");
            var numbers=AssetDatabase.LoadAssetAtPath<ActivityDefinition>(ActivityCsvImporter.Root+"/Numbers.asset");
            Check(numbers.tasks.Select(t=>t.quantity).OrderBy(n=>n).SequenceEqual(Enumerable.Range(1,99)),"All supplied numbers 1–99 playable");
            var alphabet=AssetDatabase.LoadAssetAtPath<ActivityDefinition>(ActivityCsvImporter.Root+"/Alphabet.asset");
            Check(alphabet.alphabetLetters.Count==34,"34 supplied letters");
            var allowed=alphabet.alphabetLetters.Select(l=>l.lowercase).ToArray();
            foreach(var task in alphabet.tasks) Check(SpellingRules.Letters(task.word.lemko).All(allowed.Contains),"Spelling uses supplied alphabet: "+task.id);
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset");
            foreach(var w in db.entries) Check(font.HasCharacters(w.lemko+w.polish+w.english,out uint[] missing,true,true),"Font coverage: "+w.id);
            foreach(var letter in alphabet.alphabetLetters) Check(font.HasCharacters(letter.uppercase+letter.lowercase+letter.phoneticPolish,out uint[] missing,true,true),"Letter font coverage: "+letter.id);
            var clone=UnityEngine.Object.Instantiate(alphabet);
            clone.alphabetLetters.Add(clone.alphabetLetters[0]); Check(clone.Validate()!=null,"Duplicate alphabet entry rejected");
            UnityEngine.Object.DestroyImmediate(clone);
            EditorUtility.SetDirty(font); AssetDatabase.SaveAssets();
            Debug.Log("LEMKO_SUPPLIED_CONTENT_PASS: master words, 34 letters, numbers 1–99, no demo strings, existing GUIDs, spelling alphabet, font coverage, invalid alphabet rejection.");
        }
        static void Check(bool ok,string message) { if(!ok) throw new Exception(message); }
    }
}
