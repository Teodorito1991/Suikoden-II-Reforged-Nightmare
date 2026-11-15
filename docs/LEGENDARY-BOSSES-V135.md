# v1.35 Legendary Bosses

Development build `1.35.0-legendary-bosses.1`

DLL SHA256 `89966D6AEC494F674794741B01A8B651044673B320F8DE1D29DAA1BA440EAF24`

## What changes on Normal

| Original encounter | Policy |
|---|---|
| Mist Shade (`boss_ki1`) | Its initial form keeps the original behavior. The transformed form favors its stronger two-use attack in pressure windows against a healthy group. It otherwise prefers the ordinary spell. |
| Gorudo (`boss_gor`) | Gorudo keeps physical pressure as his fallback, uses spells against sufficiently large groups with stronger physical than magical defenses, and favors his stronger limited spell in pressure windows. The five escorts retain their existing behavior. |
| Worm and Moth (`boss_imo2`) | The Worm favors its physical column attack, with the spell reserved for a healthy and physically defended group. The Moth can press weaker magic defense or choose status pressure when three or more targets lack that status. Otherwise it favors its physical attack. |
| Later Lucia (`boss_lus3`, `boss_lus4`, `boss_lus5`) | All three records prefer spending their one-use group spell against a healthy, physically defended group during a pressure window. The original command, forced-move slots and endings are retained. |

The main pressure window checks the original round counter and the actor's
first action. If a preferred attack is ineligible, the original selector's
result stands. The engine continues to calculate targets, spend uses, handle
retries, construct actions and process victory or defeat. No stats, moves,
targeting rules, animations, rewards, or callbacks are added or overwritten.
See [configuration](../CONFIGURATION.md) for the four independent family switches.

## Admission and scripted behavior

The policy joins the existing `SelectAttackMode` postfix after the original
selector runs. Its separate `BattleMonsterCharacter.monster_attack_sub`
prefix captures command and action ordinal before the engine can change them.
Only entry command 0 is eligible. Command 11 and unknown commands stay excluded
even if the native dispatcher falls through to selection. The hook skips
nested dispatches and restores its parent scope in a finalizer. It considers
only the first selector result in a dispatch, and preserves native negative
results and later retries.

Admission requires exact encounter, roster, source signatures, Normal-mode
copy provenance and the resolvable original callback methods. Selection checks
the active battle generation, manager and work ownership, roster actor and
battle-local phase template again. Mist Shade and Worm/Moth callbacks can
install intermediate delegates during transformation. The guard accepts the
original phase-end method when reinstated and the audited steady-state main
method from the same original declaring type. Other callback changes leave
selection native. Phase identification comes from the engine's current actor
template and does not depend on a guessed HP threshold.

The separately scripted Moth record `boss_imo3` contains original command
entries, and the multi-part final encounter `boss_las` has opening, phase,
command, main and ending callbacks. Both remain outside this policy. Earlier
Neclord keeps its original scripted or invulnerable behavior. Earlier Luca
encounters also remain untouched.

## Evidence for this build

| Check | Result |
|---|---|
| Managed regression and plugin compile | Passed. The managed cases cover opt-outs, phase identity, opening/recovery windows, status usefulness and defense tradeoffs. |
| Original loaded assets and selectors | Six production policies admitted matched native tables and provided 233 changed decisions in paired fixtures. Changes by record were Mist Shade 26, Gorudo 45, Worm/Moth 72 and later Lucia 30 each. Actor phases were supplied by the fixtures. Native selectors ran in scoped scratch state and retained range, use and RNG behavior. |
| Solo Luca `boss_luc3_00` | The original dispatcher ran in disposable scratch state. The scoped change replaced a column choice with a row attack, and the engine built the action and three player reactions. The fixture kept the counter, use budget, ending binding and scratch HP carryover unchanged. Component cases exclude nested calls and simulate command-11 fallthrough. |
| Copied game launcher | Exact-build graphics startup reached the original launcher and quit normally. The known TextMeshPro exception and Unity allocation warning recurred during shutdown. The original Steam installation was outside the deployment. |

Full rendered battles, actual phase transitions, story cinematics and campaign
balance have yet to be observed on this DLL. The Luca experiment establishes
action construction in owned scratch work, with no evidence from a genuine
Luca battle scene. Luca admission is disabled in the campaign and there is no
player-facing switch for it.
