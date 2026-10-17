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
        public const string Root = "Assets/Lemko/Generated";
        [MenuItem("Lemko/Import vocabulary and Shop CSV...")]
        public static void Choose()
        {
            string folder = EditorUtility.OpenFolderPanel("Folder containing vocabulary_master.csv and activity_shop.csv", "Content", "");
            if (folder.Length == 0) return;
            try { Import(folder); } catch (Exception e) { Debug.LogError("Import stopped: " + e.Message); }
        }
        public static void Import(string folder)
        {
            var rows = CsvTable.Records(File.ReadAllText(Path.Combine(folder, "vocabulary_master.csv")), "id", "lemko", "polish", "english", "sprite_key", "audio_key", "tags", "verified");
            var shopRows = CsvTable.Records(File.ReadAllText(Path.Combine(folder, "activity_shop.csv")), "vocabulary_id");
            var ids = new HashSet<string>();
            foreach (var r in rows)
            {
                if (!Regex.IsMatch(r["id"], "^[a-z][a-z0-9_]*$") || !ids.Add(r["id"])) throw new FormatException("Invalid or duplicate ID: " + r["id"]);
                foreach (var field in new[]{"lemko", "polish", "english"}) if (string.IsNullOrWhiteSpace(r[field])) throw new FormatException("Missing " + field + " for " + r["id"]);
                if (!bool.TryParse(r["verified"], out _)) throw new FormatException("verified must be true/false: " + r["id"]);
            }
            var shopIds = shopRows.Select(r => r["vocabulary_id"]).ToList();
            if (shopIds.Count != shopIds.Distinct().Count() || shopIds.Any(id => !ids.Contains(id))) throw new FormatException("Duplicate or unknown Shop vocabulary ID.");
            var existingShop = AssetDatabase.LoadAssetAtPath<ShopDefinition>(Root + "/Shop.asset");
            int taskCount = existingShop ? existingShop.taskCount : 5;
            if (shopIds.Count < taskCount + 1 || shopIds.Count > 12) throw new FormatException("Shop requires task count + 1 to 12 products.");
            // Resolve all explicit keys before mutating any assets. Blank keys retain hand-assigned references.
            var sprites = rows.ToDictionary(r => r["id"], r => Resolve<Sprite>(r["sprite_key"]));
            var clips = rows.ToDictionary(r => r["id"], r => Resolve<AudioClip>(r["audio_key"]));
            Directory.CreateDirectory(Root + "/Vocabulary"); AssetDatabase.Refresh();
            var database = GetOrCreate<VocabularyDatabase>(Root + "/VocabularyDatabase.asset");
            Undo.RecordObject(database, "Import vocabulary");
            var imported = new Dictionary<string,VocabularyEntry>();
            foreach (var r in rows)
            {
                var entry = GetOrCreate<VocabularyEntry>(Root + "/Vocabulary/" + r["id"] + ".asset");
                Undo.RecordObject(entry, "Import word");
                entry.id = r["id"]; entry.lemko = r["lemko"]; entry.polish = r["polish"]; entry.english = r["english"];
                entry.tags = r["tags"].Split('|'); entry.verified = bool.Parse(r["verified"]);
                if (sprites[entry.id]) entry.sprite = sprites[entry.id];
                if (clips[entry.id]) entry.pronunciation = clips[entry.id];
                if (!entry.sprite || !entry.pronunciation || !entry.verified) Debug.LogWarning("Development warning: " + entry.id + " missing art/audio or unverified language.", entry);
                if (!database.entries.Contains(entry)) database.entries.Add(entry);
                imported.Add(entry.id, entry); EditorUtility.SetDirty(entry);
            }
            var shop = GetOrCreate<ShopDefinition>(Root + "/Shop.asset"); Undo.RecordObject(shop, "Import Shop content");
            shop.products = shopIds.Select(id => imported[id]).ToList();
            EditorUtility.SetDirty(database); EditorUtility.SetDirty(shop); AssetDatabase.SaveAssets();
            Debug.Log("Imported " + rows.Count + " words; existing assets/GUIDs retained. No assets deleted.");
        }
        static T Resolve<T>(string key) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            var direct = AssetDatabase.LoadAssetAtPath<T>(key); if (direct) return direct;
            var matches = AssetDatabase.FindAssets("t:" + typeof(T).Name).Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => Path.GetFileNameWithoutExtension(p) == key).Distinct().ToList();
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
