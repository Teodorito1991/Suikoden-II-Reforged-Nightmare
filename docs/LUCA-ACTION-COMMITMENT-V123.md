# Luca Blight action commitment

Development build `1.23.0-luca-action-commitment.1` with SHA256 `D14F9F29939A099DE1470C5F78EDE34ADA92ABF74E4A937FA3F0184F4F00EA82`.

The isolated production audit loads the original `boss_luc3_00` and admits the
scratch source through the existing roster, signature, provenance and callback
registry checks. It invokes the original `monster_attack_sub`. The native
dispatcher initializes its action chain and creates reaction buffers. The probe
reads the returned chain. A scoped selector override can choose a legal mode on
the first selection for the exact scratch actor and work object. Native range,
use, silence and attack checks still apply. The override is available only inside
the explicit isolated startup fixture.

| Fixture | Native selection and committed result | Target slots | Selector calls |
| --- | --- | --- | --- |
| Full, baseline | Mode 1, column special, display index 1386 | 1, 4 | 1 |
| Full, controlled focus | Mode 1 changed to mode 0, basic attack | 1 | 1 |
| Full, controlled row | Mode 1 changed to mode 2, row special, display index 1387 | 0, 1, 2 | 1 |
| Wounded | Controlled basic and row both committed on separate runs | 1 or 0, 1, 2 | 1 each |
| Defensive | Controlled row committed | 0, 1, 2 | 1 |
| Silenced | Special override declined. Native sequence 1, 2, 1, 0 committed mode 0 | 2 | 4 |
| No legal targets | Native retry budget exhausted. No action chain produced | None | 10 |

The original mask values are 0x100, 0x400 and 0x800. The special actions
contained the selected mode in the native payload. Presentation indices were
read from the original moves. Playback was outside the fixture. The dispatcher
incremented its actor action counter once per call, including failed attempts.
The round counter, original unlimited-use counts, scratch `luka_hp` sentinel and
callback pointers stayed unchanged. The selector override consumed no extra RNG
draws inside its hook. Different native actions and retries may consume RNG
differently downstream. The supplied original moves have unlimited uses, so the
fixture does not demonstrate an exhausted limited move.

The paired native runs produced identical records. Baseline column damage was 159/156. The controlled row recorded 139/136/141, and 39/36/41 against the defensive fixture. Both passed all ten retained
boss policy checks and the production regression, including 560 Normal/Hard
regular enemy admissions. Managed regression tests passed. Original save,
configuration and installed DLL hashes matched after the runs.

## Live tactics gate

The isolated dispatcher committed different action chains with native damage. The
fixture supplies native party-slot and fixed-character data and enters at the monster
dispatcher and bypasses the manager's complete `BattleMakeData` initialization
and later action execution. Earlier attempts exposed missing party and character data.
This build adds no live policy or configuration key. Before enabling one,
exercise a fully initialized disposable action queue,
then prove the normal manager-owned dispatch boundary can distinguish scripted
commands from discretionary attacks. No opening, ending, HP transfer or
rendered battle was executed.
