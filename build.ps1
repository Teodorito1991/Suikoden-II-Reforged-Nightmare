param(
 [string]$GamePath='C:\Program Files (x86)\Steam\steamapps\common\Suikoden I and II HD Remaster',
 [string]$Dotnet='dotnet'
)
$ErrorActionPreference='Stop'
if (-not (Test-Path -LiteralPath (Join-Path $GamePath 'BepInEx/interop/GSD2.dll'))) { throw 'Install BepInEx IL2CPP and run your legally owned game once. See BUILDING.md.' }
& $Dotnet build "$PSScriptRoot/src/NightmareRebalance/NightmareRebalance.csproj" -c Release "-p:GamePath=$GamePath" "-p:DebugType=none" --nologo
if($LASTEXITCODE -ne 0){throw 'Plugin compilation failed.'}
& $Dotnet run --project "$PSScriptRoot/tests/NightmareRebalance.Tests.csproj" -c Release
if($LASTEXITCODE -ne 0){throw 'Managed regression tests failed.'}
Write-Output 'Local compilation and managed checks finished. No game launched, plugin installed, or saves written. Rebuilt DLL has its own hash and no inherited gameplay acceptance.'
