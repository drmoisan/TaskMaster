# Feature Audit — Issue #799 breadcrumb lineage below archive root and suggestion path consistency

- Date: 2026-09-07T20-30
- Branch: `bug/breadcrumb-lineage-below-archive-root-799`
- Base: `2085504e6daaa11b9ec0a8857e7777cf9b10143f` — Head: `7db935b791cf81e0f6df00fef6ef084a8a7a2b4c`
- Work mode marker: `- Work Mode: full-bug` at `issue.md:12`
- Authoritative acceptance-criteria source under `full-bug`: **`spec.md` only**, section Acceptance Criteria,
  lines 834 to 841. There is no `user-story.md` in this folder and the specification states none is to be created.
- The mirrored criteria list under `issue.md` "Proposed Fix / Validation Ideas" is the intake record, not the
  tracked source, and is correctly left unchecked.

## Verdict

**8 of 8 acceptance criteria PASS. The reviewer agrees that all eight are correctly checked off in `spec.md`.**

Two criteria are PASS with a recorded deviation from the specification's own prose (AC6 and AC7). In both cases
the deviation is pre-authorised by a decision in the same document, the escalation condition was independently
verified to hold, and the deviation is recorded by name with its reason under Rollout and Follow-up. Neither
weakens the criterion's substantive requirement.

## Criterion-by-criterion evaluation

### AC1 — PASS

> Suggestion rows and search-result rows in both the QuickFiler item view and the Efc view render the lineage
> starting at the first segment below the archive root, with the same arrow rendering and clickable ancestor
> segments for both row kinds.

**Delivered by:** `ArchiveChainProjection.TryTrimBelowArchiveRoot` applied inside
`OutlookFolderHierarchyProvider.GetAncestorChainAsync` after the snapshot walk and before segment mapping, with
the archive root supplied through a new optional lazy constructor parameter.

**Verified reaches both surfaces.** The reviewer traced the call graph in this worktree rather than accepting the
claim:

- Efc view: `EfcFormController.cs:1053-1056` constructs the provider with `() => _globals.Ol.ArchiveRootPath` and
  hands it directly to `new BreadcrumbBridgeRouter(provider, ...)`. The router's `FetchChainAsync` calls
  `GetAncestorChainAsync`.
- QuickFiler item view: `QfcItemController.BreadcrumbWiring.cs:22-26` constructs the provider with the same
  accessor and passes it to `ItemViewer.InitializeBreadcrumbPipeline`, which stores it without wrapping
  (`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:46-73`).
  `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` at lines 52-60 then calls `ResolveLeafKeyAsync` followed by
  `GetAncestorChainAsync` on that same provider.
- A repository-wide search for `new OutlookFolderHierarchyProvider(` finds exactly these two production sites, so
  no third surface was missed.

Search-result rows already carried the archive-relative stem verbatim through `SplitVerbatim` and are unchanged,
which is what makes the two row kinds agree. Arrow rendering and segment clickability are properties of the
existing renderer and are untouched; the trim removes leading segments from the chain and changes nothing else
about how the remaining segments are mapped.

**Pinned by:** `GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot` (asserts the chain
equals the two below-root paths and explicitly asserts the store path and the Archive path are absent),
`GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot_HappyPath` (the retargeted #439 test,
now asserting the trimmed expectation against a production-shaped construction), and the seven
`ArchiveChainProjectionTests` cases including the `Archive2` false-prefix boundary.

### AC2 — PASS

> A resolved chain that does not pass through the archive root node is logged as an error and rendered with the
> existing single-segment fallback; no row ever shows the mailbox or Archive segment.

**Delivered by:** the `false` return from `TryTrimBelowArchiveRoot` routing into an `EmitError` naming the
configured root, followed by `return Array.Empty<FolderBreadcrumbSegment>()`. An empty segment list routes the Efc
surface into `BreadcrumbRowBuilder`'s empty-chain leaf-only branch and the QuickFiler surface into
`CreateFallbackRow` at `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` line 62, where `chain.Count > 0` is
false.

**The specification's qualification is honoured.** `spec.md` records that "existing single-segment fallback" is
literally true only on the Efc surface, and that converting the QuickFiler multi-segment verbatim fallback to one
segment would be a new regression against the search-row rendering AC1 requires to match. The implementation
correctly does not do that. AC2's substantive requirement — no row shows a mailbox or Archive segment — holds on
both surfaces, because the QuickFiler fallback renders the archive-relative stem.

**Pinned by:** `GetAncestorChainAsync_ChainMissesArchiveRoot_LogsErrorAndReturnsEmpty` (asserts empty chain,
exactly one emission, and that the message names the root),
`GetAncestorChainAsync_LeafIsTheArchiveRoot_LogsErrorAndReturnsEmpty`, and
`TryTrimBelowArchiveRoot_ChainMissesTheRoot_ReturnsFalseAndEmptyOutput`.

Related non-blocking observation CR-3 in the code review: this diagnostic has no once-per gate, unlike the AC7
one. AC2's text imposes no frequency constraint, so this is not a criterion shortfall.

### AC3 — PASS

> Filing target and score-lookup key remain the archive-relative stem (unchanged #439 constraint); filing to the
> selected folder still lands correctly.

**Delivered by construction:** the trim removes leading segments only, so the leaf segment — into which
`BreadcrumbStateModel.Row.WithFilingTarget` and `BreadcrumbRow.FilingTarget` substitute the presented stem — is
untouched. `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` line 68 still passes the presented `path` as the
row's filing value.

**The specification required this be its own assertion rather than an incidental consequence, and it is.**
`BindRowsAsync_TrimmedChain_PreservesFilingTargetAndScoreKey` binds a row whose chain begins below the archive
root, performs a `rowSelected` round trip, and asserts both halves independently:
`SelectedFolderPath.Should().Be(RelativeTarget)` and that the last rendered document still contains `73%`.

Filing landing correctly in a live profile is the human follow-up recorded in Rollout and Follow-up; the
automated half of the criterion is fully pinned.

### AC4 — PASS

> `ProjectSuggestionPath` and `ProjectPredeterminedFolder` are replaced by one shared projection built on
> `ArchiveStemContract.TryMakeArchiveRelative`, and the empty-root one-separator strip is eliminated.

**Both named members now delegate to the same helper.** `FolderPredictor.ProjectSuggestionPath` collapses from a
thirteen-line body to
`return ArchiveStemProjection.ToDisplayStem(folderPath, _globals?.Ol.ArchiveRootPath)!;`, and
`QfcItemController.ProjectPredeterminedFolder` collapses to a one-line delegation to the same member.
`ArchiveStemProjection.ToDisplayStem` is built on `ArchiveStemContract.TryMakeArchiveRelative`, and
`ArchiveStemContract.cs` itself carries no hunk, so the contract is consumed as-is rather than modified.

**The empty-root strip is eliminated, and the elimination is pinned in both directions.**
`ToDisplayStem_EmptyRoot_ReturnsInputUnchanged` and `ToDisplayStem_WhitespaceOnlyRoot_ReturnsInputUnchanged` pin
the new behaviour at the helper. The retargeted assertion in
`QfcItemController.FolderHandlingTests.Part2.cs` flips its expectation from `@"\Archive\Projects\Active"` to
`@"\\Archive\Projects\Active"` with a because-string naming AC4, and the companion test's
`ProjectedSuggestion` constant changes from `@"Projects\Active"` to the raw value. The retargeting rewrote the
assertions and their justifications rather than deleting the tests or leaving them describing the superseded rule,
which is exactly what the specification demanded.

**Site disposition matches decision D-A.** Four sites converted (`ProjectSuggestionPath`,
`ProjectPredeterminedFolder`, the recents pair, and the include-children branch of `GetOlSubpath`); three left
with stated reasons (both wrapper relative-path loaders, for persisted-data round-tripping, and the ToDoModel sort
utility, which is not a Compile item). `FolderMinimalWrapper.cs` carries zero hunks, confirming site 5 was left.

**Side benefit worth recording:** the AC4 change also removes the last behavioural divergence between the two
members. Before the change a null root meant identity while an empty root meant strip-one-separator, which is why
the call site at `QfcItemController.FolderHandling.cs:233` had to distinguish `null` from `string.Empty`. Both now
mean identity, so the two branches of that ternary are behaviourally equivalent and the #678 R2 defect class is
closed rather than merely re-pinned.

### AC5 — PASS

> Recent-folder entries pass through the same projection before display.

**Delivered at both mirror sites.** `FolderPredictor.AddRecents` projects each entry through
`ArchiveStemProjection.ToDisplayStem` before appending to the string list, and `AddRecentRows` does the same
before constructing each `FolderRow`. The specification identified that the issue under-reported this site by
naming only the string append, and that the row mirror is what the breadcrumb surfaces actually consume; both are
converted.

**The documented text-parity contract is now asserted rather than assumed.** The XML documentation on
`FolderRowArray` at `FolderPredictor.cs:234-243` asserts that each row's `Text` equals the corresponding
`FolderArray` string. That contract was previously unasserted for recents and would have broken silently if only
one surface had been projected. `FolderRowArray_AndFolderArray_AgreeOnRecentTextAfterProjection` now pins it.

**Pinned by:** `FolderArray_RootedRecentEntry_IsProjectedToTheArchiveRelativeStem`,
`FolderRowArray_RootedRecentEntry_IsProjectedToTheArchiveRelativeStem`, the parity test above, and
`FolderArray_OutOfRootRecentEntry_IsLeftUnchanged`, which pins the lenient fallback for an entry outside the root.
Each fixture seeds one rooted and one already-relative entry, so the identity case is pinned by the same fixture.

Related non-blocking finding CR-1 in the code review concerns the new unconditional read of `ArchiveRootPath` in
these two members. It does not affect whether AC5 is delivered.

### AC6 — PASS, with a recorded deviation

> `EfcFormController.BindBreadcrumbRowsAsync` projects the score paths the same way as the rows, so
> archive-rooted suggestions retain their percentage.

**Substantive requirement met.** An archive-rooted suggestion presented as a stem now retains its percentage. The
projection is applied inside `BreadcrumbBridgeRouter.WithProjectedScoreKeys`, which is on the path
`BindBreadcrumbRowsAsync` takes: `EfcFormController.cs:1119` calls
`_router.BindRowsAsync(rows, scores, _globals.Ol.ArchiveRootPath, Token)`.

**Two deviations, both recorded by name in `spec.md` Rollout and Follow-up, Outcome item 2.**

1. *Placement.* The projection lives in the router rather than at the controller call site the criterion names.
   Rationale recorded: the controller is 1321 lines and already 2.6 times the 500-line ceiling, so every added
   line worsens a standing violation, while the router already computes and normalizes the bound root and had
   room. Placing it in the router also fixes the join for every caller of the internal overload rather than one
   controller. The reviewer accepts this: it is a strictly wider fix at a lower policy cost, and the criterion's
   observable outcome is unchanged.
2. *Additive rather than substitutive.* The projected score is added alongside the raw score. The recorded reason
   is that `BreadcrumbRowBuilder.BuildProbabilityIndex` assigns through its indexer, so a plain substitution would
   have re-keyed a rooted score onto its stem while the presented text stayed rooted, breaking the case
   `Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively` exercises — and because that test does
   not assert the percentage, the regression would have shipped unnoticed. The reviewer confirmed the indexer
   assignment and agrees this judgment is correct.

**Pinned by:** `BindRowsAsync_RootedScoreAndRelativeRow_RendersThePercentage` (the criterion's own case),
`BindRowsAsync_RootedScoreAndRootedRow_StillRendersThePercentage` (the case a substitution would have broken), and
`BindRowsAsync_EmptyBoundRoot_LeavesTheJoinUnchanged` (the public three-argument overload forwards an empty root,
making the projection the identity, so no existing caller changes behaviour).

Related non-blocking finding CR-2 concerns an alias-shadowing edge case that the additive form tolerates.

### AC7 — PASS, with a recorded and pre-authorised deviation

> Persisted suggestion labels that fail hierarchy resolution are rendered distinguishably (or filtered) and logged
> once per label per session, not once per render.

**Logging half — delivered on both surfaces.** `ResolveByUniqueSuffix` became an instance member so it can reach
the per-instance gate, and its ERROR emission is now wrapped in `if (_reportedLabels.TryAdd(folderPath, 0))`. Both
existing causes stay distinguishable in the message text. Because the gate lives in the provider, and both
production surfaces route through the same concrete provider, both surfaces get it. "Per session" is realised as
"per provider instance", which the specification states explicitly in decision D-B and justifies: a process-wide
static would be mutable global state shared across viewers and across test methods, which the unit-test policy
prohibits, and pooled QuickFiler viewers keep their provider across items, which is the desired behaviour.

**Filtering half — delivered on the Efc surface only. The escalation was verified, not assumed.** Decision D-B
requires the planner to escalate rather than silently edit a sibling-owned file if the QuickFiler presented row
set proves to be composed only inside the sibling-owned bridge router, and specifies the fallback: deliver
suppression on the Efc surface, leave QuickFiler at today's fallback rendering, and record the deviation in
Rollout and Follow-up. The reviewer confirmed the escalation condition holds:

- The QuickFiler drop-down's presented rows are built as the local `built` list inside
  `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` at lines 42-86 and swapped into the model under the shared
  lock at lines 88-96. That file is sibling-owned per decision D-D and carries zero hunks in this diff.
- `QfcItemController.FolderHandling.cs` lines 212 and 221 only hand the predictor's `FolderArray` and
  `FolderRowArray` to the viewer; no provider resolution has occurred at that point, so the zero-candidate
  classification does not yet exist there and cannot be consulted.

The escalation was therefore genuinely warranted, the documented fallback was taken, and the deviation is recorded
by name in `spec.md` Rollout and Follow-up, Outcome item 1, and mirrored in `issue.md`.

**Decision D-B's zero-candidate restriction is honoured.** Suppression is applied only where
`IsAbsentLabel` is true, and that flag is set only on the zero-candidate branch. An ambiguous label — where more
than one node matches and the folder demonstrably exists and is fileable — keeps today's rendering.

**Pinned by:** `ResolveLeafKeyAsync_SameAbsentLabelTwice_EmitsOneErrorAndReportsAbsence` (gate plus absence),
`ResolveLeafKeyAsync_AmbiguousLabel_EmitsOneErrorAndDoesNotReportAbsence` (ambiguity is not absence),
`ResolveLeafKeyAsync_AbsentThenResolvableLabel_ClearsTheAbsenceReport` (the signal resets after a snapshot
refresh), `BindRowsAsync_ZeroCandidateLabel_SuppressesTheRowAndKeepsSegmentKeysAligned` (true arm, with the
suppressed row placed mid-sequence so a misaligned segment-key attachment would fail), and
`BindRowsAsync_AmbiguousLabel_IsNotSuppressed` (false arm).

A fourth recorded deviation belongs to this criterion: the absence classification is published through the new
small `IFolderLabelAbsenceReport` interface declared in the provider's own file rather than as a fourth member on
`IFolderHierarchyProvider`. The reason recorded — net48 has no default interface members, and every router test
uses a strict mock that would throw on an un-set-up new member — is correct, and the reviewer confirmed
`IFolderHierarchyProvider.cs` carries zero hunks and remains at three members.

Related non-blocking finding CR-4 notes that suppression reaches the breadcrumb document but not the parallel
`_folderRows` list surface.

### AC8 — PASS

> The leading-underscore rendering question (`_Active Projects` vs `_ Active Projects`) is verified and, if the
> renderer alters it, corrected.

**The criterion is conditional, and its correct outcome here is no code change.** The verified finding is that no
render path alters a leading underscore, so the antecedent of the conditional is false and no correction is owed.
A renderer change would have been the defect, not the fix. `FILES-CHANGED-FOR-AC8: 0` is recorded in
`evidence/qa-gates/p3-t12-ac8-verification.md`, and `p3-t20-ac8-assets.md` records the separately anchored
confirmation that the QuickFiler resources directory carries no change. The anchored diff contains no `.html`,
`.css` or resource file, which independently confirms it.

**Reviewer independent re-derivation.** Rather than accept the traced analysis wholesale, the reviewer re-checked
one of its five claims directly: `QuickFiler/Resources/FolderBreadcrumb.html` contains exactly seven
`textContent` assignments — lines 253, 262, 266, 299, 309, 327 and 357 — and zero `innerHTML` occurrences. Line
253 is the segment-text assignment, and `textContent` performs no HTML parsing, entity decoding or escaping, so
it cannot introduce a space after an underscore. That matches the evidence artifact exactly.

The remaining four claims — `SplitVerbatim` inserting and trimming nothing, `Formatting.None` JSON escaping only
quote, backslash and control characters, `WebUtility.HtmlEncode` passing the underscore and the space through
unchanged with no `nbsp`, and neither embedded stylesheet containing `letter-spacing`, `word-spacing`,
`text-transform`, `first-letter` or `word-break` — are recorded with file and line citations and a negative
repository-wide search that exited 1 with zero output lines, declared as `ExpectedExitCode: 1` so a passing gate
is not normalised to a failure. The WinForms mnemonic prefix is the ampersand, not the underscore, which excludes
the combo-box path as well.

## Superseded-criteria check

The specification states that this item supersedes #439's full root-to-leaf ancestor lineage and carries forward
#439's filing-target and score-key constraint as AC3. Both statements are borne out by the diff: the #439 lineage
rendering is narrowed by the trim, and AC3 is pinned by its own test. The claim that all ten tests of the #439
Efc router partial class pass unmodified is consistent with the full-run result of 7085 of 7085 passed with
`NEWLY-FAILING: NONE`, and the reviewer independently confirmed the structural reason those tests cannot be
reached by the trim — every provider in both files is a `MockBehavior.Strict` mock supplying the chain directly
through `ReturnsAsync`, below the seam the trim occupies.

## Scope conformance

The branch write set is exactly the twenty paths the specification's Write Set enumerates: nine production, five
new test, two retargeted test, four project files. No file outside the Write Set carries a hunk. The six
sibling-owned files named in decision D-D carry zero hunks, and the two issue-439 test files listed in the Write
Set as "MODIFY, retarget if affected" carry zero hunks, which the specification explicitly permits ("if it proves
unaffected the diff simply contains no hunk for it"). `evidence/qa-gates/p3-t11-scope.md` records the per-path
confirmation with `TRACKED: True` on all fifteen paths asserted absent, which makes each zero-diff a real
observation rather than the trivially empty result of naming a nonexistent path.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799/spec.md
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none
```

No checkbox was changed by this review: all eight were already `[x]` in `spec.md` and all eight evaluate to PASS,
so the reviewer's check-off obligation is already satisfied. The mirrored intake list in `issue.md` under
"Proposed Fix / Validation Ideas" remains unchecked by design, which `issue.md` itself states and which is
correct under the `full-bug` single-source rule.

## Outstanding items

None blocking. Two carried items for the maintainer, neither affecting a criterion:

1. The two follow-up promotions the specification commits to under Rollout and Follow-up — the wrapper
   relative-path loaders, and optionally the mail-item `ResolveFolderRoot` comparison — have no promotion receipt
   in the feature folder. Recorded as CR-7.
2. Live-Outlook manual verification in QuickFiler ordinary and High Confidence modes and in the Efc view remains
   the documented human follow-up. The specification states it does not gate the automated review.

## Path hygiene

No absolute host path, host account name, or machine name appears in this artifact.
