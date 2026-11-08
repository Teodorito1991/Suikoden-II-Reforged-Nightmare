namespace NightmareRebalance;

// Only two audited encounters. Uses current visible combat state, not queued
// player commands, character identities or inferred healer roles. No RNG/stats.
internal static class BossCombatQuality
{
    internal static bool Supports(string table) => table is "boss_rat" or "boss_hap1";
    internal static bool GroupReady(IReadOnlyList<CombatTarget> targets) => targets.Count >= 3 && targets.All(t => t.MaxHp > 0 && t.Hp > 0 && t.Hp <= t.MaxHp) &&
        targets.Count(t => (long)t.Hp * 100 <= (long)t.MaxHp * 35) <= 1 &&
        targets.Average(t => 100.0 * t.Hp / Math.Max(1, t.MaxHp)) >= 60;

    internal static int Score(BossPattern pattern, int counter, int hp, int max, CombatAction action,
        IReadOnlyList<CombatTarget> targets, int score, int previous)
    {
        if (score == int.MinValue || !action.Legal || !Supports(pattern.TableId) || targets.Count == 0) return score;
        string window = BossTacticsRules.Window(pattern, counter, hp, max);
        bool ready = GroupReady(targets);
        // Prefer a legal focused attack over a broad opening or near-wipe. This
        // changes selection only: if no alternative is native-legal it still runs.
        if (action.Multi && (counter == 0 || !ready || window == "Recovery")) score -= 400;
        if (window == "Recovery" && action.Mode == pattern.RecoveryMode) score += 120;
        if (window == "Adaptive" && ready && action.Mode == pattern.PressureMode) score += 65;
        // A repeated status move is less useful when its native status mask is
        // already present. Do not invent immunity or remove native legality.
        if (action.Status != 0 && targets.Count(t => t.StatusUseful) * 2 < targets.Count) score -= 100;
        // Additional selector evaluations cannot advance this quality cadence.
        return score;
    }

    internal static int ChooseTarget(CombatAction action, IReadOnlyList<CombatTarget> targets, int original, int focusLimit)
    {
        if (action.Multi || targets.Count == 0) return original;
        int best = original, bestScore = int.MinValue;
        foreach (var target in targets.OrderBy(t => t.Slot == original ? 0 : 1))
        {
            if (target.Hp <= 0 || target.MaxHp <= 0 || target.Hp > target.MaxHp) continue;
            int defense = action.Physical ? target.Defense : target.MagicDefense;
            int score = 100 - Math.Clamp(defense, 0, 300) / 3;
            if (action.Status != 0 && !target.StatusUseful) score -= 100;
            // Soft spread is shared with other enemy decisions in this phase.
            score -= Math.Max(0, target.Focus - Math.Max(1, focusLimit) + 1) * 60;
            if ((long)target.Hp * 100 <= (long)target.MaxHp * 35 && targets.Count > 1) score -= 25;
            if (score > bestScore) { bestScore = score; best = target.Slot; }
        }
        return best;
    }
}
