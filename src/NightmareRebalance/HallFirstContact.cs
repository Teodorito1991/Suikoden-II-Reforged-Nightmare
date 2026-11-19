using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace NightmareRebalance;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class HallTrialPatchAttribute : Attribute { }

internal static class HallFirstContact
{
    static Trial? trial;
    static bool faulted;
    internal static bool EnabledAtStartup;
    internal static bool Available => EnabledAtStartup && HallFirstContactDriver.Instance != null &&
        !faulted && trial == null;

    internal static bool Request(int selection = 0)
    {
        if (selection < 0 || selection >= HallTrialCatalogue.Entries.Length ||
            !HallAvailability.CanSelect(selection) ||
            !Available || !CastleGuildRuntime.Context || !Plugin.CheckCompatibility() ||
            CastleGuildRuntime.Owner is not { ano: 10, vno: 6, mno: 3, _mstep: 20 } field ||
            CastleGuildRuntime.Field is not {} events ||
            CastleGuildRuntime.Copy is not {} map || MachiLoader.GetMachiID(field.ano, field.vno) != "vk07" ||
            Framework.Singleton<GAME_WORK>.instance is not {} game ||
            game.sys_work is not {} sys || game.game_data?.difficulty != 0 ||
            sys.mcon?.Pointer != field.Pointer || sys.game_mode != 1 || sys.now_save != 0 ||
            HuntAccess.Ticket.Armed || HuntLaunchProof.PendingWalkingRequest || TrainingWar.Active)
        {
            Plugin.Logger.LogWarning("HallFirstContact: Guild request was rejected before acquiring resources.");
            return false;
        }

        trial = new Trial(HallTrialCatalogue.Entries[selection], game, sys, field, events, map);
        Plugin.Logger.LogInfo($"HallFirstContact: Guild confirmed {trial.Definition.Name}.");
        return true;
    }

    sealed class Trial
    {
        internal readonly HallTrialDefinition Definition;
        internal string? PackId;
        internal bool SourcePrepared;
        internal readonly GAME_WORK Game;
        internal readonly SYS_WORK Sys;
        internal readonly MACHICON Field;
        internal readonly EVENTCON Events;
        internal readonly MACHIDAT Map;
        internal readonly IntPtr Party;
        internal readonly int X, Y;
        internal readonly AsyncOperationHandle<BATTLE_DATA> Handle;
        internal HallPackLease? Pack;
        internal string? SceneName;
        internal SceneLoader? Scenes;
        internal ResourceLoader? SceneResource;
        internal int SceneSlot = -1, FieldSlot = -1;
        internal bool SceneUnloading;
        internal BATTLE_DATA? Table;
        internal MONSTER_PARTY_DATA? Formation;
        internal bool Abort, Committed, Setup, ActorBound, Win, Loss, Released, HpChanged, RatLookup, OutcomeRecorded;
        internal int ActorHp;
        internal int TerminalResult;
        internal bool NativeFieldReturn, AtGuildPosition;
        internal BATTLE_WORK? Work;
        internal readonly HashSet<(string, bool, bool, bool)> PackLookups = new();
        internal int Destroyed = -1;
        internal byte[][]? Flags;
        internal string? War;
        internal string? Progress;
        internal uint Potch;
        internal byte[] RewardIds = Array.Empty<byte>();
        internal int[] RewardCounts = Array.Empty<int>();
        internal Trial(HallTrialDefinition definition, GAME_WORK game, SYS_WORK sys, MACHICON field, EVENTCON events, MACHIDAT map)
        {
            Definition = definition;
            Game = game; Sys = sys; Field = field; Events = events; Map = map;
            Party = game.party_data.Pointer;
            X = events.ehuman[0].x.ss.inte; Y = events.ehuman[0].y.ss.inte;
            Handle = Addressables.LoadAssetAsync<BATTLE_DATA>(
                (Il2CppSystem.String)("gsd2-classic-battle-battle-" + definition.TableId));
        }
    }

    static byte[][] Flags(GAME_WORK g) =>
        new[] {g.chara_flag, g.town_flag, g.event_flag, g.scpoi_flag, g.t_box_flag}
            .Select(a => a.ToArray()).ToArray();

    static string Progress(GAME_WORK g)
    {
        var d = g.game_data;
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            d.base_lv, d.star_bit, d.hanyou_po, d.luka_hp, d.save_num, d.save_slot, d.load_count,
            HonFlags = d.hon_flag?.ToArray(), Party = g.party_data.party_cha_no?.ToArray(),
            EventItems = g.party_data.event_item?.ToArray()
        });
    }

    static int[] CountRewardItems(Trial t) => t.RewardIds.Select(id =>
        t.Game.party_data.party_item.Count(item => item?.item_no == id)).ToArray();

    static bool SameCampaign(Trial t) =>
        Framework.Singleton<GAME_WORK>.instance?.Pointer == t.Game.Pointer &&
        t.Game.party_data?.Pointer == t.Party && t.Game.sys_work?.Pointer == t.Sys.Pointer;

    static bool SafeField(Trial t) => SameCampaign(t) && CastleGuildRuntime.Idle &&
        CastleGuildRuntime.Near && CastleGuildRuntime.Owner?.Pointer == t.Field.Pointer &&
        CastleGuildRuntime.Field?.Pointer == t.Events.Pointer && t.Field._mstep == 20 &&
        t.Field.econ?.Pointer == t.Events.Pointer && t.Events.mdat?.Pointer == t.Map.Pointer &&
        t.Field.mdat?.Pointer == t.Map.Pointer && t.Sys.game_mode == 1 &&
        t.Sys.now_save == 0 && t.Sys.filter_timer == 0 && t.Sys.battle_data == null &&
        Framework.Singleton<BATTLE_WORK>.instance == null &&
        t.Events.ehuman[0].x.ss.inte == t.X && t.Events.ehuman[0].y.ss.inte == t.Y;

    static void Prepare(Trial t)
    {
        // Reuse the exact audited source contract from the native fixture.
        var original = t.Handle.Result;
        var audit = ContentRules.Read<BossSourceAudit[]>(ContentRuntime.Resource("boss-resource-audit.json"))
            .Single(a => a.Data.TableId == t.Definition.TableId).Data;
        var source = HallEchoesProbe.ValidateSource(original, audit);
        if (source.id != t.Definition.PartyId) throw new InvalidOperationException("Trial source party mismatch.");
        int bossIndex = Array.IndexOf(BattleLoader.btl_bos_data.ToArray(), t.Definition.TableId);
        if (bossIndex < 0) throw new InvalidOperationException("Trial has no original resource mapping.");
        t.PackId = BattleLoader.GetBossResID(bossIndex);
        if (t.PackId != t.Definition.PackId || original.battle_field_data?.hd_field_id != t.Definition.SceneId ||
            original.monster_data[audit.Monsters.Single().Index].hp != t.Definition.Hp)
            throw new InvalidOperationException("Trial resource, background or HP identity mismatch.");
        Plugin.Logger.LogInfo($"HallFirstContact: source={original.id} pack={t.PackId} scene={original.battle_field_data?.hd_field_id} opener={audit.Hooks[0]}.");
        t.Formation = CombatEvolutionRuntime.CopyParty(source, audit.OldCodes);
        t.Formation.id = "echo_trial_" + t.Definition.Id + "_00";
        t.Formation.party_flg = 0;
        t.Formation.item_no = 0;
        // The original opening creates the boss's battle-local effect work
        // and turns off the captured field background. Retain its audited
        // binding while leaving command, phase and ending callbacks detached.
        t.Formation.data.phase_start_func_index = 0;
        t.Formation.data.phase_end_func_index = 0;
        t.Formation.data.battle_end_func_index = 0;
        t.Formation.data.command_func_index = 0;
        t.Formation.data.battle_main_func_index = 0;
        t.Table = ScriptableObject.CreateInstance<BATTLE_DATA>();
        t.Table.id = "echo_trial_" + t.Definition.Id;
        t.Table.is_battle_field_data = original.is_battle_field_data;
        t.Table.battle_field_data = original.battle_field_data;
        t.Table.monster_party_num = 1;
        t.Table.monster_party = new Il2CppReferenceArray<MONSTER_PARTY_DATA>(new[] { t.Formation });
        t.Table.monster_data = original.monster_data;
        // Keep the original coin value and ordinary monster-drop table.
        // The rematch grants neither a separate Hall payout nor story loot.
        var monster = original.monster_data[audit.Monsters.Single().Index];
        t.RewardIds = monster.item_data.Where(item => item != null && item.item_no != 0)
            .Select(item => item.item_no).Distinct().ToArray();
        Plugin.Logger.LogInfo($"HallFirstContact: native loot policy; Potch={monster.mochi_kin}, item IDs={string.Join(",", t.RewardIds)}.");
    }

    static void RequestNative(Trial t)
    {
        var tables = t.Map.battle_dat;
        int index = tables?.Length ?? 0;
        // Index zero is reserved. Preserve all original entries including nulls.
        index = Math.Max(index, 1);
        if (index >= 0xF0 || t.Sys.yobi == null || t.Sys.yobi.Length == 0 || t.Sys.yobi[0] != 0 ||
            !CastleGuildRuntime.HuntEntryCompatible || !Plugin.CheckCompatibility() ||
            !HallAvailability.CanSelect(Array.IndexOf(HallTrialCatalogue.Entries, t.Definition)))
            throw new InvalidOperationException("Ordinary request preconditions rejected.");
        var extended = new Il2CppReferenceArray<BATTLE_DATA>(index + 1);
        if (tables != null)
            for (int i = 0; i < tables.Length; i++) extended[i] = tables[i];
        extended[index] = t.Table!;
        t.Flags = Flags(t.Game);
        t.War = LitJson.JsonMapper.ToJson(t.Game.war_data);
        t.Progress = Progress(t.Game);
        t.Potch = t.Game.party_data.gold;
        t.RewardCounts = CountRewardItems(t);
        if (t.SceneSlot >= 0)
        {
            if (t.Scenes!.activeMem != t.FieldSlot || t.SceneResource == null ||
                !IsSceneOwned(t) || t.SceneResource.IsLoading)
                throw new InvalidOperationException("Native battle scene lease changed before request.");
        }
        t.Map.battle_dat = extended;
        t.Committed = true;
        try
        {
            t.Events.EventBatlleSet((byte)index, 0);
            if (t.Sys.battle_no != -1 || t.Sys.battle_data?.Pointer != t.Table!.Pointer ||
                t.Field._mstep != 40)
            {
                // Never try to undo a partially applied native request.
                throw new InvalidOperationException("Native ordinary setter returned an unexpected transition.");
            }
            t.Sys.yobi[0] = 1;
        }
        finally { t.Map.battle_dat = tables; }
        Plugin.Logger.LogInfo("HallFirstContact: original setter entered step 40, battle_no=-1, selection=1.");
    }

    internal static void Tick()
    {
        if (trial is not {} t || faulted) return;
        try
        {
            if (!t.Committed)
            {
                t.Abort |= !SafeField(t) || !HallAvailability.CanSelect(
                    Array.IndexOf(HallTrialCatalogue.Entries, t.Definition));
                if (!t.Handle.IsDone) return;
                if (t.Abort && !t.SourcePrepared) { Finish(t); return; }
                if (t.Handle.Status != AsyncOperationStatus.Succeeded)
                    throw new InvalidOperationException("Hall source table failed to load.");
                if (!t.SourcePrepared)
                {
                    Prepare(t);
                    if (t.PackId != null) t.Pack = new HallPackLease(t.PackId);
                    t.SourcePrepared = true;
                }
                if (t.Pack != null && !t.Pack.Step()) return;
                if (!t.Abort || t.Scenes != null)
                {
                    if (!StepScene(t)) return;
                }
                if (t.Abort) { Finish(t); return; }
                if (!CastleGuildRuntime.Modal && !CastleGuildRuntime.ReleaseFence) RequestNative(t);
                return;
            }

            if (t.Destroyed < 0 || !t.Released || Time.frameCount <= t.Destroyed + 1 ||
                Framework.Singleton<BATTLE_WORK>.instance != null ||
                GRChapterManager.GRInstance?.activeChapter is BattleChapter)
                return;
            bool fieldReturn = t.TerminalResult == 1 && !t.Loss && t.NativeFieldReturn &&
                t.Sys.game_mode == 1 && GRChapterManager.GRInstance?.activeChapter is MapChapter;
            bool victoryReturn = fieldReturn && t.Win;
            bool nonVictoryReturn = fieldReturn && !t.Win;
            bool defeat = t.Loss && GRChapterManager.GRInstance?.activeChapter is GameOverChapter;
            if (!fieldReturn && !defeat) return;
            bool flags = SameCampaign(t) && t.Flags!.Zip(Flags(t.Game), (a,b) => a.SequenceEqual(b)).All(x => x);
            bool war = SameCampaign(t) && t.War == LitJson.JsonMapper.ToJson(t.Game.war_data);
            bool progress = SameCampaign(t) && t.Progress == Progress(t.Game);
            if (!t.OutcomeRecorded)
            {
                t.OutcomeRecorded = true;
                var itemDeltas = CountRewardItems(t).Select((after, i) => $"{t.RewardIds[i]}:{after - t.RewardCounts[i]}");
                Plugin.Logger.LogInfo($"HallFirstContact: outcome victoryReturn={victoryReturn} nonVictoryReturn={nonVictoryReturn} gameOver={defeat} guildPosition={t.AtGuildPosition} flagsUnchanged={flags} warUnchanged={war} progressionUnchanged={progress} potchDelta={(long)t.Game.party_data.gold-t.Potch} itemSlotDeltas={string.Join(",",itemDeltas)}.");
            }
            Finish(t);
            if (trial == null && defeat)
            {
                // Stock Game Over can bypass BattleEndWait. Retire only the
                // combat state associated with this released Hall work owner.
                if (t.Work != null && Combat.State?.Work == t.Work.Pointer)
                {
                    Combat.Clear();
                    Plugin.Logger.LogInfo("HallFirstContact: retired the defeated trial's combat state before Retry.");
                }
                if (CastleGuildRuntime.Owner?.Pointer == t.Field.Pointer)
                    CastleGuildRuntime.Detach();
            }
            if (!flags || !war || !progress || !t.ActorBound || !t.Setup ||
                (fieldReturn && !t.AtGuildPosition)) faulted = true;
        }
        catch (Exception e)
        {
            faulted = true;
            Plugin.Logger.LogError($"HallFirstContact: stopped (committed={t.Committed}): {e}");
            // An unknown acquisition or partially dispatched battle stays pinned
            // until process exit. Never unload an unclassified native consumer.
        }
    }

    static void Finish(Trial t)
    {
        if (!FinishScene(t)) return;
        bool borrowed = t.Pack?.Borrowed == true;
        t.Pack?.Release();
        if (t.Formation != null) UnityEngine.Object.Destroy(t.Formation);
        if (t.Table != null) UnityEngine.Object.Destroy(t.Table);
        if (t.Handle.IsValid()) Addressables.Release(t.Handle);
        Plugin.Logger.LogInfo($"HallFirstContact: complete; committed={t.Committed} packBorrowed={borrowed}.");
        trial = null;
    }

    static bool IsSceneOwned(Trial t) =>
        t.SceneSlot >= 0 && t.Scenes?.asyncLoads[t.SceneSlot].TryGetValue(t.SceneName!, out var loader) == true &&
        loader?.Pointer == t.SceneResource?.Pointer;

    static bool StepScene(Trial t)
    {
        var name = t.Table?.battle_field_data?.hd_field_id;
        if (!GAME_WORK.HDMode || string.IsNullOrEmpty(name)) return true;
        if (t.Scenes == null)
        {
            var scenes = Framework.Singleton<SceneLoader>.instance ??
                throw new InvalidOperationException("There is no live native scene loader.");
            var slots = scenes.asyncLoads;
            t.FieldSlot = scenes.activeMem;
            if (slots.Any(slot => slot != null && slot.ContainsKey(name)))
                throw new InvalidOperationException("Battle scene already belongs to another native context.");
            t.SceneSlot = Enumerable.Range(0, slots.Length)
                .FirstOrDefault(i => i != t.FieldSlot && slots[i]?.Count == 0, -1);
            if (t.SceneSlot < 0) throw new InvalidOperationException("No free native scene slot.");
            t.SceneName = name;
            t.Scenes = scenes;
            HallFirstContactDriver.Instance!.StartCoroutine(scenes.LoadAsync(name, t.SceneSlot,
                t.Field.ano, t.Field.vno, LoadSceneMode.Additive, false));
            Plugin.Logger.LogInfo($"HallFirstContact: requested original HD battle scene {name} in unused slot {t.SceneSlot}.");
        }
        if (t.Scenes.asyncLoads[t.SceneSlot].TryGetValue(name, out var resource) && resource != null)
            t.SceneResource ??= resource;
        if (t.SceneResource == null || t.SceneResource.IsLoading) return false;
        if (!IsSceneOwned(t) || !t.SceneResource.IsScene || !t.SceneResource.SceneHandle.IsValid() ||
            t.SceneResource.SceneHandle.Status != AsyncOperationStatus.Succeeded)
            throw new InvalidOperationException("Native HD scene failed ownership or load validation.");
        return true;
    }

    static bool FinishScene(Trial t)
    {
        if (t.Scenes == null) return true;
        if (!t.SceneUnloading)
        {
            if (!IsSceneOwned(t)) throw new InvalidOperationException("Owned scene binding changed before unload.");
            if (t.Scenes.activeMem == t.SceneSlot) t.Scenes.ChangeActiveMem(t.FieldSlot);
            t.SceneUnloading = true;
            HallFirstContactDriver.Instance!.StartCoroutine(t.Scenes.Unload(t.SceneSlot, t.SceneName!));
            return false;
        }
        return !t.Scenes.asyncLoads[t.SceneSlot].ContainsKey(t.SceneName!) &&
            t.SceneResource?.SceneHandle?.IsValid() != true;
    }

    static bool Owns(BATTLE_WORK? work) => trial is {Committed:true} t &&
        work?.battle_data?.Pointer == t.Table?.Pointer;

    internal static bool UsesOriginalHp() => trial != null && Owns(Framework.Singleton<BATTLE_WORK>.instance);

    internal static void ChapterStarting()
    {
        if (trial is not {Committed:true} t || t.Sys.game_mode != 2 ||
            t.Sys.battle_data?.Pointer != t.Table?.Pointer || t.Sys.battle_no != -1 ||
            t.SceneSlot < 0 || !IsSceneOwned(t) || t.SceneResource?.IsLoading != false) return;
        int previous = t.Scenes!.activeMem;
        t.Scenes.ChangeActiveMem(t.SceneSlot);
        t.Scenes.SetActive(t.SceneName!, true);
        Plugin.Logger.LogInfo($"HallFirstContact: original chapter starts with owned scene slot {previous}->{t.SceneSlot}.");
    }

    internal static bool TryBossSprite(int index, out OPD_DATA? sprite)
    {
        sprite = null;
        if (trial is not {Committed:true, Released:false} t || t.Sys.battle_no != -1 ||
            !Owns(Framework.Singleton<BATTLE_WORK>.instance) || t.Pack?.Loaded != true ||
            index < 0 || index >= t.Pack.Records) return false;
        sprite = OPDResLoader.GetData(t.PackId!, index);
        if (sprite != null && !t.RatLookup)
        {
            t.RatLookup = true;
            Plugin.Logger.LogInfo($"HallFirstContact: native attack consumed {t.PackId} pack record {index}.");
        }
        return sprite != null;
    }

    internal static void Setup(BATTLE_WORK work)
    {
        if (!Owns(work) || trial is not {} t) return;
        t.Setup = work.monster_party_data?.Pointer == t.Formation?.Pointer && work.monster_party_index == 1 &&
            t.Sys.battle_no == -1;
        if (t.Setup && t.Sys.yobi[0] == 1) t.Sys.yobi[0] = 0;
        Plugin.Logger.LogInfo($"HallFirstContact: original setup identity matched={t.Setup}.");
    }

    internal static void Scene(BATTLE_WORK work)
    {
        if (!Owns(work) || trial is not {} t) return;
        var actor = work.pm_data?.FirstOrDefault(a => a != null && !a.IsPlayer && a.hp > 0);
        t.Work = work;
        t.ActorBound = actor != null;
        t.ActorHp = actor?.hp ?? 0;
        Plugin.Logger.LogInfo($"HallFirstContact: Field.Setup completed; liveMonster={t.ActorBound}.");
    }

    internal static void BattleTick(int result)
    {
        if (trial is not {ActorBound:true} t || !t.Setup) return;
        if (result != 0)
        {
            t.TerminalResult = result;
            Plugin.Logger.LogInfo($"HallFirstContact: BattleChapter.Update terminal result={result}.");
        }
        if (t.Released || !Owns(t.Work)) return;
        var actor = t.Work!.pm_data?.FirstOrDefault(a => a != null && !a.IsPlayer);
        if (!t.HpChanged && actor != null && actor.hp != t.ActorHp)
        {
            t.HpChanged = true;
            Plugin.Logger.LogInfo($"HallFirstContact: living battle tick changed monster HP from {t.ActorHp} to {actor.hp}.");
        }
    }

    internal static void FieldReturned(MACHICON field)
    {
        if (trial is not {Committed:true, Released:true, TerminalResult:1} t ||
            t.Destroyed < 0 || field.Pointer != t.Field.Pointer ||
            t.Sys.mcon?.Pointer != field.Pointer || t.Sys.game_mode != 1 ||
            Framework.Singleton<BATTLE_WORK>.instance != null ||
            field.econ?.Pointer != t.Events.Pointer ||
            field.econ.ehuman?[0] is not {} player) return;
        t.NativeFieldReturn = true;
        t.AtGuildPosition = player.x.ss.inte == t.X && player.y.ss.inte == t.Y;
    }

    internal static void Outcome(BATTLE_WORK work, bool win)
    {
        if (!Owns(work) || trial is not {} t) return;
        t.Win |= win;
        t.Loss |= !win;
        Plugin.Logger.LogInfo($"HallFirstContact: original terminal handler; win={win}.");
    }

    internal static bool Releasing(BATTLE_WORK? work) => Owns(work);
    internal static void ReleaseObserved(bool owned)
    {
        if (owned && trial is {} t) t.Released = true;
    }

    internal static void DestroyObserved()
    {
        if (trial is {Released:true} t) t.Destroyed = Time.frameCount;
    }

    internal static void PackUse(string id, string method)
    {
        if (trial is {Committed:true} t && id == t.PackId && t.PackLookups.Add((method, t.Setup, t.ActorBound, t.Win || t.Loss)))
            Plugin.Logger.LogInfo($"HallFirstContact: native {t.PackId} lookup; method={method}, setup={t.Setup}, actors={t.ActorBound}, terminal={t.Win || t.Loss}.");
    }
}

public sealed class HallFirstContactDriver : MonoBehaviour
{
    internal static HallFirstContactDriver? Instance;
    public HallFirstContactDriver(IntPtr pointer) : base(pointer) { }
    public void Awake() => Instance = this;
}

[HarmonyPatch(typeof(MACHICON), nameof(MACHICON.MachiMain))]
[HallTrialPatch]
internal static class HallFirstContactField
{
    static void Postfix() => HallFirstContact.Tick();
}

[HarmonyPatch(typeof(Field), nameof(Field.Setup))]
[HallTrialPatch]
internal static class HallFirstContactScene
{
    static void Postfix(BATTLE_WORK work) => HallFirstContact.Scene(work);
}

[HarmonyPatch(typeof(MACHICON), "OnEndBattle")]
[HallTrialPatch]
internal static class HallFirstContactFieldReturn
{
    static void Postfix(MACHICON __instance) => HallFirstContact.FieldReturned(__instance);
}

[HarmonyPatch(typeof(BattleManager), nameof(BattleManager.Release))]
[HallTrialPatch]
internal static class HallFirstContactRelease
{
    static void Prefix(BattleManager __instance, out bool __state) =>
        __state = HallFirstContact.Releasing(__instance.battle_work);
    static void Postfix(bool __state) => HallFirstContact.ReleaseObserved(__state);
}

[HarmonyPatch(typeof(BattleChapter), nameof(BattleChapter.OnDestroy))]
[HallTrialPatch]
internal static class HallFirstContactDestroy
{
    static void Postfix() => HallFirstContact.DestroyObserved();
}

[HarmonyPatch(typeof(BattleChapter), nameof(BattleChapter.Start))]
[HallTrialPatch]
internal static class HallFirstContactStart
{
    static void Prefix() => HallFirstContact.ChapterStarting();
}

[HarmonyPatch(typeof(OverlayFunctionBase), "GetBossOPD")]
[HallTrialPatch]
internal static class HallFirstContactRatSprite
{
    static bool Prefix(int __0, ref OPD_DATA? __result)
    {
        if (!HallFirstContact.TryBossSprite(__0, out var sprite)) return true;
        __result = sprite;
        return false;
    }
}

[HarmonyPatch(typeof(GameOverChapter), nameof(GameOverChapter.Update))]
[HallTrialPatch]
internal static class HallFirstContactGameOver
{
    static void Postfix() => HallFirstContact.Tick();
}

[HarmonyPatch(typeof(OPDResLoader), nameof(OPDResLoader.GetData))]
[HarmonyPatch(typeof(OPDResLoader), nameof(OPDResLoader.GetActionData))]
[HallTrialPatch]
internal static class HallFirstContactPackUse
{
    static void Prefix(string __0, System.Reflection.MethodBase __originalMethod) =>
        HallFirstContact.PackUse(__0, __originalMethod.Name);
}

[HarmonyPatch(typeof(BattleChapter), nameof(BattleChapter.Update))]
[HallTrialPatch]
internal static class HallFirstContactBattleTick
{
    static void Postfix(int __result) => HallFirstContact.BattleTick(__result);
}
