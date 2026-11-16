using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NightmareRebalance;

// Launcher-only research. The original request and data setup run on scratch
// owners. No map update, chapter request, callback, camera or battle tick runs.
internal static class HallEchoesProbe
{
    internal static bool Requested => Application.isBatchMode && ProductionAssetAudit.Requested &&
        Environment.GetCommandLineArgs().Contains("--reforged-hall-echoes-probe") &&
        Environment.GetCommandLineArgs().Contains("--reforged-private-startup-smoke");

    internal static object? Evidence;
    internal static object[] GuestEvidence = Array.Empty<object>();

    internal static void InspectGuests(IMPORT_GAME_DATA assets, GAME_WORK scratch)
    {
        if (!Requested) return;
        var previous = scratch.chara_data.c_varia_dat;
        var original = assets.all_data_table.c_kotei_dat;
        try
        {
            var entries = new Il2CppReferenceArray<C_VARIA_DAT>(original.Length);
            scratch.chara_data.c_varia_dat = entries;
            GuestEvidence = new[]{80,82}.Select(id =>
            {
                Check(id<original.Length && original[id] is {mon_kosu_type.Length:3,monsyo_seigen.Length:4},
                    "original fixed character record for guest candidate");
                var candidate = new C_VARIA_DAT();
                entries[id] = candidate;
                Check(G2_SYS.G2_varia_dat(id)?.Pointer == candidate.Pointer,
                    "native variable lookup aliases the installed campaign record");
                return (object)new { CharacterId=id, OriginalFixedRecord=true,
                    VariableRecordAlias=true, FullActorSetup=false, SubstituteInstalled=false };
            }).ToArray();
        }
        finally { scratch.chara_data.c_varia_dat = previous; }
    }

    static T Allocate<T>() where T : Il2CppObjectBase
    {
        RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
        return (T)Activator.CreateInstance(typeof(T), IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;
    }

    static void Check(bool valid, string what)
    {
        if (!valid) throw new InvalidOperationException("Hall of Echoes fixture: " + what);
    }

    internal static void Inspect(BATTLE_DATA original, FormationAudit audit)
    {
        if (!Requested || original.id != "boss_rat") return;
        Check(Combat.State == null && !WarRuntime.Active && !HuntLaunchProof.PendingWalkingRequest &&
            !HuntAccess.Ticket.Armed, "unloaded launcher and idle battle hooks");
        var source = original.monster_party[audit.Indices.Single()];
        var monster = original.monster_data[audit.Monsters.Single().Index];
        Check(source.id == audit.PartyId && source.party_flg == audit.Flags &&
            source.item_no == audit.Item && source.monster_kazu == 1 &&
            source.monster_code.Select(x => (int)x).SequenceEqual(audit.OldCodes) &&
            source.monster_id.Select(x => (int)x).SequenceEqual(audit.OldIds) &&
            source.data.monster_position.Select(x => (int)x).SequenceEqual(audit.Positions) &&
            source.data.be_data.command_num == 0 && source.data.be_data.com_data.Length == 0 &&
            CombatEvolutionRuntime.Hooks(source.data).SequenceEqual(audit.Hooks) &&
            monster.id == audit.Monsters.Single().Id &&
            CombatReforgedRuntime.Signature(monster).SequenceEqual(audit.Monsters.Single().Signature),
            "exact source and move signature");

        Plugin.Logger.LogInfo("Hall of Echoes fixture: exact source audited.");
        var opd = monster.opd_data;
        Check(opd != null && opd.anime_kazu > 0 && opd.pattern_kazu > 0 &&
            opd.anime_table.Count > 0 && opd.pattern_table.Count > 0, "original sprite animation records");

        Plugin.Logger.LogInfo("Hall of Echoes fixture: original sprite records loaded.");
        var previousGame = Framework.Singleton<GAME_WORK>.instance;
        var previousWork = Framework.Singleton<BATTLE_WORK>.instance;
        var rng = UnityEngine.Random.state;
        var clone = ScriptableObject.CreateInstance<BATTLE_DATA>();
        var field = ScriptableObject.CreateInstance<MACHIDAT>();
        var party = CombatEvolutionRuntime.CopyParty(source, audit.OldCodes);
        var copies = new List<MONSTER_DATA>();
        var legalModes = new HashSet<int>();
        int baseHp = monster.hp, rematchHp = 0;
        try
        {
            party.id = "echo_fixture_rat_00";
            party.party_flg = 0;
            party.item_no = 0;
            party.data.battle_start_func_index = 0;
            party.data.phase_start_func_index = 0;
            party.data.phase_end_func_index = 0;
            party.data.battle_end_func_index = 0;
            party.data.command_func_index = 0;
            party.data.battle_main_func_index = 0;
            clone.id = "echo_fixture_rat";
            clone.is_battle_field_data = original.is_battle_field_data;
            clone.battle_field_data = original.battle_field_data;
            clone.monster_party_num = 1;
            clone.monster_party = new Il2CppReferenceArray<MONSTER_PARTY_DATA>(new[]{party});
            clone.monster_data = new Il2CppReferenceArray<MONSTER_DATA>(original.monster_data.ToArray());
            field.battle_dat = new Il2CppReferenceArray<BATTLE_DATA>(2);
            field.battle_dat[1] = clone;

            var game = Allocate<GAME_WORK>();
            game.sys_work = Allocate<SYS_WORK>();
            game.sys_work.yobi = new Il2CppStructArray<byte>(8);
            game.game_data = Allocate<GAME_DATA>();
            game.party_data = Allocate<PARTY_DATA>();
            Framework.Singleton<GAME_WORK>.instance = game;

            for (int run = 0; run < 2; run++)
            {
                var mcon = Allocate<MACHICON>();
                var eventcon = Allocate<EVENTCON>();
                mcon.mdat = field;
                mcon.econ = eventcon;
                eventcon.mcon = mcon;
                eventcon.mdat = field;
                game.sys_work.mcon = mcon;
                game.sys_work.battle_data = null;
                game.sys_work.battle_no = 0;
                Plugin.Logger.LogInfo($"Hall of Echoes fixture: executing request {run+1}.");
                eventcon.EventBatlleSet(1,0);
                Check(mcon._mstep == 40 && mcon.btltyp == 0 && game.sys_work.battle_no == -1 &&
                    game.sys_work.battle_data?.Pointer == clone.Pointer, "native ordinary request writes the field handoff");

                var manager = Allocate<BattleManager>();
                var w = Allocate<BATTLE_WORK>();
                Framework.Singleton<BATTLE_WORK>.instance = w;
                // The native explicit one-based selector is a separate existing path.
                // The random branch for one record reached a trap in the patched
                // get_rand_maxlimit(0) routine in this copied runtime.
                game.sys_work.yobi[0] = 1;
                Plugin.Logger.LogInfo($"Hall of Echoes fixture: executing explicit setup {run+1}.");
                manager.BattleDataSetup();
                copies.AddRange(w.monster_data.Where(x => x != null && !original.monster_data.Any(s => s != null && s.Pointer == x.Pointer)));
                Check(w.battle_data.Pointer == clone.Pointer && w.monster_party_index == 1,
                    "native setup retained the request table and explicit index");
                Check(w.monster_party_data.Pointer == party.Pointer &&
                    CombatEvolutionRuntime.Hooks(w.monster_party_data.data).All(h => h == 0) &&
                    w.monster_party_data.data.be_data.command_num == 0 && w.monster_party_data.party_flg == 0,
                    "native setup selected detached, callback-free party");
                // Valid explicit selections can remain in the scratch sys_work.
                // Never let the test carry that value into the second request.
                game.sys_work.yobi[0] = 0;
                int code = audit.Monsters.Single().Index;
                var local = w.monster_data[code];
                Check(local.Pointer != monster.Pointer && local.hp == baseHp, "original native battle-local monster clone");
                local.hp = (ushort)(baseHp * 5 / 4);
                rematchHp = local.hp;
                Plugin.Logger.LogInfo($"Hall of Echoes fixture: building scratch actors {run+1}.");
                HuntCatalogueProbe.BuildActors(manager,w,audit.OldCodes);
                var actor = w.pm_data[6].Cast<BattleMonsterCharacter>();
                Check(actor.hp == rematchHp && actor.monster_data.Pointer == local.Pointer, "disposable rematch HP recognized by actor fixture");
                Plugin.Logger.LogInfo($"Hall of Echoes fixture: checking legal modes {run+1}.");
                for (int seed = 0; seed < 24; seed++)
                {
                    UnityEngine.Random.InitState(14100+seed);
                    int mode = actor.SelectAttackMode();
                    Check(mode is >= 0 and < 4 && actor.monster_data.attack_data[mode].attack_prob > 0 &&
                        actor.CheckMonsterAttackCnt(mode) && actor.AttackModeKyoriCheck(mode), "original legal selector on rematch clone");
                    legalModes.Add(mode);
                }
                Check(monster.hp == baseHp && source.party_flg == audit.Flags &&
                    CombatEvolutionRuntime.Hooks(source.data).SequenceEqual(audit.Hooks), "original source remains untouched");
            }
        }
        finally
        {
            Framework.Singleton<BATTLE_WORK>.instance = previousWork;
            Framework.Singleton<GAME_WORK>.instance = previousGame;
            HuntLaunchProof.Clear();
            UnityEngine.Random.state = rng;
            foreach (var copy in copies) UnityEngine.Object.Destroy(copy);
            UnityEngine.Object.Destroy(field);
            UnityEngine.Object.Destroy(clone);
            UnityEngine.Object.Destroy(party);
        }

        Check(Framework.Singleton<GAME_WORK>.instance?.Pointer == previousGame?.Pointer &&
            Framework.Singleton<BATTLE_WORK>.instance?.Pointer == previousWork?.Pointer &&
            UnityEngine.Random.state.Equals(rng), "global identities and RNG restored");
        Evidence = new { Source="boss_rat", BaseHp=baseHp, RematchHp=rematchHp, RequestStep=40,
            SpriteAnimations=opd!.anime_kazu, LegalModes=legalModes.OrderBy(i => i).ToArray(),
            NativeRequestCalls=2, NativeSetupCalls=2, NativeSelectorCalls=48,
            CampaignLoaded=false, ChapterDispatched=false, BattleRendered=false, FieldReturned=false };
        Plugin.Logger.LogInfo("Hall of Echoes fixture PASS: native ordinary request and Pest Rat setup repeated twice.");
    }
}
