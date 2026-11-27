using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NightmareRebalance;

public sealed record BossProfile(string ProfileId, bool Enabled, int Priority, string TableId,
    string EncounterId, string EnemyId, double? HpMultiplier, double? DamageMultiplier);

public sealed record BossResolution(double HpMultiplier, double DamageMultiplier, string? ProfileId,
    int? Priority, string Reason, bool KnownBoss, bool ScalingAllowed, string HpSource, string DamageSource);

/// <summary>Asset-verified selectors, not an extension of Stage A's scripted allowlist.</summary>
public static class BossRegistry
{
    private sealed record Entry(string Table, string Party, int Hp, int Start, int End, string StartName, string EndName);
    private static readonly Entry[] Entries = {
        new("boss_rat", "boss_rat_00", 5200, 1467, 1468, "rat_normal_mae_func00", "rat_boss_end_func00"),
        new("boss_hap1", "boss_hap1_00", 7400, 1295, 1296, "hap1_normal_mae_func00", "hap1_boss_end_func00"),
        new("boss_sie1", "boss_sie1_00", 2800, 1500, 1501, "sie1_special_mae_func00", "sie1_boss_end_func00")
    };
    public static bool IsKnownSelector(string table, string party, string enemy) =>
        Entries.Any(e => e.Table == table && e.Party == party && e.Party == enemy);

    public static bool Matches(EncounterIdentity id, string enemy)
    {
        var entry = Entries.FirstOrDefault(e => e.Table == id.TableId && e.Party == id.PartyId && e.Party == enemy);
        return entry != null && id.HasParty && id.PartyIndex == 0 && id.Players > 0 && id.Enemies == 1 &&
            id.PartyFlags == 268435456 && !id.HasMainHandler && id.HasEndHandler && id.ScriptCommandCount == 0 &&
            id.HookIndices.SequenceEqual(new[] {entry.Start, 0, 0, entry.End, 0, 0}) &&
            id.HookNames.SequenceEqual(new[] {entry.StartName, "(null)", "(null)", entry.EndName, "(null)", "(null)"}) &&
            id.EnemyIds.SequenceEqual(new[] {entry.Party}) && id.EnemyMaxHp.SequenceEqual(new[] {entry.Hp}) &&
            id.MonsterIds.SequenceEqual(new[] {1,0,0,0,0,0}) && id.MonsterCodes.SequenceEqual(new[] {0,1,0,0,0,0});
    }
}

public sealed class BossProfileSet
{
    public const int MaxFileBytes = 256 * 1024;
    public static BossProfileSet Empty { get; } = new(Array.Empty<BossProfile>());
    public IReadOnlyList<BossProfile> Profiles { get; }
    private BossProfileSet(BossProfile[] profiles) => Profiles = new ReadOnlyCollection<BossProfile>(profiles);

    public static BossProfileSet Parse(string json)
    {
        if (System.Text.Encoding.UTF8.GetByteCount(json) > MaxFileBytes) throw new FormatException("Boss profile file exceeds 256 KiB.");
        using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 8 });
        var root = Properties(doc.RootElement, new[] {"schemaVersion", "profiles"}, "root");
        Require(root, "schemaVersion", "profiles");
        if (Integer(root["schemaVersion"], "schemaVersion", 1, 1) != 1) throw new FormatException("Unsupported schemaVersion.");
        var list = root["profiles"];
        if (list.ValueKind != JsonValueKind.Array || list.GetArrayLength() > 64) throw new FormatException("profiles must be an array of at most 64 entries.");
        var parsed = new List<BossProfile>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var activePriorities = new HashSet<(string, string, string, int)>();
        int index = 0;
        foreach (var item in list.EnumerateArray())
        {
            var p = Properties(item, new[] {"profileId", "enabled", "priority", "tableId", "encounterId", "enemyId", "hpMultiplier", "damageMultiplier"}, $"profiles[{index++}]");
            Require(p, "profileId", "enabled", "priority", "tableId", "encounterId", "enemyId");
            string profileId = Text(p["profileId"], "profileId");
            if (!Regex.IsMatch(profileId, @"\A[A-Za-z0-9_.-]{1,64}\z") || !ids.Add(profileId)) throw new FormatException($"Invalid or duplicate profileId '{profileId}'.");
            if (p["enabled"].ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw new FormatException($"{profileId}.enabled must be boolean.");
            bool enabled = p["enabled"].GetBoolean();
            int priority = Integer(p["priority"], profileId + ".priority", -1000, 1000);
            string table = Text(p["tableId"], "tableId"), party = Text(p["encounterId"], "encounterId"), enemy = Text(p["enemyId"], "enemyId");
            if (!BossRegistry.IsKnownSelector(table, party, enemy)) throw new FormatException($"{profileId}: unverified exact table/encounter/enemy selector.");
            double? hp = Multiplier(p, "hpMultiplier", profileId), damage = Multiplier(p, "damageMultiplier", profileId);
            if (hp == null && damage == null) throw new FormatException($"{profileId}: at least one HP/damage multiplier is required.");
            if (enabled && !activePriorities.Add((table, party, enemy, priority))) throw new FormatException($"{profileId}: overlapping enabled profiles have equal priority.");
            parsed.Add(new(profileId, enabled, priority, table, party, enemy, hp, damage));
        }
        return new(parsed.OrderByDescending(p => p.Priority).ThenBy(p => p.ProfileId, StringComparer.Ordinal).ToArray());
    }

    public BossResolution Resolve(EncounterIdentity identity, string enemy, CoverageDecision coverage,
        bool enabled, bool fallbackToGlobal, double globalHp, double globalDamage, bool reinforcement = false)
    {
        bool known = BossRegistry.Matches(identity, enemy);
        if (!coverage.StatScaling) return new(1, 1, null, null, "stage-a-scope-excluded", known, false, "excluded", "excluded");
        if (!enabled) return new(globalHp, globalDamage, null, null, "boss-profiles-disabled", known, true, "global-nightmare", "global-nightmare");
        if (!known) return new(globalHp, globalDamage, null, null, "unverified-boss-shape-global-nightmare", false, true, "global-nightmare", "global-nightmare");
        if (reinforcement) return new(globalHp, globalDamage, null, null, "reinforcement-profile-unsupported-global-nightmare", true, true, "global-nightmare", "global-nightmare");
        double hp = fallbackToGlobal ? globalHp : 1, damage = fallbackToGlobal ? globalDamage : 1;
        string fallbackSource = fallbackToGlobal ? "global-nightmare" : "vanilla-fallback";
        var match = Profiles.FirstOrDefault(p => p.Enabled && p.TableId == identity.TableId && p.EncounterId == identity.PartyId && p.EnemyId == enemy);
        return match == null ? new(hp, damage, null, null, fallbackToGlobal ? "no-active-profile-global-nightmare" : "no-active-profile-vanilla", true, true, fallbackSource, fallbackSource)
            : new(match.HpMultiplier ?? hp, match.DamageMultiplier ?? damage, match.ProfileId, match.Priority, "profile-final-relative-to-vanilla", true, true,
                match.HpMultiplier.HasValue ? "boss-profile" : fallbackSource, match.DamageMultiplier.HasValue ? "boss-profile" : fallbackSource);
    }

    private static Dictionary<string, JsonElement> Properties(JsonElement item, string[] allowed, string context)
    {
        if (item.ValueKind != JsonValueKind.Object) throw new FormatException($"{context} must be an object.");
        var result = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var p in item.EnumerateObject())
        {
            if (!allowed.Contains(p.Name, StringComparer.Ordinal)) throw new FormatException($"{context}: unsupported property '{p.Name}'. Resistance settings are not supported in Stage B.");
            if (!result.TryAdd(p.Name, p.Value)) throw new FormatException($"{context}: duplicate property '{p.Name}'.");
        }
        return result;
    }
    private static void Require(Dictionary<string, JsonElement> p, params string[] names)
    {
        foreach (var name in names) if (!p.ContainsKey(name)) throw new FormatException($"Missing required property '{name}'.");
    }
    private static string Text(JsonElement value, string name) => value.ValueKind == JsonValueKind.String && value.GetString() is { Length: > 0 and <= 128 } text
        ? text : throw new FormatException($"{name} must be a nonempty string of at most 128 characters.");
    private static int Integer(JsonElement value, string name, int min, int max) => value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int n) && n >= min && n <= max
        ? n : throw new FormatException($"{name} must be an integer in [{min},{max}].");
    private static double? Multiplier(Dictionary<string, JsonElement> p, string name, string id)
    {
        if (!p.TryGetValue(name, out var value)) return null;
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDouble(out double n) || !double.IsFinite(n) || n < 0.1 || n > 5)
            throw new FormatException($"{id}.{name} must be finite in [0.1,5]. Omit it to use fallback.");
        return n;
    }
}

/// <summary>Shared production guards, independently testable without invoking IL2CPP.</summary>
public sealed class EnemyInstanceLedger
{
    private readonly HashSet<long> seen = new();
    public bool Add(long instance) => instance != 0 && seen.Add(instance);
}

public static class DamageScaling
{
    [ThreadStatic] private static int depth;
    public static bool Enter() => depth++ == 0;
    public static Exception? Exit(Exception? exception) { depth = Math.Max(0, depth - 1); return exception; }
    public static bool Apply(bool outer, bool eligible, ref int value, double multiplier)
    {
        if (!outer || !eligible || value <= 0) return false;
        value = Rules.Scale(value, multiplier, 9999);
        return true;
    }
}
