namespace NightmareRebalance;
// Legacy Raider policy is retained by the safe top-level native target hook.
// Ref-array helper detours are deliberately absent: generated IL2CPP originals
// forward their ref-array arguments incorrectly on this verified runtime.
internal static class TargetingRuntime
{
    internal static readonly HashSet<string> Raiders=ContentRules.Read<EnemyFile>(ContentRuntime.Resource("enemy-profiles.json")).Profiles.Where(e=>e.Role=="Raider").Select(e=>e.Id).ToHashSet(StringComparer.Ordinal);
    internal static int Changes;
}
