using HarmonyLib;
namespace NightmareRebalance;
// Owned-session controls. Ordinary campaign/system settings saves are untouched.
[HarmonyPatch(typeof(SystemSave), nameof(SystemSave.Save))]
internal static class TrainingNativeSave
{
    static bool Prefix(string path, string json, Il2CppSystem.Action<bool>? cb, ref bool __result)
    {
        if ((!TrainingPersistenceRules.IsCampaignPath(path) || !HallLucaSession.Active) &&
            TrainingWar.AllowNativeSave(path)) return true;
        __result = false;
        // Follow the original failure contract: callers must not wait forever
        // for a completion callback after we refused the write.
        try { cb?.Invoke(false); }
        catch (Exception error) { Plugin.Logger.LogWarning("Refused save callback failed: " + error.GetType().Name); }
        return false;
    }

    static void Postfix(string path, bool __result)
    {
        if (__result && !TrainingWar.Active && !HallLucaSession.Active)
            ArmyProgressionRuntime.ObserveCampaignSave(path);
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
