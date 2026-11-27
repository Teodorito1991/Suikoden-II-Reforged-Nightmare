namespace NightmareRebalance;

// Pure ordered receipt/cleanup state, never an engine outcome generator.
internal sealed class HallGauntletRules
{
    internal static readonly string[] LucaSources={"boss_luc1","boss_luc2","boss_luc3"};
    internal int Stage {get;private set;}
    internal int CarryHp {get;private set;}
    internal bool AwaitingTeardown {get;private set;}
    internal bool Completed {get;private set;}
    internal bool Failed {get;private set;}
    internal HallGauntletRules(int initialHp){if(initialHp is <1 or >32767)throw new ArgumentOutOfRangeException();CarryHp=initialHp;}
    internal bool NativeOutcome(string source,bool win,int actualHp,bool nativeHandoff=false)
    {
        if(Failed||Completed||AwaitingTeardown||source!=LucaSources[Stage]||actualHp is <0 or >32767)return false;
        if(!win&&!(nativeHandoff&&Stage<2&&actualHp>0)){Failed=true;return true;}
        CarryHp=actualHp;AwaitingTeardown=true;return true;
    }
    internal bool NativeDisposed(string source,bool fieldReturned,bool ownershipPreserved)
    {
        if(Failed||Completed||!AwaitingTeardown||source!=LucaSources[Stage])return false;
        if(!fieldReturned||!ownershipPreserved){Failed=true;return false;}
        AwaitingTeardown=false;
        if(Stage==2)Completed=true;else Stage++;
        return true;
    }
    internal bool BonusEligible=>Completed&&!Failed;
    internal static bool DistinctTeams(IReadOnlyList<int[]> teams)=>teams.Count==3&&teams.All(t=>t.Length==6)&&
        teams.SelectMany(t=>t).All(c=>c>0&&c<255)&&teams.SelectMany(t=>t).Distinct().Count()==18;
}
