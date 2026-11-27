using HarmonyLib;
using UnityEngine;
using DG.Tweening;
using System.Text.Json;

namespace NightmareRebalance;

public sealed class NativeWarUiProbe : MonoBehaviour
{
    internal static bool Requested => Plugin.Options.RunNativeWarSelfTest.Value && Plugin.Options.RunCombatSelfTest.Value && Application.isBatchMode &&
        Environment.GetCommandLineArgs().Contains("--reforged-war-ui-probe") && Environment.GetCommandLineArgs().Contains("--reforged-test-exit") &&
        Environment.GetCommandLineArgs().Contains("--reforged-private-startup-smoke") && File.Exists(Path.Combine(BepInEx.Paths.GameRootPath, "PRIVATE_DO_NOT_UPLOAD.txt"));
    internal static bool Scheduled;
    ResourceLoader? field, prefab;
    NativeWarUiLease? lease;
    NativeWarReleaseDomain? auditDomain;
    string? baseline, failure;
    int cycles, cleanupFrame, checks;
    float started;
    bool finished;
    readonly List<object> rows = new();
    readonly NativeWarUiAudit audit = new();
    string? fontBaseline;
    int runtimeExceptions;
    Application.LogCallback? logObserver;
    public NativeWarUiProbe(IntPtr p) : base(p) { }
    void Check(bool ok, string label) { checks++; if (!ok) throw new InvalidOperationException(label); }

    public void Update()
    {
        if (finished || !Requested) return;
        try
        {
            if (started == 0)
            {
                started = Time.realtimeSinceStartup; Application.runInBackground = true;
                logObserver = (Application.LogCallback)(Action<string, string, LogType>)((_, _, type) => { if (type is LogType.Exception or LogType.Error or LogType.Assert) runtimeExceptions++; });
                Application.add_logMessageReceived(logObserver);
                field = ResourceLoader.LoadAssetAsync<MACHIDAT>("gsd2-classic-machi-vk08-vk08"); return;
            }
            if (Time.realtimeSinceStartup - started > 55) throw new TimeoutException("Native UI ownership probe timed out.");
            if (field == null || field.IsLoading || Time.realtimeSinceStartup - started < 2) return;
            Check(field.handle.IsValid() && CastleGuildRuntime.Matches(field.Asset.Cast<MACHIDAT>()), "Retained field resource signature.");
            if (prefab == null)
            {
                baseline ??= NativeWarUiAudit.Globals();
                auditDomain = new NativeWarReleaseDomain();
                prefab = ResourceLoader.LoadAssetAsync<GameObject>("share-hd-ui-war-ui_war_header"); return;
            }
            if (prefab.IsLoading) return;
            if (lease == null)
            {
                Check(prefab.handle.IsValid() && prefab.Asset?.name == "UI_War_Header", "Original header prefab source.");
                audit.Audit(prefab.Asset!.Cast<GameObject>());
                auditDomain!.Register(prefab, WarResourceOwner.SessionAcquisition);
                fontBaseline = NativeWarUiAudit.FontRegistry();
                lease = new NativeWarUiLease(audit.borrowedMaterials, prefab.Asset!.Cast<GameObject>(), field.Asset.Cast<MACHIDAT>());
            }
            if (cleanupFrame == 0)
            {
                if (!lease.Step()) { Check(NativeWarUiAudit.Globals() == baseline, "Original hierarchy/resources restored between factory stages."); return; }
                Check(NativeWarUiAudit.Globals() == baseline && prefab.handle.IsValid(), "Native factory preserved unrelated owners and audit reference.");
                lease.RequestCleanup(); lease.RequestCleanup(); cleanupFrame = Time.frameCount; return;
            }
            if (Time.frameCount - cleanupFrame < 3) return;
            auditDomain!.Close();
            Check(lease.CleanupComplete(prefab), "Native component callbacks and prefab acquisitions cleaned.");
            Check(NativeWarUiAudit.FontRegistry() == fontBaseline, "Original font callback registry restored exactly.");
            Check(audit.sharedFont != null && audit.sharedFontMaterial != null && SystemObject.GetLanguageFont().Pointer == audit.sharedFont.Pointer && audit.sharedFont.material.Pointer == audit.sharedFontMaterial.Pointer, "Original shared font/material preserved.");
            Check(runtimeExceptions == 0, "Unity error/exception during owned UI lifecycle.");
            Check(NativeWarUiAudit.Globals() == baseline && field.handle.IsValid(), "Original roots/static war caches and retained field preserved.");
            rows.Add(new { Cycle = ++cycles, lease.Moves, lease.CreatedTweens, lease.CreatedMaterials, NativeFontRegistryRestored = true, OriginalFactoryExecuted = true, OriginalRoot7Restored = true,
                NativeOnDestroyTweenCleanupVerified = true, lease.FactoryLoaderReleased, SharedFontMaterialPreserved = true, RuntimeErrorsBeforeQuit = runtimeExceptions, OwnedObjectsDestroyed = true, VisibleRenderingAccepted = false, TacticalBattle = false });
            prefab = null; lease = null; auditDomain = null; cleanupFrame = 0;
            if (cycles == 2) { Check(!WarBoundaryRules.CanLaunch, "Launcher remains unavailable."); Finish(true); }
        }
        catch (Exception e) { failure = e.GetType().Name + ": " + e.Message; Plugin.Logger.LogError("Native war UI ownership failed: " + failure); Finish(false); }
    }

    void Finish(bool passed)
    {
        finished = true;
        if (logObserver != null) Application.remove_logMessageReceived(logObserver);
        try { if (field != null && field.handle.IsValid()) field.Unload(); }
        catch (Exception e) { passed = false; failure = e.GetType().Name; }
        try
        {
            string dir = Path.Combine(BepInEx.Paths.BepInExRootPath, "ReforgedNightmare", "native-war"); Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "ui-ownership-probe.json"), JsonSerializer.Serialize(new { Version = "1.18.0-field-observation.1", Passed = passed, Checks = checks, Failure = failure, Cycles = rows,
                GraphicsEnabled = !Environment.GetCommandLineArgs().Contains("-nographics"), NativeHeaderFactory = true, FullWarResLoad = false, ActiveWarChapter = false, FieldReturn = false, CampaignLoaded = false }, new JsonSerializerOptions { WriteIndented = true }));
            Plugin.Logger.LogInfo($"Native war UI ownership {(passed ? "PASS" : "FAIL")}: {cycles} native header creation/cleanup cycles; no battle or field return.");
        }
        catch (Exception e) { Plugin.Logger.LogError("Native UI diagnostics unavailable: " + e.GetType().Name); }
        finally { Application.Quit(); }
    }
}
