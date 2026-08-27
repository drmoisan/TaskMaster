# QuickFiler.Test Coverage Baseline (P0-T13)

Timestamp: 2026-08-27T10-25
Task: [P0-T13]
Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput TestResults\plan-logs\p0-t13\coverage-baseline.cobertura.xml` (run from `<repo-root>`)
EXIT_CODE: 1
Output Summary: Every test passed (`Test Run Successful.`, Total tests 1066, Passed 1066) and the
non-zero exit came solely from the 80% line-coverage threshold check, not from a test failure.
Root `coverage` element attribute values read from the emitted Cobertura file:
**line-rate = `0.19049434489769984`**, **branch-rate = `0.16177560720359307`**,
**lines-valid = `78690`** (with `lines-covered = 14990`, `branches-covered = 3710`,
`branches-valid = 22933`). `CoberturaPostProcessed: false`, so those three values are the raw
all-modules-instrumented totals, not first-party recomputed totals.

NonZeroExitCause: `Assert-CoberturaLineCoverageThreshold`
(`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:489`) threw
`Cobertura line coverage 22.8059% is below the required 80% threshold.` No test failed. Per the
plan's § Decisions Record D5 this plan asserts no coverage floor, so this exit code is a recorded
observation rather than a gate failure, which is why the task text forbids asserting `EXIT_CODE: 0`
here.

CoberturaPostProcessed: false

The rule stated by the task is applied verbatim: `CoberturaPostProcessed:` is `true` when this
task's `EXIT_CODE:` is `0` and `false` otherwise. It is `false` here, which means the document on
disk is the pre-post-processing form. The threshold check throws after
`ConvertTo-KoverageCoberturaXml` has recomputed the root attributes in memory but before the
`Set-Content` that would persist them, so the recomputed document was discarded and the file retains
the raw totals. The 22.8059% figure in the error message is the discarded recomputed first-party
rate; the 19.049% figure below is the raw rate actually present on disk. **These are two different
quantities, not two samples of one quantity.** `P3-T6` may compare its triple against this one only
when its own `CoberturaPostProcessed:` also equals `false`.

## Root `coverage` element attributes (read from the emitted file)

| Attribute | Value |
| --- | --- |
| `line-rate` | `0.19049434489769984` |
| `branch-rate` | `0.16177560720359307` |
| `lines-covered` | `14990` |
| `lines-valid` | `78690` |
| `branches-covered` | `3710` |
| `branches-valid` | `22933` |
| `complexity` | `24403` |
| `version` | `1.9` |

Source file: `TestResults/plan-logs/p0-t13/coverage-baseline.cobertura.xml` (git-ignored, 17,213,319
bytes; raw Cobertura XML is deliberately not committed).

## Test run summary from the same invocation

| Metric | Value |
| --- | --- |
| Verdict line | `Test Run Successful.` |
| Total tests | 1066 |
| Passed | 1066 |
| Failed | 0 |
| Skipped | 0 |

The inner `vstest.console` run reported
`Test Parallelization enabled for <repo-root>/QuickFiler.Test/bin/Debug/QuickFiler.Test.dll (Workers: 24, Scope: ClassLevel)`,
confirming that this invocation exercises the class-level parallelization the CI invocation does not,
which is the configuration in which the #493 race is reachable.

## CoverageBaselineFailedTests

(empty)

**An empty list is a legitimate recorded value**, and it is the value recorded here. It is recorded
in the run's own console spelling, which for this pipeline is a bare test-method name rather than a
fully-qualified name: `Get-DotnetCoverageArgumentList`
(`scripts/vscode/Invoke-MSTestWithCoverage.ps1:70-76`) appends only `/Settings:`, `/InIsolation`,
and `/TestCaseFilter:` to the inner `vstest.console` invocation and supplies no `/Logger:trx`, so no
TRX is produced and no fully-qualified name is available from this pipeline. `P3-T6` runs the
identical command, so both sides of that comparison carry the same spelling.

This empty list on a non-zero exit is the case the task text anticipates: the non-zero came from the
coverage threshold, not from a failing test.

## Discovered test-assembly list

Exactly one assembly, as the acceptance condition requires:

```
QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
```

The script logged `Discovered 1 test assemblies.` and
`A total of 1 test files matched the specified pattern.` No `.claude` worktree copy and no sibling
assembly was discovered.

## Instrumentation note bearing on the P3-T6 denominator

`ConvertTo-DerivedCoverageSettingsXml` adds the module exclusion pattern for `*.Test.dll` before
collection, so every line this feature adds sits in an uninstrumented assembly and the expected
`lines-valid` delta between this baseline and `P3-T6` is zero. `P3-T6`'s `AddedLineCount:` is
therefore a tolerance band rather than a prediction.

Console log: `TestResults/plan-logs/p0-t13/coverage.log` (git-ignored).
