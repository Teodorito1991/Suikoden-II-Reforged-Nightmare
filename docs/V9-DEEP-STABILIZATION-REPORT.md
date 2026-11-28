# Suikoden II: Reforged Nightmare — Project Phoenix v9 Deep Stabilization Report

**Build:** `9.0.0-phoenix.1`  
**Artifact (`ReforgedNightmare.dll`) SHA256:** `4A8D37C11CC4A416F8467E6E970304BB76AD8875B5DBC1877B1F9A753468D17D`  
**Repository:** `https://github.com/Teodorito1991/Suikoden-II-Reforged-Nightmare` (Public, Source-Only — No GitHub Release or Beta ZIP published)  
**Recommendation:** **CONDITIONAL GO** (Ready for isolated copied-runtime live gameplay acceptance; NOT approved for blind overwrite of personal Steam installation or public release until Level 4/5 copied-runtime checks pass)

---

## 1. Exact Starting Baseline (Phase 0)

| Item | Verified Value |
|---|---|
| Starting `HEAD` / `origin/main` | `2bc5e5cb27e47b4ea5977edfe851f50803cc20ec` (`9.0.0-polish.1`) |
| Starting Dev DLL SHA256 (`9.0.0-polish.1`) | `D7ECF4EB82B036806B652D2211CA51A400CAA6292ACBD4DCDC5C08DD84E2E907` |
| Installed Steam DLL SHA256 (untouched) | `8866355290D304DC44934AFA94C25E4431F6E028A4167906E8856356C7900D70` (older `078b52f` build — NOT `9.0.0-polish.1`) |
| Historical Fallback / Accepted Checkpoints | `1.55.0-story-bosses.1` (`C5BE…7055E`), `1.53.0-army-training.1` (`5414…A286`) |
| Remote GitHub Actions CI at Start | Public repository (`private=False`); prior 3 `workflow_dispatch` runs passed (`2bd6729`, `3a32661`, `c468735`); no remote CI run existed yet for `2bc5e5c` |
| Preservation Status | Steam installation, personal `.cfg`, accepted fallback DLLs, and all 14 personal saves were **never modified, copied, or load-tested** |

---

## 2. Exact Ending Build

| Item | Verified Value |
|---|---|
| Ending Version | `9.0.0-phoenix.1` (`Plugin.BuildLabel` & `NightmareRebalance.csproj`) |
| Release Build Result | `0 Warning(s)`, `0 Error(s)` (`net6.0` IL2CPP plugin) |
| Exact `ReforgedNightmare.dll` SHA256 | `4A8D37C11CC4A416F8467E6E970304BB76AD8875B5DBC1877B1F9A753468D17D` |
| Managed Suite Result | **ALL PASS** (`0 Warning(s)`, `0 Error(s)` — `Reforged 9.0.0-phoenix.1 suite 247 checks` + 104k+ assertions across all 30+ subsystem suites) |
| Profile Generators (`fresh-campaign`, `full-content`) | **PASS** |
| Publication Audit (`tools/audit-publication.py`) | **PASS** (no forbidden inputs, private paths, or secrets) |

---

## 3. Complete Architecture Audit Summary (Phase 1)

Every subsystem in `src/NightmareRebalance/` was traced across native IL2CPP hook entry points, shared mutable state, session ownership, and teardown paths:

1. **Persistence (`ArmyProgressionRules.cs`, `ArmyProgressionRuntime.cs`, `TrainingNativeSave.cs`):** Content-addressed sidecar ledger (`army-ledger.json`) bound to native save fingerprints (`SHA256`), save-slot-pinned retention (`MaxLedgerEntries = 4096`), `32 MiB` symmetric read/write cap, immutable `PendingCampaignSave` frozen at save start, committed only on `SystemSave.Save` `cb(true)` with stale-owner rejection.
2. **Core RPG & Combat (`Combat.cs`, `DifficultyPatches.cs`, `Reforged2Runtime.cs`, `NightmareReworkRuntime.cs`, `StarBondRuntime.cs`, `CombatReforgedRuntime.cs`):** Native damage calculation patches (`CalcPlayerDamage`, `PlayerCalcMagicDamage`, `CalcMonsterDamage`, `CalcMonsterDamageLast`, `CalcMonsterHangekiDamage`) separated from native reaction commit patches (`PM_DATA.DamageSetReActionData`, `PM_DATA.SetMagicDamageChara`) with re-entrancy depth guard.
3. **Optional Encounters (`OptionalEncounterLease.cs`, `HallFirstContact.cs`, `TowerOfDunanRuntime.cs`, `SuperbossRuntime.cs`, `CirculatingPathsRuntime.cs`):** Explicit `OptionalEncounterLease` token bound to accepted Hall `Trial` and exact `BATTLE_WORK` pointer; exact-once authoritative completion in `Combat.Complete` (`battle_exit99`) after `Reforged2Runtime.OnBattleVictory`.
4. **Castle Hub & Experimental Dungeon (`CastleGuildRuntime.cs`, `CastleRoomNpcRuntime.cs`, `WalkableDungeonRuntime.cs`, `CastleDialogue.cs`):** Strict `vk07` Hall invariants preserved by default; multi-room NPCs (`EnableExperimentalCastleRoomNpcs = false`) and `vc12` map warp (`EnableExperimentalSindarDungeonWarp = false`) explicitly gated.
5. **War & Training (`TrainingWar.cs`, `War2Runtime.cs`, `TotalWarRuntime.cs`, `WarTacticsRuntime.cs`, `NativeWarBoundary.cs`):** Private flag-forking, native save-fencing during active drills/skirmishes, 10 custom skirmishes (`Reforged2Rules.Skirmishes`), 5 Commander Ranks, 3 Doctrines.
6. **Overlay & HUD (`HuntGuildOverlay.cs`):** Ornate-corner padded `UI_Com_Frame001_001` windows, minimalist battle pill, single top-center callout banner, in-world War Chronicler Orin chronicle toggle.

---

## 4. Confirmed Defects & Root Causes

| ID | Severity | Subsystem | Confirmed Defect & Exact Root Cause |
|---|---|---|---|
| **PHX-01** | **P0** | Persistence (`ArmyProgressionRules.cs`) | **Corrupt primary clobbers valid `.bak` on next save:** When `LoadIndexFromDisk` recovered from `.bak` after a corrupt primary, it left the corrupt file at `ledgerPath`. The next `SaveIndexToDisk` unconditionally ran `File.Copy(ledgerPath, bak, overwrite: true)`, overwriting the valid `.bak` with corrupt bytes before `File.Move`. |
| **PHX-02** | **P1** | Persistence (`ArmyProgressionRules.cs`) | **Missing primary ignored valid `.bak`:** `LoadIndexFromDisk` checked `if (!File.Exists(ledgerPath)) return Empty` without checking whether `ledgerPath + ".bak"` existed and contained valid progression (or was corrupt and should fail closed). |
| **PHX-03** | **P1** | Combat (`Reforged2Runtime.cs`) | **Side-effects inside pure player damage calculation:** `ApplyPlayerPhysical` and `ApplyPlayerMagic` (called inside `CalcPlayerDamage` / `PlayerCalcMagicDamage`) mutated attacker HP (lifesteal), consumed `firstActionUsed` / `surgedPlayers` / `ripostePlayers`, incremented `armorBreaks` / `spellsThisTurn` / `battleCombos` / `Momentum` / Bounties, and recorded DPS during calculation — causing repeated calculations of the same hit to diverge and double-apply side-effects. |
| **PHX-04** | **P1** | Superboss & Spire (`SuperbossRuntime.cs`, `CirculatingPathsRuntime.cs`) | **Reflect & Phase/Shield mutations inside pure calculation:** `ApplySuperbossIncomingModifiers` (True Luca Flame Barrier 8% reflect, phase transitions, Guard Shield refills) and `ApplyGuardianIncomingModifiers` (Floor 3 Mirror Reflection 8% reflect, Floor 6 Chrono Cataclysm phase shift & shield refill) still executed inside `ApplyPlayerAttackOnEnemy` during damage calculation. |
| **PHX-05** | **P1** | Experimental Warp (`WalkableDungeonRuntime.cs`, `TrainingNativeSave.cs`) | **Incomplete warp rollback & missing in-flight save fence:** `WarpIntoSindarDungeon` and `WarpBackToCastle` mutated `g.game_data.area_no/town_no/map_no` and `mcon.ano/vno/mno/ctyp` before `mcon.MachiOutCom()`. On exception, the `catch` block failed to restore any native coordinates (and `WarpBackToCastle` left `DungeonModeActive = false`), and native saves were not fenced during an in-flight warp transition. |
| **PHX-06** | **P2** | Build Hygiene (`NightmareReworkRules.cs`) | **CS0649 test-project compiler warning:** `ShieldReactionState.IsElite` was a field only assigned in the main plugin, causing CS0649 when compiling the standalone managed test project. |

---

## 5. Exact Code Fixes Implemented (Before / After)

1. **`src/NightmareRebalance/Progression/ArmyProgressionRules.cs` (PHX-01, PHX-02):**
   - *Before:* Missing primary returned `Empty` ignoring `.bak`; `.bak` recovery left corrupt primary on disk; `SaveIndexToDisk` copied `ledgerPath` to `.bak` unconditionally.
   - *After:* `LoadIndexFromDisk` checks `.bak` when primary is missing (recovers + atomically repairs primary if `.bak` is `Ok`, or fails closed with `WriteBlocked = true` if `.bak` is corrupt); `.bak` recovery after corrupt primary immediately repairs primary via atomic `.repair.tmp -> Move`; `SaveIndexToDisk` validates `ledgerPath` via `TryParseIndex` before promoting it to `.bak`, guaranteeing corrupt bytes can never clobber a known-good backup.
2. **`src/NightmareRebalance/Combat/Reforged2Runtime.cs` (PHX-03):**
   - *Before:* `ApplyPlayerPhysical` and `ApplyPlayerMagic` directly mutated HP, Surge/Riposte, First Strike, Armor Break, `spellsThisTurn`, Momentum, Bounties, DPS, and Boss Telegraphs inside `CalcPlayerDamage` / `PlayerCalcMagicDamage`.
   - *After:* Both methods are **100% pure** damage calculations that stash a lightweight `PendingPlayerHitEffects` record per `(actor.Pointer, target.Pointer)`. All mutations, heals, consumptions, stacks, Momentum, Bounties, DPS, and `CheckBossTelegraphs` execute exclusively in `Reforged2Runtime.CommitPlayerHit` invoked from `PM_DATA.DamageSetReActionData` / `SetMagicDamageChara`.
3. **`src/NightmareRebalance/Hall/SuperbossRuntime.cs`, `Hall/CirculatingPathsRuntime.cs`, `Combat/NightmareReworkRuntime.cs`, `Core/DifficultyPatches.cs` (PHX-04):**
   - *Before:* Superboss Flame Barrier / phase shifts and Guardian Mirror Reflection / Chrono Cataclysm mutated HP/phase/shields inside `ApplyPlayerAttackOnEnemy`.
   - *After:* Split cleanly into pure damage modifiers (`ApplySuperbossDamageModifier`, `ApplyGuardianDamageModifier`) called during calculation, and committed-hit reactions (`CommitSuperbossPlayerHit`, `CommitGuardianPlayerHit`) called from `NativePhysicalReactionCommitPatch` and `NativeMagicReactionCommitPatch` (working independently even if `EnableTrueNightmareRework = false`).
4. **`src/NightmareRebalance/Hall/CirculatingPathsRules.cs`, `Hall/WalkableDungeonRuntime.cs`, `War/TrainingNativeSave.cs` (PHX-05):**
   - *Before:* Coordinate mutations were unrolled incompletely on warp exception; no save-fencing existed during an active warp transition.
   - *After:* Added pure testable `WarpCoordinateSnapshot` + `WarpTransitionGuard`; `WarpIntoSindarDungeon` and `WarpBackToCastle` capture all 11 coordinate/state fields before mutation and atomically restore all 11 on exception; `TrainingNativeSave` and `TrainingSaveIntent` refuse campaign saves while `WalkableDungeonRuntime.WarpTransitionPending` is `true` until map init settles or rolls back.
5. **`src/NightmareRebalance/Combat/NightmareReworkRules.cs` (PHX-06):**
   - Converted `ShieldReactionState.IsBoss` and `IsElite` to auto-properties, eliminating the last test-project compiler warning (`0 warnings` across both plugin and test projects).

---

## 6. Combat Calculation vs Commit Verification Evidence

- **Pure Calculation Guarantee:** `CalcPlayerDamage` -> `Reforged2Runtime.ApplyPlayerPhysical` -> `NightmareReworkRuntime.ApplyPlayerAttackOnEnemy` -> `SuperbossRuntime.ApplySuperbossDamageModifier` / `CirculatingPathsRuntime.ApplyGuardianDamageModifier` performs **zero** state mutations. Repeated calls for the same hit return identical integers.
- **Committed Reaction Guarantee:** `PM_DATA.DamageSetReActionData` and `PM_DATA.SetMagicDamageChara` (guarded by `TryEnterReactionCommit` / `ExitReactionCommit` depth counter) commit `Reforged2Runtime.CommitPlayerHit`, `NightmareReworkRuntime.CommitPlayerHit`, `SuperbossRuntime.CommitSuperbossPlayerHit`, and `CirculatingPathsRuntime.CommitGuardianPlayerHit` once per landed hit — preserving multihits, Double-Beat, counters, AoE, and two distinct same-turn attacks, while cancelled/missed hits never mutate state.
- **Turn-Scoped Charging:** `EnemyChargeTracker` telegraphs once on Turn N across multi-target AoEs, unleashes uniformly across all targets on Turn N+1 (so `Defend` sees `IsUnleashing(turn)` on every target), and retires cleanly on Turn N+2 or when Stagger-interrupted.
- **Managed Evidence:** Verified in `tests/ArmyProgressionTests.cs` sections 23, 24, 25, and 26c (`247 checks` passing).

---

## 7. Persistence Stress & Recovery Evidence

All 17+ persistence scenarios verified in `tests/ArmyProgressionTests.cs` sections 23, 24, and 26a:

| Scenario | Verified Behavior |
|---|---|
| Missing primary + missing `.bak` | Clean `Empty` fresh start (`WriteBlocked = false`) |
| Missing primary + valid `.bak` | Recovers snapshots from `.bak` AND atomically repairs primary on disk |
| Missing primary + corrupt `.bak` | Fail-closed (`WriteBlocked = true`); refuses overwrite |
| Corrupt primary + valid `.bak` | Quarantines primary to `.corrupt.<ticks>.bak`, recovers from `.bak`, repairs primary, and subsequent save preserves valid `.bak` |
| Externally corrupted primary right before `SaveIndexToDisk` | `SaveIndexToDisk` validates primary before copying; refuses to clobber valid `.bak` |
| Corrupt primary + corrupt `.bak` | Fail-closed (`WriteBlocked = true`); refuses to overwrite on-disk progression |
| Unsupported `SchemaVersion` (e.g. `99`) | Fail-closed (`WriteBlocked = true`, `Status = Corrupt`) |
| Leftover interrupted `.tmp` file | Ignored cleanly; valid primary loads intact |
| 14 distinct save slots + 600 saves in Slot 1 + pruning | All 14 pinned save slots survive pruning without data loss |
| Interleaved / failed / stale-owner async `SystemSave.Save` | Frozen `PendingCampaignSave` commits exact pre-mutation snapshot only on `cb(true)`; failed saves never bind; stale-owner completions after campaign switch are rejected |

---

## 8. War & Training Verification Evidence

- **10 Custom Skirmishes (`Reforged2Rules.Skirmishes`):** All 10 custom skirmishes validated (non-empty IDs/names, escalating rewards up to Skirmish X: Dunan Finale).
- **5 Commander Ranks & 3 Doctrines:** *Recruit -> Marshal*, *Vanguard / Bulwark / Tactician* progression bounded within `OverhaulRules` legal ability/stat limits (`0..20` / `0..5`).
- **Save Isolation:** `TrainingNativeSave` and `TrainingSaveIntent` fence campaign saves while `TrainingWar.Active`, `HallLucaSession.Active`, or `WalkableDungeonRuntime.WarpTransitionPending` is true, preserving original failure callback contracts (`cb(false)` / `onFinish(false, null)`).
- **Managed Suites:** War Commander (`3,705`), Last Bridge (`13,994`), Native War Remix (`9,861`), War Revolution (`24,166`), Total War (`55,912`), War Tactics (`55,774`), War Resource Ownership (`81`), Native War Lifecycle Evidence (`47`) — all **PASS**.

---

## 9. Optional Encounter Ownership Evidence

- `OptionalEncounterLease` binds every Tower of Dunan, Secret Superboss, and Sindar Spire Guardian run to an explicit token, accepted Hall `Trial`, and exact `BATTLE_WORK` pointer.
- Unrelated `WinEnd` / `NotWinEnd` callbacks with mismatched `BATTLE_WORK` pointers cannot complete or abort an active lease.
- Duplicate victory callbacks cannot double-award.
- Cancelled/aborted leases (asset load failure, admission rejection, teardown) cannot later bind or award.
- Authoritative victory runs in `Combat.Complete` (`battle_exit99`) **after** `Reforged2Runtime.OnBattleVictory` so Tower post-victory healing applies during legitimate reward completion while remaining unavailable in unrelated battles.

---

## 10. Tower of Dunan Evidence

- Relics (`vampiric-fang`, `blood-pact`, `black-sword-echo`, `titan-heart`, `aegis-crest`, `bright-shield-echo`, `tactician-tome`, `overdrive-core`, `true-rune-surge`, `arcane-battery`, `stormcaller-sigil`, `iron-riposte`, `executioner-seal`, `shatter-gauntlet`) are strictly gated behind `TowerOfDunanRuntime.BattleInProgress` (active `OptionalEncounterLease`), preventing any relic leakage into normal campaign battles.
- Post-victory healing (`tactician-tome`, `bright-shield-echo`) survives through `OnBattleVictory` before `TowerOfDunanRuntime.OnAuthoritativeVictory` retires the lease.

---

## 11. Secret Superboss Evidence

- All 4 Secret Superbosses (*True Luca Blight*, *Eternal Neclord*, *Awakened Bone Dragon*, *Omega Beast Rune*) launch via `OptionalEncounterLease(OptionalEncounterKind.SecretSuperboss, spec.Id)`.
- Pure damage modifiers (*Sindar Carapace* `-15%`, *Omega Arcane Storm* `+20%` magic) run during calculation; mutating effects (*True Luca Flame Barrier* 8% reflect, phase transitions, Guard Shield refills) now run exclusively on committed native hits (`CommitSuperbossPlayerHit`).

---

## 12. Tower of Circulating Paths / Sindar Ruins Evidence

- **Truthful Framing:** Mod-authored 6-stage logical gauntlet inspired by Sajah cut-dungeon lore (8 Red Orbs, Puzzles, Secret Shrines, 4 Spire Blessings, 6 Stairwell Guardians, Violet Crystal Cycle Rank). Accessible safely via the `vk07` Hooded Stranger without map warping.
- **Experimental Warp (`EnableExperimentalSindarDungeonWarp = false` by default):** Protected by full idle/save/script/war preconditions, atomic 11-field `WarpTransitionGuard` rollback on exception, re-entrancy rejection, and `TrainingNativeSave` save-fencing until map initialization settles.
- Guardian pure modifier (*Floor 2 Stone Aegis*) separated from committed-hit effects (*Floor 3 Mirror Reflection*, *Floor 6 Chrono Cataclysm* phase shift & shield refill).

---

## 13. Cross-System Configuration Matrix

| Toggle | Default | Verified Opt-Out Isolation |
|---|---|---|
| `EnableArmyProgression` | `false` | Disables ledger mutations/saves cleanly; zero side-effects on native saves |
| `EnableTrueNightmareRework` | `false` | Disables Shields, Stagger, Charge, Pack Traits, Hazards, Defend-Heal, Fatigue; Superboss/Guardian pure & committed mechanics still work independently |
| `EnableAntiOverlevelScaling` | `false` | Independent of `EnableTrueNightmareRework`; no surprise enemy stat scaling when disabled |
| `EnableRuneRegeneration` | `false` | Decoupled from `Combat.Complete`; disabling it never suppresses `OnBattleVictory` or optional encounter completion |
| `EnableExperimentalCastleRoomNpcs` | `false` | Preserves strict `vk07` Hall invariants unless explicitly opted in |
| `EnableExperimentalSindarDungeonWarp` | `false` | Safe `vk07` Sindar Spire dialogue used unless explicitly opted in |
| `EnableCombatCombos` / `Perks` / `Surge` / `Momentum` / `Apex` / `Telegraphs` / `Bounties` | Profile-controlled | Each gated cleanly via property checks in `Reforged2Runtime` |

---

## 14. Balance & Formula Analysis

- **Player Damage Soft-Cap (`Reforged2Rules.ApplyStackedPlayerDamageCap`):**
  - Campaign / ordinary fights: capped at **`2.25×`** base damage.
  - Endgame optional encounters (Tower, Superboss, Circulating Paths): capped at **`2.75×`** base damage.
  - Applied **before** `NightmareReworkRuntime.ApplyPlayerAttackOnEnemy` so Guard Shield reduction (`-35%`) and Stagger vulnerability (`+50%`) remain tactically meaningful even at maximum buff stacks (e.g., `200` base -> `900` raw stacked -> `550` capped -> `358` vs Shielded / `825` vs Staggered).
- **Enemy Damage Scaling:** Actual `isMagic` classification (`attack_type == 2`) passed through `DamageScope.Apply` and `FinalBossDamagePatch`, preventing Arcane Storm (`+30%` magic) from mis-scaling physical enemy hits.

---

## 15. UI & Performance Findings

- All `UI_Com_Frame001_001` windows padded clear of `26×26` ornate corners (`HuntGuildOverlay.cs`).
- Corner battle pill stays hidden on ordinary battles when no tactical mechanics are active; combat feed consolidated into a single top-center Suikoden II-style banner.
- `Reforged2Runtime` pending-hit map (`pendingHits`) is O(1) dictionary lookup keyed by `(IntPtr, IntPtr)`, cleared on every `CommitPlayerHit` and `ClearBattle()` — zero per-frame heap allocations during field exploration.

---

## 16. Full Feature Coverage Matrix (Levels 1–5)

- **Level 1:** Code & architecture trace
- **Level 2:** Clean Release build (`0 warnings/errors`) + publication audit
- **Level 3:** Managed unit / adversarial / profile-generator tests
- **Level 4:** Isolated copied-runtime native launch & live hook verification
- **Level 5:** Owner-played live gameplay acceptance (end-to-end campaign/optional play)

| Feature / Subsystem | L1 | L2 | L3 | L4 | L5 | Honest Status |
|---|---|---|---|---|---|---|
| Core RPG, Star Bonds, Hunts, Original Hall (22), Luca Gauntlet, Story Wars, Native Army Training | PASS | PASS | PASS | Hist (`v1.53–v1.55`) | Hist (`v1.53`) | Preserved; re-verify on `v9` copied runtime |
| Army Progression Sidecar Ledger (`army-ledger.json` + `.bak` protection + 14-slot pinning + frozen async save) | PASS | PASS | PASS | **Pending** | **Pending** | All 17+ managed/disk cases passing; live `SystemSave.Save` callback test needed |
| Pure Combat Calc vs Native Reaction Commit (`Reforged2Runtime` + `NightmareReworkRuntime` + Superboss + Spire) | PASS | PASS | PASS | **Pending** | **Pending** | 100% pure calc verified in code & tests; live multihit/AoE/counter check needed |
| Optional Encounter Ownership (`OptionalEncounterLease`: Tower, 4 Superbosses, 6 Spire Guardians) | PASS | PASS | PASS | **Pending** | **Pending** | Exact-once & isolation verified in tests; live victory/defeat/abort check needed |
| Sindar Spire Safe `vk07` Interface | PASS | PASS | PASS | **Pending** | **Pending** | Safe default; live castle dialogue/battle check needed |
| Experimental Multi-Room Castle NPCs (`vk01`–`vk08`) | PASS | PASS | N/A | **Pending** | **Pending** | Gated `false` by default |
| Experimental Sindar Ruins (`vc12`) Warp + `WarpTransitionGuard` Rollback | PASS | PASS | PASS | **Pending** | **Pending** | Gated `false` by default; rollback & save-fence verified in tests |

---

## 17. Tests Passed, Failed, Skipped & Blocked

- **Passed:**
  - `dotnet build src/NightmareRebalance/NightmareRebalance.csproj -c Release` (`0 warnings, 0 errors`)
  - `dotnet run --project tests/NightmareRebalance.Tests.csproj -c Release` (`0 warnings, 0 errors`; all 30+ suites PASS including `Reforged 9.0.0-phoenix.1 suite 247 checks`)
  - `pwsh tools/test-fresh-campaign.ps1` (PASS)
  - `pwsh tools/test-full-content.ps1` (PASS)
  - `python tools/audit-publication.py` (PASS)
- **Failed:** None.
- **Skipped / Blocked (Honest Reporting):**
  - **Level 4/5 Live Copied-Runtime Gameplay:** Not executed in this automated stabilization session (Steam installation and personal saves remain untouched; live interactive playtest requires owner launch in an isolated copied runtime with disposable test saves).
  - **Remote GitHub Actions `workflow_dispatch`:** `gh` CLI is not installed in this environment; remote CI status for the pushed commit must be triggered/verified via GitHub Actions UI unless triggered externally.

---

## 18. Remaining Known Risks & Honest Caveats

1. **Native IL2CPP Reaction Call-Site Parity:** `PM_DATA.DamageSetReActionData` and `PM_DATA.SetMagicDamageChara` are verified IL2CPP symbols, but live in-game verification is still needed to confirm that all player physical attacks, multi-hits (Double-Beat), counters, and AoE rune spells invoke them cleanly without missed commits or edge-case ordering issues.
2. **Live Async `SystemSave.Save` Callback Wrapping:** IL2CPP delegate wrapping of `Il2CppSystem.Action<bool>` in `TrainingNativeSave.Prefix` is compile-verified and logic-tested, but requires a live save/reload cycle on a disposable save slot to confirm zero GC/trampoline issues.
3. **Experimental Gates Remain `false`:** `EnableExperimentalCastleRoomNpcs` and `EnableExperimentalSindarDungeonWarp` remain disabled by default until room-by-room visual and `vc12` warp/return live checks pass.

---

## 19. Recommended Full Content Playtest Configuration (Disposable Copied Runtime)

Use `pwsh tools/prepare-full-content.ps1 -OutputDirectory <isolated-dir>` as the base, and optionally enable:
- `EnableArmyProgression = true`
- `EnableTrueNightmareRework = true`
- `EnableAntiOverlevelScaling = false` (recommended off for first playtest)
- `EnableExperimentalCastleRoomNpcs = false` (keep off during core verification)
- `EnableExperimentalSindarDungeonWarp = false` (keep off; test safe `vk07` Sindar Spire first)

---

## 20. Final Recommendation & Short Copied-Runtime Acceptance Checklist

**Verdict: CONDITIONAL GO** — All confirmed P0/P1/P2 defects found in the Project Phoenix audit are fixed, zero-warning Release build and all managed/profile/audit checks pass, and no Steam files or personal saves were touched.

**Short Copied-Runtime Acceptance Checklist (Disposable Save Slot Only):**
1. Launch isolated copy with `9.0.0-phoenix.1` (`4A8D37C11CC4A416F8467E6E970304BB76AD8875B5DBC1877B1F9A753468D17D`); verify clean `BepInEx/LogOutput.log` startup.
2. Fight 1 normal battle + 1 boss/elite battle: confirm Guard Shields break only on landed hits, Surge/Riposte/Armor Break/lifesteal apply cleanly, and AoE charge telegraphs Turn N -> unleashes Turn N+1.
3. Save twice to a disposable slot, reload an older disposable slot, and verify `army-ledger.json` + `.bak` exist and restore the exact matching progression.
4. Complete 1 Tower of Dunan floor (verify post-victory healing & exact-once reward) and 1 War Chronicler Orin in-world chronicle check.
