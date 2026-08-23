# [P5-T6] Scope and Additivity Guard

- **Issue:** #438
- **Task:** [P5-T6]
- **Timestamp:** 2026-08-08T11-41
- **Baseline HEAD:** `904b4c38dba0f9f41707c3c0f077e123c78de59c`

## Command 1 — full change inventory

`pwsh -NoProfile -Command "git diff --name-only ; git ls-files --others --exclude-standard -- '*.cs' ; exit $LASTEXITCODE"`

- **EXIT_CODE:** 0

### Modified tracked files (26 total; 4 agent-memory + 1 promotion-lifecycle deletion are pre-existing)

| File | Category |
|---|---|
| `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | production — the behavior flip |
| `QuickFiler/Viewers/IItemViewer.cs` | production — additive interface member |
| `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs` | production — additive interface overload |
| `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | production — thin forwarding |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | production — non-focusing viewer path |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | production — `partial` token |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | production — the latch |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | production — `partial` token, body relocated |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | production — `partial` token, flag threaded |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | production — `partial` token |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | production — `partial` token |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` | production — `partial` token |
| `QuickFiler/QuickFiler.csproj`, `UtilitiesCS/UtilitiesCS.csproj` | `<Compile Include>` wiring |
| `QuickFiler.Test/QuickFiler.Test.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | `<Compile Include>` wiring |
| `QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs` | test — D4 single method rewrite |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` | test — D3 one-token `partial` |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` | test — D7 additive fake member |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | test — D7 additive fake member |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | test — D7 additive fake member |
| `.claude/agent-memory/*` (4 files) | pre-existing, allowlisted |
| `docs/features/potential/promoted/2026-08-07-...md` (deleted) | pre-existing promotion-lifecycle deletion |

The 12 production files touched are exactly the 12 estimated by research §3 Option 3 and named in the spec's Implementation strategy.

### New untracked `.cs` files (12)

```
QuickFiler.Test/Controllers/QfcItemController.SearchFocusRegressionTests.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.cs
QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs
QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs
QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs
UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionHighlightTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterReplaceItemsTests.cs
UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Highlight.cs
UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.SearchPresentation.cs
```

Exactly the 12 files the plan's P6-T2 audit requires. No stray file.

## Command 2 — AC-13 zero-diff proof (EfcViewer search path)

`pwsh -NoProfile -Command "git diff -- QuickFiler/Controllers/EfcFormController.cs QuickFiler/Controllers/BreadcrumbBridgeRouter.cs ; exit $LASTEXITCODE"`

- **EXIT_CODE:** 0
- **Output:** empty (`git diff --stat` produced no rows)

Neither file appears anywhere in the change inventory. `EfcFormController.SearchText_TextChanged`, `BindFolderRows`, `BindBreadcrumbRowsAsync`, and `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` are byte-unmodified. **AC-13 satisfied.**

## AC-10 — interface diffs are additive only

`git diff -U0 -- QuickFiler/Viewers/IItemViewer.cs QuickFiler/Viewers/IBreadcrumbDropDownHost.cs`, filtered for removed lines (`^-` excluding the `---` header):

- **Removed/altered lines: 0**

Both interface diffs consist exclusively of added lines. Concretely:

- `IItemViewer` gains exactly one member: `void PresentFolderSearchResults(string[] items);`
- `IBreadcrumbDropDownHost` gains exactly one overload: `Task<bool> OpenAsync(Rectangle, Rectangle, Size, bool takeFocus);` — the pre-existing 3-parameter member is unchanged and delegates with `takeFocus: true`.

No existing signature was removed or altered. **AC-10 satisfied**, corroborated by `ItemViewerBreadcrumbDropDownContractTests` passing unmodified (P4-T5).

## AC-7 — explicitly unchanged behavior

### `TextBoxSearch_KeyDown` byte-identical

The method body was extracted from `git show HEAD:QuickFiler/Controllers/QfcItemController.EventHandlers.cs` and from the working copy and compared with an ordinal, case-sensitive comparison after newline normalization:

```
KEYDOWN_IDENTICAL (chars=338)
```

The Down-arrow handler still issues both `SetFolderDroppedDown(true)` and `FocusFolderDropDown()`. The only occurrence of the token `TextBoxSearch_KeyDown` in the diff is inside an added explanatory comment on `TextBoxSearch_TextChanged`.

### `AssignFolderComboBox` suggestions-path selection unchanged

`git diff --stat -- QuickFiler/Controllers/QfcItemController.FolderHandling.cs` produced **no rows** — the file is byte-unmodified. The call at `QfcItemController.FolderHandling.cs:202-204` remains:

```csharp
_itemViewer.SetFolderSelectedIndex(
    _folderHandler.FolderArray.Length == 1 ? 0 : 1
);
```

## AC-11 — test-modification inventory

### The single sanctioned test-method rewrite

`QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs`. Diff hunk ranges against the original file:

```
@@ -311,0 +312,18 @@
@@ -314   +332   @@
@@ -316,0 +335,2 @@
@@ -331   +351,15 @@
@@ -345,5 +379,10 @@
```

Every hunk falls inside original lines **311-350** — the doc comment (`:308-312`) and body (`:313-350`) of `TextBoxSearch_TextChanged_UsesInjectedFolderSearchHandler_PopulatesAndSelectsFolder`, renamed to `..._PresentsSearchResultsWithoutFocusOrCommit`. No other test method in the file is touched; the Down-arrow tests at original `:355-388` are outside every hunk and pass.

### Sanctioned structural, non-test edits (4 files)

| File | Edit |
|---|---|
| `BreadcrumbDropDownHostTests.cs` | one-token `partial` on the class declaration (verified as a single-line, single-token diff) |
| `BreadcrumbDropDownOpenCoordinatorTests.cs` | `ControlledHost`: additive `RequestedTakeFocus` list + 4-parameter `OpenAsync`; 3-parameter method becomes a one-line delegation |
| `BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | `RecordingHost`: same additive pattern |
| `BreadcrumbSelectorOpenRetryTests.cs` | `RecordingDropDownHost`: same additive pattern |

Diff audit across all four files, counting added or removed lines containing `[TestMethod]`, `Should()`, `Verify(`, or `Assert.`:

```
0
```

**No test method was added, removed, weakened, disabled, or otherwise altered in any of the four files.** `BreadcrumbDropDownIntegrationTests.cs` has zero diff (P4-T5). **AC-11 satisfied.**

## Deviations recorded to date

| ID | Deviation | Impact |
|---|---|---|
| D10 | Plan `TestCaseFilter` values of the form `QfcItemController.<Suite>` corrected to `QfcItemController_<Suite>`. vstest `~` is a literal substring match; the dotted form selects **zero tests and exits 0** (proved empirically). Applies to P1-T2, P5-T2, P5-T5. | Command-string only; no acceptance criterion, assertion, or scope boundary changes. Without it the gates would be vacuous passes. |
| D11 | The 4-parameter `OpenAsync` is an **explicit** `IBreadcrumbDropDownHost` implementation on `BreadcrumbDropDownHost`. A second public overload made `GetMethod("OpenAsync")` in `BreadcrumbDropDownHostTests.cs:342-350` throw `AmbiguousMatchException`, failing 8 pre-existing tests. | Keeps the existing test file within the sanctioned edit list and keeps the concrete host's public surface minimal per `.claude/rules/csharp.md`. Interface contract (AC-10) unchanged. |
| D12 | P3-T4's lifetime plumbing landed inside the P3-T1..T3 compile unit, because P3-T2 cannot compile unless the lifetime already accepts the flag. | Sequencing only; both gates run and returned EXIT_CODE 0. |
| D13 | The router gained a one-line `HighlightRow(int)` pass-through in the P2-T2 partial. `BreadcrumbSelectionSession` is router-private, so the P4-T1 composite cannot reach the P2-T1 transition otherwise. | Mechanically required; adds no behavior beyond exposing the P2-T1 transition; covered by a dedicated test. |
| D14 | P4-T4's harness caveat resolved by driving the harness's published `internal void SetHostOpen(bool)` seam from the new 4-parameter `.Callback` instead of re-registering `SetupGet(h => h.IsOpen)`. | Same effect on the same private `_hostOpen` field; `BreadcrumbDropDownIntegrationTests.cs` stays byte-unmodified, which was the requirement. |

## Result

- **Output Summary:** EXIT_CODE 0 on both commands. `EfcFormController.cs` and `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` have zero diff (AC-13). Both interface diffs contain zero removed or altered lines (AC-10). `TextBoxSearch_KeyDown` is byte-identical (338 characters, ordinal match) and `QfcItemController.FolderHandling.cs` is byte-unmodified (AC-7). The only existing test files with diffs are the five sanctioned by D3/D4/D7; every hunk in `QfcItemController.EventHandlersTests.cs` lies inside the single sanctioned method's original line range, and a targeted audit of the other four returns zero changed test-assertion lines (AC-11). Twelve new `.cs` files exist, matching the P6-T2 manifest exactly. Accept criteria met.
