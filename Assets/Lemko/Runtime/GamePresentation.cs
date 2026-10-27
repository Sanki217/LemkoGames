using UnityEngine;
namespace Lemko {
    [CreateAssetMenu(menuName="Lemko/Game presentation")]
    public sealed class GamePresentation : ScriptableObject {
        [Header("Alphabet world tiles")]
        public Vector2 letterSize=new Vector2(.72f,.82f);
        [Range(.8f,1)] public float letterSpacing=.96f;
        public Color letterColor=new Color(1,.84f,.48f),slotColor=new Color(.75f,.88f,.85f);
        [Header("Menu palette")]
        public Color panelColor=new Color(.09f,.19f,.17f),buttonColor=new Color(.28f,.43f,.34f),accentColor=new Color(.95f,.79f,.42f);
        void OnValidate() { letterSize.x=Mathf.Clamp(letterSize.x,.4f,letterSpacing-.1f); letterSize.y=Mathf.Clamp(letterSize.y,.5f,1); }
    }
}
