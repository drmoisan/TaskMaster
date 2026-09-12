# Research — QuickFiler Coverage Ledger (F1 / issue #432, epic #136)

- Date: 2026-08-07T22-15
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a344bd2214b6bf290`
- Repo HEAD at time of research: `74be1964`
- Scope: research only. No production file, rule file, or configuration was modified.

Every factual claim below carries the file path and line number, or the exact search and its
output, that supports it. Where the epic manifest is contradicted by ground truth the corrected
figure is stated plainly. Items that could not be verified from repository contents are marked
UNVERIFIED.

---

## 1. Executive summary of corrections to the epic manifest

| Manifest claim | Ground truth | Status |
| --- | --- | --- |
| 121 compiled files | 121 `<Compile Include=...>` entries in `QuickFiler/QuickFiler.csproj` lines 290–461 | **Confirmed** |
| Every compiled file assigned to exactly one child | Verified file-by-file: 121 assignments, no gap, no duplicate, no phantom | **Confirmed** |
| "33 currently carry `[ExcludeFromCodeCoverage]`" | 33 is the count of **files containing the string** across the whole `QuickFiler/` tree, including 7 non-compiled files and 5 comment/XML-doc-only mentions. Real attribute usages in the compiled surface: **40 usages across 21 files** | **Refuted; hypothesis confirmed exactly** |
| `Controllers/QfcScanProgressBandMapper.cs` `[X]` | Carries **no** attribute; the match at line 12 is inside an XML `<remarks>` block | **Refuted** |
| `Controllers/QfcHighConfidencePreFilter.cs` `[X]` | Attribute at line 166 decorates a **second top-level type** (`FolderScoringService`), not the file's primary type. The file is measured, at line-rate 1.0 | **Mis-scoped** |
| Six `QfcItemController.*` partials `[X]` | Those attributes are **member-level**, not type-level. All six files are measured (line-rates 0.74–0.91) | **Mis-scoped** |
| `Viewers/BreadcrumbPopupUiOperations.cs` `[X]` | Seven **member-level** attributes. The file is measured at line-rate 0.929412 | **Mis-scoped** |
| `ItemViewer.Breadcrumb.cs` not marked `[X]` | It is a partial of the exempt `ItemViewer` type, so it emits **no** coverage data | **Under-marked** |
| `QfcDatamodel.FrameBuilding.cs` / `.QueueProcessing.cs` not marked `[X]` | Partials of the exempt `QfcDatamodel` type; emit no coverage data | **Under-marked** |
| `Viewers/EfcViewer3.cs`, `QfcFormViewerDark.cs`, `QfcFormViewerExpanded.cs`, `QfcItemViewer.cs`, `QfcItemViewerExpandedLight.cs`, `QfcItemViewerLightSelected.cs`, `QfcItemViewerV1.cs` | **Not compiled.** Absent from the csproj `<Compile>` list. All seven carry the attribute but are out of scope | **Confirmed out of scope** |
| Line counts | Three designer files differ by exactly 1 line from the manifest (see §2.3) | **Minor drift** |

Two further findings not anticipated by the delegation prompt:

- **The Cobertura `<line>` set is counted twice by the existing summary function.** Each `<class>`
  carries both `<methods>/<method>/<lines>` and a class-level `<lines>` rollup;
  `Get-CoberturaCoverageSummary` selects `.//lines/line`, which matches both. Verified numerically
  in §4.2. The per-file harness must not replicate this.
- **The `line-rate` attribute on a merged `<class>` node is biased and must not be trusted.**
  `Merge-CoberturaClassesByFilename` recomputes it through the same double-counting path, weighting
  the primary class's lines twice. Detail in §4.3.

---

## 2. Q1 — The compiled denominator

### 2.1 Derivation

The list was taken from `<Compile Include=...>` entries in
`QuickFiler/QuickFiler.csproj`, lines 290–461 (three `ItemGroup`-internal ranges: Controllers
290–341, Helper Classes 342–354, Interfaces 355–368, Properties 369–379, Viewers 380–461).
Enumerating them gives **exactly 121** entries. The manifest's figure is confirmed.

Files on disk but **not** compiled, therefore out of scope:

| Path | Lines | Reason |
| --- | --- | --- |
| `QuickFiler/Legacy/**` (11 `.cs`) | — | Not in `<Compile>` list |
| `QuickFiler/Notes/**` (2 `.cs`) | — | Not in `<Compile>` list |
| `QuickFiler/Helper Classes/FormFocusListener.cs` | 14 | Orphan; not in `<Compile>` list |
| `QuickFiler/Interfaces/IQfcHomeController.cs` | 18 | Orphan. The compiled one is `Controllers\IQfcHomeController.cs` (20 lines) |
| `QuickFiler/Viewers/Form1.cs`, `Form1.Designer.cs` | 20 / 108 | Orphans |
| `QuickFiler/Viewers/EfcViewer3.cs` + `.Designer.cs` | 88 / 510 | Orphans, attribute-carrying |
| `QuickFiler/Viewers/QfcFormViewerDark.cs` + `.Designer.cs` | 55 / 232 | Orphans, attribute-carrying |
| `QuickFiler/Viewers/QfcFormViewerExpanded.cs` + `.Designer.cs` | 55 / 245 | Orphans, attribute-carrying |
| `QuickFiler/Viewers/QfcItemViewer.cs` + `.Designer.cs` | 86 / 961 | Orphans, attribute-carrying |
| `QuickFiler/Viewers/QfcItemViewerExpandedLight.cs` + `.Designer.cs` | 45 / 820 | Orphans, attribute-carrying |
| `QuickFiler/Viewers/QfcItemViewerLightSelected.cs` + `.Designer.cs` | 67 / 781 | Orphans, attribute-carrying |
| `QuickFiler/Viewers/QfcItemViewerV1.cs` + `.Designer.cs` | 45 / 762 | Orphans, attribute-carrying |
| `QuickFiler/Viewers/QFCItemViewerDarkNew.cs` + `.Designer.cs` | 43 / 773 | Orphans |
| `QuickFiler/Viewers/QFCItemViewerLightNew.cs` + `.Designer.cs` | 43 / 764 | Orphans |

Directory `.cs` totals versus compiled counts: `Controllers` 52 on disk / 52 compiled;
`Helper Classes` 14 / 13; `Interfaces` 15 / 14; `Properties` 3 / 3; `Viewers` 59 / 39. Sum of
compiled: 52 + 13 + 14 + 3 + 39 = **121**.

### 2.2 Cross-check against the manifest's Feature File Assignments

Per-child counts derived from the manifest text: F2 = 11, F3 = 11, F4 = 14, F5 = 5, F6 = 10,
F7 = 5, F8 = 6, F9 = 4, F10 = 11, F11 = 2, F12 = 5, F13 = 15, F14 = 10, F15 = 12. Sum = **121**.

Every one of the 121 compiled files was matched by hand to exactly one child assignment.
Result:

- **(a) Compiled files assigned to no child: none.**
- **(b) Files assigned to a child that are not compiled: none.**
- **(c) Files assigned to more than one child: none.**

Two near-misses worth recording because they would have produced false positives if the ledger
were built by directory walk or by filename alone:

- `IQfcFormController.cs` exists twice as a *compiled* file — `Controllers\IQfcFormController.cs`
  (43 lines) and `Interfaces\IQfcFormController.cs` (25 lines). Both are assigned to F6. The
  ledger key must be the full repo-relative path, never the leaf filename.
- `IQfcHomeController.cs` also exists twice on disk, but only `Controllers\IQfcHomeController.cs`
  (20 lines) is compiled. The manifest correctly cites the Controllers path and the 20-line count.

The manifest's file-assignment table is therefore **sound** and the ledger can adopt it verbatim
as the `owning_child` column. This is the single largest de-risking finding for F1.

### 2.3 Line counts

Counts were obtained with a ripgrep count of the pattern `^` per file (physical line count,
including a final line with no trailing newline). Full enumeration:

**Controllers (52 files)**

| File | Lines | Child |
| --- | --- | --- |
| `Controllers\BayesianPerformanceController.cs` | 156 | F15 |
| `Controllers\BreadcrumbBridgeRouter.cs` | 450 | F12 |
| `Controllers\BreadcrumbOutboundQueue.cs` | 67 | F2 |
| `Controllers\EfcDataModel.cs` | 397 | F5 |
| `Controllers\EfcFormController.cs` | 1086 | F9 |
| `Controllers\EfcHomeController.cs` | 441 | F8 |
| `Controllers\EfcHomeController.ExecuteMoves.cs` | 144 | F8 |
| `Controllers\EfcHomeController.Metrics.cs` | 87 | F8 |
| `Controllers\EfcHomeController.Timing.cs` | 43 | F8 |
| `Controllers\EfcHomeControllerDependencies.cs` | 428 | F8 |
| `Controllers\EfcHomeControllerDependencyFactories.cs` | 268 | F8 |
| `Controllers\EfcItemController.cs` | 1170 | F9 |
| `Controllers\EmailSorter.cs` | 85 | F2 |
| `Controllers\FilerQueue.cs` | 83 | F2 |
| `Controllers\IQfcFormController.cs` | 43 | F6 |
| `Controllers\IQfcHomeController.cs` | 20 | F7 |
| `Controllers\IQfcQueue.cs` | 41 | F2 |
| `Controllers\IQfcQueue1.cs` | 44 | F2 |
| `Controllers\KaChar.cs` | 99 | F3 |
| `Controllers\KaKey.cs` | 99 | F3 |
| `Controllers\KaStringAsync.cs` | 95 | F3 |
| `Controllers\KbdActions.cs` | 146 | F3 |
| `Controllers\KeyboardHandler.cs` | 414 | F3 |
| `Controllers\QfcCollectionController.cs` | 2349 | F11 |
| `Controllers\QfcDatamodel.cs` | 496 | F5 |
| `Controllers\QfcDatamodel.FrameBuilding.cs` | 154 | F5 |
| `Controllers\QfcDatamodel.QueueProcessing.cs` | 177 | F5 |
| `Controllers\QfcExplorerController.cs` | 323 | F6 |
| `Controllers\QfcFormController.cs` | 196 | F6 |
| `Controllers\QfcFormController.Actions.cs` | 302 | F6 |
| `Controllers\QfcFormController.EventHandlers.cs` | 399 | F6 |
| `Controllers\QfcFormController.SetupDisposal.cs` | 232 | F6 |
| `Controllers\QfcFormKeyHandler.cs` | 20 | F3 |
| `Controllers\QfcHighConfidencePreFilter.cs` | 191 | F2 |
| `Controllers\QfcHomeController.cs` | 487 | F7 |
| `Controllers\QfcHomeController.Iteration.cs` | 86 | F7 |
| `Controllers\QfcHomeController.Metrics.cs` | 234 | F7 |
| `Controllers\QfcItemController.cs` | 323 | F10 |
| `Controllers\QfcItemController.Conversation.cs` | 235 | F10 |
| `Controllers\QfcItemController.EventHandlers.cs` | 219 | F10 |
| `Controllers\QfcItemController.EventWiring.cs` | 391 | F10 |
| `Controllers\QfcItemController.FocusAndTheme.cs` | 326 | F10 |
| `Controllers\QfcItemController.FolderHandling.cs` | 235 | F10 |
| `Controllers\QfcItemController.Initialization.cs` | 466 | F10 |
| `Controllers\QfcItemController.MailActions.cs` | 224 | F10 |
| `Controllers\QfcItemController.Navigation.cs` | 228 | F10 |
| `Controllers\QfcItemController.ViewerSetup.cs` | 426 | F10 |
| `Controllers\QfcItemGroup.cs` | 52 | F2 |
| `Controllers\QfcQueue.cs` | 610 | F2 |
| `Controllers\QfcRemainingQueueAdmission.cs` | 48 | F2 |
| `Controllers\QfcScanProgressBandMapper.cs` | 79 | F2 |
| `Controllers\QfcStreamingDequeueConfidenceGate.cs` | 171 | F2 |

**Helper Classes (13 files, all F4)**

`cInfoMail.cs` 231, `ConversationResolver.cs` 358, `ConversationResolver.Loading.cs` 329,
`EfcThemeHelper.cs` 499, `EfcViewerQueue.cs` 101, `EmailMoveMonitor.cs` 262,
`IConversationResolver.cs` 33, `ItemViewerQueue.cs` 123, `QfcThemeControlSet.cs` 110,
`QfcThemeHelper.cs` 375, `QfEnums.cs` 16, `TlpCellSnapShot.cs` 223, `ViewerQueueCore.cs` 161.

**Interfaces (14 files)**

`IEmailMoveMonitor.cs` 39 (F4), `IFilerFormController.cs` 25 (F6), `IFilerHomeController.cs` 45
(F7), `IItemControler.cs` 15 (F3), `IKbdAction.cs` 18 (F3), `IQfcCollectionController.cs` 118
(F11), `IQfcDatamodel.cs` 59 (F5), `IQfcExplorerController.cs` 15 (F6),
`IQfcFormController.cs` 25 (F6), `IQfcFormViewer.cs` 51 (F6), `IQfcItemController.cs` 107 (F10),
`IQfcKeyboardHandler.cs` 37 (F3), `IMailItemActions.cs` 35 (F3),
`MailItemActionsAdapter.cs` 47 (F3).

**Properties (3 files, all F15)**

`AssemblyInfo.cs` 38, `Resources.Designer.cs` 432, `Settings.Designer.cs` 107.

**Viewers (39 files)**

| File | Lines | Child |
| --- | --- | --- |
| `Viewers\BayesianPerformanceViewer.cs` | 67 | F15 |
| `Viewers\BayesianPerformanceViewer.Designer.cs` | **499** (manifest: 498) | F15 |
| `Viewers\BreadcrumbBridgeCoordinator.cs` | 487 | F12 |
| `Viewers\BreadcrumbCollapsedSurfaceController.cs` | 308 | F13 |
| `Viewers\BreadcrumbCoordinatorUpgradeLifetime.cs` | 309 | F12 |
| `Viewers\BreadcrumbDropDownHost.cs` | 480 | F13 |
| `Viewers\BreadcrumbDropDownOpenCoordinator.cs` | 309 | F13 |
| `Viewers\BreadcrumbDropDownOpenLifetime.cs` | 477 | F13 |
| `Viewers\BreadcrumbItemViewerLifecycleCoordinator.cs` | 481 | F12 |
| `Viewers\BreadcrumbMessengerHub.cs` | 456 | F12 |
| `Viewers\BreadcrumbPopupPlacement.cs` | 87 | F13 |
| `Viewers\BreadcrumbPopupUiOperations.cs` | 494 | F13 |
| `Viewers\BreadcrumbUiDispatcher.cs` | 285 | F13 |
| `Viewers\BreadcrumbWebViewSurfaceFactory.cs` | 225 | F13 |
| `Viewers\EfcViewer.cs` | 162 | F9 |
| `Viewers\EfcViewer.Designer.cs` | **4277** (manifest: 4276) | F9 |
| `Viewers\IBreadcrumbDropDownHost.cs` | 42 | F13 |
| `Viewers\IBreadcrumbWebHost.cs` | 27 | F13 |
| `Viewers\IItemViewer.cs` | 133 | F14 |
| `Viewers\IWebViewCoreInitializer.cs` | 30 | F13 |
| `Viewers\IWebViewMessenger.cs` | 27 | F13 |
| `Viewers\ItemViewer.cs` | 432 | F14 |
| `Viewers\ItemViewer.Breadcrumb.cs` | 298 | F14 |
| `Viewers\ItemViewer.Commands.cs` | 109 | F14 |
| `Viewers\ItemViewer.Designer.cs` | 6224 | F14 |
| `Viewers\ItemViewer.DisplayState.cs` | 81 | F14 |
| `Viewers\ItemViewer.FolderSearch.cs` | 74 | F14 |
| `Viewers\ItemViewer.WebViewThread.cs` | 37 | F14 |
| `Viewers\ItemViewerExpanded.cs` | 181 | F14 |
| `Viewers\ItemViewerExpanded.Designer.cs` | 821 | F14 |
| `Viewers\QfcFormViewer.cs` | 262 | F15 |
| `Viewers\QfcFormViewer.Designer.cs` | **258** (manifest: 257) | F15 |
| `Viewers\QfcItemViewerExpanded.cs` | 63 | F15 |
| `Viewers\QfcItemViewerExpanded.Designer.cs` | 942 | F15 |
| `Viewers\ToolStripMenuItemCb.cs` | 87 | F15 |
| `Viewers\ToolStripMenuItemCb.Designer.cs` | 40 | F15 |
| `Viewers\WebView2BreadcrumbHost.cs` | 143 | F13 |
| `Viewers\WebView2CoreInitializer.cs` | 30 | F13 |
| `Viewers\WebView2Messenger.cs` | 147 | F13 |

All other line counts match the manifest exactly. The three off-by-one values are consistent with
the manifest having counted newline characters (`wc -l` semantics) on files whose last line has no
trailing newline. This is cosmetic but the ledger should state its counting method so the capstone
can reproduce it.

---

## 3. Q2 — `[ExcludeFromCodeCoverage]` ground truth

### 3.1 Method

Two searches over `QuickFiler/`:

1. `rg -n "ExcludeFromCodeCoverage"` — every textual occurrence (54 hits).
2. `rg -n -A2 "^\s*\[(System\.Diagnostics\.CodeAnalysis\.)?ExcludeFromCodeCoverage\]"` — real
   attribute usages with the declaration they decorate.

The second search is what distinguishes an attribute from a prose mention, and the `-A2` context
is what distinguishes a type-level from a member-level target.

### 3.2 Type-level attribute usages in compiled files (14 usages / 14 files)

| File:Line | Decorated declaration | Partial? | Child |
| --- | --- | --- | --- |
| `Controllers\EfcFormController.cs:27` | `internal class EfcFormController : IFilerFormController` | no | F9 |
| `Controllers\EfcItemController.cs:25` | `internal class EfcItemController : IItemControler` | no | F9 |
| `Controllers\KeyboardHandler.cs:22` | `internal class KeyboardHandler : IQfcKeyboardHandler` | no | F3 |
| `Controllers\QfcCollectionController.cs:21` | `public class QfcCollectionController : IQfcCollectionController` | no | F11 |
| `Controllers\QfcDatamodel.cs:25` | `public partial class QfcDatamodel : IQfcDatamodel` | **yes** | F5 |
| `Controllers\QfcExplorerController.cs:20` | `internal class QfcExplorerController : IQfcExplorerController` | no | F6 |
| `Controllers\QfcHighConfidencePreFilter.cs:166` | `internal sealed class FolderScoringService : IFolderScoringService` | no | F2 |
| `Viewers\EfcViewer.cs:20` | `public partial class EfcViewer : Form` | **yes** | F9 |
| `Viewers\ItemViewer.cs:20` | `public partial class ItemViewer : UserControl, IItemViewer, IContainerControlLocal` | **yes** | F14 |
| `Viewers\QfcFormViewer.cs:17` | `public partial class QfcFormViewer : Form, IQfcFormViewer` | **yes** | F15 |
| `Viewers\QfcItemViewerExpanded.cs:18` | `public partial class QfcItemViewerExpanded : UserControl` | **yes** | F15 |
| `Viewers\WebView2BreadcrumbHost.cs:29` | `public sealed class WebView2BreadcrumbHost : IBreadcrumbWebHost` | no | F13 |
| `Viewers\WebView2CoreInitializer.cs:15` | `public sealed class WebView2CoreInitializer : IWebViewCoreInitializer` | no | F13 |
| `Viewers\WebView2Messenger.cs:20` | `public sealed class WebView2Messenger : IWebViewMessenger, IDisposable` | no | F13 |

**`QfcHighConfidencePreFilter.cs:166` is a special case.** Reading
`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` lines 150–191 shows `FolderScoringService`
is a *second top-level type declared in the same file* at namespace scope (line 190 closes the
class, line 191 closes the namespace) — not a nested type and not the file's primary type. The
XML `<remarks>` at lines 157–164 records the justification (COM-bound adapter for the scoring
seam). The primary type `QfcHighConfidencePreFilter` is not exempt and is measured at line-rate
1.0 in the sample report. The ledger disposition unit for this file is therefore
*type-level, secondary type*, and the file itself is `testable`.

### 3.3 Member-level attribute usages in compiled files (26 usages / 7 files)

| File | Lines | Decorated members | Child |
| --- | --- | --- | --- |
| `Controllers\QfcItemController.Initialization.cs` | 138, 168, 200, 260, 291, 403, 436 | `Initialize(...)`, `Initialize(bool)`, `InitializeAsync`, `InitializeGraphicsAsync`, `InitializeSequentialAsync`, `CreateAsync`, `CreateSequentialAsync` | F10 |
| `Controllers\QfcItemController.EventHandlers.cs` | 60, 83, 97, 111, 125 | `BtnPopOut_Click`, `BtnReply_Click`, `BtnReplyAll_Click`, `BtnForward_Click`, `TxtboxBody_DoubleClick` | F10 |
| `Controllers\QfcItemController.ViewerSetup.cs` | 38, 132, 253 | `InitializeWebViewAsync`, `EnsureBreadcrumbPipeline`, `ResolveControlGroupsAsync` | F10 |
| `Controllers\QfcItemController.Navigation.cs` | 173, 191 | `ToggleExpansion`, `ToggleExpansionAsync` | F10 |
| `Controllers\QfcItemController.Conversation.cs` | 79 | `DoLoadConversationResolverCoreAsync` | F10 |
| `Controllers\QfcItemController.EventWiring.cs` | 99 | `WebView2Control_CoreWebView2InitializationCompleted` | F10 |
| `Viewers\BreadcrumbPopupUiOperations.cs` | 105, 380, 383, 390, 394, 412, 457 | `ShowOwnedPopup`, `CreateProductionControl`, `BeginProductionInitialization`, `ReadProductionCore`, `BeginProductionNavigation`, `DisposeProductionSurface`, `BindProductionNavigation` | F13 |

All 26 sit on files that **are** measured. The disposition unit for these is the *member*, not the
file. This is materially different from a type-level attribute and the ledger must model it as
such — a `remove` disposition on one of these members changes only that member's lines, whereas
a `remove` on a type-level attribute puts an entire file (or several partial files) back into the
denominator at once.

### 3.4 Reconciled totals

| Measure | Value |
| --- | --- |
| Total attribute **usages** in the compiled surface | **40** (14 type-level + 26 member-level) |
| Distinct **compiled files** carrying at least one usage | **21** |
| Distinct compiled files whose **coverage is fully suppressed** by a type-level attribute (including partial-class inheritance) | **24** |
| Attribute usages in files **not** in the compile list | 7 (7 files) |
| Compiled files containing the string but **no** attribute | 5 |
| **Distinct files containing the string anywhere under `QuickFiler/`** | **33** |

The manifest's "33" is exactly the last row: `21 (compiled, with attribute) + 5 (compiled,
comment/doc mention only) + 7 (not compiled, with attribute) = 33`. The orchestrator's hypothesis
is confirmed numerically and without residual.

Separately, the manifest's own `[X]` markers in "Feature File Assignments" number **26**, which
agrees with neither 33 nor 40 nor 21. The manifest prose and the manifest table are internally
inconsistent; neither is usable as an authority.

### 3.5 Partial-class inheritance — the 24 fully-suppressed files

A type-level `[ExcludeFromCodeCoverage]` on one partial suppresses the whole type, therefore all
its partial files. Verified against the sample Cobertura report (§4.4): none of these 24 filenames
appears in the report.

| Exempt type | Attribute site | Files suppressed |
| --- | --- | --- |
| `ItemViewer` | `Viewers\ItemViewer.cs:20` | `ItemViewer.cs`, `.DisplayState.cs`, `.Commands.cs`, `.Breadcrumb.cs`, `.FolderSearch.cs`, `.WebViewThread.cs`, `.Designer.cs` (7) |
| `QfcDatamodel` | `Controllers\QfcDatamodel.cs:25` | `QfcDatamodel.cs`, `.FrameBuilding.cs`, `.QueueProcessing.cs` (3) |
| `EfcViewer` | `Viewers\EfcViewer.cs:20` | `EfcViewer.cs`, `EfcViewer.Designer.cs` (2) |
| `QfcFormViewer` | `Viewers\QfcFormViewer.cs:17` | `QfcFormViewer.cs`, `QfcFormViewer.Designer.cs` (2) |
| `QfcItemViewerExpanded` | `Viewers\QfcItemViewerExpanded.cs:18` | `QfcItemViewerExpanded.cs`, `.Designer.cs` (2) |
| 8 non-partial types | see §3.2 | `EfcFormController.cs`, `EfcItemController.cs`, `KeyboardHandler.cs`, `QfcCollectionController.cs`, `QfcExplorerController.cs`, `WebView2BreadcrumbHost.cs`, `WebView2CoreInitializer.cs`, `WebView2Messenger.cs` (8) |

Total 7 + 3 + 2 + 2 + 2 + 8 = **24**.

The four ledger-relevant consequences:

1. `ItemViewer.Designer.cs` (6,224 lines) and `EfcViewer.Designer.cs` (4,277 lines) are already
   suppressed **as a side-effect** of the `.cs` partial's attribute, not by an attribute of their
   own. If F14/F9 remove the type-level attribute to cover the hand-written partials, the two
   largest designer files re-enter the denominator simultaneously. Removing the type-level
   attribute is therefore **not** a per-file decision — it is a per-type decision affecting up to
   seven files at once.
2. `ItemViewerExpanded.Designer.cs` (821 lines) and `BayesianPerformanceViewer.Designer.cs`
   (499 lines) demonstrate what happens when a designer file is *not* suppressed: they appear in
   the report at line-rates 0.9950980392156863 and 0.9914285714285714 respectively. Designer files
   are not intrinsically uncoverable.
3. The ledger's exempt-rationale column must distinguish *carries the attribute* from *inherits
   the exemption*. Only the former is a disposition unit.
4. The five comment-only mentions (`QfcScanProgressBandMapper.cs:12`,
   `ItemViewer.Commands.cs:10`, `ItemViewer.DisplayState.cs:9`, `ItemViewer.FolderSearch.cs:17`,
   `ItemViewer.WebViewThread.cs:12`) exist precisely to document inheritance. They must not be
   counted as dispositions, but four of them are *correct* documentation of case (3).

### 3.6 The seven attribute-carrying files that are not compiled

`Viewers\EfcViewer3.cs:17`, `Viewers\QfcFormViewerDark.cs:16`,
`Viewers\QfcFormViewerExpanded.cs:16`, `Viewers\QfcItemViewer.cs:18`,
`Viewers\QfcItemViewerExpandedLight.cs:14`, `Viewers\QfcItemViewerLightSelected.cs:15`,
`Viewers\QfcItemViewerV1.cs:14`. All seven are `partial class ... : Form`/`: UserControl`
declarations. None appears in `QuickFiler.csproj`. They are out of scope for this epic and should
be recorded in the ledger's reconciliation note only, not as rows.

### 3.7 Recommendation for the acceptance criterion wording

The AC currently reads "Every existing `[ExcludeFromCodeCoverage]` attribute in the compiled
surface has a recorded disposition naming the owning child" and the manifest cites 33. The AC text
is already correct in *scope* ("in the compiled surface"); only the manifest's numeral is wrong.
Recommended handling:

- Do **not** restate 33 anywhere in the ledger as a target.
- State the verified figures: **40 attribute usages across 21 compiled files, of which 14 are
  type-level and 26 member-level; 24 compiled files are fully coverage-suppressed once
  partial-class inheritance is applied.**
- Add an explicit "Reconciliation with epic manifest" section to the ledger recording that the
  manifest's 33 is a count of files containing the string across the whole `QuickFiler/` tree
  (21 + 5 + 7), and that the manifest's `[X]` markers are advisory only. Cite this research
  document.
- Keep the AC checkbox as written; it is satisfied by 40 dispositions, not 33.

---

## 4. Q3 — The real Cobertura schema

Read in full: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (348 lines) and
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (357 lines).

### 4.1 A real sample exists — do not construct the schema from generic knowledge

**176 committed `*.cobertura.xml` files** exist under `docs/features/**/evidence/`. The most
recent QuickFiler-relevant one, and the one used throughout this section, is:

```
docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml
```

It is post-processed output (it carries `<sources><source>.</source></sources>` and repo-relative
`filename` values, both injected by `ConvertTo-KoverageCoberturaXml`). Another suitable sample is
`docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/evidence/qa-gates/coverage-final.cobertura.xml`.

Consequence for planning: the Pester tests **may** still construct in-memory XML (that is the
established pattern, §5.1, and it keeps the tests deterministic and file-free), but the fixtures
must be modelled on the real document shape recorded below, in particular the method-level
`<lines>` nesting, which a naive fixture would omit.

### 4.2 Confirmed element nesting

```
<coverage line-rate branch-rate complexity version timestamp
          lines-covered lines-valid branches-covered branches-valid>
  <sources><source>.</source></sources>
  <packages>
    <package line-rate branch-rate complexity name>
      <classes>
        <class line-rate branch-rate complexity name filename>
          <methods>
            <method line-rate branch-rate complexity name signature>
              <lines>
                <line number hits branch [condition-coverage]>
                  [<conditions><condition number type coverage /></conditions>]
                </line>
              </lines>
            </method>
          </methods>
          <lines>                       <-- class-level rollup, SIBLING of <methods>
            <line number hits branch [condition-coverage]> ... </line>
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
```

Every level is present. Evidence: sample lines 2 (`<coverage ...>`), 3–5 (`<sources>`), 6
(`<packages>`), 7 (`<package ... name="QuickFiler">`), 8 (`<classes>`), 9 (`<class ...>`), 10
(`<methods>`), 11 (`<method ...>`), 12–21 (`<lines>` inside the method), 310–311 (`</methods>`
immediately followed by the class-level `<lines>`).

**The class-level `<lines>` block duplicates the method-level line numbers.** Method
`SetDefaultDependenciesFactory` (sample lines 11–22) reports lines 30, 31, 32, 33; the class-level
rollup at sample lines 311–324 reports 20, 21, 22, 24, 25, 30, 31, 32, 33 — a superset. The
class-level block additionally carries field-initializer/constructor lines.

**Numerical proof of double counting:** the document's header declares
`lines-valid="110849"` (sample line 2), and a count of the literal string `<line number=` in the
same file returns exactly **110849**. `Get-CoberturaCoverageSummary`
(`Invoke-MSTestWithCoverage.Helpers.ps1:122`) iterates `$cls.SelectNodes('.//lines/line')`, whose
descendant axis matches both the method-level and the class-level `<line>` nodes. The repository's
headline `lines-valid` is therefore approximately twice the number of distinct executable lines,
and `line-rate="0.856453"` is a ratio over that doubled set rather than over distinct lines.

This is a pre-existing condition of the shared script. **F1 must not change it** (changing the
repo-wide figure is outside this child's scope and would perturb every downstream gate), but the
per-file harness must not reproduce it.

### 4.3 Attribute names, exact spelling and value formats

`<class>` attributes actually emitted: `line-rate`, `branch-rate`, `complexity`, `name`,
`filename` (sample line 9). All hyphenated, lowercase. In PowerShell these require
quoted-member access (`$class.'line-rate'`), as `scripts/temp-extract-coverage.ps1:13` already
does.

`<line>` attributes: `number` (integer string), `hits` (integer string), `branch` (`"True"` /
`"False"`, capitalised — matched literally at `Invoke-MSTestWithCoverage.Helpers.ps1:128` and
`:236`), and optional `condition-coverage` in the form `"50% (1/2)"`. The parser
`Get-CoberturaLineConditionCoverageParts` (`:146`–`:165`) extracts the fraction with the regex
`\(([0-9]+)/([0-9]+)\)`.

`line-rate` values come in **two distinct formats** in the same document, and the format identifies
the provenance:

- Full double precision, for example `0.8001906577693041` (the `QuickFiler` package, sample line 7)
  or `0.9583333333333334` (`BreadcrumbOutboundQueue`): emitted directly by `dotnet-coverage`.
- Exactly six decimal places, for example `0.462302` (`QfcQueue`) or `0.968481`
  (`EfcHomeController`): **recomputed** by `Merge-CoberturaClassesByFilename`
  (`Invoke-MSTestWithCoverage.Helpers.ps1:275`), which rounds via
  `[math]::Round($x, 6)` at `:137`.

**The recomputed value is biased.** `Merge-CoberturaClassesByFilename` builds the merged node by
`$primaryNode.CloneNode($true)` (`:200`), which copies the primary class's entire `<methods>`
subtree, then replaces the class-level `<lines>` with the union across the whole group (`:208`–
`:268`). It then calls `Get-CoberturaCoverageSummary` on that node (`:270`–`:273`), whose `.//`
axis counts the primary class's method-level lines *plus* the merged class-level lines. The
primary class's lines therefore carry double weight relative to the other group members'. The
resulting `line-rate` attribute is not a faithful per-file line rate.

**Design consequence: the harness must recompute per-file coverage from `<line>` nodes and must
not read the `line-rate` attribute.**

### 4.4 `filename` format after post-processing

`ConvertTo-KoverageCoberturaXml` rewrites every `//class[@filename]` through
`ConvertTo-KoverageRelativePath` (`Invoke-MSTestWithCoverage.Helpers.ps1:324`–`:326`). That helper
(`:49`–`:96`) strips a repo-root prefix — trying the actual root, the canonical
`.../TaskMaster` sibling root (the worktree-tolerance branch at `:69`–`:75`), and both `\` and `/`
separator variants (`:77`–`:82`) — then normalises separators to `$PathSeparator`, which defaults
to `[System.IO.Path]::DirectorySeparatorChar` (`:61`), i.e. **backslash on Windows**.

Confirmed in the sample: `filename="QuickFiler\Controllers\EfcHomeController.cs"` (line 9) and
`filename="QuickFiler\Helper Classes\EfcViewerQueue.cs"` (line 2213 — note the embedded space,
which the harness must tolerate without quoting assumptions).

**Mapping back to the csproj is therefore a string operation with no path resolution required:**
a `<Compile Include="Controllers\BayesianPerformanceController.cs" />` entry maps to the Cobertura
`filename` `QuickFiler\Controllers\BayesianPerformanceController.cs` by prefixing the project
directory name and normalising separators. Recommendation: normalise both sides to backslash and
compare with `OrdinalIgnoreCase`, because `ConvertTo-KoverageRelativePath` itself compares
case-insensitively (`:85`).

Caveat: an evidence artifact produced under a non-default `-PathSeparator` would use forward
slashes. The harness should accept either by normalising the input, rather than assuming
backslash.

### 4.5 Multiple `<class>` nodes for the same file

`Merge-CoberturaClassesByFilename` (`:167`–`:292`) groups `./class[@filename]` within each
`<package>` by exact `filename` string, picks a primary (first whose `name` contains no `<`, i.e.
not a compiler-generated closure type, `:195`–`:198`), unions the class-level `<line>` nodes into
a map keyed by `[int]$lineNode.number` (`:220`–`:223`), and on collision takes `max(hits)`
(`:234`) and the richer `condition-coverage` (`:240`–`:261`). It then sorts by line number
(`:265`) and removes the non-primary siblings (`:285`–`:289`).

Two facts follow:

- **Line numbers cannot be double-counted across classes after the merge**, because the merge is
  keyed on line number. In the sample, each of the 70 QuickFiler filenames appears exactly once.
- **The merge only unions `./lines/line` (direct children), not method-level lines.** Any
  non-primary class's method-level `<lines>` are discarded with the node. That is harmless for a
  harness that reads class-level lines, and is another reason not to depend on the merged
  `line-rate`.

Partial classes across *different* files stay separate, which is exactly what per-file measurement
requires: four distinct `<class name="QuickFiler.Controllers.QfcFormController">` nodes appear
with four distinct filenames (`QfcFormController.cs`, `.SetupDisposal.cs`, `.EventHandlers.cs`,
`.Actions.cs`), and ten distinct `QfcItemController` nodes likewise.

Defensive recommendation: the harness should still union by `filename` and dedupe by line number
with `max(hits)`, so it is correct on both merged and unmerged input (a child may capture raw
`dotnet-coverage` output without post-processing).

### 4.6 Package identification and stripping

`<package name="QuickFiler">` (sample line 7). The package name is the assembly name, and
`ConvertTo-KoverageCoberturaXml` retains only packages whose `name` is in `$ProjectNames`
(`:318`–`:322`), where the default allowlist `Get-KoverageProjectAllowlist` (`:3`–`:47`) scans all
`*.csproj|*.vbproj|*.fsproj` outside `bin`/`obj`/`packages`, resolves `<AssemblyName>`, and drops
anything ending in `.Test` (`:39`–`:41`). `QuickFiler.csproj:12` declares
`<AssemblyName>QuickFiler</AssemblyName>`, so the package name is stable and predictable.

Packages for other first-party assemblies **are** present alongside QuickFiler; only third-party
and `.Test` packages are stripped. The harness must select the `QuickFiler` package explicitly
(as `scripts/temp-extract-coverage.ps1:7` does for `UtilitiesCS`) rather than scanning all
classes, or it will pick up same-named files from other projects.

### 4.7 What the QuickFiler package currently contains

Extracting every `filename="QuickFiler\..."` from the sample yields **70 distinct filenames**,
each exactly once. Against the 121-file compiled surface that leaves **51 compiled files absent
from the report entirely**:

| Absence cause | Count | Files |
| --- | --- | --- |
| Fully suppressed by a type-level `[ExcludeFromCodeCoverage]` (incl. partial inheritance) | 24 | §3.5 |
| Interface-only declarations (no executable code) | 23 | `Controllers\IQfcFormController.cs`, `Controllers\IQfcHomeController.cs`, `Controllers\IQfcQueue.cs`, `Controllers\IQfcQueue1.cs`, all 13 `Interfaces\I*.cs`, `Helper Classes\IConversationResolver.cs`, `Viewers\IItemViewer.cs`, `Viewers\IBreadcrumbDropDownHost.cs`, `Viewers\IBreadcrumbWebHost.cs`, `Viewers\IWebViewCoreInitializer.cs`, `Viewers\IWebViewMessenger.cs` |
| Enum-only | 1 | `Helper Classes\QfEnums.cs` (16 lines; `public enum InitTypeEnum` at line 5 is the only declaration) |
| Entirely commented out | 1 | `Helper Classes\cInfoMail.cs` (231 lines; the whole `namespace QuickFiler { ... }` block is commented from line 13) |
| Assembly attributes only | 1 | `Properties\AssemblyInfo.cs` |
| Suppressed by `DebuggerNonUserCodeAttribute` | 1 | `Properties\Resources.Designer.cs` (attribute at line 23) |

24 + 23 + 1 + 1 + 1 + 1 = 51; 70 + 51 = 121. The arithmetic closes.

Two of these deserve emphasis:

- `Properties\Resources.Designer.cs:23` carries
  `[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]`, which Microsoft code coverage
  honours as an exclusion. `Properties\Settings.Designer.cs` does **not** carry that attribute
  (only `CompilerGeneratedAttribute` at line 14 and `GeneratedCodeAttribute` at line 15) and
  consequently *does* appear in the report, at `line-rate="0"`. So the two generated
  `Properties/` files behave differently, and a naive "generated files are absent" rule is wrong.
- `Settings.Designer.cs` at `line-rate="0"` proves that a type which is never loaded still appears
  with `hits="0"`. Absence from the report therefore means *no instrumentable code was produced*,
  not *the code was never executed*. This is the key to the harness's absent-file semantics (§6.2).

Two enum declarations live inside interface files — `Viewers\IBreadcrumbDropDownHost.cs:9`
(`public enum BreadcrumbDropDownCloseReason`) and `Interfaces\IQfcDatamodel.cs:13`
(`public enum SortOptionsEnum`). Neither produces executable lines, so both files remain in the
interface-only class.

---

## 5. Q4 — Existing PowerShell conventions the harness must match

### 5.1 Test-file conventions

`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` (223 lines) establishes:

- `Set-StrictMode -Version Latest` as line 1 of the test file (line 1).
- A `BeforeAll` block that resolves the repo root relative to `$PSScriptRoot` and **dot-sources**
  the helper script into the test scope (lines 3–7):
  ```powershell
  $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
  $helperScriptPath = Join-Path $repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1'
  . $helperScriptPath
  ```
  No AST extraction, no `ScriptBlock` reconstruction. This works because the helper file contains
  only function definitions plus `Set-StrictMode` — it has no top-level side effects. **The new
  pure-logic file must have the same property.**
- In-memory XML fixtures built as single-quoted here-strings (`@' ... '@`, lines 11–27, 54–82,
  98–122, 139–168) and cast with `[xml]$resultXml = ConvertTo-... `. **No temporary files are
  created anywhere in the file**, satisfying the repo prohibition.
- Assertions use Pester's `Should` operators (`Should -Be`, `Should -Not -Match`,
  `Should -Contain`, `Should -BeNullOrEmpty`). FluentAssertions is C#-only; Pester's `Should` is
  the PowerShell equivalent here.
- Mocks are registered inside the `It` block with `Mock -CommandName <Cmdlet> -MockWith { ... }`
  (lines 206–215), and the mocked cmdlets are the *filesystem* cmdlets `Get-ChildItem` /
  `Get-Content`, not an external executable. That test exists specifically to exercise a fallback
  branch without touching disk.
- Determinism is achieved by passing the variable inputs explicitly rather than relying on ambient
  state — see the comment at lines 29–30: "Supply ProjectNames explicitly so this path-
  normalization test does not depend on the production allowlist". The new harness should follow
  the same rule: every environment-derived default must be an overridable parameter.

Each `Describe` is named for the function under test, and each `It` states a behaviour in prose.
Regression tests cite the issue number in a leading comment (lines 136–138, 187, 195, 202–205).

### 5.2 Production-script conventions

From `Invoke-MSTestWithCoverage.Helpers.ps1`:

- Every function is an advanced function with `[CmdletBinding()]` and an explicit `[OutputType(...)]`
  (`:3`–`:6`, `:49`–`:52`, `:98`–`:101`, `:146`–`:149`, `:294`–`:297`).
- Parameters carry `[Parameter(Mandatory = $true|$false)]` and, where the domain is closed,
  `[ValidateSet(...)]` (`:60`, `:308`).
- Defaults are computed expressions where sensible (`:8` resolves the repo root from
  `$PSScriptRoot`; `:305` defaults `ProjectNames` to `(Get-KoverageProjectAllowlist)`).
- Failures are raised with bare `throw` and a specific literal message
  (`:113` `'Cobertura XML does not contain a <packages> node.'`, `:315` identical). No silent
  catch-alls.
- Functions return objects (`[pscustomobject]`, `:136`–`:143`) or strings; the module emits no
  console output at all. All user-facing output lives in the entry script.

From `Invoke-MSTestWithCoverage.ps1`:

- `param(...)` block at the top of the file, before any function (lines 1–13).
- `Set-StrictMode -Version Latest` and `$ErrorActionPreference = 'Stop'` at script scope
  (lines 245–246).
- Comment-based help (`.SYNOPSIS` / `.DESCRIPTION`) on every function (lines 16–27, 42–50, etc.).
- Status messages via `Write-Output` (lines 314–316, 338, 343) — **not** `Write-Host`.
- The dot-source-safe entry guard at lines 346–348:
  ```powershell
  if ($MyInvocation.InvocationName -ne '.') {
      Invoke-MSTestWithCoverageMain @PSBoundParameters
  }
  ```
  This is the pattern that lets a test dot-source the entry script without executing it. The new
  entry script must use it.
- Wrapper seams for external executables follow the rule in `.claude/rules/powershell.md` §"Design
  Seams": `Invoke-DotnetCoverageExe -DotnetCoverageArgs` (lines 138–154) and `Invoke-VsWhereExe`
  (156–170), with array parameters deliberately not named `Args`. **The new harness invokes no
  external executable**, so no wrapper seam is required; its only I/O is reading the Cobertura
  file and optionally writing a report file.

### 5.3 Prior art: `scripts/temp-extract-coverage.ps1`

An 81-line script exists that already does approximately this job for `UtilitiesCS`. It should be
read for intent but **not** used as a template:

- It is named `temp-` and was written as a throwaway for feature `...-utilities-coverage-part-three-87`;
  its `$OutputPath` default (line 3) is hard-coded to that feature's evidence folder.
- It reads the `<class>` `line-rate` attribute directly (line 13), which §4.3 shows is unreliable
  for merged classes.
- It has no `[CmdletBinding()]`, no `Set-StrictMode`, no error handling for a missing or malformed
  file, and it uses `Write-Host` (lines 78–80), which PSScriptAnalyzer flags.
- It has no mirrored Pester test.
- Its classification is a pair of 1,000-character regex alternations (lines 27, 30) — precisely the
  fragile hard-coded classification that the JSON ledger contract (§6.1) is meant to replace.

Its one directly reusable decision is the threshold comparison at line 17:
`if ($lr -lt 0.80) { below } else { at-or-above }` — exactly 80% passes.

Planning note: F1 should consider whether to delete `scripts/temp-extract-coverage.ps1` as part of
this change. It is dead, untested, and PSScriptAnalyzer-dirty, and its continued existence invites
a sibling child to use it instead of the new harness. Deleting it is in scope (it is not under
`QuickFiler/`, not a rule file, and not a threshold). Recommendation: delete it, and say so in the
plan; if the plan prefers minimal scope, leave it and add a one-line deprecation comment pointing
at the new harness.

### 5.4 Recommended file paths

`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` is **357 lines** against the 500-line cap in
`.claude/rules/general-code-change.md` §"File Size Limit" and `.claude/rules/powershell.md`
§"Coding Standards". Appending the per-file logic there is not viable — confirmed.

The only existing PowerShell test tree is `tests/scripts/vscode/`, containing exactly four files
(`Install-RepoDotNetSdk.Tests.ps1`, `Invoke-MSTest.RunSettings.Tests.ps1`,
`Invoke-MSTestWithCoverage.Helpers.Tests.ps1`, `Invoke-VSBuild.Tests.ps1`). All coverage tooling
lives in `scripts/vscode/`. Recommended, consistent with both trees:

| Role | Path | Approx. size |
| --- | --- | --- |
| Pure logic (no I/O, dot-sourceable, function definitions only) | `scripts/vscode/Get-PerFileCoverage.Helpers.ps1` | 200–300 lines |
| Entry script (file I/O, report rendering, exit code) | `scripts/vscode/Get-PerFileCoverage.ps1` | 100–150 lines |
| Pester tests for the pure logic | `tests/scripts/vscode/Get-PerFileCoverage.Helpers.Tests.ps1` | — |

Two production files plus one test file sits inside the per-batch cap in
`.claude/rules/powershell.md` §"Change Budget" (at most 3 production and 3 test files). If the
ledger-consistency check (csproj `<Compile>` list vs ledger JSON) is also implemented as a script,
that is a third production file and still within the cap — but it is cleaner as a Pester test
assertion in a second test file, `tests/scripts/vscode/QuickFilerCoverageLedger.Tests.ps1`.

Function names must use approved verbs (PSScriptAnalyzer enforces this). Proposed:

- `Get-CoberturaPerFileCoverage` — pure: `[xml]` + package name → row objects.
- `Merge-CoberturaFileLines` — pure: union `<line>` nodes for one filename, dedupe by number,
  `max(hits)`. (`Merge` is an approved verb.)
- `Test-PerFileCoverageThreshold` — pure: rows + classification + threshold → verdict object.
- `Format-PerFileCoverageReport` — pure: rows → deterministic report text.
- `Invoke-PerFileCoverageGate` — the entry function in `Get-PerFileCoverage.ps1`.

### 5.5 PoshQC scan configuration — assessed risk

No `config/poshqc-scan.json` or equivalent exists. Searches performed: `Glob **/poshqc*` (no
files), `Glob scripts/powershell/PoshQC/**` (no files), `Glob scripts/**/*.psd1` (no files),
`Glob config/*.json` (only `config/orchestration-routing.json`), and a case-insensitive grep for
`poshqc|PoshQC|ScanFolder|scanFolder` across all `*.json`, which returns only four permission
entries in `.claude/settings.json:12`–`15`.

PoshQC is supplied by the MCP server declared in `.mcp.json`
(`npx -y @danmoisan/drm-copilot-mcp`); `.claude/rules/powershell.md:18` references
`scripts/powershell/PoshQC/settings/pester.runsettings.psd1`, **which does not exist in this
repository**. There is therefore no repo-local scan-folder allowlist that a new file could fall
outside of.

Assessment: **low risk, but not zero and not fully verifiable from the repo.** The existing
`scripts/vscode/*.ps1` and `tests/scripts/vscode/*.Tests.ps1` are demonstrably inside the scan set
(they are formatted, analyzer-clean, and their Pester tests run). Placing the new files in those
same two directories inherits whatever scan configuration those files already satisfy. Placing
them anywhere else — for example a new `scripts/coverage/` directory — introduces an unverifiable
assumption. **Recommendation: use `scripts/vscode/` and `tests/scripts/vscode/` for this reason
alone.** UNVERIFIED: the MCP server's internal default scan root and Pester discovery glob cannot
be inspected from this repository; the plan's Phase 0 should confirm empirically that
`run_poshqc_format` / `run_poshqc_analyze` / `run_poshqc_test` pick up the new files (for example
by introducing a deliberate formatting deviation and confirming the formatter corrects it) before
the harness is considered gated.

---

## 6. Q5 — Harness design constraints

### 6.1 Classification input — recommend a machine-readable JSON sidecar

Three options were considered.

| Option | Assessment |
| --- | --- |
| Parse the Markdown ledger table | Rejected. Couples pure logic to prose formatting; a reviewer reflowing a table breaks the gate for 15 children. It also forces the parser to handle escaped pipes, embedded backticks in paths, and rationale prose containing `|`. |
| Explicit `-ExemptFile <string[]>` parameter | Rejected as the *primary* mechanism. Moves the ledger into 15 call sites, which is exactly the inconsistency F1 exists to prevent. Retain it as an override for ad-hoc diagnosis. |
| **Machine-readable JSON sidecar** | **Recommended.** |

Recommended location: `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.json`,
a sibling of the Markdown ledger that `issue.md` already places at
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. Keeping them adjacent means
they are reviewed, versioned, and rebased together, and the epic folder is already the shared
contract surface for all 16 children.

Recommended shape — one entry per compiled file, keyed by the repo-relative path with backslash
separators exactly as the Cobertura `filename` expresses it:

```json
{
  "schema_version": 1,
  "generated_from": "QuickFiler/QuickFiler.csproj",
  "source_commit": "74be1964",
  "package": "QuickFiler",
  "threshold_percent": 80.0,
  "files": [
    {
      "path": "QuickFiler\\Controllers\\QfcQueue.cs",
      "lines": 610,
      "owning_child": "F2",
      "classification": "testable",
      "exempt_ground": null,
      "rationale": null,
      "attribute_dispositions": []
    },
    {
      "path": "QuickFiler\\Viewers\\ItemViewer.Designer.cs",
      "lines": 6224,
      "owning_child": "F14",
      "classification": "ratified-exempt",
      "exempt_ground": "generated-designer",
      "rationale": "WinForms Designer-generated code; exempt under CLAUDE.md UT2 (b).",
      "attribute_dispositions": [
        { "kind": "inherited", "from": "QuickFiler\\Viewers\\ItemViewer.cs:20" }
      ]
    }
  ]
}
```

`exempt_ground` should be a closed enum matching the three permitted grounds in `issue.md`:
`generated-designer`, `interface-only`, `irreducible-host-wiring`. A closed enum lets a Pester test
assert that no row invents a fourth ground, which is the mechanical form of the AC "Every
`ratified-exempt` row carries a rationale meeting one of the three permitted grounds".

`attribute_dispositions` entries should carry `{ kind: "type" | "member" | "inherited",
site: "<path>:<line>", target: "<declaration>", disposition: "ratified" | "remove",
owning_child: "F<n>", rationale: "..." }`. `kind: "inherited"` rows are informational and are not
counted toward the 40.

Drift control: the Markdown ledger is the human view and the JSON is authoritative. A Pester test
should assert (a) every `<Compile Include=>` path in `QuickFiler/QuickFiler.csproj` has exactly one
JSON row and vice versa, and (b) the Markdown table's row count equals the JSON `files` length.
Regenerating the Markdown from the JSON at authoring time is acceptable but should not be a runtime
dependency of the gate.

### 6.2 Files with no executable lines, and absent files

Verified behaviour (§4.7): a file with no executable code **does not appear in the Cobertura
document at all**. It does *not* appear as a `<class>` with an empty `<lines>` node. All 23
interface-only files, `QfEnums.cs`, `cInfoMail.cs`, `AssemblyInfo.cs`, and
`Resources.Designer.cs` are absent. Conversely `Settings.Designer.cs` — a type that exists but is
never loaded — **does** appear, at `line-rate="0"`.

Therefore "absent" means *no instrumentable code was emitted*, and "present at 0%" means
*instrumented but never executed*. These are genuinely different and the harness must not conflate
them. Required handling:

| Case | Classification | Harness behaviour | Exit contribution |
| --- | --- | --- | --- |
| Absent from report | `ratified-exempt` | Row status `EXEMPT (not measured)`, coverage column `n/a` | none |
| Absent from report | `testable` | Row status `NO DATA`, coverage column `n/a`, distinct diagnostic message naming the file | **failure** |
| Present, `<lines>` empty or zero `<line>` nodes | either | Row status `NO EXECUTABLE LINES`, coverage `n/a` | none — must **not** be reported as 0% |
| Present with lines | `ratified-exempt` | Row reports the percentage for information | none |
| Present with lines | `testable` | Row reports the percentage; compared against threshold | failure iff below |
| In report but not in the ledger | — | Row status `UNLEDGERED`, distinct message | **failure** (denominator drift; e.g. a sibling added a file) |

The "absent + testable" case is the one that would silently pass a naive implementation, and it is
exactly what happens if a child removes a type-level `[ExcludeFromCodeCoverage]` in source but the
build is stale — the file would be absent, and a harness that skipped absent files would report a
false green. Treating it as a failure is the safe default.

### 6.3 Exit-code and output contract

Recommended:

- **Pure functions** return `[pscustomobject]` rows and a verdict object; they perform no output
  and no `exit`.
- **Entry script** writes the rendered report to stdout via `Write-Output`, and additionally to
  `-OutputPath` when supplied (children write it to `<FEATURE>/evidence/qa-gates/` per
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`). Writing to both is what lets a
  child capture evidence and a human read the result in one invocation.
- Diagnostics for failing rows go to the error stream via `Write-Error` so they are visible when
  stdout is redirected to the evidence file.
- Exit codes: `0` all testable files at or above threshold; `1` at least one testable file below
  threshold, or `NO DATA`, or `UNLEDGERED`; `2` input error (file missing, not well-formed XML, no
  `<packages>` node, no `QuickFiler` package, ledger JSON missing or malformed). Distinguishing 2
  from 1 matters because a child must not mistake a broken input for a coverage regression. This
  is a new convention — no existing repo script uses more than 0/1 — so the plan should state it
  explicitly and the Pester tests should assert it.
- The `throw` idiom for malformed input in the pure layer matches
  `Invoke-MSTestWithCoverage.Helpers.ps1:113`/`:315`; the entry script catches and maps to exit 2.

Determinism requirements, each testable:

- **Row order**: sort by `path` using `[StringComparer]::Ordinal`, not `Sort-Object` default
  (which is culture-sensitive). Alternatively preserve ledger JSON order, which is itself
  ordinal-sorted at authoring time. Ordinal sorting is the stronger guarantee.
- **Percentage formatting**: compute `covered / total` as `[double]`, render with
  `.ToString('0.0', [System.Globalization.CultureInfo]::InvariantCulture)` so a comma decimal
  separator can never appear.
- **No timestamps, no host names, no paths outside the repo** in the report body; if a timestamp
  is wanted for evidence, put it in a header line that the determinism test excludes, or pass it
  as an explicit parameter so a test can pin it.
- The AC "identical input yields byte-identical report output" is then assertable by calling
  `Format-PerFileCoverageReport` twice on the same rows and comparing strings.

### 6.4 Threshold boundary semantics

Both existing repo gates treat the boundary as **inclusive pass**:

- `scripts/temp-extract-coverage.ps1:17` — `if ($lr -lt 0.80) { $below80 += $obj }`.
- `.codex/hooks/validate-feature-review-coverage.ps1:204` — `if ($null -ne $RepoWidePct -and $RepoWidePct -lt 80.0)`.

Policy language agrees: `CLAUDE.md` §UT2 says "must remain `>= 80%`"; `issue.md` says "flagging any
`testable` file below 80%".

**Recommendation: exactly 80.0% is a PASS**, implemented as `if ($rate -lt 0.80) { fail }`.

One refinement the existing scripts do not make: compare the **unrounded** ratio and display the
rounded value. A file at 799/1000 = 79.9% displays as 79.9 and fails; a file at 7999/10000 =
79.99% would display as 80.0 but must still fail. Comparing the rounded display value would let it
pass. The Pester suite should include that exact boundary case alongside the plain 80.0 case
already named in `issue.md` §"Test Conditions to Consider".

---

## 7. Q6 — Risks and prior art

### 7.1 Assembly-level coverage exclusion configuration

`coverage.config` (repo root, 25 lines) is the `dotnet-coverage` settings file consumed at
`Invoke-MSTestWithCoverage.ps1:320`. Its `ModulePaths/Exclude` list (lines 13–21) contains exactly
seven third-party patterns: `.*Deedle.*`, `.*FSharp.*`, `.*Castle\.Core.*`, `.*FluentAssertions.*`,
`.*Moq.*`, `.*Microsoft\.Testing.*`, `.*MSTest.*`. **QuickFiler is not excluded in whole or in
part.** The ledger is unaffected.

`ConvertTo-DerivedCoverageSettingsXml` (`Invoke-MSTestWithCoverage.ps1:79`–`:116`) adds one further
exclusion at run time, `.*\.Test\.dll$`, in an in-memory copy written beside the output and deleted
in a `finally` block (`:238`–`:242`). The canonical file is never modified. Again, no QuickFiler
impact.

### 7.2 Runsettings

- `TaskMaster.runsettings` (repo root, 31 lines): MSTest `Parallelize` `Workers=0`,
  `Scope=ClassLevel`, plus a `Code Coverage` data collector whose exclusions are byte-identical to
  `coverage.config`'s. No QuickFiler include/exclude rule.
- `scripts/vscode/TaskMaster.cli.runsettings` (9 lines): MSTest parallelisation only, **no** data
  collector — matching the docstring at `Invoke-MSTestWithCoverage.ps1:20`–`26`. Instrumentation
  comes solely from the outer `dotnet-coverage --settings coverage.config` path.
- Other `*.runsettings` found are per-project (`TaskTree.Test`, `TaskVisualization.Test`,
  `UtilitiesCS.Test`) or archived evidence artifacts. None affects QuickFiler.

**Conclusion: no configuration-level coverage exclusion touches QuickFiler.** The only exclusion
mechanism in play is the `[ExcludeFromCodeCoverage]` attribute, plus the framework's implicit
honouring of `DebuggerNonUserCodeAttribute` (§4.7).

This matters for the policy reconciliation: `.claude/rules/general-unit-test.md` §"Coverage
Exclusion Policy" prohibits `exclude` *entries* that match production source paths — a rule about
coverage-tool configuration globs. `CLAUDE.md` §UT2 sanctions applying the exemption "via
`[ExcludeFromCodeCoverage]` attributes in source code (reviewable in PRs) or via `coverage.config`
assembly-level excludes". Since QuickFiler's exclusions are entirely attribute-based and no
`coverage.config` entry names a QuickFiler path, the two rules do not conflict on mechanism; they
conflict only on *whether a given file qualifies*, which is precisely the irreducible-remainder
test the epic manifest §1 settles. F1 implements that test; it does not re-legislate either rule.

### 7.3 In-flight conflicting features (#400, #424)

F1 modifies no file under `QuickFiler/`, so it has **no textual merge conflict** with either
feature. Beyond that:

- The compiled surface at `74be1964` **already contains** both features' production files:
  `Controllers\QfcScanProgressBandMapper.cs` and `Controllers\QfcHighConfidencePreFilter.cs`
  (#424) appear at `QuickFiler.csproj:322`–`323`, and the fifteen `Viewers\Breadcrumb*` /
  `WebView2*` files (#400) appear at `:393`–`:411`. Both feature folders are still under
  `docs/features/active/`, i.e. not archived.
- The residual risk is **semantic, not textual**: if either branch subsequently adds, removes, or
  renames a compiled file, or adds/removes an `[ExcludeFromCodeCoverage]` attribute, the ledger's
  file list and attribute inventory go stale, and every wave-1 child inherits the stale
  denominator.
- Mitigation, and the reason the JSON sidecar matters: the Pester assertion "every
  `<Compile Include=>` in `QuickFiler.csproj` has exactly one ledger row, and vice versa" turns
  ledger staleness into a **test failure at the next toolchain run**, rather than a silent
  divergence. Recommend making that assertion part of F1's own test suite, so any later drift
  fails fast for whoever introduces it.

State plainly in the plan: **for F1's own diff, #400 and #424 are a non-issue; for F1's
deliverable, they are a staleness risk that the csproj-vs-ledger test is designed to catch.**

### 7.4 Current coverage figures from existing evidence

From `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
line 2 and line 7 (timestamp `1786072633`; no C# suite was run for this research):

| Scope | `line-rate` | `branch-rate` | Provenance |
| --- | --- | --- | --- |
| Repository-wide | `0.856453` (85.65%) | `0.790039` (79.00%) | Recomputed by `ConvertTo-KoverageCoberturaXml`; **denominator is double-counted** per §4.2, so treat as indicative only |
| `QuickFiler` package | `0.8001906577693041` (80.02%) | `0.7371154614462645` (73.71%) | Emitted by `dotnet-coverage`; not recomputed |

Also recorded: `lines-covered="94937"`, `lines-valid="110849"`, `branches-covered="22001"`,
`branches-valid="27848"`.

Two cautions for the capstone F16:

- The QuickFiler package figure of 80.02% is computed over **instrumented classes only** — the 24
  fully-suppressed files contribute nothing to numerator or denominator. Removing a type-level
  attribute will add that file's lines to the denominator immediately and to the numerator only as
  tests are written. A child that removes an attribute before writing the tests will register as a
  coverage *regression*, both for the package and repo-wide. Sequencing within each child must be
  tests-first, attribute-removal-last, and F1's ledger should say so in the disposition
  instructions.
- The repo-wide figure is above the 80% floor in `CLAUDE.md` §UT2 but below the 85% figure in
  `.claude/rules/general-unit-test.md` §"Coverage Requirements". F1 must not adjudicate that
  tension — `issue.md` §"Constraints & Risks" forbids changing repository-wide thresholds, and
  epic #136's per-file target is unambiguously 80%. The harness's default threshold should be
  80.0 with a `-ThresholdPercent` parameter, and the ledger should carry `threshold_percent: 80.0`
  so the number lives in data rather than in code.

---

## 8. Decisions required by planning

| # | Decision | Recommendation | Basis |
| --- | --- | --- | --- |
| D1 | How to express the `[ExcludeFromCodeCoverage]` count in the ledger and AC | Record **40 usages / 21 compiled files / 14 type-level / 26 member-level / 24 fully-suppressed files**; add a "Reconciliation with epic manifest" section explaining that the manifest's 33 = 21 + 5 + 7 files-containing-the-string. Do not restate 33 as a target. Leave the AC checkbox text unchanged. | §3.4 |
| D2 | Ledger disposition unit | Per **attribute usage**, not per file, with `kind ∈ {type, member, inherited}`. `inherited` rows are informational. | §3.2, §3.3, §3.5 |
| D3 | How the harness receives classification | **JSON sidecar** at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.json`, authoritative; Markdown is the human view. Keep `-ExemptFile` as an ad-hoc override only. | §6.1 |
| D4 | Per-file rate computation | **Recompute from `<line>` nodes**, unioned by `filename` and deduped by line number with `max(hits)`. Never read the `<class>` `line-rate` attribute. Never use the `.//lines/line` descendant axis. | §4.2, §4.3, §4.5 |
| D5 | Whether to fix the double-counted repo-wide `lines-valid` | **No.** Out of F1's scope; changing it perturbs every existing gate and evidence baseline. Record it in the ledger's notes and, per the standing preference, promote it to its own GitHub issue. | §4.2 |
| D6 | File paths for the new harness | `scripts/vscode/Get-PerFileCoverage.Helpers.ps1` (pure), `scripts/vscode/Get-PerFileCoverage.ps1` (entry), `tests/scripts/vscode/Get-PerFileCoverage.Helpers.Tests.ps1`, plus `tests/scripts/vscode/QuickFilerCoverageLedger.Tests.ps1` for the csproj-vs-ledger assertion. | §5.4, §5.5 |
| D7 | Absent-file semantics | Absent + `ratified-exempt` → no failure. Absent + `testable` → **failure**, distinct `NO DATA` message. Present with zero `<line>` nodes → `n/a`, never 0%. In-report-but-unledgered → **failure**. | §6.2 |
| D8 | Exit-code contract | `0` pass, `1` coverage/ledger failure, `2` input error. Assert all three in Pester. | §6.3 |
| D9 | Threshold boundary | Exactly 80.0% **passes**; implement as `-lt 0.80` on the **unrounded** ratio, display rounded to one decimal with `InvariantCulture`. Add a 79.99%-displays-as-80.0 boundary test. | §6.4 |
| D10 | Determinism mechanism | Ordinal sort on the repo-relative path; `'0.0'` format string with `InvariantCulture`; no timestamp in the comparable body. | §6.3 |
| D11 | Path matching between csproj and Cobertura | Prefix `<Compile Include>` with `QuickFiler\`, normalise both sides to backslash, compare `OrdinalIgnoreCase`. Accept forward-slash input by normalising. | §4.4 |
| D12 | Package selection | Select `<package name="QuickFiler">` explicitly; throw (exit 2) if absent. Do not scan all packages. | §4.6 |
| D13 | Fate of `scripts/temp-extract-coverage.ps1` | **Delete it** as part of this change (it is dead, untested, analyzer-dirty, and reads the unreliable `line-rate` attribute). If the plan prefers minimal scope, leave it with a deprecation comment. State the choice explicitly. | §5.3 |
| D14 | Line-counting method for the ledger | State it in the ledger header. Recommend physical line count (count of lines including a final line lacking a trailing newline), which reproduces every manifest figure except the three noted off-by-ones. | §2.3 |
| D15 | Ledger staleness guard against #400 / #424 | Include the csproj-vs-ledger completeness Pester assertion in F1's own suite so later drift fails at the next toolchain run. | §7.3 |
| D16 | Sequencing guidance to siblings | The ledger's `remove` dispositions must instruct: write tests first, remove the attribute last, because removal expands the denominator immediately. Type-level removals affect up to seven partial files at once and must be planned as a single unit. | §7.4, §3.5 |
| D17 | Phase 0 verification of PoshQC scan coverage | Empirically confirm the new file paths are picked up by `run_poshqc_format` / `run_poshqc_analyze` / `run_poshqc_test` before treating the harness as gated. No repo-local scan config exists to inspect. | §5.5 |

---

## 9. Items explicitly not verified

- The PoshQC MCP server's default scan root and Pester discovery glob (server is external:
  `@danmoisan/drm-copilot-mcp`). The repo contains no scan configuration to read.
- `scripts/powershell/PoshQC/settings/pester.runsettings.psd1`, referenced by
  `.claude/rules/powershell.md:18`, does not exist in this worktree.
- No C# test suite was executed for this research. All coverage figures are read from the
  committed evidence artifact named in §7.4 and are as of that artifact's capture, not as of now.
- Whether branches `#400` and `#424` carry further unmerged QuickFiler changes beyond what is on
  `main` at `74be1964` was not checked (no git tooling was used in this session).
- The exact reason three designer files differ by one line from the manifest (§2.3) is inferred
  from `wc -l` versus physical-line-count semantics; it was not confirmed by inspecting the files'
  trailing bytes.
