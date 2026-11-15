namespace NightmareRebalance;

internal static class TeamworkNativeProbe
{
    static readonly List<object> rows = new();
    static readonly HashSet<string> covered = new();
    static int actionChanges, targetChanges;
    static readonly List<object> guardFixtures = new();
    static void Check(bool value, string label)
    {
        if (!value) throw new InvalidOperationException("Tactical teamwork: " + label);
    }

    internal static void Compare(BattleManager manager, BATTLE_WORK w, BattleState state, string id, bool expected)
    {
        TeamworkRuntime.Ready(state, w);
        Check((state.Teamwork?.Id == id) == expected, "Exact formation admission " + id);
        if (state.Teamwork is not {} binding) return;

        var cfg = CombatReforgedRuntime.Config;
        bool teamwork = cfg.EnableTeamwork, guard = cfg.EnableArcaneGuard, ai = cfg.EnableEnemyAi;
        var rng = UnityEngine.Random.state;
        var players = w.pm_data.Where(u => u != null && u.IsPlayer).ToArray();
        var actors = w.pm_data.Where(u => u != null && !u.IsPlayer).Select(u => u.Cast<BattleMonsterCharacter>()).ToArray();
        var hp = actors.Select(u => u.hp).ToArray();
        var originalUses = actors.Select(u => w.pm_attack_data[u.chara_no].curent_use_cnt.ToArray()).ToArray();
        var examples = new List<object>();
        int modes = 0, targets = 0, paired = 0;
        try
        {
            cfg.EnableEnemyAi = true;
            Check(TeamworkRuntime.Bound(w) == binding, "Same native work/party/roster");
            var member = actors[0];
            w.pm_data[member.chara_no] = null;
            Check(TeamworkRuntime.Bound(w) == null && state.Teamwork == null, "Roster replacement invalidates owner");
            w.pm_data[member.chara_no] = member;
            TeamworkRuntime.Ready(state,w);
            binding = state.Teamwork ?? throw new InvalidOperationException("Restored probe roster failed admission");
            var sample = new CombatAction(0,2,20,false,true,10,33,1,true);
            binding.Selected(actors[0].chara_no,sample);
            Check(binding.AreaReserved(actors[1].chara_no,10),"Area selection intent is visible to another unit");
            binding.Phase(w.battle_phase_no + 1);
            Check(!binding.AreaReserved(actors[1].chara_no,10),"Phase change discards selection intent");

            for (int scenario = 0; scenario < 5; scenario++)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    var player = players[i];
                    player.hp = (short)(scenario >= 1 && i < 2 ? 90 + i * 45 : 600);
                    player.status = scenario == 2 && i % 2 == 0
                        ? b_game_h.STATUS_SLEEP | b_game_h.STATUS_FUSEN : 0;
                    player.calc_para[1] = i % 2 == 0 ? 115 : 55;
                    player.calc_para[2] = i % 2 == 0 ? 120 : 42;
                    player.calc_para[3] = i % 2 == 0 ? 42 : 120;
                }
                int dead = Array.FindIndex(actors, a => a.monster_data.attack_data.Any(x =>
                    x != null && x.attack_type == 2 && x.attack_prob > 0 && (x.attack_zokusei & 0x10) == 0));
                for (int i = 0; i < actors.Length; i++)
                {
                    var actor = actors[i];
                    actor.hp = scenario == 3 && i == dead ? (short)0 : scenario >= 3 ? (short)Math.Max(1, hp[i] / 4) : hp[i];
                    var uses = w.pm_attack_data[actor.chara_no].curent_use_cnt;
                    for (int m = 0; m < 4; m++)
                    {
                        int limit = actor.monster_data.attack_data[m].kaisu;
                        uses[m] = limit > 0 ? (byte)(scenario == 4 ? 0 : limit) : byte.MaxValue;
                    }
                }
                for (int seed = 0; seed < 4; seed++)
                {
                    (int Mode, int Target, UnityEngine.Random.State ModeRng, UnityEngine.Random.State TargetRng)[] Run(bool enabled)
                    {
                        cfg.EnableTeamwork = enabled;
                        binding.ResetChoices();
                        CombatReforgedRuntime.Previous.Clear();
                        CombatReforgedRuntime.Focus.Clear();
                        CombatMasteryRuntime.Recovery.Clear();
                        var decisions = new List<(int, int, UnityEngine.Random.State, UnityEngine.Random.State)>();
                        for (int i = 0; i < actors.Length; i++)
                        {
                            var actor = actors[i];
                            if (actor.hp == 0) { decisions.Add((-1,-1,default,default)); continue; }
                            UnityEngine.Random.InitState(1900 + seed * 16 + i);
                            int mode = actor.SelectAttackMode();
                            var modeRng = UnityEngine.Random.state;
                            Check(mode < 0 || mode < 4 && actor.monster_data.attack_data[mode].attack_prob > 0 &&
                                actor.CheckMonsterAttackCnt(mode) && actor.AttackModeKyoriCheck(mode), "Native legal mode " + id);
                            int target = -1;
                            bool legal = true;
                            if (mode >= 0)
                            {
                                actor.c_mode = (sbyte)mode;
                                UnityEngine.Random.InitState(2900 + seed * 16 + i);
                                CombatReforgedRuntime.LastNativeTargets = Array.Empty<int>();
                                target = manager.parties[1].MonsterFindKojinMokuhyo(actor.chara_no);
                                legal = target < 0 || CombatReforgedRuntime.LastNativeTargets.Contains(target);
                            }
                            Check(legal, "Target remains in the original selector candidate set " + id);
                            decisions.Add((mode,target,modeRng,UnityEngine.Random.state));
                        }
                        return decisions.ToArray();
                    }

                    var before = Run(false);
                    var after = Run(true);
                    for (int i = 0; i < before.Length; i++)
                    {
                        if (before[i].Mode < 0 && after[i].Mode < 0) continue;
                        paired++;
                        Check(before[i].ModeRng.Equals(after[i].ModeRng), "Action selector RNG differs " + id);
                        if (before[i].Mode == after[i].Mode)
                            Check(before[i].TargetRng.Equals(after[i].TargetRng), "Target sampler RNG differs " + id);
                        bool mode = before[i].Mode != after[i].Mode;
                        bool target = before[i].Mode == after[i].Mode && before[i].Target != after[i].Target;
                        if (mode) modes++;
                        if (target) targets++;
                        if ((mode || target) && examples.Count < 5)
                            examples.Add(new{Scenario=scenario,Enemy=actors[i].monster_data.id,BeforeMode=before[i].Mode,
                                AfterMode=after[i].Mode,BeforeTarget=before[i].Target,AfterTarget=after[i].Target});
                    }
                }
            }

            if (TacticalTeamwork.Formations.ContainsKey(id))
                VerifyGuard(id, players[0], actors);
            state.Teamwork = binding;
            if (HuntRuntime.Active == null)
            {
                Combat.NonVictory(manager);
                Check(TeamworkRuntime.Bound(w) == null && state.Teamwork == null, "Non-victory clears encounter policy");
            }

            Check(modes+targets>0,"Authored profile yielded no tactical difference " + id);
            actionChanges += modes;
            targetChanges += targets;
            rows.Add(new { Id=id, Style=binding.Style.ToString(), Paired=paired, ActionChanges=modes, TargetChanges=targets, Examples=examples });
            covered.Add(id);
        }
        finally
        {
            cfg.EnableTeamwork=teamwork;
            cfg.EnableArcaneGuard=guard;
            cfg.EnableEnemyAi=ai;
            UnityEngine.Random.state=rng;
            CombatReforgedRuntime.Previous.Clear();
            CombatReforgedRuntime.Focus.Clear();
            CombatMasteryRuntime.Recovery.Clear();
            for (int i = 0; i < actors.Length; i++)
            {
                actors[i].hp=hp[i];
                var uses=w.pm_attack_data[actors[i].chara_no].curent_use_cnt;
                for (int m=0;m<4;m++) uses[m]=originalUses[i][m];
            }
            foreach(var player in players)
            {
                player.hp=600;
                player.status=0;
            }
        }
    }

    static void VerifyGuard(string id, PM_DATA player, BattleMonsterCharacter[] actors)
    {
        var cfg = CombatReforgedRuntime.Config;
        foreach (var actor in actors)
        {
            var attacks = actor.monster_data.attack_data;
            int mode = Array.FindIndex(attacks.ToArray(), a => a != null && a.attack_type == 2 &&
                a.attack_prob > 0 && a.attack > 0 && (a.attack_zokusei & 0x10) == 0);
            if (mode < 0) continue;

            int Damage(bool enabled, uint status, int move)
            {
                cfg.EnableArcaneGuard=enabled;
                player.status=status;
                UnityEngine.Random.InitState(9402);
                return actor.CalcMonsterDamage(player.chara_no,move);
            }

            int open=Damage(false,0,mode);
            if (open<20) continue;
            int native=Damage(false,b_game_h.STATUS_GUARD,mode);
            int guarded=Damage(true,b_game_h.STATUS_GUARD,mode);
            Check(native<open && guarded==TacticalTeamwork.ArcaneGuard(native), "Native Guard plus bounded magical protection");
            Check(Damage(true,b_game_h.STATUS_GUARD,mode)==guarded && Damage(true,0,mode)==open,
                "Repeated calculations do not compound, and unguarded damage is unchanged");
            int physical=Array.FindIndex(attacks.ToArray(),a=>a!=null&&a.attack_prob>0&&a.attack>0&&(a.attack_zokusei&0x10)!=0);
            if(physical>=0)
                Check(Damage(true,b_game_h.STATUS_GUARD,physical)==Damage(false,b_game_h.STATUS_GUARD,physical),"Physical defend unchanged");
            guardFixtures.Add(new{Id=id,Enemy=actor.monster_data.id,Open=open,NativeGuard=native,TacticalGuard=guarded});
            return;
        }
    }

    internal static void Finish()
    {
        Check(covered.SetEquals(TacticalTeamwork.Formations.Keys.Concat(TacticalTeamwork.Hunts.Keys)), "All authored profiles exercised");
        Check(actionChanges > 0 && targetChanges > 0 && guardFixtures.Count > 0, "Observed native tactical differences plus arcane guard");
    }

    internal static object Snapshot() => new { Covered=covered.OrderBy(x=>x).ToArray(), actionChanges, targetChanges, guardFixtures, Rows=rows };
}
