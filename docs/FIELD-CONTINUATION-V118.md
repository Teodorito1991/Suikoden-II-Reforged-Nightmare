# v1.18 Real Field Continuation Decision

Development version: `1.18.0-field-observation.1`

DLL SHA256: `376F51AB416C9A785F18B12B6884B690224BC6BEE4DC1F32B8607CA4937AD691`

8 November 2026. Separate development artifact. Installed v1.13, original saves
and configuration, v1.17 artifact/source reference and private beta are preserved.

## Decision: NO-GO; pause independent skirmish development

The required genuine MapChapter baseline and reversible field continuation were
NOT demonstrated. There is no safe basis for executing NativeSkirmishSession under
a live field, starting WarChapter, or claiming a playable skirmish. The launcher
and field-retention authorization remain unconditionally disabled.

This milestone stops at passive instrumentation, using the task's explicit fallback.
It does not add another standalone resource experiment or repeat flag rehearsals.

## Exact observation bottleneck

The independent copied runtime has an audited PRIVATE startup guard. It redirects
the verified Steam installation-root getter into the copy, permits only shared
settings paths and explicitly rejects campaign/import reads and campaign writes.
It also blocks achievements. The original runtime and saves are never used.

That existing protection has NOT been verified for a normally loaded campaign.
There is no established unattended normal-menu route into a disposable loaded
field. Earlier keyboard automation was unreliable; no human session is required
or requested here. We did not weaken the startup guard, call load internals,
construct an EVENTCON, fabricate a MapChapter or change story state to get past it.
No campaign save was copied into or loaded by this test environment.

Consequently the genuine field baseline is unavailable in this milestone. This is
a concrete testing/ownership gap, not proof that retaining a field is impossible.
A new campaign-capable isolation harness would need an independently verified read
allowlist, complete write/cloud/achievement boundaries, immutable disposable inputs
and legitimate menu-driven load evidence before using any field observation.
The current startup-only harness must not be relabeled as such a harness.

## Implemented passive recorder

NativeFieldObserver is activated only by explicit private-test arguments, a private
runtime marker and both native/managed self-test switches OFF. Normal gameplay
has no new setting/menu and does not schedule this component.

It observes actual original MapChapter.Update prefix/postfix calls, sampling every
30 frames, only after active chapter identity and real GAME_WORK/SYS_WORK/MACHICON/
EVENTCON/MACHIDAT back-pointers match. It never invokes a field update itself.
The bounded ring retains at most 32 private snapshots, written every two seconds
under BepInEx diagnostics, never into game saves. No raw field data is published.

The field branch inventories verified native members:

- Active MapChapter, chapter manager and ChapterBase object identities; original
  chapter types, pending RequestType, coroutine reference, scene operation and
  chapter-changed callback, GameStep and input/pause flags.
- GAME_WORK/system/party/game-data identities, MACHI/event/data ownership, area/
  town/map/machine state and player coordinates.
- Interpreter mstep/sstep/load/initialization state; each COMMAND_LINE identity,
  compos/comst/cmdIdx, status, work owner and command-buffer identity/length.
  Bytecode contents and translated dialogue are not exported. No Current/Advance
  getter is called to inspect instructions.
- Original input manager/key table, menu/backlog/dialogue ownership and native
  hierarchy identities/activity/child counts; native night/menu filter flags.
- Native SE bank IDs, bank registry and sound-manager BGM/SE registry identities
  and counts. These are inventories, not complete audio playback ownership.
- now_save and auto_save_skip values; actual autosave-check results, high-level
  save intents, save-file calls, command advances, load and map-destruction events.
- Five original flag-array fingerprints, explicitly not the whole campaign state.

Hooks only count/observe and return void; they do not change native results,
suppress original calls, set pause/time flags, stop coroutines, consume bytecode,
create menus, load/release game resources or invoke NativeSkirmishSession.
Native capture exceptions disable the observation, never alter original execution.

The passive capture path remains deliberately INCOMPLETE: full render/filter state,
audio playback owners, coroutine contents/continuation and pending save completion
callbacks are unresolved even if registry references exist. A stable sample cannot
authorize a lease. NativeFieldObserver never suspends or resumes a field.

## What actually ran on this DLL

The exact final DLL loaded in the protected graphics-enabled startup runtime with
self-tests OFF. The observer component registered, its periodic diagnostic write
ran, and it accurately reported zero genuine field samples, no baseline, no field
suspension/session attempt/continuation and no observed save/event activity.
This is real launcher/runtime instrumentation execution, NOT field acceptance.
The field-capture branch has compiled but has NOT executed on a genuine field.
No idle field, native input restoration or menu/render/audio continuation was tested.

The retained managed regression suite plus 14 targeted passive-evidence comparison
checks passed. Synthetic missing/changed/unknown owners and even stable complete
records never enable retention. Existing RPG production native checks passed in an
unloaded isolated run; native-war self-tests stayed OFF, so no flag/casualty primitive
rehearsal or integrated resource session was rerun for this milestone.
Historical v1.17 integrated-session proof remains scoped to its original DLL hash.

The previously documented TMP_SubMeshUI shutdown exception and JobTempAlloc warning
remain isolated-runtime limitations where observed; no engine fix or global
application leak-free certification is claimed. All processes exit normally.

## Baseline coverage and missing evidence

| Required boundary | Current status | Missing evidence |
|---|---|---|
| Genuine idle MapChapter and interpreter | Instrumentation implemented; not observed | A legitimate protected normal load and real idle update samples. |
| Reversible field lease | NOT implemented or tested | Coordinated ownership of manager/coroutines, MACHI/EVENTCON and native input/presentation/audio, with bounded event and save effects. |
| No unsafe event progression | Counter/inventory hooks implemented; no live field evidence | Before/during/after command positions and native execution on an actual field. |
| Autosave containment | Startup guard denies campaign access; live-field policy unverified | Save eligibility, pending request/callback ownership, filesystem/cloud boundaries under a normally loaded campaign. Zero launcher calls are not proof. |
| Resource session under genuine field | NOT attempted | All preceding boundaries; v1.17 adapters still reject active MapChapter contexts. |
| Ordinary continuation | NOT observed | Preserved native execution context and normal input/menu/audio/field behavior after a bounded session. |

## Minimum campaign-isolation work before any tactical advance

Do not expand the five-array fork and call it a sandbox. A live war requires:

1. A closed list of mutable GAME_WORK/SYS_WORK/party/game/character records and war
   static owners reached by War1 initializer, battle/demo and ending paths, with
   shared versus session-owned aliases identified before any replacement.
2. Owned unit/character and casualty work plus native RNG state: casualty selection
   consumes RNG and maintains static work; permanent event-character writes are
   reached through DriveUnitDestroy. No live casualty path is authorized.
3. Initialization/render/effect/audio restoration. War initialization reaches field
   background setup and bank loading, which can release existing sound banks.
   Snapshotting registry pointers cannot reverse a destroyed field/audio resource.
4. All ending writes/callbacks, including the inlined WarChapter branch, and a
   campaign continuation boundary that does not resume campaign event bytecode.
5. Verified save/autosave/import/cloud/achievement isolation and async callback
   ownership. Blocking one SaveFile function cannot establish that whole boundary.

These are the remaining critical owner/write closures from prior native evidence,
not newly completed isolation work. No full mutable state graph exists yet.

## Practical recommendation

Pause independent skirmish development now. Preserve the working v1.17 resource
foundation and v1.18 passive recorder for future use. Do not schedule more prefab,
flag or resource milestones as a substitute for a real field experiment.

Reopen this work only after a campaign-capable disposable environment and a
legitimate protected field baseline are independently available. The first future
experiment must be observation only, then a precisely owned reversible boundary;
no tactical war until field continuation, save hazards and campaign-write closure
are evidenced. A blanket pause, stopping all coroutines, or reconstructing a field
from a retained asset would not meet that standard.

Focus current release work on the accepted Hunt Guild, RPG overhaul and original
story-war modifications. No new UI, XP/training, balance change or custom arena was
introduced. Full campaign stability remains experimental; no public release made.
