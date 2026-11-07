using NightmareRebalance;
using System.Reflection;
using System.Text.Json;
internal static class TotalWarTests
{
    internal static void Run()
    {
        int n=0;void Check(bool value){n++;if(!value)throw new Exception("Total War managed assertion "+n);}
        string Resource(string name){using var s=Assembly.GetExecutingAssembly().GetManifestResourceStream("ReforgedContent."+name)!;using var reader=new StreamReader(s);return reader.ReadToEnd();}
        string text=Resource("total-war-rules.json");var f=TotalWarRules.Parse(text);
        void Reject(string json){bool bad=false;try{TotalWarRules.Parse(json);}catch{bad=true;}Check(bad);}
        Reject(text.Replace("\"WarNumber\": 3","\"WarNumber\": 10"));Reject(text.Replace("\"WarNumber\": 6","\"WarNumber\": 3"));
        Reject(text.Replace("\"MaximumTurn\": 24","\"MaximumTurn\": 13"));Reject(text.Replace("\"MaximumTurn\": 18","\"MaximumTurn\": 41"));
        Reject(text.Replace("\"Objective\": \"EligibleElimination\"","\"Objective\": \"CommanderDefeat\""));
        Reject(text.Replace("\"Style\": \"Tactical\"","\"Style\": \"900\""));Reject(text.Replace("\"AttackBonus\": 3","\"AttackBonus\": -1"));
        Reject(text.Replace("\"ObjectiveWeight\": 2","\"ObjectiveWeight\": 13"));Reject(text.Replace("\"EnemyAi\": true","\"EnmeyAi\": true"));
        Reject(text.Replace("\"SchemaVersion\": 1","\"SchemaVersion\": 2"));Reject(text.Replace("\"NameId\": 143","\"NameId\": 999"));
        List<WarUnitState> Fresh()=>Enumerable.Range(0,2).SelectMany(side=>Enumerable.Range(0,15).Select(i=>new WarUnitState(side,i,i==0?1:0,side==0?4:12,side==0?8:2))).ToList();
        foreach(var r in f.Scenarios)
        {
            var audit=TotalWarRules.Audits.Single(a=>a.War==r.WarNumber);
            for(int war=0;war<=17;war++)for(int turn=0;turn<=45;turn++)
            {
                var u=Fresh();var result=TotalWarRules.Decide(r,true,true,war,turn,u);
                if(war!=r.WarNumber||turn==0)Check(result==CampaignDecision.Vanilla);
                else if(turn<r.MaximumTurn)Check(result==CampaignDecision.Continue);
                else Check(result==(war==13?CampaignDecision.HoldCompleted:CampaignDecision.Withdrawal));
                Check(TotalWarRules.Decide(r,false,true,war,turn,u)==CampaignDecision.Vanilla);
                Check(TotalWarRules.Decide(r,true,false,war,turn,u)==CampaignDecision.Vanilla);
            }
            var roster=Fresh();roster[0]=roster[0] with{Flag=2};Check(TotalWarRules.Decide(r,true,true,r.WarNumber,2,roster)==CampaignDecision.Vanilla);
            roster=Fresh();roster[15]=roster[15] with{Flag=2};Check(TotalWarRules.Decide(r,true,true,r.WarNumber,2,roster)==CampaignDecision.Vanilla);
            foreach(int slot in new[]{0,15}){roster=Fresh();roster[slot]=roster[slot] with{Flag=0};Check(TotalWarRules.Decide(r,true,true,r.WarNumber,30,roster)==CampaignDecision.Vanilla);}
            roster=Fresh();roster[10]=roster[0];Check(TotalWarRules.Decide(r,true,true,r.WarNumber,2,roster)==CampaignDecision.Vanilla);
            roster=Fresh();roster[15]=roster[15] with{X=16};Check(TotalWarRules.Decide(r,true,true,r.WarNumber,2,roster)==CampaignDecision.Vanilla);
            roster=Fresh();roster[15]=roster[15] with{Flag=3};Check(TotalWarRules.Decide(r,true,true,r.WarNumber,2,roster)==CampaignDecision.Vanilla);
            Check(TotalWarRules.Decide(r,true,true,r.WarNumber,2,Fresh().Take(29).ToArray())==CampaignDecision.Vanilla);
            r.Enabled=false;Check(TotalWarRules.Decide(r,true,true,r.WarNumber,30,Fresh())==CampaignDecision.Vanilla);r.Enabled=true;
        }
        var six=f.Scenarios.Single(r=>r.WarNumber==6);var army=Fresh();for(int i=16;i<=26;i++)army[i]=army[i] with{Flag=2};
        Check(TotalWarRules.Decide(six,true,true,6,8,army)==CampaignDecision.Continue);Check(TotalWarRules.Decide(six,true,true,6,9,army)==CampaignDecision.EligibleElimination);
        for(int i=16;i<=26;i++)
        {var copy=army.ToList();copy[i]=copy[i] with{Flag=0};Check(TotalWarRules.Decide(six,true,true,6,9,copy)==CampaignDecision.Continue);copy[i]=copy[i] with{Flag=1};Check(TotalWarRules.Decide(six,true,true,6,9,copy)==CampaignDecision.Continue);}
        army[0]=army[0] with{Flag=2};Check(TotalWarRules.Decide(six,true,true,6,9,army)==CampaignDecision.Vanilla);
        var aggressive=f.AiProfiles.Single(a=>a.NameId==145);var defensive=f.AiProfiles.Single(a=>a.NameId==88);var screen=f.AiProfiles.Single(a=>a.NameId==3369&&a.Branch=="Infantry");var archer=f.AiProfiles.Single(a=>a.NameId==3369&&a.Branch=="Archer");
        var actor=new AiUnit(0,8,5,0,0);var friends=new[]{actor,new AiUnit(1,10,5,0,1)};var enemies=new[]{new AiUnit(0,12,5,0,0)};
        int Score(WarAiProfile p,AiCandidate c,AiUnit? a=null)=>TotalWarRules.Score(0,p,a??actor,c,12,5,8,5,friends,enemies);
        Check(Score(aggressive,new(1,9,5,255,0,100,0))>Score(aggressive,new(1,7,5,255,0,100,0)));
        Check(Score(defensive,new(1,8,5,255,0,100,0))>Score(defensive,new(1,9,5,255,0,100,0)));
        Check(Score(screen,new(1,9,5,255,0,100,0))>Score(screen,new(1,7,5,255,0,100,0)));
        Check(Score(archer,new(2,8,5,0,0,100,10))>Score(archer,new(2,8,5,0,0,100,0)));
        Check(Score(aggressive,new(0,8,5,0,0,80,0))>Score(aggressive,new(0,8,5,1,0,80,0)));
        Check(Score(aggressive,new(0,8,5,1,0,40,0),actor with{Damage=1})<Score(aggressive,new(0,8,5,1,0,40,0)));
        foreach(var p in f.AiProfiles)foreach(int action in new[]{0,1,2,3})foreach(int score in new[]{-32768,-30000,-1,0,1,30000,32767})for(int x=0;x<16;x++)for(int y=0;y<12;y++)
        {
            var c=new AiCandidate(action,x,y,0,1,60,10);
            int v=TotalWarRules.Score(score,p,actor,c,12,5,8,5,friends,enemies);
            Check(action==3?v==score:v is >=-32768 and <=32767);
        }
        foreach(var bad in new[]{new AiCandidate(1,-1,5,0,0,100,0),new(1,16,5,0,0,100,0),new(0,8,5,15,0,80,0),new(0,8,5,0,0,101,0)})Check(TotalWarRules.Score(7,aggressive,actor,bad,12,5,8,5,friends,enemies)==7);
        var specs=ContentRules.Read<CommanderFile>(Resource("war-specializations.json"));Check(OverhaulRules.Commanders(Resource("war-specializations.json"),specs).Profiles.Length==9);
        Check(ContentRules.Read<CommanderFile>(Resource("war-unit-profiles.json")).Profiles.Length==21);
        Check(TotalWarRules.Audits.All(a=>a.War is 3 or 6 or 13));
        Console.WriteLine($"PASS: Total War {n} assertions; original regressions retained; scoped objectives, eligible elimination, malformed policies, commander strategies and score bounds. Native and gameplay evidence separate.");
    }
}
