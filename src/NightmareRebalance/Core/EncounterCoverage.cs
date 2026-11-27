using System.Text.Json;

namespace NightmareRebalance;

// Managed evidence snapshots. Native objects and offsets never enter the policy layer.
public sealed record EncounterIdentity(string TableId, string PartyId, int PartyIndex,
    uint PartyFlags, int Players, int Enemies, bool HasMainHandler, bool HasEndHandler,
    int[] HookIndices, string[] HookNames, string[] EnemyIds, int[] EnemyMaxHp,
    int[] MonsterIds, int[] MonsterCodes, int ScriptCommandCount, bool HasParty = true);

public sealed record CoverageDecision(bool FullEffects, bool StatScaling, string Reason);

public static class EncounterCoverage
{
    public const string RatEncounter = "boss_rat/boss_rat_00";
    public static IReadOnlyList<string> VerifiedIds { get; } = Array.AsReadOnly(new[] { RatEncounter });

    public static CoverageDecision Decide(EncounterIdentity id, bool compatible, bool enableOverrides, IReadOnlySet<string> selected)
    {
        if (!compatible) return new(false, false, "conflicting-harmony-owner");
        if (!id.HasParty || id.Players <= 0 || id.Enemies <= 0)
            return new(false, false, "missing-party-or-empty-roster");
        // Exactly the v0.1 inclusion rule; no naming convention is treated as a boss detector.
        if (!id.HasMainHandler && !id.HasEndHandler) return new(true, true, "v0.1-standard-pipeline");
        if (!enableOverrides) return new(false, false, "custom-handler-overrides-disabled");
        string key = id.TableId + "/" + id.PartyId;
        if (!selected.Contains(key)) return new(false, false, "custom-handler-not-selected");
        if (!IsVerifiedRat(id)) return new(false, false, "custom-handler-unverified-identity-or-shape");
        // Stage A admits HP/damage only. XP, Potch and regeneration keep v0.1 exclusion.
        return new(false, true, "verified-rat-stat-only-opt-in");
    }

    public static bool IsVerifiedRat(EncounterIdentity id) =>
        id.HasParty && id.TableId == "boss_rat" && id.PartyId == "boss_rat_00" && id.PartyIndex == 0 &&
        id.PartyFlags == 268435456 && id.Players > 0 && id.Enemies == 1 &&
        !id.HasMainHandler && id.HasEndHandler && id.ScriptCommandCount == 0 &&
        id.HookIndices.SequenceEqual(new[] {1467, 0, 0, 1468, 0, 0}) &&
        id.HookNames.SequenceEqual(new[] {"rat_normal_mae_func00", "(null)", "(null)", "rat_boss_end_func00", "(null)", "(null)"}) &&
        id.EnemyIds.SequenceEqual(new[] {"boss_rat_00"}) && id.EnemyMaxHp.SequenceEqual(new[] {5200}) &&
        id.MonsterIds.SequenceEqual(new[] {1,0,0,0,0,0}) && id.MonsterCodes.SequenceEqual(new[] {0,1,0,0,0,0});

    public static HashSet<string> ParseSelection(string json)
    {
        if (json.Length > 65536) throw new FormatException("Allowlist is larger than 64 KiB.");
        using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 8 });
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object) throw new FormatException("Allowlist root must be an object.");
        var properties = root.EnumerateObject().Select(p => p.Name).ToArray();
        if (properties.Length != 2 || properties.Distinct().Count() != 2 ||
            !properties.Contains("schemaVersion") || !properties.Contains("encounters") ||
            root.GetProperty("schemaVersion").GetInt32() != 1)
            throw new FormatException("Expected schemaVersion 1 and encounters only.");
        var list = root.GetProperty("encounters");
        if (list.ValueKind != JsonValueKind.Array) throw new FormatException("encounters must be an array.");
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in list.EnumerateArray())
        {
            string? key = item.GetString();
            if (key == null || !VerifiedIds.Contains(key, StringComparer.Ordinal) || !result.Add(key))
                throw new FormatException("Unknown or duplicate encounter; no selection will be applied.");
        }
        return result;
    }
}
