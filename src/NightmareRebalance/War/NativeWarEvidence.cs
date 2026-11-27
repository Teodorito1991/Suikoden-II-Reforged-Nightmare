using System.Text.Json;
namespace NightmareRebalance;
internal sealed record NativeWarFrame(long Owner,string? FlagsHash,int FlagBytes,int? Area,int? Town,int? Map);
internal sealed class NativeWarEvidence
{
 public string Session{get;}=Guid.NewGuid().ToString("N");public int War{get;set;}public bool OriginalEventObserved{get;set;}
 public bool BattleInitialized{get;set;}public int? Ending{get;set;}public bool EndingStarted{get;set;}public bool EndingFinished{get;set;}public bool FinishCallbackReturned{get;set;}public bool ChapterDestroyed{get;set;}public int MapUpdates{get;set;}public int SaveRequests{get;set;}
 public string? Invalidated{get;set;}public NativeWarFrame? Entry{get;set;}public List<object> Events{get;}=new();
 public bool IndependentSkirmishVerified=>false;public bool TrainingAwardIssued=>false;
 internal void Event(string stage,NativeWarFrame? before,NativeWarFrame? after,int? value=null)
 {if(Events.Count>=64)return;bool sameOwner=before!=null&&after!=null&&before.Owner!=0&&before.Owner==after.Owner;bool observed=sameOwner&&before!.FlagsHash!=null&&after!.FlagsHash!=null&&before.FlagBytes==after.FlagBytes;
  Events.Add(new{Stage=stage,Value=value,Before=before,After=after,SameOwner=sameOwner,FlagsComparable=observed,FlagBytesChanged=observed?before!.FlagsHash!=after!.FlagsHash:(bool?)null,UnchangedHashIsNotFullCampaignIsolation=true});}
 internal bool MapTick(NativeWarFrame? frame){if(!ChapterDestroyed||Invalidated!=null||MapUpdates>=30||frame==null||Entry==null||frame.Owner!=Entry.Owner)return false;MapUpdates++;return MapUpdates==30;}
 internal static bool RecentRequest(long requestOwner,int requestWar,int requestFrame,long owner,int war,int frame)=>owner!=0&&owner==requestOwner&&war is >=1 and <=16&&war==requestWar&&frame>=requestFrame&&(long)frame-requestFrame<=3600;
}
internal sealed class NativeWarWitnessFile{public int SchemaVersion{get;set;}=1;public bool Enabled{get;set;}=true;}
internal static class NativeWarEvidenceRules
{
 internal static NativeWarWitnessFile Parse(string text){if(text.Length>4096)throw new JsonException("War witness configuration exceeds4KiB.");using var d=JsonDocument.Parse(text);OverhaulRules.Keys(d.RootElement,"SchemaVersion","Enabled");var c=ContentRules.Read<NativeWarWitnessFile>(text);if(c.SchemaVersion!=1)throw new JsonException("Unsupported war witness schema.");return c;}
}
