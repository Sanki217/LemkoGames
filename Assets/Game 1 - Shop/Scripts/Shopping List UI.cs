using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class ShoppingListUI : MonoBehaviour
{
    public GameObject itemRowPrefab;
    public Transform listParent;

    [Header("Translation Tooltip")]
    public TMP_Text translationTooltip; // a TMP text that appears next to hovered list item

    Dictionary<string, TMP_Text> rows = new();

    public void Populate(List<ShopItemData> items)
    {
        foreach (Transform child in listParent)
            Destroy(child.gameObject);
        rows.Clear();

        foreach (var item in items)
        {
            var row = Instantiate(itemRowPrefab, listParent);
            var txt = row.GetComponentInChildren<TMP_Text>();
            txt.text = item.lemko;
            rows[item.itemID] = txt;

            // Add hover listener
            var trigger = row.AddComponent<EventTrigger>();
            var itemRef = item; // capture for lambda

            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener(_ => ShowTranslation(itemRef, txt));
            trigger.triggers.Add(enterEntry);

            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ => HideTranslation());
            trigger.triggers.Add(exitEntry);
        }

        HideTranslation();
    }

    void ShowTranslation(ShopItemData item, TMP_Text sourceText)
    {
        if (translationTooltip == null) return;

        // Show English and Polish translation
        string lang1 = item.english;
        string lang2 = item.polish;
        translationTooltip.text = $"{lang1}  /  {lang2}";

        // Position tooltip next to the hovered list row
        var tooltipRect = translationTooltip.rectTransform;
        var sourceRect = sourceText.rectTransform;

        // Place it to the right of the list item in the same canvas space
        tooltipRect.position = sourceRect.position + new Vector3(sourceRect.rect.width + 10f, 0f, 0f);

        translationTooltip.gameObject.SetActive(true);
    }

    void HideTranslation()
    {
        if (translationTooltip != null)
            translationTooltip.gameObject.SetActive(false);
    }

    public void CrossOff(string itemID)
    {
        if (rows.TryGetValue(itemID, out var txt))
        {
            txt.fontStyle = FontStyles.Italic | FontStyles.Strikethrough;
            txt.color = new Color(0.5f, 0.5f, 0.5f);
        }
    }
}