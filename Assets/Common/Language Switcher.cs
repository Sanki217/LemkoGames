using UnityEngine;

public class LanguageSwitcher : MonoBehaviour
{
    public void SetLemko() => GameSettings.CurrentLanguage = GameSettings.Language.Lemko;
    public void SetEnglish() => GameSettings.CurrentLanguage = GameSettings.Language.English;
    public void SetPolish() => GameSettings.CurrentLanguage = GameSettings.Language.Polish;
}