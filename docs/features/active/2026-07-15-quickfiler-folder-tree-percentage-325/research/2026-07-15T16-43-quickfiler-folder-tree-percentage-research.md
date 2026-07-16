# Research: QuickFiler Folder-Tree + Percentage UI (Issue #325)

- Feature: epic child `folder-tree-percentage-ui` child 9003, wave 1, complexity C3
- Canonical issue: **#325**
- Mode: preparation-only research; no production code modified.
- Timestamp: 2026-07-15T16-43
- Branch: TaskMaster-wt-2026-07-15T16-43

## Objective (restated)

In the QuickFiler folder dropdown(s), deliver EfcViewer-parity behavior (sibling child 9002):
(a) render folders that contain subfolders as expandable tree nodes with a plus/minus affordance
(clicking plus expands, clicking minus collapses; when a node is highlighted, Right arrow expands
and Left arrow collapses); and
(b) print each suggestion's prediction probability right-aligned as a whole-number percentage
(no decimals).

The per-folder probability is consumed from the upstream sibling `folder-probability-plumbing`
(epic placeholder issue 9001), which introduces a NEW public contract exposing a per-folder
probability from the scoring layer to the presentation layer. This feature does NOT recompute
scores and does NOT implement 9001; it plans against the PLANNED contract shape.

---

## Q1. Exact enumeration of viewer variants declaring `CboFolders`

Verified by field/property declaration grep in `QuickFiler/Viewers/`. Ten distinct WinForms types
declare a `CboFolders` member:

| # | Type | Declaration site | Kind | Instantiated at runtime? |
|---|------|------------------|------|--------------------------|
| 1 | `ItemViewer` | `ItemViewer.cs:389` (property) + Designer field `_cboFolders` | Property over Designer field | **YES** — `QuickFiler/Helper Classes/ItemViewerQueue.cs:105` `return new ItemViewer();` |
| 2 | `Form1` | `Form1.Designer.cs:106` | Designer field | No |
| 3 | `ItemViewerExpanded` | `ItemViewerExpanded.Designer.cs:807` | Designer field | No |
| 4 | `QfcItemViewer` | `QfcItemViewer.Designer.cs:942` | Designer field | No |
| 5 | `QFCItemViewerDarkNew` | `QFCItemViewerDarkNew.Designer.cs:739` | Designer field | No |
| 6 | `QfcItemViewerExpanded` | `QfcItemViewerExpanded.Designer.cs:924` | Designer field | No |
| 7 | `QfcItemViewerExpandedLight` | `QfcItemViewerExpandedLight.Designer.cs:793` | Designer field | No |
| 8 | `QFCItemViewerLightNew` | `QFCItemViewerLightNew.Designer.cs:730` | Designer field | No |
| 9 | `QfcItemViewerLightSelected` | `QfcItemViewerLightSelected.Designer.cs:772` | Designer field | No |
| 10 | `QfcItemViewerV1` | `QfcItemViewerV1.Designer.cs:728` | Designer field | No |

**Precise count of variants that require the functional change: ONE (`ItemViewer`).**

Evidence: the only `new ItemViewer()`/`new <variant>()` construction in production code is
`ItemViewerQueue.cs:105` (`return new ItemViewer();`). No production code instantiates any of the
other nine types. They are dead design-time variants (each carrying `[ExcludeFromCodeCoverage]`).
`ItemViewer` is also the only one that implements `IItemViewer` and is wired to the live controllers
(`QfcItemController`, `EfcItemController`).

The epic's "up to nine variants" estimate corresponds to the nine dead Designer-field declarations
(rows 2-10). The actual required-change surface is the single live `ItemViewer` type plus its seam
interface `IItemViewer`. The nine dead variants require **no change** and should be left untouched
(touching them adds churn and coverage-denominator risk without runtime effect). If the epic
requires them changed for consistency, that is a separate, optional cleanup decision to be surfaced
to the maintainer — it is not required to deliver #325.

Files that carry the live change:
- `QuickFiler/Viewers/IItemViewer.cs` (seam interface — add new intent members)
- `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` (forwarding implementations over `CboFolders`)
- `QuickFiler/Viewers/ItemViewer.Designer.cs` and/or `ItemViewer.cs` (owner-draw config on `CboFolders`)

---

## Q2. Data path: folder candidates -> array -> `CboFolders`

Current path (folder NAMES only; no probability crosses the seam):

1. **Scoring source** — `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`
   - `FolderPredictor.Suggestions` is a `FolderScorer` (`UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs`).
   - `FolderScorer` holds `ScoDictionaryNew<string, long> _folderNameScores` (folderPath -> integer score in 0-1000 units; `AddBayesianSuggestionsAsync` does `score = (long)Math.Round(prediction.Probability * 1000, 0)`).
   - `FolderScorer.ToArray(int topN)` returns folder path strings ordered by descending score — **scores are discarded at this boundary** (only the keys survive).
   - `FolderPredictor.FolderArray` (`FolderPredictor.cs:210`) composes the displayed list: header sentinels (`"========= SUGGESTIONS ========="`, `"======= SEARCH RESULTS ======="`, `"======= RECENT SELECTIONS ========"`), `Suggestions.ToArray(5)`, matches, and `_globals.AF.RecentsList`. Returns `string[]`.

2. **Controller builds/hands off the array** — `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`
   - `LoadFolderHandler` / `LoadFolderHandlerAsync` construct `_folderHandler` (a `FolderPredictor`) via `_folderPredictorFactory`.
   - `AssignFolderComboBox` (`FolderHandling.cs:161`) calls `_itemViewer.SetFolderItems(_folderHandler.FolderArray)` then selects the predetermined folder or index-1.
   - Static `PopulateAndSelectFolder(ComboBox, string[], string)` (`FolderHandling.cs:201`) is the pure legacy equivalent kept for existing tests.

3. **Second populate call site (search-driven)** — `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:164-178` `TextBoxSearch_TextChanged`:
   `_folderHandler.FindFolder(...)` -> `ClearFolderItems()` -> `SetFolderItems(folders)`.

4. **"Trash to Delete" injection** — `QuickFiler/Controllers/QfcItemController.MailActions.cs:204-218` adds the literal `"Trash to Delete"` via `SetFolderItems`/`FolderContains`.

5. **Seam** — `IItemViewer.SetFolderItems(string[])` / `ClearFolderItems()` / `FolderContains(string)` / `GetFolderItems()` (`IItemViewer.cs:80-88`), implemented in `ItemViewer.FolderSearch.cs:13-30` forwarding to `CboFolders.Items`.

**Where probability + hierarchy inject:** at the seam boundary between step 1 (scoring) and step 5
(the combo). The minimal-churn injection point is:
- Have `FolderPredictor` expose the 9001 contract (per-folder probability keyed by folder identity)
  in ADDITION to `FolderArray` (9001 owns this; see Q7).
- Add a NEW `IItemViewer` intent member (e.g. `SetFolderSuggestions(IReadOnlyList<FolderSuggestion>)`)
  that carries folder identity + probability, alongside the existing `SetFolderItems(string[])`
  (kept for the sentinel/recents/"Trash to Delete" paths and backward test compatibility).
- The presentation layer (host-neutral seams in Q8) builds the hierarchy and formats percentages.

Note: `EfcItemController` (`EfcItemController.cs`) also hosts an `ItemViewer` and reads
`_itemViewer.CboFolders.SelectedItem` (`EfcItemController.cs:590`), but its folder population runs
through the ListBox-based `EfcFormController.FolderListBox` path (see Sibling reference), not through
`ItemViewer.SetFolderItems`. The #325 change is scoped to the `ItemViewer.CboFolders` population path
driven by `QfcItemController`.

---

## Q3. Implementation options for tree expand/collapse on the `ComboBox` — RECOMMENDATION

### Recommended: (i) Owner-draw the existing `CboFolders` ComboBox + visible-row projection

Set `CboFolders.DrawMode = OwnerDrawFixed` (or `OwnerDrawVariable`) and bind the ComboBox `Items`
to a host-neutral projection of the currently-visible tree rows. Expand/collapse mutates a
host-neutral tree-state model (Q4) and re-projects the visible rows; the ComboBox is a dumb renderer.

- **Rendering:** `DrawItem` paints, per visible row: an indent proportional to `Depth`, a `+`/`−`
  glyph when the node has children (no glyph for leaves), the folder display name, and the
  right-aligned percentage.
- **Plus/minus click:** hit-test the glyph rectangle in the dropdown against the mouse X within the
  drawn item bounds; on hit, toggle that node in the state model and re-project.
- **Arrow keys:** already routed through `QuickFiler/Controllers/KeyboardHandler.cs`
  (`CboFolders_KeyDown` / `DdOpen_KeyDownAsync` / `DdClosed_KeyDownAsync`). Right/Left currently drive
  `DroppedDown` and the pop-out dialog; the new expand/collapse hooks in on the highlighted-row node.

**Feasibility:** High. There is a repo precedent for owner-drawing this exact control:
`QuickFiler/Viewers/QfcItemViewerLightSelected.cs:46` `CboFolders_DrawItem(object, DrawItemEventArgs)`
already uses `e.Graphics.FillRectangle` / `e.Graphics.DrawString` / `StringFormat` on `CboFolders`.

**Consistency with existing QuickFiler UI:** High. Keeps the ComboBox the rest of QuickFiler uses;
does not replace the Designer control; preserves the `IItemViewer.SetFolderItems(string[])` seam and
every existing controller call site and MSTest expectation
(`QfcItemController.FolderHandlingTests`, `MailActionsTests`, `EventHandlersTests` all
`Verify(v => v.SetFolderItems(It.IsAny<string[]>()))`).

**Testability:** High. All hierarchy/state/format logic lives in host-neutral seams (Q8);
only the `DrawItem` paint + glyph hit-test + `Items` rebind glue is WinForms-bound and exempt.

**Limitation:** A ComboBox dropdown has no native hierarchy, so expand/collapse is implemented as a
re-projection of the visible-row list (rebind `Items` on toggle). This is a deliberate design choice;
the tree-state model owns correctness and is fully unit-tested.

### Rejected alternatives (brief)

- **(ii) Replace `CboFolders` with a TreeView-in-dropdown or ObjectListView `TreeListView`**
  (`BrightIdeasSoftware` is already referenced by the project). Gives native expand/collapse, native
  glyphs, and native arrow-key handling, but breaks the `IItemViewer.SetFolderItems(string[])`
  contract and every controller call site plus existing MSTest expectations, requires swapping the
  Designer control on `ItemViewer`, and diverges from the ComboBox UX used elsewhere in QuickFiler.
  Higher churn and regression risk for marginal native-behavior benefit. Not selected.

---

## Q4. Tree state model and expand/collapse state-transition invariants (C3 driver)

A reusable, host-neutral tree-state component models one folder forest. Reuse the existing
`UtilitiesCS/ReusableTypeClasses/Other/TreeNodeOfT.cs` (`TreeNode<T>`), which already provides
`Parent`, `Children`, `Depth`, `AddChild`, `FindSequentialNode`, `Descendents`, `Traverse`, and
level traversal — no COM/WinForms dependency.

### Per-node fields
- `HasChildren : bool` — structural, fixed once the forest is built (`Children.Count > 0`).
- `Expanded : bool` — mutable; meaningful only when `HasChildren`.
- `Depth : int` — structural (`TreeNode<T>.Depth`).
- `Highlighted` — tree-level (at most one node), not stored per node necessarily.
- `Visible` — derived: a node is visible iff every ancestor is `Expanded` (root rows always visible).
- `Glyph` — derived from `(HasChildren, Expanded)`.

### Transitions
1. `Expand(node)` — pre: `HasChildren && !Expanded` -> `Expanded = true`. No-op otherwise.
2. `Collapse(node)` — pre: `HasChildren && Expanded` -> `Expanded = false`. No-op otherwise.
3. `Toggle(node)` — `Expand` if collapsed, `Collapse` if expanded; no-op for a leaf.
4. `Highlight(node)` — sets the single highlighted node; clears the previous highlight.
5. `RightArrow(highlighted)` — if `HasChildren && !Expanded` -> `Expand`; else no-op (leaf or already
   expanded). Matches "right arrow key expands it."
6. `LeftArrow(highlighted)` — if `HasChildren && Expanded` -> `Collapse`; else no-op. Matches "left
   arrow key collapses it."

### Invariants (precise)
- **INV1 (no leaf expansion):** `Expanded => HasChildren`. A leaf is never `Expanded`.
- **INV2 (visibility):** a node is `Visible` iff all ancestors are `Expanded`; roots are always visible.
- **INV3 (single highlight):** at most one node is `Highlighted` across the whole forest at any time.
- **INV4 (glyph bijection):** `Glyph = '+'` iff `HasChildren && !Expanded`; `Glyph = '-'` iff
  `HasChildren && Expanded`; no glyph iff `!HasChildren`. (Bijection with `(HasChildren, Expanded)`.)
- **INV5 (descendant-state preservation):** collapsing a node does not change the `Expanded` state of
  its descendants; it only changes their `Visible`-ness. Re-expanding restores the prior descendant
  expansion. Collapse/re-expand of a subtree is idempotent on descendant `Expanded` state.
- **INV6 (toggle involution):** for a node with children, `Toggle . Toggle = identity` on `Expanded`.
- **INV7 (indent monotonic):** the rendered indent of a row equals `node.Depth` (monotonic with hierarchy).
- **INV8 (stable order):** the visible/flattened row order is a stable pre-order DFS over expanded
  nodes; ordering among siblings is deterministic (reuse `FolderScorer`'s descending-score then
  ordinal-key tie-break where scores are the ordering key, otherwise ordinal by segment name).

These invariants are the C3 complexity driver and must be enforced and unit-tested exhaustively
(positive, negative/no-op, boundary at root and leaf, single-highlight enforcement, collapse/re-expand
round-trip).

---

## Q5. Hierarchy building (host-neutral)

Derive parent/child structure from the folder identity (folder full path) rather than any live
Outlook object.

- Outlook folder paths are backslash-delimited (`FolderPredictor.GetFolder` splits on `\`, and
  `GetFolder` strips a leading `\\`). Segment a folder identity by splitting the path on `\`.
- A `FolderHierarchyBuilder` (pure) takes the 9001 suggestions (`IReadOnlyList<FolderSuggestion>` of
  folder identity + probability) and produces a `TreeNode<FolderNodeViewModel>` forest:
  - For each suggestion, split its path into segments; walk/insert nodes using
    `TreeNode<T>.FindSequentialNode` (already present) or an explicit segment-by-segment
    find-or-add; attach the probability at the leaf node that represents the full folder.
  - Intermediate ancestor nodes that are not themselves suggestions get no probability (render blank
    percentage) but are `HasChildren` and expandable.
  - `FolderNodeViewModel.DisplayName` is the last path segment; the full path is retained as the
    node key / selection value so selection still yields the same string the controller expects.
- Sentinel header rows (`"===== ... ====="`) and recents/"Trash to Delete" are flat, non-hierarchical
  entries; treat them as depth-0 leaf rows with no probability so they render unchanged.
- Reuse `UtilitiesCS/OutlookObjects/Folder/FolderTreeNodeKey.cs` if a store-qualified identity is
  needed; for #325 the display path string is sufficient because selection is by path string today.

The builder is fully deterministic and unit-testable with in-memory path/probability inputs — no
Outlook, no filesystem.

---

## Q6. Percentage formatting (whole-number percent, right-aligned)

- **Probability domain (confirmed):** `Prediction<T>.Probability` is a `double` fraction in `[0,1]`
  (`UtilitiesCS/EmailIntelligence/Bayesian/Prediction.cs:27`). `FolderScorer.AddBayesianSuggestionsAsync`
  multiplies by 1000 (`prediction.Probability * 1000`), confirming the fraction domain. The PLANNED
  9001 contract therefore exposes a `[0,1]` fraction (see Q7). Formatting must clamp/guard to `[0,1]`.
- **Formatting rule:** `percent = (int)Math.Round(Math.Clamp(p, 0.0, 1.0) * 100.0, MidpointRounding.AwayFromZero)`,
  rendered as `percent.ToString() + "%"` (e.g., `0.4267 -> "43%"`, `1.0 -> "100%"`, `0.0 -> "0%"`).
  Rows with no probability (ancestors, sentinels, recents) render an empty percentage field.
  Extract this as a pure static `PercentageFormatter.Format(double) : string` seam (NOT exempt).
- **Right-alignment in the owner-draw ComboBox:** reserve a fixed-width right column in `DrawItem`;
  draw the percentage with `StringFormat { Alignment = StringAlignment.Far }` (or
  `TextFormatFlags.Right`) into a right-anchored rectangle computed from `e.Bounds.Right` minus the
  reserved width, and draw the folder name (with indent + glyph) in the remaining left region. The
  percentage string itself is produced by the host-neutral formatter; only the paint rectangle math
  is WinForms glue.

---

## Q7. Planned upstream 9001 contract shape (consumed, not implemented)

9001 (`folder-probability-plumbing`, placeholder issue 9001) **owns and introduces** a NEW public
contract that surfaces a per-folder prediction probability from the `FolderScorer` / `FolderPredictor`
scoring layer to the presentation layer. Today that probability is discarded at
`FolderScorer.ToArray(int)` (only folder-path keys survive). This feature CONSUMES that contract.

**Minimal consumed shape (planned):** a per-folder pairing of folder identity and its probability,
ordered as the suggestion list is ordered today. Proposed minimal surface:

```csharp
// OWNED / INTRODUCED BY 9001 — do not implement here; plan against this shape.
public readonly struct FolderSuggestion   // net48: readonly struct, explicit ctor (no record struct)
{
    public FolderSuggestion(string folderPath, double probability) { ... }
    public string FolderPath { get; }   // folder identity: rooted/relative Outlook folder path
    public double Probability { get; }  // [0,1] fraction
}

// Exposed from the scoring layer (FolderPredictor/FolderScorer), consumed by #325:
IReadOnlyList<FolderSuggestion> GetFolderSuggestions();  // or equivalent property
```

Consumption contract:
- This feature reads `FolderSuggestion.Probability` verbatim and formats it (Q6). It does **NOT**
  recompute scores, re-run the classifier, or touch `FolderScorer` scoring math.
- The exact member name/return type is 9001's decision; #325 plans against "folder identity +
  its probability" and adapts to the concrete member at epic execution time.
- Net48 constraint: no `record`/`record struct`/`init` (no `IsExternalInit` polyfill on net48) —
  the DTO must be a plain `class` or a `readonly struct` with an explicit constructor.
- At epic execution time, 9001 merges into the integration branch before #325 runs, so the contract
  is present.

**Dependency citation:** #325 depends on 9001 for the per-folder probability. Absent 9001, the
percentage column cannot be populated (current arrays are folder-name strings only, confirmed at
`ItemViewer.FolderSearch.cs:13` and `FolderScorer.ToArray`).

---

## Q8. Reusable, testable, host-neutral seam design

New host-neutral seams to extract (all **NOT** COM/WinForms-exempt; must meet coverage thresholds):

| Seam | Responsibility | Location (proposed) | Purity |
|------|----------------|---------------------|--------|
| `FolderSuggestion` (from 9001) | folder identity + probability DTO | UtilitiesCS (9001-owned) | value type |
| `FolderNodeViewModel` | per-node view model: FolderPath, DisplayName, Probability?, Depth, HasChildren, Expanded, Glyph, indent level, formatted percentage | `UtilitiesCS/OutlookObjects/Folder/` or `QuickFiler/Models/` | plain class/struct |
| `FolderHierarchyBuilder` | build `TreeNode<FolderNodeViewModel>` forest from `IReadOnlyList<FolderSuggestion>` by path-segment splitting; reuse `TreeNode<T>` | UtilitiesCS (host-neutral) | pure |
| `FolderTreeStateModel` | expand/collapse/highlight state machine + visible-row projection; enforces INV1-INV8 | UtilitiesCS or QuickFiler (host-neutral) | pure |
| `PercentageFormatter` | `double [0,1] -> "NN%"` with clamp/round | UtilitiesCS (host-neutral) | pure static |

Rendering/glue that STAYS host-bound (WinForms) and is exempt:
- `ItemViewer` (already `[ExcludeFromCodeCoverage]`, `ItemViewer.cs:20`) and all `*.Designer.cs`.
- The `CboFolders` owner-draw `DrawItem` paint + glyph hit-test + `Items` rebind in
  `ItemViewer.FolderSearch.cs` — thin WinForms glue; keep minimal.
- `KeyboardHandler` (`[ExcludeFromCodeCoverage]`, `KeyboardHandler.cs:22`) arrow-key routing glue —
  but the state transition it invokes (`FolderTreeStateModel.RightArrow/LeftArrow/Toggle`) is NOT
  exempt and must be unit-tested directly.

Exempt vs NOT-exempt classification (per CLAUDE.md COM/VSTO/WinForms exemption and
`.claude/rules/general-unit-test.md`):
- **Exempt:** `ItemViewer` + Designer files (WinForms form-derived / Designer-generated); the nine
  dead viewer variants; `KeyboardHandler` (Interop-bound glue).
- **NOT exempt (must meet coverage floor):** `FolderHierarchyBuilder`, `FolderTreeStateModel`,
  `PercentageFormatter`, `FolderNodeViewModel` — all host-neutral, mockable, deterministic.

**Coverage-threshold discrepancy (flagged for maintainer):**
- `CLAUDE.md` (embedded General Unit Test Policy) states repository line coverage `>= 80%`, new
  modules `>= 90%`.
- `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state **uniform**
  line `>= 85%` and branch `>= 75%` across tiers T1-T4.
- These conflict (85/75 in the rules files is stricter than 80 in CLAUDE.md). Recommendation: plan
  the new host-neutral seams to the **stricter** bar — line `>= 85%`, branch `>= 75%`, and the
  `>= 90%` new-module target from CLAUDE.md — and surface the discrepancy to the maintainer for
  ratification. Per CLAUDE.md policy-compliance order, CLAUDE.md is authority #1; the stricter rules
  numbers do not relax it, so satisfying 90% new-module + 85/75 satisfies both.

Test tooling (per CUT1/CUT2): MSTest (`[TestClass]`/`[TestMethod]`), Moq for the `IItemViewer` seam,
FluentAssertions for assertions. New test files require explicit `<Compile Include>` entries — both
`QuickFiler.Test.csproj` and `UtilitiesCS.Test.csproj` are legacy non-SDK `net4.8.1` projects (no
glob compile). Follow the existing `Controllers\QfcItemController.*Tests.cs` include pattern.

---

## Q9. Coverage / toolchain posture

- **In-scope production projects:** `UtilitiesCS` (host-neutral seams: hierarchy builder, tree-state
  model, percentage formatter, view model, and the 9001-owned DTO consumption point), `QuickFiler`
  (`IItemViewer`, `ItemViewer.FolderSearch.cs`, `ItemViewer` Designer owner-draw config,
  `QfcItemController.*` injection).
- **In-scope test assemblies:** `UtilitiesCS.Test` (host-neutral seam tests) and `QuickFiler.Test`
  (controller-injection tests via `Mock<IItemViewer>`; existing `SetFolderItems(string[])`
  expectations must remain green). Both are non-SDK `net4.8.1`; add `<Compile Include>` entries.
- **C# toolchain order (CLAUDE.md CUT3):**
  1. `dotnet tool run csharpier .` (or `csharpier .`)
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
  Restart from step 1 on any failure or auto-fix.
- **Coverage config:** `coverage.config` excludes only third-party/mixed-mode modules (Deedle,
  FSharp, Castle.Core, FluentAssertions, Moq, MSTest, Microsoft.Testing). It does NOT exclude any
  first-party production path; the new host-neutral seams will be in the coverage denominator, as
  required.

---

## Q10. Explicit non-interference with the 9004 inline-image `cid:` bugfix

9004 is confined to `MailItemHelper.Html.cs` and WebView2 body-render wiring. #325 touches only the
folder-dropdown population/rendering path and shares **no files** with 9004.

**Body-render / WebView2 files that MUST NOT be touched by #325:**
- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs`  (9004's file)
- `QuickFiler/Viewers/ItemViewer.WebViewThread.cs`
- `QuickFiler/Viewers/WebView2CoreInitializer.cs`
- `QuickFiler/Viewers/IWebViewCoreInitializer.cs`
- The `NavigateToString` / WebView2 members on `IItemViewer` (`IItemViewer.cs:101-102`) and the
  WebView2 initialization members on `ItemViewer.cs` / `ItemViewer.Designer.cs`.

#325's file set (Q1, Q8) is disjoint from the above. Confirmed: no overlap.

---

## Automation Feasibility

This is a WinForms/COM Outlook add-in. Assessment of whether any preparation/implementation step
requires human interaction with the Outlook desktop UI:

- **Implementation logic** — the folder hierarchy builder, the expand/collapse/highlight tree-state
  machine, the percentage formatter, and the per-node view-model projection are all host-neutral and
  drivable from in-memory `FolderSuggestion` inputs (folder path + probability). No live Outlook, no
  `Microsoft.Office.Interop.Outlook` object, no filesystem.
- **Testing** — MSTest + Moq + FluentAssertions can cover every seam without a live Outlook process.
  The `IItemViewer` seam is mockable; controller-injection tests reuse the existing
  `Mock<IItemViewer>` pattern in `QuickFiler.Test/Controllers/QfcItemController.*Tests.cs`.
- **Host-bound glue** — the `CboFolders` owner-draw paint/hit-test and the Designer control config on
  `ItemViewer` are WinForms and carry the COM/VSTO/WinForms coverage exemption
  (`ItemViewer` is `[ExcludeFromCodeCoverage]`; Designer-generated code is exempt). These require no
  Outlook process to compile or to run the automated gates.
- **Human interaction** — **NOT REQUIRED** for preparation, implementation, or the automated
  toolchain/coverage gates. An optional manual visual confirmation in the live add-in (correct
  glyphs, indentation, and right-aligned percentages in the actual dropdown) is advisable as a final
  UX check but is not a gate and is not needed to satisfy the repository's automated policies.

**Human-interaction assessment:** No third-party-UI (Outlook desktop) interaction is required for
any tracked step. Implementation and testing proceed entirely through host-neutral seams and MSTest.

---

## Requirements mapping (summary)

| Acceptance criterion | Design element |
|----------------------|----------------|
| Folders with subfolders render as expandable tree nodes | `FolderHierarchyBuilder` (path-segment forest) + owner-draw indent + INV1/INV7 |
| Plus affordance left of expandable folder; click plus->expand, click minus->collapse | `Glyph` bijection INV4 + `DrawItem` glyph hit-test -> `FolderTreeStateModel.Toggle` |
| Highlighted node: Right expands, Left collapses | `KeyboardHandler` arrow routing -> `FolderTreeStateModel.RightArrow/LeftArrow` (transitions 5-6) |
| Probability printed right-aligned, whole-number percent, no decimals | `PercentageFormatter.Format` (Q6) + right-anchored draw rectangle |
| Consume upstream probability, do not recompute | `FolderSuggestion` (9001 contract, Q7) read verbatim |
| Parity with EfcViewer folder-list (9002) | Same host-neutral seams; note 9002 is parallel/independent and shares no base class (`ListBox` vs `ComboBox`) |

## Testing implications (strategy, no test code)

- **Unit (host-neutral, NOT exempt):** `FolderHierarchyBuilder` (path splitting, ancestor synthesis,
  probability placement, sentinel/flat rows, ordering/tie-break); `FolderTreeStateModel` (INV1-INV8:
  no-leaf-expansion, visibility, single-highlight, glyph bijection, descendant-state preservation,
  toggle involution, arrow-key transitions incl. no-ops at root/leaf, collapse/re-expand round-trip);
  `PercentageFormatter` (0, 1, midpoint rounding away-from-zero, clamp of out-of-range, empty for
  no-probability). Cover positive, negative/no-op, boundary, and state-transition scenarios per UT2.
- **Controller-injection (QuickFiler.Test):** verify `QfcItemController` hands suggestions
  (identity+probability) to the new `IItemViewer` intent member and that existing
  `SetFolderItems(string[])` behavior (sentinels, "Trash to Delete", index-1 selection,
  predetermined-folder selection) remains green via `Mock<IItemViewer>`.
- **Determinism:** all inputs in-memory; no clock/RNG/timer usage; no temp files (prohibited).
- **Exempt (no unit tests):** `ItemViewer` Designer/owner-draw glue, `KeyboardHandler` routing glue.
