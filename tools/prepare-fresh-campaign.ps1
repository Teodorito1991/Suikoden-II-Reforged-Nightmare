param([Parameter(Mandatory)][string]$OutputDirectory,
    [ValidateSet("fresh-campaign","full-content")][string]$Profile="fresh-campaign")
$ErrorActionPreference='Stop'
$sourceRoot=Split-Path -Parent $PSScriptRoot
$profileRoot=Join-Path $sourceRoot ('profiles/'+$Profile)
$destination=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $destination){
    if(!(Test-Path -LiteralPath $destination -PathType Container) -or
       @(Get-ChildItem -LiteralPath $destination -Force).Count){
        throw 'Choose an empty staging directory. This command never overwrites existing settings.'
    }
}
# Generate configuration only. No DLL, game discovery, installation or save operation.
$settings=Join-Path $destination 'BepInEx/config'
New-Item -ItemType Directory -Path $settings -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $profileRoot 'nightmare.suikoden2.reforged.cfg') -Destination $settings
Copy-Item -LiteralPath (Join-Path $sourceRoot 'config/ReforgedNightmare') -Destination $settings -Recurse
$overrides=Get-Content -LiteralPath (Join-Path $profileRoot 'module-overrides.json') -Raw | ConvertFrom-Json -AsHashtable
foreach($name in $overrides.Keys){
    $file=Join-Path $settings "ReforgedNightmare/$name"
    $data=Get-Content -LiteralPath $file -Raw | ConvertFrom-Json -AsHashtable
    foreach($key in $overrides[$name].Keys){$data[$key]=$overrides[$name][$key]}
    $data | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $file -Encoding utf8
}
$selection=Get-Content -LiteralPath (Join-Path $profileRoot 'selection.json') -Raw | ConvertFrom-Json
$file=Join-Path $settings 'ReforgedNightmare/hunt-catalogue.json'
$data=Get-Content -LiteralPath $file -Raw | ConvertFrom-Json
foreach($hunt in $data.Hunts){$hunt.Enabled=$hunt.Id -in $selection.EnabledHunts}
$data | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $file -Encoding utf8
$lines=Get-ChildItem -LiteralPath $destination -Recurse -File | Sort-Object FullName | ForEach-Object {
    $relative=[IO.Path]::GetRelativePath($destination,$_.FullName).Replace('\','/')
    "{0}  {1}" -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash,$relative
}
$lines | Set-Content -LiteralPath (Join-Path $destination 'SHA256SUMS.txt') -Encoding utf8
Write-Output "Configuration profile $Profile generated in $destination. No game or saves modified."
