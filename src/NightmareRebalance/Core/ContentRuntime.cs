using BepInEx;
using HarmonyLib;

namespace NightmareRebalance;

internal static class ContentRuntime
{
    static ContentBaseline baseline = null!;
    static Dictionary<int, CharacterProfile> characters = new();
    static Dictionary<int, SpellProfile> spells = new();
    static readonly HashSet<string> warned = new();
    internal static double SleepMultiplier { get; private set; } = 1;
    internal static string Resource(string name)
    {
        var assembly = typeof(ContentRuntime).Assembly;
        using var stream = assembly.GetManifestResourceStream("ReforgedContent." + name) ?? throw new FileNotFoundException(name);
        using var reader = new StreamReader(stream); return reader.ReadToEnd();
    }
    static string Load(string name)
    {
        string path = Path.Combine(Paths.ConfigPath, "ReforgedNightmare", name);
        if (!File.Exists(path)) { Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllText(path, Resource(name)); }
        if (new FileInfo(path).Length > 262144) throw new InvalidDataException("Content file exceeds 256 KiB.");
        return File.ReadAllText(path);
    }
    internal static void Initialize()
    {
        baseline = ContentRules.Read<ContentBaseline>(Resource("baseline.json"));
        if (Plugin.Options.EnableCharacterBalancing.Value)
            try { characters = ContentRules.Characters(Load("characters.json"), baseline).Characters.ToDictionary(c => c.Id, RpgEvolutionRuntime.Character); }
            catch (Exception ex) { Plugin.Logger.LogError($"Character content disabled: {ex.Message}"); }
        if (Plugin.Options.EnableRuneBalancing.Value)
            try
            {
                var magic = ContentRules.Magic(Load("magic.json"), baseline);
                spells = magic.Spells.ToDictionary(c => c.Id); SleepMultiplier = magic.WindOfSleepSusceptibilityMultiplier;
            }
            catch (Exception ex) { Plugin.Logger.LogError($"Magic content disabled: {ex.Message}"); }
        Plugin.Logger.LogInfo($"Reforged content loaded: {characters.Count} character profiles, {spells.Count} spell effects; Wind of Sleep susceptibility x{SleepMultiplier}.");
    }
    static void Warn(string key)
    { CombatRealityRuntime.Event("ContentGuard","RejectedUnexpectedSignature",key); if (warned.Add(key)) Plugin.Logger.LogWarning($"Reforged left incompatible data unchanged: {key}. Check overlapping mods."); }
    internal static void Character(int id, C_KOTEI_DAT? data)
    {
        if (data == null || !characters.TryGetValue(id, out var c)) return;
        var b = baseline.Characters.Single(b => b.Id == id);
        var growth = data.seicyo_type; var affinity = data.mon_zoku_aisyo; var slots = data.mon_kosu_type;
        if (growth == null || affinity == null || slots == null || data.monsyo_seigen == null ||
            !ContentRules.CharacterCompatible(new CharacterBaseline { Growth = growth.ToArray(), Affinity = affinity.ToArray(), Slots = slots.ToArray(),
                Restrictions = data.monsyo_seigen.ToArray(), Fixed = data.soubi_henkou, Equipment = data.soubi_kanou, Panic = data.okori_panic }, b, c))
        { Warn($"character {id}"); return; }
        for (int i = 0; i < 4; i++) { if (growth[i] != c.Growth[i]) growth[i] = c.Growth[i]; if (affinity[i] != c.Affinity[i]) affinity[i] = c.Affinity[i]; }
        for (int i = 0; i < 3; i++) if (slots[i] != c.Slots[i]) slots[i] = c.Slots[i];
        CombatRealityRuntime.Event("CharacterProfile","MatchedAuditedValues",id.ToString());
    }
    internal static void Spell(int id, MAGI_DATA? data)
    {
        if (data == null || !spells.TryGetValue(id, out var c)) return;
        var b = baseline.Spells.Single(b => b.Id == id);
        short effective=(short)RpgEvolutionRuntime.Magic(id,c.Effect,CombatReforgedRuntime.Spell(id,c.Effect));
        var accepted=new SpellProfile {Id=c.Id,Name=c.Name,Effect=effective};
        if (data.info == null || data.status == null || !ContentRules.SpellCompatible(new SpellBaseline { Name = data.name, Text = data.text,
            Attribute = data.atrb, Unite = data.uni_no, UniteMagic = data.uni_magic, Info = data.info.ToArray(), Status = data.status.ToArray(), Effect = data.eff_val }, b, accepted))
        { Warn($"spell {id}"); return; }
        data.eff_val = effective;
        CombatRealityRuntime.Event("SpellEffects","MatchedAuditedValues",id.ToString(),new{Effect=effective});
    }
}

[HarmonyPatch(typeof(CHARA_DATA), nameof(CHARA_DATA.UnityEngine_ISerializationCallbackReceiver_OnAfterDeserialize))]
internal static class CharacterDeserializePatch
{
    [HarmonyPostfix] static void Postfix(CHARA_DATA __instance)
    {
        if (__instance.c_kotei_dat == null) return;
        for (int i = 0; i < __instance.c_kotei_dat.Count; i++) ContentRuntime.Character(i, __instance.c_kotei_dat[i]);
    }
}
[HarmonyPatch(typeof(G2_SYS), nameof(G2_SYS.G2_kotei_dat))]
internal static class CharacterLookupPatch
{
    [HarmonyPostfix] static void Postfix(int chano, C_KOTEI_DAT __result) => ContentRuntime.Character(chano, __result);
}
[HarmonyPatch(typeof(G2_LEVEL), nameof(G2_LEVEL.level_up))]
[HarmonyPatch(typeof(G2_SYS), nameof(G2_SYS.G2_level_up))]
internal static class CharacterGrowthPatch
{
    [HarmonyPrefix] static void Prefix(int chano)
    { if (Plugin.Options.EnableCharacterBalancing.Value && chano is >= 0 and < 85) ContentRuntime.Character(chano, G2_SYS.G2_kotei_dat(chano)); }
}
[HarmonyPatch(typeof(G2_SYS), nameof(G2_SYS.G2_magi_data))]
internal static class SpellLookupPatch
{
    [HarmonyPostfix] static void Postfix(int magi_no, MAGI_DATA __result) { ContentRuntime.Spell(magi_no, __result); CombatMasteryRuntime.Spell(magi_no,__result); CombatRealityRuntime.Spell(magi_no,__result); RpgEvolutionRuntime.Spell(magi_no,__result); }
}
[HarmonyPatch(typeof(GAME_WORK), nameof(GAME_WORK.g2_MagiEffVal))]
internal static class SpellMenuPatch
{
    [HarmonyPrefix] static void Prefix(int __0)
    { if (Plugin.Options.EnableRuneBalancing.Value && __0 is >= 0 and < 106) ContentRuntime.Spell(__0, G2_SYS.G2_magi_data(__0)); }
}
[HarmonyPatch(typeof(BattlePlayerCharacter), nameof(BattlePlayerCharacter.NormalMagicSleepFunc))]
internal static class WindSleepPatch
{
    [HarmonyPrefix] static void Prefix(BattlePlayerCharacter __instance, byte magi_no, out bool __state)
    {
        // No state leak into an unrelated nested sleep invocation.
        __state = SleepScope.Enter(magi_no == 11 && __instance.IsPlayer && ContentRuntime.SleepMultiplier > 1 &&
            Combat.Matches(BattleManager.Instance?.battle_work));
    }
    [HarmonyFinalizer] static Exception? Finalizer(bool __state, Exception? __exception)
    { SleepScope.Exit(__state); return __exception; }
}
[HarmonyPatch(typeof(MONSTER_DATA), nameof(MONSTER_DATA.MonsterTaiseiCheck))]
internal static class SleepSusceptibilityPatch
{
    [HarmonyPostfix] static void Postfix(int taisei, ref int __result)
    {
        __result = SleepScope.Apply(taisei, __result, CombatReforgedRuntime.Config is {Enabled:true,EnableSleepBalance:true} cfg?cfg.SleepSusceptibilityMultiplier:ContentRuntime.SleepMultiplier);
    }
}
