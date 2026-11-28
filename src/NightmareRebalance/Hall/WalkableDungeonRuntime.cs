using UnityEngine;

namespace NightmareRebalance;

internal enum DungeonNodeKind { None, Puzzle, Shrine, Guardian, Portal }

// REAL WALKABLE DUNGEON: "The Tower of Circulating Paths" inside native Sindar Ruins (vc12).
// Warps the player from Castle HQ (vk07) directly into Sindar Ruins (vc12) and physically places
// interactive Sindar Puzzle, Secret Shrine, Stairwell Guardian, and Floor/Return Portal actors
// across walkable floor tiles. Defeating the physical Guardian unlocks the Portal to Floors 2..6
// and the Floor 6 Rune of Circulating Paths violet crystal!
internal static class WalkableDungeonRuntime
{
    internal const string DungeonMapId = "vc12";

    internal static bool DungeonModeActive { get; private set; }
    internal static int ReturnAno = 10, ReturnVno = 6, ReturnMno = 3;
    static readonly WarpTransitionGuard warpGuard = new();
    internal static bool WarpTransitionPending => warpGuard.TransitionPending;

    internal static MACHICON? Owner;
    internal static MACHIDAT? Source, Copy;
    internal static EVENTCON? Field;
    internal static EVENT_HUMAN? PuzzleActor, ShrineActor, GuardianActor, PortalActor;
    internal static int PuzzleIdx = -1, ShrineIdx = -1, GuardianIdx = -1, PortalIdx = -1;
    internal static DungeonNodeKind ActiveNode = DungeonNodeKind.None;
    internal static bool Modal, ReleaseFence;
    static int releaseFrames;
    static IntPtr Party;

    internal static bool InDungeon =>
        Owner != null && Copy != null && Field != null &&
        Owner.mdat?.Pointer == Copy.Pointer && Field.mdat?.Pointer == Copy.Pointer &&
        Framework.Singleton<GAME_WORK>.instance?.party_data?.Pointer == Party;

    internal static bool Idle =>
        InDungeon && CastleGuildRuntime.Config.Enabled && CastleGuildRuntime.Compatible &&
        HuntFieldContext.Active && Combat.State == null && !WarRuntime.Active &&
        Field!.mstep == 2 && Owner!.ctyp == 0 && !EVENTCON.noScriptRunning &&
        Field.coml != null && Field.coml.All(c => c == null || c.flg == 0) &&
        !HuntGuildRuntime.PartyMenuOpen && CastleGuildRuntime.InteractionDifficultySupported &&
        WindowManager.instance != null && !WindowManager.instance.IsUseMessageWindow &&
        !WindowManager.instance.IsOpenCommonDialogWindow();

    internal static bool Near
    {
        get
        {
            if (!InDungeon || Field!.ehuman.Count == 0 || Field.ehuman[0] is not {} p) return false;
            int x = p.x.ss.inte, y = p.y.ss.inte;
            if (PuzzleActor is { disp: 1 } pa && GuildApproach.AtCounter(x, y, pa.x.ss.inte, pa.y.ss.inte)) { ActiveNode = DungeonNodeKind.Puzzle; return true; }
            if (ShrineActor is { disp: 1 } sa && GuildApproach.AtCounter(x, y, sa.x.ss.inte, sa.y.ss.inte)) { ActiveNode = DungeonNodeKind.Shrine; return true; }
            if (GuardianActor is { disp: 1 } ga && GuildApproach.AtCounter(x, y, ga.x.ss.inte, ga.y.ss.inte)) { ActiveNode = DungeonNodeKind.Guardian; return true; }
            if (PortalActor is { disp: 1 } po && GuildApproach.AtCounter(x, y, po.x.ss.inte, po.y.ss.inte)) { ActiveNode = DungeonNodeKind.Portal; return true; }
            ActiveNode = DungeonNodeKind.None;
            return false;
        }
    }

    internal static bool Context => Modal && Idle && Near && !HuntGuildRuntime.RenderFaulted;
    internal static bool PromptVisible => !Modal && !ReleaseFence && Idle && Near && !HuntGuildRuntime.RenderFaulted;

    internal static bool ExperimentalWarpEnabled => Plugin.Options?.EnableExperimentalSindarDungeonWarp.Value == true;

    internal static void ClearOnCampaignLoad()
    {
        warpGuard.MarkTransitionSettled();
        DungeonModeActive = false;
        Detach();
    }

    static WarpCoordinateSnapshot CaptureSnapshot(GAME_WORK g, MACHICON mcon) => new(
        g.game_data.area_no, g.game_data.town_no, g.game_data.map_no,
        mcon.ano, mcon.vno, mcon.mno, mcon.ctyp,
        DungeonModeActive, ReturnAno, ReturnVno, ReturnMno);

    static void RestoreSnapshot(GAME_WORK g, MACHICON mcon, WarpCoordinateSnapshot snap)
    {
        g.game_data.area_no = snap.GameArea;
        g.game_data.town_no = snap.GameTown;
        g.game_data.map_no = snap.GameMap;
        mcon.ano = snap.MachiArea;
        mcon.vno = snap.MachiTown;
        mcon.mno = snap.MachiMap;
        mcon.ctyp = snap.MachiCtyp;
        DungeonModeActive = snap.DungeonModeActive;
        ReturnAno = snap.ReturnAno;
        ReturnVno = snap.ReturnVno;
        ReturnMno = snap.ReturnMno;
    }

    internal static bool WarpIntoSindarDungeon(out string feedback)
    {
        if (!ExperimentalWarpEnabled)
        {
            feedback = "Experimental Sindar Ruins warp is disabled (EnableExperimentalSindarDungeonWarp = false). Using safe vk07 Sindar Spire interface.";
            return false;
        }
        var g = Framework.Singleton<GAME_WORK>.instance;
        var sys = g?.sys_work;
        var mcon = sys?.mcon;
        if (g?.game_data == null || sys == null || mcon == null ||
            sys.game_mode != 1 || sys.now_save != 0 || mcon._mstep != 20 ||
            EVENTCON.noScriptRunning || TrainingWar.Active || HallLucaSession.Active || HuntAccess.Ticket.Armed || WarpTransitionPending)
        {
            feedback = "Cannot open the Sindar portal right now (field must be idle with no active save, war, or pending warp).";
            return false;
        }
        var loc = FindMapLocation(DungeonMapId);
        if (loc is not {} target)
        {
            feedback = $"Sindar Ruins ({DungeonMapId}) routing not found.";
            return false;
        }
        var before = CaptureSnapshot(g, mcon);
        if (warpGuard.TryExecute(
            before,
            () =>
            {
                ReturnAno = mcon.ano;
                ReturnVno = mcon.vno;
                ReturnMno = mcon.mno;
                DungeonModeActive = true;
                CirculatingPathsRuntime.EnterOrResume();
                g.game_data.area_no = (sbyte)target.Area;
                g.game_data.town_no = (sbyte)target.Town;
                g.game_data.map_no = 0;
                mcon.ano = (sbyte)target.Area;
                mcon.vno = (sbyte)target.Town;
                mcon.mno = 0;
                mcon.ctyp = 0;
            },
            () => mcon.MachiOutCom(),
            snap => RestoreSnapshot(g, mcon, snap),
            out var ex))
        {
            Plugin.Logger.LogInfo($"WalkableDungeonRuntime: experimental warp into {DungeonMapId} (ano={target.Area}, vno={target.Town}) for logical Stage {CirculatingPathsRuntime.CurrentFloor}/6.");
            feedback = $"Experimental warp to Sindar Ruins ({DungeonMapId}) — Logical Stage {CirculatingPathsRuntime.CurrentFloor}/6.";
            return true;
        }
        Plugin.Logger.LogWarning($"WalkableDungeonRuntime: warp failed and rolled back: {ex?.Message}");
        feedback = "Experimental Sindar portal warp failed; coordinates restored. Using safe vk07 interface.";
        return false;
    }

    internal static bool WarpBackToCastle(out string feedback)
    {
        var g = Framework.Singleton<GAME_WORK>.instance;
        var mcon = g?.sys_work?.mcon;
        if (g?.game_data == null || mcon == null || WarpTransitionPending)
        {
            feedback = "Cannot return to Castle right now.";
            return false;
        }
        var before = CaptureSnapshot(g, mcon);
        if (warpGuard.TryExecute(
            before,
            () =>
            {
                DungeonModeActive = false;
                g.game_data.area_no = (sbyte)ReturnAno;
                g.game_data.town_no = (sbyte)ReturnVno;
                g.game_data.map_no = (sbyte)ReturnMno;
                mcon.ano = (sbyte)ReturnAno;
                mcon.vno = (sbyte)ReturnVno;
                mcon.mno = (sbyte)ReturnMno;
                mcon.ctyp = 0;
            },
            () => mcon.MachiOutCom(),
            snap => RestoreSnapshot(g, mcon, snap),
            out var ex))
        {
            feedback = "Returning to Dunan Castle HQ...";
            return true;
        }
        Plugin.Logger.LogWarning($"WalkableDungeonRuntime: return warp failed and rolled back: {ex?.Message}");
        feedback = "Return portal failed; state restored — exit Sindar Ruins on foot.";
        return false;
    }

    static (int Area, int Town)? FindMapLocation(string mapId)
    {
        try
        {
            var ids = MachiLoader.GetMachiIDList();
            if (ids == null) return null;
            for (int a = 0; a < ids.Count; a++)
            {
                var row = ids[a];
                if (row == null) continue;
                for (int v = 0; v < row.Count; v++)
                    if (string.Equals(row[v], mapId, StringComparison.OrdinalIgnoreCase))
                        return (a, v);
            }
        }
        catch { }
        return null;
    }

    static ushort PickDungeonSprite(MACHIDAT source)
    {
        if (source.epersondat == null) return 0;
        // Prefer clean standing person sprites; fall back to any valid sprite in vc12
        for (int i = 0; i < source.epersondat.Length; i++)
        {
            var p = source.epersondat[i];
            if (p?.hito_opd_table is {} o && o.path == "person" && o.anime_kazu == 4 && o.pattern_kazu == 12)
                return p.fp_no;
        }
        for (int i = 0; i < source.epersondat.Length; i++)
        {
            var p = source.epersondat[i];
            if (p?.hito_opd_table != null) return p.fp_no;
        }
        return 0;
    }

    internal static void BeforeInit(MACHICON m)
    {
        warpGuard.MarkTransitionSettled();
        if (!ExperimentalWarpEnabled || !CastleGuildRuntime.Config.Enabled || Application.isBatchMode || !CastleGuildRuntime.Compatible) return;
        if (Combat.State != null || WarRuntime.Active || Owner != null || m?.mdat is not {} source) return;
        string mapId = MachiLoader.GetMachiID(m.ano, m.vno);
        if (!string.Equals(mapId, DungeonMapId, StringComparison.OrdinalIgnoreCase)) return;
        if (!DungeonModeActive && !CirculatingPathsRuntime.RunActive) return;
        if (source.eventdata?.mapeventdat == null || m.mno < 0 || m.mno >= source.eventdata.mapeventdat.Count) return;

        var party = Framework.Singleton<GAME_WORK>.instance?.party_data;
        if (party == null) return;

        CirculatingPathsRuntime.EnterOrResume();
        ushort sprite = PickDungeonSprite(source);
        if (sprite == 0) return;

        var copy = UnityEngine.Object.Instantiate(source);
        copy.name = source.name;
        PuzzleIdx = ShrineIdx = GuardianIdx = PortalIdx = -1;
        try
        {
            var events = new EVENTDAT();
            events.sce_msg = source.eventdata.sce_msg;
            events.mapeventdat = new Il2CppSystem.Collections.Generic.List<MAPEVDAT>();
            for (int i = 0; i < source.eventdata.mapeventdat.Count; i++)
            {
                var old = source.eventdata.mapeventdat[i];
                bool add = i == m.mno;
                // Anchor 4 physical interactive nodes relative to player spawn (m.px/8, m.py/8) so they're always reachable!
                int bx = Math.Clamp(m.px > 0 ? m.px / 8 : 20, 8, 80);
                int by = Math.Clamp(m.py > 0 ? m.py / 8 : 20, 8, 80);
                int extra = add ? 4 : 0;
                var map = new MAPEVDAT
                {
                    mapno = old.mapno, batlno = old.batlno, batltim = old.batltim, batlpro = old.batlpro,
                    mapst = old.mapst, okaz = checked((byte)(old.okaz + extra)), evkaz = old.evkaz, hkaz = old.hkaz
                };
                var objects = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<EVENT_OBJ>(old.eventobj.Length + extra);
                for (int j = 0; j < old.eventobj.Length; j++)
                {
                    var o = old.eventobj[j];
                    objects[j] = new EVENT_OBJ { otyp = o.otyp, disp = o.disp, spd = o.spd, wt = o.wt, ano = o.ano, ityp = o.ityp, fpno = o.fpno, ozok = o.ozok, pri = o.pri, x = o.x, y = o.y, w = o.w, h = o.h };
                }
                if (add)
                {
                    int slot = old.eventobj.Length;
                    PortalIdx = slot;
                    objects[slot++] = new EVENT_OBJ { otyp = 1, disp = 1, spd = 4, wt = 255, ano = 0, ityp = 0, fpno = sprite, ozok = 0, pri = 0, x = (short)(bx - 3), y = (short)by, w = 0, h = 0 };
                    PuzzleIdx = slot;
                    objects[slot++] = new EVENT_OBJ { otyp = 1, disp = 1, spd = 4, wt = 255, ano = 0, ityp = 0, fpno = sprite, ozok = 0, pri = 0, x = (short)(bx + 3), y = (short)(by - 2), w = 0, h = 0 };
                    ShrineIdx = slot;
                    objects[slot++] = new EVENT_OBJ { otyp = 1, disp = 1, spd = 4, wt = 255, ano = 0, ityp = 0, fpno = sprite, ozok = 0, pri = 0, x = (short)(bx - 3), y = (short)(by - 4), w = 0, h = 0 };
                    GuardianIdx = slot;
                    objects[slot++] = new EVENT_OBJ { otyp = 1, disp = 1, spd = 4, wt = 255, ano = 0, ityp = 0, fpno = sprite, ozok = 0, pri = 0, x = (short)bx, y = (short)(by - 6), w = 0, h = 0 };
                }
                var scripts = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<EVENT_WK>(old.eventdat.Length);
                for (int j = 0; j < old.eventdat.Length; j++)
                {
                    var e = old.eventdat[j];
                    scripts[j] = new EVENT_WK { etyp = e.etyp, eventForm = e.eventForm.ToArray(), eventCom = e.eventCom.ToArray() };
                }
                map.eventobj = objects;
                map.eventdat = scripts;
                events.mapeventdat.Add(map);
            }
            copy.eventdata = events;
            Source = source;
            Copy = copy;
            Owner = m;
            Party = party.Pointer;
            m.mdat = Copy;
            Plugin.Logger.LogInfo($"WalkableDungeonRuntime: populated real Sindar Ruins ({mapId}) for Floor {CirculatingPathsRuntime.CurrentFloor}/6 with 4 physical nodes (sprite={sprite}).");
        }
        catch (Exception ex)
        {
            UnityEngine.Object.Destroy(copy);
            Plugin.Logger.LogWarning($"WalkableDungeonRuntime: setup failed: {ex.Message}");
        }
    }

    internal static void AfterInit(MACHICON m, EVENTCON e)
    {
        if (Owner?.Pointer != m.Pointer || Copy == null) return;
        Field = e;
        if (e?.ehuman == null) return;
        PortalActor = PortalIdx >= 0 && PortalIdx + 8 < e.ehuman.Count ? e.ehuman[PortalIdx + 8] : null;
        PuzzleActor = PuzzleIdx >= 0 && PuzzleIdx + 8 < e.ehuman.Count ? e.ehuman[PuzzleIdx + 8] : null;
        ShrineActor = ShrineIdx >= 0 && ShrineIdx + 8 < e.ehuman.Count ? e.ehuman[ShrineIdx + 8] : null;
        GuardianActor = GuardianIdx >= 0 && GuardianIdx + 8 < e.ehuman.Count ? e.ehuman[GuardianIdx + 8] : null;
        RefreshActorVisibility();
    }

    internal static void RefreshActorVisibility()
    {
        if (PortalActor != null) { PortalActor.disp = 1; PortalActor.forceHDDisable = false; }
        if (PuzzleActor != null)
        {
            bool showPuzzle = !CirculatingPathsRuntime.AlcoveSearchedThisFloor && CirculatingPathsRules.Floor(CirculatingPathsRuntime.CurrentFloor).OrbsOnFloor > 0;
            PuzzleActor.disp = (byte)(showPuzzle ? 1 : 0);
            PuzzleActor.forceHDDisable = !showPuzzle;
        }
        if (ShrineActor != null)
        {
            bool showShrine = !CirculatingPathsRuntime.ShrineVisitedThisFloor;
            ShrineActor.disp = (byte)(showShrine ? 1 : 0);
            ShrineActor.forceHDDisable = !showShrine;
        }
        if (GuardianActor != null)
        {
            bool showGuard = !CirculatingPathsRuntime.GuardianDefeatedThisFloor;
            GuardianActor.disp = (byte)(showGuard ? 1 : 0);
            GuardianActor.forceHDDisable = !showGuard;
        }
    }

    internal static (string Header, string Prompt) ActivePromptBanner()
    {
        var spec = CirculatingPathsRules.Floor(CirculatingPathsRuntime.CurrentFloor);
        return ActiveNode switch
        {
            DungeonNodeKind.Puzzle => ($"Sindar Puzzle  [{spec.ShortTag}]", $"A glowing Sindar mechanism holds +{spec.OrbsOnFloor} Red Orb(s). Press Confirm to inspect."),
            DungeonNodeKind.Shrine => ($"Secret Shrine: {spec.ShrineTitle}", $"{spec.ShrineDescription} Press Confirm to activate."),
            DungeonNodeKind.Guardian => ($"Stairwell Guardian  [⚔ {spec.GuardianName}]", $"{spec.GuardianMechanicName}: {spec.GuardianMechanicDesc}. Press Confirm to challenge!"),
            DungeonNodeKind.Portal => CirculatingPathsRuntime.SummitCrystalReady
                ? ("Rune of Circulating Paths  [★ SUMMIT]", "The floating violet crystal pulses with ancient power! Press Confirm to touch it.")
                : CirculatingPathsRuntime.GuardianDefeatedThisFloor
                    ? ($"Stairwell Portal  [✓ UNLOCKED]", $"The path to Floor {CirculatingPathsRuntime.CurrentFloor + 1}/6 is open! Press Confirm to ascend or return to Castle.")
                    : ("Sindar Entrance Portal", "Return to Dunan Castle HQ (your Red Orbs & Floor progress are saved), or read this floor's Inscription."),
            _ => ("Tower of Circulating Paths", "Explore the Sindar Ruins.")
        };
    }

    static bool Press(GRInputManager.Type t) => GRInputManager.IsInstantiated && GRInputManager.Instance.keys != null && GRInputManager.Instance.keys.TryGetValue(t, out var k) && k != null && k.on;
    static bool Held(GRInputManager.Type t) => GRInputManager.IsInstantiated && GRInputManager.Instance.keys != null && GRInputManager.Instance.keys.TryGetValue(t, out var k) && k != null && k.press;

    internal static bool CanTalk(EVENTCON e) =>
        !Modal && !ReleaseFence && Field?.Pointer == e.Pointer && Idle && Near && !HuntGuildRuntime.RenderFaulted && Press(GRInputManager.Type.Decide);

    internal static void OpenAfterNative(EVENTCON e)
    {
        if (Modal || ReleaseFence || Field?.Pointer != e.Pointer || !Idle || !Near || HuntGuildRuntime.RenderFaulted) return;
        Modal = true;
        var spec = CirculatingPathsRules.Floor(CirculatingPathsRuntime.CurrentFloor);
        if (ActiveNode == DungeonNodeKind.Puzzle)
        {
            CastleGuildRuntime.Dialogue.Open(CastleDialogueScripts.CirculatingPuzzleScript(CirculatingPathsRuntime.CurrentFloor));
        }
        else if (ActiveNode == DungeonNodeKind.Shrine)
        {
            string cost = spec.ShrinePotchCost > 0 ? $"{spec.ShrinePotchCost:N0} Potch" : $"{spec.ShrineCommCost} Commendation(s)";
            CastleGuildRuntime.Dialogue.Open(new(spec.ShrineTitle, $"Sindar Ruins · Floor {CirculatingPathsRuntime.CurrentFloor}/6",
                new[] { spec.ShrineDescription },
                new[] { new DialogueChoiceSpec($"Activate ({cost})", "sajah-walk-shrine"), new DialogueChoiceSpec("Step Back", "close") }));
        }
        else if (ActiveNode == DungeonNodeKind.Guardian)
        {
            CastleGuildRuntime.Dialogue.Open(new(spec.GuardianName, $"Stairwell Guardian · Floor {CirculatingPathsRuntime.CurrentFloor}/6",
                new[] { $"The {spec.GuardianName} blocks the stairwell! [{spec.GuardianMechanicName}: {spec.GuardianMechanicDesc}]" },
                new[] { new DialogueChoiceSpec($"Challenge {spec.GuardianName}!", "sajah-guardian"), new DialogueChoiceSpec("Step Back", "close") }));
        }
        else
        {
            if (CirculatingPathsRuntime.SummitCrystalReady)
            {
                CastleGuildRuntime.Dialogue.Open(CastleDialogueScripts.CirculatingPathsScript(
                    CirculatingPathsRuntime.CurrentFloor, CirculatingPathsRuntime.RedOrbsCollected, true, true, true, true, CirculatingPathsRuntime.CycleRank));
            }
            else
            {
                var choices = new List<DialogueChoiceSpec>(3);
                if (CirculatingPathsRuntime.GuardianDefeatedThisFloor)
                    choices.Add(new DialogueChoiceSpec(CirculatingPathsRuntime.CurrentFloor < CirculatingPathsRules.TotalFloors ? $"Ascend to Floor {CirculatingPathsRuntime.CurrentFloor + 1}/6" : "Touch the Violet Crystal", "sajah-walk-ascend"));
                choices.Add(new DialogueChoiceSpec("Read Ancient Sindar Inscription", "sajah-walk-lore"));
                choices.Add(new DialogueChoiceSpec("Return to Dunan Castle HQ", "sajah-return-castle"));
                choices.Add(new DialogueChoiceSpec("Keep Exploring", "close"));
                CastleGuildRuntime.Dialogue.Open(new(spec.Name, $"Sindar Ruins · Floor {CirculatingPathsRuntime.CurrentFloor}/6",
                    new[] { $"{spec.Atmosphere}\nRed Orbs: {CirculatingPathsRules.OrbProgressBar(CirculatingPathsRuntime.RedOrbsCollected)} ({CirculatingPathsRuntime.RedOrbsCollected}/{CirculatingPathsRules.TotalRedOrbs})" },
                    choices.ToArray()));
            }
        }
    }

    internal static bool Tick(EVENTCON e)
    {
        if (Field?.Pointer != e.Pointer) return true;
        if (!Idle || !Near) { Close(false); return true; }
        if (ReleaseFence)
        {
            if (++releaseFrames > 120 || (!Held(GRInputManager.Type.Decide) && !Held(GRInputManager.Type.Cancel)))
            {
                ReleaseFence = false;
                return true;
            }
            e.EventPlayerDisp(false);
            return false;
        }
        if (!Modal) return true;
        if (CastleGuildRuntime.Dialogue.Active)
        {
            bool handled = CastleGuildRuntime.TickSharedDialogue(e);
            RefreshActorVisibility();
            if (!CastleGuildRuntime.Dialogue.Active) Close(true);
            return handled;
        }
        Close(false);
        return true;
    }

    internal static void Close(bool fence)
    {
        CastleGuildRuntime.Dialogue.Close();
        Modal = false;
        ReleaseFence = fence;
        releaseFrames = 0;
    }

    internal static void Detach()
    {
        Close(false);
        if (Owner != null && Copy != null && Owner.mdat?.Pointer == Copy.Pointer) Owner.mdat = Source;
        if (Copy != null) UnityEngine.Object.Destroy(Copy);
        Owner = null; Source = null; Copy = null; Field = null;
        PuzzleActor = ShrineActor = GuardianActor = PortalActor = null;
        PuzzleIdx = ShrineIdx = GuardianIdx = PortalIdx = -1;
        ActiveNode = DungeonNodeKind.None;
        Party = IntPtr.Zero;
    }
}
