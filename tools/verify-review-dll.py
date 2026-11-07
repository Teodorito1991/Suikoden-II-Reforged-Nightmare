"""Read-only verification of an original/private DLL against its public review copy.
Usage: python tools/verify-review-dll.py ORIGINAL PUBLIC A
Do not upload the original DLL: it retains a personal debug path.
"""
from pathlib import Path
import hashlib
import json
import sys

if len(sys.argv) != 4 or sys.argv[3] not in ['A', 'B']:
    raise SystemExit('Expected ORIGINAL PUBLIC A-or-B')
root = Path(__file__).resolve().parents[1]
record = next(x for x in json.loads((root/'publication/binary-privacy-audit.json').read_text())
              if x['Candidate'] == sys.argv[3])
a = Path(sys.argv[1]).read_bytes()
b = Path(sys.argv[2]).read_bytes()
digest = lambda x: hashlib.sha256(x).hexdigest().upper()
assert digest(a) == record['OriginalSha256'], 'Original hash mismatch'
assert digest(b) == record['PublicCopySha256'], 'Public hash mismatch'
assert len(a) == len(b), 'File length changed'
allowed = set()
for item in record['DebugRecords']:
    start, length = item['PathOffset'], item['ReplacedByteCapacity']
    assert a[start-24:start-20] == b'RSDS' == b[start-24:start-20]
    assert b[start:start+length].split(b'\0')[0] == b'ReforgedNightmare.pdb'
    allowed.update(range(start, start+length))
changes = {i for i, (x, y) in enumerate(zip(a, b)) if x != y}
assert changes and changes <= allowed, 'Change outside reviewed debug-path range'
assert len(changes) == record['ChangedBytes'], 'Changed-byte count mismatch'
print('PASS: exact hash pair; every byte outside the reviewed CodeView path is unchanged. No runtime/gameplay acceptance inferred.')
