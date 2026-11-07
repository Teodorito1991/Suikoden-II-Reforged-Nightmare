using NightmareRebalance;
using System.Text.Json;
internal static class WarRevolutionTests
{
    internal static void Run()
    {
        int n=0;void Check(bool v){n++;if(!v)throw new Exception("War revolution assertion "+n);}
        var r=new WarPolicy{WarNumber=2,Enabled=true,PlayerCommands=true,CommanderVictory=true,MaximumTurn=20,EnemyAi=true,WoundedTargetBonus=6,PoorOddsPenalty=4};
        var file=new WarPolicyFile{Wars=new[]{r}};string json=JsonSerializer.Serialize(file);Check(WarPolicyRules.Parse(json).Wars[0].MaximumTurn==20);
        void Reject(string text){bool rejected=false;try{WarPolicyRules.Parse(text);}catch{rejected=true;}Check(rejected);}
        Reject(json.Replace("\"WarNumber\":2","\"WarNumber\":3"));Reject(json.Replace("\"MaximumTurn\":20","\"MaximumTurn\":5"));Reject(json.Replace("\"MaximumTurn\":20","\"MaximumTurn\":61"));Reject(json.Replace("\"WoundedTargetBonus\":6","\"WoundedTargetBonus\":21"));Reject(json.Replace("\"Enabled\":true","\"Enabeld\":true"));
        List<WarUnitState> Fresh()=>Enumerable.Range(0,2).SelectMany(f=>Enumerable.Range(0,15).Select(i=>new WarUnitState(f,i,i==0?1:0,f==0?4:9,f==0?7:2))).ToList();
        var units=Fresh();War2Outcome Decide(int war=2,long turn=2,bool enable=true,bool scope=true)=>WarPolicyRules.Decide(r,enable,scope,war,turn,units);
        Check(Decide()==War2Outcome.Continue);units[15]=units[15] with{Flag=2};Check(Decide()==War2Outcome.TacticalVictory);Check(Decide(turn:20)==War2Outcome.TacticalVictory);
        units[0]=units[0] with{Flag=2};Check(Decide()==War2Outcome.AlliedDefeat);
        units=Fresh();units[16]=new(1,1,1,3,6);units[15]=units[15] with{Flag=2};Check(Decide()==War2Outcome.ProtectedAreaBreached);
        units=Fresh();Check(Decide(turn:19)==War2Outcome.Continue);Check(Decide(turn:20)==War2Outcome.OrderlyWithdrawal);
        r.CommanderVictory=false;units[15]=units[15] with{Flag=2};Check(Decide()==War2Outcome.Continue);Check(Decide(turn:20)==War2Outcome.OrderlyWithdrawal);r.CommanderVictory=true;
        for(int war=0;war<=17;war++)for(int turn=0;turn<=61;turn++)
        {
            units=Fresh();var expected=war!=2||turn<1?War2Outcome.Vanilla:turn<20?War2Outcome.Continue:War2Outcome.OrderlyWithdrawal;
            Check(Decide(war,turn)==expected);Check(Decide(war,turn,false)==War2Outcome.Vanilla);Check(Decide(war,turn,true,false)==War2Outcome.Vanilla);
        }
        for(int x=0;x<=15;x++)for(int y=0;y<=11;y++)
        {units=Fresh();units[15]=units[15] with{X=x,Y=y};Check(Decide()==(x is >=3 and <=5&&y is >=6 and <=8?War2Outcome.ProtectedAreaBreached:War2Outcome.Continue));}
        units=Fresh();units.RemoveAt(1);Check(Decide()==War2Outcome.Vanilla);units=Fresh();units[1]=units[0];Check(Decide()==War2Outcome.Vanilla);units=Fresh();units[0]=units[0] with{Flag=3};Check(Decide()==War2Outcome.Vanilla);units=Fresh();units[0]=units[0] with{X=16};Check(Decide()==War2Outcome.Vanilla);
        for(int score=-20;score<=30;score++)for(int chance=0;chance<=100;chance++)
        {
            Check(WarPolicyRules.AiScore(score,0,1,1,2,chance,6,4)==score+6-(chance<=10?4:0));
            Check(WarPolicyRules.AiScore(score,0,1,0,2,chance,6,4)==score-(chance<=10?4:0));
            Check(WarPolicyRules.AiScore(score,1,1,1,2,chance,6,4)==score);
            Check(WarPolicyRules.AiScore(score,0,2,1,2,chance,6,4)==score);
        }
        Check(WarPolicyRules.AiScore(int.MaxValue,0,1,1,2,50,6,4)==30000);Check(WarPolicyRules.AiScore(int.MaxValue,0,1,1,2,50,0,0)==int.MaxValue);
        Check(WarPolicyRules.AiScore(7,0,1,2,2,50,6,4)==7);
        Console.WriteLine($"PASS: War Revolution {n} assertions; strict per-war configuration, outcomes, protected area, withdrawal bound, opt-outs and actual-action scoring policy. Native/UI verification separate.");
    }
}
