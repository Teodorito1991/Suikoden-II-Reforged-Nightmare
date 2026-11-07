using NightmareRebalance;
using System.Text.Json;
internal static class CombatBalanceScenarios
{
    // Explicit mathematical fixtures, not native battle replays. Player-first
    // initiative, no counters/crits/affinity/equipment procs; one-action sleep.
    internal static void Run(CombatReforgedFile config)
    {
        var rows=new List<object>();
        foreach(int stage in new[]{0,1,2})foreach(string composition in new[]{"Mixed","Bulwark","Glass"})foreach(string enemyRole in CombatReforgedRules.Roles)foreach(bool worn in new[]{false,true})foreach(bool status in new[]{false,true})foreach(bool reforged in new[]{false,true})
        {
            int victories=0,rounds=0,received=0,healed=0,spent=0,recovered=0,blocked=0,survivors=0,stalled=0,defeated=0;
            for(int seed=0;seed<200;seed++)
            {
                var rng=new Random(7000+seed);int count=stage==0?3:6;
                int maxHp=new[]{120,400,650}[stage]+(composition=="Bulwark"?80:composition=="Glass"?-30:0);
                int attack=new[]{65,165,290}[stage]+(composition=="Glass"?30:composition=="Bulwark"?-15:0);
                int defense=new[]{25,70,145}[stage]+(composition=="Bulwark"?25:composition=="Glass"?-20:0);
                int enemyPower=new[]{45,155,275}[stage],enemyDefense=new[]{30,75,160}[stage];
                bool magical=enemyRole=="FragilePressure";int enemyMagic=new[]{25,65,130}[stage];
                if(enemyRole=="Bulwark")enemyDefense+=20;if(enemyRole=="Raider")enemyPower+=10;
                int enemyCount=stage==0?3:4;double hpFactor=reforged?config.RolePacing.Single(p=>p.Role==enemyRole).HpMultiplier:enemyRole=="Bulwark"?1.85:enemyRole=="Raider"?1.25:enemyRole=="FragilePressure"?1.1:1.5;
                double damageFactor=reforged?config.RolePacing.Single(p=>p.Role==enemyRole).DamageMultiplier:enemyRole=="FragilePressure"?1.9:enemyRole=="Raider"?1.75:enemyRole=="Bulwark"?1.1:1.55;
                var hp=Enumerable.Repeat(worn?maxHp*2/3:maxHp,count).ToArray();var enemy=Enumerable.Repeat(Rules.Scale(new[]{45,220,460}[stage],hpFactor,32767),enemyCount).ToArray();
                var sleep=new int[enemyCount];int charges=stage==0?4:7,initial=charges;int turns=0;
                for(;turns<30&&hp.Any(v=>v>0)&&enemy.Any(v=>v>0);turns++)
                {
                    for(int a=0;a<count;a++)
                    {
                        if(hp[a]<=0)continue;
                        int target=Array.FindIndex(enemy,v=>v>0);if(target<0)break;
                        int lowest=Enumerable.Range(0,count).Where(i=>hp[i]>0).OrderBy(i=>hp[i]).First();
                        if(a==count-1&&charges>0&&hp[lowest]<maxHp*.45)
                        {int heal=reforged?config.SpellEffects.Single(s=>s.Id==(stage==0?6:12)).Effect:stage==0?100:210;int actual=Math.Min(heal,maxHp-hp[lowest]);hp[lowest]+=actual;healed+=actual;charges--;continue;}
                        // An armored physical party must spend a real charge/action
                        // on its modeled caster's existing Flaming Arrows option.
                        if(a==count-1&&charges>0&&attack-enemyDefense<=5){enemy[target]-=80;charges--;continue;}
                        if(status&&a==count-1&&charges>0&&turns%3==0)
                        {charges--;for(int e=0;e<enemyCount;e++)if(enemy[e]>0&&rng.NextDouble()<(reforged?.30*config.SleepSusceptibilityMultiplier:.45))sleep[e]=1;continue;}
                        int raw=Math.Max(0,attack-enemyDefense);int damage=raw+rng.Next(1,raw<=5?4:10);enemy[target]-=damage;
                    }
                    for(int e=0;e<enemyCount;e++)
                    {
                        if(enemy[e]<=0)continue;if(sleep[e]>0){sleep[e]--;blocked++;continue;}
                        var legal=Enumerable.Range(0,count).Where(i=>hp[i]>0).ToArray();if(legal.Length==0)break;
                        int t=reforged?legal.OrderBy(i=>hp[i]).First():legal[rng.Next(legal.Length)];
                        int raw=Math.Max(0,enemyPower+(magical?enemyMagic:0)-defense);int damage=Rules.Scale(CombatReforgedRules.BaseDamage(enemyPower,enemyMagic,defense,defense,!magical,rng.Next(1,raw<=5?3:9)),damageFactor,9999);
                        int actual=Math.Min(hp[t],damage);hp[t]-=actual;received+=actual;
                    }
                }
                bool win=hp.Any(v=>v>0)&&enemy.All(v=>v<=0);if(win)victories++;
                if(!hp.Any(v=>v>0))defeated++;else if(!win)stalled++;
                spent+=initial-charges;rounds+=turns;survivors+=hp.Count(v=>v>0);
                if(win&&hp[count-1]>0&&charges<initial){Span<byte> packed=new byte[4];packed[0]=(byte)((charges<<4)|initial);recovered+=Rules.Recover(packed,1,RecoveryPriority.LowestDepletedLevel);}
            }
            rows.Add(new{Stage=new[]{"Early","Middle","Late"}[stage],Composition=composition,EnemyRole=enemyRole,StartingHealth=worn?"TwoThirds":"Full",StatusEnabled=status,Rules=reforged?"v1.0":"v0.9",Runs=200,WinRate=victories/200.0,PartySurvivalRate=1-defeated/200.0,TurnLimitRate=stalled/200.0,MeanRounds=rounds/200.0,MeanSurvivors=survivors/200.0,MeanDamageReceived=received/200.0,MeanHealing=healed/200.0,MeanRuneSpent=spent/200.0,MeanRuneRecovered=recovered/200.0,MeanSleepActionsPrevented=blocked/200.0,MathematicalFixtureOnly=true});
        }
        string path=Environment.GetEnvironmentVariable("REFORGED_BALANCE_REPORT")??Path.Combine(AppContext.BaseDirectory,"combat-balance-scenarios.json");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path,JsonSerializer.Serialize(new{Model="Reduced deterministic turn model using inspected base damage; not the complete game engine",Assumptions="Player-first initiative; template stage stats; no criticals, counters, affinities, unite/passive/equipment effects; one-action sleep; healing spends an action/charge; enemy weak-target pressure is an upper-bound heuristic",Scenarios=rows},new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"BALANCE: {rows.Count} mathematical scenarios / {rows.Count*200} seeded battles; report {path}; no in-game acceptance claim.");
    }
}
