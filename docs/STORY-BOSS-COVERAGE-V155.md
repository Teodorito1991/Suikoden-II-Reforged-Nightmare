# Story-boss difficulty completion — v1.55

Full Content Preview, **in-game Normal**. Baseline source is v1.54 commit
`cc0bb1c02de5037de7d15f707915daa7c90694b1`.

**Result: Full Content selects existing AI policies for22 native encounter records,
up from13. Nine records gain tactical choices; no boss gains HP or damage here.**
These are guarded, native/component-tested selections, not new rendered campaign
acceptance. Original Luca and Beast Rune controllers remain native. The separate
Hall roster and accepted gameplay are preserved.

## Inventory and effective before/after

The supported binary's original boss index contains31 distinct `boss_*` table IDs.
All31 were loaded read-only in the disposable runtime. This covers named RPG boss
records from the opening through Beast Rune, including optional/recruitment,
variant and scripted-outcome records. It is **not** a claim that every listed row
is mandatory, a travel itinerary, or that the native index is chronological.
Event battles (`eb_*`), regular enemies, duels and wars are separate systems.

HP below is the initial live-actor result on admitted stat paths, or original
source/controller values where no stat path is admitted. Native opening/phase
scripts retain authority. Damage factors concern the eligible generic native
calculation hook; custom controller/special attacks are not automatically scaled.
A1.0 factor does not make an AI encounter vanilla. None of these factors changed.

| Story source / native table | Effective initial HP, v1.54→v1.55 | Eligible damage | Full AI before→after | Evidence / risk |
|---|---|---:|---|---|
| Mist Shade `boss_ki1` |800 per form, unchanged|1.0|OFF→ON: transformed form uses its original group attack in spaced pressure windows; initial form stays native|Native selectors on supplied phase templates; actual transformation unplayed; medium|
| Double Head `boss_ryo` |2875→2875 (source2500×1.15)|1.10|ON→ON: existing cadence/defence choices|Actual HP and damage hook; native phase callbacks retained|
| Celestial Sword `boss_sei` |2500→2500|1.0|ON→ON: existing pressure/recovery choices|Loaded source and native selectors|
| Pest Rat `boss_rat` |6240→6240 (source5200×1.20)|1.15|ON→ON: existing attrition/quality policy|Actual HP/damage hook and unchanged policy|
| Abomination `boss_nik` |4500→4500|1.0|ON→ON: physical/magic tradeoff and recovery|Native selectors; controller unchanged|
| Bone Dragon `boss_dor` |8900→8900|1.0|ON→ON: existing powerful native-move priorities|Native selectors; controller unchanged|
| Lucia and retinue `boss_lus1` |4700 +3×350→same|1.0|OFF→ON: Lucia only; stronger two-use attack gains wounded-boss pressure priority; basic recovery and defence-sensitive alternatives|Actual native selectors, source/callback/command fence; story ending unplayed; medium|
| Luca `boss_luc1`, `boss_luc2`, `boss_luc3` |6500 source cap per Luca record; original HP handoff, unchanged|1.0|Native→native; original three teams and soldier groups retained|No campaign tactic opt-in or admission; Hall success is separate|
| First Harpy `boss_hap1` |8510→8510 (source7400×1.15)|1.10|ON→ON: existing status/pressure/recovery policy|Actual HP/damage hook; policy unchanged|
| First Worm `boss_imo1` |4200→4200|1.0|ON→ON: native column/group attack choices and finite spell budget|AI-only Boss Revolution, not a stat plan|
| Stone Golem `boss_gol` |7500→7500|1.0|ON→ON: existing pressure/recovery choices|Native selectors; controller unchanged|
| Golden Wolf `boss_gwo1`, `boss_gwo2` |3500→3500 each|1.0|ON→ON: existing physical/magic adaptation and recovery|Both verified1.0 stat plans and native selectors|
| Sierra `boss_sie1` |3080→3080 (source2800×1.10)|1.10|ON→ON: existing quality/defence choices|Actual HP/damage hook; record/recruitment sequence unchanged|
| Earlier Neclord `boss_nec1`, `boss_nec2` |20000 source each, native escorts as applicable; unchanged|1.0|Excluded→excluded: original scripted/invulnerable behavior|Not in Major allowlist; no safe reason to change required outcomes|
| Final Neclord `boss_nec3` |4500→4500|1.0|OFF→ON: original two-use pressure attack, recovery/basic and defence-sensitive choices|Actual native selectors, immutable command/owner/callback fence; live main-controller continuation unplayed; medium|
| Gorudo `boss_gor` |4500 +5×410→same|1.0|OFF→ON: Gorudo spends limited magic against physically defended healthy groups; escorts unchanged|Exact original callbacks/manager/dispatcher scope; native selectors; medium|
| Transforming Worm/Moth `boss_imo2` |4200 /6800 phase sources, unchanged|1.0|OFF→ON: spaced physical/magic/status pressure appropriate to current native form|Original phase/main/command callbacks retained; phases supplied in fixture; medium|
| Separately scripted Moth `boss_imo3` |32000 source, unchanged|1.0|Excluded→excluded|Original BE commands; no supported discretionary policy|
| Later Lucia variants `boss_lus3`, `boss_lus4`, `boss_lus5` |1500→1500 each|1.0|OFF→ON: one-use group spell against a healthy physically defended party, otherwise physical fallback|Entry0 only, finite uses and exact callbacks; native selectors; medium|
| Lucia Highland `boss_lus2` |6500→6500|1.0|OFF→ON: limited three-use pressure attack, basic recovery and defence-sensitive alternatives|Actual native selectors and new command fence; live ending unplayed; medium|
| Second Harpy `boss_hap2` |12000→12000|1.0|ON→ON: existing limited status pressure and recovery|Actual HP/damage hook; variant access is not inferred from its ID|
| Seed & Culgan `boss_kul_sid` |4200 /3800→same|1.0|ON→ON: existing configured `_00` actor policy; partner retains native behavior|Both source identities/stat paths checked; not a claim of a new two-actor coordination system|
| Beast Rune `boss_las` |Five initial active sources4800/3300/4800/5700/4500; unchanged|Native controller|Native→native: barriers, body parts, regeneration/phase rules and ending untouched|Additional1000HP source is not an extra initial active part; no naive summed-HP rebalance|
| Rowd variants `boss_rau1`, `boss_rau2` |3000 /30000 principal sources and original escorts; unchanged|Native|Excluded→excluded|Scripted/bonus/variant records; no source-audited tactic policy; no mandatory-outcome rewrite|

The22 enabled policy records are10 BossTactics +6 Boss Revolution +6 Legendary.
This counts encounter records, including variants, not22 distinct mandatory bosses.
Seven stat plans are the four scaled core records plus **both Golden Wolves and
second Harpy**. v1.54 incorrectly called first Worm the third additional stat plan;
its effective1.0 behavior was correct, but it is AI-only. The old reports retain
their historical conclusions with a correction note.

## Switches, dependencies and decisions

| Setting | Full v1.54→v1.55 | Actual role / decision |
|---|---|---|
|`EnableBossProfiles`|false→false|Legacy exact-ID HP/damage overrides, not an AI switch. Stage A must independently admit scaling. Supplied profile list is empty; enabling adds no useful tactics. Explicit valid profile factors would replace the fallback, not multiply it.|
|`EnableScriptedBossOverrides`|false→false|Legacy selected compiled-identity stat admission, not permission to scale every scripted boss. Supplied allowlist is empty; no broad script opt-in.|
|`boss-tactics.json / EnableMajorBosses`|false→true|Exactly Lucia1, Lucia2 and final Neclord. Does not mean Luca, early Neclord or all bosses. New discretionary command fence is required.|
|`boss-revolution.json / EnableMajorBosses`|false→false|Not needed: all six existing Revolution IDs use EnableExistingBosses. Their existing selection remains ON.|
|Legendary `EnableMistShade`|false→true|Existing form-aware selector; never forces a transformation.|
|Legendary `EnableGorudo`|false→true|Existing actor-specific guarded choice; no escort/ending edits.|
|Legendary `EnableWormMoth`|false→true|Existing native-form selector; separate scripted `boss_imo3` remains unsupported.|
|Legendary `EnableLaterLucia`|false→true|Exactly`boss_lus3/4/5`, not original Luca or earlier Lucia IDs.|

All AI additionally requires compatible supported binaries/Harmony ownership,
Combat Reforged enabled, enemy AI/boss admission enabled, per-pattern Enabled,
Normal difficulty, exact source/party/roster/attack identity and observed native
copy provenance. General per-module and per-boss opt-outs remain. The Full profile
is generated separately and never auto-overwrites personal settings. Fresh Campaign,
embedded defaults and user configurations are unchanged.

## Small production change before activating the three legacy majors

`StoryBossSelectionScope` observes entry to the original `monster_attack_sub`.
It captures the entry command before native code can alter it. Only command0 can
reach mod selection;7,11 and unknown commands remain excluded even after a
fallthrough to0. A separate prefix/finalizer reads ownership and restores its own
scope, including nesting/exceptions. The original function always runs.

The scope checks battle generation, manager/work/actor identity and original
callback methods. Native completion may clear the opening delegate. Unknown main,
command, phase or ending replacements retain native selection rather than being
guessed. In particular, an unobserved final-Neclord main-delegate transition can
make the policy fall back; this build does not invent a steady-state callback.
The existing Legendary scope already handles its explicitly audited steady-state
transitions. No queue, camera, command, callback, flag, resource or outcome is
changed by the new observer. Only the existing legal selector can choose a move.
The accepted Luca implementation and its dispatcher patch were not refactored.

## What the native evidence establishes

- Loaded all31 original named boss tables and matched the existing audited sources.
- Existing source/Normal/callback/roster guards, native legality, finite uses and
  RNG checks passed. Source assets remained unchanged.
- Scoped native selector comparisons changed78/144 Lucia1 choices,94/144 Lucia2
  choices and94/144 final-Neclord choices. Six Legendary records changed233 paired
  choices in total, including26 Mist Shade form-two choices,45 Gorudo,72 combined
  Worm/Moth and30 for each later Lucia. These are decision samples, not win rates.
- Targeted negative tests rejected scripted-entry fallthrough, nested calls,
  stale battle generations, absent/wrong endings and unknown main callbacks.
  Opening completion remained eligible. These tests supplied dispatcher scopes
  around actual native selectors; they did **not** execute full major-boss turns.
-31 native actor-stat measurements across22 policy tables exercised production
  one-time HP application and actual native damage hooks. AI-only additions stayed
  at1.0. Example scratch Rat damage102→117 uses its existing1.15 factor; Lucia287→287
  and final Neclord257→257 show no extra multiplier. Party stats are synthetic,
  not predicted campaign damage. Native callbacks were inspected, never invoked.

## Limits and preservation

New activations are **native/component-tested, not current-hash rendered story
acceptance**. The owner should not replay every boss before starting the campaign.
Actual status chains, party equipment and ending/controller behavior remain full-
campaign observations. Stronger choices of original limited abilities can defeat
an unprepared party; this pass does not certify every difficulty curve.

No regular enemy, character, rune recovery, Hunt, Hall battle/reward, Army Training,
war/save logic or story state changed. No boss HP, attack power, action count,
critical chance or animation was added. No original Steam/config/save overwrite,
owner campaign start or public beta/release. Accepted v1.53 and other DLL fallbacks
are preserved. Duels and unsupported event-battle controllers stay native.

## Build and verification

Build`1.55.0-story-bosses.1`, SHA256`C5BE2F48118E205083096599F0C2B2285DF2CE4E2807DA26B5E3EB561497055E`.
Two Release builds are byte-identical. Managed suite, Fresh/Full configuration
checks and the unloaded native production audit passed. Full-profile copied-game
startup and synchronization are recorded in
[the milestone evidence](../publication/development-build-v155.json).
No gameplay PASS is inferred from these checks.

Implementation references: [major command fence](../src/NightmareRebalance/StoryBossSelectionScope.cs),
[existing boss admission](../src/NightmareRebalance/BossTacticsRuntime.cs),
[existing Legendary dispatcher](../src/NightmareRebalance/LegendaryBossRuntime.cs),
[native coverage fixtures](../src/NightmareRebalance/BossTacticsProbe.cs) and
[explicit Full selection](../profiles/full-content/module-overrides.json).
