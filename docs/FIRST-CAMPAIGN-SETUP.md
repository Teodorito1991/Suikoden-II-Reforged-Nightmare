# First campaign setup — recommended v1.55

**Owner installation completed on 26 November 2026:** exact v1.55, Full Content plus
selectable Nightmare/growth rewards and experimental training defeat return.
Spoiler Mode remains OFF. The owner's explicit no-additional-backups instruction
was followed; all fourteen original saves are unchanged. See the
[installed selection and startup evidence](INSTALLATION-STATUS-V155.md).
The general steps below describe a future manual setup; they were not rerun as
backup operations. No campaign has been started for the owner.

This is a setup guide, not an installer. Use exact `1.55.0-story-bosses.1`, SHA256
`C5BE2F48118E205083096599F0C2B2285DF2CE4E2807DA26B5E3EB561497055E`.
Keep [the readiness report](FIRST-CAMPAIGN-READINESS.md) with it.

1. Close the game normally. Make one plain backup of the game's `Save` folder,
   current Reforged plugin folder and `BepInEx/config`. Store rollback DLLs outside
   `BepInEx/plugins` so an old copy cannot also load. No Cloud change is required
   by this guide; this review did not change Cloud or save files.
2. Use the existing supported Steam/BepInEx installation. Replace only the old
   Reforged DLL at `BepInEx/plugins/ReforgedNightmare/ReforgedNightmare.dll` with the
   recommended exact DLL. Ensure there is one loaded Reforged plugin. Do not copy
   private test guards, saves, interop, game binaries or resource packs from a test copy.
3. Generate Full Content to a new empty staging folder with
   `tools/prepare-full-content.ps1 -OutputDirectory <empty-folder>` from the source
   repository. It creates configuration only and refuses existing contents. Review
   its `BepInEx/config` files, then use them as the new campaign configuration after
   preserving your old settings. Deliberately compare/merge any personal choices;
   replacing the DLL alone does not migrate old settings to Full Content.
4. Launch normally. Check `BepInEx/LogOutput.log` for
   `Reforged Nightmare 1.55.0-story-bosses.1 loaded` and no unsupported-build or
   compatibility rejection. Choose **Suikoden II → New Game → Normal** yourself.
   Do not use the boosted or downloaded test checkpoints for this fresh campaign.
5. Play the story normally. The Guild attendant beside the Suggestion Box uses
   ordinary talk/confirm/cancel/navigation. No F6/F10/Ctrl or private harness is
   required for the normal castle workflows. Guild access depends on its supported
   castle state; training additionally requires the original wars' completion.

Recommended choices: Full Content, Normal, one level1–2 recovery charge per eligible
survivor/victory; ten Hunts and22 supported Original Hall definitions selected;
story-based availability and all supported story tactics; four optional training
drills/Veteran; Spoiler Mode OFF, Nightmare OFF initially, experimental defeat
return OFF, developer/selftests/protection/retired arena OFF. Optional Nightmare
can be chosen later through its separate opt-in configuration and Hall tier.

Maintain a recent manual save in another slot before difficult optional activities,
especially Luca or training. If training explicitly says saving is disabled after
an unverified return, load the last good save; do not force a write or toggle
features mid-session. Defeat uses the original Game Over route. Contracts are
session-only and must be reaccepted after loading. Ordinary saves are not fenced
when no optional session owns them.

For a real bug, keep the game version, location/enemies, party levels/equipment,
a short description and these existing logs (no debug controls needed):
- `BepInEx/LogOutput.log`
- `BepInEx/ReforgedNightmare/combat/last-battle.json`
- `BepInEx/ReforgedNightmare/combat/battle-history.json`

The combat files appear after tracked battles and may be absent before one has
completed. Keep saves/logs private; do not commit them or post personal paths
publicly. Enable extra diagnostics only if needed for a concrete reported issue.

Fallback: close the game, restore the backed-up DLL **and its configuration**,
then load a known good save. Keep the accepted v1.53 DLL
`54144F2F61644F6B632F4513B2A4F30C240033AFD68F6F80CE1453051F7BA286`
and v1.54 fallback separately. Switching builds does not undo levels/items already
saved. No full-campaign stability guarantee or automatic downgrade conversion is
claimed. The next step is the owner's campaign, not more compulsory playtests.
