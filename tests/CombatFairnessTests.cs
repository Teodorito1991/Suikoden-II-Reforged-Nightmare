using NightmareRebalance;
using System.Text.Json;

internal static class CombatFairnessTests
{
    internal static void Run()
    {
        int checks=0;
        void Check(bool ok,string label){checks++;if(!ok)throw new Exception("Combat fairness: "+label);}
        for(int tech=0;tech<=255;tech++)for(int luck=0;luck<=255;luck+=5)
        {
            int t=CombatFairnessRules.CriticalThreshold(tech,luck);
            Check(t>=3&&t<=25,"native critical bounds");
            Check(CombatFairnessRules.CriticalThreshold(tech,luck,true)==t*3/2,"critical rune integer rounding");
            int n=0;for(int roll=0;roll<99;roll++)if(CombatFairnessRules.Critical(t,roll))n++;
            Check(n==t,"99-outcome roll denominator");
        }
        for(int speed=1;speed<=255;speed++)for(int roll=0;roll<Math.Max(1,speed/10*2);roll++)
        {
            int n=CombatFairnessRules.EnemyInitiative(speed,roll);
            Check(n>=speed-speed/10+1||speed<10,"enemy initiative lower bound");
            Check(n<=speed+speed/10,"enemy initiative upper bound");
            Check(CombatFairnessRules.EnemyInitiative(speed,roll,1)==n*90/100,"native action-count code1 initiative");
            Check(CombatFairnessRules.EnemyInitiative(speed,roll,2)==n*80/100,"native action-count code2 initiative");
            Check(CombatFairnessRules.PlayerInitiative(speed,true)==2*speed,"player native haste");
        }
        Check(CombatFairnessRules.Review(100,100,0,1,.95,0)=="Easy","easy fixture");
        Check(CombatFairnessRules.Review(100,95,0,4,.5,2)=="Challenging","challenge fixture");
        Check(CombatFairnessRules.Review(100,60,0,7,.3,4)=="Dangerous","danger fixture");
        Check(CombatFairnessRules.Review(100,20,0,9,.1,5)=="OverwhelmingReview","loss fixture");
        Check(CombatFairnessRules.Review(100,80,20,30,.5,0)=="StallReview","stalls separate");
        Check(CombatFairnessRules.Review(0,0,0,0,0,0)=="Invalid","invalid sample");
        Console.WriteLine($"PASS: Combat fairness {checks} assertions; inspected formula projections and separate outcome review policy; native comparison and runtime calibration pending.");
        CombatEvolutionScenarios.Run();
    }
}

internal static class CombatEvolutionScenarios
{
    sealed class Fighter
    {
        internal int Hp,Max,Attack,Defense,MagicDefense,Speed,Tech,Luck,Charges;
        internal bool Player;
    }
    internal static void Run()
    {
        var rows=new List<object>();
        // Sensitivity fixtures, not named characters or claimed campaign equipment.
        // Equipment is represented by projected defense changes, not item procs.
        foreach(int stage in new[]{0,1,2})foreach(int levelTier in new[]{-1,0,1})foreach(string build in new[]{"Balanced","Armored","Caster"})foreach(bool prepared in new[]{false,true})foreach(bool area in new[]{false,true})
        {
            int wins=0,stalls=0,roundSum=0,spentSum=0,receivedSum=0,healedSum=0,critSum=0,firstDeaths=0,sleepSum=0,survivorSum=0,recoveredSum=0;
            double remaining=0;
            const int runs=128;
            for(int seed=0;seed<runs;seed++)
            {
                var rng=new Random(seed+91100);int count=stage==0?3:6;
                int tier=levelTier;var team=new List<Fighter>();
                for(int i=0;i<count;i++)
                {
                    int max=new[]{115,350,570}[stage]+tier*new[]{20,50,70}[stage];
                    team.Add(new(){Player=true,Max=max,Hp=prepared?max:max*2/3,Attack=new[]{50,145,250}[stage]+tier*new[]{9,25,35}[stage]+(build=="Caster"?-15:0),Defense=new[]{24,68,130}[stage]+tier*9+(build=="Armored"?new[]{12,25,35}[stage]:0),MagicDefense=new[]{22,65,120}[stage]+tier*9,Speed=new[]{32,80,145}[stage]+tier*10,Tech=new[]{30,80,140}[stage],Luck=45,Charges=prepared?4:1});
                }
                int enemyCount=stage==0?3:4;
                for(int i=0;i<enemyCount;i++)team.Add(new(){Max=new[]{70,330,620}[stage],Hp=new[]{70,330,620}[stage],Attack=new[]{55,160,280}[stage],Defense=new[]{25,65,120}[stage],MagicDefense=new[]{18,60,110}[stage],Speed=new[]{38,95,160}[stage],Tech=new[]{35,85,145}[stage],Luck=40});
                int turns=0,received=0,healed=0,spent=0,crits=0,prevented=0;var asleep=new HashSet<Fighter>();
                for(;turns<30&&team.Any(x=>x.Player&&x.Hp>0)&&team.Any(x=>!x.Player&&x.Hp>0);turns++)
                {
                    var queue=team.Where(x=>x.Hp>0).Select((u,i)=>(u,i,speed:u.Player?CombatFairnessRules.PlayerInitiative(u.Speed,false):CombatFairnessRules.EnemyInitiative(u.Speed,rng.Next(Math.Max(1,u.Speed/10*2))))).OrderByDescending(x=>x.speed).ThenBy(x=>x.i).ToArray();
                    foreach(var action in queue)
                    {
                        var a=action.u;if(a.Hp<=0)continue;
                        if(asleep.Remove(a)){prevented++;continue;}
                        var targets=team.Where(x=>x.Player!=a.Player&&x.Hp>0).ToArray();if(targets.Length==0)break;
                        var t=targets.OrderBy(x=>x.Hp).First();bool healer=a.Player&&team.IndexOf(a)==count-1;
                        var hurt=team.Where(x=>x.Player&&x.Hp>0).OrderBy(x=>(double)x.Hp/x.Max).FirstOrDefault();
                        if(healer&&a.Charges>0&&hurt!=null&&hurt.Hp<hurt.Max*.5)
                        {int amount=Math.Min(hurt.Max-hurt.Hp,stage==0?180:240);hurt.Hp+=amount;healed+=amount;a.Charges--;spent++;continue;}
                        if(healer&&build=="Caster"&&a.Charges>0&&turns%3==0)
                        {a.Charges--;spent++;foreach(var e in targets)if(rng.NextDouble()<.3)asleep.Add(e);continue;}
                        bool magic=healer&&a.Charges>0&&build=="Caster";
                        if(magic){a.Charges--;spent++;}
                        var victims=!a.Player&&area&&turns%3==1?targets:new[]{t};
                        foreach(var victim in victims)
                        {
                            int power=a.Attack-(victims.Length>1?a.Attack/3:0);
                            int raw=Math.Max(0,power-(magic?victim.MagicDefense:victim.Defense));
                            int damage=raw+rng.Next(1,raw<=5?(a.Player?4:3):(a.Player?10:9));
                            // Only the inspected threshold is used; 2x crit magnitude
                            // remains an explicit model assumption until full native proof.
                            if(!magic&&CombatFairnessRules.Critical(CombatFairnessRules.CriticalThreshold(a.Tech,a.Luck),rng.Next(99))){damage*=2;crits++;}
                            if(!a.Player)damage=Rules.Scale(damage,1.30,9999);
                            int actual=Math.Min(victim.Hp,damage);victim.Hp-=actual;if(victim.Player)received+=actual;
                            asleep.Remove(victim); // one-action sleep, modeled damage wake
                        }
                    }
                    if(turns==0&&team.Any(x=>x.Player&&x.Hp==0))firstDeaths++;
                }
                bool win=team.Any(x=>x.Player&&x.Hp>0)&&team.All(x=>x.Player||x.Hp==0);
                if(win)wins++;else if(team.Any(x=>x.Player&&x.Hp>0))stalls++;
                var caster=team[count-1];if(win&&caster.Hp>0&&caster.Charges< (prepared?4:1))recoveredSum++;
                remaining+=team.Where(x=>x.Player).Sum(x=>x.Hp)/(double)team.Where(x=>x.Player).Sum(x=>x.Max);
                survivorSum+=team.Count(x=>x.Player&&x.Hp>0);roundSum+=turns;spentSum+=spent;receivedSum+=received;healedSum+=healed;critSum+=crits;sleepSum+=prevented;
            }
            double hpFraction=remaining/runs,rounds=(double)roundSum/runs;
            rows.Add(new{Stage=new[]{"Early","Middle","Late"}[stage],LevelTier=new[]{"Underleveled","Normal","Optimized"}[levelTier+1],Build=build,Prepared=prepared,EnemyAreaPressure=area,Runs=runs,Victories=wins,Defeats=runs-wins-stalls,Stalls=stalls,WinRate=(double)wins/runs,MeanRounds=rounds,RemainingHpFraction=hpFraction,MeanSurvivors=(double)survivorSum/runs,MeanDamage=(double)receivedSum/runs,MeanHealing=(double)healedSum/runs,MeanChargesSpent=(double)spentSum/runs,MeanChargesRecovered=(double)recoveredSum/runs,MeanCrits=(double)critSum/runs,MeanSleepActionsPrevented=(double)sleepSum/runs,FirstRoundCasualtyRate=(double)firstDeaths/runs,Review=CombatFairnessRules.Review(runs,wins,stalls,rounds,hpFraction,(double)spentSum/runs)});
        }
        string path=Environment.GetEnvironmentVariable("REFORGED_EVOLUTION_REPORT")??Path.Combine(AppContext.BaseDirectory,"combat-evolution-scenarios.json");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path,JsonSerializer.Serialize(new{Status="Development sensitivity model; NOT runtime calibrated",Inspected="Base physical jitter, critical threshold and 99-outcome roll, enemy/player initiative projections",Assumptions="Illustrative stage/equipment tiers; normal hits assumed; 2x critical magnitude; one-action damage-woken sleep; periodic reduced-power area attack; no counter/passive/unite/affinity/procs; independent healing charges; enemy factor1.30; no claim of real formation replay or campaign fairness",Scenarios=rows},new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"EVOLUTION MODEL: {rows.Count} sensitivity fixtures / {rows.Count*128} seeded battles; calibration pending: {path}");
    }
}
