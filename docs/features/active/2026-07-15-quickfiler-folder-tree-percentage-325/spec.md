# quickfiler-folder-tree-percentage — Spec

- **Issue:** #325
- **Parent (optional):** epic `folder-tree-percentage-ui` (child 9003, wave 1, complexity C3)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-15T16-43
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-feature
- **Depends on:** `folder-probability-plumbing` (epic placeholder issue 9001)
- **Authoritative current-state source:** `research/2026-07-15T16-43-quickfiler-folder-tree-percentage-research.md`

## Overview

In the QuickFiler folder dropdown, filing-target suggestions are presented as a flat list of folder
names in a plain `ComboBox` (`CboFolders`). Two gaps result:

1. Folders that contain subfolders are not navigable as a hierarchy; the user cannot expand or
   collapse parent folders.
2. The prediction probability that the scoring layer already computes for internal ranking is not
   surfaced to the dropdown; the arrays handed to the control contain folder names only.

This feature delivers the same behavior the EfcViewer folder-list feature (sibling child 9002)
delivers on its `ListBox`:

1. Render folders that contain subfolders as expandable tree nodes with a plus/minus affordance.
2. Print each suggestion's prediction probability right-aligned as a whole-number percentage
   (no decimal places).

The prediction probability is **consumed** from the upstream sibling `folder-probability-plumbing`
(issue 9001). This feature does not recompute scores and does not implement 9001's scoring changes.

## Behavior

In the QuickFiler folder dropdown (`CboFolders`):

- A folder that contains subfolders renders as an expandable tree node with a plus (`+`) affordance
  to its left. Clicking the plus expands the node; clicking the resulting minus (`-`) collapses it.
- When a node is highlighted, the Right arrow key expands it and the Left arrow key collapses it.
- Each suggestion prints its prediction probability right-aligned in whole-number percentage format
  (for example `43%`), with no decimal places. Rows that have no probability (synthesized ancestor
  nodes, sentinel header rows, recents, and the injected "Trash to Delete" row) render an empty
  percentage field.

## Scope

### In scope

- The single runtime-live viewer `ItemViewer` — the only `IItemViewer` implementer instantiated at
  runtime (`QuickFiler/Helper Classes/ItemViewerQueue.cs:105`, `return new ItemViewer();`) and the
  only type wired to the live controllers (`QfcItemController`, `EfcItemController`). The functional
  change is scoped to `ItemViewer` and its seam interface `IItemViewer`.
- Owner-draw configuration of the existing `CboFolders` ComboBox on `ItemViewer`.
- New host-neutral, testable seams: `FolderHierarchyBuilder`, `FolderTreeStateModel`,
  `PercentageFormatter`, `FolderNodeViewModel` (see Host-Neutral Seam Architecture).
- A new `IItemViewer` intent member carrying folder identity plus probability (for example
  `SetFolderSuggestions(IReadOnlyList<FolderSuggestion>)`), alongside the retained
  `SetFolderItems(string[])` seam.
- Controller injection in `QfcItemController.*` to hand suggestions (identity + probability) to the
  new intent member.

### Out of scope

- The other nine `CboFolders`-declaring types (`Form1`, `ItemViewerExpanded`, `QfcItemViewer`,
  `QFCItemViewerDarkNew`, `QfcItemViewerExpanded`, `QfcItemViewerExpandedLight`,
  `QFCItemViewerLightNew`, `QfcItemViewerLightSelected`, `QfcItemViewerV1`). These are dead,
  design-time-only variants. Each is `[ExcludeFromCodeCoverage]` and none is instantiated in
  production code. They require **no change** and must be left untouched; touching them adds churn
  and coverage-denominator risk with no runtime effect. If the epic later requires them changed for
  consistency, that is a separate, optional cleanup decision for the maintainer and is not required
  to deliver #325.
- The 9001 scoring changes. #325 consumes the 9001 contract; it does not implement, recompute, or
  modify `FolderScorer`/`FolderPredictor` scoring math.
- Any change to the EfcViewer `ListBox` folder-population path (owned by sibling 9002).
- Any change to the QuickFiler body-render / WebView2 path (see Non-Interference with 9004).

## Consumed Upstream Contract (9001)

9001 owns and introduces a new public contract exposing a per-folder prediction probability from the
scoring layer (`FolderScorer`/`FolderPredictor`) to the presentation layer. Today that probability is
discarded at `FolderScorer.ToArray(int)` (only folder-path keys survive). #325 consumes the contract.

Planned consumed shape (owned by 9001; **not implemented here**):

```csharp
// OWNED / INTRODUCED BY 9001 — do not implement here; plan against this shape.
public readonly struct FolderSuggestion   // net48: readonly struct, explicit ctor (no record struct)
{
    public FolderSuggestion(string folderPath, double probability) { ... }
    public string FolderPath { get; }   // folder identity: rooted/relative Outlook folder path
    public double Probability { get; }  // [0,1] fraction
}

// Exposed from the scoring layer (FolderPredictor/FolderScorer), consumed by #325:
IReadOnlyList<FolderSuggestion> GetFolderSuggestions();  // or equivalent member
```

Consumption rules:

- #325 reads `FolderSuggestion.Probability` verbatim and formats it. It does **not** recompute
  scores, re-run the classifier, or touch `FolderScorer` scoring math.
- The exact member name and return type are 9001's decision; #325 plans against "folder identity plus
  its probability" and adapts to the concrete member at epic execution time.
- **Dependency citation:** #325 depends on 9001 for the per-folder probability. Absent 9001, the
  percentage column cannot be populated, because the arrays handed to `CboFolders` today are
  folder-name strings only (confirmed at `ItemViewer.FolderSearch.cs:13` and `FolderScorer.ToArray`).
- **Ordering at epic execution:** 9001 merges into the integration branch before #325 runs, so the
  contract is present when this feature executes.

## Inputs / Outputs

- **Inputs:** the ordered per-folder suggestion list from the 9001 contract
  (`IReadOnlyList<FolderSuggestion>`: folder path + `[0,1]` probability), plus the existing flat
  string entries (sentinel headers, recents, "Trash to Delete") already routed through
  `SetFolderItems(string[])`.
- **Outputs:** the projected visible-row list bound to `CboFolders.Items` (indent + glyph + display
  name + right-aligned percentage), and the selected full folder path returned to the controller
  unchanged.
- **Config keys and defaults:** none introduced.
- **Versioning / backward-compatibility:** the `SetFolderItems(string[])` seam and every existing
  controller call site and MSTest expectation are preserved; the new `SetFolderSuggestions` intent
  member is additive.

## API / CLI Surface

- **New `IItemViewer` intent member:** `SetFolderSuggestions(IReadOnlyList<FolderSuggestion>)` (or an
  equivalently named member) carrying folder identity plus probability. Additive; does not replace
  `SetFolderItems(string[])`.
- **Retained seam members:** `SetFolderItems(string[])`, `ClearFolderItems()`,
  `FolderContains(string)`, `GetFolderItems()` remain for the sentinel/recents/"Trash to Delete"
  paths and backward test compatibility.
- **Host-neutral seams (public within the assembly):** `FolderHierarchyBuilder`,
  `FolderTreeStateModel`, `PercentageFormatter.Format(double) : string`, `FolderNodeViewModel`.
- **Validation rules:** `PercentageFormatter` clamps probability to `[0,1]`; the tree-state model
  enforces INV1-INV8 (see State-Transition Invariants).

## Data & State

- **Data flow:** 9001 contract -> `FolderHierarchyBuilder` (build `TreeNode<FolderNodeViewModel>`
  forest by splitting folder paths on `\`) -> `FolderTreeStateModel` (expand/collapse/highlight state
  plus visible-row projection) -> `CboFolders.Items` rebind -> owner-draw `DrawItem` render.
- **Data transformations and invariants:** hierarchy building attaches probability at the leaf node
  representing the full folder; synthesized ancestor nodes carry no probability. The tree-state model
  enforces INV1-INV8. Selection value remains the full folder-path string the controller expects.
- **Caching / persistence:** none. All state is in-memory per dropdown population.
- **Migration / backfill:** none.

## Recommended Approach: Owner-Draw the Existing ComboBox

Set `CboFolders.DrawMode = OwnerDrawFixed` and bind the ComboBox `Items` to a host-neutral projection
of the currently-visible tree rows. Expand/collapse mutates the host-neutral `FolderTreeStateModel`
and re-projects the visible rows; the ComboBox is a dumb renderer.

- **Rendering:** `DrawItem` paints, per visible row, an indent proportional to `Depth`, a `+`/`-`
  glyph when the node has children (no glyph for leaves), the folder display name, and the
  right-aligned percentage.
- **Plus/minus click:** hit-test the glyph rectangle against the mouse X within the drawn item
  bounds; on hit, toggle that node in the state model and re-project.
- **Arrow keys:** routed through the existing `QuickFiler/Controllers/KeyboardHandler.cs`; Right/Left
  invoke the state model's transitions on the highlighted-row node.

This approach preserves the `IItemViewer.SetFolderItems(string[])` seam and every existing controller
call site and MSTest expectation. There is repo precedent for owner-drawing this exact control
(`QuickFiler/Viewers/QfcItemViewerLightSelected.cs:46` `CboFolders_DrawItem`). A ComboBox dropdown has
no native hierarchy, so expand/collapse is a deliberate re-projection of the visible-row list on
toggle; the tree-state model owns correctness and is fully unit-tested.

Rejected alternative: replacing `CboFolders` with a TreeView-in-dropdown or ObjectListView
`TreeListView`. It gives native expand/collapse but breaks the `SetFolderItems(string[])` contract and
every controller call site, requires swapping the Designer control, and diverges from the ComboBox UX
used elsewhere in QuickFiler. Higher churn and regression risk for marginal benefit. Not selected.

## Host-Neutral Seam Architecture

All shared logic lives in host-neutral, testable seams. These are **not** COM/WinForms-exempt and
must meet the coverage thresholds below.

| Seam | Responsibility | Purity |
|------|----------------|--------|
| `FolderNodeViewModel` | per-node view model: FolderPath, DisplayName, Probability?, Depth, HasChildren, Expanded, Glyph, indent, formatted percentage | plain class/struct |
| `FolderHierarchyBuilder` | build a `TreeNode<FolderNodeViewModel>` forest from `IReadOnlyList<FolderSuggestion>` by splitting folder paths on `\`; reuse existing `TreeNode<T>` | pure |
| `FolderTreeStateModel` | expand/collapse/highlight state machine plus visible-row projection; enforces INV1-INV8 (the C3 state-transition driver) | pure |
| `PercentageFormatter` | `double` in `[0,1]` -> `"NN%"` with clamp and round | pure static |

- `FolderHierarchyBuilder` splits each suggestion's path into segments, walks/inserts nodes
  (find-or-add), and attaches the probability at the leaf node representing the full folder.
  Synthesized ancestor nodes carry no probability (blank percentage) but are `HasChildren` and
  expandable. Sentinel header rows, recents, and "Trash to Delete" are flat depth-0 leaf rows with no
  probability so they render unchanged. `DisplayName` is the last path segment; the full path is
  retained as the node key/selection value so selection still yields the string the controller
  expects. Reuse `UtilitiesCS/ReusableTypeClasses/Other/TreeNodeOfT.cs` (`TreeNode<T>`).
- The builder and state model are fully deterministic and unit-testable from in-memory
  path/probability inputs. No live Outlook, no `Microsoft.Office.Interop.Outlook` object, no
  filesystem, no temp files.

Host-bound glue that stays WinForms and is exempt:

- `ItemViewer` (already `[ExcludeFromCodeCoverage]`) and all `*.Designer.cs`.
- The `CboFolders` owner-draw `DrawItem` paint, glyph hit-test, and `Items` rebind in
  `ItemViewer.FolderSearch.cs` — thin WinForms glue; keep minimal.
- `KeyboardHandler` arrow-key routing glue (`[ExcludeFromCodeCoverage]`). The state transition it
  invokes (`FolderTreeStateModel.RightArrow/LeftArrow/Toggle`) is **not** exempt and is unit-tested
  directly.

## State-Transition Invariants (C3 driver)

`FolderTreeStateModel` enforces the following invariants, which must be unit-tested exhaustively
(positive, negative/no-op, boundary at root and leaf, single-highlight enforcement, collapse/re-expand
round-trip):

- **INV1 (no leaf expansion):** `Expanded => HasChildren`. A leaf is never `Expanded`.
- **INV2 (visibility):** a node is `Visible` iff all ancestors are `Expanded`; roots are always
  visible.
- **INV3 (single highlight):** at most one node is `Highlighted` across the forest at any time.
- **INV4 (glyph bijection):** `Glyph = '+'` iff `HasChildren && !Expanded`; `Glyph = '-'` iff
  `HasChildren && Expanded`; no glyph iff `!HasChildren`.
- **INV5 (descendant-state preservation):** collapsing a node does not change the `Expanded` state of
  its descendants, only their `Visible`-ness; re-expanding restores prior descendant expansion.
- **INV6 (toggle involution):** for a node with children, `Toggle . Toggle = identity` on `Expanded`.
- **INV7 (indent monotonic):** the rendered indent of a row equals `node.Depth`.
- **INV8 (stable order):** the visible/flattened row order is a stable pre-order DFS over expanded
  nodes; sibling ordering is deterministic (descending score then ordinal-key tie-break where scores
  are the ordering key, otherwise ordinal by segment name).

Transitions: `Expand`, `Collapse`, `Toggle`, `Highlight`, `RightArrow` (expands the highlighted node
when `HasChildren && !Expanded`, else no-op), `LeftArrow` (collapses when `HasChildren && Expanded`,
else no-op).

## Percentage-Formatting Rule

- Probability domain is a `double` fraction in `[0,1]` (confirmed:
  `UtilitiesCS/EmailIntelligence/Bayesian/Prediction.cs:27`; `FolderScorer` multiplies by 1000).
- Format:
  `percent = (int)Math.Round(Math.Clamp(p, 0.0, 1.0) * 100.0, MidpointRounding.AwayFromZero)`,
  rendered as `percent.ToString() + "%"` (for example `0.4267 -> "43%"`, `1.0 -> "100%"`,
  `0.0 -> "0%"`). The formatter clamps out-of-range input to `[0,1]`.
- Rows with no probability (ancestors, sentinels, recents, "Trash to Delete") render an empty
  percentage field.
- The rule is a pure static seam, `PercentageFormatter.Format(double) : string` (not exempt).
- Right-alignment is achieved in `DrawItem` by reserving a fixed-width right column and drawing the
  percentage with `StringAlignment.Far` / `TextFormatFlags.Right` into a right-anchored rectangle
  computed from `e.Bounds.Right` minus the reserved width. The percentage string is produced by the
  host-neutral formatter; only the paint-rectangle math is WinForms glue.

## Constraints & Risks

- Depends on the upstream 9001 per-folder probability contract exposed to the presentation layer. At
  epic execution time 9001 merges into the integration branch before this feature runs.
- `ComboBox` does not natively support hierarchy; expand/collapse is built on top of it by
  re-projecting the visible-row list on toggle (deliberate design choice; correctness owned by the
  host-neutral tree-state model).
- The functional change is confined to the single runtime-live `ItemViewer`; the nine dead
  design-time variants require no change.
- WinForms/COM coverage exemption applies to Designer-generated classes and `ItemViewer`; testable
  logic stays in host-neutral seams meeting coverage thresholds.
- net48 target forbids `record`, `record struct`, and `init` accessors (no `IsExternalInit` polyfill);
  DTOs and view models are plain classes or `readonly struct`s with explicit constructors.
- Shares NO files with the QuickFiler inline-image `cid:` bugfix sibling (9004); the body-render path
  must not be touched.

## Non-Interference with 9004 (QuickFiler inline-image `cid:` fix)

9004 is confined to `MailItemHelper.Html.cs` and the WebView2 body-render wiring. #325 shares **no
files** with 9004 and must not touch the body-render path. The following files, owned by or shared
with the body-render path, must not be modified by #325:

- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs` (9004's file)
- `QuickFiler/Viewers/ItemViewer.WebViewThread.cs`
- `QuickFiler/Viewers/WebView2CoreInitializer.cs`
- `QuickFiler/Viewers/IWebViewCoreInitializer.cs`
- The `NavigateToString`/WebView2 members on `IItemViewer` and the WebView2 initialization members on
  `ItemViewer.cs` / `ItemViewer.Designer.cs`.

#325's file set is disjoint from the above.

## Implementation Strategy

- **Implementation scope:** add the host-neutral seams (`FolderHierarchyBuilder`,
  `FolderTreeStateModel`, `PercentageFormatter`, `FolderNodeViewModel`); add the `SetFolderSuggestions`
  intent member to `IItemViewer` and implement it on `ItemViewer` via owner-draw `CboFolders`; inject
  suggestions from `QfcItemController.*` while retaining existing `SetFolderItems(string[])` behavior.
- **New classes/functions:** the four host-neutral seams above; the additive `IItemViewer` member;
  the `CboFolders` owner-draw `DrawItem`/glyph-hit-test/`Items`-rebind glue in
  `ItemViewer.FolderSearch.cs`.
- **Dependency changes:** none. Reuse existing `TreeNode<T>`.
- **Logging/telemetry:** none introduced beyond existing patterns.
- **Rollout:** no feature flag; behavior is delivered directly on the live `ItemViewer` dropdown.

## Toolchain and Coverage Posture

- **In-scope production projects:** `UtilitiesCS` (host-neutral seams) and `QuickFiler`
  (`IItemViewer`, `ItemViewer.FolderSearch.cs`, `ItemViewer` Designer owner-draw config,
  `QfcItemController.*` injection).
- **In-scope test assemblies:** `UtilitiesCS.Test` (host-neutral seam tests) and `QuickFiler.Test`
  (controller-injection tests via `Mock<IItemViewer>`; existing `SetFolderItems(string[])`
  expectations must remain green). Both are non-SDK `net4.8.1` projects with no glob compile — new
  test files require explicit `<Compile Include>` entries following the existing
  `Controllers\QfcItemController.*Tests.cs` pattern.
- **Test tooling:** MSTest (`[TestClass]`/`[TestMethod]`), Moq for the `IItemViewer` seam,
  FluentAssertions for assertions.
- **C# toolchain order (run in this exact order; restart from step 1 on any failure or auto-fix):**
  1. `dotnet tool run csharpier .` (or `csharpier .`)
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- **Coverage-threshold discrepancy (flagged for maintainer):** `CLAUDE.md` (embedded General Unit
  Test Policy) states repository line coverage `>= 80%` and new modules `>= 90%`.
  `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state uniform line
  `>= 85%` and branch `>= 75%` across tiers T1-T4. These conflict. This feature targets the
  **stricter** bar: line `>= 85%`, branch `>= 75%`, and the `>= 90%` new-module target from CLAUDE.md
  for the new host-neutral seams. Satisfying the stricter bar satisfies both documents. The
  discrepancy is surfaced to the maintainer for ratification.
- **Coverage config:** `coverage.config` excludes only third-party/mixed-mode modules and does not
  exclude any first-party production path; the new host-neutral seams are in the coverage denominator,
  as required.

## Acceptance Criteria

- [ ] Folders that contain subfolders render with a plus/minus expand affordance to the left of the
      node in the QuickFiler folder dropdown (`ItemViewer.CboFolders`); leaf folders render with no
      glyph.
- [ ] Clicking the plus affordance on a node expands it, and clicking the resulting minus affordance
      collapses it.
- [ ] With a node highlighted, the Right arrow key expands it (no-op when it is a leaf or already
      expanded) and the Left arrow key collapses it (no-op when it is a leaf or already collapsed).
- [ ] Each suggestion displays its prediction probability right-aligned in whole-number percentage
      format with no decimal places (for example `43%`); rows with no probability (synthesized
      ancestors, sentinel headers, recents, "Trash to Delete") render an empty percentage field.
- [ ] The percentage value is consumed verbatim from the upstream 9001 `FolderSuggestion` probability
      contract; scores are not recomputed and `FolderScorer`/`FolderPredictor` scoring math is not
      modified.
- [ ] Shared tree-state, hierarchy-building, and percentage-formatting logic lives in host-neutral,
      testable seams (`FolderHierarchyBuilder`, `FolderTreeStateModel`, `PercentageFormatter`,
      `FolderNodeViewModel`) that meet the repository coverage thresholds (target: line `>= 85%`,
      branch `>= 75%`, new-module `>= 90%`), with INV1-INV8 unit-tested exhaustively.
- [ ] The change is confined to the runtime-live `ItemViewer` and its `IItemViewer` seam; the nine
      dead `[ExcludeFromCodeCoverage]` design-time viewer variants are left untouched.
- [ ] The change shares no files with the 9004 inline-image `cid:` bugfix and does not modify the
      body-render / WebView2 path.
- [ ] The full C# toolchain (csharpier, .NET analyzers, nullable/TreatWarningsAsErrors, MSTest with
      coverage) passes green in a single final pass, and existing `SetFolderItems(string[])` controller
      expectations remain green.
