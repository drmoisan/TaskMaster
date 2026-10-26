# Test Renames (Issue #1007, N-2, P4-T3)

Timestamp: 2026-10-25T23-59
Command: SCRATCH helper: fixed-string `Select-String` of each of the 12 names over every `.cs` and `.mjs` file in the worktree (1775 files; build output, `.git`, `node_modules`, and nested `.claude` paths excluded)
EXIT_CODE: 0
Output Summary: Each old name has 0 matches and each new name has exactly 1. Only names changed; summaries, attributes, and bodies are unchanged.

## Rename map (plan D5)

| File | Old name | New name |
|---|---|---|
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs | HandleArrowKey_LeftOnMultiSegmentRow_ActivatesParentSegment | HandleArrowKey_LeftOnNonExpandedMultiSegmentRow_IsNoOpAndKeepsLeafActive |
| same | HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior | HandleArrowKey_RepeatedLeftOnNonExpandedRow_PostsNoRenderAndKeepsSelection |
| same | HandleArrowKey_RightAfterExpansion_DescendsByChildActivation | HandleArrowKey_RightOnExpandedRow_SelectsFirstDescendantUnlessNoChildren |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs | RightArrow_OnCollapsedRow_ReExpandsBeforeLeafExpansion | RightArrow_OnRowShortenedAtActiveSegment_ExpandsAndStaysShortened |
| same | LeftArrow_RepeatedOnThreeSegmentChain_WalksToRootThenReportsUnhandled | LeftArrow_RepeatedOnNonExpandedChain_IsUnhandledAndKeepsLeafActive |
| same | LeftArrow_WalkFromAnOpenLeafExpansion_ClearsTheExpansionAndStillReachesTheRoot | LeftArrow_FromOpenLeafExpansion_CollapsesThenIsUnhandled |

## Match counts

| Name | Kind | Matches |
|---|---|---|
| HandleArrowKey_LeftOnMultiSegmentRow_ActivatesParentSegment | old | 0 |
| HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior | old | 0 |
| HandleArrowKey_RightAfterExpansion_DescendsByChildActivation | old | 0 |
| RightArrow_OnCollapsedRow_ReExpandsBeforeLeafExpansion | old | 0 |
| LeftArrow_RepeatedOnThreeSegmentChain_WalksToRootThenReportsUnhandled | old | 0 |
| LeftArrow_WalkFromAnOpenLeafExpansion_ClearsTheExpansionAndStillReachesTheRoot | old | 0 |
| HandleArrowKey_LeftOnNonExpandedMultiSegmentRow_IsNoOpAndKeepsLeafActive | new | 1 |
| HandleArrowKey_RepeatedLeftOnNonExpandedRow_PostsNoRenderAndKeepsSelection | new | 1 |
| HandleArrowKey_RightOnExpandedRow_SelectsFirstDescendantUnlessNoChildren | new | 1 |
| RightArrow_OnRowShortenedAtActiveSegment_ExpandsAndStaysShortened | new | 1 |
| LeftArrow_RepeatedOnNonExpandedChain_IsUnhandledAndKeepsLeafActive | new | 1 |
| LeftArrow_FromOpenLeafExpansion_CollapsesThenIsUnhandled | new | 1 |

Historical names in Markdown documents (earlier plans, evidence, and review artifacts) are left unchanged, because they record the state at the time they were written.
