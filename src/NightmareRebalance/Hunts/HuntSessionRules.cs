namespace NightmareRebalance;
// Observational state only: no game methods, currency writes or fabricated outcomes.
internal sealed class HuntRunProgress
{
 public string HuntId{get;set;}="";public string TableId{get;set;}="";
 public string? PreparedItemId{get;private set;}public bool NativeItemHandoff{get;private set;}public int NativeItemTransferCalls{get;private set;}public int InventoryIncreaseObservations{get;private set;}
 internal void ItemPrepared(string id){if(!Finished&&VictorySignal)PreparedItemId=id;}
 internal void ItemHandoff(){if(!Finished&&VictorySignal)NativeItemHandoff=true;}
 internal void ItemTransfer(bool increased){if(!Finished&&VictorySignal){NativeItemTransferCalls++;if(increased)InventoryIncreaseObservations++;}}
 public string RunId{get;set;}=Guid.NewGuid().ToString("N");
 public string Build{get;set;}="";
 public int Entry{get;set;}
 public bool VictorySignal{get;private set;}
 public bool Finished{get;private set;}
 public bool RewardsCompleted{get;private set;}
 public bool FieldChapterResumed{get;private set;}
 public bool PlayerControlVerified=>false;
 public bool DebugArmedDuringRun{get;private set;}
 public int ProtectedHits{get;private set;}
 public long PreventedDamage{get;private set;}
 public int PreparedReward{get;private set;}
 public int NativeCreditCalls{get;private set;}
 public long ActualCredit{get;private set;}
 public uint? PurseBefore{get;private set;}
 public uint? PurseAfter{get;private set;}
 public int RuneChargesRestored{get;private set;}
 public string Outcome{get;private set;}="InProgress";
 public bool JournalWritten{get;set;}
 public bool LifecycleObserved=>VictorySignal&&RewardsCompleted&&NativeCreditCalls>0&&FieldChapterResumed;
 public bool BalanceVerified=>false;
 internal void Victory(){if(!Finished)VictorySignal=true;}
 internal void DebugArmed(bool armed){if(!Finished&&armed)DebugArmedDuringRun=true;}
 internal void Block(int damage){if(!Finished&&damage>0){DebugArmedDuringRun=true;ProtectedHits++;PreventedDamage+=damage;}}
 internal void Prepare(int value){if(!Finished&&VictorySignal&&value>=0)PreparedReward=value;}
 internal void Credit(int mode,uint before,uint after)
 {if(Finished||!VictorySignal||mode!=1||after<before)return;PurseBefore??=before;PurseAfter=after;ActualCredit+=(long)after-before;NativeCreditCalls++;}
 internal void Finish(string outcome,int charges)
 {if(Finished)return;Finished=true;Outcome=outcome;RewardsCompleted=VictorySignal&&outcome=="VictoryRewardsCompleted";RuneChargesRestored=RewardsCompleted?Math.Max(0,charges):0;}
 internal bool FieldResume(){if(!Finished||FieldChapterResumed)return false;FieldChapterResumed=true;return true;}
}
