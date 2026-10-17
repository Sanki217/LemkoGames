using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
namespace Lemko.Editor
{
    public static class ContentValidator
    {
        [MenuItem("Lemko/Validate imported content and font")]
        public static void Validate()
        {
            int errors = 0, warnings = 0;
            var database = AssetDatabase.LoadAssetAtPath<VocabularyDatabase>(VocabularyImporter.Root + "/VocabularyDatabase.asset");
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset");
            if (!database || !font) { Debug.LogError("Generate starter assets first; vocabulary database or font missing."); return; }
            var ids = new HashSet<string>();
            foreach (var entry in database.entries)
            {
                if (!entry) { Debug.LogError("Null vocabulary entry."); errors++; continue; }
                if (string.IsNullOrWhiteSpace(entry.id) || !ids.Add(entry.id) || new[]{entry.lemko,entry.polish,entry.english}.Any(string.IsNullOrWhiteSpace))
                { Debug.LogError("Duplicate/blank ID or missing translation: " + entry.id,entry); errors++; }
                if (!entry.sprite || !entry.pronunciation || !entry.verified)
                { Debug.LogWarning("Missing optional art/audio or unverified language: " + entry.id,entry); warnings++; }
                string characters = (entry.lemko ?? "") + (entry.polish ?? "") + (entry.english ?? "");
                if (!font.HasCharacters(characters, out uint[] missing, true, true))
                { Debug.LogError("Font missing Unicode code points for " + entry.id + ": " + string.Join(",",missing),entry); errors++; }
            }
            var shop = AssetDatabase.LoadAssetAtPath<ShopDefinition>(VocabularyImporter.Root + "/Shop.asset");
            if (!shop || shop.Validate() != null) { Debug.LogError(shop ? shop.Validate() : "Shop definition missing."); errors++; }
            EditorUtility.SetDirty(font); AssetDatabase.SaveAssets();
            Debug.Log("Content validation: " + errors + " errors, " + warnings + " development warnings.");
        }
    }
}
