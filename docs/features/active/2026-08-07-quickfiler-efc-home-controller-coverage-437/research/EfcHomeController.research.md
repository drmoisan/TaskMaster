# Per-File Research — `QuickFiler/Controllers/EfcHomeController.cs`

- **Feature:** `2026-08-07-quickfiler-efc-home-controller-coverage-437` (issue #437)
- **Epic:** #136 `quickfiler-per-file-coverage`, child F8
- **Production file:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aea998f94efaa2eb4\QuickFiler\Controllers\EfcHomeController.cs` (441 lines)
- **Research date:** 2026-08-07
- **Method:** static read of the production file, of the four sibling partials, of the dependency
  contract, and of all seven existing `EfcHomeController*` test files. No build and no test run was
  performed (research-only mandate).

---

## 0. Constraints restated

These constraints govern every recommendation below and are restated verbatim per the delegation:

1. `EfcHomeControllerDependencies` and `EfcHomeControllerDependencyFactories` are the injection-seam
   contract for the **whole** EFC controller family, including `EfcFormController` and
   `EfcItemController`, which belong to **sibling child F9**. This child must not propose or apply
   edits to F9's files. Any dependency-contract change must be **additive** so that F9 needs no
   edit. Where an additive change is impossible for a given gap, that gap is flagged below as a
   **cross-child contract note** rather than resolved here.
2. No change to `coverage.config` or to any shared build property file.
3. Tests: MSTest, Moq, FluentAssertions, deterministic, isolated, no temporary files, no external
   services, no live WinForms forms, no popups, no live Outlook store.
4. Seam hierarchy: **interface seam > injectable delegate > adapter**. Never construct a live form.
5. No production file may exceed 500 lines.
6. Upstream dependency **F1 `quickfiler-coverage-ledger`** delivers (a) the per-file coverage
   harness that is the sole per-file coverage evidence mechanism for this epic, and (b) the ratified
   exemption ledger at
   `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`, which is the future
   authority for whether any file is `ratified-exempt`. F1's outputs **do not exist on disk yet**
   and were not read or executed during this research. Every numeric coverage figure in this
   artifact is a **static estimate** and must be replaced by F1-harness output before any acceptance
   criterion is closed.
7. `EfcHomeController.cs` carries **no** `[ExcludeFromCodeCoverage]` attribute, so it is already in
   the coverage denominator. Nothing in this artifact proposes adding one.

**Evidence location:** all coverage evidence produced by this child goes to
`docs/features/active/2026-08-07-quickfiler-efc-home-controller-coverage-437/evidence/qa-gates/`
per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Non-canonical evidence paths
(`artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`) are rejected.

---

## 1. Current state — what the file actually is

`EfcHomeController.cs` is the primary partial of `public partial class EfcHomeController :
IFilerHomeController`. The other three partials in this child are `.Metrics.cs` (87),
`.ExecuteMoves.cs` (144), and `.Timing.cs` (43); the two dependency files are
`EfcHomeControllerDependencies.cs` (428) and `EfcHomeControllerDependencyFactories.cs` (268). Each
gets its own research artifact per the per-file mandate of #136.

The file is **already heavily seamed**. Every construction dependency is routed through the
`EfcHomeControllerDependencies` delegate bundle, and the three host-bound UI operations are routed
through settable instance delegates. That is why 2,502 lines of existing tests exist against this
family and why the residual gap is small and specific.

### 1.1 Existing seam inventory (all already present, nothing new required for most of the file)

| Seam | Kind | Location | Already used by tests |
| --- | --- | --- | --- |
| `_defaultDependenciesFactory` + `SetDefaultDependenciesFactory` / `ResetDefaultDependenciesFactory` | static injectable delegate | L24-L43 | `EfcHomeControllerLifecycleTests` (both public static wrappers) |
| `EfcHomeControllerDependencies` ctor parameter (internal 4-arg ctor, internal `CreateAsync`, internal `LoadFinderAsync`) | injectable dependency bundle | L54-L59, L113-L118, L149-L154 | `EfcHomeControllerSeamTests`, `EfcHomeControllerMetricsTests` |
| `DataModelFactory` | injectable delegate | dependencies | Seam/Lifecycle/Metrics tests |
| `AsyncDataModelFactory` | injectable delegate | dependencies | Seam/Lifecycle tests |
| `ViewerFactory` | injectable delegate | dependencies | Seam/Lifecycle tests (returns an *uninitialized* `EfcViewer` via `FormatterServices.GetUninitializedObject`, never a live form) |
| `KeyboardHandlerFactory` | injectable delegate | dependencies | Seam/Lifecycle tests (`Mock<IQfcKeyboardHandler>`) |
| `ExplorerControllerFactory` | injectable delegate | dependencies | Seam/Lifecycle tests (`Mock<IQfcExplorerController>`) |
| `FormControllerWithDataFactory` / `FormControllerWithoutDataFactory` | injectable delegates | dependencies | Seam/Lifecycle tests |
| `InitializeDataFields` | injectable delegate | dependencies | Seam/Lifecycle tests |
| `SelectionLoader` | injectable delegate | dependencies | Seam/Lifecycle tests — this is what keeps `globals.Ol.App.ActiveExplorer().Selection` out of the test path |
| `ViewerShowAction` | settable instance delegate | L294 | `EfcHomeControllerLifecycleTests.Run_WithMail_ShowsViewerThroughInjectedSeam` |
| `ViewerShowAsyncAction` | settable instance delegate | L296-L297 | `EfcHomeControllerLifecycleTests.RunAsync_WithMail_ShowsViewerThroughInjectedSeam` |
| `MessageBoxShowAction` | settable instance delegate | L299-L305 | `EfcHomeControllerLifecycleTests.Run_WithoutMail_...` / `RunAsync_WithoutMail_...` |

Established test construction techniques already proven in this family (reuse; do not reinvent):

- `FormatterServices.GetUninitializedObject(typeof(EfcViewer))` and `...(typeof(EfcFormController))`
  to obtain non-live viewer/form-controller instances without running a WinForms constructor.
- Reflection into the private `(IApplicationGlobals, System.Action)` constructor
  (`EfcHomeControllerTests.CreateMinimalController`).
- `FormatterServices.GetUninitializedObject(typeof(EfcHomeController))` to get a controller with no
  ctor side effects (`EfcHomeControllerExecuteMovesTests.CreateController`).
- Hand-written `FakeApplicationGlobals` / `FakeFileSystemFolderPaths` implementing the interfaces
  directly (three separate copies exist across the test files).
- `Mock<MailItem>(MockBehavior.Loose)` for the Outlook `MailItem` parameter type.

### 1.2 COM / Outlook-Interop exposure reachable in this file

`Microsoft.Office.Interop.Outlook` is imported (L9) and `MailItem` appears in the signatures of the
public constructor (L50), the internal constructor (L58), both `CreateAsync` overloads (L107, L117),
both `LoadFinderAsync` overloads (L143, L153), `CaptureSelectionSnapshot` (L196), `InitAsync` (L205),
and `LoadToList` (L257).

**No COM member is ever dereferenced in this file.** The only uses of `MailItem` values are:

- `DataModel.Mail is not null` (L73) and `_dataModel?.Mail is not null` (L310, L327) — reference
  comparisons against a `MailItem`-typed field, no COM call;
- list membership operations in `CaptureSelectionSnapshot` (L200).

`Application`, `Store`, and `MAPIFolder` do not appear anywhere in this file. The one place the EFC
family reaches `globals.Ol.App.ActiveExplorer().Selection` is
`EfcHomeControllerDependencies.LoadSelection` (a different file, and already behind the
`SelectionLoader` delegate). Consequently, **the CLAUDE.md § UT2 COM/VSTO exemption does not apply to
this file**: the interop types are present but an injectable seam exists for every one of them, and
`Mock<MailItem>(MockBehavior.Loose)` is sufficient. This matches the epic's Shared Design §1 reading
of "without an injectable seam" as a live obligation, not a standing permission.

### 1.3 WinForms exposure reachable in this file

Three host-bound operations appear, all three already behind a settable delegate. Only the
**default delegate bodies** are host-bound:

- L294 `viewer => viewer.Show()` — shows a live `EfcViewer` form.
- L297 `async viewer => await UiThread.Dispatcher.InvokeAsync(() => viewer.Show())` — needs a live
  WPF dispatcher **and** shows a form.
- L305 `(text, caption, buttons, icon) => MessageBox.Show(text, caption, buttons, icon)` — shows a
  modal popup, which is a unit-test-policy violation by itself.

These three lambda bodies are the irreducible line-level remainder of this file (§4).

---

## 2. Member-by-member coverage inventory

Status legend: **COVERED** (an existing test reaches every statement), **PARTIAL** (statements
reached, at least one branch not), **UNCOVERED** (no existing test reaches the statement),
**ORDER-DEPENDENT** (reached in a full-suite run, but which of two alternatives is reached depends
on test-class execution order).

| Lines | Member / region | Status | Covering test (class.method) |
| --- | --- | --- | --- |
| L20-22 | `logger` static field init | COVERED | any test that touches the type (static init) |
| L24-25 | `_defaultDependenciesFactory` field init; **lambda body L25** | field COVERED; **body ORDER-DEPENDENT** | body reached only via `CreateDefaultDependencies` (L42) from the private ctor (L101) when the initial lambda is still installed — e.g. `EfcHomeControllerSeamTests.CreateAsync_WithExplicitMail_...` if it runs before `EfcHomeControllerLifecycleTests` cleanup |
| L27-33 | `SetDefaultDependenciesFactory` | COVERED (assignment); **null-rejection branch not asserted** | `EfcHomeControllerLifecycleTests.CreateAsync_PublicWrapper_UsesInjectedDefaultDependencies`, `...LoadFinderAsync_PublicWrapper_...` |
| L35-38 | `ResetDefaultDependenciesFactory`; **lambda body L37** | method COVERED; **body ORDER-DEPENDENT** | `EfcHomeControllerLifecycleTests.Cleanup` (`[TestCleanup]`) invokes the method; the body runs only on a subsequent `CreateDefaultDependencies` call |
| L40-43 | `CreateDefaultDependencies` | COVERED | reached from private ctor L101 in every `EfcHomeControllerSeamTests` case |
| **L47-52** | **public `EfcHomeController(IApplicationGlobals, Action, MailItem = null)`** | **UNCOVERED** | none — every test uses the internal 4-arg overload |
| L54-L95 | internal `EfcHomeController(globals, parentCleanup, dependencies, mail = null)` | COVERED, both arms of `DataModel.Mail is not null` | mail-present arm: `EfcHomeControllerLifecycleTests.Run_WithMail_ShowsViewerThroughInjectedSeam`, `Cleanup_ClearsControllerFieldsAndInvokesParentCleanup`; mail-absent arm: `...Run_WithoutMail_...`, `EfcHomeControllerMetricsTests.QuickFileMetricsWrite_*` |
| L61 | `dependencies.ThrowIfNull()` | line COVERED; **throw path not asserted** | statement executed in every construction; the throw itself is in `UtilitiesCS\Extensions\NullExtensions.cs`, not in this file |
| L97-102 | private `EfcHomeController(globals, parentCleanup)` | COVERED | `EfcHomeControllerTests.CreateMinimalController` (reflection) and both internal static factories |
| L104-111 | public `CreateAsync` | COVERED | `EfcHomeControllerLifecycleTests.CreateAsync_PublicWrapper_UsesInjectedDefaultDependencies` |
| L113-138 | internal `CreateAsync`, both arms of `mailItems.Count() > 0` | COVERED | non-empty: `EfcHomeControllerSeamTests.CreateAsync_WithExplicitMail_UsesSelectionAndInitializationFactories`; empty: `...CreateAsync_WithEmptySelection_DoesNotInitializeViewerOrDataModel` |
| L120-122 | `globals/parentCleanup/dependencies.ThrowIfNull()` | lines COVERED; **rejection contract not asserted** | as above |
| L140-147 | public `LoadFinderAsync` | COVERED | `EfcHomeControllerLifecycleTests.LoadFinderAsync_PublicWrapper_UsesInjectedDefaultDependencies` |
| L149-168 | internal `LoadFinderAsync` | COVERED | `EfcHomeControllerSeamTests.LoadFinderAsync_WithEmptySelection_InitializesFindShellAndDummyDataModel` |
| L156-158 | three `ThrowIfNull` guards | lines COVERED; **rejection contract not asserted** | as above |
| L170-194 | `HandleSelectionChangedAsync` | COVERED | `EfcHomeControllerSeamTests.HandleSelectionChangedAsync_SnapshotsSelectionBeforeAsyncDataLoad`; also traversed by both static factories |
| L196-201 | `CaptureSelectionSnapshot` | **PARTIAL** — non-null arm covered, **`mailItems is null` arm UNCOVERED** | `EfcHomeControllerTests.CaptureSelectionSnapshot_ReturnsIndependentCopyBeforeBackgroundModelLoad` (non-null only) |
| L203-253 | `InitAsync`, both arms of `mailItems.Count() > 0` | COVERED | non-empty: `EfcHomeControllerSeamTests.CreateAsync_WithExplicitMail_...` (asserts the exact call order `viewer, keyboard, explorer, form-without-data, initialize-data`); empty: `...LoadFinderAsync_WithEmptySelection_...` |
| L255-262 | `LoadToList` | COVERED | every `EfcHomeControllerSeamTests` / `...LifecycleTests` static-factory case (via `SelectionLoader`) |
| L264-269 | `FormViewer` get/set | COVERED | get: `EfcHomeControllerSeamTests.CreateAsync_WithExplicitMail_...`; set: ctor / `InitAsync` |
| L271-276 | `Globals` get/set | COVERED | get: `EfcHomeControllerLifecycleTests.Cleanup_ClearsControllerFieldsAndInvokesParentCleanup` |
| L278-283 | `InitType` get/set | COVERED | `EfcHomeControllerSeamTests.LoadFinderAsync_WithEmptySelection_...` |
| **L285-290** | **`ParentCleanup` property (get L288, private set L289)** | **UNCOVERED (both accessors)** | none — no in-repo caller exists; both constructors assign the backing field `_parentCleanup` directly |
| L292 | `_dependencies` field | COVERED | — |
| L294 | `ViewerShowAction` property + **default lambda body** | property COVERED; **default body UNCOVERED (irreducible)** | get: `Run`; set: `EfcHomeControllerLifecycleTests.Run_WithMail_...` |
| L296-297 | `ViewerShowAsyncAction` property + **default lambda body** | property COVERED; **default body UNCOVERED (irreducible)** | set: `EfcHomeControllerLifecycleTests.RunAsync_WithMail_...` |
| L299-305 | `MessageBoxShowAction` property + **default lambda body** | property COVERED; **default body UNCOVERED (irreducible)** | set: `EfcHomeControllerLifecycleTests.Run_WithoutMail_...` |
| L308-323 | `Run()` | **PARTIAL** — both statements reached; the sub-branch `_dataModel?.Mail is null && InitType.HasFlag(Find)` is **UNCOVERED** | show arm: `EfcHomeControllerLifecycleTests.Run_WithMail_ShowsViewerThroughInjectedSeam` (mail non-null, short-circuits before `HasFlag`); message arm: `...Run_WithoutMail_ShowsMessageThroughInjectedSeam` |
| L325-340 | `RunAsync(ProgressTracker = null)` | **PARTIAL** — same missing sub-branch | `EfcHomeControllerLifecycleTests.RunAsync_WithMail_...` / `...RunAsync_WithoutMail_...` |
| L342-350 | `Cleanup()` | COVERED (all six statements) | `EfcHomeControllerLifecycleTests.Cleanup_ClearsControllerFieldsAndInvokesParentCleanup` |
| L356-361 | `ExplorerController` get/set | COVERED | `EfcHomeControllerLifecycleTests.ExplorerControllerAndKeyboardHandler_SettersStoreAssignedInstances` |
| L363-367 | `FormController` get | COVERED | `EfcHomeControllerLifecycleTests.Cleanup_ClearsControllerFieldsAndInvokesParentCleanup` |
| L369-374 | `KeyboardHandler` get/set | COVERED | `EfcHomeControllerLifecycleTests.ExplorerControllerAndKeyboardHandler_SettersStoreAssignedInstances` |
| L376-381 | `DataModel` get/set | COVERED | get: many; set: `EfcHomeControllerLifecycleTests.OpenFolderMethods_DelegateToDataModelWithoutExternalServices` |
| L383-387 | **`StopWatch` get** | **UNCOVERED** | none — no test reads the property (it is an `IFilerHomeController` member and cannot be deleted) |
| L389 | `_isExecuting` field | COVERED | `EfcHomeControllerExecuteMovesTests.TryBeginExecuteMoves_ReturnsFalseUntilExecutionStateIsReset` (via `.ExecuteMoves.cs`) |
| L391 | `Loaded => throw new NotImplementedException()` | COVERED | `EfcHomeControllerLifecycleTests.LoadedAndFilerQueue_PreserveNotImplementedContracts` |
| L393-397 | `CreateCancellationToken` | COVERED | every construction path |
| L399-403 | `TokenSource` get | COVERED | read at L69 and L216 during construction/init |
| L405-409 | `Token` get | COVERED | read at L70, L92, L217, L236 |
| **L411-415** | **`UiSyncContext` get** | **UNCOVERED** | none — no test reads the property (it is an `IFilerHomeController` member and cannot be deleted) |
| L417 | `FilerQueue => throw new NotImplementedException()` | COVERED | `EfcHomeControllerLifecycleTests.LoadedAndFilerQueue_PreserveNotImplementedContracts` |
| L423-426 | `OpenOlFolderAsync` | COVERED | `EfcHomeControllerLifecycleTests.OpenFolderMethods_DelegateToDataModelWithoutExternalServices` |
| L428-431 | `OpenFsFolderAsync` | COVERED | same test |
| L435-439 | comment-only "Helper Methods" region | n/a | no executable code |

### 2.1 Static line-coverage estimate

Counting executable statements (excluding braces, declarations, and comment-only lines) gives
approximately **108 statement-level sequence points** in this file. The statements identified as
UNCOVERED above total **9-10**:

| Uncovered item | Statements |
| --- | --- |
| public 3-arg constructor (`: this(...)` delegation + empty body) | 2 |
| `ParentCleanup` get + private set | 2 |
| `StopWatch` get | 1 |
| `UiSyncContext` get | 1 |
| `ViewerShowAction` default lambda body | 1 |
| `ViewerShowAsyncAction` default lambda body | 1 |
| `MessageBoxShowAction` default lambda body | 1 |
| whichever of L25 / L37 the run order leaves unreached | 1 |

**Estimated current line coverage: ~90-91%.** This file therefore **most likely already clears the
80% per-file floor** before any new test is written. This estimate is manual and static; the
authoritative figure is F1's per-file coverage harness output, which must be captured to
`<FEATURE>/evidence/qa-gates/` before any acceptance criterion is closed. Treat the estimate as a
planning input only — in particular, a Cobertura line-based denominator counts multi-line statement
continuations and closing braces differently from a statement count, which can move the percentage
by several points in either direction.

**Planning consequence:** the value of F8's work on this file is not bulk coverage, it is (a)
closing the small set of genuinely uncovered members, (b) removing the order-dependent coverage
hazard, and (c) closing the missing *branch* and *contract* scenarios that a line metric does not
see.

---

## 3. Genuine remaining gaps and the test scenario required for each

Each item states the scenario class (positive / invalid-input / boundary / error-handling) and how
to reach it deterministically. All use MSTest + Moq + FluentAssertions and reuse the existing
`FormatterServices.GetUninitializedObject` and `Fake*Globals` techniques already present in the
family. None constructs a live form, shows a popup, or touches an Outlook store.

### G1 — public 3-arg constructor is never executed (UNCOVERED, 2 statements)

- **Scenario:** positive.
- **How:** in a `[TestClass]` with `[TestCleanup] EfcHomeController.ResetDefaultDependenciesFactory()`,
  call `EfcHomeController.SetDefaultDependenciesFactory(() => probeDependencies)`, then
  `new EfcHomeController(fakeGlobals, () => {}, mockMailItem)`. Assert `controller.DataModel` is the
  probe model and `controller.InitType == (Sort | SortConv)`, proving the public overload delegated
  to the internal one with the injected bundle.
- **Determinism:** no ambient state beyond the static factory, which the cleanup resets. Reuse the
  `LifecycleProbe` dependency-bundle shape from `EfcHomeControllerLifecycleTests`.
- **New seam required:** none.

### G2 — order-dependent default-factory lambda bodies (L25 / L37)

- **Problem:** `_defaultDependenciesFactory` is initialized with one lambda (L24-25) and
  `ResetDefaultDependenciesFactory` installs a *different* lambda instance (L37) with identical
  behavior. Exactly one of the two bodies is executed in a given full-suite run, and which one
  depends on whether `EfcHomeControllerLifecycleTests`'s `[TestCleanup]` has run before the first
  `EfcHomeControllerSeamTests` case. Per-file coverage is therefore not reproducible run to run —
  which directly undermines the numeric evidence F1's harness is supposed to produce.
- **Recommended production change (behavior-preserving, additive, 1 net line):** introduce a single
  shared default and have both sites reference it:
  `private static readonly Func<EfcHomeControllerDependencies> DefaultDependenciesFactory = () => new EfcHomeControllerDependencies();`
  then `private static Func<EfcHomeControllerDependencies> _defaultDependenciesFactory = DefaultDependenciesFactory;`
  and `ResetDefaultDependenciesFactory() { _defaultDependenciesFactory = DefaultDependenciesFactory; }`.
  This removes one duplicated lambda body and makes the remaining one deterministically covered.
- **Scenario:** positive. Call `ResetDefaultDependenciesFactory()`, then invoke the private static
  `CreateDefaultDependencies` by reflection (the reflection-helper pattern is already established in
  `EfcHomeControllerTests`) and assert the result is a non-null `EfcHomeControllerDependencies` whose
  `DataModelFactory` is non-null.
- **New seam required:** none. This is a de-duplication of an existing seam, not a contract change;
  both members are `private`/`internal static` on `EfcHomeController` and are not part of the
  `EfcHomeControllerDependencies` contract that F9 consumes.

### G3 — `SetDefaultDependenciesFactory(null)` rejection is never asserted

- **Scenario:** invalid input / error handling.
- **How:** `Action act = () => EfcHomeController.SetDefaultDependenciesFactory(null);`
  `act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("factory");`
  Requires `[TestCleanup] ResetDefaultDependenciesFactory()` for isolation.
- **Note:** the `?? throw` is one statement (L31-32), so this closes a *branch*, not a line. It is
  still required: the guard is an explicit contract and the epic's scenario-completeness rule
  (`.claude/rules/general-unit-test.md` § Scenario Completeness) requires negative flows.

### G4 — `CaptureSelectionSnapshot(null)` arm is never executed

- **Scenario:** invalid input / boundary.
- **How (preferred, no reflection):** `await controller.HandleSelectionChangedAsync(globals, null, QfEnums.InitTypeEnum.Find)`
  on a controller built from the internal ctor with a probe bundle, and assert it completes without
  throwing and that `AsyncDataModelFactory` was **not** invoked (empty snapshot → `mailItems.Count() > 0`
  is false → the dummy-model path at L250-L251 runs instead). This exercises the null arm end-to-end
  through the public-ish `protected internal` entry point rather than by reflecting on a private
  static.
- **How (alternative):** extend the existing reflection-based
  `EfcHomeControllerTests.CaptureSelectionSnapshot_...` with a `null` invocation asserting an empty,
  non-null list. Lower value: it does not prove the caller behaves correctly.
- **New seam required:** none.

### G5 — `Run()` / `RunAsync()` "Find without mail" arm is never executed

- **Problem:** the condition is `_dataModel?.Mail is not null || InitType.HasFlag(QfEnums.InitTypeEnum.Find)`.
  Every existing show-the-viewer test supplies a non-null `Mail`, which short-circuits before
  `HasFlag` is evaluated. The Finder flow — the *entire reason* `LoadFinderAsync` exists — has never
  had its `Run`/`RunAsync` path exercised.
- **Scenario:** positive (the Finder flow) plus boundary (flag combination). `InitTypeEnum` is
  `[Sort=1, Find=2, Info=4, Reminder=8, SortConv=16]`, so `Find` is a real bit and `HasFlag` on the
  default value `0` returns false — which is why the message arm is reachable today.
- **How:** build a controller through the internal `LoadFinderAsync` overload with an empty
  `SelectionLoader` (this already sets `InitType = Find` and leaves `DataModel.Mail` null, as
  `EfcHomeControllerSeamTests.LoadFinderAsync_WithEmptySelection_...` proves), then assign
  `ViewerShowAction` / `ViewerShowAsyncAction` capture delegates and call `Run()` / `RunAsync()`.
  Assert the viewer was shown and that `MessageBoxShowAction` was **not** invoked (assign a capture
  delegate to it and assert it stayed unset — this also guarantees the test can never raise a popup).
- **Determinism:** fully synchronous; `RunAsync` uses the injected `Func<EfcViewer, Task>` returning
  `Task.CompletedTask`. No `Thread.Sleep`, no `Task.Delay`, no wall-clock wait.
- **New seam required:** none.

### G6 — `StopWatch` getter (UNCOVERED, 1 statement)

- **Scenario:** positive / state-transition.
- **How:** with the mail-present internal ctor (which allocates `_stopWatch` at L76), assert
  `controller.StopWatch.Should().NotBeNull()`; with the mail-absent path assert
  `controller.StopWatch.Should().BeNull()`. This is a genuine state-transition assertion, not a
  coverage-only getter poke: it pins the invariant that the stopwatch is allocated only on the
  data-bearing construction path.
- **New seam required:** none. `StopWatch` is an `IFilerHomeController` member and must not be
  removed.

### G7 — `UiSyncContext` getter (UNCOVERED, 1 statement)

- **Scenario:** positive.
- **How:** the internal ctor sets `_uiSyncContext = FormViewer.UiSyncContext` (L78) from the
  probe's uninitialized `EfcViewer` (whose `UiSyncContext` is null), and `InitAsync` does the same at
  L227. Assert `controller.UiSyncContext.Should().BeSameAs(probeViewer.UiSyncContext)` — for the
  uninitialized-viewer probe that is `null`, so assert `BeNull()` and document that the assertion
  pins the *propagation*, not a live context.
- **Stronger option (still no live form):** set a non-null `SynchronizationContext` on the probe
  `EfcViewer` instance before returning it from `ViewerFactory` (the field is settable on the
  uninitialized object, and `EfcHomeControllerDependenciesProductionFactoryTests.CreateConfiguredViewer`
  already demonstrates mutating an uninitialized `EfcViewer`'s members). Then assert the controller
  surfaces that exact instance. Prefer this: it distinguishes "propagated" from "always null".
- **New seam required:** none.

### G8 — `ParentCleanup` property (UNCOVERED, 2 statements, dead code)

- **Finding:** a repository-wide grep confirms `EfcHomeController.ParentCleanup` has **no caller**.
  Both constructors assign the backing field `_parentCleanup` directly (L64, L100) and `Cleanup()`
  invokes `_parentCleanup` directly (L349). The property is not on `IFilerHomeController`. The
  `private set` accessor is unreachable from any code path in the repository.
- **Recommendation (preferred):** collapse to an expression-bodied get-only property,
  `internal System.Action ParentCleanup => _parentCleanup;`, which deletes the unreachable setter.
  This is dead-code removal, not an API change: the member is `internal`, has no in-repo caller, and
  is not part of the `EfcHomeControllerDependencies` contract, so **F9 needs no edit**. Then cover
  the getter by extending
  `EfcHomeControllerLifecycleTests.Cleanup_ClearsControllerFieldsAndInvokesParentCleanup` with an
  assertion that `controller.ParentCleanup` is the injected delegate before `Cleanup()` runs.
- **Fallback if the maintainer prefers no production edit:** cover the getter only; the `private set`
  remains 1 permanently unreachable statement. That is acceptable given the ~90% estimate but should
  then be recorded in F1's ledger as a *line-level* irreducible item, not a file-level exemption.

### G9 — argument-rejection contracts on the three static entry points

- **Scenario:** invalid input / error handling.
- **Finding:** `globals.ThrowIfNull()`, `parentCleanup.ThrowIfNull()`, and `dependencies.ThrowIfNull()`
  (L120-122, L156-158, L61) all execute today, so they are line-covered; the *throw* itself lives in
  `UtilitiesCS\Extensions\NullExtensions.cs` and raises `ArgumentNullException`. No existing test
  asserts that these entry points reject nulls.
- **How:** six FluentAssertions cases —
  `await ((Func<Task>)(() => EfcHomeController.CreateAsync(null, cleanup, deps))).Should().ThrowAsync<ArgumentNullException>();`
  and the equivalents for `parentCleanup`, `dependencies`, plus the three `LoadFinderAsync` variants,
  plus `new EfcHomeController(globals, cleanup, (EfcHomeControllerDependencies)null)`.
- **Note:** `ThrowIfNull` uses `[CallerArgumentExpression]`, so the `ParamName` is the *expression
  text* (`"globals"`, `"parentCleanup"`, `"dependencies"`). Assert on `ParamName` only after
  confirming that against the helper's implementation, or assert only the exception type to avoid a
  brittle test.
- **New seam required:** none. These close branches, not lines.

### G10 — construction ordering invariant on the mail-present constructor path

- **Finding:** `EfcHomeControllerSeamTests.CreateAsync_WithExplicitMail_...` asserts the exact
  factory call order for the **`InitAsync`** path (`viewer, keyboard, explorer, form-without-data,
  initialize-data`). No test asserts the order for the **constructor** path (L77-L85:
  `viewer, keyboard, explorer, form-with-data`), which is the only place
  `FormControllerWithDataFactory` is used in this file.
- **Scenario:** state transition / ordering.
- **How:** build via the internal 4-arg ctor with a mail-bearing data model and assert
  `probe.Calls.Should().ContainInOrder("viewer", "keyboard", "explorer", "form-with-data")` and that
  `"form-without-data"` was not recorded. This is a real behavioral invariant (the
  `_uiSyncContext` capture at L78 must happen after the viewer is created and before the keyboard
  handler is built), not a coverage filler.
- **New seam required:** none.

---

## 4. Irreducible line-level remainder

Three statements cannot be executed by any policy-compliant unit test:

| Line | Statement | Why irreducible |
| --- | --- | --- |
| L294 | `viewer => viewer.Show()` | shows a live WinForms form |
| L297 | `async viewer => await UiThread.Dispatcher.InvokeAsync(() => viewer.Show())` | requires a live WPF dispatcher **and** shows a live form |
| L305 | `(text, caption, buttons, icon) => MessageBox.Show(text, caption, buttons, icon)` | shows a modal popup, which requires human interaction |

These are ~3 of ~108 statements (~2.8%). The file still clears 80% without them, so **no file-level
`[ExcludeFromCodeCoverage]` is warranted and none is proposed** — consistent with
`.claude/rules/general-unit-test.md` § Coverage Exclusion Policy, which prohibits excluding a
production file from measurement. Record them in F1's ledger as *line-level* irreducible items
inside a **testable** file.

A rejected alternative is documented in §6.

---

## 5. File-size risk

`EfcHomeController.cs` is at **441 of 500** lines, leaving a 59-line headroom. The recommendations
above have the following net effect:

| Recommendation | Net lines |
| --- | --- |
| G2 shared default factory field | +2 to +3 |
| G8 collapse `ParentCleanup` to expression-bodied get-only | −4 |
| G1, G3-G7, G9, G10 | 0 (test-only) |

Net effect is approximately **−1 line**, so the file stays at ~440 and no partial split is needed.
**No new seam proposed here adds meaningful lines to this file.** If a future change would push this
file past 500, the correct destination is a new partial (the family already uses
`.Metrics.cs`, `.ExecuteMoves.cs`, `.Timing.cs`); host-bound default delegates would belong in a new
`EfcHomeController.UiDefaults.cs` partial. That is **not** required by any recommendation in this
artifact.

---

## 6. Rejected alternatives

- **Extract the three host-bound default lambda bodies into a separate adapter class (e.g.
  `EfcHomeControllerUiOperations`).** This would raise *this* file's percentage by ~3 points but
  merely relocates the same three untestable statements into a new file that would then need its own
  ledger entry. It also adds an indirection layer for no behavioral gain, contradicting the
  "simplicity first" design principle. Rejected: the file already clears 80% without it.
- **Replace `UiThread.Dispatcher` with the existing `UtilitiesCS.Threading.IUiDispatcher` /
  `WpfUiDispatcher` interface seam (an established precedent in this repository).** This would make
  the dispatch call substitutable, but `viewer.Show()` inside the lambda remains a live-form call,
  so the statement stays uncoverable. Rejected as not closing the gap it targets. Worth revisiting
  only as part of the separate VSTO/WebView2 migration effort, not in F8.
- **`[STATestClass]` STA-thread tests constructing an in-memory `EfcViewer`.** The epic's STA
  last-resort clause (Shared Design §3) permits in-memory *controls*, not *forms*, and explicitly
  forbids showing anything. `EfcViewer` is a `Form`, and the three uncovered statements all *show*
  it. Rejected as out of clause scope.
- **Adding overloads to `EfcHomeControllerDependencies` to inject the three UI actions.** Rejected:
  the three UI actions are already injectable as settable instance properties on the controller
  (the tests use them today), so a dependency-bundle change would be duplicative — and any change
  to that bundle is exactly the cross-child contract surface F9 consumes and this child must leave
  additive-only.

---

## 7. Cross-child contract notes

**None required for this file.** Every recommendation is confined to
`QuickFiler/Controllers/EfcHomeController.cs` and to new test files under
`QuickFiler.Test/Controllers/`. Specifically:

- No change to `EfcHomeControllerDependencies` or `EfcHomeControllerDependencyFactories` is needed to
  close any gap in this file, so the contract F9 consumes is untouched.
- `EfcFormController` and `EfcItemController` (F9) are referenced only through the existing
  `FormControllerWithDataFactory` / `FormControllerWithoutDataFactory` / `InitializeDataFields`
  delegates, all of which tests already substitute. No F9 file needs to be read or edited.
- The G8 `ParentCleanup` simplification touches an `internal` member with zero in-repo callers,
  verified by a repository-wide grep; `EfcFormController` has its own unrelated `_parentCleanup`
  field.

---

## 8. Do not duplicate — scenarios already covered

Do **not** write new tests for any of the following; they are already covered and re-covering them
inflates the test suite without moving per-file coverage:

- Re-entrant `ExecuteMovesAsync` drop via the `_isExecuting` guard —
  `EfcHomeControllerTests.ExecuteMovesAsync_WhenAlreadyExecuting_ReturnsWithoutAccessingNullFields`.
- `QuickFileMetrics_WRITE` empty-list and null-list guards —
  `EfcHomeControllerTests.QuickFileMetrics_WRITE_WithEmptyList_SkipsBodyAndDoesNotThrow`,
  `...WithNullList_SkipsBodyAndDoesNotThrow`, and
  `EfcHomeControllerMetricsTests.QuickFileMetricsWrite_WithNoMovedItems_DoesNotInvokeWriter`.
- `CaptureSelectionSnapshot` producing an independent copy from a **non-null** live list —
  `EfcHomeControllerTests.CaptureSelectionSnapshot_ReturnsIndependentCopyBeforeBackgroundModelLoad`
  (only the **null** input remains open, per G4).
- Selection snapshotting across the async model load —
  `EfcHomeControllerSeamTests.HandleSelectionChangedAsync_SnapshotsSelectionBeforeAsyncDataLoad`.
- Public `CreateAsync` / `LoadFinderAsync` wrappers routing through the injected default factory —
  `EfcHomeControllerLifecycleTests.CreateAsync_PublicWrapper_UsesInjectedDefaultDependencies`,
  `...LoadFinderAsync_PublicWrapper_UsesInjectedDefaultDependencies`.
- Internal `CreateAsync` with a non-empty selection, including the full factory call order —
  `EfcHomeControllerSeamTests.CreateAsync_WithExplicitMail_UsesSelectionAndInitializationFactories`.
- Internal `CreateAsync` with an empty selection short-circuiting the viewer and data model —
  `EfcHomeControllerSeamTests.CreateAsync_WithEmptySelection_DoesNotInitializeViewerOrDataModel`.
- `LoadFinderAsync` building the Find shell plus the dummy data model —
  `EfcHomeControllerSeamTests.LoadFinderAsync_WithEmptySelection_InitializesFindShellAndDummyDataModel`.
- `Run` / `RunAsync` **with** mail showing the viewer through the injected seam, and **without**
  mail routing the message through the injected seam — all four
  `EfcHomeControllerLifecycleTests.Run*`/`RunAsync*` tests (only the **Find-without-mail** arm
  remains open, per G5).
- `Cleanup()` nulling all five fields and invoking the parent callback —
  `EfcHomeControllerLifecycleTests.Cleanup_ClearsControllerFieldsAndInvokesParentCleanup`.
- `ExplorerController` / `KeyboardHandler` setter round-trip —
  `EfcHomeControllerLifecycleTests.ExplorerControllerAndKeyboardHandler_SettersStoreAssignedInstances`.
- `Loaded` and `FilerQueue` `NotImplementedException` contracts —
  `EfcHomeControllerLifecycleTests.LoadedAndFilerQueue_PreserveNotImplementedContracts`.
- `OpenOlFolderAsync` / `OpenFsFolderAsync` delegating to the data model —
  `EfcHomeControllerLifecycleTests.OpenFolderMethods_DelegateToDataModelWithoutExternalServices`.
- Anything in `EfcHomeControllerDependencies` / `EfcHomeControllerDependencyFactories` — those are
  separate files with their own per-file research artifacts and are already served by
  `EfcHomeControllerDependenciesTests` (11 delegate defaults, `LoadSelection` explicit-mail path, and
  the null-argument matrix for all five `*WithFactory` helpers) and
  `EfcHomeControllerDependenciesTestsProductionFactory` (resettable production factories, the
  `factory`-null matrix, the construction adapters, and the reset-to-concrete-instances path).

---

## 9. Testing strategy summary (no test code written here)

- **Placement:** new tests go in `QuickFiler.Test/Controllers/`, mirroring the production tree per
  `.claude/rules/general-unit-test.md` § Test File Location. Prefer extending the existing classes
  where the scenario belongs to their theme (G5 → `EfcHomeControllerLifecycleTests`; G10 →
  `EfcHomeControllerSeamTests`; G4 → `EfcHomeControllerTests`) and add one new class only for the
  static-factory contract cases (G1, G2, G3, G9), which need their own
  `[TestCleanup] ResetDefaultDependenciesFactory()`.
- **Isolation hazard to manage explicitly:** `_defaultDependenciesFactory` and the eleven
  `Production*` statics in `EfcHomeControllerDependencyFactories.cs` are mutable global state. Every
  test class that mutates them must reset in `[TestCleanup]`, exactly as
  `EfcHomeControllerLifecycleTests` and `EfcHomeControllerDependenciesTestsProductionFactory` already
  do. G2's shared-default refactor reduces, but does not eliminate, this hazard.
- **Determinism:** no `Thread.Sleep`, no `Task.Delay`, no wall-clock wait. Async sequencing uses
  `TaskCompletionSource` (the pattern already used in
  `EfcHomeControllerSeamTests.HandleSelectionChangedAsync_SnapshotsSelectionBeforeAsyncDataLoad`).
  The only wall-clock read in this file is `Stopwatch.StartNew()` / `.ElapsedMilliseconds` at L176
  and L192, whose value is interpolated into a log string and never asserted — see the
  `EfcHomeController.Timing.cs` artifact for the clock analysis.
- **Coverage evidence:** run F1's per-file harness once F1 has merged to the integration branch and
  commit the numeric per-file result for this file under
  `<FEATURE>/evidence/qa-gates/`. Aggregate assembly coverage does not satisfy issue #136.
- **Toolchain:** the C# loop is `csharpier .` → analyzer msbuild → nullable msbuild →
  `vstest.console.exe ... /EnableCodeCoverage`, restarting from step 1 on any failure or auto-fix.
