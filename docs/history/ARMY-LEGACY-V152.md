# v1.52 — Army Legacy: save-contract fixes and completion decision

Development `1.52.0-army-legacy.1`, SHA256
`0840237D7ECB736BABADBD0C35F105954B1447AB757883ADB66D8099F6238895`.
Base source: `e085f51751a2b48ebefa3c94bc55db29d5069d2e`.

## What is finished

The existing four native drills, their audited Standard/Veteran choices and their
750/1,500-Potch qualifying victory rewards remain implemented. This patch changes
the training save boundary only. It does not rebuild war transitions, alter battle
outcomes or add any commander statistics. Normal story-war logic remains original
plus the previously configured overhaul layers.

Two concrete defects were corrected. First, rejecting SystemSave.Save returned
false but never completed its callback. UISaveLoad2.Save and GSD2SaveData.SaveFile
also need a failure completion when their intent is refused. Their callers now
receive false exactly once, without proceeding to encryption/file writing.
Second, a rejected SaveFile attempt incremented the session's SaveCalls count.
That made the valid return boundary fail even though no write occurred. Refused
intent now leaves that count unchanged. Legacy allowed intents retain their witness.

Ownership is armed before publishing private training flag arrays. Exceptions
while checking restored live owners latch the existing fault restriction instead
of escaping the save hook. Payload checks compare all five protected flag arrays
and game_data, war_data, chara_data and party_data with restored live records.
Missing records, changed values, malformed JSON and duplicate property names are
refused. JSON object property order has no effect. No save format is changed.

## Save boundary

1. Only an admitted normal-contract drill owns private flag arrays and the fence.
2. While preparing/running or faulted, campaign save intent and direct campaign
   SystemSave writes are refused. Turning the option off cannot discard ownership.
3. A native return must match the current session, original field, war teardown,
   flags, destination and protected records before establishing restored state.
4. The real direct autosave may proceed only with matching restored payload data.
   The existing verified idle-field continuation retires the protection; a genuine
   fresh load can also retire the old owner without restoring old arrays over it.
5. Other games, shared settings and ordinary unowned campaign saves are untouched.

There is no global save ban and no broad campaign snapshot overwrite. On a fault,
reload/restart is required; the feature does not silently bless transient records.
Actual disk completion, restored autosave and interrupted-return behavior must be
observed before recommending this contract in a normal campaign.

## Evidence and its limits

| Evidence | Result | What it does not prove |
|---|---|---|
| Deterministic Release build and retained managed suite | Passed | Full campaign |
| Original-engine production asset/setup audit | 5,071 checks passed | A played drill or Hall encounter |
| Synthetic-session native save-entry fixture | 12 checks passed | Native return ownership after a genuine outcome |
| Transient/faulted actual native save entry | Refused, one false callback; no campaign write | Natural in-game interruption |
| UISaveLoad2/SaveFile rejection | Completed failure; refused attempt count stayed zero | Successful autosave disk completion |
| Restored matching payload guard | Accepted by production rule; no fixture disk write | Actual restored campaign autosave |
| Current-build earned drill/return/autosave | Not observed | Must not be reported PASS |

The fixture temporarily owned scratch native records in an unloaded disposable
runtime, restored the singleton/configuration/RNG, and patched no extra gameplay
targets. It deliberately injected the fault; it did not earn a training outcome.
The production compatibility guard remained enabled. No new rendered UI or Hall
fight was needed because their implementation did not change.

A bounded owner Commander Rout check was prepared, but automatic approval review
rejected copied-campaign setup twice, including after explicit owner confirmation.
The only supplied reason was `blocked by policy`. The rejected commands did not
run. No alternate tool/route was used to evade the rejection. This is an execution
blocker, not evidence that the drill or autosave failed in gameplay.

## Persistent military progression: deferred, not invented

The native WAR_DATA record examined here contains leader/subcommander assignments,
not a commander XP field or an approved versioned mod extension. Adding arbitrary
properties would require proving their native serialization/load behavior and
branch semantics; this pass did not establish that contract. Undocumented spare
bytes, story flags, slot numbers, display names and transient file hashes are not
safe substitutes. No external campaign/branch identity has been demonstrated.

Therefore no persistent commander XP, specializations or story-war stat bonuses
are implemented. A temporary counter would not meet the requirement. Native Potch
remains the earned persistent reward for qualifying completed drills, with original
currency caps. Defeat, timer-only completion and invalid returns still pay nothing;
reporting/menu callbacks never create a second payout. Existing reward amounts and
eligibility were not changed or newly gameplay-certified in this patch.

## Final configuration and practical finish line

Full Content Preview still selects all22 supported Original Hall entries,10 Hunts,
RPG/Party Revolution/Star Bonds/rune/Unite and guarded original story-war systems.
StoryProgression remains selected; confirmed reversible Spoiler Mode defaults OFF.
Nightmare stays optional. Unsafe scripted switches and developer tools remain OFF.
Normal-build training remains default OFF in Fresh and Full profiles because the
real changed save boundary has not passed the bounded outcome/autosave check.
The existing independent opt-in remains experimental, not newly certified.

Original Luca and the boosted-party Beast Rune evidence retain their exact earlier
DLLs. No repeat boss campaign was requested. Accepted fallback binaries, Steam
installation, original saves and the unpublished beta remain unchanged.

No further content expansion is proposed. The RPG/Hunt/Original Hall roster is
substantially implemented. Persistent commander development is deferred and the
training save contract has one concrete acceptance gap. The owner decides when to
start the full Normal campaign; this assignment did not start it.
