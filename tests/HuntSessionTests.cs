using NightmareRebalance;
using System.Text.Json;
internal static class HuntSessionTests
{
 internal static void Run()
 {
  int n=0;void Check(bool x,string s){n++;if(!x)throw new Exception("Hunt session "+s);}
  var first=new HuntRunProgress{Build="fixture",Entry=1};first.Prepare(3478);first.Credit(1,100,3578);
  Check(first.PreparedReward==0&&first.ActualCredit==0,"no victory no reward evidence");Check(!first.FieldResume(),"field update before completion ignored");
  first.DebugArmed(true);first.Victory();first.Prepare(3478);first.Prepare(3478);first.Credit(1,85269,88747);first.Finish("VictoryRewardsCompleted",2);
  Check(first.ActualCredit==3478&&first.NativeCreditCalls==1,"one actual credit");Check(first.PreparedReward==3478,"prepared independent from credit");Check(first.DebugArmedDuringRun&&first.ProtectedHits==0,"armed but no blocked hit still marked debug run");
  Check(first.RewardsCompleted&&first.RuneChargesRestored==2,"real completion input");Check(!first.LifecycleObserved,"not complete lifecycle before map return");Check(first.FieldResume()&&first.LifecycleObserved,"map resume tracked after completion");Check(!first.PlayerControlVerified&&!first.BalanceVerified,"map tick never claims human control/balance");Check(!first.FieldResume(),"repeat map tick ignored");
  first.Credit(1,88747,92225);first.Finish("NonVictorySignal",0);first.Block(99);Check(first.ActualCredit==3478&&first.Outcome=="VictoryRewardsCompleted"&&first.ProtectedHits==0,"completed run immutable to late callbacks");
  var second=new HuntRunProgress{Entry=2};Check(second.RunId!=first.RunId&&!second.VictorySignal&&second.ActualCredit==0,"repeat run clean identity and reward gate");second.Victory();second.Prepare(3478);second.Credit(1,88747,92225);second.Finish("VictoryRewardsCompleted",1);second.FieldResume();Check(second.ActualCredit==3478&&first.ActualCredit==3478,"two independent victories, not compound bonus");
  var escape=new HuntRunProgress();escape.Credit(1,0,3478);escape.Finish("NonVictorySignal",4);escape.FieldResume();Check(!escape.RewardsCompleted&&escape.ActualCredit==0&&escape.RuneChargesRestored==0&&!escape.LifecycleObserved,"escape cannot become a victory");
  var capped=new HuntRunProgress();capped.Victory();capped.Prepare(3478);capped.Credit(0,999000,999999);capped.Credit(1,999000,999999);Check(capped.ActualCredit==999&&capped.NativeCreditCalls==1,"native cap and readmode distinguished");capped.Credit(1,999999,999000);Check(capped.ActualCredit==999,"opposing mutation not fabricated as award");
  var debug=new HuntRunProgress();debug.DebugArmed(false);debug.Block(-1);Check(!debug.DebugArmedDuringRun,"no false debug flag");debug.Block(25);debug.DebugArmed(false);Check(debug.ProtectedHits==1&&debug.PreventedDamage==25&&debug.DebugArmedDuringRun,"debug attribution retained after toggle off");
  string directory=Path.Combine(Path.GetTempPath(),"reforged-v16-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);string path=Path.Combine(directory,"hunt-history.json");
  try{DiagnosticHistoryStore.Append(path,JsonSerializer.Serialize(first));DiagnosticHistoryStore.Append(path,JsonSerializer.Serialize(second));using var doc=JsonDocument.Parse(File.ReadAllText(path));Check(doc.RootElement.GetProperty("Battles").GetArrayLength()==2,"completed hunt records durable");Check(doc.RootElement.GetProperty("Battles")[0].GetProperty("ActualCredit").GetInt64()==3478,"first payout retained after repeat");File.WriteAllText(path,"invalid");bool failed=false;try{DiagnosticHistoryStore.Append(path,JsonSerializer.Serialize(escape));}catch{failed=true;}Check(failed&&File.ReadAllText(path)=="invalid","bad existing journal retained");}finally{File.Delete(path);if(File.Exists(path+".tmp"))File.Delete(path+".tmp");Directory.Delete(directory);}
  Console.WriteLine($"PASS: Hunt Session {n} assertions; repeat lifecycle, actual-credit evidence, escape, native cap, debug attribution and retained journal. Managed fixtures are not gameplay.");
 }
}
