$ErrorActionPreference='Stop'
$target=Join-Path ([IO.Path]::GetTempPath()) ('reforged-full-'+[Guid]::NewGuid().ToString('N'))
& (Join-Path $PSScriptRoot 'prepare-full-content.ps1') -OutputDirectory $target
$config=Join-Path $target 'BepInEx/config'
$hunts=Get-Content (Join-Path $config 'ReforgedNightmare/hunt-catalogue.json') -Raw | ConvertFrom-Json
if(@($hunts.Hunts | Where-Object Enabled).Count -ne 10){throw 'Full Hunt catalogue missing'}
$cfg=Get-Content (Join-Path $config 'nightmare.suikoden2.reforged.cfg') -Raw
foreach($key in @('EnableDeveloperTools','EnableHuntDebugTools','RunCombatSelfTest','RunNativeWarSelfTest','EnableWarArena','EnableTrainingWar','EnableTrainingNormalBuild','EnableNightmareEchoes','UnlockAllBosses')){
 if($cfg -notmatch "(?m)^$key = false\r?$"){throw "Unsupported option enabled: $key"}
}
foreach($key in @('EnableHallSierra','EnableHallHarpySecond','EnableHallWorm','EnableHall_luca_gauntlet','EnableHall_beast_rune')){
 if($cfg -notmatch "(?m)^$key = true\r?$"){throw "Supported Original encounter missing: $key"}
}
if($cfg -notmatch 'HallUnlockPolicy = StoryProgression'){throw 'Story gate bypassed'}
if(@([regex]::Matches($cfg,'(?m)^EnableHall_[a-z_]+ = true')).Count -ne 14){throw 'Bulk Hall selection incomplete'}
'PASS: ten Hunts, fourteen expanded Hall options, accepted Original Luca and supported legacy echoes, native story/bonus gates and no developer/protection/availability bypass.'
