# v1.51.1 — Full Content integration

Development build `1.51.1-feature-completion.1`, SHA256 `DB2EA401C0F3C0DCC30E4F079AF8A29A2D5C8B70A31E42ABAE68E75998282AE9`.
No Steam installation, original-save change, new campaign or beta publication.

## What changed

Full Content Preview now selects all22 supported Original Hall definitions and
all10 Hunts, alongside the existing RPG and original story-war changes. It adds
Original Luca and enables Sierra, second Harpy and first Worm implementations
previously disabled solely because their story unlocks were unresolved. Their
story locks remain: use the player-facing Spoiler Mode for those three memories.
This is not an invented completion witness. Unimplemented/invulnerable/mirrored
story records are not added to the catalogue.

Beast Rune remains a clearly disclosed late-game Bonus Challenge after the audited
Gorudo milestone, not proof of an individually defeated final boss. It is now
selected in the preview following victory-path verification described below.

Original Luca is retained without refactoring. Its accepted DLL
`1.51.0-last-challenge.1` / SHA256
`7D368B2E115082BEDA83AF7F2590E733114512C73A49487619F64A496705534B`
is preserved separately. The three-team implementation files are unchanged from
commit `d785831c0ddcb70eedaddcf8e3d89e07af2ee7e5`. No new full Luca win
was requested. Defeat/interruption, Nightmare and a second full run remain
unverified; preview opt-in is not blanket acceptance of those paths.

## Spoiler Mode

Default OFF, with the existing major-spoiler warning, Cancel default and explicit
second confirmation. Restore Story Progression reverses the CFG preference.
Only story availability is bypassed. Per-entry disablement, native source,
compatibility, scene and resource guards remain. Disabled-entry feedback now
explains that Spoiler Mode cannot enable disabled content; unresolved story gates
explain that supported encounters can be revealed through Spoiler Mode.
Existing user settings are never overwritten by the profile generator.
Nightmare remains a separate optional overlay and per-launch choice.

## Beast Rune evidence on this exact DLL

The original multipart actors, animations, attacks, native phase controller and
extra-texture loader ran in the copied runtime. An initial ordinary-party attempt
did not establish victory acceptance. The owner then explicitly requested a
boosted test party. Only the disposable checkpoint was edited: existing party
members received level99, HP999, stats255, weapon16 and nine charges per spell
level. Equipped runes/gear, inventory, campaign records and flags were preserved.
There was no forced victory, damage protection, modified boss or synthetic result.

The owner won through native combat with that boosted party, completed the result
screens and confirmed Guild return/reopening. Production evidence recorded native
WinEnd, terminal result1, correct Guild position, all five protected flag arrays
unchanged, unchanged war and selected progression snapshots, and completed owned
cleanup. Observed purse credit and tracked item-slot deltas were zero. Do not
claim a demonstrated reward receipt or normal-party balance from this run.
The copied manual checkpoint was restored to its pre-boost data and campaign
entry disarmed. The boosted checkpoint, original assets and raw traces remain private.

This verifies a real victory/return mechanism with altered party statistics. It
is explicitly not legitimate-level campaign balance acceptance, Nightmare or
repeat-run verification. No further Beast victory was requested after success.

## Army Training decision

Keep normal-build training OFF in both campaign profiles. The archived drills,
Veteran behavior, rewards and historical player-run outcomes are preserved.
The remaining boundary is the v1.51 direct SystemSave.Save contract: it must be
observed with a genuine current-build training outcome, restored-state autosave
payload, and refusal of transient/faulted writes. The current pass provides no
such new outcome/fault-path acceptance. Component/source checks alone do not
justify enabling it. No broad restore framework or commander XP was added.
Persistent commander XP remains deferred until trustworthy campaign ownership
and a native or versioned persistence mechanism exist.

## Checks and practical finish line

Release build: no warnings/errors, sanitized DLL, accepted Luca fallback unchanged.
The separate-desktop preview captured33 screens; representative Services and Hall
pages were visually reviewed as readable. These are launcher presentation previews,
not gameplay acceptance or proof that every screen is visually flawless.
Managed regressions, generated fresh/full configuration checks and the existing
original-engine source/copy/setup audit passed. The source guards cover24 boss
tables for22 definitions, not24 player-earned victories. All prior RPG, character,
rune, Unite, Hunt and story-war implementations remain; no balance multipliers,
new bosses or duplicate records were added in this patch.

The planned RPG/Hunt/Original Hall content is substantially implemented and
available in one preview configuration. Full-campaign balance/stability, unplayed
Hunt/echo variants and the optional training save contract remain genuine limits.
The owner decides when to begin a fresh campaign; this task did not start it.
