using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace NightmareRebalance;
internal static class GuildNativeProbe
{
 static AsyncOperationHandle<IMPORT_GAME_DATA>? handle;internal static IMPORT_GAME_DATA? Data;
 internal static BATTLE_DATA? ReissueTable;
 internal static int Assertions,Inventories;internal static readonly List<object> Rows=new();internal static readonly HashSet<string> Hunts=new();
 static void Check(bool x,string label){Assertions++;if(!x)throw new InvalidOperationException("Guild native: "+label);}
 static T Alloc<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase{RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return(T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
 internal static bool Resources()
 {
  if(Data!=null)return true;
  if(handle==null){handle=Addressables.LoadAssetAsync<IMPORT_GAME_DATA>((Il2CppSystem.String)"gsd2-classic-gamedata-gamedata");return false;}
  if(!handle.IsDone)return false;Check(handle.Status==AsyncOperationStatus.Succeeded&&handle.Result!=null,"Original gamedata loads");Data=handle.Result!;
  foreach(var item in HuntGuildRuntime.Items)Check(GuildItemRuntime.Matches(Data.ovit_data,item),"Exact full native reward template "+item.Id+" actual="+GuildItemRuntime.Signature(Data.ovit_data,item));return true;
 }
 internal static void Hunt(BattleManager manager,BATTLE_WORK w,BattleState state,int noDropSeed)
 {
  if(Data==null||HuntRuntime.Active is not {} hunt)return;
  var game=Framework.Singleton<GAME_WORK>.instance;var oldOvit=game.ovit_data;var oldItems=game.party_data.party_item;var oldHave=game.have_data;
  bool enabled=HuntGuildRuntime.Config.EnableVictoryItems;var setting=HuntGuildRuntime.Config.Rewards.Single(x=>x.HuntId==hunt.Id);bool ruleEnabled=setting.Enabled;
  var exit=w.exit_work;bool snapshot=state.SnapshotTaken;string outcome=CombatReforgedDiagnostics.Outcome;var rng=UnityEngine.Random.state;
  try{
   game.ovit_data=Data.ovit_data;game.have_data=new Il2CppReferenceArray<HAVE_DATA>(176);for(int i=0;i<176;i++)game.have_data[i]=new();
   game.party_data.party_item=new Il2CppReferenceArray<PARTY_ITEM>(30);for(int i=0;i<30;i++)game.party_data.party_item[i]=new();
   var item=HuntGuildRuntime.Items.Single(x=>x.Id==setting.RewardId);HuntGuildRuntime.Config.EnableVictoryItems=true;setting.Enabled=true;GuildItemRuntime.Reset();w.exit_work=new EXIT_WORK{monster_item=new PARTY_ITEM()};
   state.SnapshotTaken=false;CombatReforgedDiagnostics.Outcome="ClosedWithoutVictorySignal";GuildItemRuntime.Prepare(w,w.exit_work);Check(w.exit_work.monster_item.item_no==0,"Non-victory cannot prepare item");
   state.SnapshotTaken=true;CombatReforgedDiagnostics.Outcome="VictorySignal";
   HuntRewardAvailabilityProbe.Run(manager,w);
   HuntGuildRuntime.Config.EnableVictoryItems=false;UnityEngine.Random.InitState(noDropSeed);manager.get_monster_okane(w);var baselineRng=UnityEngine.Random.state;
   HuntGuildRuntime.Config.EnableVictoryItems=true;UnityEngine.Random.InitState(noDropSeed);manager.get_monster_okane(w);var itemRng=UnityEngine.Random.state;
   Check(baselineRng.s0==itemRng.s0&&baselineRng.s1==itemRng.s1&&baselineRng.s2==itemRng.s2&&baselineRng.s3==itemRng.s3,"Native reward RNG unchanged by deterministic fallback item");
   Check(w.exit_work.monster_item.item_no==item.Number&&w.exit_work.monster_item.use_cnt==item.UseCount,"Production native reward calculation prepares audited fallback item "+hunt.Id);
   var nativeItem=G2_SYS.G2_data_get(game.ovit_data,item.Kind,item.Number);Check(nativeItem!=null&&GuildItemRuntime.MatchesText(GuildItemRuntime.ItemSignature(nativeItem,item),item),"Original native lookup resolves exact identity");
   // Original cache operation can hold constructor-created records with null editor-only strings.
   UNI_DATA copy=item.Kind==0?new DOGU_DATA(nativeItem!.dogu_data.name,nativeItem!.dogu_data.text,nativeItem!.dogu_data.info,nativeItem!.dogu_data.cnt,nativeItem!.dogu_data.eff_val,nativeItem!.dogu_data.atrb,nativeItem!.dogu_data.status,nativeItem!.dogu_data.botch):new EMBL_DATA(nativeItem!.embl_data.name,nativeItem!.embl_data.text,nativeItem!.embl_data.info,nativeItem!.embl_data.bonus,nativeItem!.embl_data.magic_no,nativeItem!.embl_data.botch);
   Check(GuildItemRuntime.MatchesText(GuildItemRuntime.ItemSignature(copy,item),item),"Original native constructor preserves reward identity; absent editor strings normalized only to empty");
   game.have_data[1]=new HAVE_DATA{kind=(byte)item.Kind,no=(byte)item.Number,union_data=copy};game.ovit_data=null;var cached=G2_SYS.G2_data_get(null,item.Kind,item.Number);
   Check(cached?.Pointer==copy.Pointer&&GuildItemRuntime.MatchesText(GuildItemRuntime.ItemSignature(cached,item),item),"Actual native HAVE-cache lookup resolves constructor-created audited record");game.ovit_data=Data.ovit_data;
   w.exit_work.monster_item.item_no=1;w.exit_work.monster_item.use_cnt=3;GuildItemRuntime.Prepare(w,w.exit_work);Check(w.exit_work.monster_item.item_no==1&&w.exit_work.monster_item.use_cnt==3,"Rolled native loot is never replaced");
   w.exit_work.monster_item.item_no=0;HuntGuildRuntime.Config.EnableVictoryItems=false;GuildItemRuntime.Prepare(w,w.exit_work);Check(w.exit_work.monster_item.item_no==0,"Independent item reward opt-out");HuntGuildRuntime.Config.EnableVictoryItems=true;
   GuildItemRuntime.PayoutStarted=true;GuildItemRuntime.Prepare(w,w.exit_work);Check(w.exit_work.monster_item.item_no==0,"No new item after payout starts");GuildItemRuntime.PayoutStarted=false;
   GuildItemRuntime.Prepare(w,w.exit_work);GuildItemRuntime.Prepare(w,w.exit_work);Check(w.exit_work.monster_item.item_no==item.Number&&game.party_data.party_item.All(x=>x.item_no==0),"Repeated preparation remains one pending slot, not inventory grants");
   // Same native inventory primitive/argument pattern as b_AddItemWinMain, on scratch inventory only.
   GuildItemRuntime.TransferScope=true;int result=G2_SYS.G2_min_item_move(game.ovit_data,nativeItem,0,0,item.Kind,item.Number,0,0);GuildItemRuntime.TransferScope=false;
   Check(game.party_data.party_item.Count(x=>x.item_no==item.Number&&((x.use_cnt>>4)&7)==item.Kind)==1,"Native item move inserts one audited reward into scratch inventory");Inventories++;
   var inserted=game.party_data.party_item.First(x=>x.item_no==item.Number);Check(inserted.use_cnt==item.UseCount,"Native encoding agrees with original inventory insertion");
   // Fill the scratch inventory with non-stackable real orbs, then ask native code to add another.
   var orb=HuntGuildRuntime.Items.Single(x=>x.Id=="flowing-orb");var orbData=G2_SYS.G2_data_get(game.ovit_data,orb.Kind,orb.Number);
   foreach(var slot in game.party_data.party_item){slot.item_no=(byte)orb.Number;slot.use_cnt=(byte)orb.UseCount;}
   string before=string.Join(",",game.party_data.party_item.Select(x=>$"{x.item_no}:{x.use_cnt}"));int full=G2_SYS.G2_min_item_move(game.ovit_data,orbData,0,0,orb.Kind,orb.Number,0,0);
   Check(before==string.Join(",",game.party_data.party_item.Select(x=>$"{x.item_no}:{x.use_cnt}")),"Native capacity prevents overflow / overwrite");
   Rows.Add(new{hunt.Id,Item=item.Id,item.Name,PreparedKind=item.Kind,PreparedNumber=item.Number,PreparedUseCount=item.UseCount,NativeInsertReturn=result,FullInventoryReturn=full,NativeScratchInsertVerified=true,VictoryInputSynthetic=true,ActualVictory=false,RenderedRewardUi=false,ActualPlayerInventoryChanged=false});Hunts.Add(hunt.Id);
  }finally{GuildItemRuntime.TransferScope=false;GuildItemRuntime.Reset();HuntGuildRuntime.Config.EnableVictoryItems=enabled;setting.Enabled=ruleEnabled;game.ovit_data=oldOvit;game.party_data.party_item=oldItems;game.have_data=oldHave;w.exit_work=exit;state.SnapshotTaken=snapshot;CombatReforgedDiagnostics.Outcome=outcome;UnityEngine.Random.state=rng;}
 }
 internal static void Menu()
 {
  var oldMenu=HuntGuildRuntime.Menu;var oldCommand=HuntGuildRuntime.CommandOwner;var oldParty=HuntGuildRuntime.PartyOwner;
  bool proven=HuntGuildRuntime.MenuProven,visible=HuntGuildRuntime.Visible;var state=Combat.State;var selected=HuntGuildRuntime.SessionSelection;bool enabled=HuntGuildRuntime.Config.EnableSelector;
  var oldGame=Framework.Singleton<GAME_WORK>.instance;var oldManager=WindowManager.instance;int ticks=HuntGuildRuntime.NativeMenuTicks,suppressed=HuntGuildRuntime.SuppressedTicks,renderFrame=HuntGuildRuntime.LastRenderFrame;bool renderFault=HuntGuildRuntime.RenderFaulted;
  var menu=Alloc<ShareUI.Menu.UIMainMenu>();var manager=Alloc<WindowManager>();var mcon=Alloc<MACHICON>();var command=Alloc<fcommand.COMCON>();
  var list=Alloc<ShareUI.UIWindowListTop>();
  try{
   var game=Alloc<GAME_WORK>();game.game_data=Alloc<GAME_DATA>();game.party_data=Alloc<PARTY_DATA>();Framework.Singleton<GAME_WORK>.instance=game;
   WindowManager.instance=manager;manager.menuWindow=menu;menu.menuCommon=Alloc<ShareUI.Menu.UIMainMenuCommon>();menu.menuCommon.windowList=list;
   list._IsOpen_k__BackingField=true;list._IsOpening_k__BackingField=false;list._IsClosing_k__BackingField=false;list._IsFocus_k__BackingField=true;
   command.stp=2;mcon.comcon=command;Combat.State=null;HuntGuildRuntime.Config.EnableSelector=true;HuntFieldContext.Window.Record(UnityEngine.Time.frameCount);
   HuntGuildRuntime.Observe(mcon);
   Check(HuntGuildRuntime.Context,"GSD2-owned shared MAIN menu admits Normal fixture");HuntGuildRuntime.Visible=true;
   HuntGuildRuntime.RenderFaulted=false;HuntGuildRuntime.LastRenderFrame=-1;int unpainted=42;Check(HuntGuildRuntime.NativeInput(mcon,ref unpainted)&&unpainted==42,"No successful rendering means native input remains active");HuntGuildRuntime.LastRenderFrame=UnityEngine.Time.frameCount;
   int before=HuntGuildRuntime.SuppressedTicks;int result=fcommand.MCommandMain(mcon);
   Check(result==0&&HuntGuildRuntime.SuppressedTicks==before+1&&command.stp==2,"Actual production MAIN hook returns native wait without invoking uninitialized menu");
   HuntAccess.Ticket.Toggle(true);Check(HuntGuildRuntime.Select("dreamers-circuit")&&!HuntAccess.Ticket.Armed&&HuntGuildRuntime.SelectedId=="dreamers-circuit","Select cancels arming and changes only contract");
   game.game_data.difficulty=2;Check(!HuntGuildRuntime.Select("sun-kings-retinue"),"Hard selection context denied");game.game_data.difficulty=0;
   command.stp=3;Check(!HuntGuildRuntime.Context,"Native submenu step excludes overlay");command.stp=2;
   list._IsOpen_k__BackingField=false;Check(!HuntGuildRuntime.Context,"Closed main list denied");list._IsOpen_k__BackingField=true;
   list._IsOpening_k__BackingField=true;Check(!HuntGuildRuntime.Context,"Opening animation denied");list._IsOpening_k__BackingField=false;
   list._IsClosing_k__BackingField=true;Check(!HuntGuildRuntime.Context,"Closing animation denied");list._IsClosing_k__BackingField=false;
   manager.menuWindow=Alloc<ShareUI.Menu.UIMainMenu>();Check(!HuntGuildRuntime.Context,"Foreign native menu owner denied");manager.menuWindow=menu;
   var party=game.party_data;game.party_data=Alloc<PARTY_DATA>();Check(!HuntGuildRuntime.Context,"Replaced party owner denied");game.party_data=party;
   HuntGuildRuntime.RenderFaulted=true;result=42;Check(HuntGuildRuntime.NativeInput(mcon,ref result)&&result==42,"Renderer fault cannot suppress native input");HuntGuildRuntime.Toggle();Check(!HuntGuildRuntime.Visible,"Renderer fault cannot reopen invisible panel");HuntGuildRuntime.RenderFaulted=false;
   HuntGuildRuntime.Visible=false;result=42;Check(HuntGuildRuntime.NativeInput(mcon,ref result)&&result==42,"Closed overlay restores original command dispatch");
   HuntGuildRuntime.Visible=true;HuntGuildRuntime.Closed(menu);Check(!HuntGuildRuntime.Context&&!HuntGuildRuntime.Visible,"Native close clears context");
   foreach(var method in new[]{HarmonyLib.AccessTools.Method(typeof(ShareUI.Menu.UIMainMenu),nameof(ShareUI.Menu.UIMainMenu.Close)),HarmonyLib.AccessTools.Method(typeof(ShareUI.Menu.UIMainMenu),nameof(ShareUI.Menu.UIMainMenu.OnDestroy)),HarmonyLib.AccessTools.Method(typeof(fcommand),nameof(fcommand.MCommandMain)),HarmonyLib.AccessTools.Method(typeof(BattleManager),nameof(BattleManager.BattleItemGet)),HarmonyLib.AccessTools.Method(typeof(G2_SYS),nameof(G2_SYS.G2_min_item_move))})Check(HarmonyLib.Harmony.GetPatchInfo(method)?.Owners.Contains(Plugin.Id)==true,"Production patch owner "+method.Name);
   LocalReissue(game,command,ReissueTable??throw new InvalidOperationException("Missing local reissue source"));
  }finally{HuntAccess.Ticket.Cancel();HuntGuildRuntime.Menu=oldMenu;HuntGuildRuntime.CommandOwner=oldCommand;HuntGuildRuntime.PartyOwner=oldParty;HuntGuildRuntime.MenuProven=proven;HuntGuildRuntime.Visible=visible;HuntGuildRuntime.LastRenderFrame=renderFrame;HuntGuildRuntime.RenderFaulted=renderFault;HuntGuildRuntime.NativeMenuTicks=ticks;HuntGuildRuntime.SuppressedTicks=suppressed;HuntGuildRuntime.SessionSelection=selected;HuntGuildRuntime.Config.EnableSelector=enabled;Combat.State=state;Framework.Singleton<GAME_WORK>.instance=oldGame;WindowManager.instance=oldManager;HuntFieldContext.Window.Clear();}
 }
 static void LocalReissue(GAME_WORK game,fcommand.COMCON command,BATTLE_DATA table)
 {
  var original=CastleGuildRuntime.Config;var quest=CastleGuildRuntime.Quest;var owner=CastleGuildRuntime.ContractParty;
  long gold=game.party_data.gold;var events=Alloc<EVENTCON>();var field=CastleGuildNativeProbe.BindWalkingFixture(events,table);
  var m=events.mcon;
  bool hunting=HuntRuntime.Config.EnableExperimentalHunt;
  var definition=HuntRuntime.Definitions.Single(h=>h.Audit.TableId==table.id);
  var setting=HuntRuntime.Settings(definition);bool enabled=setting.Enabled;
  try
  {
   game.sys_work=Alloc<SYS_WORK>();game.sys_work.mcon=m;m.comcon=command;
   HuntGuildRuntime.Observe(m);
   HuntRuntime.Config.EnableExperimentalHunt=true;setting.Enabled=true;
   CastleGuildRuntime.Config=new(){Enabled=true,AutoRequestAcceptedContract=true};CastleGuildRuntime.Quest=new();
   Check(!CastleGuildRuntime.ReissueAtField(m),"Field reissue stays opt-in");
   CastleGuildRuntime.Config.EnableFieldReissue=true;
   game.game_data.difficulty=2;Check(!CastleGuildRuntime.ReissueAtField(m),"Hard admission denied");game.game_data.difficulty=0;
   command.stp=3;Check(!CastleGuildRuntime.ReissueAtField(m),"Native submenu denied");command.stp=2;
   var map=field.eventdata.mapeventdat[m.mno];map.batlno=0;
   Check(!CastleGuildRuntime.ReissueAtField(m),"Missing native walking table denied");map.batlno=1;
   Check(CastleGuildRuntime.ReissueAtField(m)&&CastleGuildRuntime.Contract==definition.Id&&
       CastleGuildRuntime.ContractParty==game.party_data.Pointer&&HuntGuildRuntime.SelectedId==definition.Id&&
       !HuntAccess.Ticket.Armed&&game.party_data.gold==gold,"Explicit accept binds fresh local contract without spawning or paying");
   Check(!CastleGuildRuntime.ReissueAtField(m),"Local reissue cannot overwrite an active contract");
   CastleGuildRuntime.WalkingRequest(events,table,true);
   Check(HuntAccess.Ticket.Armed,"Normal walking observer can request reissued contract");
   CastleGuildRuntime.ClearContract();
   Check(CastleGuildRuntime.Quest.State==GuildQuestState.NoContract&&!HuntAccess.Ticket.Armed,"Local reissue still clears on load path");
   LocalReissuePassed=true;
  }
  finally
  {
   setting.Enabled=enabled;HuntRuntime.Config.EnableExperimentalHunt=hunting;
   CastleGuildRuntime.Config=original;CastleGuildRuntime.Quest=quest;CastleGuildRuntime.ContractParty=owner;
   UnityEngine.Object.Destroy(field);
  }
 }
 internal static bool LocalReissuePassed;
 internal static object Snapshot()=>new{Assertions,Inventories,Hunts=Hunts.ToArray(),Rows=Rows.ToArray(),OriginalGameDataLoaded=Data!=null,LocalReissuePassed,RenderedMenuInField=false,ActualPlayerItemAward=false};
}
