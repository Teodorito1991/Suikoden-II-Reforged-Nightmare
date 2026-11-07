"""Fail closed on forbidden publication paths and common credential/private-path leaks.
This is a hygiene check, not proof of legal rights or an exhaustive secret scanner.
"""
from pathlib import Path
import json
import re
import subprocess
import sys

root = Path(__file__).resolve().parents[1]
if (root / '.git').exists():
    result = subprocess.run(['git', '-C', str(root), 'ls-files', '--cached', '--others', '--exclude-standard', '-z'],
                            check=True, stdout=subprocess.PIPE)
    files = [root / s.decode() for s in result.stdout.split(b'\0') if s]
else:
    files = [p for p in root.rglob('*') if p.is_file()
             and not any(x in p.parts for x in ['bin', 'obj', '.git'])]
bad = []
allowed_images = {'docs/media/historical-hunt-debug-battle.png',
                  'docs/media/historical-hunt-debug-reward.png'}
for path in files:
    relative = path.relative_to(root).as_posix()
    parts = {x.lower() for x in path.relative_to(root).parts}
    if (path.suffix.lower() in {'.dll', '.exe', '.pdb', '.zip', '.bundle', '.log', '.dat'}
        or parts.intersection({'save', 'interop', 'il2cpp_data', 'streamingassets', 'private'})
        or any(x.startswith('runtime-evidence') for x in parts)
        or path.name.lower() in {'dump.cs', 'script.json'}):
        bad.append((relative, 'forbidden file/path'))
    if path.suffix.lower() == '.png':
        if relative not in allowed_images:
            bad.append((relative, 'unreviewed image'))
        continue
    if path.suffix.lower() == '.json':
        try:
            json.loads(path.read_text(encoding='utf-8-sig'))
        except Exception:
            bad.append((relative, 'invalid JSON'))
    try:
        text = path.read_text(encoding='utf-8-sig')
    except UnicodeDecodeError:
        bad.append((relative, 'unreviewed binary'))
        continue
    if re.search(r'(?i)[A-Z]:[/\\]Users[/\\]', text):
        bad.append((relative, 'personal Windows path'))
    if re.search(r'gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,}|-----BEGIN (?:RSA |OPENSSH )?PRIVATE KEY-----', text):
        bad.append((relative, 'possible credential'))
    if path != Path(__file__).resolve() and re.search(r'NativeMethodInfoPtr_|Il2CppDummyDll|class .*TypeDefIndex:', text):
        bad.append((relative, 'possible generated proprietary listing'))
    if re.search(r'(?i)[A-Za-z0-9._%+-]+@(?:gmail|hotmail|outlook)\.', text):
        bad.append((relative, 'personal email address'))
if bad:
    for path, reason in bad:
        print(f'FAIL {path}: {reason}')
    sys.exit(1)
print(f'PASS: {len(files)} publication files; no forbidden inputs/private-path/common-secret matches.')
