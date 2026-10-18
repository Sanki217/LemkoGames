using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Lemko.Editor
{
    public static class PrototypeDataChecks
    {
        static void Check(bool condition,string message) { if(!condition) throw new Exception(message); }
        [MenuItem("Lemko/Validate full prototype data and scenes")]
        public static void RunBatch()
        {
            var definitions=AssetDatabase.FindAssets("t:ActivityDefinition",new[]{ActivityCsvImporter.Root}).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<ActivityDefinition>).ToArray();
            Check(definitions.Length==7,"seven definitions");
            var guids=definitions.Select(d=>AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(d))).ToArray();
            ActivityCsvImporter.Import();
            Check(guids.SequenceEqual(definitions.Select(d=>AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(d)))),"definition GUID preservation");
            foreach(var d in definitions) Check(d.Validate()==null,"valid "+d.kind);
            Check(SpellingRules.Elements("и\u0306").Count==1,"NFC grapheme normalization");
            Check(SpellingRules.Letters("а а’а").SequenceEqual(new[]{"а","а","а"}),"repeated letters and fixed separators");
            Check(string.Concat(SpellingRules.Elements("і"))!=string.Concat(SpellingRules.Elements("и")),"meaningful letter distinctions");
            Check(SpellingRules.Fixed("’") && SpellingRules.Fixed("'") && !SpellingRules.Fixed("ї"),"intentional apostrophes");
            foreach(var definition in definitions) {
                var clone=UnityEngine.Object.Instantiate(definition); clone.tasksPerSession=0; Check(clone.Validate()!=null,"invalid session rejected"); UnityEngine.Object.DestroyImmediate(clone);
            }
            var farm=UnityEngine.Object.Instantiate(definitions.First(d=>d.kind==ActivityKind.Farm)); farm.tasks[0].quantity=0; Check(farm.Validate()!=null,"invalid quantity rejected"); UnityEngine.Object.DestroyImmediate(farm);
            var family=UnityEngine.Object.Instantiate(definitions.First(d=>d.kind==ActivityKind.FamilyTree)); family.tasks[0].validPeople=new[]{"unknown"}; Check(family.Validate()!=null,"unknown family person rejected"); UnityEngine.Object.DestroyImmediate(family);
            var weather=UnityEngine.Object.Instantiate(definitions.First(d=>d.kind==ActivityKind.Weather)); weather.tasks[0].minTemperature=51; Check(weather.Validate()!=null,"invalid weather range rejected"); UnityEngine.Object.DestroyImmediate(weather);
            foreach(var name in SeparateScenesSetup.Activities) {
                var scene=EditorSceneManager.OpenScene(SeparateScenesSetup.Folder+"/"+name+".unity",OpenSceneMode.Single);
                var session=UnityEngine.Object.FindFirstObjectByType<GameSession>(FindObjectsInactive.Include); Check(session && session.resultsPanel && session.exhaustedPanel,"shared panels: "+name);
                if(name!="Shop") Check(session.activity && session.activity.definition && session.activity.tokenPrefab && session.activity.taskRoot,"activity wiring: "+name);
                foreach(var root in scene.GetRootGameObjects()) foreach(var t in root.GetComponentsInChildren<Transform>(true)) Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"missing script: "+t.name);
            }
            EditorSceneManager.OpenScene(SeparateScenesSetup.VillagePath,OpenSceneMode.Single); ContentValidator.Validate();
            Debug.Log("LEMKO_PROTOTYPE_DATA_PASS: seven definitions/reimport GUIDs, Unicode spelling, invalid settings/answers, eight activity scene wiring and missing-script scan.");
        }
    }
}
