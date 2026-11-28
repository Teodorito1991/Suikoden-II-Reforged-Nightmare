using HarmonyLib;
namespace NightmareRebalance;
// Owned-session controls. Ordinary campaign/system settings saves are untouched.
[HarmonyPatch(typeof(SystemSave), nameof(SystemSave.Save))]
internal static class TrainingNativeSave
{
    internal readonly record struct PendingSaveState(bool HasCallback, ArmyProgressionRules.PendingCampaignSave? Pending);

    static bool Prefix(string path, string json, ref Il2CppSystem.Action<bool>? cb, ref bool __result, out PendingSaveState __state)
    {
        __state = default;
        if ((!TrainingPersistenceRules.IsCampaignPath(path) || (!HallLucaSession.Active && !WalkableDungeonRuntime.WarpTransitionPending)) &&
            TrainingWar.AllowNativeSave(path))
        {
            if (!TrainingWar.Active && !HallLucaSession.Active && !WalkableDungeonRuntime.WarpTransitionPending &&
                ArmyProgressionRuntime.CapturePendingSave(path) is {} pending)
            {
                if (cb != null)
                {
                    var original = cb;
                    var captured = pending;
                    cb = (Il2CppSystem.Action<bool>)new Action<bool>(ok =>
                    {
                        try { if (ok) ArmyProgressionRuntime.CommitObservedCampaignSave(captured, true); }
                        finally { original.Invoke(ok); }
                    });
                    __state = new(true, captured);
                }
                else
                {
                    __state = new(false, pending);
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
        // when a callback was supplied, binding happens only on cb(true) using the frozen snapshot.
        if (__result && !__state.HasCallback && __state.Pending != null &&
            !TrainingWar.Active && !HallLucaSession.Active)
            ArmyProgressionRuntime.CommitObservedCampaignSave(__state.Pending, true);
    }
}
[HarmonyPatch(typeof(UISaveLoad2), nameof(UISaveLoad2.Save))]
internal static class TrainingSaveIntent
{
    static bool Prefix(Il2CppSystem.Action<bool, GSD2SaveData.TmpSave>? onFinish)
    {
        if (!HallLucaSession.Active && !WalkableDungeonRuntime.WarpTransitionPending && TrainingWar.AllowSaveIntent()) return true;
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
