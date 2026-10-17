using TMPro;
using UnityEngine;
namespace Lemko
{
    public sealed class TooltipService : MonoBehaviour
    {
        public RectTransform box;
        public TMP_Text label;
        public CanvasGroup group;
        public bool English { get; set; }
        Object owner;
        public void Show(Object requester, VocabularyEntry word, Vector2 screenPosition)
        {
            owner = requester;
            label.text = word.Translation(English);
            box.gameObject.SetActive(true);
            group.blocksRaycasts = false;
            group.interactable = false;
            group.alpha = 0;
            var parent = (RectTransform)box.parent;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPosition, null, out var p);
            p += new Vector2(0, 48);
            var half = box.sizeDelta * .5f;
            p.x = Mathf.Clamp(p.x, parent.rect.xMin + half.x, parent.rect.xMax - half.x);
            p.y = Mathf.Clamp(p.y, parent.rect.yMin + half.y, parent.rect.yMax - half.y);
            box.anchoredPosition = p;
            box.SetAsLastSibling();
        }
        public void Hide(Object requester = null)
        {
            if (requester != null && owner != requester) return;
            owner = null;
            if (box) box.gameObject.SetActive(false);
        }
        void Update() { if (owner && group) group.alpha = Mathf.MoveTowards(group.alpha, 1, Time.unscaledDeltaTime * 8); }
        void OnDisable() => Hide();
    }
}
