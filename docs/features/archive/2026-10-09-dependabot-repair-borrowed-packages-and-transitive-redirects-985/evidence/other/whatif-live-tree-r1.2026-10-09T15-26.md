# Live-tree WhatIf Smoke (R1, issue #985)

Timestamp: 2026-10-09T15-26
Command: git status --porcelain; pwsh -NoProfile -File <SCRATCH>\985r1-cmd\985-repair.ps1 -WorkspaceRoot WORKSPACE-ROOT -WhatIfRun; git status --porcelain
EXIT_CODE: 0
Output Summary:
- Script completed; IS-SUCCESS: True (recorded, not gated).
- WRITTEN-COUNT: 0
- REDIRECTSYNC-REPAIR-COUNT: 0
- REDIRECTSYNC-UNVERIFIABLE: netstandard (the known case)
- REDIRECTSYNC-UNRESOLVABLE: (empty)
- git status --porcelain before and after: identical (26 lines each).
- Information line: "Binding redirect sync: examined 17 application configuration file(s), synchronised 0 redirect(s), unverifiable 1, unresolvable 0"

Helper output (the per-file `What if:` lines are omitted; all other lines verbatim):
```
COMMAND: & WORKSPACE-ROOT\scripts\dependencies\Repair-PackageManifestConsistency.ps1 -WhatIf
IS-SUCCESS: True
WRITTEN-COUNT: 0
REPAIR-COUNT: 0
BEYOND-KNOWN-WEAK: 0
REPORT-REPAIR-KINDS: 
SKIP-COUNT: 0
REDIRECTSYNC-REPAIR-COUNT: 0
REDIRECTSYNC-UNVERIFIABLE: netstandard
REDIRECTSYNC-UNRESOLVABLE: 
BODY-BEGIN
## Repairs applied
No repairs were applied.
BODY-END
```
