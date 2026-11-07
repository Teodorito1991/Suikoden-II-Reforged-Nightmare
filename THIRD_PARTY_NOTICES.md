# License and distribution inventory

Original mod code/documentation uses MIT with the owner's explicit approval.
See LICENSE. This does not license KONAMI content, runtime dependencies or screenshots.

| Project | Upstream terms | What is distributed here |
|---|---|---|
| Suikoden Fix |MIT| Retained notice, not its DLL |
| PKCore |MIT| Retained notice, no generated game listing |
| S2 Character Mod |MIT| Retained notice, not its DLL |
| BepInEx |LGPL2.1| License text/attribution, runtime installed separately |
| Il2CppInterop |LGPL3| License text/attribution, runtime installed separately |
| HarmonyX |MIT| License text/attribution; loader supplies runtime |
| Unity, game, generated interop |Proprietary/game-derived| Not bundled or uploaded |

The plugin dynamically references local interfaces; Private=false references do
not copy those libraries into the release. Original mod source/build guidance is
provided for review and rebuilding. This is a distribution inventory, not a legal
opinion on reverse engineering or the game's EULA in every jurisdiction.

Official license sources checked 2026-11-07:
[BepInEx](https://github.com/BepInEx/BepInEx/blob/master/LICENSE),
[Il2CppInterop](https://github.com/BepInEx/Il2CppInterop/blob/master/LICENSE),
[HarmonyX](https://github.com/BepInEx/HarmonyX/blob/master/LICENSE),
[Suikoden Fix](https://github.com/d3xMachina/Suikoden-Fix/blob/master/LICENSE.txt),
[PKCore](https://github.com/faospark/PKCore/blob/main/LICENSE.txt),
[S2 Character Mod](https://github.com/makotech222/S2_Character_Mod/blob/master/LICENSE.txt).

If a future package bundles a loader/library, review its distribution obligations
separately. These plugin-only packages do not authorise bundling proprietary inputs.
Numerical IDs/signatures used in authored compatibility guards are not a licence
to redistribute underlying original tables, event bytecode or extracted assets.
