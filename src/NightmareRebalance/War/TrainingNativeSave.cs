using HarmonyLib;
namespace NightmareRebalance;
// Owned-session controls. Ordinary campaign/system settings saves are untouched.
[HarmonyPatch(typeof(SystemSave), nameof(SystemSave.Save))]
internal static class TrainingNativeSave
{
    internal readonly record struct PendingSaveState(bool HasCallback, string Path, string? Hash, IntPtr Owner);

    static bool Prefix(string path, string json, ref Il2CppSystem.Action<bool>? cb, ref bool __result, out PendingSaveState __state)
    {
        __state = default;
        if ((!TrainingPersistenceRules.IsCampaignPath(path) || !HallLucaSession.Active) &&
            TrainingWar.AllowNativeSave(path))
        {
            if (!TrainingWar.Active && !HallLucaSession.Active &&
                ArmyProgressionRuntime.CapturePendingSave(path, out string? hash, out IntPtr owner))
            {
                if (cb != null)
                {
                    var original = cb;
                    string capturedPath = path;
                    string? capturedHash = hash;
                    IntPtr capturedOwner = owner;
                    cb = (Il2CppSystem.Action<bool>)new Action<bool>(ok =>
                    {
                        if (ok) ArmyProgressionRuntime.CommitObservedCampaignSave(capturedPath, capturedHash, capturedOwner, true);
                        original.Invoke(ok);
                    });
                    __state = new(true, path, hash, owner);
                }
                else
                {
                    __state = new(false, path, hash, owner);
                }
            }
            return true;
        }
        __result = false;
        // Follow the original failure contract: callers must not wait forever
        // for a completion callback after we refused the write.
        try { cb?.Invoke(false); }
        catch (Exception error) { Plugin.Logger.LogWarning("Refused save callback failed: " + error.GetType().Name); }
        return false;
    }

    static void Postfix(string path, bool __result, PendingSaveState __state)
    {
        // Only commit synchronously here when the caller supplied no completion callback;
        // when a callback was supplied, binding happens only on cb(true).
        if (__result && !__state.HasCallback && !string.IsNullOrWhiteSpace(__state.Hash) &&
            !TrainingWar.Active && !HallLucaSession.Active)
            ArmyProgressionRuntime.CommitObservedCampaignSave(__state.Path, __state.Hash, __state.Owner, true);
    }
}
[HarmonyPatch(typeof(UISaveLoad2), nameof(UISaveLoad2.Save))]
internal static class TrainingSaveIntent
{
    static bool Prefix(Il2CppSystem.Action<bool, GSD2SaveData.TmpSave>? onFinish)
    {
        if (!HallLucaSession.Active && TrainingWar.AllowSaveIntent()) return true;
        TrainingSaveFailure.Complete(onFinish);
        return false;
    }
}
internal static class TrainingSaveFailure
{
    internal static void Complete(Il2CppSystem.Action<bool, GSD2SaveData.TmpSave>? callback)
    {
        try { callback?.Invoke(false, null!); }
        catch (Exception error) { Plugin.Logger.LogWarning("Refused save callback failed: " + error.GetType().Name); }
    }
}
[HarmonyPatch(typeof(GSD2SaveData), nameof(GSD2SaveData.LoadDataCore))]
internal static class TrainingLoaded
{
    static void Postfix()
    {
        TrainingWar.NativeLoadCompleted();
        ArmyProgressionRuntime.ObserveCampaignLoad();
    }
}
