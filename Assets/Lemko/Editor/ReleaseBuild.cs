using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Lemko.Editor
{
    public sealed class ReleaseBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report) => ValidateContent();

        [MenuItem("Lemko/Content/Validate release vocabulary")]
        public static void ValidateContent()
        {
            var rows = CsvTable.Records(File.ReadAllText(VocabularyImporter.MasterPath), "id", "tags");
            var words = rows.Where(r => !AlphabetMasterImporter.IsLetter(r)).ToDictionary(r => r["id"]);
            var root = VocabularyImporter.Root;
            var database = AssetDatabase.LoadAssetAtPath<VocabularyDatabase>(root + "/VocabularyDatabase.asset");
            if (!database || database.entries.Any(w => !w) || database.entries.Count != words.Count ||
                database.entries.Select(w => w.id).Distinct().Count() != words.Count)
                throw new BuildFailedException("Vocabulary database differs from the master CSV.");
            foreach (var entry in database.entries)
            {
                if (!entry || !words.TryGetValue(entry.id, out var row) || entry.lemko != row["text_lemko"] ||
                    entry.polish != row["text_pl"] || entry.english != row["text_en"] ||
                    !entry.sprite || entry.sprite.name != row["sprite_id"] ||
                    entry.audioId != row["audio_id"] || entry.pronunciation != VocabularyImporter.Resolve<UnityEngine.AudioClip>(row["audio_id"], true))
                    throw new BuildFailedException("Reimport the master vocabulary. Mismatch at " + entry);
            }
            var shop = AssetDatabase.LoadAssetAtPath<ShopDefinition>(root + "/Shop.asset");
            if (shop.Validate() != null || shop.products.Any(w => !database.entries.Contains(w)))
                throw new BuildFailedException("Invalid shop products: " + shop.Validate());
            foreach (var guid in AssetDatabase.FindAssets("t:ActivityDefinition", new[]{root + "/Activities"}))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var activity = AssetDatabase.LoadAssetAtPath<ActivityDefinition>(path);
                if (activity.Validate() != null) throw new BuildFailedException(path + ": " + activity.Validate());
                if (activity.kind == ActivityKind.Memory && !new System.Collections.Generic.HashSet<VocabularyEntry>(activity.pool).SetEquals(database.entries))
                    throw new BuildFailedException("Memory must include every word in the master vocabulary.");
                if (activity.kind == ActivityKind.Alphabet)
                {
                    var letters = rows.Where(AlphabetMasterImporter.IsLetter).ToDictionary(r => r["id"]);
                    if (letters.Count != activity.alphabetLetters.Count) throw new BuildFailedException("Alphabet differs from the master CSV.");
                    foreach (var letter in activity.alphabetLetters)
                    {
                        if (!letters.TryGetValue(letter.id, out var row) || letter.lowercase != row["text_lemko"] ||
                            letter.uppercase != row["letter_uppercase"] || letter.phoneticPolish != row["phonetic_pl"] ||
                            letter.audioId != row["audio_id"] || letter.pronunciation != VocabularyImporter.Resolve<UnityEngine.AudioClip>(row["audio_id"], true))
                            throw new BuildFailedException("Reimport alphabet entry " + letter.id);
                    }
                }
                foreach (var dependency in AssetDatabase.GetDependencies(path))
                {
                    var word = AssetDatabase.LoadAssetAtPath<VocabularyEntry>(dependency);
                    if (word && !database.entries.Contains(word)) throw new BuildFailedException(path + " references " + word.id);
                }
            }
        }

        [MenuItem("Lemko/Build/Windows release")]
        public static void Windows()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var result = BuildPipeline.BuildPlayer(scenes, "Builds/WindowsRelease/LemkoGames.exe",
                BuildTarget.StandaloneWindows64, BuildOptions.None);
            if (result.summary.result != BuildResult.Succeeded) throw new BuildFailedException(result.summary.result.ToString());
        }
    }
}
