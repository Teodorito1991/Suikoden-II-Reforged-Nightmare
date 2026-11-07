namespace NightmareRebalance;

internal enum NativeWarDecision { Vanilla, Continue, Victory, Defeat }
internal static class NativeWarRules
{
    // Only replace war 1's late timer branch. All earlier tutorial checks remain native.
    internal static NativeWarDecision Decide(bool enabled,bool active,int war,long turn,int enemyCommander,ReadOnlySpan<int> allies)
    {
        if(!enabled||!active||war!=1||turn<=3||allies.Length!=15||enemyCommander is <0 or >2)return NativeWarDecision.Vanilla;
        bool living=false,hadArmy=false;
        foreach(int flag in allies)
        {
            if(flag is <0 or >2)return NativeWarDecision.Vanilla;
            living|=flag==1;hadArmy|=flag!=0;
        }
        if(!hadArmy||enemyCommander==0)return NativeWarDecision.Vanilla; // Never infer an outcome from uninitialized state.
        if(!living)return NativeWarDecision.Defeat;
        return enemyCommander==2?NativeWarDecision.Victory:NativeWarDecision.Continue;
    }
}
