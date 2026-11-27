# v1.36.1 Guild graphical preview

Development DLL `1.36.1-guild-preview.1`

SHA256 `215D469EC71AAAFF126798271F0CBFF9408FF60AB4EE4B1670228B05F74F3BEF`

## What caused the zero draws

After staging a single copy of each plugin, the exact v1.36 DLL still loaded
the original UI assets and timed out with zero draws when Windows launched it
with a hidden window. On an inactive Windows desktop with its normal window
surface, the same DLL and game copy rendered all seven original preview cases.
The graphics harness needed a window Unity could repaint. A separate desktop
supplies that surface without taking focus or showing the game on the user's
desktop.

The preview now captures eight named PNGs in a fresh run directory, advances
from actual repaints, and requires the PNGs and asset release before the
launcher reports success. Standard and Veteran each have their own capture.
All eight screens use the v1.36 renderer. NPC placement, war logic and
training return routing retain their v1.36 behavior.

## Exact-build evidence

| Check | Result |
|---|---|
| Original frame, cursor and font | Loaded and released once in each preview |
| Rendered PNG sets | Eight at 1920×1080 and eight at 3200×1800 |
| Visual inspection | Hunt cards, Army Training, Standard, Veteran and Guild Status showed readable text, selection highlights, rewards and controls |
| Managed and original-engine regression | Release build, managed suite, seven-war tactics probe and production native audit passed |
| Actual field interaction | The preview stayed at the launcher. No campaign, Guild dialogue, training return or playable story battle was exercised |

Windows launch arguments request a size. The actual image sizes reported by
Unity take precedence, since the game may apply its own resolution during
startup. Images contain original game artwork and remain local.

## Repeat locally

Use the trusted disposable game copy with its existing startup-only save guard,
private marker, and exactly one mod DLL. Close the game, then run the script
from the repository. The startup guard is local tooling and is excluded from
this repository.

```powershell
./tools/preview-guild.ps1 -CopiedGamePath '<path to the disposable copy>'
```

The script prints the capture directory and verified DLL hash. It launches on
a separate desktop and waits for normal exit. A hidden window can reproduce
the zero-repaint failure.
