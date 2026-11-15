namespace NightmareRebalance;

internal static class BossRevolutionRules
{
    internal static bool Supports(string table) => table is
        "boss_sei" or "boss_imo1" or "boss_nik" or "boss_gol" or "boss_dor" or "boss_kul_sid";

    internal static int Score(BossPattern pattern, int counter, int hp, int maxHp,
        CombatAction action, IReadOnlyList<CombatTarget> targets, int score)
    {
        if (score == int.MinValue || !action.Legal || targets.Count == 0) return score;

        bool ready = BossCombatQuality.GroupReady(targets);
        string window = BossTacticsRules.Window(pattern, counter, hp, maxHp);
        if (action.Multi && (!ready || counter == 0 || window == "Recovery"))
            score -= 135;

        int defenseGap = targets.Sum(target => Math.Clamp(target.Defense, 0, 300) -
            Math.Clamp(target.MagicDefense, 0, 300)) / targets.Count;
        if (ready && counter > 0 && window != "Recovery" && action.Multi)
            score += Math.Clamp(action.Physical ? -defenseGap : defenseGap, -60, 60);

        // Keep each swing within the source attack list. Expended or illegal
        // techniques never reach this scorer, and native ties keep their choice.
        return score + (pattern.TableId switch
        {
            "boss_sei" when action.Mode == 0 && !ready => 90,
            "boss_imo1" when action.Mode == 1 && ready && window == "Pressure" => 85,
            "boss_nik" when action.Mode == 2 && ready && window == "Adaptive" && defenseGap >= 25 => 100,
            "boss_gol" when action.Mode == 2 && (window == "Recovery" || !ready) => 160,
            "boss_dor" when action.Mode == 2 && ready && window == "Pressure" => 95,
            "boss_kul_sid" when action.Mode == 1 => -135,
            "boss_kul_sid" when action.Mode == 3 && ready && window == "Pressure" => 80,
            _ => 0
        });
    }
}
