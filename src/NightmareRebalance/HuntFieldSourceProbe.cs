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
  Rows.Add(new{d.Id,AssetKey=key,FieldNameId=m.msdat.name,WalkingMaps=maps.ToArray(),OriginalFieldRecordLoaded=true,FieldSceneEntered=false,EventCallbacksExecuted=false});
  retained.Add(pending);pending=null;index++;return false;
 }
 internal static object Snapshot()=>new{Passed,Assertions,Rows=Rows.ToArray(),FieldSceneEntered=false};
}
