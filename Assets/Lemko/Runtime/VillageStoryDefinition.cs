using System;
using UnityEngine;
namespace Lemko
{
    [Serializable] public sealed class VillageContribution
    {
        public string activityId, title;
        [TextArea] public string request, success;
        public Sprite illustration;
    }
    [CreateAssetMenu(menuName="Lemko/Village story")]
    public sealed class VillageStoryDefinition : ScriptableObject
    {
        public string storyId="village_gathering";
        public string title="A gathering for the village";
        [TextArea] public string introduction="Our neighbours are preparing a gathering. Help at any of the eight places.";
        public VillageContribution[] contributions=Array.Empty<VillageContribution>();
        public VillageContribution Find(string activityId) => Array.Find(contributions,c=>c!=null && c.activityId==activityId);
    }
}
