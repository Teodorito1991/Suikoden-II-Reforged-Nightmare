# v1.14 Native War Skirmish Bridge

Version: `1.14.0-native-war-bridge.1`
DLL SHA256: `EA5BA3832CA141D48E01966E65B4D9EFBC8D784F547530D944E554EF1B20F1B8`
Verification date: 8 November 2026. Development candidate, not a player release.

## Result

Implemented and executed a real resource-acquisition / cleanup adapter for the
original GSD2 engine. This is one missing lifecycle component, not an independent
war session. Live skirmish launching remains unconditionally unavailable.

`NativeWarReleaseDomain` owns a separate native ReleaseCheck dictionary. It
registers explicitly acquired ResourceLoader instances through the original
AddLoader function. Its synchronous close operation calls the original GSD2
WarChapter.OnDestroy on a detached scratch chapter. That method invokes timing 4;
only the private dictionary is visible during that bounded call. A finally block
restores the previous dictionary. No Start, Update, chapter transition or story
interpreter is called on the scratch chapter.

The temporary dictionary is never left installed across a yield or frame. Nested
entry and other threads are rejected before native access. Duplicate registration
is rejected; close is exactly once. A failed scope is poisoned and cannot be
reused. An uncertain teardown is not retried just because a handle is still valid.
This adapter remains restricted to explicitly requested unloaded batch probes.

## Actual original-engine evidence

Two separate processes executed the final exact DLL, each completing two cycles:

1. Load the original castle tavern MACHIDAT through ResourceLoader; require the
   existing full Castle Guild source signature.
2. Load original War 1 OPD_RES2 (`war_01`): one map record and 19 sprite records.
   The original War 1 map mapping is 1. No unit or event initialization is implied.
3. Put the retained castle loader into a disposable timing-4 ledger. Register
   the war loader separately in its own private timing-4 domain.
4. Execute the native WarChapter cleanup method. Its original ReleaseInfo.Invoke
   calls ResourceLoader.Unload. Observe the exclusive war handle become invalid.
5. Verify the retained castle handle and its colliding timing-4 ledger survive;
   verify the actual launcher's original release dictionary and entries are
   unchanged. Release the castle acquisition separately after both cycles.
6. Reacquire the war resource and perform the same cleanup again.

Exception restoration, poisoned-domain rejection, nested ownership, duplicate
registration, foreign-thread rejection and repeated-close behavior also passed.
These are genuine engine resource operations. The retained castle asset is a
resource sentinel, NOT an active field scene. Handle release is not proof that
all war UI objects, sound banks, textures or global caches have been reclaimed.

The 45 checks in each final bridge process are secondary to the observed handle
and native cleanup behavior. No earned victory, tactical turn, rendered battle,
live field-control return or second tactical entry has occurred.

## Entry and continuation boundaries

The original ECWarEventGo is a multi-stage event command. It reads command-line
bytes, configures destination context, invokes EventGameWorkSet and requests a
mode change. Calling it with a changed war number would consume story context;
we did not do so. A forged EVENTCON or command stream is not part of this design.

The implemented entry portion stops at audited asset acquisition into an owned
resource domain. A legitimate mod-owned chapter/session continuation is still
missing. The existing read-only field lease is only a prerequisite and has not
been promoted to a live return ticket.

Framework return requests recreate a chapter type rather than resume a suspended
field stack. Returning must account for native MapChapter menu recreation and
MACHI ownership, player input, rendering hierarchy, filters and sound. Keeping a
MACHIDAT handle alive alone does not retain these objects or prove controls work.

## Campaign isolation audit and remaining blockers

The v1.13 five-array flag fork and native finish-write rehearsals remain intact
and passed in this DLL's native regressions. They are not a full GAME_WORK clone.
War 1 completion writes also exist in the chapter update branch; isolating only
w_setflg is insufficient. No ending branch has been executed as an earned win.

Bounded inspection reconfirmed that w_dead.CheckDead calls native RNG, reads unit
characters and casualty eligibility/dialogue, and stages casualty state. Its
complete downstream persistent-write reachability is still unresolved. Generic
interpreter character-write primitives do not prove a particular War 1 script
executes them. We have not certified all recruitment, death or defeat effects.

WarChapter resource initialization starts asynchronous resource/UI operations and
uses shared OPD caches and release registrations. w_main.warinit also initializes
field drawing and loads sound bank 61. These operations are outside the verified
synchronous domain. Automatically attributing every timing-4 registration to a
skirmish would recreate the original shared-ownership defect.

MACHICON autosave checks and native save paths must be fenced before any field
transition. No campaign save, casualty application, recruitment, story-ending
callback, native entry command or chapter-manager request was invoked by the new
bridge experiment. The separate private harness redirected Steam's save root to
an independent runtime copy and rejected campaign loads. This is test isolation,
not a demonstrated full-battle campaign sandbox.

## Smallest credible next experiment

Inventory and own the nested WarChapter/WarResLoad coroutine registrations and
UI/cache/audio mutations, advancing only the prerequisite resource stages in a
disposable unloaded context. Require a complete per-operation cleanup ledger and
reject any unowned registration. Do not advance into warinit or chapter dispatch
until those additional lifetimes and callback writes are accounted for.

Then establish a genuine idle-field continuation that can rebuild original map
menus/resources without resuming event bytecode or autosaving. Only after the
write boundary and retained-field ownership are proven should a protected full
War 1 transition be considered. A later live test must show tactical turns,
earned victory AND defeat, original-state preservation, normal field controls and
second entry. No human session is requested for v1.14.

Commander attack/defence application remains a verified future seam. Commander
XP, persistent army training, campaign identity and progression awards stay off.

## Verification and preservation

- Clean plugin compilation: zero warnings/errors; DebugType=none; no personal
  CodeView PDB path in this new binary.
- All 703,781 retained managed checks passed, including publication-default checks.
- Two exact-hash bridge processes: two resource cycles per process.
- Two exact-hash full production-native regressions passed: 3,649 production
  asset checks and 7,919 maintenance checks each, including 560 Normal/Hard
  admissions and native selectors. Second-process diagnostic history passed.
- An initial full fixture run with proposed public defaults rejected major-boss
  admission because EnableMajorBosses=false. The same fixture passed with the
  preserved development opt-ins in the COPY. No source guard was weakened and no
  original or public configuration was changed to make the fixture pass.
- Native/component tests do not certify rendered battles, full Hunt gameplay on
  this DLL, boss transitions, campaign balance or live field return.
- All 37 protected installed files remained byte-identical: 14 saves, 22 configs
  and original installed v1.13 DLL. Game-specific registry preferences restored
  byte-identically. Test processes exited through normal Application.Quit.

The installed v1.13 DLL and accepted v1.12.2 rollback are preserved. The new DLL
is a separate local development artifact. No player-facing release asset was
created or replaced; the existing private beta remains an unpublished draft.
Original RPG, hunts, Guild UI, balance and story-war source/configurations are
unchanged except plugin version/probe scheduling and diagnostic version labels.
The custom Unity arena remains retired.

Compiled from clean parent commit `929fb1734b7048f3eca73e724d00f6d1d1f24a83` plus
the v1.14 implementation tree. No rebuild solely to stamp a later Git commit.
See [sanitized verification](../publication/development-build-v114.json).
Raw native listings, runtime assets, helper, logs and original saves stay private
and are not included in GitHub history.
