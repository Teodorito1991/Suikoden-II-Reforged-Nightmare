# Luca Blight tactical dispatch audit

Development build `1.22.0-luca-dispatch-audit.1` with SHA256 `C3360B8BFB82006E84DD4434DA18BB1E30709978449930CE022591E328A3E8ED`.

## Where the engine makes the choice

On the supported Windows remaster, the monster branch of `BattleManager.BattleMakeData` calls `BattleMonsterCharacter.monster_attack_sub`. The latter checks a separate command path before reaching its direct call to `SelectAttackMode`. It writes a nonnegative result to `c_mode`, then resolves the move's calculation overlay or calls `AttackDataMakeNormal`. That calculation can reject a choice. The surrounding loop retries up to ten times and then checks whether the basic move is available. An invocation of the selector therefore cannot stand in for a turn or a committed attack.

All three weighted solo-Luca moves have calculation overlay index zero. The physical move uses `MonsterCalcAttack` and its individual target selector. The two special moves use `MonsterCalcTokushu`, with `MonsterFindTateMokuhyo` for the 0x400 shape and `MonsterFindYokoMokuhyo` for the 0x800 shape. Their separate display indices remain original. The attack builder consumes use counts and produces reaction data downstream of selection. Any future tactic must preserve the engine's actual per-shape targeting and cannot count retries as additional attacks.

## Native evidence

The isolated startup regression loads the original solo Luca record and runs each of the two area selectors against a six-slot scratch party. It verifies count, buffer ownership, distinct slots and native eligibility. Both passes returned slots [1,4] for the column and [0,1,2] for the row with the full fixture. Both returned only slot [1] after alternating slots were marked defeated. These slot sets describe the supplied fixture, not a general targeting guarantee.

The generated interop wrapper initially reported the native count with a stale zero-filled managed buffer. The final fixture uses the original x64 by-reference array signature after the plugin's exact supported-build check. The direct calls are restricted to the existing explicit unloaded audit. The two final processes each ran 32 area calls and retained the 144 v1.21 paired selector checks, all ten boss policies and the full production regression. Managed tests passed. No campaign was loaded. The installed DLL, original saves and configurations matched their pre-test hashes.

## Gate for tactical AI

Live Luca tactics remain unavailable and there is no new configuration key. The audit locates the discretionary selector but does not exercise its full attack builder with the actual solo encounter's initialized state. The missing evidence is whether an alternative legal mode reaches action commitment without changing retry or fallback behavior unexpectedly. A guarded next test needs initialized scratch action and reaction buffers and a comparison through the original monster dispatcher, including no-target, disabled-attack and retry cases. Campaign callbacks and the HP handoff remain outside that test. The v1.21 handoff audit remains the reference for those boundaries.
