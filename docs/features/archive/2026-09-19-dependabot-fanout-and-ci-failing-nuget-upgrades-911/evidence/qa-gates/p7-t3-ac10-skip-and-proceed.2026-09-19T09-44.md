# P7-T3 — AC10: an incompatible package is skipped and the remaining upgrades proceed

Timestamp: 2026-09-20T01-16

Command:

```
pwsh -NoProfile -Command 'Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1"); $c.Filter.FullName = "*AC10-*"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p7-t3-ac10-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Output Summary

```
Filter 'FullName' set to ('*AC10-*').
Filters selected 3 tests to run.
   [+] AC10-1 leaves the incompatible package at the version its manifest already declared
   [+] AC10-2 writes the target version for the compatible package, so the run proceeded
   [+] AC10-3 returns a skip record naming the incompatible package and a reason
Tests Passed: 3, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 27
PESTER Passed=3 Failed=0 Skipped=0 Total=30 NotRun=27
```

## Acceptance conditions

| Condition | Observed | Result |
|---|---|---|
| `EXIT_CODE` | 0 | PASS |
| `Failed` | 0 | PASS |
| `Total` (executed population) | 3 | PASS |
| All three named assertions pass | AC10-1, AC10-2 and AC10-3 each reported `[+]` | PASS |

**`Total` here is the executed population, `Passed + Failed + Skipped` = 3 + 0 + 0 = 3**, per the
retarget in the plan's `CMD-PESTER-ALL` block, which governs any task whose command sets
`$c.Filter.FullName`. The raw fields are recorded as context: `TotalCount` is **30** and
`NotRunCount` is **27**, the 27 being the cases in the same file that the filter did not select.
`TotalCount` counts filtered-out tests as `NotRun`, so an exact assertion over it would measure the
file's `It` count and be invariant under the filter.

## What the three assertions establish

The fixture carries two candidate upgrades. `Contoso.Widgets` 2.0.0 ships `net472` and
`netstandard2.0`; `Fabrikam.Core` 2.0.0 ships only `netstandard2.1`, which .NET Framework 4.8.1
cannot load at all.

- **AC10-1** reads the manifest out of the in-memory store after the run and finds
  `<package id="Fabrikam.Core" version="1.0.0"` still, with no `version="2.0.0"` for that
  identifier: the incompatible package's manifest version is unchanged.
- **AC10-2** finds `<package id="Contoso.Widgets" version="2.0.0"`: the compatible package's
  manifest version is the target version, so the run proceeded past the skip. A fail-fast
  implementation fails exactly this assertion.
- **AC10-3** reads the returned report and finds exactly one skip record, whose `PackageId` is
  `Fabrikam.Core`, whose `ToVersion` is `2.0.0`, whose `Reason` is not whitespace, and whose reason
  names `netstandard2.1`.

Coverage was collected to `coverage/p7-t3-ac10-coverage.xml`, which is gitignored at
`.gitignore:144`; no collector document is written under the evidence tree, per gate rule 12. The
figure that run reports, 39.37 percent over 20 files, is the coverage of a deliberately filtered
three-case population and is not a coverage claim about this change; the whole-file figure for the
entry point is recorded at P7-T2 and the suite-wide figure is taken at P9-T3.

This task checks off **AC10** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
