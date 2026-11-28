using HarmonyLib;
using UnityEngine;

namespace NightmareRebalance;

// Spreads the 6 specialist Castle NPCs naturally across different Castle HQ rooms (vk01..vk08)
// instead of crowding them all into vk07. Strictly uses verified generic standing v* person
// sprites (anime_kazu == 4 && pattern_kazu == 12) so main-hero/cutscene poses never appear.
internal static class CastleRoomNpcRuntime
{
    static readonly Dictionary<string, CastleNpcRole> RoomRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["vk01"] = CastleNpcRole.DrillSergeant,   // Castle Great Hall
        ["vk02"] = CastleNpcRole.TowerKeeper,     // Castle Courtyard / Entrance
        ["vk03"] = CastleNpcRole.WarChronicler,   // Castle Strategy / War Room
        ["vk04"] = CastleNpcRole.Quartermaster,   // Castle Armory / Hall
        ["vk05"] = CastleNpcRole.EchoArchivist,   // Castle Library / Study
        ["vk08"] = CastleNpcRole.FieldScout       // Castle Tavern
    };

    internal static MACHICON? Owner;
    internal static MACHIDAT? Source, Copy;
    internal static EVENTCON? Field;
    internal static EVENT_HUMAN? Resident;
    internal static CastleNpcRole ActiveRole = CastleNpcRole.GuildAttendant;
    internal static int ResidentIndex = -1;
    internal static bool Modal, ReleaseFence;
    static int releaseFrames;
    static IntPtr Party;

    internal static bool Owned =>
        Owner != null && Copy != null && Field != null && Resident != null &&
        Owner.mdat?.Pointer == Copy.Pointer && Field.mdat?.Pointer == Copy.Pointer &&
        Framework.Singleton<GAME_WORK>.instance?.party_data?.Pointer == Party;

    internal static bool Idle =>
        Owned && CastleGuildRuntime.Config.Enabled && CastleGuildRuntime.Compatible &&
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
            if (!Owned || Field!.ehuman.Count == 0 || Field.ehuman[0] is not {} p || Resident is not { disp: 1 })
                return false;
            int x = p.x.ss.inte, y = p.y.ss.inte;
            return GuildApproach.AtCounter(x, y, Resident.x.ss.inte, Resident.y.ss.inte);
        }
    }

    internal static bool Context => Modal && Idle && Near && !HuntGuildRuntime.RenderFaulted;
    internal static bool PromptVisible => !Modal && !ReleaseFence && Idle && Near && !HuntGuildRuntime.RenderFaulted;

    static ushort PickStandingGenericSprite(MACHIDAT source)
    {
        if (source.epersondat == null) return 0;
        for (int i = 0; i < source.epersondat.Length; i++)
        {
            var p = source.epersondat[i];
            if (p?.hito_opd_table is not {} opd) continue;
            if (opd.path == "person" && opd.anime_kazu == 4 && opd.pattern_kazu == 12 &&
                !string.IsNullOrEmpty(opd.id) && opd.id.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                return p.fp_no;
        }
        return 0;
    }

    static bool TileWalkable(MACHIDAT source, MAPEVDAT old, int x, int y)
    {
        if (x < 4 || y < 4 || x > 120 || y > 120) return false;
        for (int i = 0; i < old.eventobj.Length; i++)
        {
            var o = old.eventobj[i];
            if (o != null && Math.Abs(o.x - x) <= 2 && Math.Abs(o.y - y) <= 2) return false;
        }
        // Verify actual native MAP collision grid before admitting any tile.
        try
        {
            if (source.map == null || old.mapno < 0 || old.mapno >= source.map.Count) return false;
            var mapData = source.map[old.mapno];
            if (mapData?.index == null || mapData.srn_type == null || mapData.srn_type.Count == 0) return false;
            var bg = new BGCON
            {
                map = mapData,
                index = mapData.index,
                srn = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<TSRN>(
                    Enumerable.Range(0, mapData.srn_type.Count).Select(i => new TSRN
                    {
                        sw = mapData.srn_type[i].w,
                        sh = mapData.srn_type[i].h
                    }).ToArray())
            };
            int px = x * 8, py = y * 8;
            int Collide(int cx, int cy) => Enumerable.Range(0, mapData.srn_kazu).Sum(i => bg.EventMapAtariCheckF1(0, i, cx, cy));
            if (Collide(px, py) != 0 || Collide(px, py + 16) != 0 || Collide(px - 7, py + 16) != 0 || Collide(px + 7, py + 16) != 0)
                return false;
            if (mapData.tokusyu_atari != null)
            {
                for (int i = 0; i < mapData.tokusyu_atari.Count; i++)
                {
                    var area = mapData.tokusyu_atari[i];
                    if (!(area.x > px + 7 || area.x + area.w < px - 7 || area.y > py + 22 || area.y + area.h < py))
                        return false;
                }
            }
            return true;
        }
        catch { return false; }
    }

    static (short X, short Y)? PickSafeFloorSpot(MACHIDAT source, MAPEVDAT old)
    {
        var anchors = new List<(int X, int Y)>();
        for (int i = 0; i < old.eventobj.Length; i++)
        {
            var o = old.eventobj[i];
            if (o != null && o.otyp == 1 && o.disp == 1 && o.x > 4 && o.y > 4)
                anchors.Add((o.x, o.y));
        }
        (int Dx, int Dy)[] offsets = { (3, 0), (-3, 0), (0, 3), (0, -3), (4, 0), (-4, 0), (3, 2), (-3, 2) };
        foreach (var (ax, ay) in anchors)
        {
            foreach (var (dx, dy) in offsets)
            {
                int cx = ax + dx, cy = ay + dy;
                if (TileWalkable(source, old, cx, cy))
                    return ((short)cx, (short)cy);
            }
        }
        return null;
    }

    internal static void BeforeInit(MACHICON m)
    {
        if (!CastleGuildRuntime.Config.Enabled || Plugin.Options?.EnableExperimentalCastleRoomNpcs.Value != true) return;
        if (Application.isBatchMode || !CastleGuildRuntime.Compatible || Combat.State != null || WarRuntime.Active) return;
        if (Owner != null || m?.mdat is not {} source) return;
        string mapId = MachiLoader.GetMachiID(m.ano, m.vno);
        if (!RoomRoles.TryGetValue(mapId, out var role)) return;
        if (source.eventdata?.mapeventdat == null || m.mno < 0 || m.mno >= source.eventdata.mapeventdat.Count) return;

        ushort sprite = PickStandingGenericSprite(source);
        if (sprite == 0)
        {
            Plugin.Logger.LogInfo($"CastleRoomNpcRuntime: {mapId} has no generic standing v* person sprite; skipping {role}.");
            return;
        }
        var oldRoom = source.eventdata.mapeventdat[m.mno];
        if (oldRoom?.eventobj == null || oldRoom.eventdat == null) return;
        var spot = PickSafeFloorSpot(source, oldRoom);
        if (spot is not {} pos) return;

        var party = Framework.Singleton<GAME_WORK>.instance?.party_data;
        if (party == null) return;

        var copy = UnityEngine.Object.Instantiate(source);
        copy.name = source.name;
        try
        {
            var events = new EVENTDAT();
            events.sce_msg = source.eventdata.sce_msg;
            events.mapeventdat = new Il2CppSystem.Collections.Generic.List<MAPEVDAT>();
            for (int i = 0; i < source.eventdata.mapeventdat.Count; i++)
            {
                var old = source.eventdata.mapeventdat[i];
                bool add = i == m.mno;
                int extra = add ? 1 : 0;
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
                    ResidentIndex = old.eventobj.Length;
                    objects[ResidentIndex] = new EVENT_OBJ { otyp = 1, disp = 1, spd = 4, wt = 255, ano = 0, ityp = 0, fpno = sprite, ozok = 0, pri = 0, x = pos.X, y = pos.Y, w = 0, h = 0 };
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
            ActiveRole = role;
            m.mdat = Copy;
            Plugin.Logger.LogInfo($"CastleRoomNpcRuntime: placed {role} in {mapId} at ({pos.X},{pos.Y}) with generic standing sprite {sprite}.");
        }
        catch (Exception ex)
        {
            UnityEngine.Object.Destroy(copy);
            Plugin.Logger.LogWarning($"CastleRoomNpcRuntime: skipped {mapId}: {ex.Message}");
        }
    }

    internal static void AfterInit(MACHICON m, EVENTCON e)
    {
        if (Owner?.Pointer != m.Pointer || Copy == null) return;
        Field = e;
        if (e?.ehuman == null || ResidentIndex < 0 || ResidentIndex + 8 >= e.ehuman.Count) return;
        Resident = e.ehuman[ResidentIndex + 8];
        if (Resident != null) { Resident.disp = 1; Resident.forceHDDisable = false; }
    }

    static bool Press(GRInputManager.Type t) => GRInputManager.IsInstantiated && GRInputManager.Instance.keys != null && GRInputManager.Instance.keys.TryGetValue(t, out var k) && k != null && k.on;
    static bool Held(GRInputManager.Type t) => GRInputManager.IsInstantiated && GRInputManager.Instance.keys != null && GRInputManager.Instance.keys.TryGetValue(t, out var k) && k != null && k.press;

    internal static bool CanTalk(EVENTCON e) =>
        !Modal && !ReleaseFence && Field?.Pointer == e.Pointer && Idle && Near && !HuntGuildRuntime.RenderFaulted && Press(GRInputManager.Type.Decide);

    internal static void OpenAfterNative(EVENTCON e)
    {
        if (Modal || ReleaseFence || Field?.Pointer != e.Pointer || !Idle || !Near || HuntGuildRuntime.RenderFaulted) return;
        Modal = true;
        CastleGuildRuntime.ActiveNpc = ActiveRole;
        if (ActiveRole is CastleNpcRole.TowerKeeper or CastleNpcRole.FieldScout or CastleNpcRole.WarChronicler)
        {
            CastleGuildRuntime.Dialogue.Open(CastleDialogueScripts.Build(ActiveRole, ArmyProgressionRuntime.Current, TowerOfDunanRuntime.PendingRelicDraft, TowerOfDunanRuntime.FloorsCleared));
            HuntGuildRuntime.Visible = false;
            return;
        }
        var menu = CastleGuildRuntime.Menu;
        menu.EchoCount = HallTrialCatalogue.Entries.Length + 1;
        menu.SpoilerRow = HallTrialCatalogue.Entries.Length;
        menu.ArmyRosterCount = Math.Max(1, ArmyProgressionRuntime.RosterSnapshot().Length);
        menu.PartyMasteryCount = Math.Max(1, ArmyProgressionRuntime.PartyMasterySnapshot().Length);
        menu.TowerRowCount = TowerOfDunanRuntime.PendingRelicDraft ? TowerOfDunanRuntime.DraftChoices.Length : 2;
        if (ActiveRole == CastleNpcRole.DrillSergeant) menu.OpenDirect(GuildPage.Training, HuntRuntime.BrowseOrder.Length);
        else if (ActiveRole == CastleNpcRole.EchoArchivist) menu.OpenDirect(GuildPage.Echoes, HuntRuntime.BrowseOrder.Length);
        else if (ActiveRole == CastleNpcRole.Quartermaster) menu.OpenDirect(GuildPage.Quartermaster, HuntRuntime.BrowseOrder.Length);
        HuntGuildRuntime.Visible = true;
        HuntGuildRuntime.LastRenderFrame = -1;
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
            if (!CastleGuildRuntime.Dialogue.Active && !HuntGuildRuntime.Visible) Close(true);
            return handled;
        }
        if (!HuntGuildRuntime.Visible || HuntGuildRuntime.RenderFaulted) { Close(false); return true; }
        if (Press(GRInputManager.Type.Cancel)) { Close(true); e.EventPlayerDisp(false); return false; }
        e.EventPlayerDisp(false);
        return false;
    }

    internal static void Close(bool fence)
    {
        CastleGuildRuntime.Dialogue.Close();
        if (Modal) { HuntGuildRuntime.Visible = false; HuntGuildRuntime.LastRenderFrame = -1; }
        Modal = false;
        ReleaseFence = fence;
        releaseFrames = 0;
    }

    internal static void Detach()
    {
        Close(false);
        if (Owner != null && Copy != null && Owner.mdat?.Pointer == Copy.Pointer) Owner.mdat = Source;
        if (Copy != null) UnityEngine.Object.Destroy(Copy);
        Owner = null; Source = null; Copy = null; Field = null; Resident = null;
        ResidentIndex = -1; Party = IntPtr.Zero;
    }
}
