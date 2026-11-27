using BepInEx;

namespace NightmareRebalance;

internal static class CoverageRuntime
{
    private static readonly string Session = Guid.NewGuid().ToString("N");
    private static long battleNumber;
    private static CoverageJournal? journal;
    private static bool warnedInspection;
    internal static HashSet<string> Selected = new(StringComparer.Ordinal);

    internal static void Initialize()
    {
        Selected.Clear();
        journal = null;
        battleNumber = 0;
        warnedInspection = false;
        if (Plugin.Options.EnableScriptedBossOverrides.Value)
        {
            var path = Path.Combine(Paths.ConfigPath, "NightmareRebalance", "scripted-battle-allowlist.json");
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length > 65536) throw new FormatException("Missing allowlist or file larger than 64 KiB.");
                Selected = EncounterCoverage.ParseSelection(File.ReadAllText(path));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or System.Text.Json.JsonException or InvalidOperationException or OverflowException)
            {
                Selected.Clear();
                Plugin.Logger.LogWarning($"Scripted boss overrides disabled for this session: {ex.Message}");
            }
        }
        if (!Plugin.Options.EnableBossDiagnostics.Value && !Plugin.Options.EnableBossProfileDiagnostics.Value) return;
        string file = Path.Combine(Paths.BepInExRootPath, "NightmareRebalance", "diagnostics", $"coverage-{Session}.jsonl");
        journal = new CoverageJournal(file, message => Plugin.Logger.LogWarning(message));
        Plugin.Logger.LogInfo($"Boss diagnostics enabled: {file}");
    }

    internal static EncounterIdentity Capture(BATTLE_WORK w)
    {
        var party = w.monster_party_data;
        var sub = party?.data;
        int[] indices = sub == null ? Array.Empty<int>() : new[] {sub.battle_start_func_index, sub.phase_start_func_index,
            sub.phase_end_func_index, sub.battle_end_func_index, sub.command_func_index, sub.battle_main_func_index};
        string Name(int index)
        {
            var names = area_func.g_area_func_name;
            return names != null && index >= 0 && index < names.Count ? names[index] ?? "" : "<unresolved>";
        }
        var enemies = w.pm_data == null ? Array.Empty<PM_DATA>() : w.pm_data.Where(p => p != null && !p.IsPlayer && p.TryCast<BattleMonsterCharacter>() != null).ToArray();
        return new(w.battle_data?.id ?? "", party?.id ?? "", w.monster_party_index-1, party?.party_flg ?? 0,
            w.player_kazu, w.monster_kazu, w.monster_bm_func != null, w.monster_be_func != null,
            indices, indices.Select(Name).ToArray(), enemies.Select(p => p.monster_data?.id ?? "").ToArray(),
            enemies.Select(p => (int)p.max_hp).ToArray(), party?.monster_id?.Select(v => (int)v).ToArray() ?? Array.Empty<int>(),
            party?.monster_code?.Select(v => (int)v).ToArray() ?? Array.Empty<int>(), sub?.be_data?.command_num ?? -1, party != null);
    }

    internal static CoverageReport? Start(EncounterIdentity identity, CoverageDecision decision, BATTLE_WORK w)
    {
        if (journal == null) return null;
        var report = new CoverageReport { SessionId = Session, BattleNumber = ++battleNumber, Identity = identity, Decision = decision, RuntimeFingerprintsVerified = true };
        report.ConfiguredMultipliers["EnemyHp"] = Plugin.Options.EnemyHpMultiplier.Value;
        report.ConfiguredMultipliers["EnemyDamage"] = Plugin.Options.EnemyDamageMultiplier.Value;
        report.ConfiguredMultipliers["Experience"] = Plugin.Options.ExperienceMultiplier.Value;
        report.ConfiguredMultipliers["Potch"] = Plugin.Options.PotchMultiplier.Value;
        report.Hook(nameof(BattleManager.BattleMainInit));
        if (w.pm_data != null) foreach (var p in w.pm_data) ObserveEnemy(report, p);
        return report;
    }
    internal static void ObserveEnemy(CoverageReport? report, PM_DATA? p)
    {
        if (report == null || p == null) return;
        try
        {
            if (p.IsPlayer) return;
            if (report.Roster.Count < 128) report.Roster.Add(new(p.chara_no, p.chara_id, p.monster_data?.name_id ?? -1, p.monster_data?.id ?? "", p.hp, p.max_hp));
            else report.DroppedRosterEntries++;
        }
        catch (Exception ex)
        {
            report.EnemyInspectionFailures++;
            WarnInspection($"Enemy diagnostic inspection failed; combat continues: {ex.Message}");
        }
    }
    internal static void WarnInspection(string message)
    {
        if (warnedInspection) return;
        warnedInspection = true;
        Plugin.Logger.LogWarning(message);
    }
    internal static void Emit(CoverageReport? report, string phase)
    {
        if (report == null) return;
        report.Event = phase;
        report.Utc = DateTimeOffset.UtcNow;
        journal?.Write(report);
    }
}
