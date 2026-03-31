using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public string itemID;

    Renderer rend;
    Color originalColor;

    [Header("Highlight")]
    public Color highlightColor = new Color(1f, 0.85f, 0.2f);
    public float hoverDelay = 1f;

    float hoverTimer = 0f;
    bool isHovering = false;
    bool tooltipShown = false;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    void Update()
    {
        if (isHovering && !tooltipShown)
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= hoverDelay)
            {
                tooltipShown = true;
                if (rend != null)
                    rend.material.color = highlightColor;
                ShopGameManager.Instance.ShowItemTooltip(itemID, transform.position);
            }
        }
    }

    void OnMouseEnter()
    {
        isHovering = true;
        hoverTimer = 0f;
        tooltipShown = false;
    }

    void OnMouseExit()
    {
        isHovering = false;
        hoverTimer = 0f;
        tooltipShown = false;

        if (rend != null)
            rend.material.color = originalColor;

        ShopGameManager.Instance.HideItemTooltip();
    }

    void OnMouseDown()
    {
        ShopGameManager.Instance.TrySelectItem(itemID);
    }
}