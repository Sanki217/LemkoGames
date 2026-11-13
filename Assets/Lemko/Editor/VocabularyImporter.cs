using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
namespace Lemko.Editor
{
    public static class VocabularyImporter
    {
        public const string Root = "Assets/Lemko/Data";
        public const string MasterPath="Content/CSV/vocabulary_master.csv";
        [MenuItem("Lemko/Content/Import master vocabulary")]
        public static void ImportMaster() {
            try { ImportFile(MasterPath); } catch(Exception e) { Debug.LogError("Master vocabulary import stopped: "+e.Message); }
        }
        [MenuItem("Lemko/Content/Choose master vocabulary CSV...")]
        public static void Choose() {
            string path=EditorUtility.OpenFilePanel("Master vocabulary CSV", "Content/CSV", "csv");
            if(path.Length==0) return;
            try { ImportFile(path); } catch(Exception e) { Debug.LogError("Master vocabulary import stopped: "+e.Message); }
        }
        [MenuItem("Lemko/Content/Edit activity settings in Inspector")]
        public static void SelectActivities() {
            Selection.activeObject=AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Root+"/Activities");
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        // Compatibility for previous setup callers: a folder now means master vocabulary only.
        public static void Import(string folder) => ImportFile(Path.Combine(folder,"vocabulary_master.csv"));
        public static void ImportFile(string path)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode before importing vocabulary.");
            var allRows = CsvTable.Records(File.ReadAllText(path), "id", "tags");
            var allIds=new HashSet<string>();
            foreach(var row in allRows) if(!Regex.IsMatch(row["id"],"^[a-z][a-z0-9_]*$") || !allIds.Add(row["id"])) throw new FormatException("Invalid or duplicate ID: "+row["id"]);
            var alphabet=AlphabetMasterImporter.Prepare(allRows);
            var rows=allRows.Where(r=>!AlphabetMasterImporter.IsLetter(r)).ToList();
            var ids = new HashSet<string>();
            foreach (var r in rows)
            {
                foreach(var pair in new[]{("lemko","text_lemko"),("polish","text_pl"),("english","text_en"),("sprite_key","sprite_id"),("audio_key","audio_id")}) {
                    if(!r.ContainsKey(pair.Item1)) r[pair.Item1]=r.TryGetValue(pair.Item2,out var value) ? value : "";
                }
                if (!Regex.IsMatch(r["id"], "^[a-z][a-z0-9_]*$") || !ids.Add(r["id"])) throw new FormatException("Invalid or duplicate ID: " + r["id"]);
                foreach (var field in new[]{"lemko", "polish", "english"}) if (string.IsNullOrWhiteSpace(r[field])) throw new FormatException("Missing " + field + " for " + r["id"]);
                if (r.TryGetValue("verified",out var verification) && !string.IsNullOrWhiteSpace(verification) && !bool.TryParse(verification, out _)) throw new FormatException("verified must be true/false: " + r["id"]);
            }
            // Resolve all explicit keys before mutating any assets. Blank keys retain hand-assigned references.
            var sprites = rows.ToDictionary(r => r["id"], r => Resolve<Sprite>(r["sprite_key"],r.ContainsKey("sprite_id")));
            var clips = rows.ToDictionary(r => r["id"], r => Resolve<AudioClip>(r["audio_key"],r.ContainsKey("audio_id")));
            Directory.CreateDirectory(Root + "/Vocabulary"); AssetDatabase.Refresh();
            var database = GetOrCreate<VocabularyDatabase>(Root + "/VocabularyDatabase.asset");
            Undo.RecordObject(database, "Import vocabulary");
            var imported = new Dictionary<string,VocabularyEntry>();
            foreach (var r in rows)
            {
                var entry = GetOrCreate<VocabularyEntry>(Root + "/Vocabulary/" + r["id"] + ".asset");
                Undo.RecordObject(entry, "Import word");
                entry.id = r["id"]; entry.lemko = r["lemko"]; entry.polish = r["polish"]; entry.english = r["english"];
                entry.spriteId=r["sprite_key"]; entry.audioId=r["audio_key"];
                entry.translations=r.Where(p=>p.Key.StartsWith("text_")).Select(p=>new VocabularyEntry.LocalizedText{language=p.Key.Substring(5),text=p.Value}).ToArray();
                entry.tags = r["tags"].Split('|');
                // Optional legacy metadata. Omission preserves existing asset values.
                if(r.TryGetValue("verified",out var verification) && !string.IsNullOrWhiteSpace(verification)) entry.verified=bool.Parse(verification);
                // done is an authoring checklist only, never a gameplay or verification flag.
                if(r.TryGetValue("notes",out var notes)) entry.notes=notes;
                if (sprites[entry.id]) entry.sprite = sprites[entry.id];
                if (clips[entry.id] || !string.IsNullOrWhiteSpace(entry.audioId)) entry.pronunciation = clips[entry.id];
                if (!database.entries.Contains(entry)) database.entries.Add(entry);
                imported.Add(entry.id, entry); EditorUtility.SetDirty(entry);
            }
            AlphabetMasterImporter.Apply(alphabet);
            EditorUtility.SetDirty(database); AssetDatabase.SaveAssets();
            foreach(var entry in database.entries.Where(w=>w && !ids.Contains(w.id))) Debug.LogWarning("Master CSV omits existing word "+entry.id+"; its asset and activity references were preserved. Remove it from activity settings explicitly if desired.",entry);
            Debug.Log("Imported " + rows.Count + " words; existing assets/GUIDs and all activity settings retained. No assets deleted.");
        }
        internal static T Resolve<T>(string key,bool allowMissing=false) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            if (key.StartsWith("Assets/",StringComparison.Ordinal)) return AssetDatabase.LoadAssetAtPath<T>(key) ?? throw new FormatException("Missing asset: "+key);
            var matches = AssetDatabase.FindAssets(key+" t:" + typeof(T).Name).Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => Path.GetFileNameWithoutExtension(p) == key).Distinct().ToList();
            if(matches.Count==0 && allowMissing) return null;
            if (matches.Count != 1) throw new FormatException("Asset key missing or ambiguous: " + key + ". Use an exact Assets/... path.");
            return AssetDatabase.LoadAssetAtPath<T>(matches[0]);
        }
        public static T GetOrCreate<T>(string path) where T : ScriptableObject
        {
            var entry = AssetDatabase.LoadAssetAtPath<T>(path);
            if (entry) return entry;
            if (File.Exists(path)) throw new InvalidOperationException("Refusing to overwrite asset: " + path);
            entry = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(entry, path); return entry;
        }
    }
}
