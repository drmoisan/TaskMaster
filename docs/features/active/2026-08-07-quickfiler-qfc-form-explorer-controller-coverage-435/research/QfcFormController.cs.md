# Per-File Coverage Research — `QuickFiler/Controllers/QfcFormController.cs`

## 1. Header

- **Timestamp:** `2026-08-07T22-00`
- **Production file:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8220048ded06d508\QuickFiler\Controllers\QfcFormController.cs`
- **Exact line count:** 196 lines (verified by full read; last line `196` is the closing namespace brace)
- **`[ExcludeFromCodeCoverage]`:** **NO.** The file contains no `ExcludeFromCodeCoverage` attribute anywhere. The type is declared `internal partial class QfcFormController : IQfcFormController` (line 19) with no attribute list.
- **Epic child:** F6 (`quickfiler-qfc-form-explorer-controller-coverage`, issue #435), wave 1, band C3.
- **Sibling partials of the same type (NOT owned by this artifact):** `QfcFormController.SetupDisposal.cs` (232), `QfcFormController.EventHandlers.cs` (399), `QfcFormController.Actions.cs` (302). All four compile into one `QfcFormController` type, so any test fixture reaches all four; anti-duplication across the four artifacts is mandatory.

### Prior measured coverage (indicative, not authoritative)

A committed Cobertura artifact from the in-flight #424 feature contains a real per-file measurement of this exact file:

- File: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`, line 19395
- `<class ... line-rate="0.755556" branch-rate="0.346154" complexity="28" name="QuickFiler.Controllers.QfcFormController" filename="QuickFiler\Controllers\QfcFormController.cs">`

Every line number in that `<class>` block matches the current source exactly (e.g. `174` = `ToggleOffNavigation`, `185` = `get_Token`, `191` = `get_TokenSource`), so the artifact is aligned with the file as it stands on this branch. **It is still an artifact from a different feature branch, not this child's evidence.** The authoritative current number must be produced by running F1's harness (section 8). Do not cite 0.755556 as this child's baseline without re-measuring.

Two useful facts follow from that artifact and are used throughout this document:

1. dotnet-coverage emits **one `<class>` element per (type, source file)** pair, so a partial class produces a separate, directly-readable per-file line-rate. This is the mechanism F1's per-file report is built on; no new tooling concept is required.
2. The precise uncovered line set is known and listed in section 4.

---

## 2. Current test coverage inventory

### 2.1 Search scope

`QfcFormController` was searched across the entire `QuickFiler.Test` tree. Eleven files match the string; nine of them (`QuickFiler.Test.csproj`, `QfcViewer_Test.cs`, and the seven `QfcHomeController*Tests.cs` files) reference only the **interface** `IQfcFormController` via `new Mock<IQfcFormController>()` or reference it inside a commented-out block (`QfcViewer_Test.cs` line 38). **None of them constructs the concrete `QfcFormController`.** Only two files exercise the production type:

- `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` (827 lines, 42 `[TestMethod]`)
- `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` (378 lines, 15 `[TestMethod]`)

### 2.2 Tests that reach a member declared in **this** file

| Test method | File | Production member(s) in `QfcFormController.cs` reached |
| --- | --- | --- |
| `QfcFormController_ShouldConstruct` | `QfcFormControllerTests.cs:118` | ctor (27–51); asserts `_globals`, `_qfcQueue`, `_initType`, `_parent`, `FormViewer`, `SetController` callback, `TokenSource`, `Token` |
| `ActiveTheme_ShouldGetAndSetCorrectly` | `QfcFormControllerTests.cs:283` | `ActiveTheme` get (102–105) and set (107–117) including the setter lambda (111–116). Injects `_themes` via reflection with a map of `Theme(name, empty dict)` |
| `DarkMode_ShouldGetAndSetCorrectly` | `QfcFormControllerTests.cs:298` | `DarkMode` get (133–142) and set (143–154); the setter lambda body 150–152 is NOT reached (the `_globals?.Ol is not null` inner branch is false) |
| `Groups_ShouldReturnCorrectValue` | `QfcFormControllerTests.cs:312` | `Groups` get (160) |
| `FormHandle_ShouldReturnCorrectValue` | `QfcFormControllerTests.cs:325` | `FormHandle` get (165) |
| `FormViewer_ShouldReturnCorrectValue` | `QfcFormControllerTests.cs:339` | `FormViewer` get (171) |
| `Token_ShouldReturnCorrectValue` | `QfcFormControllerTests.cs:352` | `Token` get (185) |
| `TokenSource_ShouldReturnCorrectValue` | `QfcFormControllerTests.cs:365` | `TokenSource` get (191) |
| `DarkMode_CheckedChanged_ShouldUpdateTheme` | `QfcFormControllerTests.cs:378` | Reaches `DarkMode` get and `ActiveTheme` set transitively (the handler itself lives in `EventHandlers.cs`) |
| Every other test in both files | — | Reaches the ctor only (all use a `CreateQfcFormController()` helper); several transitively touch `ActiveTheme`/`DarkMode` |
| All 15 tests in `QfcFormControllerSeamTests.cs` | `QfcFormControllerSeamTests.cs` | ctor only, from this file's perspective. Their assertions target `SetupDisposal.cs` and `EventHandlers.cs` members |

**Members in this file with ZERO direct test:** `Init()`, `LoadTheme()`, `ToggleOffNavigation(bool)`, `ToggleOffNavigationAsync()`, `ToggleOnNavigation(bool)`, `ToggleOnNavigationAsync()`, the `DarkMode` getter loader lambda, and the `DarkMode` setter lambda's `_globals?.Ol is not null` true-branch.

### 2.3 Existing fixture pattern (reuse this; do not invent a second one)

Both files use the same shape. `QfcFormControllerTests.cs:75-113`:

```csharp
private QfcFormController CreateQfcFormController()
{
    return new QfcFormController(
        _mockGlobals.Object, _mockFormViewer.Object, _mockQfcQueue.Object,
        QfEnums.InitTypeEnum.Sort, () => { }, _mockParent.Object,
        _tokenSource, _token);
}

[TestInitialize]
public void Setup()
{
    Console.SetOut(new DebugTextWriter());
    _mockGlobals = new Mock<IApplicationGlobals>();
    _mockAF = new Mock<IAppAutoFileObjects>();
    _mockAF.SetupSet(af => af.MaximizeQuickFileWindow = It.IsAny<System.Action>())
           .Callback<System.Action>(action => _maxQfWindow = action).Verifiable();
    _mockAF.SetupGet(_mockAF => _mockAF.MaximizeQuickFileWindow).Returns(_maxQfWindow);
    _mockGlobals.Setup(g => g.AF).Returns(_mockAF.Object);
    _mockFormViewer = new Mock<IQfcFormViewer>();
    _mockFormViewer.Setup(x => x.SetController(It.IsAny<IFilerFormController>()))
                   .Callback<IFilerFormController>(c => _filerFormController = c).Verifiable();
    _mockQfcQueue = new Mock<IQfcQueue>();
    _mockParent = new Mock<IQfcHomeController>();
    _tokenSource = new CancellationTokenSource();
    _token = _tokenSource.Token;
}
```

Load-bearing details:

- **`_mockGlobals.Setup(g => g.AF)` is mandatory.** The ctor executes `_globals.AF.MaximizeQuickFileWindow = MaximizeFormViewer;` (line 43) unguarded; a `Mock<IApplicationGlobals>` without an `AF` stub returns `null` and the ctor throws `NullReferenceException`.
- `SetController` needs no stub to avoid throwing (Moq no-ops), but the callback is how `QfcFormController_ShouldConstruct` proves the `this` reference was handed to the viewer.
- Private-state access is done by reflection helpers `GetPrivateField<T>` / `SetPrivateField<T>` duplicated verbatim in both files (`QfcFormControllerTests.cs:33-53`, `QfcFormControllerSeamTests.cs:37-57`). That duplication is the thing the new shared support file should eliminate.
- `CreateThemeMap()` (`QfcFormControllerTests.cs:60-73`) builds `Theme(name, new Dictionary<string, ThemeControlGroup>())` — an **empty** control-group map. This is deliberate and must be preserved: see section 5 on `UiThread.Dispatcher`.
- `Console.SetOut(new DebugTextWriter())` is called in both `[TestInitialize]` methods.

---

## 3. Test-file size finding

- **`QuickFiler.Test/Controllers/QfcFormControllerTests.cs` is 827 lines.** `.claude/rules/general-code-change.md` § "File Size Limit" states: *"No production code, test code, or reusable script file may exceed 500 lines."* Test code is explicitly in scope, and the listed exceptions (throwaway agent scripts, raw text fixtures, Markdown) do not apply. **The file is 327 lines over the limit today.** This is a pre-existing violation, recorded here as a finding.
- **New test cases must NOT be appended to `QfcFormControllerTests.cs`.** Doing so worsens an existing Blocking-class violation. There is already an in-repo precedent for exactly this decision: `QfcFormControllerSeamTests.cs:16-24` documents that it was created *"Kept in a separate TestClass so the pre-existing QfcFormControllerTests.cs file is not grown further."*
- `QfcFormControllerSeamTests.cs` at 378 lines has ~122 lines of headroom. It is **shared territory** — its Seam B region tests `RegisterFormEventHandlers` (my file's sibling `SetupDisposal.cs`) and its Seam D region tests `CaptureItemSettings` (also `SetupDisposal.cs`), while `LoadItemsAsync_MailItemPath_...` (line 353) reads `QfcFormController.Actions.cs` (a sibling researcher's file). Growing it invites a merge conflict with the `EventHandlers.cs` / `Actions.cs` researchers. **Do not grow it.**

### Recommended new test files (for THIS artifact's production file)

| New file | Purpose | Projected size |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcFormController.TestSupport.cs` | Shared, non-`[TestClass]` fixture builder + reflection helpers used by every new F6 test file. Mirrors the established `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` precedent (an `internal static class QfcItemControllerTestSupport` with `SetField`/`GetField`, plus a harness subclass). | ~140 lines |
| `QuickFiler.Test/Controllers/QfcFormControllerCoreTests.cs` | All new cases for `QfcFormController.cs` only: ctor, `Init()`, `ActiveTheme`, `LoadTheme`, `DarkMode`, `Groups`, `FormHandle`, `FormViewer`, the four `Toggle*Navigation*` members, `Token`, `TokenSource`. | ~330 lines |

Naming rationale: one test file per production partial, named after the partial it covers, so a reader can map file→file. The sibling researchers should be directed to `QfcFormControllerEventHandlersTests.cs` and `QfcFormControllerActionsTests.cs`, and the `SetupDisposal.cs` artifact proposes two files (see that artifact); those four names plus the two above are pairwise disjoint, so no two F6 plan phases write to the same test file.

### Is splitting the existing 827-line file in scope for F6?

**Recommendation: NO — record it as a separate finding, do not split it in this child.**

Rationale:

1. `QfcFormControllerTests.cs` mixes tests for all four `QfcFormController.*` partials. Splitting it correctly requires simultaneously deciding the destination of tests owned by the `EventHandlers.cs` and `Actions.cs` researchers. That is a four-way edit to one file, executed concurrently by four plan phases — the exact conflict shape the epic's "partial-class families stay together" rule (epic.md § Decomposition Rationale) was written to avoid, but at test-file granularity.
2. The epic's file-size NFR is worded for production: *"No production file exceeds 500 lines after refactor"* (epic.md front-matter `nfrs`). The 500-line rule in `.claude/rules/general-code-change.md` is broader, but the pre-existing violation is not caused by this child.
3. Leaving the file untouched keeps F6's diff free of any test relocation, which keeps the merge into `epic/quickfiler-per-file-coverage-integration` clean for all 14 wave-1 siblings.

**Action for the plan author:** promote "split `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` (827 lines) into per-partial files under the 500-line limit" to its own GitHub issue via the MCP promotion lifecycle, and note it in F6's `spec.md` as an out-of-scope finding. If the epic capstone F16 wants it inside the epic, it belongs there (F16 already touches nothing else), not in F6.

---

## 4. Member-by-member reachability table

Legend: **Covered** = at least one existing test executes every line; **Partial** = some lines/branches executed; **Unreachable** = no existing test executes any line. The "prior uncovered lines" column quotes the #424 Cobertura artifact and is verifiable against it.

| # | Member (line span) | Kind | Status today | Uncovered lines (prior artifact) | Concrete blocker |
| --- | --- | --- | --- | --- | --- |
| 1 | `logger` static field (21–23) | static field init | Covered (via `.cctor`) | — | none |
| 2 | `QfcFormController(...)` ctor (27–51) | ctor | Covered (line-rate 1) | — | none. Negative/guard paths untested (see below) |
| 3 | `Init()` (53–61) | method | **Unreachable** | 54,55,56,57,58,60,61 | Not a hard blocker. `Init()` calls four `SetupDisposal.cs` methods, each of which returns early through a null guard when the viewer is a bare Moq mock. No test has ever called it. |
| 4 | `log` static field (67–69) | static field init | Covered (via `.cctor`) | — | none |
| 5 | `_undoQueue = []` (90), `_helperTasks = []` (92) | field initializers | Covered (via ctor) | — | none |
| 6 | `ActiveTheme` get (100–105) | property get | Partial (branch 1/2) | branch at 103 at 50% | The `_themes is null` false-branch is exercised; the `_themes is null` true-branch (returns raw `_activeTheme`) is not, or vice-versa. Trivially fixable. |
| 7 | `ActiveTheme` set (106–117) + lambda (111–116) | property set | Partial (branch 3/4 at 112) | — | The `_themes.TryGetValue` miss-branch is untested. |
| 8 | `LoadTheme()` (120–128) | internal method | **Unreachable** | 121,122,123,124,125,126,127,128 | Not a hard blocker. Requires `_themes` injected and optionally `_globals.Ol.DarkMode` stubbed. Never called directly by any test; only `SetupLightDark()` calls it in production. |
| 9 | `DarkMode` get (131–142) | property get | Partial | 138 (the `() => _globals.Ol.DarkMode` loader lambda) | The loader lambda runs only when `_globals?.Ol is not null` AND `_darkMode == false` (the `Initializer.GetOrLoad` default-equality check). No test stubs `g.Ol` while leaving `_darkMode` false. |
| 10 | `DarkMode` set (143–154) + lambda (148–153) | property set | Partial | 150,151,152 | Requires `_globals.Ol` stubbed so the lambda's `if (_globals?.Ol is not null)` is true and `_globals.Ol.DarkMode = x` executes. |
| 11 | `Groups` get (158–161) | property get | Covered | — | none |
| 12 | `FormHandle` get (163–166) | property get | Covered | — | none. NB: dereferences `_formViewer` unguarded — throws `NullReferenceException` after `Cleanup()`. |
| 13 | `FormViewer` get (169–172) | property get | Covered | — | none |
| 14 | `ToggleOffNavigation(bool)` (174) | expression-bodied method | **Unreachable** | 174 | `_groups` is `null` in every existing test that could call it → `NullReferenceException`. Requires a `Mock<IQfcCollectionController>` injected into `_groups`. There is existing precedent for exactly this injection (`QfcFormControllerTests.cs:512`). |
| 15 | `ToggleOffNavigationAsync()` (176) | async method | **Unreachable** | 176 | same as #14 |
| 16 | `ToggleOnNavigation(bool)` (178) | expression-bodied method | **Unreachable** | 178 | same as #14 |
| 17 | `ToggleOnNavigationAsync()` (180) | async method | **Unreachable** | 180 | same as #14 |
| 18 | `Token` get (183–186) | property get | Covered | — | none |
| 19 | `TokenSource` get (189–192) | property get | Covered | — | none |

**Total prior-uncovered line set for this file: 23 lines** — `54–58, 60, 61, 121–128, 138, 150–152, 174, 176, 178, 180`.

### Reachability verdict

**No member of `QfcFormController.cs` is genuinely unreachable.** Every uncovered line is reachable with the *existing* Moq'd `IQfcFormViewer` plus a `Mock<IQfcCollectionController>` injected into the private `_groups` field, and a `Mock<IOlObjects>` returned from `_mockGlobals.SetupGet(g => g.Ol)`. This file needs **zero new production seams**.

Verifications behind that verdict:

- `IQfcFormViewer` (`QuickFiler/Interfaces/IQfcFormViewer.cs`, 51 lines, `public interface IQfcFormViewer : IForm`, `namespace QuickFiler`) is a pure interface with Seam B/C/D members already added by issue #223. Every member this file touches (`SetController`, `Handle`) is interface-declared: `Handle` comes from `IWin32Window` via `IControl` (`UtilitiesCS/Interfaces/IWinForm/IControl.cs:13`). Moq can stub all of it.
- `IApplicationGlobals.Ol` is `IOlObjects` (`UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs:11`), and `IOlObjects : INotifyPropertyChanged` (`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:11`) — mockable, including `DarkMode` and the `PropertyChanged` event.
- `IAppAutoFileObjects.MaximizeQuickFileWindow` is `System.Action { get; set; }` and `MovedMails` is `SloStack<IMovedMailInfo> { get; }` (`UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs:31-32`) — both mockable; `MovedMails` returning Moq's default `null` is harmless for this file.
- `IQfcHomeController` (`QuickFiler/Controllers/IQfcHomeController.cs`) declares `Task WriteMetricsAsync(string filename)` (line 17) and `void Iterate()` (line 13). The ctor's `WriteMetrics = parent.WriteMetricsAsync;` and `Iterate = parent.Iterate;` are **method-group conversions off a mocked interface**, which Moq supports without any special setup. This is empirically confirmed: `QfcFormController_ShouldConstruct` already passes with a bare `Mock<IQfcHomeController>`.
- `Initializer` (`UtilitiesCS/HelperClasses/Initializer.cs`, `public static class Initializer`) is a **pure static helper with no I/O**: it does `EqualityComparer<T>.Default.Equals`, null checks over a `params object[]`, and `StackFrame(1, false).GetMethod().Name` only on failure paths. It is already directly unit-tested at `UtilitiesCS.Test/HelperClasses/Initializer_Tests.cs`. **It is testable as-is and requires no seam.** The overloads bound here are:
  - `ActiveTheme` get → `GetOrLoad<T>(ref T, Func<T>, bool strict, params object[])` (line 124) with `strict: true` and `dependencies = { _themes }`. Because the call site guards on `_themes is null` first, the strict throw at `DependenciesNotNull` is unreachable from this property.
  - `DarkMode` get → the same overload with `strict: false` and `dependencies = { _globals, _globals.Ol }`.
  - `ActiveTheme`/`DarkMode` set → `SetAndSave<T>(ref T, T, Action<T>)` (line 52), which always assigns the backing field and then invokes the setter action.

---

## 5. Seam design proposal

**Proposal for this file: introduce NO new production seam.** The hierarchy (`interface seam > injectable delegate > adapter`, `.claude/rules/csharp.md` § DI Seams) says to introduce the *smallest* seam that enables reliable testing; here the smallest is *none*, because the interface seam already exists.

Each blocker the orchestrator flagged, independently verified:

| Flagged blocker | Verified finding | Seam needed |
| --- | --- | --- |
| `_globals.AF.MaximizeQuickFileWindow = MaximizeFormViewer;` (line 43) | Real dereference, but `IAppAutoFileObjects` is an interface with a settable `System.Action` property. `QfcFormControllerTests.Setup()` already stubs it with `SetupSet(...).Callback(...)` and asserts the assignment. **Not a blocker.** | none |
| `_movedItems = _globals.AF.MovedMails;` (line 49) | `MovedMails` is an interface `get` returning `SloStack<IMovedMailInfo>`. Moq returns `null`; nothing in this file dereferences `_movedItems`. **Not a blocker for this file.** (It is dereferenced in `Actions.cs`/`UndoDialog`, which is a sibling's concern.) | none |
| `_formViewer.SetController(this)` (line 44) | Interface method on `IQfcFormViewer`. Already stubbed and asserted. **Not a blocker.** | none |
| `WriteMetrics = parent.WriteMetricsAsync` (47), `Iterate = parent.Iterate` (48) | Method-group conversions from a mocked interface; the private `delegate` types (`WriteMetricsDelegate` line 82, `IterateDelegate` line 84) are declared inside the class and are never dereferenced in this file. **Not a blocker.** | none |
| `Initializer.GetOrLoad` / `Initializer.SetAndSave` statics | Pure static helper, no I/O, already unit-tested in `UtilitiesCS.Test`. Located at `UtilitiesCS/HelperClasses/Initializer.cs`. **Testable as-is; not a blocker.** | none |

### One real constraint discovered (affects test *construction*, not production code)

`ActiveTheme`'s setter lambda calls `theme.SetTheme(async: true)` (line 114). Tracing that:

- `Theme.SetTheme(bool async)` → `ControlGroups.ForEach(cg => cg.Value.ApplyTheme(async))` (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:457-460`)
- `ThemeControlGroup.ApplyTheme(bool async)` → when `_controls is not null` and `async` is true → `UiThread.Dispatcher.InvokeAsync(...)` (`UtilitiesCS/HelperClasses/ThemeHelpers/ThemeControlGroup.cs:212-224`)
- `UiThread.Dispatcher` is a plain static field `private static Dispatcher _dispatcher = null!;` whose getter does **not** lazily initialize (`UtilitiesCS/Threading/UiThread.cs:135-140`) — unlike `UiSyncContext` and `AutoScaleFactor`, which do call `Init()`.
- `UiThread.Init()` → `Initialize()` **constructs and Shows a real WinForms form**: `_syncContextForm = new SyncContextForm(); ... _syncContextForm.Show();` (`UtilitiesCS/Threading/UiThread.cs:48-79`).

**Consequence:** a test that sets `ActiveTheme` while `_themes` holds a *populated* control-group map either throws `NullReferenceException` on the null `UiThread.Dispatcher`, or (if some other test already called `UiThread.Init`) depends on process-global, order-dependent static state and a shown form. Both violate the unit-test policy.

**Mitigation (no production change):** keep the existing `CreateThemeMap()` convention — `new Theme(name, new Dictionary<string, ThemeControlGroup>())` with an **empty** `ControlGroups` dictionary. `Theme.SetTheme(bool)` then iterates zero groups and `ThemeControlGroup.ApplyTheme` is never entered, so `UiThread` is never touched. Every proposed `ActiveTheme` test in section 6 uses this map. This is exactly what `ActiveTheme_ShouldGetAndSetCorrectly` already does; the convention must be documented in `QfcFormController.TestSupport.cs` so it is not accidentally broken.

**Cross-reference for the plan author:** `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` calls `UiThread.Init(false)` today, which creates and shows the hidden `SyncContextForm` for the whole test process. That is pre-existing global state owned by sibling F7, not F6. F6 must not depend on it in either direction (must not require it to have run, must not require it not to have run). Using empty control-group maps satisfies that.

### Hard constraints — compliance statement

- `QuickFiler/Viewers/QfcFormViewer.cs` (F15): **not edited, and no edit implied.** No member is added to `IQfcFormViewer` by this proposal.
- `QuickFiler/Controllers/KeyboardHandler.cs` (F3): **not edited.** This file does not reference it at all (only `SetupDisposal.cs` consumes `_parent.KeyboardHandler`, and it does so through the `IQfcKeyboardHandler` interface).
- `QuickFiler/Controllers/QfcCollectionController.cs` (F11): **not edited.** The four `Toggle*Navigation*` members are exercised through the `IQfcCollectionController` **interface** with a Moq mock injected into the private `_groups` field. The concrete class is never constructed.
- `coverage.config` and shared build property files (F1): **not modified.**
- **No `CROSS-CHILD CONTRACT NOTE` is required for this file.**
- .NET Framework 4.8 constraints (no default interface members, no `init`, no `record`/`record struct`): satisfied trivially — no new type is introduced.

### Projected line count after seam work

**196 lines — unchanged.** No production edit is proposed for `QfcFormController.cs`. Well under the 500-line limit.

---

## 6. Proposed test cases

All go in **`QuickFiler.Test/Controllers/QfcFormControllerCoreTests.cs`** unless stated otherwise. Each is one atomic plan task. Each uses the shared fixture from `QfcFormController.TestSupport.cs`.

### 6.0 Shared support (one task, prerequisite for the rest)

| # | Task | Target file |
| --- | --- | --- |
| T0 | Create `QfcFormControllerTestSupport`: `internal static` class exposing `SetField(QfcFormController, string, object)`, `GetField<T>(QfcFormController, string)`, `CreateThemeMap()` (empty `ControlGroups`, documented reason), and a `Build(...)` mock-bundle builder returning the controller plus its `Mock<IApplicationGlobals>`, `Mock<IAppAutoFileObjects>`, `Mock<IOlObjects>`, `Mock<IQfcFormViewer>`, `Mock<IQfcQueue>`, `Mock<IQfcHomeController>`, and `CancellationTokenSource`. No `[TestClass]`. Mirrors `QfcItemController.TestSupport.cs`. | `QfcFormController.TestSupport.cs` |

### 6.1 Constructor — positive, negative, boundary

| # | Test method | Scenario → expected outcome |
| --- | --- | --- |
| T1 | `Ctor_WhenAllDependenciesSupplied_AssignsMaximizeQuickFileWindowToMaximizeFormViewer` | Construct with full mocks → `IAppAutoFileObjects.MaximizeQuickFileWindow` is set exactly once (`VerifySet`, `Times.Once`) and the captured `System.Action`'s `Method.Name` equals `"MaximizeFormViewer"`. |
| T2 | `Ctor_WhenAllDependenciesSupplied_CopiesMovedMailsFromAutoFileObjects` | Stub `AF.MovedMails` with a non-null `SloStack<IMovedMailInfo>` → the private `_movedItems` field is the same instance. |
| T3 | `Ctor_WhenAllDependenciesSupplied_PassesItselfToViewerSetController` | → `IQfcFormViewer.SetController` invoked once with an argument reference-equal to the controller. |
| T4 | `Ctor_WhenAllDependenciesSupplied_CapturesParentMetricsAndIterateDelegates` | → private `WriteMetrics` and `Iterate` delegate fields are non-null and their `Target` is the parent mock. |
| T5 | `Ctor_WhenGlobalsAutoFileObjectsIsNull_ThrowsNullReferenceException` | `Mock<IApplicationGlobals>` with no `AF` stub → `Should().Throw<NullReferenceException>()`. **Documents current behavior** (line 43 is unguarded). See open question OQ-1. |
| T6 | `Ctor_WhenFormViewerIsNull_ThrowsNullReferenceException` | `formViewer: null` → throws at line 44 `_formViewer.SetController(this)`. Documents current behavior. |
| T7 | `Ctor_WhenParentIsNull_ThrowsNullReferenceException` | `parent: null` → throws at line 47 `parent.WriteMetricsAsync`. Documents current behavior. |
| T8 | `Ctor_WhenParentCleanupIsNull_ConstructsSuccessfully` | `parentCleanup: null` → no throw; private `_parentCleanup` is null. Boundary: `Cleanup()` later uses `?.Invoke()`. |
| T9 | `Ctor_WhenInitTypeIsSort_StoresInitTypeVerbatim` | Pass `QfEnums.InitTypeEnum.Sort` → private `_initType` equals it. (Guards the `ActionOkAsync` flag check in `Actions.cs`.) |
| T10 | `Ctor_WhenTokenSourceIsNull_ExposesNullTokenSourceWithoutThrowing` | `tokenSource: null` → construction succeeds and `TokenSource` returns null. Boundary/negative. |

### 6.2 `Init()`

| # | Test method | Scenario → expected outcome |
| --- | --- | --- |
| T11 | `Init_WhenViewerGuardsAllShortCircuit_ReturnsSameControllerInstance` | Bare mocks (all four setup methods hit their null guards) → returns a reference equal to the controller and does not throw. Covers lines 54–61. |
| T12 | `Init_WhenCalled_InvokesTheFourSetupStepsInOrder` | Arrange a viewer whose `L1v0L2L3v_TableLayout`, `Panels`, `Buttons`, `Controls` are all populated and record ordering via a `MockSequence` or callback-appended list → the observed order is `CaptureItemSettings`, `RemoveTemplatesAndSetupTlp`, `SetupLightDark`, `RegisterFormEventHandlers`. Asserted through observable viewer interactions (`Show`, `CaptureTlpCellStates`, `GetKeyEventExclusionControls`, `OkClicked +=`). |
| T13 | `Init_WhenCalledTwice_SubscribesFormIntentEventsTwice` | Call `Init()` twice with a register-capable viewer → `VerifyAdd(x => x.OkClicked += It.IsAny<EventHandler>(), Times.Exactly(2))`. **Documents the current double-subscription behavior**; see finding F-2. |
| T14 | `Init_AfterCleanup_ReturnsWithoutThrowing` | `Cleanup()` then `Init()` → no throw (all four steps short-circuit on the nulled `_formViewer`). State-transition invariant. |

### 6.3 `ActiveTheme` / `LoadTheme`

| # | Test method | Scenario → expected outcome |
| --- | --- | --- |
| T15 | `ActiveTheme_WhenThemesMapIsNull_GetReturnsBackingFieldWithoutCallingInitializer` | `_themes` left null, `_activeTheme` seeded via reflection to `"LightNormal"` → getter returns `"LightNormal"`. Covers the true-branch of line 103. |
| T16 | `ActiveTheme_WhenThemesMapPopulatedAndBackingFieldNull_LoadsViaLoadTheme` | `_themes` = empty-group map, `_activeTheme` null, `_globals.Ol.DarkMode` false → getter returns `"LightNormal"` and the backing field is now populated. Covers the false-branch of 103 plus `LoadTheme` 121–127. |
| T17 | `ActiveTheme_WhenSetToKnownTheme_StoresValueAndAppliesThatTheme` | Set `"DarkNormal"` with an empty-group map → getter returns `"DarkNormal"`, no throw. Covers 107–117 and lambda 111–116 true-branch. |
| T18 | `ActiveTheme_WhenSetToUnknownThemeName_StoresValueAndAppliesNothing` | Set `"NoSuchTheme"` → getter returns `"NoSuchTheme"`; `TryGetValue` miss-branch at line 112 taken; no throw. Negative/boundary. |
| T19 | `ActiveTheme_WhenSetWhileThemesMapIsNull_StoresValueWithoutThrowing` | `_themes` null, set `"DarkNormal"` → backing field updated, lambda's `_themes is not null` false-branch taken. Negative. |
| T20 | `ActiveTheme_WhenSetToNull_StoresNull` | Set `null` → backing field is null; no throw. Boundary. |
| T21 | `LoadTheme_WhenOlDarkModeIsTrue_ReturnsDarkNormalAndAppliesIt` | `_globals.Ol.DarkMode` = true, `_themes` = empty-group map → returns `"DarkNormal"`. Covers 122 dark branch and 123–125. |
| T22 | `LoadTheme_WhenOlDarkModeIsFalse_ReturnsLightNormal` | `_globals.Ol.DarkMode` = false → returns `"LightNormal"`. Covers 122 light branch. |
| T23 | `LoadTheme_WhenGlobalsOlIsNull_FallsBackToDarkModeBackingField` | `g.Ol` returns null, `_darkMode` seeded true via reflection → returns `"DarkNormal"`. Covers the `?? _darkMode` null-coalescing path at 122. |
| T24 | `LoadTheme_WhenThemesMapMissingRequestedTheme_ReturnsNameWithoutApplyingTheme` | `_themes` contains only `"LightNormal"` but `DarkMode` is true → returns `"DarkNormal"` and does not throw. Covers 123 miss-branch → 127. |
| T25 | `LoadTheme_WhenThemesMapIsNull_ReturnsNameWithoutThrowing` | `_themes` null → returns the computed name; covers 123's first sub-condition false. Negative. |

### 6.4 `DarkMode`

| # | Test method | Scenario → expected outcome |
| --- | --- | --- |
| T26 | `DarkMode_WhenGlobalsOlIsNull_GetReturnsBackingField` | `g.Ol` null, `_darkMode` seeded true → returns true without invoking `Initializer`. Covers 134 true-branch. |
| T27 | `DarkMode_WhenOlPresentAndBackingFieldFalse_GetLoadsFromOlDarkMode` | `g.Ol.DarkMode` = true, `_darkMode` false → returns true. **Covers line 138** (the loader lambda), the only uncovered line of the getter. |
| T28 | `DarkMode_WhenOlPresentAndBackingFieldAlreadyTrue_GetDoesNotReReadOl` | `_darkMode` seeded true → returns true and `Ol.DarkMode` getter is never invoked (`VerifyGet(..., Times.Never)`). Covers the `Initializer.GetOrLoad` short-circuit. |
| T29 | `DarkMode_WhenSetWithOlPresent_WritesThroughToOlDarkMode` | `g.Ol` stubbed with `SetupProperty` → setting `true` sets `Ol.DarkMode` to true. **Covers lines 150–152.** |
| T30 | `DarkMode_WhenSetWithOlNull_UpdatesBackingFieldOnly` | `g.Ol` null → setter succeeds, getter returns the new value, no throw. Covers 149 false-branch. Negative. |
| T31 | `DarkMode_WhenSetToFalseWithOlPresent_WritesFalseThrough` | Set `false` → `Ol.DarkMode` is false. Boundary (the `default(bool)` value that defeats `GetOrLoad` caching). |

### 6.5 Simple accessors and navigation delegation

| # | Test method | Scenario → expected outcome |
| --- | --- | --- |
| T32 | `Groups_BeforeAnyLoad_ReturnsNull` | Fresh controller → `Groups` is null. (Duplicates existing `Groups_ShouldReturnCorrectValue`; **omit** unless the existing file is split. Recorded for completeness only.) |
| T33 | `FormHandle_AfterCleanup_ThrowsNullReferenceException` | `Cleanup()` then read `FormHandle` → `Should().Throw<NullReferenceException>()`. Documents the unguarded dereference at line 165. Error-handling / state-transition. |
| T34 | `ToggleOffNavigation_WhenGroupsInjected_DelegatesToCollectionControllerWithAsyncFlag` | Inject `Mock<IQfcCollectionController>` into `_groups`, call `ToggleOffNavigation(true)` → `Verify(g => g.ToggleOffNavigation(true), Times.Once)`. **Covers line 174.** |
| T35 | `ToggleOffNavigation_WhenGroupsIsNull_ThrowsNullReferenceException` | No `_groups` → throws. Negative; documents the unguarded delegation. |
| T36 | `ToggleOffNavigationAsync_WhenGroupsInjected_AwaitsCollectionControllerCall` | Mock returns `Task.CompletedTask` → awaits and verifies once. **Covers line 176.** |
| T37 | `ToggleOnNavigation_WhenGroupsInjected_DelegatesToCollectionControllerWithAsyncFlag` | `ToggleOnNavigation(false)` → verified once with `false`. **Covers line 178.** |
| T38 | `ToggleOnNavigationAsync_WhenGroupsInjected_AwaitsCollectionControllerCall` | → verified once. **Covers line 180.** |
| T39 | `ToggleOffNavigationAsync_WhenCollectionControllerThrows_PropagatesException` | Mock's `ToggleOffNavigationAsync` returns a faulted task → `await act.Should().ThrowAsync<InvalidOperationException>()`. Error handling. |

### Coverage arithmetic

The 23 prior-uncovered lines are addressed as: `54–58,60,61` by T11–T14; `121–128` by T16 and T21–T25; `138` by T27; `150–152` by T29/T31; `174,176,178,180` by T34/T36/T37/T38. Projected result is 100% of the file's measurable lines, comfortably clearing the 80% floor. Branch coverage also rises materially (the 0.346 branch-rate is dominated by the wholly-unentered `LoadTheme`).

---

## 7. Determinism and policy notes

- **Framework/libraries:** MSTest (`[TestClass]`/`[TestMethod]`, `Microsoft.VisualStudio.TestTools.UnitTesting`), Moq for all mocks, FluentAssertions for all new assertions. `QuickFiler.Test.csproj` already references `MSTest.TestFramework` 4.3.3 and `MSTest.Analyzers` 4.3.3 (lines 312–316, 433–434), so no package change is needed.
- **Arrange–Act–Assert** in every test, with an XML-doc or leading comment naming the scenario.
- **Banned APIs:** none of the proposed tests use `Thread.Sleep`, `Task.Delay`, `DateTime.Now`/`UtcNow`, `Random.Shared`, or any wall-clock wait. `.claude/rules/general-unit-test.md` § "Determinism Infrastructure" and the repo `BannedSymbols.txt` are both satisfied.
- **`CancellationTokenSource` is acceptable** and is already the fixture's `_tokenSource`. T10 exercises the null case. No token is ever cancelled on a timer.
- **No temporary files, no external services, no network.** Note that `QfcFormControllerSeamTests.ReadControllerSource`/`ResolveRepositoryPath` (lines 59–84) reads production source off disk to make a structural assertion. That is an existing pattern owned by the `Actions.cs` researcher's test; **do not replicate it** for this file — every proposed test here is a behavioral test.
- **No live forms, no popups.** Nothing in `QfcFormController.cs` calls `MessageBox.Show` (those live in `Actions.cs`) or constructs a form. The one path that could transitively reach a shown form — `ActiveTheme` set → `Theme.SetTheme(async:true)` → `UiThread.Dispatcher` → `UiThread.Init()` → `SyncContextForm.Show()` — is closed by the empty-`ControlGroups` convention (section 5).
- **No real WinForms form is constructed by any code path in this file.** Verified by reading all 196 lines: the only `System.Windows.Forms` types referenced are `RowStyle` (73, 74) and `Padding` (76) as private field declarations, both assigned in `SetupDisposal.cs`, never `new`ed here.
- **STA last-resort clause: NOT INVOKED for this file.** No proposed test constructs any WinForms control, so no `*.StaTests.cs` file is needed for `QfcFormController.cs`. Recorded for the plan author: MSTest 4.3.3 ships `[STATestClass]`/`[STATestMethod]` in `Microsoft.VisualStudio.TestTools.UnitTesting` with no additional package — `UtilitiesCS.Test/HelperClasses/TableLayoutHelper_Tests.cs:10` uses `[STATestClass]` with only `MSTest.TestFramework 4.3.3` in its `packages.config` (line 147). So if a sibling F6 file genuinely needs STA, the attribute is available in `QuickFiler.Test` today.
- **Independence / ordering:** every test constructs its own controller and its own mocks in `[TestInitialize]`. The one shared-state hazard in the assembly is `UiThread`'s process-global statics, which these tests never touch. The CLI runsettings (`scripts/vscode/TaskMaster.cli.runsettings`) sets `<Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope>`, so classes run in parallel — another reason no test may mutate `UiThread`.

---

## 8. Upstream dependency on F1

F1 (`quickfiler-coverage-denominator-and-exemption-ledger`, wave 0, C3) is the sole `depends_on` of F6 (epic.md `features[5].depends_on: [1001]`). Its two outputs that this file's verification consumes:

1. **The ratified exemption ledger** at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. The ledger is the **authority** for whether `QuickFiler/Controllers/QfcFormController.cs` is classified `testable` or `ratified-exempt`. This research expects `testable` — the file carries no `[ExcludeFromCodeCoverage]`, has no live-COM dependency, and section 4 shows every line is reachable behind existing interfaces. If the ledger were to classify it otherwise, this artifact's section 6 becomes moot and the plan must instead cite the ledger rationale. **The ledger does not exist on disk yet; its absence is expected and is not a gap.**
2. **The per-file coverage harness.** F1 derives the per-file report from the Cobertura output of `Invoke-MSTestWithCoverage.ps1`, which exists today and was read for this research.

### Concrete command and output path the plan will cite

Both scripts were read and exist:

- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8220048ded06d508\scripts\vscode\Invoke-MSTestWithCoverage.ps1` (349 lines)
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8220048ded06d508\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1`

Command (run from repo root; `-CoverageOutput` is resolved relative to the repo root by `Join-Path $repoRoot $CoverageOutput` at line 308):

```powershell
pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 `
  -SearchRoot 'QuickFiler.Test' `
  -Configuration 'Debug' `
  -CoverageOutput 'docs\features\active\2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435\evidence\qa-gates\coverage-final.cobertura.xml'
```

Mechanics verified by reading the script:

- Discovers `*.Test.dll` under `<repoRoot>\<SearchRoot>` filtered to `\bin\Debug\`, excluding `\obj\` and `\ref\` (lines 296–302).
- Resolves `vstest.console.exe` via `vswhere` (284–290) and requires the global `dotnet-coverage` tool (292–294).
- Runs `dotnet-coverage collect --output <path> --output-format cobertura --settings <derived coverage.config> -- <vstest> <assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook` (70–77).
- `coverage.config` at repo root is the canonical instrumentation-exclusion file; the script writes a **derived** copy adjacent to the output (adding only the `.*\.Test\.dll$` module exclusion) and deletes it in a `finally` block. **The canonical `coverage.config` is never written** (lines 79–116, 198–242) — this is why F6 can run the harness without violating the F1 ownership constraint on `coverage.config`.
- Post-processes the Cobertura XML for Koverage (`ConvertTo-KoverageCoberturaXml`, lines 338–343).

**Per-file lookup key.** In the committed #424 artifact, the element for this file is:

```xml
<class name="QuickFiler.Controllers.QfcFormController" filename="QuickFiler\Controllers\QfcFormController.cs" line-rate="..." branch-rate="..."/>
```

so the per-file line-rate is read from the `<class>` element whose `filename` is `QuickFiler\Controllers\QfcFormController.cs` (backslash-separated in that artifact). The plan should confirm the exact separator against F1's harness output rather than hard-coding it, because `ConvertTo-KoverageRelativePath` (Helpers, line 95) returns forward-slash paths for some elements.

**Evidence location.** Per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, the numeric per-file result is committed under `docs/features/active/2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435/evidence/qa-gates/`. No other path is acceptable. Epic.md § "Per-file coverage measurement" confirms aggregate assembly coverage alone does not satisfy any child's acceptance criteria.

---

## 9. Open questions / findings

### OQ-1 (decision required) — constructor precondition guards

`CLAUDE.md` § C#4.3 requires "Validate constructor and method preconditions," and `.claude/rules/general-code-change.md` § Error Handling requires "Enforce invariants at construction/initialization time." The ctor today throws `NullReferenceException` (not `ArgumentNullException`) for a null `appGlobals.AF`, a null `formViewer`, or a null `parent` (lines 43, 44, 47).

Adding `ArgumentNullException` guards would change the exception *type* on an already-fatal path, which is in tension with F6's AC "No behavior change to observable QuickFiler flows."

**Recommendation:** do **not** add guards in F6. Have T5–T7 assert the current `NullReferenceException` behavior (which is honest, deterministic, and adds coverage), and promote "add explicit `ArgumentNullException` preconditions to `QfcFormController`'s constructor" to its own GitHub issue via the MCP promotion lifecycle. Plan author decides.

### F-2 (finding, no fix proposed) — `Init()` is not idempotent

`Init()` (53–61) unconditionally calls `SetupLightDark()` and `RegisterFormEventHandlers()`. Reading `SetupDisposal.cs`: `SetupLightDark` does `_globals.Ol.PropertyChanged += DarkMode_CheckedChanged;` (line 84) and `RegisterFormEventHandlers` does five `+=` subscriptions (lines 170–174) plus per-control `PreviewKeyDown`/`KeyDown` subscriptions. Neither unsubscribes first. **A second `Init()` therefore double-subscribes every form intent event**, so a single OK click would run `ActionOkAsync` twice.

There is no evidence in the codebase that production calls `Init()` twice (`QfcHomeController` is the only caller, via its `QfcFormControllerLoader`), so this is latent, not live. T13 documents the current behavior rather than asserting the desired behavior. **Recommendation:** promote to a separate issue; do not fix inside F6, because a guard changes observable subscription counts.

### F-3 (finding) — pre-existing 500-line test-file violation

`QuickFiler.Test/Controllers/QfcFormControllerTests.cs` is 827 lines against a 500-line limit that explicitly covers test code. Section 3 recommends recording rather than fixing in F6. **Plan author must decide** whether to accept the recommendation or bring the split into F6 (which would require coordinating with the `EventHandlers.cs` and `Actions.cs` plan phases).

### F-4 (finding) — tautological legacy assertions in the shared test file

`QfcFormControllerTests.cs:688-701` (`UndoConsumer_ShouldConsumeUndoQueue`) is `await Task.CompletedTask;` plus a `#pragma warning disable MSTEST0032`-suppressed `Assert.IsTrue(true)`. Several other tests in that file have an empty Assert section with only a `// Add assertions based on the expected behavior of the method` comment (e.g. lines 182–192, 194–205, 227–251, 253–264). These are not in this file's scope (they target `Actions.cs`/`SetupDisposal.cs`), but the plan author should know that **existing "coverage" of some members is coverage without verification**, and the per-file line-rate therefore overstates real assurance. Do not delete or weaken them in F6; note them for F16.

### OQ-5 (informational) — measured baseline must be re-taken

Current per-file numeric coverage for `QfcFormController.cs` on this branch **cannot be determined without running the toolchain**. No number in this document other than the explicitly-attributed 0.755556 from the #424 artifact is a measurement, and that number is from a different branch. The command in section 8 is what produces the authoritative figure; the plan's Phase 0 must run it and commit the baseline to `evidence/baseline/` before any test is written.

### OQ-6 (informational) — duplicate `IQfcFormController.cs`

Two files named `IQfcFormController.cs` exist: `QuickFiler/Controllers/IQfcFormController.cs` (43 lines, `public interface IQfcFormController : IFilerFormController`, `namespace QuickFiler.Controllers`) and `QuickFiler/Interfaces/IQfcFormController.cs` (25 lines). The concrete class in this file implements the `Controllers` one (line 19 resolves in `namespace QuickFiler.Controllers`). Both are assigned to F6 but to a **sibling researcher's** artifact; the determination the issue asks for belongs there, not here. Recorded only so the plan author does not lose it.
