using System.Text.Json;

namespace NightmareRebalance;

public sealed record ModifierSample(string Hook, int Slot, int Before, int After, double Multiplier);
public sealed record EnemyObservation(int Slot, int CharacterId, int NameId, string DataId, int Hp, int MaxHp);
public sealed record BossProfileObservation(int Slot, string EnemyId, BossResolution Resolution);

public sealed class CoverageReport
{
    public int SchemaVersion { get; init; } = 2;
    public string PluginVersion { get; init; } = "0.2.0-stage-b.1";
    public string ExpectedGameBuildId { get; init; } = "20278991";
    public bool RuntimeFingerprintsVerified { get; init; }
    public string SessionId { get; init; } = "";
    public long BattleNumber { get; init; }
    public DateTimeOffset Utc { get; set; } = DateTimeOffset.UtcNow;
    public string Event { get; set; } = "initialized";
    public EncounterIdentity Identity { get; init; } = null!;
    public CoverageDecision Decision { get; init; } = null!;
    public string OutcomeSignal { get; set; } = "not-observed";
    public bool RewardCompletionObserved { get; set; }
    public int EnemyInspectionFailures { get; set; }
    public int DroppedRosterEntries { get; set; }
    public Dictionary<string, double> ConfiguredMultipliers { get; } = new(StringComparer.Ordinal);
    public string BossProfileConfigStatus { get; set; } = "disabled";
    public List<BossProfileObservation> BossProfiles { get; } = new();
    public int DroppedBossProfiles { get; set; }
    public List<EnemyObservation> Roster { get; } = new();
    public Dictionary<string, long> HookCalls { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, long> ModifierApplications { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, long> ValueChanges { get; } = new(StringComparer.Ordinal);
    public List<ModifierSample> Samples { get; } = new();
    public long DroppedSamples { get; private set; }
    public void Hook(string name) => HookCalls[name] = HookCalls.GetValueOrDefault(name) + 1;
    public void Applied(string hook, int slot, int before, int after, double multiplier)
    {
        ModifierApplications[hook] = ModifierApplications.GetValueOrDefault(hook) + 1;
        if (before != after) ValueChanges[hook] = ValueChanges.GetValueOrDefault(hook) + 1;
        if (Samples.Count < 128) Samples.Add(new(hook, slot, before, after, multiplier)); else DroppedSamples++;
    }
}

/// <summary>Opt-in JSONL output. Diagnostic failures never escape into a game callback.</summary>
public sealed class CoverageJournal
{
    public const long MaxBytes = 20 * 1024 * 1024;
    private readonly string path;
    private readonly Action<string> warning;
    private bool disabled;
    private readonly object sync = new();
    public CoverageJournal(string path, Action<string> warning) { this.path = path; this.warning = warning; }
    public void Write(CoverageReport report)
    {
        lock (sync)
        {
            if (disabled) return;
            try
            {
                var line = JsonSerializer.Serialize(report) + Environment.NewLine;
                long size = System.Text.Encoding.UTF8.GetByteCount(line);
                if (size + (File.Exists(path) ? new FileInfo(path).Length : 0) > MaxBytes)
                    throw new IOException("Session report reached the 20 MiB limit.");
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
                File.AppendAllText(path, line);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or NotSupportedException or ArgumentException)
            {
                disabled = true;
                warning($"Boss diagnostics stopped; gameplay continues: {ex.Message}");
            }
        }
    }
}
