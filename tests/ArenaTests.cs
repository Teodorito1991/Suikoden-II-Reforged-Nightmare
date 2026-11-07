using System.Text.Json;
using System.Text.Json.Nodes;
using NightmareRebalance;

internal static class ArenaTests
{
    internal static void Run()
    {
        int checks=0;
        void Check(bool ok,string why){checks++;if(!ok)throw new Exception("Arena: "+why);}
        string Load(){using var stream=typeof(ArenaTests).Assembly.GetManifestResourceStream("ReforgedContent.last-bridge.json")!;using var reader=new StreamReader(stream);return reader.ReadToEnd();}
        var json=Load();var scenario=ArenaConfig.Parse(json);
        ArenaBattle Fresh()=>new(scenario);
        var b=Fresh();Check(b.Units.Count==8&&b.Units.Count(u=>u.Side=="Player")==3,"Smaller independent defending army");
        Check(b.Phase==ArenaPhase.Player&&b.Round==1&&b.HeldRounds==0,"Scenario initialization");
        string before=JsonSerializer.Serialize(scenario);
        Check(!ReferenceEquals(scenario,b.Scenario)&&!ReferenceEquals(scenario.Types[0],b.Scenario.Types[0]),"Full mod-owned setup clone");
        Check(!b.Move("p-guard",new(6,3)),"River cannot be crossed off bridge");
        Check(!b.Move("p-guard",new(2,4)),"Occupied unit cannot be entered");
        Check(b.Move("p-guard",new(6,4)),"Legal two-tile bridge movement");
        Check(!b.Move("p-guard",new(5,4)),"No second movement action");
        Check(!b.Move("e-raider-a",new(8,4)),"Player cannot order enemies");
        Check(!b.Attack("p-guard","p-command"),"No friendly fire");
        Check(!b.Attack("p-guard","e-command"),"Out-of-range attack rejected");
        Check(JsonSerializer.Serialize(scenario)==before,"Actions do not mutate reusable configuration");
        b=Fresh();var archer=b.Unit("p-archer")!;var enemy=b.Unit("e-raider-a")!;enemy.Position=new(5,2);
        Check(b.CanAttack(archer,enemy),"Distinct ranged unit can shoot at distance two");
        Check(b.Attack(archer.Id,enemy.Id)&&enemy.Hp==2,"Configured attack/defense damage");
        Check(!b.Attack(archer.Id,enemy.Id),"Attack ends action; no repeated hits");
        Check(!b.Move(archer.Id,new(4,2)),"Attack prevents movement afterward");
        b=Fresh();archer=b.Unit("p-archer")!;enemy=b.Unit("e-raider-a")!;enemy.Position=new(4,2);
        Check(!b.CanAttack(archer,enemy),"Archer minimum range matters");
        enemy.Position=new(1,2);Check(!b.CanAttack(archer,enemy),"Intermediate forest blocks arrows");
        b=Fresh();var guard=b.Unit("p-guard")!;enemy=b.Unit("e-raider-a")!;
        enemy.Position=new(3,3);int open=b.Damage(guard,enemy);enemy.Position=new(3,2);Check(b.Damage(guard,enemy)==Math.Max(1,open-1),"Forest provides configured defense");
        Check(b.Brace("p-guard")&&guard.Braced&&guard.Acted,"Brace consumes action and adds defense");
        Check(b.Damage(enemy,guard)==2,"Braced guard reduces the shipped raider strike to two damage");
        b=Fresh();var commander=b.Unit("p-command")!;guard=b.Unit("p-guard")!;guard.Position=new(3,4);guard.Hp=3;commander.Hp=4;
        Check(b.Rally(commander.Id)&&commander.Hp==6&&guard.Hp==5&&commander.RallyLeft==1,"Rally heals self/adjacent allies and consumes charge");
        Check(!b.Rally(commander.Id),"Rally cannot repeat in same action");
        b=Fresh();Check(!b.Rally("p-command")&&b.Unit("p-command")!.RallyLeft==2,"No full-health charge waste");
        Check(b.EndPlayerTurn()&&!b.EndPlayerTurn(),"Exactly one player-to-enemy transition");
        int iterations=0;while(b.Phase==ArenaPhase.Enemy&&iterations++<30)b.StepEnemy();
        Check(iterations<30&&b.Round==2&&b.HeldRounds==1,"Enemy army completes one round without deadlock");
        Check(b.EnemyActions==5,"One command per living enemy");
        Check(b.Units.Where(u=>u.Side=="Player").All(u=>!u.Moved&&!u.Acted&&!u.Braced),"Player actions reset at round boundary");
        b=Fresh();b.Unit("e-command")!.Hp=0;b.CheckOutcome();Check(b.Phase==ArenaPhase.Victory,"Defeating attacking commander is a win");
        Check(!b.Move("p-guard",new(5,4))&&!b.EndPlayerTurn(),"Completed victory cannot accept new orders");
        b=Fresh();b.Unit("p-command")!.Hp=0;b.CheckOutcome();Check(b.Phase==ArenaPhase.Defeat,"Real player commander loss is defeat");
        b=Fresh();b.Unit("p-command")!.Position=new(1,4);b.Unit("e-raider-a")!.Position=new(2,4);b.CheckOutcome();Check(b.Phase==ArenaPhase.Defeat,"Crossing capture causes defeat");
        b=Fresh();foreach(var u in b.Units.Where(u=>u.Side=="Player"))u.Hp=0;b.CheckOutcome();Check(b.Phase==ArenaPhase.Defeat,"All defenders routed is defeat");
        var old=b;var reset=Fresh();Check(reset.Phase==ArenaPhase.Player&&reset.Round==1&&reset.Units.All(u=>u.Hp==u.Type.Health&&u.RallyLeft==u.Type.RallyUses),"Replay resets complete army/turn/charges");
        Check(!ReferenceEquals(old.Units[0],reset.Units[0])&&old.Phase==ArenaPhase.Defeat,"Replay cannot alias prior battle");
        Check(!typeof(ArenaBattle).Assembly.GetReferencedAssemblies().Any(a=>a.Name!.StartsWith("UnityEngine")||a.Name=="GSD2"||a.Name!.StartsWith("BepInEx")),"Arena core compiles without campaign/native dependencies");
        void Reject(JsonNode node,string why){bool failed=false;try{ArenaConfig.Parse(node.ToJsonString());}catch(JsonException){failed=true;}Check(failed,why);}
        foreach(string key in new[]{"HoldTurns","MinimumDamage","ForestMoveCost","EnemyStepMilliseconds"}){var n=JsonNode.Parse(json)!;n["Rules"]![key]=-1;Reject(n,"Rule bounds rejected: "+key);}
        {var n=JsonNode.Parse(json)!;n["Units"]![1]!["X"]=2;n["Units"]![1]!["Y"]=4;Reject(n,"Overlapping spawns rejected");}
        {var n=JsonNode.Parse(json)!;n["Units"]![1]!["X"]=6;n["Units"]![1]!["Y"]=3;Reject(n,"Water spawn rejected");}
        {var n=JsonNode.Parse(json)!;n["Units"]![1]!["Type"]="unknown";Reject(n,"Unknown unit type rejected");}
        {var n=JsonNode.Parse(json)!;n["Rules"]!["EnemyCommander"]="p-command";Reject(n,"Wrong-army commander rejected");}
        {var n=JsonNode.Parse(json)!;n["Rules"]!["SurvivalVictory"]=false;n["Rules"]!["CommanderKillVictory"]=false;Reject(n,"Scenario needs a real victory condition");}
        {var n=JsonNode.Parse(json)!;n["Map"]![0]="WWWWWWWWWWWWW";n["Units"]![0]!["Y"]=0;Reject(n,"Disconnected/invalid army rejected");}
        bool dup=false;try{ArenaConfig.Parse(json.Replace("\"SchemaVersion\": 1","\"SchemaVersion\": 1, \"SchemaVersion\": 1"));}catch(JsonException){dup=true;}Check(dup,"Duplicate JSON keys rejected");
        // Seeded legal-command playouts exercise pathfinding, combat, automatic enemy turns and objective termination.
        for(int seed=0;seed<80;seed++)
        {
            var random=new Random(seed);b=Fresh();int ticks=0;
            while(!b.Finished&&ticks++<500)
            {
                if(b.Phase==ArenaPhase.Enemy)b.StepEnemy();
                else
                {
                    var actor=b.Units.FirstOrDefault(u=>u.Side=="Player"&&u.Alive&&!u.Acted);
                    if(actor==null){b.EndPlayerTurn();continue;}
                    var attackable=b.Units.Where(t=>b.CanAttack(actor,t)).ToArray();
                    if(attackable.Length>0)b.Attack(actor.Id,attackable[random.Next(attackable.Length)].Id);
                    else if(!actor.Moved&&random.Next(2)==0)
                    {var legal=b.Reachable(actor.Id).Keys.Where(p=>p!=actor.Position).ToArray();if(legal.Length>0)b.Move(actor.Id,legal[random.Next(legal.Length)]);else b.Brace(actor.Id);}
                    else if(!b.Rally(actor.Id))b.Brace(actor.Id);
                }
                Check(b.Units.All(u=>u.Hp>=0&&u.Hp<=u.Type.Health),"Every command preserves HP bounds");
                Check(b.Units.All(u=>b.Passable(u.Position)),"AI and player stay on legal terrain");
                Check(b.Units.Where(u=>u.Alive).Select(u=>u.Position).Distinct().Count()==b.Units.Count(u=>u.Alive),"Living units never overlap");
                Check(b.Units.All(u=>u.RallyLeft>=0&&u.RallyLeft<=u.Type.RallyUses),"Charge bounds");
            }
            Check(b.Finished&&ticks<500,"Every seeded scenario terminates by real win/loss rules");
            Check(JsonSerializer.Serialize(scenario)==before,"Random playout cannot corrupt replay configuration");
        }
        b=Fresh();int demoTicks=0;
        while(!b.Finished&&demoTicks++<200)
        {
            if(b.Phase==ArenaPhase.Enemy)b.StepEnemy();
            else b.StepDefenderDemo();
        }
        Check(b.Finished,"Visible demo policy terminates with ordinary commands");
        Console.WriteLine($"Default Last Bridge demo: {b.Phase}; held {b.HeldRounds}; moves {b.Moves}; attacks {b.Attacks}; ticks {demoTicks}.");
        Check(b.Phase==ArenaPhase.Victory&&(b.HeldRounds==8||b.Unit("e-command")?.Alive==false)&&b.Attacks>0&&b.Moves>0,"Shipped defensive demonstration wins through the configured real objectives");
        b=Fresh();b.Scenario.Rules.CommanderKillVictory=false;int survivalTicks=0;
        while(!b.Finished&&survivalTicks++<200){if(b.Phase==ArenaPhase.Enemy)b.StepEnemy();else b.StepDefenderDemo();}
        Check(b.Phase==ArenaPhase.Victory&&b.HeldRounds==8,"Survival-only objective waits for eight full rounds even after commander defeat");
        b=Fresh();int idleTicks=0;while(!b.Finished&&idleTicks++<200){if(b.Phase==ArenaPhase.Player)b.EndPlayerTurn();else b.StepEnemy();}
        Console.WriteLine($"No-orders Last Bridge: {b.Phase}; held {b.HeldRounds}; attacks {b.Attacks}; ticks {idleTicks}.");
        Check(b.Phase==ArenaPhase.Defeat,"Ignoring every defensive command must lose the shipped scenario");
        var candidates=new[]{new TargetCandidate(0,90,100,true,true),new TargetCandidate(1,15,80,true,true),new TargetCandidate(2,1,100,false,true),new TargetCandidate(3,0,100,true,true),new TargetCandidate(4,1,100,true,false)};
        Check(TargetingRules.PreferWounded(candidates,0,0.5)==1,"Raider selects weakest living legal player");
        Check(TargetingRules.PreferWounded(candidates,99,0.5)==99,"Never repairs/overrides an invalid native original target");
        Check(TargetingRules.PreferWounded(candidates,0,0.1)==0,"Health threshold leaves normal target unchanged");
        Check(TargetingRules.PreferWounded(new[]{new TargetCandidate(0,10,100,true,true),new TargetCandidate(1,10,100,true,true)},1,.5)==1,"Preserve native choice on equally wounded targets");
        for(int seed=0;seed<1000;seed++)
        {
            var rng=new Random(seed);var legal=Enumerable.Range(0,6).Select(i=>new TargetCandidate(i,rng.Next(1,101),100,true,i%3!=2)).ToArray();
            int result=TargetingRules.PreferWounded(legal,0,.5);var picked=legal.Single(c=>c.Slot==result);
            Check(picked.Player&&picked.Targetable&&picked.Hp>0,"Selected native target stays inside supplied legal set");
            Check(!legal.Any(c=>c.Targetable&&c.Hp<=50)||picked.Hp==legal.Where(c=>c.Targetable&&c.Hp<=50).Min(c=>c.Hp),"Wounded choice has lowest HP fraction");
        }
        Console.WriteLine($"PASS: Last Bridge {checks} assertions; independent scenario, movement, attacks, turns, objectives, replay/config isolation and native-target selection rules. Runtime UI verification remains separate.");
    }
}
