# Current result — v1.53 acceptance passed

One Commander Rout Standard victory, native castle return, real autosave readback
and reload passed on the exact recorded DLL. No second victory or intentional
defeat is required. [Complete results and limits](ARMY-TRAINING-RESULTS-V153.md).

The implementation handoff below is preserved as historical pre-acceptance evidence;
its pending status and extra proposed runs do not describe the current result.

---

# Army Training v1.53 — simplified save contract, owner playtest pending

**Normal-build training remains default OFF until the owner playtest below passes.**
Development build `1.53.0-army-training.1`, SHA256
`54144F2F61644F6B632F4513B2A4F30C240033AFD68F6F80CE1453051F7BA286`.
Starting source: `d182297`. Preserved fallback: `1.52.0-army-legacy.1`.

## What changed from v1.52

v1.52 kept the save fence up after the return and re-verified every later save
by serializing and comparing all campaign records. That bought nothing (the live
state already equals the verified restored state) and could lock saving silently
if a legitimate castle map-init event changed a record. v1.53:

1. **Lifts the fence at the verified return.** The return prefix restores the
   original flag arrays, checks records, credits Potch and then retires save
   protection. `RestoredState`, `PayloadMatches` and the idle-tick retirement
   are removed. `TrainingPersistenceRules` keeps only the campaign path filter.
2. **Tells the player when saving is locked.** Any fault keeps the fence up and
   shows "Training could not be verified. Saving is disabled until you load a
   save." on the next idle field tick, on every refused save and in the Guild.
   The per-frame warning in v1.52 is gone; refusals log once per session.
3. **Restores original flags on abnormal teardown.** A mismatched `WarChapter`
   teardown now reinstalls the original arrays before faulting. If anything
   wrote past the private fork, the snapshot values are copied back as well.
4. **No permanent deaths in training.** A postfix on `w_dead.CheckDead` turns a
   staged death (`war_sys.sibou_flag` 2) into the native retreat (1). Verified in
   the native code: both branches stage the same unit and member, and the return
   value does not depend on the staged type. Story wars are unaffected.
5. **Defeat return is optional.** `EnableTrainingNormalBuild` no longer requires
   `EnableTrainingDefeatReturn`. Without it a training defeat is an ordinary
   Game Over; the fence stays up and the next load clears it (disk untouched).

A native load still retires everything. `LoadDataCore` assigns freshly
deserialized records and flag arrays to `GAME_WORK` (verified in the native
code), so new owners prove a real load.

## Evidence

| Check | Result |
|---|---|
| Release build against local interop | 0 warnings, 0 errors |
| Managed regression suite | 38 groups pass, including new casualty rule checks |
| Earned drill, return, autosave, reload | **Not run** — owner playtest below |

## Owner playtest (one session, your own copy)

The previous blocker was the coding agent's automated copy/launch policy. You
playing the drill yourself is ordinary testing.

1. Copy your `Save` folder to a backup in Explorer. Turn off Steam Cloud for the
   game (Steam → game Properties → General). Install this DLL.
2. In the config set `EnableTrainingWar`, `EnableTrainingNormalBuild` and
   `EnableTrainingChallenge` to `true`. Leave `EnableTrainingDefeatReturn` off.
3. Load a headquarters save with War 1 complete. Guild → Army Training →
   Commander Rout → Standard. Win by routing the commander.
4. On return: walk, open and close the menu, let the castle autosave run.
5. Quit to title, Continue that autosave. Check position, party and Potch.
6. Start a second drill and return again.
7. Optional: lose a drill on purpose. Expect Game Over; Continue should load
   your last save normally.

Pass if every step works and the log shows `verified return; campaign saving
restored` with no `campaign saving locked` line. Send the BepInEx log if any
step fails.
