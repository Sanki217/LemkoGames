using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ShopGameManager : MonoBehaviour
{
    public static ShopGameManager Instance;

    [Header("Config")]
    public ItemDatabase database;
    public int listSize = 4;
    public int maxHP = 3;

    [Header("UI References")]
    public ShoppingListUI listUI;
    public HudUI hudUI;
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;
    public TMP_Text itemTooltipText;   // ← NEW

    int hp, score, streak;
    List<ShopItemData> shoppingList = new();
    HashSet<string> foundItems = new();
    bool gameActive;

    void Awake() => Instance = this;
    void Start() => StartGame();

    void StartGame()
    {
        hp = maxHP;
        score = 0;
        streak = 0;
        foundItems.Clear();
        gameActive = true;
        gameOverPanel.SetActive(false);

        shoppingList = database.items
            .OrderBy(_ => Random.value)
            .Take(listSize)
            .ToList();

        listUI.Populate(shoppingList);
        hudUI.UpdateHP(hp);
        hudUI.UpdateScore(score, streak);
    }

    public void TrySelectItem(string itemID)
    {
        if (!gameActive) return;

        bool isOnList = shoppingList.Any(i => i.itemID == itemID);
        bool alreadyFound = foundItems.Contains(itemID);

        if (isOnList && !alreadyFound)
        {
            foundItems.Add(itemID);
            streak++;
            score += 100 * streak;
            listUI.CrossOff(itemID);
            hudUI.UpdateScore(score, streak);
            hudUI.ShowFeedback(true);

            if (foundItems.Count == shoppingList.Count)
                EndGame(true);
        }
        else if (!isOnList)
        {
            streak = 0;
            hp--;
            hudUI.UpdateHP(hp);
            hudUI.UpdateScore(score, streak);
            hudUI.ShowFeedback(false);

            if (hp <= 0)
                EndGame(false);
        }
    }

    public void ShowItemTooltip(string itemID, Vector3 worldPos)
    {
        if (itemTooltipText == null) return;

        var data = database.items.Find(i => i.itemID == itemID);
        if (data == null) return;

        itemTooltipText.text = $"{data.lemko}\n{data.english}  /  {data.polish}";

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos + Vector3.up * 0.8f);
        itemTooltipText.rectTransform.position = screenPos;
        itemTooltipText.gameObject.SetActive(true);
    }

    public void HideItemTooltip()
    {
        if (itemTooltipText != null)
            itemTooltipText.gameObject.SetActive(false);
    }

    void EndGame(bool won)
    {
        gameActive = false;
        gameOverPanel.SetActive(true);
        gameOverText.text = won
            ? $"Вы зробили то!\nScore: {score}"
            : $"Не маш щастя...\nScore: {score}";
    }

    public void Restart() => StartGame();
}