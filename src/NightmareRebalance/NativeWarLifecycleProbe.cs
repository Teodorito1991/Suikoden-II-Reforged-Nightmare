using HarmonyLib;
using System.Text.Json;
using UnityEngine;

namespace NightmareRebalance;

public sealed class NativeWarLifecycleProbe : MonoBehaviour
{
    internal static bool Requested => Plugin.Options.RunNativeWarSelfTest.Value && Plugin.Options.RunCombatSelfTest.Value &&
        Application.isBatchMode && Environment.GetCommandLineArgs().Contains("--reforged-war-lifecycle-probe") &&
        Environment.GetCommandLineArgs().Contains("--reforged-test-exit") && Environment.GetCommandLineArgs().Contains("--reforged-private-startup-smoke") &&
        File.Exists(Path.Combine(BepInEx.Paths.GameRootPath, "PRIVATE_DO_NOT_UPLOAD.txt"));
    internal static bool Scheduled;
    ResourceLoader? field;
    NativeWarOpdStage? stage;
    int cycles, assertions;
    float started;
    bool finished;
    string? baseline, failure;
    readonly List<object> rows = new();
    public NativeWarLifecycleProbe(IntPtr p) : base(p) { }
    void Check(bool value, string label) { assertions++; if (!value) throw new InvalidOperationException(label); }

    public void Update()
    {
        if (finished || !Requested) return;
        try
        {
            if (started == 0)
            {
                started = Time.realtimeSinceStartup;
                foreach (var method in new[] { AccessTools.Method(typeof(OPDResLoader), nameof(OPDResLoader.LoadOPDRes)), AccessTools.Method(typeof(OPDResLoader), nameof(OPDResLoader.GetData)), AccessTools.Method(typeof(OPDResLoader), nameof(OPDResLoader.RemoveResPack)), AccessTools.Method(typeof(OPD_DATA), nameof(OPD_DATA.LoadAtlas), new[] { typeof(string) }), AccessTools.Method(typeof(SystemObject), nameof(SystemObject.Changemode)) })
                    Check(method != null && Harmony.GetPatchInfo(method)?.Owners.All(x => x == Plugin.Id) != false, "Foreign OPD patch owner.");
                field = ResourceLoader.LoadAssetAsync<MACHIDAT>("gsd2-classic-machi-vk08-vk08");
                return;
            }
            if (Time.realtimeSinceStartup - started > 45) throw new TimeoutException("Native lifecycle stage timed out.");
            if (field == null || field.IsLoading) return;
            Check(field.handle.IsValid() && field.Asset != null && CastleGuildRuntime.Matches(field.Asset.Cast<MACHIDAT>()), "Retained castle source guard.");
            baseline ??= Fingerprint();
            stage ??= new NativeWarOpdStage(field.Asset!.Cast<MACHIDAT>());
            if (!stage.Step()) { Check(Fingerprint() == baseline, "Caches restored between asynchronous stages."); return; }
            Check(stage.OriginalCacheRestored && stage.AtlasNoOpStages == 19, "Real OPD cache and atlas-stub stages completed.");
            stage.Close(); stage.Close();
            Check(stage.HandleReleased && field.handle.IsValid(), "Native cleanup released war acquisition, retained castle handle valid.");
            Check(Fingerprint() == baseline, "Unrelated native caches preserved after cleanup.");
            rows.Add(new { Cycle = ++cycles, stage.Moves, stage.AtlasNoOpStages, RealNativeOpdCoroutineCompleted = true, PrivateCachePublishedAndLookedUp = true,
                stage.HandleReleased, RetainedCastleResourceValid = true, SourceSpriteStateUnchanged = true,
                SharedReferences = stage.Ownership.Count(WarResourceOwner.SharedGame), RetainedFieldReferences = stage.Ownership.Count(WarResourceOwner.RetainedField),
                UnknownOwnership = stage.Ownership.Unknown, NativeAtlasTexturesLoaded = false, FieldReturn = false });
            stage = null;
            if (cycles == 2)
            {
                Check(NativeWarOpdStage.RehearseFault(field.Asset!.Cast<MACHIDAT>()), "Cache, routing mode and release owner restored after injected scope exception.");
                var uncertain = new NativeWarReleaseDomain();
                bool admissionBlocked = false;
                try { uncertain.Register(field, WarResourceOwner.Unknown); } catch (InvalidOperationException) { admissionBlocked = true; }
                bool cleanupBlocked = false;
                try { uncertain.Close(); } catch (InvalidOperationException) { cleanupBlocked = true; }
                Check(admissionBlocked && cleanupBlocked && uncertain.NativeTeardowns == 0 && field.handle.IsValid(), "Unknown acquired-reference ownership blocks registration and cleanup.");
                // Original native registration, but UNKNOWN callback: must NOT run.
                bool callbackRan = false;
                var unknown = new NativeWarReleaseDomain();
                unknown.Within(() => ReleaseCheck.AddFunc((Il2CppSystem.Action)(Action)(() => callbackRan = true), (ReleaseCheck.ReleaseTimming)4));
                bool blocked = false; try { unknown.Close(); } catch (InvalidOperationException) { blocked = true; }
                Check(blocked && !callbackRan && unknown.NativeTeardowns == 0 && unknown.Ownership.Unknown > 0, "Unclassified native callback blocks teardown before execution.");
                Check(Fingerprint() == baseline && !WarBoundaryRules.CanLaunch, "Actual globals unchanged; chapter launch unavailable.");
                Finish(true);
            }
        }
        catch (Exception e) { failure = e.GetType().Name + ": " + e.Message; Plugin.Logger.LogError("Native war lifecycle probe failed: " + failure); Finish(false); }
    }

    static string Fingerprint()
    {
        var cache = OPDResLoader.opdData;
        var rows = new List<string>();
        if (cache != null) foreach (var pair in cache) rows.Add(pair.Key + ":" + pair.Value.Pointer);
        rows.Sort();
        var release = ReleaseCheck.releaseDelegate;
        if (release != null) foreach (var pair in release)
        {
            var list = new List<long>(); foreach (var loader in pair.Value.loaderList) list.Add(loader.Pointer.ToInt64());
            rows.Add($"release:{pair.Key}:{pair.Value.Pointer}:{pair.Value.releaseAction?.Pointer}:" + string.Join(",", list));
        }
        for (int i = 0; i < ResourceLoader.handleList.Count; i++) rows.Add("handle:" + ResourceLoader.handleList[i].GetHashCode() + ":" + ResourceLoader.handleList[i].IsValid());
        foreach (var asset in ResourceLoader.cacheList) rows.Add("cachedAsset:" + asset?.Pointer);
        return SystemObject.mode + "|" + cache?.Pointer + "|" + release?.Pointer + "|" + string.Join(";", rows);
    }

    void Finish(bool passed)
    {
        finished = true;
        // Unknown ownership aborts the experiment. Never guess a callback cleanup
        // or retry a potentially consumed reference; the disposable process exits.
        try { if (field != null && field.handle.IsValid()) field.Unload(); }
        catch (Exception e) { passed = false; failure = "Retained acquisition cleanup: " + e.GetType().Name; }
        try
        {
            string directory = Path.Combine(BepInEx.Paths.BepInExRootPath, "ReforgedNightmare", "native-war"); Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "resource-lifecycle-probe.json"), JsonSerializer.Serialize(new { Version = "1.15.0-native-war-resources.1", Passed = passed, Assertions = assertions, Failure = failure, Cycles = rows,
                UnknownCallbackExecutionBlocked = passed, GenuineTacticalBattle = false, LiveFieldReturn = false, WarChapterStartInvoked = false, CampaignLoaded = false, SaveWritten = false }, new JsonSerializerOptions { WriteIndented = true }));
            Plugin.Logger.LogInfo($"Native war resource lifecycle {(passed ? "PASS" : "FAIL")}: {cycles} OPD coroutine/cache/cleanup cycles; no battle or field return.");
        }
        catch (Exception e) { Plugin.Logger.LogError("Native lifecycle diagnostic write failed: " + e.GetType().Name); }
        finally { Application.Quit(); }
    }
}
