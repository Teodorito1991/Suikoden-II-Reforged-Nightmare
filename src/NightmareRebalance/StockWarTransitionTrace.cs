using System.Text.Json;
using HarmonyLib;
using UnityEngine;

namespace NightmareRebalance;

// Test-only witness for a normally played story war. No native state changes.
internal static class StockWarTransitionTrace
{
    internal static readonly bool Enabled =
        Environment.GetCommandLineArgs().Contains("--reforged-stock-war-trace") &&
        File.Exists(Path.Combine(BepInEx.Paths.GameRootPath, "PRIVATE_DO_NOT_UPLOAD.txt"));

    static readonly System.Reflection.PropertyInfo? ManagerReturn =
        AccessTools.Property(typeof(GRChapterManager), "mode_rtn");
    static readonly System.Reflection.FieldInfo? ManagerReturnField =
        ManagerReturn == null ? AccessTools.Field(typeof(GRChapterManager), "mode_rtn") : null;
    static StreamWriter? trace;
    static int rows;
    static bool ended, failed;

    internal static void Record(string stage, int? result = null, bool begin = false, bool close = false)
    {
        if (!Enabled || failed || ended) return;
        try
        {
            if (trace == null)
            {
                if (!begin) return;
                string path = Path.Combine(BepInEx.Paths.BepInExRootPath, "stock-war-transition.private.jsonl");
                trace = new StreamWriter(path, append: false);
            }

            var sys = Framework.Singleton<GAME_WORK>.instance?.sys_work;
            var map = sys?.mcon;
            var manager = GRChapterManager.GRInstance;
            trace.WriteLine(JsonSerializer.Serialize(new
            {
                Frame = Time.frameCount, Stage = stage, Result = result,
                Mode = sys?.game_mode, Steps = sys?.game_step?.ToArray(),
                SysReturn = sys?.mode_rtn,
                ManagerReturn = manager != null ? (ManagerReturn?.GetValue(manager) ?? ManagerReturnField?.GetValue(manager)) : null,
                War = sys?.war_no, MapStep = map?._mstep,
                EventOwner = map?.econ?.Pointer.ToInt64(),
                Area = map?.ano, Town = map?.vno, Map = map?.mno
            }));
            if (++rows % 120 == 0 || close) trace.Flush();
            if (close || rows >= 120_000) Finish();
        }
        catch (Exception e)
        {
            failed = true;
            try { Finish(); } catch { }
            Plugin.Logger.LogWarning("Stock war transition trace stopped: " + e.Message);
        }
    }

    static void Finish()
    {
        ended = true;
        trace?.Dispose();
        trace = null;
    }

    internal static bool WarDestroyed { get; set; }
}

[HarmonyPatch(typeof(EVENTCON), "ECWarEventGo")]
internal static class StockWarTraceEvent
{
    [HarmonyPrefix] static void Prefix() => StockWarTransitionTrace.Record("Event.Before", begin: true);
    [HarmonyPostfix] static void Postfix() => StockWarTransitionTrace.Record("Event.After");
}

[HarmonyPatch(typeof(MACHICON), nameof(MACHICON.MachiMain))]
internal static class StockWarTraceMachi
{
    [HarmonyPrefix] static void Prefix() => StockWarTransitionTrace.Record("Machi.Before");
    [HarmonyPostfix] static void Postfix(int __result) => StockWarTransitionTrace.Record("Machi.After", __result);
}

[HarmonyPatch(typeof(GRChapterManager), "G2_Mode_Change")]
internal static class StockWarTraceMode
{
    [HarmonyPrefix] static void Prefix() => StockWarTransitionTrace.Record("Mode.Before");
    [HarmonyPostfix] static void Postfix() => StockWarTransitionTrace.Record("Mode.After");
}

[HarmonyPatch(typeof(WarChapter), nameof(WarChapter.Start))]
internal static class StockWarTraceStart
{
    [HarmonyPrefix] static void Prefix() => StockWarTransitionTrace.Record("War.Start");
}

[HarmonyPatch(typeof(WarChapter), nameof(WarChapter.Update))]
internal static class StockWarTraceWar
{
    [HarmonyPostfix] static void Postfix(int __result) => StockWarTransitionTrace.Record("War.Update", __result);
}

[HarmonyPatch(typeof(WarChapter), nameof(WarChapter.OnDestroy))]
internal static class StockWarTraceDestroy
{
    [HarmonyPostfix]
    static void Postfix()
    {
        if (!StockWarTransitionTrace.Enabled) return;
        StockWarTransitionTrace.Record("War.Destroyed");
        StockWarTransitionTrace.WarDestroyed = true;
    }
}

[HarmonyPatch(typeof(MapChapter), nameof(MapChapter.Update))]
internal static class StockWarTraceMap
{
    [HarmonyPostfix] static void Postfix(int __result) =>
        StockWarTransitionTrace.Record("Map.Update", __result, close: StockWarTransitionTrace.WarDestroyed);
}
