# [P3-T23] Acceptance-criteria summary

Timestamp: 2026-09-07T08-38

Command: read of `spec.md` lines 834-841 (the authoritative Acceptance Criteria section for this `full-bug` item) against the evidence artifacts named below

EXIT_CODE: 0

ExpectedExitCode: 0

AC source: `docs/features/active/2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799/spec.md`, section "Acceptance Criteria", lines 834-841. Work mode `full-bug`, resolved from the `- Work Mode: full-bug` marker at `issue.md` line 12, so `spec.md` is the sole AC source and no `user-story.md` is required or present.

## The eight rows

| AC | `spec.md` line | Checkbox state | Checked by | Justifying artifact(s) | Named passing evidence |
|---|---|---|---|---|---|
| AC1 | 834 | `- [x]` | [P3-T13] | `<FEATURE>/evidence/regression-testing/p1-t16-ut-fail.md` (fail-before), `<FEATURE>/evidence/regression-testing/p2-t16-pass-after.md` (pass-after), `<FEATURE>/evidence/qa-gates/p3-t5-tests-coverage.md` (final full run) | `GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot`, `GetAncestorChainAsync_WithRootAccessor_ReturnsSegmentsBelowTheArchiveRoot_HappyPath` |
| AC2 | 835 | `- [x]` | [P3-T14] | `<FEATURE>/evidence/regression-testing/p2-t16-pass-after.md`, `<FEATURE>/evidence/qa-gates/p3-t5-tests-coverage.md` | `GetAncestorChainAsync_ChainMissesArchiveRoot_LogsErrorAndReturnsEmpty`, `GetAncestorChainAsync_LeafIsTheArchiveRoot_LogsErrorAndReturnsEmpty` |
| AC3 | 836 | `- [x]` | [P3-T15] | `<FEATURE>/evidence/regression-testing/p2-t16-pass-after.md`, `<FEATURE>/evidence/qa-gates/p3-t11-scope.md` | `BindRowsAsync_TrimmedChain_PreservesFilingTargetAndScoreKey`; the ten `BreadcrumbBridgeRouterIssue439Tests` tests, all passing unmodified |
| AC4 | 837 | `- [x]` | [P3-T16] | `<FEATURE>/evidence/regression-testing/p1-t17-qft-fail.md` (fail-before), `<FEATURE>/evidence/regression-testing/p2-t16-pass-after.md` | `ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection` as retargeted by [P1-T14]; `Issue609_FolderPredictor_ProjectsOnlyInRootFullSuggestionPaths`, `Issue609_FolderPredictor_ProjectsCaseVariantInRootFullSuggestionPath` still green |
| AC5 | 838 | `- [x]` | [P3-T17] | `<FEATURE>/evidence/regression-testing/p2-t16-pass-after.md`, `<FEATURE>/evidence/qa-gates/p3-t5-tests-coverage.md` | the four tests of `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs` |
| AC6 | 839 | `- [x]` | [P3-T18] | `<FEATURE>/evidence/regression-testing/p1-t17-qft-fail.md` (fail-before), `<FEATURE>/evidence/regression-testing/p2-t16-pass-after.md` | `BindRowsAsync_RootedScoreAndRelativeRow_RendersThePercentage`, `BindRowsAsync_RootedScoreAndRootedRow_StillRendersThePercentage`, `BindRowsAsync_EmptyBoundRoot_LeavesTheJoinUnchanged` |
| AC7 | 840 | `- [x]` | [P3-T19] | `<FEATURE>/evidence/regression-testing/p2-t16-pass-after.md`, `<FEATURE>/evidence/qa-gates/p3-t5-tests-coverage.md`, and the deviation record in `spec.md` under Rollout & Follow-up, section Outcome | logging half: `ResolveLeafKeyAsync_SameAbsentLabelTwice_EmitsOneErrorAndReportsAbsence`, `ResolveLeafKeyAsync_AmbiguousLabel_EmitsOneErrorAndDoesNotReportAbsence`, `ResolveLeafKeyAsync_AbsentThenResolvableLabel_ClearsTheAbsenceReport`; suppression half: `BindRowsAsync_ZeroCandidateLabel_SuppressesTheRowAndKeepsSegmentKeysAligned` (true arm), `BindRowsAsync_AmbiguousLabel_IsNotSuppressed` (false arm) |
| AC8 | 841 | `- [x]` | [P3-T20] | `<FEATURE>/evidence/qa-gates/p3-t12-ac8-verification.md`, `<FEATURE>/evidence/qa-gates/p3-t20-ac8-assets.md` | no test; the criterion is satisfied by a verified finding, `EXIT_CODE: 1` with zero matches from the repository-wide negative search and `AC8-ASSET-PATHS-CHANGED: 0` |

Every row names at least one artifact path that exists on disk, and every row's checkbox state matches the
corresponding line in `spec.md`, verified by reading lines 834-841 after the check-offs.

## Check-off notes required by the individual tasks

**AC1 ([P3-T13]).** The trim is applied inside the provider's `GetAncestorChainAsync`, the single seam both the
QuickFiler drop-down (through the UtilitiesCS bridge router's `SetSuggestionsAsync`) and the Efc list (through the
QuickFiler router's `FetchChainAsync`) route through, so one change satisfies AC1 on both surfaces.

**AC2 ([P3-T14]).** AC2's phrase "existing single-segment fallback" is literally true only on the Efc surface,
where the row builder's empty-chain branch renders one leaf-only segment. The QuickFiler surface routes
non-suggestion rows through the verbatim splitter and therefore renders a multi-level stem as several segments.
That is existing behaviour, unchanged by this item, and it already satisfies AC2's substantive requirement that no
row shows a mailbox prefix.

**AC3 ([P3-T15]).** This is an explicit pin rather than an incidental consequence. The structural reason it holds:
the trim removes only LEADING segments, while the filing value is substituted into the LEAF segment, so the two
operate on disjoint ends of the chain and cannot interfere.

**AC4 ([P3-T16]).** Decision D-A site disposition: four sites converted — [P2-T7] `ProjectSuggestionPath`,
[P2-T8] the two recents projections, [P2-T9] the include-children true branch of `GetOlSubpath`, and [P2-T10]
`ProjectPredeterminedFolder` — and three deliberately left: the folder minimal wrapper's relative-path loader and
the folder wrapper source file's loader, whose full-path fallback the persisted relative-path restore branch and
the classifier corpus depend on, and the ToDoModel email-utilities sort file, which is not a Compile item in its
project and has no live caller.

**AC5 ([P3-T17]).** Both the string append in `AddRecents` and the row-model mirror in `AddRecentRows` are
projected. Projecting only one would have broken the text-parity contract documented in
`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`, and the row list is the one the breadcrumb surfaces
actually consume.

**AC6 ([P3-T18]).** Two deviations from the criterion's own wording, both recorded in `spec.md` under Rollout &
Follow-up: the projection is applied in the Efc router rather than at the `EfcFormController` call site the
criterion names, because that controller is 1320 lines and cannot absorb growth while the router already
normalizes the bound root; and the projected score is ADDED alongside the raw score rather than substituted for
it, so the rooted-presented-text join cannot regress.

**AC7 ([P3-T19]).** Three things recorded, as the task requires. First, the filtered branch was taken restricted
to the zero-candidate case, per decision D-B: an ambiguous label emits its error but is NOT classified absent and
is NOT suppressed, pinned by `ResolveLeafKeyAsync_AmbiguousLabel_EmitsOneErrorAndDoesNotReportAbsence` at the
provider boundary and `BindRowsAsync_AmbiguousLabel_IsNotSuppressed` at the router boundary. Second, "per session"
is realized as "per provider instance" and is enforced by a thread-safe per-instance `ConcurrentDictionary` rather
than a static set, because a static set would be process-wide mutable state shared across viewers and across test
methods in one assembly. Third, the escalation branch decision D5 records was taken: **row suppression is
delivered on the Efc surface only, while the QuickFiler surface keeps today's fallback rendering; the logging half
is delivered on both surfaces.** The reason is that the QuickFiler presented row set is composed solely inside the
sibling-owned bridge router, which decision D-B forbids this item from editing. That deviation is recorded in
`spec.md` under Rollout & Follow-up, section Outcome, item 1, which was written before this check-off was made.

**AC8 ([P3-T20]).** AC8 is satisfied by the verified finding that the renderer does not alter a leading
underscore. No code change was made, and a renderer change would have been a defect. The criterion's own wording
is conditional — "verified and, if the renderer alters it, corrected" — so the no-change outcome is the criterion
being met, not the criterion being deferred.

## Acceptance Criteria Status

- Source: `docs/features/active/2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799/spec.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none

Output Summary: Eight rows are present, one per acceptance criterion. Each names the task that checked it and at
least one existing evidence artifact, and each row's checkbox state matches `spec.md`. All eight criteria are
checked off. AC7 carries a recorded deviation — row suppression on the Efc surface only, logging on both — which
was written into `spec.md` before the AC7 box was checked. AC8 is met by a verified no-change finding.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact.
