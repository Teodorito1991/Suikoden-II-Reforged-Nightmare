using NightmareRebalance;
using System.Reflection;
internal static class WarTacticsTests
{
    internal static void Run()
    {
        int n=0;void Check(bool v){n++;if(!v)throw new Exception("War Tactics managed assertion "+n);}
        using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("ReforgedContent.war-tactics.json")!;
        using var reader=new StreamReader(stream);string json=reader.ReadToEnd();var p=WarTacticsRules.Parse(json);
        void Reject(string t){bool bad=false;try{WarTacticsRules.Parse(t);}catch{bad=true;}Check(bad);}
        Reject(json.Replace("\"SchemaVersion\": 1","\"SchemaVersion\": 2"));Reject(json.Replace("\"ThreatWeight\": 2","\"ThreatWeight\": 9"));Reject(json.Replace("\"Enabled\": true","\"Enabld\": true"));
        Reject(json.Replace("\"WarNumber\": 6","\"WarNumber\": 10"));Reject(json.Replace("\"RequiredRouts\": 4","\"RequiredRouts\": 12"));Reject(json.Replace("\"RequiredRouts\": 2","\"RequiredRouts\": 0"));
        var actor=new TacticalUnit(0,4,4,0,2,1,false);var clear=new TacticalFacts(0,0,0,0,0,0,0,0,0,false,false);
        int Score(TacticalFacts f,int action=1,int chance=60,TacticalUnit? unit=null)=>WarTacticsRules.Score(0,p,unit??actor,action,chance,f);
        Check(Score(clear with{Threat=1})<Score(clear));Check(Score(clear with{ImmediateThreat=1})<Score(clear));
        Check(Score(clear with{NearbyAllies=1})>Score(clear));Check(Score(clear with{CoveredSupport=1})>Score(clear));
        Check(Score(clear with{Threat=2},unit:actor with{Damage=1})<Score(clear with{Threat=2}));
        Check(Score(clear with{Focus=2,NeededHits=3},0)>Score(clear,0));Check(Score(clear with{CommanderExposure=3},0)>Score(clear,0));
        Check(Score(clear with{Focus=3,NeededHits=1},0)<Score(clear,0));
        Check(Score(clear,0,5)<Score(clear,0,60));Check(Score(clear with{RepeatedMove=true})<Score(clear));
        Check(Score(clear with{RepeatedMove=true,Progress=true})==Score(clear));
        Check(Score(clear with{Utility=10,Victims=2},2)>Score(clear with{Utility=0},2));
        Check(Score(clear with{Utility=10,Victims=2,FriendlyVictims=1},2)<Score(clear with{Utility=10,Victims=2},2));
        Check(Score(clear with{CoveredSupport=2},unit:actor with{Range=2})==Score(clear,unit:actor with{Range=2}));
        foreach(int original in new[]{-32768,-30000,-100,0,100,30000,32767})for(int threat=0;threat<=15;threat++)for(int allies=0;allies<=14;allies++)foreach(int action in new[]{0,1,2,3})
        {
            var f=clear with{Threat=threat,ImmediateThreat=threat,NearbyAllies=allies,Focus=2,Utility=10,Victims=2};
            int score=WarTacticsRules.Score(original,p,actor,action,60,f);
            Check(action==3?score==original:score is >=-32768 and <=32767);
            p.Enabled=false;Check(WarTacticsRules.Score(original,p,actor,action,60,f)==original);p.Enabled=true;
        }
        foreach(var objective in p.Objectives)for(int war=0;war<=17;war++)for(int turn=0;turn<=25;turn++)for(int count=0;count<=14;count++)
        {
            bool expected=war==objective.WarNumber&&count>=objective.RequiredRouts&&turn>=(war==3?14:war==6?9:7);
            Check(WarTacticsRules.Objective(objective,war,turn,count,true,true,4,2)==expected);
            Check(!WarTacticsRules.Objective(objective,war,turn,count,false,true,4,2));
        }
        var third=p.Objectives.Single(o=>o.WarNumber==3);
        for(int x=0;x<16;x++)for(int y=0;y<12;y++)Check(WarTacticsRules.Objective(third,3,14,2,true,true,x,y)==(x>=4&&x<=6&&y>=1&&y<=3));
        third.Enabled=false;Check(!WarTacticsRules.Objective(third,3,14,3,true,true,4,2));
        Check(p.Objectives.Single(o=>o.WarNumber==6).RequiredRouts==4);
        Console.WriteLine($"PASS: War Tactics {n} assertions; threat/support/focus/ability tradeoffs, objective gates, opt-outs, malformed config and bounded scores. Native/legal-map evidence separate.");
    }
}
