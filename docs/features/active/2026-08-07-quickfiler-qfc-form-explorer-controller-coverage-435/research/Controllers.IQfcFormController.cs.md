# Research — `QuickFiler/Controllers/IQfcFormController.cs`

Timestamp: 2026-08-07T22-40

## 1. Header

| Field | Value |
| --- | --- |
| Production file | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8220048ded06d508\QuickFiler\Controllers\IQfcFormController.cs` |
| Exact line count | 43 |
| Declared namespace | `QuickFiler.Controllers` (line 11) |
| Declared type | `public interface IQfcFormController : IFilerFormController` (line 13) |
| Base interface | `QuickFiler.Interfaces.IFilerFormController` — resolved below |
| `[ExcludeFromCodeCoverage]` | **No.** Verified by reading the entire 43-line file; the file contains no attribute of any kind. |
| Compiled | **Yes** — `QuickFiler/QuickFiler.csproj` line 303: `<Compile Include="Controllers\IQfcFormController.cs" />` |
| Feature child | F6 (issue #435), epic #136 |

Current numeric per-file line coverage is **unmeasured**. It cannot be stated without running
F1's per-file harness. No figure is invented here.

### Declared member set (verified line by line)

Properties (lines 15–22): `ActiveTheme` (`string`, get/set), `DarkMode` (`bool`, get/set),
`FormViewer` (`IQfcFormViewer`, get), `Groups` (`IQfcCollectionController`, get),
`ItemsPerIteration` (`int`, get/set), `SpaceForEmail` (`int`, get), `Token`
(`CancellationToken`, get), `TokenSource` (`CancellationTokenSource`, get).

Methods (lines 24–41): `ButtonSkip_Click(object, EventArgs)`, `ButtonUndo_Click()`,
`ButtonUndo_Click(object, EventArgs)`, `CaptureItemSettings()`, `LoadItems(IList<MailItem>)`,
`LoadItems(TableLayoutPanel, List<QfcItemGroup>)`, `LoadItemsAsync(IList<MailItem>)`,
`LoadItemsAsync(IList<MailItem>, ProgressTracker)`, `LoadItemsAsync(IList<QfcPreScoredItem>)`,
`LoadItemsAsync(IList<QfcPreScoredItem>, ProgressTracker)`, `LoadItemsPerIteration()`,
`RegisterFormEventHandlers()`, `RemoveTemplatesAndSetupTlp()`, `SetupLightDark()`,
`SkipGroupAsync()`, `SpnEmailPerLoad_ValueChanged(object, EventArgs)`,
`UnregisterFormEventHandlers()`, `Viewer_Activate()`.

Inherited from `IFilerFormController`: `ActionCancelAsync()`, `ActionOkAsync()`,
`ButtonCancel_Click(object, EventArgs)`, `ButtonOK_Click(object, EventArgs)`, `Cleanup()`,
`MaximizeFormViewer()`, `MinimizeFormViewer()`, `ToggleOffNavigation(bool)`,
`ToggleOffNavigationAsync()`, `ToggleOnNavigation(bool)`, `ToggleOnNavigationAsync()`,
`FormHandle`.

### Referenced-type resolution (cited from the file's own directives)

The file's `using` directives sit at compilation-unit scope, lines 1–9, ahead of
`namespace QuickFiler.Controllers` at line 11. Consequently, for any bare type name in this file
the C# lookup walks `QuickFiler.Controllers` → `QuickFiler` → global, and only reaches the
compilation-unit `using` set at the last step:

- `IFilerFormController` (line 13) — not a member of `QuickFiler.Controllers` and not a member of
  `QuickFiler`; resolved by `using QuickFiler.Interfaces;` (line 7) to
  `QuickFiler.Interfaces.IFilerFormController` (`QuickFiler/Interfaces/IFilerFormController.cs:9`).
- `IQfcFormViewer` (line 17) — resolved at the `QuickFiler` step to `QuickFiler.IQfcFormViewer`
  (`QuickFiler/Interfaces/IQfcFormViewer.cs:12`, which declares `namespace QuickFiler` at line 10).
- `IQfcCollectionController` (line 18) — resolved via `using QuickFiler.Interfaces;` (line 7).
- `QfcItemGroup` (line 29) — `QuickFiler.Controllers.QfcItemGroup`, resolved at the first step.
- `QfcPreScoredItem` (lines 32–33) — `QuickFiler.Controllers.QfcPreScoredItem`, declared at
  `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:98` (`public readonly struct
  QfcPreScoredItem`), resolved at the first step.
- `MailItem` (lines 28, 30–31) — `using Microsoft.Office.Interop.Outlook;` (line 6).
- `TableLayoutPanel` (line 29) — `using System.Windows.Forms;` (line 5).
- `ProgressTracker` (lines 31, 33) — `using UtilitiesCS;` (line 8).

---

## 2. Executable-content determination

**The file contains no executable statement of any kind.** Verified by reading all 43 lines:

- Lines 1–9 are `using` directives.
- Line 11 opens `namespace QuickFiler.Controllers`; line 13 opens the interface declaration.
- Lines 15–22 are property signatures with accessor lists only (`{ get; set; }` / `{ get; }`) —
  no accessor bodies, no expression-bodied members, no initializers.
- Lines 24–41 are method signatures terminated by `;` — no bodies. .NET Framework 4.8 targets
  C# language levels that do not permit default interface members, and none is present regardless.
- There is no static constructor, no field, no constant, no attribute expression.

Therefore the file emits no IL sequence points and **contributes zero executable lines to the
coverage denominator**. Its line-coverage figure is undefined rather than zero; per-file coverage
tooling reports such a file either as absent from the report or as `0/0`.

`.claude/rules/general-unit-test.md`, "Coverage Requirements", line 29, states verbatim:

> Type-only / interface-only modules with no executable behavior may be omitted from coverage
> measurement. Examples: Python `Protocol`-only modules consumed only under `TYPE_CHECKING`,
> TypeScript interface/type-only files, and C# interface-only files. Such modules legitimately
> report 0% executable coverage and may be excluded from measurement. This is a clarification only;
> it does not lower any coverage threshold.

This file is exactly the "C# interface-only file" case named in that clause.

**Correct disposition: `no executable content — not a coverage target`.**

F1 (`quickfiler-coverage-denominator-and-exemption-ledger`) is the ratifying authority for this
classification; this artifact records the evidence, F1's ledger records the decision.

Explicitly out of bounds: do **not** add executable code (default interface members, static
helpers, constants) to this file to manufacture a coverage numerator. That would invert the
policy's intent and would require C# language/runtime features unavailable on this target
framework.

Note the distinction from the `Coverage Exclusion Policy` section of the same rule file
(lines 31–46), which prohibits `exclude` entries for production paths. That prohibition targets
files with real executable lines being hidden from the metric. Omitting a file with **zero**
executable lines changes no numerator and no denominator; it is a reporting convenience, not an
exclusion. Recording it as `no executable content` in F1's ledger — rather than adding it to any
`coverage.config` exclude list — keeps the two rules consistent. Adding this path to
`coverage.config` would be the wrong mechanism and is not recommended.

---

## 3. Consumer map

`QuickFiler/QuickFiler.csproj` was read directly (lines 285–461 cover the entire `<Compile>`
item group) to determine compiled status for every entry below.

### Implementers

| Implementer | File : line | Compiled | Owning child |
| --- | --- | --- | --- |
| `QfcFormController` | `QuickFiler/Controllers/QfcFormController.cs:19` — `internal partial class QfcFormController : IQfcFormController` | Yes (csproj 317; partials at 318–320) | **F6 (this child)** |

`QfcFormController` is the **only** implementer in the repository. Resolution proof is in §7,
Finding D1, step (a).

### Consumers

| Consumer | File : line | What it does | Compiled | Owning child |
| --- | --- | --- | --- | --- |
| `QfcFormController` | `QuickFiler/Controllers/QfcFormController.cs:53` | `public IQfcFormController Init()` — fluent initializer returning `this` | Yes (csproj 317) | **F6 (this child)** |
| `QfcHomeController` | `QuickFiler/Controllers/QfcHomeController.cs:208` | Final type argument of the `internal Func<...>` factory property `QfcFormControllerLoader` (declared 199–209) | Yes (csproj 325) | **F7** |
| `QfcHomeController` | `QuickFiler/Controllers/QfcHomeController.cs:415` | `private IQfcFormController _formController;` — backing field for `public IFilerFormController FormController` (416–419) | Yes (csproj 325) | **F7** |
| `QfcHomeControllerPropertyTests` | `QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs:140` | `new Mock<IQfcFormController>()` | Yes (test csproj 128) | F7 (test fixture) |
| `QfcHomeControllerIssue218Tests` | `.../QfcHomeControllerIssue218Tests.cs:84, 115, 219` | Mock + `Mock<IQfcFormController>` return type on a helper | Yes (test csproj 125) | F7 |
| `QfcHomeControllerIterationTests` | `.../QfcHomeControllerIterationTests.cs:149, 223, 283, 329, 376, 411` | `new Mock<IQfcFormController>()` | Yes (test csproj 126) | F7 |
| `QfcHomeControllerRunAsyncTests` | `.../QfcHomeControllerRunAsyncTests.cs:131, 199, 262` | `new Mock<IQfcFormController>()` | Yes (test csproj 130) | F7 |
| `QfcHomeControllerRunAsyncHighConfidenceTests` | `.../QfcHomeControllerRunAsyncHighConfidenceTests.cs:28, 62, 150, 344, 443` | Mock + helper return type | Yes (test csproj 129) | F7 |
| `QfcHomeControllerTests` | `.../QfcHomeControllerTests.cs:136, 207` | `new Mock<IQfcFormController>()`, assigned into `QfcFormControllerLoader` | Yes (test csproj 131) | F7 |
| `QfcHomeControllerMetricsTests` | `.../QfcHomeControllerMetricsTests.cs:125, 211, 302` | `new Mock<IQfcFormController>(MockBehavior.Loose)` | Yes (test csproj 127) | F7 |
| `QfcViewer_Test` | `QuickFiler.Test/QfcViewer_Test.cs:38` | Commented out (`////Mock<IQfcFormController> ...`) — inert | Yes (test csproj 167) | n/a |

### Textual matches that are NOT consumers of this type

| File : line | Why not | Compiled |
| --- | --- | --- |
| `QuickFiler/Interfaces/IQfcHomeController.cs:9` | Binds to the **`QuickFiler.Interfaces`** variant, not this one (see Finding D1). | **No** — absent from the csproj `Interfaces\` block (lines 355–368) |
| `QuickFiler/Viewers/QfcFormViewerExpanded.cs:28, 31` | Ambiguous reference; would not compile (see Finding D1, CS0104 analysis). | **No** — absent from the csproj `Viewers\` block (lines 380–461) |
| `QuickFiler/Viewers/QfcFormViewerDark.cs:28, 31` | Same. | **No** — same |
| `QuickFiler/Notes/notes_interfaces.cs:6, 13` | Declares an unrelated `QuickFiler.Notes.IQfcFormController`; the file is not valid C# (interface fields at lines 5–8, 28, 30; duplicate `ReadyForMove` at 108–109). | **No** — the csproj contains no `Notes\` `<Compile>` entry |
| `QuickFiler/QuickFiler.csproj.bak:243` | A backup of the project file; not an MSBuild input. | n/a |

**Ownership consequence.** Every non-test consumer of this interface outside F6 is
`QfcHomeController`, owned by **F7**. Any change to this interface's member set is an F6→F7
contract change. See §4.

---

## 4. Contract-stability assessment

**Assessment: F6's seam work should not need to grow this interface. Treat the member set as frozen.**

Reasoning, grounded in what the interface is for:

1. This interface is the *outbound* contract that `QfcHomeController` (F7) consumes from
   `QfcFormController` (F6). F6's coverage problem is *inbound*: the four `QfcFormController`
   partials reach the WinForms viewer, the Outlook interop surface, and `KeyboardHandler`. Seams
   that make those reachable belong on `IQfcFormViewer` (§ the `Interfaces.IQfcFormViewer.cs.md`
   artifact), on new narrow interfaces, or on injectable `Func<>`/`Action<>` properties on the
   concrete class — not on the controller's own public contract.
2. The repository already demonstrates the preferred pattern for exactly this situation:
   `QfcHomeController.cs:191–209` declares `internal Func<...>` factory properties
   (`QfcQueueLoader`, `QfcFormControllerLoader`, `QfcExplorerControllerLoader`) that tests overwrite
   directly. `[assembly: InternalsVisibleTo("QuickFiler.Test")]` at `QfcHomeController.cs:18`
   makes `internal` members visible to the test assembly. An `internal` seam member on the
   concrete `QfcFormController` class is therefore testable **without** appearing on this public
   interface. This is the recommended growth path and it costs this interface nothing.
3. The existing F6 fixtures already construct `QfcFormController` directly
   (`QfcFormControllerTests.cs:77–87`) rather than through the interface, so new tests do not need
   new interface members to reach new behavior.

**If growth nevertheless proves necessary**, the required edits are:

- `QuickFiler/Controllers/IQfcFormController.cs` — add the member (F6, this child).
- `QuickFiler/Controllers/QfcFormController.*.cs` — implement it (F6, this child).
- No `QfcHomeController` edit is required for an **additive** member, because F7's two references
  (`:208`, `:415`) are type references, not member invocations. **CROSS-CHILD CONTRACT NOTE (F7)**:
  an additive member is source-compatible for F7; a **removal or rename** is not, and would require
  F7 to re-verify `QfcHomeController.cs:208` and `:415` and the seven F7-owned test fixtures listed
  in §3. Removal/rename is not recommended within F6.

**Prior-art caution.** `docs/features/archive/2026-06-02-quickfiler-high-confidence-prefilter-171/`
records that this interface has been grown before (`policy-audit.2026-06-02T11-06.md:161` lists
`IQfcFormController.cs (+2)`, and `spec.md:101` states "add the pre-scored `LoadItemsAsync` overload
to the interface"). That is the origin of the four `LoadItemsAsync` overloads at lines 30–33.
Precedent exists for additive growth; it is not a reason to grow it again absent a demonstrated need.

---

## 5. Proposed test cases

**None. The file has no executable content, so there is nothing to execute in a test.**

Stated plainly so the plan author does not have to re-derive it: do not author reflection-based
tests that assert this interface declares a given member, that it derives from
`IFilerFormController`, or that its member count is 26. Such tests execute only reflection code in
the *test* assembly. They add zero lines to the production coverage numerator, they duplicate what
the compiler already enforces (a missing member is CS0535 at `QfcFormController.cs:19`), and they
become maintenance drag on every legitimate interface change. They do not satisfy any clause of the
coverage policy.

The interface's behavior is already covered transitively and legitimately by the tests that
exercise the *implementation*: `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` and
`QfcFormControllerSeamTests.cs` (both F6-owned), whose coverage is attributed to the
`QfcFormController.*` partials, not to this file.

The one compile-time property worth guarding — that `QfcFormController` satisfies this contract —
is already guarded by the build. `QfcFormControllerTests.cs:159` additionally performs
`Assert.AreEqual((IFilerFormController)controller, _filerFormController)`, which is a compile-time
assertion that the implemented interface derives from `IFilerFormController`. No further test is
warranted.

---

## 6. Upstream dependency on F1

F1 (`quickfiler-coverage-denominator-and-exemption-ledger`, wave 0) delivers two artifacts this
child consumes:

1. The repeatable per-file line-coverage harness derived from the Cobertura output of
   `Invoke-MSTestWithCoverage.ps1` (epic.md, "Shared Design" §6).
2. The ratified per-file classification ledger at
   `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` (epic.md, F1 deliverables).

**F1 is being prepared concurrently and its outputs do not exist on disk yet. Their absence is
neither a blocker nor a gap and is not reported as one.**

The ledger is the authority that classifies this file as `testable`, `ratified-exempt`, or
`no executable content`. This artifact's §2 supplies the evidence supporting
`no executable content`; F1 makes the call.

**F6's acceptance criteria apply only to files the ledger classifies as `testable`.** If the ledger
classifies this file as `no executable content`, the ">= 80% line coverage" criterion does not
apply to it, and F6's evidence for this file is the classification itself rather than a percentage.
The epic's F6 file list ("~1,611 lines / 10 files") counts this file; the ledger determines whether
it counts toward the coverage obligation.

---

## 7. Open questions / findings

### FINDING D1 — Two compiled `IQfcFormController` declarations (stated in full; also recorded in `Interfaces.IQfcFormController.cs.md`)

**Statement of fact.** `QuickFiler.csproj` compiles two files named `IQfcFormController.cs` that
declare two unrelated public interfaces with the same simple name in two different namespaces:

| # | Path | csproj line | Namespace | Base | Members declared |
| --- | --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Controllers/IQfcFormController.cs` | 303 | `QuickFiler.Controllers` (line 11) | `IFilerFormController` (line 13) | 8 properties + 18 methods, plus 12 inherited |
| 2 | `QuickFiler/Interfaces/IQfcFormController.cs` | 363 | `QuickFiler.Interfaces` (line 5) | **none** (line 7) | 3 properties + 11 methods |

Both csproj lines were read directly at `QuickFiler/QuickFiler.csproj:303` and `:363`.

**Member-set difference.** Members present on #2 but absent from #1 (including #1's inherited set):
`ButtonCancel_Click()` (no-arg, #2 line 9), `ButtonOK_Click()` (no-arg, #2 line 10),
`MaximizeQfcFormViewer()` (#2 line 16), `MinimizeQfcFormViewer()` (#2 line 17). Members present on
#1 but absent from #2: all eight of `ActiveTheme`, `DarkMode`, `FormViewer`, `Token`, `TokenSource`,
plus every `LoadItemsAsync` overload, `CaptureItemSettings`, `RemoveTemplatesAndSetupTlp`,
`SetupLightDark`, `Register/UnregisterFormEventHandlers`, `SkipGroupAsync`, `ButtonSkip_Click`,
`LoadItemsPerIteration`, `LoadItems(TableLayoutPanel, List<QfcItemGroup>)`, and the twelve
`IFilerFormController` members. Overlap is `ButtonUndo_Click()` / `ButtonUndo_Click(object,
EventArgs)` / `SpnEmailPerLoad_ValueChanged` / `Viewer_Activate` / `SpaceForEmail` /
`ItemsPerIteration` / `LoadItems(IList<MailItem>)` / `Groups`, plus `Cleanup()` /
`ButtonCancel_Click(object, EventArgs)` / `ButtonOK_Click(object, EventArgs)` which #1 inherits.

**Three apparent consumers are removed because their files are not compiled.** Verified against the
csproj item group at lines 289–461:

- `QuickFiler/Viewers/QfcFormViewerExpanded.cs` — the `Viewers\` block (lines 380–461) lists
  `BayesianPerformanceViewer`, `EfcViewer`, `IItemViewer`, the Breadcrumb family, `ItemViewer.*`,
  `ItemViewerExpanded.*`, `QfcFormViewer.cs`/`.Designer.cs`, `QfcItemViewerExpanded.*`,
  `ToolStripMenuItemCb.*`. `QfcFormViewerExpanded.cs` is not among them.
- `QuickFiler/Viewers/QfcFormViewerDark.cs` — likewise absent.
- `QuickFiler/Notes/notes_interfaces.cs` — the csproj contains no `Notes\` `<Compile>` entry at
  all. The file declares its own `QuickFiler.Notes.IQfcFormController` at line 13 and is not valid
  C# in any case (fields declared inside interfaces at lines 5–8, 28, 30; a duplicate
  `ReadyForMove` member at lines 108–109).

A fourth textual match, `QuickFiler/QuickFiler.csproj.bak:243`, is a backup of the project file and
is not an MSBuild input.

**Namespace-resolution mechanism.** Every file below places its `using` directives at
*compilation-unit* scope, before the `namespace` block. That placement is decisive. The C#
namespace-or-type-name lookup walks each enclosing namespace `N` from innermost outward; at each
`N` it first tests whether `N` itself contains a type of that name, and it consults `using`
directives only when the location is inside a *namespace declaration for `N`* that carries them.
Compilation-unit-level `using` directives are therefore consulted only at the outermost step. An
enclosing namespace's own type wins over a compilation-unit `using`.

(a) **`QuickFiler/Controllers/QfcFormController.cs:19` implements #1, the `Controllers` variant.**
Directives at lines 1–15 including `using QuickFiler.Interfaces;` at line 10, all at compilation-unit
scope; `namespace QuickFiler.Controllers` opens at line 17. Lookup step 1 is
`N = QuickFiler.Controllers`, which contains `IQfcFormController` (file #1) — resolved, the
`using` set is never reached. Three independent confirmations that the class implements #1 and could
not implement #2:

  - The class implements `MaximizeFormViewer()` (`QfcFormController.Actions.cs:187`) and
    `MinimizeFormViewer()` (`.Actions.cs:197`) — the `IFilerFormController` spellings. A repo-wide
    search for `MaximizeQfcFormViewer` / `MinimizeQfcFormViewer` (the #2 spellings) returns matches
    only at `QuickFiler/Interfaces/IQfcFormController.cs:16` and `:17` — the declarations
    themselves. **There is no implementation of either member anywhere in the repository.** If
    `QfcFormController` bound to #2 the build would fail CS0535.
  - The class implements the `IFilerFormController` set that #2 does not declare:
    `ActionCancelAsync` (`.EventHandlers.cs:84`), `ActionOkAsync` (`.EventHandlers.cs:110`),
    `ToggleOffNavigation`/`ToggleOffNavigationAsync`/`ToggleOnNavigation`/`ToggleOnNavigationAsync`
    (`QfcFormController.cs:174–180`), `FormHandle` (`QfcFormController.cs:163`).
  - `QuickFiler.Test/Controllers/QfcFormControllerTests.cs:159` performs
    `Assert.AreEqual((IFilerFormController)controller, _filerFormController)`. That cast compiles
    only because the implemented interface derives from `IFilerFormController`; #2 has no base
    interface (`Interfaces/IQfcFormController.cs:7`).

(b) **`QuickFiler/Interfaces/IQfcHomeController.cs:9` (`IQfcFormController FrmCtrlr { get; }`)
refers to #2 — but that file is not compiled, so it is not a live consumer.** Its directives are
`using System;` and `using ToDoModel;` (lines 1–2, compilation-unit scope);
`namespace QuickFiler.Interfaces` opens at line 4. Step 1 is `N = QuickFiler.Interfaces`, which
contains `IQfcFormController` (file #2) — resolved to #2. However, the csproj `Interfaces\` block
(lines 355–368) enumerates exactly fourteen files — `IEmailMoveMonitor`, `IFilerFormController`,
`IFilerHomeController`, `IItemControler`, `IKbdAction`, `IQfcCollectionController`, `IQfcDatamodel`,
`IQfcExplorerController`, `IQfcFormController`, `IQfcFormViewer`, `IQfcItemController`,
`IQfcKeyboardHandler`, `IMailItemActions`, `MailItemActionsAdapter` — and
`Interfaces\IQfcHomeController.cs` is **not** among them. The compiled `IQfcHomeController` is a
different file, `QuickFiler/Controllers/IQfcHomeController.cs` (csproj line 304), declaring
`public interface IQfcHomeController : IFilerHomeController` in `namespace QuickFiler.Controllers`;
it was read in full (20 lines) and contains **no** reference to `IQfcFormController`.
Corroboration: a repo-wide search for `FrmCtrlr` across all `*.cs` returns exactly one hit,
`Interfaces/IQfcHomeController.cs:9`, with no implementation anywhere — consistent with the file
being excluded from the build. `QfcHomeController` declares `: IQfcHomeController`
(`QfcHomeController.cs:22`) and exposes `FormController`, `ExplorerController`, `KeyboardHandler`
(lines 409–426), not `FrmCtrlr`/`ExplCtrlr`/`KbdHndlr`; it could not satisfy the
`QuickFiler.Interfaces` version.

(c) **`QuickFiler/Controllers/QfcHomeController.cs:208` and `:415` both refer to #1.** Directives
at lines 1–16 including `using QuickFiler.Interfaces;` at line 14, all at compilation-unit scope;
`namespace QuickFiler.Controllers` opens at line 20. Step 1 resolves to #1. Independent
confirmation at lines 415–419: `private IQfcFormController _formController;` backs
`public IFilerFormController FormController { get => _formController; }`. That implicit conversion
exists only if the field's type derives from `IFilerFormController`; #2 has no base interface, so
the field cannot be #2.

(d) **All test mocks bind to #1.** All seven fixtures declare `namespace QuickFiler.Controllers.Tests`
(`QfcHomeControllerTests.cs:20`, `RunAsyncTests.cs:21`, `RunAsyncHighConfidenceTests.cs:14`,
`PropertyTests.cs:20`, `MetricsTests.cs:22`, `IterationTests.cs:21`, `Issue218Tests.cs:14`) with
`using QuickFiler.Interfaces;` at compilation-unit scope (lines 14, 15, 11, 14, 16, 15, 9
respectively). Lookup step 1 is `N = QuickFiler.Controllers.Tests`, which declares no such type and
whose namespace body carries no `using` directives; step 2 is `N = QuickFiler.Controllers`, which
contains #1 — resolved to #1 before the compilation-unit `using` set is ever consulted. Two runtime
and compile-time confirmations:

  - `QfcHomeControllerTests.cs:136–146` assigns `(...) => mockFormController.Object` to
    `_controller.QfcFormControllerLoader`, whose declared return type is #1
    (`QfcHomeController.cs:199–209`). A `Mock<#2>.Object` is not convertible to #1 and would not
    compile.
  - `QfcHomeControllerRunAsyncTests.cs:134–141` reflectively `SetValue`s the mock into the private
    field `_formController`, typed as #1 (`QfcHomeController.cs:415`). A #2-only instance would
    throw `ArgumentException` at run time.

  Note that `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` carries **both**
  `using QuickFiler.Controllers;` (line 10) and `using QuickFiler.Interfaces;` (line 11). It does
  not reference the bare name `IQfcFormController`, and even if it did, the enclosing-namespace step
  would resolve it before either `using` was consulted.

**Verdict.**

- **Authoritative file: `QuickFiler/Controllers/IQfcFormController.cs` (#1).** It has one
  implementer (`QfcFormController`) and live compiled consumers in `QfcHomeController` and seven
  test fixtures.
- **`QuickFiler/Interfaces/IQfcFormController.cs` (#2) is compiled dead code.** It has **zero
  implementers** and **zero compiled consumers**. Its only textual consumers are three non-compiled
  files plus a `.bak` project file. Two of its declared members (`MaximizeQfcFormViewer`,
  `MinimizeQfcFormViewer`) have no implementation anywhere in the repository. It is not "live on a
  separate path"; there is no such path.

**CS0104 ambiguous-reference hazard.** A compiled file trips CS0104 only when (a) it references the
bare name `IQfcFormController`, (b) two or more `using`-imported namespaces at the deciding level
supply that name, and (c) no enclosing namespace in its chain supplies it first. Enumerating every
compiled file that references the bare name — `QfcFormController.cs:19,53`;
`QfcHomeController.cs:208,415`; and the seven `QuickFiler.Test/Controllers/QfcHomeController*.cs`
fixtures — every one sits in the `QuickFiler.Controllers` or `QuickFiler.Controllers.Tests` chain
and is resolved by condition (c) failing. **No compiled file currently has a CS0104 hazard from
this duplication.**

The hazard is latent but real and is demonstrable today in two non-compiled files:
`QuickFiler/Viewers/QfcFormViewerExpanded.cs` and `QuickFiler/Viewers/QfcFormViewerDark.cs` each
declare `namespace QuickFiler` (line 14) with **both** `using QuickFiler.Controllers;` (line 11)
and `using QuickFiler.Interfaces;` (line 12), and reference the bare name at lines 28 and 31.
`namespace QuickFiler` contains no `IQfcFormController`, so lookup falls through to the
compilation-unit `using` set, which supplies two candidates — CS0104. Adding either file to
`QuickFiler.csproj` in its present form would break the build. The same trap awaits any *new*
compiled file placed outside the `QuickFiler.Controllers` / `QuickFiler.Interfaces` chains that
imports both namespaces.

**RECOMMENDATION (not to be executed by F6).**

Recommend deleting `QuickFiler/Interfaces/IQfcFormController.cs` and its csproj entry. The single
most important de-risking fact is that **no production consumer edit is required**, because the
type has no consumers. Exact edits:

1. `QuickFiler/Interfaces/IQfcFormController.cs` — delete the file. Owner: **F6** by epic.md
   "Feature File Assignments" (listed in the F6 set as `Interfaces/IQfcFormController.cs` (25)).
2. `QuickFiler/QuickFiler.csproj` line 363 — remove `<Compile Include="Interfaces\IQfcFormController.cs" />`.
   **CROSS-CHILD CONTRACT NOTE (F1):** the csproj `<Compile>` set *is* the epic's 121-file
   coverage denominator (epic.md, "Scope"). Removing an entry changes the denominator to 120 and
   invalidates any ledger row keyed to that path. F1 owns the denominator and the ledger; this edit
   must be sequenced with F1, not landed ahead of it.
3. **CROSS-CHILD CONTRACT NOTE (F7):** none required. `QfcHomeController.cs:208` and `:415` bind to
   the `Controllers` variant and are unaffected. F7's seven test fixtures are likewise unaffected.
   This note exists so F7 can confirm the analysis rather than discover the deletion at fan-in.
4. **CROSS-CHILD CONTRACT NOTE (F15):** none required. `Viewers/QfcFormViewerExpanded.cs` and
   `Viewers/QfcFormViewerDark.cs` are not compiled and are not in F15's assigned file list
   (F15 owns `Viewers/QfcFormViewer.cs`, `QfcItemViewerExpanded.cs`, `BayesianPerformanceViewer.cs`,
   `ToolStripMenuItemCb.cs`, and the associated Designer/Properties files). Deleting #2 would make
   their bare references unambiguous rather than break them. Recorded so F15 is not surprised.
5. `QuickFiler/Interfaces/IQfcHomeController.cs` (not compiled) would be left referencing a deleted
   type. It is not an MSBuild input, so this breaks nothing. It is unassigned by epic.md, whose
   assignments cover only the 121 compiled files. Recommend deleting it in the same change as dead
   code, or leaving it untouched — either is safe; do not silently retarget it.

**Where this belongs: F16 capstone, not F6.** Three reasons:

- It changes the compiled-file denominator that F1 defines and F16 verifies. A wave-1 child
  silently reducing the denominator would make F16's "every one of the 121 compiled files" check
  ambiguous.
- It edits `QuickFiler.csproj`, a shared build input that all fourteen wave-1 children compile
  against concurrently. A csproj edit inside one wave-1 child is a predictable merge-conflict
  surface at fan-in. The issue.md constraint that "`coverage.config` and shared build property
  files belong to F1" reflects the same principle.
- F6 gains nothing measurable. The file has no executable content, so it is in neither the coverage
  numerator nor the denominator. Deleting it does not move F6's per-file evidence by one line.

If the maintainer prefers minimum churn, the alternative is to keep the file, record it in F1's
ledger as `no executable content — unreferenced`, and note the latent CS0104 trap. The coverage
metric is indifferent between the two options; the argument for deletion is dead-code hygiene and
removing the CS0104 trap, not coverage.

### Open question O1 — Does the plan author want a build-time guard against reintroducing the trap?

If `QuickFiler/Interfaces/IQfcFormController.cs` is retained, a future compiled file in
`namespace QuickFiler` (or any namespace outside the two chains) that imports both namespaces will
fail with CS0104 at a point far from the cause. Options: (i) delete per the recommendation above;
(ii) add a `using` alias convention; (iii) accept the risk and document it. This artifact
recommends (i), routed through F16. Decision belongs to the plan author and the maintainer.

### Open question O2 — Ledger key collision on the shared base name

F1's ledger is keyed per file. Two distinct rows will both be named `IQfcFormController.cs` if
keyed by base name. Recommend F1 key the ledger by repo-relative path
(`QuickFiler/Controllers/IQfcFormController.cs` vs `QuickFiler/Interfaces/IQfcFormController.cs`).
The same applies to `IQfcHomeController.cs`, which also exists in both folders, though only the
`Controllers/` copy is compiled. Raised here because F6 owns both `IQfcFormController.cs` rows and
is the first child likely to hit the collision.
