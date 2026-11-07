using HarmonyLib;
using System.Text.Json;
namespace NightmareRebalance;
// Only prepare the original empty victory-loot slot. Native UI/inventory owns the grant.
internal static class GuildItemRuntime
{
 internal static bool PayoutStarted;internal static GuildItemAudit? Prepared;internal static int Preparations,TransferCalls;internal static readonly List<object> Events=new();
 [ThreadStatic]internal static bool TransferScope;
 internal static void Reset(){PayoutStarted=false;Prepared=null;Preparations=TransferCalls=0;Events.Clear();}
 internal static void Record(object value){if(Events.Count<32)Events.Add(value);}
 internal static string? Signature(OVIT_DATA? data,GuildItemAudit a)
 {
  return ItemSignature(a.Kind==0?data?.dogu_data?[a.Number]:data?.embl_data?[a.Number],a);
 }
 internal static string? ItemSignature(UNI_DATA? raw,GuildItemAudit a)
 {
  if(a.Kind==0&&raw?.TryCast<DOGU_DATA>() is {} d)return JsonSerializer.Serialize(new{d.name,d.text,dummyname=d.dummyname??"",dummytext=d.dummytext??"",info=d.info?.Select(x=>(int)x).ToArray(),d.cnt,d.eff_val,d.atrb,status=d.status?.Select(x=>(int)x).ToArray(),d.botch});
  if(a.Kind==2&&raw?.TryCast<EMBL_DATA>() is {} r)return JsonSerializer.Serialize(new{r.name,r.text,info=r.info?.Select(x=>(int)x).ToArray(),r.bonus,magic_no=r.magic_no?.Select(x=>(int)x).ToArray(),r.botch});
  return null;
 }
 internal static bool Matches(OVIT_DATA? data,GuildItemAudit a)
 {return MatchesText(Signature(data,a),a);}
 internal static bool MatchesText(string? text,GuildItemAudit a)
 {return HuntGuildRules.MatchesTemplate(text,a.Template);}
 internal static bool Bound(BATTLE_WORK w)=>HuntRuntime.ActiveRoster&&HuntRuntime.Work==w.Pointer&&HuntRuntime.Party==w.monster_party_data?.Pointer&&Combat.State is{Allowed:true,SnapshotTaken:true} s&&s.Work==w.Pointer&&!s.ReportWritten&&CombatReforgedDiagnostics.Outcome=="VictorySignal";
 internal static void Prepare(BATTLE_WORK w,EXIT_WORK exit)
 {try{
  if(!HuntGuildRules.CanPrepare(HuntGuildRuntime.Config.EnableVictoryItems&&HuntRuntime.Config.EnableExperimentalHunt,Bound(w),CombatReforgedDiagnostics.Outcome=="VictorySignal",Combat.State?.SnapshotTaken??false,Combat.State?.ReportWritten??true,PayoutStarted||exit.item_work!=null,exit.monster_item?.item_no==0))return;
  var rule=HuntGuildRuntime.Config.Rewards.FirstOrDefault(x=>x.HuntId==HuntRuntime.Active?.Id&&x.Enabled);if(rule==null)return;
  var game=Framework.Singleton<GAME_WORK>.instance;if(game==null||(game.ovit_data==null&&(game.have_data==null||game.have_data.Length!=176)))return;
  var a=HuntGuildRuntime.Items.Single(x=>x.Id==rule.RewardId);if(!Plugin.CheckCompatibility()||!MatchesText(ItemSignature(G2_SYS.G2_data_get(Framework.Singleton<GAME_WORK>.instance?.ovit_data,a.Kind,a.Number),a),a)){Record(new{Kind="RewardSignatureRejected",a.Id});return;}
  exit.monster_item!.item_no=(byte)a.Number;exit.monster_item.use_cnt=(byte)a.UseCount;Prepared=a;Preparations++;
  HuntSessionRuntime.Current?.ItemPrepared(a.Id);Record(new{Kind="NativeLootSlotPrepared",a.Id,a.Name,a.Number,a.UseCount,ActualAward=false});
 }catch(Exception e){Record(new{Kind="RewardPreparationUnavailable",Reason=e.GetType().Name});Plugin.Debug("Guild reward retained native loot: "+e.Message);}}
 internal static (int Slots,int Uses)? Inventory()
 {
  if(Prepared is not {} a||Framework.Singleton<GAME_WORK>.instance?.party_data?.party_item is not {} items||items.Length!=30)return null;
  var same=items.Where(x=>x!=null&&x.item_no==a.Number&&((x.use_cnt>>4)&7)==a.Kind).ToArray();return(same.Length,same.Sum(x=>x.use_cnt&15));
 }
 internal static object Snapshot()=>new{HuntGuildRuntime.Config.EnableVictoryItems,PreparedRewardId=Prepared?.Id,Preparations,PayoutStarted,TransferCalls,Events=Events.ToArray(),Mode="Fill original no-drop slot only; preserve rolled loot; native inventory/UI grants",AwardInferredFromPreparation=false};
}
[HarmonyPatch(typeof(BattleManager),nameof(BattleManager.BattleItemGet))]
internal static class GuildLootHandoff
{
 [HarmonyPrefix]static void Prefix(BattleManager __instance){try{var w=__instance.battle_work;if(GuildItemRuntime.Bound(w)&&GuildItemRuntime.Prepared is {} a&&w.exit_work?.monster_item is {} item&&item.item_no==a.Number&&item.use_cnt==a.UseCount){GuildItemRuntime.PayoutStarted=true;HuntSessionRuntime.Current?.ItemHandoff();GuildItemRuntime.Record(new{Kind="NativeBattleItemGetEntered",a.Id,AwardConfirmed=false});}}catch(Exception e){Plugin.Debug("Guild handoff observation: "+e.Message);}}
}
[HarmonyPatch(typeof(BattleManager),nameof(BattleManager.b_AddItemWinMain))]
internal static class GuildItemTransferScope
{
 [HarmonyPrefix]static void Prefix(BattleManager __instance,ADD_ITEM_WIN ai_win,out bool __state){__state=GuildItemRuntime.TransferScope;GuildItemRuntime.TransferScope=false;try{var w=__instance.battle_work;GuildItemRuntime.TransferScope=GuildItemRuntime.PayoutStarted&&GuildItemRuntime.Bound(w)&&w.exit_work?.item_work?.ai_win?.Pointer==ai_win.Pointer;}catch(Exception e){Plugin.Debug("Guild item scope unavailable: "+e.Message);}}
 [HarmonyFinalizer]static Exception? Finalizer(bool __state,Exception? __exception){GuildItemRuntime.TransferScope=__state;return __exception;}
}
[HarmonyPatch(typeof(G2_SYS),nameof(G2_SYS.G2_min_item_move))]
internal static class GuildInventoryObservation
{
 [HarmonyPrefix]static void Prefix(int kind,int no,int place,int f_place,out (int Slots,int Uses)? __state){__state=null;try{__state=GuildItemRuntime.TransferScope&&place==0&&f_place==0&&GuildItemRuntime.Prepared is {} a&&kind==a.Kind&&no==a.Number?GuildItemRuntime.Inventory():null;}catch(Exception e){Plugin.Debug("Guild inventory sample unavailable: "+e.Message);}}
 [HarmonyPostfix]static void Postfix((int Slots,int Uses)? __state,int __result)
 {try{if(__state is not {} before||GuildItemRuntime.Inventory() is not {} after)return;GuildItemRuntime.TransferCalls++;bool increased=after.Slots>before.Slots||after.Uses>before.Uses;HuntSessionRuntime.Current?.ItemTransfer(increased);GuildItemRuntime.Record(new{Kind="NativeInventoryTransfer",ReturnCode=__result,BeforeSlots=before.Slots,AfterSlots=after.Slots,BeforeUses=before.Uses,AfterUses=after.Uses,InventoryIncreaseObserved=increased,RenderedItemUiInferred=false});}catch(Exception e){Plugin.Debug("Guild inventory observation unavailable: "+e.Message);}}
}
