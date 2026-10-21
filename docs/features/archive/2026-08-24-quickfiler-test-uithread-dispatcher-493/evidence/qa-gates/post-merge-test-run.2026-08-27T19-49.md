# Post-Merge Toolchain Step 4 — Full Test Suite with Coverage

Timestamp: 2026-08-27T19-49
Task: Resume verification — mandatory toolchain re-run after merging the moved epic integration base
Command: `vstest.console.exe <9 test assemblies> /EnableCodeCoverage /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /ResultsDirectory:<scratch> /Logger:trx`
EXIT_CODE: 0
Output Summary: "Test Run Successful. Total tests: 6707". Independently counted 6707 `Passed` lines in
the console log, so passed equals total and failed plus skipped equals zero. All six of this
feature's regression tests R1-R6 pass. Both AC-6 theme tests pass.

## Scope: full repository suite, not just the changed assembly

All nine test assemblies were run together in one invocation rather than only `QuickFiler.Test`.
A single-assembly run would not have detected a regression introduced into another assembly by the
11 base commits merged in during this resume, and a per-assembly coverage figure is not comparable
to the repository figure.

Assemblies: QuickFiler.Test, SVGControl.Test, Tags.Test, TaskMaster.Test, TaskTree.Test,
TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, VBFunctions.Test.

`/TestCaseFilter:TestCategory!=LiveOutlook` is required. Omitting it runs a test that attaches to a
real Outlook process, which both launches Outlook and destroys comparability with every recorded
baseline. `/InIsolation` matches CI and avoids the aggregate test-host crash seen on this repository
when many assemblies share one host.

## Baseline arithmetic

| Measurement | Count |
| --- | --- |
| Sibling feature 442's reported suite total at its merge (PR #649) | 6701 |
| This feature's new regression tests (R1-R6) | +6 |
| Expected total | 6707 |
| Observed total | 6707 |
| Observed failures | 0 |

The identity is exact: this branch adds six tests and removes none, and the merged base contributes
no test-count change beyond 442's already-merged total.

## Regression tests R1-R6 (all passed)

| ID | Test | Duration |
| --- | --- | --- |
| R1 | `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` | 12 ms |
| R2 | `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` | 2 ms |
| R3 | `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` | 3 ms |
| R4 | `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` | 9 ms |
| R5 | `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` | 5 ms |
| R6 | `Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException` | 5 ms |

## AC-6 consumer tests (unmodified file, both passed)

- `SetThemeDark_FromNormal_SelectsDarkNormalTheme` — passed (< 1 ms)
- `SetThemeLight_FromNormal_SelectsLightNormalTheme` — passed (< 1 ms)

## Coverage

`/EnableCodeCoverage` was supplied and a binary `.coverage` artifact was produced in the scratch
results directory. It is deliberately NOT converted, committed, or emitted as
`artifacts/csharp/coverage.xml`.

Rationale, recorded so the omission is not mistaken for an oversight: this change adds zero
production lines. All 624 added lines are in the `QuickFiler.Test` assembly, which coverage tooling
excludes from the instrumented denominator by policy. The coverage delta attributable to this branch
is therefore exactly zero, which the Phase 0 versus final comparison already established with a
byte-identical whole-repository Cobertura triple. Emitting a repository-wide coverage XML here would
publish a pre-existing sub-floor whole-repo figure that this test-only branch neither caused nor can
remediate. Coverage remains recorded numerically in
`quickfiler-test-coverage.2026-08-27T11-23.md` and its Phase 0 baseline counterpart.

The results directory and the binary coverage file live under a scratch path outside the repository
and are not committed. Their generated file names embed the local account and machine name, so they
are referenced here descriptively rather than by path.
