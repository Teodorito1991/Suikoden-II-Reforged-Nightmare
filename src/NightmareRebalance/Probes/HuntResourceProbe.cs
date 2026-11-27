using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
namespace NightmareRebalance;
// Exact keys verified in this installation's original Addressables catalog.
internal static class HuntResourceProbe
{
 static string[]? keys;
 static readonly Dictionary<string,string> atlases=new();
 static readonly List<AsyncOperationHandle<SpriteAtlas>> retained=new();
 static AsyncOperationHandle<SpriteAtlas>? pending;static int index;
 internal static int Assertions;internal static bool Passed;internal static readonly List<object> Rows=new();
 internal static void Record(string monster,OPD_DATA opd)
 {
  if(opd.path is not ("battle/monster" or "battle/monster_t")||string.IsNullOrWhiteSpace(opd.id))
   throw new InvalidOperationException("Missing original monster OPD identity: "+monster);
  string key="gsd2-classic-opd-battle-monster-"+opd.id+"_atlas";
  if(atlases.TryGetValue(monster,out var other)&&other!=key)
   throw new InvalidOperationException("Conflicting original atlas identities: "+monster);
  atlases[monster]=key;
 }
 internal static bool Step()
 {
  keys??=HuntRuntime.Definitions.SelectMany(h=>h.Roster).Distinct().Select(id=>atlases[id]).Distinct().ToArray();
  if(index==keys.Length){Passed=true;return true;}
  if(pending==null){pending=Addressables.LoadAssetAsync<SpriteAtlas>((Il2CppSystem.String)keys[index]);return false;}
  if(!pending.IsDone)return false;
  Assertions++;if(pending.Status!=AsyncOperationStatus.Succeeded||pending.Result==null)throw new InvalidOperationException("Hunt original sprite atlas could not load: "+keys[index]);
  var atlas=pending.Result;var sprites=new Il2CppReferenceArray<Sprite>(atlas.spriteCount);int count=atlas.GetSprites(sprites);
  Assertions++;if(count<=0||count!=atlas.spriteCount||sprites.Any(s=>s==null||s.texture==null||s.rect.width<=0||s.rect.height<=0))throw new InvalidOperationException("Hunt native atlas has missing sprites/textures: "+keys[index]);
  Rows.Add(new{Key=keys[index],Sprites=count,NativeAtlasLoaded=true,NativeSpritesRetrieved=true,RenderedInBattle=false,AnimationPlayback=false});
  retained.Add(pending);pending=null;index++;return false;
 }
 internal static object Snapshot()=>new{Passed,Assertions,Rows=Rows.ToArray(),RenderedBattle=false,AnimationPlayback=false};
}
