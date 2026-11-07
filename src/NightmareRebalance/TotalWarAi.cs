using HarmonyLib;
namespace NightmareRebalance;

internal sealed record CandidateEvidence(long Pointer,int Action,int X,int Y,int Target,int Before,int After);
internal static class TotalWarAi
{
    internal static int Score(int force,int slot,TACTICS_RESULT_TYPE candidate,int original,int baseline,WarAiRuntime.Scope scope)
    {
        int war=war_h.g_w_war_no;
        if(!TotalWarRuntime.AiAllowed(war)||candidate.act>2)return original;
        var units=w_unit.unit;if(units==null||units.Length!=2||units[0]?.Length!=15||units[1]?.Length!=15)return original;
        var actor=units[1][slot];if(actor==null||actor.flag!=1)return original;
        int name=w_chara.chara(w_unit.GetUnitChara(1,slot,0)).name;
        int branch=w_unit.GetUnitForceType(1,slot);string label=branch==0?"Infantry":branch==1?"Archer":branch==2?"Magicians":"Unknown";
        var p=TotalWarRuntime.Rules.AiProfiles.FirstOrDefault(p=>p.NameId==name&&p.Branch==label)??TotalWarRuntime.Rules.AiProfiles.FirstOrDefault(p=>p.NameId==name&&p.Branch=="Any");
        if(p==null)return original;
        List<AiUnit> Read(int side)
        {
            var list=new List<AiUnit>();for(int i=0;i<15;i++){var u=units[side][i];if(u is {flag:1})list.Add(new(i,u.x,u.y,u.damage,w_unit.GetUnitForceType(side,i)));}return list;
        }
        var friends=Read(1);var enemies=Read(0);
        if(enemies.Count==0||friends.Count==0)return original;
        var command=units[0][0];var enemyCommand=units[1][0];
        (int x,int y) goal=war==2?(4,7):war==6?(3,2):war==13?(5,8):(command.x,command.y);
        // Headquarters uses the audited original formation, not the commander's
        // current position (which would make every move look like abandonment).
        (int x,int y) home=war==2?(9,2):war==3?(5,2):war==6?(12,3):(12,5);
        int targetDamage=0,chance=100,utility=0;
        if(candidate.act==0)
        {
            if(candidate.target>=15||units[0][candidate.target] is not {flag:1} target)return original;
            targetDamage=target.damage;chance=w_kekka.CalcAttackHitRate(1,(byte)slot,0,candidate.target);
        }
        if(candidate.act==2)
        {
            if(candidate.tokusyu_chara>=3||candidate.tokusyu_nou>=3||w_unit.GetUnitTokusyuKaisu(1,slot,candidate.tokusyu_chara,candidate.tokusyu_nou)<=0)return original;
            utility=w_tach.HyoukaTokusyu(1,slot,candidate);
        }
        var c=new AiCandidate(candidate.act,candidate.tx,candidate.ty,candidate.target,targetDamage,chance,utility);
        int adjusted=TotalWarRules.Score(original,p,new(slot,actor.x,actor.y,actor.damage,branch),c,goal.x,goal.y,home.x,home.y,friends,enemies);
        if(scope.Candidates.Count<2048)scope.Candidates.Add(new(candidate.Pointer.ToInt64(),candidate.act,candidate.tx,candidate.ty,candidate.target,baseline,adjusted));else scope.Overflow=true;
        if(adjusted!=original&&!TotalWarRuntime.Probe)
        {
            TotalWarRuntime.AiChanges++;
            if(TotalWarRuntime.AiTrace.Count<32)TotalWarRuntime.AiTrace.Add(new{Turn=war_sys.turn,Unit=slot,NameId=name,p.Style,Action=c.Action,X=c.X,Y=c.Y,Target=c.Target,Before=original,After=adjusted});
        }
        return adjusted;
    }
    internal static void Selected(WarAiRuntime.Scope? scope)
    {
        if(scope==null||scope.Overflow||TotalWarRuntime.Probe||scope.Candidates.Count==0||w_tact.selected_result==null)return;
        var chosen=scope.Candidates.FirstOrDefault(c=>c.Pointer==w_tact.selected_result.Pointer.ToInt64());if(chosen==null)return;
        int best=scope.Candidates.Max(c=>c.Before);
        // Strict difference from every best original score: ties are not evidence
        // that a different command would have been chosen.
        if(chosen.Before>=best)return;
        TotalWarRuntime.SelectedChanges++;
        if(TotalWarRuntime.AiTrace.Count<32)TotalWarRuntime.AiTrace.Add(new{Turn=war_sys.turn,SelectedDecisionChanged=true,Unit=scope.Unit,Chosen=chosen,BestOriginalScore=best});
        Plugin.Debug($"Native war {war_h.g_w_war_no} enemy {scope.Unit}: selected candidate below original best after tactical scoring.");
    }
}
