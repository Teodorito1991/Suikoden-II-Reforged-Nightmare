using System.Text.Json;
using NightmareRebalance;

internal static class StageBTests
{
    internal static void Run()
    {
        int checks=0;
        void Check(bool ok,string reason) { checks++;if(!ok)throw new Exception("Stage B: "+reason); }
        var rat=new EncounterIdentity("boss_rat","boss_rat_00",0,268435456,6,1,false,true,
            new[]{1467,0,0,1468,0,0},new[]{"rat_normal_mae_func00","(null)","(null)","rat_boss_end_func00","(null)","(null)"},
            new[]{"boss_rat_00"},new[]{5200},new[]{1,0,0,0,0,0},new[]{0,1,0,0,0,0},0);
        var selected=new HashSet<string>{EncounterCoverage.RatEncounter};
        var allow=EncounterCoverage.Decide(rat,true,true,selected);
        var block=EncounterCoverage.Decide(rat,true,false,selected);
        string Profile(string id="rat",int priority=100,bool enabled=true,double? hp=1.9,double? damage=1.6) => JsonSerializer.Serialize(new Dictionary<string,object?>{
            ["profileId"]=id,["priority"]=priority,["enabled"]=enabled,["tableId"]="boss_rat",["encounterId"]="boss_rat_00",["enemyId"]="boss_rat_00"
        }.Concat(hp.HasValue?new[]{new KeyValuePair<string,object?>("hpMultiplier",hp.Value)}:Array.Empty<KeyValuePair<string,object?>>())
         .Concat(damage.HasValue?new[]{new KeyValuePair<string,object?>("damageMultiplier",damage.Value)}:Array.Empty<KeyValuePair<string,object?>>()).ToDictionary(p=>p.Key,p=>p.Value));
        string File(params string[] profiles) => "{\"schemaVersion\":1,\"profiles\":["+string.Join(',',profiles)+"]}";
        var set=BossProfileSet.Parse(File(Profile()));
        BossResolution Resolve(BossProfileSet s,bool enabled=true,bool fallback=true,EncounterIdentity? id=null,CoverageDecision? scope=null,bool reinforcement=false)
            =>s.Resolve(id??rat,"boss_rat_00",scope??allow,enabled,fallback,1.6,1.45,reinforcement);
        var result=Resolve(set);
        Check(result.ProfileId=="rat"&&result.HpMultiplier==1.9&&result.DamageMultiplier==1.6,"Exact selector and final factors");
        Check(Rules.Scale(5200,result.HpMultiplier,32767)==9880,"Profile HP is vanilla*1.90, not global*profile");
        Check(Rules.Scale(100,result.DamageMultiplier,9999)==160,"Profile damage replaces global factor");
        Check(!Resolve(set,scope:block).ScalingAllowed&&Resolve(set,scope:block).ProfileId==null,"Profile cannot override custom-handler exclusion");
        Check(!Resolve(set,scope:new(false,false,"conflict")).ScalingAllowed,"Conflict remains fail closed");
        Check(!allow.FullEffects,"Stage A custom reward exclusion unchanged");
        Check(Resolve(set,enabled:false,fallback:false).HpMultiplier==1.6,"Disabled feature ignores vanilla-fallback setting");
        Check(Resolve(BossProfileSet.Empty).HpMultiplier==1.6&&Resolve(BossProfileSet.Empty).DamageMultiplier==1.45,"Missing profiles use global baseline");
        Check(Resolve(BossProfileSet.Empty,fallback:false).HpMultiplier==1&&Resolve(BossProfileSet.Empty,fallback:false).DamageMultiplier==1,"Known boss can request vanilla fallback");
        Check(Resolve(BossProfileSet.Parse(File(Profile(enabled:false)))).ProfileId==null,"Disabled individual entry uses fallback");
        var damageOnly=BossProfileSet.Parse(File(Profile(hp:null)));
        Check(Resolve(damageOnly).HpMultiplier==1.6&&Resolve(damageOnly).DamageMultiplier==1.6,"Missing HP value uses global fallback");
        Check(Resolve(damageOnly,fallback:false).HpMultiplier==1,"Missing HP value can use vanilla fallback");
        Check(Resolve(damageOnly).HpSource=="global-nightmare"&&Resolve(damageOnly).DamageSource=="boss-profile","Diagnostic field sources distinguish profile from fallback");
        Check(Resolve(damageOnly,fallback:false).HpSource=="vanilla-fallback","Vanilla fallback source reported explicitly");
        var hpOnly=BossProfileSet.Parse(File(Profile(damage:null)));
        Check(Resolve(hpOnly).DamageMultiplier==1.45,"Missing damage value uses global fallback");
        var priority=BossProfileSet.Parse(File(Profile("low",-10,hp:1.4),Profile("high",20,hp:2),Profile("off",100,false,hp:3)));
        Check(Resolve(priority).ProfileId=="high"&&Resolve(priority).HpMultiplier==2,"Highest enabled priority wins");
        var reverse=BossProfileSet.Parse(File(Profile("high",20,hp:2),Profile("low",-10,hp:1.4)));
        Check(Resolve(reverse)==Resolve(priority),"File order does not alter priority");
        var noLayer=BossProfileSet.Parse(File(Profile("lower",0,hp:2.5,damage:3),Profile("higher",10,hp:null,damage:1.5)));
        Check(Resolve(noLayer).HpMultiplier==1.6,"Winner does not inherit fields from a lower profile");
        var unknown=rat with{TableId="another_table"};
        Check(Resolve(set,id:unknown,fallback:false).HpMultiplier==1.6&&!Resolve(set,id:unknown).KnownBoss,"Shared enemy template in another encounter stays global");
        var standard=rat with{TableId="regular",PartyId="ordinary",HasEndHandler=false};
        var standardScope=EncounterCoverage.Decide(standard,true,false,new HashSet<string>());
        Check(Resolve(set,id:standard,scope:standardScope,fallback:false).DamageMultiplier==1.45,"Regular encounters unchanged even with vanilla boss fallback");
        Check(!Resolve(set,id:rat with{PartyIndex=1}).KnownBoss,"Different party index cannot reuse a profile");
        Check(!Resolve(set,id:rat with{EnemyMaxHp=new[]{6000}}).KnownBoss,"Altered live base HP fails exact registry shape");
        var mixed=rat with{Enemies=2,EnemyIds=new[]{"boss_rat_00","minion"},EnemyMaxHp=new[]{5200,100}};
        Check(Resolve(set,id:mixed).ProfileId==null,"Unexpected mixed roster does not spread profile to minions");
        Check(set.Resolve(rat,"minion",allow,true,false,1.6,1.45).DamageMultiplier==1.45,"Nonboss entity in the same work retains global factor");
        Check(Resolve(set,reinforcement:true).ProfileId==null&&Resolve(set,reinforcement:true).HpMultiplier==1.6,"Unverified replacement phase does not inherit profile");
        var hap=rat with{TableId="boss_hap1",PartyId="boss_hap1_00",EnemyIds=new[]{"boss_hap1_00"},EnemyMaxHp=new[]{7400},
            HookIndices=new[]{1295,0,0,1296,0,0},HookNames=new[]{"hap1_normal_mae_func00","(null)","(null)","hap1_boss_end_func00","(null)","(null)"}};
        Check(BossRegistry.Matches(hap,"boss_hap1_00"),"Second asset-derived selector is recognized");
        Check(!EncounterCoverage.Decide(hap,true,true,selected).StatScaling,"Recognition never adds an allowlist entry");
        var sie=rat with{TableId="boss_sie1",PartyId="boss_sie1_00",EnemyIds=new[]{"boss_sie1_00"},EnemyMaxHp=new[]{2800},
            HookIndices=new[]{1500,0,0,1501,0,0},HookNames=new[]{"sie1_special_mae_func00","(null)","(null)","sie1_boss_end_func00","(null)","(null)"}};
        Check(BossRegistry.Matches(sie,"boss_sie1_00")&&!EncounterCoverage.Decide(sie,true,true,selected).StatScaling,"Third selector remains excluded by unchanged Stage A policy");
        var bad=new List<string>{"null","[]","{}","{",File(Profile(),Profile()),File(Profile("one"),Profile("two")),
            File(Profile().Replace("boss_rat","BOSS_RAT")),
            File(Profile()).Replace("\"schemaVersion\":1","\"schemaVersion\":1,\"schemaVersion\":1"),
            "{\"schemaVersion\":1,\"profiles\":[\"bad\"]}",
            File(Profile().Replace("boss_rat_00","unverified")),File(Profile(hp:null,damage:null)),
            File(Profile().Replace("\"priority\":100","\"priority\":1001")),File(Profile().Replace("\"enabled\":true","\"enabled\":1")),
            File(Profile().Replace("\"profileId\":\"rat\"","\"profileId\":\"rat\",\"profileId\":\"other\"")),
            File(Profile()).Replace("\"schemaVersion\":1","\"schemaVersion\":2"),File(Profile()).Replace("\"profiles\":","\"extra\":true,\"profiles\":"),
            File(Profile().Replace("\"hpMultiplier\":1.9","\"hpMultiplier\":\"1.9\"")),
            File(Profile().Replace("\"hpMultiplier\":1.9","\"hpMultiplier\":null")),
            File(Profile().Replace("\"hpMultiplier\":1.9","\"magicResistanceMultiplier\":1.1")),
            File(Profile().Replace("\"priority\":100","\"priority\":1.5")),new string(' ',BossProfileSet.MaxFileBytes+1)};
        foreach(double n in new[]{0,-1,5.001,1e300,0.099})bad.Add(File(Profile(hp:n)));
        bad.Add(File(Profile().Replace("\"hpMultiplier\":1.9","\"hpMultiplier\":1e9999")));
        bad.Add(File(Enumerable.Range(0,65).Select(i=>Profile("p"+i,i)).ToArray()));
        foreach(string json in bad)
        {
            bool rejected=false;
            try{BossProfileSet.Parse(json);}catch(Exception ex)when(ex is FormatException or JsonException){rejected=true;}
            Check(rejected,"Bad/duplicate/unsupported configuration rejected wholly: "+json[..Math.Min(json.Length,60)]);
        }
        // A valid entry before a bad one cannot leak from a failed atomic parse.
        BossProfileSet active=BossProfileSet.Empty;
        try{active=BossProfileSet.Parse(File(Profile(),Profile("bad").Replace("boss_rat_00","unknown")));}catch(FormatException){}
        Check(Resolve(active).ProfileId==null,"No partial configuration application");
        Check(Resolve(BossProfileSet.Parse(File(Profile(hp:0.1,damage:5)))).HpMultiplier==0.1,"Inclusive multiplier bounds accepted");
        for(int battle=0;battle<100;battle++)
        {
            var ledger=new EnemyInstanceLedger();int hp=5200;
            if(ledger.Add(100))hp=Rules.Scale(hp,result.HpMultiplier,32767);
            Check(!ledger.Add(100)&&hp==9880,"Same native instance cannot be scaled twice");
            Check(ledger.Add(101)&&!ledger.Add(0),"New instance distinct; null pointer rejected");
            var gate=new VictoryGate();gate.MarkVictory();Check(gate.TryConsume()&&!gate.TryConsume(),"Original victory gate stays per-battle and once only");
        }
        bool outer=DamageScaling.Enter();bool inner=DamageScaling.Enter();int hit=100;
        Check(!DamageScaling.Apply(inner,true,ref hit,1.6)&&hit==100,"Nested calculator does not scale");
        DamageScaling.Exit(null);Check(DamageScaling.Apply(outer,true,ref hit,1.6)&&hit==160,"Outer calculator scales once");DamageScaling.Exit(null);
        var exception=new InvalidOperationException("fixture");DamageScaling.Enter();Check(ReferenceEquals(DamageScaling.Exit(exception),exception),"Finalizer propagates original exception");
        Check(DamageScaling.Enter(),"Depth resets after exception");DamageScaling.Exit(null);
        DamageScaling.Enter();bool otherThreadOuter=false;
        var worker=new Thread(()=>{otherThreadOuter=DamageScaling.Enter();DamageScaling.Exit(null);});worker.Start();worker.Join();
        Check(otherThreadOuter,"Nested calculation scope is isolated per thread");DamageScaling.Exit(null);
        int player=100,zero=0,healing=-100;
        Check(!DamageScaling.Apply(true,false,ref player,2)&&player==100,"Player/unintended source unchanged");
        Check(!DamageScaling.Apply(true,true,ref zero,2)&&zero==0,"Zero damage unchanged");
        Check(!DamageScaling.Apply(true,true,ref healing,2)&&healing==-100,"Negative/healing unchanged");
        int capped=9000;DamageScaling.Apply(true,true,ref capped,5);Check(capped==9999,"Damage cap preserved");
        for(int i=0;i<200;i++)
        {
            double factor=0.1+i*0.024;var model=Resolve(BossProfileSet.Parse(File(Profile(hp:factor,damage:factor))));
            Check(Math.Abs(model.HpMultiplier-factor)<1e-12&&Math.Abs(model.DamageMultiplier-factor)<1e-12,"Final factor never compounds with global value");
        }
        string configRoot=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..","..","config"));
        foreach(string relative in new[]{"boss-profiles.json","examples/pest-rat-early-profile.json","examples/pest-rat-priority-test.json","examples/verified-boss-selectors-disabled.json"})
            Check(BossProfileSet.Parse(System.IO.File.ReadAllText(Path.Combine(configRoot,relative))).Profiles.Count<=3,"Shipped JSON example passes the real validator");
        Console.WriteLine($"PASS Stage B: {checks} assertions; profiles, priority/fallback, strict validation, scope isolation and production scaling guards.");
    }
}
