using HarmonyLib;
using ShareUI.Menu;
using System.Text.Json;
namespace NightmareRebalance;
internal static class HuntGuildRuntime
{
 internal static HuntGuildFile Config=new(){EnableSelector=false,EnableVictoryItems=false};internal static GuildItemAudit[] Items=Array.Empty<GuildItemAudit>();
 internal static string? SessionSelection;internal static string SelectedId=>SessionSelection??HuntRuntime.Catalogue.SelectedHuntId;
 internal static UIMainMenu? Menu;internal static MACHICON? CommandOwner;internal static IntPtr PartyOwner;internal static int NativeMenuTicks, SuppressedTicks;internal static bool MenuProven,Visible;internal static int Changes;internal static string Message="Select a contract; activation remains separate.";
 internal static bool Preview=>RenderSmoke||(!UnityEngine.Application.isBatchMode&&Environment.GetCommandLineArgs().Contains("--reforged-guild-preview"));
 internal static string? PreviewId;
 internal static bool RenderFaulted;internal static int LastRenderFrame=-1,RenderCalls;
 const string CaptureArgument="--reforged-guild-capture=";
 internal static readonly string? CaptureDirectory=GetCaptureDirectory();
 internal static bool RenderSmoke=>CaptureDirectory!=null&&!UnityEngine.Application.isBatchMode;
 static string? GetCaptureDirectory()
 {
  var args=Environment.GetCommandLineArgs();
  if(!args.Contains("--reforged-guild-render-smoke")||!args.Contains("--reforged-private-startup-smoke")||
      !File.Exists(Path.Combine(BepInEx.Paths.GameRootPath,"PRIVATE_DO_NOT_UPLOAD.txt")))return null;
  var capture=args.FirstOrDefault(a=>a.StartsWith(CaptureArgument,StringComparison.Ordinal));
  return capture!=null&&Guid.TryParseExact(capture[CaptureArgument.Length..],"N",out var id)
      ? Path.Combine(BepInEx.Paths.GameRootPath,"guild-previews",id.ToString("N")) : null;
 }
 internal static bool RenderReady=>!RenderFaulted&&LastRenderFrame>=0&&UnityEngine.Time.frameCount>=LastRenderFrame&&(long)UnityEngine.Time.frameCount-LastRenderFrame<=2;
 internal static void Drawn(){LastRenderFrame=UnityEngine.Time.frameCount;RenderCalls++;}
 internal static void RenderFailed(Exception e){RenderFaulted=true;Visible=false;LastRenderFrame=-1;Plugin.Logger.LogError("Guild rendering disabled; native input released: "+e);}

 static string PreferencePath=>Path.Combine(BepInEx.Paths.BepInExRootPath,"ReforgedNightmare","guild","selected-contract.json");
 internal static void Initialize()
 {
  try{Items=ContentRules.Read<GuildItemAudit[]>(ContentRuntime.Resource("guild-item-audit.json"));Config=HuntGuildRules.Parse(OverhaulRuntime.Load("hunt-guild.json"),HuntRuntime.Definitions,Items);
   try{if(!UnityEngine.Application.isBatchMode&&File.Exists(PreferencePath)&&new FileInfo(PreferencePath).Length<=4096)SessionSelection=HuntGuildRules.Preference(File.ReadAllText(PreferencePath),HuntRuntime.Definitions.Where(d=>HuntRuntime.Settings(d).Enabled).Select(d=>d.Id));}catch(Exception e){Plugin.Logger.LogWarning("Ignoring invalid contract preference: "+e.Message);}
  }catch(Exception e){Plugin.Logger.LogWarning("Hunt Guild configuration/preference unavailable: "+e.Message);Config.EnableSelector=false;Config.EnableVictoryItems=false;}
 }
 // Verified GSD2 fcommand.MCommandMain (19B0980): WindowManager.menuWindow,
 // COMCON.stp=2 handles the MAIN list. All waiting MAIN branches return 0.
 // Read the same list properties directly; never instantiate a game menu here.
 internal static bool Owned=>Menu is {} m&&WindowManager.instance?.menuWindow?.Pointer==m.Pointer&&PartyOwner!=IntPtr.Zero&&Framework.Singleton<GAME_WORK>.instance?.party_data?.Pointer==PartyOwner;
 internal static bool PartyMenuOpen=>Owned&&Menu?.menuCommon?.windowList is {} w&&(w.IsOpen||w.IsOpening);
 internal static bool Context=>CastleGuildRuntime.Context||MenuContext;
 internal static bool MenuContext=>Owned&&Menu?.menuCommon?.windowList is {} w&&CommandOwner?.comcon?.TryCast<fcommand.COMCON>() is {} c&&
  HuntGuildRules.CanSelect(Config.EnableSelector&&Plugin.Options.EnableDeveloperTools.Value,MenuProven,w.IsOpen,w.IsOpening||w.IsClosing,c.stp==2,DifficultyProvenance.Difficulty(out int d,out _)&&d==0,Combat.State!=null,WarRuntime.Active);
 internal static void Observe(MACHICON mcon)
 {
  NativeMenuTicks++;
  if(!HuntFieldContext.Active||Combat.State!=null||WarRuntime.Active||mcon?.comcon?.TryCast<fcommand.COMCON>()==null){Clear();return;}
  var menu=WindowManager.instance?.menuWindow;var party=Framework.Singleton<GAME_WORK>.instance?.party_data;
  if(menu is null||party is null){Clear();return;}
  if(Menu?.Pointer!=menu.Pointer||PartyOwner!=party.Pointer)Visible=false;
  Menu=menu;CommandOwner=mcon;PartyOwner=party.Pointer;MenuProven=true;
 }
 internal static void Clear(){Menu=null;CommandOwner=null;PartyOwner=IntPtr.Zero;MenuProven=false;Visible=false;LastRenderFrame=-1;}
 internal static void Closed(UIMainMenu menu){if(Menu?.Pointer==menu.Pointer)Clear();}
 internal static bool NativeInput(MACHICON mcon,ref int result)
 {
  if(CastleGuildRuntime.Modal)CastleGuildRuntime.Close(false); // Native party menu takes ownership; never retain a castle modal.
  Observe(mcon);
  if(!Visible||!MenuContext||!RenderReady)return true;
  SuppressedTicks++;result=0;return false;
 }
 internal static void Toggle(){LastRenderFrame=-1;if(RenderFaulted){Visible=false;Message="Guild renderer unavailable; native controls retained.";return;}if(Preview){Visible=!Visible;return;}if(Context){Visible=!Visible;Message="Select a contract. Close this panel and the party menu, then F6 in its field.";}else{Visible=false;Message="Open the Normal-mode field party menu first.";Plugin.Logger.LogInfo("Hunt Guild: "+Message);}}
 internal static bool Select(string id)
 {
  if(!HuntRuntime.Definitions.Any(d=>d.Id==id&&HuntRuntime.Settings(d).Enabled))return false;
  if(Preview){PreviewId=id;Message="Preview only: no live selection or preference was changed.";return true;}
  if(!Context)return false;HuntAccess.Ticket.Cancel();SessionSelection=id;Changes++;HuntAccess.Status="Contract selected; F6 in its original field to arm.";Message="Contract selected. No battle requested.";
  if(!UnityEngine.Application.isBatchMode){try{Directory.CreateDirectory(Path.GetDirectoryName(PreferencePath)!);File.WriteAllText(PreferencePath+".tmp",JsonSerializer.Serialize(new{SchemaVersion=1,HuntId=id}));File.Move(PreferencePath+".tmp",PreferencePath,true);}catch(Exception e){Plugin.Logger.LogWarning("Contract selected for this session; preference not saved: "+e.Message);}}
  return true;
 }
 internal static void Requests(bool enabled){if(!Preview&&Context){HuntRuntime.Config.EnableExperimentalHunt=enabled;HuntAccess.Ticket.Cancel();Message="Hunt requests "+(enabled?"enabled for this session. F6 still required.":"disabled for this session.");}}
 internal static object Snapshot()=>new{Config.EnableSelector,Config.EnableVictoryItems,SelectedHuntId=SelectedId,SessionSelection,MenuProven,MenuOpen=PartyMenuOpen,RenderFaulted,RenderReady,RenderCalls,LastRenderFrame,NativeMenuTicks,SuppressedTicks,NativeOwnerMatched=Owned,Shortcut="F10",NativeTopStep=CommandOwner?.comcon?.TryCast<fcommand.COMCON>()?.stp,AppropriateNativeMenuContext=Context,Visible,Preview,PreviewId,Changes,Message,SelectionDoesNotArm=true,NativeNpcImplemented=true,VisualResources=GuildVisualResources.Snapshot(),Presentation="Native frame/cursor/English font; shared bounded layout",Items=GuildItemRuntime.Snapshot()};
}
[HarmonyPatch(typeof(UIMainMenu),nameof(UIMainMenu.Close))]
internal static class GuildNativeMenuClose{[HarmonyPrefix]static void Prefix(UIMainMenu __instance)=>HuntGuildRuntime.Closed(__instance);}
[HarmonyPatch(typeof(UIMainMenu),nameof(UIMainMenu.OnDestroy))]
internal static class GuildNativeMenuDestroy{[HarmonyPrefix]static void Prefix(UIMainMenu __instance)=>HuntGuildRuntime.Closed(__instance);}
[HarmonyPatch(typeof(fcommand),nameof(fcommand.MCommandMain))]
internal static class GuildNativeMenuInput
{
 [HarmonyPrefix]static bool Prefix(MACHICON mcon,ref int __result)
 {
  try{return HuntGuildRuntime.NativeInput(mcon,ref __result);}
  catch(Exception e){HuntGuildRuntime.Clear();Plugin.Debug("Guild context cleared: "+e.GetType().Name);return true;}
 }
}
