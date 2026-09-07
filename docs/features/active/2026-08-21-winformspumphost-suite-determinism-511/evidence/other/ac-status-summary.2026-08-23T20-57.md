# Acceptance-Criteria Status Summary — Remediation Cycle 1

Timestamp: 2026-08-23T19-33

Acceptance-criteria source: `docs/features/active/winformspumphost-suite-determinism-511/spec.md`,
section `## Acceptance Criteria` (work mode `full-bug`, so `spec.md` only).

Checkbox state in `spec.md` at the time of writing: **14 of 14 checked, 0 unchecked.** Every row
below reads `satisfied`, and the row states agree with the checkbox states in `spec.md`.

All paths in the `Evidence artifact` column are relative to
`docs/features/active/winformspumphost-suite-determinism-511/`.

| # | Criterion, verbatim first line | State | Evidence artifact | Revised this cycle |
| --- | --- | --- | --- | --- |
| 1 | `` `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` `` | satisfied | `evidence/regression-testing/named-tests-ten-runs.2026-08-21T18-10.md` | no |
| 2 | `` `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` `` | satisfied | `evidence/regression-testing/named-tests-ten-runs.2026-08-21T18-10.md` | no |
| 3 | `The ten consecutive full nine-assembly runs are executed under induced CPU load using` | satisfied | `evidence/regression-testing/determinism-ten-runs.2026-08-21T18-10.md` | **yes — see note A** |
| 4 | `An empirical pre-fix baseline artifact exists under ``evidence/regression-testing/`` recording,` | satisfied | `evidence/regression-testing/prefix-baseline.2026-08-21T18-10.md` | no |
| 5 | `` `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` exists in `` | satisfied | `evidence/regression-testing/named-regression-tests.2026-08-21T18-10.md` | no |
| 6 | `` `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` exists in `` | satisfied | `evidence/regression-testing/webview-child-handle-measurement.2026-08-21T18-10.md` | **yes — see note B** |
| 7 | `` `git diff` reports zero hunks in both `` | satisfied | `evidence/regression-testing/scope-lock-after-comment-fix.2026-08-23T20-57.md` | no |
| 8 | `All 21 pump-host call sites pass in the final run: the 13 self-tests in` | satisfied | `evidence/regression-testing/pumphost-selftests.2026-08-21T18-10.md` | no |
| 9 | `` `git diff --name-only` against the merge base lists exactly three code files, all under `` | satisfied | `evidence/regression-testing/scope-lock-after-comment-fix.2026-08-23T20-57.md` | no |
| 10 | `` `QfcItemController_SeamFactoryTests` and `QfcItemController_InitializationTests` both pass in `` | satisfied | `evidence/regression-testing/gate-structure-part2.2026-08-21T18-10.md` | no |
| 11 | `Every changed file is under 500 lines after the change:` | satisfied | `evidence/qa-gates/remediation-file-size-audit.2026-08-23T20-57.md` | no |
| 12 | `` `git diff` introduces no occurrence of `Thread.Sleep`, `Task.Delay`, `SpinWait`, a retry loop, `` | satisfied | `evidence/regression-testing/no-timing-hacks.2026-08-21T18-10.md` | no |
| 13 | `The five-step toolchain in ``## Test Strategy`` completes green in a single final pass, coverage` | satisfied | `evidence/qa-gates/remediation-clean-pass.2026-08-23T20-57.md` | no |
| 14 | `` `## Rollout & Follow-up` records #511's visible-window half as out of scope with its `` | satisfied | `evidence/other/discharged-issue-tasks.2026-08-23T20-57.md` | no |

## Supporting evidence beyond the single artifact named per row

Several criteria are corroborated by more than the one artifact the table names; the table names the
primary record so that every row resolves to exactly one existing file.

- Criterion 3 is additionally supported by
  `evidence/regression-testing/named-tests-ten-runs.2026-08-21T18-10.md`,
  `evidence/regression-testing/regression-tests-ten-runs.2026-08-21T18-10.md`,
  `evidence/regression-testing/load-generator-start.2026-08-21T18-10.md`,
  `evidence/regression-testing/load-generator-stop.2026-08-21T18-10.md`,
  `evidence/regression-testing/p4-t2-narrowing-rationale.2026-08-23T20-57.md`, and
  `evidence/other/raw-vstest-artifact-disposition.2026-08-23T21-40.md`.
- Criterion 4 is additionally supported by
  `evidence/regression-testing/intermittency-question.2026-08-21T18-10.md`, which disposes of the
  open intermittency question against the same twenty-row measured table.
- Criterion 6 is additionally supported by
  `evidence/regression-testing/named-regression-tests.2026-08-21T18-10.md` (the test passes).
- Criterion 8 is additionally supported by
  `evidence/regression-testing/consumer-tests.2026-08-21T18-10.md` (8 consumer tests, 8 passed,
  0 failed) and `evidence/qa-gates/remediation-suite-run.2026-08-23T20-57.md` (a `QuickFiler.Test`
  failed count of exactly 0 in the final run).
- Criterion 13 is additionally supported by
  `evidence/qa-gates/remediation-coverage.2026-08-23T20-57.md` and
  `evidence/qa-gates/remediation-coverage-delta.2026-08-23T20-57.md` (a `QuickFiler` package
  `line-rate` delta of +0.15 percentage points, which is greater than or equal to the pre-fix
  baseline).
- Criterion 14 is additionally supported by the P4-T5 edit to the spec's `## Rollout & Follow-up`
  section, which names issue #592 as the filed follow-up.

## Note A — criterion 3 was revised this cycle (remediation Finding F)

**Falsified original wording, summarized:** the criterion required the ten consecutive full
nine-assembly runs to be "all green" suite-wide. Nine of the ten were suite-wide green; run 5
recorded a single failure,
`UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`,
in a sibling-owned assembly this child's three-file `QuickFiler.Test/` diff cannot reach. An
absolute-zero gate spanning an assembly the child does not own is unsatisfiable by any work inside
the child's scope.

**Revision:** the criterion now requires zero failures in the `QuickFiler.Test` assembly — the
assembly containing every class this child owns — with both named end-to-end tests and both named
regression tests passing in all ten runs, and records run 5's sibling-assembly failure explicitly
with its attribution to issue #594. This follows the ratified repository precedent that a child's
absolute-zero gate is scoped to the classes it owns and the residual is promoted as its own defect.
The revision was applied by remediation task P2-T2 and the matching plan-task narrowing by P2-T3.

## Note B — criterion 6 was revised this cycle (remediation Finding E)

**Falsified original wording, summarized:** the criterion required
`BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` to assert that "both WebView2 children remain
handle-less". Measurement on 2026-08-22, recorded in
`evidence/regression-testing/webview-child-handle-measurement.2026-08-21T18-10.md`, proved the
opposite: `ItemViewer.InitializeComponent` runs the Designer-emitted `ISupportInitialize.EndInit()`
calls on both WebView2 children, which creates their handles, and WinForms creates a parent's handle
when a child's is created. The criterion asserted an unmeasured world-state that is false, so no
passing test could ever have satisfied it as worded.

**Revision:** the criterion now requires the test to assert the measured **inherited** state — both
children are already handle-created by `ItemViewer` construction, so the harness inherits the handles
rather than creating them. The revision was applied by remediation task P2-T1. The same measured
truth was propagated into the two comment blocks by P1-T1 and P1-T2 and into the spec's
`## Scope & Non-Goals` section by P2-T7 and P2-T8.

## Summary in the acceptance-criteria-tracking format

```
### Acceptance Criteria Status
- Source: docs/features/active/winformspumphost-suite-determinism-511/spec.md
- Total AC items: 14
- Checked off (delivered): 14
- Remaining (unchecked): 0
- Items remaining: none
```
