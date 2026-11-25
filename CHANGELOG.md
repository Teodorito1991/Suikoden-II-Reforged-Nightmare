# Development history

## v1.52 — Army Legacy (development; training acceptance incomplete)

- Complete native failure callbacks when training rejects SystemSave.Save,
  UISaveLoad2.Save or GSD2SaveData.SaveFile; callers no longer wait without a result.
- Do not count a refused save intent as a completed write or invalidate the native
  return for that reason. Arm ownership before publishing the private flag arrays.
- Fail closed on live-ownership verification exceptions; compare all five flag
  arrays/four records and reject missing/duplicate/stale protected JSON values.
- Native-entry rejection/fault tests and managed/native regressions passed. No new
  player-earned drill/autosave result: automatic approval review blocked preparing
  the campaign copy. Normal-build training remains default OFF in both profiles.
- Commander XP remains deferred: no native progression extension or demonstrated
  campaign/branch identity. Existing750/1,500 Potch victory rewards are preserved.
- Hall/RPG/Hunts/story wars unchanged. No Steam install, save change or beta release.
- Build `1.52.0-army-legacy.1`, SHA256
  `0840237D7ECB736BABADBD0C35F105954B1447AB757883ADB66D8099F6238895`.
  [Evidence and practical decision](docs/ARMY-LEGACY-V152.md).


## v1.51.1 — Full Content integration (development)

- Preview selects all22 supported Original Hall entries, including accepted Luca;
  unresolved story witnesses remain locked unless the player confirms Spoiler Mode.
- Beast Rune native victory, protected return and cleanup verified on this DLL
  using an owner-requested boosted party. Normal-party balance is not certified.
- Clarified independently disabled entries versus story locks; all safety guards
  remain. No Luca implementation refactor, boss/stat changes or repeat Luca test.
- Training native-save acceptance remains incomplete/default OFF; no commander XP.
- Build `1.51.1-feature-completion.1`, SHA256
  `DB2EA401C0F3C0DCC30E4F079AF8A29A2D5C8B70A31E42ABAE68E75998282AE9`.
  [Verification](docs/FEATURE-COMPLETION-V1511.md). No beta/release or Steam installation.

## v1.51 — Last Challenge (development checkpoint)

- Connected the three native Luca encounters to distinct native-roster teams and
  a private HP handoff. Exact-build owner Original completion, party restoration,
  Guild reopening/cancel and movement passed. All three protected snapshots matched.
- Fixed reserve model/equipment initialization, command-ended cleanup and dispatch
  at native idle step20 instead of the earlier return step17.
- Added confirmed, reversible Unlock All Bosses preferences; per-entry opt-outs
  and safety guards remain authoritative. Beast Rune uses a disclosed late bonus.
- Added experimental direct native training-save payload checks; default OFF,
  new gameplay/fault acceptance pending. No commander XP. Beast Rune win, Luca
  Nightmare/defeat and a second full gauntlet remain unverified.
- Build `1.51.0-last-challenge.1`, SHA256
  `7D368B2E115082BEDA83AF7F2590E733114512C73A49487619F64A496705534B`.
  [Evidence](docs/LAST-CHALLENGE-V151.md). No public beta/release.

## v1.50 — Final Encounters (development)

- Added original Worm/Moth and multipart final-boss controller families, bringing
  the catalogue and optional Nightmare profiles to21. Story endings stay excluded.
- Fixed the final encounter's missing extra-texture initialization with the stock
  loader and native timing-3 cleanup, without changing ordinary battle_no=-1.
- Implemented ordered Luca stage/HP receipt rules and a native-getter-verified private
  HP view. Three-party preparation/restore remains blocked; no gauntlet is exposed.
- Added an opt-in normal-build training ownership contract: private flag arrays,
  exact teardown/return ownership and save refusal during active/faulted sessions.
  Exact-build Commander Rout win/return passed; a native post-return copied
  autosave bypassed the GSD2 save-file hook. Its protected flags/war/party payload
  matched the exact loaded checkpoint; direct-write fault handling remains open.
  No persistent commander XP or unknown save fields.
- Preserved prior content, fallback artifacts, personal settings and Steam game.
- Build `1.50.0-final-encounters.2`, SHA256
  `EAF67EB039F569149C9AB811AE770C82EC48DCB6D466AD070458C39D1D5338DF`.
  [Verification scope and remaining gates](docs/FINAL-ENCOUNTERS-V150.md).

## v1.49 — Grand Expansion (development)

- Eleven additional original-source Hall encounters; nineteen definitions and
  optional Nightmare profiles total. Owned group templates and explicit retained
  battle-local phase/command roles reuse the existing request/return lifecycle.
- Four additional direct post-battle story witnesses; seven clearly identified
  bonus memories use verified milestone prerequisites. Three old unresolved
  memories remain normal-locked. No story flag writes or availability bypass.
- Tier-specific native Power/Magic/Speed/Magic Protection stone offers retain the
  existing conditional25% ledger, original loot priority and native inventory.
- Separate Full Content Preview enables all ten Hunts and compatible Hall entries;
  conservative profile and all personal settings remain intact.
- Corrected the general history version label. War/RPG systems otherwise preserved;
  Army Training remains protected-copy-only; no commander progression.
- Build `1.49.0-grand-expansion.1`, SHA256 `D98E62D3B09E0FBFB9DFEC7AA8AE99AB661FF3F11D8DF14B79C2673056AC822F`.
  Actual Mist Shade/Rowd earned victories, native item gains and clean returns
  passed; other new fights and Nightmare rewards remain native-only. Preview
  capture frame ownership improved; intermittent fixed-text omission remains.
  [Complete coverage and evidence](docs/GRAND-EXPANSION-V149.md).


This changelog records development milestones. The source repository is public.
Binary distribution requires separate owner approval.

## v1.48 — The Journey Begins: Campaign Integration (development only)

- Adds a reproducible Fresh Campaign proposal and non-overwriting staging tool.
  Core RPG/story-war layers on; native EXP/Potch rates, two recommended Hunts,
  five source-gated Original Echoes. Nightmare, scripted Legendary changes and
  protected-harness Training remain separate opt-ins/off in this profile.
- Completes The Undying Watch's first real quest loop on this exact DLL: local
  acceptance, walking encounter, earned victory, 12,236 Potch and native
  Sacrificial Jizo receipt, ordinary return, Guild report and reacceptance.
- Early levels 3–6 party won an ordinary battle with actual damage/healing;
  protected save/reload and a later one-charge victory recovery observed. No
  max-level test party, reward fixture or developer protection was used.
- Fixes stale current Hunt/quest/export version labels and contradictory contract
  default text. Encounter, reward, Hall and war engines are preserved.
- Managed/native regressions, real-profile startup, 22 Guild previews and profile
  reproducibility/non-overwrite checks pass. Full campaign, current-hash Hall
  fights and native story-war outcomes remain unverified. Training stays
  protected-copy-only; no commander XP or new war experiments.
- Build `1.48.0-campaign-integration.1`, SHA256
  `56BAB506B49F10A9D2206C9E5903769C3C79A8D092B8748A67C6D53F4923D469`.
  [Profile, evidence and limitations](docs/FRESH-CAMPAIGN-V148.md).

## v1.47 — Hall of Echoes: The Nightmare Trials (development only)

- Adds separate Original/Nightmare selection for three optional advanced trials:
  Abomination (6,750 HP), Stone Golem (10,500 HP), Golden Wolf (4,375 HP).
  Existing eight Original sources, progression and rewards remain intact.
- Native-legal tactical preferences and one owned final-damage increment: 10%
  for Abomination, 15% for Golem/Wolf. Fixes a real first-candidate integration
  defect: special attacks bypassed the calculation helper. Revision .2 uses the
  existing native reaction boundary once, without changing story-war/boss scope.
- Conditional 25% Stone of Power offer on earned Nightmare victory with no native
  loot, native inventory acceptance, per-trial pending-offer ledger and no payout
  after flee/defeat. Native Strength growth/cap and full bag handling checked.
- Final .2 owner evidence: five Nightmare wins across Golem and Abomination,
  actual stone inventory +1, Nightmare and Original flee, clean repeated Guild
  returns and unchanged protected state. Wolf's earlier-candidate win is separate.
- Managed/native regressions and 22 native-style previews passed. New-tier defeat,
  midgame parties and full campaign balance remain untested. Hall and Nightmare
  default off; original installed DLL/saves and v1.46 fallback preserved.
- Build `1.47.0-nightmare-trials.2`, SHA256
  `7283F4B6873C6AFA8BE4277ADF6A7E09CEB42E4607C168D3AB3A381CABE211BF`.
  [Detailed rules and evidence](docs/HALL-NIGHTMARE-TRIALS-V147.md).

## v1.46 — Hall of Echoes: New Memories (development only)

- Adds original Stone Golem, Abomination and first Worm rematches, extending the
  existing Hall to eight entries on two pages without replacing its engine.
- Source-backed Stone Golem C9/01 and Abomination 56/20 story witnesses; retains
  corrected v1.45 gates. Worm, Sierra and second Harpy remain testing-only.
- Exact-build owner wins credited 50,000 / 10,000 / 35,000 Potch under test settings.
  Golem and Pest Rat Let Go returns paid zero; protected campaign state and cleanup
  passed throughout. No new defeat acceptance or paired story-unlock session claim.
- Native sources, callback roles, owned resources, managed regressions and 18-screen
  graphical preview passed. Second Harpy payout remains unchanged after review.
- Global/new individual switches remain off by default; original Steam DLL, saves,
  existing beta and prior accepted artifacts preserved. No binary publication.
- Build `1.46.0-new-memories.1`, SHA256
  `35F2E3BC0A8A84EB401E92C535D503ABF3059F87112AA6F3921BE87BDD59F50D`.
  [Detailed evidence](docs/HALL-NEW-MEMORIES-V146.md).

## v1.45 — Hall of Echoes: Echoes Earned (development only)

- Corrects the v1.44 story-gate identity error: map indices and nonordinary battle
  IDs are not boss-resource indices. Adds Pest Rat B6/02, first Harpy 21/10 and
  Golden Wolf 16/02 witnesses linked to original category-15 preloads and native
  victory continuations. Removes the unrelated old witnesses.
- Keeps Sierra and second Harpy normal-play unlocks unavailable rather than
  guessing. Both existing encounters and the explicit developer availability
  override remain implemented. No encounter, stat or reward changes.
- Strengthens six original event-source audits and native production gate/reset
  checks. Current-hash Golden Wolf and first Harpy owner victories passed under
  StoryProgression, with native payouts and clean Guild returns.
- Owner earned Sierra and second Harpy victories on this exact DLL under the
  explicit testing override: 850 / 127,500 Potch, no items, clean Guild returns.
  Post-Harpy Pest Rat flee passed with zero payout and working Guild interaction.
- Preserves historical reports, both accepted fallback artifacts, normal default-
  off settings and all existing RPG/Hunt/war systems.
  [Exact-build results and limitations](docs/HALL-ECHOES-EARNED-V145.md).

## v1.44 — Hall of Echoes: Echoes of the Past (development only)

- Adds original Golden Wolf, Sierra and second Harpy trial definitions to the
  shared Hall lifecycle. Golden Wolf safely uses its verified null extra-pack
  mapping; source/scene ownership and original battle index -1 are preserved.
- Adds default StoryProgression policy, read-only source-backed Harpy / Sierra /
  Golden Wolf witnesses, separate per-trial switches and an explicit developer-only
  availability override. Pest Rat and second Harpy remain conservatively locked
  under normal progression until their witnesses are established.
- Adds five-entry pagination, hidden locked identities, original foe levels and
  truthful native reward descriptions. No new stats, payout engine or story writes.
- Owner played Golden Wolf and Sierra victories (8,500 / 850 Potch), followed by
  second Harpy defeat and stock Game Over on candidate .1. Protected state and
  owned cleanup passed. Revised candidate .2 passed Pest Rat victory, native rewards and Guild reopening;
  second Harpy victory and original-story unlock timing remain unaccepted.
- Preserves exact v1.43 fallback, installed Steam game, RPG/Hunts/war systems and
  unpublished beta. [Evidence and limitations](docs/HALL-ECHOES-PAST-V144.md).

## v1.43 — Hall of Echoes Second Challenger (development only)

- Adds the authentic first Harpy rematch, with its original 7,400 HP, moves,
  hap resources, mountain battlefield and native Potch rewards.
- Reuses the accepted Hall encounter and cleanup lifecycle through audited trial
  definitions. Pest Rat keeps its original IDs, scene and native loot.
- Adds two-row Guild selection and an independent default-off EnableHallHarpy
  setting. EnableHallOfEchoes also remains false by default.
- Verifies original resource mappings and loot records in the production audit.
- The owner completed Pest Rat → Harpy → Pest Rat victories and reopened the
  Guild. Exact-build receipts recorded 17,000, 34,000 and 17,000 Potch under
  tested settings, plus a Poison Orb after each Pest Rat victory.
- Harpy flee returned with no reward. An earlier native Harpy defeat reached
  stock Game Over and released the trial's resources. All terminal receipts
  preserved protected flags, war data and progression.
- Owner-earned Harpy defeat → stock Retry → Headquarters → Guild → new Pest Rat
  encounter → native flee also passed. Default-off startup installed no optional
  Hall hooks. The accepted v1.42 fallback and fourteen original saves were preserved.
- Private automation failures are excluded from acceptance. No player package
  or release was published.
  [Build hash, verification and remaining limits](docs/HALL-SECOND-CHALLENGER-V143.md).

## v1.42 — Hall of Echoes Safe Return (development only)

- Retires the Hall's exact combat state after native defeat so a reloaded
  Castle Guild can attach to the new field owner.
- Handles original returns without a victory and waits for native teardown
  before releasing the trial's resources.
- Two consecutive player-earned wins each delivered 17,000 Potch and one
  Poison Orb on the tested settings. Both returned to the same Guild spot.
- Matched ordinary and Hall defeats against the same Retry checkpoint.
  Their autosaves differ only in timestamps and timers.
- Adds a default-off configuration gate for the Pest Rat rematch in a
  normal source build. The feature checks compatibility before installing
  its hooks. No player package was published.
- Final acceptance on the exact normal DLL passed native victory and
  defeat → Retry → Guild → rematch. Produced a separate reproducible local
  rebuild without private debug paths.
  [Exact build and verification](docs/HALL-SAFE-RETURN-V142.md).

## v1.41 — Hall of Echoes First Contact (development only)

- Opens a Pest Rat rematch through the Castle Guild in a protected copied
  runtime. Normal builds keep Hall trials unavailable.
- Rendered native combat produced a victory and a return to the Guild
  corridor. A second launch ended in stock Game Over. Both runs passed the
  trial's scene and resource teardown checks.
- Native victory delivered a Poison Orb. The test party was already at the
  Potch cap. Stock Retry restored movement but the Guild attendant was absent
  in the reloaded corridor.
  [Exact build and limitations](docs/HALL-FIRST-CONTACT-V141.md).

## Hall of Echoes investigation (untagged)

- Rehearses a native RPG request and detached Pest Rat setup on disposable owners.
- Adds an unavailable Hall information page to the development Guild source.
- Documents blockers for live chapter entry, return and battle-only guests.
  [Engineering handoff](docs/HALL-OF-ECHOES-ENGINEERING-HANDOFF-V141.md).

## v1.40 — 108 Stars: Party Revolution

- Trades growth tiers for fourteen battle characters and opens four existing
  rune slots slightly earlier. Preserves custom character rows.
- Adds four optional Star Bonds tied to Defend, Attack on a poisoned target,
  Fire Wall against armor, and Bandit Attack on a wounded target.
- Adds Party Strategy to the original-style Castle Guild services menu.
- Native damage and growth lookup comparisons, managed tests and copied-game
  graphical preview passed. Full battle and campaign acceptance remain pending.
  [Party Revolution evidence](docs/PARTY-REVOLUTION-V140.md).

## v1.39 — Tactical Revolution

- Adds coordinated targeting and native move selection to twelve ordinary
  formations and five Hunts. Packs share pressure, disruption teams check
  existing statuses, escorts react to living casters and mixed groups weigh
  physical and magical defenses.
- Adds an independently configurable Defend enhancement for qualifying magical
  hits in those formations. Older settings files load with both new keys
  optional.
- Original native attack and target comparisons, damage checks and copied-game
  startup passed. Full RPG battle play and campaign balance await acceptance.
  [Profiles, exact build and evidence](docs/TACTICAL-REVOLUTION-V139.md).

## v1.38 — Nightmare Campaign

- Enables all ten native-tested Hunts in the fresh Normal campaign
  configuration and orders the catalogue by tier.
- Adds optional local contract reissue through the original party menu.
  A new request binds only to the current party and its audited walking field.
- Reduces ordinary enemies' preference for area Sleep when at least half
  the eligible players are already asleep. The existing pacing toggle
  controls the adjustment. Hunt and boss AI retain their previous rules.
- Shows the Army Training configuration and copied-runtime restrictions
  separately. A committed drill still receives its guarded teardown if
  admission settings change before return.
- Added three-stage pressure projections and verified the local acceptance
  path with native component fixtures. Full campaign play remains untested.
  [Campaign settings and evidence](docs/NIGHTMARE-CAMPAIGN-V138.md).

## v1.37 — Grand Guild expansion

- Adds seven replayable Hunts, from Bramble Road to The Tenzan Trial, with
  audited original formations, distinct preparation advice and rewards.
- The ten-contract list shows four rows per page. Native up/down input keeps
  the selected contract visible across page boundaries.
- Original victory handles the new Potch bonuses and conditional consumable,
  growth stone and rune orb rewards. Existing three-contract files merge the
  seven new defaults in memory and keep their existing settings.
- Native checks and a rendered copied-game preview passed. Live travel and
  complete quests for the new contracts remain unplayed.
  [Exact build and catalogue](docs/GRAND-GUILD-V137.md).

## v1.36.1 — Guild preview

- Runs the copied-game graphics preview on a separate Windows desktop. A
  hidden launch loaded the UI resources but did not dispatch GUI repaint.
- Captures and verifies eight native-rendered screens, including both Army
  Training difficulties, using the existing Guild renderer.
- Live Castle Guild interaction and training return remain unverified on this
  build. [Exact build and evidence](docs/GUILD-PREVIEW-V1361.md).

## v1.36 — Total War Revolution and Castle Guild Reborn

- Adds native enemy preferences for protecting commanders and ranged squads,
  avoiding bad trades, advancing through reachable terrain and finishing an
  exposed squad. Wars 10 and 15 receive smaller adjustments.
- Moves the Castle Guild from the tavern to the original Suggestion Box
  corridor. Confirm gives original events first priority.
- Rebuilds the Guild's service menu with separate Hunt, Training difficulty
  and Guild Status screens. Training captures the player's corridor position.
- Native fixtures cover war decisions and the new corridor. Launcher and
  Guild asset loading passed. The graphical preview produced no frames. Live
  interaction, story-war battles and training returns await campaign play.

[Exact build and evidence](docs/CASTLE-GUILD-TOTAL-WAR-V136.md).

## v1.35 — Legendary Bosses

- Mist Shade reserves its stronger spell for the transformed phase and a
  pressure window. Gorudo alternates physical pressure with spells against
  parties built to absorb weapon attacks.
- The transforming Worm and Moth favor different attacks, including status
  pressure when enough targets can still receive the effect. Three later Lucia
  records hold their one-use group spell for a suitable opening.
- Scoped the changes to discretionary native attack selection on Normal.
  Original targeting, use accounting and scripted callbacks stay native.
- Solo Luca's scoped selector committed a native row attack in a disposable
  fixture. Campaign Luca tactics remain disabled.

[Exact build, settings and verification](docs/LEGENDARY-BOSSES-V135.md).

## Documentation maintenance

- Records the owner's decision to keep the source repository public.
- Updates the README and known issues to reflect v1.32 Army Training and its
  gameplay verification limits.

## v1.34 — RPG Evolution

- Revises 15 existing character profiles with distinct roles and tradeoffs.
- Tunes seven spell effects and four additional physical techniques.
- Rebalances six original Unites while preserving their participants and side effects.
- Adds seven guarded regional remixes with reduced base HP budgets.
- Native asset, setup and coefficient tests passed. Campaign balance and full
  action playback await gameplay.

[Exact build, changes and verification](docs/RPG-EVOLUTION-V134.md).

## v1.33 — Boss Revolution

- Adds guarded attack selection for Celestial Sword, Worm, Abomination,
  Stone Golem, Bone Dragon and Seed.
- Improves ordinary-enemy group status scoring when few targets can still
  receive the effect.
- Compiled and verified native selectors, retained regression and startup
  on the exact candidate. Rendered boss and story acceptance remain open.

[Build hash, controls and evidence](docs/BOSS-REVOLUTION-V133.md).

## v1.32 — Castle Army Training (`v1.32.0-dev.2`)

- Adds an optional Veteran tier using the existing native enemy tactics.
- Adds 750 Potch on Standard or 1,500 on Veteran for a qualifying commander rout.
- Separates training difficulty, objectives and rewards from the hunt cards.
- Verified Veteran War 3 timed return and Veteran Commander Rout victory in play.
- Commander Rout earned 1,500 Potch at the purse cap. A below-cap payout and the guarded War 3 defeat return remain unverified.
- Defers commander experience until campaign ownership can be verified.

See [Army Training testing and configuration](docs/ARMY-TRAINING-V132.md).

## v1.31 — Commander Rout controls and defeat return

- Adds opt-in War 3 training on a distinct original battlefield. The owner completed one run and returned to the tavern with a clean return audit.
- Transfers active allied squads at the original command-selection step in Commander Rout.
- Verified earned defeat, training-only return, manual save/reload and a subsequent Fort Defense return.
- Both tested returns preserved audited campaign and character state. Commander Rout victory remains untested.

See [War Training verification](docs/WAR-TRAINING-V130.md) for the exact hash.

## v1.30 — Experimental Castle War Training

- Adds Fort Defense using the original War 2 scenario and four controllable squads.
- Retains standard War 1 and the same-map Commander Rout variant.
- The owner completed Fort Defense and returned to the castle. The return
  audit found equal restored flags and only position and clock differences.
- Training remains opt-in and limited to the test runtime. Repeat War 2,
  defeat and manual save/reload remain unverified on this build.

See [War Training verification](docs/WAR-TRAINING-V130.md) for the exact hash.

## v1.27 — Boss expansion and campaign combat polish

- Sierra varies focused pressure and area attacks based on party health.
- Both Golden Wolf encounters use a focused recovery move and are more
  likely to use their original group spell against armour than magic defence.
- The second Harpy encounter prefers its existing limited-use status move
  when it can still affect the party.
- Regular enemy targeting reads the active attack's status and discards
  previous-phase focus before scoring native-legal candidates.
- The loader banner now identifies the current development version.

No new boss admissions, stat multipliers, story changes or independent
skirmishes. See [boss expansion](docs/BOSS-EXPANSION-V127.md).

## v1.26 — Steam campaign installation

Installed the existing candidate without rebuilding. Verified the exact DLL
hash, plugin initialization and the original game selector in the Steam
installation. New Game entry remains unobserved. See the
[Normal campaign setup](docs/NORMAL-CAMPAIGN.md) for installed options.

## Unreleased — Luca caller-scope architecture audit

Rechecked the supported native selector and dispatcher without launching the
game. Result substitution remains credible, but the actor-command branch can
fall through into selection; command-bearing dispatches must remain excluded.
Prefer observing native ownership over reconstructing managers/cameras/queues.
Campaign-capable save, preferences and Cloud isolation remains unverified.
No gameplay source, DLL, configuration or beta change; Luca tactics unavailable.
See [caller-scope audit](docs/LUCA-CALLER-SCOPE-AUDIT.md).

## Historical — private staging workflow

Added a permanent completed-milestone GitHub synchronization policy in AGENTS.md:
update documentation/configuration guidance and exact-hash evidence, push audited
source, verify the remote commit and CI, and report the commit link. Development
remains private; existing beta assets, tags and draft releases are unchanged.

## v1.26 — Playable development integration

Packaged the exact v1.25 DLL with separate Dreamer and Thornwake opt-ins.
Verified both profiles at startup and documented original local map connections,
protected checkpoint requirements and a short quest checklist. Full campaign
isolation and rendered quest acceptance remain pending.

Reused DLL SHA256 `AA27445761E1E7581FF2D32DCF5FA608E47A072F06A5B307B809A46557EA09B0`.
[Protected gameplay candidate](docs/HUNT-ACCEPTANCE-V126.md).

## v1.25 — Monster Hunt Catalogue (development only)

Connected the Castle Guild request observer to native setup for all three
contracts in isolated tests. Tightened owner and Normal-rate guards and added
specific Sleep and Balloon preparation text. Dreamer and Thornwake remain
experimental and opt-in pending gameplay acceptance.

DLL SHA256 `AA27445761E1E7581FF2D32DCF5FA608E47A072F06A5B307B809A46557EA09B0`.
[Guild evidence and limitations](HUNT_GUILD.md#v125-catalogue-integration).

## v1.25 — Luca battle context decision (research only)

Mapped original scene setup, camera queue consumption and teardown. Historical
Hunt evidence does not cover these ownership boundaries for Luca. Further
isolated Luca experiments are deferred. The v1.24 DLL remains the development
baseline. [Integration decision](docs/LUCA-BATTLE-CONTEXT-V125.md).

## v1.24 — Luca Manager Ownership (development only)

Verified native battle-reader initialization and manager/actor queue ownership,
including reaction cleanup and re-entry. Full dispatch still requires an owned
battle field, camera and continuation. Live tactics remain unavailable.

DLL SHA256 `6543164A053D3EEFD0BBA4F60B93805472CF51E1C1AC50DA4D351AE8FBC7D427`.
[Manager ownership audit](docs/LUCA-MANAGER-OWNERSHIP-V124.md).

## v1.23 — Luca Action Commitment (development only)

The original solo Luca dispatcher now commits distinct basic, row and column
actions in an isolated audit. Silence and no-target cases exercise native retries.
Live tactics remain gated on full manager initialization and damage validation.

DLL SHA256 `D14F9F29939A099DE1470C5F78EDE34ADA92ABF74E4A937FA3F0184F4F00EA82`.
[Action commitment audit](docs/LUCA-ACTION-COMMITMENT-V123.md).

## v1.22 — Luca Tactical Dispatch Audit (development only)

Traced the solo encounter's native attack selection, retry and target-building
paths. Added isolated native row and column target checks with full and depleted
party fixtures. Live Luca tactics remain gated on verifying action commitment
through the original dispatcher. No configuration migration is required.

DLL SHA256 `C3360B8BFB82006E84DD4434DA18BB1E30709978449930CE022591E328A3E8ED`.
[Dispatch audit](docs/LUCA-DISPATCH-V122.md).

## v1.21 — Luca Blight Encounter Foundation (development only)

Audited Luca's three original records and traced the HP carryover and distinct
outcome callbacks. Added two isolated native selector runs covering all three
records. Existing AI changed zero choices in the paired sequences. Luca tactics
remain unavailable pending live dispatch and party handoff verification.

DLL SHA256 `92335E7432D2805536A0409ABB366EB6E155D05EE45F4F8B0670F432863B1E8D`.
[Encounter audit and evidence](docs/LUCA-ENCOUNTER-V121.md).

## v1.20 — Boss Combat Quality (development only)

Refined Pest Rat and first Harpy action/target preferences: focused openings and
wounded-party pressure, stronger basic recovery windows, current HP/defence response
and reduced redundant status/focus. Actual native selectors/targets differ from the
v1.19 layer, preserve RNG/legality and add no damage/HP factor. Added compatible
optional EnableCombatQuality switch; existing files/opt-outs remain supported.

Corrected a substantive audit error: lus1/lus2 are Lucia, not Luca Blight. Actual
luc1/luc2/luc3 party transitions remain unmodified and unverified. Earlier Neclord
forms remain excluded. Exact DLL `288B5D76311ADFE379D08174CE0C6CE0BE22A90FA294787F10A562F25593FA7B`; native/component evidence only, no full
boss gameplay/story-return acceptance. Guild1.19, installed v1.13, original settings,
war research and private beta preserved. [Detailed audit](docs/BOSS-COMBAT-QUALITY-V120.md).

## v1.19 — Hunt Guild Gameplay Polish (development only)

Refined the existing native-looking contract layout, readable spoils and state
messages. Acceptance errors now appear on screen; disabled prompts, compact quest
actions and report/acceptance feedback are clearer. Resolved Thornwake's region to
Greenhill City area through the original native registry/save-name/text alias path.

No reward, drop probability, quest lifecycle, encounter or balance changes. Dreamer/
Thornwake remain experimental and disabled by default. Exact DLL `0EF0B648CC897C2119ED6919DA250FD1FDFFD1CAC5F8F536FB8BA8B5A8A1BEA7` passed
unloaded native rendering and RPG/Hunt component checks; no human field/quest
acceptance on this hash. Installed v1.13/beta preserved, war research paused.
See [details and known limitations](docs/GUILD-POLISH-V119.md).

## v1.18 — Real Field Continuation Assessment (development only)

Added strictly passive field ownership/interpreter/input/menu/render/audio registry
and save-intent instrumentation, bounded diagnostics and fail-closed evidence rules.
Startup smoke verified the recorder accurately reports NO genuine field baseline.
No field suspension, resource session under a field or continuation was attempted.

Decision: NO-GO; pause independent skirmish development. The startup-only isolation
guard rejects campaign loads; a protected normal field and complete ownership/write
boundary remain unavailable. Do not substitute resource probes or fictional results.
Exact DLL `376F51AB416C9A785F18B12B6884B690224BC6BEE4DC1F32B8607CA4937AD691`. Installed v1.13, v1.17 artifact/source reference and beta preserved.
See [status, limitations and restart prerequisites](docs/FIELD-CONTINUATION-V118.md).

## v1.17 — Integrated Native Session and GO/NO-GO (development only)

Combined verified original OPD coroutine and header initialization under one
exclusive NativeSkirmishSession. Added ordered native cleanup, re-entry, interrupted
loading, concurrent-owner rejection and original cache/registry identity checks.
Reused the audited adapters and shared header preflight; no new gameplay system.
Two isolated processes each completed two combined sessions and an interrupted
session. Traced two native casualty-path mode33 writes and rehearsed their flag
primitive on scratch arrays, without running casualties or story callbacks.

Active skirmish remains NO-GO: genuine field continuation and complete campaign/
casualty/audio isolation are unresolved. Exact DLL `DC44899D9F3A8182BE95686A831CD351C3C9C2C077ABEFB5A9C0E173DC8E8C97`. Known shutdown TMP
exception/JobTempAlloc warning reproduce on a no-session baseline. Installed v1.13
and beta unchanged. See [decision and evidence](docs/SKIRMISH-SESSION-V117.md).

## v1.16 — Native War UI Ownership and Feasibility (development only)

Executed the original WarHDHeader factory and native component cleanup/reacquisition
in two isolated processes. Added scoped hierarchy-root ownership, audited prefab
callback guards and separate instance/material versus borrowed-asset rights.
Preserved shared fonts, original roots and retained field resources. No active
chapter, field return, skirmish launcher or gameplay change is enabled.

Exact DLL: `1CA1F3F56200561B49EDCADCCF1295989716F0FB7D3AA4A7D077CC27261893C1`. Managed regressions, retained native resource adapters and two
production component runs passed. A shutdown TMP exception also reproduces in an
exact-hash no-header baseline; no full application error-free claim. Installed
v1.13 and beta unchanged. See [evidence and next experiment](docs/WAR-UI-V116.md).

## v1.15 — Native War Resource Lifecycle (development only)

Executed the original War01 OPD async sub-coroutine, private cache publication and
native lookups, native release and reacquisition. Added ownership categories for
acquired references, shared assets, retained resources and unknown/conflicting
registrations. Actual unclassified native callbacks now block before cleanup.
Scoped/restored the native GSD2 resource selector and private cache per coroutine
step, including exception paths. Verified that the public atlas sub-coroutine is
empty on the supported build; no texture/UI/audio or battle claim follows.
All retained regressions pass. Live chapter entry, sound-bank ownership, field
continuation and full campaign-write isolation remain blocked. No player config,
installed DLL, saves or beta assets changed. [Exact evidence](docs/WAR-RESOURCES-V115.md).

## v1.14 — Native War Skirmish Bridge (development only)

Implemented private native ReleaseCheck ownership and original WarChapter cleanup
on an isolated scratch object. Actual War1 asset loading/release and a retained
castle asset with a colliding timing-4 ledger passed twice per process in two
exact-hash native runs. Added duplicate/nested/thread/fault/once-only guards.
All retained managed and full production-native regressions passed. No tactical
battle or live field return was attempted; chapter ownership, asynchronous UI/
audio/cache lifetimes and complete campaign-write isolation remain blockers.
Live launch and commander progression remain unavailable. No install/config/save
or beta-release asset changes. [Exact hash/evidence](docs/WAR-BRIDGE-V114.md).

## Proposed v1.12.2-beta.1

Packaging-only proposal, unchanged internal 1.12.2-guild-ui.1. A personal CodeView
PDB path is removed in a separately hashed public copy; no rebuild or inherited
runtime acceptance by inheritance. Candidate A subsequently passed a separate
isolated startup smoke, not a new hunt session. Original accepted DLL preserved.
Normal foundation,
Sun Guild/item fallback on in fresh defaults; unplayed contracts/major bosses,
debug and probes off. Full campaign remains experimental.

## v1.13 — Native War Foundation

Historical development foundation. Added unloaded native flag-isolation rehearsal,
exception-safe singleton/RNG restoration and field ownership prerequisites.
Two native runs, no chapter launch, earned war outcome or current-hash human
session. Independent skirmishes and training remain unavailable.

## v1.12.2 — Guild UI

Original frame/cursor/font, bounded layout, catalogue/detail and quest-state views.
Later exact-build user acceptance: complete Sun quest, two wins, native items,
actual Potch credit, field return, report/reaccept and repeat entry.

## v1.12.1 / v1.12 — Castle integration

Guarded talkable attendant, session-only quest state, automatic eligible walking
request, deduplicated completion and report without second payout.

## v1.11 / v1.11.1 — Integration and Guild repair

Consolidated earlier combat acceptance; fixed menu binding and explicit rendering
after an invisible panel/input-blocking defect.

## v1.10 — Native war audit

Read-only lifecycle witness and precise independent launch/isolation/return blockers.
No playable skirmishes or earned training.

## v1.9 / v1.8 — Boss policies and Hunt Guild

Major boss research/native policies, selector workflow, guarded native fallback
item preparation and inventory observation.

## v1.5 through v1.7 — Native hunts

Sun prototype, native rewards, voluntary walking selection, debug support and
three-contract architecture. Historical debug wins are not acceptance of every
later DLL.

## v1.3 through v1.4 — Combat reality

Production encounter guards, Hard admission maintenance, damage diagnostics,
bounded battle reports and optional encounter groundwork.

## v1.0 through v1.2 — RPG systems

Character, spell, formation, regular-enemy, boss, rune and Unite layers, with
native/component tests and explicitly uncalibrated fairness models.

## v0.1 through v0.9 — Nightmare and original war changes

Partial victory recovery, scoped multipliers, original war unit/AI/objective work.
A separate Unity arena was investigated and retired, not a current feature.
