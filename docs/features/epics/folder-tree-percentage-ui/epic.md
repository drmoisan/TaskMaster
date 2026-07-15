---
epic: folder-tree-percentage-ui
integration_branch: epic/folder-tree-percentage-ui-integration
created_at: 2026-07-15T16-43
intent:
  epic_type: business
  business_outcome_hypothesis: Surfacing folder hierarchy as an expandable tree and printing the prediction probability as a whole-number percentage in both the EfcViewer folder list and the QuickFiler folder dropdowns lets a user judge and navigate suggested filing targets faster and with more confidence, while fixing the QuickFiler expanded-mode inline-image rendering defect removes a visible correctness gap in the reading pane.
  leading_indicators:
    - Folders that contain subfolders render with an expand affordance (plus/minus) in both the EfcViewer folder list and the QuickFiler folder dropdown, and can be expanded and collapsed by mouse and by keyboard (right arrow expands, left arrow collapses when highlighted).
    - Each suggestion shows its prediction probability right-aligned in whole-number percentage format (no decimal places) in both controls.
    - In QuickFiler expanded mode, inline images referenced by cid: render in the message body instead of appearing as broken or missing images.
  nfrs:
    - The probability value surfaced to the UI is the same score the scoring layer already computes for internal ranking; no change to ranking behavior or model output is introduced.
    - Full C# toolchain (csharpier, .NET analyzers, nullable, MSTest) green for every child feature; changed and new code meets repository coverage thresholds.
    - The QuickFiler inline-image fix carries a dedicated regression test per the repository Bugfix Workflow and does not alter the compact-mode rendering call path beyond cid: resolution.
features:
  - issue_num: 9001
    feature_folder: folder-probability-plumbing
    depends_on: []
  - issue_num: 9002
    feature_folder: efcviewer-folder-tree-percentage
    depends_on: [9001]
  - issue_num: 9003
    feature_folder: quickfiler-folder-tree-percentage
    depends_on: [9001]
  - issue_num: 9004
    feature_folder: quickfiler-inline-image-cid-fix
    depends_on: []
---

# Epic: Expandable Folder Tree, Percentage Display, and QuickFiler Inline-Image Fix

- Integration branch: `epic/folder-tree-percentage-ui-integration`
- Status: Planning phase in progress. `issue_num` values above are placeholders and are
  back-filled from each child's promotion receipt as preparation completes; `depends_on` is
  rewritten to the resolved issue numbers before the kickoff artifact is written.

> Note: the `issue_num` and `feature_folder` values are placeholders at authoring time and are
> resolved to concrete values during preparation. This manifest is committed in final, resolved
> form before the kickoff prompt is emitted.

## Goal

Deliver three related user-facing improvements to how filing suggestions are presented:

1. In the EfcViewer matching-folders list, render folders that contain subfolders as expandable
   tree nodes (plus to expand, minus to collapse; right arrow expands the highlighted node, left
   arrow collapses it), and print each suggestion's prediction probability right-aligned in
   whole-number percentage format (no decimal places).
2. Provide the same expandable-tree and percentage behavior in the QuickFiler folder dropdowns.
3. Fix the QuickFiler expanded-mode defect where inline images referenced by `cid:` do not
   render in the message body.

## Scope

- Plumb the prediction probability value end-to-end through the scoring layer so it is available
  to the UI, where today only folder names are handed to the controls.
- Build tree expand/collapse behavior (mouse and keyboard) on the EfcViewer folder `ListBox`
  and, separately, on the QuickFiler folder `ComboBox` across its viewer variants, since neither
  control natively supports hierarchy and the two implementations share no base class.
- Add `cid:` reference resolution to the QuickFiler WebView2 body-rendering path so inline
  images render.

## Non-Goals

- No change to the scoring/ranking algorithm itself or to model output; the probability surfaced
  is the score already computed for internal ranking.
- No unification of the two EfcViewer implementations or of the QuickFiler viewer variants into a
  shared base control beyond what is required to deliver the behavior.
- No change to compact-mode rendering beyond the shared `cid:` resolution that also benefits
  expanded mode.
- Creating the repo-root `quality-tiers.yml` (a pre-existing gap) is out of scope for this epic.

## Shared Design

The probability value is the single shared contract across the epic. Feature 9001 introduces the
end-to-end plumbing that exposes the per-folder probability from `FolderScorer`/`FolderPredictor`
to the presentation layer for the first time. Features 9002 and 9003 consume that contract to
render the right-aligned whole-number percentage in the EfcViewer folder list and the QuickFiler
folder dropdowns respectively. Feature 9004 is isolated in the QuickFiler WebView2 body-rendering
path and shares no files with the tree/percentage work.

## Decomposition Rationale

Current state verified in code:

- `EfcViewer.cs` / `EfcViewer3.cs` (two parallel, non-shared implementations) render matching
  folders in a flat `ListBox` (`FolderListBox`) bound to a `string[]` of full folder paths. No
  hierarchy, no `TreeView`, no expand/collapse state exists in this path.
- QuickFiler's folder dropdown (`CboFolders`) is a plain `ComboBox`, independently declared in
  multiple viewer Designer files (up to nine variants such as `QfcItemViewerV1`,
  `QfcItemViewerExpanded`, `QFCItemViewerDarkNew`) with no shared base control.
- Probability/score is not surfaced to the UI at all today. `FolderScorer`/`FolderPredictor`
  compute scores only for internal ranking; the string arrays handed to both controls contain
  folder names only.
- Email body renders via a single `WebView2.NavigateToString(ItemHelper.Html)` call. No code
  resolves `cid:` references against attachment `Content-Id`/`PR_ATTACH_CONTENT_ID`; there is no
  `WebResourceRequested` handler or virtual host mapping. Expanded and compact modes use the
  identical rendering call, differing only in on-screen size, so broken inline images are a
  missing-feature defect rather than a mode-specific regression.

This is decomposed into four independently mergeable child features:

- **9001 — Probability plumbing (wave 0, C3).** Plumbs the actual probability value end-to-end
  through the T1-tier `FolderScorer`/`FolderPredictor` scoring layer so a per-folder probability
  is exposed to the presentation layer. Complexity floor is forced by the `classifier_or_model_logic`
  and `cross_module_contract_change` signals (T1 scoring surface, new public contract consumed
  across module boundaries). No `depends_on`.
- **9002 — EfcViewer folder tree + percentage (wave 1, C3).** Builds tree expand/collapse on the
  `ListBox` (mouse plus keyboard left/right) across both `EfcViewer` implementations and renders
  the right-aligned whole-number percentage. `C3` from the expand/collapse state-transition
  invariants; consumes 9001's probability contract. `depends_on: [9001]`.
- **9003 — QuickFiler dropdown tree + percentage (wave 1, C3).** Same tree and percentage
  behavior on the QuickFiler folder `ComboBox`, repeated across the viewer variants. `C3` from
  the state-transition invariants and the cross-cutting change across variants; consumes 9001's
  probability contract. `depends_on: [9001]`.
- **9004 — QuickFiler inline-image `cid:` fix (wave 0, C2).** Bugfix-workflow child that adds
  `cid:` resolution to the WebView2 body-rendering path (`MailItemHelper.Html.cs` and the
  WebView2 setup/wiring), isolated with no file overlap with the tree/percentage work. `C2`
  localized change with no floor signal; carries its own failing regression test first per the
  Bugfix Workflow. No `depends_on`.

## Waves

Wave assignment by longest-path layering over the dependency DAG
(`wave(f) = 0` when `depends_on` is empty, else `1 + max(wave(d))`):

- **Wave 0:** 9001 (probability plumbing), 9004 (inline-image fix).
- **Wave 1:** 9002 (EfcViewer tree + percentage), 9003 (QuickFiler dropdown tree + percentage).

The DAG is cycle-free. 9002 and 9003 have no interdependency and execute in parallel within
wave 1. 9004 is independent of the entire tree/percentage chain and executes in parallel within
wave 0.
