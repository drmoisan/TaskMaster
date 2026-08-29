# Code Review — issue #440 (breadcrumb Left walk-to-root, Qfc)

- Timestamp: 2026-08-29T07-01
- Reviewer: feature-review agent
- Branch: `bug/breadcrumb-left-right-arrow-parent-child-navigation-440`
- Base ref: `b56400ab663a85b6039139d4548f408821e957ce`
- Head ref: `99767554243a7b99a71d2084823d29afcc7127ce`
- Verdict: **PASS** — 0 blocking findings

## Summary

The production change is a single-conjunct deletion from one guard, plus a comment rewrite. It is
the minimal correct fix for the stated defect. The reviewer verified the correctness argument by
reading `BreadcrumbStateRow.ActivateSegment` rather than inferring it from the tests: that method
already refuses a negative index and an index at or beyond `Chain.Count - 1`, so deleting the
leaf-anchored conjunct delegates the root boundary to the one place that already enforces it. This
is the right shape — the guard now expresses only the Qfc-specific precondition
(`_selectedSubfolderIndex < 0`) and the availability of an active index, and lets the row type own
its own bounds.

The test work is proportionate: two new state-level tests that assert the active segment index after
every press, and two in-place corrections to tests that encoded the one-step limit. One of the two
corrections is weaker than it appears and is recorded below as CR-1.

## Files reviewed

| File | Change | Lines | Assessment |
|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | +5/-5 | 248 | Correct, minimal, well-commented |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` | +58/-1 | 292 | Good; two strong new tests |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` | +6/-10 | 491 | Correct but defect-neutral; see CR-1 |

## Production change

The guard after the change:

```csharp
int? activeIndex = row.ActiveSegmentIndex;
if (
    _selectedSubfolderIndex < 0
    && activeIndex.HasValue
    && row.ActivateSegment(activeIndex.Value - 1)
)
{
    return true;
}
```

Design observations:

- **Separation of concerns is improved, not degraded.** Before the change the model duplicated a
  bounds assertion that `BreadcrumbStateRow` already owns. The two could drift. After the change
  there is one owner.
- **The remaining conjunct order is meaningful and preserved.** `_selectedSubfolderIndex < 0` is
  evaluated first and short-circuits, so a Left press with a child highlighted still falls through
  to the reset-and-collapse tail rather than performing a parent-select. The reviewer confirmed the
  landed test `LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses` still passes.
- **The side effect of `ActivateSegment` inside a guard is pre-existing and documented.** The call
  in the condition mutates state (`_activeSegmentIndex`, `LeafExpanded`, `Subfolders`) when it
  returns true. Placing a mutating call in an `if` condition is normally worth flagging, but this
  shape predates the change, is unchanged by it, and the "returns true when the selected node
  changed" contract is documented on the method. Not a finding against this diff.
- **The rewritten comment states why, not what.** It explains that no index test is needed because
  `ActivateSegment` refuses a negative index. That is exactly the non-obvious fact a later reader
  needs in order not to "restore" the deleted conjunct.
- **Public API surface is unchanged.** `LeftArrow()` keeps its signature and its `bool` contract.

Correctness walk-through performed by the reviewer against `ActivateSegment`:

| Chain length | Start index | Presses that return true | First press returning false |
|---|---|---|---|
| 1 | 0 | none — `ActivateSegment(-1)` refused | press 1 |
| 2 | 1 | 1 (to index 0) | press 2 |
| 3 | 2 | 2 (to index 1, then 0) | press 3 |

This matches the contract table in `spec.md` lines 331-336.

## Test change

### New tests

`LeftArrow_RepeatedOnThreeSegmentChain_WalksToRootThenReportsUnhandled` asserts the starting index
is 2, then the index after each of three presses (1, 0, 0) alongside each boolean. Asserting the
index after every press is what makes it impossible for the test to pass on the boolean alone. This
is the strongest test in the change.

`LeftArrow_WalkFromAnOpenLeafExpansion_ClearsTheExpansionAndStillReachesTheRoot` covers the edge
case where the walk begins from an open leaf expansion, and asserts `LeafExpanded` explicitly after
each press rather than relying on it implicitly. The spec called for exactly this
(`spec.md` lines 459-461) and the test delivers it.

Both carry XML doc comments stating the scenario, use FluentAssertions throughout, follow
Arrange-Act-Assert with labelled sections, and take no Outlook, WebView2, timer, or filesystem
dependency.

The absence of Moq in these two tests is correct rather than a gap. `ModelWithSuggestion()` builds
an in-memory model with no collaborator on the Left path; introducing a mock would add a dependency
the code under test does not take. The `IFolderHierarchyProvider` seam is mocked with
`MockBehavior.Strict` at the router level in the sibling file.

### Corrections

`Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges` was extended to the root and now
asserts index 1 and index 0 between the presses. It is defect-detecting: reverting the production
change makes press 2 return false and the test fails.

`Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` is addressed by CR-1.

## Findings

| ID | Severity | Blocking | File:line | Summary |
|---|---|---|---|---|
| CR-1 | Major | No | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs:368-385` | The corrected router test is defect-neutral, not defect-detecting: it would also pass against the unfixed production code |
| CR-2 | Minor | No | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs:83` | The terminal unhandled press carries no accompanying active-index assertion, unlike every other press in the same block |
| CR-3 | Minor | No | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs:73-77` | A five-line planning-decision rationale is embedded in a test body, duplicating plan decision D1 |
| CR-4 | Info | No | both edited test files | Neither test file carries `#nullable enable`, so the new test code sits outside nullable analysis |
| CR-5 | Info | No | repository-wide | The repo-wide line coverage margin above the 85% floor is roughly 0.29 pp and jitters about 0.016 pp between runs |

### CR-1 — Major, non-blocking

`Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` now reads:

```csharp
var router = await PopulatedRouterAsync(ProviderMock());
await ArrowAsync(router, "left");   // line 374, result discarded
await ArrowAsync(router, "left");   // line 375, result discarded

// Act
var outputs = await ArrowAsync(router, "left");

// Assert
outputs.Should().ContainSingle();
((UnhandledArrowMessage)BreadcrumbBridgeSerializer.Parse(outputs[0]))
    .Direction.Should()
    .Be(BreadcrumbArrowDirection.Left);
```

The two Arrange presses are issued and their results discarded. Nothing asserts that press 1 or
press 2 was **handled**. Against the unfixed production code the sequence is: press 1 handled
(leaf-anchored parent-select), press 2 unhandled, press 3 unhandled. The Act press therefore still
yields exactly one `UnhandledArrowMessage` with direction Left, and the assertions still hold. The
test passes both before and after the fix.

Corroborating structural evidence, not merely an argument from reading: the fail-before run
`evidence/regression-testing/p1-t4-fail-before.2026-08-29T06-30.md` records `Total tests: 2` and
names only the two new state-level tests. No fail-before evidence exists for this router test,
which is consistent with it not going red against the unfixed tree.

Why this is not blocking: AC-6 requires only that the test walk the chain to the root before
asserting the unhandled Left and that its Arrange comment no longer state the one-step limit. Both
are satisfied, and the criterion does not ask for a router-level regression guard. The walk itself
is pinned at the state level by the two new tests, whose fail-before/pass-after pair is recorded.
The correction removed a test that asserted the defect; it simply did not replace it with one that
asserts the fix.

Recommended follow-up, for a later cycle rather than this one: assert that press 2 is handled at the
router level, for example by capturing the second `ArrowAsync` result and asserting it contains a
render rather than an `UnhandledArrowMessage`. That would make the router-level test detect a
regression of the walk. Alternatively, extend
`ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition` (line 443) with a second press.
Today no router-level test asserts that a second consecutive Left is handled; the reviewer
enumerated all five `ArrowAsync(router, "left")` call sites in the file (lines 374, 375, 378, 450,
479) to confirm this.

### CR-2 — Minor, non-blocking

In `Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges` the first two presses assert
`ActiveSegmentIndex` (1, then 0) but the terminal `model.LeftArrow().Should().BeFalse();` at line 83
does not assert that the index remains 0. The dedicated new test does make that assertion, so the
behavior is covered; this is a local consistency point only.

### CR-3 — Minor, non-blocking

Lines 73-77 embed a five-line justification for a planning decision (why the sequence was extended
rather than re-pointed at a single-segment row) inside the test body. AC-7 explicitly requires the
rationale to live in the test comment, so the executor was following instructions. The observation
is that planning-decision provenance in a test body ages poorly once the plan is archived; a one-line
"see plan decision D1" plus the contract statement would carry the same information. No action
required.

### CR-4 — Info

`UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` carries `#nullable enable` at line 1, so
the nullable gate is genuinely live for the production change. Neither
`BreadcrumbStateModelSequenceTests.cs` nor `FolderBreadcrumbBridgeRouterTests.cs` carries the
pragma, so the new test code is outside nullable analysis. This is pre-existing, consistent with the
repository's per-file opt-in model, and adding the pragma to a 491-line pre-existing test file would
breach this change's declared footprint. Recorded for awareness only.

### CR-5 — Info

Repo-wide line coverage reads 85.3026% from the executor's run and 85.2870% from the reviewer's
independent reproduction, against an 85% floor. The margin is roughly 0.29 pp and the run-to-run
jitter is roughly 0.016 pp. The floor is not at immediate risk, but a future change that deletes a
few hundred covered lines without deleting the corresponding uncovered ones could cross it. Worth
knowing when sizing later refactors; no action now.

## Verdict

**PASS — 0 blocking findings.** The production change is minimal, correct, and better factored than
what it replaces. CR-1 is a genuine weakness in the router-level regression protection and is worth
a follow-up, but it does not breach any acceptance criterion or repository policy, and the behavior
it fails to guard is guarded at the state level with fail-before/pass-after evidence.
