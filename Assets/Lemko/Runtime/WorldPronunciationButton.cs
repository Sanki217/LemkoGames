using UnityEngine;
using UnityEngine.EventSystems;
namespace Lemko
{
    public sealed class WorldPronunciationButton : MonoBehaviour, IPointerClickHandler
    {
        public WordView word;
        public void OnPointerClick(PointerEventData data)
        {
            if(data.button == PointerEventData.InputButton.Left) word.Speak();
        }
    }
}
