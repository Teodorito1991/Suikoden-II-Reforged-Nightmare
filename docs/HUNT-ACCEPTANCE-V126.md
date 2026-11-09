# v1.26 protected gameplay candidate

The candidate reuses the exact `1.25.0-hunt-catalogue.1` DLL.
SHA256 `AA27445761E1E7581FF2D32DCF5FA608E47A072F06A5B307B809A46557EA09B0`. v1.26 is the packaging and acceptance milestone.
The game will identify the binary as v1.25.

## Before a session

1. Close Suikoden normally. A tester must first prepare a separate
   campaign-capable environment with verified local save routing and Steam
   Cloud isolation, preserving the installed v1.13 game. The existing copied
   launcher is protected by a startup-only guard that rejects campaign loads.
   Do not load a campaign there or copy this package over the Steam installation.
2. Verify the current Steam Cloud setting without changing it. If safe isolation
   is unavailable, stop. Snapshot the original save and configuration hashes,
   the installed DLL hash and game preferences. Keep a separate verified backup.
3. Stage the known Normal headquarters checkpoint in the disposable environment
   only after isolation is proven. A byte-identical copy of the previously
   accepted checkpoint is staged offline, outside the package. Use normal loading
   and confirm access to Leona's tavern and to the intended destination before
   accepting. The checkpoint starts at headquarters. The travel route and story
   eligibility from that checkpoint still need confirmation.
4. In the disposable copy, use the candidate configuration. To opt in, replace
   its `BepInEx/config/ReforgedNightmare/hunt-catalogue.json` with the matching
   file from one `opt-in` folder. Sun stays enabled. The other experimental
   contract stays disabled. The major-boss options and developer tools are off.

## One short quest per profile

1. At headquarters, form a healthy party at Leona and free one item slot if
   convenient. Talk to the Guild attendant, select the test contract and close
   the panel. Confirm the active contract and objective are visible.
2. Travel normally. Keep the same loaded session from acceptance through report.
   Walk in an original encounter area. The automatic request needs no hotkey.
   If an ordinary group appears, record the location and enemy roster so the
   source can be checked before attempting repeated fights.
3. Win the target battle through normal play. Record the enemies, displayed
   Potch, actual purse change and any offered or received item. A full purse
   or inventory can limit what is credited.
4. Return to field control, then return to the castle and report. Confirm that
   the contract is available again and that reporting adds no second payout.
   If you escape instead, confirm the contract remains accepted and retryable.
5. Quit normally. Archive the disposable results and any test saves separately.
   Restore any test-changed local preferences from the fresh snapshot and verify
   that original saves, configuration and installed DLL still match the baseline.
   Keep Steam Cloud as found. Use a fresh disposable checkpoint for the other
   contract. Do not reuse historical isolation scripts.

## Destination evidence

- **Dreamer's Circuit** targets Mr. Venus, Nightmare and MegaWatt in Sindar Ruins.
  The original `vc12` local map 0 has a direct transition to map 1 and a return
  link. Map 1 points to the walking table `vc12_00`. The original field records
  identify eight eligible variants. The contract adds 1,200 Potch and may offer
  Stone of Magic Protection.
- **Thornwake Patrol** targets Melonzoo and two Holly Elves in the Greenhill City
  region. Original `ve03` local maps 0, 1 and 2 connect in both directions
  through map 1 and reference the walking table `ve03_00`. The original records
  identify four eligible variants. The contract adds 3,000 Potch and may offer
  Flowing Orb.

These are local map links from original map-change commands. They do not identify
the shortest headquarters route or prove that a particular story state can use
a doorway. Stop if normal travel is blocked. A possible item is a fallback when
the native loot slot is empty, subject to eligibility and space.

## Verification

Both opt-in configurations reached the original launcher in the isolated runtime.
Startup checks confirmed the exact DLL, one experimental contract enabled at a
time, the Castle Guild and automatic requests enabled, and major-boss and debug
options off. The previous v1.25 native tests apply to these exact DLL bytes. They
verified original battle setup and reward preparation with synthetic inputs.
The 2,008 and 5,678 Potch fixture totals are examples from those tests.

Sun's complete quest remains accepted on the historical v1.12.2 DLL. v1.19's
Guild presentation and v1.20's Rat and Harpy evidence belong to their respective
builds. The current binary includes those changes and retained RPG and story-war
features. No current candidate quest or full campaign was played. Luca and
independent skirmish research remain paused.
