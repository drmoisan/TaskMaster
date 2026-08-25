# breadcrumb-router-navigation-defects (Spec)

- **Issue:** #498
- **Also closes:** #440, #499. #439 is inherited as already fixed on `main` — see "Post-#439 Reconciliation".
- **Parent (optional):** epic `quickfiler-bug-family` (integration branch `epic/quickfiler-bug-family-integration`)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-25
- **Status:** Ready for planning
- **Version:** 1.1
- **Work Mode:** full-bug — this file is the authoritative acceptance-criteria source. `user-story.md` is absent by design.

> Evidence base: every code claim in this document carries a `file:line` citation. Version 1.0 was authored
> against HEAD `988e819b`. Version 1.1 re-verified every citation against the current worktree contents, which
> include PR #605 (feature commit `c39db103`, the independent fix for issue #439). Where the research document
> `research/2026-08-24T09-50-breadcrumb-router-navigation-defects.md` disagrees with the current code, the
> current code governs and the "Post-#439 Reconciliation" section below records which research sections are
> superseded. Where a promoted potential document's citation disagrees with the research document, the research
> document's line numbers govern (research §0 correction table).
>
> Tone: this document follows `.claude/rules/tonality.md` — factual, neutral, evidence-matched wording.

## Context

- Summary of the bug and its impact (link to repro/playbook entry).

  Four defects in the breadcrumb bridge router and folder navigation surface were scoped together because they
  share the same code, the same seams, and the same test fixtures. One of the four (#439) has since been fixed
  independently on `main`; the coupling it created with #440 (research §Q4f) is therefore discharged for the
  Efc surface and remains live only for Qfc. See "Post-#439 Reconciliation" below.

  | Issue | Defect | Severity | Surface | Authoritative potential |
  |---|---|---|---|---|
  | #498 | Out-of-range `segmentIndex` escapes the `async void` host boundary and can terminate the Outlook host process | High | Efc | `docs/features/potential/promoted/2026-08-08-breadcrumb-router-segment-index-unvalidated-host-crash.md` |
  | #499 | `SelectedFolderPath` not cleared on re-bind; controller reports a folder the UI no longer shows selected | High | Efc | `docs/features/potential/promoted/2026-08-08-breadcrumb-router-stale-selectedfolderpath-after-rebind.md` |
  | #439 | Ancestor lineage never resolves; rows render as a single leaf segment — **FIXED ON `main` BY PR #605; inherited, not re-implemented here** | High | Efc + Qfc | `docs/features/potential/promoted/2026-08-07-efcviewer-missing-lineage-and-segment-navigation.md` |
  | #440 | Left/Right perform breadcrumb display-collapse, not tree navigation | Medium | Qfc + Efc | `docs/features/potential/promoted/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation.md` |

- Observed environment(s): Windows 11 Pro 10.0.26200; .NET Framework 4.8.1 WinForms VSTO add-in with
  Microsoft WebView2. Efc surface: `EfcViewer.FolderListBox` driven by `BreadcrumbBridgeRouter` over a
  document assembled by `BreadcrumbHtmlRenderer` from `BreadcrumbDocumentAssets`
  (`BreadcrumbHtmlRenderer.cs:40-49`). Qfc surface: `ItemViewer` folder selector driven by
  `FolderBreadcrumbBridgeRouter` / `BreadcrumbStateModel` over `QuickFiler/Resources/FolderBreadcrumb.html`.

- Customer impact and severity (who is affected, how often, how bad):
  - #498 — any user of the EfcViewer folder list. A malformed message from the hosted WebView2 document
    terminates the Outlook host process. Frequency is input-dependent, severity is process termination.
  - #499 — every EfcViewer search session. `BindFolderRows` (`EfcFormController.cs:871-881`) runs on every
    search keystroke, so the divergent window is common, and the failure is silent: mail can be filed to a
    folder the user can no longer see selected.
  - #439 — resolved on `main`. Before PR #605, every suggestion row and every search-result row on the Efc
    surface rendered leaf-only. The Qfc surface still renders leaf-only, which is the residual chain-resolution
    gap this feature must close as the prerequisite for #440's Qfc half.
  - #440 — keyboard-only filing cannot reach a folder that is not already in the presented row set.

- First observed date and version(s) impacted: #439 and #440 captured 2026-08-07; #498 and #499 captured
  2026-08-08 during preparation research for epic #136 child F12 (issue #495). All four were present at HEAD
  `988e819b`. #439's Efc half is fixed as of feature commit `c39db103` (PR #605). #498, #499, and #440 remain
  present in the current worktree; each was re-verified in the code at version 1.1 of this document.

## Post-#439 Reconciliation (2026-08-25)

Version 1.0 of this document was authored at HEAD `988e819b`. Since then a separate workstream fixed issue
#439 and merged it into `main` as PR #605 (feature commit `c39db103`), with its own feature folder at
`docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/`. That work is present
in this worktree. Version 1.0's #439 analysis and its D3, D5, and D6 decisions were authored against the
pre-fix shape of the code and are stale; this section records the reconciliation.

Issue #439 remains OPEN on GitHub despite the merge. Issue state is therefore not evidence of anything in
either direction; the code in this worktree is the authority for what is fixed and what is not. Every claim in
this section was established by reading the current files.

### Disposition of the four issues

| Issue | Disposition | Work items in this feature |
|---|---|---|
| #439 | **INHERITED-AND-VERIFIED.** Fixed on `main` by PR #605, including the two parts version 1.0 had descoped (the Efc mouse gesture and the `→` separator glyph). Regression coverage landed with it. | **None.** |
| #498 | **STILL IN SCOPE.** Unfixed. The `SegmentDoubleClick` arm of `ProcessInboundAsync` still calls `row.CollapseAfter(message.SegmentIndex!.Value)` with no range check (`BreadcrumbBridgeRouter.cs:241-247`), `BreadcrumbRow.CollapseAfter` still throws (`BreadcrumbRow.cs:207-214`), and `OnHostMessageReceived` is still `async void` catching only `BreadcrumbMessageException` (`:266-277`). | AC-1 through AC-3, AC-25. |
| #499 | **STILL IN SCOPE.** Unfixed. `BindRowsAsync` still sets `_selectedRowId = null` (`BreadcrumbBridgeRouter.cs:136`) and never clears `SelectedFolderPath` (`:59`). | AC-4 through AC-6, AC-26. |
| #440 | **STILL IN SCOPE on BOTH surfaces.** Unfixed. `HandleArrowKeyAsync` still routes Left to `row.LeftArrow()` and Right to `ReExpand` / `ExpandLeafAsync` (`BreadcrumbBridgeRouter.cs:304-339`). `BreadcrumbStateModel.RightArrow` / `LeftArrow` (`:424-437`, `:443-455`), `FolderBreadcrumbBridgeRouter.ArrowAsync` (`:378-406`), `KeyboardHandler.BreadcrumbArrowFallThrough` (`:288-315`), and `QuickFiler/Resources/FolderBreadcrumb.html` are all untouched by the landed work. | AC-15 through AC-24, AC-28, plus the Qfc chain-resolution prerequisite (AC-7 through AC-9, AC-11, AC-14, AC-27). |

### What #439's landed fix actually did

- **Path-form resolution (version 1.0's "#439 part A").** `BreadcrumbBridgeRouter.BindRowsAsync` gained an
  `internal` overload taking `string archiveRootPath` (`:92-138`); a new private `ToHierarchyPath`
  (`:140-163`) prefixes the archive root onto the presented stem before `ResolveLeafKeyAsync`; and
  `EfcFormController.BindBreadcrumbRowsAsync` (made `internal`, `:884-900`) passes
  `_globals.Ol.ArchiveRootPath` at `:891`. This is precisely the `BindRowsAsync` mechanism that decision D5
  REJECTED. D5's rejection rationale — that the router has no access to `ArchiveRootPath` without editing the
  unowned `EfcFormController.cs` — was correct for this feature, but the other workstream owned that file and
  made the edit.
- **Efc mouse gesture (version 1.0's "#439 part B", descoped under D3).** New inbound message types
  `segmentActivate` and `renderedChildActivate` (`BreadcrumbMessages.cs:16`, `:19`), per-type codec validation
  (`BreadcrumbMessageCodec.cs:105-118`), `data-segment-activate` and `data-child-index` attribute emission
  (`BreadcrumbHtmlRenderer.cs:182-190`, `:221-227`), click handlers in `BreadcrumbDocumentAssets.BridgeJs`
  (`:68-100`), and `ActivateSegment` / `ActivateChild` handling in the router (`:410-427`, `:429-441`).
- **Separator glyph (descoped under D3).** `BreadcrumbHtmlRenderer` now emits
  `<span class="sep">→</span>` (`:149`).
- **Active-segment model on `BreadcrumbRow`.** New members `FilingTarget` (`:88`), `ActiveSegmentIndex`
  (`:94`), `ActiveSegment` (`:97-98`), `ActiveSegmentKey` (`:101-105`), `SetSegmentKey(int,
  FolderTreeNodeKey)` (`:131-144`), `ActivateSegment(int)` (`:151-172`), `GetActiveChild(int)` (`:175-188`).
  The private `CanExpandLeaf()` was RENAMED `CanExpandActiveSegment()` (`:356-359`); `SetLeafChildren`
  (`:253-267`), `ToggleLeafExpanded` (`:274-283`), and `RightArrow` (`:320-339`) now gate on the ACTIVE
  segment, not the leaf.
- **Builder join key.** `BreadcrumbRowBuilder` now joins probability on `presentedText`
  (`BreadcrumbRowBuilder.cs:133`), previously `segments[last].FullPath`, and passes `presentedText` as the new
  `filingTarget` constructor argument (`:141`).
- **Selection derivation.** `BreadcrumbBridgeRouter.SelectRow` now derives `SelectedFolderPath` from
  `row.FilingTarget` (`:484-487`), previously `row.LeafSegment?.FullPath`. `LeafSegment` is no longer
  referenced by the router at all.
- **Cancellation.** `FetchChainAsync` no longer rethrows `OperationCanceledException`; it logs at `Error` and
  returns null so binding renders the fallback (`:461-467`).
- **Regression coverage.** `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` (531 lines,
  7 `[TestMethod]`s), registered at `QuickFiler.Test.csproj:60`.

### Research sections superseded by the landed work

The research artifact
`research/2026-08-24T09-50-breadcrumb-router-navigation-defects.md` is a dated evidence artifact recording what
was true at HEAD `988e819b`. It is **not edited**. Where it disagrees with the current code, this section
supersedes it.

| Research section | Why it is superseded |
|---|---|
| **§Q3e** (canonical path form) | The Efc canonical form is now established at the router boundary via `ArchiveRootPath`, which is the mechanism §Q3e considered and rejected. The provider-side suffix fallback survives, but for a different purpose (see D5 as rewritten). |
| **§Q3d** and the **D6 hazards** | Both hazards are already fixed on `main`: the probability join moved to `presentedText` (`BreadcrumbRowBuilder.cs:133`) and the Efc filing target now derives from `row.FilingTarget` (`BreadcrumbBridgeRouter.cs:484-487`). |
| **§Q3f** (Efc separator glyph) | The `→` glyph is landed (`BreadcrumbHtmlRenderer.cs:149`). |
| **§Q4d** (no selected-node concept) | A selected-node concept now EXISTS: `ActiveSegmentIndex`, `ActivateSegment(int)`, `ActiveSegmentKey`, `GetActiveChild(int)` on `BreadcrumbRow`. §Q4d's "blocking mechanical detail" — that `BreadcrumbSegment` carries no key, so ancestor expansion must re-resolve by path with a `ResolveLeafKeyAsync` then `GetImmediateSubfoldersAsync` two-call pattern — is also gone: per-segment keys are attached at bind time by the router's `AttachSegmentKeys` (`BreadcrumbBridgeRouter.cs:165-189`) and `ExpandLeafAsync` uses `row.ActiveSegmentKey` directly with NO `ResolveLeafKeyAsync` call (`:375-385`). |
| **§Q4f** (#440 depends on #439) | Discharged for the Efc surface — Efc rows now carry resolved multi-segment chains. It remains live for the Qfc surface, whose chains still do not resolve. |
| **§Q6a** (unowned-file blockers) | Two of the three blockers are moot: `BreadcrumbDocumentAssets.cs` and `BreadcrumbHtmlRenderer.cs` were written by the #439 workstream and need no change here. The `BreadcrumbSelectionMap.cs` blocker (D7 / RISK-1) is unaffected and still live. |
| **§Q6c** (line counts) | Every count is stale. Use the current figures recorded in decision D8 below. Two files are now over the 500-line limit that were not before. |

### Constraint on the landed `#439` scope

The landed #439 spec's "Scope & Non-Goals" scopes that work to the **Efc surface only**. It explicitly excludes
`QuickFiler/Resources/FolderBreadcrumb.html` and the ItemViewer breadcrumb popup, and explicitly excludes
"Keyboard Left/Right navigation changes, including Issue #440 keyboard-navigation work". Every acceptance
criterion in that spec is checked except its final toolchain criterion. Nothing in it closes #440, #498, or
#499, and nothing in it addresses the Qfc surface.

### Inherited condition, not a work item

The landed fix REMOVED the `[ExcludeFromCodeCoverage]` attribute from `EfcFormController`, so that 1084-line
file is now in the coverage denominator. `EfcFormController.cs` is not owned by this feature and must not be
written by it. This is recorded as an inherited condition only; it creates no work item and no acceptance
criterion here.

## Repro & Evidence

- Steps to reproduce (with data/flags/inputs):
  - **#498** — open the EfcViewer folder list so `EfcFormController.ConfigureBreadcrumbControl`
    (`EfcFormController.cs:832-852`) has wired a `BreadcrumbBridgeRouter`; have the hosted document post
    `{"type":"segmentDoubleClick","rowId":"row-0","segmentIndex":99}` for a row with fewer than 100 segments;
    observe the host process.
  - **#499** — open the EfcViewer folder list, type search text, select a folder row (`SelectRow` sets both
    `_selectedRowId` and `SelectedFolderPath`, `BreadcrumbBridgeRouter.cs:483-487`), type one more character
    (reaching `BindFolderRows`), observe no row highlighted, then trigger a move or folder-open.
  - **#439** — no longer reproducible on the Efc surface (fixed by PR #605). The residual Qfc reproduction:
    open the ItemViewer folder selector so `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` binds
    suggestions; observe each row shows only a folder name with no ancestor chain.
  - **#440** — give the folder selector keyboard focus so a row is highlighted; press Left and observe the
    selected node does not become the parent; press Right and observe expansion applies to the leaf. Repeat
    in EfcViewer.

- Expected vs actual behavior:
  - **#498** — expected: the router's own XML doc comment (`BreadcrumbBridgeRouter.cs:224-228`) states that a
    malformed payload fails fast and leaves state unchanged. Actual: `ArgumentOutOfRangeException` is thrown
    at `BreadcrumbRow.cs:207-214`, is not caught by the `catch (BreadcrumbMessageException)` at
    `BreadcrumbBridgeRouter.cs:266-277`, and escapes the `async void` boundary at `:266`.
  - **#499** — expected: after a re-bind clears the visible selection, `SelectedFolder` agrees with the UI.
    Actual: only `_selectedRowId = null` is executed (`BreadcrumbBridgeRouter.cs:136`); `SelectedFolderPath`
    (`:59`) retains its prior value, and `DeliverDocument` (`:543-555`) renders with `_selectedRowId` at
    `:545`, so no row is highlighted.
  - **#439** — expected: rows render their resolved root-to-leaf lineage. On the Efc surface this is now the
    actual behavior: the router supplies a full hierarchy path via `ToHierarchyPath`
    (`BreadcrumbBridgeRouter.cs:140-163`) and the exact match in `ResolveLeafKeyAsync`
    (`OutlookFolderHierarchyProvider.cs:52-71`) succeeds. On the Qfc surface the defect persists:
    `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` still passes the archive-relative stem
    `row.Score.Value.FolderPath` (`FolderBreadcrumbBridgeRouter.cs:49`) to `ResolveLeafKeyAsync` (`:53`), the
    exact `OrdinalIgnoreCase` comparison at `OutlookFolderHierarchyProvider.cs:66-68` never matches, and
    `CreateFallbackRow` (`FolderBreadcrumbBridgeRouter.cs:245`, called at `:46` and `:109`) yields a
    one-segment row.
  - **#440** — expected: Left selects the parent node, Right expands the selected node into its children.
    Actual: `HandleArrowKeyAsync` (`BreadcrumbBridgeRouter.cs:304-339`) maps Right to `ReExpand()` or
    `ExpandLeafAsync`, Left to `row.LeftArrow()` (`BreadcrumbRow.cs:291-312`); the Qfc analogue is
    `FolderBreadcrumbBridgeRouter.ArrowAsync` (`:378-406`) over `BreadcrumbStateModel.RightArrow()`
    (`:424-437`) and `LeftArrow()` (`:443-455`). Every one of these transitions mutates view state only; the
    class doc says so explicitly at `BreadcrumbRow.cs:22-33`. Note that the landed `ActivateSegment`
    (`BreadcrumbRow.cs:151-172`) is NOT a view-state-only transition — it is the real selected-node concept
    #440 needs — but no ARROW-KEY path reaches it.

- Logs/screenshots/error snippets: `System.ArgumentOutOfRangeException` originating in
  `BreadcrumbRow.CollapseAfter` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:207-214`) for #498. The
  other two live defects are silent: #499 is a state divergence with no error text, and #440 is a missing
  capability.

- Frequency / determinism (always, intermittent, data-dependent):
  - #498 — deterministic and data-dependent (any `segmentIndex` outside `[0, segments.Count - 1]` on a
    suggestion row; banner and trash rows return `false` before the range check, `BreadcrumbRow.cs:202-205`).
  - #499 — deterministic on every re-bind that follows a selection.
  - #439 — resolved on Efc. On Qfc, deterministic; the path forms never match (research §Q3a, §Q3b).
  - #440 — deterministic. The landed active-segment concept is reachable only from the typed activation
    messages, never from an arrow key, on either surface.

## Scope & Non-Goals

### In scope

- #498 — a range guard in the `SegmentDoubleClick` arm of `ProcessInboundAsync`.
- #499 — clear `SelectedFolderPath` on re-bind and raise `SelectedFolderPathChanged(this, null)` when the
  value actually changed.
- #440 — Left/Right tree transitions on both surfaces, inserted ahead of the existing behavior.
- **Qfc ancestor-chain resolution, as the named PREREQUISITE for #440's Qfc half only.** Delivered through the
  owned `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` segment-boundary suffix fallback (decision D5)
  plus the decision D7 Qfc filing-target preservation ladder. This is not a re-implementation of #439; it is
  the minimum chain resolution the Qfc surface needs before a Left/Right tree transition can have a parent to
  move to.

### Out of scope / non-goals

- **#439 in its entirety.** Lineage resolution on the Efc surface, the Efc mouse gesture that single-clicks a
  non-leaf segment to navigate to that ancestor, and the `>` to `→` separator glyph change all landed on
  `main` under #439's own workstream (PR #605). Version 1.0 of this document descoped the latter two under
  decision D3; that descope is moot. None of the three requires work here. See "Post-#439 Reconciliation".
- **Efc ancestor-chain resolution.** Inherited from PR #605 and not re-implemented. The Efc router supplies a
  full hierarchy path that `ResolveLeafKeyAsync`'s exact first pass already resolves.
- Unifying the two surfaces' boundary behavior (decision D2).
- Restoring the prior selection on re-bind, and any `SelectFirstRow` auto-selection side effect (decision D4).
- Changing the presented-row path form in `FolderPredictor` (decision D5).
- Any repair of the pre-existing 500-line violations in `FolderPredictor.cs` (983 lines) and
  `EfcFormController.cs` (1084 lines) (decision D8).

### Explicitly excluded systems, integrations, or datasets

- Live Outlook / COM. All tests are Moq-based over `IFolderHierarchyProvider`, `IBreadcrumbWebHost`, and
  `IOutlookFolderTreeService`; `FolderNavigator.GetOutlookFolder` (`FolderNavigator.cs:10`) is a live COM path
  walk and is not used.
- The `#400` selector-session types. `BreadcrumbSelectionSession`
  (`BreadcrumbSelectionSession.cs:98-107`) is not modified (decision D1).
- The coverage-denominator change caused by PR #605 removing `[ExcludeFromCodeCoverage]` from
  `EfcFormController` (1084 lines). That file is unowned here; the condition is inherited, not addressed.

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
- `QuickFiler/Controllers/EfcFormController.cs` — feature 464. PR #605 also wrote this file; it is still not
  owned here.
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` — **sibling epic child 501 owns it.** This is the sole
  construction site of `FolderBreadcrumbBridgeRouter`, whose only constructor takes `IFolderHierarchyProvider`
  alone. Because this file cannot be written, the archive-root route that PR #605 used for the Efc surface is
  not available to the Qfc surface (see D5 as rewritten).
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` — written by the landed #439 workstream; still
  unowned here and unwritten by this feature.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` — written by the landed #439 workstream;
  still unowned here and unwritten by this feature.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` — written by the landed #439 workstream; still
  unowned here and unwritten by this feature.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs`
- `UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs`
- `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` — per decision D1, must pass unmodified.

Read-only access to any of the above is permitted and expected; writing is not.

## Scope Decisions (recorded verbatim)

These nine decisions were made by the orchestrator. D1 through D8 were made before planning at version 1.0;
D3, D5, D6, D7, and D8 were rewritten and D9 added at version 1.1 to reconcile with the landed #439 fix. They
are recorded here as the binding scope of the feature. They are not re-opened during planning or execution.

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

### D3 — RETRACTED. The #439 part A / part B / glyph split is moot; all three landed under #439

Version 1.0 split #439 into part A (lineage resolution, in scope), part B (the Efc mouse gesture, descoped),
and the separator glyph (descoped), on the reasoning that parts B and the glyph required writing two unowned
files.

**Decision at version 1.1: the split is RETRACTED in full.** All three parts landed on `main` under #439's own
workstream (PR #605, feature commit `c39db103`). None of them is in scope here, descoped or otherwise.

Landed mechanism, verified in the current worktree:

- **Part A** — `BreadcrumbBridgeRouter.BindRowsAsync` gained an `internal` overload taking
  `string archiveRootPath` (`:92-138`), a private `ToHierarchyPath` (`:140-163`) that prefixes the archive root
  onto the presented stem before `ResolveLeafKeyAsync`, and a caller in `EfcFormController.BindBreadcrumbRowsAsync`
  (`:884-900`) passing `_globals.Ol.ArchiveRootPath` at `:891`.
- **Part B** — inbound message types `segmentActivate` and `renderedChildActivate` (`BreadcrumbMessages.cs:16`,
  `:19`), per-type codec validation (`BreadcrumbMessageCodec.cs:105-118`), `data-segment-activate` and
  `data-child-index` emission (`BreadcrumbHtmlRenderer.cs:182-190`, `:221-227`), the click handlers in
  `BreadcrumbDocumentAssets.BridgeJs` (`:68-100`), and `ActivateSegment` / `ActivateChild` in the router
  (`BreadcrumbBridgeRouter.cs:410-427`, `:429-441`).
- **Glyph** — `BreadcrumbHtmlRenderer` emits `<span class="sep">→</span>` at `:149`.

Inherited regression coverage: `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` (531 lines,
7 `[TestMethod]`s, registered at `QuickFiler.Test.csproj:60`).

The two files D3 refused to write — `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` and
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` — were written by that workstream, NOT by this
feature. They remain unowned here and must not appear in this feature's diff (AC-30).

D3 no longer creates any follow-up potential entry, because there is no residual to follow up.

### D4 — #499 clears and raises

Adopt research §Q2c's recommendation without modification: clear `SelectedFolderPath` to `null` in
`BindRowsAsync` alongside `_selectedRowId`, and raise `SelectedFolderPathChanged(this, null)`, but only when
the value actually changed. Reject the restore-prior-selection option for the three reasons the research gives
(research §Q2c: it does not fix the keystroke path where the defect matters; restoration must match by
`row.LeafSegment?.FullPath`, the exact value #439 changes the form of; and it forces a second coherent change
to `DeliverDocument`/`_selectedRowId`). Do NOT add a `SelectFirstRow` side effect.

**Orchestrator-verified fact resolving the research's one open item on this defect (research §Q2c, open item
6):** `EfcFormController.IsValidSelection` (`QuickFiler/Controllers/EfcFormController.cs:1038-1050`) ALREADY
tolerates `null` — its first disjunct is `selectedFolder is null` (`:1044`), so it returns `false` for a null
selection. Both call sites guard on `!IsValidSelection` first (`:468`, `:752`). The #499 clear therefore CANNOT
introduce a `NullReferenceException`. (Re-verified against the current worktree contents at version 1.1; PR
#605 did not change `IsValidSelection` or either guard, though it shifted their line numbers.)

Note that the two `SelectedFolderPath` write sites in the router — `SelectRow` (`BreadcrumbBridgeRouter.cs:476-492`)
and the landed `SelectHierarchyPath` (`:494-502`, used by segment and child activation) — are BOTH left intact
by this decision. The #499 clear belongs in `BindRowsAsync`, not in either write site.

The cross-feature note to feature 464 is informational only, not a blocking dependency.

### D5 — Qfc ancestor-chain resolution, the prerequisite for #440's Qfc half

**The FIX SITE is unchanged from version 1.0 and is still correct.** It is
`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` (owned, 98 lines,
method at `:52-71`): keep the existing exact `OrdinalIgnoreCase` match as the first pass (`:66-68`), then fall
back to a segment-boundary suffix match accepted ONLY when unique; on zero or multiple candidates log at
`Error` via the existing `log4net` pattern and return `null`, preserving today's single-segment rendering.

**The PURPOSE has changed.** At version 1.0 this was "#439 part A on both surfaces". At version 1.1 it is
narrower: it is the **Qfc ancestor-chain prerequisite for #440's Qfc half**, and nothing more.

The rejection rationale recorded at version 1.0 — that the `BindRowsAsync` route was unavailable because the
router has no access to `ArchiveRootPath` without editing the unowned `EfcFormController.cs` — is obsolete for
the Efc surface, because PR #605 took exactly that route. The current reasoning is:

- **The Efc surface no longer needs this fallback.** `BreadcrumbBridgeRouter.ToHierarchyPath` (`:140-163`)
  supplies a full hierarchy path, so the exact `OrdinalIgnoreCase` first pass resolves and the fallback is
  never reached. Adding the fallback must therefore be a strict no-op for the Efc path.
- **The landed archive-root route is unavailable to Qfc.** `FolderBreadcrumbBridgeRouter`'s only constructor
  takes `IFolderHierarchyProvider` alone, and it is constructed in `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`,
  which sibling epic child 501 owns and which this feature must not write. There is no owned seam through
  which an archive root could reach the Qfc router.
- **Therefore the provider-side suffix fallback is the ONLY route available in owned files.** The Qfc router
  still passes the archive-relative stem `row.Score.Value.FolderPath` (`FolderBreadcrumbBridgeRouter.cs:49`) to
  `ResolveLeafKeyAsync` (`:53`), and the fallback is what makes that stem resolve.

`FolderPredictor` remains rejected as a fix site: it would change the filing contract, and the file is already
983 lines, over the 500 limit.

**Added requirement.** A test must prove the fallback does NOT alter the Efc path — specifically, that a caller
supplying a full Outlook path still resolves through the exact first pass and never reaches the fallback. This
is AC-7.

### D6 — RETRACTED. Both #439 regression hazards were fixed on `main`

Version 1.0 recorded two regression hazards that resolving the ancestor chain would create, and put both in
scope with their own acceptance criteria. Both are already fixed on `main` by PR #605, so neither is a hazard
of this feature.

| Hazard | Landed fix |
|---|---|
| **(a)** suggestion-row PERCENTAGE, which depended on the probability join key equalling the presented stem | `BreadcrumbRowBuilder.BuildRow` now joins probability on `presentedText` (`BreadcrumbRowBuilder.cs:133`), not on `segments[last].FullPath`. `BuildProbabilityIndex` (`:210-229`, key assignment at `:224`) stays keyed on the scorer's own `FolderPath`, so the two agree by construction. |
| **(b)** Efc FILING TARGET, which depended on `row.LeafSegment?.FullPath` being the stem | `BreadcrumbRow` gained an immutable `FilingTarget` (`:88`), set from `presentedText` by the builder (`:141`), and `BreadcrumbBridgeRouter.SelectRow` now derives `SelectedFolderPath` from `row.FilingTarget` (`:484-487`). `LeafSegment` is no longer referenced by the router. The `DestinationOlStem` contract at `EfcDataModel.cs:286`, `:307`, `:325` is unaffected. |

Both are covered by the landed test
`BreadcrumbBridgeRouterIssue439Tests.Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability`
(`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs:20-116`), which asserts `73%` still renders
(`:114`) and that `SelectedFolderPath` equals the presented stem (`:115`).

**No work item, no mitigation, and no acceptance criterion of this feature depends on D6.** AC-12 and AC-13 are
retired as inherited-and-verified. `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` remains unowned
here and is not written by this feature; the landed #439 workstream wrote it.

### D7 — The Qfc filing-target hazard is an explicit verification gate, not an assumption

**Re-scoped at version 1.1: this decision now applies to the Qfc surface ONLY, and it is a prerequisite of
#440's Qfc half rather than a consequence of #439.** The Efc analogue of this hazard was resolved on `main` by
the landed `BreadcrumbRow.FilingTarget` (see D6); nothing in D7 applies to the Efc surface any more.

Research §Q6a records the feature's single largest open risk as UNVERIFIED: once the Qfc chains resolve under
D5, `BreadcrumbSelectionMap.GetSelectedFolder` (`BreadcrumbSelectionMap.cs:109`) returns
`row.Chain[row.Chain.Count - 1].FolderPath` for a suggestion row, and the Qfc filing target flips from stem to
full path — and that file is NOT owned.

**Decision:** this is carried as a named RISK (see "Risks & Mitigations", RISK-1) with a three-way resolution
ladder, taken in order:

1. **PREFERRED** — preserve the presented stem through the owned
   `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` (`CreateFallbackRow` `:245`,
   `ReplaceRowsPreservingSession` `:474`), so `BreadcrumbSelectionMap.cs` needs no change.
2. **FALLBACK** — if rung 1 is not achievable in owned files, the Qfc router must not consume the
   newly-resolved chain for the filing-target path, explicitly preserving today's Qfc filing behavior, and the
   spec records that Qfc lineage display improves while Qfc filing is deliberately left on the old path form.
3. **HALT** — if neither is achievable, this is a BLOCKING cross-feature dependency on
   `BreadcrumbSelectionMap.cs` and execution stops and reports it rather than writing an unowned file.

The choice among 1/2/3 is made by READING `BreadcrumbStateRow` and `CreateFallbackRow` during execution. That
reading is read-only. The acceptance criterion for each rung is stated under RISK-1.

**In-repo precedent for rung 1's shape.** The landed `BreadcrumbRow.FilingTarget`
(`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:88`, assigned from `presentedText` at
`BreadcrumbRowBuilder.cs:141`) is exactly the shape rung 1 needs — an immutable per-row filing target held
independently of the resolved chain — but it exists on the **Efc** row type. Whether the Qfc `BreadcrumbStateRow`
can carry an equivalent WITHOUT writing the unowned `BreadcrumbSelectionMap.cs` remains the unverified question
that the rung-1 read must settle. The precedent shows the shape is workable; it does not show that it is
reachable from owned Qfc files.

### D8 — File-size: the `BreadcrumbBridgeRouter.cs` partial split is MANDATORY

**Current line counts, re-measured at version 1.1.** The research §Q6c figures are stale; these govern.

| File | Lines | Was (§Q6c) | Status |
|---|---|---|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | **596** | 450 | **OVER the 500 limit already.** Receives #498, #499, and the #440 Efc transitions. |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | **531** | (did not exist) | **OVER the 500 limit already.** Landed with PR #605; not owned, not written, not worsened here. |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 485 | 485 | Unchanged. 15 lines of headroom; split pre-authorized. |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 457 | 457 | Unchanged. 43 lines of headroom; split pre-authorized. |
| `QuickFiler/Resources/FolderBreadcrumb.html` | 489 | 489 | Unchanged. 11 lines of headroom; **cannot be split**. |
| `QuickFiler/Controllers/KeyboardHandler.cs` | 414 | 414 | Unchanged. No change expected. |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | **361** | 265 | Owned; 139 lines of headroom. |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | 98 | 98 | Owned; no risk. |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` | 238 | — | Unowned. |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | 234 | — | Unowned. |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` | 138 | — | Unowned. |
| `QuickFiler/Controllers/EfcFormController.cs` | 1084 | 1086 | Unowned. Pre-existing violation. |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 983 | 983 | Owned but not written (D5). Pre-existing violation. |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | 462 | — | 38 lines of headroom. |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs` | 120 | — | Unowned. |

**Decision:**

- **The partial-class split of `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` is MANDATORY, not
  contingent.** The file is already at 596 lines, 96 over the limit. The split must be performed BEFORE any
  #498, #499, or #440 addition to that file, and it must bring the file to at or under 500 lines INCLUDING
  this feature's additions.
- `QuickFiler/QuickFiler.csproj` uses explicit `<Compile Include>` items (the router entry is at `:290`), so
  the new partial requires a new entry there.
- Partial-class splits remain pre-authorized for `FolderBreadcrumbBridgeRouter.cs` and `BreadcrumbStateModel.cs`
  if needed. In-repo precedent exists (`FolderBreadcrumbBridgeRouter.SearchPresentation.cs`,
  `UtilitiesCS.csproj:629-630`).
- A new `Compile Include` should be placed adjacent to its nearest sibling entry rather than forced into strict
  alphabetical order — see RISK-5, which records that the item group is no longer strictly alphabetical.
- `FolderBreadcrumb.html` cannot be split; its 11 lines of headroom is a hard constraint on the #440 `onArrow`
  change.
- `FolderPredictor.cs` (983 lines), `EfcFormController.cs` (1084 lines), and the inherited
  `BreadcrumbBridgeRouterIssue439Tests.cs` (531 lines) are PRE-EXISTING 500-line violations that this feature
  neither inherits responsibility for nor worsens. `FolderPredictor.cs` should not be written at all under D5.

### D9 — #440's Efc half consumes the landed active-segment seams

Research §Q4d concluded that no selected-node concept exists on either surface and that "the lowest-cost shape
is a new index field on `BreadcrumbRow`, e.g. `SelectedSegmentIndex`". **That conclusion is SUPERSEDED.** The
concept now exists and must be consumed, not re-invented:

| Landed member | Location | Role for #440 |
|---|---|---|
| `ActiveSegmentIndex` (`int?`, private setter) | `BreadcrumbRow.cs:94` | The selected-node index research §Q4d proposed adding. |
| `ActiveSegment` | `:97-98` | The selected node itself. |
| `ActiveSegmentKey` | `:101-105` | The provider key of the selected node, attached at bind time. |
| `SetSegmentKey(int, FolderTreeNodeKey)` | `:131-144` | Per-segment key attachment, called by the router's `AttachSegmentKeys` (`BreadcrumbBridgeRouter.cs:165-189`). |
| `ActivateSegment(int)` | `:151-172` | The upward (toward-root) selection transition. |
| `GetActiveChild(int)` | `:175-188` | Validated child lookup after expansion. |

Research §Q4d's "Blocking mechanical detail" — that `BreadcrumbSegment` carries no key, so ancestor expansion
must re-resolve by path using a `ResolveLeafKeyAsync` then `GetImmediateSubfoldersAsync` two-call pattern — is
**also SUPERSEDED**. `ExpandLeafAsync` (`BreadcrumbBridgeRouter.cs:364-408`) now reads `row.ActiveSegmentKey`
directly (`:375`) and makes a SINGLE `GetImmediateSubfoldersAsync` call (`:384-385`). There is no
`ResolveLeafKeyAsync` call inside `ExpandLeafAsync` at all. The landed test
`BreadcrumbBridgeRouterQueueTests.LeafExpand_UsesBoundActiveSegmentKeyWithoutResolvingAgain` (`:377-410`) pins
this.

**Decision:** #440's Efc half must express its transitions through these members. It must not add a second
selected-node index, and it must not reintroduce a `ResolveLeafKeyAsync` call on the expansion path.

**Constraints imposed by `ActivateSegment`'s guard (`BreadcrumbRow.cs:153-166`), which the plan must respect:**

`ActivateSegment(int segmentIndex)` returns `false` — that is, refuses the transition — in every one of these
cases:

1. the row is not a `Suggestion` row;
2. `segmentIndex < 0`;
3. `segmentIndex >= _segments.Count - 1`, i.e. **the leaf index is refused**;
4. no key has been attached for that index (`!_segmentKeys.ContainsKey(segmentIndex)`);
5. the index is already the active one.

It also RESETS `_leafChildren` to empty and sets `IsLeafExpanded = false` on success (`:169-170`).

**Consequence the plan must resolve: `ActivateSegment` cannot express a downward transition.** Because case 3
refuses the leaf index, a Right transition that moves selection back DOWN toward the leaf cannot be routed
through `ActivateSegment`. The spec therefore requires that #440's Efc Right behavior state its descent
mechanism explicitly. Two mechanisms are available in owned files, and the plan must choose one and record the
choice:

- **Descend by child activation** — after Right has expanded the active segment's children
  (`ExpandLeafAsync`), a subsequent Right selects a child through the existing `GetActiveChild(int)` /
  `SelectHierarchyPath` path (`BreadcrumbBridgeRouter.cs:429-441`, `:494-502`), which is how the landed mouse
  gesture descends. This adds no member to `BreadcrumbRow`.
- **Descend by an added owned transition on `BreadcrumbRow`** — a new method alongside `ActivateSegment` that
  moves `ActiveSegmentIndex` toward the leaf. `BreadcrumbRow.cs` is owned (361 lines, ample headroom), so this
  is permissible; it must not weaken `ActivateSegment`'s existing guard.

Either choice must preserve the D1 handling order and the D2 boundary behavior.

## Root Cause Analysis

- Current hypothesis or confirmed root cause: **every root cause below is confirmed by code read in the
  current worktree**, not hypothesised. #439's root cause is retained for the record and marked resolved.

  1. **#498** — the codec validates the *presence* and JSON-integer *type* of `segmentIndex` and nothing else
     (`BreadcrumbMessageCodec.cs:101`, `:105-113`, `:156-172`). The router then dereferences it with the
     null-forgiving operator and calls a member that throws on range
     (`BreadcrumbBridgeRouter.cs:241-247`, dereference at `:242`; throw at `BreadcrumbRow.cs:207-214`). The
     host-event handler is `async void` (`BreadcrumbBridgeRouter.cs:266`, subscribed at `:55` against
     `IBreadcrumbWebHost.MessageReceived`, `QuickFiler/Viewers/IBreadcrumbWebHost.cs:22`) and catches only
     `BreadcrumbMessageException` (`:266-277`). Research §Q1b enumerated all four inbound codec fields at HEAD
     `988e819b` and confirmed `segmentIndex` was the only presence-only value reaching a throwing member.
     **Re-checked at version 1.1 against the codec's now-five optional/required fields:** the landed
     `segmentActivate` and `renderedChildActivate` types also carry presence-only integers, but their router
     arms call `row.ActivateSegment` (`:249`) and `row.GetActiveChild` (`:252`), both of which validate
     internally and return `false` / `null` rather than throwing (`BreadcrumbRow.cs:151-172`, `:175-188`).
     `segmentDoubleClick` remains the only inbound arm that reaches a throwing member.
  2. **#499** — `BindRowsAsync` clears `_selectedRowId` at `BreadcrumbBridgeRouter.cs:136` but not
     `SelectedFolderPath` (`:59`), which now has two assignment sites: `SelectRow` (`:484-487`) and the landed
     `SelectHierarchyPath` (`:497`). `DeliverDocument` renders from `_selectedRowId` (`:545`).
  3. **#439 — RESOLVED on the Efc surface; the Qfc half is the residual.** The root cause was a path-form
     mismatch. Presented text is an archive-root-relative stem (`FolderPredictor.LoopFolders` `:883-931`, stem
     assignment `:898`, add at `:919`; `GetOlSubpath` `:933-951`, substring at `:943`), while
     `ResolveLeafKeyAsync` compares with exact `OrdinalIgnoreCase` equality against `node.FolderPath`, the raw
     Outlook `MAPIFolder.FolderPath` (`OutlookFolderHierarchyProvider.cs:52-71`, comparison at `:66-68`;
     capture at `OutlookFolderHierarchyReader.cs:143`). Research §Q3b adds the decisive correction:
     `FolderTreeSnapshotNode.RelativePath` (`FolderTreeSnapshotNode.cs:53`, computed at
     `OutlookFolderHierarchyReader.cs:206-211`) is **store**-relative while the presented stem is
     **archive**-relative, so a naive "also compare `RelativePath`" fix would not work. PR #605 closed this on
     the Efc surface by prefixing the archive root at the router boundary. The Qfc surface is unchanged and
     still exhibits the mismatch (`FolderBreadcrumbBridgeRouter.cs:49`, `:53`), which is why D5 remains in
     scope as the #440 Qfc prerequisite.
  4. **#440** — no ARROW-KEY path reaches a tree transition on either surface. The version 1.0 statement that
     "no selected-node concept exists on either surface" (research §Q4d) is superseded for the Efc surface:
     `BreadcrumbRow.ActiveSegmentIndex` / `ActivateSegment(int)` / `ActiveSegmentKey` / `GetActiveChild(int)`
     (`BreadcrumbRow.cs:94`, `:151-172`, `:101-105`, `:175-188`) are exactly that concept, and they landed with
     PR #605. What is missing is any routing from an arrow key to them: `HandleArrowKeyAsync`
     (`BreadcrumbBridgeRouter.cs:304-339`) still maps Right to `ReExpand` / `ExpandLeafAsync` and Left to
     `row.LeftArrow()`, both of which mutate display state only (`BreadcrumbRow.cs:22-33`, `:291-312`). On the
     Qfc surface no such concept exists at all: `BreadcrumbSelectionSession.cs:98-107` is row-level,
     `BreadcrumbSelectionMap.cs:15-51` is a static projection, and `FolderTreeSelectionOverlay.cs:12-37`
     belongs to the folder-filter surface.

- Signals/evidence supporting it: the `file:line` citations above, all re-verified against the current
  worktree at version 1.1. Research §0 corrects four of the potentials' citations (two wrong file paths for
  #498, off-by-a-few lines in `FolderPredictor`, a wrong method span for `FetchChainAsync`) and one surface
  attribution (`FolderBreadcrumb.html` is the Qfc document, not the Efc one). Those corrections still stand;
  only the line numbers within `BreadcrumbBridgeRouter.cs`, `BreadcrumbRow.cs`, `BreadcrumbRowBuilder.cs`,
  `BreadcrumbHtmlRenderer.cs`, `BreadcrumbDocumentAssets.cs`, `BreadcrumbMessageCodec.cs`, and
  `EfcFormController.cs` have moved.

- Affected components/modules (paths, services, pipelines):
  `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`, `QuickFiler/Controllers/KeyboardHandler.cs`,
  `QuickFiler/Resources/FolderBreadcrumb.html`,
  `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`,
  `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`,
  `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`,
  `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`.

## Proposed Fix

### Design summary (what changes where):

Three targeted defect fixes plus one named prerequisite, delivered in the sequence below.

**Sequencing: #498 → #499 → Qfc chain-resolution prerequisite (D5 + D7) → #440.**

0. **The mandatory `BreadcrumbBridgeRouter.cs` partial split first (D8).** The file is at 596 lines, already
   over the 500-line limit, and steps 1, 2, and 4 all add to it. The split must precede any other change to
   that file.
1. **#498 first.** Smallest, fully self-contained in `BreadcrumbBridgeRouter.cs:241-247`, no interaction with
   any other defect, and the highest severity (host-process termination). Landing it first also establishes
   the `_host.Raise` regression-test pattern the rest of the plan reuses
   (`BreadcrumbBridgeRouterQueueTests.cs:194-205`).
2. **#499 second.** Also confined to `BreadcrumbBridgeRouter.cs` (`:136`, `:476-492`), also independent, and it
   is sequenced before the #440 work so that the two changes touching selection state stay separable and
   separately bisectable. Note that the pre-#439 reason for this ordering — that #439 would change how
   `SelectedFolderPath` is derived — no longer applies; the derivation change already landed.
3. **The Qfc chain-resolution prerequisite third** (D5 provider suffix fallback plus the D7 filing-target
   ladder). This is not a defect fix in its own right; it exists so that #440's Qfc half has a resolved
   multi-segment chain to navigate. It is the largest change of the four steps and carries RISK-1.
4. **#440 last.** Its Efc half consumes the landed active-segment seams (D9) and could in principle be
   sequenced earlier, but it is kept last so that a single ordering serves both surfaces. Its Qfc half
   genuinely depends on step 3: research §Q4f verifies that on an unresolved one-segment row both transitions
   are permanent no-ops, because `LeftArrow()` returns `false` immediately once the terminal index is 0
   (`BreadcrumbRow.cs:304-308`) and the fallback segment is constructed with `hasSubfolders: false`
   (`BreadcrumbRowBuilder.cs:129`) so the expansion guard is always `false`. **Citation correction:** the guard
   research §Q4f named, `CanExpandLeaf()`, NO LONGER EXISTS — PR #605 renamed it `CanExpandActiveSegment()`
   and it now sits at `BreadcrumbRow.cs:356-359`. #440 is also the only defect that retracts part of a landed
   acceptance criterion (D1).

Per the CLAUDE.md Bugfix Workflow, **each** of the three defects requires a failing regression test FIRST, then
the minimal targeted fix, then verification. The Qfc prerequisite carries the same RED-first obligation
(AC-27). The regression-test homes are named in "Test Strategy" below.

### Boundaries and invariants to preserve:

- `BreadcrumbRow.CollapseAfter`'s throw is a documented contract (`BreadcrumbRow.cs:197-199`, throw at
  `:207-214`) with existing test coverage, and `BreadcrumbRow.cs` is shared by both surfaces. It is NOT changed
  to return `false` (research §Q1c).
- The `catch (BreadcrumbMessageException)` at `BreadcrumbBridgeRouter.cs:266-277` stays narrow. A broad
  `catch (Exception)` at the async-void boundary would absorb unexpected exception classes from the whole
  `ProcessInboundAsync` tree, which is the "broad-catch without added context" pattern the General Code Change
  Policy prohibits. (Research §Q1c notes a belt-and-braces variant; it is a separate decision and is not
  adopted here.)
- The presented-row text stays an archive-root-relative stem. It is contractually a stem on the filing side
  (`EfcDataModel.cs:286`, `:307`, `:325`; `EfcFormController.cs:491-492`, `:770-771`), so `FolderPredictor` is
  not changed (D5).
- The landed `BreadcrumbRow.FilingTarget` (`:88`) and the landed `SelectRow` derivation from it
  (`BreadcrumbBridgeRouter.cs:484-487`) are PRESERVED unchanged. The #499 clear operates on `BindRowsAsync`
  only and leaves both `SelectedFolderPath` write sites (`SelectRow` `:484-487`, `SelectHierarchyPath` `:497`)
  intact.
- The landed `ToHierarchyPath` archive-root prefixing (`BreadcrumbBridgeRouter.cs:140-163`) and
  `AttachSegmentKeys` (`:165-189`) are PRESERVED unchanged. The D5 provider fallback must not alter the Efc
  resolution path, which already succeeds on the exact first pass (AC-7).
- `BreadcrumbRowBuilder.cs` is not written. Its "derives no hierarchy from row text" contract and its
  last-write-wins `BuildProbabilityIndex` (`:219-226`) are untouched.
- The `arrowKey` / `unhandledArrow` message shapes are unchanged (D1). The landed `segmentActivate` /
  `renderedChildActivate` shapes are likewise unchanged.
- `BreadcrumbSelectionSession` is not written (D1).
- Per-surface boundary behavior is unchanged (D2).
- `IFolderHierarchyProvider` gains no member. `GetImmediateSubfoldersAsync`
  (`IFolderHierarchyProvider.cs:46-49`) plus `ResolveLeafKeyAsync` are sufficient, and on the Efc surface
  expansion now needs only the former because the key is already attached (D9).
- `ActivateSegment`'s guard (`BreadcrumbRow.cs:153-166`) is not weakened. In particular the leaf-index refusal
  at `:156` stays.
- #400 AC-5 through AC-8 (Up/Down/Enter/Escape) are preserved (research §Q4c).

### Dependencies or blocked work:

- **Intra-feature:** #440's **Qfc** half depends on the D5 chain-resolution prerequisite (research §Q4f).
  #440's **Efc** half has no such dependency: PR #605 already produces multi-segment Efc rows and supplies the
  active-segment seams #440 needs (D9). Everything else in the feature depends only on the D8 partial split
  landing first.
- **Inherited, already satisfied:** #440's Efc half formerly depended on #439 part A. That dependency is
  discharged by PR #605.
- **Cross-feature (informational, non-blocking):** feature 464 (`EfcFormController.cs`) must be told that after
  #499, `SelectedFolder` can return `null` immediately after a re-bind. Per D4 this is already tolerated by
  `IsValidSelection` (`EfcFormController.cs:1038-1050`, `:1044`) and both call sites guard on it (`:468`,
  `:752`), so no change to feature 464 is required.
- **Cross-feature (potentially blocking, gated by D7):** `BreadcrumbSelectionMap.cs:109` — see RISK-1. Only
  rung 3 of the D7 ladder makes this blocking.
- **Cross-feature (hard constraint):** `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` is owned by sibling
  epic child 501 and must not be written. This is what forecloses the archive-root route on the Qfc surface and
  makes D5 the only available fix site (D5, AC-30).
- **Cross-feature notes formerly recorded under D3 are withdrawn.** Both descoped items landed under #439's own
  workstream, so neither requires a follow-up potential entry and this feature creates none.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

| File | Change | Line budget (current, version 1.1) |
|---|---|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | **Mandatory D8 partial split FIRST**, then: #498 guard (`:241-247`), #499 clear + event (`:136`), #440 Efc transitions (`HandleArrowKeyAsync` `:304-339`) | **596/500 — ALREADY IN BREACH.** Split is mandatory and must bring the file to <= 500 including additions (D8). |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | D5 unique segment-boundary suffix fallback in `ResolveLeafKeyAsync` (`:52-71`, exact pass at `:66-68`) plus `Error` logging on miss/ambiguity | 98/500 — no risk |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | #440 Efc transitions expressed through the LANDED `ActivateSegment` / `ActiveSegmentIndex` seams (D9); a descent transition only if the plan chooses that option over child activation | 361/500 — low risk |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | #440 Qfc transitions (`RightArrow` `:424-437`, `LeftArrow` `:443-455`) | 457/500 — HIGH risk; partial split pre-authorized |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | #440 Qfc routing (`ArrowAsync` `:378-406`); D7 rung 1 stem preservation in `CreateFallbackRow` (`:245`) / `ReplaceRowsPreservingSession` (`:474`) | 485/500 — VERY HIGH risk; partial split pre-authorized |
| `QuickFiler/Resources/FolderBreadcrumb.html` | #440 `onArrow` gating (`:395-404`), message shapes unchanged | 489/500 — HIGH risk; **cannot be split**, 11 lines of headroom is a hard constraint |
| `QuickFiler/Controllers/KeyboardHandler.cs` | No change expected. Owned so that the D2 fall-through (`:288-315`) can be verified unchanged. | 414/500 — low risk |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | **No change** (D5). Listed as owned only; writing it is rejected. | 983 — pre-existing violation |

#### Functions/classes/CLI commands impacted:

- `BreadcrumbBridgeRouter.ProcessInboundAsync` (`:229-264`) — `SegmentDoubleClick` arm (`:241-247`) only. The
  landed `SegmentActivate` (`:248-250`) and `RenderedChildActivate` (`:251-253`) arms are not changed.
- `BreadcrumbBridgeRouter.BindRowsAsync` (`:92-138`) — the #499 selection clear at `:136` only. The landed
  `ToHierarchyPath` call (`:119`) and `AttachSegmentKeys` call (`:135`) are not changed.
- `BreadcrumbBridgeRouter.HandleArrowKeyAsync` (`:304-339`) — new transitions ahead of existing behavior.
- `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` (`:52-71`).
- `BreadcrumbRow` — the landed `ActivateSegment` (`:151-172`), `ActiveSegmentIndex` (`:94`), `ActiveSegmentKey`
  (`:101-105`), and `GetActiveChild` (`:175-188`) are CONSUMED, not re-created (D9). `LeftArrow()` (`:291-312`)
  and `RightArrow()` (`:320-339`, unused by the Efc router) are retained as the second-priority behavior.
- `BreadcrumbStateModel.RightArrow` (`:424-437`) / `LeftArrow` (`:443-455`);
  `FolderBreadcrumbBridgeRouter.ArrowAsync` (`:378-406`).
- `FolderBreadcrumb.html` `onArrow` (`:395-404`) and its keydown wiring (`:420-426`).
- **Not changed:** `BreadcrumbBridgeRouter.SelectRow` (`:476-492`) and `SelectHierarchyPath` (`:494-502`). The
  version 1.0 plan changed `SelectRow`'s `SelectedFolderPath` derivation; PR #605 already did so.
- No CLI commands.

#### Data flow and validation changes:

- **#498** — inbound `segmentIndex` is range-checked against the row's segment count in the router before
  `CollapseAfter` is called. An out-of-range value produces no transition and no render post.
- **#499** — `BindRowsAsync` writes `SelectedFolderPath = null` and raises `SelectedFolderPathChanged(this,
  null)` only when the previous value was non-null. Both existing write sites are untouched.
- **Qfc chain-resolution prerequisite (D5)** — `ResolveLeafKeyAsync` gains a second pass: exact
  `OrdinalIgnoreCase` equality first (identity case, zero behavior change for a caller that already supplies a
  full path — which is now every Efc caller), then a segment-boundary suffix match
  `node.FolderPath.EndsWith("\\" + folderPath, OrdinalIgnoreCase)`, accepted **only when unique**. Uniqueness
  is the safety property: it prevents `Projects\Alpha` from binding to `\\store\Inbox\Projects\Alpha` when
  `\\store\Archive\Projects\Alpha` also exists. The archive-root value is not needed by this rule, which is why
  it works despite `ArchiveRootPath` being unreachable from any owned Qfc site (research §Q3e; D5 as
  rewritten).
- **Not implemented here (landed on `main`):** the `rowId -> presentedText` map and the `FolderScore` aliasing
  that version 1.0 planned for `BindRowsAsync`. PR #605 solved the same two problems differently — an immutable
  `BreadcrumbRow.FilingTarget` (`:88`) carried from `presentedText` (`BreadcrumbRowBuilder.cs:141`), and a
  probability join moved onto `presentedText` (`:133`). The positional correspondence version 1.0 relied on
  (`BuildRows` assigns `row-{i}` over the presented sequence, `BreadcrumbRowBuilder.cs:53-57`) is still exact
  and is what the landed `AttachSegmentKeys` (`BreadcrumbBridgeRouter.cs:165-189`) uses, but no map or alias is
  built.
- **#440 Efc expansion** — **the version 1.0 statement here is SUPERSEDED and must not be implemented.** It
  claimed that expanding an ancestor segment requires re-resolving by path, because `MapSegments`
  (`BreadcrumbRowBuilder.cs:178-208`) drops `FolderBreadcrumbSegment.Key` and `BreadcrumbSegment` carries only
  `FullPath`, `DisplayName`, `HasSubfolders` (`BreadcrumbSegment.cs:29-43`). That is still true of
  `BreadcrumbSegment` itself, but `BreadcrumbRow` now stores per-segment keys separately
  (`BreadcrumbRow.cs:38-39`, populated via `SetSegmentKey` `:131-144`) and exposes the active one as
  `ActiveSegmentKey` (`:101-105`). The `ResolveLeafKeyAsync` call that version 1.0 cited inside
  `ExpandLeafAsync` NO LONGER EXISTS; `ExpandLeafAsync` (`BreadcrumbBridgeRouter.cs:364-408`) reads
  `row.ActiveSegmentKey` at `:375` and makes a single `GetImmediateSubfoldersAsync` call at `:384-385`. #440
  must use that single-call path.
- **#440 Qfc expansion** — unchanged from version 1.0: `BreadcrumbStateRow.Chain` holds
  `FolderBreadcrumbSegment`, which does carry `Key` (used at `FolderBreadcrumbBridgeRouter.cs:416`).

#### Error handling and logging updates:

- **#498** — the rejected index is logged at `Error` using the existing `log4net` pattern in the same file
  (`BreadcrumbBridgeRouter.cs:235`, `:336`, `:414-416`). No exception is thrown and no exception escapes.
- **Qfc chain-resolution prerequisite (D5)** — when both passes miss, or when the suffix fallback is
  ambiguous, `ResolveLeafKeyAsync` logs at `Error` and returns `null`, so a systematic resolution failure is
  visible rather than presenting as a cosmetic omission (research §Q3e, matching the pattern at
  `BreadcrumbBridgeRouter.cs:235`, `:336`, `:378-380`).
- **#499** — no logging change. The clear is a normal state transition, not an error.
- **#440** — an unmapped key continues to hit the existing `default:` branch and its `log.Error`
  (`BreadcrumbBridgeRouter.cs:335-337`).
- The existing broad `catch (Exception ex)` in `ExpandLeafAsync` (`:400-407`) is retained unchanged; it already
  contains the provider path. The landed `catch (OperationCanceledException)` immediately above it
  (`:394-399`) is likewise retained.

#### Rollback/feature-flag considerations (if applicable):

No feature flag. The mandatory D8 partial split and each of the three defect fixes plus the Qfc prerequisite
are independent commits in the stated sequence, so rollback is a revert of the corresponding commit. The split
commit must be pure mechanical relocation with no behavior change, so that reverting a later fix does not
require reverting it.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- **Inbound bridge messages** are unchanged in shape. The codec now produces five inbound fields
  (`BreadcrumbMessageCodec.cs:94-125`): `type` (required, enum-checked at `:94-98`, `:128-136`), `rowId`
  (required string, `:100`, `:138-154`; unknown id is already a logged no-op at
  `BreadcrumbBridgeRouter.cs:232-237`), `segmentIndex` (`OptionalInt`, `:101`, `:156-172`), `childIndex`
  (`OptionalInt`, `:102`, landed with PR #605), `key` (non-empty for `arrowKey` only, `:103`, `:120-123`). The
  per-type required-field checks are at `:105-118`. No field, type, or check is added or removed by this
  feature.
- **Outbound message shapes** are unchanged, including `arrowKey` and `unhandledArrow` (D1).
- `ResolveLeafKeyAsync` signature is unchanged: `Task<FolderTreeNodeKey?> ResolveLeafKeyAsync(string
  folderPath, CancellationToken cancellationToken)`. Return type is already nullable.
- `IFolderHierarchyProvider` gains no member (`IFolderHierarchyProvider.cs:46-49` is reused as-is).
- `SelectedFolderPath` remains `string?` (`BreadcrumbBridgeRouter.cs:59`) and `SelectedFolderPathChanged`
  remains `EventHandler<string?>?` (`:62`).
- `BreadcrumbBridgeRouter.BindRowsAsync` retains BOTH overloads: the `public` three-argument form (`:75-82`)
  and the `internal` four-argument form taking `archiveRootPath` (`:92-138`). Neither signature changes.
- `BreadcrumbRow`'s landed public surface — `FilingTarget` (`:88`), `ActiveSegmentIndex` (`:94`),
  `ActiveSegment` (`:97-98`), `ActiveSegmentKey` (`:101-105`), `SetSegmentKey` (`:131-144`), `ActivateSegment`
  (`:151-172`), `GetActiveChild` (`:175-188`) — is preserved. Any #440 addition is additive.

#### Required configuration keys and defaults:

None. No configuration key is added, read, or changed.

#### Backward-compatibility expectations:

- **Observable behavior change, #499:** after any `BindRowsAsync` re-bind that follows a selection,
  `EfcFormController.SelectedFolder` returns `null` instead of the previous folder, until the user re-selects.
  A move or folder-open triggered in that window acts on a null selection rather than a stale folder. Both call
  sites guard on `!IsValidSelection` first (`EfcFormController.cs:468`, `:752`), and `IsValidSelection`'s first
  disjunct is `selectedFolder is null` (`:1038-1050`, disjunct at `:1044`), so the guard rejects it (D4).
- **Observable behavior change, Qfc lineage:** Qfc rows that currently render leaf-only will render their full
  lineage once D5's fallback resolves their chain. The single-segment fallback remains for rows whose chain
  genuinely cannot be resolved. The Efc equivalent of this change already shipped with PR #605.
- **Observable behavior change, #440:** Left and Right perform tree navigation where a transition is available.
  Where none is available, prior behavior is unchanged (D1 handling order, D2 boundaries).
- **Preserved contracts:** the Qfc filing target (D7), the `arrowKey`/`unhandledArrow` message shapes (D1), the
  Qfc `unhandledArrow` fall-through including the Pop Out / Enumerate Conversation entry point (D2), and #400
  AC-5 through AC-8. The suggestion-row percentage and the Efc filing target are preserved by the landed #439
  work and are not this feature's obligation (D6).
- **Nullable analysis** (research §Q6b): all owned `.cs` files except `KeyboardHandler.cs` carry
  `#nullable enable` at line 1, so `CS86xx` diagnostics are promoted to errors under
  `/p:TreatWarningsAsErrors=true`. Removing the `!` at `BreadcrumbBridgeRouter.cs:242` in favour of a
  `HasValue` check is safer under nullable analysis, not riskier. Do **not** add `/p:Nullable=enable` to the
  msbuild command; the CI command is the authority and this repository has no `Directory.Build.props`.

#### Performance constraints (latency/throughput/memory):

- `ResolveLeafKeyAsync` currently performs one `FirstOrDefault` scan over `snapshot.NodesByKey.Values`
  (`OutlookFolderHierarchyProvider.cs:66-68`). The fallback adds at most one further scan, executed only when
  the first pass misses. The uniqueness requirement means the second pass must enumerate rather than
  short-circuit. This runs against an in-memory snapshot, not against COM, so the cost is bounded by node
  count. On the Efc path the first pass now succeeds, so the second scan is never executed there.
- `BindRowsAsync` gains nothing. The version 1.0 plan added a `rowId -> presentedText` map and a score-alias
  sequence; PR #605 made both unnecessary.
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
    root at `:202-210`), i.e. `<storeRootFolderPath>\Archive`. The D5 fallback does not read this value. The
    landed Efc path does read it, at `EfcFormController.cs:891`, but that call site is unowned and unchanged.
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
    a hard constraint and a `.csproj` entry must be placed adjacent to its nearest sibling (RISK-5).
  - `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` (sibling child 501) is the sole construction site of
    `FolderBreadcrumbBridgeRouter`, and that router's only constructor takes `IFolderHierarchyProvider` alone.
    No archive-root value can be injected into the Qfc router from an owned file.
- External dependencies (services, libraries, releases): MSTest, Moq, FluentAssertions, log4net. No new
  dependency is added.

## Data / API / Config Impact

- User-facing or API changes:
  - Qfc folder rows render their full ancestor lineage instead of a leaf-only segment (D5 prerequisite). The
    Efc equivalent already shipped with PR #605 and is not a change made here.
  - Left and Right perform tree navigation where a transition is available (#440), with prior behavior retained
    as the second and third priorities (D1) and with per-surface boundary behavior unchanged (D2).
  - `EfcFormController.SelectedFolder` returns `null` after a re-bind until the user re-selects (#499).
  - An out-of-range `segmentIndex` is a logged no-op instead of a host-process crash (#498).
  - No public interface member is added or removed. `IFolderHierarchyProvider` is unchanged. `BreadcrumbRow`
    may gain an additive descent transition under D9; no landed member is removed or narrowed.
- Data or migration considerations: none. No persisted data, schema, or stored setting is read or written.
- Logging/telemetry updates (if any): two new `Error`-level log sites, both using the existing `log4net`
  pattern — the rejected out-of-range `segmentIndex` (#498) and the unresolved or ambiguous leaf key (D5).
- Compatibility notes (CLI flags, config schemas, versioning): none. No CLI flag, config schema, or version
  identifier is affected.

## Test Strategy

Framework: **MSTest** (`[TestClass]` / `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`).
Mocking: **Moq**. Assertions: **FluentAssertions**. No live Outlook or COM dependency; no temporary files.

- Regression tests to add or update (one failing test FIRST per defect, per the CLAUDE.md Bugfix Workflow;
  homes from research §Q5a):

  | Defect | Regression-test file | Seam |
  |---|---|---|
  | **#498** | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` (462 lines) | The only file with the async-void-boundary seam `_host.Raise(h => h.MessageReceived += null, _host.Object, "<json>")` (`:201`, pattern at `:194-205`). `Setup()` at `:34-74` plus the `Segment` / `Bind` helpers at `:76-96` produces a two-segment `row-0` via `Bind()`, so `segmentIndex: 99` and `segmentIndex: -1` are both out of range and `segmentIndex: 0` is the valid control. RED assertion: `Action act = () => _host.Raise(...); act.Should().NotThrow();` — deterministic because Moq's `Raise` is synchronous and every awaited task is already completed. "State unchanged" asserts `_posted.Count.Should().Be(postedBefore)`, the idiom already used at `:140`/`:146`, `:164`/`:170`, `:314`/`:320`, `:391`/`:397`, `:426`/`:432`. |
  | **#499** | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | Same `Setup()`; `Bind()`, then `Inbound("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}")` (idiom at `:111`, `:189`, `:448`), then a second `Bind()`, then `_router.SelectedFolderPath.Should().BeNull()`. **Citation correction:** the "double-`Bind` pattern at `:428-444`" that version 1.0 cited NO LONGER EXISTS in this file; the test that occupied that range is now `LeafExpand_OnLeafWithoutSubfolders_IsNoOpWithoutProviderQuery` (`:412-441`), which binds once. The #499 test must introduce the second `Bind()` itself; the `Bind()` helper (`:86-96`) is re-entrant. Event assertion reuses `BreadcrumbBridgeRouterTests.cs:219`: `string observed = "sentinel"; _router.SelectedFolderPathChanged += (s, path) => observed = path;`. Existing test `MalformedInboundJson_ThrowsCodecExceptionWithoutCorruptingState` (`:175-191`) must be read and confirmed still passing; it is unaffected because nothing was selected before its bind. |
  | **D5 — provider resolution** | `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` (282 lines) | Owns `ResolveLeafKeyAsync` coverage (`:100-192`). Real provider over a mocked `IOutlookFolderTreeService` (`ServiceReturning`, `:231-280`). **Caution (research §Q5c):** the existing `Node` helper passes `displayName` as the `relativePath` argument (`:275`), which is not a realistic relative path. The D5 test must construct nodes with a realistic full path (`\\store\Archive\Projects\Alpha`) and must include a **decoy** node (`\\store\Inbox\Projects\Alpha`) to pin the uniqueness requirement. It must also assert the Efc no-regression case of AC-7: a full-path caller resolves through the exact first pass. |
  | **Efc bind/join/selection** | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` (435 lines) **and** `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` (531 lines) | The first owns the bind-to-document assertions including the `"90%"` join (`:126-136`) and `SelectedFolderPath` (`:214-227`); its `SetupProviderChain` mock (`:77-106`) returns `Key(path)` for **any** input. The second is the LANDED #439 regression file and already covers lineage, filing target, and probability preservation; **it is read-only for this feature** and must be confirmed still passing, not extended. |
  | **D5 — Qfc bind** | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` (314 lines) | Owns `SetSuggestionsAsync` → chain resolution (`PopulatedRouterAsync` `:72-85`). Its `ProviderMock` (`:51-70`) uses `MockBehavior.Strict` with per-path setups — the right pattern here, because resolving the wrong path form throws rather than silently succeeding, making the RED test fail for the intended reason. |
  | **#440 — Efc transitions** | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs` (379 lines) **and** `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | The state machine and its routing are separately covered today; that split is kept. The landed `ActivateSegment` / `GetActiveChild` behavior is already covered in the state-model file (it grew from 334 to 379 lines with PR #605); #440's tests are additive. |
  | **#440 — Qfc transitions** | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs` (320 lines) **and** `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` | `BreadcrumbStateModelSelectorTests.cs` / `BreadcrumbStateModelSequenceTests.cs` are the #400 selector-session and sequence files; per D1 the selector session is not touched, so they are used only for confirming no regression. |
  | **#440 — html contract** | `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` (405 lines) | **Not written.** Per D1, `LeftAndRightBreadcrumbMessages_RemainSupported` (`:359-367`) must pass unmodified. It asserts against the compiled resource string `QuickFiler.Properties.Resources.FolderBreadcrumb` (`:19`, wiring at `QuickFiler/Properties/Resources.Designer.cs:184`) with no browser, no WebView2, and no JS engine, so it is deterministic. |

  **No `.csproj` edit is required for any of these test files.** Re-verified at version 1.1: `QuickFiler.Test.csproj:58`
  (`BreadcrumbBridgeRouterQueueTests.cs`), `:59` (`BreadcrumbBridgeRouterTests.cs`), `:60`
  (`BreadcrumbBridgeRouterIssue439Tests.cs`, added by PR #605), `:96`
  (`FolderBreadcrumbAssetContractTests.cs`); `UtilitiesCS.Test.csproj:279` (`BreadcrumbRowStateTests.cs`),
  `:283` (`FolderBreadcrumbBridgeRouterTests.cs`), `:290` (`BreadcrumbStateModelTests.cs`), `:301`
  (`OutlookFolderHierarchyProviderTests.cs`). A `.csproj` edit is needed only if the plan adds a **new** file,
  in which case the entry goes adjacent to its nearest sibling (RISK-5, D8) and, per #400 AC-17
  (`spec.md:255`), every added test `.cs` must be explicitly included. The mandatory D8 partial split of
  `BreadcrumbBridgeRouter.cs` DOES require a new `<Compile Include>` entry in `QuickFiler/QuickFiler.csproj`
  next to the existing router entry at `:290`.

- Unit tests for the fixed behavior and boundaries (the template's "pytest" wording does not apply; this is a
  C# repository and the framework is MSTest):
  - #498 — out-of-range high, out-of-range negative, valid index control case, banner/trash row short-circuit.
  - #499 — clear after re-bind; event raised once with `null` when the value changed; event **not** raised when
    the value was already `null`; no auto-selection after re-bind; both `SelectedFolderPath` write sites still
    behave as they do today.
  - D5 — identity case (full path resolves exactly as today, and the Efc caller never reaches the fallback);
    relative stem resolves via unique suffix; ambiguous stem with a decoy node returns `null` and logs;
    unresolvable stem returns `null` and preserves the single-segment fallback; resolved chain yields multiple
    segments in root-to-leaf order on the Qfc surface.
  - #440 — Left from the active segment selects its parent through `BreadcrumbRow.ActivateSegment`; repeated
    Left walks to the root; Right on a selected parent requests and shows that parent's children via
    `GetImmediateSubfoldersAsync` using `row.ActiveSegmentKey`; the chosen descent mechanism (D9) moves
    selection back toward the leaf; handling-order priority (tree transition, then existing expand/collapse,
    then unhandled) asserted on both surfaces.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
  - `segmentIndex` of `-1`, `0`, `segments.Count - 1`, `segments.Count`, `99`.
  - Empty or whitespace `folderPath` into `ResolveLeafKeyAsync` (early `null` at
    `OutlookFolderHierarchyProvider.cs:57-60`) — unchanged behavior.
  - `ActivateSegment` called with the leaf index, with an index carrying no attached key, with the
    already-active index, and on a non-suggestion row — all four must remain refusals
    (`BreadcrumbRow.cs:153-166`), and #440's Left path must handle a refusal by falling through to the
    existing behavior per D1.
  - Left at the root and Right on a childless node, per surface (D2).
  - Re-bind with no prior selection; re-bind with a prior selection that survives the new row set.
- Error handling and logging verification: assert the `Error` log site is reached for the rejected
  `segmentIndex` and for the unresolved/ambiguous leaf key, and that neither path throws.
- Coverage impact and targets for changed lines/modules: changed lines must not reduce coverage. New behavior
  added to `OutlookFolderHierarchyProvider`, `BreadcrumbBridgeRouter` (including its new partial),
  `BreadcrumbRow`, `BreadcrumbStateModel`, and `FolderBreadcrumbBridgeRouter` targets `>= 90%` line coverage as
  new logic, per the General Unit Test Policy. Coverage is collected with `/EnableCodeCoverage`. The
  coverage-denominator change caused by PR #605 removing `[ExcludeFromCodeCoverage]` from the unowned
  `EfcFormController` (1084 lines) is an inherited condition and is not this feature's obligation to remedy.
- Toolchain commands to run (format → lint → type-check → test), in this exact order, restarting from step 1 if
  any step fails or changes files:
  1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- Manual validation steps (if required): in EfcViewer, confirm the inherited lineage rendering and percentage
  display are still intact after this feature's changes; select a row, type an additional search character, and
  confirm a subsequent move does not target the previously selected folder; in the ItemViewer folder selector,
  confirm rows now show the full ancestor chain and that filing still targets the presented stem; in
  QuickFiler, confirm the Pop Out / Enumerate Conversation dialog is still reachable by the same gesture as
  today and that Up/Down/Enter/Escape selector behavior from #400 is unchanged.

## Acceptance Criteria

- [ ] **AC-1 (#498)** — A `segmentDoubleClick` message with `segmentIndex` outside `[0, segments.Count - 1]`
      is rejected by a range guard in the `SegmentDoubleClick` arm of
      `BreadcrumbBridgeRouter.ProcessInboundAsync` (`:241-247`); no exception escapes
      `_host.Raise(h => h.MessageReceived += null, ...)`, and the outbound posted-message count is unchanged.
- [ ] **AC-2 (#498)** — The rejected index is logged at `Error` using the existing `log4net` pattern in the
      same file (`BreadcrumbBridgeRouter.cs:235`, `:336`), and `BreadcrumbRow.CollapseAfter`
      (`BreadcrumbRow.cs:200-229`) is unmodified: its documented throw contract (`:197-199`, `:207-214`)
      still holds when called directly.
- [ ] **AC-3 (#498)** — A valid `segmentIndex` still collapses the row and posts a render, and the
      `catch (BreadcrumbMessageException)` at `BreadcrumbBridgeRouter.cs:266-277` is still the only catch at
      the `async void` host-message boundary (no broad `catch (Exception)` added there).
- [ ] **AC-4 (#499)** — `BindRowsAsync` sets `SelectedFolderPath` to `null` alongside `_selectedRowId = null`
      (`BreadcrumbBridgeRouter.cs:136`), so after a re-bind that follows a selection `SelectedFolderPath` is
      `null` rather than the pre-rebind folder. The two existing `SelectedFolderPath` write sites — `SelectRow`
      (`:484-487`) and `SelectHierarchyPath` (`:497`) — are unchanged, and a test confirms each still assigns
      the value it assigns today.
- [ ] **AC-5 (#499)** — `SelectedFolderPathChanged(this, null)` is raised on that clear **only when the value
      actually changed**; a re-bind with no prior selection raises no event.
- [ ] **AC-6 (#499)** — No auto-selection side effect is introduced: `SelectFirstRow`
      (`BreadcrumbBridgeRouter.cs:192-199`) is still not called from `BindRowsAsync` (`:92-138`).
- [ ] **AC-7 (#440 Qfc prerequisite)** — `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` (`:52-71`) keeps
      its exact `OrdinalIgnoreCase` first pass (`:66-68`): a caller supplying a full Outlook path resolves
      exactly as it does today. A test asserts EXPLICITLY that the landed Efc full-path caller — the value
      produced by `BreadcrumbBridgeRouter.ToHierarchyPath` (`:140-163`) — resolves through the exact first pass
      and NEVER reaches the suffix fallback, so the D5 change is a strict no-op for the Efc surface.
- [ ] **AC-8 (#440 Qfc prerequisite)** — When the exact pass misses, a segment-boundary suffix match resolves
      an archive-root-relative stem (for example `Projects\Alpha`) to the unique node whose `FolderPath` ends
      with `\Projects\Alpha`.
- [ ] **AC-9 (#440 Qfc prerequisite)** — The suffix fallback is accepted **only when unique**: with a decoy
      node (`\\store\Inbox\Projects\Alpha` alongside `\\store\Archive\Projects\Alpha`) the method returns
      `null`, logs at `Error`, and the row keeps today's single-segment fallback rendering
      (`BreadcrumbRowBuilder.cs:121-142`, fallback segment constructed at `:123-131`, which is not modified).
- [x] **AC-10 (#439 — INHERITED-AND-VERIFIED, retired)** — On the Efc surface, a bound suggestion or search row
      whose stem resolves renders a multi-segment ancestor chain in root-to-leaf order. **Delivered by PR #605,
      not by this feature.** Verified by the landed test method
      `Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability` in
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs:20-116`, whose root-to-leaf ordering
      assertion is at `:108-113`. No work item here; the test file is read-only for this feature.
- [ ] **AC-11 (#440 Qfc prerequisite)** — On the Qfc surface, the D5 resolution produces a multi-segment chain,
      asserted in `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` against a
      `MockBehavior.Strict` provider (`ProviderMock`, `:51-70`).
- [x] **AC-12 (#439 / former decision D6a — INHERITED-AND-VERIFIED, retired)** — The suggestion-row
      **percentage is still rendered** after the chain resolves. **Delivered by PR #605, not by this feature**,
      by moving the probability join onto `presentedText` (`BreadcrumbRowBuilder.cs:133`). Verified by the
      `73%` assertion at `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs:114`. No work item
      here; `BreadcrumbRowBuilder.cs` remains unowned and unwritten.
- [x] **AC-13 (#439 / former decision D6b — INHERITED-AND-VERIFIED, retired)** — The **Efc filing target is
      still the presented stem** after the chain resolves. **Delivered by PR #605, not by this feature**, via
      the immutable `BreadcrumbRow.FilingTarget` (`:88`) and `SelectRow`'s derivation from it
      (`BreadcrumbBridgeRouter.cs:484-487`). Verified by the assertion that `SelectedFolderPath` equals the
      presented stem at `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs:115`, so the
      `DestinationOlStem` contract at `EfcDataModel.cs:286`, `:307`, `:325` is unbroken. No work item here.
- [ ] **AC-14 (#440 Qfc prerequisite, decision D7 — Qfc filing target)** — The D7 ladder rung actually taken is
      recorded in this spec's RISK-1 entry with the read-only evidence that selected it, and that rung's stated
      criterion is met: rung 1 — a test shows the Qfc selected-folder value is still the presented stem after
      the chain resolves, with `BreadcrumbSelectionMap.cs` unmodified; rung 2 — a test shows Qfc filing
      behavior is byte-identical to today because the router does not consume the newly-resolved chain for the
      filing path, and the deliberate limitation is recorded in this spec; rung 3 — execution halted and
      reported the blocking dependency on `BreadcrumbSelectionMap.cs:109` without writing it.
- [ ] **AC-15 (#440)** — On the Efc surface, Left on a row whose resolved chain has more than one segment
      selects that row's parent node, and repeated Left presses walk up the ancestor chain. The transition is
      expressed through the landed `BreadcrumbRow.ActivateSegment(int)` (`BreadcrumbRow.cs:151-172`), not
      through a newly added parallel selected-node index (D9). A test asserts `ActiveSegmentIndex` decreases by
      one per Left press until `ActivateSegment` refuses at the root, at which point the D1 fall-through runs.
- [ ] **AC-16 (#440)** — On the Efc surface, Right on a selected node expands it into its children, retrieved
      through the existing `IFolderHierarchyProvider.GetImmediateSubfoldersAsync`
      (`IFolderHierarchyProvider.cs:46-49`) in a SINGLE call keyed on `row.ActiveSegmentKey`
      (`BreadcrumbRow.cs:101-105`), matching the landed `ExpandLeafAsync`
      (`BreadcrumbBridgeRouter.cs:364-408`, key read at `:375`, single provider call at `:384-385`). **The
      `ResolveLeafKeyAsync` → `GetImmediateSubfoldersAsync` two-call pattern that version 1.0 required NO
      LONGER EXISTS in `ExpandLeafAsync` and must not be reintroduced.** No member is added to
      `IFolderHierarchyProvider`. Additionally, per D9, the descent mechanism by which Right moves selection
      back toward the leaf is implemented and tested — `ActivateSegment` refuses the leaf index
      (`BreadcrumbRow.cs:156`) and cannot express it — and the spec's D9 entry records which of the two
      permitted mechanisms was chosen.
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
      Right on a childless node remain silent no-ops emitting no message. **The Efc boundary code moved with
      PR #605 and is re-cited here:** the `LeftArrow` root refusal is at `BreadcrumbRow.cs:304-308`; the arrow
      routing is at `BreadcrumbBridgeRouter.cs:304-339` (Right branch `:308-321`, Left branch `:322-328`); and
      the childless-node early return, which now tests the ACTIVE segment rather than the leaf, is at
      `ExpandLeafAsync` `:366-370`.
- [ ] **AC-24 (decision D2 — Qfc boundaries)** — Qfc boundary behavior is unchanged: an unhandled transition
      still emits `UnhandledArrowMessage` (`FolderBreadcrumbBridgeRouter.cs:387-393`) and still reaches
      `KeyboardHandler.BreadcrumbArrowFallThrough` (`:288-315`, Right branch `:302-310`), so Right still opens
      the Pop Out / Enumerate Conversation dialog and Left still calls `SetFolderDroppedDown(false)` (`:313`).
      Asserted at the `BreadcrumbArrowFallThrough` call site (precedent
      `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:156-168`), never by invoking the
      modal `MyBox.ShowDialog` (`:304-309`).
- [ ] **AC-25 (#498 — RED first)** — A regression test for #498 in
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` is demonstrated **failing before the
      fix** and passing after, with the failing run recorded in the feature's evidence directory.
- [ ] **AC-26 (#499 — RED first)** — A regression test for #499 in
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` is demonstrated failing before the fix
      and passing after, with the failing run recorded.
- [ ] **AC-27 (#440 Qfc prerequisite — RED first)** — A regression test for the D5 suffix fallback in
      `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` (realistic full paths
      plus a decoy node) is demonstrated failing before the fix and passing after, with the failing run
      recorded. The test home is unchanged from version 1.0; only the attribution changed.
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
      modified. In particular `EfcFormController.cs`, `KbdActions.cs`,
      `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` (**sibling epic child 501 owns it**),
      `BreadcrumbRowBuilder.cs`, `BreadcrumbDocumentAssets.cs`, `BreadcrumbHtmlRenderer.cs`,
      `BreadcrumbSelectionMap.cs`, `IFolderHierarchyProvider.cs`, `FolderBreadcrumbAssetContractTests.cs`, and
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` are absent from the diff.
- [ ] **AC-31 (policy — file size, decision D8)** — `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`
      finishes at or under 500 lines. It starts at **596** lines, already in breach, so the mandatory D8
      partial-class split is performed FIRST and must bring the file to <= 500 INCLUDING this feature's
      additions; the new partial file carries a `<Compile Include>` entry in `QuickFiler/QuickFiler.csproj`
      placed adjacent to the existing router entry at `:290`. Every other file written by this feature is also
      at or under 500 lines, and `QuickFiler/Resources/FolderBreadcrumb.html` remains at or under 500 lines
      without being split. This criterion asserts NOTHING about
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` (531 lines) beyond recording it as
      an inherited pre-existing 500-line violation that arrived with PR #605, that this feature does not own,
      does not write, and does not worsen.

## Risks & Mitigations

- Technical or operational risks:

  - **RISK-1 (HIGHEST, decision D7) — Qfc filing target flips from stem to full path once Qfc chains resolve.**
    **Re-scoped at version 1.1 to the Qfc surface only.** Once the D5 fallback makes Qfc chains resolve,
    `BreadcrumbSelectionMap.GetSelectedFolder` returns `row.Chain[row.Chain.Count - 1].FolderPath` for a
    suggestion row (`BreadcrumbSelectionMap.cs:109`), and that file is **not owned**. Research §Q6a records
    this as the feature's single largest open risk and marks the in-ownership alternative **unverified**. The
    Efc analogue of this risk was resolved on `main` by PR #605 (see RISK-3).
    **Resolution ladder, taken in order. The choice is made by READING `BreadcrumbStateRow` and
    `CreateFallbackRow` during execution; the reading is read-only.**
    1. **PREFERRED** — preserve the presented stem through the owned `FolderBreadcrumbBridgeRouter.cs`
       (`CreateFallbackRow` `:245`, `ReplaceRowsPreservingSession` `:474`), so `BreadcrumbSelectionMap.cs`
       needs no change. The landed `BreadcrumbRow.FilingTarget` (`:88`) is the in-repo precedent for this
       shape, on the Efc row type; whether the Qfc `BreadcrumbStateRow` can carry an equivalent is what the
       rung-1 read must settle.
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

  - **RISK-2 (former decision D6a) — RESOLVED BY INHERITANCE.** The risk was that the suggestion-row
    percentage would be silently lost, because it worked only while the lineage was broken
    (`joinPath == presentedText == scorer key`). **Landed mechanism:** PR #605 moved the probability join onto
    `presentedText` (`BreadcrumbRowBuilder.cs:133`), so the join key no longer depends on whether the chain
    resolved; `BuildProbabilityIndex` (`:210-229`) stays keyed on the scorer's `FolderPath` and the two agree
    by construction. **Landed test:** the `73%` assertion at
    `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs:114`, within
    `Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability` (`:20-116`). The identifier is
    retained rather than deleted so that a reader of version 1.0 can trace the disposition. No mitigation is
    owed by this feature; AC-12 is retired as inherited-and-verified.

  - **RISK-3 (former decision D6b) — RESOLVED BY INHERITANCE.** The risk was that the Efc filing target would
    be silently broken when `SelectedFolderPath` flipped from stem to full Outlook path. **Landed mechanism:**
    PR #605 added the immutable `BreadcrumbRow.FilingTarget` (`:88`), set from `presentedText` by the builder
    (`BreadcrumbRowBuilder.cs:141`), and changed `SelectRow` to derive `SelectedFolderPath` from it
    (`BreadcrumbBridgeRouter.cs:484-487`); `row.LeafSegment` is no longer referenced by the router. The
    `DestinationOlStem` contract at `EfcDataModel.cs:286`, `:307`, `:325` is intact. **Landed test:** the
    assertion that `SelectedFolderPath` equals the presented stem at
    `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs:115`. No mitigation is owed by this
    feature; AC-13 is retired as inherited-and-verified.

  - **RISK-4 (decision D8) — the 500-line breach has ALREADY OCCURRED.** This is no longer a risk of discovery
    mid-execution; it is a present condition. `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` stands at
    **596** lines, 96 over the limit, and receives #498, #499, and the #440 Efc transitions.
    `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` stands at 531 lines but is neither
    owned nor written here. Three further owned files remain within 50 lines of the limit
    (`FolderBreadcrumbBridgeRouter.cs` 485, `BreadcrumbStateModel.cs` 457) or cannot be split at all
    (`FolderBreadcrumb.html` 489). *Response, not mitigation:* the partial-class split of
    `BreadcrumbBridgeRouter.cs` is MANDATORY and must precede every other change to that file (D8), with a new
    `<Compile Include>` in `QuickFiler/QuickFiler.csproj` beside the existing entry at `:290`. Splits remain
    pre-authorized for the other two `.cs` files, with in-repo precedent
    (`FolderBreadcrumbBridgeRouter.SearchPresentation.cs`, `UtilitiesCS.csproj:629-630`); the html change must
    fit in 11 lines; AC-31 gates the result.

  - **RISK-5 — `.csproj` rebase conflict with sibling epic children.** The `Compile Include` item groups are
    shared with concurrently-executing children. *Mitigation:* place any new entry **adjacent to its nearest
    sibling entry**, not forced into strict alphabetical order. `QuickFiler.Test.csproj`'s breadcrumb entries
    are already NOT strictly alphabetical: PR #605 placed `BreadcrumbBridgeRouterIssue439Tests.cs` at `:60`,
    after `BreadcrumbBridgeRouterTests.cs` at `:59`, although `Issue439` sorts before `Queue` and `Tests`.
    Forcing strict alphabetical placement now would move an existing line and increase, not reduce, the
    conflict surface. No `.csproj` edit is needed for the existing test files; the only required edit is the
    D8 partial entry in `QuickFiler/QuickFiler.csproj`.

  - **RISK-6 — #440's Qfc half is untestable against production data if sequenced before chain resolution.**
    **Discharged for the Efc surface**, where PR #605 already produces multi-segment rows. **Live for the Qfc
    surface only:** research §Q4f verifies both transitions are permanent no-ops on a one-segment row.
    *Mitigation:* the fixed sequence #498 → #499 → Qfc chain-resolution prerequisite (D5 + D7) → #440.

  - **RISK-7 — unverified relative-root variance across suggestion sources.** Research §Q3a marks the Bayesian
    `prediction.Class` (`FolderScorer.cs:178`) and conversation-map `EmailFolder` (`:323`) path forms
    unverified. *Mitigation:* D5's segment-boundary suffix rule does not depend on which relative root a source
    uses, and AC-9's uniqueness requirement fails closed to today's rendering when the form is ambiguous.

  - **RISK-8 — #499's null selection reaching a null-intolerant consumer.** *Mitigation:* resolved by D4's
    orchestrator-verified reading of `EfcFormController.IsValidSelection` (`:1038-1050`, `selectedFolder is
    null` disjunct at `:1044`) and both guarded call sites (`:468`, `:752`). Residual risk is low and the
    cross-feature note to feature 464 is informational.

- Mitigations and rollbacks: the mandatory D8 split and each subsequent fix land as separate commits in the
  stated sequence, so any single fix can be reverted independently. No feature flag and no configuration switch
  is introduced.

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
  - **The two D3 descopes are withdrawn.** The Efc mouse gesture and the `>` to `→` separator glyph both
    landed under #439's own workstream (PR #605). Neither requires a follow-up potential entry.
  - **Issue #439 is open on GitHub but fixed in code.** Whoever closes out this feature should reconcile that,
    citing PR #605 and feature commit `c39db103`. This spec does not change issue state.
  - **Pre-existing 500-line violations (D8)** — `FolderPredictor.cs` (983 lines) and `EfcFormController.cs`
    (1084 lines), plus the test files `FolderPredictorTests.cs` (985 lines) and the newly inherited
    `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` (531 lines). None is inherited as a
    responsibility or worsened by this feature.
  - **`EfcFormController` coverage denominator.** PR #605 removed its `[ExcludeFromCodeCoverage]` attribute,
    putting 1084 unowned lines into the coverage denominator. Recorded as an inherited condition; addressing it
    belongs to whoever owns that file (feature 464).
- Links:
  - Issues: #498 (primary), #440, #499. #439 is inherited as fixed (PR #605) and requires no work here.
  - Feature folder: `docs/features/active/breadcrumb-router-navigation-defects-498/`.
  - Research (primary input, verified at HEAD `988e819b`; superseded in the sections listed under
    "Post-#439 Reconciliation" and deliberately not edited):
    `docs/features/active/breadcrumb-router-navigation-defects-498/research/2026-08-24T09-50-breadcrumb-router-navigation-defects.md`.
  - Landed #439 feature folder:
    `docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/`.
  - Superseded-in-part criterion: `docs/features/archive/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md:247`.
  - Promoted potentials: the four documents listed in "Context".
  - PRs: PR #605 (the landed #439 fix, merged to `main`). This feature's PR to be recorded on creation.

