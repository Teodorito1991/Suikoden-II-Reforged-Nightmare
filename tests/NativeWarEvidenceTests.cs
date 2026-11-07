using NightmareRebalance;
using System.Text.Json;
internal static class NativeWarEvidenceTests
{
 internal static void Run(){int n=0;void Check(bool x,string m){n++;if(!x)throw new Exception("War evidence "+m);}var frame=new NativeWarFrame(10,"hash",25,1,2,3);var e=new NativeWarEvidence{War=2,Entry=frame};
  Check(!e.IndependentSkirmishVerified&&!e.TrainingAwardIssued,"Evidence is never a training receipt");Check(!e.MapTick(frame),"Field ticks without destroyed chapter ignored");e.ChapterDestroyed=true;
  Check(!e.MapTick(frame with{Owner=11})&&e.MapUpdates==0,"Different owner excluded");for(int i=0;i<29;i++)Check(!e.MapTick(frame),"Bounded pending map evidence");Check(e.MapTick(frame)&&e.MapUpdates==30,"One terminal map threshold");Check(!e.MapTick(frame)&&e.MapUpdates==30,"No duplicate terminal threshold");
  for(int i=0;i<90;i++)e.Event("Fixture",frame,frame);Check(e.Events.Count==64,"Bounded event rows");Check(!e.IndependentSkirmishVerified&&!e.TrainingAwardIssued,"Complete-looking witness still no skirmish or XP proof");
  e=new(){Entry=frame,ChapterDestroyed=true,Invalidated="Load"};Check(!e.MapTick(frame),"Load invalidates return association");
  foreach(int age in new[]{-1,0,1,3600,3601})Check(NativeWarEvidence.RecentRequest(10,2,100,10,2,100+age)==(age>=0&&age<=3600),"Event observation freshness");Check(!NativeWarEvidence.RecentRequest(10,2,100,11,2,101),"Request owner guard");Check(!NativeWarEvidence.RecentRequest(10,2,100,10,3,101),"Request scenario guard");
  Check(NativeWarEvidenceRules.Parse("{\"SchemaVersion\":1,\"Enabled\":false}").Enabled==false,"Independent diagnostic opt-out");bool bad=false;try{NativeWarEvidenceRules.Parse("{\"SchemaVersion\":2}");}catch(JsonException){bad=true;}Check(bad,"Unknown schema rejected");
  var incomplete=new NativeWarEvidence();incomplete.Event("Unknown",frame with{FlagsHash=null},frame);string text=JsonSerializer.Serialize(incomplete);Check(text.Contains("\"FlagsComparable\":false")&&text.Contains("\"FlagBytesChanged\":null"),"Missing flags not reported unchanged");
  Console.WriteLine($"PASS: Native war lifecycle evidence {n} assertions; bounded read-only witness, owner/return gaps, no XP/skirmish acceptance inference.");
 }
}
