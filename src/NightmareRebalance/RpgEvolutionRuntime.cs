using System.Text.Json;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace NightmareRebalance;

internal sealed class RpgEvolutionOptions
{
    public int SchemaVersion { get; set; }
    public bool Characters { get; set; }
    public bool Magic { get; set; }
    public bool PhysicalRunes { get; set; }
    public bool Unites { get; set; }
    public bool Formations { get; set; }
}

internal sealed class RpgEvolutionContent
{
    public CharacterProfile[] Characters { get; set; } = [];
    public SpellProfile[] Spells { get; set; } = [];
    public PhysicalRuneAudit[] Runes { get; set; } = [];
    public UniteAudit[] Unites { get; set; } = [];
    public FormationAudit[] Formations { get; set; } = [];
}

internal static class RpgEvolutionRuntime
{
    internal static RpgEvolutionContent Audit = new();
    internal static RpgEvolutionOptions Options = new();
    static CharacterFile originalCharacters = new();
    static MagicFile originalMagic = new();
    static EvolutionSwitch[] formationSwitches = [];

    internal static void Initialize()
    {
        try
        {
            var baseline = ContentRules.Read<ContentBaseline>(ContentRuntime.Resource("baseline.json"));
            originalCharacters = ContentRules.Characters(ContentRuntime.Resource("characters.json"), baseline);
            originalMagic = ContentRules.Magic(ContentRuntime.Resource("magic.json"), baseline);
            var audit = ContentRules.Read<RpgEvolutionContent>(ContentRuntime.Resource("rpg-evolution-audit.json"));
            ContentRules.Characters(JsonSerializer.Serialize(new CharacterFile
                { SchemaVersion = 1, Characters = audit.Characters }, ContentRules.JsonOptions), baseline);
            ContentRules.Magic(JsonSerializer.Serialize(new MagicFile
                { SchemaVersion = 1, Spells = audit.Spells }, ContentRules.JsonOptions), baseline);
            string json = OverhaulRuntime.Load("rpg-evolution.json");
            using var document = JsonDocument.Parse(json);
            OverhaulRules.Keys(document.RootElement, "SchemaVersion", "Characters", "Magic", "PhysicalRunes", "Unites", "Formations");
            var options = ContentRules.Read<RpgEvolutionOptions>(json);
            if (options.SchemaVersion != 1) throw new JsonException("Unsupported RPG Evolution schema.");
            formationSwitches = audit.Formations.Select(a => new EvolutionSwitch { PartyId = a.PartyId }).ToArray();
            Audit = audit;
            Options = options;
            Plugin.Logger.LogInfo($"RPG Evolution: {audit.Characters.Length} revised profiles, {audit.Spells.Length} revised spells, " +
                $"{audit.Runes.Length} physical techniques, {audit.Unites.Length} additional Unites, {audit.Formations.Length} formations.");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning("RPG Evolution disabled: " + e.Message);
        }
    }

    internal static CharacterProfile Character(CharacterProfile current)
    {
        if (!Options.Characters || Array.Find(Audit.Characters, c => c.Id == current.Id) is not {} next)
            return current;
        var prior = originalCharacters.Characters.Single(c => c.Id == current.Id);
        // A customized row keeps its full prior build. Missing rows are never reinserted.
        return current.Growth.SequenceEqual(prior.Growth) && current.Affinity.SequenceEqual(prior.Affinity) &&
            current.Slots.SequenceEqual(prior.Slots) ? next : current;
    }

    internal static int Magic(int id, int configured, int effective)
    {
        if (!Options.Magic || configured != effective || Array.Find(Audit.Spells, s => s.Id == id) is not {} next)
            return effective;
        var prior = originalMagic.Spells.Single(s => s.Id == id);
        return configured == prior.Effect ? next.Effect : effective;
    }

    internal static bool Spell(int id, MAGI_DATA? data)
    {
        if (!Options.PhysicalRunes || !Plugin.Options.EnableRuneBalancing.Value ||
            data?.info == null || data.status == null || Array.Find(Audit.Runes, r => r.Id == id) is not {} rune)
            return false;
        var signature = CombatMasteryRuntime.RuneSignature(data);
        if (!signature.SequenceEqual(rune.Signature)) return false;
        data.eff_val = (short)rune.Effect;
        return true;
    }

    internal static int ApplyUnites(BATTLE_WORK work)
    {
        if (!Options.Unites || work.unite_data_table is not {} original) return 0;
        var changes = Audit.Unites.Where(a => a.Id < original.Length && original[a.Id]?.player_id != null &&
            CombatEvolutionRuntime.UniteSignature(original[a.Id]).SequenceEqual(a.Signature)).ToArray();
        if (changes.Length == 0) return 0;
        var replacement = new Il2CppReferenceArray<UNITE_DATA>(original.ToArray());
        foreach (var change in changes)
            replacement[change.Id] = CombatEvolutionRuntime.CopyUnite(original[change.Id], change.Effect);
        work.unite_data_table = replacement;
        return changes.Length;
    }

    internal static bool ApplyFormation(BATTLE_WORK work, bool compatible) =>
        Options.Formations && CombatEvolutionRuntime.ApplyAuditedFormation(work, compatible, Audit.Formations, formationSwitches);
}
