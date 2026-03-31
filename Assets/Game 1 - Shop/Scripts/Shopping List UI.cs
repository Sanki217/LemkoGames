using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ShoppingListUI : MonoBehaviour
{
    public GameObject itemRowPrefab;
    public Transform listParent;

    [Header("Translation Tooltip")]
    public TMP_Text translationTooltip;
    public float hoverDelay = 1f;

    Dictionary<string, TMP_Text> rows = new();
    Coroutine pendingTooltip;

    List<ShopItemData> currentItems = new();
    public void Populate(List<ShopItemData> items)
    {
        currentItems = items;
        foreach (Transform child in listParent)
            Destroy(child.gameObject);
        rows.Clear();

        foreach (var item in items)
        {
            var row = Instantiate(itemRowPrefab, listParent);
            var txt = row.GetComponentInChildren<TMP_Text>();
            txt.text = item.lemko;
            rows[item.itemID] = txt;

            var trigger = row.AddComponent<EventTrigger>();
            var itemRef = item;

            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener(_ => OnRowEnter(itemRef, txt));
            trigger.triggers.Add(enterEntry);

            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ => OnRowExit());
            trigger.triggers.Add(exitEntry);
        }

        HideTranslation();
    }

    void OnRowEnter(ShopItemData item, TMP_Text sourceText)
    {
        if (pendingTooltip != null)
            StopCoroutine(pendingTooltip);
        pendingTooltip = StartCoroutine(DelayedShowTranslation(item, sourceText));
    }

    void OnRowExit()
    {
        if (pendingTooltip != null)
        {
            StopCoroutine(pendingTooltip);
            pendingTooltip = null;
        }
        HideTranslation();
    }

    IEnumerator DelayedShowTranslation(ShopItemData item, TMP_Text sourceText)
    {
        yield return new WaitForSeconds(hoverDelay);
        ShowTranslation(item, sourceText);
    }

    void ShowTranslation(ShopItemData item, TMP_Text sourceText)
    {
        if (translationTooltip == null) return;

        translationTooltip.text = $"{item.english}  /  {item.polish}";

        // Force layout update so we know the tooltip's real width
        Canvas.ForceUpdateCanvases();

        var tooltipRect = translationTooltip.rectTransform;
        var sourceRect = sourceText.rectTransform;

        // Place to the LEFT of the list item
        float tooltipWidth = tooltipRect.rect.width;
        tooltipRect.position = sourceRect.position + new Vector3(-tooltipWidth - 10f, 0f, 0f);

        translationTooltip.gameObject.SetActive(true);
    }

    public void RefreshLanguage()
    {
        foreach (var kvp in rows)
        {
            var item = currentItems.Find(i => i.itemID == kvp.Key);
            if (item != null)
                kvp.Value.text = GameSettings.GetName(item);
        }
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