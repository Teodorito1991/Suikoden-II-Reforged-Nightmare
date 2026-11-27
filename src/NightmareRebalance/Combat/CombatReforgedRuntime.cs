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
    internal static void Clear(){BossTacticsRuntime.Clear();LegendaryBossRuntime.Clear();CombatMasteryRuntime.Recovery.Clear();Work=IntPtr.Zero;Plans.Clear();Previous.Clear();Focus.Clear();Faulted.Clear();Trace.Clear();LastNativeTargets=Array.Empty<int>();ModeChanges=TargetChanges=0;Phase=-1;Current=null;}
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
        if(w?.pm_data is {} hallUnits&&slot>=0&&slot<hallUnits.Length&&hallUnits[slot] is {} hallActor&&
            HallNightmareRuntime.Plan(w,hallActor) is {} hallPlan&&!Faulted.Contains(hallActor.Pointer))return new(w,hallActor,hallPlan);
        if(!AiActive(w)||w!.pm_data==null||slot<0||slot>=w.pm_data.Length||w.pm_data[slot] is not {} u||u.IsPlayer||u.IsDead||!(Plans.TryGetValue(u.Pointer,out var p)||BossTacticsRuntime.Plan(u,out p))||Faulted.Contains(u.Pointer))return null;
        if(!StoryBossSelectionScope.Allows(u))return null;
        return new(w,u,CombatEvolutionRuntime.Plan(u,p));
    }
    internal static BATTLE_WORK? ActiveWork=>Probe?ProbeWork:Framework.Singleton<BATTLE_WORK>.instance;
    internal static CombatTarget Target(PM_DATA t,int slot,int status=0)
    {
        int P(int n)=>t.calc_para is {} p&&p.Length>n?Math.Max(0,p[n]):0;
        bool useful=status==0||PM_DATA.status_henkan_table is not {} masks||status>=masks.Length||(t.status&masks[status])==0;
        return new(slot,t.hp,t.max_hp,P(2),P(3),P(1),P(0),Focus.GetValueOrDefault(slot),useful,(t.status&b_game_h.STATUS_SLEEP)!=0,(t.status&b_game_h.STATUS_GUARD)!=0);
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
            var team=Config.EnableTeamwork&&!c.Plan.Boss&&c.Attacker.Pointer==u.Pointer?TeamworkRuntime.Bound(w):null;
            int magic=u.calc_para is { Length: > 1 } p?p[1]:0;
            bool escort=team!=null&&TeamworkRuntime.LivingCaster(w,team,u);
            var allies=Targets(w,false);
            bool anyAllyLow=allies.Any(t=>t.Hp*3<=t.MaxHp);
            bool ordinary=!c.Plan.Boss&&!(HuntRuntime.Active!=null&&HuntRuntime.Work==w.Pointer);
            CombatAction bestAction=default;
            for(int step=0;step<4;step++)
            {
                int i=step==0?original:(step<=original?step-1:step);
                var a=attacks[i];if(a==null||a.attack_prob==0||a is {attack:0,status:0}||a.attack_type is not(1 or 2 or 5)||!u.CheckMonsterAttackCnt(i)||!u.AttackModeKyoriCheck(i)||(w.b_magic_status.water_silent_cnt>0&&(a.attack_zokusei&0x20)!=0))continue;
                if((a.attack_zokusei&0x10)==0?!u.MagicCanCheck():!u.AttackCanCheck())continue;
                var enemies=Targets(w,true,a.status);
                bool multi=(a.attack_seishitsu&0xf00) is 0x200 or 0x400 or 0x800;
                var option=new CombatAction(i,a.attack_type,a.attack,(a.attack_zokusei&0x10)!=0,multi,a.status,a.attack_prob,w.pm_attack_data[u.chara_no].curent_use_cnt[i],true);
                int score=CombatReforgedRules.ActionScore(c.Plan.Behavior,option,enemies,allies,magic,Previous.GetValueOrDefault(u.Pointer,-1),Config.RepeatPenalty,Config.HealingThreshold,ordinary:ordinary);
                int baseScore=score;
                if(score!=int.MinValue&&team!=null)
                    score+=TacticalTeamwork.ActionBonus(team.Style,option,enemies,magic,escort,
                        anyAllyLow,multi&&team.AreaReserved(u.chara_no,a.status));
                if(score!=int.MinValue){if(ordinary&&Config.EnablePacing&&PM_DATA.status_henkan_table is {Length:>10} masks&&masks[10]==b_game_h.STATUS_SLEEP)
                    score-=CombatReforgedRules.OrdinarySleepPenalty(option,enemies);
                if(HallFirstContact.NightmareId(w,u) is {} echoId)score=HallNightmareRules.Score(echoId,w.battle_kaisuu,option,enemies);
                else if(BossTacticsRuntime.Bound(u))score=BossTacticsRuntime.Score(w,u,option,enemies,score);
                else{score+=CombatEvolutionRuntime.Bonus(u,i)+CombatMasteryRuntime.Bonus(u,i);int extra=CombatRealityRuntime.Bonus(u,i);CombatRealityRuntime.Score(u,i,score,extra);score+=extra;}}
                MaintenanceRuntime.Ai("ActionEvaluated",u.chara_no,new{Mode=i,BaseScore=baseScore,FinalScore=score,ScoreModified=score!=baseScore,NativeLegalityChecked=true});
                if(score>bestScore){bestScore=score;best=i;bestAction=option;}
            }
            if(team!=null&&bestAction.Legal)team.Selected(u.chara_no,bestAction);
            int before=selected;selected=best;CombatRealityRuntime.Selected(u,before,best);Previous[u.Pointer]=best;CombatMasteryRuntime.Selected(u,best);BossTacticsRuntime.Selected(w,u,before,best);
            HallNightmareRuntime.Selected(w,u,before,best);
            Record(new{Kind="ModeSelection",Attacker=u.chara_no,c.Plan.Id,c.Plan.Behavior,Before=before,After=best,Changed=before!=best,NativeRangeAndUsesChecked=true});
            if(before!=best){if(!Probe)ModeChanges++;Record(new{Kind="Mode",Attacker=u.chara_no,c.Plan.Id,c.Plan.Behavior,Before=before,After=best,NativeRangeAndUsesChecked=true});}
        }
        catch(Exception e){Faulted.Add(u.Pointer);Plugin.Logger.LogWarning("Combat Reforged AI retained native choice after inspection failure: "+e.Message);}
    }
    internal static void SampleTargets(BattleParty party,int attacker,ref int result)
    {
        if(Sampling||result<0)return;
        var w=ActiveWork;var c=For(w,attacker);
        if(c!=null&&BossTacticsRuntime.UsesOriginalTargets(c.Attacker))return;
        bool legacy=c==null&&Plugin.Options.EnableEnemyTargeting.Value&&Combat.Matches(w)&&w!.pm_data is {} units&&attacker>=0&&attacker<units.Length&&units[attacker] is {} enemy&&TargetingRuntime.Raiders.Contains(enemy.monster_data?.id??"");
        if(c==null&&!legacy)return;
        var saved=UnityEngine.Random.state;var choices=new HashSet<int>{result};
        int maxLegalPlayers=0;
        if(w?.pm_data is {} pmUnits)
            for(int p=0;p<pmUnits.Length;p++)
                if(pmUnits[p] is {IsPlayer:true,hp:>0,IsDead:false} pu && pu.MokuhyoCanCheck())maxLegalPlayers++;
        try
        {
            // These audited native routines only read combat state, allocate scratch
            // buffers and draw RNG. Native callers retain their complete target
            // legality filters. Restore RNG after discovery, including exceptions.
            Sampling=true;
            int stale=0;
            for(int i=0;i<128;i++)
            {
                if(maxLegalPlayers>0&&choices.Count>=maxLegalPlayers)break;
                int choice=party.MonsterFindKojinMokuhyo(attacker);
                if(choice>=0&&choices.Add(choice))stale=0;
                else if(++stale>=24)break;
            }
        }
        catch(Exception e){if(c!=null)Faulted.Add(c.Attacker.Pointer);Plugin.Logger.LogWarning("Native target discovery disabled for this enemy: "+e.Message);return;}
        finally{Sampling=false;UnityEngine.Random.state=saved;}
        if(w?.pm_data==null)return;
        LastNativeTargets=choices.OrderBy(i=>i).ToArray();
        if(c!=null)PhaseScope(w);
        int status=0;
        if(c?.Attacker is {} unit && unit.monster_data?.attack_data is {} attacks
            && unit.c_mode >= 0 && unit.c_mode < attacks.Length
            && attacks[unit.c_mode] is { attack_type: 1 or 2 } attack)
            status=attack.status;
        var legal=choices.Where(i=>i>=0&&i<w.pm_data.Length&&w.pm_data[i] is {IsPlayer:true} u&&u.hp>0&&!u.IsDead&&u.MokuhyoCanCheck()).Select(i=>Target(w.pm_data[i],i,status)).ToArray();
        int before=result;
        if(c!=null)
        {
            if(HallFirstContact.NightmareId(w,c.Attacker)!=null)
                result=HallNightmareRuntime.Target(legal,result,w.battle_kaisuu);
            else if(BossTacticsRuntime.QualityTarget(c.Attacker))
                result=BossTacticsRuntime.ChooseTarget(c.Attacker,legal,result,Config.FocusLimit);
            else
                result=ChooseTeamTarget(party,c,legal,result);
            Focus[result]=Focus.GetValueOrDefault(result)+1;
        }
        else result=TargetingRules.PreferWounded(legal.Select(t=>new TargetCandidate(t.Slot,t.Hp,t.MaxHp,true,true)).ToArray(),result,Plugin.Options.EnemyTargetHealthThreshold.Value);
        Record(new{Kind="TargetSelection",Attacker=attacker,Behavior=c?.Plan.Behavior??"LegacyRaider",Before=before,After=result,Changed=before!=result,ObservedNativeCandidates=choices.ToArray(),RandomRestored=true});
        if(result!=before){if(!Probe){TargetChanges++;if(legacy)TargetingRuntime.Changes++;}Record(new{Kind="NativeSampledTarget",Attacker=attacker,Behavior=c?.Plan.Behavior??"LegacyRaider",Before=before,After=result,ObservedNativeCandidates=choices.ToArray(),RandomRestored=true});}
    }
    static int ChooseTeamTarget(BattleParty party, Context c, CombatTarget[] legal, int original)
    {
        if (!Config.EnableTeamwork || c.Plan.Boss || TeamworkRuntime.Bound(c.Work) is not {} team ||
            c.Attacker.manager?.Pointer != Combat.State?.Manager ||
            c.Attacker.manager?.TryCast<BattleManager>()?.parties is not { Length: > 1 } parties ||
            party.Pointer != parties[1]?.Pointer ||
            c.Attacker.monster_data?.attack_data is not {} attacks || c.Attacker.c_mode < 0 ||
            c.Attacker.c_mode >= attacks.Length || attacks[c.Attacker.c_mode] is not { attack_type: 1 or 2 } a ||
            (a.attack_seishitsu & 0xf00) is 0x200 or 0x400 or 0x800)
            return CombatReforgedRules.ChooseTarget(c.Plan.Behavior,legal,original,Config.FocusLimit);

        var action=new CombatAction(c.Attacker.c_mode,a.attack_type,a.attack,(a.attack_zokusei&0x10)!=0,
            false,a.status,a.attack_prob,1,true);
        int magic=c.Attacker.calc_para is { Length: > 1 } p?p[1]:0;
        bool escort=TeamworkRuntime.LivingCaster(c.Work,team,c.Attacker);
        int best=original,bestScore=int.MinValue;
        foreach(var t in legal.OrderBy(t=>t.Slot==original?0:1))
        {
            int score=CombatReforgedRules.TargetScore(c.Plan.Behavior,t,Config.FocusLimit);
            if(score==int.MinValue)continue;
            score+=TacticalTeamwork.TargetBonus(team.Style,action,t,magic,escort,team.AlliedSelections(c.Attacker.chara_no,t.Slot));
            if(score>bestScore){bestScore=score;best=t.Slot;}
        }
        if(bestScore!=int.MinValue)team.Targeted(c.Attacker.chara_no,best);
        return best;
    }

    internal static object Snapshot()=>new{BossTactics=BossTacticsRuntime.Snapshot(),Enabled=Config.Enabled,Config.EnableTeamwork,Config.EnableArcaneGuard,Teamwork=Combat.State?.TeamworkProfile,ActiveWork=Work!=IntPtr.Zero,ModeChanges,TargetChanges,RetainedAiTraceRows=Trace.Count,ObservedActionSelectors=CombatRealityRuntime.SelectorCalls.Values.Sum(),EnemyRoutineEntries=MaintenanceRuntime.EnemyRoutineEntries,SelectorAbsenceDoesNotProveScriptBypass=true,TrackedEnemies=Plans.Count,Plans=Plans.Values.ToArray(),Trace=Trace.ToArray(),FaultedEnemies=Faulted.Count,FullBattleTested=false};
    internal static int Spell(int id,int effect)
    {var s=Config.SpellEffects.FirstOrDefault(s=>s.Id==id);return Config.Enabled&&Config.EnableHealingBalance&&s!=null&&effect==s.Expected?s.Effect:effect;}
}
[HarmonyPatch(typeof(BattleMonsterCharacter),nameof(BattleMonsterCharacter.SelectAttackMode))]
internal static class CombatModePatch
{
    [HarmonyPostfix]
    static void Postfix(BattleMonsterCharacter __instance, ref int __result)
    {
        CombatReforgedRuntime.Select(__instance, ref __result);
        LegendaryBossRuntime.Select(__instance, ref __result);
        BossTacticsProbe.ObserveDispatchSelection(__instance, ref __result);
    }
}
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

