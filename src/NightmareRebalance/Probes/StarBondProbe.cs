using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;

namespace NightmareRebalance;

internal static class StarBondProbe
{
    internal static readonly List<object> Results = new();

    static T Allocate<T>() where T : Il2CppObjectBase
    {
        RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
        return (T)Activator.CreateInstance(typeof(T),IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;
    }

    static void Check(bool result, string why)
    {
        if (!result) throw new InvalidOperationException("Star Bonds native probe: " + why);
    }

    internal static void Run(IMPORT_GAME_DATA assets, Il2CppReferenceArray<UNITE_DATA> unites, GAME_WORK game, BATTLE_WORK w)
    {
        var manager = Allocate<BattleManager>();
        var state = Combat.State;
        var rng = UnityEngine.Random.state;
        bool enabled = Plugin.Options.EnableStarBonds.Value;
        var definition = StarBondRules.Catalogue;
        try
        {
            var table = GuildNativeProbe.ReissueTable ?? throw new InvalidOperationException("Expected an original battle table");
            w.battle_data = table;
            w.monster_data = table.monster_data;
            w.monster_party_data = table.monster_party[0];
            w.unite_data_table = unites;
            game.game_data = Allocate<GAME_DATA>();
            game.party_data = Allocate<PARTY_DATA>();
            game.party_data.party_cha_no = new Il2CppStructArray<byte>(6);
            game.chara_data.c_varia_dat = new Il2CppReferenceArray<C_VARIA_DAT>(85);
            HuntCatalogueProbe.BuildActors(manager,w,new[]{1,0,0,0,0,0});
            var player = w.pm_data[0].Cast<BattlePlayerCharacter>();
            var enemy = w.pm_data[6].Cast<BattleMonsterCharacter>();

            void Put(int slot, int id)
            {
                var unit=w.pm_data[slot];
                unit.chara_id=(byte)id;
                w.b_chara[slot].chara_id=id;
                game.party_data.party_cha_no[slot]=(byte)id;
                game.chara_data.c_varia_dat[id]=w.c_varia_dat[slot];
                if (game.chara_data.c_kotei_dat[id]==null)
                {
                    var source=assets.all_data_table.c_kotei_dat[id];
                    game.chara_data.c_kotei_dat[id]=new C_KOTEI_DAT
                    {
                        seicyo_type=new Il2CppStructArray<byte>(source.seicyo_type.ToArray()),
                        mon_zoku_aisyo=new Il2CppStructArray<byte>(source.mon_zoku_aisyo.ToArray()),
                        mon_kosu_type=new Il2CppStructArray<byte>(source.mon_kosu_type.ToArray()),
                        monsyo_seigen=new Il2CppStructArray<byte>(source.monsyo_seigen.ToArray()),
                        soubi_henkou=source.soubi_henkou, soubi_kanou=source.soubi_kanou, okori_panic=source.okori_panic
                    };
                }
            }

            // Use only fixture-local actor HP/status and charge records.
            void Party(StarBond bond)
            {
                for (int i=0;i<6;i++)
                {
                    Put(i,i<bond.Members.Length ? bond.Members[i] : 1);
                    w.pm_data[i].hp=600;
                    w.pm_data[i].status=0;
                    w.pm_data[i].dead_flg=0;
                }
                enemy.hp=600;
                enemy.max_hp=600;
                enemy.status=0;
            }

            Plugin.Options.EnableStarBonds.Value=true;
            var battle = new BattleState(manager){Ready=true,Allowed=true,StatsAllowed=true,Bonds=StarBondRuntime.Eligible(w)};
            Combat.State=battle;
            Check(battle.Bonds,"Normal native record admitted");
            game.game_data.difficulty=2;
            Check(!StarBondRuntime.Eligible(w),"Hard excluded");
            game.game_data.difficulty=0;

            int Seeded(Func<int> native)
            {
                UnityEngine.Random.InitState(222);
                return native();
            }
            void Measure(StarBond bond, Func<int> native, bool reduction, Action removeCondition, int cap)
            {
                Check(StarBondRuntime.Available(w,0,bond.Unite),"original member order and function for "+bond.Title);
                Check(StarBondRules.Includes(bond,StarBondRuntime.FieldParty()),"party preview identifies exact current IDs");
                Plugin.Options.EnableStarBonds.Value=false;
                int baseline=Seeded(native);
                var baselineRng=UnityEngine.Random.state;
                Plugin.Options.EnableStarBonds.Value=true;
                int revised=Seeded(native);
                Check(baselineRng.Equals(UnityEngine.Random.state),"native RNG preserved by "+bond.Title);
                int bonus=StarBondRules.DamageBonus(baseline,8,cap);
                Check(bonus>0 && revised==baseline+(reduction?-bonus:bonus),"native bounded modifier "+bond.Title);
                Check(Seeded(native)==revised,"repeat calculation does not compound "+bond.Title);
                var partner=w.pm_data[1];
                partner.hp=0;partner.dead_flg=1;
                Check(Seeded(native)==baseline,"KO removes "+bond.Title);
                partner.hp=600;partner.dead_flg=0;
                Put(1,1);
                Check(Seeded(native)==baseline,"removed partner disables "+bond.Title);
                Put(1,bond.Members[1]);
                removeCondition();
                Plugin.Options.EnableStarBonds.Value=false;
                int open=Seeded(native);
                Plugin.Options.EnableStarBonds.Value=true;
                Check(Seeded(native)==open,"normal behavior when trigger absent "+bond.Title);
                Results.Add(new{bond.Title,Baseline=baseline,WithBond=revised,SameRng=true,RepeatedCalculation=true});
            }

            Party(definition[0]);
            player.hp=270;player.status=b_game_h.STATUS_GUARD;
            int physical=Array.FindIndex(enemy.monster_data.attack_data.ToArray(),a=>a!=null && a.attack_prob>0 &&
                a.attack>0 && (a.attack_zokusei & 0x10)!=0 && a.attack_type is 1 or 2);
            Check(physical>=0,"loaded enemy has an original physical move");
            player.calc_para[2]=0;
            Measure(definition[0],()=>enemy.CalcMonsterDamage(0,physical),true,()=>player.status=0,20);

            Party(definition[1]);
            enemy.status=b_game_h.STATUS_POISON;
            player.c_koudou=battle_h.CMD_ATTACK;
            player.calc_para[0]=195;
            StarBondRuntime.AttackOwner=player.Pointer; // Dispatcher context supplied by the fixture.
            Measure(definition[1],()=>player.CalcPlayerDamage(6),false,()=>enemy.status=0,15);
            StarBondRuntime.AttackOwner=IntPtr.Zero;

            Party(definition[2]);
            player.c_koudou=battle_h.CMD_MAGIC;
            player.calc_para[1]=72;
            enemy.calc_para[2]=90;enemy.calc_para[3]=30;
            var fire=assets.magi_data_table[2];
            StarBondRuntime.FireOwner=player.Pointer; // Native damage helper, without a full spell cast.
            Measure(definition[2],()=>player.PlayerCalcMagicDamage(fire,6),false,()=>enemy.calc_para[2]=10,18);
            StarBondRuntime.FireOwner=IntPtr.Zero;

            Party(definition[3]);
            player.c_koudou=battle_h.CMD_UNITE;
            enemy.hp=1500;enemy.max_hp=4000;
            enemy.calc_para[2]=30;
            var action = new ACTION_DATA { chara_no=0 };
            action.CreateReActionData(1);
            int Bandit()
            {
                enemy.status=0;
                player.UniteAttackDataSet(42,action,6,0,100);
                return action.GetReActionData().damage;
            }
            Measure(definition[3],Bandit,false,()=>enemy.hp=3000,24);
            Check(StarBondRuntime.Bandit==null,"native Unite dispatch scope unwound");

            Party(definition[0]);
            Combat.NonVictory(manager);
            Check(!StarBondRuntime.Available(w,0,30),"non-victory clears");
            Combat.State=new BattleState(manager){Ready=true,Allowed=true,Bonds=false};
            Check(!StarBondRuntime.Available(w,0,30),"replacement battle cannot inherit admission");
            Combat.State=null;
            Check(!StarBondRuntime.Available(w,0,30),"teardown clears");
            Check(w.c_varia_dat.All(unit=>unit==null||unit.mp.All(x=>x==0)),"bond calculations spend no extra rune charges");
            Plugin.Logger.LogInfo($"Star Bonds native probe PASS: {Results.Count} paired damage cases with original helpers; no campaign executed.");
        }
        finally
        {
            Plugin.Options.EnableStarBonds.Value=enabled;
            Combat.State=state;
            StarBondRuntime.AttackOwner=StarBondRuntime.FireOwner=IntPtr.Zero;
            StarBondRuntime.Bandit=null;
            UnityEngine.Random.state=rng;
        }
    }
}
