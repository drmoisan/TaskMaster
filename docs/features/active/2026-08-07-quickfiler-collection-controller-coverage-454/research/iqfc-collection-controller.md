# Research: `QuickFiler/Interfaces/IQfcCollectionController.cs`

- Epic: `quickfiler-per-file-coverage` (parent issue #136), child F11 (issue #454)
- Feature folder: `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/`
- Target file: `QuickFiler/Interfaces/IQfcCollectionController.cs`
- Companion file in the same child: `QuickFiler/Controllers/QfcCollectionController.cs` (2,349 lines)
- Research date: 2026-08-07
- Scope: research only. No production or test file was modified.

All paths in this document are repo-relative. Line citations are against the worktree state at
branch `TaskMaster-wt-2026-08-07T20-23` (merge base `74be1964`).

---

## A. Coverage classification

### A.1 Verified file contents

`QuickFiler/Interfaces/IQfcCollectionController.cs` is 118 lines
(`QuickFiler/Interfaces/IQfcCollectionController.cs:118` is the final `}`; a `\r$` count over the
file returns 118, confirming both the line count stated in `docs/features/epics/quickfiler-per-file-coverage/epic.md:404`
and CRLF line endings).

Its entire content decomposes as:

| Lines | Content | Emits IL? |
| --- | --- | --- |
| 1-10 | Ten `using` directives | No |
| 11 | Blank | No |
| 12-13 | `namespace QuickFiler.Interfaces` + `{` | No |
| 14-15 | `public interface IQfcCollectionController` + `{` | No |
| 16-52 | Comment banners and 16 abstract member declarations, each terminated by `;` | No |
| 53-65 | XML doc comment block for `RemoveBelowThresholdAsync` | No |
| 66-116 | 27 further abstract member declarations, each terminated by `;` | No |
| 117-118 | Closing braces | No |

Every declared member is terminated by `;` with no body. Verified negatives:

- **No default interface implementations.** No member has a `{ ... }` or `=>` body
  (`QuickFiler/Interfaces/IQfcCollectionController.cs:17-116`, all `;`-terminated).
- **No `class`, `struct`, `enum`, `record`, or `delegate` declaration.** The file contains exactly one
  type declaration, the interface at `QuickFiler/Interfaces/IQfcCollectionController.cs:14`.
- **No static members, no constants, no field initializers, no extension methods.** C# forbids all of
  these on an interface prior to C# 8 static-abstract/static members, and none appear.
- **No attribute usages of any kind.** There is no `[...]` token anywhere in the file.
- **No `[ExcludeFromCodeCoverage]`.** Explicitly reported as ABSENT. The file does not even import
  `System.Diagnostics.CodeAnalysis` (`QuickFiler/Interfaces/IQfcCollectionController.cs:1-10`). This
  satisfies the epic's requirement that bucket-3 files never carry the attribute
  (`docs/features/epics/quickfiler-per-file-coverage/epic.md:519-522`).

### A.2 Language version and framework — default interface implementations are impossible here

`QuickFiler/QuickFiler.csproj:13` sets `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` and
`QuickFiler/QuickFiler.csproj:14` sets `<LangVersion>preview</LangVersion>`.

`LangVersion=preview` alone would permit the C# 8+ default-interface-implementation *syntax*, but
default interface members require runtime support that .NET Framework 4.8.1 does not provide; the
compiler rejects them on a `v4.8.1` target. The combination therefore makes it structurally
impossible for this file — or any interface file in `QuickFiler` — to acquire executable IL through a
default implementation while the project targets .NET Framework. This is a durable property of the
file, not a snapshot of its current contents.

### A.3 Empirical confirmation from committed Cobertura

The most recent committed QuickFiler-wide Cobertura report is
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
(the report the epic cites at `docs/features/epics/quickfiler-per-file-coverage/epic.md:138-140`).

- **Positive control — the `Interfaces\` folder IS instrumented.** Exactly one `<class>` element in
  the entire report carries a `filename` under `QuickFiler\Interfaces\`:
  `coverage-final.cobertura.xml:14448`, `name="QuickFiler.Interfaces.MailItemActionsAdapter"
  filename="QuickFiler\Interfaces\MailItemActionsAdapter.cs"` with `line-rate="1"`. A concrete class
  in that folder is measured, so the folder is not excluded by path.
- **`IQfcCollectionController.cs` emits no `<class>` element.** The string
  `QfcCollectionController` appears 8 times in the report, and every occurrence is inside a
  `signature="..."` attribute of a *method belonging to another class* — the parameter type
  `QuickFiler.Interfaces.IQfcCollectionController` (for example
  `coverage-final.cobertura.xml:22981` on `QfcItemController..ctor`, and
  `coverage-final.cobertura.xml:27262` on `QfcQueue.LoadControllersViewersAsync`). Zero occurrences
  are a `name=` or `filename=` attribute.

This is the same three-way evidence pattern F7 used and matches F7's finding that interface-only
`.cs` files emit no Cobertura `<class>` entry at all.

> Note for completeness: `QuickFiler/Controllers/QfcCollectionController.cs` is also absent from that
> report, but for a different reason — `[ExcludeFromCodeCoverage]` at
> `QuickFiler/Controllers/QfcCollectionController.cs:21` removes it from instrumentation. Absence of
> the implementation file is an exemption artifact; absence of the interface file is a zero-IL
> artifact. F11 must not conflate the two.

### A.4 Conclusion (unambiguous)

`QuickFiler/Interfaces/IQfcCollectionController.cs` belongs in the **third ledger bucket:
`interface-only / not-measured`**.

It has zero coverable lines. It must be reported **N/A**, never 0%, must never count as a coverage
failure, and must **not** receive `[ExcludeFromCodeCoverage]`
(`docs/features/epics/quickfiler-per-file-coverage/epic.md:509-522`). Per
`.claude/rules/general-unit-test.md:29`, a C# interface-only file with no executable behavior may
legitimately be omitted from coverage measurement, and this is a clarification rather than a
threshold reduction; per `.claude/rules/general-unit-test.md:33` no production file may be *excluded*
via an `exclude` entry, and none is proposed here — the file simply produces no denominator.

**Shape-assertion tests for this file are PROHIBITED.** No test may be written that reflects over
`typeof(IQfcCollectionController)` for the purpose of manufacturing coverage
(`docs/features/epics/quickfiler-per-file-coverage/epic.md:521-522`). Tests that *mock* the interface
in service of covering a consumer are a different thing and remain correct and expected (see §B.3).

### A.5 Ledger row

Proposed row, matching the three-bucket vocabulary in
`docs/features/epics/quickfiler-per-file-coverage/epic.md:509-522`:

| file | bucket | line % | branch % | rationale | owning child |
| --- | --- | --- | --- | --- | --- |
| `QuickFiler/Interfaces/IQfcCollectionController.cs` | `interface-only / not-measured` | N/A | N/A | Single `public interface` declaration, 43 abstract members, zero method bodies. Emits no IL and no Cobertura `<class>` element; positive control `Interfaces\MailItemActionsAdapter.cs` proves the folder is instrumented. Default interface implementations are impossible on the `v4.8.1` target. No `[ExcludeFromCodeCoverage]` present and none to be added. | F11 (#454) |

See "Documented Deviations" D-1 regarding which child actually appends this row.

---

## B. Contract surface and consumers

### B.1 Declared members and their implementations

All 43 members. `IF:n` = line in `QuickFiler/Interfaces/IQfcCollectionController.cs`;
`Impl:n` = line in `QuickFiler/Controllers/QfcCollectionController.cs`.

| # | Member | IF | Impl | Kind |
| --- | --- | --- | --- | --- |
| 1 | `List<QfcItemGroup> ItemGroups { get; set; }` | 17 | 241 | Property (get+set, both `[MethodImpl(Synchronized)]` at Impl:243/245) |
| 2 | `Task LoadSecondaryAsync()` | 20 | 525 | async |
| 3 | `void LoadControlsAndHandlers_01(IList<MailItem>, RowStyle, RowStyle)` | 21 | 268 | sync |
| 4 | `void LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)` | 26 | 253 | sync |
| 5 | `Task LoadControlsAndHandlers_01Async(IList<MailItem>, RowStyle, RowStyle)` | 27 | 341 | async |
| 6 | `Task LoadControlsAndHandlers_01Async(IList<QfcPreScoredItem>, RowStyle, RowStyle)` | 32 | 427 | async |
| 7 | `ItemViewer LoadItemViewer_03(int, RowStyle, bool = true, int = 0)` | 37 | 951 | sync, 2 optional params |
| 8 | `void PopOutControlGroup(int)` | 43 | 964 | sync |
| 9 | `Task PopOutControlGroupAsync(int)` | 44 | 976 | async |
| 10 | `void RemoveControls()` | 45 | 991 | sync |
| 11 | `Task RemoveControlsAsync()` | 46 | 1024 | async |
| 12 | `void EliminateSpaceForItems(int, int)` | 47 | 2013 | sync |
| 13 | `void RemoveSpecificControlGroup(int)` | 48 | 1105 | sync |
| 14 | `Task RemoveSpecificControlGroupAsync(int)` | 49 | 1159 | async |
| 15 | `Task MoveEmailsAsync(SloStack<IMovedMailInfo>)` | 50 | 2206 | async |
| 16 | `void AddItemGroup(MailItem)` | 51 | 1924 | sync |
| 17 | `Task RemoveBelowThresholdAsync(double)` | 66 | 1077 | async |
| 18 | `int ActivateBySelection(int, bool)` | 69 | 1401 | sync |
| 19 | `void ChangeByIndex(int)` | 70 | 1450 | sync |
| 20 | `void SelectNextItem()` | 71 | 1486 | sync |
| 21 | `void SelectPreviousItem()` | 72 | 1503 | sync |
| 22 | `void ToggleOffNavigation(bool)` | 73 | 1600 | sync |
| 23 | `void ToggleOnNavigation(bool)` | 74 | 1634 | sync |
| 24 | `void ToggleExpansionStyle(int, Enums.ToggleState)` | 75 | 1543 | sync |
| 25 | `Task ToggleExpansionStyleAsync(int, Enums.ToggleState)` | 76 | 1591 | async |
| 26 | `void ToggleGroupConv(int, int)` | 79 | 1768 | sync |
| 27 | `void ToggleGroupConv(string)` | 80 | 1733 | sync |
| 28 | `void ToggleUnGroupConv(ConversationResolver, string, int, object)` | 81 | 1808 | sync |
| 29 | `void MakeSpaceForItems(int, int)` | 87 | 2029 | sync |
| 30 | `void SetDarkMode(bool)` | 90 | 2158 | sync |
| 31 | `void SetLightMode(bool)` | 91 | 2166 | sync |
| 32 | `int EmailsLoaded { get; }` | 94 | 148 | Property (expression-bodied getter) |
| 33 | `int EmailsToMove { get; }` | 95 | 150 | Property (expression-bodied getter) |
| 34 | `bool ReadyForMove { get; }` | 96 | 152 | Property (getter with body) |
| 35 | `void ResetPanelHeight()` | 97 | 2092 | sync |
| 36 | `void UnregisterNavigation()` | 100 | 1343 | sync |
| 37 | `void RegisterNavigation()` | 101 | 1330 | sync |
| 38 | `Task ToggleOffNavigationAsync()` | 102 | 1615 | async |
| 39 | `Task ToggleOnNavigationAsync()` | 103 | 1648 | async |
| 40 | `void CacheMoveObjects()` | 104 | 898 | sync |
| 41 | `void CleanupBackground()` | 105 | 1013 | sync |
| 42 | `void Cleanup()` | 107 | 2192 | sync |
| 43 | `string[] GetMoveDiagnostics(string, string, double, string, DateTime, ref AppointmentItem)` | 109 | 2272 | sync, **`ref` parameter** |

The single implementing type is
`public class QfcCollectionController : IQfcCollectionController`
(`QuickFiler/Controllers/QfcCollectionController.cs:22`), carrying `[ExcludeFromCodeCoverage]` at
`QuickFiler/Controllers/QfcCollectionController.cs:21`. There is no other implementer anywhere in the
repository.

### B.2 Production consumers

Repository-wide grep for both `IQfcCollectionController` and `QfcCollectionController` over `**/*.cs`.
Non-compiled locations (`QuickFiler/Legacy/**`, `QuickFiler/Notes/**` — neither appears as a
`<Compile Include>` in `QuickFiler/QuickFiler.csproj`, verified by a zero-match grep for `Notes\` and
`Legacy\` in that file) are listed separately and are outside the coverage denominator.

**F6 — `QfcFormController` family** (owns the concrete construction of the type):

| Location | Members used |
| --- | --- |
| `QuickFiler/Controllers/QfcFormController.cs:157-158` | Field `_groups` and public property `Groups` (declaration) |
| `QuickFiler/Controllers/QfcFormController.cs:174` | `ToggleOffNavigation(bool)` |
| `QuickFiler/Controllers/QfcFormController.cs:176` | `ToggleOffNavigationAsync()` |
| `QuickFiler/Controllers/QfcFormController.cs:178` | `ToggleOnNavigation(bool)` |
| `QuickFiler/Controllers/QfcFormController.cs:180` | `ToggleOnNavigationAsync()` |
| `QuickFiler/Controllers/QfcFormController.Actions.cs:29` | `LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)` |
| `QuickFiler/Controllers/QfcFormController.Actions.cs:49-58` | `new QfcCollectionController(...)` — direct concrete construction, 8 args |
| `QuickFiler/Controllers/QfcFormController.Actions.cs:59` | `LoadControlsAndHandlers_01(IList<MailItem>, RowStyle, RowStyle)` |
| `QuickFiler/Controllers/QfcFormController.Actions.cs:83-92` | `new QfcCollectionController(...)` |
| `QuickFiler/Controllers/QfcFormController.Actions.cs:93` | `LoadControlsAndHandlers_01Async(IList<MailItem>, ...)` |
| `QuickFiler/Controllers/QfcFormController.Actions.cs:104` | `LoadSecondaryAsync()` |
| `QuickFiler/Controllers/QfcFormController.Actions.cs:139-148` | `new QfcCollectionController(...)` |
| `QuickFiler/Controllers/QfcFormController.Actions.cs:149` | `LoadControlsAndHandlers_01Async(IList<QfcPreScoredItem>, ...)` |
| `QuickFiler/Controllers/QfcFormController.Actions.cs:160` | `LoadSecondaryAsync()` |
| `QuickFiler/Controllers/QfcFormController.Actions.cs:171,180` | `internal ApplyHighConfidenceFilterAsync(IQfcCollectionController)` → `RemoveBelowThresholdAsync(double)` |
| `QuickFiler/Controllers/QfcFormController.Actions.cs:275` | `AddItemGroup(MailItem)` |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:92,374` | `Cleanup()`, `CleanupBackground()` |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:121` | `ReadyForMove` |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:156,190` | `CacheMoveObjects()` |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225` | `MoveEmailsAsync(...)` |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:233` | `CleanupBackground()` |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:265,271` | `UnregisterNavigation()`, `RegisterNavigation()` |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:267` | `ItemGroups` (read only) |
| `QuickFiler/Controllers/IQfcFormController.cs:18` | Declares `IQfcCollectionController Groups { get; }` |
| `QuickFiler/Interfaces/IQfcFormController.cs:23` | Declares `IQfcCollectionController Groups { get; }` (second, separate interface of the same simple name) |

`QuickFiler/Controllers/QfcExplorerController.cs` — also F6 — contains **no** reference to either type
(zero grep matches).

**F10 — `QfcItemController` family** (holds the type as `Parent`):

| Location | Members used |
| --- | --- |
| `QuickFiler/Controllers/QfcItemController.cs:44,187` | Field `_parent` / public property `Parent` (declaration) |
| `QuickFiler/Interfaces/IQfcItemController.cs:59` | Declares `IQfcCollectionController Parent { get; }` |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:32,89,114,142,349,407,440` | Constructor/initializer parameter only; no member invoked |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs:195` | `PopOutControlGroup(int)` |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs:200` | `RemoveSpecificControlGroup(int)` |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs:282` | `PopOutControlGroupAsync` (method group) |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs:288` | `RemoveSpecificControlGroupAsync` (method group) |
| `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:70` | `PopOutControlGroupAsync(int)` |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs:176,194` | `ToggleExpansionStyle`, `ToggleExpansionStyleAsync` |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs:33` | `ToggleGroupConv(string)` |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs:41` | `ToggleUnGroupConv(...)` |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs:59,77` | `PopOutControlGroup(int)`, `PopOutControlGroupAsync(int)` |

**F7 — `QfcHomeController` family** (see §B.4 for the verdict):

| Location | Members used |
| --- | --- |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs:46,125` | `EmailsToMove` |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs:75-82,144` | `GetMoveDiagnostics(..., ref olAppointment)` |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs:29` | Passes `_formController.Groups` as an argument to `QfcQueue.EnqueueAsync`; invokes no member |

**F2 — queue/admission**:

| Location | Members used |
| --- | --- |
| `QuickFiler/Controllers/IQfcQueue.cs:26` | `Task EnqueueAsync(IList<MailItem>, IQfcCollectionController)` — parameter type only |
| `QuickFiler/Controllers/IQfcQueue1.cs:29` | Same signature — parameter type only |
| `QuickFiler/Controllers/QfcQueue.cs:34,213,227` | Field `_qfcCollectionController`, ctor/`EnqueueAsync` parameter, assignment |
| `QuickFiler/Controllers/QfcQueue.cs:384,408,487` | Passed as `parent:` to `new QfcItemController(...)`; **no member invoked** |
| `QuickFiler/Controllers/QfcItemGroup.cs:12` | `using static QuickFiler.Controllers.QfcCollectionController;` — a static-import of the concrete class |

**F8 — EFC home controller** (cross-child dependency on the *concrete* type):

| Location | Member used |
| --- | --- |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs:79` | `QfcCollectionController.xComma(...)` — the `public static string xComma(string)` at `QuickFiler/Controllers/QfcCollectionController.cs:2330` |

This is a hard constraint on F11's partial split: `xComma` must remain a `public static` member of the
`QfcCollectionController` type (any partial file is acceptable) or F8's file stops compiling. It is
not on the interface and must not be moved to a different type without coordinating with F8.

**Not compiled (outside the denominator, listed for completeness):**
`QuickFiler/Notes/notes_interfaces.cs:7,62` declares a second, unrelated `IQfcCollectionController`;
`QuickFiler/Legacy/QuickFileController.cs:422,428,676,695,1024,1026,1037` calls same-named members on
a different legacy type.

### B.3 Test consumers

| File | Usage |
| --- | --- |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:36-37,147-148,254-255,343-344` | Instantiates the concrete type via `FormatterServices.GetUninitializedObject` to bypass the WinForms-dependent constructor, then injects `_itemGroups` / `_itemGroupsToMove` via reflection (`:69,167,178,262,381,496`) |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs:50-59` | Constructs via the **real** constructor with all eight collaborators mocked (`IApplicationGlobals`, `IQfcFormViewer`, `IFilerHomeController`, `IFilerFormController`) plus a real `TlpCellStates`; exercises `Cleanup()` (`:107`) and `CleanupAsync()` (`:136`) |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:111-126,197,261,288` | `Mock<IQfcCollectionController>(MockBehavior.Loose)`; sets `EmailsToMove` and `GetMoveDiagnostics` (including the `ref` parameter) |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs:94,117,142,151-152,178,216,225-226,252,277,285-286,302` | `Mock<IQfcCollectionController>` supplied through `mockFormController.Setup(m => m.Groups)` and matched via `It.IsAny<IQfcCollectionController>()` |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs:509-523,725,755,774,809` | `Mock<IQfcCollectionController>` (both Loose and `MockBehavior.Strict` at `:809`); verifies `UnregisterNavigation()` and `RegisterNavigation()`; `:790-792` documents that `LoadItemsAsync` "constructs a real `QfcCollectionController` internally (no DI seam at ...)" |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs:50,83,118,151` | `Mock<IQfcCollectionController>` as the `parent` argument |
| `QuickFiler.Test/Controllers/QfcItemController.SeamCoreTests.cs:86,98,106` | `Mock<IQfcCollectionController>`; verifies `ToggleGroupConv("seam-entry")` |
| `QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs:160` | `Mock<IQfcCollectionController>` |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs:143,153,164` | `Mock<IQfcCollectionController>`; verifies `ToggleGroupConv("origin-123")` |
| `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:353` | A hand-rolled fake exposing `IQfcCollectionController Parent { get; private set; }` |

Both existing `QfcCollectionController*` test files are compiled:
`QuickFiler.Test/QuickFiler.Test.csproj:112-113`.

Consequence for F11's baseline: the implementation already has real tests, but because of
`[ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcCollectionController.cs:21` none of that
exercise is measured. F11's first measurement after removing the attribute will therefore be
non-zero, not zero. Do not plan on the assumption that the file is untested.

### B.4 Does F7's "no contract additions needed" conclusion hold?

**Yes, it holds.** F7's three production call sites resolve entirely against members already declared
on the interface:

- `EmailsToMove` — `QuickFiler/Controllers/QfcHomeController.Metrics.cs:46,125` against
  `QuickFiler/Interfaces/IQfcCollectionController.cs:95`.
- `GetMoveDiagnostics(..., ref olAppointment)` — `QuickFiler/Controllers/QfcHomeController.Metrics.cs:75-82`
  and `:144` against `QuickFiler/Interfaces/IQfcCollectionController.cs:109-116`.
- `QuickFiler/Controllers/QfcHomeController.Iteration.cs:29` passes `_formController.Groups` by
  reference into `QfcQueue.EnqueueAsync`; the parameter type is `IQfcCollectionController`
  (`QuickFiler/Controllers/IQfcQueue.cs:26`) and no member is dereferenced.

F7's tests already prove all three are reachable through a Moq double
(`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:111-126`). F11 must therefore treat
members 33 (`EmailsToMove`) and 43 (`GetMoveDiagnostics`) as frozen: no signature change, no
rename, no removal, and in particular no removal of the `ref AppointmentItem` parameter, which is
load-bearing for F7's existing setup expression.

---

## C. Interface-level seam implications

### C.1 Mock-friendliness of the interface today

The interface is already the primary seam for every consumer, and it is mockable with Moq without any
change. Verified properties:

- **All 43 members are implicitly `public abstract`** by virtue of being interface members, so every
  one is virtual by construction and interceptable by Castle DynamicProxy.
- **No `out` parameters.** None appear in
  `QuickFiler/Interfaces/IQfcCollectionController.cs:17-116`.
- **Exactly one `ref` parameter**: `ref AppointmentItem OlAppointment` on `GetMoveDiagnostics`
  (`QuickFiler/Interfaces/IQfcCollectionController.cs:115`). Moq supports this, and the repository
  already has a working precedent: `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:110`
  declares `AppointmentItem refAppointment = null;` and passes `ref refAppointment` inside the setup
  expression at `:121`. Note the semantic: a captured-variable `ref` setup matches only when the
  runtime by-ref value equals the captured value at setup time; `It.Ref<AppointmentItem>.IsAny` is
  the general-purpose form if a plan needs to match any value.
- **No generic methods, no events, no indexers, no `in` parameters, no `params` arrays.**
- **Two optional parameters** on `LoadItemViewer_03` (`bool blGroupConversation = true`,
  `int columnNumber = 0`, `QuickFiler/Interfaces/IQfcCollectionController.cs:40-41`). Moq handles
  these; setup expressions must supply all four arguments explicitly.
- **Two concrete-type leaks** that constrain what a mock can usefully return or receive:
  - `LoadItemViewer_03` returns the concrete `ItemViewer`
    (`QuickFiler/Interfaces/IQfcCollectionController.cs:37`), which is
    `public partial class ItemViewer : UserControl, IItemViewer, IContainerControlLocal`
    (`QuickFiler/Viewers/ItemViewer.cs:21`) — a WinForms control, not the `IItemViewer` abstraction
    that exists alongside it. A loose mock returns `null` harmlessly; a test needing a non-null return
    must construct a real `UserControl`.
  - `ToggleUnGroupConv` takes the concrete `ConversationResolver`
    (`QuickFiler/Interfaces/IQfcCollectionController.cs:82`) despite
    `QuickFiler/Helper Classes/IConversationResolver.cs` existing.

  Neither leak blocks mocking the interface. Both would be contract changes to fix, and fixing them
  is out of scope for F11 (F14 owns `ItemViewer`, F4 owns `ConversationResolver`).

- **Argument types requiring a reference from the test assembly**: `QfcPreScoredItem` is
  `public readonly struct QfcPreScoredItem` at
  `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:98` — public, so reachable.

### C.2 Public implementation members NOT declared on the interface

The class exposes substantially more than the interface declares. A seam design may legitimately want
some of these; each would be a contract addition if promoted. Full list, from
`QuickFiler/Controllers/QfcCollectionController.cs`:

| Member | Line | Note |
| --- | --- | --- |
| `int ActiveIndex { get; set; }` | 87 | |
| `int ActiveSelection { get; set; }` | 92 | |
| `CancellationToken Token` | 99 | |
| `CancellationTokenSource TokenSource` | 106 | |
| `bool TlpLayout` | 197 | both accessors `[MethodImpl(Synchronized)]` (199, 201) |
| `bool SafeSetTlpLayout(bool)` | 233 | |
| `Task<MailItemHelper> GetPartiallyInitializedHelperAsync(MailItem)` | 298 | |
| `void CreateEmptyKbdHandlerCharActions()` | 581 | |
| `Task LoadGroups_02cAsync(...)` | 587 | |
| `Task LoadGroups_02bAsync(...)` | 635 | |
| `void LoadItemGroupsAndViewers_02(IList<MailItem>, RowStyle)` | 740 | |
| `void LoadConversationsAndFolders_04()` | 756 | |
| `Task LoadConversationsAndFoldersAsync()` | 761 | |
| `void LoadSequential_5()` | 798 | |
| `Task LoadSequentialAsync()` | 827 | |
| `Task LoadGroupSequential(int, QfcItemGroup)` | 842 | |
| `void LoadItemToTlp(...)` | 904 | |
| `void WireUpKeyboardHandler()` | 1254 | |
| `void WireUpAsyncKeyboardHandler()` | 1275 | |
| `int ActivateByIndex(int, bool)` | 1391 | |
| `Task<int> ActivateByIndexAsync(int, bool)` | 1396 | |
| `Task<int> ActivateBySelectionAsync(int, bool)` | 1426 | async twin of interface member 18 |
| `Task ChangeByIndexAsync(int)` | 1466 | async twin of member 19 |
| `Task SelectNextItemAsync()` | 1498 | async twin of member 20 |
| `Task SelectPreviousItemAsync()` | 1516 | async twin of member 21 |
| `bool ToggleOffActiveItem(bool)` | 1667 | |
| `Task<bool> ToggleOffActiveItemAsync(bool)` | 1687 | |
| `void ChangeConversationSilently(int, bool)` | 1714 | |
| `void ChangeConversationSilently(QfcItemGroup, bool)` | 1725 | |
| `void EnumerateConversationMembers(...)` | 1875 | |
| `int PromoteFirstChild(string, ref int)` | 1970 | |
| `void InsertItemGroups(int, int)` | 2004 | |
| `void UpdateSelectionNumberForRemoval(int)` | 2044 | |
| `void RenumberGroups()` | 2064 | |
| `void RenumberGroups(int)` | 2072 | |
| `Task ResetPanelHeightAsync()` | 2080 | async twin of member 35 |
| `void SetupLightDark(bool)` | 2113 | called from the constructor (`:52`) |
| `void DarkMode_CheckedChanged(object, EventArgs)` | 2120 | `PropertyChanged` handler |
| **`Task CleanupAsync()`** | **2178** | **declared `async public`**; the interface declares only `void Cleanup()` at IF:107 |
| `static string xComma(string)` | 2330 | consumed by F8 at `QuickFiler/Controllers/EfcHomeController.Metrics.cs:79` |

`CleanupAsync()` is the single most notable omission: it is public, it is exercised by an existing
test (`QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs:136`), it has an issue-#251
regression contract, and it is invisible through `IQfcCollectionController`. F6's only cleanup call
site uses the synchronous form (`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:92`), so
nothing is currently broken — but any future async-cleanup path would need an interface addition.
F11 should *not* add it speculatively.

The class also carries a substantial `internal` surface already reachable from tests (see §D):
`BackgroundLoadingTasks` (:80), `Digits` (:114), `EncapsulateItemGroup` (:607), `LoadItemGroup` (:776),
`ActivateQueuedTlp` (:859), `CacheTlpForMove` (:865), `SwapTlp` (:870), `CacheItemGroupsForMove` (:876),
`ActivateQueuedItemGroups` (:883), `SwapItemGroups` (:888), `RemovedItemMonitor` (:1046),
`RemoveSpecificControlGroup(string)` (:1053), `RegisterAsyncKeyActions` (:1282),
`RegisterAlwaysOnAsyncKeyActions` (:1293), `CustomReturnKeyHandler` (:1307), `AnyOpenDropDowns` (:1319),
`AnyOpenDropDownsAsync` (:1324), `RegisterNavigationAsyncAction` (:1358),
`GenerateStringKbdAction` (:1363), `ScrollIntoView` (:1521), `InitializeGroup` (:1849),
`CaptureTlpTemplate` (:1991).

### C.3 Can the seam work be done WITHOUT touching this interface?

**Yes. F11 can and should complete its seam extraction and coverage work with zero edits to
`QuickFiler/Interfaces/IQfcCollectionController.cs`.** Basis:

1. **Every external consumer is already satisfied.** §B.2 enumerates all production call sites; every
   member they touch is already declared. No consumer requires a member the interface lacks.
2. **The constructor is the real untestable boundary, and it is not on the interface.** The eight
   constructor parameters at `QuickFiler/Controllers/QfcCollectionController.cs:30-39` are already
   abstractions (`IApplicationGlobals`, `IQfcFormViewer`, `IFilerHomeController`,
   `IFilerFormController`) plus two token types and a `TlpCellStates`. The only host-bound work in
   the constructor body is `_formViewer.L1v0L2L3v_TableLayout` / `_formViewer.L1v0L2_PanelMain`
   (`:44-45`) and `SetupLightDark(_globals.Ol.DarkMode)` (`:52`) — all of which
   `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs:50-59` already drives
   successfully with mocks. Constructor-level seams therefore need no interface change.
3. **New seams belong as `internal` members on the class.** The class already uses this pattern
   heavily (22 `internal` members listed above), and `QuickFiler.Test` has the internals grant
   (§D). Delegate-injection seams (`Func<...>` / `Action<...>` fields with production defaults) can be
   added as `internal` and set from tests, matching the precedent used elsewhere in QuickFiler.
4. **The partial split is a file-layout change, not a contract change.** Splitting 2,349 lines across
   `QfcCollectionController.cs` plus new `QfcCollectionController.*.cs` partials changes no member
   signature and requires no interface edit. The only hard constraint is that `xComma` stays a
   `public static` member of the type (§B.2, F8 row).

**No interface addition is unavoidable.** If a plan later concludes otherwise, the two candidates
most likely to be proposed are `Task CleanupAsync()` (§C.2) and an `IItemViewer`-typed replacement
for `LoadItemViewer_03`'s return. Both are cross-child contract changes — `CleanupAsync` touches F6,
and the return-type change touches F14 — and both should be deferred rather than bundled into F11.

---

## D. `InternalsVisibleTo` reality check

### D.1 `QuickFiler` → `QuickFiler.Test`: **GRANTED**

`QuickFiler/Properties/AssemblyInfo.cs:5`:

```
[assembly: InternalsVisibleTo("QuickFiler.Test")]
```

`internal` seams added to `QfcCollectionController` are therefore directly reachable from
`QuickFiler.Test`. The internal-seam strategy in §C.3 is viable; no grant needs to be added and no
fallback to public/interface-level seams is required.

### D.2 `QuickFiler` → `DynamicProxyGenAssembly2`: **GRANTED, but from a non-obvious file**

The grant is **not** in `QuickFiler/Properties/AssemblyInfo.cs`. It is at
`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11`:

```
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
```

That file is in the compile set (`QuickFiler/QuickFiler.csproj:322`), so the attribute is emitted and
Moq can proxy `internal` types and `internal` interfaces declared in `QuickFiler`.

**Risk to record.** `QfcHighConfidencePreFilter.cs` is assigned to **F2**
(`docs/features/epics/quickfiler-per-file-coverage/epic.md:331`), not to F11. If F2's refactor moves,
splits, or removes that file without preserving the assembly-level attribute, every child that relies
on mocking a `QuickFiler` internal loses that capability at fan-in — a failure that appears as a
Castle `ProxyGenerationException` at runtime, not a compile error. F11's plan should either avoid
depending on mocking `internal` QuickFiler types, or note this coupling as a cross-child risk. The
same attribute also appears at `QuickFiler/Legacy/IAcceleratorCallbacks.cs:5`, but `Legacy/` is not
compiled (zero `Legacy\` matches in `QuickFiler/QuickFiler.csproj`), so it provides no redundancy.

### D.3 `UtilitiesCS` grants — epic's stated fact **CONFIRMED**

`UtilitiesCS/Properties/AssemblyInfo.cs:18-20`:

```
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("UtilitiesCS.Test")]
[assembly: InternalsVisibleTo("ToDoModel.Test")]
```

`QuickFiler.Test` is **not** among them. The epic's statement at
`docs/features/epics/quickfiler-per-file-coverage/epic.md:619-631` is accurate. Any `UtilitiesCS`
internal remains unreachable from F11's tests, and per the epic's resolution F11 must build a local
seam in its own file assignment rather than editing `UtilitiesCS/Properties/AssemblyInfo.cs`.

### D.4 Net effect on seam strategy

The seam strategy in §C.3 stands unchanged: `internal` seams on `QfcCollectionController`, no
interface edits, no `UtilitiesCS` edits.

---

## E. csproj impact

### E.1 Project shape

`QuickFiler/QuickFiler.csproj` is a legacy non-SDK MSBuild project:
`<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">`
(`QuickFiler/QuickFiler.csproj:2`), with `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`
(`:13`) and `<LangVersion>preview</LangVersion>` (`:14`).

It uses **no globbing**. Every source file is an explicit `<Compile Include="..." />` entry, confirming
`docs/features/epics/quickfiler-per-file-coverage/epic.md:594-600`. Files present on disk but absent
from the project are not compiled: a grep for `Notes\` and `Legacy\` in the csproj returns zero
matches, which is why `QuickFiler/Notes/notes_interfaces.cs` and `QuickFiler/Legacy/**` are outside
the denominator.

### E.2 Line endings: **CRLF confirmed**

A `\r$` count over `QuickFiler/QuickFiler.csproj` returns 593 matches — i.e. every line is
CRLF-terminated. The epic's warning at
`docs/features/epics/quickfiler-per-file-coverage/epic.md:610-612` applies: use the Edit tool or
`perl -0777` with explicit `\r\n`; a git-bash `sed -i` will rewrite the whole file and guarantee a
fan-in conflict.

(The target research file itself is also CRLF: a `\r$` count over
`QuickFiler/Interfaces/IQfcCollectionController.cs` returns 118, equal to its line count.)

### E.3 Exact existing entries and their context

**Implementation entry — `QuickFiler/QuickFiler.csproj:311`**, shown with surrounding context
(:307-315):

```
    <Compile Include="Controllers\KaChar.cs" />
    <Compile Include="Controllers\KaKey.cs" />
    <Compile Include="Controllers\KaStringAsync.cs" />
    <Compile Include="Controllers\KbdActions.cs" />
    <Compile Include="Controllers\QfcCollectionController.cs" />
    <Compile Include="Controllers\EmailSorter.cs" />
    <Compile Include="Controllers\QfcDatamodel.cs" />
    <Compile Include="Controllers\QfcDatamodel.FrameBuilding.cs" />
    <Compile Include="Controllers\QfcDatamodel.QueueProcessing.cs" />
```

**Interface entry — `QuickFiler/QuickFiler.csproj:360`**, shown with surrounding context (:355-368):

```
    <Compile Include="Interfaces\IEmailMoveMonitor.cs" />
    <Compile Include="Interfaces\IFilerFormController.cs" />
    <Compile Include="Interfaces\IFilerHomeController.cs" />
    <Compile Include="Interfaces\IItemControler.cs" />
    <Compile Include="Interfaces\IKbdAction.cs" />
    <Compile Include="Interfaces\IQfcCollectionController.cs" />
    <Compile Include="Interfaces\IQfcDatamodel.cs" />
    <Compile Include="Interfaces\IQfcExplorerController.cs" />
    <Compile Include="Interfaces\IQfcFormController.cs" />
    <Compile Include="Interfaces\IQfcFormViewer.cs" />
    <Compile Include="Interfaces\IQfcItemController.cs" />
    <Compile Include="Interfaces\IQfcKeyboardHandler.cs" />
    <Compile Include="Interfaces\IMailItemActions.cs" />
    <Compile Include="Interfaces\MailItemActionsAdapter.cs" />
```

### E.4 Guidance for the planner

- **No csproj edit is required for the interface file.** F11's work on
  `IQfcCollectionController.cs` is classification only; its `<Compile Include>` entry at
  `QuickFiler/QuickFiler.csproj:360` stays exactly as-is.
- **The only csproj edit F11 needs** is inserting the new `QfcCollectionController.*.cs` partial
  entries. Insert them **immediately after line 311**, before
  `<Compile Include="Controllers\EmailSorter.cs" />`. This keeps the diff to a single contiguous
  additive hunk anchored on an F11-owned line, minimizing overlap with the concurrent hunks that F2
  (near :340-341), F3, F7 (near :325-327), and F9 (near :297-301) will each produce.
- Existing sibling entries in this file already demonstrate the naming convention F11 should follow
  (`QfcDatamodel.FrameBuilding.cs` at :314, `QfcItemController.Initialization.cs` at :329, etc.):
  `Controllers\QfcCollectionController.<Concern>.cs`.
- Per `docs/features/epics/quickfiler-per-file-coverage/epic.md:606-608`, F11 must add only
  `<Compile Include>` entries for files it owns — no property changes, no reference changes, no
  reordering of unrelated entries.
- Per `docs/features/epics/quickfiler-per-file-coverage/epic.md:583-585`, each new partial file is
  new production code and defaults to the `testable` bucket with a **>= 90%** line target, and F11
  appends a ledger row for each in the same change that adds its `<Compile Include>` entry.

---

## Documented Deviations

**D-1 — "Mid-Wave File Creation" rule 3 does not literally apply to this file.**
The prompt asks for "the ledger row this child must append per epic.md 'Mid-Wave File Creation'
rule 3." Rule 3 (`docs/features/epics/quickfiler-per-file-coverage/epic.md:580-582`) binds a child
that *adds* a production file. `QuickFiler/Interfaces/IQfcCollectionController.cs` is a pre-existing
member of the 121-file compile set (`QuickFiler/QuickFiler.csproj:360`), so its ledger row is F1's
responsibility, not an F11 append. The row supplied in §A.5 should be read as the row F11 must
**verify or reconcile** against F1's ledger, and as the row F11 would author if F1's ledger is found
to lack one or to classify the file as `testable` or `ratified-exempt`. Rule 3 *does* apply to the
new `QfcCollectionController.*.cs` partials F11 will create; those default to `testable` at >= 90%
per rule 4.

**D-2 — The implementation file already has real, compiled tests.**
The epic's baseline section states that exempted files "will appear for the first time at an unknown
coverage level, most likely near zero" and names `QfcCollectionController.cs` (F11) among them
(`docs/features/epics/quickfiler-per-file-coverage/epic.md:180-187`). That is accurate about the
report but potentially misleading about the starting point: two compiled test classes already
exercise the type — `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` and
`QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs`, both listed at
`QuickFiler.Test/QuickFiler.Test.csproj:112-113`. F11's first measurement after removing
`[ExcludeFromCodeCoverage]` should be treated as unknown-but-probably-non-zero, and must be captured
before planning test volume.

**D-3 — The `DynamicProxyGenAssembly2` grant for `QuickFiler` lives outside `AssemblyInfo.cs`.**
It is at `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11`, an F2-owned file, not at
`QuickFiler/Properties/AssemblyInfo.cs`. This is not contradicted by the epic (which only makes a
claim about `UtilitiesCS`), but it is a cross-child coupling the epic does not record. See §D.2.

**D-4 — A second, non-compiled `IQfcCollectionController` exists.**
`QuickFiler/Notes/notes_interfaces.cs:62` declares an unrelated interface of the same name in the
working tree. It is not compiled (no `Notes\` entry in `QuickFiler/QuickFiler.csproj`) and is outside
the denominator. Any grep-based tooling in F11's plan must filter it out to avoid a false consumer
count.

**Constraints verified and NOT disproved:**
- 118-line count for the target file — confirmed.
- `QuickFiler/Controllers/QfcCollectionController.cs` at 2,349 lines with `[ExcludeFromCodeCoverage]`
  at `:21` — confirmed (file ends at `:2349`).
- `QuickFiler.csproj` is non-SDK with explicit `<Compile Include>` entries and no globbing — confirmed.
- `UtilitiesCS` does not grant `InternalsVisibleTo` to `QuickFiler.Test` — confirmed.
- F7 needs no contract additions from this interface — confirmed.
