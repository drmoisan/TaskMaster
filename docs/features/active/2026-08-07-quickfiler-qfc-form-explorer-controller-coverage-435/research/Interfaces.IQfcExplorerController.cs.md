# Research — `QuickFiler/Interfaces/IQfcExplorerController.cs`

Timestamp: 2026-08-07T22-40

## 1. Header

| Field | Value |
| --- | --- |
| Production file | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8220048ded06d508\QuickFiler\Interfaces\IQfcExplorerController.cs` |
| Exact line count | 15 |
| Declared namespace | `QuickFiler.Interfaces` (line 4) |
| Declared type | `public interface IQfcExplorerController` (line 6) — no base interface |
| `[ExcludeFromCodeCoverage]` | **No.** Verified by reading the entire 15-line file; it contains no attribute of any kind. |
| Compiled | **Yes** — `QuickFiler/QuickFiler.csproj` line 362: `<Compile Include="Interfaces\IQfcExplorerController.cs" />` |
| Feature child | F6 (issue #435), epic #136 |

Current numeric per-file line coverage is **unmeasured**, and will remain undefined regardless of
testing because the file has no executable lines (§2).

### Declared member set (verified line by line)

| Line | Member |
| --- | --- |
| 8 | `bool BlShowInConversations { get; set; }` |
| 9 | `Task OpenQFItem(MailItem mailItem)` |
| 10 | `void ExplConvView_ToggleOff()` |
| 11 | `void ExplConvView_ToggleOn()` |
| 12 | `void ExplConvView_Cleanup()` |
| 13 | `void ExplConvView_ReturnState()` |

Six members. Referenced-type resolution from this file's own directives
(`using System.Threading.Tasks;` line 1 and `using Microsoft.Office.Interop.Outlook;` line 2, both
at compilation-unit scope; `namespace QuickFiler.Interfaces` opens at line 4): `Task` →
`System.Threading.Tasks`; `MailItem` → `Microsoft.Office.Interop.Outlook`.

**No name collision.** Unlike `IQfcFormController`, only one compiled file declares
`IQfcExplorerController`. The only other declaration in the working tree is
`QuickFiler/Notes/notes_interfaces.cs:52` (`namespace QuickFiler.Notes`), and the csproj contains
no `Notes\` `<Compile>` entry, so it is not a build input. There is therefore no CS0104 hazard for
this name.

---

## 2. Executable-content determination

**The file contains no executable statement of any kind.** Verified by reading all 15 lines:

- Lines 1–2 are `using` directives.
- Line 4 opens `namespace QuickFiler.Interfaces`; line 6 opens the interface declaration.
- Line 8 is a property signature with a bare `{ get; set; }` accessor list — no accessor bodies,
  no expression-bodied member, no initializer.
- Lines 9–13 are method signatures terminated by `;` — no bodies. .NET Framework 4.8 does not
  support default interface members, and none is present regardless.
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

Do **not** add executable code to this interface file to manufacture coverage. The coverage
obligation for this cluster belongs to the implementation, `QuickFiler/Controllers/QfcExplorerController.cs`
(323 lines, currently `[ExcludeFromCodeCoverage]` at line 20), which is the file F6's acceptance
criteria actually target.

The `Coverage Exclusion Policy` prohibition in the same rule file (lines 31–46) targets files with
real executable lines being hidden from the metric. A file with zero executable lines changes
neither numerator nor denominator, so recording it as `no executable content` in F1's ledger — as
opposed to adding it to a `coverage.config` exclude list — keeps the two rules consistent.

---

## 3. Consumer map

`QuickFiler/QuickFiler.csproj` was read directly (the `<Compile>` item group spans lines 289–461)
to determine compiled status for every entry below. Every consumer listed resolves the bare name
`IQfcExplorerController` unambiguously: the name exists in exactly one compiled namespace
(`QuickFiler.Interfaces`), so any file with `using QuickFiler.Interfaces;` reaches it and no other
candidate exists.

### Implementers

| Implementer | File : line | Compiled | Owning child |
| --- | --- | --- | --- |
| `QfcExplorerController` | `QuickFiler/Controllers/QfcExplorerController.cs:21` — `internal class QfcExplorerController : IQfcExplorerController` (with `[ExcludeFromCodeCoverage]` at line 20) | Yes (csproj 316) | **F6 (this child)** |

`QfcExplorerController` is the **only** implementer in the repository.

### Production consumers

| Consumer | File : line | What it does | Compiled | Owning child |
| --- | --- | --- | --- | --- |
| `IFilerHomeController` | `QuickFiler/Interfaces/IFilerHomeController.cs:30` | `IQfcExplorerController ExplorerController { get; set; }` — puts this type on the home-controller contract | Yes (csproj 357) | **F7** |
| `QfcHomeController` | `QuickFiler/Controllers/QfcHomeController.cs:179–182` | Final type argument of the `internal Func<...>` factory property `QfcExplorerControllerLoader`, whose default lambda constructs `new QfcExplorerController(initType, globals, homeController)` | Yes (csproj 325) | **F7** |
| `QfcHomeController` | `QuickFiler/Controllers/QfcHomeController.cs:92, 137` | `_explorerController = QfcExplorerControllerLoader(InitTypeEnum.Sort, Globals, this);` | Yes (csproj 325) | **F7** |
| `QfcHomeController` | `QuickFiler/Controllers/QfcHomeController.cs:408–413` | `private IQfcExplorerController _explorerController;` backing `public IQfcExplorerController ExplorerController { get; set; }` | Yes (csproj 325) | **F7** |
| `EfcHomeController` | `QuickFiler/Controllers/EfcHomeController.cs:356–357` | Field + property, same shape | Yes (csproj 295) | **F8** |
| `EfcHomeControllerDependencies` | `QuickFiler/Controllers/EfcHomeControllerDependencies.cs:56, 110, 209, 223, 231` | Factory-delegate type argument; `CreateExplorerController`; `internal static ... CreateExplorerControllerWithFactory` | Yes (csproj 300) | **F8** |
| `EfcHomeControllerDependencyFactories` | `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:59, 66, 149, 155, 209` | Factory-delegate type arguments; `CreateProductionExplorerControllerInstance` returns `new QfcExplorerController(initType, globals, homeController)` | Yes (csproj 299) | **F8** |
| `QfcItemController` | `QuickFiler/Controllers/QfcItemController.cs:45` | `private IQfcExplorerController _explorerController;` | Yes (csproj 328) | **F10** |
| `EfcItemController` | `QuickFiler/Controllers/EfcItemController.cs:372` | `private IQfcExplorerController _explorerController;` | Yes (csproj 301) | **F9** |

### Test consumers

| Fixture | File : line | Compiled | Owning child |
| --- | --- | --- | --- |
| `QfcHomeControllerTests` | `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:125–126, 196–197` | Yes (test csproj 131) | F7 |
| `QfcHomeControllerPropertyTests` | `.../QfcHomeControllerPropertyTests.cs:123` | Yes (test csproj 128) | F7 |
| `QfcItemController.InitializationTests` | `.../QfcItemController.InitializationTests.cs:26, 31, 46, 79, 114, 147` | Yes (test csproj 141) | F10 |
| `EfcHomeControllerSeamTests` | `.../EfcHomeControllerSeamTests.cs:235` | Yes (test csproj 111) | F8 |
| `EfcHomeControllerLifecycleTests` | `.../EfcHomeControllerLifecycleTests.cs:178, 306` | Yes (test csproj 105) | F8 |
| `EfcHomeControllerDependenciesTests` | `.../EfcHomeControllerDependenciesTests.cs:43, 71, 227` | Yes (test csproj 102) | F8 |
| `EfcHomeControllerDependenciesProductionFactoryTests` | `.../EfcHomeControllerDependenciesProductionFactoryTests.cs:401, 438, 466` | Yes (test csproj 103) | F8 |

### Textual matches that are NOT compiled

| File : line | Compiled |
| --- | --- |
| `QuickFiler/Interfaces/IQfcHomeController.cs:8` — `IQfcExplorerController ExplCtrlr { get; set; }` | **No** — absent from the csproj `Interfaces\` block (lines 355–368) |
| `QuickFiler/Notes/notes_interfaces.cs:5, 52` — declares a separate `QuickFiler.Notes.IQfcExplorerController` with a different member set (`void OpenQFMail(MailItem)` rather than `Task OpenQFItem(MailItem)`) | **No** — the csproj contains no `Notes\` `<Compile>` entry |

**Ownership consequence — this is the widest-reaching interface in the F6 set.** Its consumers span
**four other children**: F7 (`QfcHomeController`, `IFilerHomeController`), F8 (the entire EFC
home-controller dependency-factory family), F9 (`EfcItemController`), and F10 (`QfcItemController`).
Six mock sites across F7/F8/F10 test fixtures already bind to it with `MockBehavior.Strict` in two
places (`EfcHomeControllerDependenciesTests.cs:43, 227`), which means any **added** member is
tolerated but any **removed or renamed** member breaks those fixtures immediately. Treat this
interface as the most change-hostile file in the F6 set.

---

## 4. Contract-stability assessment

**Assessment: F6's `QfcExplorerController` seam extraction can and should complete without adding,
removing, or renaming any member of this interface. Recommend treating the six members as frozen.**

### What actually blocks coverage of `QfcExplorerController.cs`

The file was read in full (323 lines). Its testability barriers are all *inside* the class, not on
the interface:

1. **Constructor COM call.** `QfcExplorerController.cs:35` —
   `_activeExplorer = _globals.Ol.App.ActiveExplorer();` — executes live Outlook interop during
   construction. Until this is seamed, no test can construct the type at all, so none of the six
   interface members is reachable.
2. **Direct `Explorer` / `Outlook.View` / `MAPIFolder` interop throughout.** `ExplConvView_ToggleOff`
   (lines 72–106) drives `_activeExplorer.CommandBars.GetPressedMso`, `CurrentView`, `View.XML`,
   `View.Save()`, `View.Apply()`, `View.Copy(...)`. `ExplConvView_ToggleOn` (123–131) drives
   `CurrentFolder.Views[...]`. `NavigateToOutlookFolder` (133–143) drives `CurrentFolder.FolderPath`
   and assigns `CurrentFolder`. `OpenQFItem` (146–181) drives `IsItemSelectableInView`,
   `ClearSelection`, `AddToSelection`, `mailItem.Display()`.
3. **A modal popup.** `QfcExplorerController.cs:168–173` calls `MessageBox.Show(...)` on the
   not-selectable-in-view path. Epic.md "Shared Design" §2 states that a popup requiring human
   interaction is a unit-test-policy violation. This path cannot be covered without a prompt seam.
4. **`ExplConvView_Cleanup()` (lines 61–64) throws `NotImplementedException`.** It is an interface
   member with a deliberate not-implemented body. Its only honest test is a negative one asserting
   the throw; changing it to a no-op would be a behavior change and violates the epic NFR "No
   behavior change to end-user QuickFiler flows".
5. **Already-pure statics that need no seam at all.** `StripTabsCrLf` (internal static, lines
   203–213) and `GetSiblingView` (public, lines 108–121) are the cheapest coverage in the file.
   `GetSiblingView` takes `Outlook.View` parameters but only reads `.Parent` and `.Name`, both of
   which Moq can supply from an interop interface mock — the same technique already used repo-wide
   for `MailItem` (`new Mock<MailItem>().Object` at `QfcHomeControllerRunAsyncTests.cs:184`).

### Why none of that requires an interface change

- Barrier 1 is solved by an **injectable-delegate or extra-parameter seam on the concrete class**,
  not on the interface. Repo precedent is directly applicable: the assembly declares
  `[assembly: InternalsVisibleTo("QuickFiler.Test")]` twice
  (`QuickFiler/Properties/AssemblyInfo.cs:5` and `QuickFiler/Controllers/QfcHomeController.cs:18`),
  so an `internal` constructor overload or an `internal Func<IApplicationGlobals, Explorer>`
  resolver on `QfcExplorerController` is directly visible to `QuickFiler.Test`. This is exactly the
  shape `QfcHomeController.cs:179–182` already uses for `QfcExplorerControllerLoader`. Per
  `.claude/rules/csharp.md` "DI Seams", an injectable delegate is the correct tier here because
  only one call path is involved.
- Barriers 2 and 5 are solved by mocking the interop interfaces that are already reachable as
  constructor/method inputs once barrier 1 is seamed. No new interface member is needed.
- Barrier 3 needs a narrow prompt seam. **Note that `IUserPrompt` is NOT reusable here**:
  it is declared at `Tags/IUserPrompt.cs:10` in the `Tags` project, and `QuickFiler.csproj`'s
  `<ProjectReference>` list (lines 464–479) contains only `SVGControl`, `TaskVisualization`,
  `ToDoModel`, and `UtilitiesCS` — not `Tags`. F6 must introduce its own narrow prompt seam
  (an `internal Func<string, string, DialogResult>` on the concrete class is the smallest form) or
  route the request to add one to `UtilitiesCS`. Adding a project reference to `Tags` purely for a
  seam would be disproportionate and is not recommended. The `Tags` implementation
  (`Tags/WinFormsUserPrompt.cs:15`, `public class WinFormsUserPrompt : IUserPrompt`) is useful as a
  *pattern* precedent, not as a dependency.
- Barrier 4 needs no seam.

### If growth nevertheless proves necessary

Any member added here forces an edit to `QfcExplorerController.cs` (F6-owned, so no cross-child
cost) but is also visible to every consumer listed in §3. An **additive** member is source-compatible
for all of them, because every consumer uses the type as a field/property/factory-return type rather
than implementing it — `QfcExplorerController` is the sole implementer. A **removal or rename** is
not source-compatible and would break F7's `IFilerHomeController.cs:30`, F8's five factory sites,
F9's and F10's fields, and seven test fixtures.

**CROSS-CHILD CONTRACT NOTE (F7, F8, F9, F10):** if F6 concludes it must change this interface's
member set, the change must be additive only, and F7 (`IFilerHomeController.cs:30`,
`QfcHomeController.cs:179–182, 408–413`), F8 (`EfcHomeController.cs:356–357`,
`EfcHomeControllerDependencies.cs:56, 110, 209, 223, 231`,
`EfcHomeControllerDependencyFactories.cs:59, 66, 149, 209`), F9 (`EfcItemController.cs:372`), and
F10 (`QfcItemController.cs:45`) must each be notified so their own mocks and factory signatures are
re-verified. This artifact's recommendation is that no such change be made.

**Note on `[ExcludeFromCodeCoverage]` scope.** The attribute at `QfcExplorerController.cs:20` is on
the *implementation*, not on this interface. Removing it (F6 acceptance criterion 2 in issue.md)
requires no edit to this file.

---

## 5. Proposed test cases

**None. The file has no executable content, so there is nothing to execute in a test.**

Stated plainly: do not author reflection-based tests asserting that this interface declares
`OpenQFItem` or that `QfcExplorerController` implements it. The compiler already enforces the
latter at `QfcExplorerController.cs:21` (a missing member is CS0535). Such tests execute only
test-assembly code, add zero lines to the production coverage numerator, and satisfy no clause of
the coverage policy.

The behavior described by this interface is covered legitimately by tests against the
implementation. Those tests are the subject of the sibling research artifact
`QfcExplorerController.cs.md` and their coverage is attributed to
`QuickFiler/Controllers/QfcExplorerController.cs`, not to this file. Nothing in this artifact
duplicates that test enumeration.

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

One item to hand to F1: the paired implementation `QuickFiler/Controllers/QfcExplorerController.cs`
carries `[ExcludeFromCodeCoverage]` at line 20 and is one of the 33 attributes F1 must dispose of.
Per epic.md "Shared Design" §1, that attribute is treated as unratified. This artifact's §4 shows
the barriers are seamable (constructor COM call, direct interop, a `MessageBox.Show`), which argues
against ratifying the exemption and for removing the attribute. The ledger, not F6, makes that call.

---

## 7. Open questions / findings

### Finding E1 — This is the highest cross-child blast radius in the F6 set

Four sibling children (F7, F8, F9, F10) and seven of their test fixtures consume this interface,
which is more consumers than any other file assigned to F6. The plan author should treat it as
frozen and should not schedule any task that edits it without an explicit cross-child notification
step. §4 gives the exact consumer list and line numbers for that notification.

### Open question E2 — Prompt seam ownership for `MessageBox.Show`

`QfcExplorerController.cs:168` is the only modal popup in the F6 file set. Three options, in
increasing cost:

1. An `internal Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>` seam on
   `QfcExplorerController`, defaulting to `MessageBox.Show`. Smallest change; fully inside F6;
   consistent with `.claude/rules/csharp.md` "DI Seams" tier 2. **Recommended.**
2. A new `IUserPrompt`-shaped interface added to `UtilitiesCS` and consumed by QuickFiler. Larger
   surface, cross-project, and `UtilitiesCS` is outside the epic's file assignments entirely.
3. Adding a `<ProjectReference>` to `Tags` to reuse `Tags/IUserPrompt.cs:10`. Not recommended —
   `QuickFiler.csproj` lines 464–479 show no such reference today, and introducing one for a seam
   is disproportionate.

Decision belongs to the plan author. Recorded here because the choice determines whether F6 stays
inside its assigned file set.

### Open question E3 — `ExplConvView_Cleanup()` throws `NotImplementedException`

`QfcExplorerController.cs:61–64` implements this interface member as an unconditional throw, with a
`//PRIORITY: Implement ExplConvView_Cleanup` comment at line 60. The honest coverage treatment is a
negative test asserting `NotImplementedException`. Implementing the member instead would be a
behavior change and is outside F6's stated non-goal set (issue.md: "No behavior change to observable
QuickFiler flows"). Recommend the negative test and, if the missing implementation matters, a
separate issue per the CLAUDE.md bugfix-workflow rule "If you uncover deeper design problems, open a
new issue instead of widening scope."

### Non-finding — no duplicate declaration, no CS0104 hazard

Unlike `IQfcFormController`, this name has exactly one compiled declaration. The only other
declaration, `QuickFiler/Notes/notes_interfaces.cs:52`, is in `namespace QuickFiler.Notes`, has a
different member set (`void OpenQFMail(MailItem)` instead of `Task OpenQFItem(MailItem)`), and is not
compiled. Recorded explicitly so the plan author does not have to re-derive it after reading
Finding D1 in the `IQfcFormController` artifacts.
