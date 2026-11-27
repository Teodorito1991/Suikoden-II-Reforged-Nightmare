using Il2CppInterop.Runtime.InteropTypes.Arrays;
namespace NightmareRebalance;
internal sealed class NativeTacticsBoard
{
    static readonly (int dx,int dy)[] Directions={(1,0),(-1,0),(0,1),(0,-1)};
    internal readonly int[] Threat=new int[192],Immediate=new int[192],GoalCosts=new int[192];
    internal readonly List<TacticalUnit> Friends=new(),Enemies=new();
    internal TacticalUnit Actor;
    internal int GoalX,GoalY;
    internal static bool Cell(int x,int y)=>x is >=0 and <16&&y is >=0 and <12;
    internal static byte[] Reach(int side,int slot,Il2CppStructArray<byte>? scratch=null)
    {
        var u=w_unit.unit[side][slot];var mask=scratch??new Il2CppStructArray<byte>(192);
        w_hyoka.InitMovePossibility(mask);
        int move=w_unit.GetUnitMove(side,slot);if(move is <0 or >12)return new byte[192];
        w_hyoka.AssessMovePossibility(u.x,u.y,move,mask,side,slot);
        return mask.ToArray();
    }
    internal static int[] Costs(int side,int slot,int gx,int gy,int range,bool occupancy)
    {
        var distance=new int[192];Array.Fill(distance,10000);var done=new bool[192];
        for(int y=0;y<12;y++)for(int x=0;x<16;x++)
            if(w_data.CheckFieldOnStage(x,y)!=0&&TotalWarRules.Distance(x,y,gx,gy)<=range&&(!occupancy||w_unit.GetUnitNumber(x,y,side^1)==-1))distance[y*16+x]=0;
        for(int step=0;step<192;step++)
        {
            int at=-1;for(int i=0;i<192;i++)if(!done[i]&&(at<0||distance[i]<distance[at]))at=i;
            if(at<0||distance[at]>=10000)break;done[at]=true;
            int x=at%16,y=at/16;
            for(int d=0;d<Directions.Length;d++)
            {
                var (dx,dy)=Directions[d];
                int nx=x+dx,ny=y+dy;if(!Cell(nx,ny)||w_data.CheckFieldOnStage(nx,ny)==0||(occupancy&&w_unit.GetUnitNumber(nx,ny,side^1)!=-1))continue;
                int cost=w_data.GetMoveCost(nx,ny,x,y,side,slot);
                if(cost is <1 or >=255)continue;
                int n=ny*16+nx;distance[n]=Math.Min(distance[n],distance[at]+cost);
            }
        }
        return distance;
    }
    internal static NativeTacticsBoard Build(int slot)
    {
        var b=new NativeTacticsBoard();var units=w_unit.unit;
        for(int side=0;side<2;side++)for(int i=0;i<15;i++)
        {
            var u=units[side][i];if(u==null||u.flag!=1||!Cell(u.x,u.y)||u.chara==null||u.chara.Length!=3||u.chara[0]<0)continue;
            bool support=w_unit.GetUnitForceType(side,i)!=0||w_unit.GetUnitAbility(side,i,war_data_h.tagSPECIAL_ABILITY.SP_MEDICAL1)!=-1||w_unit.GetUnitAbility(side,i,war_data_h.tagSPECIAL_ABILITY.SP_MEDICAL2)!=-1||w_unit.GetUnitAbility(side,i,war_data_h.tagSPECIAL_ABILITY.SP_SHINING_SHIELD)!=-1;
            var view=new TacticalUnit(i,u.x,u.y,u.damage,w_unit.GetUnitHP(side,i),w_unit.GetUnitAttackRange(side,i),support);
            (side==1?b.Friends:b.Enemies).Add(view);if(side==1&&i==slot)b.Actor=view;
        }
        int war=war_h.g_w_war_no;var goal=b.Enemies.OrderBy(u=>u.Slot).FirstOrDefault();
        (b.GoalX,b.GoalY)=war==2?(4,7):war==6?(3,2):war==13?(5,8):(goal.X,goal.Y);
        var costs=Costs(1,slot,b.GoalX,b.GoalY,1,true);Array.Copy(costs,b.GoalCosts,192);
        var moveScratch=new Il2CppStructArray<byte>(192);
        var attackScratch=new Il2CppStructArray<byte>(192);
        var currentScratch=new Il2CppStructArray<byte>(192);
        foreach(var hostile in b.Enemies)
        {
            int range=hostile.Range;if(range is <1 or >8)continue;
            byte[] moves=Reach(0,hostile.Slot,moveScratch);
            for(int at=0;at<192;at++){attackScratch[at]=0;currentScratch[at]=0;}
            w_hyoka.AssessAttackPossibility(hostile.X,hostile.Y,range,currentScratch,0);
            for(int at=0;at<192;at++)
            {
                if(moves[at]!=1||w_unit.GetUnitNumber(at%16,at/16,0) is int occupied&&occupied!=-1&&occupied!=hostile.Slot)continue;
                w_hyoka.AssessAttackPossibility(at%16,at/16,range,attackScratch,0);
            }
            for(int at=0;at<192;at++){if(attackScratch[at]==1)b.Threat[at]++;if(currentScratch[at]==1)b.Immediate[at]++;}
        }
        return b;
    }
    internal TacticalFacts Facts(TACTICS_RESULT_TYPE c,int focus,bool repeated)
    {
        int at=c.ty*16+c.tx;
        int close=Friends.Count(u=>u.Slot!=Actor.Slot&&TotalWarRules.Distance(c.tx,c.ty,u.X,u.Y)<=1);
        int covered=0;
        foreach(var support in Friends.Where(u=>u.Support&&u.Slot!=Actor.Slot))
        {
            var closest=Enemies.OrderBy(u=>TotalWarRules.Distance(u.X,u.Y,support.X,support.Y)).FirstOrDefault();
            if(Enemies.Count!=0&&TotalWarRules.Distance(c.tx,c.ty,support.X,support.Y)==1&&TotalWarRules.Distance(c.tx,c.ty,closest.X,closest.Y)<TotalWarRules.Distance(support.X,support.Y,closest.X,closest.Y))covered++;
        }
        int exposure=0;if(c.act==0&&c.target==0)
        {var commander=Enemies.FirstOrDefault(u=>u.Slot==0);int guard=Enemies.Count(u=>u.Slot!=0&&TotalWarRules.Distance(u.X,u.Y,commander.X,commander.Y)<=1);exposure=Math.Max(0,3-guard);}
        int utility=c.act==2?w_tach.HyoukaTokusyu(1,Actor.Slot,c):0;
        var target=Enemies.FirstOrDefault(u=>u.Slot==c.target);
        return new(Threat[at],Immediate[at],close,covered,focus,exposure,utility,c.tokusyu_ct,Math.Max(0,(int)c.tokusyu_victim_ct),repeated,GoalCosts[at]<GoalCosts[Actor.Y*16+Actor.X],Math.Max(1,target.Hp-target.Damage));
    }

    internal WarManeuver Maneuver(TACTICS_RESULT_TYPE candidate)
    {
        int start = Actor.Y * 16 + Actor.X;
        int end = candidate.ty * 16 + candidate.tx;
        int Distance(int x, int y, TacticalUnit unit) => TotalWarRules.Distance(x, y, unit.X, unit.Y);
        int Screen(int x, int y) => Actor.Range > 1 || Enemies.Count == 0 ? 0 : Friends.Count(friend =>
        {
            if(friend.Slot == Actor.Slot || friend.Range <= 1 || Distance(x,y,friend) != 1 ||
                Immediate[friend.Y * 16 + friend.X] == 0 || Immediate[y * 16 + x] > 2)return false;
            var opponent=Enemies.MinBy(u=>Distance(friend.X,friend.Y,u));
            return Distance(x,y,opponent)<Distance(friend.X,friend.Y,opponent);
        });
        int Guard(int x, int y) => Actor.Slot != 0 && Friends.Any(friend => friend.Slot == 0 &&
            Distance(x, y, friend) == 1 && Threat[friend.Y * 16 + friend.X] > 0) ? 1 : 0;
        // Native movement costs include terrain and occupancy. An unreachable
        // objective supplies no progress preference.
        int approach = GoalCosts[start] < 10000 && GoalCosts[end] < 10000 ?
            GoalCosts[start] - GoalCosts[end] : 0;
        int inRange = Enemies.Count(enemy => Distance(candidate.tx, candidate.ty, enemy) <= Actor.Range);
        var target = Enemies.FirstOrDefault(enemy => enemy.Slot == candidate.target);
        // A conservative range estimate only influences scores. Counterattack
        // legality and terrain modifiers still belong to original resolution.
        bool counter = candidate.act == 0 && target.Hp > 0 &&
            Distance(candidate.tx, candidate.ty, target) <= target.Range;
        return new(Threat[start], Immediate[start], Screen(candidate.tx, candidate.ty) - Screen(Actor.X, Actor.Y),
            Guard(candidate.tx, candidate.ty) - Guard(Actor.X, Actor.Y), approach, inRange, counter);
    }
}
