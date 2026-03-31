public static class GameSettings
{
    public enum Language { Lemko, English, Polish }
    public static Language CurrentLanguage = Language.Lemko;

    public static string GetName(ShopItemData item)
    {
        return CurrentLanguage switch
        {
            Language.English => item.english,
            Language.Polish => item.polish,
            _ => item.lemko,
        };
    }
}