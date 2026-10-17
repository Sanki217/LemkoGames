using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Lemko.Editor
{
    public static class MilestoneChecks
    {
        // Intended for a disposable project copy with starter content.
        public static void Batch()
        {
            if (!StarterSetup.HasTmpResources) { StarterSetup.ImportTmpResources(Batch); return; }
            try
            {
                StarterSetup.Create();
                Run();
                ContentValidator.Validate();
                StarterSetup.Build();
                EditorApplication.Exit(0);
            }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }
        static int count;
        static void Check(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); count++; }
        [MenuItem("Lemko/Run milestone data checks")]
        public static void Run()
        {
            count = 0;
            var round = new ShopRound(new[]{"a","b"},new[]{"a","b","c"});
            Check(round.Select("c") == SelectionResult.Incorrect,"wrong product");
            Check(round.Select("c") == SelectionResult.Ignored && round.Mistakes == 1,"repeated wrong product ignored");
            Check(round.Select("unknown") == SelectionResult.Ignored,"unknown product ignored");
            Check(round.Select("a") == SelectionResult.Correct,"correct product");
            Check(round.Select("a") == SelectionResult.Ignored && round.Correct == 1,"repeated correct product ignored");
            Check(round.Select("b") == SelectionResult.Correct && round.Complete,"round completes");
            Check(round.Select("c") == SelectionResult.Ignored,"complete round locked");
            bool failed = false; try { new ShopRound(new[]{"x"},new[]{"a"}); } catch(ArgumentException) { failed = true; }
            Check(failed,"unsolvable round rejected");
            var csv = CsvTable.Parse("\uFEFFid,text\r\na,\"ї, ł\"\r\nb,\"line\n\"\"quote\"\"\"\r\n");
            Check(csv.Count == 3 && csv[1][1] == "ї, ł" && csv[2][1] == "line\n\"quote\"", "Unicode quoted CSV multiline");
            Check(CsvTable.Parse("sep=;\r\nid;text\r\na;баба")[1][1] == "баба","Excel separator");
            failed = false; try { CsvTable.Parse("id,text\na,\"unterminated"); } catch(FormatException) { failed = true; }
            Check(failed,"malformed CSV rejected");
            Check(ProgressStore.Parse("not json").shopCompletions == 0,"corrupt save recovery");
            var data = new ProgressData { shopCompletions = 3, english = true }; data.encountered.Add("a");
            var restored = ProgressStore.Parse(JsonUtility.ToJson(data));
            Check(restored.shopCompletions == 3 && restored.english && restored.encountered.Contains("a"),"save round trip");
            Check(ProgressStore.Parse("{\"version\":99}").version == 1,"unsupported save version fallback");
            var shop = AssetDatabase.LoadAssetAtPath<ShopDefinition>(VocabularyImporter.Root + "/Shop.asset");
            Check(shop && shop.Validate() == null,"imported Shop is solvable");
            string asset = VocabularyImporter.Root + "/Vocabulary/demo_apple.asset";
            var guid = AssetDatabase.AssetPathToGUID(asset); var reference = AssetDatabase.LoadAssetAtPath<VocabularyEntry>(asset);
            VocabularyImporter.Import(Directory.Exists("Content/CSV/Archive/Starter") ? "Content/CSV/Archive/Starter" : "Content/Starter");
            Check(guid == AssetDatabase.AssetPathToGUID(asset) && reference == AssetDatabase.LoadAssetAtPath<VocabularyEntry>(asset),"reimport preserves GUID and object reference");
            var before = File.ReadAllBytes(StarterSetup.ScenePath);
            StarterSetup.Create();
            Check(before.SequenceEqual(File.ReadAllBytes(StarterSetup.ScenePath)),"setup rerun preserves scene bytes");
            Directory.CreateDirectory("Docs/Validation");
            File.WriteAllText("Docs/Validation/data-checks.txt",count + " milestone assertions passed in Unity Editor.\n" + DateTime.UtcNow.ToString("O"));
            Debug.Log("LEMKO_CHECKS_OK: " + count);
        }
    }
}

