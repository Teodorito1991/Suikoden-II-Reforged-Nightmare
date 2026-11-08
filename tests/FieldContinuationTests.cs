using NightmareRebalance;

internal static class FieldContinuationTests
{
    internal static void Run()
    {
        int n = 0;
        void Check(bool ok) { n++; if (!ok) throw new Exception("Field observation must fail closed."); }
        var original = new FieldObservation("owners", "commands", "render", "audio", "chapter", "flags", 0, 0, true, true, true);
        Check(FieldContinuationRules.Compare(null, original).StartsWith("No genuine"));
        Check(FieldContinuationRules.Compare(original, original with { GenuineMap = false }).StartsWith("No genuine"));
        Check(FieldContinuationRules.Compare(original, original with { Complete = false }).StartsWith("Observation incomplete"));
        Check(FieldContinuationRules.Compare(original, original with { Idle = false }).StartsWith("Field is not idle"));
        Check(FieldContinuationRules.Compare(original, original with { SaveCalls = 1 }).StartsWith("Native save"));
        Check(FieldContinuationRules.Compare(original, original with { CommandAdvances = 1 }).StartsWith("Native event"));
        Check(FieldContinuationRules.Compare(original, original with { Commands = "changed" }).StartsWith("Native event"));
        Check(FieldContinuationRules.Compare(original, original with { Owners = "changed" }).StartsWith("Field/chapter"));
        Check(FieldContinuationRules.Compare(original, original with { Chapter = "changed" }).StartsWith("Field/chapter"));
        Check(FieldContinuationRules.Compare(original, original with { Presentation = "changed" }).StartsWith("Presentation"));
        Check(FieldContinuationRules.Compare(original, original with { Audio = "changed" }).StartsWith("Presentation"));
        Check(FieldContinuationRules.Compare(original, original with { Flags = "changed" }).StartsWith("Observed campaign"));
        Check(FieldContinuationRules.Compare(original, original).StartsWith("Stable observation only"));
        Check(!FieldContinuationRules.CanRetain);
        Console.WriteLine($"PASS: Field continuation {n} assertions; missing/changed/unknown owners and stable observations cannot authorize a lease. Synthetic records only.");
    }
}
