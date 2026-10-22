# P5-T3 — AnalyzerItemRepair declared pass-through

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $p = "<execution-worktree-root>\scripts\dependencies\AnalyzerItemRepair.psm1"; Import-Module $p -Force -ErrorAction Stop; "IMPORT=ok"; (Get-Command -Module AnalyzerItemRepair | Select-Object -ExpandProperty Name | Sort-Object) -join ", "; "LINES=" + ([System.IO.File]::ReadAllLines($p)).Count'
```

EXIT_CODE: 0

## Output Summary

```
IMPORT=ok
Get-AnalyzerAssemblyPath, Invoke-AnalyzerItemRepair
LINES=161
```

## Acceptance

- The module imports without error: `IMPORT=ok`, with `-ErrorAction Stop` in force.
- `Get-Command -Module AnalyzerItemRepair` lists every function the plan's later tasks
  cite:
  - `Get-AnalyzerAssemblyPath` — the derivation function, cited by P5-T11's nine `AC12-`
    cases and by P5-T12.
  - `Invoke-AnalyzerItemRepair` — the rewrite function, cited by P5-T11's preservation,
    missing-segment and `AC13-` cases, by P5-T12, and by the entry point at P5-T8.
  No other function of this module is cited by any later task.
- File size 161 lines, inside the 500-line ceiling. Re-measured at P5-T22.

## Pass-through shape

`Get-AnalyzerAssemblyPath` returns an empty `[string[]]`. `Invoke-AnalyzerItemRepair`
returns an `AnalyzerItemRepair.Result` whose `Text` is the input unchanged, whose
`RepairedItem` and `MissingSegmentRecord` are empty and whose `ExaminedItemCount` is zero.
P5-T12 replaces the bodies only.

The module builds no report and exposes no count for the missing-segment class. It returns
the records; `ConsistencyVerifier.psm1` aggregates and counts them. That split is the one
S4 fixes, and Batch C has no production slot for a third owner of the class.

The module imports `PackageGraph.psm1` for parsing and does not write to it.
