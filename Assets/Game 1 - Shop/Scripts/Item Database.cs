using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ShopItems", menuName = "LemkoGame/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ShopItemData> items = new List<ShopItemData>
    {
        new ShopItemData { itemID = "apple",   lemko = "яблуко",  english = "apple",  polish = "jabłko"  },
        new ShopItemData { itemID = "carrot",  lemko = "морква",  english = "carrot", polish = "marchew" },
        new ShopItemData { itemID = "bread",   lemko = "хліб",    english = "bread",  polish = "chleb"   },
        new ShopItemData { itemID = "milk",    lemko = "молоко",  english = "milk",   polish = "mleko"   },
        new ShopItemData { itemID = "meat",    lemko = "м'ясо",   english = "meat",   polish = "mięso"   },
        new ShopItemData { itemID = "tomato",  lemko = "помідор", english = "tomato", polish = "pomidor" },
        new ShopItemData { itemID = "onion",   lemko = "цибуля",  english = "onion",  polish = "cebula"  },
        new ShopItemData { itemID = "cheese",  lemko = "сир",     english = "cheese", polish = "ser"     },
        new ShopItemData { itemID = "pear",    lemko = "груша",   english = "pear",   polish = "gruszka" },
    };
}