# P5-T1 — ProjectConsistency declared pass-through

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $p = "<execution-worktree-root>\scripts\dependencies\ProjectConsistency.psm1"; Import-Module $p -Force -ErrorAction Stop; "IMPORT=ok"; (Get-Command -Module ProjectConsistency | Select-Object -ExpandProperty Name | Sort-Object) -join ", "; "LINES=" + ([System.IO.File]::ReadAllLines($p)).Count'
```

EXIT_CODE: 0

## Output Summary

```
IMPORT=ok
Invoke-BindingRedirectReconciliation, Invoke-VersionReconciliation
LINES=135
```

## Acceptance

- The module imports without error: `IMPORT=ok` was printed, and the import ran with
  `-ErrorAction Stop`, so a load-time failure would have terminated the command.
- `Get-Command -Module ProjectConsistency` lists every reconciliation function the plan's
  later tasks cite:
  - `Invoke-VersionReconciliation` — cited by P5-T6 and by the AC11 cases at P5-T15.
  - `Invoke-BindingRedirectReconciliation` — cited by P5-T7 and by the AC14 cases at P5-T16.
  No other reconciliation function is cited by any later task, so the exported set and the
  cited set agree exactly.
- File size 135 lines, inside the 500-line ceiling. Re-measured at P5-T22.

## Pass-through shape

Both functions carry their final names, parameter names, parameter types and output type,
and both return a `ProjectConsistency.ReconciliationResult` whose `Text` is the input text
unchanged, whose `Repair` is empty and whose `ExaminedCount` is zero. P5-T6 and P5-T7
replace the bodies only.

The shape is deliberate. It makes the P5-T5 red run a behavioural failure on assertions
rather than an import failure or a missing-command failure, so the red proves the absent
behaviour rather than an absent file.

The module imports `PackageGraph.psm1` for parsing and does not write to it; no Phase 5
task may write to that file, which is the fourth production file of Batch C.
