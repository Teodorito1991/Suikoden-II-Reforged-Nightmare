# v1.16 Native War UI Ownership and Feasibility

Development version: `1.16.0-native-war-ui.1`

DLL SHA256: `1CA1F3F56200561B49EDCADCCF1295989716F0FB7D3AA4A7D077CC27261893C1`

Verified 8 November 2026. Separate development artifact only; installed v1.13,
original saves/configuration and the private beta remain unchanged.

## Concrete result

The original `WarHDHeader.LoadInstance(7)` factory initialized the actual tactical
header prefab in a copied, unloaded game runtime. Two independent processes each
completed creation, original component cleanup and reacquisition: four cycles.
Each cycle created eleven native tweens and three per-instance font materials.
Original component destruction removed those resources and restored the native
font callback registry. The factory's own asset acquisition and the separate audit
reference were balanced. Root 7, original WarRes references, shared font/material
and the independently retained castle resource survived unchanged.

This is genuine original-engine object/component execution, not a mock. The owned
Canvas was deliberately disabled. It is NOT visible tactical UI acceptance, a
complete WarRes.Load, an active WarChapter, tactical combat or field-return proof.

## Initialization and ownership boundary

WarRes.Load assembles war data, texture/map caches, tactical header, tips, command
UI, effects and text, publishing shared references. This milestone isolates its
header dependency before attempting the chapter-wide loader.

The verified header factory resolves hierarchy root 7, loads the original header,
instantiates and parents it, then releases its own loader reference before returning
the instance. It completed without an active chapter or an EVENTCON interpreter.
Original language-font initialization is required. The full tactical controller,
effect manager, cameras and chapter-specific initialization are not covered.

NativeWarUiLease borrows the existing hierarchy entries in a private dictionary and
substitutes only root 7 with an owned GameObject. The native dictionary is swapped
only during synchronous factory steps and restored in finally; it is never left
swapped across an asynchronous wait. Other entries are borrowed, never destroyed.
An object reference does not confer ownership of its fonts, sprites or materials.

Preflight admits only the audited prefab component types. Foreign animation/text
targets, autoplay, persistent events, preinitialized font materials or unknown
components stop the experiment before factory execution. Runtime ownership checks
then verify the generated instance, targets and new font materials.

Ownership categories distinguish acquired references, new session instances,
shared game assets, retained field assets and unknown/conflicting identities.
Unknown ownership blocks cleanup/transition. Instance destruction rights do not
allow releasing an asset reference or its dependency tree.

Cleanup uses Unity's original Object.Destroy primitive on the owned root, invoking
original DOTweenAnimation and UIFontBehabiour callbacks. The latter unregisters
its text callbacks and active registry entry and destroys its private materials.
Captured tweens become inactive. The audit acquisition is released separately via
the existing native timing-4 release domain. No global tween cleanup is used.

The global WarRes.DeleteClassicWarTexture routine was NOT called: it can destroy
shared header/tips/text state whose complete ownership has not been established.
Failure paths terminate the disposable test normally rather than attempting an
unclassified global cleanup; this adapter is not enabled in live field gameplay.

## Verification and limitations

- Build: zero warnings/errors; no private CodeView build path in the artifact.
- Managed regression suite: 703,862 checks passed, including instance rights.
- Exact-hash native UI: two processes, two cycles each, original factory and cleanup.
- Retained native OPD coroutine and direct resource bridge: reacquisition passed.
- Two production native runs retained combat/maintenance regressions; second run
  verified cross-process history. These are component probes, not rendered battles.
- No Unity error/exception was observed during the bounded UI lifecycle itself.
- One TMP_SubMeshUI.UpdateMaterial NullReferenceException occurred during shutdown.
  A separate graphics-enabled, exact-hash launcher baseline without header loading
  or probes reproduced the same exception. It is documented, not suppressed or
  fixed. The complete application is therefore NOT claimed error-free.
- All 37 original protected files and restored game registry export match baseline.
  No original save was loaded/written. Processes exited through Application.Quit.

Sanitized evidence: [development-build-v116.json](../publication/development-build-v116.json).
Historical Castle Hunt acceptance belongs to the earlier v1.12.2 hash; there was
no human gameplay acceptance, battle, earned war outcome or active field return
on this development DLL. No claim that all engine resources or writes are isolated.

## Independent-skirmish feasibility decision

| Question | Established evidence | Remaining boundary / assessment |
|---|---|---|
| Entry without campaign bytecode? | Original OPD sub-coroutine and header factory execute without EVENTCON or an active chapter. | Full WarChapter initialization is unproven. A mod-owned context is plausible; modifying war_no or fabricating an interpreter is insufficient. |
| Scene/render/audio dependencies? | Owned header root, font and animation cleanup are repeatable. | War2UIHD command/input ownership, UITips/WarUIText, shared WarRes fields, effect dictionaries/cameras, field drawing/fades and sound bank 61 remain. Audio loading can release existing banks; copying a list cannot restore released sound. |
| Campaign writes and casualties? | Prior scratch flag forks prove selected native writes can be isolated. | Finish helpers, inlined ending writes, character initialization, casualties, full mutable GAME_WORK/static graph and autosave closure remain incomplete. Blocking saves or restoring a few flags is insufficient. |
| Return to MapChapter? | Framework transitions recreate chapter types; RequestBack is not a suspended session stack. | Map start rebuilds menus and field update can run events/autosave. Retained MACHIDAT alone proves neither controls nor safe continuation. No return is demonstrated. |
| Smallest integrated experiment? | OPD and header ownership adapters now exist. | Combine them under one disposable session owner with controlled abort/cleanup and repeat entry, while proving the field/chapter ownership envelope and War1 initialization write boundary before calling chapter Start or tactical update. |
| Safer original-engine architecture? | Native mechanics can be reused in bounded factories. | Retaining an idle genuine MapChapter under a mod-owned session host may avoid destructive chapter replacement and story-bytecode continuation, but pausing its execution/render/audio safely is still a hypothesis. Original story-war remixes remain the safe shipped alternative. |

## Practical next milestone

Build a single integrated session-owner and abort/return envelope, rather than
more disconnected prefab experiments. Inventory and guard the mutable writes of
War1 initialization, determine whether a real field can be retained without its
event/update loop running, and combine the already verified OPD/header owners.
Set a bounded go/no-go before any active WarChapter execution: if field ownership,
audio restoration or campaign-write closure cannot be established, keep independent
skirmishes deferred and focus release work on the working RPG and story-war overhaul.

A playable skirmish still requires earned combat, safe return and genuine second
entry. None has occurred. The launcher remains unconditionally disabled; no XP,
commander training, persistence, rewards or new gameplay menus were introduced.
