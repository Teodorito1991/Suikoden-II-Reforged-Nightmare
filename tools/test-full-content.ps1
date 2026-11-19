$ErrorActionPreference='Stop'
$target=Join-Path ([IO.Path]::GetTempPath()) ('reforged-full-'+[Guid]::NewGuid().ToString('N'))
& (Join-Path $PSScriptRoot 'prepare-full-content.ps1') -OutputDirectory $target
$config=Join-Path $target 'BepInEx/config'
$hunts=Get-Content (Join-Path $config 'ReforgedNightmare/hunt-catalogue.json') -Raw | ConvertFrom-Json
if(@($hunts.Hunts | Where-Object Enabled).Count -ne 10){throw 'Full Hunt catalogue missing'}
$cfg=Get-Content (Join-Path $config 'nightmare.suikoden2.reforged.cfg') -Raw
foreach($key in @('EnableDeveloperTools','EnableHuntDebugTools','RunCombatSelfTest','RunNativeWarSelfTest','EnableWarArena','EnableTrainingWar','EnableNightmareEchoes','EnableHallSierra','EnableHallHarpySecond','EnableHallWorm')){
 if($cfg -notmatch "(?m)^$key = false\r?$"){throw "Unsupported option enabled: $key"}
}
if($cfg -notmatch 'HallUnlockPolicy = StoryProgression'){throw 'Story gate bypassed'}
if(@([regex]::Matches($cfg,'(?m)^EnableHall_[a-z_]+ = true')).Count -ne 11){throw 'Bulk Hall selection incomplete'}
'PASS: ten Hunts, eleven new Hall options, native story/bonus gates and no developer/protection/availability bypass.'
