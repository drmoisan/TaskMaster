# Code Review — Issue #799 breadcrumb lineage below archive root and suggestion path consistency

- Date: 2026-09-07T20-30
- Branch: `bug/breadcrumb-lineage-below-archive-root-799`
- Base: `2085504e6daaa11b9ec0a8857e7777cf9b10143f` — Head: `7db935b791cf81e0f6df00fef6ef084a8a7a2b4c`
- Scope: the full branch diff against the resolved base, twenty paths.
- Verdict: **ACCEPT. 0 Blocking findings, 7 non-blocking findings.**

## Overall assessment

The design is the right one. Placing the archive-root trim inside
`OutlookFolderHierarchyProvider.GetAncestorChainAsync` puts a single change at the one seam both breadcrumb
surfaces route through, which satisfies AC1 and AC2 on both surfaces without touching any of the six files a
concurrent sibling item owns. The two new types are genuinely pure, small, fully documented and fully covered.
The duplication AC4 was written to eliminate is actually eliminated rather than merely wrapped: both former
copies of the stripping expression collapse to one-line delegations to a single shared member, and the empty-root
one-separator behaviour that made them diverge is gone from both.

The change is also unusually well disciplined about not doing more than it should. Three of the seven candidate
stripping sites are deliberately left, each with a stated reason, and the two persisted-data wrapper loaders are
recorded as a real but separate defect rather than folded in. Every null-forgiving `!` in the diff carries an
in-code comment naming the exact diagnostic it suppresses. Every deviation from the specification's own prose is
recorded by name with its reason.

The findings below are all improvements, not defects that block merge.

## Design and structure

**Positive: the lazy accessor is the correct shape and its failure mode is handled.** The archive-root property
throws `InvalidOperationException` when the root is unresolvable — confirmed at
`TaskMaster/AppGlobals/AppOlObjects.cs:260-270`, whose getter calls `ResolveValidatedArchiveRootPath()`. Supplying
it as `System.Func<string>?` rather than a `string` means no construction site gains a throw site, and
`TryReadArchiveRoot` additionally swallows an accessor fault back to "no trim configured" with a `logger.Debug`
and an in-code justification. `GetAncestorChainAsync_RootAccessorThrows_DoesNotThrowAndReturnsTheUntrimmedChain`
pins it. The optional parameter with a `null` default is also a real off switch and keeps roughly twenty existing
provider constructions compiling unchanged.

**Positive: the AC7 interface is separated for a stated, verifiable reason.** Adding a fourth member to
`IFolderHierarchyProvider` would break every implementer on net48 (no default interface members) and would make
every `Mock<IFolderHierarchyProvider>(MockBehavior.Strict)` throw the first time production called it. Declaring
`IFolderLabelAbsenceReport` separately and obtaining it with `provider as IFolderLabelAbsenceReport` at
`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:56` makes suppression inert in every existing router test
without a single edit to those tests. The reviewer confirmed the mechanism: `IFolderHierarchyProvider.cs` carries
zero hunks, and both production construction sites hand the concrete provider straight through with no adapter
(`QfcItemController.BreadcrumbWiring.cs:22-26`, `EfcFormController.cs:1053-1056`,
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:46-73`).

**Positive: the AC7 gate uses two distinct sets rather than one, and the distinction is load-bearing.**
`_reportedLabels` only ever gains entries, which is what makes the diagnostic once per label per provider
instance. `_absentLabels` also loses entries, cleared on both the exact-path route
(`OutlookFolderHierarchyProvider.cs`, after the `match != null` branch) and the suffix-success route, so a label
that becomes resolvable after a snapshot refresh stops being suppressed. Collapsing these into one set would have
been the obvious simplification and would have been wrong. It is pinned by
`ResolveLeafKeyAsync_AbsentThenResolvableLabel_ClearsTheAbsenceReport`.

**Positive: the suppression filters before row construction, not after.** `RetainedRows` is applied to the
presented list before `BuildRows`, and the same retained instance is handed to `AttachSegmentKeys`, with an
in-code comment stating why. Row ids are `row-<index>` over that sequence, so filtering afterwards would have
misaligned every row after the suppressed one. The test deliberately places the suppressed row in the middle of a
four-row sequence and then activates a segment and toggles a leaf on `row-2`, which would fail on a misaligned
attachment. That is a correctly constructed regression pin rather than a shape assertion.

**Positive: `RetainedRows` returns the original instance when nothing was suppressed**, so the common path
allocates nothing and the row identity handed to the builder is unchanged from before this change.

**Positive: segment identity is preserved by reference through the trim.**
`ArchiveChainProjection.TryTrimBelowArchiveRoot` copies existing `FolderBreadcrumbSegment` references into the
output array rather than rebuilding them, and
`TryTrimBelowArchiveRoot_ChainPassesThroughRoot_ReturnsSegmentsAfterTheRoot` asserts `BeSameAs` on two of them.
That rules out a rebuild silently dropping a key or a display name.

**Positive: AC3 is safe by construction and is still pinned explicitly.** The trim removes leading segments only,
so the leaf — and therefore the filing target substituted into it — is untouched. The specification required this
be its own assertion rather than left to inspection, and
`BindRowsAsync_TrimmedChain_PreservesFilingTargetAndScoreKey` asserts both halves: `SelectedFolderPath` equals the
archive-relative stem after a `rowSelected` round trip, and the percentage still renders.

## Findings

### CR-1 — Non-blocking, Low-Medium. The AC5 recents projection creates a narrow new throw site.

**Files:** `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:795` (`AddRecents`) and `:876` (`AddRecentRows`).

**Observation.** Both members now read `var r = _globals.Ol.ArchiveRootPath;` unconditionally inside the
`RecentsList.Count > 0` branch. `ArchiveRootPath` throws `InvalidOperationException` when the archive root is
unresolvable — `TaskMaster/AppGlobals/AppOlObjects.cs:260-270`, documented on the property with an explicit
`<exception>` tag. Before this change neither member read that property.

The reachable state is narrow but real. In `FolderPredictor.FolderArray` at `:221-228`, `AddSuggestions` is
guarded by `Suggestions.Count > 0` while `AddRecents` is guarded by `RecentsList.Count > 0`. `AddSuggestions` in
turn reaches `ArchiveRootPath` only through `ProjectSuggestionPath`, which is invoked per element of
`Suggestions.ToArray(5)`, so with zero suggestions the property is never touched on the old code path. The same
shape holds for `FolderRowArray` at `:249-256`. So in the state (zero suggestions) and (non-empty recents) and
(unresolvable archive root), `AddRecents` is now the first reader and the getter throws where it previously did
not.

**Why this matters more than usual here.** The specification reasoned explicitly and at length about not creating
new throw sites for this exact property — that is the entire justification for the provider's accessor being a
lazy `Func<string>` wrapped in a `try`. The same reasoning was not carried across to the two recents sites.

**Mitigating facts, which are why this is not Blocking.** On the QuickFiler item-view path the surrounding method
already reads the same property a few lines later:
`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:231-234` calls
`ProjectPredeterminedFolder(_predeterminedFolder, _globals is null ? null : (_globals.Ol?.ArchiveRootPath ?? string.Empty))`
unconditionally, and that line is pre-existing context in this diff. So on that surface the change moves an
existing throw a few lines earlier rather than introducing one. The `FolderPredictor.FindFolder` path at `:337-340`
calls `AddSuggestions` unconditionally, but again only reaches the property when suggestions exist, so that path
does gain the new reader. And an unresolvable archive root is a globally degraded application state rather than a
routine one.

**What would close it.** Read the root once through a small private helper that returns `null` on
`InvalidOperationException` — the same treatment `OutlookFolderHierarchyProvider.TryReadArchiveRoot` already
applies — and pass that value to `ToDisplayStem`, which is already lenient about a null root. That is a
three-line change, keeps AC5's behaviour identical in every non-degraded state, and restores the invariant the
specification set for this property. Add one test seeding a predictor whose `ArchiveRootPath` getter throws and
asserting `FolderArray` still returns the recents unprojected.

### CR-2 — Non-blocking, Low. The additive AC6 score alias can shadow a genuine relative-keyed score.

**File:** `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`, `WithProjectedScoreKeys`.

**Observation.** For each score whose path is archive-rooted, a second `FolderScore` carrying the projected stem
is appended immediately after the original. `BreadcrumbRowBuilder.BuildProbabilityIndex` assigns through its
indexer, so the last write for a key wins. If the score sequence ever contains both a rooted entry for a folder
and, later in the sequence, a genuine relative-keyed entry for the same folder with a different probability, the
ordering is safe; if the relative entry comes first and the rooted entry second, the rooted entry's alias
overwrites the relative entry's probability and the row renders the wrong percentage.

**Assessment.** The scorer emits one entry per folder in practice, so a same-folder rooted/relative pair should
not occur. The additive form remains strictly better than substitution, which would have broken the rooted-presented
case outright — decision D7 is correct. This is an edge that the current design tolerates rather than a defect it
introduces.

**What would close it.** Append the aliases as a second pass after all original scores rather than interleaving
them, so an original key can never be overwritten by an alias. Alternatively, add the alias only when no original
score already carries that key. Either is a small change inside the same method, and a test seeding a
relative-then-rooted pair for one folder would pin it.

### CR-3 — Non-blocking, Low. The AC2 chain-misses-root diagnostic has no once-per gate.

**File:** `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`, `GetAncestorChainAsync`, the
`EmitError($"Resolved ancestor chain does not pass through the configured archive root ...")` call.

**Observation.** AC7 exists precisely because a per-render ERROR emission produced eighteen log lines for two
distinct labels in a single session. The new AC2 diagnostic is emitted on every failing render with no gate, so
it reproduces the same emission pattern for a chain that persistently misses the root — a mis-set archive root
would emit once per suggestion row per render.

**Assessment.** AC2's text requires only that the condition be "logged as an error" and imposes no frequency
constraint, so this is compliant as written and is not a criterion failure. It is a consistency observation: the
change fixes one log-spam source and adds a second, smaller one adjacent to it. In normal operation every filing
target is under the archive root, so the path should be unreachable.

**What would close it.** Reuse the existing `_reportedLabels` pattern with a second per-instance set keyed on the
leaf path or on the archive root, so the AC2 error is also once per distinct condition per provider instance.

### CR-4 — Non-blocking, Low. Efc row suppression is applied to the breadcrumb document only.

**Files:** `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` (`RetainedRows`) and
`QuickFiler/Controllers/EfcFormController.cs:1107-1108`.

**Observation.** `EfcFormController` keeps a parallel presented-row surface: `_folderRows = rows ?? Array.Empty<string>()`
followed by `BindFolderRows(_folderRows)`, set on the synchronous path, while the asynchronous
`BindBreadcrumbRowsAsync` at `:1112-1128` hands the same `rows` array to the router. The router's suppression
removes the zero-candidate label from the WebView2 document; it does not remove it from `_folderRows`. If that
list drives any other visible control, a label suppressed on one surface remains present on the other.

**Assessment.** The suppressed label names a folder that no longer exists in the snapshot, so leaving it on a
secondary surface is a cosmetic inconsistency rather than a correctness problem, and the D-B decision explicitly
scopes suppression to where each surface composes its presented row set. The reviewer did not fully trace whether
`BindFolderRows` renders a user-visible list in the current Efc layout.

**What would close it.** Either confirm in a comment that `_folderRows` is not a rendered surface, or route the
same retained list to both consumers.

### CR-5 — Non-blocking, Informational. Comment overstates the number of construction sites.

**File:** `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`, the `TryReadArchiveRoot` summary,
and the same phrase in `OutlookFolderHierarchyProviderTrimTests.cs`: "two of the three construction sites are
outside any try block".

**Observation.** A repository-wide search for `new OutlookFolderHierarchyProvider(` returns exactly two production
call sites — `QfcItemController.BreadcrumbWiring.cs:22` and `EfcFormController.cs:1053` — with the remainder in
test files. The reasoning the comment supports is sound and the design decision is right; only the count is off.

**What would close it.** Reword to "both production construction sites are outside any try block".

### CR-6 — Non-blocking, Informational. Pre-existing `[ExcludeFromCodeCoverage]` carried across the relocation.

**File:** `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs:14`.

**Observation.** `EnsureBreadcrumbPipeline` was moved verbatim out of `QfcItemController.ViewerSetup.cs`,
attribute included. The general unit-test policy's exclusion section is written against configuration `exclude`
entries matching production source paths, and this is a member-level attribute that predates the change, so no
new exclusion is introduced. Recording it so that the attribute's appearance in a newly created file is not later
mistaken for a new exclusion introduced by this item.

**What would close it.** Nothing is required for this change. If the attribute is ever revisited, the member is a
thin wiring helper whose only untestable dependency is the concrete `ItemViewer` type check.

### CR-7 — Non-blocking, Informational. Two committed follow-up promotions are not yet evidenced.

**Source:** `spec.md` Rollout and Follow-up, "Follow-up issues to open" items (1) and (2), and decision D-A sites
5 and 6.

**Observation.** The specification commits to promoting the two wrapper relative-path loaders as their own issue —
describing their unanchored, case-sensitive `Replace` with full-path fallback as "a real defect" that lets a
rooted label enter the persisted classifier corpus — and optionally the unanchored `ResolveFolderRoot` comparison
in the mail-item loading helper. No promotion receipt or follow-up record appears in the feature folder's
`evidence/` tree.

**Assessment.** This is a delivery-hygiene item, not a code defect, and it does not affect any acceptance
criterion. It matters because prose in a feature folder does not survive the merge, whereas an issue does.

**What would close it.** Run both through the promotion lifecycle and record the resulting issue numbers under
`evidence/issue-updates/`.

## Best-practice checklist

| Practice | Verdict | Note |
|---|---|---|
| Single responsibility per new type | PASS | One public member each, both pure. |
| Public API documented | PASS | Every new public member has an XML summary stating contract and every failing path, including the null, empty, whitespace-root, equal-to-root and leaf-is-root cases. |
| Comments explain why, not what | PASS | Consistently. The `WithProjectedScoreKeys` summary explains why addition rather than substitution; the `RetainedRows` summary explains why the filter precedes row construction; each `!` names its diagnostic. |
| Guard clauses over deep nesting | PASS | `TryTrimBelowArchiveRoot` uses `continue` and early `return false` rather than nested conditionals; `ToDisplayStem` is a two-branch guard plus a single condition. |
| No broad `catch (Exception)` without justification | PASS with one justified exception | `TryReadArchiveRoot` catches `Exception` deliberately; the rationale is in the summary and the alternative would create throw sites at call sites outside any try. The pre-existing broad catch in `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` is untouched. |
| Cancellation not swallowed | PASS | The router's suppression predicate explicitly excludes a null chain "arising from cancellation or from a provider fault", so a cancelled row is never treated as absent. |
| No magic values | PASS | Separator handling is delegated to `ArchiveStemContract`, which is unmodified; no new hard-coded `"\\"` prefix arithmetic remains at any converted site. |
| Thread safety where required | PASS | `ConcurrentDictionary` with `TryAdd` / `TryRemove` for both label sets, chosen because the resolve path is async and its continuations are not guaranteed to be on one thread. Documented in-code. |
| Tests assert behaviour, not implementation | PASS | Router tests assert on the rendered document text and on `SelectedFolderPath`; provider tests assert on returned chains and on emission counts through the injected sink. |
| No test asserts a superseded rule against a disabled configuration | PASS | The retargeted provider test is now constructed with a root accessor, which is how production constructs it. |
| File size ceiling | PASS | Largest added file 463 lines; no created or previously-compliant file exceeds 500 after formatting. |

## Summary

Accept. The implementation matches the specification's design, delivers all eight criteria, keeps its footprint
inside the authorised Write Set, and documents every deviation. The seven findings are improvements: CR-1 is the
one worth scheduling, because it slightly weakens an invariant the specification itself established; the rest are
consistency, edge-case and hygiene items.

## Path hygiene

No absolute host path, host account name, or machine name appears in this artifact.
