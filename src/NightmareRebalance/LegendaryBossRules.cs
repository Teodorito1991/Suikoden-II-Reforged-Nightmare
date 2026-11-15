namespace NightmareRebalance;

internal sealed class LegendaryBossOptions
{
    public bool EnableMistShade { get; init; } = true;
    public bool EnableGorudo { get; init; } = true;
    public bool EnableWormMoth { get; init; } = true;
    public bool EnableLaterLucia { get; init; } = true;

    internal bool Enabled(string table) => table switch
    {
        "boss_ki1" => EnableMistShade,
        "boss_gor" => EnableGorudo,
        "boss_imo2" => EnableWormMoth,
        "boss_lus3" or "boss_lus4" or "boss_lus5" => EnableLaterLucia,
        _ => false
    };
}

internal static class LegendaryBossRules
{
    internal static bool Discretionary(int command) => command == 0;

    internal static int ActorIndex(string table) => table == "boss_gor" ? 2 : 1;

    internal static bool AcceptsPhase(string table, int index) =>
        index == ActorIndex(table) || index == 2 && table is "boss_ki1" or "boss_imo2";

    internal static int Preferred(string table, int phase, int round, bool freshAction,
        IReadOnlyList<CombatTarget> targets, int targetsWithoutStatus)
    {
        if (targets.Count == 0) return -1;
        bool pressure = round >= 1 && round % 3 == 1 && freshAction &&
            BossCombatQuality.GroupReady(targets);
        bool braced = targets.Sum(t => t.Defense - t.MagicDefense) >= 20 * targets.Count;
        return table switch
        {
            "boss_ki1" when phase == 2 => pressure && targets.Count >= 3 ? 1 : 0,
            "boss_gor" => pressure && targets.Count >= 4 && braced ? 2 :
                round != 0 && round % 3 != 2 && targets.Count >= 3 && braced &&
                BossCombatQuality.GroupReady(targets) ? 1 : 0,
            "boss_imo2" when phase == 1 => pressure && targets.Count >= 3 && braced ? 1 : 0,
            "boss_imo2" when phase == 2 => pressure && targets.Count >= 4 && braced ? 2 :
                pressure && targetsWithoutStatus >= 3 ? 1 : 0,
            "boss_lus3" or "boss_lus4" or "boss_lus5" =>
                pressure && targets.Count >= 3 && braced ? 2 : 0,
            "boss_luc3" => pressure && targets.Count >= 3 ? 2 : 0,
            _ => -1
        };
    }
}
