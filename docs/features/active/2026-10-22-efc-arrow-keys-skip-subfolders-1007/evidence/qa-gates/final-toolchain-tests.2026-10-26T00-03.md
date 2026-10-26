# Final Toolchain: Tests with Coverage (Issue #1007, remediation cycle 1, P5-T4)

Timestamp: 2026-10-26T00-03
Command: scripts/vscode/Invoke-MSTestWithCoverage.ps1 (TEST-COVERAGE, against the P5-T3 rebuild)
EXIT_CODE: 0
Output Summary: Test Run Successful. Total 7761, passed 7761, failed 0. The total is the P0-T4 total of 7756 plus the 5 new tests. First-party coverage: lines 58427/67553 (86.49%), branches 14440/17860 (80.85%). Both are above the 80 and 75 percent floors. BASELINE-FAILURES was empty, and no test failed.

## Counts (derived from the trx `ResultSummary` counters)

| Figure | Value |
|---|---|
| Total | 7761 |
| Executed | 7761 |
| Passed | 7761 |
| Failed | 0 |
| Not executed | 0 |
| P0-T4 total + 5 | 7756 + 5 = 7761 (matches) |

## One-line first-party coverage summary

`First-party coverage: lines 58427/67553 (86.49%), branches 14440/17860 (80.85%)`

## Trx-derived list (named tests)

| Test | Kind | Outcome |
|---|---|---|
| Stylesheet_StylesPendingDescendantLikeSelectedSubfolder | new (B-1) | Passed |
| LeafExpandToggle_OnRowCollapsedAwayFromActiveSegment_RestoresChain | new (N-1) | Passed |
| AffordanceToggle_OnRowCollapsedAwayFromActiveSegment_RestoresChainWithoutQuery | new (N-1) | Passed |
| RightArrow_OnRowCollapsedAwayFromActiveSegment_RestoresChain | new (N-1) | Passed |
| OpenUnhandledArrowLeft_OnExpandedPendingRow_RaisesUnhandledArrowFromRouterOutput | new (N-1) | Passed |
| HandleArrowKey_LeftOnNonExpandedMultiSegmentRow_IsNoOpAndKeepsLeafActive | renamed (N-2) | Passed |
| HandleArrowKey_RepeatedLeftOnNonExpandedRow_PostsNoRenderAndKeepsSelection | renamed (N-2) | Passed |
| HandleArrowKey_RightOnExpandedRow_SelectsFirstDescendantUnlessNoChildren | renamed (N-2) | Passed |
| RightArrow_OnRowShortenedAtActiveSegment_ExpandsAndStaysShortened | renamed (N-2) | Passed |
| LeftArrow_RepeatedOnNonExpandedChain_IsUnhandledAndKeepsLeafActive | renamed (N-2) | Passed |
| LeftArrow_FromOpenLeafExpansion_CollapsesThenIsUnhandled | renamed (N-2) | Passed |
| OpenMove_DoesNotChangeCommittedValue | existing AC4 | Passed |
| CommitPending_WithPendingNode_SelectsDescendant | existing AC4 | Passed |
| Cancel_RestoresOriginalAndClearsPendingNode | existing AC4 | Passed |
| OpenLeft_OnNonExpandedEntry_CommitsAndCloses | existing AC4 | Passed |

The post-processed Cobertura document was copied to SCRATCH as `issue-1007-r1-final-cobertura.xml` for P5-T5 and is not committed. The JaCoCo projection was written to `coverage/coverage.cobertura.jacoco.xml`, which P5-T6 consumes.
