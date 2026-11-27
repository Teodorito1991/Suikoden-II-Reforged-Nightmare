using BepInEx;

namespace NightmareRebalance;

internal static class BossRuntime
{
    internal static BossProfileSet Profiles = BossProfileSet.Empty;
    internal static string ConfigStatus = "disabled";
    internal static void Initialize()
    {
        Profiles = BossProfileSet.Empty;
        ConfigStatus = "disabled";
        if (!Plugin.Options.EnableBossProfiles.Value) return;
        try
        {
            string configured = Plugin.Options.BossProfilePath.Value;
            if (string.IsNullOrWhiteSpace(configured)) throw new FormatException("BossProfilePath is empty.");
            string path = Path.GetFullPath(Path.IsPathFullyQualified(configured) ? configured : Path.Combine(Paths.ConfigPath, configured));
            if (!File.Exists(path) || new FileInfo(path).Length > BossProfileSet.MaxFileBytes) throw new FormatException("Boss profile file is missing or larger than 256 KiB.");
            Profiles = BossProfileSet.Parse(File.ReadAllText(path));
            ConfigStatus = "loaded";
            Plugin.Logger.LogInfo($"Boss profiles: {Profiles.Profiles.Count} validated entries loaded from {path}; final vanilla-relative factors, restart reload only.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or System.Text.Json.JsonException or ArgumentException or NotSupportedException)
        {
            ConfigStatus = "invalid-configuration-fallback";
            Plugin.Logger.LogWarning($"No boss profiles applied from configuration: {ex.Message} FallbackToGlobalNightmare={Plugin.Options.FallbackToGlobalNightmare.Value}. Stage A scope retained.");
        }
    }

    internal static void Register(BattleState state, PM_DATA unit, bool reinforcement = false)
    {
        if (unit.IsPlayer || unit.TryCast<BattleMonsterCharacter>() == null || state.BossPlans.ContainsKey(unit.Pointer)) return;
        // Profile selection is frozen before any HP mutation. Uninspected spawns never inherit a boss profile.
        string enemyId = unit.monster_data?.id ?? "";
        var resolution = Resolve(state, enemyId, reinforcement);
        // Finish native reads before committing a plan, so inspection failure cannot leave a partial selection.
        var observation = state.Report == null ? null : new BossProfileObservation(unit.chara_no, enemyId, resolution);
        state.BossPlans.Add(unit.Pointer, resolution);
        if (state.Report is { } report)
        {
            report.BossProfileConfigStatus = ConfigStatus;
            if (report.BossProfiles.Count < 128) report.BossProfiles.Add(observation!);
            else report.DroppedBossProfiles++;
        }
    }
    private static BossResolution Resolve(BattleState state, string enemy, bool reinforcement)
    {
        if (state.Identity == null) return new(Plugin.Options.EnemyHpMultiplier.Value, Plugin.Options.EnemyDamageMultiplier.Value,
            null, null, "identity-unavailable-stage-a-global-fallback", false, state.StatsAllowed, "global-nightmare", "global-nightmare");
        return Profiles.Resolve(state.Identity, enemy, new(state.Allowed, state.StatsAllowed, "stage-a-coverage"),
            Plugin.Options.EnableBossProfiles.Value, Plugin.Options.FallbackToGlobalNightmare.Value,
            Plugin.Options.EnemyHpMultiplier.Value, Plugin.Options.EnemyDamageMultiplier.Value, reinforcement);
    }
    internal static BossResolution? Plan(BattleState state, PM_DATA unit) => state.BossPlans.GetValueOrDefault(unit.Pointer);
}
