# Reforged Nightmare v1.29 — native War 1 skirmish report

**10 November 2026 · Two consecutive normal endings verified in the disposable game copy**

The Castle Guild can launch War 1 through the original Suikoden II tactical engine, return the player to Leona’s tavern, and launch the battle again without a reload. The owner completed that sequence twice in the same session after the spawn fix. Defeat and manual save/reload after training remain open, so this is an experimental build.

## What the player tested

| Run | Build | Observed result |
|---|---|---|
| Stock War 1 | `1.29.0-stock-observer.1` | The owner reached the original story war from the early checkpoint, finished it, postponed the next story fight, and confirmed field movement. This provided the native transition trace. |
| First Guild training run | `1.29.0-training.1` | The Guild launched War 1. The battle ended normally and the tavern map loaded, but the player spawned outside the walkable area. The return hook reported matching flag arrays and zero save calls. |
| Corrected training run 1 | `1.29.0-training.2` | The owner finished War 1 and returned inside the tavern. The native return position matched the captured standing position `896,392`. |
| Corrected training run 2 | `1.29.0-training.2` | The owner entered again without restarting or reloading, finished the battle, and reported a successful return. The native return position matched the new standing position `896,400`. |

On both corrected runs, the return hook reported equal flag arrays, zero intervening `SaveFile` calls, and repeat entry available. The owner confirmed the returns. A separate menu open/close result was not recorded.

## What changed

**Guild entry.** In the disposable copy, with the opt-in enabled and War 1’s campaign completion flag already set, the Guild offers **Train: War 1** on the bottom row when there is no active hunt to report or withdraw. After the player releases confirm, the code checks that the field is still idle, takes a snapshot, runs the native fade, and sets up the original step-45 handover. Stock war scripts, turns, ending callbacks, and chapter teardown then run normally. The training scope suppresses the mod’s story-war adjustments for this run.

**Spawn fix.** The first build used coordinates from the field event controller. They did not describe the player’s live standing position in this tavern test. The corrected build reads the player actor’s live coordinates, checks that the actor stays there during the fade, and passes those coordinates to the original handover. The return check verifies that the engine’s stored destination matches the captured position. Both corrected runs used different standing coordinates and returned inside the tavern.

**Return and campaign checks.** The hook restores the five captured flag arrays immediately before the first matching return call to `MachiMain`. It checks the session’s game, system and field owner identities, the load epoch, the matched war chapter’s normal completion and teardown, mode 1, step 48, an absent event owner, and zero intervening save calls. The hook compares native serialized game, war, character and party data and logs changed paths. An unexpected difference disables further training in that process. The rollback covers the five flag arrays. The other serialized objects are compared for diagnosis.

## Native sequence observed

| Boundary | Field step | Native result | Game mode | Manager return | Event owner |
|---|---:|---:|---:|---:|---|
| Stock war command finishes | 45 | — | 1 | 0 | Present |
| Field handover | 46 → 47 | 0 | 1 | 0 | Cleared at 47 |
| `MachiMain` and `MapChapter.Update` hand over | 48 | 2 | 1 | 0 | Absent |
| `WarChapter.Start` | 48 | — | 5 | 2 | Absent |
| `WarChapter.Update` finishes normally | 48 | 1 | 5 | 0 | Absent |
| `WarChapter.OnDestroy` completes | 48 | — | 1 | 1 | Absent |
| First returning `MachiMain` | 48 → 49 | 0 | 1 | 1 | Absent |

The stock trace recorded teardown and the first returning field call in the same frame. Inspection confirmed that step 48 reads event flag `0xBA` with mask `0x04` before moving to 49, which is why the restore runs in a prefix. The stock observer did not record the `MACHICON` identity itself. The training hook checked that identity and accepted both corrected returns.

## What the checks cover

| Check | Result |
|---|---|
| Stock field → War 1 → field | Observed in owner play and native trace |
| Guild → playable War 1 → walkable tavern | Verified by the owner on two corrected runs |
| Repeat entry in the same game session | Verified |
| Five flag arrays match after return | Logged on both corrected runs |
| Saving during the training round trip | Zero `SaveFile` calls logged on both runs |
| Game, war, character and party data at the return boundary | Only expected destination coordinates and native clock fields changed in the logged comparison |
| Story effects after the return boundary | Separate check pending |
| Defeat and any Game Over routing | Pending. The current candidate handles the observed normal ending |
| Manual save and reload after training | Pending |
| Production enablement | Experimental option off by default. Tested in the disposable runtime only |

The comparison happens before the first returning `MachiMain` body. It covers the serialized fields at that point. Post-return events, a separately verified casualty scenario, and save/reload behavior require their own observations.

## Build and project record

- Tested DLL label `1.29.0-training.2`
- SHA-256 `5920CE9FF9C856FA6B35104BD6D7915445AE4D7F135BA6C3478817F7664539CD`
- The corrected build compiled with zero warnings and errors and loaded in the disposable copy. Managed regressions passed on the preceding `training.1` build. The spawn correction was verified through the two owner runs.
- The installed Steam build remains the v1.26 campaign candidate. The v1.29 work is local and the private beta is still unpublished.
- Added `TrainingWar.cs` and `StockWarTransitionTrace.cs`. Updated `CastleGuildRuntime.cs`, `HuntGuildOverlay.cs`, `Settings.cs`, `Plugin.cs`, `NightmareRebalance.csproj`, `WarBoundaryRules.cs`, `OverhaulRuntime.cs`, `NativeWarRemix.cs`, `War2Runtime.cs`, `TotalWarRuntime.cs`, and `WarTacticsRuntime.cs`. This report replaces the earlier observation notes in the same file.
