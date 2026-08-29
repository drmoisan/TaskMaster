# 2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation (Spec)

- **Issue:** #440
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-29
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-bug

> **Sole acceptance-criteria source.** Work mode is `full-bug`, so this file is the only authoritative
> acceptance-criteria source for issue #440 per `.claude/skills/acceptance-criteria-tracking/SKILL.md`.
> No `user-story.md` exists for this feature and none is to be created.

> **This spec supersedes the 2026-08-07 problem statement.** Most of issue #440 was already
> implemented and landed on `main` as a secondary payload of feature
> `docs/features/active/breadcrumb-router-navigation-defects-498/`, whose `spec.md` line 4 reads
> "**Also closes:** #440, #499". The original `issue.md` and the seeded version 0.1 of this spec were
> both written against the pre-#498 tree and are stale. All statements below are against commit
> `b56400ab`. The supporting evidence is
> `docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440/research/arrow-navigation-contract.2026-08-29T00-52.md`,
> which supersedes `issue.md` wherever the two disagree.

## Context

Issue #440 asked that Left and Right arrow keys perform parent/child tree navigation on the two
breadcrumb surfaces: the QuickFiler item folder selector (Qfc) and the EfcViewer folder list (Efc).
Left should select the parent as a node, repeated Left should walk the ancestor chain to the root,
and Right should expand the selected node into its children.

That contract has been delivered on the Efc surface and is partially delivered on the Qfc surface.
The residual defect is Qfc-only and narrow: on Qfc, Left performs the parent-select transition
exactly once, from the leaf. The second Left is reported as unhandled and falls through to legacy
handling, which closes the QuickFiler folder drop-down and destroys the user's navigation context
instead of moving one more level toward the root. Efc walks the whole chain. The two surfaces
therefore do not implement the same contract.

Environment:

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2
- Affected UI path: Qfc only — the ItemViewer breadcrumb folder selector, whose host-neutral state
  machine is `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`
- Unaffected UI path: Efc — the EfcViewer breadcrumb, which already satisfies the contract
- Data or fixture: any suggestion row whose resolved ancestor chain has more than two segments

Impact / Severity:

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Keyboard-only navigation on Qfc can move up exactly one level. A second Left press dismisses the
folder drop-down, so the user must reopen it and retype in the search textbox to reach a grandparent,
sibling, or cousin folder. Severity remains Medium because mouse navigation and the search textbox
are unaffected.

## Repro & Evidence

Preconditions: a Qfc suggestion row whose resolved chain has three segments, for example
`\Inbox` -> `\Inbox\Projects` -> `\Inbox\Projects\Apollo`, with the active segment index at the leaf
(index 2) and no subfolder selected.

Steps to Reproduce:

1. Open QuickFiler and give the folder selector keyboard focus so a suggestion row is selected.
2. Press Left. The parent segment (index 1) becomes the active node. This is correct.
3. Press Left again.

Expected:

- The grandparent segment (index 0, the root of the resolved chain) becomes the active node.
- A further Left, with the root already active, is reported as unhandled and falls through to the
  existing legacy behavior, which closes the folder drop-down.
- The Efc surface behaves identically with respect to the walk, and is not modified.

Actual:

- The second Left is not handled. The transition guard requires the active index to equal
  `row.Chain.Count - 1`, which is false once the first Left has moved the active index off the leaf.
  Control falls through to `TryCollapseLeaf()`, which returns `false` because no leaf expansion is
  open, so the router emits `UnhandledArrowMessage`, which reaches the legacy fall-through and calls
  `SetFolderDroppedDown(false)`. The drop-down closes on the second press instead of after the user
  has genuinely reached the root.

Evidence:

- The one-step limit is codified in a landed test comment at
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` lines 370-371:
  "the first Left consumes the one available #440 parent-select transition, after which nothing
  remains to collapse and no further tree transition applies."
- Press-by-press traces for both surfaces are tabulated in the research artifact, section 2.3.
- No runtime log capture. The defect is fully determined by the host-neutral state machine and is
  reproducible in-process with MSTest.

## Scope & Non-Goals

- In scope:
  - `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` — remove only the leaf-anchored
    clause `activeIndex.Value == row.Chain.Count - 1` from the `LeftArrow()` transition guard, so the
    parent-select transition applies at every non-root position in the chain. Retain every other
    clause in that guard, including `_selectedSubfolderIndex < 0`, and update the adjacent `#440`
    explanatory comment, which currently describes the leaf-anchored behavior.
  - `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` — update the
    existing test `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft`, which currently asserts
    that the second Left is unhandled and therefore encodes the defect. Rewrite it to drive the chain
    to the root first and then assert the unhandled report on the next press, and rewrite its Arrange
    comment so it states the corrected contract rather than the one-step limit.
  - `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` — update the
    existing test `Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges`, which asserts
    `model.LeftArrow().Should().BeFalse()` on the second Left against a three-segment chain and
    therefore encodes the defect. Add the new MSTest coverage for the walk-to-root behavior to this
    file.

- Out of scope / non-goals. The repository paths in this subsection are deliberately written without
  backticks so that the change-footprint harvester does not read them as part of this fix's diff.
  - Any change to the Efc surface, including QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs.
    Reason: the Efc `"Left"` case has no leaf-anchored clause, already walks one step toward the root
    on every press, and is covered green by
    QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs
    `HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior`. Efc already
    satisfies the contract, so there is nothing to fix there.
  - Any change to QuickFiler/Resources/FolderBreadcrumb.html. Reason: the client-side arrow gate was
    already widened by #498 and posts the arrow for any selected suggestion row; the fix is entirely
    below the bridge, in the state machine.
  - Any change to QuickFiler/Controllers/KeyboardHandler.cs. Reason: the legacy fall-through
    behavior is retained unchanged, by decision (see Boundary decisions below).
  - Any change to UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs or
    UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs. Reason: the first is the Efc
    row type and the second is pure transport for the Qfc arrow; neither holds the defective guard.
  - Unifying BreadcrumbRow (Efc) and BreadcrumbStateRow (Qfc), or hoisting a shared transition helper
    across them. Reason: the direction recorded in `issue.md` and copied into version 0.1 of this
    spec — "the two surfaces already share `BreadcrumbRow`" — is factually incorrect. The research
    artifact, section 2.6, verifies that Efc uses BreadcrumbRow and Qfc uses BreadcrumbStateRow, two
    distinct types with different key-attachment invariants. Sharing the transition logic is
    therefore a refactor requiring an adapter over two non-identical row types, with no
    defect-fixing content. #498 decision D9 already ratified expressing each surface's transitions
    through its own landed active-segment seams. **Explicitly rejected.**
  - Adding any new test file. Reason: UtilitiesCS.Test/UtilitiesCS.Test.csproj enumerates 453
    explicit `Compile Include` items, so a new file forces a project-file edit. Two in-flight
    branches touch that project and the conflict cost is not justified by this fix. All new tests go
    into the two existing enumerated test files named in the in-scope list.
  - Changing the Qfc Pop Out / Enumerate Conversation dialog entry point. Reason: `issue.md` itself
    declares this out of scope, and no maintainer decision to the contrary exists in the tree.
  - The Right-descent divergence between the surfaces (Efc commits a filing target through
    `SelectHierarchyPath`; Qfc only moves a highlight through `SelectSubfolder(0)`). Reason: #498
    decision D1 ratified that #440 does not write the Qfc selector session. Recorded as a known
    divergence in Risks, not fixed here.
  - The single-level Right descent limit present on both surfaces. Reason: descending a second level
    requires Up/Down movement within a level, which is owned by the #400 selector session and is out
    of scope per #498 D1. Recorded as a known limitation in Risks.
  - The #498 decision D7 filing-target form, which deliberately keeps the Qfc filing target on the
    presented path rather than the newly-resolved full Outlook path. Reason: changing it silently
    drops suggestion percentages and alters the filing destination.

- Explicitly excluded systems, integrations, or datasets:
  - Microsoft Outlook Interop. The change is confined to a host-neutral state machine; no live
    Outlook process, no COM object, and no mail store is required to reproduce, fix, or verify.
  - WebView2 and the HTML bridge documents for both surfaces. No JavaScript, no HTML resource, and
    no bridge message shape changes.
  - `IFolderHierarchyProvider` and its implementation. No new interface member is required; #498
    AC-16 and AC-30 forbid adding one and forbid reintroducing the retired two-call resolution
    pattern on the expansion path.

## Root Cause Analysis

All eight source citations inherited from the 2026-08-07 code-read are stale; six are wrong about
current behavior. Section 1 of the research artifact ground-truths each one. The analysis below
replaces them.

The Qfc parent-select transition exists and works. It is gated too narrowly. In `LeftArrow()` in
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` the guard is:

```csharp
int? activeIndex = row.ActiveSegmentIndex;
if (
    _selectedSubfolderIndex < 0
    && activeIndex.HasValue
    && activeIndex.Value == row.Chain.Count - 1   // leaf-anchored: the defect
    && row.ActivateSegment(activeIndex.Value - 1)
)
{
    return true;
}
```

The clause `activeIndex.Value == row.Chain.Count - 1` requires the active segment to be the leaf. It
is satisfied only on the first Left press. After that press the active index is `Chain.Count - 2`, so
every subsequent press fails the guard and falls through to `TryCollapseLeaf()`.

The one-step limit is imposed by this clause alone. `BreadcrumbStateRow.ActivateSegment` is already
capable of walking the whole chain: it refuses only a non-suggestion row, a negative index, an index
at or beyond `Chain.Count - 1`, and a no-change index. Removing the leaf-anchored clause therefore
lets `ActivateSegment` itself impose the correct root boundary, with no other change.

The Efc counterpart has no such clause. Its `"Left"` case tests only
`row.ActiveSegmentIndex.HasValue && row.ActivateSegment(row.ActiveSegmentIndex.Value - 1)`, which is
exactly the guard shape this fix produces on Qfc, minus the Qfc-only subfolder-selection term.

The `_selectedSubfolderIndex < 0` clause is a separate and necessary concern. It keeps Left from
performing a parent-select while a child of an open expansion is highlighted, so that Left first
resets the subfolder selection. Dropping it would regress that behavior and break the landed test
`LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses` in
`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs`. The fix must drop only
the leaf-anchored clause.

### Boundary decisions — DECIDED, do not reopen

Both boundary questions that `issue.md` left "to be decided during planning" were already ratified
under feature #498, decision D2, and are locked by its landed acceptance criteria AC-23 and AC-24,
both checked. They are recorded here so a reviewer does not reopen them. Provenance:
`docs/features/active/breadcrumb-router-navigation-defects-498/spec.md`.

- **Left at the root — DECIDED: retain the fall-through on Qfc; retain the silent no-op on Efc.**
  On Qfc, an unhandled Left reaches the legacy fall-through, which issues `SetFolderDroppedDown(false)`.
  That is the only keyboard gesture that dismisses the QuickFiler folder drop-down, so suppressing it
  would strand keyboard users inside an open drop-down. After this fix it fires only once the user has
  genuinely walked to the root, which is the intended behavior. On Efc the boundary is a silent no-op
  and structurally cannot be otherwise: the Efc bridge document has no unhandled-arrow message at all.
  The asymmetry is intrinsic — the Qfc breadcrumb is a drop-down with a close gesture, the Efc
  breadcrumb is the whole form and has none.
- **Right on a childless node — DECIDED: retain the existing fall-through to the Pop Out / Enumerate
  Conversation dialog on Qfc; retain the silent no-op on Efc.** That fall-through is the only keyboard
  entry point to the dialog. `issue.md` itself declares changing it out of scope, and no maintainer
  decision to the contrary exists in the tree.

Neither decision requires a behavior change. Both are preserved as-is.

### Issue #400 reconciliation

No new supersession record is to be authored. Feature #498 already performed the reconciliation and
recorded it in a reviewer-findable section, `#### #400 AC-9 supersession record (reviewer-findable)`,
at `docs/features/active/breadcrumb-router-navigation-defects-498/spec.md` line 304, with execution
evidence at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p7-t7-ac21-supersession-record.md`.
That record retracts, in part, only the "Left and Right preserve the existing breadcrumb expand,
collapse behavior" clause of #400 AC-9, and only for rows whose resolved chain has more than one
segment. It preserves the unhandled-key behavior, the committed/original/pending selector session,
and #400 AC-5 through AC-8 (Up/Down/Enter/Escape). The residual fix specified here falls entirely
inside the already-retracted clause — it is the same retraction applied one step further up the
chain — so it needs no new retraction. Cite the existing record; do not duplicate it.

### Issue #439 lineage dependency — discharged

`issue.md` recorded a dependency on the EfcViewer lineage defect, on the grounds that parent
selection is meaningful only once rows carry a resolved multi-segment ancestor chain, and asked
whether #440 must be scoped to rows whose chain already resolves. Both halves are satisfied on
`main`: #439 landed for Efc, and the analogous Qfc chain resolution landed as #498 decision D5. No
scoping restriction is required.

## Proposed Fix

### Design summary (what changes where):

One boolean clause is removed from one guard in one production method. `LeftArrow()` in
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` loses the leaf-anchored term
`activeIndex.Value == row.Chain.Count - 1`. The remaining guard reads: no subfolder is selected, an
active segment index exists, and `row.ActivateSegment(activeIndex.Value - 1)` succeeds. The root
boundary is then enforced by `ActivateSegment` itself, which refuses a negative index, so Left at the
root continues to return `false` and fall through unchanged. Two existing tests that encode the
one-step limit are corrected, and new coverage for the walk is added.

### Boundaries and invariants to preserve:

- `_selectedSubfolderIndex < 0` remains in the guard. Left with a subfolder highlighted must still
  reset the subfolder selection and collapse rather than perform a parent-select.
- Left at the root of the chain still returns `false`, so the Qfc router still emits the unhandled
  arrow and the legacy fall-through still closes the drop-down.
- Right on a childless node still returns `false`, so the Pop Out / Enumerate Conversation dialog
  remains reachable.
- The Efc surface is byte-identical after this change.
- The Qfc selector session (committed/original/pending) is not written by any Left or Right
  transition, per #498 decision D1.
- No bridge message shape changes, so the Qfc HTML asset contract test remains out of the diff.
- Public API surface is unchanged: `LeftArrow()` keeps its signature and its `bool` contract of
  "true when something changed".

### Dependencies or blocked work:

None. Both prerequisite lineage fixes (#439 for Efc, #498 decision D5 for Qfc) have landed on `main`.
This fix has no unmerged dependency.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` — production. The sole production file
  in this change.
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` — test. Router-level
  correction of one existing test.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` — test. State-level
  correction of one existing test, plus all new walk-to-root coverage.

#### Functions/classes/CLI commands impacted:

- `BreadcrumbStateModel.LeftArrow()` — guard relaxed; the adjacent `#440` comment updated to describe
  a walk rather than a single leaf-anchored step.
- `BreadcrumbStateModel.RightArrow()` and `BreadcrumbStateModel.TryRightTreeTransition()` — not
  modified. Their leaf-anchored condition is a different and correct condition: Right's tree
  transition is available only once a non-leaf node has been selected.
- `BreadcrumbStateRow.ActivateSegment(int)` — not modified. It already imposes the correct root
  boundary.
- No CLI commands.

#### Data flow and validation changes:

None. The change alters only which of two already-existing branches a Left press takes for active
indices strictly between 1 and `Chain.Count - 1`. No new data is read, written, fetched, or
serialized. `IFolderHierarchyProvider` is not called on the Left path.

#### Error handling and logging updates:

None. `LeftArrow()` performs no I/O, throws no exception, and logs nothing. Its false return remains
the sole signal for the unhandled-arrow fall-through.

#### Rollback/feature-flag considerations (if applicable):

Not applicable. No feature flag. Rollback is the inverse one-clause edit and the revert of the two
test corrections.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

`bool LeftArrow()` on `BreadcrumbStateModel`, unchanged in signature. Returns `true` when the model
changed and the caller should re-render; `false` when nothing changed and the caller should report an
unhandled arrow. The corrected contract for a selected suggestion row with a resolved chain of
`N` segments, active index `i`, and no subfolder selected:

| Condition | Result |
|---|---|
| `0 < i <= N-1` | active index becomes `i-1`; returns `true` |
| `i == 0` (root active) | no transition; falls through to `TryCollapseLeaf()` |
| subfolder selected | no transition; resets the subfolder selection; returns `true` |
| no row selected | returns `false` |

#### Required configuration keys and defaults:

None.

#### Backward-compatibility expectations:

No public API change and no serialized-format change. The user-visible behavior change is intended
and is confined to the second and subsequent Left presses on a Qfc row whose resolved chain has more
than two segments. All other keyboard behavior on both surfaces is unchanged.

#### Performance constraints (latency/throughput/memory):

No measurable impact. The change removes one integer comparison from a synchronous, allocation-free
guard evaluated once per key press.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access):
  - The tree is at commit `b56400ab` or a descendant that has not reverted #498. Every line and
    behavior citation in this spec is against that commit.
  - Qfc suggestion rows carry a resolved multi-segment ancestor chain, delivered by #498 decision D5.
  - No live Outlook process, WebView2 host, STA apartment, timer, or temporary file is required to
    reproduce or verify. The affected state machine is host-neutral and `IFolderHierarchyProvider` is
    constructor-injected and already mocked with `MockBehavior.Strict` in the landed tests.

- Constraints (budget, performance, compatibility):
  - Production footprint is exactly one file. Widening it would put this fix inside the blast radius
    of the in-flight branches described below.
  - No new test file, therefore no edit to UtilitiesCS.Test/UtilitiesCS.Test.csproj, which
    enumerates 453 explicit `Compile Include` items.
  - The 500-line-per-file limit is binding on the router test file.
    `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` is currently 495
    lines, so it has under five lines of headroom. Mitigation: confine the change in that file to the
    in-place correction of `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft`, and rewrite its
    Arrange block to use the file's existing `ArrowAsync(router, "left")` helper instead of the
    current inline four-line `RouteAsync` call, which makes the correction net line-neutral or
    line-negative. Place all NEW walk-to-root coverage in
    `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs`, which is currently
    235 lines and has headroom. That file is already a partial of the same test class as
    `BreadcrumbStateModelTests.cs` and reuses its `ThreeSegmentChain` and `ModelWithSuggestion`
    helpers, so no new fixture is needed.
  - Target framework is .NET Framework 4.8.1. Language features requiring `IsExternalInit`
    (`init` accessors, `record`, `record struct`) are unavailable.

- External dependencies (services, libraries, releases):
  - MSTest, Moq, and FluentAssertions, all already referenced by `UtilitiesCS.Test`. No new package.
  - **Two in-flight branches predate this work and own overlapping test files.**
    `feature/quickfiler-breadcrumb-bridge-coverage-r2` (issue #495) and
    `feature/quickfiler-per-file-coverage-capstone-r2` (issue #497) are both cut from a base older
    than #439, #498, #499, and #614, and neither contains any part of the landed #440 implementation.
    Section 7 of the research artifact records the branch SHAs, worktree locations, and the
    file-by-file collision assessment. #495 rewrites the Efc test files that hold the landed #440 Efc
    tests, against a version of the Efc router that no longer exists. Neither branch touches any of
    the three files in this fix's scope. **Mitigation: keep the production footprint to the single
    file `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` and the test footprint to the two
    named `UtilitiesCS.Test` files.** Do not rebase onto, cherry-pick from, or merge either branch.

## Data / API / Config Impact

- User-facing or API changes: one intended behavior change on the Qfc surface. Repeated Left now
  walks the ancestor chain to the root, and the folder drop-down closes only after the root has been
  reached rather than on the second press. No public code API changes.
- Data or migration considerations: none. No persisted state, no schema, no serialized format.
- Logging/telemetry updates (if any): none. The affected method neither logs nor emits telemetry.
- Compatibility notes (CLI flags, config schemas, versioning): none. No CLI flag, no configuration
  key, no version bump.

## Test Strategy

Framework and libraries: **MSTest** (`Microsoft.VisualStudio.TestTools.UnitTesting`) with **Moq** for
`IFolderHierarchyProvider` and **FluentAssertions** for assertions, per the C# Unit Test Policy. This
repository is C#; there is no Python or pytest involvement. (Version 0.1 of this spec carried a stray
"Unit tests (pytest)" line inherited from the scaffold template; it was incorrect and is removed.)

All tests must be deterministic, in-process, and free of Outlook, WebView2, timers, sleeps, and
temporary files. The landed tests in both target files already meet this bar and state so in their
class doc comments.

- Regression tests to add or update:
  - Update `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` in
    `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`. It currently sends
    two Left presses and asserts the second yields an unhandled Left, which encodes the defect. Under
    the fix the second Left is handled. Correct it to walk to the root first — on the three-segment
    fixture that is two Left presses in Arrange — and then assert that the third press yields
    `UnhandledArrowMessage` with direction Left. Rewrite the Arrange comment, which currently states
    "the first Left consumes the one available #440 parent-select transition", to state the corrected
    contract.
  - Update `Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges` in
    `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs`. Its final assertion
    `model.LeftArrow().Should().BeFalse()` is the second Left on a three-segment chain and becomes
    `true` under the fix. Correct it by either extending the sequence to the root before asserting
    the unhandled press, or re-pointing the "nothing changes" assertion at a single-segment row.
    State the chosen rationale in the test comment.
  - Verify unchanged and keep passing without edits:
    `LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses` and
    `Arrows_WithNoSelection_AreUnhandled` in
    `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs`; the #440 and #400
    tests in `BreadcrumbStateModelTests.cs`, `BreadcrumbSelectionSessionTests.cs`,
    `BreadcrumbStateModelSelectorTests.cs`, and the Qfc HTML asset contract tests; and the entire Efc
    router test suite.

- Unit tests (MSTest) for the fixed behavior and boundaries — add to
  `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs`:
  - Repeated Left on a three-segment chain: press 1 returns `true` and sets the active index to 1;
    press 2 returns `true` and sets it to 0; press 3 returns `false` at the root.
  - Assert the active index after each press, not only the boolean, so a test that passes for the
    wrong reason is not possible.
  - Router-level confirmation that the walk is observable through the bridge is provided by the
    corrected `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` in
    `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`, which asserts that
    the unhandled report now arrives only after the root has been reached.

- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
  - Single-segment row: no tree transition is ever available; the pre-existing collapse path runs and
    Left reports unhandled. Already covered; must remain green.
  - No row selected: `LeftArrow()` returns `false`. Already covered; must remain green.
  - Subfolder selected: Left resets the subfolder selection instead of performing a parent-select.
    Already covered by `LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses`; this
    is the test that pins the retention of the `_selectedSubfolderIndex < 0` clause.
  - Root already active: `ActivateSegment(-1)` is refused, the collapse path runs, and Left reports
    unhandled.
  - Open leaf expansion during the walk: `ActivateSegment` incidentally clears the leaf expansion.
    The corrected sequence test must assert the resulting `LeafExpanded` state explicitly rather than
    relying on it implicitly.

- Error handling and logging verification: not applicable. The changed method throws nothing and logs
  nothing; there is no error path to assert.

- Coverage impact and targets for changed lines/modules:
  - The change removes one clause from an already-covered guard, so the changed lines must be fully
    covered by the new and corrected tests. Coverage for
    `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` must not decrease relative to the
    pre-change measurement on the same assembly.
  - Removing a `&&` clause removes a branch, so branch coverage for `LeftArrow()` must be re-measured
    rather than assumed to carry over.

- Toolchain commands to run (format -> lint -> type-check -> test), in this exact order, restarting
  from step 1 if any step fails or modifies files:
  1. `dotnet tool run csharpier format .` and verify with `dotnet tool run csharpier check .`
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
  Use `/t:Rebuild`, not `/t:Build`: MSBuild's up-to-date check does not invalidate on a command-line
  `/p:` change, so a warm `/t:Build` skips `CoreCompile` and the gate cannot fail. When discovering
  test assemblies locally, exclude paths containing `\.claude\` and pass `/InIsolation`, per repo
  convention.

- Manual validation steps (if required): optional confirmation only, not an acceptance gate. In a
  live QuickFiler drop-down against a real mail store, press Left twice on a suggestion row with a
  three-segment chain and confirm that the drop-down no longer closes on the second press and that
  the grandparent segment becomes active. This requires a live Outlook process, a real WebView2
  render, and real focus, so it cannot be automated. Correctness of the change is fully determined by
  the state-machine and router tests.

## Acceptance Criteria

- [ ] AC-1: On the Qfc surface, for a selected suggestion row whose resolved chain has `N` segments
      with the active segment index at the leaf and no subfolder selected, each of the first `N-1`
      consecutive Left presses returns `true` and decrements the active segment index by exactly one,
      ending with index 0 (the root of the resolved chain) active. Verified by an MSTest test that
      asserts the active segment index after every press.
- [ ] AC-2: The clause `activeIndex.Value == row.Chain.Count - 1` is removed from the `LeftArrow()`
      transition guard in `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`, and no other
      conditional in that method is altered. Verified by inspection of the diff for that file.
- [ ] AC-3: The clause `_selectedSubfolderIndex < 0` is retained in the same guard, and the existing
      test `LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses` in
      `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` passes without
      modification. Verified by the diff plus a green run of that test.
- [ ] AC-4: With the root of the resolved chain already active, Left still returns `false`, the Qfc
      router still emits an unhandled Left arrow, and the legacy fall-through that closes the folder
      drop-down is retained unchanged. Verified by the corrected router test and by the absence of
      QuickFiler/Controllers/KeyboardHandler.cs from the diff.
- [ ] AC-5: Right on a node with no children still returns `false` and still reaches the unhandled
      fall-through, so the Pop Out / Enumerate Conversation dialog remains reachable by keyboard.
      Verified by the existing `Route_RightArrow_NothingToExpand_ReportsUnhandledRight` test passing
      unmodified.
- [ ] AC-6: `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` in
      `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` is updated so it
      walks the chain to the root before asserting the unhandled Left, and its Arrange comment no
      longer states the one-step limit. Verified by reading the updated test and its comment.
- [ ] AC-7: `Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges` in
      `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` is updated to the
      corrected contract, with the rationale for the chosen repair recorded in the test comment.
      Verified by reading the updated test and its comment.
- [ ] AC-8: New MSTest coverage for the walk-to-root behavior exists in
      `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs`, uses
      FluentAssertions, follows Arrange-Act-Assert, and creates no temporary file and no Outlook,
      WebView2, or timer dependency. Moq is required only where the test under construction
      exercises a collaborator seam; the walk-to-root transition runs entirely inside an in-memory
      `BreadcrumbStateModel` built by the existing `ModelWithSuggestion()` helper and has no
      collaborator to mock, so introducing a mock there would add a dependency the code under test
      does not take. The mocked `IFolderHierarchyProvider` seam is exercised at the router level by
      the corrected test in `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`.
      Verified by reading the new tests and by a green headless run.

      Provenance of this wording: the original criterion required Moq unconditionally, carried over
      from the validation notes in `issue.md`, which were written against the superseded broad scope
      in which this issue was expected to change the router-level child-expansion path. Under the
      narrowed scope the unconditional clause had no referent. The orchestrator amended it on
      2026-08-29 rather than leave a criterion that no correct implementation could satisfy.
- [ ] AC-9: No behavior change on the Efc surface. QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs,
      QuickFiler/Controllers/BreadcrumbBridgeRouter.cs, and
      UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs are absent from the diff, and the full
      QuickFiler.Test Efc breadcrumb router suite passes unmodified. Verified by the diff file list
      and the test run.
- [ ] AC-10: No new test file is added and UtilitiesCS.Test/UtilitiesCS.Test.csproj is absent from
      the diff. Verified by the diff file list.
- [ ] AC-11: `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` and
      `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` each remain at or
      under 500 lines after the change. Verified by a line count of each file.
- [ ] AC-12: The diff touches exactly three repository files:
      `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`,
      `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`, and
      `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs`, plus this
      feature folder's own documentation and evidence artifacts. Verified by the diff file list.
- [ ] AC-13: The two boundary decisions are recorded in this spec as already decided under #498
      decision D2 / AC-23 / AC-24, and the #400 AC-9 supersession record is cited rather than
      re-authored. No new supersession record is created anywhere in this feature folder. Verified by
      inspection of this spec and the feature folder contents.
- [ ] AC-14: A full C# toolchain pass completes with zero errors in a single final pass, in order:
      `dotnet tool run csharpier check .`; `msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`;
      `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true`; and
      `vstest.console.exe ... /EnableCodeCoverage`. The msbuild steps must be shown to be non-vacuous,
      with no `Skipping target "CoreCompile"` occurrences. Evidence recorded under this feature
      folder's `evidence/qa-gates/` directory.
- [ ] AC-15: Line and branch coverage for
      `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` is not reduced relative to the
      pre-change measurement, and every changed line is covered. Evidence recorded under this feature
      folder's `evidence/coverage/` directory.

## Risks & Mitigations

- Technical or operational risks:
  - **Over-removal of the guard.** Removing `_selectedSubfolderIndex < 0` alongside the leaf-anchored
    clause would regress subfolder-selection reset. Mitigation: AC-2 and AC-3 pin exactly one clause
    for removal and one for retention, and the existing subfolder test must pass unmodified.
  - **File-size limit on the router test file.** At 495 lines it has five lines of headroom, and
    a new test file is out of scope. Mitigation: the router test correction is in-place and reuses the
    existing `ArrowAsync` helper so it is net line-neutral or line-negative; all new coverage goes to
    the 235-line sequence file. AC-11 gates this.
  - **Stale in-flight branch collision.** `feature/quickfiler-breadcrumb-bridge-coverage-r2` (#495) is
    cut from a pre-#439/#498/#614 base and, merged as-is, would silently revert the landed #440 Efc
    implementation, the #439 lineage fix, and the #614 stem guards, with no merge conflict.
    Mitigation for #440: keep the footprint to the three files above, none of which either branch
    touches. Separately, #495 requires a rebuild on `main` rather than a conflict resolution — its
    coverage baseline, line-number citations, and file model are all void. #440 must not attempt that
    rebuild but must not be planned as though #495 will land cleanly beside it.
  - **Known divergence left in place: Right descent semantics.** Efc commits a filing target on
    descent; Qfc only moves a highlight. Making Qfc commit would breach #498 decision D1 and pull the
    #400 selector session back into scope. Mitigation: recorded as a non-goal, not fixed.
  - **Known limitation left in place: single-level Right descent.** Neither surface descends two
    levels with Right alone. Mitigation: recorded as a non-goal; report to the maintainer rather than
    silently expanding scope.
  - **Regression in the reviewer's mental model.** A reviewer unfamiliar with #498 may reopen the two
    boundary decisions or ask for a supersession record. Mitigation: AC-13 and the Boundary decisions
    subsection record both as decided with their #498 provenance.

- Mitigations and rollbacks: rollback is the inverse one-clause edit in
  `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` plus reverting the corrections in
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` and
  `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs`. No data migration, no
  feature flag, and no configuration change is involved, so the revert is complete and immediate.

## Rollout & Follow-up

- Release/rollout steps: merge with the normal add-in build. No configuration change, no migration,
  and no staged rollout. The behavior takes effect on the next add-in load.
- Post-fix monitoring or clean-up tasks:
  - Confirm on first live use that repeated Left walks to the root and that the folder drop-down
    closes only after the root is reached.
  - Track the #495 rebuild-on-`main` requirement separately from this issue.
  - Consider filing the Right-descent divergence and the single-level Right descent limit as their
    own issues if the maintainer wants surface parity beyond the Left contract.
- Links:
  - Issue: https://github.com/drmoisan/TaskMaster/issues/440
  - Research artifact:
    `docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440/research/arrow-navigation-contract.2026-08-29T00-52.md`
  - Prior delivery of the bulk of #440:
    `docs/features/active/breadcrumb-router-navigation-defects-498/spec.md`
  - #400 AC-9 supersession record:
    `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p7-t7-ac21-supersession-record.md`
  - #400 (archived): `docs/features/archive/2026-07-21-quickfiler-folder-selector-dropdown-400/`
