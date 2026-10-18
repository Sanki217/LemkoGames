namespace Lemko.Editor
{
    public static class PrototypeCompletionChecks
    {
        public static void RunBatch()
        {
            PrototypeVisualRepair.Apply();
            PrototypeDataChecks.RunBatch();
            PrototypePlayChecks.RunBatch();
        }
    }
}
