# P1-T1 — Projection Part File Structural Verification

Timestamp: 2026-09-13T05-22
Task: [P1-T1]

Command: pwsh -NoProfile -Command '<abstract-syntax-tree parse of scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1, enumerating function definitions, command names, hashtable literals, string constants naming a condition-coverage attribute, and top-level non-function statements>'
EXIT_CODE: 0

## Observations

```
PARSE_ERRORS=0
FUNCTION_COUNT=3
FUNCTION=ConvertTo-JacocoPackageProjection
FUNCTION=Assert-JacocoProjectionReconciliation
FUNCTION=Test-RawCoverageDocumentRetained
COMMANDS=Get-CoberturaPackageLineSummary, Join-Path, Set-StrictMode, Split-Path
HASHTABLE_COUNT=0
CONDITION_COVERAGE_STRINGS=0
TOP_LEVEL_NON_FUNCTION_STATEMENTS=1
TOP_STMT=Set-StrictMode -Version Latest
```

File encoding and size, observed separately:

```
BOM=True
LINES=197
```

## Acceptance mapping

- The file exists and measures 197 content lines, which is at most 500.
- It declares exactly the three named functions and no fourth.
- The only top-level executable statement is `Set-StrictMode -Version Latest`.
- The complete set of commands the file invokes is `Get-CoberturaPackageLineSummary`, `Join-Path`,
  `Set-StrictMode` and `Split-Path`. None reads, writes or deletes a file, and none starts an
  external process. `Join-Path` and `Split-Path` are path arithmetic and do not require either path
  to exist.
- `Get-CoberturaPackageLineSummary` is present as a command invocation, so the counting rule is
  delegated rather than re-derived. Zero hashtable literals and zero string constants naming a
  condition-coverage attribute are present, which is the mechanical form of the same claim.
- The file carries a UTF-8 byte-order mark and opens with `Set-StrictMode -Version Latest`, as the
  plan's new-file convention requires.

## Output Summary

EXIT_CODE: 0. The part file parses with zero errors, declares exactly the three specified functions,
carries one top-level statement which is the strict-mode line, invokes no filesystem or process
command, and delegates the per-package counting rule to `Get-CoberturaPackageLineSummary`. 197 lines,
byte-order mark present.
