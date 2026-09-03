# Baseline MSTest run with coverage (P0-T11)

Timestamp: 2026-09-03T01-27

Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput 'docs\features\active\2026-09-02-test-determinism-and-hygiene-debt-729\evidence\baseline\coverage-baseline.cobertura.xml'`

EXIT_CODE: 0

CoberturaProcessingState: processed

The script exited 0, so both the collection call and the 80% line-coverage threshold assertion
returned and the post-processed Koverage-compatible XML was written. Neither authorized non-zero
branch (threshold assertion) nor the authorized #743 mechanical re-run branch was taken.

## Output Summary

Runner discovery line:

```
Discovered 9 test assemblies.
```

vstest counts:

```
Test Run Successful.
Total tests: 6952
     Passed: 6952
 Total time: 33.7905 Seconds
```

TotalCount: 6952
PassedCount: 6952
FailedCount: 0

Coverage root element attributes read from
`docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline/coverage-baseline.cobertura.xml`:

```
line-rate="0.85386"
branch-rate="0.794589"
lines-covered="55138"
lines-valid="64575"
branches-covered="13187"
branches-valid="16596"
```

BaselineLineRate: 0.85386
BaselineBranchRate: 0.794589

## Recorded coverage-floor conflict (plan D5)

Recorded verbatim rather than reconciled:

- `CLAUDE.md` UT2: "Repository-wide line coverage must remain `>= 80%`." and "Any new modules,
  classes, or methods added must target `>= 90%` coverage."
- `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`: "Line coverage must
  remain >= 85% across all tiers (T1-T4)." and "Branch coverage must remain >= 75% across all
  tiers (T1-T4) for languages whose coverage tooling measures branch coverage."
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` enforces only the 80% line figure, via the helper
  `Assert-CoberturaLineCoverageThreshold` in `Invoke-MSTestWithCoverage.Helpers.ps1` lines 459-491.

This plan adopts no absolute repository-wide floor as its own gate. Its binding coverage gates are
the two no-regression comparisons stated in D5 and verified by P6-T6.

## Stranded derived-settings confirmation

```
Test-Path 'docs\features\active\2026-09-02-test-determinism-and-hygiene-debt-729\evidence\baseline\coverage-baseline.cobertura.xml.effective-coverage.config'  ->  False
Get-ChildItem -Path 'docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/baseline' -Filter '*.effective-coverage.config'  ->  0 items
```

No stranded derived coverage-settings file remains; the runner's finally block removed it.

## D9 discovered-assembly assertion

DiscoveredAssemblyCount: 9 (matches the runner's `Discovered 9 test assemblies.` line)
ZeroDiscoveredPathsContainWorktreesSegmentBelowSearchRoot: True

Repository-relative form of each discovered assembly path, derived by removing the resolved
search-root prefix from each enumerated `FullName`:

```
QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
SVGControl.Test\bin\Debug\SVGControl.Test.dll
Tags.Test\bin\Debug\Tags.Test.dll
TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
TaskTree.Test\bin\Debug\TaskTree.Test.dll
TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

No absolute path is written into this artifact.
