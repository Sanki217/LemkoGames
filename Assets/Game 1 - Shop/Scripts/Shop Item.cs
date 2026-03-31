using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public string itemID;

    Renderer rend;
    Color originalColor;

    [Header("Highlight")]
    public Color highlightColor = new Color(1f, 0.85f, 0.2f); // golden yellow

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    void OnMouseEnter()
    {
        if (rend != null)
            rend.material.color = highlightColor;

        ShopGameManager.Instance.ShowItemTooltip(itemID, transform.position);
    }

    void OnMouseExit()
    {
        if (rend != null)
            rend.material.color = originalColor;

        ShopGameManager.Instance.HideItemTooltip();
    }

    void OnMouseDown()
    {
        ShopGameManager.Instance.TrySelectItem(itemID);
    }
}