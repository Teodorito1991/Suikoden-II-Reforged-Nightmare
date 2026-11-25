using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace NightmareRebalance;

internal static class RpgEvolutionProbe
{
    static AsyncOperationHandle<UNITE_DATAS>? handle;
    static bool finished;

    static T Allocate<T>() where T : Il2CppObjectBase
    {
        RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
        return (T)Activator.CreateInstance(typeof(T),
            IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;
    }

    static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException("RPG Evolution native: " + message);
    }

    internal static bool Step()
    {
        if (finished) return true;
        if (handle == null)
        {
            handle = Addressables.LoadAssetAsync<UNITE_DATAS>((Il2CppSystem.String)"gsd2-classic-unite-bc_udat");
            return false;
        }
        if (!handle.IsDone) return false;
        try
        {
            Check(handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null, "original Unite table loaded");
            Inspect(GuildNativeProbe.Data!, handle.Result!.unite_data);
            BalanceAuditProbe.Growth();
            finished = true;
            return true;
        }
        finally { Addressables.Release(handle); }
    }

    static void Inspect(IMPORT_GAME_DATA assets, Il2CppReferenceArray<UNITE_DATA> unites)
    {
        var audit = RpgEvolutionRuntime.Audit;
        var prior = Framework.Singleton<GAME_WORK>.instance;
        var priorWork = Framework.Singleton<BATTLE_WORK>.instance;
        var game = Allocate<GAME_WORK>();
        var work = Allocate<BATTLE_WORK>();
        try
        {
            Framework.Singleton<GAME_WORK>.instance = game;
            Framework.Singleton<BATTLE_WORK>.instance = work;
            game.chara_data = Allocate<CHARA_DATA>();
            game.chara_data.c_kotei_dat = new Il2CppReferenceArray<C_KOTEI_DAT>(85);
            game.magi_data = new Il2CppReferenceArray<MAGI_DATA>(assets.magi_data_table.Length);
            var profiles = audit.Characters
                .Concat(Plugin.Options.EnablePartyIdentity.Value ? RpgEvolutionRuntime.PartyIdentity.Characters : [])
                .GroupBy(x=>x.Id).Select(group=>group.Last());
            foreach (var profile in profiles)
            {
                var source = assets.all_data_table.c_kotei_dat[profile.Id];
                var growth = source.seicyo_type.ToArray();
                var copy = new C_KOTEI_DAT
                {
                    seicyo_type = new Il2CppStructArray<byte>(growth),
                    mon_zoku_aisyo = new Il2CppStructArray<byte>(source.mon_zoku_aisyo.ToArray()),
                    mon_kosu_type = new Il2CppStructArray<byte>(source.mon_kosu_type.ToArray()),
                    monsyo_seigen = new Il2CppStructArray<byte>(source.monsyo_seigen.ToArray()),
                    soubi_henkou = source.soubi_henkou, soubi_kanou = source.soubi_kanou, okori_panic = source.okori_panic
                };
                game.chara_data.c_kotei_dat[profile.Id] = copy;
                var native = G2_SYS.G2_kotei_dat(profile.Id);
                Check(native.Pointer == copy.Pointer && native.seicyo_type.SequenceEqual(profile.Growth) &&
                    native.mon_kosu_type.SequenceEqual(profile.Slots) && native.mon_zoku_aisyo.SequenceEqual(profile.Affinity), "patched lookup uses revised profile " + profile.Id);
                foreach (int level in new[]{12,30,55})
                    for (int stat=0;stat<8;stat++)
                        Check(G2_LEVEL.GetParaSeicyoIndex(level,G2_LEVEL.g2_seicyo_type(profile.Id,stat)) ==
                            G2_LEVEL.GetParaSeicyoIndex(level,ContentRules.Nibble(profile.Growth,stat)),
                            "original growth tier lookup at representative level");
                Check(source.seicyo_type.SequenceEqual(growth) && native.monsyo_seigen.SequenceEqual(source.monsyo_seigen)
                    && native.soubi_henkou == source.soubi_henkou && native.soubi_kanou == source.soubi_kanou,
                    "source and restrictions preserved");
            }

            foreach (var (id, effect) in audit.Spells.Select(x => (x.Id, (int)x.Effect))
                .Concat(audit.Runes.Select(x => (x.Id, x.Effect))))
            {
                var source = assets.magi_data_table[id];
                var signature = CombatMasteryRuntime.RuneSignature(source);
                var copy = new MAGI_DATA
                {
                    name = source.name, text = source.text, atrb = source.atrb, eff_val = source.eff_val,
                    uni_no = source.uni_no, uni_magic = source.uni_magic,
                    info = new Il2CppStructArray<byte>(source.info.ToArray()),
                    status = new Il2CppStructArray<byte>(source.status.ToArray())
                };
                game.magi_data[id] = copy;
                Check(G2_SYS.G2_magi_data(id).eff_val == effect && G2_SYS.G2_magi_data(id).eff_val == effect,
                    "new coefficient through original lookup " + id);
                signature[3] = effect;
                Check(CombatMasteryRuntime.RuneSignature(copy).SequenceEqual(signature) && source.eff_val != effect,
                    "spell/rune metadata remains original");
            }

            work.unite_data_table = unites;
            Check(RpgEvolutionRuntime.ApplyUnites(work) == audit.Unites.Length, "all original Unite signatures admitted");
            foreach (var entry in audit.Unites)
            {
                var source = unites[entry.Id];
                var copy = work.GetUniteData(entry.Id);
                var signature = entry.Signature.ToArray();
                signature[6] = entry.Effect;
                Check(copy.Pointer != source.Pointer && copy.player_id.Pointer != source.player_id.Pointer &&
                    CombatEvolutionRuntime.UniteSignature(copy).SequenceEqual(signature) &&
                    CombatEvolutionRuntime.UniteSignature(source).SequenceEqual(entry.Signature), "battle-local Unite " + entry.Id);
            }
            Check(RpgEvolutionRuntime.ApplyUnites(work) == 0, "no duplicate Unite rewrite");
            StarBondProbe.Run(assets,unites,game,work);
            HallEchoesProbe.InspectGuests(assets,game);
            Plugin.Logger.LogInfo("RPG Evolution native lookups passed for all revised characters, spells, physical runes and Unites.");
        }
        finally
        {
            Framework.Singleton<GAME_WORK>.instance = prior;
            Framework.Singleton<BATTLE_WORK>.instance = priorWork;
        }
    }
}
