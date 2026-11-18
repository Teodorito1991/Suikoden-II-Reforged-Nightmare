using System.Reflection;
using System.Security.Cryptography;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace NightmareRebalance;

[BepInPlugin(Id, "Suikoden II Reforged Nightmare", "1.43.0")]
[BepInDependency("d3xMachina.suikoden_fix", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("faospark.pkcore", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("nightmare.suikoden2.rebalance", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("s2_character_mod", BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BasePlugin
{
    public const string Id = "nightmare.suikoden2.reforged";
    internal static Settings Options = null!;
    internal static ManualLogSource Logger = null!;
    private Harmony? harmony;

    public override void Load()
    {
        Logger = Log;
        Options = new Settings(Config);
        // Native control-flow evidence is valid only for this binary pair.
        if (!SupportedBuild()) { Log.LogError("Unsupported game binary/metadata. No Nightmare patches installed. See docs/RESEARCH.md."); return; }
        harmony = new Harmony(Id);
        try
        {
            HallFirstContact.EnabledAtStartup = Options.EnableHallOfEchoes.Value && !UnityEngine.Application.isBatchMode &&
                CheckPatchTargets(PatchTargets(DeclaredPatchTypes.Where(IsHallPatch)), hallOnly: true);
            if (!CheckCompatibility()) return;
            RpgEvolutionRuntime.Initialize();
            ContentRuntime.Initialize();
            OverhaulRuntime.Initialize();
            NativeWarRemix.Initialize();
            War2Runtime.Initialize();
            TotalWarRuntime.Initialize();
            WarTacticsRuntime.Initialize();
            WarResearch.Export();
            BossRuntime.Initialize();
            CoverageRuntime.Initialize();
            CombatReforgedRuntime.Initialize();
            CombatEvolutionRuntime.Initialize();
            CombatMasteryRuntime.Initialize();
            CombatRealityRuntime.Initialize();
            MaintenanceRuntime.Initialize();
            HuntRuntime.Initialize();
            HuntGuildRuntime.Initialize();
            CastleGuildRuntime.Initialize();
            BossTacticsRuntime.Initialize();
            LegendaryBossRuntime.Initialize();
            NativeWarWitness.Initialize();
            if(NativeFieldObserver.Requested)AddComponent<NativeFieldObserver>();
            if(HuntGuildRuntime.RenderSmoke||(HuntGuildRuntime.Config.EnableSelector&&Options.EnableDeveloperTools.Value)||CastleGuildRuntime.Config.Enabled)AddComponent<HuntGuildOverlay>();
            if(HuntRuntime.Config.EnableExperimentalHunt||HuntGuildRuntime.Config.EnableSelector) AddComponent<HuntAccessControl>();
            HuntDebug.Initialize(this);
            foreach (var type in ActivePatchTypes()) harmony.CreateClassProcessor(type).Patch();
            WarBoundaryRules.CanLaunch = TrainingWar.Enabled;
            if (WarBoundaryRules.CanLaunch) Log.LogInfo("Disposable native war training enabled.");
            if(UnityEngine.Application.isBatchMode&&Environment.GetCommandLineArgs().Contains("--reforged-hunt-debug-selftest")){HuntDebugProbe.Run();AddComponent<HuntDebugExit>();}
            WarRuntime.NativeCopyProbe();
            NativeWarRemix.SelfTest();
            War2Probe.Run();
            TotalWarProbe.Run();
            WarTacticsProbe.Run();
            NativeWarSafetyProbe.Run();
            NativeWarFoundationProbe.Run();
            if(NativeSkirmishProbe.Requested){AddComponent<NativeSkirmishProbe>();NativeSkirmishProbe.Scheduled=true;}
            if(NativeWarUiProbe.Requested){AddComponent<NativeWarUiProbe>();NativeWarUiProbe.Scheduled=true;}
            if(NativeWarLifecycleProbe.Requested){AddComponent<NativeWarLifecycleProbe>();NativeWarLifecycleProbe.Scheduled=true;}
            if(NativeWarBridgeProbe.Requested){AddComponent<NativeWarBridgeProbe>();NativeWarBridgeProbe.Scheduled=true;}
            CombatRealityRuntime.Suppress=true;
            try {
            CombatMasteryRuntime.Suppress=true;
            try {
            CombatEvolutionRuntime.Suppress=true;
            try { CombatReforgedProbe.Run(); } finally { CombatEvolutionRuntime.Suppress=false; }
            CombatEvolutionProbe.Run();
            } finally { CombatMasteryRuntime.Suppress=false; }
            CombatMasteryProbe.Run();
            } finally { CombatRealityRuntime.Suppress=false; }
            CombatRealityProbe.Run();
            if(ProductionAssetAudit.Requested){AddComponent<ProductionAssetAudit>();ProductionAssetAudit.Scheduled=true;}
            if (Options.EnableDeveloperTools.Value) AddComponent<DeveloperOverlay>();
            if (HallFirstContact.EnabledAtStartup) AddComponent<HallFirstContactDriver>();
            Log.LogInfo($"Reforged Nightmare 1.43.0-second-challenger.1 loaded. Hall of Echoes enabled={HallFirstContact.EnabledAtStartup}.");
        }
        catch (Exception ex)
        {
            harmony.UnpatchSelf();
            Log.LogError($"Nightmare installation rolled back: {ex}");
        }
        finally
        {
            // Only our explicitly requested, noninteractive startup process may
            // exit automatically. A normal player session never meets this gate.
            if (UnityEngine.Application.isBatchMode && Environment.GetCommandLineArgs().Contains("--reforged-test-exit") &&
                Options.RunCombatSelfTest.Value && !NativeWarBridgeProbe.Scheduled && !NativeWarLifecycleProbe.Scheduled && !NativeWarUiProbe.Scheduled && !NativeSkirmishProbe.Scheduled && (!ProductionAssetAudit.Requested||!ProductionAssetAudit.Scheduled) && Combat.State==null && !WarRuntime.Active && !WarTacticsRuntime.Armed)
            {
                Log.LogInfo("Headless probe requested normal application exit; no campaign loaded.");
                UnityEngine.Application.Quit();
            }
        }
    }

    private static bool SupportedBuild()
    {
        bool Match(string path, string hash) => File.Exists(path) && Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) == hash;
        try
        {
            return Match(Path.Combine(Paths.GameRootPath, "GameAssembly.dll"), "E093F175EAF3734D293CA0A236C652DD7FCC72EA5FA10D7B21103EDF12F11C3A") &&
                Match(Path.Combine(Paths.GameRootPath, "Suikoden I and II HD Remaster_Data", "il2cpp_data", "Metadata", "global-metadata.dat"), "ABB6489CA1EF1728F9DE74F64A64D9C109F5DDD5BA9046226EA964B15B62F964");
        }
        catch (Exception ex) { Logger.LogError(ex.Message); return false; }
    }

    static readonly Type[] DeclaredPatchTypes = typeof(Plugin).Assembly.GetTypes()
        .Where(t => t.IsDefined(typeof(HarmonyPatch), false)).ToArray();

    static bool IsHallPatch(Type type) => type.IsDefined(typeof(HallTrialPatchAttribute), false);

    static IEnumerable<Type> ActivePatchTypes() =>
        DeclaredPatchTypes.Where(t => HallFirstContact.EnabledAtStartup || !IsHallPatch(t));

    static IEnumerable<MethodBase?> PatchTargets(IEnumerable<Type> types) =>
        types.SelectMany(t => t.GetCustomAttributes<HarmonyPatch>().Select(p => p.info))
            .Where(p => p.declaringType != null && p.methodName != null)
            .Select(p => AccessTools.Method(p.declaringType, p.methodName));

    // Also protect nearby template/recovery hooks used by the reference mod.
    internal static bool CheckCompatibility() => CheckPatchTargets(
        PatchTargets(ActivePatchTypes()).Append(AccessTools.Method(typeof(BattleManager), nameof(BattleManager.BattleEnd))),
        hallOnly: false);

    static bool CheckPatchTargets(IEnumerable<MethodBase?> targets, bool hallOnly)
    {
        foreach (MethodBase? target in targets)
        {
            if (target == null)
            {
                if (!hallOnly) throw new MissingMethodException("A required verified patch target is unavailable.");
                Logger.LogWarning("Hall of Echoes disabled: a verified trial patch target is unavailable.");
                return false;
            }
            var owners = Harmony.GetPatchInfo(target)?.Owners.Where(o => o != Id).ToArray();
            if (owners is not { Length: > 0 }) continue;
            var message = $"overlapping Harmony patch on {target.DeclaringType?.Name}.{target.Name}: {string.Join(", ", owners)}.";
            if (hallOnly) Logger.LogWarning($"Hall of Echoes disabled: {message}");
            else Logger.LogError($"Nightmare disabled: {message} Disable overlapping gameplay features and restart.");
            return false;
        }
        return true;
    }

    internal static void Debug(string message) { if (Options.EnableDebugLogging.Value) Logger.LogInfo(message); }
    public override bool Unload() { harmony?.UnpatchSelf(); Combat.Clear(); WarRuntime.Clear(); NativeWarRemix.Clear(); War2Runtime.Clear(); TotalWarRuntime.Clear(); WarTacticsRuntime.Clear(); CombatReforgedRuntime.Clear(); return true; }
}

