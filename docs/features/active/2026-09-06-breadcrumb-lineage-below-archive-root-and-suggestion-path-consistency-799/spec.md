# 2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency (Spec)

- **Issue:** #799
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-06T18-40
- **Status:** Implemented
- **Version:** 1.0
- **Work Mode:** full-bug. This spec is the sole authoritative acceptance-criteria source. There is no
  user-story.md in this folder, and none is to be created.

## Path notation convention

Backticks in this document are reserved for repository paths that this change will actually create or
modify. Every one of those paths appears in the Write Set section below, written with forward slashes.
Every other file reference in this document — comparisons, precedents, citations, out-of-scope files,
and files this change deliberately leaves alone — is written as plain prose with no backticks,
including its File.cs:123 line citation.

This is not a style preference. Get-BlastRadius in .claude/lib/blast-radius/BlastRadius.psm1 is given
both the atomic-plan text and this spec text, and it harvests path tokens from inline code spans in
both. The extraction and classification rules are in .claude/lib/blast-radius/BlastRadiusExtraction.psm1:
Get-InlineCodeToken splits every backtick span on whitespace, and Get-PathTokenKind accepts a token as
a concrete repository path when it contains a forward slash, is not absolute, carries no placeholder
marker, and ends in a recognized extension such as cs, csproj, md, ps1 or json. An accepted token is
added to this item's blast radius exactly as if the change had written that file, which makes this item
contend with concurrent sibling items that touch the same file. The extractor has no notion of
polarity: a backticked path inside a sentence saying the change will not touch it is read as a write
claim in exactly the same way.

Two consequences are load-bearing for this item and are applied throughout:

- No path in this document is written with angle brackets, dollar-brace, dollar-paren, or a percent
  sign when a real write claim is meant. Write Set entries are concrete.
- The two repository paths that contain a space are not in the Write Set and carry no backticks. They
  are restated in words in the Write Set section.

## Specification change, not a regression

The behaviour this specification removes — the full root-to-leaf ancestor lineage rendered on
suggestion rows, beginning at the mailbox store node — is exactly what issue #439 delivered. It was
correct against #439's Expected Behavior, and it is being deliberately superseded here, not repaired.

A later reader must not read this change as a regression fix against #439. The maintainer settled the
new rendering rule on 2026-09-06: for every row kind, on both surfaces, the lineage begins at the first
segment below the archive root.

The #439 constraint that is **not** changing is that the filing target and the score-lookup key remain
the archive-relative stem. Trimming affects display segments only. That constraint is carried forward
unchanged as AC3 in the Acceptance Criteria section below, and it is the invariant that every part of
the design is checked against.

Because #439's behaviour is currently pinned by existing tests, those tests must be **retargeted**, not
merely extended. Retargeting means the assertion is rewritten to encode the new rule against a provider
configured the way production configures it; deleting the test or leaving it asserting the superseded
rule against a deliberately disabled configuration are both unacceptable, because either outcome leaves
the suite pinning nothing. The tests concerned are named in the Write Set section under test sources:
the provider chain test, the #439 Efc router tests, and the AC4 empty-root assertion in the
QuickFiler item-controller folder-handling tests.

## Context

In QuickFiler (ordinary and High Confidence modes) suggestion rows render their breadcrumb lineage from
the store root — mailbox, then Archive, then _Active Projects, then Build RGF Org and Team, then Sales
Lead — while typed search-result rows render only _Active Projects, Build RGF Org and Team, Sales Lead.
The mailbox and Archive segments are superfluous: every filing target is under the archive root, and
the lineage must begin at the first segment below it for every row kind. The same change unifies
several archive-root stripping paths that have drifted into duplicates and gaps.

Throughout this document the word "mailbox" stands in for the account name of the default Outlook
store. The literal address is not reproduced here.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO Outlook add-in with a WebView2 breadcrumb; debug build from
  TaskMaster\bin\Debug at HEAD c431dc32 (2026-09-06)
- Command/flags used: Outlook ribbon, QuickFiler and QuickFiler High Confidence; the folder drop-down
  in the item view (the ItemViewer breadcrumb, backed by the FolderBreadcrumb.html page asset)
- Data source or fixture: live mailbox. The archive root resolves to the default store root followed by
  the literal folder Archive, exposed as AppOlObjects.ArchiveRootPath.

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Display-only: filing lands correctly. The redundant segments consume most of the row width and make
same-named folders harder to distinguish, which is the problem #439 set out to solve.

## Repro & Evidence

Steps to Reproduce:
1. Launch QuickFiler on Inbox. Open the folder drop-down on an item without typing.
2. Observe the first suggestion row: mailbox, then Archive, then _Active Projects, then Build RGF Org
   and Team, then Sales Lead.
3. Type a few letters into the search box. Observe the same folder as a search result: _Active
   Projects, then Build RGF Org and Team, then Sales Lead. Typing 90 shows _Active Projects, then 90
   Day Plan.
4. Accept a suggestion: filing lands in the correct Outlook and file-system folders. The filing target
   is the archive-relative stem, not the displayed lineage.

Note carried from the issue: the maintainer's transcription of the search row showed a space after the
leading underscore. That question is resolved in decision D-C below and by AC8.

Expected:
- Every row in the folder list, suggestion or search result, renders beginning at the first segment
  below the archive root, each segment is clickable for ancestor navigation, and no row shows the
  mailbox or Archive segments.
- A resolved ancestor chain that does not pass through the archive root node is logged as an error and
  rendered with the existing single-segment fallback, never with a mailbox prefix. There are no
  legitimate filing targets outside the archive root.
- One archive-root projection rule is used everywhere a suggestion path is prepared for display or
  compared against displayed entries.

Actual:
- Suggestion rows show the mailbox and Archive segments; search rows do not. Ordinary and High
  Confidence modes behave the same.
- Recent-folder entries are appended to the suggestion list with no projection at all.
- The Efc breadcrumb binding joins projected row text against raw scorer paths, so an archive-rooted
  suggestion loses its percentage.
- Several suggestion rows fail hierarchy resolution and fall back to a single segment.

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet from the debug log in the debug output tree (TaskMaster\bin\Debug\logs\debug_2026-09-06.log),
  stale-label resolution failures:

```
2026-09-06 16:45:51,908 [VSTA_Main] ERROR UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyProvider - No snapshot node path ends with '\Scorecards\Monthly Scans'; leaving 'Scorecards\Monthly Scans' unresolved.
2026-09-06 16:45:51,937 [VSTA_Main] ERROR UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyProvider - No snapshot node path ends with '\Forums\Pricing'; leaving 'Forums\Pricing' unresolved.
```

Research counted 18 occurrences of that error family in one session across only two distinct labels,
Scorecards\Monthly Scans and Forums\Pricing, which confirms once-per-render rather than once-per-label
emission. The same log also shows the Efc route reaching the same resolver with a fully rooted path
after the Efc router re-roots a relative presented target, so the AC7 gate must cover both surfaces.

## Scope & Non-Goals

- In scope:
  - The archive-root chain trim for AC1 and AC2, placed in the hierarchy provider's ancestor-chain
    method.
  - One shared display projection built on ArchiveStemContract.TryMakeArchiveRelative, replacing the
    two duplicated stripping members named by AC4 and applied to the recents append, the recents row
    mirror, and the include-children branch of the search-subpath helper.
  - The Efc score-path projection for AC6, applied in the Efc router where the bound root is already
    known and normalized.
  - The once-per-label-per-provider stale-label log gate and the zero-candidate row suppression for
    AC7, per decision D-B.
  - The AC8 verification finding, recorded in decision D-C. No renderer change.
  - MSTest coverage for the two new pure helpers, the trimmed provider, the recents projection and the
    Efc score join, plus the retargeting of the tests that pin superseded behaviour.
  - The explicit Compile Include entries every new .cs file needs in these legacy non-SDK projects.

- Out of scope / non-goals. The paths below are deliberately unbackticked; they are not part of the
  change footprint:
  - Anything under the dot-claude, dot-codex or dot-agents trees. No governance, skill, hook, rule or
    library file is required by this fix.
  - The two published JSON files under the config directory (the blast-radius module map and the
    orchestration routing map). Research confirmed neither is required.
  - Every GitHub workflow file under .github/workflows. No CI gate changes.
  - The solution file TaskMaster.sln, and the repository-root build property files. Every new .cs is
    added to an existing project through an explicit Compile Include line and no new project is
    introduced, so neither the solution nor any root build property file is touched.
  - UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs and the folder wrapper source file — see
    decision D-A, LEAVE.
  - The ToDoModel email-utilities sort file — see decision D-A, LEAVE.
  - UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Loading.cs:122-130, ResolveFolderRoot. Research
    found this unanchored archive-root comparison during the cross-check. It selects a root folder
    rather than stripping a prefix, so it is outside the family AC4 names and outside this fix.
  - The sibling-owned files listed in decision D-D: the UtilitiesCS breadcrumb bridge router and its
    search-presentation partial, the breadcrumb selection session and its highlight partial, and the
    QuickFiler breadcrumb bridge coordinator and its search partial.
  - UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs. GetAncestorChain keeps returning
    the full store-rooted chain; the trim is applied above it, so the query stays a faithful snapshot
    walk and its existing tests stay green on their own terms.

- Explicitly excluded systems, integrations, or datasets:
  - The persisted classifier corpus and the persisted subject map. This change does not rewrite,
    migrate or re-shape any stored label. The two LEAVE decisions in D-A exist specifically to keep
    persisted-data round-tripping unchanged.
  - Outlook COM automation in tests. Every new test is constructible from plain objects, a mocked tree
    service and hand-built snapshots, with no live Outlook and no WebView2.
  - The SpamBayes and Triage scoring engines. Scores are consumed, never recomputed.

## Root Cause Analysis

Why the two row kinds differ (verified by code read against this worktree):

- Suggestion rows. FolderBreadcrumbBridgeRouter.SetSuggestionsAsync decorates each row with the
  ancestor chain from IFolderHierarchyProvider. FolderTreeSnapshotQueries.GetAncestorChain, in
  UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs:109-146, walks ParentKey until it is
  null, that is, up to the store node. BreadcrumbRowBuilder.MapSegments, in
  UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:178-208, maps every node to a segment with
  no trimming. This is the #439 design, recorded in the #439 feature folder under
  docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439, Expected
  Behavior.
- Search rows. FolderBreadcrumbBridgeRouter.ReplaceItemsPreservingSession, in
  FolderBreadcrumbBridgeRouter.SearchPresentation.cs:38-55, carries the verbatim archive-relative stem
  produced by FolderPredictor.GetOlSubpath at FolderPredictor.cs:953-971, and
  BreadcrumbRenderProjection.SplitVerbatim at BreadcrumbRenderProjection.cs:242-246 splits it on path
  separators for rendering. No provider chain, so no store segments.

The snapshot genuinely contains the store node. OutlookFolderHierarchyReader.ReadStoreAsync, in
UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs:117-160, pushes the store root itself
with an empty parent entry id, and ToNode at :177-204 resolves its parent key to null. A null parent
key therefore identifies exactly the store root and terminates the walk, which is why the rendered
lineage begins at the mailbox.

The archive-root node is not identifiable by entry id, by a null parent key, or by a literal child
named Archive. The reliable identifier is the node's FolderPath compared ordinal-case-insensitively
against IOlObjects.ArchiveRootPath with trailing separators trimmed — precisely
ArchiveStemContract.TryMakeArchiveRelative's root handling. The node's RelativePath property is not
usable: OutlookFolderHierarchyReader.GetRelativePath at :206-211 makes it relative to the store root
using the same unanchored Replace pattern as the wrapper sites below.

Archive-root projection sites that have drifted, and that AC4 and AC5 unify under ArchiveStemContract
(UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs, the canonical ordinal, anchored,
separator-terminated stripper from #614):

- FolderPredictor.ProjectSuggestionPath, FolderPredictor.cs:848-861. Private, backslash-only, appends a
  single backslash to the root unconditionally so a root that already ends in a separator can never
  match, silently returns the full path on no match, and with an empty root strips exactly one leading
  separator from any path. That last behaviour is the defect AC4 names.
- QfcItemController.ProjectPredeterminedFolder,
  QuickFiler/Controllers/QfcItemController.FolderHandling.cs:272-285. A hand-copied duplicate of the
  above, added by #678 AC12 solely because the original is private. Its own comment at :228-230 states
  that reason, which disappears once the shared helper is public.
- FolderPredictor.AddRecents, FolderPredictor.cs:788-795. Appends the recents list verbatim with no
  projection. The issue under-reports this site: the row-model mirror AddRecentRows at
  FolderPredictor.cs:866-882 has the same gap at :879, and that row list is what the breadcrumb
  surfaces actually consume. The row array's own XML documentation at FolderPredictor.cs:233-242
  asserts the string list and the row list are text-identical, so projecting only one of them would
  break a documented contract.
- FolderPredictor.GetOlSubpath, FolderPredictor.cs:953-971. A blind Substring with no prefix
  verification. If the path does not start with the ancestor the result is a garbage substring, and if
  the path is no longer than the ancestor it throws ArgumentOutOfRangeException. The ancestor here is a
  search root, not necessarily the archive root; TryMakeArchiveRelative is root-agnostic, so it serves
  this site.
- FolderMinimalWrapper.ToRelativePath, UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs:56-86,
  with the Replace at :84; and the folder wrapper's LoadRelativePath at :194-224, with a
  character-identical body at :222. Both use an unanchored, case-sensitive Replace of the root plus a
  separator, with a full-path fallback when the root is null. The issue names the wrapper member as
  RelativePath; that is the persisted JSON accessor at :187-192, and the stripping actually lives in
  the internal LoadRelativePath. The full-path fallback is how a rooted label can reach the persisted
  classifier corpus.
- EfcFormController.BindBreadcrumbRowsAsync, QuickFiler/Controllers/EfcFormController.cs:1111-1128,
  with the load-bearing lines at :1115-1118. It passes already-projected row text together with the
  raw scorer output from Suggestions.ToScoredArray() into BindRowsAsync, which joins by presented-text
  equality. BreadcrumbRowBuilder.BuildProbabilityIndex keys on the score's FolderPath at
  BreadcrumbRowBuilder.cs:222-224 and BuildRow looks up the presented text at :133, so for an
  archive-rooted suggestion the key is the rooted path while the presented text is the stem. The
  lookup misses, the probability is null, and the percentage cell renders empty. By contrast
  AddSuggestionRows at FolderPredictor.cs:835-846 projects both the row text and the score path, which
  is why the QuickFiler surface does not show the defect.
- The ToDoModel email-utilities sort file at :67-109: legacy unanchored Contains and Substring root
  handling, using a bitwise and instead of a short-circuit and, and taking a Substring against the
  email root rather than the archive root.

Stale labels. The resolution failures logged above come from
OutlookFolderHierarchyProvider.ResolveByUniqueSuffix, in
UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs:90-114 with the emission at
:108-112, when a persisted suggestion label from the classifier corpus, the subject map or the recents
list names a folder that no longer exists at that path. Those rows fall back to a single segment. Note
that the emission has two distinct causes in one message: zero candidates and multiple candidates.
That distinction is the basis of decision D-B.

Runtime facts that constrain the design. AppOlObjects.ArchiveRootPath, in
TaskMaster/AppGlobals/AppOlObjects.cs:260-270 and TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs:86-93,
is derived from the default store root plus the literal folder Archive, validated by
ArchiveRootPathGuard, and throws InvalidOperationException rather than returning null when the root is
unresolvable. An existing test, QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs:242,
exists precisely because of that throw. Persisted suggestion labels are archive-relative when written
correctly, per OlFolderClassifierGroup.cs:205, SubjectMapSco.Orchestration.cs:19-33 and
AppAutoFileObjects.cs:211-229.

## Spec Decisions

Each decision below is settled by this specification. A planner implements it as written; a reviewer
checks the diff against it.

### D-A. AC4 site disposition

AC4 names only ProjectSuggestionPath and ProjectPredeterminedFolder as the pair to be replaced by one
shared projection built on ArchiveStemContract.TryMakeArchiveRelative, and additionally requires the
empty-root one-separator strip to be eliminated. That is implemented exactly as written. Research
enumerated seven drifted stripping sites; this decision records, site by site, which are converted and
which are deliberately left, so that a reviewer does not read a LEAVE as an oversight.

**CONVERT (four sites):**

1. **FolderPredictor.ProjectSuggestionPath.** Replaced by a delegation to the new shared display
   projection. Both callers (AddSuggestions at FolderPredictor.cs:810 and AddSuggestionRows at :842)
   are display paths. Rationale: it is the origin of the duplicated expression and the site that
   carries the empty-root defect. Evidence: FolderPredictor.cs:848-861.
2. **QfcItemController.ProjectPredeterminedFolder.** Replaced by a one-line delegation to the same
   helper, retaining the member so its existing test keeps a target. Rationale: its own comment states
   it exists only because the original was private, and AC4 names it explicitly. Evidence:
   QuickFiler/Controllers/QfcItemController.FolderHandling.cs:272-285 and its sole caller at :231-234.
3. **The recents append AND the row-model mirror.** Both FolderPredictor.AddRecents at
   FolderPredictor.cs:788-795 and AddRecentRows at :866-882 project each recent entry through the same
   helper. Rationale: the issue under-reports this site by naming only the string append. The row
   mirror is the path the breadcrumb actually consumes, and the documented text-parity contract at
   FolderPredictor.cs:233-242 would be broken if only one were projected. Evidence: the unprojected
   row construction at FolderPredictor.cs:879. This is what AC5 requires.
4. **FolderPredictor.GetOlSubpath, the include-children true branch only.** Rationale: only that
   branch is a stem strip and can be expressed as a verified prefix removal. The false branch computes
   a leaf name, which the contract does not do; converting it would change a different function.
   Evidence: FolderPredictor.cs:953-971, and the two existing public-caller assertions in the ToDoModel
   folder-handler tests, which pin both branches and survive a contract-based rewrite of the true
   branch unchanged.

**LEAVE, with reasons (three sites):**

5. **The folder minimal wrapper's relative-path loader**, UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs:56-86.
   Left unchanged. Its RestoreFromRelativePath branch at :88-131 explicitly tests for a leading double
   separator at :105-114 and walks the COM parent chain to recover a folder whose stored relative path
   is in fact a full store path. That branch exists precisely because the loader can return a rooted
   path. The property is JSON-persisted, so failing the loader closed — which is what the contract does
   — would change persisted-data round-tripping for values already on disk. No acceptance criterion
   covers it.
6. **The folder wrapper's relative-path loader**, at :194-224 in the folder wrapper source file (named
   in words in the Write Set section). Left unchanged, for the same round-tripping reason, and
   additionally because that file is 532 lines and already exceeds the repository's 500-line ceiling,
   so any edit invites a split that is out of scope here. Recorded plainly: its rooted output reaching
   the persisted classifier corpus, through SortEmail.cs:597 and QfcFormController.Actions.cs:250, is a
   real defect. It is a **separate** defect from #799, it has no acceptance criterion here, and this
   item does not fix it. It should be promoted as its own issue together with site 5.
7. **The ToDoModel sort utility.** Left unchanged because it is not a Compile item in its project and
   has no live caller. The project is non-SDK-style with explicit Compile Include items and no
   globbing, and its entire email-utilities membership is a single unrelated file; the sort file
   appears only in a stale .bak project backup that MSBuild never reads. Two searches confirmed this: a
   csproj-scoped search for the file name returned no matches, and a repository-wide search for the
   entry-point method name found only the source file itself, the .bak backup, and commented-out
   references in test files. Converting it would therefore be a no-op unless the file were first added
   to the project, which would compile previously-uncompiled code and introduce roughly seven unrelated
   compiler problems. That is out of scope.

### D-B. AC7 rendering branch

AC7 offers "rendered distinguishably (or filtered)". Neither branch is safe as written, so this
specification settles it.

**Rejected alternative 1 — the distinguishable branch as stated.** Research verified that no existing
flag or CSS class carries the signal on either surface: the Efc row model has no unresolved flag, the
Efc renderer emits a fixed class string for every suggestion, the Efc stylesheet has no stale rule, the
QuickFiler render DTO exposes eight fields with no flag, and the QuickFiler page CSS has no stale
class. Delivering it therefore costs eight files, including the QuickFiler breadcrumb HTML page
resource and the bridge-messages source. Those two are shared breadcrumb surface that a concurrent
sibling item also edits, so taking this branch would serialise the cohort against a display nicety.

**Rejected alternative 2 — the naive filter branch.** The unique-suffix resolver returns null for two
different reasons, distinguishable only inside it: zero candidates and multiple candidates. Filtering
every unresolved row would also drop the ambiguous case, in which the folder demonstrably exists — more
than one node matches — and is therefore fileable. That is a silent behaviour regression, not a
display change.

**DECISION.** Take the "(or filtered)" branch, **restricted to the zero-candidate case only**. A label
for which no snapshot node path ends with the suffix names a folder that no longer exists and cannot be
filed to, so suppressing its row loses nothing. The multiple-candidate case keeps today's rendering
untouched: the row remains present and selectable with the existing fallback. The once-per-label,
per-session logging that AC7 requires unconditionally is implemented for **both** cases, because it
lives in the resolver, above the branch that distinguishes them.

Placement. The classification (absent versus ambiguous) is produced in the provider, which is the seam
both surfaces route through, so a single change serves both and the log gate and the classification
share one code path. Row suppression is applied where each surface composes its presented row set: the
Efc router file already in the Write Set, and the QuickFiler item-controller files already in the Write
Set. If implementation finds that the QuickFiler drop-down composes its row set only inside the
sibling-owned bridge router, the planner must escalate rather than silently editing that file; the
fallback is to deliver suppression on the Efc surface only, leave the QuickFiler surface at today's
fallback rendering, and record the deviation in Rollout and Follow-up. In every case the AC7 logging
half is delivered on both surfaces.

Session scope. The provider is created once per form or viewer, not once per process. "Per session" is
therefore realized as "per provider instance", and this specification states that explicitly rather
than leaving it implied. A process-wide static set was rejected because it is mutable global state
shared across viewers and across test methods in one assembly, which the repository unit-test policy
prohibits. QuickFiler viewers are pooled and keep their provider across items, so a pooled viewer keeps
its gate, which is the desired behaviour. The gate must be thread-safe — a concurrent dictionary with
TryAdd, or a HashSet under a lock — because the resolve path is async and its continuations are not
guaranteed to be on one thread.

### D-C. AC8 requires no code change

Both render paths were traced end to end. No code alters a leading underscore.

- The QuickFiler page assigns segment text through the DOM textContent property, which performs no
  entity decoding, no escaping and no transformation.
- The Efc page HTML-encodes through a call that encodes ampersand, less-than, greater-than and
  double-quote only; underscore and space pass through unchanged and no non-breaking space is emitted.
- The JSON serializer escapes only the double-quote, the backslash and control characters, and the
  non-indenting format adds no whitespace inside string values.
- The verbatim splitter splits on path separators with empty entries removed; it inserts nothing and
  trims nothing.
- Neither stylesheet contains letter-spacing, word-spacing, text-transform, a first-letter
  pseudo-element, or word-break. The QuickFiler segment rule is flex with hidden overflow, ellipsis and
  no-wrap; the Efc assets are fifteen rules, none of them a spacing or casing transform.
- A repository-wide search across the six product projects for underscore rewriting and for
  letter-spacing, word-spacing, text-transform, first-letter and word-break returned no files.
- The WinForms mnemonic prefix character is the ampersand, not the underscore, so no combo-box or
  owner-draw path can be responsible either.

**Conclusion, stated as a definite finding and not as a probability: the reported space after the
leading underscore was a transcription artifact.** AC8 is satisfied by recording this verified finding.
The renderer is correct and must not be changed. No file is added to the Write Set for AC8.

### D-D. Sibling separability

A concurrent sibling item owns the QuickFiler folder drop-down OPEN and CLOSE lifecycle and the
selection-commit ordering. This item owns row TEXT projection and the archive-root stripping contract.

The AC1 and AC2 chain trim is placed in the hierarchy provider's ancestor-chain method for two reasons,
in this order. First, it is the single seam both surfaces route through: the QuickFiler drop-down
reaches the chain through the UtilitiesCS bridge router's SetSuggestionsAsync and the Efc list through
the QuickFiler router's FetchChainAsync, and both call the provider's GetAncestorChainAsync, so one
change satisfies AC1 on both. Second, it keeps this item's diff off the contended files: trimming
inside SetSuggestionsAsync would require a new field and a constructor parameter on the exact type the
sibling owns.

Recorded honestly, because it is the one place the two items genuinely overlap: the UtilitiesCS
breadcrumb bridge router file, UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs (489
lines), contains BOTH concerns.

- This item's side, the suggestion-decoration members: SetSuggestionsAsync at :29-97,
  SetSuggestionFallbacks at :100-119, AddPlainRows at :156-168, CreateFallbackRow at :247-257, and the
  single-parameter constructor at :19-23.
- The sibling's side, the lifecycle and selection members: OpenSelector at :185-186, MoveSelector at
  :188-189, CommitSelector at :191-192, ActivateSelector at :194-195, ActivateSelectorSubfolder at
  :205-208, CancelSelector at :210-211, GetSelectorState at :213, SelectRow at :178-179, SelectItem at
  :182-183, Clear at :171-175, Mutate at :239-245, Transition at :267-276,
  ReplaceRowsPreservingSession at :478-482, and the selection session field at :14.

The two sides share the sync lock at :15, the suggestion-generation counter at :16, and the
constructor. A constructor-signature change from this item would collide directly with any sibling edit
to that file. That risk is not softened here; it is the reason for the placement decision above.

Because the design keeps this item's diff out of that file, it is named in bare prose and is **not** in
the Write Set. A reviewer must verify at review time that the final diff contains no hunk in it, nor in
its search-presentation partial, the breadcrumb selection session or its highlight partial, or the
QuickFiler breadcrumb bridge coordinator or its search partial. Two files that could be mistaken for
the sibling's are in fact this item's Efc-surface files and are in the Write Set:
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs (304 lines). Its Selection partial contains
FetchChainAsync alongside Efc row-selection helpers; the recommended design needs no edit there, and a
reviewer should confirm that. Its Arrows partial is lifecycle-adjacent and is not touched.

## Proposed Fix

### Design summary (what changes where):

Two new pure static helper types are introduced, both in the UtilitiesCS folder namespace, both free of
I/O, COM and logging, and both unit-testable without a provider, a snapshot or Outlook.

1. **ArchiveStemProjection**, with a single member ToDisplayStem(folderPath, archiveRoot). It returns
   the archive-relative stem when the path is strictly under the root, and otherwise returns the input
   unchanged. An empty or whitespace root yields the input unchanged, which is what eliminates the AC4
   one-separator strip. It is a separate type rather than a new member on ArchiveStemContract on
   purpose: the contract is a hard boundary type that yields an empty string on failure and never
   passes its input through, and every display site needs the opposite fallback so an unexpected value
   still renders. Adding a lenient overload to the strict contract would blur the invariant #614
   created it for. ToDisplayStem treats "path equals root" as not projectable, because the contract
   returns true with an empty stem in that case and an empty display row is worse than the full path;
   that reproduces the existing length guard at FolderPredictor.cs:858 exactly.
2. **ArchiveChainProjection**, with a single member TryTrimBelowArchiveRoot(chain, archiveRoot, out
   trimmed). It finds the first chain index whose segment path is the archive root itself — detected as
   TryMakeArchiveRelative returning true with an empty stem, which is exactly the equality case — and
   returns the remainder of the chain after it. It returns false when no such index exists, and also
   when that index is the last one, because the leaf is then the root and there is nothing to render
   below it.

The provider's GetAncestorChainAsync applies TryTrimBelowArchiveRoot after the snapshot walk and before
segment mapping, so row order, banner placement and the trash pseudo-row are untouched. On a false
return it logs an error through the log4net ILog already declared in that file and returns an empty
segment list, which routes each surface into its existing fallback. Its GetImmediateSubfoldersAsync is
unaffected: subfolders are below the leaf and therefore below the archive root by construction.

The provider needs one piece of data it does not have: the archive root string. It is supplied as an
**optional second constructor parameter of delegate type returning a string** — a lazy accessor, not an
eagerly read value, and defaulting to null meaning "no trim".

- Lazy is mandatory. The archive root property throws InvalidOperationException when the root is
  unresolvable, and an existing test at QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs:242
  exists because of that throw. Reading it eagerly at construction would create a new throw site inside
  the Efc controller's breadcrumb configuration and the QuickFiler item controller's pipeline setup,
  neither of which is inside a try.
- Optional keeps the roughly twenty existing test constructions in the provider and adapter test files
  compiling unchanged, including a construction that passes a single null argument, which binds
  unambiguously to the first parameter.

AC4 and AC5 are then a set of delegations to ArchiveStemProjection: the two duplicated members collapse
to one-line delegations, and the recents append, the recents row mirror and the include-children branch
of the search-subpath helper each gain a projection.

AC6 projects the score paths inside the Efc router's BindRowsAsync rather than at the controller call
site. The router already computes and normalizes the bound root and already consumes
ArchiveStemContract, and it is 304 lines with room; the controller is 1320 lines, already 2.6 times the
ceiling, so every line added there worsens a standing violation. Placing it in the router also fixes
the join for every caller of the internal overload, not only the one controller. The public
three-argument overload forwards an empty root, so the projection is the identity there and no existing
caller of that overload changes behaviour. Re-keying the join on something other than presented text
was considered and rejected: the builder takes only a string list and a score sequence, so there is no
correlating identity to key on without changing a public signature and every test that calls it.

AC7 is implemented per decision D-B: the resolver distinguishes the zero-candidate and
multiple-candidate causes, gates its error emission on a thread-safe per-provider set of
already-reported labels, and reports the classification so each surface's row composition can suppress
only the zero-candidate rows.

AC8 is implemented per decision D-C: no code change; the verified finding is the deliverable.

### Boundaries and invariants to preserve:

- **AC3, above all.** The filing target and the score-lookup key remain the archive-relative stem. This
  is safe by construction: the QuickFiler state row's WithFilingTarget substitutes the presented stem
  into the leaf segment's path only, at BreadcrumbStateModel.Row.cs:77-98, and the Efc row's
  FilingTarget at BreadcrumbRow.cs:63-65 does the same. Trimming removes leading segments, so the leaf
  and therefore the filing value are untouched. A test must pin this rather than leave it to
  inspection.
- The archive-stem contract itself is not modified. Its three public members and its semantics are
  consumed as they are.
- The snapshot query GetAncestorChain is not modified. It continues to return the full store-rooted
  chain; the trim is applied above it.
- The QuickFiler surface's fallback is **not** to be converted to a single segment. AC2's phrase
  "existing single-segment fallback" is literally true only on the Efc surface, where the row builder's
  empty-chain branch at BreadcrumbRowBuilder.cs:122-131 renders one leaf-only segment. On the
  QuickFiler surface the render projection routes non-suggestion rows through the verbatim splitter, so
  a fallback for a multi-level stem renders as several segments. That is existing behaviour, it already
  satisfies AC2's real requirement that no row shows a mailbox prefix because the verbatim text is the
  archive-relative stem, and "fixing" it to one segment would be a new regression against the
  search-row rendering that AC1 requires to match.
- Persisted-data round-tripping. Neither wrapper relative-path loader changes, per D-A sites 5 and 6.
- The sibling's files are not touched, per D-D.
- Cancellation and disposal semantics of the provider and both routers are unchanged.

### Dependencies or blocked work:

None. #614 delivered ArchiveStemContract and #439 delivered the chain rendering this supersedes; both
are present at c431dc32. Live-Outlook confirmation is a human follow-up and does not gate the automated
review.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

See the Write Set section. It is the authoritative list.

**Ordering constraint that the planner must honour.**
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is **exactly 500 lines** — at the repository
ceiling, not near it. The provider construction at :147-149 must gain one argument, and CSharpier will
format the result as an additional line because the collapsed single-line call is about 126 columns
including indent, well past the print width. Adding that argument first would push the file to 501
lines and break the ceiling. Therefore the breadcrumb pipeline helper EnsureBreadcrumbPipeline
(:132-163, 32 lines including its comment and its ExcludeFromCodeCoverage attribute) must be relocated
into the new partial `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs` **before** the
provider construction gains its argument, leaving the viewer-setup file at roughly 468 lines. That
relocation is the only reason both files appear in the Write Set.

#### Functions/classes/CLI commands impacted:

New: ArchiveStemProjection.ToDisplayStem; ArchiveChainProjection.TryTrimBelowArchiveRoot. Modified:
OutlookFolderHierarchyProvider's constructor, GetAncestorChainAsync and ResolveByUniqueSuffix (the last
becomes an instance member so it can reach the per-instance gate); FolderPredictor's
ProjectSuggestionPath, AddRecents, AddRecentRows and GetOlSubpath; QfcItemController's
ProjectPredeterminedFolder and EnsureBreadcrumbPipeline (relocated); the Efc router's BindRowsAsync;
the Efc controller's provider construction. No CLI surface exists in this component.

#### Data flow and validation changes:

Display text only. The trim is applied to segment lists after chain resolution; the display projection
is applied to suggestion, recents and score path strings before they are presented or joined. The
filing target and the score-lookup key are unchanged in value, and the score join changes only in that
both sides of it are now projected against the same root. No persisted data, schema or file format
changes.

#### Error handling and logging updates:

- A chain that does not pass through the archive root is logged at ERROR by the provider, through the
  ILog already declared in that file, and yields an empty segment list so each surface uses its
  existing fallback.
- The stale-label ERROR emission in the resolver is gated to once per distinct label per provider
  instance, using a thread-safe set. Its message keeps both existing causes distinguishable.
- No new logger shape is introduced anywhere. Tests observe the gate through an injected delegate sink
  rather than a log4net appender, so no test mutates the process-global logger repository.

#### Rollback/feature-flag considerations (if applicable):

No feature flag. The optional constructor parameter is itself the effective off switch: constructed
without a root accessor, the provider behaves exactly as it does today. Rollback is a revert of the
branch, with no data or configuration migration.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

The shared projection inherits its semantics from ArchiveStemContract.TryMakeArchiveRelative, verified
at UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs:106-145:

- **Prefix-anchored.** The test is StartsWith at :131, never Contains and never Replace.
- **Ordinal, case-insensitive**, on both the equality test at :124 and the prefix test at :131.
- **Separator-terminated.** The character at the root's length must be a backslash or a forward slash,
  checked at :137-141; otherwise the result is false.
- **Trailing separators on the root are ignored**, trimmed at :118 for both separator characters. A
  root that is nothing but separators trims to length zero and is treated as empty.
- **Returns false with an empty stem rather than passing the input through**, at :112 and :129-141.
  Null, empty or whitespace input on either parameter returns false with an empty stem, at :113-122.
- **Returns true with an empty stem when the path equals the root**, at :124-127.
- The returned stem never leads with a separator; both separator characters are trimmed from its start
  at :143.
- **The Archive2 false-prefix boundary case.** A path under a sibling folder named Archive2, tested
  against a root ending in Archive, yields the character 2 at the root's length. That is not a
  separator, so the contract returns false and the path is not mis-stripped. The current
  ProjectSuggestionPath handles this case only by accident, because its concatenated prefix happens to
  carry the trailing separator; the contract handles it by rule. This case must appear in the tests for
  both new helpers.

ArchiveStemProjection.ToDisplayStem returns the stem when TryMakeArchiveRelative returns true **and**
the stem is non-empty, and returns the input unchanged in every other case, including the equal-to-root
case and the empty-root case.

ArchiveChainProjection.TryTrimBelowArchiveRoot returns true and the segments after the archive-root
node when such a node exists and is not the last element; it returns false and leaves the output empty
otherwise.

#### Required configuration keys and defaults:

None. No new setting, no settings-designer change, and no user-facing configuration surface. The
archive root continues to come from the existing application globals accessor.

#### Backward-compatibility expectations:

Additive at the type level: two new public static types, one new optional constructor parameter on the
provider, and one added field on no persisted contract. Existing callers compile unchanged. The
behavioural changes that are intended and not backward compatible are the superseded #439 lineage
rendering and the removal of the empty-root one-separator strip that AC4 names.

#### Performance constraints (latency/throughput/memory):

The trim is a single linear scan of a chain whose length is the folder depth, executed once per
resolved row. The display projection is one StartsWith and at most one Substring per string. Neither
introduces allocation per call beyond the produced string, and the contract performs no per-call regex
allocation. No performance threshold is asserted as an acceptance criterion, because no measured
baseline exists for breadcrumb binding.

## Write Set

Every path below is a concrete repository-relative path this change creates or modifies, written with
forward slashes and marked CREATE or MODIFY. This section is the change footprint; the planner reads
it. Any file the implementation must touch that is not listed here is a scope deviation and must be
recorded as one in Rollout and Follow-up rather than added silently.

### Production sources

- `UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs` — CREATE. The shared display projection.
- `UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs` — CREATE. The chain trim.
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` — MODIFY. 141 lines, ample
  room. Chain trim, optional root accessor, AC2 error, AC7 gate and classification.
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` — MODIFY. 1003 lines, already over the ceiling;
  the change is net line-neutral or negative because the thirteen-line stripping body collapses to a
  delegation.
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` — MODIFY. 313 lines. AC4 delegation and
  the QuickFiler-surface zero-candidate suppression.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` — MODIFY. 304 lines. AC6 score projection and the
  Efc-surface zero-candidate suppression.
- `QuickFiler/Controllers/EfcFormController.cs` — MODIFY. 1320 lines. One argument added at the provider
  construction near line 1053.
- `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs` — CREATE. Receives the relocated
  breadcrumb pipeline helper.
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` — MODIFY. Exactly 500 lines today; see the
  ordering constraint in Implementation strategy.

### Test sources

- `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` — CREATE.
- `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs` — CREATE.
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs` — CREATE. A new
  file rather than growing the existing provider test file.
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs` — CREATE. AC5.
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs` — CREATE. AC6.
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` — MODIFY, retarget.
  GetAncestorChainAsync_HappyPath_ReturnsRootToLeafSegments pins the superseded #439 root-to-leaf
  chain. It would remain green untouched only because it constructs the provider without a root
  accessor, which is not how production constructs it; leaving it that way would pin only the disabled
  configuration.
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` — MODIFY, retarget. Its chain
  fixtures build a segment for the Archive root node and assert it renders.
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.Activation.cs` — MODIFY, retarget if
  its fixtures also encode the rooted chain. Listed so a required change is not a scope violation; if
  it proves unaffected the diff simply contains no hunk for it.
- `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` — MODIFY, retarget. Its
  assertion at :220-223 pins the empty-root one-separator strip that AC4 eliminates. It must be updated
  to assert the new behaviour, not preserved.

### Project files

All four are legacy non-SDK projects with explicit Compile Include items and no globbing, so every new
.cs file needs a one-line self-closing entry.

- `UtilitiesCS/UtilitiesCS.csproj` — MODIFY. Entries for the two new production helpers.
- `QuickFiler/QuickFiler.csproj` — MODIFY. Entry for the new item-controller partial.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — MODIFY. Entries for the four new UtilitiesCS test files.
- `QuickFiler.Test/QuickFiler.Test.csproj` — MODIFY. Entry for the new QuickFiler test file.

### The two space-containing paths

Research concluded that neither is to be modified, so neither belongs in the Write Set and neither
carries backticks. Both are restated in words so a path extractor cannot misread them as write targets:

- **The ToDoModel email-utilities sort file** — directory ToDoModel, then a directory whose name is the
  two words Email and Utilities separated by one space, then SortItemsToExistingFolder.cs. Not
  modified: it is not a Compile item in its project and has no live caller, so converting it would be a
  no-op unless the file were first added to the build, which would compile previously-uncompiled code
  and is out of scope. See decision D-A, site 7.
- **The folder wrapper source file** — directory UtilitiesCS, then OutlookObjects, then Folder, then a
  file named FolderWrapper followed by one space and then the .cs extension. Not modified: it is 532
  lines and already over the 500-line ceiling, and its full-path fallback is depended upon by the
  persisted relative-path restore and by the classifier corpus. See decision D-A, site 6.

Because both are excluded, **this item's footprint contains no space-containing path.** That matters
mechanically as well as for scope: the token extractor splits every backtick span on whitespace, so a
backticked space-containing path could never be harvested as one token in any case.

Also not modified, and written without backticks for the same reason:
UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs — see decision D-A, site 5. Note the absence
of backticks on that line is deliberate and must not be "corrected".

## Assumptions, Constraints, Dependencies

- Assumptions: the archive root resolves successfully in the running add-in, and throws rather than
  returning null when it does not, which is why the accessor is lazy; persisted suggestion labels are
  archive-relative when written by the current code paths; the snapshot contains the archive-root node
  whenever it contains any node below it.
- Constraints: .NET Framework 4.8 and legacy non-SDK projects, so every new file needs an explicit
  Compile entry; the 500-line file limit, with the viewer-setup file exactly at it and three other
  files already over it; no temporary files, no wall-clock waits and no mutable global state in tests;
  the provider is constructed on the UI thread inside methods that are not wrapped in a try.
- External dependencies: MSTest, Moq, FluentAssertions and log4net, all already referenced. No new
  package.

## Data / API / Config Impact

- User-facing changes: suggestion rows in both QuickFiler modes and in the Efc view no longer show the
  mailbox and Archive segments; recent-folder rows are projected the same way as suggestions; an
  archive-rooted suggestion on the Efc surface now shows its percentage; a persisted label naming a
  folder that no longer exists is suppressed on the zero-candidate case per decision D-B.
- API changes: two new public static helper types; one new optional constructor parameter on the
  hierarchy provider; one resolver member changes from private static to a private instance member. No
  public interface signature changes.
- Superseded criteria recorded for the reviewer: #439's full root-to-leaf ancestor lineage is
  superseded by AC1 and AC2 of this specification. #439's filing-target and score-key constraint is
  preserved and is carried forward as AC3.
- Data or migration considerations: none. No persisted label is rewritten, and the two wrapper loaders
  that feed persisted data are deliberately unchanged.
- Logging/telemetry updates: one new ERROR for a chain that misses the archive root; the existing
  stale-label ERROR gated to once per distinct label per provider instance. No new logger shape.
- Compatibility notes: no CLI flags, no config schema, no settings surface. New files require explicit
  Compile Include entries in four legacy projects.

## Test Strategy

MSTest with Moq and FluentAssertions, per the C# unit test policy. No temporary files, no Thread.Sleep,
no Task.Delay, no wall-clock waits, no live Outlook COM, no WebView2, and no mutation of the
process-global log4net repository.

Validation notes carried forward from the issue:

- Unit coverage areas: chain trimming below the archive root (chain through root, chain not through
  root, chain equal to root); the shared projection against the #614 contract cases (case-insensitive,
  trailing separators, the Archive2 boundary); recents projection; the Efc score join with rooted and
  relative paths; renderer segment text preservation for leading underscores.
- Integration scenario to retest: bind a row set containing a suggestion, a search result, a banner
  row, the trash pseudo-row and a stale label; assert lineage on both folder row kinds, fallback on the
  stale label, and no lineage on the banner or trash rows. This is drivable entirely through the row
  builder and the render projection, with no WebView2 and no Outlook.
- Manual verification notes: QuickFiler ordinary and High Confidence, plus the Efc view. Confirm no row
  begins with the mailbox or Archive, and that clicking a middle segment navigates to that ancestor.

New unit tests:

- ArchiveStemProjection: table tests over the #614 boundary cases — under root, equal to root, Archive2,
  a root supplied with trailing separators, an empty and a whitespace root, null and empty path,
  forward-slash separators, and mixed case.
- ArchiveChainProjection: chain through the root; chain not through the root; chain whose leaf is the
  root; empty chain; single-element chain; root supplied with a trailing separator. All constructible
  from segment literals with no snapshot and no COM.
- Provider trim tests: a mocked tree service returning a hand-built snapshot, plus a root accessor.
  Assert the trimmed chain, assert the AC2 error and empty result for a chain that misses the root, and
  assert the AC7 gate by resolving the same unresolvable label twice and counting emissions through an
  injected delegate sink.
- Recents projection: exercise both the string array and the row array on a predictor whose recents
  list holds one rooted and one relative entry, and assert text parity between the two, which is the
  documented contract at FolderPredictor.cs:233-242 and is currently unasserted.
- Efc score join: a rooted-score and relative-row pair asserting a non-empty percentage cell. The
  existing #614 router fixtures already build rooted and relative target pairs and are the model.
- AC3 pin: after trimming, assert the filing target and the score-lookup key are still the
  archive-relative stem. This must be its own assertion, not an incidental consequence.

Retargeting obligations. These encode behaviour this specification supersedes; retarget, do not delete,
and do not merely add new tests alongside them:

- The provider chain test GetAncestorChainAsync_HappyPath_ReturnsRootToLeafSegments in
  `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` — rewrite so the
  chain assertion is made against a provider configured with a root accessor, as production configures
  it. A companion case may keep the untrimmed expectation for the no-accessor construction, but the
  production configuration must be the one that is pinned.
- The #439 Efc router tests in `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`
  and, if affected, `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.Activation.cs` —
  their fixtures build an Archive-root segment and assert it renders.
- The empty-root assertion at :220-223 in
  `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` — it pins the
  one-separator strip AC4 eliminates.

Fail-before evidence is required for at least one AC1 test, the AC4 empty-root behaviour change, and
the AC6 score-join test, recorded under this feature folder's evidence/regression-testing directory.
Baseline and final QA gate notes are recorded under this feature folder's evidence/baseline and
evidence/qa-gates directories. No evidence artifact is written to any other location.

Coverage: changed lines must not regress, and the two new helper types target at least 90 percent per
the repository unit-test policy. The repository-wide figure is reported against the testable
denominator per the CLAUDE.md UT2 exemptions and must not decrease.

Toolchain commands to run, in this order, restarting from the first on any failure or auto-fix:
1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. vstest.console.exe over the UtilitiesCS.Test and QuickFiler.Test assemblies and their dependents,
   with coverage collection.

Manual validation: performed by a human against a live Outlook profile after the fix is built, per the
manual verification notes above. It does not gate the automated review.

## Acceptance Criteria

- [x] AC1: Suggestion rows and search-result rows in both the QuickFiler item view and the Efc view render the lineage starting at the first segment below the archive root, with the same arrow rendering and clickable ancestor segments for both row kinds. Example: `_Active Projects -> Build RGF Org and Team -> Sales Lead`.
- [x] AC2: A resolved chain that does not pass through the archive root node is logged as an error and rendered with the existing single-segment fallback; no row ever shows the mailbox or Archive segment.
- [x] AC3: Filing target and score-lookup key remain the archive-relative stem (unchanged #439 constraint); filing to the selected folder still lands correctly.
- [x] AC4: `ProjectSuggestionPath` and `ProjectPredeterminedFolder` are replaced by one shared projection built on `ArchiveStemContract.TryMakeArchiveRelative`, and the empty-root one-separator strip is eliminated.
- [x] AC5: Recent-folder entries pass through the same projection before display.
- [x] AC6: `EfcFormController.BindBreadcrumbRowsAsync` projects the score paths the same way as the rows, so archive-rooted suggestions retain their percentage.
- [x] AC7: Persisted suggestion labels that fail hierarchy resolution are rendered distinguishably (or filtered) and logged once per label per session, not once per render.
- [x] AC8: The leading-underscore rendering question (`_Active Projects` vs `_ Active Projects`) is verified and, if the renderer alters it, corrected.

Notes for the reviewer, which do not add or weaken any criterion:
- AC4's site disposition is settled by decision D-A. Four sites convert, three are deliberately left
  with reasons recorded.
- AC7 is satisfied through the filtered branch restricted to the zero-candidate case, per decision D-B.
  The logging half applies to both causes and both surfaces.
- AC8 is satisfied by the verified finding in decision D-C. The renderer is correct; no code change is
  the correct outcome, and a renderer change would be a defect.

## Risks & Mitigations

- **Cohort contention.** Three sibling items run concurrently, and one of them owns the QuickFiler
  drop-down lifecycle in a file that also contains this item's suggestion-decoration members.
  Mitigation: the AC1 and AC2 trim is placed in the hierarchy provider, the single seam both surfaces
  route through, so this item's diff stays entirely off that file and the five other sibling-owned
  files; AC7's distinguishable branch, which would have pulled in the shared QuickFiler page resource
  and the bridge-messages source, is deliberately not taken; and every path in this document that the
  change does not write is unbackticked so the harvested blast radius matches the real footprint.
  Verification: the reviewer confirms the final diff contains no hunk in any file named in decision
  D-D.
- **Persisted-corpus risk.** Converting either wrapper relative-path loader to the strict contract would
  make it yield an empty string where it now yields a full path, which would change the values written
  to the persisted classifier corpus and would silently break the restore branch that exists to recover
  a rooted stored value. Mitigation: both sites are left unchanged by decision D-A, the reason is
  recorded rather than left implicit, and the underlying defect is promoted as a separate issue instead
  of being folded in here.
- **500-line ceiling risk.** The viewer-setup file is exactly at the ceiling, so the single argument
  this change adds to the provider construction would break it. Mitigation: the ordering constraint in
  Implementation strategy requires the pipeline helper to be relocated into the new partial first. Two
  further files in the Write Set are already over the ceiling; the change to each is a delegation or a
  single argument and must be net line-neutral or negative, and the two new helpers are new files
  rather than additions to the predictor.
- **Retargeting risk.** A retargeted test can be rewritten in a way that no longer pins anything.
  Mitigation: each retargeting obligation names the specific assertion and the specific configuration
  the rewritten test must exercise, and the AC3 pin is required as its own assertion.
- **Under-reported site risk.** AC5 names only the recents append, but the row mirror is what the
  breadcrumb consumes. Mitigation: decision D-A converts both and a test asserts text parity between
  the string array and the row array.
- Rollback: revert the branch. The optional constructor parameter means an intermediate state with the
  accessor unsupplied behaves exactly as today.

## Rollout & Follow-up

- Release/rollout steps: merge to main after review; the add-in is picked up by rebuilding the
  registered checkout, with no re-registration step.
- Post-fix manual verification: a human confirms in QuickFiler ordinary and High Confidence modes and
  in the Efc view that no row begins with the mailbox or Archive segment, that a middle segment click
  navigates to that ancestor, that filing still lands in the correct folder, and that an archive-rooted
  suggestion in the Efc view shows its percentage. Recorded as an evidence note under this feature
  folder's evidence directory. It does not gate the automated review.
- Post-fix monitoring: confirm the stale-label error appears once per distinct label per viewer rather
  than once per render, and that no new chain-misses-archive-root error appears in normal operation.
- Follow-up issues to open: (1) the two wrapper relative-path loaders, whose unanchored, case-sensitive
  Replace with a full-path fallback lets a rooted label enter the persisted classifier corpus — a real
  defect, deliberately out of scope here per decision D-A sites 5 and 6; (2) optionally, the unanchored
  archive-root comparison in the mail-item loading helper's ResolveFolderRoot, recorded during research
  as a root selector rather than a stripper.
- Links: issue https://github.com/drmoisan/TaskMaster/issues/799; the research record in this feature
  folder under research/; superseded predecessor #439; the archive-stem contract predecessor #614; the
  duplication predecessor #678.

### Outcome

Implemented on branch `bug/breadcrumb-lineage-below-archive-root-799` across three phases of the atomic plan
`plan.2026-09-06T22-01.md`. The final toolchain loop closed clean in one pass: CSharpier format and check both
exit 0 at 1601 checked files, the analyzer gate and the nullable gate each exit 0 with 0 Warning(s) and 0
Error(s), and the coverage-enabled nine-assembly run exits 0 with 7085 tests, 7085 passed, 0 failed and
`NEWLY-FAILING: NONE`. Evidence is under this feature folder's `evidence/qa-gates/` and
`evidence/regression-testing/` directories.

The delivered change departs from this specification's own prose in four respects. Each is recorded here by name
with the reason, so a reviewer reads a deliberate decision rather than an omission.

**1. AC7 row suppression is delivered on the Efc surface only; the QuickFiler surface keeps today's fallback
rendering.** Decision D-B above requires the planner to escalate rather than edit a sibling-owned file if the
QuickFiler presented row set proves to be composed only inside the sibling-owned bridge router. It is: the
QuickFiler drop-down's row set is built as a local list inside `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync`
at UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs lines 42-86 and swapped into the model under
the shared lock at lines 88-96, while `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` only hands the
predictor's row model to the viewer at its lines 212 and 221, at which point no provider resolution has been
attempted, so the zero-candidate classification does not yet exist there. Producing it would have required a
second synchronous resolution pass on the UI thread inside `AssignFolderComboBox`, duplicating the router's work
and changing the very ordering the sibling item owns. The documented fallback was therefore taken. The AC7
LOGGING half is delivered on BOTH surfaces, because it lives in the hierarchy provider that both surfaces route
through. The suppression half is pinned on the Efc surface by
`BindRowsAsync_ZeroCandidateLabel_SuppressesTheRowAndKeepsSegmentKeysAligned` (true arm) and
`BindRowsAsync_AmbiguousLabel_IsNotSuppressed` (false arm, the zero-candidate restriction).

**2. The AC6 score projection is additive rather than substitutive.** The projected score is ADDED alongside the
raw score rather than replacing it. `BreadcrumbRowBuilder.BuildProbabilityIndex` assigns through the indexer at
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs line 224, so duplicate keys are tolerated and the last
write wins. A plain substitution would have fixed the stem-presented case and silently broken the
rooted-presented case that
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs lines 118-166
(`Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively`) exercises: the score key would have
become the stem while the presented text stayed rooted, and the percentage would have vanished. That test does not
assert the percentage, so the regression would have shipped unnoticed.
`BindRowsAsync_RootedScoreAndRootedRow_StillRendersThePercentage` now pins the case explicitly. A second, smaller
deviation sits inside the same criterion: the projection is applied in the Efc router rather than at the
`EfcFormController.BindBreadcrumbRowsAsync` call site the criterion names, because that controller is 1320 lines
and cannot absorb growth while the router already normalizes the bound root.

**3. The two #439 Efc router test files listed in the Write Set carry no hunk.** Every test in
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs and its Activation partial constructs a
`Mock<IFolderHierarchyProvider>(MockBehavior.Strict)` and supplies the ancestor chain directly through
`ReturnsAsync`. The AC1/AC2 trim lives inside the provider's `GetAncestorChainAsync`, below that mock boundary, so
it cannot reach either file. Editing them would also have been actively harmful: the shared `Chain` helper emits a
leading `\Archive` segment that three tests depend on, and
`Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection` asserts that activating segment index 0 yields
`\Archive`, so removing that segment from the fixture would have broken a #614 boundary test unrelated to this
change while pinning nothing new. All ten tests of that partial class pass unmodified in the final run, which is
the behavioural confirmation that the no-hunk disposition was correct.

**4. The AC7 absence classification is published through a new small public interface declared in the provider's
own file, not through a fourth member on the shared hierarchy contract.**
UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs declares exactly three members. net48 has no default
interface members, so a fourth member would break every implementer, and every breadcrumb router test constructs
`new Mock<IFolderHierarchyProvider>(MockBehavior.Strict)`, so a strict mock would throw the first time production
called the new member. `IFolderLabelAbsenceReport` is instead declared in
UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs and obtained by the Efc router with an `as`
cast, so a strict mock simply is not an `IFolderLabelAbsenceReport`, the field is null, and suppression is inert in
every existing router test. Production is unaffected: no adapter wraps the provider, and both production
constructions hand the concrete provider straight to the router.

Two further dispositions are recorded in the plan rather than as deviations, because this specification already
authorises them: the AC4 site disposition (four sites converted, three deliberately left, per decision D-A sites
5, 6 and 7), and the AC8 outcome (no code change, per decision D-C, verified and recorded in
`evidence/qa-gates/p3-t12-ac8-verification.md`).
