using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace NightmareRebalance;
internal static class HallLucaSession
{
    internal static HallTrialDefinition[] Stages=>HallTrialCatalogue.LucaStages;
    static GAME_WORK? game;static long epoch;static bool running,nightmare,waiting;static HallGauntletRules? rules;static HallPartyLease? lease;
    static readonly List<int>[] teams={new(),new(),new()};static HallNativeRoster.Candidate[] roster=Array.Empty<HallNativeRoster.Candidate>();
    static AsyncOperationHandle<IMPORT_GAME_DATA>? equipmentSource;static HallPartyEquipment? equipment;
    static bool nativeHandoff;
    static int group;static bool restored;static byte[] originalMembers=Array.Empty<byte>();
    internal static bool Active=>game!=null;
    internal static bool Selecting=>Active&&!running;
    internal static bool HoldsFieldControls=>running&&Active;
    internal static int ChoiceCount=>roster.Length+2;
    internal static int CarryHp=>rules?.CarryHp??6500;
    internal static bool FinalStage=>running&&rules?.Stage==2;
    internal static bool IsStage(string id)=>id is "luca-gauntlet" or "luca-stage-2" or "luca-stage-3";
    internal static byte[]? OriginalMembers=>Active?originalMembers:null;
    internal static bool Begin(bool advanced)
    {
        if(Active||Framework.Singleton<GAME_WORK>.instance is not {} g)return false;
        roster=HallNativeRoster.Read(g);
        if(roster.Length<18||!roster.Any(c=>c.Id==1)){CastleGuildRuntime.Status="Requires Riou and eighteen healthy, available single-slot combatants.";return false;}
        game=g;epoch=NativeWarBoundary.Epoch;nightmare=advanced;group=0;running=false;waiting=false;restored=false;
        originalMembers=g.party_data.party_cha_no.ToArray();foreach(var t in teams)t.Clear();
        try
        {
            var draft=Plugin.Options.LucaPartyDraft.Value.Split(';').Select(row=>row.Split(',').Select(int.Parse).ToArray()).ToArray();
            var available=roster.Select(c=>(int)c.Id).ToHashSet();
            if(HallGauntletRules.DistinctTeams(draft)&&draft[2][0]==1&&draft.SelectMany(x=>x).All(available.Contains))
            {for(int i=0;i<3;i++)teams[i].AddRange(draft[i]);group=2;CastleGuildRuntime.Status="Prepared teams restored. Review them, then choose Begin Gauntlet.";}
        }catch(FormatException){}catch(IndexOutOfRangeException){}
        return true;
    }
    internal static void Fail(string message)
    {waiting=false;CastleGuildRuntime.Status="Gauntlet stopped: "+message;Plugin.Logger.LogError(CastleGuildRuntime.Status);if(lease==null){running=false;game=null;}}
    internal static object AdmissionStatus=>new{running,waiting,Stage=rules?.Stage,Bound=lease?.Bound,Game=game!=null,
        Epoch=epoch,CurrentEpoch=NativeWarBoundary.Epoch,SameGame=game!=null&&Framework.Singleton<GAME_WORK>.instance?.Pointer==game.Pointer};
    internal static bool OwnsIdleRequest(HallTrialDefinition? definition)=>running&&lease!=null&&rules!=null&&game!=null&&
        lease.Bound&&definition?.TableId==Stages[rules.Stage].TableId&&epoch==NativeWarBoundary.Epoch&&
        Framework.Singleton<GAME_WORK>.instance?.Pointer==game.Pointer&&CastleGuildRuntime.Owner?._mstep==20&&CastleGuildRuntime.Idle&&CastleGuildRuntime.Near&&
        !CastleGuildRuntime.Modal&&!HuntGuildRuntime.RenderFaulted;
    internal static void Cancel(){if(running)return;game=null;roster=Array.Empty<HallNativeRoster.Candidate>();}
    internal static bool Choose(int index)
    {
        if(!Selecting)return false;
        if(index==roster.Length){if(teams[group].Count>0)teams[group].RemoveAt(teams[group].Count-1);else if(group>0)group--;return false;}
        if(index==roster.Length+1)
        {
            if(!HallGauntletRules.DistinctTeams(teams.Select(t=>t.ToArray()).ToArray())||teams[2][0]!=1){CastleGuildRuntime.Status="Choose six distinct characters for each of the three groups first.";return false;}
            try{Plugin.Options.LucaPartyDraft.Value=string.Join(";",teams.Select(t=>string.Join(",",t)));Plugin.Options.PreferenceFile.Save();}
            catch(Exception e){Plugin.Logger.LogWarning("Party draft was not saved: "+e.Message);}
            running=true;rules=new HallGauntletRules(nightmare?HallNightmareRules.Hp(6500,HallNightmareRules.Find("luca-gauntlet")!):6500);waiting=true;return true;
        }
        if(index<0||index>=roster.Length)return false;int id=roster[index].Id;
        if(teams.Any(t=>t.Contains(id))||id==1&&group!=2||group==2&&teams[2].Count==0&&id!=1){CastleGuildRuntime.Status="Each character serves once. Reserve Riou for the final group.";return false;}
        if(teams[group].Count==6){CastleGuildRuntime.Status="That group is complete.";return false;}
        teams[group].Add(id);
        if(teams[group].Count==6&&group<2){group++;if(group==2)teams[2].Add(1);}
        return false;
    }
    internal static GuildMenuView View(int focus)
    {
        string Name(int id)=>roster.FirstOrDefault(c=>c.Id==id)?.Name??"?";
        var rows=roster.Select(c=>new GuildMenuRow(c.Name,$"Lv {c.Level} · "+(teams.Any(t=>t.Contains(c.Id))?"Assigned":"Available")))
            .Append(new GuildMenuRow("Undo last choice","Return a character to the roster"))
            .Append(new GuildMenuRow("Begin Gauntlet","Three native battles; no party may be reused")).ToArray();
        return GuildRebornScreen.Paginate(new("LUCA · THREE PARTIES",rows,Math.Clamp(focus,0,rows.Length-1),$"Choose group {group+1} · {teams[group].Count}/6",nightmare?"Nightmare Gauntlet":"Original Gauntlet",
            string.Join("\n",teams.Select((t,i)=>$"Group {i+1}: "+string.Join(", ",t.Select(Name)))),"",
            "Riou serves in group three. Six healthy single-slot characters per group. Front row: first three choices.",CastleGuildRuntime.Status,"Choose","Cancel preparation",false));
    }
    internal static void Tick()
    {
        if(!running||!waiting||game==null)return;
        if(epoch!=NativeWarBoundary.Epoch||Framework.Singleton<GAME_WORK>.instance?.Pointer!=game.Pointer)throw new InvalidOperationException("Gauntlet campaign owner changed.");
        // EVENTCON can already report idle while MACHICON is still in its return/fade steps.
        // Use the same native step boundary as RequestCore before preparing or binding the next team.
        if(!GuildGauntletInput.CanHold(running&&waiting,CastleGuildRuntime.Idle,CastleGuildRuntime.ReleaseFence,
            CastleGuildRuntime.Owner?._mstep??-1,CastleGuildRuntime.Field?.mstep??-1)||CastleGuildRuntime.Modal||!HallFirstContact.Available)return;
        var selected=teams[rules!.Stage].ToArray();
        if(equipmentSource==null){equipmentSource=Addressables.LoadAssetAsync<IMPORT_GAME_DATA>((Il2CppSystem.String)"gsd2-classic-gamedata-gamedata");return;}
        if(!equipmentSource.IsDone)return;
        if(equipmentSource.Status!=AsyncOperationStatus.Succeeded)throw new InvalidOperationException("Original equipment definitions failed to load.");
        if(lease==null)
        {
            if(!selected.All(HallNativeRoster.Read(game).Select(c=>(int)c.Id).ToHashSet().Contains))
                throw new InvalidOperationException("Gauntlet party became unavailable before its stage.");
            lease=new HallPartyLease(game,selected);restored=false;
        }
        if(!lease.Ready)return;
        equipment??=new HallPartyEquipment(game,equipmentSource.Result!.ovit_data,selected);
        lease.Bind();
        if(!HallFirstContact.RequestStage(Stages[rules.Stage],nightmare)){lease.Restore();lease.ReleaseGraphics();lease=null;equipment?.Restore();equipment=null;Addressables.Release(equipmentSource!);equipmentSource=null;throw new InvalidOperationException("Gauntlet stage request rejected.");}
        waiting=false;nativeHandoff=false;Plugin.Logger.LogInfo($"Luca gauntlet: stage {rules.Stage+1} requested; six distinct native player IDs, private carry {rules.CarryHp}.");
    }
    internal static void VerifyActors(BATTLE_WORK work)
    {
        if(!running||rules==null)return;
        var actual=work.pm_data.Where(p=>p!=null&&p.IsPlayer).Select(p=>(int)p.chara_id).ToArray();
        if(!actual.SequenceEqual(teams[rules.Stage]))throw new InvalidOperationException("Native battle did not bind the six selected player IDs.");
        foreach(var player in work.pm_data.Where(p=>p!=null&&p.IsPlayer))
        {
            if(OldSrcBase.CANOS(player.party_player_no)!=player.chara_id||G2_SYS.G2_gra_dat(player.party_player_no)?.Pointer!=HallPartyLease.Cached(player.chara_id)?.Pointer)
                throw new InvalidOperationException("Native actor slot, equipment and character graphics disagree.");
            var native=player.Cast<BattlePlayerCharacter>();
            for(int attribute=0;attribute<=8;attribute++)native.G2_def_atrb(attribute);
        }
        Plugin.Logger.LogInfo($"Luca gauntlet: native equipment attributes and character graphics verified for stage {rules.Stage+1}.");
        Plugin.Logger.LogInfo($"Luca gauntlet: native player binding verified for stage {rules.Stage+1}; IDs="+string.Join(",",actual));
    }
    internal static void CommandCompleted(BATTLE_WORK work,int result)
    {
        if(!running||rules==null||rules.Stage>=2||result!=1||HallFirstContact.GameForOwnedLucaStage(work)==null)return;
        if(!work.pm_data.Any(p=>p!=null&&p.IsPlayer&&!p.IsDead))return;
        nativeHandoff=true;
        Plugin.Logger.LogInfo($"Luca gauntlet: original stage {rules.Stage+1} command requested its normal handoff.");
    }
    internal static bool Outcome(BATTLE_WORK work,bool won)
    {
        if(!running||rules==null||rules.AwaitingTeardown||rules.Failed)return false;
        var actor=work.pm_data?.FirstOrDefault(p=>p!=null&&!p.IsPlayer&&(p.monster_data?.id is "boss_luc1_02" or "boss_luc2_02" or "boss_luc3_00"));
        bool handoff=!won&&nativeHandoff&&work.pm_data?.Any(p=>p!=null&&p.IsPlayer&&!p.IsDead)==true;
        if(actor==null||!rules.NativeOutcome(Stages[rules.Stage].TableId,won,actor.hp,handoff))throw new InvalidOperationException("Native Luca outcome identity failed.");
        return handoff&&!rules.Failed;
    }
    internal static void RestoreParty(){if(!running||lease==null||restored)return;lease.Restore();equipment?.Restore();restored=true;}
    internal static void Disposed(string source,bool won,bool safe)
    {
        if(!running||rules==null)return;RestoreParty();lease?.ReleaseGraphics();lease=null;equipment=null;if(equipmentSource!=null){Addressables.Release(equipmentSource);equipmentSource=null;}
        if(!won||!safe||!rules.NativeDisposed(source,true,true)){game=null;running=false;CastleGuildRuntime.TrainingFeedback("Gauntlet ended. Original party restored.");return;}
        if(rules.Completed){game=null;running=false;CastleGuildRuntime.TrainingFeedback("Luca Blight Gauntlet complete. Original party restored.");return;}
        waiting=true;
    }
}
