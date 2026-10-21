# P7-T2 — Repair entry-point suite authored

Timestamp: 2026-09-20T01-12

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $p = "tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1"; $lines = Get-Content -LiteralPath $p; $q = "\x27"; @($lines | Select-String -Pattern ("^\s*It\s+" + $q + "AC10-")).Count'
```

EXIT_CODE: 0

## Output Summary

`tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` was created with the
`Write` tool. It drives the entry point over an in-memory fixture carrying two candidate upgrades,
one of which is incompatible, and asserts the three skip-and-proceed conditions in three separate
`It` blocks whose names begin with the token `AC10-`. Every filesystem dependency is injected: the
file store is a hashtable, the reader and writer read and write that hashtable, and the three
restore-directory delegates are hashtable lookups. No temporary file is created.

## Acceptance conditions

| Condition | Observed | Result |
|---|---|---|
| File is at most 500 lines | 374 | PASS |
| `It` blocks whose name begins `AC10-` | 3 | PASS |
| Other `It` names matching `AC\d` | 0 | PASS |
| `Describe` and `Context` names matching `AC\d` | 0, over 8 such blocks | PASS |
| Temporary-file idioms (`New-TemporaryFile`, `GetTempPath`, `GetTempFileName`, `New-Item`) | 0 | PASS |

The prohibited token is measured as the regex `AC\d` rather than the bare two letters, per gate
rule 11: `-match` is case-insensitive by default and a bare `AC` matches ordinary English.

## The three AC10 cases, verbatim

```
It 'AC10-1 leaves the incompatible package at the version its manifest already declared'
It 'AC10-2 writes the target version for the compatible package, so the run proceeded'
It 'AC10-3 returns a skip record naming the incompatible package and a reason'
```

The fixture gives `Contoso.Widgets` a candidate 2.0.0 shipping `net472` and `netstandard2.0`, and
`Fabrikam.Core` a candidate 2.0.0 shipping only `netstandard2.1`, which is the framework issue #902
excludes outright. A fail-fast implementation stops at the incompatible package and fails AC10-2,
which is the assertion that proves the remaining upgrades proceeded.

## Whole-file run, before the filtered run of P7-T3

```
PESTER Passed=30 Failed=0 Total=30
Covered 91.12% / 75%. 428 analyzed Commands in 1 File.
```

The 27 cases beyond the three AC10 ones carry no criterion token, so they are outside every
criterion-filtered population. They cover the reconciliation of each dependent element kind, the
analyzer-item regeneration and its non-fatal missing-segment class, the binding-redirect
reconciliation, the what-if path, the manifest-without-a-project path, the absent-from-manifest
class and the default filesystem delegates. They exist because `P9-T3` requires at least 90 percent
line coverage for `Repair-PackageManifestConsistency.ps1`, which the three AC10 cases alone do not
reach; the 91.12 percent figure above is the measurement against that requirement, taken with
`CodeCoverage.Path` scoped to the entry point.

## Two defects the first run of this suite surfaced, both fixed in the entry point

1. **`-WhatIf` did not reach `Invoke-ManifestNormalization`.** A preference variable set on a script
   does not propagate into a module's own session state, so the normalisation pass wrote files
   during a what-if run while the script's own three `ShouldProcess` sites correctly declined. The
   call now passes `-WhatIf:$WhatIfPreference` explicitly. The case that caught it asserts the store
   is byte-identical after a what-if run.
2. **A disagreement on an item the repair deliberately left alone produced a failure result.** The
   missing-segment class is non-fatal by design, so those lines are now excused from the residual
   set before the failure result is built, which is the behaviour
   `Invoke-ProjectConsistencyRepair` already implements for the same class.
