# v1.20 Boss Reforged: Combat Quality

Development version: `1.20.0-boss-combat-quality.1`

DLL SHA256: `288B5D76311ADFE379D08174CE0C6CE0BE22A90FA294787F10A562F25593FA7B`

8 November 2026. Separate verified development candidate only; installed v1.13,
original saves/configurations and the private beta are preserved. No user session.

## Implemented improvements: Pest Rat and first Harpy

The new independently configurable EnableCombatQuality layer applies ONLY to
boss_rat and boss_hap1, under their existing exact source/callback/roster guards.
Normal remains the foundation. No HP, damage multiplier, attack power, ability,
attack count, reward or party progression value was changed.

Both bosses now strongly prefer a legal focused attack instead of a broad opening,
a broad recovery-window attack, or an area attack when the eligible party is badly
wounded. Broad pressure is discouraged when at least two eligible targets are at
35% HP or lower, or mean eligible HP is below60%; it remains available if no focused
alternative passes the original engine's checks. This is a finite preference, not
new invulnerability or a rewritten legality rule.

The existing native-counter recovery window now has a stronger preference for its
basic move. On an adaptive decision, a recovered/healthy party can invite the
existing pressure move again. This reads CURRENT HP/defence values, not queued
commands or healer identities. No actual healing cast detection was implemented.
It responds to the resulting party condition; it does not punish a named healer.

Pest Rat therefore alternates focused physical pressure and its original area
specials more deliberately. Harpy can use its original limited status strike in
focused windows while preserving the basic recovery choice and limited area move.
Status-bearing actions lose appeal when most native target masks already contain
the status. This does not infer equipment immunity or remove native status rules.

Single-target choices consider the selected move's defence type (the tested native
single-target moves here are physical),
existing phase focus and current HP. They reduce automatic wounded-target execution
and repeated focus when other native-returned candidates exist. Broad attacks keep
their original target anchor. Candidate discovery still uses the verified native
selector and restores RNG; no new RNG draws or illegal targets are introduced.

No new evaluation-count state machine exists. Cadence uses the existing native
battle_kaisuu counter; extra evaluations do not advance that counter. The prior
mod's base heuristics remain beneath this layer. Shared focus is a decision-phase
heuristic, not proof of coordinated animated enemy actions.

## Critical identity correction: Lucia is not Luca Blight

The older startup message and documentation incorrectly called boss_lus1/lus2
Luca. Their native name_id values833/834 resolve through original English aliases
to **Lucia**. The actual Luca Blight records are boss_luc1_02, boss_luc2_02 and
boss_luc3_00, with original name825/aliases and different party layouts (six, four
and one enemy slots respectively).

Those actual Luca paths are NOT among the ten admitted tactics policies. They were
not reassigned, enabled or altered here. Earlier Lucia selector evidence must never
be presented as Luca phase, party-transition or ending proof. The optional existing
EnableMajorBosses switch retains its identity/scope: two Lucia policies and final
Neclord, default OFF. It does not enable Luca Blight tactics.

[Sanitized original-data identity evidence](../publication/boss-identity-correction-v120.json).
No original game assets, bytecode or decompiled listings are distributed.

Neclord's existing policy is limited to boss_nec3. Earlier boss_nec1/nec2 encounters
remain excluded. The custom nec3 main and ending callbacks are left untouched and
were not invoked by these tests. No new claim of invulnerability, phase or final
story-return acceptance is made. Actual Luca multi-party/command/ending integration
remains a future audited task, not a v1.20 feature.

## All ten existing policies audited

All retain the existing wounded-HP pressure, recovery and adaptive mode preferences,
native positive-weight/use/range/magic legality and status/defence scoring. Only the
two rows marked refined receive the new layer. New target logic likewise excludes
the other eight. The old comparison disables BossTactics but still includes retained
base AI; it is not a complete vanilla boss fight comparison.

| Native table | Original resolved name | Pressure / recovery / adaptive slots | Native selector differences in retained audit | Scope |
|---|---|---|---|---|
| boss_ryo | Double Head | 1 / 2 / 0 | 120/144 | Existing policy retained |
| boss_rat | Pest Rat | 2 / 0 / 1 | 97/144 | Refined in v1.20 |
| boss_hap1 | Harpy | 3 / 0 / 1 | 72/144 | Refined in v1.20 |
| boss_sie1 | Sierra | 2 / 0 / 1 | 72/144 | Existing policy retained |
| boss_gwo1 | Golden Wolf | 2 / 1 / 0 | 120/144 | Existing policy retained |
| boss_gwo2 | Golden Wolf | 2 / 1 / 0 | 120/144 | Existing policy retained |
| boss_hap2 | Display name unresolved; source ID retained | 3 / 0 / 1 | 120/144 | Existing policy retained |
| boss_lus1 | Lucia | 3 / 0 / 2 | 78/144 | Existing policy retained |
| boss_lus2 | Lucia | 3 / 0 / 2 | 94/144 | Existing policy retained |
| boss_nec3 | Neclord | 3 / 0 / 1 | 94/144 | Existing policy retained |

All ten were admitted from original loaded records plus provenance-tracked native
ShallowCopy instances in disposable fixtures. Unexpected data mutation, unobserved
copies, roster/callback changes, foreign compatibility and per-boss opt-outs are
rejected. Source assets and callback bindings remain unchanged.

Boss tactics deliberately remain Normal-only. Hard-mode requests are rejected by
the existing tactics guard; the separate280-enemy Normal/Hard compatibility path
still passes. No broad acceptance of Hard-scaled boss records was introduced.

## Measured native decisions, not full fights

Two independent processes ran this exact DLL. The refined bosses were compared
against the retained v1.19 policy and, separately, the native selector with this
mod's enemy AI disabled. Each boss has288 action comparisons across physical,
magic, balanced, defensive/healing-stat, underprepared and wounded-balanced stat
fixtures. Single-target comparisons add36 per boss. RNG and original counter
invariants passed, as did native range/use checks and use-exhaustion fallback.

- Pest Rat:162/288 action choices and36/36 single-target choices differed from
  v1.19 under these fixtures.
- Harpy:114/288 action choices and32/36 single-target choices differed from v1.19.
- These are production detours on actual native helpers, not just managed scores.
- Recovery/basic and wounded-party focused selections were checked in native
  fixtures. The native helper does not consume a complete rendered attack here.

| Boss table | Illustrative party | Changed actions vs v1.19 | Changed targets vs v1.19 | Maximum sampled modified damage / fixture max HP |
|---|---|---|---|---|
| boss_rat | Physical | 30/48 | 6/6 | 146 / 440 |
| boss_rat | Magic | 6/48 | 6/6 | 114 / 440 |
| boss_rat | Balanced | 30/48 | 6/6 | 146 / 440 |
| boss_rat | DefensiveHealing | 30/48 | 6/6 | 68 / 440 |
| boss_rat | Underprepared | 30/48 | 6/6 | 160 / 352 |
| boss_rat | WoundedBalanced | 36/48 | 6/6 | 114 / 440 |
| boss_hap1 | Physical | 12/48 | 6/6 | 260 / 485 |
| boss_hap1 | Magic | 6/48 | 6/6 | 161 / 485 |
| boss_hap1 | Balanced | 12/48 | 6/6 | 260 / 485 |
| boss_hap1 | DefensiveHealing | 24/48 | 6/6 | 161 / 485 |
| boss_hap1 | Underprepared | 24/48 | 6/6 | 260 / 388 |
| boss_hap1 | WoundedBalanced | 36/48 | 2/6 | 140 / 485 |

Defensive/healing means defensive stats and HP condition, NOT an actual cast or
complete equipment/rune build. Stats derive from boss level for representative
contrast; they are not a calibrated campaign party. No win rate or full fight
outcome was simulated. No physical/magic/defensive composition is certified viable.

Native CalcMonsterDamage executed for selected original moves. Enabling/disabling
quality yielded identical final damage for an identical move/RNG/state, including
existing modifiers. Thus the new layer adds no damage factor. These samples do
not include all criticals, affinity, status, rune, animation or multi-action effects.
Harpy's underprepared fixture can still take260/388 HP from a sampled hit; this is
substantial pressure and needs eventual gameplay assessment. Preparation reduced
sampled damage, but grind-free fairness is not proven.

## Integration and preservation

Existing damage/defence hooks, character profiles,31 spell edits,280 enemies,
51 formations, physical runes, Unites, rune regeneration and Guild1.19 remain.
Native production regression includes Normal/Hard admission, original reward and
inventory components, source/formation and spell/rune checks. These do not replace
full boss tests with all equipment, Unites, rune stacks and story-required parties.

No stat/damage/reward or story callback implementation was rewritten. The accepted
Castle Hunt implementation and v1.19 presentation are source-identical. Native-war
research remains paused and source-identical; no war experiment was run.

Old boss config files lacking EnableCombatQuality remain valid with the new layer
on for its two eligible encounters. Set it false to restore prior tactics/target
behaviour; existing global/per-boss/major opt-outs still apply. No installed user
configuration was migrated or edited. Public/default major policies remain OFF.

A preparatory native check caught and fixed optional-key compatibility. Another
caught insufficient broad-attack discouragement because existing defence preference
could outweigh it; the final finite safety preference passed original evaluators.
Those earlier failed candidates are not final verification evidence.

## Verification limits and recommendation

Build and retained managed/component suites pass; exact-hash native runs pass all
ten admissions,6815 boss checks, actual focused action/target comparisons and
unchanged same-move damage. Original callbacks/rosters/assets remain intact; NO
ending callback, cinematic transition, player-earned boss victory or save/reload
was executed. Historical Sun quest acceptance remains on original v1.12.2 only.

Keep this candidate separate from the installed game and beta. Later brief naturally
available boss sessions can assess duration, healing pressure, status recovery and
story return. Do not require a campaign replay for this milestone. Main remaining
risks are unplayed full-fight cadence/criticals, deliberate low-HP pressure suppression,
limited-move consumption in rendered combat and all major story transitions.
Do not advertise Luca AI or complete Normal/Hard boss balance as verified features.
