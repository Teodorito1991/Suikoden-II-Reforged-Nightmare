using NightmareRebalance;

internal static class NativeWarTests
{
    internal static void Run()
    {
        int checks=0;void Check(bool value){checks++;if(!value)throw new Exception("Native war rule regression "+checks);}
        var live=new int[15];live[0]=1;
        Check(NativeWarRules.Decide(true,true,1,4,1,live)==NativeWarDecision.Continue);
        Check(NativeWarRules.Decide(true,true,1,4,2,live)==NativeWarDecision.Victory);
        live[0]=2;Check(NativeWarRules.Decide(true,true,1,4,1,live)==NativeWarDecision.Defeat);
        Check(NativeWarRules.Decide(true,true,1,4,2,live)==NativeWarDecision.Defeat);
        live[14]=1;Check(NativeWarRules.Decide(true,true,1,4,1,live)==NativeWarDecision.Continue);
        for(int war=-1;war<=18;war++)for(long turn=-1;turn<=50;turn++)for(int commander=0;commander<=2;commander++)
        {
            var expected=war!=1||turn<=3||commander==0?NativeWarDecision.Vanilla:commander==2?NativeWarDecision.Victory:NativeWarDecision.Continue;
            Check(NativeWarRules.Decide(true,true,war,turn,commander,live)==expected);
            Check(NativeWarRules.Decide(false,true,war,turn,commander,live)==NativeWarDecision.Vanilla);
            Check(NativeWarRules.Decide(true,false,war,turn,commander,live)==NativeWarDecision.Vanilla);
        }
        Check(NativeWarRules.Decide(true,true,1,50,1,new int[14])==NativeWarDecision.Vanilla);
        Check(NativeWarRules.Decide(true,true,1,50,1,new int[15])==NativeWarDecision.Vanilla);
        foreach(int bad in new[]{-1,3,255,int.MaxValue})
        {
            var invalid=(int[])live.Clone();invalid[3]=bad;
            Check(NativeWarRules.Decide(true,true,1,50,1,invalid)==NativeWarDecision.Vanilla);
            Check(NativeWarRules.Decide(true,true,1,50,bad,live)==NativeWarDecision.Vanilla);
        }
        // Every alive/routed/absent combination on five slots, including a survivor in the last slot.
        for(int mask=0;mask<243;mask++)
        {
            var army=new int[15];int digits=mask;
            for(int i=0;i<5;i++){army[i]=digits%3;digits/=3;}
            var expected=!army.Any(x=>x!=0)?NativeWarDecision.Vanilla:army.Contains(1)?NativeWarDecision.Continue:NativeWarDecision.Defeat;
            Check(NativeWarRules.Decide(true,true,1,100,1,army)==expected);
            var saved=(int[])army.Clone();NativeWarRules.Decide(true,true,1,100,2,army);Check(army.SequenceEqual(saved));
        }
        Console.WriteLine($"PASS: Native war remix {checks} assertions; exact war/turn scope, opt-out, native flag contract, tactical outcomes, malformed/uninitialized state. Native runtime probe and battlefield acceptance remain separate.");
    }
}
