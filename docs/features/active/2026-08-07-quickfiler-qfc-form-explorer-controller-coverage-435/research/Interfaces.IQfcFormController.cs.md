# Research — `QuickFiler/Interfaces/IQfcFormController.cs`

Timestamp: 2026-08-07T22-40

## 1. Header

| Field | Value |
| --- | --- |
| Production file | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8220048ded06d508\QuickFiler\Interfaces\IQfcFormController.cs` |
| Exact line count | 25 |
| Declared namespace | `QuickFiler.Interfaces` (line 5) |
| Declared type | `public interface IQfcFormController` (line 7) — **no base interface** |
| `[ExcludeFromCodeCoverage]` | **No.** Verified by reading the entire 25-line file; it contains no attribute of any kind. |
| Compiled | **Yes** — `QuickFiler/QuickFiler.csproj` line 363: `<Compile Include="Interfaces\IQfcFormController.cs" />` |
| Feature child | F6 (issue #435), epic #136 |

Current numeric per-file line coverage is **unmeasured**, and will remain undefined regardless of
testing because the file has no executable lines (§2).

### Declared member set (verified line by line)

Methods (lines 9–19): `ButtonCancel_Click()`, `ButtonOK_Click()`, `ButtonUndo_Click()`,
`ButtonCancel_Click(object, EventArgs)`, `ButtonOK_Click(object, EventArgs)`,
`ButtonUndo_Click(object, EventArgs)`, `Cleanup()`, `MaximizeQfcFormViewer()`,
`MinimizeQfcFormViewer()`, `SpnEmailPerLoad_ValueChanged(object, EventArgs)`, `Viewer_Activate()`.

Properties (lines 20–23): `SpaceForEmail` (`int`, get), `ItemsPerIteration` (`int`, get),
`Groups` (`IQfcCollectionController`, get). Plus method `LoadItems(IList<MailItem>)` at line 22.

Referenced-type resolution, cited from this file's own directives (`using System;` line 1,
`using System.Collections.Generic;` line 2, `using Microsoft.Office.Interop.Outlook;` line 3, all
at compilation-unit scope; `namespace QuickFiler.Interfaces` opens at line 5):
`EventArgs` → `System`; `IList<>` → `System.Collections.Generic`; `MailItem` →
`Microsoft.Office.Interop.Outlook`; `IQfcCollectionController` → resolved at lookup step 1 as
`QuickFiler.Interfaces.IQfcCollectionController` (`QuickFiler/Interfaces/IQfcCollectionController.cs`,
csproj line 360).

---

## 2. Executable-content determination

**The file contains no executable statement of any kind.** Verified by reading all 25 lines:

- Lines 1–3 are `using` directives.
- Line 5 opens `namespace QuickFiler.Interfaces`; line 7 opens the interface declaration.
- Lines 9–19 and line 22 are method signatures terminated by `;` — no bodies. .NET Framework 4.8
  does not support default interface members, and none is present regardless.
- Lines 20, 21, 23 are property signatures with a bare `{ get; }` accessor list — no accessor
  bodies, no expression-bodied members, no initializers.
- There is no static constructor, no field, no constant, no attribute expression.

The file emits no IL sequence points and **contributes zero executable lines to the coverage
denominator**. Its line-coverage figure is undefined rather than zero.

`.claude/rules/general-unit-test.md`, "Coverage Requirements", line 29, states verbatim:

> Type-only / interface-only modules with no executable behavior may be omitted from coverage
> measurement. Examples: Python `Protocol`-only modules consumed only under `TYPE_CHECKING`,
> TypeScript interface/type-only files, and C# interface-only files. Such modules legitimately
> report 0% executable coverage and may be excluded from measurement. This is a clarification only;
> it does not lower any coverage threshold.

This file is exactly the "C# interface-only file" case named in that clause.

**Correct disposition: `no executable content — not a coverage target`.**

F1 (`quickfiler-coverage-denominator-and-exemption-ledger`) is the ratifying authority for this
classification; this artifact supplies the evidence, F1's ledger records the decision.

Do **not** add executable code to this file to manufacture coverage. That is doubly inappropriate
here, because Finding D1 (§7) establishes the file is unreferenced dead code — adding executable
lines would create *uncoverable* lines in a type nothing constructs.

The `Coverage Exclusion Policy` prohibition in the same rule file (lines 31–46) targets files with
real executable lines being hidden from the metric. A file with zero executable lines changes
neither numerator nor denominator, so recording it as `no executable content` in F1's ledger — as
opposed to adding it to a `coverage.config` exclude list — keeps the two rules consistent. Adding
this path to `coverage.config` would be the wrong mechanism and is not recommended.

---

## 3. Consumer map

`QuickFiler/QuickFiler.csproj` was read directly (the `<Compile>` item group spans lines 289–461)
to determine compiled status for every entry below.

### Implementers

**None.** A repository-wide search across all `*.cs` files finds no type declaring
`: QuickFiler.Interfaces.IQfcFormController`, and no type declaring the bare name in a context that
would bind to it. The one type that declares `: IQfcFormController` —
`QuickFiler/Controllers/QfcFormController.cs:19` — binds to the `QuickFiler.Controllers` variant
(Finding D1, step (a)).

Corroborating evidence: two members of this interface, `MaximizeQfcFormViewer()` (line 16) and
`MinimizeQfcFormViewer()` (line 17), match **nowhere else in the repository**. A repo-wide search
for those two identifiers returns only these two declaration lines. No implementation exists.

### Consumers

| Consumer | File : line | Compiled | Owning child |
| --- | --- | --- | --- |
| `IQfcHomeController.FrmCtrlr` | `QuickFiler/Interfaces/IQfcHomeController.cs:9` — `IQfcFormController FrmCtrlr { get; }` | **No** — absent from the csproj `Interfaces\` block (lines 355–368) | Unassigned by epic.md (which assigns only the 121 compiled files) |

**That is the complete list, and it is not compiled. This type has zero compiled consumers.**

### Textual matches that are NOT consumers of this type

| File : line | Why not | Compiled |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcFormController.cs:19, 53` | Binds to the `QuickFiler.Controllers` variant (Finding D1(a)). | Yes (csproj 317) — but of the *other* type |
| `QuickFiler/Controllers/QfcHomeController.cs:208, 415` | Binds to the `QuickFiler.Controllers` variant (Finding D1(c)). | Yes (csproj 325) — other type |
| Seven `QuickFiler.Test/Controllers/QfcHomeController*.cs` fixtures | Bind to the `QuickFiler.Controllers` variant (Finding D1(d)). | Yes — other type |
| `QuickFiler/Viewers/QfcFormViewerExpanded.cs:28, 31` | Ambiguous reference; would not compile (Finding D1, CS0104 analysis). | **No** — absent from the csproj `Viewers\` block (lines 380–461) |
| `QuickFiler/Viewers/QfcFormViewerDark.cs:28, 31` | Same. | **No** |
| `QuickFiler/Notes/notes_interfaces.cs:6, 13` | Declares an unrelated `QuickFiler.Notes.IQfcFormController`; the file is not valid C#. | **No** — no `Notes\` `<Compile>` entry exists |
| `QuickFiler/QuickFiler.csproj.bak:243` | Backup of the project file; not an MSBuild input. | n/a |
| `QuickFiler.Test/QfcViewer_Test.cs:38` | Commented out (`////Mock<IQfcFormController> ...`). | Yes, but the line is inert |

---

## 4. Contract-stability assessment

**Assessment: this interface will not grow, because nothing consumes it.** F6's seam work cannot
require adding a member to a type that no compiled file references and no type implements. The
member set is not merely stable — it is inert.

There is one live question about this file, and it is disposition, not growth: whether it should be
deleted. That is Finding D1 in §7, and the recommendation is that the deletion be routed to F16
rather than executed by F6.

**CROSS-CHILD CONTRACT NOTE (F7):** none required for any change to this file, including deletion.
`QfcHomeController.cs:208` and `:415` bind to `QuickFiler.Controllers.IQfcFormController`, not to
this type. This note is recorded so F7 can verify the analysis rather than discover a deletion at
fan-in.

**CROSS-CHILD CONTRACT NOTE (F15):** none required. The two `Viewers/` files that reference the bare
name (`QfcFormViewerExpanded.cs`, `QfcFormViewerDark.cs`) are not compiled and are not in F15's
assigned file set. Deleting this type would make their references unambiguous rather than break them.

**CROSS-CHILD CONTRACT NOTE (F1):** deleting this file requires removing csproj line 363, which
reduces the epic's compiled-file denominator from 121 to 120. F1 owns the denominator and the
ledger. This edit must be sequenced with F1.

---

## 5. Proposed test cases

**None. The file has no executable content, so there is nothing to execute in a test.**

Stated plainly: do not author reflection-based tests asserting that this interface declares
`MaximizeQfcFormViewer`, that it has fourteen members, or that some type implements it. The last of
those would fail — nothing implements it. Reflection tests over a declaration file execute only
test-assembly code, add zero lines to the production coverage numerator, and satisfy no clause of
the coverage policy.

Nor is there an implementation whose tests could cover this file transitively — unlike
`Controllers/IQfcFormController.cs`, which is covered indirectly through `QfcFormControllerTests.cs`
and `QfcFormControllerSeamTests.cs`. This interface has no implementation at all.

---

## 6. Upstream dependency on F1

F1 (`quickfiler-coverage-denominator-and-exemption-ledger`, wave 0) delivers the per-file coverage
harness (epic.md, "Shared Design" §6) and the ratified classification ledger at
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` (epic.md, F1 deliverables).

**F1 is being prepared concurrently and its outputs do not exist on disk yet. Their absence is
neither a blocker nor a gap and is not reported as one.**

The ledger is the authority that classifies this file as `testable`, `ratified-exempt`, or
`no executable content`. §2 supplies the evidence for `no executable content`; F1 makes the call.

**F6's acceptance criteria apply only to files the ledger classifies as `testable`.** If the ledger
classifies this file as `no executable content`, the ">= 80% line coverage" criterion does not apply
to it and F6's evidence for this file is the classification itself, not a percentage.

Two specific items this artifact hands to F1:

- Finding D1 recommends this file's deletion. If F1 accepts that recommendation and routes it to
  F16, the ledger should carry a row for this path marked `no executable content — unreferenced,
  deletion recommended (see F6 research)` so the intent survives to the capstone.
- Open question O2 in `Controllers.IQfcFormController.cs.md`: the ledger must be keyed by
  repo-relative path, not base name, because two distinct compiled files share the base name
  `IQfcFormController.cs`.

---

## 7. Open questions / findings

### FINDING D1 — Two compiled `IQfcFormController` declarations (stated in full; also recorded in `Controllers.IQfcFormController.cs.md`)

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
#1 but absent from #2: all of `ActiveTheme`, `DarkMode`, `FormViewer`, `Token`, `TokenSource`, the
four `LoadItemsAsync` overloads, `CaptureItemSettings`, `RemoveTemplatesAndSetupTlp`,
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
- `QuickFiler/Notes/notes_interfaces.cs` — the csproj contains no `Notes\` `<Compile>` entry at all.
  The file declares its own `QuickFiler.Notes.IQfcFormController` at line 13 and is not valid C# in
  any case (fields declared inside interfaces at lines 5–8, 28, 30; a duplicate `ReadyForMove`
  member at lines 108–109).

A fourth textual match, `QuickFiler/QuickFiler.csproj.bak:243`, is a backup of the project file and
is not an MSBuild input.

**Namespace-resolution mechanism.** Every file below places its `using` directives at
*compilation-unit* scope, before the `namespace` block. That placement is decisive. The C#
namespace-or-type-name lookup walks each enclosing namespace `N` from innermost outward; at each `N`
it first tests whether `N` itself contains a type of that name, and it consults `using` directives
only when the location is inside a *namespace declaration for `N`* that carries them.
Compilation-unit-level `using` directives are therefore consulted only at the outermost step. An
enclosing namespace's own type wins over a compilation-unit `using`.

(a) **`QuickFiler/Controllers/QfcFormController.cs:19` implements #1, the `Controllers` variant.**
Directives at lines 1–15 including `using QuickFiler.Interfaces;` at line 10, all at
compilation-unit scope; `namespace QuickFiler.Controllers` opens at line 17. Lookup step 1 is
`N = QuickFiler.Controllers`, which contains `IQfcFormController` (file #1) — resolved, the `using`
set is never reached. Three independent confirmations that the class implements #1 and could not
implement #2:

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
    interface (line 7 of this file).

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

**This step (b) is the load-bearing result for this file.** `IQfcHomeController.FrmCtrlr` was the
only candidate live consumer of #2, and it is not compiled. #2 therefore has no live consumer at all.

(c) **`QuickFiler/Controllers/QfcHomeController.cs:208` and `:415` both refer to #1.** Directives at
lines 1–16 including `using QuickFiler.Interfaces;` at line 14, all at compilation-unit scope;
`namespace QuickFiler.Controllers` opens at line 20. Step 1 resolves to #1. Independent confirmation
at lines 415–419: `private IQfcFormController _formController;` backs
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

- **Authoritative file: `QuickFiler/Controllers/IQfcFormController.cs` (#1).** One implementer
  (`QfcFormController`), live compiled consumers in `QfcHomeController` and seven test fixtures.
- **This file, `QuickFiler/Interfaces/IQfcFormController.cs` (#2), is compiled dead code.** Zero
  implementers, zero compiled consumers. Its only textual consumers are three non-compiled files
  plus a `.bak` project file. Two of its declared members (`MaximizeQfcFormViewer`,
  `MinimizeQfcFormViewer`) have no implementation anywhere in the repository. It is **not** "live on
  a separate path"; there is no such path.

**CS0104 ambiguous-reference hazard.** A compiled file trips CS0104 only when (a) it references the
bare name `IQfcFormController`, (b) two or more `using`-imported namespaces at the deciding level
supply that name, and (c) no enclosing namespace in its chain supplies it first. Enumerating every
compiled file that references the bare name — `QfcFormController.cs:19,53`;
`QfcHomeController.cs:208,415`; and the seven `QuickFiler.Test/Controllers/QfcHomeController*.cs`
fixtures — every one sits in the `QuickFiler.Controllers` or `QuickFiler.Controllers.Tests` chain
and is resolved by condition (c) failing. **No compiled file currently has a CS0104 hazard from this
duplication.**

The hazard is latent but real and is demonstrable today in two non-compiled files:
`QuickFiler/Viewers/QfcFormViewerExpanded.cs` and `QuickFiler/Viewers/QfcFormViewerDark.cs` each
declare `namespace QuickFiler` (line 14) with **both** `using QuickFiler.Controllers;` (line 11) and
`using QuickFiler.Interfaces;` (line 12), and reference the bare name at lines 28 and 31.
`namespace QuickFiler` contains no `IQfcFormController`, so lookup falls through to the
compilation-unit `using` set, which supplies two candidates — CS0104. Adding either file to
`QuickFiler.csproj` in its present form would break the build. The same trap awaits any *new*
compiled file placed outside the `QuickFiler.Controllers` / `QuickFiler.Interfaces` chains that
imports both namespaces.

**RECOMMENDATION (not to be executed by F6).**

Recommend deleting this file (`QuickFiler/Interfaces/IQfcFormController.cs`) and its csproj entry.
The single most important de-risking fact is that **no production consumer edit is required**,
because the type has no consumers. Exact edits:

1. `QuickFiler/Interfaces/IQfcFormController.cs` — delete the file. Owner: **F6** by epic.md
   "Feature File Assignments" (listed in the F6 set as `Interfaces/IQfcFormController.cs` (25)).
2. `QuickFiler/QuickFiler.csproj` line 363 — remove
   `<Compile Include="Interfaces\IQfcFormController.cs" />`.
   **CROSS-CHILD CONTRACT NOTE (F1):** the csproj `<Compile>` set *is* the epic's 121-file coverage
   denominator (epic.md, "Scope"). Removing an entry changes the denominator to 120 and invalidates
   any ledger row keyed to that path. F1 owns the denominator and the ledger; this edit must be
   sequenced with F1, not landed ahead of it.
3. **CROSS-CHILD CONTRACT NOTE (F7):** none required. `QfcHomeController.cs:208` and `:415` bind to
   the `Controllers` variant and are unaffected, as are F7's seven test fixtures. Recorded so F7 can
   confirm the analysis rather than discover the deletion at fan-in.
4. **CROSS-CHILD CONTRACT NOTE (F15):** none required. `Viewers/QfcFormViewerExpanded.cs` and
   `Viewers/QfcFormViewerDark.cs` are not compiled and are not in F15's assigned file list (F15 owns
   `Viewers/QfcFormViewer.cs`, `QfcItemViewerExpanded.cs`, `BayesianPerformanceViewer.cs`,
   `ToolStripMenuItemCb.cs`, and the associated Designer/Properties files). Deleting this type would
   make their bare references unambiguous rather than break them. Recorded so F15 is not surprised.
5. `QuickFiler/Interfaces/IQfcHomeController.cs` (not compiled) would be left referencing a deleted
   type. It is not an MSBuild input, so this breaks nothing. It is unassigned by epic.md, whose
   assignments cover only the 121 compiled files. Recommend deleting it in the same change as dead
   code, or leaving it untouched — either is safe; do not silently retarget it.

**Where this belongs: F16 capstone, not F6.** Three reasons:

- It changes the compiled-file denominator that F1 defines and F16 verifies. A wave-1 child silently
  reducing the denominator would make F16's "every one of the 121 compiled files" check ambiguous.
- It edits `QuickFiler.csproj`, a shared build input that all fourteen wave-1 children compile
  against concurrently. A csproj edit inside one wave-1 child is a predictable merge-conflict surface
  at fan-in. The issue.md constraint that "`coverage.config` and shared build property files belong
  to F1" reflects the same principle.
- F6 gains nothing measurable. The file has no executable content, so it is in neither the coverage
  numerator nor the denominator. Deleting it does not move F6's per-file evidence by one line.

If the maintainer prefers minimum churn, the alternative is to keep the file, record it in F1's
ledger as `no executable content — unreferenced`, and document the latent CS0104 trap. The coverage
metric is indifferent between the two options; the argument for deletion is dead-code hygiene and
removing the CS0104 trap, not coverage.

### Open question O3 — Was #2 intended to replace #1, or vice versa?

The evidence points to #2 being the older, abandoned declaration: its member vocabulary
(`MaximizeQfcFormViewer`, `MinimizeQfcFormViewer`, no-arg `ButtonCancel_Click`/`ButtonOK_Click`)
matches the sketch in the non-compiled `QuickFiler/Notes/notes_interfaces.cs:13–24`
(`QFD_Maximize`, `QFD_Minimize`, no-arg click handlers), which reads as a design-notes file. #1
carries the current vocabulary (`MaximizeFormViewer` / `MinimizeFormViewer` on
`IFilerFormController`) and the members added by later features. Two archived feature records
(`docs/features/archive/2026-06-02-quickfiler-high-confidence-prefilter-171/spec.md:101` and
`policy-audit.2026-06-02T11-06.md:161`) show `IQfcFormController.cs` being extended with the
pre-scored `LoadItemsAsync` overload — those overloads are present in #1 and absent from #2,
confirming #1 is the maintained file. This is inference from file content, not from a recorded
decision; the plan author should treat it as supporting context for the deletion recommendation, not
as independent proof. The proof is the zero-consumer, zero-implementer result above.

### Open question O2 (repeated from `Controllers.IQfcFormController.cs.md`) — Ledger key collision

F1's ledger must be keyed by repo-relative path, not base name, because two distinct compiled files
share the base name `IQfcFormController.cs`. The same applies to `IQfcHomeController.cs`, which also
exists in both `Controllers/` and `Interfaces/` (only the `Controllers/` copy is compiled).
