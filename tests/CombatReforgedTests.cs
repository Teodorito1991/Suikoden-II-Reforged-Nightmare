using NightmareRebalance;
using System.Reflection;
internal static class CombatReforgedTests
{
    internal static void Run()
    {
        int n=0;void Check(bool v){n++;if(!v)throw new Exception("Combat Reforged managed assertion "+n);}
        string Resource(string name){using var s=Assembly.GetExecutingAssembly().GetManifestResourceStream("ReforgedContent."+name)!;using var r=new StreamReader(s);return r.ReadToEnd();}
        string json=Resource("combat-reforged.json");var p=CombatReforgedRules.Parse(json);var audit=ContentRules.Read<CombatAudit>(Resource("combat-audit.json"));
        var settings=System.Text.Json.Nodes.JsonNode.Parse(json)!.AsObject();
        settings.Remove("EnableTeamwork");
        settings.Remove("EnableArcaneGuard");
        var legacy=CombatReforgedRules.Parse(settings.ToJsonString());
        Check(legacy.Enabled&&legacy.EnableTeamwork&&legacy.EnableArcaneGuard);
        settings["EnableTeamwork"]=false;
        settings["EnableArcaneGuard"]=false;
        var optedOut=CombatReforgedRules.Parse(settings.ToJsonString());
        Check(optedOut.Enabled&&!optedOut.EnableTeamwork&&!optedOut.EnableArcaneGuard);
        void Reject(string s){bool failed=false;try{CombatReforgedRules.Parse(s);}catch{failed=true;}Check(failed);}
        Reject(json.Replace("\"SchemaVersion\": 1","\"SchemaVersion\": 2"));Reject(json.Replace("\"FocusLimit\": 2","\"FocusLimit\": 0"));Reject(json.Replace("\"RepeatPenalty\": 28","\"RepeatPenalty\": 999"));Reject(json.Replace("boss_ryo","boss_luc1"));Reject(json.Replace("\"Id\": 6","\"Id\": 99"));Reject(json.Replace("\"Enabled\": true","\"Typo\": true"));Reject(json.Replace("\"HpMultiplier\": 1.35","\"HpMultiplier\": 9"));
        Check(audit.Enemies.Length==280);Check(audit.Enemies.Select(x=>x.Id).Distinct().Count()==280);Check(audit.Bosses.Length==4);
        Check(p.SleepSusceptibilityMultiplier==1.0&&ContentRules.SleepSusceptibility(0,p.SleepSusceptibilityMultiplier)==0);
        foreach(var a in audit.Enemies){Check(a.Signature.Length==47);Check(a.Signature[0]>0);Check(a.Behavior is "Caster" or "Aggressive" or "Cautious" or "Opportunist");}
        var targets=new[]{new CombatTarget(0,20,100,40,50,40,100,0),new CombatTarget(1,100,100,45,5,180,80,0),new CombatTarget(2,100,100,60,60,10,5,0)};
        Check(CombatReforgedRules.ChooseTarget("Aggressive",targets,1,2)==0);Check(CombatReforgedRules.ChooseTarget("Caster",targets,0,2)==1);Check(CombatReforgedRules.ChooseTarget("Cautious",targets,0,2)==2);Check(CombatReforgedRules.ChooseTarget("Disruptor",targets,0,2)==1);
        Check(CombatReforgedRules.ChooseTarget("Aggressive",new[]{targets[0] with{Focus=3},targets[1]},0,2)==1);
        foreach(string role in new[]{"Aggressive","Caster","Cautious","Disruptor","Opportunist","Alternating","Attrition"})
        for(int hp=0;hp<=100;hp+=5)for(int power=0;power<=300;power+=15)foreach(int type in new[]{0,1,2,5})
        {
            var t=targets.Select(x=>x with{Hp=hp}).ToArray();var a=new CombatAction(1,type,power,false,false,0,20,1,true);
            int score=CombatReforgedRules.ActionScore(role,a,t,targets,50,-1,p.RepeatPenalty,p.HealingThreshold);
            Check(type==0?score==int.MinValue:score is >int.MinValue and <10000);
            Check(CombatReforgedRules.ActionScore(role,a with{Legal=false},t,targets,50,-1,28,.65)==int.MinValue);
            Check(CombatReforgedRules.ActionScore(role,a with{Remaining=0},t,targets,50,-1,28,.65)==int.MinValue);
            Check(CombatReforgedRules.ActionScore(role,a with{Probability=0},t,targets,50,-1,28,.65)==int.MinValue);
        }
        var spell=new CombatAction(1,2,50,false,false,10,30,1,true);
        int S(CombatAction a,IReadOnlyList<CombatTarget> ts,int prior=-1)=>CombatReforgedRules.ActionScore("Caster",a,ts,ts,50,prior,28,.65);
        Check(S(spell,targets)>S(spell,targets.Select(t=>t with{StatusUseful=false}).ToArray()));Check(S(spell,targets)>S(spell,targets,1));
        var heal=spell with{Type=5};Check(S(heal,targets)>S(heal,targets.Select(t=>t with{Hp=100}).ToArray()));
        foreach(var b in audit.Bosses)
        {
            var id=new EncounterIdentity(b.TableId,b.PartyId,0,b.Flags,6,1,b.Hooks[5]!=0,true,b.Hooks,b.HookNames,new[]{b.EnemyId},new[]{b.Signature[0]},new[]{1,0,0,0,0,0},new[]{0,1,0,0,0,0},0,true);
            Check(CombatReforgedRules.BossMatches(b,id));Check(!CombatReforgedRules.BossMatches(b,id with{TableId="unknown"}));Check(!CombatReforgedRules.BossMatches(b,id with{EnemyMaxHp=new[]{b.Signature[0]+1}}));Check(!CombatReforgedRules.BossMatches(b,id with{HookNames=new[]{"edited"}}));Check(!CombatReforgedRules.BossMatches(b,id with{Enemies=2}));Check(!CombatReforgedRules.BossMatches(b,id with{ScriptCommandCount=1}));
        }
        for(int power=0;power<300;power+=5)for(int defense=0;defense<300;defense+=5)
        {Check(CombatReforgedRules.BaseDamage(power,40,defense,defense,true,4)>=1);Check(CombatReforgedRules.BaseDamage(power,40,defense,defense,false,4)>=1);}
        Console.WriteLine($"PASS: Combat Reforged {n} assertions; audited identity/action scope, legal choices, role tradeoffs, resource constraints, pacing bounds and preserved native exclusions. Native and full battle evidence separate.");
        CombatBalanceScenarios.Run(p);
    }
}
