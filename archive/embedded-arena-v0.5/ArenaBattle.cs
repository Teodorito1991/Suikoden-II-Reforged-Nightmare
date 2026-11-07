using System.Text.Json;

namespace NightmareRebalance;

internal readonly record struct ArenaPoint(int X,int Y);
internal enum ArenaPhase { Player,Enemy,Victory,Defeat }
internal sealed class ArenaType
{
    public string Id {get;set;}="";
    public string Name {get;set;}="";
    public string Icon {get;set;}="";
    public int Health {get;set;}
    public int Attack {get;set;}
    public int Defense {get;set;}
    public int Move {get;set;}
    public int MinRange {get;set;}
    public int Range {get;set;}
    public int RallyUses {get;set;}
    public int RallyHeal {get;set;}
}
internal sealed class ArenaSpawn
{
    public string Id {get;set;}="";
    public string Name {get;set;}="";
    public string Type {get;set;}="";
    public string Side {get;set;}="";
    public int X {get;set;}
    public int Y {get;set;}
}
internal sealed class ArenaRules
{
    public int HoldTurns {get;set;}
    public int CriticalX {get;set;}
    public int CriticalY {get;set;}
    public string PlayerCommander {get;set;}="";
    public string EnemyCommander {get;set;}="";
    public bool DefeatOnBreach {get;set;}
    public bool DefeatOnCommanderLoss {get;set;}
    public bool CommanderKillVictory {get;set;}
    public bool SurvivalVictory {get;set;}
    public int MinimumDamage {get;set;}
    public int BraceDefense {get;set;}
    public int ForestDefense {get;set;}
    public int FortDefense {get;set;}
    public int ForestMoveCost {get;set;}
    public int EnemyStepMilliseconds {get;set;}
    public bool ForestBlocksShots {get;set;}
    public bool UnitsBlockShots {get;set;}
}
internal sealed class ArenaScenario
{
    public int SchemaVersion {get;set;}
    public string Id {get;set;}="";
    public string Name {get;set;}="";
    public string Description {get;set;}="";
    public string[] Map {get;set;}=Array.Empty<string>();
    public ArenaType[] Types {get;set;}=Array.Empty<ArenaType>();
    public ArenaSpawn[] Units {get;set;}=Array.Empty<ArenaSpawn>();
    public ArenaRules Rules {get;set;}=new();
}
internal static class ArenaConfig
{
    internal static ArenaScenario Parse(string json)
    {
        if(json.Length>131072)throw new JsonException("Arena scenario exceeds 128 KiB.");
        using var doc=JsonDocument.Parse(json);
        OverhaulRules.Keys(doc.RootElement,"SchemaVersion","Id","Name","Description","Map","Types","Units","Rules");
        foreach(var t in doc.RootElement.GetProperty("Types").EnumerateArray())OverhaulRules.Keys(t,"Id","Name","Icon","Health","Attack","Defense","Move","MinRange","Range","RallyUses","RallyHeal");
        foreach(var u in doc.RootElement.GetProperty("Units").EnumerateArray())OverhaulRules.Keys(u,"Id","Name","Type","Side","X","Y");
        OverhaulRules.Keys(doc.RootElement.GetProperty("Rules"),"HoldTurns","CriticalX","CriticalY","PlayerCommander","EnemyCommander","DefeatOnBreach","DefeatOnCommanderLoss","CommanderKillVictory","SurvivalVictory","MinimumDamage","BraceDefense","ForestDefense","FortDefense","ForestMoveCost","EnemyStepMilliseconds","ForestBlocksShots","UnitsBlockShots");
        var s=ContentRules.Read<ArenaScenario>(json);
        bool Text(string? text,int max)=>!string.IsNullOrWhiteSpace(text)&&text.Length<=max;
        if(s.SchemaVersion!=1||!Text(s.Id,64)||!Text(s.Name,80)||!Text(s.Description,500)||s.Map==null||s.Map.Length is <5 or >9||s.Map[0]==null||s.Map[0].Length is <7 or >13)throw new JsonException("Invalid arena schema or map size (7..13 by 5..9).");
        int width=s.Map[0].Length;
        if(s.Map.Any(row=>row==null||row.Length!=width||row.Any(c=>!".FW=CR".Contains(c))))throw new JsonException("Invalid map rows/terrain. Use . F W = C R.");
        if(s.Types==null||s.Types.Length is <2 or >12||s.Units==null||s.Units.Length is <3 or >24||s.Rules==null)throw new JsonException("Invalid army/type count.");
        var typeIds=new HashSet<string>();
        foreach(var t in s.Types)
            if(t==null||!Text(t.Id,40)||!typeIds.Add(t.Id)||!Text(t.Name,60)||!Text(t.Icon,4)||t.Health is <1 or >40||t.Attack is <1 or >20||t.Defense is <0 or >15||t.Move is <1 or >5||t.MinRange<1||t.Range<t.MinRange||t.Range>6||t.RallyUses is <0 or >5||t.RallyHeal is <0 or >12||(t.RallyUses>0&&t.RallyHeal==0))throw new JsonException("Invalid unit type or combat bounds.");
        bool Cell(int x,int y)=>x>=0&&y>=0&&x<width&&y<s.Map.Length&&s.Map[y][x]!='W';
        var ids=new HashSet<string>();var positions=new HashSet<ArenaPoint>();
        foreach(var u in s.Units)
            if(u==null||!Text(u.Id,40)||!ids.Add(u.Id)||!Text(u.Name,60)||!typeIds.Contains(u.Type)||u.Side is not ("Player" or "Enemy")||!Cell(u.X,u.Y)||!positions.Add(new(u.X,u.Y)))throw new JsonException("Invalid/overlapping unit placement.");
        var r=s.Rules;
        if(r.HoldTurns is <1 or >30||!Cell(r.CriticalX,r.CriticalY)||r.MinimumDamage is <1 or >5||r.BraceDefense is <0 or >5||r.ForestDefense is <0 or >5||r.FortDefense is <0 or >5||r.ForestMoveCost is <1 or >4||r.EnemyStepMilliseconds is <100 or >3000||(!r.CommanderKillVictory&&!r.SurvivalVictory))throw new JsonException("Invalid objective/terrain/turn rules.");
        if(!s.Units.Any(u=>u.Id==r.PlayerCommander&&u.Side=="Player")||!s.Units.Any(u=>u.Id==r.EnemyCommander&&u.Side=="Enemy"))throw new JsonException("Commander IDs must refer to the correct army.");
        // Validate actual generated board connectivity, not just rectangular coordinates.
        var visited=new HashSet<ArenaPoint>{new(r.CriticalX,r.CriticalY)};var queue=new Queue<ArenaPoint>(visited);
        while(queue.TryDequeue(out var point))foreach(var next in ArenaBattle.Neighbors(point))if(Cell(next.X,next.Y)&&visited.Add(next))queue.Enqueue(next);
        if(s.Units.Any(u=>!visited.Contains(new(u.X,u.Y))))throw new JsonException("Every unit must have a terrain route to the crossing.");
        return s;
    }
}
internal sealed class ArenaUnit
{
    internal readonly string Id,Name,Side;
    internal readonly ArenaType Type;
    internal ArenaPoint Position;
    internal int Hp,RallyLeft;
    internal bool Moved,Acted,Braced;
    internal bool Alive=>Hp>0;
    internal ArenaUnit(ArenaSpawn s,ArenaType t){Id=s.Id;Name=s.Name;Side=s.Side;Type=t;Position=new(s.X,s.Y);Hp=t.Health;RallyLeft=t.RallyUses;}
}
internal sealed class ArenaBattle
{
    internal readonly ArenaScenario Scenario;
    internal readonly List<ArenaUnit> Units;
    internal ArenaPhase Phase {get;private set;}=ArenaPhase.Player;
    internal int Round {get;private set;}=1;
    internal int HeldRounds {get;private set;}
    internal string Reason {get;private set;}="Hold the crossing.";
    internal int Moves,Attacks,EnemyActions,Rallies;
    internal readonly List<string> Journal=new();
    internal bool Finished=>Phase is ArenaPhase.Victory or ArenaPhase.Defeat;
    internal int Width=>Scenario.Map[0].Length;
    internal int Height=>Scenario.Map.Length;
    internal ArenaBattle(ArenaScenario scenario)
    {
        // Complete mod-owned clone: never shares mutable setup data with replay/configuration.
        Scenario=ArenaConfig.Parse(JsonSerializer.Serialize(scenario));
        Units=Scenario.Units.Select(s=>new ArenaUnit(s,Scenario.Types.Single(t=>t.Id==s.Type))).ToList();
        Log("THE LAST BRIDGE - defend for "+Scenario.Rules.HoldTurns+" rounds or defeat the enemy commander.");
    }
    internal ArenaUnit? Unit(string? id)=>Units.FirstOrDefault(u=>u.Id==id);
    internal ArenaUnit? At(ArenaPoint p)=>Units.FirstOrDefault(u=>u.Alive&&u.Position==p);
    internal bool Inside(ArenaPoint p)=>p.X>=0&&p.Y>=0&&p.X<Width&&p.Y<Height;
    internal char Terrain(ArenaPoint p)=>Inside(p)?Scenario.Map[p.Y][p.X]:'W';
    internal bool Passable(ArenaPoint p)=>Inside(p)&&Terrain(p)!='W';
    internal static IEnumerable<ArenaPoint> Neighbors(ArenaPoint p){yield return new(p.X-1,p.Y);yield return new(p.X+1,p.Y);yield return new(p.X,p.Y-1);yield return new(p.X,p.Y+1);}
    internal static int Distance(ArenaPoint a,ArenaPoint b)=>Math.Abs(a.X-b.X)+Math.Abs(a.Y-b.Y);
    void Log(string text){if(Journal.Count>=64)Journal.RemoveAt(0);Journal.Add(text);}
    bool MayAct(ArenaUnit u)=>!Finished&&u.Alive&&!u.Acted&&u.Side==(Phase==ArenaPhase.Player?"Player":"Enemy");
    int MoveCost(ArenaPoint p)=>Terrain(p)=='F'?Scenario.Rules.ForestMoveCost:1;
    internal Dictionary<ArenaPoint,int> Reachable(string id)
    {
        var u=Unit(id);if(u==null||!MayAct(u)||u.Moved)return new();
        return Paths(u.Position,u.Type.Move,p=>At(p)==null||At(p)==u);
    }
    Dictionary<ArenaPoint,int> Paths(ArenaPoint start,int limit,Func<ArenaPoint,bool> allowed)
    {
        var costs=new Dictionary<ArenaPoint,int>{{start,0}};var queue=new PriorityQueue<ArenaPoint,int>();queue.Enqueue(start,0);
        while(queue.TryDequeue(out var p,out int cost))
        {
            if(costs[p]!=cost)continue;
            foreach(var n in Neighbors(p))
            {if(!Passable(n)||!allowed(n))continue;int next=cost+MoveCost(n);if(next<=limit&&(!costs.TryGetValue(n,out int old)||next<old)){costs[n]=next;queue.Enqueue(n,next);}}
        }
        return costs;
    }
    internal bool Move(string id,ArenaPoint destination)
    {
        var u=Unit(id);if(u==null||destination==u.Position||!Reachable(id).ContainsKey(destination)||At(destination)!=null)return false;
        u.Position=destination;u.Moved=true;Moves++;Log($"{u.Name} advances to {destination.X},{destination.Y}.");CheckOutcome();return true;
    }
    internal bool ClearShot(ArenaUnit attacker,ArenaPoint from,ArenaPoint to)
    {
        int x=from.X,y=from.Y,dx=Math.Abs(to.X-x),dy=-Math.Abs(to.Y-y),sx=x<to.X?1:-1,sy=y<to.Y?1:-1,err=dx+dy;
        while(x!=to.X||y!=to.Y)
        {
            int twice=2*err;if(twice>=dy){err+=dy;x+=sx;}if(twice<=dx){err+=dx;y+=sy;}
            var p=new ArenaPoint(x,y);if(p==to)break;
            if(Scenario.Rules.ForestBlocksShots&&Terrain(p)=='F')return false;
            if(Scenario.Rules.UnitsBlockShots&&At(p) is {} u&&u!=attacker)return false;
        }
        return true;
    }
    internal bool CanAttack(ArenaUnit a,ArenaUnit b,ArenaPoint? origin=null)
    {
        if(!MayAct(a)||!b.Alive||a.Side==b.Side)return false;
        var from=origin??a.Position;int distance=Distance(from,b.Position);
        return distance>=a.Type.MinRange&&distance<=a.Type.Range&&(a.Type.Range==1||ClearShot(a,from,b.Position));
    }
    internal int Damage(ArenaUnit attacker,ArenaUnit target)
    {
        int cover=Terrain(target.Position)=='F'?Scenario.Rules.ForestDefense:Terrain(target.Position)=='C'?Scenario.Rules.FortDefense:0;
        return Math.Max(Scenario.Rules.MinimumDamage,attacker.Type.Attack-target.Type.Defense-cover-(target.Braced?Scenario.Rules.BraceDefense:0));
    }
    internal bool Attack(string attacker,string target)
    {
        var a=Unit(attacker);var b=Unit(target);if(a==null||b==null||!CanAttack(a,b))return false;
        int damage=Damage(a,b);b.Hp=Math.Max(0,b.Hp-damage);a.Moved=true;a.Acted=true;Attacks++;
        Log($"{a.Name} hits {b.Name}: {damage} damage"+(b.Alive?".":" - routed!"));CheckOutcome();return true;
    }
    internal bool Brace(string id)
    {var u=Unit(id);if(u==null||!MayAct(u))return false;u.Braced=true;u.Moved=true;u.Acted=true;Log(u.Name+" braces (+"+Scenario.Rules.BraceDefense+" defense).");return true;}
    internal bool Rally(string id)
    {
        var u=Unit(id);if(u==null||!MayAct(u)||u.RallyLeft<=0)return false;
        var allies=Units.Where(a=>a.Alive&&a.Side==u.Side&&a.Hp<a.Type.Health&&Distance(a.Position,u.Position)<=1).ToArray();if(allies.Length==0)return false;
        foreach(var a in allies)a.Hp=Math.Min(a.Type.Health,a.Hp+u.Type.RallyHeal);
        u.RallyLeft--;u.Moved=true;u.Acted=true;Rallies++;Log(u.Name+" rallies nearby allies (+"+u.Type.RallyHeal+" HP).");return true;
    }
    internal bool EndPlayerTurn()
    {
        if(Phase!=ArenaPhase.Player)return false;Phase=ArenaPhase.Enemy;
        foreach(var u in Units.Where(u=>u.Side=="Enemy")){u.Acted=false;u.Moved=false;u.Braced=false;}
        Log("Round "+Round+": Highland advances.");return true;
    }
    internal bool StepDefenderDemo()
    {
        if(Phase!=ArenaPhase.Player)return false;
        var u=Units.FirstOrDefault(u=>u.Side=="Player"&&u.Alive&&!u.Acted);
        if(u==null)return EndPlayerTurn();
        var target=Units.Where(t=>CanAttack(u,t)).OrderBy(t=>t.Hp).FirstOrDefault();
        if(target!=null)return Attack(u.Id,target.Id);
        return Rally(u.Id)||Brace(u.Id);
    }
    internal void StepEnemy()
    {
        if(Phase!=ArenaPhase.Enemy)return;
        var u=Units.FirstOrDefault(u=>u.Side=="Enemy"&&u.Alive&&!u.Acted);
        if(u==null){FinishRound();return;}
        EnemyActions++;
        var goal=new ArenaPoint(Scenario.Rules.CriticalX,Scenario.Rules.CriticalY);
        var distanceToGoal=Paths(goal,Width*Height*4,_=>true);
        (ArenaPoint Point,ArenaUnit? Target,double Score)? best=null;
        foreach(var p in Reachable(u.Id))
        {
            double baseScore=-(distanceToGoal.GetValueOrDefault(p.Key,10000)*5)-p.Value*0.1;
            if(p.Key==goal&&Scenario.Rules.DefeatOnBreach)baseScore+=100000;
            var targets=Units.Where(t=>CanAttack(u,t,p.Key)).ToArray();
            ArenaUnit? target=targets.OrderByDescending(t=>(Damage(u,t)>=t.Hp?1000:0)+Damage(u,t)*20+(t.Id==Scenario.Rules.PlayerCommander?30:0)).ThenBy(t=>t.Hp).ThenBy(t=>t.Id,StringComparer.Ordinal).FirstOrDefault();
            double score=baseScore+(target==null?0:10000+(Damage(u,target)>=target.Hp?1000:0)+Damage(u,target)*20);
            if(best==null||score>best.Value.Score)best=(p.Key,target,score);
        }
        if(best is {} plan)
        {if(plan.Point!=u.Position)Move(u.Id,plan.Point);if(!Finished&&plan.Target!=null)Attack(u.Id,plan.Target.Id);}
        if(!Finished&&!u.Acted)Brace(u.Id);
    }
    void FinishRound()
    {
        HeldRounds++;CheckOutcome();if(Finished)return;
        Round++;Phase=ArenaPhase.Player;
        foreach(var u in Units.Where(u=>u.Side=="Player")){u.Acted=false;u.Moved=false;u.Braced=false;}
        Log("Round "+Round+": issue your orders.");
    }
    internal void CheckOutcome()
    {
        if(Finished)return;var r=Scenario.Rules;string? defeat=null,victory=null;
        if(!Units.Any(u=>u.Side=="Player"&&u.Alive))defeat="The defending army was routed.";
        if(r.DefeatOnCommanderLoss&&Unit(r.PlayerCommander)?.Alive!=true)defeat="The defending commander fell.";
        if(r.DefeatOnBreach&&At(new(r.CriticalX,r.CriticalY))?.Side=="Enemy")defeat="Highland captured the crossing.";
        if(r.CommanderKillVictory&&Unit(r.EnemyCommander)?.Alive!=true)victory="The attacking commander was defeated.";
        if(r.SurvivalVictory&&HeldRounds>=r.HoldTurns)victory="The crossing held for "+r.HoldTurns+" rounds.";
        if(defeat!=null){Phase=ArenaPhase.Defeat;Reason=defeat;Log("DEFEAT: "+defeat);}
        else if(victory!=null){Phase=ArenaPhase.Victory;Reason=victory;Log("VICTORY: "+victory);}
    }
    internal object Snapshot()=>new {Scenario=Scenario.Id,Phase=Phase.ToString(),Round,HeldRounds,Reason,Moves,Attacks,EnemyActions,Rallies,Units=Units.Select(u=>new {u.Id,u.Name,u.Side,Type=u.Type.Id,X=u.Position.X,Y=u.Position.Y,u.Hp,MaxHp=u.Type.Health,u.Moved,u.Acted,u.Braced,u.RallyLeft}).ToArray(),Journal=Journal.ToArray()};
}
