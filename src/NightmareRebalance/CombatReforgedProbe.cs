using BepInEx;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
using System.Text.Json;
namespace NightmareRebalance;
internal static class CombatReforgedProbe
{
    internal static int Assertions,ModeDifferences,TargetDifferences;
    internal static bool Passed;
    internal static readonly List<object> Results=new();
    static T Allocate<T>() where T:Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
    {RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);return (T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;}
    internal static MONSTER_DATA Template(string id,int[] sig)
    {
        var d=UnityEngine.ScriptableObject.CreateInstance<MONSTER_DATA>();d.id=id;d.hp=(ushort)sig[0];d.level=(byte)sig[1];d.name_id=sig[2];d.attack_data_num=4;d.attack_data=new Il2CppReferenceArray<MONSTER_ATTACK_DATA>(4);
        for(int i=0;i<4;i++){int n=3+i*11;d.attack_data[i]=new(){attack_type=(byte)sig[n],attack_prob=(byte)sig[n+1],kaisu=(byte)sig[n+2],attack=(ushort)sig[n+3],tech=(ushort)sig[n+4],status=(byte)sig[n+5],status_prob=(byte)sig[n+6],attack_zokusei=(ushort)sig[n+7],attack_seishitsu=(ushort)sig[n+8],attack_func_index=sig[n+9],attack_disp_func_index=sig[n+10]};}
        return d;
    }
    internal static void Run()
    {
        if(!Plugin.Options.RunCombatSelfTest.Value)return;
        if(!CombatReforgedRuntime.Config.Enabled||!Plugin.Options.EnableCombatReforgedAi.Value){Plugin.Logger.LogInfo("Combat Reforged probe not run: module or AI disabled.");return;}
        if(Combat.State!=null||WarRuntime.Active||WarTacticsRuntime.Armed)throw new InvalidOperationException("Combat probe requires unloaded startup.");
        var saved=Framework.Singleton<BATTLE_WORK>.instance;var savedGame=Framework.Singleton<GAME_WORK>.instance;var rng=UnityEngine.Random.state;
        var manager=Allocate<BattleManager>();var w=Allocate<BATTLE_WORK>();var nativeManager=manager.Cast<IBattleManager>();
        var created=new List<MONSTER_DATA>();bool ai=CombatReforgedRuntime.Config.EnableEnemyAi;
        var auxiliary=new List<UnityEngine.Object>();
        var savedBossSettings=CombatReforgedRuntime.Config.Bosses.Select(b=>(b,b.Enabled)).ToArray();
        bool savedPacing=CombatReforgedRuntime.Config.EnablePacing;
        void Check(bool value){Assertions++;if(!value)throw new InvalidOperationException("Combat native assertion "+Assertions);}
        try
        {
            Framework.Singleton<BATTLE_WORK>.instance=w;
            // The native equipment target filter reads G2_varia_dat through
            // GAME_WORK. Supply scratch character data; never touch its saved rows.
            var scratchGame=Allocate<GAME_WORK>();scratchGame.chara_data=Allocate<CHARA_DATA>();
            scratchGame.chara_data.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(1);
            Framework.Singleton<GAME_WORK>.instance=scratchGame;
            w.pm_data=new Il2CppReferenceArray<PM_DATA>(12);w.b_chara=new Il2CppReferenceArray<BATTLE_CHARA>(12);w.pm_attack_data=new Il2CppReferenceArray<PM_ATTACK_DATA>(12);
            w.c_varia_dat=new Il2CppReferenceArray<C_VARIA_DAT>(12);w.MokuhyoKaisu=new Il2CppStructArray<byte>(12);w.monster_data=new Il2CppReferenceArray<MONSTER_DATA>(1);w.b_magic_status=new();w.player_kazu=6;w.monster_kazu=1;w.pm_kazu=7;
            Check(manager.battle_work.Pointer==w.Pointer);
            manager.parties=new Il2CppReferenceArray<BattleParty>(2);manager.parties[0]=new(0,nativeManager);manager.parties[1]=new(1,nativeManager);
            for(int i=0;i<12;i++)
            {
                PM_DATA u=i<6?Allocate<BattlePlayerCharacter>():Allocate<BattleMonsterCharacter>();
                u.manager=nativeManager;u.chara_no=i;u.chara_id=0;u.position=(byte)(i<6?i:0);u.hp=100;u.max_hp=100;u.calc_para=new Il2CppStructArray<int>(new[]{90,60,40,30,50,50,50,100});w.pm_data[i]=u;
                w.b_chara[i]=Allocate<BATTLE_CHARA>();w.b_chara[i].chara_id=0;
                w.pm_attack_data[i]=new(){curent_use_cnt=new Il2CppStructArray<byte>(new byte[]{255,255,255,255})};
                w.c_varia_dat[i]=new(){mon_eqp=new Il2CppStructArray<byte>(3),bogu_eqp=new Il2CppStructArray<byte>(5),mp=new Il2CppStructArray<byte>(new byte[]{0x77,0x55,0x33,0x11})};
                w.c_varia_dat[i].item_eqp=new Il2CppReferenceArray<PARTY_ITEM>(3);for(int j=0;j<3;j++)w.c_varia_dat[i].item_eqp[j]=new PARTY_ITEM();
            }
            scratchGame.chara_data.c_varia_dat[0]=w.c_varia_dat[0];
            for(int i=7;i<12;i++)w.pm_data[i].dead_flg=1;
            var actor=w.pm_data[6].Cast<BattleMonsterCharacter>();
            CombatReforgedRuntime.Probe=true;CombatReforgedRuntime.ProbeWork=w;CombatReforgedRuntime.Work=w.Pointer;
            var first=CombatReforgedRuntime.Audit.Enemies.Single(a=>a.Id=="vb07_00_03");var firstData=Template(first.Id,first.Signature);created.Add(firstData);w.monster_data[0]=firstData;w.front_player=3;w.back_player=3;
            w.pm_data[0].hp=25;w.pm_data[1].calc_para[3]=5;w.pm_data[2].calc_para[0]=10;w.pm_data[3].calc_para[1]=180;
            foreach(string behavior in new[]{"Aggressive","Caster","Cautious","Disruptor","Opportunist"})
            {
                CombatReforgedRuntime.Plans[actor.Pointer]=new("fixture",behavior,false,1,1);
                var pool=new Il2CppStructArray<int>(new[]{0,1,2,3});
                CombatReforgedRuntime.Config.EnableEnemyAi=false;CombatReforgedRuntime.Current=null;UnityEngine.Random.InitState(12345);int before=manager.parties[1].MonsterFindKojinMokuhyo(6);var nativeRng=UnityEngine.Random.state;
                CombatReforgedRuntime.Config.EnableEnemyAi=true;CombatReforgedRuntime.Current=new(w,actor,CombatReforgedRuntime.Plans[actor.Pointer]);
                CombatReforgedRuntime.Focus.Clear();UnityEngine.Random.InitState(12345);int after=manager.parties[1].MonsterFindKojinMokuhyo(6);
                var finalRng=UnityEngine.Random.state;Check(finalRng.s0==nativeRng.s0&&finalRng.s1==nativeRng.s1&&finalRng.s2==nativeRng.s2&&finalRng.s3==nativeRng.s3);
                Check(after>=0&&after<6);Check(!w.pm_data[after].IsDead);if(before!=after)TargetDifferences++;
                Results.Add(new{Kind="NativeTarget",Behavior=behavior,Before=before,After=after,NativeLegalCandidates=CombatReforgedRuntime.LastNativeTargets,DisposableFixture=true});
                Plugin.Logger.LogInfo($"Combat fixture {behavior}: {before}->{after}; native candidates {string.Join(",",CombatReforgedRuntime.LastNativeTargets)}; target0 HP={w.pm_data[0].hp}, valid={w.pm_data[0].MokuhyoCanCheck()}");
                CombatReforgedRuntime.Current=null;
            }
            Check(TargetDifferences>=3);
            foreach(string id in new[]{"vc12_00_01","vb07_00_03","va11_00_01","boss_rat_00","boss_hap1_00","boss_ryo_00","boss_sie1_00"})
            {
                var audit=CombatReforgedRuntime.Audit.Enemies.FirstOrDefault(a=>a.Id==id);
                var boss=CombatReforgedRuntime.Audit.Bosses.FirstOrDefault(a=>a.EnemyId==id);
                var d=Template(id,audit?.Signature??boss!.Signature);created.Add(d);w.monster_data[0]=d;
                Check(CombatReforgedRuntime.Signature(d).SequenceEqual(audit?.Signature??boss!.Signature));
                CombatReforgedRuntime.Plans[actor.Pointer]=new(id,audit?.Behavior??boss!.Behavior,boss!=null,1,1);
                for(int seed=0;seed<12;seed++)
                {
                    CombatReforgedRuntime.Config.EnableEnemyAi=false;UnityEngine.Random.InitState(seed);int before=actor.SelectAttackMode();
                    CombatReforgedRuntime.Config.EnableEnemyAi=true;CombatReforgedRuntime.Previous.Clear();UnityEngine.Random.InitState(seed);int after=actor.SelectAttackMode();
                    Check(after is >=0 and <4);Check(d.attack_data[after].attack_prob>0);Check(actor.CheckMonsterAttackCnt(after));Check(actor.AttackModeKyoriCheck(after));Check(!CombatReforgedRuntime.Faulted.Contains(actor.Pointer));
                    if(before!=after)ModeDifferences++;
                    if(seed==0)Results.Add(new{Kind="NativeAction",Id=id,Before=before,After=after,NativeRangeAndUses=true,FixtureOnly=true});
                }
                // Exhaustion must exclude an otherwise preferred original move.
                int used=actor.SelectAttackMode();w.pm_attack_data[6].curent_use_cnt[used]=0;
                if(d.attack_data.Count(a=>a.attack_type!=0&&a.attack_prob>0)>1){int after=actor.SelectAttackMode();Check(after!=used);}
                w.pm_attack_data[6].curent_use_cnt[used]=255;
                w.b_magic_status.water_silent_cnt=1;int silent=actor.SelectAttackMode();if(silent>=0)Check((d.attack_data[silent].attack_zokusei&0x20)==0);w.b_magic_status.water_silent_cnt=0;
            }
            Check(ModeDifferences>=1);
            // Real native formula comparisons: no critical/passive/affinity/status modifiers.
            var formula=created[0];w.monster_data[0]=formula;
            foreach(bool physical in new[]{false,true})foreach(int power in new[]{10,80,200})foreach(int defense in new[]{5,90,250})for(int seed=0;seed<8;seed++)
            {
                formula.attack_data[0].attack=(ushort)power;formula.attack_data[0].attack_zokusei=(ushort)(physical?16:32);
                w.pm_data[0].calc_para[2]=defense;w.pm_data[0].calc_para[3]=defense;
                int raw=Math.Max(0,physical?power-defense:power+actor.calc_para[1]-defense);
                UnityEngine.Random.InitState(seed);int expected=CombatReforgedRules.BaseDamage(power,actor.calc_para[1],defense,defense,physical,OldSrcBase.get_rand(raw<=5?2:8)+1);
                UnityEngine.Random.InitState(seed);Check(actor.CalcMonsterDamage(0,0)==expected);
            }
            var player=w.pm_data[0].Cast<BattlePlayerCharacter>();
            foreach(int attack in new[]{20,100,280})foreach(int defense in new[]{5,90,250})for(int seed=0;seed<4;seed++)
            {
                player.calc_para[0]=attack;actor.calc_para[2]=defense;int raw=Math.Max(0,attack-defense);
                UnityEngine.Random.InitState(seed);int expected=raw+OldSrcBase.get_rand_maxlimit(raw<=5?2:8)+1;
                UnityEngine.Random.InitState(seed);Check(player.CalcPlayerBaseDamage(6)==expected);
            }
            if(Plugin.Options.EnableRuneBalancing.Value&&CombatReforgedRuntime.Config.EnableHealingBalance)
            {
                var baseline=ContentRules.Read<ContentBaseline>(ContentRuntime.Resource("baseline.json"));
                foreach(var edit in CombatReforgedRuntime.Config.SpellEffects)
                {
                    var b=baseline.Spells.Single(s=>s.Id==edit.Id);
                    var magic=new MAGI_DATA{name=b.Name,text=b.Text,atrb=b.Attribute,uni_no=b.Unite,uni_magic=b.UniteMagic,info=new Il2CppStructArray<byte>(b.Info),status=new Il2CppStructArray<byte>(b.Status),eff_val=b.Effect};
                    ContentRuntime.Spell(edit.Id,magic);Check(magic.eff_val==edit.Effect);ContentRuntime.Spell(edit.Id,magic);Check(magic.eff_val==edit.Effect);Check(magic.info.SequenceEqual(b.Info)&&magic.status.SequenceEqual(b.Status));
                }
            }
            for(int i=7;i<12;i++)w.pm_data[i]=null!;
            foreach(var b in CombatReforgedRuntime.Audit.Bosses)
            {
                var id=new EncounterIdentity(b.TableId,b.PartyId,0,b.Flags,6,1,b.Hooks[5]!=0,true,b.Hooks,b.HookNames,new[]{b.EnemyId},new[]{b.Signature[0]},new[]{1,0,0,0,0,0},new[]{0,1,0,0,0,0},0,true);
                Check(CombatReforgedRules.BossMatches(b,id));Check(!CombatReforgedRules.BossMatches(b,id with{HasEndHandler=false}));
                var data=Template(b.EnemyId,b.Signature);created.Add(data);w.monster_data[0]=data;actor.hp=(short)data.hp;actor.max_hp=data.hp;
                var table=UnityEngine.ScriptableObject.CreateInstance<BATTLE_DATA>();auxiliary.Add(table);table.id=b.TableId;w.battle_data=table;
                var party=UnityEngine.ScriptableObject.CreateInstance<MONSTER_PARTY_DATA>();auxiliary.Add(party);party.id=b.PartyId;party.monster_kazu=1;party.party_flg=b.Flags;
                party.monster_id=new Il2CppStructArray<sbyte>(new sbyte[]{1,0,0,0,0,0});party.monster_code=new Il2CppStructArray<sbyte>(new sbyte[]{0,1,0,0,0,0});
                party.data=new MONSTER_PARTY_SUB_DATA{battle_start_func_index=b.Hooks[0],phase_start_func_index=b.Hooks[1],phase_end_func_index=b.Hooks[2],battle_end_func_index=b.Hooks[3],command_func_index=b.Hooks[4],battle_main_func_index=b.Hooks[5],be_data=new BE_DATA()};w.monster_party_data=party;w.monster_party_index=1;
                // Presence-only fixture delegates are never executed. The production
                // game keeps its real original handlers and progression callbacks.
                w.monster_be_func=(BattleOverlayDelegate)(Func<BATTLE_WORK,int>)(_=>0);
                w.monster_bm_func=b.Hooks[5]==0?null:(BattleOverlayDelegate)(Func<BATTLE_WORK,int>)(_=>0);
                var state=new BattleState(manager){Ready=true,Allowed=false};Combat.State=state;
                CombatReforgedRuntime.Ready(state,w,true);Check(state.StatsAllowed);Check(CombatReforgedRuntime.Plans.TryGetValue(actor.Pointer,out var plan)&&plan.Boss);
                var setting=CombatReforgedRuntime.Config.Bosses.Single(s=>s.TableId==b.TableId);
                Combat.ScaleEnemy(actor);Check(actor.max_hp==Rules.Scale(b.Signature[0],setting.HpMultiplier,32767));int once=actor.max_hp;Combat.ScaleEnemy(actor);Check(actor.max_hp==once);
                Check(CombatReforgedRuntime.Factor(state,actor,1.45,false)==setting.DamageMultiplier);
                Results.Add(new{Kind="BossProductionAdmissionAndScaling",b.TableId,BaseHp=b.Signature[0],FinalHp=once,NoDoubleScaling=true,StoryCallbacksExecuted=false});
                actor.hp=(short)data.hp;actor.max_hp=data.hp;setting.Enabled=false;var disabled=new BattleState(manager){Ready=true,Allowed=false};CombatReforgedRuntime.Ready(disabled,w,true);Check(!disabled.StatsAllowed);setting.Enabled=true;
                Combat.State=null;
            }
            var ordinary=CombatReforgedRuntime.Audit.Enemies[0];var ordinaryData=Template(ordinary.Id,ordinary.Signature);created.Add(ordinaryData);w.monster_data[0]=ordinaryData;actor.hp=(short)ordinaryData.hp;actor.max_hp=ordinaryData.hp;
            w.monster_bm_func=w.monster_be_func=null;
            var noPacing=new BattleState(manager){Ready=true,Allowed=true,StatsAllowed=true};Combat.State=noPacing;CombatReforgedRuntime.Config.EnablePacing=false;CombatReforgedRuntime.Ready(noPacing,w,true);
            Check(CombatReforgedRuntime.Plans.TryGetValue(actor.Pointer,out var unpaced)&&unpaced.Hp==null);Check(CombatReforgedRuntime.Factor(noPacing,actor,1.6,true)==1.6);Check(JsonSerializer.Serialize(CombatReforgedRuntime.Snapshot()).Length>0);
        }
        finally
        {
            CombatReforgedRuntime.Config.EnableEnemyAi=ai;CombatReforgedRuntime.Probe=false;CombatReforgedRuntime.ProbeWork=null;CombatReforgedRuntime.Clear();Framework.Singleton<BATTLE_WORK>.instance=saved;Framework.Singleton<GAME_WORK>.instance=savedGame;UnityEngine.Random.state=rng;
            Combat.State=null;foreach(var (setting,value) in savedBossSettings)setting.Enabled=value;
            CombatReforgedRuntime.Config.EnablePacing=savedPacing;
            foreach(var obj in created)UnityEngine.Object.Destroy(obj);
            foreach(var obj in auxiliary)UnityEngine.Object.Destroy(obj);
        }
        Check(Framework.Singleton<BATTLE_WORK>.instance?.Pointer==saved?.Pointer);Check(Framework.Singleton<GAME_WORK>.instance?.Pointer==savedGame?.Pointer);var restored=UnityEngine.Random.state;Check(restored.s0==rng.s0&&restored.s1==rng.s1&&restored.s2==rng.s2&&restored.s3==rng.s3);
        Passed=true;string dir=Path.Combine(Paths.BepInExRootPath,"ReforgedNightmare","combat");Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"native-combat-probe.json"),JsonSerializer.Serialize(Snapshot(),new JsonSerializerOptions{WriteIndented=true}));
        Plugin.Logger.LogInfo($"Combat Reforged probe PASS: {Assertions} assertions; native mode differences={ModeDifferences}; native target differences={TargetDifferences}; formulas checked; work/RNG restored. No battle/story/save played.");
    }
    internal static object Snapshot()=>new{Version="1.12.2-guild-ui.1",Ran=Assertions>0,Passed,Assertions,ModeDifferences,TargetDifferences,Results=Results.ToArray(),BattlefieldPlayed=false,StoryCallbacksInvoked=false};
}
