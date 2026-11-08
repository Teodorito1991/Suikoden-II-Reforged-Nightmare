# Building the source

Players install the binary ZIP; no developer tooling is required for that.

The C# plugin targets **net6.0**, using BepInEx 6 IL2CPP and Harmony. Retained managed
tests target **net9.0**. The verified SDK was **9.0.320**; install .NET9 SDK from
Microsoft. global.json permits later9 feature bands, not exact binary reproduction.

Use your own supported game and tested loader. Run once to generate interop locally,
then close. No proprietary reference is provided in this repository.

| Local folder | Required references |
|---|---|
| GAME/BepInEx/core |BepInEx.Core.dll, BepInEx.Unity.IL2CPP.dll, 0Harmony.dll, Il2CppInterop.Runtime.dll|
| GAME/BepInEx/interop |GSD2.dll, GSDShare.dll, FrameWork.dll, ShareUI.dll, Il2Cppmscorlib.dll|
| GAME/BepInEx/interop |UnityEngine.CoreModule.dll, UnityEngine.IMGUIModule.dll, UnityEngine.TextRenderingModule.dll, UnityEngine.UI.dll|
| GAME/BepInEx/interop |Unity.Addressables.dll, Unity.ResourceManager.dll, Unity.InputSystem.dll|

Supported native/metadata hashes are in INSTALL.md and Plugin.SupportedBuild.
Generated interop, metadata, raw assets and native binaries must remain local.

```powershell
./build.ps1 -GamePath 'D:\SteamLibrary\steamapps\common\Suikoden I and II HD Remaster'
```

Or separately:

```powershell
dotnet build src/NightmareRebalance/NightmareRebalance.csproj -c Release '-p:GamePath=D:\SteamLibrary\steamapps\common\Suikoden I and II HD Remaster' -p:DebugType=none
dotnet run --project tests/NightmareRebalance.Tests.csproj -c Release
```

This helper compiles/runs managed tests only. It does not install, launch the game,
load saves or execute native acceptance. Archived arena inputs compile into managed
tests only, not the plugin. No custom arena UI/runtime is reintroduced.

Main contains v1.14 development source. `beta/v1.12.2` is the clean older candidate
snapshot. Original development branches/history remain independently preserved,
not imported into the publication repository. Source provenance is recorded in
publication/source-provenance.json.

**A rebuild is a new binary.** SDK and Git informational metadata can change its
hash. Original accepted/native-tested DLLs remain preserved unchanged. Public review
copies remove only a personal CodeView PDB path, with new hashes and no inherited
exact-hash acceptance; see publication/binary-privacy-audit.json. Never
replace one with a rebuild and retain its acceptance label. Public CI runs managed
tests and hygiene; it neither downloads game files nor certifies gameplay. Native
fixtures require locally owned inputs and must not run on a loaded player save.

The public helper passes DebugType=none for future plugin builds to avoid embedding
personal PDB paths. Private debugging builds may use symbols, but must be audited
before distribution. This helper was not used to rebuild either review payload.

The v1.14 resource bridge probe is NOT a player option. It requires both existing
self-test settings, explicit batch/test/bridge flags and the independent-copy
marker, plus the private isolation harness for save-root protection. Do not run
it in the installed game or infer safety from copying the executable alone.
It adds no JSON setting or live launcher. See docs/WAR-BRIDGE-V114.md for scope.
