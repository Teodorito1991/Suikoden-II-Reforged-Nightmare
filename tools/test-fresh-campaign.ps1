$ErrorActionPreference='Stop'
$generator=Join-Path $PSScriptRoot 'prepare-fresh-campaign.ps1'
$testRoot=Join-Path ([IO.Path]::GetTempPath()) ("reforged-profile-"+[Guid]::NewGuid().ToString('N'))
$first=Join-Path $testRoot 'first'
$second=Join-Path $testRoot 'second'
& $generator -OutputDirectory $first
& $generator -OutputDirectory $second
$checksum=Get-FileHash -LiteralPath (Join-Path $first 'SHA256SUMS.txt')
if($checksum.Hash -ne (Get-FileHash -LiteralPath (Join-Path $second 'SHA256SUMS.txt')).Hash){throw 'Nonreproducible profile'}
try{& $generator -OutputDirectory $first;throw 'Existing files were not refused'}
catch{if($_.Exception.Message -notlike 'Choose an empty staging directory*'){throw}}
foreach($line in Get-Content -LiteralPath (Join-Path $first 'SHA256SUMS.txt')){
    $expected,$relative=$line -split '  ',2
    if((Get-FileHash -LiteralPath (Join-Path $first $relative)).Hash -ne $expected){throw 'Existing staging content changed'}
}
$config=Join-Path $first 'BepInEx/config'
$cfg=Get-Content -LiteralPath (Join-Path $config 'nightmare.suikoden2.reforged.cfg') -Raw
foreach($key in @('EnableDeveloperTools','EnableHuntDebugTools','RunCombatSelfTest','RunNativeWarSelfTest','EnableWarArena','EnableTrainingWar','EnableNightmareEchoes')){
    if($cfg -notmatch "(?m)^$key = false\r?$"){throw "Unsafe campaign default: $key"}
}
if($cfg -notmatch 'HallUnlockPolicy = StoryProgression'){throw 'Story gate policy lost'}
$legendary=Get-Content -LiteralPath (Join-Path $config 'ReforgedNightmare/legendary-bosses.json') -Raw | ConvertFrom-Json -AsHashtable
if(@($legendary.Values | Where-Object {$_}).Count){throw 'Unaccepted scripted changes enabled'}
$hunts=Get-Content -LiteralPath (Join-Path $config 'ReforgedNightmare/hunt-catalogue.json') -Raw | ConvertFrom-Json
$enabled=@($hunts.Hunts | Where-Object Enabled | Select-Object -ExpandProperty Id | Sort-Object)
if(($enabled -join ',') -ne 'sun-kings-retinue,undying-watch'){throw 'Unexpected recommended Hunt set'}
'PASS: reproducible profile, existing files untouched, dangerous defaults off and explicit content selection.'
