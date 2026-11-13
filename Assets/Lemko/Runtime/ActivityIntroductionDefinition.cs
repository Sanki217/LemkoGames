using UnityEngine;
namespace Lemko {
    [CreateAssetMenu(menuName="Lemko/Activity introduction")]
    public sealed class ActivityIntroductionDefinition : ScriptableObject {
        public string activityId;
        public string title;
        [TextArea(2,4)] public string story;
        [TextArea(3,7)] public string instructions;
        [Tooltip("Farm only: instructions before the first Numbers completion.")]
        [TextArea(3,7)] public string beforeNumbers;
        public Sprite illustration;
    }
}
