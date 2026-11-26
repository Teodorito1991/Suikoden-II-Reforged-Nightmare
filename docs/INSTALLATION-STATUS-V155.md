# v1.55 installed — full gameplay playtest

Owner-authorized installation, 26 November 2026. No new gameplay version or DLL
rebuild. The existing supported Steam/BepInEx installation now uses exact
`1.55.0-story-bosses.1`, SHA256
`C5BE2F48118E205083096599F0C2B2285DF2CE4E2807DA26B5E3EB561497055E`.
Gameplay source remains `c46873505f3899230cbf84782873207887645fc8`.

Generated Full Content was deliberately applied, plus the separate
[full-playtest overlay](../profiles/full-content/full-playtest-opt-in.cfg).
This is the owner's requested experimental playtest selection, not a change to
the conservative generator defaults or a claim that every path is accepted.

## Active configuration

| System | Installed selection |
|---|---|
| Campaign difficulty | Choose **Suikoden II → New Game → Normal**. The campaign was not started. |
| Regular enemies | Profiles, enhanced AI/teamwork and all formation layers ON. Global fallback HP1.60/damage1.45; exact role profiles replace rather than multiply these factors. EXP/Potch1.00. |
| Party/combat | Character and spell balancing, RPG Evolution, Party Identity, Star Bonds, physical techniques and Unite improvements ON. |
| Rune recovery | One missing level1–2 charge per eligible surviving participant after victory; lowest depleted level first. No level3–4 victory refill. |
| Story bosses | All22 audited tactical policy records selected through the existing Boss Tactics/Revolution/Legendary dependencies. Normal/source/command/callback guards retained. Native campaign Luca and Beast Rune controllers preserved. |
| Hunt Guild | All10 contracts, automatic eligible walking entry, native items and report-back ON. Contracts remain session-only. |
| Hall | All22 supported Original definitions, including Luca and Beast Rune, selected. Compatibility/resource/availability guards still apply. |
| Nightmare Hall | All22 supported profiles selectable where their Original entry is available. Original remains a choice; no forced Nightmare battle. Growth-stone rewards ON: conditional25% fallback offer, native loot priority and inventory acceptance retained. |
| Spoiler Mode | OFF initially; normal story availability. Player can use Unlock All Bosses with its warning and explicit second confirmation, then restore Story Progression. No developer override. |
| Army Training | Four drills, Standard and supported Veteran choices, qualifying750/1,500-Potch rewards and normal-build ownership contract ON. Original campaign prerequisites retained. No commander XP. |
| Training defeat return | **ON as an unverified v1.55 playtest opt-in**, limited to owned War1 sessions and the audited War3 defeat ending. Fort Defense retains native Game Over. No defeat reward. |
| Original story wars | Native unit profiles/remix, AI, specializations, tactics and maneuvers ON; native scripted outcomes retained. |
| Excluded | Developer/Hunt debug, damage protection, internal combat/war selftests, research export, private transition harnesses, synthetic outcomes, retired Unity arena and unsupported legacy scripted-boss/stat overrides. |

## Defeat-return review

The toggle does not bypass training admission or save protection. Entry still
requires verified native definitions, a genuine idle field, original war completion
and the normal-build contract. Interception checks the current session's exact
WarChapter/field/system/epoch owners and native ending before changing only its
manager return result. The original result is retained for reward eligibility;
a defeat earns nothing. Normal story-war defeat is outside this owner scope.

The existing private flag fork, teardown witness, first verified returning field
prefix and campaign-data comparison remain in place. `SystemSave.Save`,
`UISaveLoad2.Save` and `GSD2SaveData.SaveFile` keep their campaign save fences until
the owned return is verified. Unexpected state keeps saving fenced until a fresh
native load. No fence or callback was weakened. If the saving-disabled message
appears, load the last good save; do not force a write or toggle modules mid-session.

This source/dependency review establishes why the opt-in can be tested, **not**
an earned defeat/return/autosave acceptance on the installed hash. Original Luca
defeat/interruption, complete campaign balance and many Nightmare paths also
remain unverified. Three Hall entries lack automatic story witnesses; player
Spoiler Mode provides access without changing campaign progression.

## Installation and startup evidence

- Supported original binary/metadata hashes matched the plugin's compiled guard.
- Game closed before installation; exact accepted DLL copied without rebuilding.
- Exactly one Reforged DLL present and one plugin loaded by BepInEx.
- All generated module JSON files parsed and matched the applied files. Bound
  configuration retained the authorized selections after normal startup/shutdown.
- Actual Steam game reached the original game-selection launcher. BepInEx logged
  the exact build label,10 Hunt definitions, Hall enabled, native war definitions
  verified and training enabled under its selected ownership contract.
- No unsupported-build, plugin installation rollback, missing target or Harmony
  compatibility rejection observed. The pre-existing IL2CPP signature-substitute
  warning is recorded separately, not presented as an error-free application claim.
- Normal shutdown also reproduced the previously documented
  `TMP_SubMeshUI.UpdateMaterial` / `OnDisable` NullReferenceException in the Unity
  player log. The process exited; no fatal error or campaign load occurred. This
  existing cleanup issue was not fixed or used to justify changing the accepted DLL.
- Game closed normally from the launcher. No campaign loaded or started, no battle
  played, no save operation requested, no developer test flags supplied.
- Fourteen original save files matched their existing recorded hashes before and
  after. No additional backups were made. Existing fallback artifacts retained.
- No gameplay source, DLL contents, Steam Cloud setting or beta/release changed
  by the documentation synchronization. The accepted DLL was installed as requested.

The existing binary emits an unconditional native commander-copy startup sanity
check even with the optional selftest settings OFF; no selftest setting or private
harness was activated for this smoke. Launcher verification is not campaign or
new experimental gameplay acceptance.

Sanitized installed configuration fingerprints and results are in
[the installation record](../publication/installation-v155.json). No personal
paths, saves, raw logs, game assets or installable DLLs are included in this commit.

Ready for the owner to launch and start **New Game → Normal**. This is the beginning
of the full gameplay playtest; fix demonstrated problems as they arise.
