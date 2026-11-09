# Luca battle context and integration decision

**NO-GO for further isolated Luca integration experiments.** Keep the v1.23
dispatcher evidence and v1.24 queue ownership checks as references. Resume Luca
only when a legitimate, protected battle session can supply the missing evidence.
In the meantime, prioritize improvements to encounters already supported by the
live RPG pipeline.

## Original ownership chain

- **Scene and setup.** The original `BattleChapter.Start` coroutine obtains a
  Unity `Field`, creates the manager with the chapter's game-function callback
  and awaits `BattleManager.Init(field)`. The manager coroutine runs battle data
  loading and setup, effect loading and main initialization, then passes its
  `BATTLE_WORK` to `Field.Setup`. That setup binds cameras and uses scene
  components and hierarchy state. A raw manager or an allocated field reference
  cannot demonstrate those bindings.
- **Action queue.** The manager's `BattleMakeData` initializes the read table,
  chooses actors from the battle-local player and monster arrays, builds actions
  and reactions, checks casualties and sets battle-local outcome flags. It
  assigns the queue head to the action cursor before invoking its own
  `Field.CalcCameraData`. The camera routine uses the work bound by field setup,
  reads the actor and next node and advances the cursor while calculating camera
  data. The manager tail installs continuation delegates. The v1.24 probe
  verified separate reader and queue operations on scratch work. It did not
  execute this lifecycle.
- **Script boundary.** Native monster dispatch checks the separate command
  branch before discretionary attack selection. The manager owns the scheduling
  and later consumes the queue. The v1.23 isolated override changed genuinely
  committed solo-Luca attacks, but did not prove a production interception can
  distinguish the exact scene-owned invocation or preserve the whole scripted
  handoff. The first two Luca encounters also use distinct outcome callbacks
  and write the HP consumed by subsequent openings.
- **Teardown.** The original `BattleChapter.OnDestroy` destroys the battle UI
  and calls `BattleManager.Release`. That routine unloads addressable resources,
  destroys scene objects, calls `BATTLE_WORK.PreRelease` and invokes a release
  timing bucket. This is a scene teardown path. The scratch probe's unlinking of
  owned action nodes does not establish cleanup of that resource domain, field,
  or continuation delegates.

## What the existing Hunt evidence supports

The historical v1.12.2 accepted hunt shows legitimate walking entry, two
player-earned victories, native rewards and ordinary field return. The runtime
witness uses walking encounter provenance, battle and party ownership, reward
observations and a subsequent map update. It does not capture the field's work
backpointer, camera traversal, continuation identities or their disposal.
Walking-hunt provenance cannot authorize a scripted Luca encounter. The
isolated startup guard is a launcher-only boundary, so it cannot provide a
genuinely loaded campaign battle. Neither mechanism was extended or rerun.

## Feasibility

| Question | Decision |
| --- | --- |
| Can we build and clean up a scene-owned context? | The original game has an identified setup and teardown path. Safe independent ownership and repeat teardown remain unverified. |
| Can we distinguish scripted actions from discretionary choices? | The native branch is identified statically. Production interception in the real solo encounter remains unverified. |
| Is an experimental option safe to expose? | No. The existing live behavior and configuration surface are preserved. |
| What evidence would change the decision? | A legitimately entered protected solo Luca session with read-only observations of chapter, manager, field and work identity, command path, queue and camera consumption, continuation installation and release, and original story return. Any later policy test must also compare legal attacks and confirm the three-encounter HP handoff. |

This milestone used read-only local binary and source inspection. It ran no
new native fixture, startup smoke or gameplay session. There is no new DLL.
The v1.24 baseline was rehashed as `6543164A053D3EEFD0BBA4F60B93805472CF51E1C1AC50DA4D351AE8FBC7D427`.
