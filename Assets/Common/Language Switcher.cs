using UnityEngine;

public class LanguageSwitcher : MonoBehaviour
{
    public ShoppingListUI listUI; // drag in via Inspector

    public void SetLemko()
    {
        GameSettings.CurrentLanguage = GameSettings.Language.Lemko;
        listUI.RefreshLanguage();
    }

    public void SetEnglish()
    {
        GameSettings.CurrentLanguage = GameSettings.Language.English;
        listUI.RefreshLanguage();
    }

    public void SetPolish()
    {
        GameSettings.CurrentLanguage = GameSettings.Language.Polish;
        listUI.RefreshLanguage();
    }
}