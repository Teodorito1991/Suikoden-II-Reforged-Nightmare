# Installation — Windows Steam beta

Recommended review package: **v1.12.2-beta.1**, internal **1.12.2-guild-ui.1**.
Separate development review package: **v1.13.0-dev.1**, internal
**1.13.0-native-war-foundation.1**. Install exactly one. No public release is live.

## 1. Supported game

Steam app **1932640**, Windows x64, installed build **20278991**, Unity2022.3.28f1.
English presentation was tested. Changes apply to Suikoden II only. Other game
builds, storefronts, consoles, Linux/Steam Deck and locales are not verified.
The plugin checks both original files and refuses unsupported builds:

| File relative to game root | Required SHA256 |
|---|---|
| GameAssembly.dll | E093F175EAF3734D293CA0A236C652DD7FCC72EA5FA10D7B21103EDF12F11C3A |
| Suikoden I and II HD Remaster_Data/il2cpp_data/Metadata/global-metadata.dat | ABB6489CA1EF1728F9DE74F64A64D9C109F5DDD5BA9046226EA964B15B62F964 |

Do not obtain replacement proprietary files from mod repositories or force-enable
an unsupported build.

## 2. Back up saves and check Cloud

Close the game normally. Steam → game → Manage → Browse local files opens `GAME`,
the directory containing `Suikoden I and II HD Remaster.exe`. A common location is
`C:\Program Files (x86)\Steam\steamapps\common\Suikoden I and II HD Remaster`;
other Steam libraries are valid.

Independently copy the **entire GAME/Save directory** outside the Steam game folder.
That was the active save directory verified locally; verify your installation if
it differs. Also back up `BepInEx/config` and the previous plugin DLL. Keep several
manual saves; do not rely on one autosave.

Check Steam Properties → General → Steam Cloud. Cloud can synchronise changed saves.
For disposable testing, record its original setting and consider turning it off
before staging copies; decide which saves to keep before re-enabling. This package
does not change Cloud, restore saves or install save collections. Never overwrite
originals or move saves while the game is running.

## 3. Install BepInEx

Verified loader: **BepInEx6.0.0-be.697+5362580, Unity.IL2CPP-win-x64**.
Use the official [BepInEx builds page](https://builds.bepinex.dev/projects/bepinex_be),
find build697 and choose **Unity.IL2CPP-win-x64**, not Mono/x86/BepInEx5.
Newer loader versions are not automatically verified.

Follow the official [IL2CPP installation guide](https://docs.bepinex.dev/master/articles/user_guide/installation/unity_il2cpp.html).
Extract the loader into GAME beside the executable. Run once, allow interop
creation to finish, then close normally. Initial generation may need Internet
access. Keep the loader's bundled runtime/core files together. Tested core:
HarmonyX2.10.2 and Il2CppInterop1.4.6-ci.426.

BepInEx is installed separately, not bundled. Suikoden Fix, PKCore and S2 Character
Mod are reference projects, **not additional requirements**. Begin with Reforged
Nightmare alone; other balance/recovery/war patches can conflict.

## 4. Install the beta package

Use the reviewed **installation ZIP**, not GitHub's source Code ZIP. Verify the ZIP
against `SHA256SUMS.txt`. With the game closed, extract its **BepInEx folder into
GAME** on a fresh installation. ZIP-root documents are for reading.

```text
GAME/
  Suikoden I and II HD Remaster.exe
  BepInEx/
    core/                           # supplied by the separate loader
    interop/                        # generated locally, never supplied by us
    plugins/ReforgedNightmare/
      ReforgedNightmare.dll
    config/
      nightmare.suikoden2.reforged.cfg
      ReforgedNightmare/
        characters.json
        magic.json
        combat-reforged.json
        castle-guild.json
        first-expansion.json
        hunt-catalogue.json
        hunt-guild.json
        ... other supplied module JSON files
      NightmareRebalance/
        boss-profiles.json
        scripted-battle-allowlist.json
```

Move previous `NightmareRebalance.dll`/`ReforgedNightmare.dll` copies **outside**
the active plugins tree, including nested folders. Keep exactly one active DLL.
Do not replace GameAssembly.dll, metadata, original models or other game assets.

## 5. Start on Normal and check the log

Use **Normal** for a new Suikoden II campaign; the overhaul supplies its own
challenge. Hard admission was component-tested but is not the balance foundation.
This plugin does not convert your save's difficulty.

Check `GAME/BepInEx/LogOutput.log` for:
`Reforged Nightmare 1.12.2-guild-ui.1 experimental content installed.`
The development package instead logs `1.13.0-native-war-foundation.1`.
There must be no unsupported-build or installation-rolled-back error. A startup
marker alone is not evidence that a battle or quest passed.

Public review-copy plugin hashes (personal PDB path removed, no rebuild):

- Recommended1.12.2: `8F3F085C3D9338C43FD74F80110D86CEFAD5F77A08A738EBA06D0B341C9DBD34`
- Development1.13: `386CF34C6B549A185C5DACC6A5F64C38150FED935D36D1B9E9551E6CEDDBDE17`

Preserved original accepted1.12.2:
`0B520FDD9182B6C024FE96376D0F284BE249DF93AF2E748DC331D21324910F46`;
original native-tested1.13:
`1A59986703E8346E1B2F6D7968C451C0609A4FB990C8F88EB3D63DBCEBB2CD59`.
Those originals contain personal debug paths and are not public ZIP payloads.
Only CodeView path bytes differ; managed metadata and every other byte are
unchanged. Neither new public hash has runtime/gameplay acceptance. Startup smoke
verification remains a release-readiness item; historical original-DLL quest proof
must not be transferred to the public review hash.

Fresh public defaults enable core/earlier boss/story-war modules and Sun's castle
quest/native item fallback. Unplayed contracts, major boss policies, debug protection,
probes, research export and developer tools are OFF. Story-war campaign coverage
remains incomplete. No independent skirmish launcher is exposed.

## 6. Optional content

Edit settings only while closed; see [CONFIGURATION.md](CONFIGURATION.md). Normal
Guild play uses the attendant, not hotkeys. The actor is guarded to the audited
headquarters tavern variant and may be absent at other castle stages. Do not
change story flags to make it appear. Contracts reset on actual loading/restart.

## Updating without losing settings

Back up first, then replace **only the plugin DLL** while closed. Copy JSON files
only if missing; compare changed examples manually. Do not extract an entire
supplied defaults folder over an existing configuration. These candidates retain
compatible schemas; deliberate user opt-ins are not silently changed. An older
installation may still have hunts disabled and need a conscious opt-in.

## Disable, uninstall and rollback

Move the plugin folder outside `BepInEx/plugins`, or remove its one DLL while
closed. Keep configuration backups. Do not remove the loader if other mods use it.
To roll back, replace the DLL with the verified saved earlier DLL, never keep both
active. No save conversion occurs. Some mod-influenced values may already have
been saved by original game routines; uninstalling does not necessarily undo
saved character growth. Restore a pre-mod save for a clean comparison.

## Troubleshooting

No loader log: verify IL2CPP/x64 loader placement and its official guide. Missing
references/interop generation failure: finish loader setup; do not download someone
else's generated assemblies. Hash refusal: stop and report game build. Overlapping
plugins: return to one active Nightmare DLL and isolate conflicts. Invisible Guild
or blocked input: cancel/exit normally and disable `castle-guild.json`'s Enabled
setting if needed; do not force-save. See [KNOWN_ISSUES.md](KNOWN_ISSUES.md).
