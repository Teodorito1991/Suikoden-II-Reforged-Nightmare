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
    readonly HashSet<IntPtr> borrowedMaterials = new();
    string? fontBaseline;
    int runtimeExceptions;
    Application.LogCallback? logObserver;
    TMPro.TMP_FontAsset? sharedFont;
    Material? sharedFontMaterial;
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
                baseline ??= Globals();
                auditDomain = new NativeWarReleaseDomain();
                prefab = ResourceLoader.LoadAssetAsync<GameObject>("share-hd-ui-war-ui_war_header"); return;
            }
            if (prefab.IsLoading) return;
            if (lease == null)
            {
                Check(prefab.handle.IsValid() && prefab.Asset?.name == "UI_War_Header", "Original header prefab source.");
                Audit(prefab.Asset!.Cast<GameObject>());
                auditDomain!.Register(prefab, WarResourceOwner.SessionAcquisition);
                fontBaseline = FontRegistry();
                lease = new NativeWarUiLease(borrowedMaterials, prefab.Asset!.Cast<GameObject>(), field.Asset.Cast<MACHIDAT>());
            }
            if (cleanupFrame == 0)
            {
                if (!lease.Step()) { Check(Globals() == baseline, "Original hierarchy/resources restored between factory stages."); return; }
                Check(Globals() == baseline && prefab.handle.IsValid(), "Native factory preserved unrelated owners and audit reference.");
                lease.RequestCleanup(); lease.RequestCleanup(); cleanupFrame = Time.frameCount; return;
            }
            if (Time.frameCount - cleanupFrame < 3) return;
            auditDomain!.Close();
            Check(lease.CleanupComplete(prefab), "Native component callbacks and prefab acquisitions cleaned.");
            Check(FontRegistry() == fontBaseline, "Original font callback registry restored exactly.");
            Check(sharedFont != null && sharedFontMaterial != null && SystemObject.GetLanguageFont().Pointer == sharedFont.Pointer && sharedFont.material.Pointer == sharedFontMaterial.Pointer, "Original shared font/material preserved.");
            Check(runtimeExceptions == 0, "Unity error/exception during owned UI lifecycle.");
            Check(Globals() == baseline && field.handle.IsValid(), "Original roots/static war caches and retained field preserved.");
            rows.Add(new { Cycle = ++cycles, lease.Moves, lease.CreatedTweens, lease.CreatedMaterials, NativeFontRegistryRestored = true, OriginalFactoryExecuted = true, OriginalRoot7Restored = true,
                NativeOnDestroyTweenCleanupVerified = true, lease.FactoryLoaderReleased, SharedFontMaterialPreserved = true, RuntimeErrorsBeforeQuit = runtimeExceptions, OwnedObjectsDestroyed = true, VisibleRenderingAccepted = false, TacticalBattle = false });
            prefab = null; lease = null; auditDomain = null; cleanupFrame = 0;
            if (cycles == 2) { Check(!WarBoundaryRules.CanLaunch, "Launcher remains unavailable."); Finish(true); }
        }
        catch (Exception e) { failure = e.GetType().Name + ": " + e.Message; Plugin.Logger.LogError("Native war UI ownership failed: " + failure); Finish(false); }
    }

    void Audit(GameObject source)
    {
        var allowed = new HashSet<string> { "UnityEngine.RectTransform", "UnityEngine.CanvasGroup", "UnityEngine.CanvasRenderer", "UnityEngine.UI.Image", "TMPro.TextMeshProUGUI", "DG.Tweening.DOTweenAnimation", "UnityEngine.UI.HorizontalLayoutGroup", "UnityEngine.UI.VerticalLayoutGroup", "UnityEngine.UI.ContentSizeFitter", "UnityEngine.UI.LayoutElement", "ShareUI.Component.FontColorSetter", "UIFontBehabiour", "WarHDHeader" };
        foreach (var component in source.GetComponentsInChildren<Component>(true))
            Check(component != null && allowed.Contains(component.GetIl2CppType().FullName), "Unowned prefab component: " + component?.GetIl2CppType().FullName);
        foreach (var animation in source.GetComponentsInChildren<DOTweenAnimation>(true))
        {
            Check(animation.target == null || animation.target.transform.IsChildOf(source.transform), "Borrowed animation target.");
            Check(animation.targetGO == null || animation.targetGO.transform.IsChildOf(source.transform), "Borrowed animation object.");
            Check(!animation.autoPlay && !animation.hasOnStart && !animation.hasOnPlay && !animation.hasOnUpdate && !animation.hasOnStepComplete && !animation.hasOnComplete && !animation.hasOnTweenCreated && !animation.hasOnRewind, "Unclassified animation callback/autoplay.");
            foreach (var ev in new[] { animation.onStart, animation.onPlay, animation.onUpdate, animation.onStepComplete, animation.onComplete, animation.onTweenCreated, animation.onRewind })
                Check(ev == null || ev.GetPersistentEventCount() == 0, "Unclassified persistent Unity event.");
        }
        Check(source.GetComponent<WarHDHeader>()?.bars?.Length == 3, "Audited header layout identity.");
        foreach (var fonts in source.GetComponentsInChildren<UIFontBehabiour>(true))
        {
            Check(fonts.materialList.Count == 0 && fonts._texts.Length == 3, "Source font ownership/preinitialized-material guard.");
            foreach (var text in fonts._texts) Check(text != null && text.transform.IsChildOf(source.transform), "Source callback target is within prefab.");
        }
        foreach (var text in source.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
            if (text.fontSharedMaterial != null) borrowedMaterials.Add(text.fontSharedMaterial.Pointer);
        var globalFont = SystemObject.GetLanguageFont();
        Check(globalFont != null && globalFont.material != null, "Original language font initialized before native instance creation.");
        borrowedMaterials.Add(globalFont!.material!.Pointer);
        sharedFont = globalFont; sharedFontMaterial = globalFont.material;
    }

    static string FontRegistry() => string.Join(",", UIFontBehabiour.m_actives.ToArray().Select(x => x.Pointer.ToInt64()).OrderBy(x => x));

    static string Globals()
    {
        var rows = new List<string>();
        foreach (var pair in HierarchyManager.keyObjectDictionary)
            rows.Add($"{pair.Key}:{pair.Value.Pointer}:{pair.Value.item2?.Pointer}:{(pair.Value.item2 != null ? pair.Value.item2.transform.childCount : -1)}");
        rows.Sort();
        return HierarchyManager.keyObjectDictionary.Pointer + "|" + WarRes.header?.Pointer + "|" + WarRes.uiText?.Pointer + "|" + WarRes.tips?.Pointer + "|" +
            WarRes.war_csr_data?.Pointer + "|" + WarRes.war_icon_data?.Pointer + "|" + WarRes.war_arrow_data?.Pointer + "|" + WarRes.war_face_data?.Pointer + "|" + ReleaseCheck.releaseDelegate?.Pointer + "|" + string.Join(";", rows);
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
            File.WriteAllText(Path.Combine(dir, "ui-ownership-probe.json"), JsonSerializer.Serialize(new { Version = "1.16.0-native-war-ui.1", Passed = passed, Checks = checks, Failure = failure, Cycles = rows,
                GraphicsEnabled = !Environment.GetCommandLineArgs().Contains("-nographics"), NativeHeaderFactory = true, FullWarResLoad = false, ActiveWarChapter = false, FieldReturn = false, CampaignLoaded = false }, new JsonSerializerOptions { WriteIndented = true }));
            Plugin.Logger.LogInfo($"Native war UI ownership {(passed ? "PASS" : "FAIL")}: {cycles} native header creation/cleanup cycles; no battle or field return.");
        }
        catch (Exception e) { Plugin.Logger.LogError("Native UI diagnostics unavailable: " + e.GetType().Name); }
        finally { Application.Quit(); }
    }
}
