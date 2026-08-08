# Per-File Coverage Research — `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs`

## 1. Header

- **Timestamp:** `2026-08-07T22-00`
- **Production file:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8220048ded06d508\QuickFiler\Controllers\QfcFormController.SetupDisposal.cs`
- **Exact line count:** 232 lines (verified by full read; line 232 is the closing namespace brace)
- **`[ExcludeFromCodeCoverage]`:** **NO.** No attribute anywhere in the file. The type is declared `internal partial class QfcFormController` (line 18) with no attribute list.
- **Epic child:** F6 (`quickfiler-qfc-form-explorer-controller-coverage`, issue #435), wave 1, band C3.
- **Sibling partials (NOT owned by this artifact):** `QfcFormController.cs` (196), `QfcFormController.EventHandlers.cs` (399), `QfcFormController.Actions.cs` (302).

### Prior measured coverage (indicative, not authoritative)

`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`, line 19715:

```xml
<class line-rate="0.70684" branch-rate="0.588235" complexity="71"
       name="QuickFiler.Controllers.QfcFormController"
       filename="QuickFiler\Controllers\QfcFormController.SetupDisposal.cs">
```

Every line number in that block matches the current source (e.g. `120` = `private int _itemsPerIteration = -1;`, `209` = `Cleanup`'s opening brace). The artifact is from the in-flight #424 branch, not this child; it is quoted here as evidence of the *shape* of the gap, not as this child's baseline. **The authoritative number must be produced by running F1's harness (section 8).** The measured uncovered line set from that artifact is 44 lines and is enumerated in section 4; 44 uncovered against a `line-rate` of 0.70684 implies a measurable-line denominator of about 150.

---

## 2. Current test coverage inventory

### 2.1 Search scope

Same as the sibling artifact: only two files in the whole `QuickFiler.Test` tree construct the concrete `QfcFormController` — `QfcFormControllerTests.cs` (827 lines, 42 tests) and `QfcFormControllerSeamTests.cs` (378 lines, 15 tests). The seven `QfcHomeController*Tests.cs` files and `QfcViewer_Test.cs` reference only `Mock<IQfcFormController>` or commented-out code.

### 2.2 Tests that reach a member declared in **this** file

| Test method | File:line | Production member reached | What it actually verifies |
| --- | --- | --- | --- |
| `CaptureItemSettings_ShouldCaptureSettings` | `QfcFormControllerTests.cs:169` | `CaptureItemSettings` | Only `act.Should().NotThrow()`; the bare mock hits the guard at line 24 and returns immediately. Guard-path only. |
| `CaptureItemSettings_WhenCellStatesPopulated_StoresStates` | `QfcFormControllerSeamTests.cs:300` | `CaptureItemSettings` happy path (32–46) | Asserts `_states` is the returned snapshot and `Hide()` called once. Real coverage. |
| `CaptureItemSettings_WhenCellStatesNull_StoresNullAndHides` | `QfcFormControllerSeamTests.cs:319` | `CaptureItemSettings` null-states branch (39–43) | Asserts `_states` null, `CaptureTlpCellStates` once, `Hide` once. |
| `CaptureItemSettings_WhenRowStylesNull_ReturnsEarly` | `QfcFormControllerSeamTests.cs:338` | `CaptureItemSettings` guard (24–29) | Asserts `CaptureTlpCellStates` and `Show` never called. |
| `RemoveTemplatesAndSetupTlp_ShouldSetupTlp` | `QfcFormControllerTests.cs:182` | `RemoveTemplatesAndSetupTlp` guard only (50–56) | **No assertions at all** (empty Assert section with a placeholder comment). Bare mock ⇒ guard returns. |
| `SetupLightDark_ShouldSetupThemes` | `QfcFormControllerTests.cs:195` | `SetupLightDark` guard only (77–79) | **No assertions.** Bare mock ⇒ guard returns. |
| `SpaceForEmail_ShouldReturnCorrectValue` | `QfcFormControllerTests.cs:208` | `SpaceForEmail` happy path (99–117) | Stubs `Size`, `ClientSize`, and a real 2-row `TableLayoutPanel`; asserts `result > 0`. See finding F-5 (environment-dependent assertion). |
| `RegisterFormEventHandlers_ShouldRegisterHandlers` | `QfcFormControllerTests.cs:228` | `RegisterFormEventHandlers` guard only (151–153) | **No assertions.** |
| `UnregisterFormEventHandlers_ShouldUnregisterHandlers` | `QfcFormControllerTests.cs:241` | `UnregisterFormEventHandlers` guard only (179–181) | **No assertions.** |
| `Cleanup_ShouldCleanupResources` | `QfcFormControllerTests.cs:254` | `Cleanup` (209–228) | **No assertions.** Executes the whole method with `_globals.Ol` null, so lines 211–213 are skipped. |
| `ItemsPerIteration_ShouldGetAndSetCorrectly` | `QfcFormControllerTests.cs:269` | `ItemsPerIteration` set (129–137) and get (121–128) | Sets 5, asserts get returns 5. |
| `RegisterFormEventHandlers_WiresAllIntentCommandEvents` | `QfcFormControllerSeamTests.cs:135` | `RegisterFormEventHandlers` body (156–175) | `VerifyAdd` for all five intent events, `Times.Once` each. Real coverage of 170–174. |
| `RegisterFormEventHandlers_UsesExclusionControlsFromFormViewer` | `QfcFormControllerSeamTests.cs:156` | `RegisterFormEventHandlers` (167) | `GetKeyEventExclusionControls` verified once. |
| `OkClicked_WhenRaised_RoutesToControllerWithoutThrowing` | `QfcFormControllerSeamTests.cs:170` | `RegisterFormEventHandlers` + `ButtonOK_Click` (EventHandlers.cs) | Registration side-effect. |
| `CancelClicked_WhenRaised_CancelsParentTokenSource` | `QfcFormControllerSeamTests.cs:185` | same | Registration side-effect; asserts parent CTS cancelled. |
| `UndoClicked_WhenRaised_RoutesToControllerWithoutThrowing` | `QfcFormControllerSeamTests.cs:204` | same | Registration side-effect. |
| `ItemsPerLoadValueChanged_WhenRaised_RoutesToSpinnerHandler` | `QfcFormControllerSeamTests.cs:220` | same | Registration side-effect. |
| `SkipClicked_WhenRaised_TogglesSkipButtonTextAndEnabled` | `QfcFormControllerSeamTests.cs:246` | same | Registration side-effect. |
| `ButtonSkipHandler_WhenInvoked_...` | `QfcFormControllerSeamTests.cs:268` | ctor only, from this file's view | — |

**Members in this file with NO real assertion today:** `RemoveTemplatesAndSetupTlp` (body never entered), `SetupLightDark` (body never entered), `LoadItemsPerIteration` (never called), `UnregisterFormEventHandlers` (body never entered), `Cleanup` (executed but nothing asserted), the `ForAllControls` lambdas in both Register/Unregister, and the `SpaceForEmail` guard and `catch` paths.

### 2.3 Existing fixture pattern (reuse this)

Two variants exist. The `SetupDisposal.cs` work depends on the second one.

**Base fixture** — identical in both files (`QfcFormControllerTests.cs:75-113`, `QfcFormControllerSeamTests.cs:86-130`): a `CreateQfcFormController()` helper passing `_mockGlobals.Object, _mockFormViewer.Object, _mockQfcQueue.Object, QfEnums.InitTypeEnum.Sort, () => { }, _mockParent.Object, _tokenSource, _token`, with `[TestInitialize] Setup()` creating `Mock<IApplicationGlobals>`, `Mock<IAppAutoFileObjects>` (**mandatory** — the ctor dereferences `_globals.AF` at `QfcFormController.cs:43`), `Mock<IQfcFormViewer>`, `Mock<IQfcQueue>`, `Mock<IQfcHomeController>`, and a real `CancellationTokenSource`.

**Register-enabling helper** — `QfcFormControllerSeamTests.cs:105-116`, the exact arrangement needed to get past the guard at line 151:

```csharp
private void SetupForRegister()
{
    _mockFormViewer.SetupGet(x => x.Controls)
                   .Returns(new Control.ControlCollection(new Control()));
    _mockFormViewer.Setup(x => x.GetKeyEventExclusionControls())
                   .Returns(new List<Control>());
    _mockParent.SetupGet(x => x.KeyboardHandler)
               .Returns(new Mock<IQfcKeyboardHandler>().Object);
}
```

**TLP helper** — `QfcFormControllerSeamTests.cs:291-297`:

```csharp
private static TableLayoutPanel CreateTlpWithRowStyles()
{
    var tlp = new TableLayoutPanel();
    tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize, 0));
    tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
    return tlp;
}
```

**Invoke-dispatching helper** — `QfcFormControllerTests.cs:640-642`, needed to make `_formViewer.Invoke(...)` actually run its delegate:

```csharp
_mockFormViewer.Setup(fv => fv.Invoke(It.IsAny<Delegate>()))
               .Callback<Delegate>(action => action.DynamicInvoke());
```

Plus reflection helpers `GetPrivateField<T>` / `SetPrivateField<T>`, duplicated verbatim in both files. New tests must consume these from a single shared support file, not copy them a third and fourth time.

---

## 3. Test-file size finding

- **`QuickFiler.Test/Controllers/QfcFormControllerTests.cs` is 827 lines**, against the 500-line ceiling in `.claude/rules/general-code-change.md` § "File Size Limit", which explicitly covers *test code*. That is a pre-existing violation of 327 lines. **Recorded as a finding.**
- **New test cases must NOT be appended to it.** In-repo precedent for this exact decision is `QfcFormControllerSeamTests.cs:16-24`: *"Kept in a separate TestClass so the pre-existing QfcFormControllerTests.cs file is not grown further."*
- `QfcFormControllerSeamTests.cs` (378 lines) has ~122 lines of headroom, but it is **shared territory**: its Seam D region tests `CaptureItemSettings` (this file), its Seam B region tests `RegisterFormEventHandlers` (this file) *and* the `ButtonOK_Click`/`ButtonSkip_Click` handlers (`EventHandlers.cs`, a sibling researcher's file), and `LoadItemsAsync_MailItemPath_...` (line 353) reads `QfcFormController.Actions.cs` (another sibling's file). Growing it would put two or three F6 plan phases in the same file. **Do not grow it.**

### Recommended new test files for THIS production file

The proposed case list (section 6) is 46 cases. At the repository's typical ~11 lines per MSTest method plus usings and class scaffolding, one file would land around 560–620 lines — over the limit. Split by concern, matching the file's two `#region`-free but semantically distinct halves:

| New file | Covers | Projected size |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcFormController.TestSupport.cs` | Shared, non-`[TestClass]` builder: `SetField`/`GetField`, `CreateThemeMap()`, `SetupForRegister()`, `CreateTlpWithRowStyles()`, `DispatchInvoke()`, and a `KeyRaisingControl : Control` test double exposing `RaiseKeyDown`/`RaisePreviewKeyDown`. Mirrors the established `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` precedent (`internal static class QfcItemControllerTestSupport` with `SetField`/`GetField`, plus a harness subclass). | ~150 lines |
| `QuickFiler.Test/Controllers/QfcFormControllerSetupTests.cs` | `CaptureItemSettings`, `RemoveTemplatesAndSetupTlp`, `SetupLightDark`, `SpaceForEmail`, `ItemsPerIteration` get/set, `LoadItemsPerIteration` | ~360 lines |
| `QuickFiler.Test/Controllers/QfcFormControllerDisposalTests.cs` | `RegisterFormEventHandlers`, `UnregisterFormEventHandlers`, `Cleanup`, and all state-transition sequences | ~300 lines |

Together with the sibling artifact's `QfcFormControllerCoreTests.cs`, and the `EventHandlers`/`Actions` researchers' recommended files, the set is pairwise disjoint — no two F6 plan phases write to the same test file, and none touches `QfcFormControllerTests.cs` or `QfcFormControllerSeamTests.cs`.

### Is splitting the existing 827-line file in scope for F6?

**Recommendation: NO — record as a separate finding.** Splitting it correctly requires simultaneously relocating tests owned by the `EventHandlers.cs` and `Actions.cs` researchers, i.e. a four-way concurrent edit to a single file inside one wave. That is precisely the conflict shape the epic avoids by keeping partial-class families in one child (epic.md § Decomposition Rationale) — but at test-file granularity the four F6 phases are still concurrent. Promote "split `QfcFormControllerTests.cs` (827 lines) into per-partial files under the 500-line limit" to its own GitHub issue via the MCP promotion lifecycle and note it in F6's `spec.md` as an out-of-scope finding. It is a natural F16 capstone item.

---

## 4. Member-by-member reachability table

| # | Member (line span) | Status today | Uncovered lines (prior artifact) | Concrete blocker |
| --- | --- | --- | --- | --- |
| 1 | `CaptureItemSettings()` (22–46) | Covered (line-rate 1, branch 0.9) | — | none. One guard sub-condition (`RowStyles.Count < 2`) untested. |
| 2 | `RemoveTemplatesAndSetupTlp()` (48–73) | **Body unreachable today** (line-rate 0.474) | 60, 62, 65, 66, 67, 68, 69, 70, 71, 72 | Not a hard blocker. Needs a real in-memory `TableLayoutPanel` from `_formViewer.L1v0L2L3v_TableLayout` plus `_rowStyleTemplate` seeded. See §5.1. |
| 3 | `SetupLightDark()` (75–85) | **Body unreachable today** (line-rate 0.625) | 82, 83, 84 | Not a hard blocker. Needs non-empty `Panels`, non-null `Buttons`, non-null `_globals.Ol`. See §5.2 — an empty `Panels` list *throws*. |
| 4 | `SpaceForEmail` get (87–118) | Partial (line-rate 0.75) | 95, 96 (guard return 0); 107, 108, 109, 110 (`catch`) | Not a hard blocker. Guard return is trivially reachable; the `catch` is reachable by making the mocked `Handle` getter throw. See §5.3. |
| 5 | `_itemsPerIteration = -1` (120) | Covered (via ctor) | — | none |
| 6 | `ItemsPerIteration` get (121–128) | Covered (line-rate 1, branch 1) | — | none. The `-1` sentinel path that calls `LoadItemsPerIteration` is not separately asserted. |
| 7 | `ItemsPerIteration` set (129–138) + lambda (134, 136) | Covered (line-rate 1) | — | The inner action body at 135 (`ItemsPerLoadValue = (decimal)x`) executes only when the mocked `Invoke` dispatches its delegate. |
| 8 | `LoadItemsPerIteration()` (140–147) | **Unreachable today** (line-rate 0) | 141, 142, 143, 145, 146, 147 | Not a hard blocker. Needs `_rowStyleTemplate` with non-zero `Height`; deterministic once `L1v_TableLayout` is left null so `SpaceForEmail` returns 0. See §5.4. |
| 9 | `RegisterFormEventHandlers()` (149–175) | Body covered; **lambda unreachable** | 158, 159, 160, 161, 163, 164, 165, 166 | Not a hard blocker. The existing `SetupForRegister()` supplies an **empty** `Control.ControlCollection`, so `ForAllControls` never invokes the action. Needs one child control. See §5.5. |
| 10 | `UnregisterFormEventHandlers()` (177–203) | Body covered; **lambda unreachable** | 186, 187, 188, 189, 191, 192, 193, 194 | same as #9 |
| 11 | `Cleanup()` (205–228) | Partial (line-rate 0.842) | 211, 212, 213 | Not a hard blocker. Needs `_globals.Ol` non-null so the `PropertyChanged -=` unsubscription runs. |

**Total prior-uncovered line set: 44 lines.**

### Reachability verdict

**No member of `QfcFormController.SetupDisposal.cs` is genuinely unreachable.** Every uncovered line is reachable with the existing `IQfcFormViewer` interface seam, in-memory (never shown) WinForms controls that the current test suite already constructs, and reflection-based private-field seeding that the current test suite already uses. **This file needs zero new production seams.**

Interface-surface verification (all read, not assumed):

- `IQfcFormViewer` (`QuickFiler/Interfaces/IQfcFormViewer.cs`, 51 lines, `public interface IQfcFormViewer : IForm`, `namespace QuickFiler`) declares every member this file touches: `L1v0L2L3v_TableLayout` (24), `L1v_TableLayout` (25), `Panels` (15), `Buttons` (14), `CaptureTlpCellStates()` (32), `GetKeyEventExclusionControls()` (33), `ItemViewerTemplateMargin` (34), `ItemsPerLoadValue` (47), and the five intent events `OkClicked`/`CancelClicked`/`UndoClicked`/`SkipClicked`/`ItemsPerLoadValueChanged` (37–40, 48). `Show()`, `Hide()`, `Invoke(Delegate)`, `Controls`, `Size`, `ClientSize`, `Refresh()` come from `IControl` (`UtilitiesCS/Interfaces/IWinForm/IControl.cs` lines 207, 169, 176, 40, 73, 35, 187); `Handle` from `IWin32Window` and `Dispose()` from `IDisposable`, both inherited at `IControl.cs:10-14`. All Moq-stubbable.
- `IQfcKeyboardHandler` (`QuickFiler/Interfaces/IQfcKeyboardHandler.cs`) declares `KeyboardHandler_PreviewKeyDownAsync(object, PreviewKeyDownEventArgs)` (16) and `KeyboardHandler_KeyDownAsync(object, KeyEventArgs)` (18). `IFilerHomeController.KeyboardHandler` is `IQfcKeyboardHandler { get; set; }` (`QuickFiler/Interfaces/IFilerHomeController.cs:32`). **`KeyboardHandler.cs` (F3) is therefore never needed and never edited.**
- `IApplicationGlobals.Ol` is `IOlObjects` (`UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs:11`) and `IOlObjects : INotifyPropertyChanged` (`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:11`), so `PropertyChanged +=` / `-=` are `VerifyAdd`/`VerifyRemove`-able.

---

## 5. Seam design proposal

**Proposal: introduce NO new production seam for this file.** Each flagged blocker, independently verified:

### 5.1 `TableLayoutHelper.RemoveSpecificRow` / `InsertSpecificRow` on a real `TableLayoutPanel` (lines 60, 65)

Read `UtilitiesCS/HelperClasses/Windows Forms/TableLayoutHelper.cs`:

- `InsertSpecificRow` (13–53) begins `if (panel.InvokeRequired) { panel.Invoke(...); return; }` (21–25). A handle-less, never-shown `TableLayoutPanel` reports `InvokeRequired == false`, so the method runs **inline on the calling thread**. No UI thread and no message pump is required.
- `RemoveSpecificRow` (55–104) has the identical `InvokeRequired` early-dispatch (61–66) and then `if (rowIndex >= panel.RowCount) return;` (68–71).

**Verified in-repo precedent that this works headless in `QuickFiler.Test`:** `QfcFormControllerSeamTests.CreateTlpWithRowStyles()` (line 291) constructs `new TableLayoutPanel()` inside an ordinary non-STA `[TestClass]`, and the #424 coverage artifact shows `CaptureItemSettings` at line-rate 1 — i.e. that panel was really constructed and consumed during a real run. `QfcFormControllerTests.SpaceForEmail_ShouldReturnCorrectValue` (line 215) and `AdjustTlp_ShouldAdjustTlp` (line 540) do the same. Separately, `UtilitiesCS.Test/HelperClasses/TableLayoutHelper_Tests.cs` exercises both helpers directly against plain `TableLayoutPanel` instances.

**Important behavioral detail for test design:** `new TableLayoutPanel()` has `RowCount == 0` by default even after `RowStyles.Add(...)`. With `RowCount == 0`, `RemoveSpecificRow(tlp, 0, 2)` hits the `rowIndex >= panel.RowCount` early return and is a no-op. To exercise the real removal path the test must set `tlp.RowCount = 2` explicitly. Both variants are worth a case (T13/T14 below).

**Seam required: none.**

### 5.2 `QfcThemeHelper.SetupFormThemes` (line 82)

Read `QuickFiler/Helper Classes/QfcThemeHelper.cs:240-296`. `SetupFormThemes(IList<Control> panels, IList<Control> buttons)` is a pure factory returning `Dictionary<string, Theme>` with `"LightNormal"` and `"DarkNormal"`. It performs no I/O and touches no COM.

**Verified constraint that changes test design:** the `"Default2Color"` group is built with `new ThemeControlGroup(controls: panels, back: ..., fore: ...)`, which binds the 3-argument ctor at `UtilitiesCS/HelperClasses/ThemeHelpers/ThemeControlGroup.cs:42-62`. That ctor throws `ArgumentOutOfRangeException` when `controls.Count == 0` (lines 48–56). **`SetupLightDark` with a non-null but EMPTY `Panels` list therefore throws.** The `"Buttons"` group uses the 7-argument ctor (82–100), which has no count check, so an empty `Buttons` list is safe and is the deterministic choice.

Then line 83 calls `LoadTheme()` (declared in `QfcFormController.cs:120`), which calls `theme.SetTheme()` — the **synchronous** overload (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:452-455`) → `ThemeControlGroup.ApplyTheme()` (no `bool` parameter) → `ApplyThemeTwoField` sets `ForeColor`/`BackColor` on the in-memory panels. **The synchronous overload never touches `UiThread.Dispatcher`**, so `SetupLightDark` is safe. (Contrast: `ActiveTheme`'s *setter* uses `SetTheme(async: true)` → `ApplyTheme(bool)` → `UiThread.Dispatcher.InvokeAsync` — see the sibling artifact's §5. Do not conflate them.)

Line 84's `_globals.Ol.PropertyChanged += DarkMode_CheckedChanged;` is an event on the mockable `IOlObjects`. **Seam required: none.**

### 5.3 `Screen.PrimaryScreen` and `_formViewer.GetScreen()` (lines 102–110)

`GetScreen()` is **not** an `IQfcFormViewer` member. It is the extension method `IControlExtensions.GetScreen(this IControl control) => Screen.FromHandle(control.Handle);` (`UtilitiesCS/Extensions/IControlExtensions.cs:16-19`). Moq cannot stub an extension method — but it does not need to, because the extension's only input is the **stubbable** `Handle` property.

- To exercise the success path: leave `Handle` at its default `IntPtr.Zero`; `Screen.FromHandle(IntPtr.Zero)` resolves to the primary screen and does not throw.
- **To exercise the uncovered `catch` block at 107–110:** `_mockFormViewer.SetupGet(x => x.Handle).Throws(new InvalidOperationException());`. The exception is raised while `GetScreen()` evaluates `control.Handle`, inside the `try` at 104, so the bare `catch` at 107 runs and assigns `Screen.PrimaryScreen`. Fully deterministic, no seam.
- `Screen.PrimaryScreen` at 102/109 and `_screen?.WorkingArea.Height ?? 0` at 115 remain environment-reads. **New tests must not assert absolute magnitudes.** Either assert the deterministic early-return (`0`) or compute the expected value in the test from the same `Screen.PrimaryScreen?.WorkingArea.Height ?? 0` source so the assertion is self-consistent on any host, including a headless agent where `PrimaryScreen` may be null.

**Seam required: none.** (An `IScreenProvider` adapter seam would be the textbook answer, but it is unnecessary here and would enlarge `IQfcFormViewer` — prohibited by the F15 constraint.)

### 5.4 `_formViewer.Invoke(...)` in `ItemsPerIteration.set` (134) and `LoadItemsPerIteration()` (143)

`Invoke(Delegate)` is an `IControl` member (`IControl.cs:176`). Moq's default returns `null` and does not dispatch, which is why line 135's inner assignment never runs today. The in-repo dispatching precedent is `QfcFormControllerTests.cs:640-642` (`.Callback<Delegate>(action => action.DynamicInvoke())`), used by `MaximizeFormViewer_ShouldMaximizeForm`. Reuse it.

`LoadItemsPerIteration` computes `(int)Math.Round(SpaceForEmail / _rowStyleTemplate.Height, 0)` (142). With `_rowStyleTemplate = new RowStyle(SizeType.Absolute, 100)` and `L1v_TableLayout` left null, `SpaceForEmail` returns `0` through its guard, so the result is a deterministic `0` on every host. That is the recommended arrangement. **Seam required: none.**

### 5.5 `Controls.ForAllControls(...)` + `_parent.KeyboardHandler` (156–168, 184–196)

`_formViewer.Controls` is `Control.ControlCollection` (`IControl.cs:40`), binding the extension overload at `UtilitiesCS/Extensions/WinFormsExtensions.cs:86-97`, which iterates children and, for each not in `except`, recurses into `ForAllControls(Control, Action<Control>, IList<Control>)` (57–71) — which recurses into the child's own `Controls` and then invokes `action(parent)`.

**Root cause of the uncovered lambda:** `SetupForRegister()` returns `new Control.ControlCollection(new Control())` — an **empty** collection. The `foreach` body never runs, so the action is never invoked and lines 158–166 / 186–194 stay at zero hits.

**Fix, with no production change:** build a host control with one child.

```csharp
var host = new Control();
var child = new KeyRaisingControl();   // Control subclass in TestSupport
host.Controls.Add(child);
_mockFormViewer.SetupGet(x => x.Controls).Returns(host.Controls);
_mockFormViewer.Setup(x => x.GetKeyEventExclusionControls()).Returns(new List<Control>());
_mockParent.SetupGet(x => x.KeyboardHandler).Returns(mockKbd.Object);
```

`KeyRaisingControl` is a tiny in-memory `Control` subclass exposing `public void RaiseKeyDown(KeyEventArgs e) => OnKeyDown(e);` and `public void RaisePreviewKeyDown(PreviewKeyDownEventArgs e) => OnPreviewKeyDown(e);`, which turns the subscription into a **behaviorally verifiable** fact: after `RegisterFormEventHandlers()`, raising `KeyDown` invokes `mockKbd.Object.KeyboardHandler_KeyDownAsync` exactly once; after `UnregisterFormEventHandlers()`, it invokes it zero times. This is far stronger than a "did not throw" assertion and covers both lambdas plus the exclusion branch.

**One trap to encode as a test:** `_formViewer.GetKeyEventExclusionControls().ToList()` (167, 195). If a test satisfies the guard at 151/179 but leaves `GetKeyEventExclusionControls()` unstubbed, Moq returns `null` and `.ToList()` throws `ArgumentNullException`. That is a genuine, deterministic negative case (T31/T36).

**`KeyboardHandler.cs` (F3) is not touched.** Only the `IQfcKeyboardHandler` interface is consumed. **Seam required: none.**

### 5.6 `Cleanup()` — double-dispose determination (the orchestrator's explicit question)

**Finding: a SECOND call to `Cleanup()` does NOT throw. The method is idempotent by construction.** Traced line by line against the source:

| Line | First call | Second call |
| --- | --- | --- |
| 210 `if (_globals?.Ol is not null)` | evaluated | `_globals` is `null` (set at 217) → `?.` short-circuits → condition false → **211–213 skipped, no dereference** |
| 215 `UnregisterFormEventHandlers()` | runs | enters; guard at 179 is `_formViewer?.Controls is null \|\| _parent?.KeyboardHandler is null`. `_formViewer` is `null` (set at 219) → `?.` yields `null` → `is null` true → **early return at 181. No NullReferenceException.** |
| 216 `_undoQueue?.Dispose()` | disposes | `_undoQueue` is **not** nulled by `Cleanup`, so `BlockingCollection<T>.Dispose()` is called again. `BlockingCollection<T>.Dispose(bool)` guards its body with `if (!_isDisposed)`, so the repeat call is a no-op. **No throw.** |
| 217 `_globals = null` | — | idempotent |
| 218 `_formViewer?.Dispose()` | disposes viewer | `_formViewer` null → skipped |
| 219–225 | assignments | idempotent |
| 226 `_parentCleanup?.Invoke()` | invokes parent cleanup | `_parentCleanup` is `null` (set at 227) → **skipped. Parent cleanup runs exactly once, which is correct.** |

**Consequence for the plan: there is NO double-dispose defect, so there is NO behavior fix to propose and NO tension with the "no behavior change to observable QuickFiler flows" acceptance criterion.** The correct action is to *pin* the idempotence with regression tests (T38–T41), not to add a guard. This directly answers the orchestrator's grounding question, and the answer is the opposite of the hypothesis.

Three secondary observations from the same trace, recorded as findings rather than fixes:

- **F-1.** `_undoQueue` is disposed but never nulled (216). `Actions.cs:232` (`UndoDialog`) does `_undoQueue.Add(...)`. Calling `ButtonUndo_Click` after `Cleanup()` would therefore throw `ObjectDisposedException`, not return quietly. That path is in `Actions.cs` (a sibling researcher's file) — cross-referenced here so it is not lost.
- **F-2.** `_qfcQueue`, `_states`, `_itemsPerIteration`, `_rowStyleExpanded`, `_itemMarginTemplate`, `_themes`, `_activeTheme`, `_darkMode`, `_token`, `_tokenSource`, `_helperTasks`, and `_undoConsumerTask` are **not** reset by `Cleanup()`. `_rowStyleTemplate` and `_groups` are. The asymmetry is undocumented. Informational only; not a defect on any observed path.
- **F-3.** `Init()` is not idempotent: a second `Init()` re-runs `SetupLightDark` (adding a second `PropertyChanged` subscription at 84) and `RegisterFormEventHandlers` (adding a second copy of all five intent subscriptions at 170–174 and of every per-control key subscription). No production caller invokes `Init()` twice (`QfcHomeController` is the only caller), so this is latent. **Do not fix in F6** — a guard would change observable subscription counts. Document with T42 and promote to a separate issue.

### Hard constraints — compliance statement

- `QuickFiler/Viewers/QfcFormViewer.cs` (F15): **not edited; no edit implied.** No member is added to `IQfcFormViewer`.
- `QuickFiler/Controllers/KeyboardHandler.cs` (F3): **not edited.** Consumed only through `IQfcKeyboardHandler`.
- `QuickFiler/Controllers/QfcCollectionController.cs` (F11): **not edited.** This file does not reference it.
- `coverage.config` and shared build property files (F1): **not modified.** Note that `Invoke-MSTestWithCoverage.ps1` writes a *derived* copy of `coverage.config` beside the output and deletes it in a `finally` block (lines 79–116, 198–242); the canonical file is never written, so running the harness does not breach F1 ownership.
- **No `CROSS-CHILD CONTRACT NOTE` is required for this file.**
- .NET Framework 4.8: no default interface members, no `init`, no `record`/`record struct` are introduced. The only new type is a plain `internal sealed class KeyRaisingControl : Control` in the **test** assembly.

### Projected line count after seam work

**232 lines — unchanged.** No production edit is proposed. Well under the 500-line limit.

---

## 6. Proposed test cases

Each entry is one atomic plan task. Target files as stated.

### 6.0 Shared support (prerequisite)

| # | Task | File |
| --- | --- | --- |
| T0 | Create `QfcFormControllerTestSupport`: `SetField`/`GetField`, `Build(...)` mock bundle, `CreateThemeMap()` (empty `ControlGroups`), `SetupForRegister(...)` (with an optional child-control overload), `CreateTlpWithRowStyles(int rowCount)`, `DispatchInvoke(Mock<IQfcFormViewer>)`, and `internal sealed class KeyRaisingControl : Control` exposing `RaiseKeyDown`/`RaisePreviewKeyDown`. No `[TestClass]`. | `QfcFormController.TestSupport.cs` |

### 6.1 `CaptureItemSettings` — guard clauses enumerated one per condition

Target: `QfcFormControllerSetupTests.cs`

| # | Test method | Scenario → expected outcome |
| --- | --- | --- |
| T1 | `CaptureItemSettings_WhenTableLayoutIsNull_ReturnsBeforeShowingViewer` | `L1v0L2L3v_TableLayout` returns null → `Show()` never called, `CaptureTlpCellStates()` never called. (Overlaps existing `CaptureItemSettings_WhenRowStylesNull_ReturnsEarly`; **omit** unless the existing seam file is retired. Listed for completeness of the guard enumeration.) |
| T2 | `CaptureItemSettings_WhenRowStyleCountIsOne_ReturnsBeforeShowingViewer` | TLP with exactly **1** RowStyle → `Show()` never called. **New: covers the second guard sub-condition (`Count < 2`), currently at 50% branch coverage.** |
| T3 | `CaptureItemSettings_WhenRowStyleCountIsTwo_CapturesBothTemplateStyles` | TLP with 2 RowStyles → private `_rowStyleTemplate` is `RowStyles[0]`, `_rowStyleExpanded` is `RowStyles[1]`. Boundary at the guard threshold; the existing seam tests assert only `_states` and `Hide`. |
| T4 | `CaptureItemSettings_WhenInvoked_ShowsThenHidesViewerExactlyOnce` | Happy path → `Show()` once and `Hide()` once. |
| T5 | `CaptureItemSettings_WhenInvoked_StoresItemViewerTemplateMargin` | Stub `ItemViewerTemplateMargin` = `new Padding(7)` → private `_itemMarginTemplate` equals it. Line 35 is currently executed but never asserted. |
| T6 | `CaptureItemSettings_WhenCaptureTlpCellStatesThrows_PropagatesException` | `CaptureTlpCellStates()` set to throw `InvalidOperationException` → the exception propagates and `Hide()` is **never** called (no `finally`). Error handling; documents the current no-cleanup-on-throw behavior. |
| T7 | `CaptureItemSettings_AfterCleanup_ReturnsWithoutThrowing` | `Cleanup()` then `CaptureItemSettings()` → no throw (`_formViewer` null ⇒ guard). State transition. |

### 6.2 `RemoveTemplatesAndSetupTlp` — guard clauses one per condition, then body

Target: `QfcFormControllerSetupTests.cs`

| # | Test method | Scenario → expected outcome |
| --- | --- | --- |
| T8 | `RemoveTemplatesAndSetupTlp_WhenTableLayoutIsNull_DoesNotTouchQueue` | `L1v0L2L3v_TableLayout` null → `IQfcQueue.TlpTemplate` never set (`VerifySet`, `Times.Never`). Guard condition 1. |
| T9 | `RemoveTemplatesAndSetupTlp_WhenQueueIsNull_DoesNotMutateTableLayout` | Real TLP supplied, `_qfcQueue` nulled via reflection → TLP `RowCount` unchanged. Guard condition 2. |
| T10 | `RemoveTemplatesAndSetupTlp_WhenRowStyleTemplateIsNull_DoesNotTouchQueue` | Real TLP + real queue, `_rowStyleTemplate` left null → `TlpTemplate` never set. Guard condition 3 (**the only guard sub-condition at 0% branch coverage today**). |
| T11 | `RemoveTemplatesAndSetupTlp_WhenAllDependenciesPresent_AssignsTemplateToQueue` | Real 2-row TLP, `_rowStyleTemplate` seeded, `_itemsPerIteration` seeded to 3 → `IQfcQueue.TlpTemplate` set to that TLP exactly once. **Covers lines 60–71.** |
| T12 | `RemoveTemplatesAndSetupTlp_WhenStatesCaptured_AssignsStatesToQueue` | `_states` seeded with a `TlpCellStates` instance → `IQfcQueue.TlpStates` set to it once. **Covers line 72.** |
| T13 | `RemoveTemplatesAndSetupTlp_WhenPanelRowCountIsZero_RemovalIsNoOpAndRowsAreInserted` | TLP with 2 RowStyles but default `RowCount == 0` → `RemoveSpecificRow` early-returns (`TableLayoutHelper.cs:68`) and `RowCount` becomes `_itemsPerIteration`. Boundary; documents the default-`RowCount` behavior. |
| T14 | `RemoveTemplatesAndSetupTlp_WhenPanelRowCountIsTwo_RemovesBothTemplateRows` | `tlp.RowCount = 2` with 2 RowStyles → after the call, `RowStyles.Count` equals `_itemsPerIteration`. Exercises the real removal path. |
| T15 | `RemoveTemplatesAndSetupTlp_WhenItemsPerIterationIsZero_ThrowsArgumentOutOfRange` | `_itemsPerIteration` seeded to 0 → `InsertSpecificRow` throws `ArgumentOutOfRangeException` (`TableLayoutHelper.cs:31-34`). Boundary/error handling; documents current behavior. |
| T16 | `RemoveTemplatesAndSetupTlp_WhenInvoked_GrowsMinimumSizeByTemplateHeightTimesCount` | `_rowStyleTemplate` height 100, `_itemsPerIteration` 3, initial `MinimumSize.Height` 0 → resulting `MinimumSize.Height` is 300 and `Width` is unchanged. **Covers lines 66–70** with a real arithmetic assertion. |

### 6.3 `SetupLightDark` — guard clauses one per condition, then body

Target: `QfcFormControllerSetupTests.cs`

| # | Test method | Scenario → expected outcome |
| --- | --- | --- |
| T17 | `SetupLightDark_WhenPanelsIsNull_DoesNotSubscribeToPropertyChanged` | `Panels` returns null → `VerifyAdd(o => o.PropertyChanged += It.IsAny<PropertyChangedEventHandler>(), Times.Never)`. Guard condition 1. |
| T18 | `SetupLightDark_WhenButtonsIsNull_DoesNotSubscribeToPropertyChanged` | `Panels` non-empty, `Buttons` null → never subscribes. Guard condition 2. |
| T19 | `SetupLightDark_WhenGlobalsOlIsNull_DoesNotBuildThemes` | `Panels`/`Buttons` supplied, `g.Ol` returns null → private `_themes` stays null. Guard condition 3 (**at 0% branch coverage today**). |
| T20 | `SetupLightDark_WhenAllDependenciesPresent_BuildsLightAndDarkThemes` | `Panels = { new Panel() }`, `Buttons = new List<Control>()`, `g.Ol` stubbed → `_themes` contains exactly the keys `"LightNormal"` and `"DarkNormal"`. **Covers line 82.** |
| T21 | `SetupLightDark_WhenOlDarkModeIsFalse_SetsActiveThemeToLightNormal` | `Ol.DarkMode` false → `_activeTheme` is `"LightNormal"`. **Covers line 83.** |
| T22 | `SetupLightDark_WhenOlDarkModeIsTrue_SetsActiveThemeToDarkNormal` | `Ol.DarkMode` true → `_activeTheme` is `"DarkNormal"`. Positive counterpart. |
| T23 | `SetupLightDark_WhenAllDependenciesPresent_SubscribesToOlPropertyChangedOnce` | → `VerifyAdd(o => o.PropertyChanged += It.IsAny<PropertyChangedEventHandler>(), Times.Once)`. **Covers line 84.** |
| T24 | `SetupLightDark_WhenPanelsIsEmpty_ThrowsArgumentOutOfRangeException` | `Panels = new List<Control>()` (non-null but empty) → `ThemeControlGroup` ctor throws (`ThemeControlGroup.cs:48-56`). Negative/boundary; **documents a real trap for future maintainers.** |
| T25 | `SetupLightDark_WhenCalledTwice_SubscribesToPropertyChangedTwice` | Two calls → `Times.Exactly(2)`. Documents finding F-3 (non-idempotent setup). |

### 6.4 `SpaceForEmail`

Target: `QfcFormControllerSetupTests.cs`

| # | Test method | Scenario → expected outcome |
| --- | --- | --- |
| T26 | `SpaceForEmail_WhenTableLayoutIsNull_ReturnsZero` | `L1v_TableLayout` null → returns exactly `0`. **Covers lines 95–96.** Environment-independent. |
| T27 | `SpaceForEmail_WhenRowStyleCountIsOne_ReturnsZero` | TLP with 1 RowStyle → returns `0`. Second guard sub-condition; boundary. |
| T28 | `SpaceForEmail_WhenViewerHandleThrows_FallsBackToPrimaryScreen` | `SetupGet(x => x.Handle).Throws(new InvalidOperationException())` → does not throw and returns `(Screen.PrimaryScreen?.WorkingArea.Height ?? 0) - (rowHeight + frameHeight)`, computed in the test from the same source. **Covers the catch at lines 107–110.** |
| T29 | `SpaceForEmail_WhenScreenResolves_SubtractsRowHeightAndFrameHeight` | `Size` 800x600, `ClientSize` 780x580, `RowStyles[1].Height` 100 → result equals `(Screen.PrimaryScreen?.WorkingArea.Height ?? 0) - 120`. Self-consistent, host-independent. Replaces the magnitude assertion pattern of the existing test (finding F-5). |

### 6.5 `ItemsPerIteration` and `LoadItemsPerIteration`

Target: `QfcFormControllerSetupTests.cs`

| # | Test method | Scenario → expected outcome |
| --- | --- | --- |
| T30 | `ItemsPerIteration_WhenSetAndInvokeDispatches_WritesValueToViewerSpinner` | `Invoke` callback dispatching via `DynamicInvoke`, `SetupProperty(x => x.ItemsPerLoadValue)` → setting `7` leaves `ItemsPerLoadValue == 7m`. **Covers the inner action at line 135.** |
| T31 | `ItemsPerIteration_WhenAlreadyInitialized_GetDoesNotCallLoadItemsPerIteration` | `_itemsPerIteration` seeded to 4 → getter returns 4 and `Invoke` is never called (`Times.Never`), proving the `x != -1` predicate short-circuits. |
| T32 | `ItemsPerIteration_WhenUninitializedSentinel_GetCallsLoadItemsPerIteration` | `_itemsPerIteration` at its `-1` default, `_rowStyleTemplate` height 100, `L1v_TableLayout` null → getter returns `0` and `Invoke` called once. Covers the sentinel branch at 124. |
| T33 | `LoadItemsPerIteration_WhenSpaceForEmailIsZero_ReturnsZeroAndPushesToSpinner` | Same arrangement, called directly, `Invoke` dispatching → returns `0` and `ItemsPerLoadValue == 0m`. **Covers lines 141–147.** Deterministic on any host. |
| T34 | `LoadItemsPerIteration_WhenRowStyleTemplateIsNull_ThrowsNullReferenceException` | `_rowStyleTemplate` null → `Should().Throw<NullReferenceException>()`. Negative; documents the unguarded dereference at 142. |
| T35 | `LoadItemsPerIteration_WhenRowStyleTemplateHeightIsZero_DoesNotThrow` | `_rowStyleTemplate` height 0 → `Should().NotThrow()` and `Invoke` called once. Boundary (float division by zero); asserts only the non-throwing contract, not the numeric result. |

### 6.6 `RegisterFormEventHandlers` — guard clauses one per condition, then body

Target: `QfcFormControllerDisposalTests.cs`

| # | Test method | Scenario → expected outcome |
| --- | --- | --- |
| T36 | `RegisterFormEventHandlers_WhenControlsIsNull_DoesNotSubscribeIntentEvents` | `Controls` returns null → `VerifyAdd(x => x.OkClicked += It.IsAny<EventHandler>(), Times.Never)`. Guard condition 1. |
| T37 | `RegisterFormEventHandlers_WhenParentIsNull_DoesNotSubscribeIntentEvents` | `_parent` nulled via reflection → never subscribes. Guard condition 2. |
| T38 | `RegisterFormEventHandlers_WhenKeyboardHandlerIsNull_DoesNotSubscribeIntentEvents` | `_parent.KeyboardHandler` returns null → never subscribes. Guard condition 3. |
| T39 | `RegisterFormEventHandlers_WhenExclusionListIsNull_ThrowsArgumentNullException` | Guard satisfied but `GetKeyEventExclusionControls()` unstubbed (returns null) → `.ToList()` at line 167 throws `ArgumentNullException`. Error handling. |
| T40 | `RegisterFormEventHandlers_WhenChildControlPresent_RoutesKeyDownToKeyboardHandler` | Host with one `KeyRaisingControl`; register; `child.RaiseKeyDown(...)` → `mockKbd.Verify(k => k.KeyboardHandler_KeyDownAsync(It.IsAny<object>(), It.IsAny<KeyEventArgs>()), Times.Once)`. **Covers lines 163–165.** |
| T41 | `RegisterFormEventHandlers_WhenChildControlPresent_RoutesPreviewKeyDownToKeyboardHandler` | Same; `child.RaisePreviewKeyDown(...)` → `KeyboardHandler_PreviewKeyDownAsync` once. **Covers lines 158–161.** |
| T42 | `RegisterFormEventHandlers_WhenChildIsInExclusionList_DoesNotWireKeyEvents` | `GetKeyEventExclusionControls()` returns `{ child }` → raising `KeyDown` never reaches the handler, while the five intent events are still subscribed. Covers the `except` branch in `WinFormsExtensions.cs:94`. |

### 6.7 `UnregisterFormEventHandlers`

Target: `QfcFormControllerDisposalTests.cs`

| # | Test method | Scenario → expected outcome |
| --- | --- | --- |
| T43 | `UnregisterFormEventHandlers_WhenControlsIsNull_DoesNotUnsubscribeIntentEvents` | `Controls` null → `VerifyRemove(x => x.OkClicked -= It.IsAny<EventHandler>(), Times.Never)`. Guard condition 1. |
| T44 | `UnregisterFormEventHandlers_WhenKeyboardHandlerIsNull_DoesNotUnsubscribeIntentEvents` | `KeyboardHandler` null → never unsubscribes. Guard conditions 2–3. |
| T45 | `UnregisterFormEventHandlers_WhenPreviouslyRegistered_UnsubscribesAllFiveIntentEvents` | Register then Unregister → `VerifyRemove(..., Times.Once)` for `OkClicked`, `CancelClicked`, `UndoClicked`, `ItemsPerLoadValueChanged`, `SkipClicked`. Covers 198–202. |
| T46 | `RegisterThenUnregister_WhenKeyRaised_KeyboardHandlerIsNotInvoked` | Register, Unregister, then `child.RaiseKeyDown(...)` → `Times.Never`. **Covers lines 186–194** and is the `Register → Unregister` state-transition invariant. |
| T47 | `UnregisterFormEventHandlers_WithoutPriorRegister_DoesNotThrow` | Unregister on a freshly constructed controller with a register-capable viewer → no throw (`-=` on an unsubscribed handler is legal). State transition. |

### 6.8 `Cleanup` and the state-transition invariants

Target: `QfcFormControllerDisposalTests.cs`

| # | Test method | Scenario → expected outcome |
| --- | --- | --- |
| T48 | `Cleanup_WhenOlPresent_UnsubscribesFromPropertyChangedOnce` | `g.Ol` stubbed → `VerifyRemove(o => o.PropertyChanged -= It.IsAny<PropertyChangedEventHandler>(), Times.Once)`. **Covers lines 211–213**, the only uncovered lines of `Cleanup`. |
| T49 | `Cleanup_WhenOlIsNull_SkipsUnsubscriptionAndStillCompletes` | `g.Ol` null → completes; private `_globals` is null afterwards. Negative branch at 210. |
| T50 | `Cleanup_WhenInvoked_DisposesFormViewerExactlyOnce` | → `_mockFormViewer.Verify(x => x.Dispose(), Times.Once)` and private `_formViewer` is null. |
| T51 | `Cleanup_WhenInvoked_InvokesParentCleanupExactlyOnce` | Pass a counting `System.Action` as `parentCleanup` → counter is 1 and private `_parentCleanup` is null. |
| T52 | `Cleanup_WhenInvoked_NullsGlobalsFormViewerGroupsParentAndMovedItems` | → all five private fields (`_globals`, `_formViewer`, `_groups`, `_parent`, `_movedItems`) plus `_rowStyleTemplate`, `WriteMetrics`, `Iterate` are null. Covers 217–225 with real assertions (today they are executed but unasserted). |
| T53 | `Cleanup_WhenCalledTwice_DoesNotThrow` | Two consecutive calls → `Should().NotThrow()`. **Pins the idempotence established in §5.6.** |
| T54 | `Cleanup_WhenCalledTwice_InvokesParentCleanupOnlyOnce` | Counting action → counter is exactly 1 after two calls. State-transition invariant; guards against a future regression that reorders line 227. |
| T55 | `Cleanup_WhenCalledTwice_DisposesFormViewerOnlyOnce` | → `Verify(x => x.Dispose(), Times.Once)`. |
| T56 | `Cleanup_WhenCalledTwice_UnsubscribesFromPropertyChangedOnlyOnce` | `g.Ol` stubbed → `VerifyRemove(..., Times.Once)` after two calls (the second call cannot reach 212 because `_globals` is null). |
| T57 | `Cleanup_BeforeAnySetup_CompletesWithoutThrowing` | Freshly constructed controller, no `Init()` → no throw. Dispose-before-setup invariant. (Supersedes the assertion-free `Cleanup_ShouldCleanupResources`.) |
| T58 | `Cleanup_AfterRegisterFormEventHandlers_UnsubscribesKeyHandlersBeforeNullingViewer` | Register with a child control, then `Cleanup()`, then `child.RaiseKeyDown(...)` → keyboard handler never invoked. Proves line 215 runs **before** line 219 nulls `_formViewer` — an ordering invariant that a naive refactor would break. |
| T59 | `RegisterThenUnregisterThenCleanup_CompletesWithoutThrowing` | Full `Register → Unregister → Cleanup` sequence → no throw; `_formViewer` null; `Dispose()` once. The explicit three-step sequence invariant. |
| T60 | `InitThenCleanupThenInit_CompletesWithoutThrowing` | `Init()`, `Cleanup()`, `Init()` → no throw; the second `Init()` short-circuits every step on the nulled `_formViewer`. Cross-file state transition (`Init` is declared in `QfcFormController.cs`; this case asserts the *disposal-side* invariant and belongs here). |

### Coverage arithmetic

The 44 prior-uncovered lines map as: `60,62,65–72` → T11/T12/T14/T16; `82,83,84` → T20/T21/T23; `95,96` → T26/T27; `107–110` → T28; `141–147` → T33; `158–161,163–166` → T40/T41; `186–194` → T46; `211–213` → T48. Projected result is effectively 100% of measurable lines, well clear of the 80% floor. Branch coverage rises sharply because the guard sub-conditions currently at 0% (`RemoveTemplatesAndSetupTlp` condition 2 at line 50; `SetupLightDark` conditions 3–4 at line 77) are each given a dedicated case.

---

## 7. Determinism and policy notes

- **Framework/libraries:** MSTest (`[TestClass]`/`[TestMethod]`), Moq, FluentAssertions. `QuickFiler.Test.csproj` already references `MSTest.TestFramework` 4.3.3 and `MSTest.Analyzers` 4.3.3 (lines 312–316, 433–434). No package change.
- **Arrange–Act–Assert** in every test, with a leading comment or XML doc naming the scenario.
- **Banned APIs:** no `Thread.Sleep`, no `Task.Delay`, no `DateTime.Now`/`UtcNow`, no `Random.Shared`, no wall-clock waits anywhere in the proposed set. Satisfies `.claude/rules/general-unit-test.md` § "Determinism Infrastructure" and the repo `BannedSymbols.txt`.
- **`CancellationTokenSource` is acceptable** and is the fixture's existing `_tokenSource`. No token is cancelled on a timer.
- **No temporary files, no external services, no network.** Note that `QfcFormControllerSeamTests.ReadControllerSource`/`ResolveRepositoryPath` (lines 59–84) reads production `.cs` files off disk for a structural assertion. Do not replicate that pattern; every case above is behavioral.
- **No live forms, no popups.** Nothing in this file calls `MessageBox.Show` (those are in `Actions.cs`) or constructs a `Form`. **Confirmed by reading all 232 lines: no code path in this file constructs a real WinForms form.** The only WinForms types instantiated are `System.Drawing.Size` (66, 312-equivalent), `System.Action` (135, 144), and the two `EventHandler` delegate types at 159/163 and 187/191 — none is a form or a shown control.
- **In-memory controls are constructed by the tests, not by production.** `TableLayoutPanel`, `Control`, `Panel`, and the `KeyRaisingControl` test double are created and never shown, and no handle is ever forced (`InvokeRequired` stays false, so `TableLayoutHelper` runs inline).
- **STA last-resort clause: NOT invoked, with a documented fallback.** Existing `QuickFiler.Test` tests already construct `TableLayoutPanel` and `Control.ControlCollection` in ordinary non-STA `[TestClass]`es and pass (`QfcFormControllerSeamTests.cs:291`, `:107`; `QfcFormControllerTests.cs:215`, `:540`), so the proposed cases require no apartment change. **If a specific manipulation turns out to require STA at execution time**, the epic's Shared Design § 3 clause applies: move only those cases into a dedicated `QuickFiler.Test/Controllers/QfcFormControllerSetupDisposal.StaTests.cs` marked `[STATestClass]`, keeping the STA surface minimal and documenting per test why no seam is feasible. Verified enabler: `[STATestClass]`/`[STATestMethod]` ship inside `Microsoft.VisualStudio.TestTools.UnitTesting` in MSTest.TestFramework 4.3.3 — `UtilitiesCS.Test/HelperClasses/TableLayoutHelper_Tests.cs:10` uses `[STATestClass]` with only `MSTest.TestFramework 4.3.3` in its `packages.config` (line 147), and `QuickFiler.Test` references the same version. **No new package would be required.** This remains a genuine last resort, not a default.
- **Independence / ordering:** every test builds its own controller and mocks. The assembly's one global-state hazard is `UtilitiesCS.UiThread`'s statics — `QfcHomeControllerRunAsyncTests.cs:329` calls `UiThread.Init(false)`, which constructs and shows a hidden `SyncContextForm` (`UiThread.cs:48-79`). None of the proposed tests reads or writes `UiThread`, and `SetupLightDark`'s synchronous `Theme.SetTheme()` path never reaches `UiThread.Dispatcher` (§5.2). The CLI runsettings (`scripts/vscode/TaskMaster.cli.runsettings`) runs classes in parallel (`<Workers>0</Workers>`, `<Scope>ClassLevel</Scope>`), so no test may mutate process-global state.

---

## 8. Upstream dependency on F1

F1 (`quickfiler-coverage-denominator-and-exemption-ledger`, wave 0, C3) is F6's only `depends_on` (epic.md `features[5].depends_on: [1001]`). Its two outputs consumed by this file's verification:

1. **The ratified exemption ledger** at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` is the **authority** on whether `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` is `testable` or `ratified-exempt`. This research expects `testable`: the file carries no `[ExcludeFromCodeCoverage]`, has no direct dependency on `Microsoft.Office.Interop.Outlook.Application`/`MailItem`/`Store`/`MAPIFolder` (the `using Microsoft.Office.Interop.Outlook;` at line 9 is inherited boilerplate shared by all four partials and is unused in this file's body), and section 4 shows every line reachable behind existing interfaces. **The ledger does not exist on disk yet; its absence is expected and is not a gap or a blocker.**
2. **The per-file coverage harness**, derived from the Cobertura output of `Invoke-MSTestWithCoverage.ps1`. Both scripts exist today and were read:
   - `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8220048ded06d508\scripts\vscode\Invoke-MSTestWithCoverage.ps1` (349 lines)
   - `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8220048ded06d508\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1`

### Concrete command and output path the plan will cite

```powershell
pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 `
  -SearchRoot 'QuickFiler.Test' `
  -Configuration 'Debug' `
  -CoverageOutput 'docs\features\active\2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435\evidence\qa-gates\coverage-final.cobertura.xml'
```

Mechanics verified by reading the script:

- `-CoverageOutput` is resolved relative to the repo root (`Join-Path $repoRoot $CoverageOutput`, line 308); the parent directory is created if missing (310–312).
- Test assemblies are discovered as `*.Test.dll` under `<repoRoot>\<SearchRoot>`, filtered to `\bin\Debug\` and excluding `\obj\` and `\ref\` (296–302).
- `vstest.console.exe` is located via `vswhere` (279–290); the global `dotnet-coverage` tool is required (292–294).
- Invocation shape (70–77): `dotnet-coverage collect --output <path> --output-format cobertura --settings <derived config> -- <vstest> <assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`.
- The canonical repo-root `coverage.config` is read but **never written**; a derived copy adding only the `.*\.Test\.dll$` module exclusion is written beside the output and removed in a `finally` (79–116, 198–242). This is why F6 can run the harness without breaching F1's ownership of `coverage.config`.
- The Cobertura XML is post-processed for Koverage compatibility (338–343).

**Per-file lookup key.** In the committed #424 artifact the element is:

```xml
<class name="QuickFiler.Controllers.QfcFormController"
       filename="QuickFiler\Controllers\QfcFormController.SetupDisposal.cs"
       line-rate="..." branch-rate="..."/>
```

so the per-file rate is read from the `<class>` whose `filename` is `QuickFiler\Controllers\QfcFormController.SetupDisposal.cs`. Note that `ConvertTo-KoverageRelativePath` (Helpers, line 95) returns forward-slash paths for some elements, so the plan should match on a separator-insensitive comparison rather than hard-coding a backslash.

**Evidence location.** Per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, the numeric per-file result is committed under `docs/features/active/2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435/evidence/qa-gates/`. Epic.md § "Per-file coverage measurement" confirms that aggregate assembly coverage alone does not satisfy any child's acceptance criteria.

---

## 9. Open questions / findings

### Answered — double-dispose

**`Cleanup()` called twice does NOT throw.** Full line-by-line trace in §5.6. Idempotence is achieved by the null-conditional operators at lines 210, 215-via-179, 216, 218, and 226 combined with the field nulling at 217/219/227. **There is no defect, no behavior fix to propose, and no tension with the "no behavior change" acceptance criterion.** The correct action is to pin the invariant with T53–T56.

### F-1 — `_undoQueue` disposed but not nulled (cross-file)

`Cleanup()` line 216 disposes `_undoQueue` without nulling it. `Actions.cs:232` (`UndoDialog`) calls `_undoQueue.Add(...)`. `ButtonUndo_Click` after `Cleanup()` would therefore throw `ObjectDisposedException`. That path lives in `QfcFormController.Actions.cs`, owned by a sibling researcher — flagged here so it is not lost. **Recommendation:** the `Actions.cs` artifact should carry the decision; if neither artifact claims it, promote to a separate issue.

### F-2 — asymmetric field reset in `Cleanup()`

`_rowStyleTemplate` and `_groups` are nulled; `_qfcQueue`, `_states`, `_itemsPerIteration`, `_rowStyleExpanded`, `_itemMarginTemplate`, `_themes`, `_activeTheme`, `_darkMode`, `_token`, `_tokenSource`, `_helperTasks`, `_undoConsumerTask` are not. The asymmetry is undocumented. Informational; no defect observed on any live path. Do not "tidy" it in F6 — nulling `_tokenSource` or `_states` would be an observable behavior change.

### F-3 — `Init()` / `SetupLightDark` / `RegisterFormEventHandlers` are not idempotent

A second `Init()` adds a second `IOlObjects.PropertyChanged` subscription (line 84) and a second copy of all five form intent subscriptions (170–174) plus every per-control key subscription, so a single OK click would run `ActionOkAsync` twice. `QfcHomeController` is the only production caller and calls `Init()` once, so this is latent. **Recommendation:** document with T25/T42-adjacent cases (T25 asserts the current double-subscription), do **not** fix in F6 (a guard changes observable subscription counts), and promote to a separate GitHub issue via the MCP promotion lifecycle.

### F-4 — pre-existing 500-line test-file violation

`QuickFiler.Test/Controllers/QfcFormControllerTests.cs` is 827 lines against a 500-line limit that explicitly covers test code. §3 recommends recording rather than fixing in F6. **Plan author must decide.**

### F-5 — environment-dependent assertion in an existing test

`QfcFormControllerTests.SpaceForEmail_ShouldReturnCorrectValue` (line 208) asserts `result > 0`. That holds only when `Screen.PrimaryScreen.WorkingArea.Height` exceeds `100 + frameSize.Height (20)` on the executing host. On a headless or session-0 agent where `Screen.PrimaryScreen` is null, line 115's `?? 0` makes the result `-120` and the test fails. **This is a latent flakiness source, not a current failure.** F6 should not modify that test (it lives in the file F6 is not touching), but the new T28/T29 must not repeat the pattern — they compute the expected value from the same `Screen.PrimaryScreen?.WorkingArea.Height ?? 0` source so the assertion holds on any host. **Recommendation:** promote "make `SpaceForEmail_ShouldReturnCorrectValue` host-independent" to a separate issue.

### F-6 — assertion-free legacy tests inflate apparent coverage

`RemoveTemplatesAndSetupTlp_ShouldSetupTlp`, `SetupLightDark_ShouldSetupThemes`, `RegisterFormEventHandlers_ShouldRegisterHandlers`, `UnregisterFormEventHandlers_ShouldUnregisterHandlers`, and `Cleanup_ShouldCleanupResources` (`QfcFormControllerTests.cs` lines 182, 195, 228, 241, 254) all have an empty Assert section containing only the placeholder comment *"// Add assertions based on the expected behavior of the method"*. They execute production code and therefore count toward the line-rate, but verify nothing. The new cases in §6 supersede them behaviorally. **Do not delete or weaken them in F6** (they are in the file F6 is not touching); flag for F16.

### OQ-7 (informational) — measured baseline must be re-taken

Current per-file numeric coverage for `QfcFormController.SetupDisposal.cs` **on this branch cannot be determined without running the toolchain.** The only number quoted in this document (0.70684) is explicitly attributed to the #424 branch artifact. The command in §8 produces the authoritative figure; the plan's Phase 0 must run it and commit the baseline under `evidence/baseline/` before any test is written.

### OQ-8 (informational) — no `[ExcludeFromCodeCoverage]` to remove here

Neither `QfcFormController.cs` nor `QfcFormController.SetupDisposal.cs` carries the attribute, so F6's acceptance criterion about removing `[ExcludeFromCodeCoverage]` applies only to `QfcExplorerController.cs` (323 lines), which is a sibling researcher's file.
