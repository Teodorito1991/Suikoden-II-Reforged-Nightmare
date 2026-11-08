using NightmareRebalance;
using System.Text.Json;

internal static class BossCombatQualityTests
{
    internal static void Run()
    {
        int n = 0;
        void Check(bool ok, string why) { n++; if (!ok) throw new Exception("Boss quality: " + why); }
        var p = new BossPattern { TableId = "boss_rat", PressureMode = 2, RecoveryMode = 0, AdaptiveMode = 1 };
        var healthy = Enumerable.Range(0, 6).Select(i => new CombatTarget(i, 400, 500, 80, 80, 80, 80, 0)).ToArray();
        var hurt = healthy.Select((x, i) => i < 3 ? x with { Hp = 100 } : x).ToArray();
        var broad = new CombatAction(2, 2, 125, false, true, 0, 20, 255, true);
        Check(BossCombatQuality.GroupReady(healthy) && !BossCombatQuality.GroupReady(hurt), "Current HP distribution allows recovery without a queued-heal read.");
        Check(!BossCombatQuality.GroupReady(healthy.Select(x => x with { MaxHp = 0 }).ToArray()), "Unknown health state cannot authorize pressure.");
        Check(BossCombatQuality.Score(p, 0, 100, 100, broad, healthy, 100, -1) < 100, "No broad opening preference.");
        Check(BossCombatQuality.Score(p, 3, 40, 100, broad, hurt, 100, -1) < 100, "Low party HP reduces broad attacks even at low boss HP.");
        Check(BossCombatQuality.Score(p, 1, 40, 100, broad with { Mode = 0, Multi = false }, healthy, 100, -1) > 100, "Basic recovery window strengthened.");
        Check(BossCombatQuality.Score(p, 2, 100, 100, broad, healthy, 100, 2) == BossCombatQuality.Score(p, 2, 100, 100, broad, healthy, 100, -1), "Quality cadence does not advance from selector evaluation history.");
        Check(BossCombatQuality.Score(p, 2, 100, 100, broad, healthy, int.MinValue, -1) == int.MinValue, "Native-illegal actions remain unavailable.");
        Check(!BossCombatQuality.Supports("boss_nec1") && !BossCombatQuality.Supports("boss_lus1") && !BossCombatQuality.Supports("boss_nec3"), "Major/scripted phases excluded from new quality scope.");
        var choices = new[] { healthy[0] with { Hp = 80 }, healthy[1] with { Defense = 20 }, healthy[2] with { Defense = 30, Focus = 3 } };
        Check(BossCombatQuality.ChooseTarget(broad with { Physical = true, Multi = false }, choices, 0, 2) == 1, "Attack exposed defence without automatic wounded execution or repeated focus.");
        Check(BossCombatQuality.ChooseTarget(broad, choices, 0, 2) == 0, "Multi-target routines retain native targeting.");
        using var stream = typeof(BossCombatQualityTests).Assembly.GetManifestResourceStream("ReforgedContent.boss-tactics.json")!;
        using var reader = new StreamReader(stream); string old = reader.ReadToEnd();
        using var auditStream = typeof(BossCombatQualityTests).Assembly.GetManifestResourceStream("ReforgedContent.boss-tactics-audit.json")!;
        using var auditReader = new StreamReader(auditStream);var audit = ContentRules.Read<FormationAudit[]>(auditReader.ReadToEnd());
        var legacy=System.Text.Json.Nodes.JsonNode.Parse(old)!.AsObject();legacy.Remove("EnableCombatQuality");
        Check(BossTacticsRules.Parse(legacy.ToJsonString(),audit).EnableCombatQuality,"Existing config without new optional key remains valid.");
        var config = BossTacticsRules.Parse(old, audit);config.EnableCombatQuality = false;
        Check(!BossTacticsRules.Parse(JsonSerializer.Serialize(config), audit).EnableCombatQuality, "Independent quality opt-out parses without changing other boss settings.");
        Console.WriteLine($"PASS: Boss combat quality {n} assertions; recovery pressure, native legality, soft targeting and opt-out. No full fight simulation or gameplay claim.");
    }
}
