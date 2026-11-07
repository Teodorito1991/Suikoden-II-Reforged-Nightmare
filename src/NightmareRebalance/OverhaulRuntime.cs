using BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace NightmareRebalance;

internal static class OverhaulRuntime
{
    internal static Dictionary<string,EnemyProfile> Enemies=new(StringComparer.Ordinal);
    internal static CommanderFile Commanders=new();
    internal static int EnemyApplications, CommanderApplications;
    internal static string Load(string name)
    {
        string path=Path.Combine(Paths.ConfigPath,"ReforgedNightmare",name);
        if(!File.Exists(path)){Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.WriteAllText(path,ContentRuntime.Resource(name));}
        if(new FileInfo(path).Length>262144)throw new InvalidDataException("File exceeds 256 KiB.");
        return File.ReadAllText(path);
    }
    internal static void Initialize()
    {
        if(Plugin.Options.EnableEnemyProfiles.Value)
            try { var baseline=ContentRules.Read<EnemyFile>(ContentRuntime.Resource("enemy-profiles.json"));Enemies=OverhaulRules.Enemies(Load("enemy-profiles.json"),baseline).Profiles.ToDictionary(e=>e.Id,StringComparer.Ordinal); }
            catch(Exception e){Plugin.Logger.LogError("Enemy profiles disabled: "+e.Message);}
        if(Plugin.Options.EnableWarUnitProfiles.Value)
            try { var baseline=ContentRules.Read<CommanderFile>(ContentRuntime.Resource("war-unit-profiles.json"));Commanders=OverhaulRules.Commanders(Load("war-unit-profiles.json"),baseline); }
            catch(Exception e){Plugin.Logger.LogError("War unit profiles disabled: "+e.Message);}
        Plugin.Logger.LogInfo($"Reforged content: {Enemies.Count} ordinary enemy profiles; {Commanders.Profiles.Length} native war commander/branch profiles. Custom arena retired.");
    }
    internal static EnemyProfile? Register(BattleState state,PM_DATA unit)
    {
        if(state.EnemyPlans.TryGetValue(unit.Pointer,out var selected))return selected;
        var data=unit.monster_data;
        if(data==null||!Enemies.TryGetValue(data.id??"",out var profile)||!OverhaulRules.EnemyEligible(profile,state.Allowed,!unit.IsPlayer&&unit.TryCast<BattleMonsterCharacter>()!=null,DifficultyProvenance.LegacyFixture?unit.max_hp:profile.ExpectedHp,data.name_id,data.level))return null;
        if(!DifficultyProvenance.LegacyFixture&&(!CombatReforgedRuntime.Audit.Enemies.Any(a=>a.Id==data.id&&DifficultyProvenance.Regular(Framework.Singleton<BATTLE_WORK>.instance,data,a.Signature))||unit.max_hp!=data.hp))return null;
        state.EnemyPlans.Add(unit.Pointer,profile);EnemyApplications++;
        Plugin.Debug($"Enemy profile {profile.Id}/{profile.Role}: final HP x{profile.HpMultiplier}, calculated damage x{profile.DamageMultiplier}");
        return profile;
    }
}

internal static class WarRuntime
{
    static readonly WarSession<WAR_CHARA_TYPE> session=new();
    static readonly HashSet<int> unsafeAbilities=new();
    internal static bool Active=>session.Active;
    internal static int Generation=>session.Generation;
    internal static void Arm(){session.Arm();unsafeAbilities.Clear();}
    internal static void Begin(){unsafeAbilities.Clear();session.Begin(Plugin.Options.EnableWarUnitProfiles.Value&&Plugin.CheckCompatibility(),war_h.g_w_war_no);}
    internal static void Clear(){session.Clear();unsafeAbilities.Clear();}
    static string Branch(byte type)=>type==war_data_h.INFANTRY?"Infantry":type==war_data_h.ARCHER?"Archer":type==war_data_h.MAGICIANS?"Magicians":"Unknown";
    static bool Usable(int id)
    {
        if(!OverhaulRules.SafeAbility(id))return false;
        if(!OverhaulRules.ActiveAbility(id))return true;
        // No missing/cut active delegate can be installed even if it appeared in a reference guide.
        return w_chara.GetTokusyuInitFunc(id)!=null&&w_chara.GetTokusyuActFunc(id)!=null&&w_chara.GetTokusyuCheckFunc(id)!=null&&w_chara.GetTokusyuFinishFunc(id)!=null&&w_chara.GetTokusyuSelectFunc(id)!=null;
    }
    internal static WAR_CHARA_TYPE Resolve(WAR_CHARA_TYPE original)
    {
        if(!Active||original==null)return original!;
        if(session.TryGet(original.Pointer.ToInt64(),out var cached))return cached!;
        if(session.Count>=512)return original;
        var profile=TotalWarRuntime.Specialize(original)??OverhaulRules.Select(OverhaulRuntime.Commanders,original.name,Branch(original.force_type));
        if(profile==null)return original;
        var ids=profile.Abilities.Select(a=>Array.IndexOf(OverhaulRules.Abilities,a)).ToArray();
        if(ids.Any(id=>!Usable(id)))
        {if(unsafeAbilities.Add(profile.NameId))Plugin.Logger.LogWarning($"War profile {profile.Name}: missing runtime ability delegates; original record retained.");return original;}
        var copy=Copy(original,profile);
        if(!session.Store(original.Pointer.ToInt64(),copy))return original;
        OverhaulRuntime.CommanderApplications++;
        Plugin.Debug($"War {war_h.g_w_war_no}: disposable profile {profile.Name}; ATK {original.attack}->{copy.attack}, DEF {original.defense}->{copy.defense}");
        return copy;
    }
    internal static WAR_CHARA_TYPE Copy(WAR_CHARA_TYPE original,CommanderProfile profile)
    {
        // Copy every scalar, preserving non-profile fields; allocate every mutable array.
        // Never write a w_dat_* source roster record or a normal saved character record.
        var ids=profile.Abilities.Select(a=>Array.IndexOf(OverhaulRules.Abilities,a)).ToArray();
        var copy=new WAR_CHARA_TYPE { force_type=original.force_type,face_no=original.face_no,name=original.name,big_name=original.big_name,
            attack=profile.Attack,defense=profile.Defense,assist_attack=profile.AssistAttack,assist_defense=profile.AssistDefense,
            magic_no=original.magic_no,status_flag=original.status_flag,yarare_serifu=original.yarare_serifu,sibou_serifu=original.sibou_serifu,
            tokusyu_serifu1=original.tokusyu_serifu1,tokusyu_serifu2=original.tokusyu_serifu2,
            nouryoku=new Il2CppStructArray<war_data_h.tagSPECIAL_ABILITY>(3),kaisu=new Il2CppStructArray<byte>(3),kaisu_max=new Il2CppStructArray<byte>(3) };
        for(int i=0;i<ids.Length;i++){copy.nouryoku[i]=(war_data_h.tagSPECIAL_ABILITY)ids[i];copy.kaisu[i]=profile.Uses[i];copy.kaisu_max[i]=profile.Uses[i];}
        return copy;
    }
    internal static void NativeCopyProbe()
    {
        int checks=0;
        void Check(bool value){checks++;if(!value)throw new InvalidOperationException("Native commander copy probe failed at assertion "+checks);}
        var source=new WAR_CHARA_TYPE {name=3,big_name=77,force_type=1,face_no=4,attack=7,defense=6,assist_attack=1,assist_defense=2,
            magic_no=2,status_flag=5,yarare_serifu=100,sibou_serifu=101,tokusyu_serifu1=102,tokusyu_serifu2=103,
            nouryoku=new Il2CppStructArray<war_data_h.tagSPECIAL_ABILITY>(3),kaisu=new Il2CppStructArray<byte>(3),kaisu_max=new Il2CppStructArray<byte>(3)};
        source.kaisu[0]=1;source.kaisu_max[0]=1;
        var target=new CommanderProfile {Attack=9,Defense=8,AssistAttack=3,AssistDefense=1,Abilities=new[]{"SP_AIMING","SP_HP_PLUS"},Uses=new byte[]{2,0}};
        var copy=Copy(source,target);
        Check(copy.Pointer!=source.Pointer);Check(copy.kaisu.Pointer!=source.kaisu.Pointer);Check(copy.kaisu_max.Pointer!=source.kaisu_max.Pointer);Check(copy.nouryoku.Pointer!=source.nouryoku.Pointer);
        Check(copy.attack==9);Check(copy.defense==8);Check(copy.assist_attack==3);Check(copy.assist_defense==1);
        Check(copy.name==source.name);Check(copy.big_name==77);Check(copy.force_type==1);Check(copy.face_no==4);Check(copy.magic_no==2);Check(copy.status_flag==5);
        Check(copy.yarare_serifu==100);Check(copy.sibou_serifu==101);Check(copy.tokusyu_serifu1==102);Check(copy.tokusyu_serifu2==103);
        Check(copy.nouryoku[0]==war_data_h.tagSPECIAL_ABILITY.SP_AIMING);Check(copy.nouryoku[1]==war_data_h.tagSPECIAL_ABILITY.SP_HP_PLUS);Check(copy.nouryoku[2]==war_data_h.tagSPECIAL_ABILITY.SP_NONE);
        Check(copy.kaisu[0]==2&&copy.kaisu_max[0]==2);copy.kaisu[0]--;
        Check(source.kaisu[0]==1&&source.kaisu_max[0]==1);Check(source.attack==7&&source.defense==6&&source.assist_attack==1&&source.assist_defense==2);
        copy.status_flag=9;Check(source.status_flag==5);
        Plugin.Logger.LogInfo($"Native commander copy probe PASS: {checks} assertions on disposable records. No battlefield launched, campaign record accessed or save modified.");
    }
}
[HarmonyPatch(typeof(WarChapter),nameof(WarChapter.Start))]
internal static class WarChapterStartPatch
{ [HarmonyPrefix]static void Prefix()=>WarRuntime.Arm(); }
[HarmonyPatch(typeof(w_chara),nameof(w_chara.charaInit))]
internal static class WarInitializePatch
{ [HarmonyPostfix]static void Postfix()=>WarRuntime.Begin(); }
[HarmonyPatch(typeof(w_chara),nameof(w_chara.chara))]
internal static class WarCommanderPatch
{ [HarmonyPostfix]static void Postfix(ref WAR_CHARA_TYPE __result){if(__result!=null)__result=WarRuntime.Resolve(__result);} }
[HarmonyPatch(typeof(WarChapter),nameof(WarChapter.OnDestroy))]
internal static class WarExitPatch
{ [HarmonyPostfix]static void Postfix()=>WarRuntime.Clear(); }
