using HarmonyLib;
using UnityEngine;
using System.Text.Json;
using System.Security.Cryptography;
using Il2CppInterop.Runtime.InteropTypes;

namespace NightmareRebalance;

// Passive inventory only. No native setters, scene requests, interpreter methods,
// time controls, coroutine termination, save suppression or session invocation.
public sealed class NativeFieldObserver : MonoBehaviour
{
    internal static bool Requested => Environment.GetCommandLineArgs().Contains("--reforged-field-observer") &&
        Environment.GetCommandLineArgs().Contains("--reforged-private-startup-smoke") &&
        File.Exists(Path.Combine(BepInEx.Paths.GameRootPath, "PRIVATE_DO_NOT_UPLOAD.txt")) &&
        !Plugin.Options.RunCombatSelfTest.Value && !Plugin.Options.RunNativeWarSelfTest.Value;
    internal static int SaveCalls, SaveFileCalls, AutosaveChecks, AutosaveEligible, CommandAdvances, GenuineSamples, Errors, MapEnds, Loads;
    static readonly Queue<object> rows = new();
    static readonly HashSet<IntPtr> commandOwners = new();
    static FieldObservation? before, previous;
    static int captureFrame = -1;
    static string comparison = "No genuine field baseline";
    float nextWrite;
    public NativeFieldObserver(IntPtr p) : base(p) { }
    static string P(Il2CppObjectBase? value) => value?.Pointer.ToInt64().ToString() ?? "null";
    static string Hash(string value) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)));
    static void Record(object row) { while (rows.Count >= 32) rows.Dequeue(); rows.Enqueue(row); }
    internal static void Begin(MapChapter map)
    {
        if (!Requested || Time.frameCount % 30 != 0) return;
        try { before = Capture(map, "BeforeMapUpdate"); captureFrame = Time.frameCount; }
        catch { Errors++; before = null; captureFrame = -1; }
    }
    internal static void End(MapChapter map)
    {
        if (!Requested || captureFrame != Time.frameCount) return;
        try { var after = Capture(map, "AfterMapUpdate"); comparison = FieldContinuationRules.Compare(before, after); previous = after; }
        catch { Errors++; previous = null; comparison = "Native observation failed; no lease permitted"; }
    }
    internal static void Invalidate(string stage)
    {
        if (!Requested) return;
        commandOwners.Clear(); before = previous = null; captureFrame = -1;
        comparison = stage; Record(new { Stage = stage, Frame = Time.frameCount });
    }
    internal static void Advance(COMMAND_LINE command)
    { if (Requested && commandOwners.Contains(command.Pointer)) CommandAdvances = Math.Min(1000000, CommandAdvances + 1); }

    static FieldObservation? Capture(MapChapter map, string stage)
    {
        var manager = GRChapterManager.GRInstance;
        var g = Framework.Singleton<GAME_WORK>.instance; var sys = g?.sys_work; var m = sys?.mcon; var e = m?.econ;
        if (manager?.activeChapter?.Pointer != map.Pointer || !NativeWarBoundary.OwnersMatch(g, m, e)) return null;
        if (e!.coml == null || e.coml.Length > 128) throw new InvalidOperationException("Unbounded/missing command owners.");
        commandOwners.Clear(); var commands = new List<string>();
        foreach (var c in e.coml)
        {
            if (c == null) { commands.Add("null"); continue; }
            commandOwners.Add(c.Pointer);
            commands.Add($"{P(c)}:{c.flg}:{c.compos}:{c.comst}:{c.pno}:{c.cno}:{c.cmdIdx}:{P(c.command)}:{c.command?.Length}:{P(c.work)}");
        }
        var windows = WindowManager.instance;
        var input = GRInputManager.IsInstantiated ? GRInputManager.Instance : null;
        var roots = new List<string>();
        if (HierarchyManager.keyObjectDictionary == null || HierarchyManager.keyObjectDictionary.Count > 256) throw new InvalidOperationException("Unknown hierarchy bounds.");
        foreach (var pair in HierarchyManager.keyObjectDictionary)
            roots.Add($"{pair.Key}:{P(pair.Value)}:{P(pair.Value.item2)}:{pair.Value.item2?.activeInHierarchy}:{pair.Value.item2?.transform.childCount}");
        roots.Sort();
        var banks = SeUtility2.arBankID;
        if (banks == null || banks.Count > 256) throw new InvalidOperationException("Unknown audio bank registry.");
        var owner = $"{P(g)}:{P(sys)}:{P(g!.party_data)}:{P(g.game_data)}:{P(m)}:{P(e)}:{P(m!.mdat)}:{P(e.mdat)}:{P(map)}";
        var commandState = $"{m.ano}:{m.vno}:{m.mno}:{m._mstep}:{m.ctyp}:{e.mstep}:{e.sstep}:{e.load}:{e.eventInitState}:{e.px}:{e.py}:{EVENTCON.noScriptRunning}:" + string.Join(";", commands);
        var chapter = $"{P(manager)}:{P(manager.activeChapter)}:{P(manager.coroutine)}:{P(manager.RequestType)}:{P(manager.Active)}:{P(manager.Old)}:{P(Framework.Chapter.SceneState)}:{GRChapterManager.GameStep}:{manager.KeyEnable}:{manager.IsPause}:{P(manager.OnChapterChangedCallback)}";
        var presentation = $"{P(input)}:{P(input?.keys)}:{P(windows)}:{P(windows?.menuWindow)}:{P(windows?.backLogPanel)}:{P(windows?.commonDialog)}:{windows?.IsUseMessageWindow}:{windows?.IsUseSelectMenu}:" + string.Join(";", roots);
        var sound = SoundManager.IsInstantiated ? SoundManager.Instance : null;
        var audio = $"{P(sound)}:{P(sound?.seAcbList)}:{sound?.seAcbList?.Count}:{P(sound?.bgmAcbList)}:{sound?.bgmAcbList?.Count}:" + P(banks) + ":" + string.Join(",", banks.ToArray());
        var flags = NativeWarWitness.Capture()?.FlagsHash ?? "unknown";
        bool idle = e.mstep == 2 && m.ctyp == 0 && e.coml.All(c => c == null || c.flg == 0) && manager.RequestType == null && sys!.now_save == 0 && !sys.g_bInBattle && Combat.State == null;
        // Playback/filter/native coroutine *contents* and pending async autosave
        // completion owners remain unverified even when their registries exist.
        var result = new FieldObservation(owner, commandState, presentation, audio, chapter, flags, SaveCalls + SaveFileCalls, CommandAdvances, true, idle, false);
        GenuineSamples = Math.Min(1000000, GenuineSamples + 1);
        Record(new { Stage = stage, Frame = Time.frameCount, Owners = owner, Commands = commandState, Chapter = chapter,
            PresentationHash = Hash(presentation), AudioBanks = banks.ToArray(), AudioRegistry = P(banks), FlagsHash = flags,
            sys!.now_save, g.auto_save_skip, m.IsNightFilterDisable, m.isOverlayFieldMenuFilter, SaveCalls, SaveFileCalls, AutosaveChecks, AutosaveEligible, CommandAdvances, Idle = idle,
            Unknown = new[] { "Full render/filter state", "Audio playback ownership", "Coroutine contents/continuation", "Pending save completion callbacks" } });
        return result;
    }

    public void Update()
    {
        if (!Requested || Time.realtimeSinceStartup < nextWrite) return;
        nextWrite = Time.realtimeSinceStartup + 2;
        try
        {
            var manager = GRChapterManager.GRInstance;
            string dir = Path.Combine(BepInEx.Paths.BepInExRootPath, "ReforgedNightmare", "native-war"); Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "field-observation.json"), JsonSerializer.Serialize(new {
                Version = "1.18.0-field-observation.1", GenuineSamples, Errors, SaveCalls, SaveFileCalls, AutosaveChecks, AutosaveEligible, CommandAdvances, MapEnds, Loads,
                ActiveChapterType = manager?.activeChapter?.GetIl2CppType().FullName, LastComparison = comparison, HasBaseline = previous != null,
                Samples = rows.ToArray(), CanRetain = FieldContinuationRules.CanRetain, FieldSuspended = false, SessionAttempted = false,
                ContinuationProven = false, LauncherAvailable = WarBoundaryRules.CanLaunch, PassiveOnly = true
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { Errors++; } // Diagnostic failure never changes original execution.
    }
}

[HarmonyPatch(typeof(MapChapter), nameof(MapChapter.Update))]
internal static class FieldObserverMap
{ [HarmonyPrefix] static void Prefix(MapChapter __instance) => NativeFieldObserver.Begin(__instance); [HarmonyPostfix] static void Postfix(MapChapter __instance) => NativeFieldObserver.End(__instance); }
[HarmonyPatch(typeof(MACHICON), "AutoSaveCheck")]
internal static class FieldObserverAutosave
{ [HarmonyPostfix] static void Postfix(bool __result) { if (NativeFieldObserver.Requested) { NativeFieldObserver.AutosaveChecks++; if (__result) NativeFieldObserver.AutosaveEligible++; } } }
[HarmonyPatch(typeof(GSD2SaveData), nameof(GSD2SaveData.SaveFile))]
internal static class FieldObserverSave
{ [HarmonyPrefix] static void Prefix() { if (NativeFieldObserver.Requested) NativeFieldObserver.SaveFileCalls++; } }
[HarmonyPatch(typeof(COMMAND_LINE), nameof(COMMAND_LINE.Advance))]
internal static class FieldObserverCommand
{ [HarmonyPrefix] static void Prefix(COMMAND_LINE __instance) => NativeFieldObserver.Advance(__instance); }
[HarmonyPatch(typeof(MapChapter), nameof(MapChapter.OnDestroy))]
internal static class FieldObserverEnd
{ [HarmonyPrefix] static void Prefix() { if (NativeFieldObserver.Requested) NativeFieldObserver.MapEnds++; NativeFieldObserver.Invalidate("Original MapChapter destruction"); } }
[HarmonyPatch(typeof(GSD2SaveData), nameof(GSD2SaveData.LoadDataCore))]
internal static class FieldObserverLoad
{ [HarmonyPrefix] static void Prefix() { if (NativeFieldObserver.Requested) NativeFieldObserver.Loads++; NativeFieldObserver.Invalidate("Original campaign load"); } }

[HarmonyPatch(typeof(UISaveLoad2), nameof(UISaveLoad2.Save))]
internal static class FieldObserverSaveIntent
{ [HarmonyPrefix] static void Prefix() { if (NativeFieldObserver.Requested) NativeFieldObserver.SaveCalls++; } }
