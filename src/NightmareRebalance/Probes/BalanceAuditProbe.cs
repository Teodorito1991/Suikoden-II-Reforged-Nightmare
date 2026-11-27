using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Runtime.CompilerServices;

namespace NightmareRebalance;

// Explicit unloaded production-audit extension; no campaign, scene or outcome.
internal static class BalanceAuditProbe
{
    static T Allocate<T>() where T : Il2CppObjectBase
    {
        RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
        return (T)Activator.CreateInstance(typeof(T), IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr))!;
    }
    internal static bool Requested => ProductionAssetAudit.Requested &&
        UnityEngine.Application.isBatchMode &&
        Environment.GetCommandLineArgs().Contains("--reforged-balance-audit");

    internal static void Growth()
    {
        if (!Requested) return;
        var baseline = ContentRules.Read<ContentBaseline>(ContentRuntime.Resource("baseline.json"));
        var profiles = ContentRules.Characters(ContentRuntime.Resource("characters.json"), baseline);
        var rows = new List<object>();
        foreach (var original in profiles.Characters)
        {
            var current = RpgEvolutionRuntime.Character(original);
            var vanilla = baseline.Characters.Single(x => x.Id == current.Id);
            foreach (int level in new[] {12, 30, 55, 65})
            {
                double Gain(byte[] growth, int stat)
                {
                    int curve = ContentRules.Nibble(growth, stat);
                    double sum = 0;
                    // Expected fixed-point growth only: excludes starting/recruitment
                    // stats, gear, runes, caps and random rounding of individual levels.
                    for (int l = 2; l <= level; l++)
                    {
                        int index = stat == 7 ? curve * 3 + (l >= 20 ? 1 : 0) + (l >= 60 ? 1 : 0)
                            : G2_LEVEL.GetParaSeicyoIndex(l, curve);
                        var table = stat == 7 ? G2_LEVEL.hp_up_table : G2_LEVEL.noryoku_up_table;
                        if (index < 0 || index >= table.Length) throw new InvalidOperationException("Unknown native growth index");
                        sum += table[index] / (stat == 7 ? 512.0 : 256.0);
                    }
                    return Math.Round(sum, 3);
                }
                rows.Add(new { current.Id, current.Name, current.Role, Level = level,
                    VanillaGrowthGain = Enumerable.Range(0, 8).Select(s => Gain(vanilla.Growth, s)).ToArray(),
                    ModifiedGrowthGain = Enumerable.Range(0, 8).Select(s => Gain(current.Growth, s)).ToArray(),
                    VanillaSlots = vanilla.Slots.Select(x => (int)x).ToArray(),
                    ModifiedSlots = current.Slots.Select(x => (int)x).ToArray(),
                    NativeCurveLookup = true, ActualCharacterLevelled = false });
            }
        }
        ProductionAssetAudit.Rows.Add(new { Kind = "BalanceGrowthAudit", Rows = rows.ToArray(),
            ExpectedGainsOnly = true, FullBattlePlayed = false });
        var prior = Framework.Singleton<GAME_WORK>.instance;
        try
        {
            var game = Allocate<GAME_WORK>();
            game.table_data = GuildNativeProbe.Data!.table_data;
            game.chara_data = Allocate<CHARA_DATA>();
            game.chara_data.c_varia_dat = new Il2CppReferenceArray<C_VARIA_DAT>(1);
            var character = new C_VARIA_DAT();game.chara_data.c_varia_dat[0] = character;
            Framework.Singleton<GAME_WORK>.instance = game;
            var prices = new List<object>();
            foreach (int level in new[] {1, 4, 8, 12, 15})
            {
                character.buki_lv = (byte)level;
                int cost = G2_SYS.G2_arms_val(0);
                if(cost != game.table_data.arms_val[level-1])throw new InvalidOperationException("Unknown native weapon price lookup");
                prices.Add(new { StoredWeaponLevel=level, NativePrice=cost });
            }
            ProductionAssetAudit.Rows.Add(new {Kind="BalanceWeaponPrices", Rows=prices.ToArray(), NativePriceLookupExecuted=true, UpgradePurchased=false});
        }
        finally {Framework.Singleton<GAME_WORK>.instance=prior;}
    }

    internal static void PlayerDamage(BattlePlayerCharacter player, BattleMonsterCharacter target)
    {
        if(!Requested)return;
        var oldPlayer=player.calc_para;var oldTarget=target.calc_para;var rng=UnityEngine.Random.state;
        try
        {
            foreach(var model in new[]{(Name:"Early",Attack:30,Auxiliary:15,Defense:8),
                (Name:"Middle",Attack:95,Auxiliary:75,Defense:45),(Name:"Late",Attack:160,Auxiliary:130,Defense:110)})
            {
                player.calc_para=new Il2CppStructArray<int>(new[]{model.Attack,70,60,50,80,75,60,model.Auxiliary});
                target.calc_para=new Il2CppStructArray<int>(new[]{120,70,model.Defense,50,80,75,60,100});
                var samples=new int[8];
                for(int seed=0;seed<8;seed++){UnityEngine.Random.InitState(1700+seed);samples[seed]=player.CalcPlayerDamage(6);}
                ProductionAssetAudit.Rows.Add(new {Kind="BalanceNativePlayerDamage",model.Name,PhysicalAttack=model.Attack,AuxiliaryParameter7=model.Auxiliary,
                    EnemyDefense=model.Defense,Samples=samples,AssumedStats=true,ActualCharacterParty=false});
            }
        }
        finally {player.calc_para=oldPlayer;target.calc_para=oldTarget;UnityEngine.Random.state=rng;}
    }

    internal static void Setup(BATTLE_DATA table)
    {
        if(!Requested || table.id is not ("va01_00" or "vc12_00" or "va11_00"))return;
        var priorGame=Framework.Singleton<GAME_WORK>.instance;
        var priorWork=Framework.Singleton<BATTLE_WORK>.instance;
        var priorState=Combat.State;var rng=UnityEngine.Random.state;
        var priorCopy=CombatEvolutionRuntime.LastCopy;var priorFormation=CombatEvolutionRuntime.LastFormation;
        int applications=CombatEvolutionRuntime.FormationApplications;
        var copies=new List<MONSTER_DATA>();var parties=new List<MONSTER_PARTY_DATA>();
        try
        {
            var game=Allocate<GAME_WORK>();
            game.sys_work=Allocate<SYS_WORK>();game.sys_work.battle_data=table;
            game.sys_work.yobi=new Il2CppStructArray<byte>(8);game.sys_work.yobi[0]=1;
            game.game_data=Allocate<GAME_DATA>();game.party_data=Allocate<PARTY_DATA>();
            Framework.Singleton<GAME_WORK>.instance=game;Combat.State=null;
            var manager=Allocate<BattleManager>();
            foreach(int difficulty in new[]{0,2})
            {
                var work=Allocate<BATTLE_WORK>();Framework.Singleton<BATTLE_WORK>.instance=work;
                game.game_data.difficulty=(byte)difficulty;
                manager.BattleDataSetup();
                foreach(var local in work.monster_data.Where(x=>x!=null))
                {
                    copies.Add(local);
                    var source=table.monster_data.First(x=>x!=null&&x.id==local.id);
                    if(local.Pointer==source.Pointer || !local.para.SequenceEqual(source.para) ||
                        local.hp!=MaintenanceRules.NativeHp(source.hp,ConstShare.GetDifficultyRate(difficulty)))
                        throw new InvalidOperationException("Actual native difficulty setup differs from audited HP/parameter path");
                    ProductionAssetAudit.Rows.Add(new {Kind="BalanceActualDifficultySetup",Table=table.id,
                        Enemy=local.id,Difficulty=difficulty,SourceHp=source.hp,NativeHp=local.hp,
                        ParametersUnchanged=true,OriginalSetupExecuted=true,SceneLoaded=false});
                }
                if(work.monster_party_data.Pointer!=table.monster_party[0].Pointer)parties.Add(work.monster_party_data);
            }
        }
        finally
        {
            foreach(var copy in copies)UnityEngine.Object.Destroy(copy);
            foreach(var party in parties)UnityEngine.Object.Destroy(party);
            CombatEvolutionRuntime.LastCopy=priorCopy;CombatEvolutionRuntime.LastFormation=priorFormation;
            CombatEvolutionRuntime.FormationApplications=applications;Combat.State=priorState;
            Framework.Singleton<GAME_WORK>.instance=priorGame;Framework.Singleton<BATTLE_WORK>.instance=priorWork;
            UnityEngine.Random.state=rng;
        }
    }
}
