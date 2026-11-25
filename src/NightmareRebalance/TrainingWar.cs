using HarmonyLib;
using System.Text.Json;
using UnityEngine;

namespace NightmareRebalance;

internal static class TrainingWar
{
    internal enum Drill { Standard, FortDefense, CommanderRout, War3 }

    sealed class Session(GAME_WORK game, SYS_WORK system, MACHICON field, EVENTCON events, EVENT_HUMAN player, Drill drill, bool veteran)
    {
        internal readonly Guid Identity=Guid.NewGuid();
        internal readonly IntPtr DataOwner=game.game_data.Pointer;
        internal string[]? RestoredData;
        internal readonly bool NormalContract=Plugin.Options.EnableTrainingNormalBuild.Value;
        internal readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>[] OriginalFlags=ReadFlags(game);
        internal Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>[]? PrivateFlags;
        internal readonly GAME_WORK Game = game;
        internal readonly SYS_WORK System = system;
        internal readonly MACHICON Field = field;
        internal readonly EVENTCON Events = events;
        internal readonly long Epoch = NativeWarBoundary.Epoch;
        internal readonly int Area = field.ano, Town = field.vno, Map = field.mno, Memory = field.memtyp;
        internal readonly string MapId = CastleGuildRuntime.Audit.MapId;
        internal readonly int X = player.x.ss.inte, Y = player.y.ss.inte;
        internal readonly byte[][] Flags = ReadFlags(game).Select(a => a.ToArray()).ToArray();
        internal readonly string[] Data = Serialize(game);
        internal readonly int FadeFrame = Time.frameCount;
        internal readonly IntPtr PartyOwner = game.party_data.Pointer;
        internal readonly bool Veteran = veteran;
        internal WarChapter? War;
        internal int NativeResult, Ending, GameOver;
        internal bool CommanderRouted, AlliesRemain, FortVictory;
        internal readonly Drill SelectedDrill = drill;
        internal readonly byte WarNumber = drill switch { Drill.FortDefense => 2, Drill.War3 => 3, _ => 1 };
        internal bool Committed, Destroyed;
        internal int Result, SaveCalls;
    }

    static Session? current, protectedSession;
    static long loadedEpoch=-1;
    static bool requested, faulted;
    static Drill requestedDrill;
    static bool requestedVeteran;
    internal static bool Active => current is { Committed: true };
    internal static bool HasTransitionHarness => StockWarTransitionTrace.Enabled && !Application.isBatchMode &&
        Harmony.GetPatchInfo(AccessTools.Method(typeof(SteamService), nameof(SteamService.GetAppInstallDir)))?
            .Prefixes.Any(p => p.owner == "private.reforged.stocktrace.root") == true;
    internal static bool NormalContractEnabled=>Plugin.Options?.EnableTrainingNormalBuild.Value==true &&
        Plugin.Options.EnableTrainingDefeatReturn.Value && !Application.isBatchMode &&
        NativeWarRemix.DefinitionsVerified;
    internal static bool Enabled => Plugin.Options?.EnableTrainingWar.Value == true && (HasTransitionHarness||NormalContractEnabled);
    internal static bool Available => Enabled && !faulted && current == null && !requested &&
        Framework.Singleton<GAME_WORK>.instance?.eventFlgCHK(0xA9, 0x40) == true;

    internal static bool ChallengeAvailable => Available && Plugin.Options.EnableTrainingChallenge.Value &&
        NativeWarRemix.DefinitionsVerified;
    internal static bool FortDefenseAvailable => Available && Plugin.Options.EnableTrainingWar2.Value &&
        War2Runtime.DefinitionsVerified && Framework.Singleton<GAME_WORK>.instance?.eventFlgCHK(0xAA, 0x02) == true;
    internal static bool War3Available => Available && Plugin.Options.EnableTrainingWar3.Value &&
        TotalWarRuntime.Verified.Contains(3) && Framework.Singleton<GAME_WORK>.instance?.eventFlgCHK(0x0C, 0x80) == true;
    internal static bool ChallengeActive => current is { Committed: true, SelectedDrill: Drill.CommanderRout };
    internal static bool VeteranActive => current is { Committed: true, Veteran: true };
    internal static bool VeteranAvailable(Drill drill)
    {
        int war = drill switch { Drill.FortDefense => 2, Drill.War3 => 3, _ => 1 };
        return Plugin.Options.EnableTrainingVeteran.Value && Plugin.Options.EnableNativeWarAI.Value &&
            Plugin.Options.EnableWarTactics.Value && WarTacticsRuntime.Config.Enabled &&
            WarTacticsRuntime.Config.EnabledWars.Contains(war) && WarTacticsRuntime.Verified.Contains(war);
    }

    internal static bool FortDefenseActive => current is { Committed: true, SelectedDrill: Drill.FortDefense };

    internal static string Unavailable(Drill drill)
    {
        if (Plugin.Options?.EnableTrainingWar.Value!=true) return "Army Training is disabled in settings.";
        if (!HasTransitionHarness&&!NormalContractEnabled) return "Enable the experimental normal-build return contract, or use the protected training copy.";
        if (faulted || current != null || requested) return "A drill cannot start right now.";
        if (CastleGuildRuntime.Quest.State is GuildQuestState.Accepted or GuildQuestState.AwaitingReport)
            return "Settle your Hunt contract first.";
        if (!Available) return "Complete the original War 1 first.";
        return drill switch
        {
            Drill.Standard => "",
            Drill.FortDefense when !Plugin.Options.EnableTrainingWar2.Value => "Fort Defense is disabled.",
            Drill.FortDefense when !FortDefenseAvailable => "Requires completed War 2 and its original battlefield.",
            Drill.CommanderRout when !ChallengeAvailable => "Requires an enabled drill and verified War 1 formations.",
            Drill.War3 when !Plugin.Options.EnableTrainingWar3.Value => "War 3 Training is disabled.",
            Drill.War3 when !War3Available => "Requires completed War 3 and its original battlefield.",
            Drill.FortDefense or Drill.CommanderRout or Drill.War3 => "",
            _ => "This drill is unavailable."
        };
    }

    internal static bool Request(Drill drill, bool veteran)
    {
        string reason = Unavailable(drill);
        if (reason.Length == 0 && veteran && !VeteranAvailable(drill)) reason = "Veteran tactics are unavailable.";
        if (reason.Length != 0)
        {
            CastleGuildRuntime.Status = reason;
            return false;
        }
        requested = true;
        requestedDrill = drill;
        requestedVeteran = veteran;
        CastleGuildRuntime.TrainingFeedback("Assembling the training squads.");
        return true;
    }

    static Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>[] ReadFlags(GAME_WORK game) =>
        [game.chara_flag, game.town_flag, game.event_flag, game.scpoi_flag, game.t_box_flag];

    static string[] Serialize(GAME_WORK game) =>
        [LitJson.JsonMapper.ToJson(game.game_data), LitJson.JsonMapper.ToJson(game.war_data),
         LitJson.JsonMapper.ToJson(game.chara_data), LitJson.JsonMapper.ToJson(game.party_data)];

    static EVENT_HUMAN? Player(EVENTCON events) =>
        events.ehuman is { Count: > 0 } actors ? actors[0] : null;

    static bool Owns(Session session, MACHICON field)
    {
        var game = Framework.Singleton<GAME_WORK>.instance;
        return session.Epoch == NativeWarBoundary.Epoch && game?.Pointer == session.Game.Pointer &&
            game.sys_work?.Pointer == session.System.Pointer && field.Pointer == session.Field.Pointer &&
            session.System.mcon?.Pointer == field.Pointer;
    }

    static void SetFlags(GAME_WORK game,Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>[] flags)
    {game.chara_flag=flags[0];game.town_flag=flags[1];game.event_flag=flags[2];game.scpoi_flag=flags[3];game.t_box_flag=flags[4];}
    static void ForkFlags(Session session)
    {
        if(!session.NormalContract)return;
        if(session.OriginalFlags.Any(a=>a==null||a.Length==0||a.Length>4096))throw new InvalidOperationException("Incomplete training flag ownership.");
        session.PrivateFlags=session.OriginalFlags.Select(a=>new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>(a.ToArray())).ToArray();
        protectedSession=session;
        SetFlags(session.Game,session.PrivateFlags);
    }
    static bool RestoreFlags(Session session)
    {
        if(!session.NormalContract)return true;
        if(Framework.Singleton<GAME_WORK>.instance?.Pointer!=session.Game.Pointer||session.Epoch!=NativeWarBoundary.Epoch||
           session.PrivateFlags==null||!ReadFlags(session.Game).Select((a,i)=>a.Pointer==session.PrivateFlags[i].Pointer).All(x=>x))return false;
        bool unchanged=session.OriginalFlags.Select((a,i)=>a.ToArray().SequenceEqual(session.Flags[i])).All(x=>x);
        SetFlags(session.Game,session.OriginalFlags);session.PrivateFlags=null;
        return unchanged;
    }

    static void Reject(string reason)
    {
        Plugin.Logger.LogWarning("Training war stopped. " + reason);
        CastleGuildRuntime.TrainingFeedback(reason);
        requested = false;
        faulted |= current is { Committed: true };
        if(current is {NormalContract:true} owner)
        {
            // Never guess that a running war has stopped using its private arrays.
            if(owner.Committed&&!owner.Destroyed){faulted=true;return;}
            if(owner.PrivateFlags!=null&&!RestoreFlags(owner))faulted=true;
            if(!owner.Committed&&!faulted)protectedSession=null;
        }
        current = null;
    }

    internal static void FieldTick(MACHICON field)
    {
        if(current==null&&protectedSession is {} pending&&field._mstep==20)
        {
            var live=Framework.Singleton<GAME_WORK>.instance;
            if(loadedEpoch>pending.Epoch&&NativeWarBoundary.Epoch==loadedEpoch&&live?.sys_work?.mcon?.Pointer==field.Pointer&&
               live.game_data?.Pointer!=pending.DataOwner&&live.party_data?.Pointer!=pending.PartyOwner&&
               pending.PrivateFlags?.All(old=>ReadFlags(live).All(now=>now.Pointer!=old.Pointer))!=false)
            {protectedSession=null;faulted=false;requested=false;Plugin.Logger.LogInfo("Training persistence ownership retired after a genuine load and idle field.");}
            else if(!faulted&&RestoredState(pending))
            {protectedSession=null;Plugin.Logger.LogInfo($"Training persistence contract {pending.Identity} completed on idle field.");}
            else {faulted=true;Plugin.Logger.LogWarning("Training return remains unverified; native campaign saves remain blocked.");}
        }
        if (!requested && current == null) return;
        // Eligibility guards admission. Once committed, teardown must still
        // restore the field even if the option or external guard changes.
        if (current is not {Committed:true} && !Enabled)
        {
            if (current!=null) G2_BACK.FilterBlackIn(2,15);
            Reject("Training prerequisites changed before the battle.");
            return;
        }
        try
        {
            if (requested)
            {
                if (!CastleGuildRuntime.Idle || CastleGuildRuntime.Modal ||
                    CastleGuildRuntime.Owner?.Pointer != field.Pointer)
                {
                    Reject("Castle field is no longer idle.");
                    return;
                }
                if (CastleGuildRuntime.ReleaseFence) return;

                requested = false;
                var game = Framework.Singleton<GAME_WORK>.instance;
                var system = game?.sys_work;
                var events = field.econ;
                if (game == null || system == null || events == null || system.mcon?.Pointer != field.Pointer ||
                    Player(events) is not {} player || !CastleGuildRuntime.Near ||
                    system.game_mode != 1 || system.game_step?[1] != 1 || system.filter_timer != 0 ||
                    !game.eventFlgCHK(0xA9, 0x40))
                {
                    Reject("Castle field did not match the training prerequisites.");
                    return;
                }

                if (requestedDrill == Drill.FortDefense &&
                    (!game.eventFlgCHK(0xAA, 0x02) || !War2Runtime.Audit() || !Plugin.CheckCompatibility()))
                {
                    Reject("War 2 requires the original battle to be complete and its native scripts to match.");
                    return;
                }

                if (requestedDrill == Drill.War3 &&
                    (!game.eventFlgCHK(0x0C, 0x80) || !TotalWarRuntime.Audit(3) || !Plugin.CheckCompatibility()))
                {
                    Reject("War 3 requires the original battle to be complete and its native scripts and roster to match.");
                    return;
                }

                if (requestedVeteran && !WarTacticsRuntime.Audit(requestedDrill switch
                    { Drill.FortDefense => 2, Drill.War3 => 3, _ => 1 }))
                {
                    Reject("Veteran requires the original army and battlefield definitions.");
                    return;
                }

                if(!Plugin.CheckCompatibility()||system.now_save!=0)throw new InvalidOperationException("Training compatibility/save boundary changed.");
                current = new Session(game, system, field, events, player, requestedDrill, requestedVeteran);
                ForkFlags(current);
                G2_BACK.FilterSetRGB(255, 0);
                G2_BACK.FilterBlackOut(2, 15, false);
                return;
            }

            var session = current!;
            if (!session.Committed)
            {
                if (!Owns(session, field) || !CastleGuildRuntime.Idle ||
                    field.econ?.Pointer != session.Events.Pointer ||
                    field.ano != session.Area || field.vno != session.Town || field.mno != session.Map ||
                    Player(session.Events) is not {} livePlayer || livePlayer.x.ss.inte != session.X || livePlayer.y.ss.inte != session.Y ||
                    session.SaveCalls != 0 || Time.frameCount - session.FadeFrame > 180)
                {
                    G2_BACK.FilterBlackIn(2, 15);
                    Reject("Field changed while preparing training.");
                    return;
                }
                if (session.System.filter_timer != 0 || session.Events.EventWindowChk() != 0) return;

                StockWarTransitionTrace.Record("Training.Before", begin: true);
                session.Committed = true;
                field._mstep = 45;
                session.System.war_no = session.WarNumber;
                field.ano = session.Area;
                field.vno = session.Town;
                field.mno = session.Map;
                field.memtyp = session.Memory;
                field.px = session.X;
                field.py = session.Y;
                session.Events.EventGameWorkSet(field);
                OldSrcBase.SD_call(15, null);
                Plugin.Logger.LogInfo($"Training War {session.WarNumber} handed to MachiMain step 45. Return position {session.X},{session.Y}. Drill={session.SelectedDrill}, veteran={session.Veteran}.");
                return;
            }

            if (!session.Destroyed) return;
            if (session.Result != 1 || !Owns(session, field) || field._mstep != 48 ||
                field.econ != null || session.System.game_mode != 1 || session.SaveCalls != 0)
            {
                Reject("Return boundary did not match the armed training session.");
                return;
            }

            var flags = ReadFlags(session.Game);
            if (flags.Where((a, i) => a == null || a.Length != session.Flags[i].Length).Any())
            {
                Reject("Campaign flag storage changed during training.");
                return;
            }
            if(session.NormalContract)
            {
                if(!RestoreFlags(session))throw new InvalidOperationException("Training original flag references changed.");
                flags=ReadFlags(session.Game);
            }
            else for (int i = 0; i < flags.Length; i++)
                for (int j = 0; j < flags[i].Length; j++) flags[i][j] = session.Flags[i][j];

            bool equal = flags.Select((a, i) => a.ToArray().SequenceEqual(session.Flags[i])).All(x => x);
            var after = Serialize(session.Game);
            string[] names = ["game_data", "war_data", "chara_data", "party_data"];
            var diff = new List<string>();
            for (int i = 0; i < names.Length; i++)
            {
                using var beforeJson = JsonDocument.Parse(session.Data[i]);
                using var afterJson = JsonDocument.Parse(after[i]);
                Compare(beforeJson.RootElement, afterJson.RootElement, names[i], diff);
            }
            Plugin.Logger.LogInfo($"Training War {session.WarNumber} return prefix. Flags equal={equal}, save calls={session.SaveCalls}.");
            Plugin.Logger.LogInfo($"Training War {session.WarNumber} data differences: " + JsonSerializer.Serialize(diff));
            var destination = session.Game.game_data.deru_pos;
            bool destinationMatches = MachiLoader.GetMachiID(session.Area, session.Town)==session.MapId &&
                CastleGuildRuntime.Audit.SupportedIndices.Contains(session.Map) &&
                destination is { Length: >= 2 } &&
                destination[0] == session.X && destination[1] == session.Y;
            faulted = !equal || !destinationMatches || diff.Count >= 256 ||
                diff.Any(path => !ExpectedDifference(path)) || session.Game.party_data.Pointer != session.PartyOwner;
            Plugin.Logger.LogInfo($"Training War {session.WarNumber} destination matches={destinationMatches}, repeat available={!faulted}.");
            // Consume before crediting, so a repeated field tick can never pay twice.
            current = null;
            if (!faulted)
            {
                try { Finish(session); }
                catch (Exception exception)
                {
                    faulted = true;
                    Plugin.Logger.LogWarning("Training reward status could not be confirmed. " + exception.Message);
                }
            }
        }
        catch (Exception exception) { Reject(exception.Message); }
    }

    static void Finish(Session session)
    {
        int reward = TrainingResultRules.VictoryPotch(session.WarNumber, session.Veteran,
            session.NativeResult, session.Ending, session.GameOver, session.CommanderRouted,
            session.AlliesRemain, session.FortVictory);
        uint before = session.Game.party_data.gold;
        if (reward > 0 && Plugin.Options.EnableTrainingRewards.Value)
            G2_SYS.G2_party_gold(1, reward);
        long credited = (long)session.Game.party_data.gold - before;

        string message = session.NativeResult == 2 || session.GameOver != 0
            ? "Training defeat. Your troops have returned to the castle."
            : reward == 0 ? "Drill complete. Route the enemy commander to earn Potch."
            : credited > 0 ? $"Training victory. You earned {credited:N0} Potch."
            : Plugin.Options.EnableTrainingRewards.Value ? "Training victory. Your purse is full."
            : "Training victory.";
        if(session.NormalContract)session.RestoredData=Serialize(session.Game);
        CastleGuildRuntime.TrainingFeedback(message);
        Plugin.Logger.LogInfo($"Training War {session.WarNumber} receipt: ending={session.Ending}, gameOver={session.GameOver}, commanderRouted={session.CommanderRouted}, alliesRemain={session.AlliesRemain}, fortVictory={session.FortVictory}, earned={reward}, credited={credited}.");
    }

    // Read the original outcome at the decision point, before ending scripts can clear the roster.
    [HarmonyPatch(typeof(w_syouha), nameof(w_syouha.CheckSyouhai))]
    static class OutcomeWitness
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        static void Postfix(int __result)
        {
            if (__result != -1 || current is not { Committed: true, War: not null } session ||
                session.System.game_mode != 5 || war_h.g_w_war_no != session.WarNumber ||
                w_unit.unit is not { Length: 2 } units || units[0] is not { Length: 15 } allies ||
                units[1] is not { Length: 15 } enemies)
                return;

            session.CommanderRouted = enemies[0]?.flag == 2;
            session.AlliesRemain = allies.Any(unit => unit?.flag == 1);
            session.FortVictory = War2Runtime.Outcome == War2Outcome.TacticalVictory;
            session.Ending = war_sys.end_event_no;
            session.GameOver = war_sys.game_over_flag;
        }
    }

    static bool ExpectedDifference(string path) =>
        path is "game_data.deru_pos[0]" or "game_data.deru_pos[1]" or "game_data.hon_time" or "game_data.hon_time2" ||
        System.Text.RegularExpressions.Regex.IsMatch(path,
            @"\Agame_data\.(play_time\[\d+\]|event_time\[\d+\]\[[12]\])\z",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);

    static void Compare(JsonElement before, JsonElement after, string path, List<string> diff)
    {
        if (diff.Count >= 256 || before.GetRawText() == after.GetRawText()) return;
        if (before.ValueKind == JsonValueKind.Object && after.ValueKind == JsonValueKind.Object)
        {
            foreach (var item in before.EnumerateObject())
            {
                if (after.TryGetProperty(item.Name, out var value)) Compare(item.Value, value, path + "." + item.Name, diff);
                else if (diff.Count < 256) diff.Add(path + "." + item.Name);
            }
            foreach (var item in after.EnumerateObject())
                if (!before.TryGetProperty(item.Name, out _) && diff.Count < 256) diff.Add(path + "." + item.Name);
            return;
        }
        if (before.ValueKind == JsonValueKind.Array && after.ValueKind == JsonValueKind.Array &&
            before.GetArrayLength() == after.GetArrayLength())
        {
            for (int i = 0; i < before.GetArrayLength() && diff.Count < 256; i++)
                Compare(before[i], after[i], path + "[" + i + "]", diff);
            return;
        }
        diff.Add(path);
    }

    [HarmonyPatch(typeof(MACHICON), nameof(MACHICON.MachiMain))]
    static class FieldHook
    {
        [HarmonyPrefix] static void Prefix(MACHICON __instance) => FieldTick(__instance);
    }

    [HarmonyPatch(typeof(WarChapter), nameof(WarChapter.Start))]
    static class WarStart
    {
        [HarmonyPrefix] static void Prefix(WarChapter __instance)
        {
            if (current is not { Committed: true } session) return;
            if (session.War != null || session.System.game_mode != 5 || session.System.war_no != session.WarNumber)
                Reject("Unexpected war chapter start.");
            else session.War = __instance;
        }
    }

    [HarmonyPatch(typeof(WarChapter), nameof(WarChapter.Update))]
    static class WarUpdate
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        static void Postfix(WarChapter __instance, ref int __result)
        {
            if (__result == 0 || current is not {} session || session.War?.Pointer != __instance.Pointer) return;

            int nativeResult = __result;
            session.NativeResult = nativeResult;
            if (Plugin.Options.EnableTrainingDefeatReturn.Value &&
                TrainingResultRules.CanRedirectDefeat(session.WarNumber, nativeResult, session.Ending, session.GameOver) &&
                Owns(session, session.Field) && session.System.game_mode == 5 &&
                session.System.war_no == session.WarNumber && session.Field._mstep == 48 &&
                session.Field.econ == null && session.SaveCalls == 0)
                __result = 1;

            session.Result = __result;
            Plugin.Logger.LogInfo($"Training War {session.WarNumber} completed. Native result={nativeResult}, manager result={__result}.");
        }
    }

    [HarmonyPatch(typeof(WarChapter), nameof(WarChapter.OnDestroy))]
    static class WarEnd
    {
        [HarmonyPostfix] static void Postfix(WarChapter __instance)
        {
            if (current is not { Committed: true } session) return;
            if(session.War?.Pointer!=__instance.Pointer||session.Destroyed)
            {
                Reject("Training teardown owner did not match the armed war.");
                return; // A foreign/duplicate teardown never authorizes flag restoration.
            }
            session.Destroyed=true;
            if(session.Result!=1)
            {
                if(session.NormalContract&&session.PrivateFlags!=null&&!RestoreFlags(session))faulted=true;
                Reject($"Training teardown observed result {session.Result}. A normal return is required.");
            }
        }
    }

    internal static void NativeLoadCompleted()
    {
        loadedEpoch=NativeWarBoundary.Epoch;
        if(current is {} old&&loadedEpoch>old.Epoch&&Framework.Singleton<GAME_WORK>.instance is {} live&&
           live.game_data?.Pointer!=old.DataOwner&&live.party_data?.Pointer!=old.PartyOwner)
        {current=null;requested=false;} // Never restore old arrays over a newly loaded campaign.
    }
    static bool RestoredState(Session s)
    {
        if(!s.Destroyed||s.Result!=1||s.PrivateFlags!=null||s.RestoredData==null||
           Framework.Singleton<GAME_WORK>.instance?.Pointer!=s.Game.Pointer||s.Epoch!=NativeWarBoundary.Epoch||
           s.Game.game_data?.Pointer!=s.DataOwner||s.Game.party_data?.Pointer!=s.PartyOwner||
           s.System.mcon?.Pointer!=s.Field.Pointer||s.System.game_mode!=1||s.Field.ano!=s.Area||s.Field.vno!=s.Town||s.Field.mno!=s.Map)return false;
        var flags=ReadFlags(s.Game);
        if(flags.Where((a,i)=>a.Pointer!=s.OriginalFlags[i].Pointer||!a.ToArray().SequenceEqual(s.Flags[i])).Any())return false;
        var after=Serialize(s.Game);string[] names={"game_data","war_data","chara_data","party_data"};var differences=new List<string>();
        for(int i=0;i<4;i++){using var a=JsonDocument.Parse(s.RestoredData[i]);using var b=JsonDocument.Parse(after[i]);Compare(a.RootElement,b.RootElement,names[i],differences);}
        return differences.Count<256&&differences.All(ExpectedDifference);
    }
    internal static bool AllowSaveIntent()
    {
        if(protectedSession is not {} s)return true;
        try
        {
            bool allowed=current==null&&!faulted&&RestoredState(s);
            if(!allowed)Plugin.Logger.LogWarning($"Training {s.Identity}: refused save intent while transient, faulted or unverified.");
            return allowed;
        }
        catch(Exception error){return FaultSave(s,"live ownership verification: "+error.GetType().Name);}
    }
    internal static bool AllowNativeSave(string path,string json)
    {
        if(!TrainingPersistenceRules.IsCampaignPath(path)||protectedSession is not {} s)return true;
        try
        {
            if(!AllowSaveIntent())return false;
            if(!TrainingPersistenceRules.PayloadMatches(json,s.Flags,Serialize(s.Game),out string reason))
                return FaultSave(s,reason);
            Plugin.Logger.LogInfo($"Training {s.Identity}: allowed native restored-state autosave payload; five flags and four records verified.");
            return true;
        }
        catch(Exception e){return FaultSave(s,"payload verification: "+e.GetType().Name);}
    }
    static bool FaultSave(Session s,string reason){faulted=true;Plugin.Logger.LogWarning($"Training {s.Identity}: refused unsafe native save ({reason}); restart/load required.");return false;}

    [HarmonyPatch(typeof(GSD2SaveData), nameof(GSD2SaveData.SaveFile))]
    static class SaveWitness
    {
        [HarmonyPrefix] static bool Prefix(Il2CppSystem.Action<bool, GSD2SaveData.TmpSave>? callback, ref bool __result)
        {
            // Rejected intent is not a disk write and must not invalidate an
            // otherwise intact native training return.
            if (!AllowSaveIntent())
            {
                __result=false;
                TrainingSaveFailure.Complete(callback);
                return false;
            }
            if(current is {} session) session.SaveCalls++;
            return true;
        }
    }
}
