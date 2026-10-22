# P0-T7 — CSharpier baseline over the whole tree

Timestamp: 2026-09-07T14-08
Task: [P0-T7]
Issue: #796
Channel used: A

Command:
`pwsh -NoProfile -Command 'dotnet tool run csharpier check .; "EXIT_CODE=$LASTEXITCODE"'`

EXIT_CODE: 0

Recorded stdout, verbatim:

```
Checked 1601 files in 6592ms.
EXIT_CODE=0
```

## Files reported as unformatted

None. CSharpier lists each unformatted file on its own line before the summary line;
the recorded output carries no such line, only the summary. The list is therefore
empty.

CSHARPIER-BASELINE: CLEAN

The verdict line above carries exactly one of the two admitted values. Task P9-T1
branches on it.

Output Summary: 1601 files checked, 0 unformatted, EXIT_CODE 0. The tree is clean
against the manifest-pinned CSharpier 1.2.6 before any Phase 1 edit.
