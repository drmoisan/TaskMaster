# breadcrumb-router-navigation-defects (Spec)

- **Issue:** #498
- **Also closes:** #439, #440, #499
- **Parent (optional):** epic `quickfiler-bug-family` (integration branch `epic/quickfiler-bug-family-integration`)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-24T10-20
- **Status:** Ready for planning
- **Version:** 1.0
- **Work Mode:** full-bug — this file is the authoritative acceptance-criteria source. `user-story.md` is absent by design.

> Evidence base: every code claim in this document carries a `file:line` citation taken from
> `research/2026-08-24T09-50-breadcrumb-router-navigation-defects.md`, verified at HEAD `988e819b` in this
> worktree. Where a promoted potential document's citation disagrees with the research document, the research
> document's line numbers govern (research §0 correction table).
>
> Tone: this document follows `.claude/rules/tonality.md` — factual, neutral, evidence-matched wording.

## Context

- Summary of the bug and its impact (link to repro/playbook entry).

  Four defects in the breadcrumb bridge router and folder navigation surface are fixed together because they
  share the same code, the same seams, and the same test fixtures. Two of them (#439 and #440) are coupled:
  parent-node navigation is only meaningful once rows carry a resolved multi-segment ancestor chain
  (research §Q4f).

  | Issue | Defect | Severity | Surface | Authoritative potential |
  |---|---|---|---|---|
  | #498 | Out-of-range `segmentIndex` escapes the `async void` host boundary and can terminate the Outlook host process | High | Efc | `docs/features/potential/promoted/2026-08-08-breadcrumb-router-segment-index-unvalidated-host-crash.md` |
  | #499 | `SelectedFolderPath` not cleared on re-bind; controller reports a folder the UI no longer shows selected | High | Efc | `docs/features/potential/promoted/2026-08-08-breadcrumb-router-stale-selectedfolderpath-after-rebind.md` |
  | #439 | Ancestor lineage never resolves; rows render as a single leaf segment | High | Efc + Qfc | `docs/features/potential/promoted/2026-08-07-efcviewer-missing-lineage-and-segment-navigation.md` |
  | #440 | Left/Right perform breadcrumb display-collapse, not tree navigation | Medium | Qfc + Efc | `docs/features/potential/promoted/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation.md` |

- Observed environment(s): Windows 11 Pro 10.0.26200; .NET Framework 4.8.1 WinForms VSTO add-in with
  Microsoft WebView2. Efc surface: `EfcViewer.FolderListBox` driven by `BreadcrumbBridgeRouter` over a
  document assembled by `BreadcrumbHtmlRenderer` from `BreadcrumbDocumentAssets`
  (`BreadcrumbHtmlRenderer.cs:40-49`). Qfc surface: `ItemViewer` folder selector driven by
  `FolderBreadcrumbBridgeRouter` / `BreadcrumbStateModel` over `QuickFiler/Resources/FolderBreadcrumb.html`.

- Customer impact and severity (who is affected, how often, how bad):
  - #498 — any user of the EfcViewer folder list. A malformed message from the hosted WebView2 document
    terminates the Outlook host process. Frequency is input-dependent, severity is process termination.
  - #499 — every EfcViewer search session. `BindFolderRows` (`EfcFormController.cs:873-883`) runs on every
    search keystroke, so the divergent window is common, and the failure is silent: mail can be filed to a
    folder the user can no longer see selected.
  - #439 — every suggestion row and every search-result row on both surfaces renders leaf-only. Users cannot
    disambiguate same-named folders under different parents.
  - #440 — keyboard-only filing cannot reach a folder that is not already in the presented row set.

- First observed date and version(s) impacted: #439 and #440 captured 2026-08-07; #498 and #499 captured
  2026-08-08 during preparation research for epic #136 child F12 (issue #495). All four are present at HEAD
  `988e819b`. No fixed version exists.

## Repro & Evidence

- Steps to reproduce (with data/flags/inputs):
  - **#498** — open the EfcViewer folder list so `EfcFormController.ConfigureBreadcrumbControl`
    (`EfcFormController.cs:834-854`) has wired a `BreadcrumbBridgeRouter`; have the hosted document post
    `{"type":"segmentDoubleClick","rowId":"row-0","segmentIndex":99}` for a row with fewer than 100 segments;
    observe the host process.
  - **#499** — open the EfcViewer folder list, type search text, select a folder row (`SelectRow` sets both
    `_selectedRowId` and `SelectedFolderPath`, `BreadcrumbBridgeRouter.cs:372-375`), type one more character
    (reaching `BindFolderRows`), observe no row highlighted, then trigger a move or folder-open.
  - **#439** — open EfcViewer on a mail item so suggestions populate; observe each row shows only a folder
    name with no ancestor chain; type a search string and observe the same on the SEARCH RESULTS rows.
  - **#440** — give the folder selector keyboard focus so a row is highlighted; press Left and observe the
    selected node does not become the parent; press Right and observe expansion applies to the leaf. Repeat
    in EfcViewer.

- Expected vs actual behavior:
  - **#498** — expected: the router's own XML doc comment (`BreadcrumbBridgeRouter.cs:151-154`) states that a
    malformed payload fails fast and leaves state unchanged. Actual: `ArgumentOutOfRangeException` is thrown
    at `BreadcrumbRow.cs:111-118`, is not caught by the `catch (BreadcrumbMessageException)` at
    `BreadcrumbBridgeRouter.cs:187-198`, and escapes the `async void` boundary at `:187`.
  - **#499** — expected: after a re-bind clears the visible selection, `SelectedFolder` agrees with the UI.
    Actual: only `_selectedRowId = null` is executed (`BreadcrumbBridgeRouter.cs:114`); `SelectedFolderPath`
    (`:58`) retains its prior value, and `DeliverDocument` (`:397-409`) renders with `_selectedRowId` at
    `:399`, so no row is highlighted.
  - **#439** — expected: rows render their resolved root-to-leaf lineage. Actual:
    `ResolveLeafKeyAsync` (`OutlookFolderHierarchyProvider.cs:52-71`) returns `null`, `FetchChainAsync`
    returns `null` at `BreadcrumbBridgeRouter.cs:345-348` without calling `GetAncestorChainAsync`, and
    `BreadcrumbRowBuilder.BuildRow` takes its documented single-segment fallback
    (`BreadcrumbRowBuilder.cs:119-134`).
  - **#440** — expected: Left selects the parent node, Right expands the selected node into its children.
    Actual: `HandleArrowKeyAsync` (`BreadcrumbBridgeRouter.cs:225-260`) maps Right to `ReExpand()` or
    `ExpandLeafAsync`, Left to `row.LeftArrow()` (`BreadcrumbRow.cs:195-216`); the Qfc analogue is
    `FolderBreadcrumbBridgeRouter.ArrowAsync` (`:378-406`) over `BreadcrumbStateModel.RightArrow()`
    (`:424-437`) and `LeftArrow()` (`:443-455`). Every transition mutates view state only; the class doc
    says so explicitly at `BreadcrumbRow.cs:23-33`.

- Logs/screenshots/error snippets: `System.ArgumentOutOfRangeException` originating in
  `BreadcrumbRow.CollapseAfter` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:111-118`) for #498. The
  other three defects are silent: #499 is a state divergence with no error text, #439 presents as a cosmetic
  omission because the single-segment fallback keeps the row visible and selectable, and #440 is a missing
  capability.

- Frequency / determinism (always, intermittent, data-dependent):
  - #498 — deterministic and data-dependent (any `segmentIndex` outside `[0, segments.Count - 1]` on a
    suggestion row; banner and trash rows return `false` before the range check, `BreadcrumbRow.cs:106-109`).
  - #499 — deterministic on every re-bind that follows a selection.
  - #439 — deterministic; the path forms never match (research §Q3a, §Q3b).
  - #440 — deterministic; no selected-node concept exists on either surface (research §Q4d).

## Scope & Non-Goals

### In scope

- #498 — a range guard in the `SegmentDoubleClick` arm of `ProcessInboundAsync`.
- #499 — clear `SelectedFolderPath` on re-bind and raise `SelectedFolderPathChanged(this, null)` when the
  value actually changed.
- #439 **part A only** — lineage resolution, so Efc and Qfc rows carry a resolved multi-segment ancestor chain.
- #440 — Left/Right tree transitions on both surfaces, inserted ahead of the existing behavior.
- The two #439 regression hazards of decision D6 (suggestion-row percentage; Efc filing target).
- The Qfc filing-target risk of decision D7, resolved by the recorded ladder.

### Out of scope / non-goals

- #439 **part B** — the Efc mouse gesture that single-clicks a non-leaf segment to navigate to that ancestor
  (decision D3). Requires writing `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs:59-89`, which
  is not owned.
- The Efc separator glyph change from `>` to `→` (decision D3). Requires writing
  `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs:147-150`, which is not owned.
- Unifying the two surfaces' boundary behavior (decision D2).
- Restoring the prior selection on re-bind, and any `SelectFirstRow` auto-selection side effect (decision D4).
- Changing the presented-row path form in `FolderPredictor` (decision D5).
- Any repair of the pre-existing 500-line violations in `FolderPredictor.cs` (983 lines) and
  `EfcFormController.cs` (1086 lines) (decision D8).

### Explicitly excluded systems, integrations, or datasets

- Live Outlook / COM. All tests are Moq-based over `IFolderHierarchyProvider`, `IBreadcrumbWebHost`, and
  `IOutlookFolderTreeService`; `FolderNavigator.GetOutlookFolder` (`FolderNavigator.cs:10`) is a live COM path
  walk and is not used.
- The `#400` selector-session types. `BreadcrumbSelectionSession`
  (`BreadcrumbSelectionSession.cs:98-107`) is not modified (decision D1).

### Files this feature owns (the only production files the fix may write)

- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`
- `QuickFiler/Controllers/KeyboardHandler.cs`
- `QuickFiler/Resources/FolderBreadcrumb.html`
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`
- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`
- plus new partial-class siblings of the owned `.cs` files, and the test files named in research §Q5a.

### Files this feature must not write (sibling epic children are executing concurrently)

- `QuickFiler/Controllers/KbdActions.cs` — feature 444. A different file from the owned `KeyboardHandler.cs`.
- `QuickFiler/Controllers/EfcFormController.cs` — feature 464.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs`
- `UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs`
- `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` — per decision D1, must pass unmodified.

Read-only access to any of the above is permitted and expected; writing is not.

## Scope Decisions (recorded verbatim)

These eight decisions were made by the orchestrator before planning. They are recorded here as the binding
scope of the feature. They are not re-opened during planning or execution.

### D1 — #400 AC-9 is superseded in part, and the supersession is deliberately narrow

Research §Q4c establishes that issue #400's landed, checked-off AC-9
(`docs/features/archive/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md:247`) states "Left and Right
preserve the existing breadcrumb expand, collapse, and unhandled-key behavior in both view modes and do not
mutate the committed/original/pending selector session", and that it is enforced by a live test at
`QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs:359-367`.

**Decision:** #440 supersedes ONLY the "existing breadcrumb expand, collapse" clause, and ONLY for rows whose
ancestor chain resolves to more than one segment. Specifically:

- The `arrowKey` and `unhandledArrow` MESSAGE SHAPES are PRESERVED unchanged.
  `FolderBreadcrumbAssetContractTests.LeftAndRightBreadcrumbMessages_RemainSupported` MUST continue to pass
  UNMODIFIED. It is not to be edited.
- The new tree transitions are inserted BEFORE the existing behavior in the handling order, producing:
  1. new parent-select / expand-node transition when one is available for this row, else
  2. the existing breadcrumb expand/collapse behavior, else
  3. the existing unhandledArrow fall-through, unchanged.
- AC-9's "unhandled-key behavior" clause is PRESERVED.
- AC-9's "do not mutate the committed/original/pending selector session" clause is PRESERVED. The #440
  implementation must leave `BreadcrumbSelectionSession` untouched.

#### #400 AC-9 supersession record (reviewer-findable)

| Clause of #400 AC-9 (`spec.md:247`) | Disposition under this feature |
|---|---|
| "Left and Right preserve the existing breadcrumb **expand, collapse** ... behavior in both view modes" | **RETRACTED IN PART.** Retracted only for rows whose resolved ancestor chain has more than one segment, and only to the extent that a new tree transition is attempted first. Where no tree transition is available, the existing expand/collapse behavior runs unchanged. |
| "... and **unhandled-key** behavior ..." | **PRESERVED.** The `unhandledArrow` message shape and its downstream fall-through are unchanged. |
| "... and do not mutate the **committed/original/pending selector session**." | **PRESERVED.** `BreadcrumbSelectionSession` is not written by this feature. |
| #400 AC-5 through AC-8 (`spec.md:243-246`), the Up/Down/Enter/Escape selector contract | **PRESERVED.** Untouched by #440 (research §Q4c). |

This is a deliberate, auditable retraction of one clause of a landed acceptance criterion. It is recorded here
so that a reviewer of #400's archived spec can trace why the criterion is no longer literally true.

### D2 — Boundary behavior is not unified across the two surfaces

Research §Q4e establishes that Efc and Qfc already disagree today at both boundaries (Left at root: Efc is a
silent no-op, Qfc emits `unhandledArrow` and closes the drop-down; Right on a childless node: Efc silent no-op,
Qfc opens the MyBox Pop Out / Enumerate Conversation dialog).

**Decision:** preserve today's per-surface boundary behavior exactly. Do NOT unify it. When no tree transition
is available, each surface falls through to precisely what it does today.

**Rationale:** unifying is a second user-visible change that #440 did not request, and the #440 potential
explicitly places the Pop Out / Enumerate Conversation entry point out of scope "unless the maintainer decides
otherwise" (`2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation.md:75`). No such decision exists.
The Pop Out dialog therefore stays reachable by exactly the gesture that reaches it today
(`KeyboardHandler.cs:288-315`, Right branch at `:302-310`).

### D3 — #439 is split: part A is in scope, part B and the glyph are descoped

Research §Q3f and §Q6a establish that the Efc surface does NOT use `FolderBreadcrumb.html` (that is the Qfc
document); the Efc document comes from `BreadcrumbDocumentAssets.cs` and its separator is `&gt;`, not `→`.

**Decision:**

- **IN SCOPE:** #439 part A — lineage resolution, so Efc and Qfc rows carry a resolved multi-segment ancestor
  chain. This is the High-severity half and is fully achievable in owned files.
- **DESCOPED:** #439 part B, the Efc mouse gesture that single-clicks a non-leaf segment to navigate to that
  ancestor. It requires writing `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`, which is not
  an owned file.
- **DESCOPED:** changing the Efc separator glyph from `>` to `→`. It requires writing
  `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs`, which is not an owned file. It is purely
  cosmetic and is the weakest part of #439.
- The ancestor-navigation CAPABILITY that #439 part B asks for is nevertheless delivered on both surfaces by
  #440's keyboard Left/Right transitions. #439 closes on part A plus the #440 keyboard capability; the
  residual is the Efc MOUSE gesture and the glyph.

Both descoped items are recorded under "Cross-feature notes" below with the exact unowned file and line each
would require. Each should become a follow-up potential entry; this feature does not create them.

### D4 — #499 clears and raises

Adopt research §Q2c's recommendation without modification: clear `SelectedFolderPath` to `null` in
`BindRowsAsync` alongside `_selectedRowId`, and raise `SelectedFolderPathChanged(this, null)`, but only when
the value actually changed. Reject the restore-prior-selection option for the three reasons the research gives
(research §Q2c: it does not fix the keystroke path where the defect matters; restoration must match by
`row.LeafSegment?.FullPath`, the exact value #439 changes the form of; and it forces a second coherent change
to `DeliverDocument`/`_selectedRowId`). Do NOT add a `SelectFirstRow` side effect.

**Orchestrator-verified fact resolving the research's one open item on this defect (research §Q2c, open item
6):** `EfcFormController.IsValidSelection` (`QuickFiler/Controllers/EfcFormController.cs:1039-1050`) ALREADY
tolerates `null` — its first disjunct is `selectedFolder is null`, so it returns `false` for a null selection.
Both call sites guard on `!IsValidSelection` first (`:470`, `:754`). The #499 clear therefore CANNOT introduce
a `NullReferenceException`. (Read-only confirmation at HEAD in this worktree: the `IsValidSelection` property
block spans `EfcFormController.cs:1040-1052`, the `selectedFolder is null` disjunct is at `:1046`, and the two
guarded call sites are at `:470` and `:754`.)

The cross-feature note to feature 464 is informational only, not a blocking dependency.

### D5 — #439 canonical path form

Adopt research §Q3e without modification. Fix site is
`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` (owned, 98 lines): keep
the existing exact `OrdinalIgnoreCase` match as the first pass, then fall back to a segment-boundary suffix
match accepted ONLY when unique; on zero or multiple candidates log at `Error` via the existing `log4net`
pattern and return `null`, preserving today's single-segment rendering. One change fixes both surfaces
(the Qfc router resolves through the same method, `FolderBreadcrumbBridgeRouter.cs:49-54`).

Reject the `FolderPredictor` option (it would change the filing contract, and the file is already 983 lines,
over the 500 limit) and reject the `BindRowsAsync`-only option (the router has no access to `ArchiveRootPath`
without editing the unowned `EfcFormController.cs`; the router constructor is
`BreadcrumbBridgeRouter.cs:40-55` and is called at `EfcFormController.cs:843-849`).

### D6 — The two #439 regression hazards are in scope and must have their own acceptance criteria

Research §Q3d and §Q6a establish that resolving the chain BREAKS two things that work today, because both
currently depend on the chain being unresolved:

- **(a)** the suggestion-row PERCENTAGE (`BreadcrumbRowBuilder.cs:131` joins on `segments[last].FullPath`,
  which flips from the archive-relative stem to the full Outlook path while `BuildProbabilityIndex`
  (`:208-227`, key assignment at `:222`) stays keyed on the stem);
- **(b)** the Efc FILING TARGET (`BreadcrumbBridgeRouter.cs:372-375` sets `SelectedFolderPath` from
  `row.LeafSegment?.FullPath`, consumed as `DestinationOlStem` at `EfcDataModel.cs:286-289`).

Both are fixable inside owned files per research §Q3e (a `rowId -> presentedText` map plus `FolderScore`
aliasing, both in `BindRowsAsync`). `BreadcrumbRowBuilder.cs` must NOT be written. Each hazard gets its own
explicit acceptance criterion asserting the behavior is PRESERVED, not merely that the lineage resolved.

### D7 — The Qfc filing-target hazard is an explicit verification gate, not an assumption

Research §Q6a records the feature's single largest open risk as UNVERIFIED: once #439 makes Qfc chains resolve,
`BreadcrumbSelectionMap.GetSelectedFolder` (`BreadcrumbSelectionMap.cs:109`) returns
`row.Chain[last].FolderPath` and the Qfc filing target flips from stem to full path — and that file is NOT
owned.

**Decision:** this is carried as a named RISK (see "Risks & Mitigations", RISK-1) with a three-way resolution
ladder, taken in order:

1. **PREFERRED** — preserve the presented stem through the owned
   `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` (`CreateFallbackRow` /
   `ReplaceRowsPreservingSession`), mirroring the Efc fix, so `BreadcrumbSelectionMap.cs` needs no change.
2. **FALLBACK** — if rung 1 is not achievable in owned files, the Qfc router must not consume the
   newly-resolved chain for the filing-target path, explicitly preserving today's Qfc filing behavior, and the
   spec records that Qfc lineage display improves while Qfc filing is deliberately left on the old path form.
3. **HALT** — if neither is achievable, this is a BLOCKING cross-feature dependency on
   `BreadcrumbSelectionMap.cs` and execution stops and reports it rather than writing an unowned file.

The choice among 1/2/3 is made by READING `BreadcrumbStateRow` and `CreateFallbackRow` during execution. That
reading is read-only. The acceptance criterion for each rung is stated under RISK-1.

### D8 — File-size: partial-class splits are pre-authorized

Research §Q6c line counts at HEAD: `BreadcrumbBridgeRouter.cs` 450/500 (receives all four fixes),
`FolderBreadcrumbBridgeRouter.cs` 485/500, `BreadcrumbStateModel.cs` 457/500, `FolderBreadcrumb.html` 489/500.
In-repo precedent exists (`FolderBreadcrumbBridgeRouter.SearchPresentation.cs`, `UtilitiesCS.csproj:629-630`).

**Decision:** partial-class splits are pre-authorized for the three `.cs` files. A new partial file needs a
`Compile Include` in `QuickFiler.csproj` or `UtilitiesCS.csproj` placed in its own ALPHABETICAL neighbourhood —
the item groups are alphabetically ordered and shared with concurrently-executing sibling epic children, so a
misplaced entry causes a rebase conflict. `FolderBreadcrumb.html` cannot be split; its 11 lines of headroom is
a hard constraint on the #440 `onArrow` change.

`FolderPredictor.cs` (983 lines) and `EfcFormController.cs` (1086 lines) are PRE-EXISTING 500-line violations
that this feature neither inherits responsibility for nor worsens. `FolderPredictor.cs` should not be written
at all under D5.

## Root Cause Analysis

- Current hypothesis or confirmed root cause: **all four root causes are confirmed by code read at HEAD
  `988e819b`**, not hypothesised.

  1. **#498** — the codec validates the *presence* and JSON-integer *type* of `segmentIndex` and nothing else
     (`BreadcrumbMessageCodec.cs:100`, `:103-106`, `:142-158`). The router then dereferences it with the
     null-forgiving operator and calls a member that throws on range
     (`BreadcrumbBridgeRouter.cs:166-174`; throw at `BreadcrumbRow.cs:111-118`). The host-event handler is
     `async void` (`BreadcrumbBridgeRouter.cs:187`, subscribed at `:54` against
     `IBreadcrumbWebHost.MessageReceived`, `QuickFiler/Viewers/IBreadcrumbWebHost.cs:22`) and catches only
     `BreadcrumbMessageException` (`:187-198`). Research §Q1b enumerates all four inbound codec fields and
     confirms `segmentIndex` is the only presence-only value reaching a throwing member; there is no second
     instance of this defect class on the Efc surface.
  2. **#499** — `BindRowsAsync` clears `_selectedRowId` at `BreadcrumbBridgeRouter.cs:114` but not
     `SelectedFolderPath` (`:58`), which is assigned in exactly one place, `SelectRow` (`:372-375`).
     `DeliverDocument` renders from `_selectedRowId` (`:399`).
  3. **#439** — a path-form mismatch. Presented text is an archive-root-relative stem
     (`FolderPredictor.LoopFolders` `:883-931`, stem assignment `:898`, add at `:919`;
     `GetOlSubpath` `:933-951`, substring at `:943`), while `ResolveLeafKeyAsync` compares with exact
     `OrdinalIgnoreCase` equality against `node.FolderPath`, the raw Outlook `MAPIFolder.FolderPath`
     (`OutlookFolderHierarchyProvider.cs:52-71`; capture at `OutlookFolderHierarchyReader.cs:143`). Research
     §Q3b adds the decisive correction: `FolderTreeSnapshotNode.RelativePath`
     (`FolderTreeSnapshotNode.cs:53`, computed at `OutlookFolderHierarchyReader.cs:206-211`) is
     **store**-relative while the presented stem is **archive**-relative, so a naive "also compare
     `RelativePath`" fix would not work.
  4. **#440** — no selected-node concept exists on either surface. Research §Q4d enumerates every candidate
     type (`BreadcrumbSelectionSession.cs:98-107` is row-level; `BreadcrumbSelectionMap.cs:15-51` is a static
     projection; `FolderTreeSelectionOverlay.cs:12-37` belongs to the folder-filter surface;
     `BreadcrumbRow.CollapsedAfterIndex` is documented display state at `BreadcrumbRow.cs:23-33`) and
     concludes none exists.

- Signals/evidence supporting it: the `file:line` citations above, all verified at HEAD `988e819b` and
  recorded in the research document with quoted source. Research §0 corrects four of the potentials' citations
  (two wrong file paths for #498, off-by-a-few lines in `FolderPredictor`, a wrong method span for
  `FetchChainAsync`) and one surface attribution (`FolderBreadcrumb.html` is the Qfc document, not the Efc one).

- Affected components/modules (paths, services, pipelines):
  `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`, `QuickFiler/Controllers/KeyboardHandler.cs`,
  `QuickFiler/Resources/FolderBreadcrumb.html`,
  `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`,
  `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`,
  `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`,
  `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`.

## Proposed Fix

### Design summary (what changes where):

Four targeted changes, delivered in the sequence below.

**Sequencing (research "Recommended intra-feature sequencing"): #498 → #499 → #439 part A → #440.**

1. **#498 first.** Smallest, fully self-contained in `BreadcrumbBridgeRouter.cs:168-174`, no interaction with
   any other defect, and the highest severity (host-process termination). Landing it first also establishes
   the `_host.Raise` regression-test pattern the rest of the plan reuses
   (`BreadcrumbBridgeRouterQueueTests.cs:194-205`).
2. **#499 second.** Also confined to `BreadcrumbBridgeRouter.cs` (`:114`, `:364-380`), also independent, and it
   must land **before** #439 because #439 changes how `SelectedFolderPath` is derived. Sequencing it second
   keeps the two changes to that assignment separable and separately bisectable.
3. **#439 part A third.** The largest change (provider suffix-match plus router presented-text map plus score
   aliasing), it carries the two regression hazards of D6, and it is the prerequisite for #440.
4. **#440 last.** It depends on #439 producing multi-segment rows — research §Q4f verifies that before #439
   both transitions are permanent no-ops on production rows, because a one-segment row makes `LeftArrow()`
   return `false` immediately (`BreadcrumbRow.cs:208-212`) and the fallback segment is constructed with
   `hasSubfolders: false` (`BreadcrumbRowBuilder.cs:127`) so `CanExpandLeaf()` is always `false`
   (`BreadcrumbRow.cs:260-263`). #440 is also the only defect that retracts part of a landed acceptance
   criterion (D1) and the change most likely to force the D8 partial-class splits.

Per the CLAUDE.md Bugfix Workflow, **each** of the four defects requires a failing regression test FIRST, then
the minimal targeted fix, then verification. The regression-test homes are named in "Test Strategy" below.

### Boundaries and invariants to preserve:

- `BreadcrumbRow.CollapseAfter`'s throw is a documented contract (`BreadcrumbRow.cs:101-103`) with existing
  test coverage, and `BreadcrumbRow.cs` is shared by both surfaces. It is NOT changed to return `false`
  (research §Q1c).
- The `catch (BreadcrumbMessageException)` at `BreadcrumbBridgeRouter.cs:187-198` stays narrow. A broad
  `catch (Exception)` at the async-void boundary would absorb unexpected exception classes from the whole
  `ProcessInboundAsync` tree, which is the "broad-catch without added context" pattern the General Code Change
  Policy prohibits. (Research §Q1c notes a belt-and-braces variant; it is a separate decision and is not
  adopted here.)
- The presented-row text stays an archive-root-relative stem. It is contractually a stem on the filing side
  (`EfcDataModel.cs:286-289`, `:307-310`, `:325-328`; `EfcFormController.cs:493-494`, `:772-773`), so
  `FolderPredictor` is not changed (D5).
- `BreadcrumbRowBuilder.cs`'s "derives no hierarchy from row text" contract is untouched; the percentage fix is
  an additive score alias at the `BindRowsAsync` boundary, and `BuildProbabilityIndex` is a last-write-wins
  dictionary build (`BreadcrumbRowBuilder.cs:217-224`), so aliasing cannot drop an existing key (D6).
- The `arrowKey` / `unhandledArrow` message shapes are unchanged (D1).
- `BreadcrumbSelectionSession` is not written (D1).
- Per-surface boundary behavior is unchanged (D2).
- `IFolderHierarchyProvider` gains no member. `GetImmediateSubfoldersAsync`
  (`IFolderHierarchyProvider.cs:46-49`) plus `ResolveLeafKeyAsync` are sufficient (research §Q4d).
- #400 AC-5 through AC-8 (Up/Down/Enter/Escape) are preserved (research §Q4c).

### Dependencies or blocked work:

- **Intra-feature:** #440 depends on #439 part A (research §Q4f). #439 depends on #499 landing first only for
  bisectability, not functionally.
- **Cross-feature (informational, non-blocking):** feature 464 (`EfcFormController.cs`) must be told that after
  #499, `SelectedFolder` can return `null` immediately after a re-bind. Per D4 this is already tolerated by
  `IsValidSelection` (`EfcFormController.cs:1039-1050`, `:1046`) and both call sites guard on it (`:470`,
  `:754`), so no change to feature 464 is required.
- **Cross-feature (potentially blocking, gated by D7):** `BreadcrumbSelectionMap.cs:109` — see RISK-1. Only
  rung 3 of the D7 ladder makes this blocking.
- **Cross-feature notes (descoped, D3):**
  - Efc ancestor-click gesture — would require writing `BreadcrumbDocumentAssets.cs:59-67` (the delegated
    `dblclick` listener posting `{ type: 'segmentDoubleClick', rowId, segmentIndex }`) and its surrounding
    `BridgeJs` block at `:59-89`. Not owned. Should become a follow-up potential entry.
  - Efc `→` separator glyph — would require writing `BreadcrumbHtmlRenderer.cs:147-150`, which emits
    `<span class="sep"> &gt; </span>`. Not owned. Should become a follow-up potential entry.
  - Neither follow-up entry is created by this feature.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

| File | Change | Line budget (research §Q6c) |
|---|---|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | #498 guard (`:168-174`), #499 clear + event (`:114`, `:364-380`), #439 presented-text map and score aliasing (`BindRowsAsync` `:88-116`), #440 Efc transitions (`HandleArrowKeyAsync` `:225-260`) | 450/500 — HIGH risk; partial split pre-authorized (D8) |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | #439 unique segment-boundary suffix fallback in `ResolveLeafKeyAsync` (`:52-71`) plus `Error` logging on miss/ambiguity | 98/500 — no risk |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | #440 selected-node index field and its transitions | 265/500 — low risk |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | #440 Qfc transitions (`RightArrow` `:424-437`, `LeftArrow` `:443-455`) | 457/500 — HIGH risk; partial split pre-authorized |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | #440 Qfc routing (`ArrowAsync` `:378-406`); D7 rung 1 stem preservation in `CreateFallbackRow` / `ReplaceRowsPreservingSession` | 485/500 — VERY HIGH risk; partial split pre-authorized |
| `QuickFiler/Resources/FolderBreadcrumb.html` | #440 `onArrow` gating (`:395-404`), message shapes unchanged | 489/500 — HIGH risk; **cannot be split**, 11 lines of headroom is a hard constraint |
| `QuickFiler/Controllers/KeyboardHandler.cs` | No change expected. Owned so that the D2 fall-through (`:288-315`) can be verified unchanged. | 414/500 — low risk |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | **No change** (D5). Listed as owned only; writing it is rejected. | 983 — pre-existing violation |

#### Functions/classes/CLI commands impacted:

- `BreadcrumbBridgeRouter.ProcessInboundAsync` — `SegmentDoubleClick` arm.
- `BreadcrumbBridgeRouter.BindRowsAsync` — selection clear, presented-text map, score aliasing.
- `BreadcrumbBridgeRouter.SelectRow` (`:364-380`) — `SelectedFolderPath` derivation.
- `BreadcrumbBridgeRouter.HandleArrowKeyAsync` (`:225-260`) — new transitions ahead of existing behavior.
- `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` (`:52-71`).
- `BreadcrumbRow` — new selected-segment index plus transitions; `LeftArrow()` (`:195-216`) and `RightArrow()`
  (`:224-243`, currently unused by the Efc router) retained as the second-priority behavior.
- `BreadcrumbStateModel.RightArrow` / `LeftArrow`; `FolderBreadcrumbBridgeRouter.ArrowAsync`.
- `FolderBreadcrumb.html` `onArrow` (`:395-404`) and its keydown wiring (`:420-426`).
- No CLI commands.

#### Data flow and validation changes:

- **#498** — inbound `segmentIndex` is range-checked against the row's segment count in the router before
  `CollapseAfter` is called. An out-of-range value produces no transition and no render post.
- **#499** — `BindRowsAsync` writes `SelectedFolderPath = null` and raises `SelectedFolderPathChanged(this,
  null)` only when the previous value was non-null.
- **#439** — `ResolveLeafKeyAsync` gains a second pass: exact `OrdinalIgnoreCase` equality first (identity
  case, zero behavior change for a caller that already supplies a full path), then a segment-boundary suffix
  match `node.FolderPath.EndsWith("\\" + folderPath, OrdinalIgnoreCase)`, accepted **only when unique**.
  Uniqueness is the safety property: it prevents `Projects\Alpha` from binding to `\\store\Inbox\Projects\Alpha`
  when `\\store\Archive\Projects\Alpha` also exists. The archive-root value is not needed by this rule, which
  is why it works despite `ArchiveRootPath` being unavailable at every owned site (research §Q3e).
- **#439 / D6** — `BindRowsAsync` retains a `rowId -> presentedText` map (the loop at
  `BreadcrumbBridgeRouter.cs:88-107` already visits each presented text, and `BuildRows` assigns `row-{i}` over
  the same sequence, `BreadcrumbRowBuilder.cs:53-57`, so the correspondence is positional and exact).
  `SelectRow` derives `SelectedFolderPath` from the presented text rather than `row.LeafSegment?.FullPath`.
  `BindRowsAsync` also extends the `scores` sequence with an alias `FolderScore` for each presented text whose
  chain resolved: `new FolderScore(resolvedChain[last].FolderPath, originalScore.Score,
  originalScore.Probability)`. `FolderScore` is a net48-safe `readonly struct` with a public
  `(string folderPath, long score, double probability)` constructor (`FolderScore.cs:22-27`).
- **#440** — expanding an ancestor segment on the Efc surface must re-resolve by path, because `MapSegments`
  drops `FolderBreadcrumbSegment.Key` (`BreadcrumbRowBuilder.cs:196-202`) and `BreadcrumbSegment` carries only
  `FullPath`, `DisplayName`, `HasSubfolders` (`BreadcrumbSegment.cs:29-43`). The pattern is
  `ResolveLeafKeyAsync(ancestorSegment.FullPath, ct)` then `GetImmediateSubfoldersAsync(key, ct)` — exactly the
  two-call pattern `ExpandLeafAsync` already uses at `BreadcrumbBridgeRouter.cs:296-309`. The Qfc side does not
  need this: `BreadcrumbStateRow.Chain` holds `FolderBreadcrumbSegment`, which does carry `Key`
  (used at `FolderBreadcrumbBridgeRouter.cs:416`).

#### Error handling and logging updates:

- **#498** — the rejected index is logged at `Error` using the existing `log4net` pattern in the same file
  (`BreadcrumbBridgeRouter.cs:162`, `:257`). No exception is thrown and no exception escapes.
- **#439** — when both passes miss, or when the suffix fallback is ambiguous, `ResolveLeafKeyAsync` logs at
  `Error` and returns `null`, so a systematic resolution failure is visible rather than presenting as a
  cosmetic omission (research §Q3e, matching `BreadcrumbBridgeRouter.cs:162`, `:257`, `:302-305`).
- **#499** — no logging change. The clear is a normal state transition, not an error.
- **#440** — an unmapped key continues to hit the existing `default:` branch and its `log.Error`
  (`BreadcrumbBridgeRouter.cs:256-258`).
- The existing broad `catch (Exception ex)` in `ExpandLeafAsync` (`:324-331`) is retained unchanged; it already
  contains the provider path.

#### Rollback/feature-flag considerations (if applicable):

No feature flag. Each of the four fixes is an independent commit in the stated sequence, so rollback is a
revert of the corresponding commit. The sequencing rationale in "Design summary" is chosen partly to keep the
two changes to the `SelectedFolderPath` assignment separately bisectable.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- **Inbound bridge messages** are unchanged in shape. The codec produces exactly four inbound fields
  (`BreadcrumbMessageCodec.cs:93-113`): `type` (required, enum-checked at `:93-97`, `:116-122`), `rowId`
  (required string, `:99`, `:124-140`; unknown id is already a logged no-op at
  `BreadcrumbBridgeRouter.cs:160-164`), `segmentIndex` (`OptionalInt`, `:100`, `:142-158`), `key` (non-empty
  for `arrowKey` only, `:101`, `:108-111`).
- **Outbound message shapes** are unchanged, including `arrowKey` and `unhandledArrow` (D1).
- `ResolveLeafKeyAsync` signature is unchanged: `Task<FolderTreeNodeKey?> ResolveLeafKeyAsync(string
  folderPath, CancellationToken cancellationToken)`. Return type is already nullable.
- `IFolderHierarchyProvider` gains no member (`IFolderHierarchyProvider.cs:46-49` is reused as-is).
- `SelectedFolderPath` remains `string?` (`BreadcrumbBridgeRouter.cs:58`) and `SelectedFolderPathChanged`
  remains `EventHandler<string?>?` (`:61`).

#### Required configuration keys and defaults:

None. No configuration key is added, read, or changed.

#### Backward-compatibility expectations:

- **Observable behavior change, #499:** after any `BindRowsAsync` re-bind that follows a selection,
  `EfcFormController.SelectedFolder` returns `null` instead of the previous folder, until the user re-selects.
  A move or folder-open triggered in that window acts on a null selection rather than a stale folder. Both call
  sites guard on `!IsValidSelection` first (`EfcFormController.cs:470`, `:754`), and `IsValidSelection`'s first
  disjunct is `selectedFolder is null` (`:1039-1050`), so the guard rejects it (D4).
- **Observable behavior change, #439:** rows that previously rendered leaf-only now render their full lineage.
  The single-segment fallback remains for rows whose chain genuinely cannot be resolved.
- **Observable behavior change, #440:** Left and Right perform tree navigation where a transition is available.
  Where none is available, prior behavior is unchanged (D1 handling order, D2 boundaries).
- **Preserved contracts:** the suggestion-row percentage (D6a), the Efc filing target (D6b), the Qfc filing
  target (D7), the `arrowKey`/`unhandledArrow` message shapes (D1), the Qfc `unhandledArrow` fall-through
  including the Pop Out / Enumerate Conversation entry point (D2), and #400 AC-5 through AC-8.
- **Nullable analysis** (research §Q6b): all owned `.cs` files except `KeyboardHandler.cs` carry
  `#nullable enable` at line 1, so `CS86xx` diagnostics are promoted to errors under
  `/p:TreatWarningsAsErrors=true`. Removing the `!` at `BreadcrumbBridgeRouter.cs:169` in favour of a
  `HasValue` check is safer under nullable analysis, not riskier. Do **not** add `/p:Nullable=enable` to the
  msbuild command; the CI command is the authority and this repository has no `Directory.Build.props`.

#### Performance constraints (latency/throughput/memory):

- `ResolveLeafKeyAsync` currently performs one `FirstOrDefault` scan over `snapshot.NodesByKey.Values`
  (`OutlookFolderHierarchyProvider.cs:63-66`). The fallback adds at most one further scan, executed only when
  the first pass misses. The uniqueness requirement means the second pass must enumerate rather than
  short-circuit. This runs against an in-memory snapshot, not against COM, so the cost is bounded by node
  count.
- `BindRowsAsync` gains a positional `rowId -> presentedText` map and an additive score-alias sequence, both
  O(row count) over a set bounded by the presented row list.
- No new I/O, no new network or COM call, no new allocation on the message-receive hot path beyond the
  range check.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access):
  - The presented row text is a relative path form on all three suggestion sources. Research §Q3a verifies the
    word-sequence source is store-root-relative (`SubjectMapSco.Orchestration.cs:30`, `:33`) and the search
    matches are archive-root-relative; the Bayesian `prediction.Class` (`FolderScorer.cs:178`) and the
    conversation-map `EmailFolder` (`:323`) forms are recorded as **unverified**. What is verified is that none
    of the three is a full Outlook `FolderPath` beginning with `\\<store>`. The segment-boundary suffix rule of
    D5 is chosen precisely because it does not depend on which relative root a given source uses.
  - `ArchiveRootPath` is `Path.Combine(Root.FolderPath, "Archive")` (`TaskMaster/AppGlobals/AppOlObjects.cs:238-248`,
    root at `:202-210`), i.e. `<storeRootFolderPath>\Archive`. The fix does not read this value.
- Constraints (budget, performance, compatibility):
  - .NET Framework 4.8.1; legacy `.csproj` files with explicit `Compile Include` entries.
  - 500-line file limit per `.claude/rules/general-code-change.md`; D8 governs.
  - `QuickFiler.Test` deliberately carries no Newtonsoft reference, so all outbound assertions are raw-JSON
    substring assertions (`BreadcrumbBridgeRouterTests.cs:19-20`). This constraint is kept.
  - `MyBox.ShowDialog` (`KeyboardHandler.cs:304-309`) is a modal WinForms call with no injectable seam. Any
    test reaching the Qfc Right fall-through would block, so tests assert at the
    `BreadcrumbArrowFallThrough` call site instead (precedent:
    `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:156-168`, mocking
    `IQfcKeyboardHandler.BreadcrumbArrowFallThrough`, declared at `QuickFiler/Interfaces/IQfcKeyboardHandler.cs:32`).
  - Sibling epic children execute concurrently against the same integration branch, so the unowned-file list is
    a hard constraint and `.csproj` edits must be alphabetically placed.
- External dependencies (services, libraries, releases): MSTest, Moq, FluentAssertions, log4net. No new
  dependency is added.

## Data / API / Config Impact

- User-facing or API changes:
  - Efc and Qfc folder rows render their full ancestor lineage instead of a leaf-only segment (#439 part A).
  - Left and Right perform tree navigation where a transition is available (#440), with prior behavior retained
    as the second and third priorities (D1) and with per-surface boundary behavior unchanged (D2).
  - `EfcFormController.SelectedFolder` returns `null` after a re-bind until the user re-selects (#499).
  - An out-of-range `segmentIndex` is a logged no-op instead of a host-process crash (#498).
  - No public interface member is added or removed. `IFolderHierarchyProvider` is unchanged.
- Data or migration considerations: none. No persisted data, schema, or stored setting is read or written.
- Logging/telemetry updates (if any): two new `Error`-level log sites, both using the existing `log4net`
  pattern — the rejected out-of-range `segmentIndex` (#498) and the unresolved or ambiguous leaf key (#439).
- Compatibility notes (CLI flags, config schemas, versioning): none. No CLI flag, config schema, or version
  identifier is affected.

## Test Strategy

Framework: **MSTest** (`[TestClass]` / `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`).
Mocking: **Moq**. Assertions: **FluentAssertions**. No live Outlook or COM dependency; no temporary files.

- Regression tests to add or update (one failing test FIRST per defect, per the CLAUDE.md Bugfix Workflow;
  homes from research §Q5a):

  | Defect | Regression-test file | Seam |
  |---|---|---|
  | **#498** | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | The only file with the async-void-boundary seam `_host.Raise(h => h.MessageReceived += null, _host.Object, "<json>")` (`:201`, pattern at `:194-205`). `Setup()` at `:34-96` produces a two-segment `row-0` via `Bind()`, so `segmentIndex: 99` and `segmentIndex: -1` are both out of range and `segmentIndex: 0` is the valid control. RED assertion: `Action act = () => _host.Raise(...); act.Should().NotThrow();` — deterministic because Moq's `Raise` is synchronous and every awaited task is already completed. "State unchanged" asserts `_posted.Count.Should().Be(postedBefore)`, the idiom already used at `:140`/`:146`, `:164`/`:170`, `:314`/`:320`, `:379`/`:385`, `:410`/`:416`. |
  | **#499** | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | Same `Setup()`; `Bind()`, then `Inbound("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}")` (idiom at `:189`, `:432`), then a second `Bind()` (double-`Bind` pattern at `:428-444`), then `_router.SelectedFolderPath.Should().BeNull()`. Event assertion reuses `BreadcrumbBridgeRouterTests.cs:219`: `string observed = "sentinel"; _router.SelectedFolderPathChanged += (s, path) => observed = path;`. Existing test `MalformedInboundJson_ThrowsCodecExceptionWithoutCorruptingState` (`:175-191`) must be read and confirmed still passing; it is unaffected because nothing was selected before its bind. |
  | **#439 — provider resolution** | `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` (282 lines) | Owns `ResolveLeafKeyAsync` coverage (`:100-192`). Real provider over a mocked `IOutlookFolderTreeService` (`ServiceReturning`, `:231-280`). **Caution (research §Q5c):** the existing `Node` helper passes `displayName` as the `relativePath` argument (`:275`), which is not a realistic relative path. The #439 test must construct nodes with a realistic full path (`\\store\Archive\Projects\Alpha`) and must include a **decoy** node (`\\store\Inbox\Projects\Alpha`) to pin the uniqueness requirement. |
  | **#439 — Efc bind/join/selection** | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` (435 lines) | Owns the bind-to-document assertions including the `"90%"` join (`:126-136`) and `SelectedFolderPath` (`:214-227`). The D6 percentage-preservation and filing-target criteria belong here. The existing `SetupProviderChain` mock (`:77-106`) returns `Key(path)` for **any** input and therefore cannot reproduce the #439 mismatch; a #439 test needs a path-form-sensitive mock. |
  | **#439 — Qfc bind** | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` (314 lines) | Owns `SetSuggestionsAsync` → chain resolution (`:72-85`). Its `ProviderMock` (`:51-70`) uses `MockBehavior.Strict` with per-path setups — the right pattern for #439, because resolving the wrong path form throws rather than silently succeeding, making the RED test fail for the intended reason. |
  | **#440 — Efc transitions** | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs` (334 lines) **and** `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | The state machine and its routing are separately covered today; that split is kept. |
  | **#440 — Qfc transitions** | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs` (320 lines) **and** `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` | `BreadcrumbStateModelSelectorTests.cs` / `BreadcrumbStateModelSequenceTests.cs` are the #400 selector-session and sequence files; per D1 the selector session is not touched, so they are used only for confirming no regression. |
  | **#440 — html contract** | `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` (405 lines) | **Not written.** Per D1, `LeftAndRightBreadcrumbMessages_RemainSupported` (`:359-367`) must pass unmodified. It asserts against the compiled resource string `QuickFiler.Properties.Resources.FolderBreadcrumb` (`:19`, wiring at `QuickFiler/Properties/Resources.Designer.cs:184`) with no browser, no WebView2, and no JS engine, so it is deterministic. |

  **No `.csproj` edit is required for any of these test files.** Research §Q5b verifies every one is already
  included: `QuickFiler.Test.csproj:58` (`BreadcrumbBridgeRouterQueueTests.cs`), `:59`
  (`BreadcrumbBridgeRouterTests.cs`), `:95` (`FolderBreadcrumbAssetContractTests.cs`);
  `UtilitiesCS.Test.csproj:279` (`BreadcrumbRowStateTests.cs`), `:283` (`FolderBreadcrumbBridgeRouterTests.cs`),
  `:290` (`BreadcrumbStateModelTests.cs`), `:301` (`OutlookFolderHierarchyProviderTests.cs`). A `.csproj` edit
  is needed only if the plan adds a **new** file, in which case the entry goes in its own alphabetical
  neighbourhood (D8) and, per #400 AC-17 (`spec.md:255`), every added test `.cs` must be explicitly included.

- Unit tests for the fixed behavior and boundaries (the template's "pytest" wording does not apply; this is a
  C# repository and the framework is MSTest):
  - #498 — out-of-range high, out-of-range negative, valid index control case, banner/trash row short-circuit.
  - #499 — clear after re-bind; event raised once with `null` when the value changed; event **not** raised when
    the value was already `null`; no auto-selection after re-bind.
  - #439 — identity case (full path resolves exactly as today); relative stem resolves via unique suffix;
    ambiguous stem with a decoy node returns `null` and logs; unresolvable stem returns `null` and preserves
    the single-segment fallback; resolved chain yields multiple segments in root-to-leaf order on both surfaces.
  - #440 — Left from a leaf selects its parent; repeated Left walks to the root; Right on a selected parent
    requests and shows that parent's children via `GetImmediateSubfoldersAsync`; handling-order priority
    (tree transition, then existing expand/collapse, then unhandled) asserted on both surfaces.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
  - `segmentIndex` of `-1`, `0`, `segments.Count - 1`, `segments.Count`, `99`.
  - Empty or whitespace `folderPath` into `ResolveLeafKeyAsync` (early `null` at
    `OutlookFolderHierarchyProvider.cs:57-60`) — unchanged behavior.
  - Left at the root and Right on a childless node, per surface (D2).
  - Re-bind with no prior selection; re-bind with a prior selection that survives the new row set.
- Error handling and logging verification: assert the `Error` log site is reached for the rejected
  `segmentIndex` and for the unresolved/ambiguous leaf key, and that neither path throws.
- Coverage impact and targets for changed lines/modules: changed lines must not reduce coverage. New behavior
  added to `OutlookFolderHierarchyProvider`, `BreadcrumbBridgeRouter`, `BreadcrumbRow`, `BreadcrumbStateModel`,
  and `FolderBreadcrumbBridgeRouter` targets `>= 90%` line coverage as new logic, per the General Unit Test
  Policy. Coverage is collected with `/EnableCodeCoverage`.
- Toolchain commands to run (format → lint → type-check → test), in this exact order, restarting from step 1 if
  any step fails or changes files:
  1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- Manual validation steps (if required): in EfcViewer, confirm suggestion and search rows show the full
  ancestor chain and that the suggestion percentage is still displayed; select a row, type an additional search
  character, and confirm a subsequent move does not target the previously selected folder; in QuickFiler,
  confirm the Pop Out / Enumerate Conversation dialog is still reachable by the same gesture as today and that
  Up/Down/Enter/Escape selector behavior from #400 is unchanged.

## Acceptance Criteria

- [ ] **AC-1 (#498)** — A `segmentDoubleClick` message with `segmentIndex` outside `[0, segments.Count - 1]`
      is rejected by a range guard in the `SegmentDoubleClick` arm of
      `BreadcrumbBridgeRouter.ProcessInboundAsync` (`:166-174`); no exception escapes
      `_host.Raise(h => h.MessageReceived += null, ...)`, and the outbound posted-message count is unchanged.
- [ ] **AC-2 (#498)** — The rejected index is logged at `Error` using the existing `log4net` pattern in the
      same file (`BreadcrumbBridgeRouter.cs:162`, `:257`), and `BreadcrumbRow.CollapseAfter`
      (`BreadcrumbRow.cs:104-133`) is unmodified: its documented throw contract (`:101-103`, `:111-118`)
      still holds when called directly.
- [ ] **AC-3 (#498)** — A valid `segmentIndex` still collapses the row and posts a render, and the
      `catch (BreadcrumbMessageException)` at `BreadcrumbBridgeRouter.cs:187-198` is still the only catch at
      the `async void` host-message boundary (no broad `catch (Exception)` added there).
- [ ] **AC-4 (#499)** — `BindRowsAsync` sets `SelectedFolderPath` to `null` alongside `_selectedRowId = null`
      (`BreadcrumbBridgeRouter.cs:114`), so after a re-bind that follows a selection `SelectedFolderPath` is
      `null` rather than the pre-rebind folder.
- [ ] **AC-5 (#499)** — `SelectedFolderPathChanged(this, null)` is raised on that clear **only when the value
      actually changed**; a re-bind with no prior selection raises no event.
- [ ] **AC-6 (#499)** — No auto-selection side effect is introduced: `SelectFirstRow`
      (`BreadcrumbBridgeRouter.cs:119-126`) is still not called from `BindRowsAsync`.
- [ ] **AC-7 (#439 part A)** — `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` (`:52-71`) keeps its exact
      `OrdinalIgnoreCase` first pass: a caller supplying a full Outlook path resolves exactly as it does today.
- [ ] **AC-8 (#439 part A)** — When the exact pass misses, a segment-boundary suffix match resolves an
      archive-root-relative stem (for example `Projects\Alpha`) to the unique node whose `FolderPath` ends with
      `\Projects\Alpha`.
- [ ] **AC-9 (#439 part A)** — The suffix fallback is accepted **only when unique**: with a decoy node
      (`\\store\Inbox\Projects\Alpha` alongside `\\store\Archive\Projects\Alpha`) the method returns `null`,
      logs at `Error`, and the row keeps today's single-segment fallback rendering
      (`BreadcrumbRowBuilder.cs:119-134`, which is not modified).
- [ ] **AC-10 (#439 part A)** — On the Efc surface, a bound suggestion or search row whose stem resolves
      renders a multi-segment ancestor chain in root-to-leaf order, asserted in
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs`.
- [ ] **AC-11 (#439 part A)** — On the Qfc surface, the same resolution produces a multi-segment chain,
      asserted in `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` against a
      `MockBehavior.Strict` provider (`:51-70`).
- [ ] **AC-12 (#439, decision D6a — preservation)** — The suggestion-row **percentage is still rendered** after
      the chain resolves. A test binds a row whose chain resolves and whose `FolderScore` is keyed on the
      presented stem, and asserts the percentage still appears in the delivered document (the existing
      `"90%"` assertion shape at `BreadcrumbBridgeRouterTests.cs:126-136`). `BreadcrumbRowBuilder.cs` is not
      modified; preservation is achieved by additive `FolderScore` aliasing at the `BindRowsAsync` boundary.
- [ ] **AC-13 (#439, decision D6b — preservation)** — The **Efc filing target is still the presented stem**
      after the chain resolves. `SelectRow` (`BreadcrumbBridgeRouter.cs:364-380`) derives `SelectedFolderPath`
      from the presented text rather than `row.LeafSegment?.FullPath`, and a test asserts that selecting a row
      whose chain resolved yields the stem, not the full `\\store\...` path, so the `DestinationOlStem`
      contract at `EfcDataModel.cs:286-289` is unbroken.
- [ ] **AC-14 (#439, decision D7 — Qfc filing target)** — The D7 ladder rung actually taken is recorded in this
      spec's RISK-1 entry with the read-only evidence that selected it, and that rung's stated criterion is
      met: rung 1 — a test shows the Qfc selected-folder value is still the presented stem after the chain
      resolves, with `BreadcrumbSelectionMap.cs` unmodified; rung 2 — a test shows Qfc filing behavior is
      byte-identical to today because the router does not consume the newly-resolved chain for the filing path,
      and the deliberate limitation is recorded in this spec; rung 3 — execution halted and reported the
      blocking dependency on `BreadcrumbSelectionMap.cs:109` without writing it.
- [ ] **AC-15 (#440)** — On the Efc surface, Left on a row whose resolved chain has more than one segment
      selects that row's parent node, and repeated Left presses walk up the ancestor chain.
- [ ] **AC-16 (#440)** — On the Efc surface, Right on a selected node expands it into its children, retrieved
      through the existing `IFolderHierarchyProvider.GetImmediateSubfoldersAsync`
      (`IFolderHierarchyProvider.cs:46-49`) via the `ResolveLeafKeyAsync` → `GetImmediateSubfoldersAsync`
      pattern already used at `BreadcrumbBridgeRouter.cs:296-309`. No member is added to
      `IFolderHierarchyProvider`.
- [ ] **AC-17 (#440)** — The Qfc surface implements the same Left/Right tree contract through
      `BreadcrumbStateModel` (`:424-455`) and `FolderBreadcrumbBridgeRouter.ArrowAsync` (`:378-406`), asserted
      by tests on both the state model and the router.
- [ ] **AC-18 (#440, decision D1 — handling order)** — On both surfaces the handling order is exactly:
      (1) the new parent-select / expand-node transition when one is available for this row, else (2) the
      existing breadcrumb expand/collapse behavior, else (3) the existing `unhandledArrow` fall-through
      unchanged. A test asserts a row with a single-segment chain still takes the pre-existing path.
- [ ] **AC-19 (decision D1 — message shapes)** — The `arrowKey` and `unhandledArrow` message shapes are
      unchanged, and `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`
      `LeftAndRightBreadcrumbMessages_RemainSupported` (`:359-367`) passes **unmodified**; that file does not
      appear in the feature diff.
- [ ] **AC-20 (decision D1 — selector session)** — `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs`
      is unmodified, and the #400 selector-session tests
      (`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSelectorTests.cs`) pass unmodified, so
      #400 AC-9's committed/original/pending clause survives.
- [ ] **AC-21 (decision D1 — supersession record)** — This spec contains a reviewer-findable record (section
      "#400 AC-9 supersession record") naming exactly which clause of #400 AC-9
      (`docs/features/archive/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md:247`) is retracted and
      which clauses survive.
- [ ] **AC-22 (decision D1 — #400 residual contract)** — #400 AC-5 through AC-8 (`spec.md:243-246`), the
      Up/Down/Enter/Escape selector contract, are unchanged, demonstrated by the corresponding #400 tests
      passing unmodified.
- [ ] **AC-23 (decision D2 — Efc boundaries)** — Efc boundary behavior is unchanged: Left at the root and
      Right on a childless node remain silent no-ops emitting no message
      (`BreadcrumbRow.cs:209-212`; `BreadcrumbBridgeRouter.cs:243-249`, `:287-291`).
- [ ] **AC-24 (decision D2 — Qfc boundaries)** — Qfc boundary behavior is unchanged: an unhandled transition
      still emits `UnhandledArrowMessage` (`FolderBreadcrumbBridgeRouter.cs:387-393`) and still reaches
      `KeyboardHandler.BreadcrumbArrowFallThrough` (`:288-315`), so Right still opens the Pop Out / Enumerate
      Conversation dialog and Left still calls `SetFolderDroppedDown(false)`. Asserted at the
      `BreadcrumbArrowFallThrough` call site (precedent
      `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:156-168`), never by invoking the
      modal `MyBox.ShowDialog`.
- [ ] **AC-25 (#498 — RED first)** — A regression test for #498 in
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` is demonstrated **failing before the
      fix** and passing after, with the failing run recorded in the feature's evidence directory.
- [ ] **AC-26 (#499 — RED first)** — A regression test for #499 in
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` is demonstrated failing before the fix
      and passing after, with the failing run recorded.
- [ ] **AC-27 (#439 — RED first)** — A regression test for #439 part A in
      `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` (realistic full paths
      plus a decoy node) is demonstrated failing before the fix and passing after, with the failing run
      recorded.
- [ ] **AC-28 (#440 — RED first)** — A regression test for #440 in
      `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs` and/or
      `BreadcrumbStateModelTests.cs` is demonstrated failing before the fix and passing after, with the failing
      run recorded.
- [ ] **AC-29 (policy — toolchain)** — The full C# toolchain passes in one clean pass, in order:
      `dotnet tool run csharpier format .` (verified by `dotnet tool run csharpier check .`), the
      `EnableNETAnalyzers`/`EnforceCodeStyleInBuild` rebuild, the `TreatWarningsAsErrors` rebuild, and
      `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`, with the commands and their results
      reported.
- [ ] **AC-30 (policy — ownership)** — No file outside the owned set listed under "Scope & Non-Goals" is
      modified. In particular `EfcFormController.cs`, `KbdActions.cs`, `BreadcrumbRowBuilder.cs`,
      `BreadcrumbDocumentAssets.cs`, `BreadcrumbHtmlRenderer.cs`, `BreadcrumbSelectionMap.cs`,
      `IFolderHierarchyProvider.cs`, and `FolderBreadcrumbAssetContractTests.cs` are absent from the diff.
- [ ] **AC-31 (policy — file size, decision D8)** — Every file written by this feature is at or under 500
      lines. Where a partial-class split was used, the new partial file carries a `Compile Include` placed in
      its own alphabetical neighbourhood in `QuickFiler.csproj` or `UtilitiesCS.csproj`, and
      `QuickFiler/Resources/FolderBreadcrumb.html` remains at or under 500 lines without being split.

## Risks & Mitigations

- Technical or operational risks:

  - **RISK-1 (HIGHEST, decision D7) — Qfc filing target flips from stem to full path after #439.**
    Once #439 makes Qfc chains resolve, `BreadcrumbSelectionMap.GetSelectedFolder` returns
    `row.Chain[row.Chain.Count - 1].FolderPath` for a suggestion row (`BreadcrumbSelectionMap.cs:109`), and
    that file is **not owned**. Research §Q6a records this as the feature's single largest open risk and marks
    the in-ownership alternative **unverified**.
    **Resolution ladder, taken in order. The choice is made by READING `BreadcrumbStateRow` and
    `CreateFallbackRow` during execution; the reading is read-only.**
    1. **PREFERRED** — preserve the presented stem through the owned `FolderBreadcrumbBridgeRouter.cs`
       (`CreateFallbackRow` / `ReplaceRowsPreservingSession`), mirroring the Efc fix, so
       `BreadcrumbSelectionMap.cs` needs no change.
       *Criterion:* a test shows the Qfc selected-folder value is still the presented stem after the chain
       resolves, and `BreadcrumbSelectionMap.cs` is absent from the diff.
    2. **FALLBACK** — if rung 1 is not achievable in owned files, the Qfc router must not consume the
       newly-resolved chain for the filing-target path, explicitly preserving today's Qfc filing behavior.
       *Criterion:* a test shows Qfc filing behavior is unchanged from today, and this spec is updated to
       record that Qfc lineage display improves while Qfc filing is deliberately left on the old path form.
    3. **HALT** — if neither is achievable, this is a BLOCKING cross-feature dependency on
       `BreadcrumbSelectionMap.cs` and execution stops and reports it rather than writing an unowned file.
       *Criterion:* execution halted, the blocking dependency reported with the `BreadcrumbSelectionMap.cs:109`
       citation, and no unowned file written.
    **Rung taken:** _to be recorded during execution._

  - **RISK-2 (decision D6a) — suggestion-row percentage silently lost.** The percentage works today *because*
    the lineage is broken: `joinPath == presentedText == scorer key`. After resolution `joinPath` becomes the
    full Outlook path while `BuildProbabilityIndex` stays keyed on the stem
    (`BreadcrumbRowBuilder.cs:131`, `:208-227`). The existing test at `BreadcrumbBridgeRouterTests.cs:126-136`
    will **not** catch this, because `SetupProviderChain` (`:77-106`) returns a chain whose leaf `FolderPath`
    equals the presented text. *Mitigation:* AC-12, plus additive `FolderScore` aliasing at the `BindRowsAsync`
    boundary; `BuildProbabilityIndex` is last-write-wins (`:217-224`) so aliasing cannot drop an existing key.

  - **RISK-3 (decision D6b) — Efc filing target silently broken.** Same shape as RISK-2, on
    `BreadcrumbBridgeRouter.cs:372-375` feeding `DestinationOlStem` at `EfcDataModel.cs:286-289`.
    *Mitigation:* AC-13, deriving `SelectedFolderPath` from the retained presented text. This makes the filing
    contract independent of whether the chain resolved, which is a strict improvement over today.

  - **RISK-4 (decision D8) — 500-line breach discovered mid-execution.** Four owned files are within 50 lines
    of the limit and one (`FolderBreadcrumb.html`, 489) cannot be split. *Mitigation:* partial-class splits are
    pre-authorized for the three `.cs` files with in-repo precedent
    (`FolderBreadcrumbBridgeRouter.SearchPresentation.cs`, `UtilitiesCS.csproj:629-630`); the html change must
    fit in 11 lines; AC-31 gates the result.

  - **RISK-5 — `.csproj` rebase conflict with sibling epic children.** The `Compile Include` item groups are
    alphabetically ordered and shared with concurrently-executing children. *Mitigation:* place any new entry
    in its own alphabetical neighbourhood (D8); research §Q5b confirms no `.csproj` edit is needed for the
    existing test files.

  - **RISK-6 — #440 is untestable against production data if sequenced before #439.** Research §Q4f verifies
    both transitions are permanent no-ops on a one-segment row. *Mitigation:* the fixed sequence
    #498 → #499 → #439 part A → #440.

  - **RISK-7 — unverified relative-root variance across suggestion sources.** Research §Q3a marks the Bayesian
    `prediction.Class` and conversation-map `EmailFolder` path forms unverified. *Mitigation:* D5's
    segment-boundary suffix rule does not depend on which relative root a source uses, and AC-9's uniqueness
    requirement fails closed to today's rendering when the form is ambiguous.

  - **RISK-8 — #499's null selection reaching a null-intolerant consumer.** *Mitigation:* resolved by D4's
    orchestrator-verified reading of `EfcFormController.IsValidSelection` (`:1039-1050`) and both guarded call
    sites (`:470`, `:754`). Residual risk is low and the cross-feature note to feature 464 is informational.

- Mitigations and rollbacks: each defect lands as a separate commit in the stated sequence, so any single fix
  can be reverted independently. No feature flag and no configuration switch is introduced.

## Rollout & Follow-up

- Release/rollout steps: merge into the epic integration branch `epic/quickfiler-bug-family-integration` after
  the full toolchain passes in one clean pass and all acceptance criteria above are checked off. No deployment
  artifact, migration, or configuration change accompanies this feature.
- Post-fix monitoring or clean-up tasks:
  - Watch the two new `Error` log sites. A high rate of unresolved or ambiguous leaf keys would indicate that
    the segment-boundary suffix rule does not cover one of the suggestion sources whose path form research
    §Q3a marks unverified (RISK-7).
  - Manual confirmation in EfcViewer and QuickFiler per "Test Strategy — Manual validation steps".
- Follow-up items (cross-feature notes; this feature does not create them):
  - **#439 part B descope (D3)** — the Efc mouse gesture that single-clicks a non-leaf segment to navigate to
    that ancestor. Requires writing `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs:59-67`
    (within `BridgeJs`, `:59-89`), which is not owned. The equivalent *capability* is delivered by #440's
    keyboard Left/Right transitions on both surfaces; the residual is the mouse gesture only. Should become a
    follow-up potential entry.
  - **Efc separator glyph descope (D3)** — changing the Efc separator from `>` to `→` requires writing
    `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs:147-150`, which is not owned. Purely cosmetic.
    Should become a follow-up potential entry.
  - **Pre-existing 500-line violations (D8)** — `FolderPredictor.cs` (983 lines) and `EfcFormController.cs`
    (1086 lines), plus the test file `FolderPredictorTests.cs` (985 lines). Not inherited and not worsened by
    this feature.
- Links:
  - Issues: #498 (primary), #439, #440, #499.
  - Feature folder: `docs/features/active/breadcrumb-router-navigation-defects-498/`.
  - Research (primary input, verified at HEAD `988e819b`):
    `docs/features/active/breadcrumb-router-navigation-defects-498/research/2026-08-24T09-50-breadcrumb-router-navigation-defects.md`.
  - Superseded-in-part criterion: `docs/features/archive/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md:247`.
  - Promoted potentials: the four documents listed in "Context".
  - PRs: to be recorded on creation.
</content>
</invoke>
