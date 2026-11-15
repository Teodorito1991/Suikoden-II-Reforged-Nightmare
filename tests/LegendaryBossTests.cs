using NightmareRebalance;

internal static class LegendaryBossTests
{
    internal static void Run(Action<bool, string> check)
    {
        var defaults = new LegendaryBossOptions();
        check(defaults.Enabled("boss_gor") && !defaults.Enabled("boss_luc3") &&
            !defaults.Enabled("boss_las") && !defaults.Enabled("boss_imo3"), "Scripted policy allowlist");
        check(!new LegendaryBossOptions { EnableLaterLucia = false }.Enabled("boss_lus5"),
            "Later Lucia opt-out");
        check(LegendaryBossRules.Discretionary(0) && !LegendaryBossRules.Discretionary(11) &&
            !LegendaryBossRules.Discretionary(7), "Only known entry command allowed");
        check(LegendaryBossRules.AcceptsPhase("boss_ki1", 2) &&
            LegendaryBossRules.AcceptsPhase("boss_imo2", 2) &&
            !LegendaryBossRules.AcceptsPhase("boss_gor", 1), "Transform templates and Gorudo escort exclusion");
        var healthy = Enumerable.Range(0, 4).Select(i => new CombatTarget(i, 500, 500, 100, 40, 0, 0, 0)).ToArray();
        var wounded = healthy.Select(t => t with { Hp = 120 }).ToArray();
        var warded = healthy.Select(t => t with { Defense = 40, MagicDefense = 100 }).ToArray();
        check(LegendaryBossRules.Preferred("boss_gor", 2, 1, true, healthy, 4) == 2 &&
            LegendaryBossRules.Preferred("boss_gor", 2, 2, true, healthy, 4) == 0, "Gorudo pressure and recovery");
        check(LegendaryBossRules.Preferred("boss_ki1", 1, 1, true, healthy, 4) == -1 &&
            LegendaryBossRules.Preferred("boss_ki1", 2, 1, true, healthy, 4) == 1 &&
            LegendaryBossRules.Preferred("boss_ki1", 2, 1, false, healthy, 4) == 0 &&
            LegendaryBossRules.Preferred("boss_ki1", 2, 1, true, wounded, 4) == 0, "Mist Shade phase and pressure boundaries");
        check(LegendaryBossRules.Preferred("boss_imo2", 2, 1, true, warded, 3) == 1 &&
            LegendaryBossRules.Preferred("boss_imo2", 2, 1, true, warded, 2) == 0, "Moth avoids redundant status");
        check(LegendaryBossRules.Preferred("boss_lus4", 1, 1, true, healthy, 4) == 2 &&
            LegendaryBossRules.Preferred("boss_lus4", 1, 1, true, warded, 4) == 0, "Lucia spends spell on physical defense");
    }
}
