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
  var d=HuntRuntime.Definitions[index];string field=d.Audit.TableId.Split('_')[0];string key="gsd2-classic-machi-"+field+"-"+field;
  if(pending==null){pending=Addressables.LoadAssetAsync<MACHIDAT>((Il2CppSystem.String)key);return false;}
  if(!pending.IsDone)return false;
  Check(pending.Status==AsyncOperationStatus.Succeeded&&pending.Result!=null,"Original MACHIDAT loads "+key);var m=pending.Result!;
  Check(m.battle_dat!=null&&m.eventdata?.mapeventdat!=null,"Field owns native battle and event records");
  var maps=new List<object>();foreach(var e in m.eventdata!.mapeventdat!){
   if(e.batlno==0)continue;
   Check(e.batlno<m.battle_dat!.Length,"Walking table index in native array");var source=m.battle_dat![e.batlno];
   Check(source?.id==d.Audit.TableId&&HuntAccess.SourceMatches(source,d),"Walking map links exact source roster and signatures");
   maps.Add(new{Map=e.mapno,TableIndex=e.batlno,NativeInterval=e.batltim,NativeProbability=e.batlpro});
  }
  Check(maps.Count>0,"At least one legitimate walking-encounter map");
  Rows.Add(new{d.Id,AssetKey=key,FieldNameId=m.msdat.name,SaveLocation=Location(field),WalkingMaps=maps.ToArray(),OriginalFieldRecordLoaded=true,FieldSceneEntered=false,EventCallbacksExecuted=false});
  retained.Add(pending);pending=null;index++;return false;
 }
 // Read the original map-ID registry before querying its save-screen name SID.
 // No map initialization or scene load occurs here.
 static object Location(string field)
 {
  var ids=MachiLoader.GetMachiIDList();var found=new List<(int A,int V)>();
  if(ids==null||ids.Count>64)return new{Verified=false};
  for(int a=0;a<ids.Count;a++){
   if(ids[a]==null||ids[a].Count>256)return new{Verified=false};
   for(int v=0;v<ids[a].Count;v++)if(ids[a][v]==field)found.Add((a,v));
  }
  if(found.Count!=1)return new{Verified=false};
  var location=found[0];if(MachiLoader.GetMachiID(location.A,location.V)!=field)return new{Verified=false};
  return new{Verified=true,Area=location.A,Town=location.V,NameSid=savedata_c.GetMachiNameSID(location.A,location.V)};
 }
 internal static object Snapshot()=>new{Passed,Assertions,Rows=Rows.ToArray(),FieldSceneEntered=false};
}
