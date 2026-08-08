# Research — `QuickFiler/Interfaces/IFilerFormController.cs`

Timestamp: 2026-08-07T22-40

## 1. Header

| Field | Value |
| --- | --- |
| Production file | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8220048ded06d508\QuickFiler\Interfaces\IFilerFormController.cs` |
| Exact line count | 25 |
| Declared namespace | `QuickFiler.Interfaces` (line 7) |
| Declared type | `public interface IFilerFormController` (line 9) — no base interface |
| `[ExcludeFromCodeCoverage]` | **No.** Verified by reading the entire 25-line file; it contains no attribute of any kind. |
| Compiled | **Yes** — `QuickFiler/QuickFiler.csproj` line 356: `<Compile Include="Interfaces\IFilerFormController.cs" />` |
| Feature child | F6 (issue #435), epic #136 |

Current numeric per-file line coverage is **unmeasured**, and will remain undefined regardless of
testing because the file has no executable lines (§2).

### Declared member set (verified line by line)

| Line | Member |
| --- | --- |
| 11 | `Task ActionCancelAsync()` |
| 12 | `Task ActionOkAsync()` |
| 13 | `void ButtonCancel_Click(object sender, EventArgs e)` |
| 14 | `void ButtonOK_Click(object sender, EventArgs e)` |
| 15 | `void Cleanup()` |
| 16 | `void MaximizeFormViewer()` |
| 17 | `void MinimizeFormViewer()` |
| 18 | `void ToggleOffNavigation(bool async)` |
| 19 | `Task ToggleOffNavigationAsync()` |
| 20 | `void ToggleOnNavigation(bool async)` |
| 21 | `Task ToggleOnNavigationAsync()` |
| 23 | `IntPtr FormHandle { get; }` |

Twelve members: eleven methods and one get-only property. This is the **shared** form-controller
contract abstracting over both the QuickFiler (`Qfc`) and Email-Filer (`Efc`) controller families —
the two `Toggle*Navigation` pairs and the `Maximize`/`MinimizeFormViewer` pair are the members both
families genuinely have in common.

### Referenced-type resolution

The `using` directives sit at compilation-unit scope, lines 1–5, ahead of
`namespace QuickFiler.Interfaces` at line 7. `IntPtr` (line 23) and `EventArgs` (lines 13–14)
resolve via `using System;` (line 1); `Task` (lines 11, 12, 19, 21) via
`using System.Threading.Tasks;` (line 3).

**Minor observation:** three of the five `using` directives are unreferenced by any declaration in
the file — `System.Collections.Generic` (line 2), `Microsoft.Office.Interop.Outlook` (line 4), and
`UtilitiesCS.Interfaces.IWinForm` (line 5). No type in the file comes from any of them. This is a
cosmetic IDE0005 candidate, not a defect, and the build passes today. See Open question F3.

---

## 2. Executable-content determination

**The file contains no executable statement of any kind.** Verified by reading all 25 lines:

- Lines 1–5 are `using` directives.
- Line 7 opens `namespace QuickFiler.Interfaces`; line 9 opens the interface declaration.
- Lines 11–21 are method signatures terminated by `;` — no bodies. .NET Framework 4.8 does not
  support default interface members, and none is present regardless.
- Line 23 is a property signature with a bare `{ get; }` accessor list — no accessor body, no
  expression-bodied member, no initializer.
- There is no static constructor, no field, no constant, no attribute expression, no event.

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

Do **not** add executable code to this interface file to manufacture coverage.

The `Coverage Exclusion Policy` prohibition in the same rule file (lines 31–46) targets files with
real executable lines being hidden from the metric. A file with zero executable lines changes
neither numerator nor denominator, so recording it as `no executable content` in F1's ledger — as
opposed to adding it to a `coverage.config` exclude list — keeps the two rules consistent.

---

## 3. Consumer map

`QuickFiler/QuickFiler.csproj` was read directly (the `<Compile>` item group spans lines 289–461)
to determine compiled status for every entry below. The name `IFilerFormController` is declared in
exactly one place repository-wide, so every reference resolves unambiguously and there is no CS0104
hazard for this name.

### Implementers

| Implementer | File : line | Kind | Compiled | Owning child |
| --- | --- | --- | --- | --- |
| `QuickFiler.Controllers.IQfcFormController` | `QuickFiler/Controllers/IQfcFormController.cs:13` — `public interface IQfcFormController : IFilerFormController` | Interface inheritance | Yes (csproj 303) | **F6 (this child)** |
| `QfcFormController` | `QuickFiler/Controllers/QfcFormController.cs:19` | Concrete, **transitively** via `IQfcFormController` | Yes (csproj 317; partials 318–320) | **F6 (this child)** |
| `EfcFormController` | `QuickFiler/Controllers/EfcFormController.cs:28` — `internal class EfcFormController : IFilerFormController`, carrying `[ExcludeFromCodeCoverage]` at line 27 | Concrete, **directly** | Yes (csproj 294) | **F9 — MUST NOT be edited by this child** |

**Two independent concrete implementers in two different children.** This is the single most
important structural fact about this file, and it is what makes it the most change-hostile
declaration in the F6 set. See §4.

The `QfcFormController` implementations of each member were located directly:
`ActionCancelAsync` at `QfcFormController.EventHandlers.cs:84`; `ActionOkAsync` at `:110`;
`ButtonCancel_Click(object, EventArgs)` at `:70`; `ButtonOK_Click(object, EventArgs)` at `:96`;
`Cleanup()` at `QfcFormController.SetupDisposal.cs:208`; `MaximizeFormViewer()` at
`QfcFormController.Actions.cs:187`; `MinimizeFormViewer()` at `:197`; `ToggleOffNavigation` /
`ToggleOffNavigationAsync` / `ToggleOnNavigation` / `ToggleOnNavigationAsync` at
`QfcFormController.cs:174–180`; `FormHandle` at `QfcFormController.cs:163`.

### Production consumers

| Consumer | File : line | What it does | Compiled | Owning child |
| --- | --- | --- | --- | --- |
| `IQfcFormViewer` | `QuickFiler/Interfaces/IQfcFormViewer.cs:20` | `void SetController(IFilerFormController controller);` — the viewer accepts the **base** contract, not `IQfcFormController` | Yes (csproj 364) | **F6 (this child)** |
| `QfcExplorerController` | `QuickFiler/Controllers/QfcExplorerController.cs:148` | `_parent.FormController.MinimizeFormViewer();` — the only **member invocation** on this interface inside F6 | Yes (csproj 316) | **F6 (this child)** |
| `IFilerHomeController` | `QuickFiler/Interfaces/IFilerHomeController.cs:31` | `IFilerFormController FormController { get; }` | Yes (csproj 357) | **F7** |
| `QfcHomeController` | `QuickFiler/Controllers/QfcHomeController.cs:416–419` | `public IFilerFormController FormController { get => _formController; }` over a field typed `IQfcFormController` | Yes (csproj 325) | **F7** |
| `EfcHomeController` | `QuickFiler/Controllers/EfcHomeController.cs:364` | `public IFilerFormController FormController` | Yes (csproj 295) | **F8** |
| `QfcCollectionController` | `QuickFiler/Controllers/QfcCollectionController.cs:35` (ctor param `IFilerFormController parent`), `:64` (field `_parent`) | Yes (csproj 311) | **F11** |
| `QfcFormViewer` | `QuickFiler/Viewers/QfcFormViewer.cs:31` (field), `:46` (`public virtual void SetController(IFilerFormController controller)`) | Yes (csproj 444) | **F15 — MUST NOT be edited by this child** |
| `QfcItemController` | `QuickFiler/Controllers/QfcItemController.cs:47` | Commented out (`//private IFilerFormController _formController;`) — inert | Yes (csproj 328) | F10 |

### Test consumers

| Fixture | File : line | Compiled | Owning child |
| --- | --- | --- | --- |
| `QfcFormControllerTests` | `QuickFiler.Test/Controllers/QfcFormControllerTests.cs:31, 105, 106, 159` | Yes (test csproj 117) | **F6** |
| `QfcItemController.SeamFactoryTests` | `.../QfcItemController.SeamFactoryTests.cs:80` | Yes (test csproj 147) | F10 |
| `QfcItemController.EventHandlersTests` | `.../QfcItemController.EventHandlersTests.cs:289` | Yes (test csproj 143) | F10 |
| `QfcCollectionControllerDarkModeTests` | `.../QfcCollectionControllerDarkModeTests.cs:45` | Yes (test csproj 113) | F11 |
| `QfcHomeControllerTests` | `.../QfcHomeControllerTests.cs:259, 275` | Yes (test csproj 131) — but both lines are commented out | F7 (inert) |
| `QfcViewer_Test` | `.../QfcViewer_Test.cs:20, 30` | Yes (test csproj 167) — both lines commented out | inert |

### The `SetController` design detail worth recording

`IQfcFormViewer.SetController` (line 20 of that file) takes `IFilerFormController`, not
`IQfcFormController`. `QfcFormController.cs:44` passes `this`, which up-converts. The F6 fixture
`QfcFormControllerTests.cs:104–106` captures that argument via a Moq callback typed
`Callback<IFilerFormController>` and asserts it at line 159 with
`Assert.AreEqual((IFilerFormController)controller, _filerFormController)`. That single assertion is
also the compile-time proof that `QfcFormController` implements the `QuickFiler.Controllers`
variant of `IQfcFormController` rather than the `QuickFiler.Interfaces` variant — see Finding D1 in
`Controllers.IQfcFormController.cs.md`, step (a).

---

## 4. Contract-stability assessment

**Assessment: freeze. This is the least safe file in the F6 set to modify, and F6's seam work does
not need it modified.**

### Why it is the least safe

Adding a member to this interface forces an implementation in **both** concrete implementers,
because .NET Framework 4.8 has no default interface members:

1. `QfcFormController` — F6-owned, cheap.
2. `EfcFormController` (`QuickFiler/Controllers/EfcFormController.cs:28`) — **F9-owned**, 1,086
   lines, carrying `[ExcludeFromCodeCoverage]` at line 27, and named in epic.md as part of the
   "heaviest seam-extraction child" which also breaches the 500-line rule. F6 must not edit it.

It would additionally ripple to `IQfcFormViewer.SetController` consumers and to the six production
consumers in §3 spanning F7, F8, F11, and F15.

### Why F6 does not need it modified

F6 touches this interface in exactly two places, both read-only in contract terms:

- As the base of `Controllers/IQfcFormController.cs:13`. F6 owns that derived interface. **If F6
  needs a new controller-side member, it belongs on `QuickFiler.Controllers.IQfcFormController`,
  which has exactly one implementer (`QfcFormController`, F6-owned), not on
  `IFilerFormController`, which has two implementers in two children.** That is the recommended
  growth path and it costs zero cross-child coordination.
- `QfcExplorerController.cs:148` invokes `MinimizeFormViewer()` through
  `_parent.FormController`. That member already exists (line 16). The testability barrier at that
  call site is not the interface — it is that `_parent` is an `IFilerHomeController`
  (`QfcExplorerController.cs:30, 41`), which Moq can already supply, so the call is directly
  mockable once the constructor's COM call at `QfcExplorerController.cs:35` is seamed. See the
  `Interfaces.IQfcExplorerController.cs.md` artifact §4 for that seam analysis.

Even the smaller-scope option — moving a member *up* from `Controllers/IQfcFormController.cs` into
this file — is not needed and is not recommended, because it would newly obligate `EfcFormController`
(F9) to implement it.

### If growth nevertheless proves necessary

**CROSS-CHILD CONTRACT NOTE (F9).** Adding any member `M` to
`QuickFiler/Interfaces/IFilerFormController.cs` requires a corresponding implementation in
`QuickFiler/Controllers/EfcFormController.cs` (the second concrete implementer, at line 28), which
is owned by **F9** and must not be edited by this child. The required F9 edit would be a new member
body in that 1,086-line class. F6 cannot land such a change alone; the member name and the required
`EfcFormController.cs` edit must be recorded in `spec.md` as a cross-child contract note and F9
notified before either child merges.

**CROSS-CHILD CONTRACT NOTE (F15).** `QuickFiler/Viewers/QfcFormViewer.cs:46` implements
`SetController(IFilerFormController)`. Any change to *that* member's signature — as distinct from
adding an unrelated member — would force an F15 edit. Adding an unrelated member does not.

Additional consumers to re-verify on any change: F7 (`IFilerHomeController.cs:31`,
`QfcHomeController.cs:416`), F8 (`EfcHomeController.cs:364`), F11 (`QfcCollectionController.cs:35,
64`), and four live test fixtures.

**Removals and renames are strictly off the table.** A removal would break both implementers, six
production consumers across five children, and four fixtures simultaneously, during a wave in which
all fourteen children build in parallel.

---

## 5. Proposed test cases

**None. The file has no executable content, so there is nothing to execute in a test.**

Stated plainly: do not author reflection-based tests asserting that this interface declares
`MinimizeFormViewer`, that it has twelve members, or that `QfcFormController` and
`EfcFormController` both implement it. The compiler already enforces the last of those at
`QfcFormController.cs:19` and `EfcFormController.cs:28` (a missing member is CS0535). Reflection
tests over a declaration file execute only test-assembly code, add zero lines to the production
coverage numerator, and satisfy no clause of the coverage policy.

The behavior described by these members is covered legitimately through the implementations. The
`QfcFormController` side is exercised by `QuickFiler.Test/Controllers/QfcFormControllerTests.cs`
and `QfcFormControllerSeamTests.cs` (both F6-owned), whose coverage is attributed to the
`QfcFormController.*` partials. The `EfcFormController` side is F9's responsibility.

The one compile-time property worth guarding — that a `QfcFormController` instance is usable where
an `IFilerFormController` is expected — is already guarded by
`QfcFormControllerTests.cs:159`, `Assert.AreEqual((IFilerFormController)controller,
_filerFormController)`, which is an existing F6-owned assertion. No further test is warranted.

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

One item to hand to F1: `EfcFormController.cs:27` carries `[ExcludeFromCodeCoverage]` and is one of
the 33 attributes F1 must dispose of. It is **F9's** file. F6 has an indirect interest only —
recorded here because this interface is the shared contract between the two children's controllers,
so a decision to seam `EfcFormController` may surface a request to widen `IFilerFormController`.
Per §4, F6's position is that such a widening should be resisted in favour of putting new members
on the family-specific derived interfaces.

---

## 7. Open questions / findings

### Finding F1a — Two implementers in two children makes this the most change-hostile file in the F6 set

`QfcFormController` (F6) and `EfcFormController` (F9) both implement it directly or transitively.
The plan author should schedule **zero** tasks that edit this file, and should route any new
controller-side member to `QuickFiler/Controllers/IQfcFormController.cs` (one implementer, F6-owned)
instead. §4 gives the full consumer list and line numbers for the escalation path if that proves
impossible.

### Finding F1b — `IQfcFormViewer.SetController` deliberately takes the base type

`IQfcFormViewer.cs:20` and `QfcFormViewer.cs:46` both use `IFilerFormController`, not
`IQfcFormController`. This is what lets the same viewer abstraction serve both controller families,
and it is load-bearing for the namespace-resolution proof in Finding D1 of
`Controllers.IQfcFormController.cs.md`. Do not "tighten" the parameter type to `IQfcFormController`
as a convenience: it would break `EfcFormController`'s ability to be set as a controller and would
require an F15 edit to `QfcFormViewer.cs:46`.

### Open question F3 — Three unreferenced `using` directives

Lines 2 (`System.Collections.Generic`), 4 (`Microsoft.Office.Interop.Outlook`), and 5
(`UtilitiesCS.Interfaces.IWinForm`) are not used by any declaration in the file. Removing them is
zero-risk, produces no behavior change, adds no executable content, and is consistent with
`CLAUDE.md` §C#5.3 ("Prefer explicit `using` directives at file scope"). It is also strictly
optional: it gains no coverage, and every edit to a file compiled by `QuickFiler.csproj` during a
fourteen-way parallel wave is a small merge-conflict surface.

**Recommendation: leave them.** The plan author may include it as a cosmetic task if the plan is
already editing this file for another reason, which on the analysis in §4 it should not be. If it is
done, it must be done with `csharpier` and the full toolchain per `CLAUDE.md` §CUT3, not by hand.

### Non-finding — no duplicate declaration, no CS0104 hazard

`IFilerFormController` is declared in exactly one place repository-wide
(`QuickFiler/Interfaces/IFilerFormController.cs:9`). Unlike `IQfcFormController` there is no second
declaration in `Controllers/`, none in `Notes/`, and none in any non-compiled file. Recorded
explicitly so the plan author does not have to re-derive it after reading Finding D1 in the
`IQfcFormController` artifacts.
