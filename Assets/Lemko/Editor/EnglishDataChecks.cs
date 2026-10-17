using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Lemko.Editor
{
    public static class EnglishDataChecks
    {
        public static void RunBatch()
        {
            var shop=AssetDatabase.LoadAssetAtPath<ShopDefinition>(VocabularyImporter.Root+"/Shop.asset");
            var references=shop.products.ToArray();
            var guids=references.Select(w=>AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(w))).ToArray();
            VocabularyImporter.Import("Content/CSV");
            if(shop.products.Count!=47 || !references.SequenceEqual(shop.products)) throw new Exception("Reimport changed references");
            if(!guids.SequenceEqual(shop.products.Select(w=>AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(w))))) throw new Exception("Reimport changed GUIDs");
            var csv=CsvTable.Parse("\uFEFFid,text\r\na,\"ї, ł\"\r\nb,\"line\n\"\"quote\"\"\"\r\n");
            if(csv.Count!=3 || csv[1][1]!="ї, ł" || csv[2][1]!="line\n\"quote\"") throw new Exception("CSV quoting regression");
            var data=new ProgressData{lastShopHints=3,shopCompletions=2}; data.encountered.Add("shop_chleb");
            var restored=ProgressStore.Parse(JsonUtility.ToJson(data));
            if(restored.lastShopHints!=3 || restored.shopCompletions!=2 || restored.encountered[0]!="food_bread") throw new Exception("Save migration roundtrip");
            if(ProgressStore.Parse("not JSON").shopCompletions!=0) throw new Exception("Invalid save recovery");
            ContentValidator.Validate();
            Debug.Log("LEMKO_ENGLISH_REIMPORT_CHECKS_PASS: 47 object references and GUIDs retained; CSV Unicode/quoting; save migration and hint count roundtrip; invalid save recovery.");
        }
    }
}
