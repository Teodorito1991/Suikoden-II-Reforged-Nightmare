# Known issues and validation limits

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
- **No independent war or training.** Launcher unavailable; no repeatable skirmish,
  commander XP or persistent army progression.
- **Story-war transitions incompletely played.** Audited wars 1,2,3,6,13 retain
  original story handlers, but most full ending/return routes remain unplayed.
- **Major bosses opt-in.** Luca/Neclord policies have not completed rendered
  multi-party/phase/story validation. Public EnableMajorBosses=false. Seven earlier
  admissions also do not certify every move or transition.
- **Dreamer/Thornwake unplayed.** Native-tested prototypes, disabled publicly.
  Thornwake's English destination label is pending.
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


## v1.16 development note

v1.16 isolated graphics tests reproduce a TMP_SubMeshUI.UpdateMaterial shutdown exception, including a no-header/probe baseline on the exact DLL. Bounded header initialization/cleanup has zero observed runtime errors, but full application shutdown is not error-free. Full WarRes.Load, chapter entry and field return remain unverified.
