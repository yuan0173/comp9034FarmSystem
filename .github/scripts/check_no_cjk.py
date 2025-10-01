#!/usr/bin/env python3
import sys
import re
from pathlib import Path

# Directories and extensions to enforce (source code only)
ENFORCED_DIRS = [
    Path('frontendWebsite/src'),
    Path('backend/src'),
    Path('frontendWebsite/tests'),
    Path('backend/tests'),
    Path('scripts'),
]
ENFORCED_EXTS = {'.ts', '.tsx', '.js', '.jsx', '.cs'}

# CJK Unified Ideographs (basic + extensions A/B range simplified)
CJK_REGEX = re.compile(r"[\u3400-\u4DBF\u4E00-\u9FFF\U00020000-\U0002A6DF]")

violations = []

for root in ENFORCED_DIRS:
    if not root.exists():
        continue
    for p in root.rglob('*'):
        if p.suffix in ENFORCED_EXTS and p.is_file():
            try:
                content = p.read_text(encoding='utf-8', errors='ignore')
            except Exception:
                continue
            if CJK_REGEX.search(content):
                violations.append(str(p))

if violations:
    print('❌ Found CJK characters in source files (not allowed):')
    for v in violations:
        print('-', v)
    sys.exit(1)
else:
    print('✅ No CJK characters detected in enforced source directories.')
    sys.exit(0)
