# Tactical Revolution — v1.39

The v1.39 candidate adds formation-level decisions to 12 existing remixes and
five Hunt contracts on Normal. It uses the original RPG attack selector and
target candidates, the existing battle-local combat state, and the original
Defend command. Hunt rewards, native action playback and story wars use their
existing paths.

## What the formations do

| Behavior | Audited ordinary parties | Player decisions |
|---|---|---|
| Pack | `wa_00_02`, `vf06_00_04` | A pack prefers a plausible finishing hit, then spreads pressure when allies already chose that target. Heal exposed characters before their HP falls into finishing range. |
| Disruption | `wa_00_04`, `vb07_00_06`, `vc12_00_02` (Sindar Ruins), `ve03_00_05` (Greenhill area), `ve08_00_03`, `vh04_00_05` | Active native statuses change the value of a target. A second planned copy of the same broad status receives a penalty. Physical follow-ups favor leaving a sleeping target alone unless the hit looks sufficient to finish it. |
| Caster Escort | `vd06_00_02` and `vd06_00_04` (Cave of the Wind), `ve08_00_06` | Allies with physical attacks pressure powerful casters while a living ally can still cast. Removing the enemy caster changes how its companions pick targets. This is offensive pressure using original attacks. |
| Mixed | `vb21_00_03` (Trial Road) | Enemies with both attack types can favor the weaker relevant defense. A party with both a durable front line and magical protection presents fewer easy targets. |

Bramble Road and The Matilda Choir favor useful status applications. Twin Head
Pack and The Undying Watch match physical or magical attacks against relative
defenses. The Tenzan Trial adds caster-escort pressure. Hunt admission, rewards,
reporting and the available moves still follow their existing definitions.

Decisions use current native HP, status, defensive attributes and move
availability. The only remembered choices are a bounded set of target and area
status intentions, one per participating actor and phase. A selected status is
never treated as a landed hit. Reinforcements that alter the roster invalidate
the binding. The state is discarded on a terminal signal or teardown and lives
solely in the current battle generation. No new RNG draws or native move-use
writes are introduced by the tactics. A restriction or immunity remains
authoritative in the original game.

## Defend

In the audited formations and Hunts above, Defend reduces qualifying magical
monster damage by a further 10%, rounded down after existing damage scaling.
The enhancement requires the engine's live Guard flag and applies only to direct
type-2 magical damage in the existing damage function. Physical damage, counters,
status application, resources and turn order retain their existing rules.

Example from the Cave of the Wind fixture, with the same damage roll: exposed
83, native Guard 55, enhanced Guard 50. In the Sindar fixture the corresponding
values were 66, 44 and 40. Recalculating the same attack does not compound the
reduction. The bonus consumes no extra action beyond choosing Defend and grants
no charge, item, currency or lasting buff.

`EnableTeamwork` and `EnableArcaneGuard` in `combat-reforged.json` are independent
switches, on by default. Older configurations can omit them. Explicit false
restores the previous behavior for that setting. Existing formation and combat AI
opt-outs still control their respective paths.

## Evidence and boundaries

- With only the new tactics switched off to reproduce v1.38 decisions, compared
  1,062 pairs from the original native selectors across all 17 formation/Hunt
  profiles. Saw 42 attack-choice changes and 342 target changes at the same
  selected attack. Every profile changed at least one decision. Checked the
  original positive-weight, range and use restrictions and original target
  candidate sets. Action-selector RNG matched across the pairs, and target RNG
  matched where the attack was held constant.
- In the Greenhill area fixture, a Holly Elf changed from its area attack to its
  single-target Balloon attack in some seeds. In Cave of the Wind, a physical
  escort redirected attacks toward the stronger caster while its ally was
  available. These are native selector results on fixture parties with mixed
  defense, wounds, existing status, removed support or exhausted limited moves.
- Ran the Release build, managed regression suite, native production audit,
  ten-Hunt native catalogue checks, RPG Evolution rune/Unite lookups, boss
  probes and seven-war tactic checks. The exact DLL opened the original
  launcher in the copied game. That run also rendered the existing Guild preview.
- Fixture party attributes and outcomes are controlled inputs. There were no
  rendered RPG battle playthroughs or earned campaign victories on this build.
  Campaign difficulty and any long-running status chains still need playthrough
  calibration. No native enemy healing option was identified in the selected
  rosters. The Winger Attack and Bandit Attack options already have original
  Poison and Knockdown behavior with displayed fixed chances; v1.39 keeps their
  native coefficients, chances and text synchronized by retaining those values.
  No new scripted boss behavior was added. Luca remains outside the production
  tactics scope.

Development DLL `1.39.0-tactical-revolution.1`

SHA256 `0AF9B6B9199607A39DE62D80DD3C11B527338442FC8DD79BBA9FA30296FB6004`
