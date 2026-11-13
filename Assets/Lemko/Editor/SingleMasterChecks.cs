using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Lemko.Editor {
    public static class SingleMasterChecks {
        public static void Verify() {
            var paths=AssetDatabase.FindAssets("t:ActivityDefinition").Select(AssetDatabase.GUIDToAssetPath)
                .Concat(new[]{VocabularyImporter.Root+"/Shop.asset"}).ToArray();
            var snapshots=paths.ToDictionary(p=>p,p=>File.ReadAllBytes(p));
            var ids=AssetDatabase.FindAssets("t:VocabularyEntry").Select(AssetDatabase.GUIDToAssetPath).ToDictionary(p=>p,AssetDatabase.AssetPathToGUID);
            // A folder with ONLY the master proves that no supporting CSV is required.
            const string folder="Temp/SingleMasterCheck"; Directory.CreateDirectory(folder);
            File.Copy(VocabularyImporter.MasterPath,folder+"/vocabulary_master.csv",true);
            VocabularyImporter.Import(folder);
            VocabularyImporter.Import(folder);
            foreach(var pair in snapshots) if(!File.ReadAllBytes(pair.Key).SequenceEqual(pair.Value)) throw new Exception("Activity configuration changed: "+pair.Key);
            foreach(var pair in ids) if(AssetDatabase.AssetPathToGUID(pair.Key)!=pair.Value) throw new Exception("Vocabulary GUID changed: "+pair.Key);
            foreach(var p in paths.Where(p=>p.EndsWith(".asset") && !p.EndsWith("/Shop.asset"))) {
                var definition=AssetDatabase.LoadAssetAtPath<ActivityDefinition>(p);
                if(definition && definition.Validate()!=null) throw new Exception(p+": "+definition.Validate());
            }
            var shop=AssetDatabase.LoadAssetAtPath<ShopDefinition>(VocabularyImporter.Root+"/Shop.asset");
            if(!shop || shop.Validate()!=null) throw new Exception("Shop configuration invalid");
            Debug.Log("LEMKO_SINGLE_MASTER_PASS: master-only folder imported twice; vocabulary GUIDs retained; Shop and seven activity asset contents unchanged; configurations valid. No builds, game simulation or save access.");
        }
    }
}
