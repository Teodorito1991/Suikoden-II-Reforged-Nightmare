# Luca caller-scoped selection audit

Date: 2026-11-09. Documentation-only follow-up to v1.21-v1.25.
Baseline: 60efcbaca14d818e39f72696797561362fc671df.
No new DLL, gameplay configuration, hook, game launch, or campaign load.

## Decision

The narrow result-change architecture is technically credible on the supported
binary. Inspection found no selection-dependent persistent combat write in the
selector or its inspected range/use helpers that would require state repair.
This is static evidence, not live integration acceptance.

Do not reconstruct a manager, Field, camera, queue, or reaction records.
Native ownership is available through the original running game. Observe that
ownership before intervention. Live Luca tactics remain unavailable.

A correction to the earlier plan is essential: entering the selector does not
by itself prove the dispatch bypassed the actor-command branch. That branch can
fall through to selection when it does not succeed. Exclude the entire dispatch
when its entry command is 11, even if selection subsequently occurs. Unknown
commands or ambiguous identity also remain ineligible.

## Verification performed

Fresh SHA256 checks matched the installed native binary and metadata against
Plugin.SupportedBuild:
- GameAssembly: E093F175EAF3734D293CA0A236C652DD7FCC72EA5FA10D7B21103EDF12F11C3A
- Metadata: ABB6489CA1EF1728F9DE74F64A64D9C109F5DDD5BA9046226EA964B15B62F964

Fresh local disassembly was generated from that installed binary using the
existing private symbol index. Selector, dispatcher, range/use helpers, work
getter, RNG wrapper, and relevant SteamService storage methods were inspected.
Raw listings remain outside this repository.

The unchanged development DLL was rehashed:
1.25.0-hunt-catalogue.1,
AA27445761E1E7581FF2D32DCF5FA608E47A072F06A5B307B809A46557EA09B0.
This hash is a baseline, not new gameplay acceptance.

## Selector and dispatcher findings

SelectAttackMode allocates private candidate/weight arrays, reads native battle
state, filters enabled attacks through range/use checks and silence handling,
and returns a mode or a negative result. Inspected persistent writes are runtime
initialization bookkeeping; candidate data is local scratch. The range and use
helpers read eligibility rather than decrementing uses. The work getter resolves
the actor's manager relationship; it is not a global singleton lookup.

The selector calls get_rand on the candidate-selection path; the inspected
wrapper delegates to the native Unity integer RNG. There is no such draw on
the no-candidate early return. Preserve the original selector call and any
negative result. Do not assume one RNG draw for every selector invocation.

The dispatcher stores a nonnegative selector return into the roster actor's
c_mode before looking up and executing the selected attack's calculation.
Thus replacing the return does not require separately setting c_mode on this
path. Original calculation, targeting, use accounting, reactions and presentation
must remain native.

The actor counter and finished flag update before the command/selection split.
The command-11 path can produce its own action and return success, or fall
through into the selection loop. A boolean "inside monster_attack_sub" is not
adequate policy admission. Capture entry command from the authenticated roster
actor and retain the rejection for the entire invocation.

The loop permits ten selector attempts before its native basic fallback.
A failed construction or negative choice can cause another attempt. A selector
return, action allocation, and routine-entry diagnostic are not interchangeable
with native construction success, manager consumption, or finished playback.

A direct-call byte scan found the dispatcher call to SelectAttackMode. This
supports the identified boundary but is not an exhaustive indirect-call or
reentrancy proof. Overlays and foreign hooks can introduce nested execution.

## Reuse existing source boundaries

- CombatReforgedRuntime.cs: CombatModePatch already postprocesses the original
  selector result. It currently calls existing AI and then the isolated
  BossTacticsProbe observer. Extend this existing boundary only after gates pass;
  do not add a competing result patch or broaden existing boss admission.
- BossTacticsProbe.cs: activeDispatch is explicit scratch-fixture state. It is
  not a production invocation stack and must never be reused as live admission.
- CombatReforgedRuntime.cs: CombatActionScope/CombatTargetScope demonstrate
  Harmony prefix state and finalizer restoration of a prior thread-local scope.
- Plugin.cs: exact binary/metadata hashes gate patches. CheckCompatibility
  rejects known overlapping Harmony owners at its check points. This does not
  detect every native detour, later patch, or altered loaded-process instruction.
- Combat.cs: BattleState already records manager/work and is replaced at Begin
  and cleared at lifecycle boundaries. Its object identity can anchor a future
  encounter generation, after verifying those boundaries in a real battle.
- CoverageRuntime.Capture and BossTacticsRuntime.Source already inspect encounter,
  roster, source membership, callbacks and signatures. Reuse their relevant
  checks rather than inventing a second source database.
- MaintenanceRuntime already distinguishes routine entry from completed playback.
  Reuse its diagnostics; it does not currently supply dispatcher commitment.

The framework has the mechanisms required for managed prefix/postfix/finalizer
scoping. Reliable cleanup of a new dispatcher hook across actual IL2CPP/native
failures has not been executed here. A finalizer is not a recovery guarantee for
process termination, access violations or a native exception that never returns
through the managed wrapper.

## Proposed lifetime and admission contract (not implemented)

Maintain a thread-local stack of dispatch frames. Each frame contains a dispatch
sequence, parent, actor/manager/work identity, BattleState generation, entry
command, attempt count, and eligibility. Install an ineligible sentinel even for
an unrecognized nested dispatch, so it cannot inherit outer admission.

Validate actor.manager -> manager.work, manager/work against the current
BattleState, and work.pm_data[actor.chara_no] against the exact actor. Recheck
live membership before reading native state or considering substitution.
Do not authorize using Combat.Matches alone: its ordinary-battle Allowed flag
intentionally excludes scripted boss scenes. Luca needs separate observation
admission without widening existing gameplay/stat permissions.

On a future supported solo encounter, require original boss_luc3/source
boss_luc3_00, original roster/source/attack signatures and callback registry
bindings. Earlier encounters are explicit negatives. Use observed thread and
generation identity; reject unexpected threads, nested ambiguity, command 11
at entry or selection, unknown command classifications, stale state, or conflicts.
A finite positive allowlist of observed discretionary entry commands is safer
than treating every non-11 value as supported.

Prefix sets up the frame without suppressing the original. Postfix records the
returned dispatch result. Finalizer restores the exact prior frame and preserves
the original exception. Diagnostics failures must not replace engine failures.
Release/clear invalidates the encounter generation. Logs retain copied values,
not native wrappers for later dereference.

Initial active trial, only after observation acceptance: execute the original
selector exactly once; change at most the first eligible attempt in a dispatch;
preserve negative results and leave subsequent retries and fallback unchanged.
Return only an enabled legal original mode. Never call the dispatcher again
to obtain a preferred outcome or reset RNG. Keep mod RNG draws at zero;
separate native retry draws from native draws for different attacks.

Capture the pre-existing-postfix result and resulting value separately if the
observer is integrated with CombatModePatch. Existing AI postprocessing may
already affect ordinary enemies. Never label the final postfix value "vanilla"
unless native versus mod contributions have actually been separated.

## Safe-observation assessment

Campaign-capable isolation is NOT established by the existing copied runtime.

The private startup guard redirects SteamService.GetAppInstallDir, permits only
relative Save/share paths for its load/save hooks, rejects campaign/import
loads, suppresses achievements, and requests normal exit. It is explicitly
startup-only. Inspected native SteamService Save/Load/Delete use the install
root and local filesystem operations. A separate copy alone would therefore
not establish safe routing without the redirect.

This does not prove all campaign persistence paths are covered. The existing
launcher uses the normal Steam application identity. The guard does not isolate
PlayerPrefs or establish an external Steam Cloud boundary. Historical smoke
verification restored registry preferences; restoration is not prevention and
does not satisfy this task's no-installed-configuration-write requirement.

Do not run historical isolation/restoration scripts, widen the path allowlist,
disable Cloud globally, or copy a checkpoint into the installed game. No game
was launched and no guard or runtime configuration was changed in this audit.
Because the conditional safe-loading prerequisite was not met, no new passive
hook was installed merely to accumulate unexecutable infrastructure.

## Comparison with prior milestones

- v1.21 identifies sources and HP transfer; it does not authorize a live actor.
- v1.22 establishes retry/fallback structure. This inspection sharpens the
  command boundary by documenting possible command-to-selector fallthrough.
- v1.23 supports legal replacement producing native constructed actions, using
  synthetic inputs. It is not proof of caller scope, playback or story return.
- v1.24 tests scratch allocation/cleanup. No further reconstruction is needed
  to pursue a selector hook inside the original scene.
- v1.25 correctly requires legitimate lifecycle evidence. The proposed approach
  avoids custom ownership but does not remove provenance or lifecycle proof.

## One minimal next experiment

Qualify one ordinary menu load in an independently isolated disposable profile,
with the unchanged development DLL and all Luca/war probes disabled.

Preconditions: prove save, BepInEx configuration, preferences/registry, log and
Cloud boundaries before launch; use an existing legitimate save copy; confirm
no original namespace is writable through the test path. If no existing
supported profile/sandbox can provide this, stop and report that exact boundary.
Do not replace preconditions with a post-run restoration promise.

Control: pre-launch original-file/preferences manifests and the unchanged
startup-only guard baseline. Inputs: one disposable runtime/profile and one
legitimate checkpoint. Observe actual read/write destinations, normal menu-load
completion and normal exit. Pass only if the campaign loads normally, outputs
remain disposable, originals remain unchanged, and no external sync occurs.
Stop before load on unknown routing, preferences access or Cloud behavior.

This experiment changes the immediate decision from "no live observation route"
to "ordinary battle instrumentation can be exercised." It does not authorize
Luca tactics. Next would be a passive ordinary battle; actual Luca identity,
three-fight handoff and scripted continuation still require the legitimate
Luca sequence. No teleport, fabricated context, forced win or story edit.

## Limits and preserved behavior

No managed gameplay tests, native fixture, startup smoke or gameplay trial was
rerun for this documentation-only audit. Publication hygiene and remote CI are
recorded separately in the synchronization receipt. No gameplay version or tag
is warranted. Hunts and other gameplay source are unchanged. War work remains
paused, Luca tactics unavailable, the repository private and beta unpublished.
