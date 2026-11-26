$ErrorActionPreference='Stop'
$target=Join-Path ([IO.Path]::GetTempPath()) ('reforged-full-'+[Guid]::NewGuid().ToString('N'))
& (Join-Path $PSScriptRoot 'prepare-full-content.ps1') -OutputDirectory $target
$config=Join-Path $target 'BepInEx/config'
$hunts=Get-Content (Join-Path $config 'ReforgedNightmare/hunt-catalogue.json') -Raw | ConvertFrom-Json
if(@($hunts.Hunts | Where-Object Enabled).Count -ne 10){throw 'Full Hunt catalogue missing'}
$cfg=Get-Content (Join-Path $config 'nightmare.suikoden2.reforged.cfg') -Raw
foreach($key in @('EnableDeveloperTools','EnableHuntDebugTools','RunCombatSelfTest','RunNativeWarSelfTest','EnableWarArena','EnableTrainingDefeatReturn','EnableNightmareEchoes','UnlockAllBosses')){
 if($cfg -notmatch "(?m)^$key = false\r?$"){throw "Unsupported option enabled: $key"}
}
foreach($key in @('EnableTrainingWar','EnableTrainingNormalBuild','EnableTrainingChallenge','EnableTrainingWar2','EnableTrainingWar3','EnableTrainingVeteran','EnableTrainingRewards')){
 if($cfg -notmatch "(?m)^$key = true\r?$"){throw "Selected training option missing: $key"}
}
foreach($key in @('EnableHallSierra','EnableHallHarpySecond','EnableHallWorm','EnableHall_luca_gauntlet','EnableHall_beast_rune')){
 if($cfg -notmatch "(?m)^$key = true\r?$"){throw "Supported Original encounter missing: $key"}
}
if($cfg -notmatch 'HallUnlockPolicy = StoryProgression'){throw 'Story gate bypassed'}
$bosses=Get-Content (Join-Path $config 'ReforgedNightmare/boss-tactics.json') -Raw | ConvertFrom-Json
if(!$bosses.EnableMajorBosses -or !$bosses.EnableExistingBosses){throw 'Audited story tactics missing'}
$legendary=Get-Content (Join-Path $config 'ReforgedNightmare/legendary-bosses.json') -Raw | ConvertFrom-Json
foreach($key in @('EnableMistShade','EnableGorudo','EnableWormMoth','EnableLaterLucia')){
 if(!$legendary.$key){throw "Audited story policy missing: $key"}
}
foreach($key in @('EnableBossProfiles','EnableScriptedBossOverrides')){
 if($cfg -notmatch "(?m)^$key = false\r?$"){throw "Legacy stat override enabled: $key"}
}

if($cfg -notmatch '(?m)^RuneRecoveryMaxLevel = 2\r?$'){throw 'Full profile must reserve high-level rune charges'}
if(@([regex]::Matches($cfg,'(?m)^EnableHall_[a-z_]+ = true')).Count -ne 14){throw 'Bulk Hall selection incomplete'}
'PASS: ten Hunts, 22 Original echoes and four training drills with supported Veteran/rewards; native story gates, stock defeat, no developer/protection/availability bypass.'
