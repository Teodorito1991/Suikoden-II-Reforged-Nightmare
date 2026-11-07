using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace NightmareRebalance;
// Original local assets only; never change or destroy game-owned font/texture data.
internal static class GuildVisualResources
{
 internal const string FrameKey="gsd2-hd-ui-common-texture-ui_com_frame001_001",CursorKey="gsd2-hd-window-arrow_1",FontKey="Font/CALLYB__/CALLYB__";
 static AsyncOperationHandle<Sprite>? frame,cursor;static AsyncOperationHandle<Font>? font;
 internal static Sprite? Frame,Cursor;internal static Font? Font;internal static bool Done,Ready,Failed;internal static int Loads,Releases;
 internal static bool Poll()
 {
  if(Done)return true;
  try{
   if(frame==null){Plugin.Logger.LogInfo("Guild UI: requesting original frame");frame=Addressables.LoadAssetAsync<Sprite>((Il2CppSystem.String)FrameKey);Plugin.Logger.LogInfo("Guild UI: requesting original cursor");cursor=Addressables.LoadAssetAsync<Sprite>((Il2CppSystem.String)CursorKey);Plugin.Logger.LogInfo("Guild UI: requesting original font");font=Addressables.LoadAssetAsync<Font>((Il2CppSystem.String)FontKey);Loads++;Plugin.Logger.LogInfo("Guild UI: original resource requests queued");return false;}
   if(!frame.IsDone||!cursor!.IsDone||!font!.IsDone)return false;
   if(frame.Status!=AsyncOperationStatus.Succeeded||cursor.Status!=AsyncOperationStatus.Succeeded||font.Status!=AsyncOperationStatus.Succeeded)throw new InvalidOperationException("Original Guild UI resources unavailable");
   var f=frame.Result;var c=cursor.Result;var t=font.Result;
   if(f==null||f.name!="UI_Com_Frame001_001"||f.texture==null||f.rect.width!=56||f.rect.height!=56||f.border.x!=27||f.border.y!=24||f.border.z!=27||f.border.w!=30||c==null||c.name!="arrow_1"||c.texture==null||c.rect.width!=72||c.rect.height!=64||t==null||t.name!="CALLYB__")throw new InvalidOperationException("Original Guild UI resource signature rejected");
   Frame=f;Cursor=c;Font=t;Ready=true;Done=true;return true;
  }catch(Exception e){Failed=true;Done=true;Plugin.Logger.LogWarning("Guild UI uses safe styled fallback: "+e.Message);return true;}
 }
 internal static void Release()
 {
  // Drop borrowed references before releasing only our own Addressables handles.
  bool owned=frame!=null||cursor!=null||font!=null;Frame=null;Cursor=null;Font=null;Ready=false;Done=false;Failed=false;
  if(frame!=null){Addressables.Release(frame);frame=null;}if(cursor!=null){Addressables.Release(cursor);cursor=null;}if(font!=null){Addressables.Release(font);font=null;}if(owned)Releases++;
 }
 internal static object Snapshot()=>new{FrameKey,CursorKey,FontKey,Done,Ready,Failed,Loads,Releases,Frame=Frame?.name,Cursor=Cursor?.name,Font=Font?.name,OriginalAssetsEdited=false};
}
internal static class GuildVisualNativeProbe
{
 internal static int Assertions;internal static bool Passed;static bool reloaded;
 static void Check(bool ok,string message){Assertions++;if(!ok)throw new InvalidOperationException("Guild visual: "+message);}
 internal static bool Step(){if(Passed)return true;if(!GuildVisualResources.Poll())return false;Check(GuildVisualResources.Ready,"original native frame/cursor/font signatures");Check(GuildVisualResources.Font!.HasCharacter('A')&&GuildVisualResources.Font.HasCharacter('g')&&GuildVisualResources.Font.HasCharacter('0'),"native font English glyphs");Check(GuildVisualResources.Frame!.texture.width==56&&GuildVisualResources.Cursor!.texture.width==72,"unpacked original textures");if(!reloaded){GuildVisualResources.Release();Check(!GuildVisualResources.Ready&&GuildVisualResources.Frame==null&&GuildVisualResources.Font==null,"drop references and release owned handles");reloaded=true;return false;}Passed=true;Plugin.Logger.LogInfo("Guild visual resources PASS: original frame/cursor/font loaded, released and reloaded; no rendered field menu claimed.");return true;}
 internal static object Snapshot()=>new{Assertions,Passed,Resources=GuildVisualResources.Snapshot(),RenderedInField=false,HeadlessSkipped=Application.isBatchMode,VerificationPath="Explicit graphics render smoke; HD UI bundles are not loaded in nographics"};
}
