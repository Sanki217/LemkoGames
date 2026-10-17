namespace Lemko
{
    // Same rule as the original Game 1: 100 * consecutive correct answers.
    public sealed class ShopScore
    {
        public int Points { get; private set; }
        public int Streak { get; private set; }
        public int NextMultiplier => Streak + 1;
        public int Apply(SelectionResult result)
        {
            if(result == SelectionResult.Ignored) return 0;
            if(result == SelectionResult.Incorrect) { Streak=0; return 0; }
            int awarded=100 * ++Streak; Points+=awarded; return awarded;
        }
    }
}
