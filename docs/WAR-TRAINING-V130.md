# v1.30 War Training expansion

The baseline for repeated standard War 1 is
`1.29.0-training.2`. The owner completed two consecutive standard War 1
runs on that DLL, with matching return positions and flag arrays. See
[the v1.29 report](SKIRMISH-TRANSITION-V129.md).

The v1.30 candidate adds **Commander Rout**, a War 1 drill with the same
original map and units. It uses the existing audited War 1 outcome rules
to extend the battle beyond the timed withdrawal and end it on a commander
rout or allied defeat. Native ending playback and teardown still run.
The drill requires the original script/map fingerprints, the original
commander and a routable commander record. If runtime admission fails,
War 1 uses the stock rules. Enemy AI and stats are unchanged.
War 2 was outside the initial training roster.

The Guild bottom row starts the standard drill by default. Left or right
toggles Commander Rout while that row is selected. The objective panel
describes the selected drill. Hunt report and withdrawal retain priority
when a contract is active. Training awards no items, Potch or progression.

The optional defeat route observes the matching WarChapter's completed
native result. It can hand result 1 to the manager after native result 2
only for the armed War 1 in mode 5, with the original field owner, step 48,
no event owner, the same load epoch and zero observed save calls. The return
prefix still requires normal teardown and all existing ownership checks.
It logs both results. This interception is unverified in gameplay.

The flag restore is limited to the five arrays. The return hook compares
serialized game, war, character and party data at the first return boundary.
It disables repeat entry when an unexpected change appears. That comparison
does not certify later field events, a casualty path or a saved campaign.

All three initial Native War options default off and require the disposable runtime.
They are enabled in the staged test copy. This candidate compiled without
warnings or errors, passed the managed regression suite and loaded its
training gate in the copied runtime. The loader banner retains its old
version string. Use the exact DLL hash to identify this build.

- Build `1.30.0-training.1`
- SHA-256 `FF4EEA8779E94404AC56624090851179ADA5E43602286917C173748833C13C2F`
- Steam installation and private beta unchanged. Sync pending gameplay acceptance.

## Consolidated playtest

1. In the copied game, choose Suikoden II and Continue. Talk to the Guild
   attendant in Leona's tavern with no active hunt. Choose the bottom row
   and complete the standard drill. Issue a move and attack when possible.
   On return, walk and open and close the field menu. Note any unsolicited
   cutscene or change in the party.
2. Talk to the attendant again. On the bottom row, press right to select
   Commander Rout. Check that it continues beyond the timed ending. Aim
   for a natural defeat so the result 2 interception can be observed.
   Check movement and the menu if returned. Stop at Game Over, unexpected
   story activity, a stuck field or a missing challenge.
3. If both returns work, make a manual save in the disposable campaign,
   reload it and start training again. Report which outcomes actually
   occurred. The log will identify the chosen drill, native result,
   destination match, flag equality, save calls and persistent differences.

The first worthwhile follow-up after these checks is verifying admission
and a full return for a second native scenario before adding rewards.

## Owner test on v1.30

The standard drill completed with native result 1 and manager result 1.
The return log recorded matching flags and destination, zero save calls
and only expected position and clock changes. The second run selected
Commander Rout and passed the War 1 remix admission checks. It uses the
same map and roster. The log stops before a timed-withdrawal decision or
a completion result for that run. The observer trace closed after the
standard run, so there is no second native transition trace in this test.
Commander Rout's extended turns, defeat return and save/reload remain open.

## Fort Defense candidate

The second selectable drill in 1.30.0-training.2 uses original War 2.
It shares the base battlefield with War 1 and uses War 2's opening and
ending scripts. The option defaults off and requires both original war
completion flags. Four verified allied squads are made player-controlled
when the original roster matches. Story remix outcomes and defeat
interception are inactive for this drill.

Candidate SHA256: 8E3F3D1000270BEAD0433B4C2F515876874D6FC87B18902EDE28FD3B683383D2.
Release build and managed regressions passed. BepInEx loaded the candidate
in the copied runtime. Actual War 2 gameplay was subsequently observed.

The owner completed Fort Defense on the exact candidate. The runtime recorded
War 2 with four player squads, native result 1, matching castle destination,
equal restored flag arrays and zero save calls before the return hook.
The compared data differed only in position and clocks. The owner confirmed
the return worked. Repeat entry was available at the return boundary, but a
second War 2 run, defeat, and manual save/reload were not observed on this DLL.

## Commander Rout control candidate (v1.31)

On v1.30.0-training.2, the owner entered Commander Rout and played past the
original timed withdrawal. The original war continued, but the owner could
not reliably lose while allied squads were under their original control.
No native defeat or ending result was observed.

The v1.31.0-training.1 candidate uses the existing native SetUnitControl seam
after admitted Commander Rout initialization to make each active allied
squad player-controlled. It preserves native attacks, casualty handling and
ending playback. The existing War 1 defeat-result interceptor is enabled in
the disposable copy. It checks the armed training session, current chapter
and field ownership, return step, load epoch and save-call count. Story wars
cannot use that interceptor.

SHA-256: 3E9B06357328FC2E71E1C094334DDE179D4189BFB3C84302C535E1B88987861E.
The candidate compiled, passed managed regression checks and loaded in the
copied game. Live squad control, earned defeat, altered ending, subsequent
campaign state and post-save re-entry remain awaiting owner playtest.

The first control candidate assigned zero squads because InitBattle ran before
the War 1 opening staged them. Candidate 1.31.0-training.2 instead assigns
active allied squads just before the original CheckControlStepCheck examines
command availability. It only runs in the admitted Commander Rout drill and
uses the same native control setter. Build, managed regression and copied
startup passed. Native input acceptance and battle outcomes remain pending.
SHA-256: 90983930FF4495FFA95DB4B2640C21DEEF9A8F0FB0821F1C6C771C585968FECD.

### v1.31.0-training.2 gameplay acceptance

The owner earned a Commander Rout defeat. The native command step transferred
three allied squads, the battle continued past turn three, and native ending
3 ran. WarChapter returned 2. The armed training-only intercept passed 1 to
the manager and the owner regained control in the tavern. The owner manually
saved to an empty slot, reloaded it, completed Fort Defense, and returned.

Both returns recorded matching destination and restored flags, zero save calls
before restoration, and only position and clock differences in compared data.
The new manual save also matched the original test checkpoint for all five
flag arrays and the serialized war, character, and party data. Commander Rout
victory and defeat in other scenarios were outside this acceptance run.

### War 3 distinct-field candidate

Candidate 1.31.0-training.3 adds a separate default-off War 3 drill. The
original completion flag, map 9, roster and script fingerprints must match.
The drill uses the stock tactical rules and original ending dispatch. The
War 1 defeat-result interceptor does not apply to War 3.

SHA-256: 4D86F2D17378AF28E3A925CDE61C5CDB7CDC1DE0E7D859BADB2B53EBF6D39BA2.
Build and managed checks passed. The disposable runtime loaded the DLL and
entered native WarChapter with scenario 3. The owner completed the battle and
returned to the tavern. Native result 1 passed through unchanged. The trace
recorded WarChapter teardown before MapChapter resumed. The return audit found
equal restored flags, zero save calls before restoration, a matching destination
and only position and clock differences. Repeat entry was available at return.
A second War 3 run, defeat and post-War 3 save/reload were outside this test.
