using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Lemko.Editor
{
    public static class ActivityCsvImporter
    {
        public const string Root="Assets/Lemko/Data/Activities";
        [MenuItem("Lemko/Import all prototype CSV tables")]
        public static void Import()
        {
            VocabularyImporter.Import("Content/CSV");
            var db=AssetDatabase.LoadAssetAtPath<VocabularyDatabase>(VocabularyImporter.Root+"/VocabularyDatabase.asset");
            var words=db.entries.Where(w=>w).ToDictionary(w=>w.id);
            VocabularyEntry Word(string id) { if(!words.TryGetValue(id,out var word)) throw new FormatException("Unknown vocabulary ID: "+id); return word; }
            var pending=new List<ActivityDefinition>();
            try {
                foreach(ActivityKind kind in Enum.GetValues(typeof(ActivityKind))) {
                    string file=kind==ActivityKind.FamilyTree?"family_tree":kind.ToString().ToLowerInvariant();
                    var rows=CsvTable.Records(File.ReadAllText("Content/CSV/activity_"+file+".csv"),"vocabulary_id");
                    var existing=AssetDatabase.LoadAssetAtPath<ActivityDefinition>(Root+"/"+kind+".asset");
                    var def=existing ? UnityEngine.Object.Instantiate(existing) : ScriptableObject.CreateInstance<ActivityDefinition>(); pending.Add(def); def.kind=kind; def.pool.Clear(); def.tasks.Clear(); def.rooms.Clear(); def.people.Clear();
                    foreach(var row in rows) {
                        var word=Word(row["vocabulary_id"]); if(!def.pool.Contains(word)) def.pool.Add(word);
                        if(kind==ActivityKind.Memory) continue;
                        var task=new LearningTask {id=row["task_id"],word=word};
                        if(kind==ActivityKind.Farm) task.quantity=int.Parse(row["quantity"],CultureInfo.InvariantCulture);
                        if(kind==ActivityKind.House) { task.context=Word(row["room_id"]); if(!def.rooms.Contains(task.context)) def.rooms.Add(task.context); }
                        if(kind==ActivityKind.FamilyTree) task.validPeople=row["valid_person_ids"].Split('|');
                        if(kind==ActivityKind.Numbers) task.quantity=int.Parse(row["numeral"],CultureInfo.InvariantCulture);
                        if(kind==ActivityKind.Weather) { task.context=Word(row["character_id"]); task.minTemperature=int.Parse(row["min_temperature"]); task.maxTemperature=int.Parse(row["max_temperature"]); task.request=row["request"]; }
                        def.tasks.Add(task);
                    }
                    if(kind==ActivityKind.FamilyTree) foreach(var row in CsvTable.Records(File.ReadAllText("Content/CSV/family_people.csv"),"person_id","vocabulary_id","x","y","parent_id"))
                        def.people.Add(new FamilyPerson{id=row["person_id"],appearance=Word(row["vocabulary_id"]),position=new Vector2(float.Parse(row["x"],CultureInfo.InvariantCulture),float.Parse(row["y"],CultureInfo.InvariantCulture)),parentId=row["parent_id"],secondParentId=row.TryGetValue("second_parent_id",out var second)?second:""});
                    var error=def.Validate(); if(error!=null) throw new FormatException(kind+": "+error);
                }
                Directory.CreateDirectory(Root); AssetDatabase.Refresh();
                foreach(var def in pending) { string path=Root+"/"+def.kind+".asset"; var existing=AssetDatabase.LoadAssetAtPath<ActivityDefinition>(path); if(existing) { Undo.RecordObject(existing,"Import activity"); EditorUtility.CopySerialized(def,existing); EditorUtility.SetDirty(existing); } else { var copy=UnityEngine.Object.Instantiate(def); copy.name=def.kind.ToString(); AssetDatabase.CreateAsset(copy,path); } }
                AssetDatabase.SaveAssets(); Debug.Log("LEMKO_ACTIVITY_IMPORT_PASS: seven validated activity tables; existing definition references retained.");
            } finally { foreach(var def in pending) UnityEngine.Object.DestroyImmediate(def); }
        }
    }
}
