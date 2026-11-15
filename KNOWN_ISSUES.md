# Known issues and validation limits

The public source repository tracks v1.35 (`1.35.0-legendary-bosses.1`). An installable
release remains unpublished. [Current Army Training evidence](docs/ARMY-TRAINING-V132.md).

- **Legendary Bosses gameplay acceptance is pending.** Original selectors ran
  with disposable phase states. Transformation playback, cinematic endings and
  campaign balance await play. All three Luca fights remain native in campaign
  play. [v1.35 evidence](docs/LEGENDARY-BOSSES-V135.md).
- **Boss Revolution gameplay acceptance is pending.** All six new policies passed
  native selection fixtures. Rendered battles, the story endings and campaign
  balance still need gameplay. [v1.33 checks](docs/BOSS-REVOLUTION-V133.md).
- **Whole campaign unverified.** Native fixtures cover more than actual gameplay.
  Fairness projections are not a guarantee for every party, boss or story phase.
- **Public-hash verification is limited.** A passed its own isolated startup smoke
  with fresh public config and the rendered original launcher, not a battle/quest.
  B remains runtime-untested. Both originals are preserved. Only debug-path bytes
  changed in the public copies; original quest proof remains original-hash scoped.
- **Exact builds matter.** Sun's accepted full quest belongs to v1.12.2's0B520...
  DLL. v1.13's1A599... DLL has native regression evidence, not current-hash human
  gameplay. Full hashes and scope are in [ACCEPTANCE](docs/ACCEPTANCE.md).
- **v1.15 resource evidence only.** The original OPD async/cache stage completes;
  its public atlas iterator is empty in this binary. This does not validate actual
  tactical textures, UI/audio, full chapter ownership or live field return.
  See [resource report](docs/WAR-RESOURCES-V115.md).
- **Castle Army Training is experimental.** Three original scenarios have live
  tavern returns in the copied test runtime. v1.32 confirmed Veteran War 3's timed
  return and Veteran Commander Rout's earned victory. The latter earned 1,500
  Potch at the purse cap. Below-cap credit, War 3 defeat return and a second
  consecutive War 3 run remain unverified. Commander XP and persistent army
  progression are deferred. [Exact-build training evidence](docs/ARMY-TRAINING-V132.md).
- **Story-war transitions incompletely played.** Audited wars 1,2,3,6,13 retain
  original story handlers, but most full ending/return routes remain unplayed.
- **Legacy major bosses opt-in.** Earlier Lucia/final Neclord policies have not completed rendered
  multi-party/phase/story validation. Public EnableMajorBosses=false. Seven earlier
  admissions also do not certify every move or transition.
- **Dreamer and Thornwake await full gameplay acceptance.** Both are native-tested
  and opt-in. Thornwake's Greenhill City region is verified. Its exact travel
  route remains unplayed.
- **Guild map coverage.** Only the exact audited headquarters tavern variant is
  admitted. Other castle stages may not show the attendant.
- **Session-only contracts.** Actual loading/restart clears accepted/unreported
  state. Selector preference is separate from quest progression.
- **Reward limits remain native.** Rolled loot takes priority over the fallback;
  inventory capacity, decline and money cap apply. Offers are not inventory receipt.
- **Historical UI labels.** Unchanged binaries may still call Sun experimental or
  say acceptance is pending. Later documentation records exact v1.12.2 proof; the
  DLL was not rebuilt just to update those labels.
- **Hard is not the target.** Record admission is Normal/Hard component-tested,
  with historical Hard encounters, not a full Hard campaign/boss/hunt acceptance.
- **Locale/input/resolution coverage is limited.** English and selected physical
  keyboard/controller paths were observed; every mapping/display mode is unverified.
- **Uninstalling does not undo saved growth.** Back up before using the character
  module; original game save routines can retain mod-influenced values.

## Compatibility and reporting

Use the exact game hashes and BepInEx 6 IL2CPP x64 family in INSTALL.md. BepInEx 5,
Mono or interop generated for another build is unsuitable. HarmonyX/Il2CppInterop
come with the separate loader. Suikoden Fix, PKCore and S2 Character Mod are not
required. Their MP/HP/reward/character/war patches can overlap; coexistence is not
guaranteed. Foreign hooks or unexpected signatures can disable modules rather
than risk duplicate scaling.

If UI blocks controls, cancel/exit normally and disable the castle module if needed.
Keep debug/probes off. For failed entry/story transitions stop, preserve the
pre-event backup and report evidence; do not force a result or alter story flags.

Include internal mod version/hash, game build, difficulty, area, party and relevant
settings, reproduction steps and selected redacted log lines. Remove personal paths
and identifiers. Never attach tokens, proprietary binaries, generated interop,
metadata, full development logs or someone else's save collection to public issues.


## Historical development notes

### v1.16 development note

v1.16 isolated graphics tests reproduce a TMP_SubMeshUI.UpdateMaterial shutdown exception, including a no-header/probe baseline on the exact DLL. Bounded header initialization/cleanup has zero observed runtime errors, but full application shutdown is not error-free. Full WarRes.Load, chapter entry and field return remain unverified.


### v1.17 development note

v1.17 has no verified genuine field suspension/return or complete campaign/casualty isolation. Isolated graphics shutdown emits a TMP_SubMeshUI exception and JobTempAlloc warning, also reproduced without a session. Track-specific cleanup success does not establish global application leak freedom.


### v1.18 development note

No genuine field continuation evidence exists for v1.18. The native field-capture branch has not run on a normally loaded MapChapter. Audio/filter/coroutine/save callback owners remain incomplete. Zero save/event activity at a launcher is not field isolation proof.


### v1.19 development note

v1.19 has no human current-hash field/menu/complete-quest acceptance. Dreamer/Thornwake remain native/component-only; Greenhill City region is verified but Thornwake exact travel route is unplayed. A preparatory unloaded asset render timed out, then passed on retry; final native render passed. Existing shutdown exceptions/warnings remain documented.


### v1.20 development note

No v1.20 player-earned boss fight or story transition was tested. Harpy can still deal heavy damage to an underprepared illustrative party; critical/status/rune/full-fight interactions remain uncertain. Existing major policies are Lucia and final Neclord; earlier references to Luca were erroneous. No new earlier-Neclord or Luca phase changes. Boss tactics remain Normal-only.
