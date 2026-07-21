# Research — efcviewer-folder-tree-percentage (issue #327)

- **Issue:** #327
- **Epic:** folder-tree-percentage-ui
- **Branch:** feature/efcviewer-folder-tree-percentage-327 (cut from epic integration)
- **Author:** task-researcher
- **Timestamp:** 2026-07-15T17-15
- **Scope:** research only; no source/config changes

## 1. Current State Analysis (verified)

### 1.1 The control is a flat WinForms ListBox in both viewers

Both viewers declare `FolderListBox` as `System.Windows.Forms.ListBox`, confirmed in the Designer files:

- `QuickFiler/Viewers/EfcViewer.Designer.cs:50` (`new System.Windows.Forms.ListBox()`), field declared at line 4250, added to TLP at cell (1,4) line 126, column span 14 (line 882), `Dock=Fill`, font `Microsoft Sans Serif` 10.125pt (proportional), `ItemHeight=31` (lines 883-891). `DrawMode` is not set, so it defaults to `Normal` (text-only, no owner draw).
- `QuickFiler/Viewers/EfcViewer3.Designer.cs:39` / field at line 495, TLP cell (2,5) line 86, `ItemHeight=37`, font default.

The font is proportional, so column alignment by string padding is not viable; right-alignment requires either an owner-draw control or a column-capable control (see §2).

### 1.2 Three DataSource binding sites (all bind a `string[]`)

`QuickFiler/Controllers/EfcFormController.cs`:

- Line 551-557 — `SearchText_TextChanged`: `_formViewer.FolderListBox.DataSource = _dataModel.FindMatches(_formViewer.SearchText.Text);` then selects index 1 when count > 1.
- Line 799-803 — `RefreshSuggestionsAsync`: `_formViewer.FolderListBox.DataSource = matches;` (matches = `_dataModel.FindMatches(...)`), selects index 1 when count > 0.
- Line 961-965 — `PopulateFolderCombobox`: `formViewer.FolderListBox.DataSource = _dataModel.FolderHelper.FolderArray;` selects index 1 when count > 0.

There is also `ActionDeleteAsync` (line 737-744) which reads `(string[])_formViewer.FolderListBox.DataSource`, inserts `"Trash to Delete"` at index 0, and rebinds. This is a fourth mutation site that must survive the redesign.

### 1.3 What the `string[]` contains

`FindMatches` (EfcDataModel.cs:374-387) wraps the search text in `*...*` and calls `_folderHelper.FindFolder(...)`. `FolderHelper` is a `FolderPredictor` (EfcDataModel.cs:168-177). `FolderPredictor.FindFolder` (FolderPredictor.cs:256-306) assembles `_folderList` from three ordered sections, each preceded by a banner row:

- `AddMatches` (FolderPredictor.cs:688-696): banner `"======= SEARCH RESULTS ======="` then matching folder **stems** (relative to archive root, back-slash separated), ordered alphabetically.
- `AddSuggestions` (FolderPredictor.cs:698-702): banner `"========= SUGGESTIONS ========="` then `Suggestions.ToArray(5)` (top-5 scored folder paths).
- `AddRecents` (FolderPredictor.cs:679-686): banner `"======= RECENT SELECTIONS ========"` then `_globals.AF.RecentsList`.

`FolderArray` (FolderPredictor.cs:210-225) returns `_folderList.ToArray()`; when empty it lazily rebuilds from suggestions + recents (no search-results banner). Paths are full/relative folder path strings using `\` as the separator (`GetOlSubpath`, FolderPredictor.cs:773-791). Note: the item set is **not a complete folder tree** — it is a filtered, sectioned list, so any hierarchy must be derived from the presented strings themselves (see §4).

### 1.4 Separator / banner rows

The banner rows all begin with `====`. Two consumers depend on this:

- `ActionOkAsync` (EfcFormController.cs:703): `((string)SelectedItem).StartsWith("====")` blocks OK on a banner row.
- `IsValidSelection` (EfcFormController.cs:968-980): rejects null/empty/`len<3`/`Substring(0,3)=="==="`.

Any redesign must preserve "banner rows are non-selectable / not a valid filing target," and `SelectedFolder`/`IsValidSelection` must keep returning the full path string for real folders. `SelectedFolder` currently is `FolderListBox.SelectedItem as string` (line 278-281).

### 1.5 Selection and JumpTo

- `SelectedFolder` reads `SelectedItem as string`.
- `'F'` keyboard action calls `JumpToAsync(_formViewer.FolderListBox)` → `control.Focus()` (EfcFormController.cs:580, 822-827).
- `SearchText_DownArrow` (line 390-396): Down in the search box selects the FolderListBox.

### 1.6 KeyDown handler — Left/Right are already stubbed for this feature

`FolderListBox_KeyDown` (EfcFormController.cs:398-406), wired at line 379:

```
if (e.KeyCode == Keys.Up && SelectedIndex == 0) { SearchText.Select(); }
else if (e.KeyCode == Keys.Left) { }    // empty placeholder
else if (e.KeyCode == Keys.Right) { }   // empty placeholder
```

The empty `Left`/`Right` branches are the intended integration points for collapse/expand. All controls also get a global `PreviewKeyDown`/`KeyDown` via `KeyboardHandler` (lines 361-372); arrow keys must be routed so the tree receives Left/Right (a single-column ListBox does not consume Left/Right for navigation, so `KeyDown` currently fires for them).

### 1.7 Every place that must change to add hierarchy + percentage

1. Both Designer files (control type / draw mode) — Designer-generated, coverage-exempt but manually edited.
2. `EfcFormController` binding sites 551, 799, 961 and the delete-rebind at 737-744 (bind projected rows instead of `string[]`).
3. `SelectedFolder` (278-281) and `IsValidSelection` (968-980) — derive the path from the selected node/row.
4. `ActionOkAsync` banner guard (703).
5. `FolderListBox_KeyDown` (398-406) — fill Left/Right branches.
6. A new host-neutral model consuming the probability contract (§3, §5).
7. `EfcViewer3.cs` currently lacks `[ExcludeFromCodeCoverage]` (see §7) — must be attributed if UI code is added.

## 2. Control Approach (evaluated)

The repository already contains a proven, tested pattern for folder-tree UI. `FilterOlFoldersController` (UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs) uses `BrightIdeasSoftware` **TreeListView** with an injectable viewer interface (`IFilterOlFoldersViewer`), a host-neutral model (`FolderTreeCompatibilityView`, with `Roots`/`Children`/`FlattenIf`), and STA unit tests via fakes (`UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersController_Tests.cs`, `FilterOlFoldersViewer_Tests.cs`). Critically, **QuickFiler already references the library**: `QuickFiler/QuickFiler.csproj:93-94` references `ObjectListView 2.9.1` (`packages\ObjectListView.Official.2.9.1\lib\net20\ObjectListView.dll`). The `TaskTree` project shows a second in-repo tree pattern (`TaskTree/TaskTreeController.cs`, tested in `TaskTree.Test`).

### Recommendation: BrightIdeasSoftware TreeListView + host-neutral hierarchy/percentage model

Bind both `FolderListBox` cells' replacement to a `TreeListView`. The TreeListView natively provides the three explicitly required affordances, which means they are supplied by tested library code rather than reimplemented:

- Expandable nodes with plus/minus glyphs via `CanExpandGetter` + `ChildrenGetter`.
- Mouse click on plus/minus expand/collapse — native.
- Right-arrow expand / left-arrow collapse on the highlighted node — native `TreeListView` keyboard behavior.
- Right-aligned percentage as a **second `OLVColumn` with `TextAlign = HorizontalAlignment.Right`** and an `AspectToStringConverter` — native column alignment, no owner-draw geometry math.

The custom, feature-specific logic (build hierarchy from the flat sectioned `string[]`, project children, format the percentage, keep banner rows non-expandable/non-selectable) is factored into a **host-neutral, testable model** shared by both viewers. This is the primary testable seam (§3, §7).

Rationale against repo principles: CLAUDE.md §7.1 ("match existing style") and General Code Change Policy "Simplicity first" both favor reusing the already-referenced, already-tested TreeListView pattern over hand-rolling tree keyboard/glyph/hit-testing code. The percentage requirement forces column/owner-draw work regardless; TreeListView gives a right-aligned column for free.

Impact on the two viewers: identical shape. Both replace the `ListBox` field with a `TreeListView` in their Designer, and both are fed by the same shared model instance the controller builds. `EfcFormController` is shared by both viewers (it is typed to `EfcViewer` today; `EfcViewer3` is a parallel form). The shared model minimizes duplication; only thin per-viewer/per-Designer wiring differs.

Delivery split (shared host-neutral vs per-viewer wiring):

- Shared host-neutral (testable, non-exempt): hierarchy builder from `string[]`, node model, visible/children projection, expand/collapse state transitions, percentage formatter, banner-row classification, contract adapter (path→probability).
- Per-viewer wiring (WinForms, exempt): TreeListView column setup, `CanExpandGetter`/`ChildrenGetter`/`AspectGetter` delegates, `SetObjects` calls at the 3 binding sites, `SelectedObject`→path mapping, and Left/Right/Up key wiring in `FolderListBox_KeyDown`.

### Rejected alternatives (brief)

- **Native WinForms `TreeView`.** Gives native hierarchy, glyphs, and Left/Right expand/collapse, but has **no native column model**, so the right-aligned percentage still requires `DrawMode.OwnerDrawText` + `TextRenderer` geometry. More custom draw code than TreeListView and not the pattern the repo already uses/tests.
- **Owner-draw `ListBox` + custom state model.** Keeps the control type (smallest Designer change) but requires reimplementing expand/collapse state, plus/minus hit-testing in `MouseDown`, Left/Right keyboard handling, indentation, and owner-draw right-alignment — i.e., re-creating TreeListView behavior by hand. Highest custom-code surface for a worse result; conflicts with "Simplicity first." Retained only as a fallback if adding a TreeListView to these Designers proves infeasible.

## 3. Tree State Model (primary testable seam)

Propose a host-neutral class (e.g. `FolderSuggestionTree` / `FolderSuggestionNode`) placed in a non-exempt location. Two viable homes: `UtilitiesCS` (maximizes reuse; sibling to `FolderPredictor`/`FolderScorer`; covered by `UtilitiesCS.Test`) or `QuickFiler\Helper Classes` (namespace `QuickFiler.Helper_Classes`, covered by `QuickFiler.Test`). Recommend `UtilitiesCS` for reuse and because the upstream contract also lives there.

Node shape (pure data + behavior, no WinForms/COM types):

- `FullPath` (string), `DisplayName` (leaf segment), `Depth` (int), `Children` (ordered list), `HasChildren` (bool), `IsExpanded` (bool), `Probability` (double? — null when the row carries no probability), `Kind` (enum: `Folder`, `Banner`).

Projection: `VisibleRows()` performs a pre-order flatten that emits a node's children only when `IsExpanded == true`. Banner rows are always emitted in their section order and are never expandable/selectable.

State transitions (all pure, unit-testable):

- `Expand(node)`: if `HasChildren && !IsExpanded` → set expanded; else no-op.
- `Collapse(node)`: if `IsExpanded` → set collapsed (recursively hide descendants via projection); else no-op.
- `Toggle(node)`: plus/minus mouse click → expand if collapsed, collapse if expanded.
- `RightArrow(selected)`: expand if `HasChildren && !IsExpanded`; on an already-expanded node, no-op (or move-to-first-child if the plan chooses the standard tree convention — requirement only mandates expand).
- `LeftArrow(selected)`: collapse if `IsExpanded`; on a collapsed/leaf node, no-op (or move-to-parent per standard tree convention — requirement only mandates collapse).

Edge cases to cover in tests: highlighted **leaf** node (Right/Left = no-op on that node); already-expanded (Right no-op) / already-collapsed (Left no-op); **root** nodes (no parent to move to); banner row highlighted (Left/Right = no-op, not selectable); empty list; single node.

## 4. Hierarchy Building from flat full-path strings

Input is the sectioned `string[]` from §1.3 (`\` separator; verified in `GetOlSubpath` and `GetFolder` split on `@"\"`, FolderPredictor.cs:322, 773-791). The presented set is filtered, so ancestors are not guaranteed present. Proposed deterministic build:

1. Partition the array into sections on banner rows (`StartsWith("====")`), preserving order. Banners become non-expandable `Banner` nodes / section headers.
2. Within a section, establish parent/child edges by prefix: path `Y` is a child of presented path `X` iff `Y` starts with `X + "\"` and `X` is the longest such presented prefix of `Y`. A path whose parent prefix is not present in the section is a **root** at that section.
3. A node `HasChildren` iff at least one other presented path in the section is its child by rule (2). This satisfies "render folders that contain subfolders as expandable tree nodes" using only the presented data.
4. Ordering: preserve the existing per-section order (search results already alphabetical via `AddMatches`; suggestions already score-ordered via `ToArray(5)`). Do not re-sort suggestions.

Decision to record in the plan: whether to synthesize intermediate ancestor nodes when a deep path appears without its parent (e.g. only `A\B\C` present). Simplest correct behavior consistent with the requirement: do **not** synthesize; show such a node at its section root with its full/relative path. Synthesis can be added later if UX requires it.

## 5. Percentage Formatting and Upstream Contract

### 5.1 Formatting rule

Whole-number percent, no decimals: `Math.Round(probability * 100, MidpointRounding.AwayFromZero)` rendered as an integer + `"%"` (e.g. `0.732 → "73%"`). Right-alignment is delivered by the TreeListView percentage column (`TextAlign = HorizontalAlignment.Right`), not by string padding (the proportional font rules out padding). A pure `FormatPercent(double) : string` helper is the testable unit; rows with `Probability == null` (banners, recents, search matches with no score) render an empty percentage cell.

### 5.2 Upstream contract assumption (folder-probability-plumbing, epic placeholder 9001)

Evidence for the realistic shape:

- `FolderScorer._folderNameScores` is `ScoDictionaryNew<string, long>` — path → integer **score**, not a probability (FolderScorer.cs:27, 195-199). Scores are heterogeneous across sources: Bayesian entries store `Math.Round(prediction.Probability * 1000)` (FolderScorer.cs:176-177), while conversation and word-sequence entries store much larger weighted integers (FolderScorer.cs:270-277, 350-356). So the current score is **not** a normalized probability.
- The genuine probability exists upstream as `Prediction<string>.Probability`, a `double` in `[0,1]` (`UtilitiesCS/EmailIntelligence/Bayesian/Prediction.cs:26-31`), produced by the classifier in `AddBayesianSuggestionsAsync` (FolderScorer.cs:152-179) and currently discarded when `ToArray`/`FolderArray` drop the value and keep only the key (FolderScorer.cs:242-255, 210-225).

**Assumption (state explicitly in the plan):** feature #327 consumes, from the upstream contract, a mapping of **folder identity (full folder-path string) → prediction probability (`double` in `[0,1]`)**, most plausibly surfaced as either a keyed lookup or an ordered `IReadOnlyList<(string FolderPath, double Probability)>` / `Prediction<string>[]`. This feature does **not** compute or normalize scores. The presentation layer joins the probability to each row **by full-path string equality** with the existing `FolderArray`/`FindMatches` output, and formats per §5.1. Rows with no upstream probability (banners, recents, unscored search matches) render a blank percentage. If upstream instead exposes an already-scaled percentage, only `FormatPercent` changes (drop the `*100`). This assumption is the only coupling to 9001 and should be re-confirmed once 9001 merges into the integration branch first (per spec.md:64-65).

## 6. Keyboard Integration

- Keep the existing `Up`-at-index-0 → `SearchText.Select()` behavior (EfcFormController.cs:400-403). With a TreeListView, "index 0" becomes "first visible row"; map accordingly.
- Fill the empty `Left`/`Right` branches (lines 404-405) by delegating to the model transitions in §3, or rely on TreeListView native Left/Right and only add app-specific behavior (e.g. Left on a collapsed root could still move focus). Recommend: let the control handle expand/collapse natively and keep `FolderListBox_KeyDown` for the search-box hand-off only, to avoid double-handling.
- The global `PreviewKeyDown`/`KeyDown` wiring (lines 361-372) already forwards keys to `KeyboardHandler`; verify arrow keys are not swallowed by an accelerator handler before the tree sees them (the `'F'` jump and Alt-accelerator path in `ProcessCmdKey`, EfcViewer.cs:88-99 / EfcViewer3.cs:73-84, only trigger on Alt).

## 7. Testability and Coverage

### Coverage policy (CLAUDE.md, authoritative per delegation)

CLAUDE.md "General Unit Test Policy" (UT2) sets repository-wide line coverage `>= 80%` on the **testable denominator**, `>= 90%` for any new modules/classes/methods, and "code changes must not reduce coverage for changed lines." The **COM/VSTO/WinForms exemption** (UT2 bullet, ratified in `feature/csharp-coverage-uplift`) exempts, via `[ExcludeFromCodeCoverage]`: (b) WinForms form-derived and Designer-generated classes; (c) Outlook Interop event-handler classes in `QuickFiler` that depend on `Application`/`MailItem`/`Store`/`MAPIFolder` without an injectable seam. Testable seams are explicitly NOT exempt.

(Note: `.claude/rules/general-unit-test.md` states a stricter uniform `>= 85%` line / `>= 75%` branch and a "no production file excluded" stance. To satisfy both the CLAUDE.md floor and the stricter rules-file thresholds, target `>= 90%` on the new host-neutral code and rely on `[ExcludeFromCodeCoverage]` for the Form-derived UI only.)

### Classification for this feature

- **Exempt (WinForms form-derived):** `EfcViewer` (`EfcViewer.cs:20` already `[ExcludeFromCodeCoverage]`) and `EfcViewer3`. **Finding:** `EfcViewer3.cs` currently has **no** `[ExcludeFromCodeCoverage]` attribute (verified: no match in the file). Adding tree/column wiring to `EfcViewer3` would pull it into the testable denominator unless the attribute is added. The plan must add `[ExcludeFromCodeCoverage]` to `EfcViewer3`.
- **Exempt (COM-bound controller):** `EfcFormController` is `[ExcludeFromCodeCoverage]` (line 26); the TreeListView wiring lives here and stays exempt.
- **Non-exempt (must meet `>= 90%`):** the new host-neutral model — hierarchy builder (§4), node/state machine and visible-row projection (§3), percentage formatter (§5.1), banner classification, and the path→probability adapter (§5.2). These have no WinForms/COM dependency and are the coverage-bearing deliverable.

### Existing tests and seams (§8 detail)

- `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` is minimal: it constructs the controller via reflection on the **private no-arg constructor** (`CreateMinimalController`, lines 18-28) to exercise method guards with all fields null and no live COM (the only current test is the `PopulateFolderCombobox` null-viewer guard, lines 34-53). There is **no viewer fake/interface seam** for `EfcViewer`/`EfcViewer3` today — the controller is typed to the concrete `EfcViewer`. New tree/percentage logic should therefore be tested through the standalone host-neutral model, not through the controller.
- `QuickFiler.Test/Controllers/EfcDataModelTests.cs` demonstrates the established Moq/FluentAssertions/MSTest style for this area (strict Outlook mocks: `MailItem`, `Folder`, `Conversation`, `Table`), useful if a test needs to feed representative `FolderArray`/`FindMatches` output.
- Precedent for the recommended control: `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersController_Tests.cs` shows how a TreeListView-backed controller is tested with an injected fake viewer and a host-neutral tree model under `[STATestMethod]`; `FolderTreeCompatibilityViewTests.cs` shows a host-neutral tree model tested directly. These are the templates for #327's tests.
- `FolderPredictorTests.cs` / `FolderPredictorCoverageExpansionTests.cs` cover `FindFolder`/`FolderArray` and are the reference for the shape of the `string[]` the model must parse.

### Test strategy (no test code written here)

- Unit (MSTest + Moq + FluentAssertions, host-neutral): hierarchy building from representative sectioned `string[]` (roots, nested children, deep-path-without-parent, banners); expand/collapse transitions and visible-row projection for all §3 edge cases; percentage formatting (0, 1, rounding at .5 away-from-zero, null probability → blank); path→probability join by full-path equality including unmatched rows.
- Determinism: pure in-memory model; no clock, RNG, filesystem, or COM — satisfies General Unit Test Policy determinism/no-temp-file rules.
- WinForms/COM wiring in the two Forms and the controller is exempt and verified by build (msbuild) + manual QA, not unit tests.

## Automation Feasibility

Preparation and later automated execution are **fully automatable**; no step in delivering or verifying the testable portion requires human interaction or a live Outlook/COM process.

- **Build/format/lint/type-check** run headless: `csharpier .`, then `msbuild TaskMaster.sln` (analyzers + nullable). QuickFiler is a .NET Framework/VSTO project referencing Outlook interop and `ObjectListView`, all resolvable from `packages\` (verified in `QuickFiler.csproj`); it compiles without a running Outlook.
- **Unit tests** run headless via `vstest.console.exe` with Moq mocks; the new coverage-bearing logic is the host-neutral model (§3–§5), which has zero COM/WinForms dependency. The TreeListView-backed pattern is already unit-tested headlessly in `FilterOlFoldersController_Tests` using `[STATestMethod]` and fake viewers, so even control-adjacent tests are automatable.
- **Coverage-exemption vs autonomous blocker (distinct):** `EfcViewer`, `EfcViewer3`, Designer files, and `EfcFormController` are COM/VSTO/WinForms-bound and coverage-**exempt** (untestable in isolation) — this is a coverage classification, **not** an autonomous-execution blocker. They still compile and are exercised by the headless toolchain.
- **The only human/live-Outlook element** is end-to-end manual QA of the running add-in (visually confirming plus/minus glyphs, mouse expand/collapse, arrow-key behavior, and the right-aligned percentage against real suggestions). That is standard manual UI verification for a VSTO add-in and a coverage-exemption/manual-QA matter — it does **not** block autonomous preparation, implementation, or the automated toolchain and unit tests.
- **External dependency:** the upstream `folder-probability-plumbing` (9001) contract. #327 plans against the assumed shape in §5.2 and re-confirms after 9001 merges to the integration branch first; this is a sequencing dependency, not an automation blocker.

## Key File References

- `QuickFiler/Controllers/EfcFormController.cs` — binding 551/799/961, delete-rebind 737-744, `SelectedFolder` 278-281, `IsValidSelection` 968-980, banner guard 703, KeyDown 398-406, wiring 361-388.
- `QuickFiler/Viewers/EfcViewer.cs` (`[ExcludeFromCodeCoverage]` :20) and `QuickFiler/Viewers/EfcViewer3.cs` (**missing** the attribute).
- `QuickFiler/Viewers/EfcViewer.Designer.cs:50,882-891,4250`; `EfcViewer3.Designer.cs:39,225-235,495`.
- `QuickFiler/Controllers/EfcDataModel.cs:168-212,374-393` (FolderHelper=FolderPredictor, FindMatches).
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:210-225,256-306,679-702,773-791` (FolderArray/FindFolder/sections).
- `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs:27,152-179,195-199,242-255` (scores, Bayesian probability source).
- `UtilitiesCS/EmailIntelligence/Bayesian/Prediction.cs:26-31` (`double Probability` in [0,1]).
- `QuickFiler/QuickFiler.csproj:93-94` (ObjectListView 2.9.1 already referenced).
- `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs` + `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersController_Tests.cs` + `UtilitiesCS/OutlookObjects/Folder/FolderTreeCompatibilityView.cs` (TreeListView + injectable-viewer + host-neutral-model + STA-test precedent).
- `QuickFiler.Test/Controllers/EfcFormControllerTests.cs`, `EfcDataModelTests.cs` (existing test style / seams).
