# Luca Blight manager ownership

Development build `1.24.0-luca-manager-ownership.1` with SHA256 `6543164A053D3EEFD0BBA4F60B93805472CF51E1C1AC50DA4D351AE8FBC7D427`.

## Verified native prerequisites

The isolated audit admitted the original solo Luca record using the retained
roster, callback and source-provenance checks. A private native reader bound to
the scratch manager initialized an 18-slot read table through `InitBRead`.
The actual `InitBReadTable` routine used by `BattleMakeData` cleared the same
table after the probe seeded its first and last entries.

The native `BattleManager.CreateActionData` published a queue head, and
`PM_DATA.GetActionData` appended a separate node to the same scratch work.
The probe checked head, cursor, link and actor identity. It allocated two native
reaction entries on the tail and verified that `DeleteActionData` cleared its
reaction references without affecting the head. After unlinking the owned nodes,
a second queue cycle passed. These are allocation and cleanup checks. The
synthetic queue nodes carry no attack result. Both cycles preserved the original
reader singleton and RNG state.

The retained v1.23 monster-dispatch probe ran separately, including the original
column decision, controlled basic and row choices, wounded and defensive parties,
silence retries and the no-target case. Those decisions still enter below the
manager. The original Luca slots are unlimited. No limited-use exhaustion or
manager-level scripted-command test is claimed.

## Full dispatch boundary

Static inspection puts `InitBReadTable` at the start of `BattleMakeData`.
The manager recalculates actor parameters and selects the next actor. It runs
player or monster action construction, updates casualty checks and sets battle
result flags. The native monster dispatcher has a separate command path before
discretionary selection. Observing a selector call or an empty queue is
insufficient to establish ownership of an entire manager cycle.

After action construction the manager copies the queue head to the action
cursor and calls its own `Field.CalcCameraData`. The field must be bound to
the same `BATTLE_WORK`. The camera routine traverses the action chain and
dispatches to actor-specific camera calculations. The manager tail also writes
battle-local flags and installs continuation delegates. The unloaded fixture has
a private work object and manager but no verified scene-owned field or teardown
for that continuation. Calling the entire routine would cross this boundary,
even if a valid action was already queued. No camera bypass, fake scene owner,
script callback invocation or battle result fabrication was used.

Live tactics remain unavailable. The next required experiment is a disposable
scene-owned field and manager pair, with the camera bound to the scratch queue
and ownership of the new battle-step delegates verified through cleanup. Only
then can the full original routine establish scripted-command precedence and
compare normal versus controlled manager-owned action construction.

Both isolated runs passed the new prerequisite checks and retained native
regressions. Managed tests passed. Installed DLL, saves, settings and preferences
matched their pre-test bytes. No animated attack or campaign result was observed.
