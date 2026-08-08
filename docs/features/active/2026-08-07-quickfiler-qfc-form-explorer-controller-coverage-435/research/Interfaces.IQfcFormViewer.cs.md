# Research — `QuickFiler/Interfaces/IQfcFormViewer.cs`

Timestamp: 2026-08-07T22-40

## 1. Header

| Field | Value |
| --- | --- |
| Production file | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8220048ded06d508\QuickFiler\Interfaces\IQfcFormViewer.cs` |
| Exact line count | 51 |
| Declared namespace | **`QuickFiler`** (line 10) — **not** `QuickFiler.Interfaces`, despite the file living in the `Interfaces/` folder. See §3, "Namespace/folder mismatch". |
| Declared type | `public interface IQfcFormViewer : IForm` (line 12) |
| Base interface | `UtilitiesCS.Interfaces.IWinForm.IForm` — resolved below |
| `[ExcludeFromCodeCoverage]` | **No.** Verified by reading the entire 51-line file; it contains no attribute of any kind. (The *implementer*, `QuickFiler/Viewers/QfcFormViewer.cs`, does carry the attribute at line 17 — that is F15's file, not this one.) |
| Compiled | **Yes** — `QuickFiler/QuickFiler.csproj` line 364: `<Compile Include="Interfaces\IQfcFormViewer.cs" />` |
| Feature child | F6 (issue #435), epic #136 |

Current numeric per-file line coverage is **unmeasured**, and will remain undefined regardless of
testing because the file has no executable lines (§2).

### Base-interface resolution

The `using` directives sit at compilation-unit scope, lines 1–8, ahead of `namespace QuickFiler`
at line 10. Lookup for the bare name `IForm` at line 12 walks `N = QuickFiler` (no such type) →
global, where the compilation-unit `using` set decides. `using UtilitiesCS.Interfaces.IWinForm;`
(line 8) supplies `UtilitiesCS.Interfaces.IWinForm.IForm`. A repository-wide search for
`interface IForm` returns exactly one file — `UtilitiesCS/Interfaces/IWinForm/IForm.cs:8` — so the
match is unique and no CS0104 arises even though `QuickFiler.Interfaces` (line 6) and `UtilitiesCS`
(line 7) are also imported.

`IForm` was read in full (88 lines). It declares `IForm : IContainerControl, IScrollableControl`
(line 8) and contributes 40 properties, 23 events, and 14 methods, including `WindowState`
(line 49), `ShowDialog()` (line 82), `Activate()` (line 74), and `Close()` (line 76). The
`Show()` / `Hide()` / `Refresh()` / `Invoke(...)` / `Controls` / `Size` / `ClientSize` / `Handle`
members that `QfcFormController` uses arrive through the `IContainerControl` / `IScrollableControl`
chain. `IForm` is owned by `UtilitiesCS`, which is **outside this epic's file assignments
entirely** — epic.md assigns only the 121 files compiled by `QuickFiler.csproj`.

---

## 2. Executable-content determination

**The file contains no executable statement of any kind.** Verified by reading all 51 lines:

- Lines 1–8 are `using` directives.
- Line 10 opens `namespace QuickFiler`; line 12 opens the interface declaration.
- Lines 14–18, 24–26, 34, 43–44, 47, 49 are property signatures with bare `{ get; }` or
  `{ get; set; }` accessor lists — no accessor bodies, no expression-bodied members, no initializers.
- Lines 20–21, 29, 32–33 are method signatures terminated by `;` — no bodies. .NET Framework 4.8
  does not support default interface members, and none is present regardless.
- Lines 37–40 and 48 are field-like event declarations **inside an interface**. An interface
  event declaration emits only `add`/`remove` accessor *metadata*; it has no accessor body and
  therefore no sequence point. (The corresponding *implementations* at
  `QuickFiler/Viewers/QfcFormViewer.cs:128–147, 167–171` do have executable `add`/`remove` bodies,
  but those lines belong to that file, which is F15's.)
- Lines 23, 28, 31, 36, 42, 46 are comments.
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

Do **not** add executable code to this interface file to manufacture coverage.

The `Coverage Exclusion Policy` prohibition in the same rule file (lines 31–46) targets files with
real executable lines being hidden from the metric. A file with zero executable lines changes
neither numerator nor denominator, so recording it as `no executable content` in F1's ledger — as
opposed to adding it to a `coverage.config` exclude list — keeps the two rules consistent.

---

## 3. Consumer map

`QuickFiler/QuickFiler.csproj` was read directly (the `<Compile>` item group spans lines 289–461)
to determine compiled status for every entry below.

### Namespace/folder mismatch — decisive for resolution

The file is at `QuickFiler/Interfaces/IQfcFormViewer.cs` but declares `namespace QuickFiler`
(line 10). Every consumer below is inside the `QuickFiler.*` namespace tree, so the bare name
resolves at the `N = QuickFiler` step of the enclosing-namespace walk — **no `using` directive
imports this type at any consumer site**. Concretely:

- Consumers in `namespace QuickFiler.Controllers` (`QfcFormController`, `QfcHomeController`,
  `QfcCollectionController`, `KeyboardHandler`): `N = QuickFiler.Controllers` has no such type →
  `N = QuickFiler` supplies it.
- Consumers in `namespace QuickFiler.Controllers.Tests` (all test fixtures): two steps out to
  `N = QuickFiler`.
- The implementer in `namespace QuickFiler` (`QfcFormViewer`): step 1.

Consequence to record: a future consumer placed **outside** the `QuickFiler.*` tree would need an
explicit `using QuickFiler;`, which is not the directive a reader would guess from the file's
folder. This is a latent readability trap, not a current defect.

### Implementers — complete list

| Implementer | File : line | Compiled | Owning child |
| --- | --- | --- | --- |
| `QfcFormViewer` | `QuickFiler/Viewers/QfcFormViewer.cs:18` — `public partial class QfcFormViewer : Form, IQfcFormViewer`, carrying `[ExcludeFromCodeCoverage]` at line 17 | Yes (csproj 444; Designer at 447) | **F15 — MUST NOT be edited by this child** |

**`QfcFormViewer` is the only implementer in the repository.** Two files that a name-based search
suggests might implement it do not:

- `QuickFiler/Viewers/QfcFormViewerExpanded.cs:17` declares `internal partial class
  QfcFormViewerExpanded : Form` — `Form` only, no interface. Its `SetController` takes
  `IQfcFormController` (line 31), not `IFilerFormController`, so it does not even match this
  interface's line 20 signature. It is **not compiled** (absent from the csproj `Viewers\` block,
  lines 380–461).
- `QuickFiler/Viewers/QfcFormViewerDark.cs:17` — identical situation.

**.NET Framework 4.8 has no default interface members.** Any member added to this interface
therefore forces an edit to every implementer. Because there is exactly one implementer and it is
`QuickFiler/Viewers/QfcFormViewer.cs` — owned by sibling child **F15** and explicitly off-limits to
F6 per issue.md "Constraints & Risks" — **any growth of this interface is a cross-child contract
change requiring an F15 edit.** See §4.

### Production consumers

| Consumer | File : line | What it does | Compiled | Owning child |
| --- | --- | --- | --- | --- |
| `IQfcFormController` | `QuickFiler/Controllers/IQfcFormController.cs:17` | `IQfcFormViewer FormViewer { get; }` | Yes (csproj 303) | **F6 (this child)** |
| `QfcFormController` | `QuickFiler/Controllers/QfcFormController.cs:29` (ctor param), `:168` (field), `:169` (property) | Primary consumer | Yes (csproj 317) | **F6 (this child)** |
| `QfcFormController` partials | `.SetupDisposal.cs` (30 call sites), `.Actions.cs` (8), `.EventHandlers.cs` (10), `.cs:44, 165` | Full member usage inventory in §4 | Yes (csproj 318–320) | **F6 (this child)** |
| `KeyboardHandler` | `QuickFiler/Controllers/KeyboardHandler.cs:29` | `public KeyboardHandler(IQfcFormViewer viewer, IFilerHomeController parent)` | Yes (csproj 339) | **F3** |
| `QfcCollectionController` | `QuickFiler/Controllers/QfcCollectionController.cs:32` (ctor param), `:60` (field) | Consumes `SwapItemTableLayout` / `L1v0L2_PanelMain` | Yes (csproj 311) | **F11** |
| `QfcHomeController` | `QuickFiler/Controllers/QfcHomeController.cs:185, 201` (loader `Func<>` type args), `:450` (field) | Yes (csproj 325) | **F7** |

### Test consumers

| Fixture | File : line | Compiled | Owning child |
| --- | --- | --- | --- |
| `QfcFormControllerTests` | `QuickFiler.Test/Controllers/QfcFormControllerTests.cs:23, 103` | Yes (test csproj 117) | **F6** |
| `QfcFormControllerSeamTests` | `.../QfcFormControllerSeamTests.cs:19, 20, 29, 125` | Yes (test csproj 118) | **F6** |
| `QfcHomeControllerRunAsyncTests` | `.../QfcHomeControllerRunAsyncTests.cs:144, 204, 278, 330` | Yes (test csproj 130) | F7 |
| `QfcHomeControllerRunAsyncHighConfidenceTests` | `.../QfcHomeControllerRunAsyncHighConfidenceTests.cs:71, 163, 351, 450` | Yes (test csproj 129) | F7 |
| `QfcHomeControllerIssue218Tests` | `.../QfcHomeControllerIssue218Tests.cs:124, 226` | Yes (test csproj 125) | F7 |
| `QfcCollectionControllerTests` | `.../QfcCollectionControllerTests.cs:333 (comment), 353` | Yes (test csproj 112) | F11 |
| `QfcCollectionControllerDarkModeTests` | `.../QfcCollectionControllerDarkModeTests.cs:39` | Yes (test csproj 113) | F11 |

`Tags.Test/Fakes/FakeTagViewer.cs:14` mentions `IQfcFormViewer` in an XML doc comment only; it is a
different project with no reference to QuickFiler and is not a consumer.

**Ownership consequence.** Fifteen mock sites across F7, F11, and F6 fixtures bind to this
interface. An **additive** member is tolerated by all of them under `MockBehavior.Loose` (the
default, and the explicit mode at `QfcCollectionControllerTests.cs:353`). A **removal or rename**
would break every fixture that stubs the removed member.

---

## 4. Contract-stability assessment

### 4.1 Current member inventory and what each Seam marker covers

Twenty-three declared members, plus everything inherited from `IForm`. The Seam markers are not
decoration: they record a completed testability refactor delivered by issue **#223**
(`docs/features/archive/2026-06-28-qfc-form-viewer-testability-223/`). That feature's policy audit
(`policy-audit.2026-06-29T07-44.md:350`) records the outcome verbatim: "**QuickFiler/Interfaces/IQfcFormViewer.cs**
(MODIFIED) — narrowed to 23 intent members; removed 4 Button + 1 NumericUpDown + 2 template
properties; `L1v0L2L3v_TableLayout` get-only; added Seam B/C/D members."

| Lines | Members | Marker | What it covers / what it replaced |
| --- | --- | --- | --- |
| 14–18 | `Buttons`, `Panels`, `UiScheduler`, `UiSyncContext`, `Worker` | (pre-#223) | Theme control sets, UI scheduling/marshalling context, and the `BackgroundWorker` used by `QfcHomeController`. Consumed by `QfcThemeHelper.SetupFormThemes` (`SetupDisposal.cs:82`) and by the async paths in `.EventHandlers.cs`. |
| 20–21 | `SetController(IFilerFormController)`, `SetKeyboardHandler(IQfcKeyboardHandler)` | (pre-#223) | Back-reference injection. Note the parameter is the **base** `IFilerFormController`, not `IQfcFormController` — that is what makes the cast at `QfcFormControllerTests.cs:159` meaningful. |
| 23–26 | `L1v0L2L3v_TableLayout`, `L1v_TableLayout`, `L1v0L2_PanelMain` | **Seam C** (comment line 23) | Get-only TLP/panel accessors. The comment records that the **setter was removed**; the single setter call site (`QfcCollectionController.ActivateQueuedTlp`) was absorbed into `SwapItemTableLayout`. |
| 28–29 | `SwapItemTableLayout(TableLayoutPanel)` | **Seam C** (comment line 28) | The intent method that replaced the removed `L1v0L2L3v_TableLayout` setter. Implementation at `QfcFormViewer.cs:119–125`. |
| 31–34 | `CaptureTlpCellStates()`, `GetKeyEventExclusionControls()`, `ItemViewerTemplateMargin` | **Seam D** (comment line 31) | Replaced **two raw item-viewer template properties**. `CaptureTlpCellStates()` returns `TlpCellStates` (declared at `QuickFiler/Helper Classes/TlpCellSnapShot.cs:12`, owned by **F4**) and may return `null` when the form is not yet shown (`QfcFormViewer.cs:187–192`). Consumed at `SetupDisposal.cs:35, 37, 167` and `.cs:195`. |
| 36–40 | `OkClicked`, `CancelClicked`, `UndoClicked`, `SkipClicked` | **Seam B** (comment line 36) | Replaced **four raw `Button` properties**. Subscribed/unsubscribed at `SetupDisposal.cs:170–174` and `:198–202`. |
| 42–44 | `SkipButtonText`, `SkipButtonEnabled` | **Seam B** (comment line 42) | Skip-button state, replacing direct control access. Consumed at `.EventHandlers.cs:338–342`. |
| 46–49 | `ItemsPerLoadValue`, `ItemsPerLoadValueChanged`, `ItemsPerLoadEnabled` | **Seam B** (comment line 46) | Replaced **the raw `NumericUpDown` property**. Consumed at `SetupDisposal.cs:135, 144, 173, 201` and `.EventHandlers.cs:257, 280`. |

(Issue #223 also defined a "Seam A" — extracting `IsAltKeyCommand` into
`QuickFiler/Controllers/QfcFormKeyHandler.cs`, now owned by **F3**. Seam A left no marker in this
file because it added no member here.)

### 4.2 Can F6's seam work complete without adding a member? — Yes.

Every viewer member the four `QfcFormController` partials touch was enumerated by searching
`_formViewer.` / `FormViewer.` across `QfcFormController*.cs`. Each one is already available:

| Member used | Where declared |
| --- | --- |
| `L1v0L2L3v_TableLayout` | this file, line 24 |
| `L1v_TableLayout` | this file, line 25 |
| `ItemViewerTemplateMargin` | this file, line 34 |
| `CaptureTlpCellStates()` | this file, line 32 |
| `GetKeyEventExclusionControls()` | this file, line 33 |
| `Panels`, `Buttons` | this file, lines 15, 14 |
| `ItemsPerLoadValue` | this file, line 47 |
| `SkipButtonText`, `SkipButtonEnabled` | this file, lines 43–44 |
| `OkClicked`/`CancelClicked`/`UndoClicked`/`SkipClicked`/`ItemsPerLoadValueChanged` | this file, lines 37–40, 48 |
| `SetController(...)` | this file, line 20 |
| `UiSyncContext` | this file, line 17 |
| `Worker` | this file, line 18 |
| `Show()`, `Hide()`, `Refresh()`, `Invoke(...)`, `Controls`, `Size`, `ClientSize`, `Handle`, `WindowState` | inherited via `IForm` (`WindowState` at `IForm.cs:49`; the rest via `IContainerControl`/`IScrollableControl`) |
| `GetScreen()` | **not an interface member** — an extension method, `UtilitiesCS/Extensions/IControlExtensions.cs:16`, `public static Screen GetScreen(this IControl control)`. Used at `SetupDisposal.cs:105`. Requires no interface change. |

**Conclusion: no member needs to be added to `IQfcFormViewer` for F6 to reach its coverage target.
Recommend the plan treat this interface as frozen.** That result is the reason issue #223 exists —
the seam work was already done, and F6 inherits a viewer contract that is already intent-shaped.

### 4.3 If growth nevertheless proves necessary

**CROSS-CHILD CONTRACT NOTE (F15).** Adding any member `M` to `QuickFiler/Interfaces/IQfcFormViewer.cs`
requires, on .NET Framework 4.8 (no default interface members), a corresponding implementation in
`QuickFiler/Viewers/QfcFormViewer.cs` — the sole implementer, at line 18, owned by **F15**, which
issue.md "Constraints & Risks" states "must not be edited" by this child. The required F15 edit
would be: add the member implementation inside the `#region IQfcFormViewer` block
(`QfcFormViewer.cs:106–260`), forwarding to the Designer-backed control, matching the existing
Seam B/C/D style at lines 111–113, 119–125, 128–147, 150–176, 179–183. F6 cannot land such a change
alone. If the plan author concludes growth is unavoidable, the member name and its exact
`QfcFormViewer.cs` implementation must be recorded in `spec.md` as a cross-child contract note per
issue.md, and F15 notified before either child merges. **This artifact's finding is that no such
growth is needed**; the note exists so the plan author has the escalation path if the executor
discovers otherwise.

Additional consumers to re-verify if the interface does change: F3 (`KeyboardHandler.cs:29`),
F7 (`QfcHomeController.cs:185, 201, 450`), F11 (`QfcCollectionController.cs:32, 60`), and the fifteen
mock sites in §3.

### 4.4 Anti-pattern warning inherited from #223

Issue #223's own coverage evidence
(`evidence/regression-testing/coverage-delta.2026-06-28T20-52.md:10`) records that
`QfcFormController` coverage rose from 39.24% to 51.86% partly because "the denominator decreased
from 767 to 700 because Seam D moved the ~58-line `new TlpCellStates(...)` construction block out of
the controller and into the `[ExcludeFromCodeCoverage]` Form (`CaptureTlpCellStates`)."

**F6 must not repeat that manoeuvre.** Moving logic out of a testable controller into the
`[ExcludeFromCodeCoverage]`-marked `QfcFormViewer.cs` raises F6's per-file number by shrinking its
denominator while pushing untested lines onto F15's file — which this epic obliges F15 to cover.
Under epic.md "Shared Design" §1 the attribute on `QfcFormViewer.cs:17` is treated as *unratified*,
so the destination is not a safe harbour. It would also be a cross-child cost transfer that no
sibling agreed to. Per `.claude/rules/general-unit-test.md` lines 33–35, the correct direction is
the opposite: extract logic **out of** the host-bound file into host-neutral testable modules.

Note also that the 51.86% figure is a **historical** measurement dated 2026-06-28 against the
then-current partials. It is not F6's baseline. F6's baseline must come from F1's harness.

---

## 5. Proposed test cases

**None. The file has no executable content, so there is nothing to execute in a test.**

Stated plainly: do not author reflection-based tests asserting that this interface declares
`SwapItemTableLayout`, that it exposes exactly 23 members, or that it derives from `IForm`. The
compiler already enforces implementation completeness at `QfcFormViewer.cs:18` (a missing member is
CS0535). Reflection tests over a declaration file execute only test-assembly code, add zero lines
to the production coverage numerator, and satisfy no clause of the coverage policy.

The behavior described by these members is already exercised legitimately through the
implementation-side tests. Two F6-owned fixtures target the Seam members specifically —
`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` regions "Seam B — intent command event
routing" (lines 132–241), "Seam B — skip flow state transitions" (243–287), and "Seam D —
CaptureItemSettings via CaptureTlpCellStates" (289–376) — and their coverage is attributed to the
`QfcFormController.*` partials, not to this file.

**One implementation-side observation the plan author needs** (it belongs to the partials'
artifacts, but it is a property of this interface's shape so it is recorded once here): four
`QfcFormController` call sites route through `_formViewer.Invoke(new System.Action(...))` —
`SetupDisposal.cs:134, 143` and `Actions.cs:189, 199`. On a Moq mock of `IQfcFormViewer`, `Invoke`
returns `null` and **does not execute the supplied delegate** unless the test explicitly sets up a
callback that invokes it. Any test that expects the lambda body to be covered must stub `Invoke`
accordingly. This is a test-authoring detail, not a reason to change the interface.

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

Two items to hand to F1:

- The `[ExcludeFromCodeCoverage]` at `QuickFiler/Viewers/QfcFormViewer.cs:17` is one of the 33
  attributes F1 must dispose of. It is **F15's** file, not F6's, but §4.4 shows F6 has a direct
  interest in the disposition: if that attribute is ratified as an irreducible remainder, the
  temptation to push logic into it grows. Recommend F1 note the dependency explicitly.
- `TlpCellStates`, the return type of `CaptureTlpCellStates()` (line 32), is declared at
  `QuickFiler/Helper Classes/TlpCellSnapShot.cs:12` and is assigned to **F4**. F6's tests will
  construct or mock it. No contract change is anticipated, but F4 should not narrow its public
  surface without notice.

---

## 7. Open questions / findings

### Finding V1 — The interface is already seam-complete for F6's needs

The member-by-member usage audit in §4.2 shows every viewer member the `QfcFormController` partials
touch is already declared here or inherited from `IForm`. Issue #223 did this work in June 2026.
The plan author should schedule **zero** tasks that edit this file, and should treat any executor
proposal to add a member as a signal to re-read §4.2 first.

### Finding V2 — Denominator-shifting into `QfcFormViewer.cs` is prohibited

See §4.4. Issue #223's evidence shows the pattern was used once, legitimately at the time. Under
this epic's Shared Design §1 it is no longer available: the destination file's exemption is
unratified and F15 is obliged to cover it. Any F6 task whose net effect is "move lines from a
`QfcFormController` partial into `QfcFormViewer.cs`" should be rejected at plan review.

### Open question V3 — Should the namespace/folder mismatch be corrected?

The file sits in `Interfaces/` but declares `namespace QuickFiler` (line 10) while its ten siblings
in the same folder declare `namespace QuickFiler.Interfaces`. Moving it to `QuickFiler.Interfaces`
would be a **breaking** change: it would require adding `using QuickFiler.Interfaces;` to — or
verifying it already exists in — every consumer in §3, and it would break the seven consumers in
`namespace QuickFiler.Controllers` that currently resolve it through the enclosing `QuickFiler`
namespace with no `using` at all. It touches files owned by F3, F7, F11, and F15.

**Recommendation: do not change it in F6, and do not change it in this epic.** It delivers no
coverage, and the blast radius crosses four siblings during a fully-parallel wave. If it is worth
doing, it is a standalone follow-up issue after the epic closes. Recorded because a reader
encountering the file path will otherwise assume the namespace and re-derive this analysis.

### Open question V4 — `CaptureTlpCellStates()` may return `null`

`QfcFormViewer.cs:187–192` returns `null` when either item-viewer template is uninitialised ("form
not yet shown"). The interface signature at line 32 carries no annotation of this, and the consumer
at `SetupDisposal.cs:37` assigns the result to `_states` without a null check on that line. Whether
the downstream code tolerates `null` is a question for the `QfcFormController.SetupDisposal.cs`
research artifact (sibling researcher), not for this file. Recorded here only because the nullable
contract is a property of this interface's documentation, and if the plan author decides to record
it, the correct fix is an **XML doc comment on line 32** — which adds no executable content and no
member, and therefore does not trigger the F15 cross-child note in §4.3.
