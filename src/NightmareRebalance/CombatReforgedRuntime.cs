using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
namespace NightmareRebalance;

internal sealed record CombatUnitPlan(string Id,string Behavior,bool Boss,double? Hp,double? Damage);
internal static class CombatReforgedRuntime
{
    internal static CombatReforgedFile Config=new(){Enabled=false};
    internal static CombatAudit Audit=new();
    internal static readonly Dictionary<IntPtr,CombatUnitPlan> Plans=new();
    internal static readonly Dictionary<IntPtr,int> Previous=new();
    internal static readonly Dictionary<int,int> Focus=new();
    internal static readonly HashSet<IntPtr> Faulted=new();
    internal static readonly List<object> Trace=new();
    internal static IntPtr Work;
    internal static int ModeChanges,TargetChanges,Phase=-1;
    internal static bool Probe;
    [ThreadStatic]internal static bool Sampling;
    internal static BATTLE_WORK? ProbeWork;
    internal static int[] LastNativeTargets=Array.Empty<int>();
    internal sealed record Context(BATTLE_WORK Work,PM_DATA Attacker,CombatUnitPlan Plan,bool Healing=false);
    [ThreadStatic]internal static Context? Current;
    internal static void Initialize()
    {
        try{Config=CombatReforgedRules.Parse(OverhaulRuntime.Load("combat-reforged.json"));Audit=ContentRules.Read<CombatAudit>(ContentRuntime.Resource("combat-audit.json"));Plugin.Logger.LogInfo($"Combat Reforged: {Audit.Enemies.Length} audited enemies; {Audit.Bosses.Length} encounter-specific bosses; native actions only.");}
        catch(Exception e){Config.Enabled=false;Plugin.Logger.LogWarning("Combat Reforged disabled; previous modules retained: "+e.Message);}
    }
    internal static void Clear(){BossTacticsRuntime.Clear();CombatMasteryRuntime.Recovery.Clear();Work=IntPtr.Zero;Plans.Clear();Previous.Clear();Focus.Clear();Faulted.Clear();Trace.Clear();LastNativeTargets=Array.Empty<int>();ModeChanges=TargetChanges=0;Phase=-1;Current=null;}
    internal static int[] Signature(MONSTER_DATA d)
    {
        var values=new List<int>{d.hp,d.level,d.name_id};
        // Original serialized records use count0 with four physical slots. Native
        // SelectAttackMode iterates those slots directly; count4 remains accepted
        // for the previously supported representation. Never trust other counts.
        if(d.attack_data==null||!CombatMasteryRules.AttackLayout(d.attack_data.Length,d.attack_data_num))return Array.Empty<int>();
        foreach(var a in d.attack_data){if(a==null)return Array.Empty<int>();values.AddRange(new[]{(int)a.attack_type,a.attack_prob,a.kaisu,a.attack,a.tech,a.status,a.status_prob,a.attack_zokusei,a.attack_seishitsu,a.attack_func_index,a.attack_disp_func_index});}
        return values.ToArray();
    }
    internal static void Ready(BattleState state,BATTLE_WORK w,bool compatible)
    {
        Clear();Work=w.Pointer;
        if(!Config.Enabled||!compatible||w.pm_data==null)return;
        bool previousStats=state.StatsAllowed;
        try
        {
            var id=CoverageRuntime.Capture(w);
            var boss=Config.EnableBossEncounters?Audit.Bosses.FirstOrDefault(a=>CombatReforgedRules.BossMatches(a,id)):null;
            var setting=boss==null?null:Config.Bosses.Single(p=>p.TableId==boss.TableId);
            foreach(var u in w.pm_data)
            {
                if(u==null||u.IsPlayer||u.TryCast<BattleMonsterCharacter>()==null||u.monster_data is not {} d)continue;
                var sig=Signature(d);
                if(state.Allowed&&Audit.Enemies.FirstOrDefault(a=>a.Id==d.id&&DifficultyProvenance.Regular(w,d,a.Signature)) is {} a)
                {
                    var old=OverhaulRuntime.Enemies.GetValueOrDefault(d.id);var pace=Config.RolePacing.FirstOrDefault(p=>p.Role==old?.Role);
                    Plans[u.Pointer]=new(d.id,a.Behavior,false,Config.EnablePacing&&pace!=null?pace.HpMultiplier:null,Config.EnablePacing&&pace!=null?pace.DamageMultiplier:null);
                }
                else if(boss!=null&&setting is {Enabled:true}&&boss.EnemyId==d.id&&boss.Signature.SequenceEqual(sig))
                {
                    Plans[u.Pointer]=new(d.id,boss.Behavior,true,setting.HpMultiplier,setting.DamageMultiplier);
                    state.StatsAllowed=true;
                    Plugin.Logger.LogInfo($"Combat Reforged boss admitted: {id.TableId}/{id.PartyId}; exact handlers/roster/attacks; HP x{setting.HpMultiplier}, damage x{setting.DamageMultiplier}. Original story handlers retained.");
                }
            }
        }
        catch(Exception e){Plans.Clear();state.StatsAllowed=previousStats;Plugin.Logger.LogWarning("Combat Reforged encounter inspection unavailable: "+e.Message);}
    }
    internal static double Factor(BattleState? s,PM_DATA u,double prior,bool hp)
    {
        if(!Config.Enabled||s==null||s.Work!=Work||!Plans.TryGetValue(u.Pointer,out var p))return prior;
        if(p.Boss&&BossRuntime.Plan(s,u)?.ProfileId!=null)return prior; // Explicit existing boss profile takes priority.
        double? factor=HuntRuntime.Factor(u,hp)??(hp?p.Hp:p.Damage);
        if(!p.Boss&&OverhaulRuntime.Enemies.TryGetValue(p.Id,out var custom)&&MaintenanceRuntime.Baseline.TryGetValue(p.Id,out var baseline))
            return MaintenanceRules.Factor(prior,factor,hp?custom.HpMultiplier:custom.DamageMultiplier,hp?baseline.HpMultiplier:baseline.DamageMultiplier);
        return factor is double f&&double.IsFinite(f)?f:prior;
    }
    internal static bool AiActive(BATTLE_WORK? w)=>Config.Enabled&&Config.EnableEnemyAi&&Plugin.Options.EnableCombatReforgedAi.Value&&w!=null&&w.Pointer==Work&&(Probe||Combat.State is {Ready:true} s&&s.Work==w.Pointer);
    internal static Context? For(BATTLE_WORK? w,int slot)
    {
        if(!AiActive(w)||w!.pm_data==null||slot<0||slot>=w.pm_data.Length||w.pm_data[slot] is not {} u||u.IsPlayer||u.IsDead||!(Plans.TryGetValue(u.Pointer,out var p)||BossTacticsRuntime.Plan(u,out p))||Faulted.Contains(u.Pointer))return null;
        return new(w,u,CombatEvolutionRuntime.Plan(u,p));
    }
    internal static BATTLE_WORK? ActiveWork=>Probe?ProbeWork:Framework.Singleton<BATTLE_WORK>.instance;
    internal static CombatTarget Target(PM_DATA t,int slot,int status=0)
    {
        int P(int n)=>t.calc_para is {} p&&p.Length>n?Math.Max(0,p[n]):0;
        bool useful=status==0||PM_DATA.status_henkan_table is not {} masks||status>=masks.Length||(t.status&masks[status])==0;
        return new(slot,t.hp,t.max_hp,P(2),P(3),P(1),P(0),Focus.GetValueOrDefault(slot),useful);
    }
    internal static List<CombatTarget> Targets(BATTLE_WORK w,bool players,int status=0)=>w.pm_data.Select((u,i)=>(u,i)).Where(x=>x.u!=null&&x.u.IsPlayer==players&&x.u.hp>0&&!x.u.IsDead&&x.u.MokuhyoCanCheck()).Select(x=>Target(x.u,x.i,status)).ToList();
    static void PhaseScope(BATTLE_WORK w){if(Phase!=w.battle_phase_no){Phase=w.battle_phase_no;Focus.Clear();}}
    internal static void Record(object row){if(!Probe&&Trace.Count<64)Trace.Add(row);}
    internal static void Select(BattleMonsterCharacter u,ref int selected)
    {
        var c=For(ActiveWork,u.chara_no);if(c==null||selected is <0 or >3)return;
        try
        {
            var w=c.Work;var data=u.monster_data;var attacks=data?.attack_data;
            if(attacks?.Length!=4||w.pm_attack_data==null||u.chara_no>=w.pm_attack_data.Length||w.pm_attack_data[u.chara_no]?.curent_use_cnt?.Length!=4||w.b_magic_status==null)return;
            if(attacks[selected] is {attack:0,status:0})return; // Unknown support/summon semantics retain the native choice.
            PhaseScope(w);int original=selected,best=selected,bestScore=int.MinValue;
            foreach(int i in Enumerable.Range(0,4).OrderBy(i=>i==original?0:1))
            {
                var a=attacks[i];if(a==null||a.attack_prob==0||a is {attack:0,status:0}||a.attack_type is not(1 or 2 or 5)||!u.CheckMonsterAttackCnt(i)||!u.AttackModeKyoriCheck(i)||(w.b_magic_status.water_silent_cnt>0&&(a.attack_zokusei&0x20)!=0))continue;
                if((a.attack_zokusei&0x10)==0?!u.MagicCanCheck():!u.AttackCanCheck())continue;
                var enemies=Targets(w,true,a.status);var allies=Targets(w,false);
                bool multi=(a.attack_seishitsu&0xf00) is 0x200 or 0x400 or 0x800;
                var option=new CombatAction(i,a.attack_type,a.attack,(a.attack_zokusei&0x10)!=0,multi,a.status,a.attack_prob,w.pm_attack_data[u.chara_no].curent_use_cnt[i],true);
                int score=CombatReforgedRules.ActionScore(c.Plan.Behavior,option,enemies,allies,u.calc_para is {Length:>1}?u.calc_para[1]:0,Previous.GetValueOrDefault(u.Pointer,-1),Config.RepeatPenalty,Config.HealingThreshold);
                int baseScore=score;
                if(score!=int.MinValue){if(BossTacticsRuntime.Bound(u))score=BossTacticsRuntime.Score(w,u,option,enemies,score);
                else{score+=CombatEvolutionRuntime.Bonus(u,i)+CombatMasteryRuntime.Bonus(u,i);int extra=CombatRealityRuntime.Bonus(u,i);CombatRealityRuntime.Score(u,i,score,extra);score+=extra;}}
                MaintenanceRuntime.Ai("ActionEvaluated",u.chara_no,new{Mode=i,BaseScore=baseScore,FinalScore=score,ScoreModified=score!=baseScore,NativeLegalityChecked=true});
                if(score>bestScore){bestScore=score;best=i;}
            }
            int before=selected;selected=best;CombatRealityRuntime.Selected(u,before,best);Previous[u.Pointer]=best;CombatMasteryRuntime.Selected(u,best);BossTacticsRuntime.Selected(w,u,before,best);
            Record(new{Kind="ModeSelection",Attacker=u.chara_no,c.Plan.Id,c.Plan.Behavior,Before=before,After=best,Changed=before!=best,NativeRangeAndUsesChecked=true});
            if(before!=best){if(!Probe)ModeChanges++;Record(new{Kind="Mode",Attacker=u.chara_no,c.Plan.Id,c.Plan.Behavior,Before=before,After=best,NativeRangeAndUsesChecked=true});}
        }
        catch(Exception e){Faulted.Add(u.Pointer);Plugin.Logger.LogWarning("Combat Reforged AI retained native choice after inspection failure: "+e.Message);}
    }
    internal static void SampleTargets(BattleParty party,int attacker,ref int result)
    {
        if(Sampling||result<0)return;
        var w=ActiveWork;var c=For(w,attacker);
        bool legacy=c==null&&Plugin.Options.EnableEnemyTargeting.Value&&Combat.Matches(w)&&w!.pm_data is {} units&&attacker>=0&&attacker<units.Length&&units[attacker] is {} enemy&&TargetingRuntime.Raiders.Contains(enemy.monster_data?.id??"");
        if(c==null&&!legacy)return;
        var saved=UnityEngine.Random.state;var choices=new HashSet<int>{result};
        try
        {
            // These audited native routines only read combat state, allocate scratch
            // buffers and draw RNG. Native callers retain their complete target
            // legality filters. Restore RNG after discovery, including exceptions.
            Sampling=true;
            for(int i=0;i<128;i++){int choice=party.MonsterFindKojinMokuhyo(attacker);if(choice>=0)choices.Add(choice);}
        }
        catch(Exception e){if(c!=null)Faulted.Add(c.Attacker.Pointer);Plugin.Logger.LogWarning("Native target discovery disabled for this enemy: "+e.Message);return;}
        finally{Sampling=false;UnityEngine.Random.state=saved;}
        if(w?.pm_data==null)return;
        LastNativeTargets=choices.OrderBy(i=>i).ToArray();
        var legal=choices.Where(i=>i>=0&&i<w.pm_data.Length&&w.pm_data[i] is {IsPlayer:true} u&&u.hp>0&&!u.IsDead&&u.MokuhyoCanCheck()).Select(i=>Target(w.pm_data[i],i)).ToArray();
        int before=result;
        if(c!=null){PhaseScope(w);result=BossTacticsRuntime.QualityTarget(c.Attacker)?BossTacticsRuntime.ChooseTarget(c.Attacker,legal,result,Config.FocusLimit):CombatReforgedRules.ChooseTarget(c.Plan.Behavior,legal,result,Config.FocusLimit);Focus[result]=Focus.GetValueOrDefault(result)+1;}
        else result=TargetingRules.PreferWounded(legal.Select(t=>new TargetCandidate(t.Slot,t.Hp,t.MaxHp,true,true)).ToArray(),result,Plugin.Options.EnemyTargetHealthThreshold.Value);
        Record(new{Kind="TargetSelection",Attacker=attacker,Behavior=c?.Plan.Behavior??"LegacyRaider",Before=before,After=result,Changed=before!=result,ObservedNativeCandidates=choices.ToArray(),RandomRestored=true});
        if(result!=before){if(!Probe){TargetChanges++;if(legacy)TargetingRuntime.Changes++;}Record(new{Kind="NativeSampledTarget",Attacker=attacker,Behavior=c?.Plan.Behavior??"LegacyRaider",Before=before,After=result,ObservedNativeCandidates=choices.ToArray(),RandomRestored=true});}
    }
    internal static object Snapshot()=>new{BossTactics=BossTacticsRuntime.Snapshot(),Enabled=Config.Enabled,ActiveWork=Work!=IntPtr.Zero,ModeChanges,TargetChanges,RetainedAiTraceRows=Trace.Count,ObservedActionSelectors=CombatRealityRuntime.SelectorCalls.Values.Sum(),EnemyRoutineEntries=MaintenanceRuntime.EnemyRoutineEntries,SelectorAbsenceDoesNotProveScriptBypass=true,TrackedEnemies=Plans.Count,Plans=Plans.Values.ToArray(),Trace=Trace.ToArray(),FaultedEnemies=Faulted.Count,FullBattleTested=false};
    internal static int Spell(int id,int effect)
    {var s=Config.SpellEffects.FirstOrDefault(s=>s.Id==id);return Config.Enabled&&Config.EnableHealingBalance&&s!=null&&effect==s.Expected?s.Effect:effect;}
}
[HarmonyPatch(typeof(BattleMonsterCharacter),nameof(BattleMonsterCharacter.SelectAttackMode))]
internal static class CombatModePatch{[HarmonyPostfix]static void Postfix(BattleMonsterCharacter __instance,ref int __result)=>CombatReforgedRuntime.Select(__instance,ref __result);}
[HarmonyPatch(typeof(BattleMonsterCharacter),nameof(BattleMonsterCharacter.AttackDataMakeNormal))]
internal static class CombatActionScope
{
    [HarmonyPrefix]static void Prefix(BattleMonsterCharacter __instance,out CombatReforgedRuntime.Context? __state){__state=CombatReforgedRuntime.Current;CombatReforgedRuntime.Current=CombatReforgedRuntime.For(CombatReforgedRuntime.ActiveWork,__instance.chara_no);}
    [HarmonyFinalizer]static Exception? Finalizer(CombatReforgedRuntime.Context? __state,Exception? __exception){CombatReforgedRuntime.Current=__state;return __exception;}
}
[HarmonyPatch(typeof(BattleParty),nameof(BattleParty.MonsterFindKojinMokuhyo))]
internal static class CombatTargetScope
{
    [HarmonyPrefix]static void Prefix(int chara_no,out CombatReforgedRuntime.Context? __state){__state=CombatReforgedRuntime.Current;CombatReforgedRuntime.Current=CombatReforgedRuntime.For(CombatReforgedRuntime.ActiveWork,chara_no);}
    [HarmonyFinalizer]static Exception? Finalizer(CombatReforgedRuntime.Context? __state,Exception? __exception){CombatReforgedRuntime.Current=__state;return __exception;}
}
[HarmonyPatch(typeof(BattleParty),nameof(BattleParty.MonsterFindKojinMokuhyo))]
internal static class CombatTargetPatch
{[HarmonyPostfix]static void Postfix(BattleParty __instance,int chara_no,ref int __result)=>CombatReforgedRuntime.SampleTargets(__instance,chara_no,ref __result);}

