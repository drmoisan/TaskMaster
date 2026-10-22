# [P1-T17] [expect-fail] QuickFiler.Test fail-before run

Timestamp: 2026-09-07T07-17

Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults\799-p1-t17 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterScoreJoinTests|FullyQualifiedName~QfcItemController_FolderHandlingTests`

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

`Test Run Failed.` — total 30, passed 25, failed 5. The failing set is the intended Phase 1 red set
for the QuickFiler surface: three new router tests that pin AC3, AC6 and AC7, and two retargeted
folder-handling tests that pin the AC4 removal of the empty-root strip.

## TRX read

TRX file (name reduced per R3): `<user>_<host>_2026-09-07_07_16_50_net481.trx`, the most recently
modified TRX under the results directory for this task. Counter values are read from the
`ResultSummary/Counters` element. No raw TRX content is pasted (R3).

- TOTAL: 30
- PASSED: 25
- FAILED: 5

## Suite selection (R13)

This is a SINGLE-assembly run over QuickFiler.Test driven by an explicit `FullyQualifiedName~`
class filter over two classes. The four environmentally-hanging shell-icon classes live in
UtilitiesCS.Test and are not reachable by this filter or this assembly, so the R13 exclusion
clauses are unnecessary here and the reduced denominator is the 30 tests of the two named classes.
The filter contains no `&` clause, so the `&`-binds-tighter-than-`|` precedence hazard does not
apply.

## Failing tests, by fully qualified name, with the reduced failure reason

### NEW (3)

- QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_RootedScoreAndRelativeRow_RendersThePercentage
  - Reason: the rendered document does not contain the `73%` cell. The score is keyed by the
    archive-rooted path while the presented row is the archive-relative stem, so the join misses.
    This is exactly the AC6 defect; [P2-T11] supplies the additive projected key.
- QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_TrimmedChain_PreservesFilingTargetAndScoreKey
  - Reason: same missing `73%` cell under a chain that begins below the archive root. The filing
    target half of the assertion already holds; the score-key half does not.
- QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_ZeroCandidateLabel_SuppressesTheRowAndKeepsSegmentKeysAligned
  - Reason: `Did not expect document to contain "Stale"` — the zero-candidate label is still
    rendered because no suppression exists yet. [P2-T12] supplies it.

### RETARGETED (2)

- QuickFiler.Controllers.Tests.QfcItemController_FolderHandlingTests.ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection
  - Reason: the empty-archive-root case still strips one leading separator. The retargeted
    expectation is the identity projection, per AC4.
- QuickFiler.Controllers.Tests.QfcItemController_FolderHandlingTests.AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder
  - Reason: `Moq.MockException` — `SetFolderSelectedItem` expected once, observed 0 times. The
    recorded invocations show the production path still calling `FolderContains` with the STRIPPED
    value while the array entry carries the unstripped value, so the selection falls back to
    `SetFolderSelectedIndex(1)`. This is the same AC4 removal observed at the `FolderContains`
    boundary, and it turns green when [P2-T9] routes both sides through the shared projection.

## Acceptance check

- `EXIT_CODE: 1` — met.
- `BindRowsAsync_RootedScoreAndRelativeRow_RendersThePercentage` appears in the failure set — met.
- `ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection` appears in the failure
  set — met.

## New tests that are GREEN at the end of Phase 1 (4), recorded so the inventory is not misread

These four new router tests pin behaviour that already holds today — the no-regression and
restriction cases — so a red result for them would have been a finding rather than a result. They
are NOT part of the [P1-T18] red inventory.

- QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_RootedScoreAndRootedRow_StillRendersThePercentage
- QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_EmptyBoundRoot_LeavesTheJoinUnchanged
- QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_MixedRowSet_RendersLineageOnFolderRowsOnly
- QuickFiler.Test.Controllers.BreadcrumbBridgeRouterScoreJoinTests.BindRowsAsync_AmbiguousLabel_IsNotSuppressed

The remaining 21 passing tests are the untouched tests of
QuickFiler.Controllers.Tests.QfcItemController_FolderHandlingTests.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact. The TRX file
name is recorded with `<user>` and `<host>` substituted. Failure reasons are paraphrased from the
parsed TRX; no raw TRX content is pasted.
