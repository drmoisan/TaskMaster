# Acceptance Criteria Status

Timestamp: 2026-08-27T14-32
Task: [P7-T34]
Command: count of `[x]` checkboxes in the `## Acceptance Criteria` section of `FF/spec.md`
EXIT_CODE: 0

Authored per `.claude/skills/acceptance-criteria-tracking/SKILL.md`. Work mode is `full-bug`, so
`spec.md` is the sole acceptance-criteria source. `user-story.md` is correctly absent.

### Acceptance Criteria Status
- Source: `docs/features/active/quickfiler-home-controller-metrics-442/spec.md`
- Total AC items: 25
- Checked off (delivered): 24
- Remaining (unchecked): 1
- Items remaining: **AC-19 (ownership boundary)**

## Per-criterion record

| AC | State | Evidence pointer(s) | Verifying test or command |
| --- | --- | --- | --- |
| AC-1 | `[x]` | `evidence/regression-testing/efc-metrics-red.2026-08-26T11-06.md`, `qfc-stopwatch-red.2026-08-26T11-14.md`, `qfc-flush-red.2026-08-26T11-19.md`, `fail-before-exception.2026-08-26T11-10.md` | every root cause RC-1 to RC-9 carries a named red observation or is covered by the fail-before exception dossier |
| AC-2 | `[x]` | `evidence/regression-testing/qfc-flush-green.2026-08-26T11-23.md` | `WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce` — Passed |
| AC-3 | `[x]` | `evidence/regression-testing/qfc-flush-green.2026-08-26T11-23.md`, `evidence/qa-gates/qfc-flush-search-census.2026-08-26T11-23.md` | `WriteMetricsAsync_CompletesWriterTaskBeforeReturning` — Passed; `git grep -nE "NonBlockingProducer\|TimedConsumerAsync\|_metricsConsumers\|_lockObject\|_fileName" QuickFiler/Controllers/` returns zero matches (exit 1) |
| AC-4 | `[x]` | `evidence/regression-testing/qfc-flush-green.2026-08-26T11-23.md` | `WriteMetricsAsync_PassesUncancelledTokenToWriter` — Passed |
| AC-5 | `[x]` | `evidence/regression-testing/qfc-flush-green.2026-08-26T11-23.md` | `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` — Passed |
| AC-6 | `[x]` | `evidence/regression-testing/qfc-stopwatch-green.2026-08-26T11-16.md` | `WriteMetricsAsync_ReadsMovedStopwatchForDuration` — Passed |
| AC-7 | `[x]` | `evidence/qa-gates/qfc-stopwatch-search-census.2026-08-26T11-16.md`, `evidence/regression-testing/efc-metrics-green.2026-08-26T11-09.md` | `BuildQuickFileMetricLines_WithNinetySeconds_RendersUntruncatedDuration` — Passed; `git grep -n "Elapsed.Seconds" QuickFiler/Controllers/` returns zero matches |
| AC-8 | `[x]` | `evidence/qa-gates/qfc-stopwatch-search-census.2026-08-26T11-16.md` | `QfcHomeController.Metrics.cs:141` reads `OlStartTime = OlEndTime.Subtract(_stopWatchMoved.Elapsed);`; `git grep -n "(int)Duration"` on that file returns zero matches. Verified by inspection, as the criterion provides, because `UtilitiesCS.Calendar.GetCalendar` returns `null` in every unit fixture |
| AC-9 | `[x]` | `evidence/regression-testing/efc-metrics-green.2026-08-26T11-09.md`, `evidence/qa-gates/efc-search-census.2026-08-26T11-09.md`, `evidence/qa-gates/efc-stopwatch-site-reachability.2026-08-26T11-31.md` | `StopWatch_AfterControllerConstruction_IsRunning` — Passed; `git grep -n "Stopwatch.StartNew" QuickFiler/Controllers/EfcHomeController.cs` returns both `_stopWatch` construction sites, `:76` and `:225` |
| AC-10 | `[x]` | `evidence/qa-gates/efc-search-census.2026-08-26T11-09.md`, `evidence/qa-gates/msbuild-nullable.2026-08-27T14-18.md` | `double elapsedSeconds` declared at `EfcHomeController.Metrics.cs:63` and `:85`; `git grep -n "int elapsedSeconds" QuickFiler/` returns zero matches; the nullable/type-check gate is exit 0 with zero errors. **Line-number note:** the criterion names `:35` and `:57`, which were the declaration lines at spec time. The file grew, so the declarations now sit at `:63` and `:85`. The substance — both parameters declared `double`, no `int` overload surviving — is verified; the criterion's line numbers are stale, not its requirement |
| AC-11 | `[x]` | `evidence/regression-testing/efc-metrics-green.2026-08-26T11-09.md`, `evidence/other/pr-body-statements.2026-08-26T11-31.md` §5 | `BuildQuickFileMetricLines_WithMultipleMovedItems_PinsRealDivisionRounding` — Passed; the rounding change is stated in the PR body |
| AC-12 | `[x]` | `evidence/regression-testing/efc-metrics-green.2026-08-26T11-09.md`, `evidence/qa-gates/efc-search-census.2026-08-26T11-09.md` | `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine` and `BuildQuickFileMetricLines_RendersTwelveCommaSeparatedFields` — both Passed; the substring `,Recipient,Sender,` is asserted; `git grep -n "RecipientSender" QuickFiler.Test/` returns zero matches |
| AC-13 | `[x]` | `evidence/regression-testing/efc-metrics-green.2026-08-26T11-09.md` | `BuildQuickFileMetricLines_WithEmbeddedCommas_StillRendersTwelveFields` — Passed; `xComma` applied at four sites in `EfcHomeController.Metrics.cs` |
| AC-14 | `[x]` | `evidence/regression-testing/efc-reentrancy-green.2026-08-26T11-12.md`, `evidence/regression-testing/fail-before-exception.2026-08-26T11-10.md` | `TryBeginExecuteMoves_SecondCallBeforeReset_ReturnsFalse` and `TryBeginExecuteMoves_AfterResetExecuteMovesState_ReturnsTrue` — both Passed; `private int _isExecuting;` at `EfcHomeController.cs:393`; `git grep -n "volatile" QuickFiler/Controllers/EfcHomeController.cs` returns zero matches |
| AC-15 | `[x]` | `evidence/qa-gates/efc-search-census.2026-08-26T11-09.md`, `evidence/regression-testing/efc-metrics-green.2026-08-26T11-09.md` | `QuickFileMetricsWriteFilenameOnly_WithAbsentPrerequisites_DoesNotThrow` and `QuickFileMetricsWriteFilenameOnly_WithPrerequisites_DelegatesToThreeArgumentOverload` — both Passed; `git grep -n "NotImplementedException" QuickFiler/Controllers/EfcHomeController.Metrics.cs` returns zero matches; `QuickFiler/Interfaces/IFilerHomeController.cs` is unmodified against the merge base |
| AC-16 | `[x]` | `evidence/regression-testing/efc-metrics-green.2026-08-26T11-09.md`, `evidence/regression-testing/qfc-stopwatch-green.2026-08-26T11-16.md` | `BuildQuickFileMetricLines_UnderGermanCulture_RendersInvariantDecimalSeparator` and `WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator` — both Passed; six `CultureInfo.InvariantCulture` sites (four in `QfcHomeController.Metrics.cs`, two in `EfcHomeController.Metrics.cs`) |
| AC-17 | `[x]` | `evidence/qa-gates/test-determinism.2026-08-27T14-19.md` | the banned-construct search over both owned test files returns zero hits (exit 1) |
| AC-18 | `[x]` | `evidence/other/pr-body-statements.2026-08-26T11-31.md` §6, `evidence/other/pr-body-statements-addendum.2026-08-27T14-30.md` | all four deliberately broken tests carry a stated disposition; both deleted tests are confirmed absent from the [P6-T5] run log |
| **AC-19** | **`[ ]` NOT MET** | `evidence/qa-gates/ownership-gate.2026-08-27T14-03.md`, `evidence/qa-gates/changed-file-inventory.2026-08-27T14-32.md` | see the dedicated section below |
| AC-20 | `[x]` | `evidence/qa-gates/project-file-gate.2026-08-27T14-03.md` | all three gate commands return zero output lines against the merge base |
| AC-21 | `[x]` | `evidence/qa-gates/owned-file-line-counts.2026-08-27T14-19.md`, `evidence/qa-gates/qfc-test-file-size.2026-08-26T11-26.md` | all seven owned files at most 499 lines; `QfcHomeController.cs` at 449 against its pre-change 487; `QfcHomeControllerMetricsTests.cs` at 453 |
| AC-22 | `[x]` with a stated exception | `evidence/baseline/mstest-coverage.2026-08-26T10-42.md`, `evidence/qa-gates/mstest-coverage.2026-08-27T14-19.md`, `evidence/qa-gates/coverage-delta.2026-08-27T14-19.md` | see the exception note below |
| AC-23 | `[x]` | `evidence/qa-gates/csharpier-check.2026-08-27T14-18.md`, `msbuild-analyzers.2026-08-27T14-18.md`, `msbuild-nullable.2026-08-27T14-18.md`, `mstest-coverage.2026-08-27T14-19.md`, `toolchain-loop.2026-08-27T14-18.md` | final pass: exit 0 at every step, zero files rewritten by the formatter, 6701 tests / 6701 passed / 0 failed |
| AC-24 | `[x]` | `evidence/other/pr-body-statements.2026-08-26T11-31.md` §1 to §4, `evidence/other/pr-body-statements-addendum.2026-08-27T14-30.md` | the four required statements are recorded for the PR body |
| AC-25 | `[x]` | `evidence/issue-updates/cross-feature-notes-handoff.2026-08-26T11-32.md`, `evidence/issue-updates/cfn4-promotion-complete.2026-08-27T13-59.md` | CFN-4 promoted to issue #645, verified `OPEN`, number written back into the CFN-4 section of `spec.md`; CFN-1 and CFN-3 directed to 446 and CFN-2 to 468, none fixed here |

## AC-19 — the single outstanding item

AC-19's first sentence requires `git diff --name-only <merge-base>..HEAD` to list **only** the five
owned production files, the two owned test files, and paths under
`docs/features/active/quickfiler-home-controller-metrics-442/`. The diff carries an eighth source
path: `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`, one line, commit `889fa298`.

AC-19's second sentence — that `QfcHomeController.Iteration.cs`,
`QfcFormController.EventHandlers.cs`, `QfcCollectionController.cs`, `EfcFormController.cs`,
`IFilerHomeController.cs` and `EfcHomeControllerDependencies.cs` are unmodified — **is** satisfied;
all six return zero output lines against the merge base.

The criterion is therefore partially satisfied, and a partially satisfied criterion is left
unchecked. The write itself is a parent-ratified documented deviation, fully reasoned in
`evidence/qa-gates/ownership-gate.2026-08-27T14-03.md`: AC-14 makes `_isExecuting` a `private int`,
`FieldInfo.SetValue` cannot inject a `bool` into an `int` field, and no production-side change can
reconcile the two, so the choice was between an unfixable failing test and one line in a file whose
ban rationale (protecting concurrent epic siblings) provably does not apply to it. The deviation is
disclosed in the PR body; the criterion is not claimed.

`[P7-T6]` and `[P7-T27]` are correspondingly left unchecked in the plan.

## AC-22 — checked, with the exception called out per CLAUDE.md § UT5

AC-22 has four requirements. Three are met without qualification:

- toolchain step 4 ran with coverage enabled;
- **no line changed by this feature lost coverage**: changed-line coverage is 39 of 39, 100.00%;
- the repository-wide figure is recorded alongside the merge-base baseline and moved **up**,
  84.8433% to 85.1255% on lines and 78.8181% to 79.2096% on branches.

The fourth requires the six members named in the spec's Test Strategy to reach at least 90%. Five
are at 100.00%: `BuildQuickFileMetricLines`, `SelectMoveMetricsItems`, `TryBeginExecuteMoves`,
`ResetExecuteMovesState` and `WriteMetricsAsync`. The sixth, `QuickFileMetrics_WRITE`, aggregates
**88.37%** (76/86).

The entire shortfall is ten lines of inline Outlook `AppointmentItem` creation in the QFC overload,
guarded by `UtilitiesCS.Calendar.GetCalendar("Email Time", Globals.Ol.App.Session)`, which returns
`null` in every unit fixture. That overload reads 39/49 in the baseline document and 39/49 in the
post-change document — identical, so this feature neither caused nor worsened it. Excluding those
ten Interop lines the member measures 76/76 = 100.00%. The spec's own Test Strategy scopes the
COM/VSTO exemption to "Outlook-Interop-bound members of the same classes" while holding the metrics
writers' injectable seams to the floor; the uncovered block is a direct Interop call sequence, not a
seam.

AC-22 is checked on that basis, and the exception is stated here, in
`evidence/qa-gates/coverage-delta.2026-08-27T14-19.md`, and in the PR body, rather than absorbed
silently.
