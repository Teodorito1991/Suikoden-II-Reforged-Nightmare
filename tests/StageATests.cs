using System.Text.Json;
using NightmareRebalance;

internal static class StageATests
{
    internal static void Run()
    {
        int assertions = 0;
        void Check(bool ok, string why) { assertions++; if (!ok) throw new Exception("Stage A: " + why); }
        var none = new HashSet<string>(StringComparer.Ordinal);
        var selected = EncounterCoverage.ParseSelection("""{"schemaVersion":1,"encounters":["boss_rat/boss_rat_00"]}""");
        var rat = new EncounterIdentity("boss_rat", "boss_rat_00", 0, 268435456, 6, 1, false, true,
            new[] {1467,0,0,1468,0,0}, new[] {"rat_normal_mae_func00","(null)","(null)","rat_boss_end_func00","(null)","(null)"},
            new[] {"boss_rat_00"}, new[] {5200}, new[] {1,0,0,0,0,0}, new[] {0,1,0,0,0,0}, 0);
        Check(EncounterCoverage.IsVerifiedRat(rat), "Verified asset fixture matches registry");
        Check(!EncounterCoverage.Decide(rat, true, false, selected).StatScaling, "Flag defaults closed even if selected");
        Check(!EncounterCoverage.Decide(rat, true, true, none).StatScaling, "Selection required in addition to flag");
        var admit = EncounterCoverage.Decide(rat, true, true, selected);
        Check(admit.StatScaling && !admit.FullEffects, "Script opt-in enables stats only, not rewards/regeneration");
        Check(!EncounterCoverage.Decide(rat, false, true, selected).StatScaling, "Conflict cannot be overridden");
        foreach (bool party in new[] {false,true}) foreach (bool main in new[] {false,true})
        foreach (bool end in new[] {false,true}) foreach (bool compatible in new[] {false,true})
        foreach (int players in new[] {-1,0,1,6}) foreach (int enemies in new[] {-1,0,1,6})
        {
            var id = rat with {HasParty=party, HasMainHandler=main, HasEndHandler=end, Players=players, Enemies=enemies};
            bool legacy = compatible && party && !main && !end && players>0 && enemies>0;
            var result = EncounterCoverage.Decide(id, compatible, false, selected);
            Check(result.FullEffects==legacy && result.StatScaling==legacy, "Default scope equals original v0.1 truth table");
        }
        var badShapes = new[] {
            rat with {TableId="other"}, rat with {PartyId="boss_rat_01"}, rat with {PartyIndex=1},
            rat with {PartyFlags=0}, rat with {Enemies=2}, rat with {HasMainHandler=true},
            rat with {HookIndices=new[]{1467,0,1,1468,0,0}}, rat with {HookIndices=Array.Empty<int>()},
            rat with {HookNames=new[]{"unknown"}}, rat with {EnemyIds=new[]{"same-name-different-id"}},
            rat with {EnemyIds=new[]{"boss_rat_00","boss_rat_00"}}, rat with {EnemyMaxHp=new[]{6000}},
            rat with {MonsterIds=new[]{0,1}}, rat with {MonsterCodes=new[]{1,0,0,0,0,0}},
            rat with {ScriptCommandCount=1}, rat with {HasParty=false}, rat with {Players=0}
        };
        foreach (var id in badShapes) Check(!EncounterCoverage.Decide(id,true,true,selected).StatScaling,"Unknown identity/changed script or roster is excluded");
        Check(EncounterCoverage.Decide(rat with {TableId="unknown",PartyId="",HasEndHandler=false},true,false,none).FullEffects,
            "Ordinary unknown/empty string IDs preserve original nonnull-party behavior");
        foreach (string json in new[] {"{}", "[]", "null", "{", """{"schemaVersion":2,"encounters":[]}""",
            """{"schemaVersion":1,"encounters":["*"]}""", """{"schemaVersion":1,"encounters":[null]}""",
            """{"schemaVersion":1,"encounters":["boss_rat/boss_rat_00","boss_rat/boss_rat_00"]}""",
            """{"schemaVersion":1,"encounters":[],"extra":true}""", """{"schemaVersion":1,"schemaVersion":1,"encounters":[]}""",
            """{"schemaVersion":"1","encounters":[]}""", """{"schemaVersion":1,"encounters":true}""", new string(' ',65537)})
        {
            bool rejected=false;
            try { EncounterCoverage.ParseSelection(json); }
            catch (Exception ex) when (ex is FormatException or JsonException or InvalidOperationException or OverflowException) { rejected=true; }
            Check(rejected,"Invalid configuration rejected as a whole");
        }
        Check(EncounterCoverage.ParseSelection("""{"schemaVersion":1,"encounters":[]}""").Count==0,"Empty selection is valid conservative mode");
        // Repeated battles/reloads receive independent lifecycle gates; support/party switching never adds IDs to the registry.
        for (int battle=0;battle<100;battle++)
        {
            var gate=new VictoryGate();
            Check(!gate.TryConsume(),"Reload/new battle starts unarmed");
            gate.MarkVictory();Check(gate.TryConsume()&&!gate.TryConsume(),"Repeated completion applies once per battle");
        }
        string dir=Path.GetFullPath(Path.Combine("work","stage-a-tests-"+Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(dir);
        var warnings=new List<string>();
        var journal=new CoverageJournal(Path.Combine(dir,"coverage.jsonl"),warnings.Add);
        var report=new CoverageReport{SessionId="fixture-not-gameplay",BattleNumber=1,Identity=rat,Decision=admit};
        report.Hook("BattleMainInit");report.Applied("EnemyMaxHp",6,5200,8320,1.6);
        journal.Write(report);
        report.Event="reward-pipeline-completed";report.Hook("WinEnd");report.RewardCompletionObserved=true;journal.Write(report);
        report.Event="closed-or-replaced";journal.Write(report);
        var lines=File.ReadAllLines(Path.Combine(dir,"coverage.jsonl"));
        Check(lines.Length==3 && warnings.Count==0,"JSONL events flushed separately");
        using(var first=JsonDocument.Parse(lines[0]))
        {
            Check(first.RootElement.GetProperty("Event").GetString()=="initialized","Previous report snapshot not mutated later");
            Check(first.RootElement.GetProperty("ModifierApplications").GetProperty("EnemyMaxHp").GetInt32()==1,"Actual modifier applications recorded");
            Check(!first.RootElement.GetProperty("HookCalls").TryGetProperty("WinEnd",out _),"Compile/fixture does not imply a native hook ran");
        }
        for(int i=0;i<300;i++) report.Applied("EnemyDamage",6,100,145,1.45);
        Check(report.Samples.Count==128 && report.DroppedSamples==173,"Diagnostic sample buffer bounded");
        Check(report.ModifierApplications["EnemyDamage"]==300,"Aggregate counts survive sample truncation");
        report.Applied("Neutral",0,100,100,1);Check(!report.ValueChanges.ContainsKey("Neutral"),"Applied vs actually changed distinguished");
        string capped=Path.Combine(dir,"capped.jsonl");
        using(var stream=File.Create(capped))stream.SetLength(CoverageJournal.MaxBytes);
        var cap=new CoverageJournal(capped,warnings.Add);cap.Write(report);cap.Write(report);
        Check(warnings.Count==1 && new FileInfo(capped).Length==CoverageJournal.MaxBytes,"Disk cap disables logging once without growing file");
        string blocked=Path.Combine(dir,"not-a-directory");File.WriteAllText(blocked,"fixture");
        var broken=new CoverageJournal(Path.Combine(blocked,"file.jsonl"),warnings.Add);broken.Write(report);broken.Write(report);
        Check(warnings.Count==2,"I/O failure does not throw into gameplay and warns once");
        // Files are test evidence in work/, not a game report or save operation.
        Console.WriteLine($"PASS Stage A: {assertions} assertions; scope matrix, exact allowlist, malformed config, lifecycle isolation and bounded JSONL failure handling.");
    }
}
