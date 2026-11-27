namespace NightmareRebalance;

internal sealed record FieldObservation(string Owners, string Commands, string Presentation, string Audio, string Chapter, string Flags,
    int SaveCalls, int CommandAdvances, bool GenuineMap, bool Idle, bool Complete);

internal static class FieldContinuationRules
{
    internal static string Compare(FieldObservation? before, FieldObservation? after)
    {
        if (before == null || after == null || !before.GenuineMap || !after.GenuineMap) return "No genuine field baseline";
        if (!before.Idle || !after.Idle) return "Field is not idle";
        if (before.SaveCalls != after.SaveCalls) return "Native save request observed";
        if (before.CommandAdvances != after.CommandAdvances || before.Commands != after.Commands) return "Native event execution changed";
        if (before.Owners != after.Owners || before.Chapter != after.Chapter) return "Field/chapter owner changed";
        if (before.Presentation != after.Presentation || before.Audio != after.Audio) return "Presentation/input/audio owner changed";
        if (before.Flags != after.Flags) return "Observed campaign flags changed";
        if (!before.Complete || !after.Complete) return "Observation incomplete; unknown owners block any lease";
        return "Stable observation only; no reversible session demonstrated";
    }
    internal static bool CanRetain => false; // An observation is never authority to suspend or launch.
}
