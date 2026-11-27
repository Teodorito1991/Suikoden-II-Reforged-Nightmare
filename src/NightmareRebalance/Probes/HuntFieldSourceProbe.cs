using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace NightmareRebalance;
// Read original field records; no MapChapter / EVENTCON is created or invoked.
internal static class HuntFieldSourceProbe
{
 static int index;static AsyncOperationHandle<MACHIDAT>? pending;
 static readonly List<AsyncOperationHandle<MACHIDAT>> retained=new();
 internal static int Assertions;internal static bool Passed;internal static readonly List<object> Rows=new();
 static void Check(bool ok,string label){Assertions++;if(!ok)throw new InvalidOperationException("Hunt field source: "+label);}
 internal static bool Step()
 {
  if(index==HuntRuntime.Definitions.Length){Passed=true;return true;}
  var d=HuntRuntime.Definitions[index];string field=d.FieldId;string key="gsd2-classic-machi-"+field+"-"+field;
  if(pending==null){pending=Addressables.LoadAssetAsync<MACHIDAT>((Il2CppSystem.String)key);return false;}
  if(!pending.IsDone)return false;
  Check(pending.Status==AsyncOperationStatus.Succeeded&&pending.Result!=null,"Original MACHIDAT loads "+key);var m=pending.Result!;
  Check(m.battle_dat!=null&&m.eventdata?.mapeventdat!=null,"Field owns native battle and event records");
  var maps=new List<object>();var eligible=new List<int>();
  for(int variant=0;variant<m.eventdata!.mapeventdat!.Count;variant++){
   var e=m.eventdata.mapeventdat[variant];
   if(e.batlno==0)continue;
   Check(e.batlno<m.battle_dat!.Length,"Walking table index in native array");var source=m.battle_dat![e.batlno];
   if(d.WalkingVariants.Length>0&&(source?.id!=d.Audit.TableId||e.batltim==0||e.batlpro==0))continue;
   Check(source?.id==d.Audit.TableId&&HuntAccess.SourceMatches(source,d),"Walking map links exact source roster and signatures");
   eligible.Add(variant);
   maps.Add(new{Variant=variant,Map=e.mapno,TableIndex=e.batlno,NativeInterval=e.batltim,NativeProbability=e.batlpro});
  }
  Check(maps.Count>0,"At least one legitimate walking-encounter map");
  Check(d.WalkingVariants.Length==0||eligible.SequenceEqual(d.WalkingVariants),"Exact audited walking variants");
  Rows.Add(new{d.Id,AssetKey=key,FieldNameId=m.msdat.name,SaveLocation=Location(field),WalkingMaps=maps.ToArray(),OriginalFieldRecordLoaded=true,FieldSceneEntered=false,EventCallbacksExecuted=false});
  retained.Add(pending);pending=null;index++;return false;
 }
 // Read the original map-ID registry before querying its save-screen name SID.
 // No map initialization or scene load occurs here.
 internal static (int Area,int Town)? FindLocation(string field)
 {
  var ids=MachiLoader.GetMachiIDList();var found=new List<(int A,int V)>();
  if(ids==null||ids.Count>64)return null;
  for(int a=0;a<ids.Count;a++){
   if(ids[a]==null||ids[a].Count>256)return null;
   for(int v=0;v<ids[a].Count;v++)if(ids[a][v]==field)found.Add((a,v));
  }
  if(found.Count!=1)return null;
  var location=found[0];if(MachiLoader.GetMachiID(location.A,location.V)!=field)return null;
  return location;
 }
 static object Location(string field)
 {
  if(FindLocation(field) is not {} location)return new{Verified=false};
  return new{Verified=true,location.Area,location.Town,NameSid=savedata_c.GetMachiNameSID(location.Area,location.Town)};
 }
 internal static object Snapshot()=>new{Passed,Assertions,Rows=Rows.ToArray(),FieldSceneEntered=false};
}
