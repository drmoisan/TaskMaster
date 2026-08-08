# Research — `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs`

- **Feature:** `2026-08-07-quickfiler-efc-home-controller-coverage-437` (issue #437)
- **Epic:** #136 `quickfiler-per-file-coverage`, child F8
- **Production file:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aea998f94efaa2eb4\QuickFiler\Controllers\EfcHomeControllerDependencyFactories.cs` (268 lines)
- **Method:** static analysis only. No build, no test run, no coverage run was performed.
- **Coverage status of the file itself:** the file carries **no** `[ExcludeFromCodeCoverage]`
  attribute, so it is already inside the coverage denominator.

## 1. Headline finding — the spec's premise needs correcting

`spec.md` L30 states that "`EfcHomeControllerDependencyFactories.cs` has no dedicated test file."
That is true of the **file name** but false of the **effect**. The class
`EfcHomeControllerDependenciesTestsProductionFactory`, which lives in
`QuickFiler.Test\Controllers\EfcHomeControllerDependenciesProductionFactoryTests.cs` (473 lines),
is in practice the dedicated test class for this partial: all four of its test methods target
members declared in this file. Note the class name and file name do not match, which is why a
name-based search misses the association.

Member-by-member analysis (§ 3) finds **26 of 28 members already exercised**. The two genuine gaps
are `CreateProductionExplorerControllerInstance` (L149-156) and five one-statement initializer
closure bodies. Estimated current line coverage is **~86-91%** — that is, this file is very likely
**already above the 80% floor**, not near-zero. Planning must be sized accordingly: this is
gap-closure and invariant-pinning, not rescue work. The authoritative number must still come from
F1's harness.

## 2. Current state

This file is the second half of `internal sealed partial class EfcHomeControllerDependencies`
(first half: `EfcHomeControllerDependencies.cs`, 428 lines, researched in the sibling artifact).

It contains exactly three kinds of member:

1. **16 mutable `internal static` delegate properties** (`Production*`), each with an initializer.
   These are the process-global test seam: production code reads them, tests replace them.
2. **`ResetProductionFactoriesForTesting()`** (L107-129), which restores all 16 to their defaults.
3. **11 private static adapter methods** in two layers:
   - a *composition* layer (`CreateProductionDataModel` L191-199, `CreateProductionKeyboardHandler`
     L201-207, `CreateProductionExplorerController` L209-216,
     `CreateProductionFormControllerWithData` L218-238,
     `CreateProductionFormControllerWithoutData` L240-258, `CreateProductionDataFields` L260-266)
     which read a `*Constructor` static and, for the form-controller pair, then a `*Initializer`
     static;
   - a *construction* layer (`CreateProduction*Instance`, L131-189) which performs the actual `new`.

The two-layer split is what makes the `new` expressions individually replaceable in tests. It is a
deliberate injectable-delegate seam and it works.

## 3. Member-by-member inventory

Legend: **COVERED** = an existing test provably executes the line(s); **UNCOVERED** = no existing
test reaches them. All test citations are in class
`EfcHomeControllerDependenciesTestsProductionFactory` unless another class is named.

| # | Member | Lines | Status | Covering test(s) |
| --- | --- | --- | --- | --- |
| 1 | `ProductionDataModelFactory` (get/set + initializer) | 14-20 | COVERED | set: `Constructor_WithNoOverrides_UsesResettableProductionFactories` L31; get: read at `EfcHomeControllerDependencies.cs` L141 during the same test; initializer runs in the static ctor |
| 2 | `ProductionDataModelConstructor` | 22-28 | COVERED | set: `ConstructorDefaults_InvokeProductionConstructionAdapters` L237; get: `ResetProductionFactories_ConstructorDelegatesCreateConcreteInstances` L354 |
| 3 | `ProductionAsyncDataModelFactory` (default `EfcDataModel.CreateAsync`) | 30-37 | COVERED (assignment + read) | set: `Constructor_WithNoOverrides_UsesResettableProductionFactories` L37; get: `EfcHomeControllerDependencies.cs` L67. Default target is **never invoked** (correct — it would start a real async data load) |
| 4 | `ProductionViewerFactory` (default `EfcViewerQueue.Dequeue`) | 39-40 | COVERED (assignment + read) | set: L49 of the same test; get: `EfcHomeControllerDependencies.cs` L68. Default target **never invoked** (correct — it would dequeue a real `EfcViewer` WinForms control) |
| 5 | `ProductionKeyboardHandlerFactory` | 42-46 | COVERED | set L50; get at `EfcHomeControllerDependencies.cs` L183 |
| 6 | `ProductionKeyboardHandlerConstructor` | 48-53 | COVERED | set: `ConstructorDefaults_...` L243; get: `ResetProductionFactories_...` L363 |
| 7 | `ProductionExplorerControllerFactory` | 55-60 | COVERED | set L52; get at `EfcHomeControllerDependencies.cs` L219 |
| 8 | `ProductionExplorerControllerConstructor` | 62-68 | COVERED (property only) | set: `ConstructorDefaults_...` L247; get: `CreateProductionExplorerController` L215 during the same test. **Its default target is never invoked — see #20.** |
| 9 | `ProductionFormControllerWithDataFactory` | 70-71 | COVERED | set L57; get at `EfcHomeControllerDependencies.cs` L269 |
| 10 | `ProductionFormControllerWithDataConstructor` | 73-74 | COVERED | set: `ConstructorDefaults_...` L252; get: `ResetProductionFactories_...` L367 |
| 11 | `ProductionFormControllerWithDataInitializer` (property) | 76-79 | COVERED | set: `ConstructorDefaults_...` L261; get: `CreateProductionFormControllerWithData` L237 |
| 11a | …its **default closure body** `controller => controller.Initialize()` | **80** | **UNCOVERED (block)** | none — see **CCN-1** |
| 12 | `ProductionFormControllerWithoutDataFactory` | 82-83 | COVERED | set L66; get at `EfcHomeControllerDependencies.cs` L328 |
| 13 | `ProductionFormControllerWithoutDataConstructor` | 85-86 | COVERED | set: `ConstructorDefaults_...` L267; get: `ResetProductionFactories_...` L379 |
| 14 | `ProductionFormControllerWithoutDataInitializer` (property) | 88-91 | COVERED | set: `ConstructorDefaults_...` L275; get: `CreateProductionFormControllerWithoutData` L257 |
| 14a | …its **default closure body** `controller => controller.InitializeWithoutData()` | **92** | **UNCOVERED (block)** | none — see **CCN-1** |
| 15 | `ProductionInitializeDataFields` | 94-98 | COVERED | set L74; get at `EfcHomeControllerDependencies.cs` L374 |
| 16 | `ProductionDataFieldsInitializer` (property) | 100-104 | COVERED | set: `ConstructorDefaults_...` L281; get: `CreateProductionDataFields` L265 |
| 16a | …its **default closure body** `(controller, dataModel) => controller.InitializeDataFields(dataModel)` | **105** | **UNCOVERED (block)** | none — see **CCN-1** |
| 17 | `ResetProductionFactoriesForTesting` body (16 assignments) | 107-129 | COVERED | `[TestCleanup] Cleanup()` L19-23 runs it after every test in the class; also called explicitly at `ResetProductionFactories_ConstructorDelegatesCreateConcreteInstances` L347 |
| 17a | …reset closure body on L120 (`controller => controller.Initialize()`, same line as the assignment) | 120 | line hit, **block uncovered** | see **CCN-1** |
| 17b | …reset closure body on **L125** (`controller.InitializeWithoutData()`, own line) | **124-125** | **UNCOVERED (block; L125 likely uncovered as a line)** | see **CCN-1** |
| 17c | …reset closure body on **L128** (`controller.InitializeDataFields(dataModel)`, own line) | **127-128** | **UNCOVERED (block; L128 likely uncovered as a line)** | see **CCN-1** |
| 18 | `CreateProductionDataModelInstance` (`new EfcDataModel(...)`) | 131-139 | COVERED | `ResetProductionFactories_ConstructorDelegatesCreateConcreteInstances` L353-361 (passes `null` mail, a strict-mock `IApplicationGlobals`, a real `CancellationTokenSource`) |
| 19 | `CreateProductionKeyboardHandlerInstance` (`new KeyboardHandler(...)`) | 141-147 | COVERED | same test L362-365 (asserts `BeOfType<KeyboardHandler>()`) |
| 20 | `CreateProductionExplorerControllerInstance` (`new QfcExplorerController(...)`) | **149-156** | **UNCOVERED** | none. `ResetProductionFactories_...` deliberately skips it — it is the only `*Instance` adapter omitted from that test |
| 21 | `CreateProductionFormControllerWithDataInstance` (`new EfcFormController(7-arg)`) | 158-177 | COVERED | same test L366-377 (uses `CreateConfiguredViewer()` L421-427) |
| 22 | `CreateProductionFormControllerWithoutDataInstance` (`new EfcFormController(6-arg)`) | 179-189 | COVERED | same test L378-388 |
| 23 | `CreateProductionDataModel` (composition layer) | 191-199 | COVERED | `ConstructorDefaults_InvokeProductionConstructionAdapters` L294-297 |
| 24 | `CreateProductionKeyboardHandler` | 201-207 | COVERED | same test L298-301 |
| 25 | `CreateProductionExplorerController` | 209-216 | COVERED | same test L302-309 |
| 26 | `CreateProductionFormControllerWithData` (ctor then initializer) | 218-238 | COVERED | same test L310-321 (asserts `withDataInitialized == true` at L339) |
| 27 | `CreateProductionFormControllerWithoutData` | 240-258 | COVERED | same test L322-332 (asserts `withoutDataInitialized == true` at L340) |
| 28 | `CreateProductionDataFields` | 260-266 | COVERED | same test L333-336 (asserts `dataFieldsInitialized == true` at L341) |

### Estimated current line coverage

Roughly **6-9 uncovered sequence-point lines** (the `CreateProductionExplorerControllerInstance`
body plus the five initializer closure bodies) out of an estimated 70-80 coverable lines, i.e.
**~86-91%**. Static estimate only; F1's harness is authoritative.

## 4. Genuine gaps and the specific tests that close them

Proposed new test file:
`QuickFiler.Test\Controllers\EfcHomeControllerDependencyFactoriesTests.cs`
(`[TestClass]`, `[DoNotParallelize]`, `[TestCleanup] ResetProductionFactoriesForTesting()`).

All items below are **test-only**. No production line changes are proposed for this file.

### G-F1 — `CreateProductionExplorerControllerInstance` (the one uncovered method)

- Target: L149-156.
- Barrier: `new QfcExplorerController(initType, globals, homeController)` dereferences
  `_globals.Ol.App.ActiveExplorer()` in its constructor
  (`QuickFiler\Controllers\QfcExplorerController.cs` L27-37).
- Scenario `ProductionExplorerControllerConstructor_AfterReset_CreatesQfcExplorerController`:
  1. `EfcHomeControllerDependencies.ResetProductionFactoriesForTesting();`
  2. build the mock chain `globals.SetupGet(x => x.Ol.App).Returns(app.Object)`,
     `app.Setup(a => a.ActiveExplorer()).Returns(explorer.Object)` — the exact recursive-Moq pattern
     already used in this test project at `QuickFiler.Test\Controllers\QfcHomeControllerTests.cs`
     L44-47;
  3. `var homeController = (EfcHomeController)FormatterServices.GetUninitializedObject(typeof(EfcHomeController));`
     (the `CreateUninitialized<T>()` helper already present in the existing test file at L415-419);
  4. act: `EfcHomeControllerDependencies.ProductionExplorerControllerConstructor(QfEnums.InitTypeEnum.Find, globals.Object, homeController)`;
  5. assert `Should().BeOfType<QfcExplorerController>()` and
     `app.Verify(a => a.ActiveExplorer(), Times.Once)`.
- `QfcExplorerController`'s constructor performs only four field assignments, so no further COM
  interaction occurs. `EfcHomeController` satisfies the `IFilerHomeController` parameter type.
- Determinism: no real COM object, no form, no I/O.

### G-F2 — `ResetProductionFactoriesForTesting` as a first-class state transition

Today the method's body is covered only incidentally by `[TestCleanup]`; nothing asserts that it
actually restores anything. That is a real untested state-transition invariant for a method whose
entire purpose is state restoration.

- Scenario `ResetProductionFactoriesForTesting_AfterAllSixteenAreReplaced_RestoresEveryDefault`:
  1. assign a distinguishable sentinel to all 16 `Production*` statics;
  2. call `ResetProductionFactoriesForTesting()`;
  3. assert each static is no longer the sentinel, and for the method-group defaults assert the
     restored target by name, which is a non-invoking identity check:
     - `ProductionDataModelFactory.Method.Name == "CreateProductionDataModel"`
     - `ProductionDataModelConstructor.Method.Name == "CreateProductionDataModelInstance"`
     - `ProductionAsyncDataModelFactory.Method.Name == "CreateAsync"` (i.e. `EfcDataModel.CreateAsync`)
     - `ProductionViewerFactory.Method.Name == "Dequeue"` (i.e. `EfcViewerQueue.Dequeue`)
     - `ProductionKeyboardHandlerFactory` / `Constructor`,
       `ProductionExplorerControllerFactory` / `Constructor`,
       `ProductionFormControllerWithDataFactory` / `Constructor`,
       `ProductionFormControllerWithoutDataFactory` / `Constructor`,
       `ProductionInitializeDataFields` — same pattern.
- This single test covers L109-128 with meaningful assertions and pins the restoration contract that
  every other test in the F8 suite silently depends on.
- **It must not invoke** `ProductionViewerFactory` or `ProductionAsyncDataModelFactory` — see § 6.

### G-F3 — composition-layer ordering and result propagation (with data)

`ConstructorDefaults_InvokeProductionConstructionAdapters` asserts only *that* the initializer ran
(booleans at L339-341). It does not assert **ordering**, and it does not assert that the value
returned by `CreateProductionFormControllerWithData` is the **initializer's** result rather than the
constructor's result (L228-237 constructs, then returns the initializer's output).

- Scenario `CreateProductionFormControllerWithData_ReturnsInitializerResult_AfterConstructor`:
  - `ProductionFormControllerWithDataConstructor` returns instance `A` and records `"ctor"`;
  - `ProductionFormControllerWithDataInitializer` asserts its argument is `A`, returns instance `B`,
    records `"init"`;
  - invoke via `new EfcHomeControllerDependencies().FormControllerWithDataFactory(...)`;
  - assert result `BeSameAs(B)` and `calls.Should().Equal("ctor", "init")`.
- Covers L228-237 with a genuine behavioural assertion rather than a boolean flag.

### G-F4 — composition-layer ordering and result propagation (without data)

Same shape for `CreateProductionFormControllerWithoutData` (L249-257).

### G-F5 — `CreateProductionDataFields` result propagation

`CreateProductionDataFields` (L260-266) returns whatever `ProductionDataFieldsInitializer` returns.
Assert that a *different* instance returned by the initializer is propagated, rather than the input
`controller`. The existing test returns the same object, so propagation is not actually proven.

### G-F6 — late-binding of the composition layer

`CreateProductionDataModel` (L198) reads `ProductionDataModelConstructor` **at invocation time**.
Scenario: obtain the composition-layer delegate first (`new EfcHomeControllerDependencies()`), then
swap `ProductionDataModelConstructor`, then invoke, and assert the swap took effect. Pins the
two-layer seam's late-binding contract, which no existing test covers.

### G-F7 — the five initializer closure bodies

Lines **80, 92, 105, 125, 128**. **Not closable from F8.** See § 7, CCN-1.

## 5. Lazy vs. cached resolution — state-transition invariants

Verified from source:

1. **Nothing in this file is lazy or memoized.** All 16 `Production*` members are ordinary
   `{ get; set; }` auto-properties with static initializers. There is no `Lazy<T>`, no
   double-checked locking, and no caching of constructed objects.
2. **Every dependency is re-created per call.** The `*Instance` adapters (L131-189) execute `new` on
   each invocation; the composition adapters (L191-266) re-read their `*Constructor` /
   `*Initializer` static on each invocation. Calling
   `ProductionFormControllerWithDataConstructor(...)` twice yields two distinct
   `EfcFormController` instances. This "no memoization" property is an invariant worth an explicit
   test (`ProductionConstructors_InvokedTwice_ReturnDistinctInstances`) because a future performance
   change to add caching would silently alter `EfcHomeController` disposal semantics.
3. **State is process-global and mutable, and the only transition operator is
   `ResetProductionFactoriesForTesting`.** Because the statics survive across tests, the state
   machine is: `defaults -> (test replaces N statics) -> replaced -> Reset() -> defaults`. G-F2 is
   the test of the reset edge; G-F6 is the test that the read edge is late-bound.
4. **Asymmetry with the constructor partial.** `EfcHomeControllerDependencies`'s constructor binds
   `ProductionAsyncDataModelFactory` (L67 of the sibling file) and `ProductionViewerFactory` (L68)
   **eagerly**, while the other six defaults are bound **late** through private adapters. A swap of
   those two statics after an instance is constructed therefore has no effect on that instance. This
   is real and untested; the test is named in the sibling artifact (§ 4 item 4).
5. **No thread-safety.** The `Production*` statics are plain static properties with no volatility or
   synchronization. Concurrent mutation from parallel test classes is a real hazard — see § 8.

## 6. COM / Outlook-Interop reachability

Interop reach in this file is **indirect**: the `*Instance` adapters construct concrete types whose
constructors touch COM or WinForms. Testability per adapter:

| Adapter | Constructs | Interop/WinForms touched in ctor | Testable? |
| --- | --- | --- | --- |
| `CreateProductionDataModelInstance` (L131-139) | `EfcDataModel` | none observed; a strict `IApplicationGlobals` mock and `null` mail suffice | **Yes — already proven** (`ResetProductionFactories_...` L353-361) |
| `CreateProductionKeyboardHandlerInstance` (L141-147) | `KeyboardHandler` | reads `viewer.ItemViewer` / `viewer.L0vh_TLP` | **Yes — already proven** via `CreateConfiguredViewer()` (existing test L421-427): an uninitialized `EfcViewer` with an uninitialized `ItemViewer` and a real in-memory, never-shown `TableLayoutPanel` |
| `CreateProductionExplorerControllerInstance` (L149-156) | `QfcExplorerController` | `globals.Ol.App.ActiveExplorer()` (`QfcExplorerController.cs` L35) | **Yes** — mockable interface chain; this is gap G-F1 |
| `CreateProductionFormControllerWithDataInstance` (L158-177) | `EfcFormController` (7-arg) | reads `_formViewer.ItemViewer` / `.L0vh_TLP`; transitively `new EfcItemController(...)` | **Yes — already proven** (existing test L366-377) |
| `CreateProductionFormControllerWithoutDataInstance` (L179-189) | `EfcFormController` (6-arg) | same (`EfcFormController.cs` L62-77) | **Yes — already proven** (existing test L378-388) |
| default `ProductionViewerFactory` = `EfcViewerQueue.Dequeue` (L39-40) | a real `EfcViewer` form | WinForms form construction | **No — must never be invoked.** Assert delegate identity only |
| default `ProductionAsyncDataModelFactory` = `EfcDataModel.CreateAsync` (L37) | async data load over a `List<MailItem>` | Outlook item traversal | **No — must never be invoked.** Assert delegate identity only |
| the three `Initializer` closures (L80, L92, L105) | call `EfcFormController.Initialize()` / `.InitializeWithoutData()` / `.InitializeDataFields(...)` | see § 7 | **No — CCN-1** |

No `Store` and no `MAPIFolder` is reachable from this file. `MailItem` appears only as a forwarded
parameter and is never dereferenced here.

**Verdict on the CLAUDE.md § UT2 exemption:** it does not apply to this file. Injectable seams exist
for the whole construction layer, and the two-layer split was built precisely to provide them. Under
the epic's refactor-first reconciliation (epic.md § "Shared Design" item 1) this file must be
covered, not exempted.

## 7. Cross-child contract (F9 boundary)

### (a) What `EfcFormController.cs` / `EfcItemController.cs` consume from this file

**Verified by grep: nothing.** A repository-wide search for
`EfcHomeControllerDependencies|Production[A-Za-z]*Factory|Production[A-Za-z]*Constructor|Production[A-Za-z]*Initializer`
across all `*.cs` matched 13 files; `QuickFiler\Controllers\EfcFormController.cs` and
`QuickFiler\Controllers\EfcItemController.cs` are **not** among them.

The consumption runs the other way — **this file consumes F9's surface**:

| F9 member consumed | F9 location | Consumed at (this file) |
| --- | --- | --- |
| `EfcFormController(IApplicationGlobals, EfcDataModel, EfcViewer, EfcHomeController, System.Action, QfEnums.InitTypeEnum, CancellationToken)` | `EfcFormController.cs` L32-52 | L168-176 |
| `EfcFormController(IApplicationGlobals, EfcViewer, EfcHomeController, System.Action, QfEnums.InitTypeEnum, CancellationToken)` | `EfcFormController.cs` L53-77 | L188 |
| `internal EfcFormController Initialize()` | `EfcFormController.cs` L81-99 | L80 and L120 |
| `internal EfcFormController InitializeWithoutData()` | `EfcFormController.cs` L101-111 | L92 and L124-125 |
| `internal EfcFormController InitializeDataFields(EfcDataModel)` | `EfcFormController.cs` L113-119 | L105 and L127-128 |
| `EfcFormController` (type) | `EfcFormController.cs` L28 | L70-105, L158-189, L218-266 |
| `EfcViewer` (type) and its `ItemViewer` / `L0vh_TLP` members | `Viewers\EfcViewer.cs` | L160, L181 (parameter flow into the `EfcFormController` ctors) |
| `EfcItemController` — **transitively only** | `EfcItemController.cs` L25 | both `EfcFormController` ctors execute `new EfcItemController(...)` (`EfcFormController.cs` L69-75), so L168 and L188 construct a real `EfcItemController` |

Both F9 controllers currently carry `[ExcludeFromCodeCoverage]` (`EfcFormController.cs` L27,
`EfcItemController.cs` L25). Note that the **existing** F8 test
`ResetProductionFactories_ConstructorDelegatesCreateConcreteInstances` already constructs real
`EfcFormController` and (transitively) real `EfcItemController` instances without STA, without a
shown form, and without a live Outlook process. That is the working precedent for construction-layer
coverage and it requires no F9 edit.

### (b) Additive-only verdict per proposed change

| Proposed change | Requires an F9 edit? | Verdict |
| --- | --- | --- |
| G-F1 `ProductionExplorerControllerConstructor_AfterReset_CreatesQfcExplorerController` | No | **ADDITIVE — test-only.** Constructs `QfcExplorerController` (F6-owned) read-only; no F9 type involved |
| G-F2 `ResetProductionFactoriesForTesting_...RestoresEveryDefault` | No | **ADDITIVE — test-only.** Identity assertions only, no invocation of F9 members |
| G-F3 / G-F4 form-controller composition ordering | No | **ADDITIVE — test-only.** Both `Constructor` and `Initializer` statics are replaced with fakes, so no real `EfcFormController` method is called |
| G-F5 `CreateProductionDataFields` propagation | No | **ADDITIVE — test-only** |
| G-F6 late-binding of the composition layer | No | **ADDITIVE — test-only** |
| "no memoization" invariant test (§ 5 item 2) | No | **ADDITIVE — test-only.** Uses replaced `*Constructor` statics |

**No production line of `EfcHomeControllerDependencyFactories.cs` changes.** No new constructor
overload, no new property, no new interface, no delegate-type change, no signature change, no
removal. F9 requires no edit for any proposed work.

### (c) CROSS-CHILD CONTRACT NOTE — CCN-1 (record verbatim in `spec.md`)

**Gap:** the five initializer closure bodies at L80, L92, L105, L125, L128 cannot be executed by any
F8 test.

**Why:** each body invokes an F9-owned method on a live `EfcFormController`:

- L80 / L120 -> `EfcFormController.Initialize()` (`EfcFormController.cs` L81-99)
- L92 / L125 -> `EfcFormController.InitializeWithoutData()` (`EfcFormController.cs` L101-111)
- L105 / L128 -> `EfcFormController.InitializeDataFields(EfcDataModel)` (`EfcFormController.cs` L113-119)

`Initialize()` and `InitializeWithoutData()` both begin with `LoadUserSettings()`
(`EfcFormController.cs` L1009-1022), which reads user-scope `Settings.Default` (disk-backed
configuration) and then dereferences designer-generated menu-item controls
(`_formViewer.SaveAttachmentsMenuItem.Checked` etc.), followed by `CaptureConfigureItemViewer()`,
`ConfigureFind()`, `ResolveControlGroups()`, `SetupThemes()`, `WireEventHandlers()` and
`PopulateFolderCombobox()`. `InitializeDataFields` calls into `EfcItemController` and
`PopulateFolderCombobox()`. Reaching any of these deterministically would require a live
`EfcViewer` / `ItemViewer` form graph, which the epic's shared design and UT4 both prohibit.

**Exact change that would be required, and the exact F9 call sites affected, if the epic later
decides to close these five lines:**

- Add a new interface file (F8-creatable, e.g.
  `QuickFiler\Interfaces\IEfcFormControllerInitialization.cs`) declaring
  `EfcFormController Initialize(); EfcFormController InitializeWithoutData(); EfcFormController InitializeDataFields(EfcDataModel dataModel);`
- Edit **`QuickFiler\Controllers\EfcFormController.cs` line 28** — the class declaration — from
  `internal class EfcFormController : IFilerFormController` to
  `internal class EfcFormController : IFilerFormController, IEfcFormControllerInitialization`.
  The three methods already have matching signatures, so no method body changes.
- That is the **only** F9 call site affected. No other F9 line changes.
- Honest caveat: adding the interface alone does **not** by itself make L80/L92/L105 coverable,
  because those closures are typed `Func<EfcFormController, EfcFormController>` and would still call
  the concrete method. Full closure would additionally require changing the
  `Production*Initializer` property types to accept the interface, which cascades into the return
  types of `CreateProductionFormControllerWithData` / `...WithoutData` (L237, L257). That cascade is
  a real design change, not a one-line addition.

**Recommendation:** do **not** perform the F9 edit. Leave the five lines uncovered. They are
single-statement pass-throughs into `[ExcludeFromCodeCoverage]`-marked F9 members, they represent
roughly 5 lines out of ~75 coverable, and the file clears the 80% floor without them. Record CCN-1
in `spec.md` as a known residual with "no F9 edit required, no F8 production change proposed", and
escalate only if F1's ledger sets a bar above 80% for this file (epic.md sets 80%).

### (d) CROSS-CHILD CONTRACT NOTE — CCN-2 (informational, non-blocking)

G-F1 constructs `QfcExplorerController`, which is assigned to sibling child **F6**
(`quickfiler-qfc-form-explorer-controller-coverage`; epic.md F6 file list). F8 only *constructs* it
in a test; no F6 file is edited. Residual coupling: if F6 changes that constructor's signature or
removes the `ActiveExplorer()` call (`QfcExplorerController.cs` L35), F8's G-F1 mock setup needs a
one-line update. Record as a note in `spec.md`; it is not a blocker and does not affect merge order.

## 8. Test-suite hazards to respect

- **Process-global mutable statics + class-level parallelism.**
  `scripts\vscode\TaskMaster.cli.runsettings` L4-7 sets `<Scope>ClassLevel</Scope>` with
  `<Workers>0</Workers>`, so **test classes execute in parallel**. Every one of the 16 `Production*`
  members is unsynchronized shared state. The existing class
  `EfcHomeControllerDependenciesTestsProductionFactory` is the only class mutating them today and is
  **not** marked `[DoNotParallelize]`, which is safe only because it is the sole mutator. Adding a
  second mutating class makes the hazard live.
  - **Required:** mark the new
    `QuickFiler.Test\Controllers\EfcHomeControllerDependencyFactoriesTests.cs` class
    `[DoNotParallelize]` and give it `[TestCleanup] ResetProductionFactoriesForTesting()`.
  - **Recommended:** also add `[DoNotParallelize]` to the existing
    `EfcHomeControllerDependenciesTestsProductionFactory`. That is a test-file-only change inside
    F8's scope.
  - In-repo precedent for exactly this pattern:
    `QuickFiler.Test\Helper Classes\ViewerQueueStaticWrapperTests.cs` L11.
- **Never invoke** the default `ProductionViewerFactory` (`EfcViewerQueue.Dequeue`, L39-40) or the
  default `ProductionAsyncDataModelFactory` (`EfcDataModel.CreateAsync`, L37). Identity assertions
  via `.Method.Name` only.
- **Never invoke** the three default `*Initializer` closures (CCN-1).

## 9. Do not duplicate — scenarios already covered

Do **not** re-author any of the following:

1. Replacing all seven top-level `Production*Factory` statics before construction and verifying the
   `EfcHomeControllerDependencies` instance routes to them —
   `EfcHomeControllerDependenciesTestsProductionFactory.Constructor_WithNoOverrides_UsesResettableProductionFactories`.
2. Replacing the `*Constructor` and `*Initializer` statics and verifying every composition adapter
   (`CreateProductionDataModel`, `CreateProductionKeyboardHandler`,
   `CreateProductionExplorerController`, `CreateProductionFormControllerWithData`,
   `CreateProductionFormControllerWithoutData`, `CreateProductionDataFields`) routes through them —
   `...ConstructorDefaults_InvokeProductionConstructionAdapters`. Only the **ordering** and
   **result-propagation** aspects (G-F3/G-F4/G-F5) are missing; do not re-test the routing itself.
3. Concrete construction by `CreateProductionDataModelInstance`,
   `CreateProductionKeyboardHandlerInstance`, `CreateProductionFormControllerWithDataInstance`,
   and `CreateProductionFormControllerWithoutDataInstance` after a reset —
   `...ResetProductionFactories_ConstructorDelegatesCreateConcreteInstances`. Only the
   **explorer-controller** instance adapter is missing (G-F1).
4. `factory`-null `ArgumentNullException` guards on the six `...WithFactory` helpers —
   `...WithFactoryHelpers_ValidateFactoryArguments`. Those guards live in
   `EfcHomeControllerDependencies.cs`, not this file, and are fully covered there.
5. `LoadSelection` with an explicit `MailItem` — covered twice already (see the sibling artifact).
6. The `[TestCleanup]` invocation of `ResetProductionFactoriesForTesting` — the *call* is covered;
   only the *assertion* that it restores defaults is missing (G-F2).

## 10. Line-count risk (500-line ceiling)

- Current: **268 lines**. Ceiling: 500. Headroom: **232 lines**.
- The proposed change set adds **zero** production lines; the file remains at 268. No 500-line risk.
- Contingency, if a future increment genuinely needs new production members here: create a new
  F8-owned partial file `QuickFiler\Controllers\EfcHomeControllerDependencyFactories.Initialization.cs`
  rather than appending. Do not append to `EfcHomeControllerDependencies.cs`, which is at 428 lines
  with only 72 lines of headroom. Do not create a partial file for a sibling-owned type.

## 11. Upstream dependency on F1

- The **testable vs ratified-exempt** classification for this file is owned by F1's ledger at
  `docs\features\epics\quickfiler-per-file-coverage\coverage-ledger.md`. That file does not exist on
  disk yet and was not read. Based on § 6 this file is expected to be classified **testable**; it
  carries no `[ExcludeFromCodeCoverage]` today and is already in the denominator.
- The numeric per-file figure must come from **F1's per-file coverage harness** (Cobertura output of
  `Invoke-MSTestWithCoverage.ps1`, per epic.md § "Per-file coverage measurement") and be committed
  under
  `docs\features\active\2026-08-07-quickfiler-efc-home-controller-coverage-437\evidence\qa-gates\`.
  The ~86-91% estimate in § 3 is static analysis and is **not** acceptable as evidence.
- F1 also owns the disposition of the 33 existing `[ExcludeFromCodeCoverage]` attributes. Neither of
  F8's two dependency files carries one, so F8 has no attribute to remove for this pair.

## 12. Test strategy summary

- Framework: MSTest, Moq, FluentAssertions (CLAUDE.md § CUT1/CUT2).
- New file: `QuickFiler.Test\Controllers\EfcHomeControllerDependencyFactoriesTests.cs`,
  `[TestClass]`, `[DoNotParallelize]`, `[TestCleanup] EfcHomeControllerDependencies.ResetProductionFactoriesForTesting()`.
- Six named gap tests (G-F1 … G-F6) plus the "no memoization" invariant test from § 5 item 2.
- Arrange-Act-Assert; no temporary files, no external services, no live or shown forms, no popups,
  no `Thread.Sleep` / `Task.Delay`, no real Outlook object created. The only WinForms object needed
  is the in-memory, never-shown `TableLayoutPanel` already used by the existing
  `CreateConfiguredViewer()` helper, which does not require STA in the current suite.
- Seam hierarchy respected with **no** production change: the existing injectable-delegate seam (the
  16 `Production*` statics) plus the `IApplicationGlobals` / `IOlObjects` interface seam are
  sufficient for every closable gap.
