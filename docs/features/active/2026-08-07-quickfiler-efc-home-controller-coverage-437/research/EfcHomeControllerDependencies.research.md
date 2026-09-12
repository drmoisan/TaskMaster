# Research — `QuickFiler/Controllers/EfcHomeControllerDependencies.cs`

- **Feature:** `2026-08-07-quickfiler-efc-home-controller-coverage-437` (issue #437)
- **Epic:** #136 `quickfiler-per-file-coverage`, child F8
- **Production file:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aea998f94efaa2eb4\QuickFiler\Controllers\EfcHomeControllerDependencies.cs` (428 lines)
- **Method:** static analysis only. No build, no test run, no coverage run was performed.
- **Coverage status of the file itself:** the file carries **no** `[ExcludeFromCodeCoverage]` attribute, so it is already inside the coverage denominator.

## 1. Current state

`EfcHomeControllerDependencies` is an `internal sealed partial class` in namespace `QuickFiler`. This
file holds the injection-seam surface: two delegate type declarations, one optional-parameter
constructor, eleven get-only delegate properties, six private static default adapters, six
`internal static ...WithFactory` guard-and-forward helpers, and one `internal static LoadSelection`.

The second half of the partial lives in `EfcHomeControllerDependencyFactories.cs` (268 lines) and is
researched in the sibling artifact `EfcHomeControllerDependencyFactories.research.md`.

### Consumers

`EfcHomeController.cs` is the only production consumer. Verified call sites in
`QuickFiler\Controllers\EfcHomeController.cs`:

| Member | EfcHomeController.cs lines |
| --- | --- |
| `EfcHomeControllerDependencies()` ctor | 24-25, 37, 52, 110, 146 (via `_defaultDependenciesFactory` / `CreateDefaultDependencies`) |
| `DataModelFactory` | 66, 250 |
| `AsyncDataModelFactory` | 214 |
| `ViewerFactory` | 77, 226 |
| `KeyboardHandlerFactory` | 79, 228 |
| `ExplorerControllerFactory` | 80, 229 |
| `FormControllerWithDataFactory` | 85 |
| `FormControllerWithoutDataFactory` | 230 |
| `InitializeDataFields` | 245, 251 |
| `SelectionLoader`, `MetricsNowFactory`, `MetricsLineWriter` | consumed by the sibling partials `EfcHomeController.Metrics.cs` / `EfcHomeController.ExecuteMoves.cs` (also F8-owned) |

### Existing test files touching this file

- `QuickFiler.Test\Controllers\EfcHomeControllerDependenciesTests.cs` — class `EfcHomeControllerDependenciesTests` (476 lines)
- `QuickFiler.Test\Controllers\EfcHomeControllerDependenciesProductionFactoryTests.cs` — class **`EfcHomeControllerDependenciesTestsProductionFactory`** (473 lines; note the class name does not match the file name)
- `QuickFiler.Test\Controllers\EfcHomeControllerSeamTests.cs` — constructs the type with full overrides (L210-274)
- `QuickFiler.Test\Controllers\EfcHomeControllerLifecycleTests.cs` — constructs the type with full overrides (L285-339)
- `QuickFiler.Test\Controllers\EfcHomeControllerMetricsTests.cs` — constructs the type with overrides (L160)

## 2. Member-by-member inventory

Legend: **COVERED** = an existing test provably executes the line(s); **UNCOVERED** = no existing
test reaches them. Test citations are `Class.Method`.

| # | Member | Lines | Status | Covering test(s) |
| --- | --- | --- | --- | --- |
| 1 | `delegate FormControllerWithDataFactoryDelegate` | 15-23 | N/A (no executable lines) | Type referenced by `EfcHomeControllerDependenciesTests.Constructor_WithOverrides_PreservesInjectedDelegates` (L73) |
| 2 | `delegate FormControllerWithoutDataFactoryDelegate` | 25-32 | N/A (no executable lines) | Same test (L76) |
| 3 | ctor `EfcHomeControllerDependencies(...)` — default (`??` right-hand) branch of all 11 params | 34-79 | COVERED | `EfcHomeControllerDependenciesTests.Constructor_WithNoOverrides_InstallsProductionDefaults`; `EfcHomeControllerDependenciesTestsProductionFactory.Constructor_WithNoOverrides_UsesResettableProductionFactories`; `...ConstructorDefaults_InvokeProductionConstructionAdapters` |
| 3b | ctor — injected (`??` left-hand) branch of all 11 params | 66-78 | COVERED | `EfcHomeControllerDependenciesTests.Constructor_WithOverrides_PreservesInjectedDelegates` (all 11 supplied); also `EfcHomeControllerSeamTests` / `EfcHomeControllerLifecycleTests` probes |
| 3c | ctor — **lambda body** `() => DateTime.Now` | 77 | **UNCOVERED (block)** | The *line* is hit by the assignment; the closure body is never invoked. `Constructor_WithNoOverrides_InstallsProductionDefaults` only asserts `NotBeNull` (L32). |
| 3d | ctor — default `FileIO2.WriteTextFile` method group | 78 | COVERED as assignment; **never invoked (by design)** | Invoking it would write to disk; prohibited by UT4. |
| 4 | `DataModelFactory` get | 81-87 | COVERED | `Constructor_WithNoOverrides_InstallsProductionDefaults` (L23), `Constructor_WithOverrides_...` (L103) |
| 5 | `AsyncDataModelFactory` get | 89-96 | COVERED | same (L24, L104) |
| 6 | `ViewerFactory` get | 98 | COVERED | same (L25, L105) |
| 7 | `KeyboardHandlerFactory` get | 100-104 | COVERED | same (L26, L106) |
| 8 | `ExplorerControllerFactory` get | 106-111 | COVERED | same (L27, L107) |
| 9 | `FormControllerWithDataFactory` get | 113 | COVERED | same (L28, L108) |
| 10 | `FormControllerWithoutDataFactory` get | 115 | COVERED | same (L29, L109) |
| 11 | `InitializeDataFields` get | 117-121 | COVERED | same (L30, L110) |
| 12 | `SelectionLoader` get | 123 | COVERED | same (L31, L111) |
| 13 | `MetricsNowFactory` get | 125 | COVERED | same (L32, L112 — invoked) |
| 14 | `MetricsLineWriter` get | 127 | COVERED | same (L33, L114 — invoked with an injected writer) |
| 15 | `CreateDataModel` (private static default adapter) | 129-143 | COVERED | `EfcHomeControllerDependenciesTestsProductionFactory.Constructor_WithNoOverrides_UsesResettableProductionFactories` (L84) and `...ConstructorDefaults_InvokeProductionConstructionAdapters` (L295) invoke `dependencies.DataModelFactory(...)`, which is this method |
| 16 | `CreateDataModelWithFactory` — happy path + forwarding | 145-158, 172-173 | COVERED | `EfcHomeControllerDependenciesTests.CreateDataModelWithFactory_ValidatesAndForwardsArguments` (L140-155) |
| 16a | guard `globals is null` throw | 159-162 | COVERED | same test (L156-167) |
| 16b | guard `tokenSource is null` throw | 163-166 | COVERED | same test (L168-179) |
| 16c | guard `factory is null` throw | 167-170 | COVERED | `EfcHomeControllerDependenciesTestsProductionFactory.WithFactoryHelpers_ValidateFactoryArguments` (L150-160) |
| 16d | **absence** of a `mail` guard (null `mail` is legal and forwarded) | 172 | COVERED implicitly | `EfcHomeControllerSeamTests.LoadFinderAsync_WithEmptySelection_...` asserts `mail.Should().BeNull()` inside the injected factory (L104); `ResetProductionFactories_ConstructorDelegatesCreateConcreteInstances` passes `null` mail (L356) |
| 17 | `CreateKeyboardHandler` (private static) | 175-185 | COVERED | `Constructor_WithNoOverrides_UsesResettableProductionFactories` (L103), `ConstructorDefaults_InvokeProductionConstructionAdapters` (L299) |
| 18 | `CreateKeyboardHandlerWithFactory` — happy path | 187-194, 206-207 | COVERED | `EfcHomeControllerDependenciesTests.CreateKeyboardHandlerWithFactory_ValidatesViewerAndHomeController` (L190-201) |
| 18a | guard `viewer is null` | 192-195 | COVERED | same test (L202-210) |
| 18b | guard `homeController is null` | 196-199 | COVERED | same test (L211-219) |
| 18c | guard `factory is null` | 200-204 | COVERED | `WithFactoryHelpers_ValidateFactoryArguments` (L162-170) |
| 19 | `CreateExplorerController` (private static) | 209-221 | COVERED | `Constructor_WithNoOverrides_UsesResettableProductionFactories` (L107), `ConstructorDefaults_InvokeProductionConstructionAdapters` (L303) |
| 20 | `CreateExplorerControllerWithFactory` — happy path | 223-234, 248-249 | COVERED | `EfcHomeControllerDependenciesTests.CreateExplorerControllerWithFactory_ValidatesGlobalsAndHomeController` (L229-242) |
| 20a | guard `globals is null` | 234-238 | COVERED | same test (L243-252) |
| 20b | guard `homeController is null` | 239-242 | COVERED | same test (L253-262) |
| 20c | guard `factory is null` | 243-246 | COVERED | `WithFactoryHelpers_ValidateFactoryArguments` (L171-180) |
| 21 | `CreateInitializedFormControllerWithData` (private static) | 251-271 | COVERED | `Constructor_WithNoOverrides_UsesResettableProductionFactories` (L115), `ConstructorDefaults_InvokeProductionConstructionAdapters` (L311) |
| 22 | `CreateInitializedFormControllerWithDataFactory` — happy path | 273-283, 309-310 | COVERED | `EfcHomeControllerDependenciesTests.CreateInitializedFormControllerWithDataFactory_ValidatesRequiredArguments` (L270-287) |
| 22a-e | guards `globals` / `dataModel` / `viewer` / `homeController` / `cleanup` | 284-303 | COVERED | same test (L288-299 via helper `CreateFormControllerWithData`, L371-394) |
| 22f | guard `factory is null` | 304-307 | COVERED | `WithFactoryHelpers_ValidateFactoryArguments` (L181-194) |
| 23 | `CreateInitializedFormControllerWithoutData` (private static) | 312-330 | COVERED | `Constructor_WithNoOverrides_UsesResettableProductionFactories` (L127), `ConstructorDefaults_InvokeProductionConstructionAdapters` (L323) |
| 24 | `CreateInitializedFormControllerWithoutDataFactory` — happy path | 332-341, 363-364 | COVERED | `EfcHomeControllerDependenciesTests.CreateInitializedFormControllerWithoutDataFactory_ValidatesRequiredArguments` (L306-322) |
| 24a-d | guards `globals` / `viewer` / `homeController` / `cleanup` | 342-357 | COVERED | same test (L323-329 via helper `CreateFormControllerWithoutData`, L396-411) |
| 24e | guard `factory is null` | 358-361 | COVERED | `WithFactoryHelpers_ValidateFactoryArguments` (L195-207) |
| 25 | `InitializeFormControllerDataFields` (private static) | 366-376 | COVERED | `Constructor_WithNoOverrides_UsesResettableProductionFactories` (L138), `ConstructorDefaults_InvokeProductionConstructionAdapters` (L334) |
| 26 | `InitializeFormControllerDataFieldsWithFactory` — happy path | 378-383, 397-398 | COVERED | `EfcHomeControllerDependenciesTests.InitializeFormControllerDataFieldsWithFactory_ValidatesArguments` (L338-350) |
| 26a | guard `controller is null` | 384-387 | COVERED | same test (L351-359) |
| 26b | guard `dataModel is null` | 388-391 | COVERED | same test (L360-368) |
| 26c | guard `factory is null` | 392-395 | COVERED | `WithFactoryHelpers_ValidateFactoryArguments` (L208-216) |
| 27 | `LoadSelection` — guard `globals is null` | 400-405 | **UNCOVERED** | none |
| 28 | `LoadSelection` — explicit-mail branch (`mail is not null`) | 407-413 | COVERED (duplicated twice) | `EfcHomeControllerDependenciesTests.LoadSelection_WithExplicitMail_ReturnsOnlyExplicitMail`; `EfcHomeControllerDependenciesTestsProductionFactory.LoadSelection_WithExplicitMail_DoesNotTraverseOutlookSelection` |
| 29 | `LoadSelection` — Outlook selection path, `selection.Count > 0` true | 415-423 | **UNCOVERED** | none |
| 30 | `LoadSelection` — `selection.Count > 0` false (empty selection returns empty list) | 415-417, 425 | **UNCOVERED** | none |
| 31 | `LoadSelection` — `Where(x => x is MailItem)` filter lambda (non-`MailItem` element dropped) | 420 | **UNCOVERED** | none |

### Estimated current line coverage

Approximately **90-93%** of sequence-point lines, with roughly 7-9 uncovered lines concentrated in
`LoadSelection` (L403-405 and L415-425). This is a static estimate; the authoritative number must
come from F1's per-file coverage harness. Even at the low end the file is very likely **already
above the 80% floor**, so F8's work on this file is gap-closure and invariant-pinning rather than
rescue work.

## 3. Genuine gaps and the specific tests that close them

All items below are **test-only**. No production line changes are proposed for this file.

Proposed new test file: `QuickFiler.Test\Controllers\EfcHomeControllerDependenciesSelectionTests.cs`
(mirrors the production tree per `.claude/rules/general-unit-test.md` § Test File Location).

### G-D1 — `LoadSelection` null-`globals` rejection (negative)

- Target: L402-405.
- Scenario: `Action act = () => EfcHomeControllerDependencies.LoadSelection(null, null);`
  assert `Throw<ArgumentNullException>().Where(e => e.ParamName == "globals")`.
- Determinism: no mocks, no I/O. Trivially deterministic.

### G-D2 — `LoadSelection` Outlook selection path, mixed contents (positive + filter branch)

- Target: L415-423 and the `x is MailItem` filter lambda at L420.
- Mock chain (all interfaces; **proven in-repo**, see § 5):
  - `var globals = new Mock<IApplicationGlobals>(MockBehavior.Loose);`
  - `globals.SetupGet(x => x.Ol.App).Returns(app.Object);` (recursive Moq chain — exact pattern at `QuickFiler.Test\Controllers\QfcHomeControllerTests.cs` L47)
  - `app.Setup(a => a.ActiveExplorer()).Returns(explorer.Object);`
  - `explorer.Setup(e => e.Selection).Returns(selection.Object);`
  - `selection.Setup(s => s.Count).Returns(3);`
  - `selection.As<IEnumerable>().Setup(s => s.GetEnumerator()).Returns(new List<object> { mail1, new object(), mail2 }.GetEnumerator());`
- Act: `LoadSelection(globals.Object, null)`.
- Assert: `result.Should().Equal(mail1, mail2)` — proves both the `Cast`/`Where` filter and the
  ordering of the retained items.
- Determinism: no COM instance is created; `MailItem` mocks use `MockBehavior.Loose` exactly as the
  existing tests do (`EfcHomeControllerDependenciesTests.cs` L123).

### G-D3 — `LoadSelection` empty selection (boundary, `Count == 0`)

- Target: the false arm of `if (selection.Count > 0)` at L417, and the L425 return.
- Same chain with `selection.Setup(s => s.Count).Returns(0)`.
- Assert: `result.Should().BeEmpty()` **and** `selection.As<IEnumerable>().Verify(s => s.GetEnumerator(), Times.Never)` —
  the `Verify` is what makes this a real branch test rather than a duplicate of G-D2.

### G-D4 — `LoadSelection` selection of exactly one item (boundary, `Count == 1`)

- Target: lower boundary of the `> 0` comparison.
- Same chain, `Count = 1`, single `MailItem` in the enumerator.
- Assert: `result.Should().ContainSingle().Which.Should().BeSameAs(mail)`.

### G-D5 — default `MetricsNowFactory` closure body (L77)

- Target: the `() => DateTime.Now` closure block, which no test currently invokes.
- Scenario:
  ```
  var before = DateTime.Now;
  var value = new EfcHomeControllerDependencies().MetricsNowFactory();
  var after = DateTime.Now;
  value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
  ```
- Determinism note: this is a **bounded-interval** assertion, not a fixed-value assertion, so it is
  deterministic and requires no sleep. `.claude/rules/general-unit-test.md` bans wall-clock *waits*
  and bans reading wall-clock time in *code under test that should use an injected clock*; here the
  production line under test **is** the default clock adapter, and a bounded assertion is the only
  way to execute it. Record this rationale in the test's summary comment. If a reviewer objects,
  the fallback is to leave the closure block uncovered — line L77 already registers a hit from the
  assignment, so the line-coverage number is unaffected either way.

### G-D6 — late-binding invariant: default delegates read the statics at **invocation** time

This is the single most important untested behavioral invariant in the file, and it is a genuine
state-transition rule (see § 4).

- Target: L66/L141 (`CreateDataModel` reads `ProductionDataModelFactory` when called, not when the
  `EfcHomeControllerDependencies` instance is constructed).
- Scenario `DataModelFactory_WhenProductionFactoryReplacedAfterConstruction_UsesReplacement`:
  1. `ResetProductionFactoriesForTesting()`;
  2. `var deps = new EfcHomeControllerDependencies();` (construct **first**);
  3. assign a sentinel to `EfcHomeControllerDependencies.ProductionDataModelFactory`;
  4. invoke `deps.DataModelFactory(globals, mail, cts, cts.Token)`;
  5. assert the sentinel's value is returned.
- Requires `[TestCleanup] ResetProductionFactoriesForTesting()` and `[DoNotParallelize]` (see § 7).
- Every existing test sets the statics **before** construction, so this ordering is untested today.

### G-D7 — the complementary invariant: an injected override ignores later static swaps

- Scenario `DataModelFactory_WhenOverrideInjected_IgnoresLaterProductionFactoryReplacement`:
  construct with an explicit `dataModelFactory`, then swap the static, invoke, assert the injected
  delegate still wins. Pins the precedence rule of the `??` at L66.

### G-D8 — delegate identity stability (no per-call re-creation)

- Scenario `DelegateProperties_ReturnTheSameInstanceOnRepeatedReads`: for each of the 11 properties,
  `deps.X.Should().BeSameAs(deps.X)`. Cheap, and pins the "resolved once in the constructor,
  never re-created" contract that F9/F7 consumers rely on for event-handler identity.

## 4. Lazy vs. eager resolution — state-transition invariants

Verified from source, not assumed:

1. **Resolution is eager and one-shot.** All eleven delegate fields are assigned in the constructor
   body (L66-78). There is no `Lazy<T>`, no null-coalescing-assignment in a getter, and no
   backing-field memoization. Every property is a get-only auto-property (L81-127).
2. **Instance state is immutable after construction.** No setter exists on any of the eleven
   properties, so no post-construction mutation of an `EfcHomeControllerDependencies` instance is
   possible.
3. **But the *defaults* are late-bound through mutable process-global statics.** The six private
   default adapters (`CreateDataModel` L129-143, `CreateKeyboardHandler` L175-185,
   `CreateExplorerController` L209-221, `CreateInitializedFormControllerWithData` L251-271,
   `CreateInitializedFormControllerWithoutData` L312-330, `InitializeFormControllerDataFields`
   L366-376) read the corresponding `Production*Factory` static from
   `EfcHomeControllerDependencyFactories.cs` **on each invocation** (e.g. L141, L183, L219, L269,
   L328, L374). Consequence: an instance constructed with defaults observes any later mutation of
   the statics. This is the invariant G-D6 pins.
4. **Three of the eleven defaults are bound eagerly, not late.** `AsyncDataModelFactory` (L67),
   `ViewerFactory` (L68) and the two metrics delegates (L77-78) are assigned **directly** from the
   static property value / method group at construction time rather than through a private adapter.
   A later swap of `ProductionAsyncDataModelFactory` or `ProductionViewerFactory` therefore does
   **not** affect an already-constructed instance. This asymmetry between L67-68 and L66/L69-76 is
   real, undocumented, and untested; it deserves an explicit test
   (`AsyncDataModelFactory_WhenProductionFactoryReplacedAfterConstruction_KeepsOriginal`) so that a
   future refactor cannot silently change it.

## 5. COM / Outlook-Interop reachability

- The **only** direct interop reach in this file without an injectable seam is
  `globals.Ol.App.ActiveExplorer().Selection` at L415, plus `selection.Count` (L417) and the
  `Cast<object>()` enumeration (L418-422).
- No `Store` and no `MAPIFolder` is reachable from this file. `MailItem` appears only as a parameter
  type that is forwarded, never dereferenced.
- The L415 chain is **fully mockable**: `IApplicationGlobals.Ol` is `IOlObjects` (an interface,
  `UtilitiesCS\Interfaces\IGlobals\IApplicationGlobals.cs` L11), and
  `Microsoft.Office.Interop.Outlook.Application`, `Explorer`, `Selection` and `MailItem` are all
  interop **interfaces** that Moq can proxy. Proven in-repo:
  - `QuickFiler.Test\Controllers\QfcHomeControllerTests.cs` L44-47 — `Mock<Outlook.Application>`,
    `Mock<Explorer>`, `SetupGet(x => x.Ol.App)` recursive chain, in this very test project.
  - `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.TrainSelection.cs`
    L59-90 — the complete `globals -> Ol -> App -> ActiveExplorer -> Selection` chain including
    `mockSelection.As<IEnumerable>().Setup(s => s.GetEnumerator())`, driving a
    `Cast<object>().Where(x => x is MailItem)` pipeline that is structurally identical to L418-422.
  - `UtilitiesCS.Test\EmailIntelligence\SpamBayes_Tests.cs` L430-437 — `selection.Setup(x => x.GetEnumerator())`.
- **Conclusion: `LoadSelection` is fully testable with no production change.** The CLAUDE.md § UT2
  exemption clause "without an injectable seam" does not apply, and the epic's
  refactor-first/exempt-only-the-irreducible reconciliation therefore forbids exempting it.

## 6. Cross-child contract (F9 boundary)

### (a) What `EfcFormController.cs` / `EfcItemController.cs` consume from this file

**Verified by grep: nothing.** A repository-wide search for
`EfcHomeControllerDependencies|Production[A-Za-z]*Factory|Production[A-Za-z]*Constructor|Production[A-Za-z]*Initializer`
across all `*.cs` returned 13 files; neither `QuickFiler\Controllers\EfcFormController.cs` nor
`QuickFiler\Controllers\EfcItemController.cs` is among them. A targeted grep inside
`EfcFormController.cs` for `EfcHomeControllerDependencies` returned no matches.

The dependency runs in the **opposite** direction: this file consumes F9's surface.

| F9 member consumed | F9 location | Consumed from |
| --- | --- | --- |
| `EfcFormController` (type) | `EfcFormController.cs` L28 | delegate decls L15, L25; properties L113, L115, L117-121; methods L251, L273, L312, L332, L366, L378 |
| `EfcFormController(IApplicationGlobals, EfcDataModel, EfcViewer, EfcHomeController, Action, QfEnums.InitTypeEnum, CancellationToken)` | `EfcFormController.cs` L32-52 | the shape of `FormControllerWithDataFactoryDelegate` (L15-23); actual `new` is in the Factories partial |
| `EfcFormController(IApplicationGlobals, EfcViewer, EfcHomeController, Action, QfEnums.InitTypeEnum, CancellationToken)` | `EfcFormController.cs` L53-77 | the shape of `FormControllerWithoutDataFactoryDelegate` (L25-32) |
| `EfcViewer` (type) | `Viewers\EfcViewer.cs` | delegate decls L17, L27; property L98; methods L189, L253, L275, L314, L334 |

Both F9 controllers are currently marked `[ExcludeFromCodeCoverage]`
(`EfcFormController.cs` L27, `EfcItemController.cs` L25).

### (b) Additive-only verdict

| Proposed change | Requires an F9 edit? | Verdict |
| --- | --- | --- |
| G-D1 through G-D8 (all eight) | **No** | **ADDITIVE — test-only.** No production line in `EfcHomeControllerDependencies.cs` changes; no signature, property, delegate type, or interface changes. F9 needs no edit. |

There is **no** proposed new constructor overload, no new property, no new interface, and no
signature change for this file. The additive-only constraint is satisfied trivially because the
change set is empty on the production side.

### (c) Cross-child contract notes

- **CCN-A (informational).** G-D2/G-D3/G-D4 construct `Mock<MailItem>` objects that are placed into a
  mocked `Selection`. This touches no F9 file and no F9 type. No note required for `spec.md` beyond
  recording that `LoadSelection` was closed without a contract change.
- No blocking cross-child item arises from this file. The one genuine blocking-adjacent item in F8's
  pair lives in `EfcHomeControllerDependencyFactories.cs`; see **CCN-1** in that artifact.

## 7. Line-count risk (500-line ceiling)

- Current: **428 lines**. Ceiling: 500. Headroom: **72 lines**.
- The proposed change set adds **zero** production lines, so the file remains at 428. **No 500-line
  risk is introduced by this child's work on this file.**
- Contingency, if a future increment genuinely needs a new seam here: do **not** append to
  `EfcHomeControllerDependencies.cs`. Create a new F8-owned partial file
  `QuickFiler\Controllers\EfcHomeControllerDependencies.Selection.cs` for selection/COM-adjacent
  seams. Do not create a partial file for a type owned by a sibling child.

## 8. Test-suite hazards to respect

- **Process-global mutable statics.** G-D6 and G-D7 mutate
  `EfcHomeControllerDependencies.Production*` statics (declared in the Factories partial). The CLI
  runsettings `scripts\vscode\TaskMaster.cli.runsettings` L4-7 sets `<Scope>ClassLevel</Scope>` with
  `<Workers>0</Workers>`, so **test classes run in parallel**. Any new class that mutates those
  statics must carry `[DoNotParallelize]` and a `[TestCleanup]` calling
  `EfcHomeControllerDependencies.ResetProductionFactoriesForTesting()`. In-repo precedent for
  exactly this pattern: `QuickFiler.Test\Helper Classes\ViewerQueueStaticWrapperTests.cs` L11.
- The existing class `EfcHomeControllerDependenciesTestsProductionFactory` mutates the same statics
  and is **not** marked `[DoNotParallelize]` today. Adding a second static-mutating class turns a
  latent hazard into a live one. Adding `[DoNotParallelize]` to the existing class is a test-file
  change inside F8's scope and is recommended alongside the new tests.
- Never invoke the default `MetricsLineWriter` (`FileIO2.WriteTextFile`, L78) — it writes to disk.
  Assert delegate identity only.

## 9. Do not duplicate — scenarios already covered

Do **not** re-author any of the following; they are fully covered today:

1. Constructor with no overrides installs non-null defaults for all eleven delegates —
   `EfcHomeControllerDependenciesTests.Constructor_WithNoOverrides_InstallsProductionDefaults`.
2. Constructor with all eleven overrides preserves the injected delegate instances —
   `EfcHomeControllerDependenciesTests.Constructor_WithOverrides_PreservesInjectedDelegates`.
3. `LoadSelection` with an explicit non-null `mail` returns exactly that item and does not touch
   Outlook — covered **twice** (`EfcHomeControllerDependenciesTests.LoadSelection_WithExplicitMail_ReturnsOnlyExplicitMail`
   and `EfcHomeControllerDependenciesTestsProductionFactory.LoadSelection_WithExplicitMail_DoesNotTraverseOutlookSelection`).
   These two are already redundant with each other; do not add a third.
4. Happy-path argument forwarding for all six `...WithFactory` helpers.
5. **Every** `ArgumentNullException` guard in all six `...WithFactory` helpers — all 23 guard
   branches (`globals`, `tokenSource`, `viewer`, `homeController`, `dataModel`, `cleanup`,
   `controller`, and `factory` on each helper) are already asserted, with `ParamName` verified,
   across `EfcHomeControllerDependenciesTests` and
   `EfcHomeControllerDependenciesTestsProductionFactory.WithFactoryHelpers_ValidateFactoryArguments`.
   **No new null-argument test is needed for anything except `LoadSelection`'s `globals` guard.**
6. Default private adapters routing to the `Production*` statics when the statics are set **before**
   construction — `EfcHomeControllerDependenciesTestsProductionFactory.Constructor_WithNoOverrides_UsesResettableProductionFactories`
   and `...ConstructorDefaults_InvokeProductionConstructionAdapters`.
7. Injected-delegate ordering through the `EfcHomeController` lifecycle —
   `EfcHomeControllerSeamTests` (four tests) and `EfcHomeControllerLifecycleTests`.

## 10. Upstream dependency on F1

- The **ratified-exempt vs testable** classification for this file is owned by F1's ledger at
  `docs\features\epics\quickfiler-per-file-coverage\coverage-ledger.md`. That file does not exist on
  disk yet and was not read. Based on § 5, this file is expected to be classified **testable** (no
  seam-free COM dependency remains after G-D2), and it carries no `[ExcludeFromCodeCoverage]`
  attribute today, so it is already in the denominator.
- The numeric per-file coverage figure must be produced by **F1's per-file coverage harness**
  (derived from the Cobertura output of `Invoke-MSTestWithCoverage.ps1`, per epic.md § "Per-file
  coverage measurement") and committed under
  `docs\features\active\2026-08-07-quickfiler-efc-home-controller-coverage-437\evidence\qa-gates\`.
  The ~90-93% figure in § 2 is a static estimate and is **not** acceptable as evidence.

## 11. Test strategy summary

- Framework: MSTest, Moq, FluentAssertions (per CLAUDE.md § CUT1/CUT2).
- New file: `QuickFiler.Test\Controllers\EfcHomeControllerDependenciesSelectionTests.cs`,
  `[TestClass]`, `[DoNotParallelize]`, `[TestCleanup] ResetProductionFactoriesForTesting()`.
- Eight tests (G-D1 … G-D8) plus the asymmetry test named in § 4 item 4.
- Arrange-Act-Assert throughout; no temporary files, no external services, no live forms, no popups,
  no `Thread.Sleep`/`Task.Delay`, no real Outlook object is ever created.
- Seam hierarchy is respected without any production change: the existing `IApplicationGlobals` /
  `IOlObjects` **interface seam** is sufficient for every remaining gap.
