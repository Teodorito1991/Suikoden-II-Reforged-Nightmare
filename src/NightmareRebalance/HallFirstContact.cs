#if HALL_FIRST_CONTACT
using BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace NightmareRebalance;

internal static class HallFirstContact
{
    static Trial? trial;
    static bool faulted;
    static Func<bool>? isolated;
    internal static readonly bool CopyLaunch = !Application.isBatchMode &&
        Environment.GetCommandLineArgs().Contains("--reforged-hall-first-contact") &&
        Path.GetFullPath(Paths.GameRootPath).EndsWith(
            Path.Combine("github-private-staging", "isolated-game"), StringComparison.OrdinalIgnoreCase) &&
        File.Exists(Path.Combine(Paths.GameRootPath, "PRIVATE_DO_NOT_UPLOAD.txt")) &&
        File.Exists(Path.Combine(Paths.GameRootPath, "PRIVATE_HALL_COPY_ARMED"));

    internal static bool Available => CopyLaunch && !faulted && trial == null && IsolationReady();

    static bool IsolationReady()
    {
        if (isolated == null)
        {
            var guard = AppDomain.CurrentDomain.GetAssemblies()
                .SingleOrDefault(a => a.GetName().Name == "HallCampaignGuard")?.GetType("HallGuard", false);
            var method = AccessTools.Method(guard, "FirstContactIsolated");
            if (method == null) return false;
            isolated = method.CreateDelegate<Func<bool>>();
        }
        return isolated();
    }

    internal static bool Request()
    {
        if (!Available || !CastleGuildRuntime.Context || !Plugin.CheckCompatibility() ||
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

        trial = new Trial(game, sys, field, events, map);
        Plugin.Logger.LogInfo("HallFirstContact: Guild confirmed Pest Rat.");
        return true;
    }

    sealed class Trial
    {
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
        internal BATTLE_WORK? Work;
        internal readonly HashSet<(string, bool, bool, bool)> PackLookups = new();
        internal int Destroyed = -1;
        internal byte[][]? Flags;
        internal string? War;
        internal string? Progress;
        internal uint Potch;
        internal Trial(GAME_WORK game, SYS_WORK sys, MACHICON field, EVENTCON events, MACHIDAT map)
        {
            Game = game; Sys = sys; Field = field; Events = events; Map = map;
            Party = game.party_data.Pointer;
            X = events.ehuman[0].x.ss.inte; Y = events.ehuman[0].y.ss.inte;
            Handle = Addressables.LoadAssetAsync<BATTLE_DATA>(
                (Il2CppSystem.String)"gsd2-classic-battle-battle-boss_rat");
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

    static bool SameCampaign(Trial t) =>
        Framework.Singleton<GAME_WORK>.instance?.Pointer == t.Game.Pointer &&
        t.Game.party_data?.Pointer == t.Party && t.Game.sys_work?.Pointer == t.Sys.Pointer &&
        IsolationReady();

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
            .Single(a => a.Data.TableId == "boss_rat").Data;
        var source = HallEchoesProbe.ValidatePestRat(original, audit);
        t.Formation = CombatEvolutionRuntime.CopyParty(source, audit.OldCodes);
        t.Formation.id = "echo_trial_rat_00";
        t.Formation.party_flg = 0;
        t.Formation.item_no = 0;
        // The original opening creates Pest Rat's battle-local effect work
        // and turns off the captured field background. Retain its audited
        // binding while leaving command, phase and ending callbacks detached.
        t.Formation.data.phase_start_func_index = 0;
        t.Formation.data.phase_end_func_index = 0;
        t.Formation.data.battle_end_func_index = 0;
        t.Formation.data.command_func_index = 0;
        t.Formation.data.battle_main_func_index = 0;
        t.Table = ScriptableObject.CreateInstance<BATTLE_DATA>();
        t.Table.id = "echo_trial_rat";
        t.Table.is_battle_field_data = original.is_battle_field_data;
        t.Table.battle_field_data = original.battle_field_data;
        t.Table.monster_party_num = 1;
        t.Table.monster_party = new Il2CppReferenceArray<MONSTER_PARTY_DATA>(new[] { t.Formation });
        t.Table.monster_data = original.monster_data;
    }

    static void RequestNative(Trial t)
    {
        var tables = t.Map.battle_dat;
        int index = tables?.Length ?? 0;
        // Index zero is reserved. Preserve all original entries including nulls.
        index = Math.Max(index, 1);
        if (index >= 0xF0 || t.Sys.yobi == null || t.Sys.yobi.Length == 0 || t.Sys.yobi[0] != 0 ||
            !CastleGuildRuntime.HuntEntryCompatible || !Plugin.CheckCompatibility())
            throw new InvalidOperationException("Ordinary request preconditions rejected.");
        var extended = new Il2CppReferenceArray<BATTLE_DATA>(index + 1);
        if (tables != null)
            for (int i = 0; i < tables.Length; i++) extended[i] = tables[i];
        extended[index] = t.Table!;
        t.Flags = Flags(t.Game);
        t.War = LitJson.JsonMapper.ToJson(t.Game.war_data);
        t.Progress = Progress(t.Game);
        t.Potch = t.Game.party_data.gold;
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
                t.Abort |= !SafeField(t);
                if (!t.Handle.IsDone) return;
                if (t.Abort && t.Pack == null) { Finish(t); return; }
                if (t.Handle.Status != AsyncOperationStatus.Succeeded)
                    throw new InvalidOperationException("Pest Rat table failed to load.");
                if (t.Pack == null)
                {
                    Prepare(t);
                    t.Pack = new HallPackLease("rat");
                }
                if (!t.Pack.Step()) return;
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
            bool victoryReturn = t.Win && !t.Loss && GRChapterManager.GRInstance?.activeChapter is MapChapter &&
                t.Sys.game_mode == 1 && t.Sys.mcon?.Pointer == t.Field.Pointer &&
                t.Field._mstep == 20 && t.Field.econ?.ehuman?[0] is {} player &&
                player.x.ss.inte == t.X && player.y.ss.inte == t.Y;
            bool defeat = t.Loss && GRChapterManager.GRInstance?.activeChapter is GameOverChapter;
            if (!victoryReturn && !defeat) return;
            bool flags = SameCampaign(t) && t.Flags!.Zip(Flags(t.Game), (a,b) => a.SequenceEqual(b)).All(x => x);
            bool war = SameCampaign(t) && t.War == LitJson.JsonMapper.ToJson(t.Game.war_data);
            bool progress = SameCampaign(t) && t.Progress == Progress(t.Game);
            if (!t.OutcomeRecorded)
            {
                t.OutcomeRecorded = true;
                Plugin.Logger.LogInfo($"HallFirstContact: outcome victoryReturn={victoryReturn} gameOver={defeat} flagsUnchanged={flags} warUnchanged={war} progressionUnchanged={progress} potchDelta={(long)t.Game.party_data.gold-t.Potch}.");
            }
            Finish(t);
            if (trial == null && defeat && CastleGuildRuntime.Owner?.Pointer == t.Field.Pointer)
                CastleGuildRuntime.Detach();
            if (!flags || !war || !progress || !t.ActorBound || !t.Setup) faulted = true;
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

    internal static bool UsesOriginalRatHp(BATTLE_WORK work) => Owns(work);

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

    internal static bool TryRatSprite(int index, out OPD_DATA? sprite)
    {
        sprite = null;
        if (trial is not {Committed:true, Released:false} t || t.Sys.battle_no != -1 ||
            !Owns(Framework.Singleton<BATTLE_WORK>.instance) || t.Pack?.Loaded != true ||
            index < 0 || index >= t.Pack.Records) return false;
        sprite = OPDResLoader.GetData("rat", index);
        if (sprite != null && !t.RatLookup)
        {
            t.RatLookup = true;
            Plugin.Logger.LogInfo($"HallFirstContact: native attack consumed rat pack record {index}.");
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
        if (trial is not {ActorBound:true} t || !Owns(t.Work) || t.Released) return;
        var actor = t.Work!.pm_data?.FirstOrDefault(a => a != null && !a.IsPlayer);
        if (!t.HpChanged && actor != null && actor.hp != t.ActorHp)
        {
            t.HpChanged = true;
            Plugin.Logger.LogInfo($"HallFirstContact: living battle tick changed monster HP from {t.ActorHp} to {actor.hp}.");
        }
        if (result != 0) Plugin.Logger.LogInfo($"HallFirstContact: BattleChapter.Update terminal result={result}.");
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
        if (id == "rat" && trial is {Committed:true} t && t.PackLookups.Add((method, t.Setup, t.ActorBound, t.Win || t.Loss)))
            Plugin.Logger.LogInfo($"HallFirstContact: native rat lookup; method={method}, setup={t.Setup}, actors={t.ActorBound}, terminal={t.Win || t.Loss}.");
    }
}

public sealed class HallFirstContactDriver : MonoBehaviour
{
    internal static HallFirstContactDriver? Instance;
    public HallFirstContactDriver(IntPtr pointer) : base(pointer) { }
    public void Awake() => Instance = this;
}

[HarmonyPatch(typeof(MACHICON), nameof(MACHICON.MachiMain))]
internal static class HallFirstContactField
{
    static void Postfix() => HallFirstContact.Tick();
}

[HarmonyPatch(typeof(Field), nameof(Field.Setup))]
internal static class HallFirstContactScene
{
    static void Postfix(BATTLE_WORK work) => HallFirstContact.Scene(work);
}

[HarmonyPatch(typeof(BattleManager), nameof(BattleManager.Release))]
internal static class HallFirstContactRelease
{
    static void Prefix(BattleManager __instance, out bool __state) =>
        __state = HallFirstContact.Releasing(__instance.battle_work);
    static void Postfix(bool __state) => HallFirstContact.ReleaseObserved(__state);
}

[HarmonyPatch(typeof(BattleChapter), nameof(BattleChapter.OnDestroy))]
internal static class HallFirstContactDestroy
{
    static void Postfix() => HallFirstContact.DestroyObserved();
}

[HarmonyPatch(typeof(BattleChapter), nameof(BattleChapter.Start))]
internal static class HallFirstContactStart
{
    static void Prefix() => HallFirstContact.ChapterStarting();
}

[HarmonyPatch(typeof(OverlayFunctionBase), "GetBossOPD")]
internal static class HallFirstContactRatSprite
{
    static bool Prefix(int __0, ref OPD_DATA? __result)
    {
        if (!HallFirstContact.TryRatSprite(__0, out var sprite)) return true;
        __result = sprite;
        return false;
    }
}

[HarmonyPatch(typeof(GameOverChapter), nameof(GameOverChapter.Update))]
internal static class HallFirstContactGameOver
{
    static void Postfix() => HallFirstContact.Tick();
}

[HarmonyPatch(typeof(OPDResLoader), nameof(OPDResLoader.GetData))]
[HarmonyPatch(typeof(OPDResLoader), nameof(OPDResLoader.GetActionData))]
internal static class HallFirstContactPackUse
{
    static void Prefix(string __0, System.Reflection.MethodBase __originalMethod) =>
        HallFirstContact.PackUse(__0, __originalMethod.Name);
}

[HarmonyPatch(typeof(BattleChapter), nameof(BattleChapter.Update))]
internal static class HallFirstContactBattleTick
{
    static void Postfix(int __result) => HallFirstContact.BattleTick(__result);
}
#endif
