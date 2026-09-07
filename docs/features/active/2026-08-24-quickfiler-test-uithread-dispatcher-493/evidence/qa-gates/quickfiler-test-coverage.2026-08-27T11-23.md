# Coverage Gate — Parallelized Supplementary Run (P3-T6)

Timestamp: 2026-08-27T11-23
Task: [P3-T6]
Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput TestResults\plan-logs\p3-t6\coverage-final.cobertura.xml` (run from `<repo-root>`)
EXIT_CODE: 1
Output Summary: Every test passed (`Test Run Successful.`, Total tests 1072, Passed 1072) and the
non-zero exit came solely from the 80% line-coverage threshold check, not from a test failure.
Post-change root `coverage` element attributes: **line-rate = `0.19049434489769984`**,
**branch-rate = `0.16177560720359307`**, **lines-valid = `78690`** (with `lines-covered = 14990`,
`branches-covered = 3710`, `branches-valid = 22933`). All three are byte-identical to the `P0-T13`
baseline triple. `CoberturaPostProcessed: false`, matching the baseline, so the two triples are
comparable and the rate gate ran. Line-rate delta is **0.00 percentage points**, which satisfies the
`>= -0.50` condition.

CoberturaPostProcessed: false
PipelineMismatch: false
DenominatorAnomaly: false
AddedLineCount: 624
ProductionSourcePathCount: PROVISIONAL — established by P4-T7

## Cited baseline artifact

Resolved per § Conventions from the stem `quickfiler-test-coverage-baseline`:
`<FEATURE>/evidence/baseline/quickfiler-test-coverage-baseline.2026-08-27T10-25.md`

## Coverage comparison

| Attribute | Baseline (`P0-T13`) | Post-change (this task) | Delta |
| --- | --- | --- | --- |
| `line-rate` | `0.19049434489769984` | `0.19049434489769984` | `0.00000000000000000` |
| `branch-rate` | `0.16177560720359307` | `0.16177560720359307` | `0.00000000000000000` |
| `lines-valid` | `78690` | `78690` | `0` |
| `lines-covered` | `14990` | `14990` | `0` |
| `branches-covered` | `3710` | `3710` | `0` |
| `branches-valid` | `22933` | `22933` | `0` |
| `CoberturaPostProcessed` | `false` | `false` | matched |

Both sides carry `CoberturaPostProcessed: false`, computed by the rule `P0-T13` states
(`true` when the task's own `EXIT_CODE` is `0`, `false` otherwise). Because the two values match, the
two triples were produced by the same post-processing path, they are comparable, `PipelineMismatch`
is `false`, and the rate gate ran rather than being skipped.

## Denominator condition and the rate gate

`AddedLineCount:` is established by this task and by no other, as the sum of four measurements this
task took itself:

| Input | Measured line count | Contribution |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | 278 | 278 |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | 346 | 346 |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 440 (baseline 489) | `max(0, 440 - 489)` = 0 |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 393 (baseline 418) | `max(0, 393 - 418)` = 0 |
| **Total** | | **624** |

The two baseline counts (489 and 418) are the values `P0-T11` recorded in
`<FEATURE>/evidence/baseline/file-inventory-baseline.2026-08-27T10-18.md`. Both owned files shrank,
so each contributes zero rather than a negative number.

Gate evaluation:

| Condition | Threshold | Observed | Held |
| --- | --- | --- | --- |
| `CoberturaPostProcessed` equality | must match baseline | `false` == `false` | yes |
| `abs(lines-valid delta)` | at most `AddedLineCount` = 624 | 0 | yes |
| Line-rate delta in percentage points | at least `-0.50` | `0.00` | yes |

The measured `lines-valid` delta of exactly zero is the expected outcome:
`ConvertTo-DerivedCoverageSettingsXml` adds the module exclusion for `*.Test.dll` before collection,
so every line this feature added sits in an uninstrumented assembly. `AddedLineCount:` is a
tolerance band rather than a prediction, and a movement larger than it would have been attributable
to the tool rather than to this diff. Neither `PipelineMismatch: true` nor
`DenominatorAnomaly: true` occurred, so no repeat collection was required.

## Failed-test set comparison

The failed-test names are compared in the same console spelling `P0-T13` records, because this task
runs the identical command and that pipeline supplies no `/Logger:trx` and therefore no
fully-qualified name.

| Set | Contents |
| --- | --- |
| `CoverageBaselineFailedTests` recorded by `P0-T13` | (empty) |
| This run's failed test names | (empty) |
| Is this run's set a subset of the baseline set? | **yes** |

An absolute `EXIT_CODE: 0` is not asserted here, for the reason `P0-T13` states.

NonZeroExitCause: `Assert-CoberturaLineCoverageThreshold`
(`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:489`) threw
`Cobertura line coverage 22.8059% is below the required 80% threshold.` No test failed. Per the
plan's § Decisions Record D5 this plan asserts no coverage floor — a decision ratified by
`<FEATURE>/spec.md` § Test Strategy, which records that this is a test-only change with no
production line in the diff — so this exit code is a recorded observation rather than a gate failure.
The 22.8059% figure is the discarded recomputed first-party rate; the 19.049% figure above is the raw
rate actually on disk, and the two are different quantities.

## Runsettings substitution, recorded rather than silently made

Spec § Test Strategy names `TaskMaster.runsettings` for this supplementary parallelized run. This
task instead used `scripts/vscode/TaskMaster.cli.runsettings`, which
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` resolves unconditionally from its own script directory
and which cannot be overridden by a parameter. The substitution is sound: both files declare
`<Scope>ClassLevel</Scope>`, so the parallelization the spec asks to exercise is identical, and the
CLI file additionally omits the Code Coverage `<DataCollector>` so the inner vstest run does not
activate a second collector alongside the outer `dotnet-coverage` instrumentation.

## Test run summary

| Metric | Value | Baseline (`P0-T13`) |
| --- | --- | --- |
| Verdict line | `Test Run Successful.` | `Test Run Successful.` |
| Total tests | 1072 | 1066 |
| Passed | 1072 | 1066 |
| Failed | 0 | 0 |
| Discovered test assemblies | 1 | 1 |

Raw Cobertura XML (17,213,352 bytes) is deliberately not committed; only the numeric headline values
above are recorded. Console log: `TestResults/plan-logs/p3-t6/coverage.log` (git-ignored).
