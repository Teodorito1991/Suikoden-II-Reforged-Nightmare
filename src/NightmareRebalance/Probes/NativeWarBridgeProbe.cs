using HarmonyLib;
using System.Text.Json;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using ReleaseTable = Il2CppSystem.Collections.Generic.Dictionary<ReleaseCheck.ReleaseTimming, ReleaseCheck.ReleaseInfo>;

namespace NightmareRebalance;

// Executes the resource-acquisition / native-teardown portion only, on a copied
// unloaded runtime. Never calls ECWarEventGo, WarChapter.Start/Update or Request.
public sealed class NativeWarBridgeProbe : MonoBehaviour
{
    internal static bool Requested => File.Exists(Path.Combine(BepInEx.Paths.GameRootPath, "PRIVATE_DO_NOT_UPLOAD.txt")) &&
        Environment.GetCommandLineArgs().Contains("--reforged-private-startup-smoke") &&
        Plugin.Options.RunNativeWarSelfTest.Value &&
        Plugin.Options.RunCombatSelfTest.Value && Application.isBatchMode &&
        Environment.GetCommandLineArgs().Contains("--reforged-war-bridge-probe") &&
        Environment.GetCommandLineArgs().Contains("--reforged-test-exit");
    internal static bool Scheduled, Passed, Finished;
    internal static int Assertions;
    const string WarKey = "gsd2-classic-war-war_01-war_01";
    const string FieldKey = "gsd2-classic-machi-vk08-vk08";
    ResourceLoader? field, war;
    NativeWarReleaseDomain? domain;
    int cycle;
    float started;
    bool previousBackground;
    NativeWarFrame? baseline;
    readonly List<object> rows = new();
    string? failure;
    public NativeWarBridgeProbe(IntPtr p) : base(p) { }
    static void Check(bool ok, string label) { Assertions++; if (!ok) throw new InvalidOperationException("War bridge: " + label); }

    public void Update()
    {
        if (Finished || !Requested) return;
        try
        {
            if (started == 0)
            {
                started = Time.realtimeSinceStartup; previousBackground = Application.runInBackground; Application.runInBackground = true;
                baseline = NativeWarWitness.Capture();
                foreach (var method in new[] { AccessTools.Method(typeof(ReleaseCheck), nameof(ReleaseCheck.AddLoader)), AccessTools.Method(typeof(ReleaseCheck), nameof(ReleaseCheck.Invoke)), AccessTools.Method(typeof(ReleaseCheck.ReleaseInfo), nameof(ReleaseCheck.ReleaseInfo.Invoke)), AccessTools.Method(typeof(ResourceLoader), nameof(ResourceLoader.Unload)), AccessTools.Method(typeof(WarChapter), nameof(WarChapter.OnDestroy)) })
                    Check(method != null && Harmony.GetPatchInfo(method)?.Owners.All(x => x == Plugin.Id) != false, "No foreign patch owner on release seam");
                field = ResourceLoader.LoadAssetAsync<MACHIDAT>(FieldKey);
                return;
            }
            if (Time.realtimeSinceStartup - started > 45) throw new TimeoutException("Isolated resource boundary probe timed out.");
            if (field == null || field.IsLoading) return;
            Check(field.handle.IsValid() && field.handle.Status == AsyncOperationStatus.Succeeded && field.Asset?.name == "vk08", "Retained original castle asset acquired");
            Check(CastleGuildRuntime.Matches(field.Asset!.Cast<MACHIDAT>()), "Retained castle source passes existing full tavern signature guard");
            if (war == null)
            {
                domain = new NativeWarReleaseDomain();
                war = ResourceLoader.LoadAssetAsync<OPD_RES2>(WarKey);
                return;
            }
            if (war.IsLoading) return;
            Check(war.handle.IsValid() && war.handle.Status == AsyncOperationStatus.Succeeded && war.Asset?.name == "war_01", "Original War1 resource acquired");
            var data = war.Asset!.Cast<OPD_RES2>();
            Check(data.map?.Count == 1 && data.opd_tbl?.Count == 19, "Audited War1 map and unit sprite record counts");
            var retainedTable = ReleaseCheck.releaseDelegate;
            string originalTableBefore = TableFingerprint();
            var retainedAsset = field.Asset!.Pointer;
            int maps = data.map!.Count, sprites = data.opd_tbl!.Count;
            // Explicit scratch retained-field ledger WITH a conflicting timing-4
            // registration. Never add a sentinel to the real launcher's ledger.
            var retainedFixture = new ReleaseTable();
            try
            {
            ReleaseCheck.releaseDelegate = retainedFixture;
            ReleaseCheck.AddLoader(field, (ReleaseCheck.ReleaseTimming)4);
            string tableBefore = TableFingerprint();
            var fault = new NativeWarReleaseDomain();
            bool injected = false;
            try { fault.Within(() => throw new ApplicationException("Injected release-scope fault")); }
            catch (ApplicationException) { injected = true; }
            Check(injected && !NativeWarReleaseDomain.Busy && ReleaseCheck.releaseDelegate.Pointer == retainedFixture.Pointer && TableFingerprint() == tableBefore, "Original ledger restored even when bounded operation throws");
            bool faultRejected = false;
            try { fault.Within(() => { }); } catch (InvalidOperationException) { faultRejected = true; }
            Check(faultRejected, "Faulted domain cannot be reused");
            bool nested = false;
            domain!.Within(() => { try { new NativeWarReleaseDomain().Within(() => { }); } catch (InvalidOperationException) { nested = true; } });
            Check(nested, "Nested ownership rejected");
            bool threadRejected = System.Threading.Tasks.Task.Run(() => { try { domain.Register(war, WarResourceOwner.SessionAcquisition); return false; } catch (InvalidOperationException) { return true; } }).GetAwaiter().GetResult();
            Check(threadRejected && domain.Registrations == 0, "Foreign thread rejected before native registration");
            domain!.Register(war, WarResourceOwner.SessionAcquisition);
            bool duplicate = false;
            try { domain.Register(war, WarResourceOwner.SessionAcquisition); } catch (InvalidOperationException) { duplicate = true; }
            Check(duplicate && domain.Registrations == 1, "Duplicate loader rejected before native registration");
            Check(ReleaseCheck.releaseDelegate?.Pointer == retainedFixture.Pointer && TableFingerprint() == tableBefore, "Retained timing-4 ledger untouched by registration");
            Check(domain.Close(), "Original scratch WarChapter destructor executed");
            Check(!domain.Close() && domain.NativeTeardowns == 1 && domain.Empty, "Exactly-once native teardown consumes private bucket");
            Check(ReleaseCheck.releaseDelegate?.Pointer == retainedFixture.Pointer && TableFingerprint() == tableBefore, "Retained timing-4 ledger restored exactly");
            Check(!war.handle.IsValid(), "Exclusive War1 addressable handle released by original engine");
            // This is an actually loaded field resource sentinel, not a live field.
            Check(field.handle.IsValid() && field.Asset.Pointer == retainedAsset && field.Asset.Cast<MACHIDAT>().epersondat != null, "Separately retained castle handle survives native war teardown");
            }
            finally { ReleaseCheck.releaseDelegate = retainedTable; }
            Check(ReleaseCheck.releaseDelegate?.Pointer == retainedTable?.Pointer && TableFingerprint() == originalTableBefore, "Actual launcher release ledger restored untouched");
            Check(NativeWarWitness.Capture() == baseline && war_h.g_w_war_no == 0 && !WarBoundaryRules.CanLaunch, "Startup campaign evidence unchanged; launcher remains unavailable");
            rows.Add(new { Cycle = ++cycle, WarAsset = "war_01", MapRecords = maps, SpriteRecords = sprites, OriginalWarDestructorExecuted = true, ExclusiveWarHandleReleased = true, RetainedCastleHandleValid = true, OriginalReleaseTableUnchanged = true, TacticalBattle = false, FieldReturn = false });
            war = null; domain = null;
            if (cycle == 2) { Passed = true; Finish(); }
        }
        catch (Exception e) { failure = e.GetType().Name + ": " + e.Message; Plugin.Logger.LogError("Native war bridge probe failed: " + failure); Finish(); }
    }

    // Private diagnostics only; do not publish raw pointers or this fingerprint.
    static string TableFingerprint()
    {
        var table = ReleaseCheck.releaseDelegate;
        if (table == null) return "null";
        var entries = new List<string>();
        foreach (var pair in table)
        {
            var info = pair.Value;
            var pointers = new List<long>();
            for (int i = 0; i < info.loaderList.Count; i++) pointers.Add(info.loaderList[i].Pointer.ToInt64());
            entries.Add(((int)pair.Key) + ":" + info.Pointer + ":" + info.releaseAction?.Pointer + ":" + string.Join(",", pointers));
        }
        return string.Join(";", entries.OrderBy(x => x));
    }

    void Finish()
    {
        // Native unload must be invoked once even when later assertions fail.
        try
        {
            if (domain is { Registrations: > 0 }) { if (!domain.TeardownAttempted) domain.Close(); }
            else if (domain?.Faulted == true) { Passed = false; failure = "Uncertain registration; no repeated unload attempted"; }
            else if (war != null && war.handle.IsValid()) war.Unload();
        }
        catch (Exception e) { Passed = false; failure = "Cleanup failure: " + e.GetType().Name; }
        try { if (field != null && field.handle.IsValid()) field.Unload(); }
        catch (Exception e) { Passed = false; failure = "Retained resource cleanup failure: " + e.GetType().Name; }
        war = field = null; domain = null;
        if (NativeWarReleaseDomain.Busy) { Passed = false; failure = "Release ownership scope not restored"; }
        Application.runInBackground = previousBackground;
        Finished = true;
        string dir = Path.Combine(BepInEx.Paths.BepInExRootPath, "ReforgedNightmare", "native-war"); Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "bridge-probe.json"), JsonSerializer.Serialize(new { Version = "1.18.0-field-observation.1", Passed, Assertions, Failure = failure, Cycles = rows, OriginalResourceLoaderExecuted = true, GenuineTacticalBattle = false, GenuineFieldReturn = false, EntryInterpreterInvoked = false, CampaignSaveLoaded = false, EarnedOutcome = false, CommanderXpGranted = 0 }, new JsonSerializerOptions { WriteIndented = true }));
        Plugin.Logger.LogInfo($"Native war bridge probe {(Passed ? "PASS" : "FAIL")}: {cycle} actual resource acquisition/teardown cycles; no tactical battle or field return.");
        Application.Quit();
    }
}
