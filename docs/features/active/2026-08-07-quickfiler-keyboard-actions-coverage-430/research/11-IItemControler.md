# Per-File Coverage Research — `QuickFiler/Interfaces/IItemControler.cs`

Timestamp: 2026-08-07T22-00
Feature: `quickfiler-keyboard-actions-coverage` (child F3, issue #430)
Parent epic: `quickfiler-per-file-coverage` (issue #136)
Branch: `feature/quickfiler-keyboard-actions-coverage`

---

## 1. File Under Research

| Attribute | Value |
| --- | --- |
| Path | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aafcc2531072ca96b\QuickFiler\Interfaces\IItemControler.cs` |
| Line count | 15 lines of source |
| Compiled | Yes — `QuickFiler/QuickFiler.csproj:358` `<Compile Include="Interfaces\IItemControler.cs" />` |
| `[ExcludeFromCodeCoverage]` status | **Absent.** Grep for `ExcludeFromCodeCoverage` across `QuickFiler\Interfaces\` returned no matches. |
| Namespace / type | `QuickFiler.IItemControler` (public interface) — note the namespace is `QuickFiler`, **not** `QuickFiler.Interfaces`, despite the file living in the `Interfaces\` folder |
| Existing tests | None targeting this file directly. |
| Exemption-status authority | **F1's `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`.** Recommended classification recorded in §4. |

### Executable-behavior determination (the central question)

**Determination: the file contains ZERO executable behavior.** Verified by reading the whole file:

- No default interface members — all three declarations terminate in `;` with auto-property accessor
  lists and no bodies.
- No `static` members.
- No constants, and therefore no constant initializers.
- No nested types.
- No extension class, no partial class, no second type declared in the file.
- Six `using` directives (lines 1–5 plus the implicit list); a `using` emits no IL.

Note on language capability: `QuickFiler/QuickFiler.csproj:14` sets `<LangVersion>preview</LangVersion>`,
so a default interface member would be syntactically permitted (and the `public` modifier at line 13
requires C# 8+). None is present. The determination is by direct reading, not by language restriction.

**Empirical corroboration.** The most recent committed Cobertura report,
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`,
contains **no `<class>` element named `QuickFiler.IItemControler`** — a grep for
`name="QuickFiler\.(Interfaces\.IMailItemActions|IItemControler)"` returned no matches. The type name
appears in that report only inside a *method signature* attribute of a consumer:
`<method ... name="set_Controller" signature="(QuickFiler.IItemControler)">` at line 5401 — i.e. as
the parameter type of `ItemViewer.Controller`'s setter, which is an `ItemViewer.cs` line, not an
`IItemControler.cs` line. The file therefore contributes **zero lines to both the numerator and the
denominator** of any line-coverage metric.

---

## 2. Structural Inventory

| Lines | Member | Kind | Body? | Dependencies |
| --- | --- | --- | --- | --- |
| 1–5 | `using System; using System.Collections.Generic; using System.Linq; using System.Text; using System.Threading.Tasks;` | using directives | n/a | Only `System.Collections.Generic` is load-bearing (`Dictionary<,>`). `System.Action` is written fully qualified at line 13, so even `System` is not strictly required. Lines 3, 4, 5 are unused. |
| 7 | `namespace QuickFiler` | namespace | n/a | — |
| 9 | `public interface IItemControler` | type declaration | no | — |
| 11 | `int CounterEnter { get; set; }` | property decl | no | — |
| 12 | `int CounterComboRight { get; set; }` | property decl | no | — |
| 13 | `public Dictionary<string, System.Action> RightKeyActions { get; }` | property decl (get-only) | no | `System.Collections.Generic.Dictionary`, `System.Action` |

Total: 3 declared members, **0 method bodies**, **0 branches**, **0 IL-emitting lines**.

### Observations that are NOT defects to fix in this child

Recorded for completeness; each is followed by an explicit recommendation against acting.

1. **Inconsistent accessibility modifier.** Line 13 carries an explicit `public`; lines 11–12 do not.
   Both compile identically (interface members are implicitly public). The same inconsistency exists
   in the sibling `QuickFiler\Interfaces\IQfcItemController.cs`, at lines 95 and 97. Cosmetic;
   zero coverage effect. **Do not change** — pure merge-conflict surface on the integration branch.
2. **Three unused `using` directives** (lines 3, 4, 5). Zero coverage effect; removing them risks
   surfacing an analyzer-severity difference during the `/p:TreatWarningsAsErrors=true` gate for no
   benefit. **Do not change.**
3. **Type name is misspelled**: `IItemControler` (one `l`) versus the conventional `IItemController`.
   Renaming is a **BREAKING** change across five sibling-owned files (see §6). **Do not change.**
4. **Namespace/folder mismatch**: the type sits in namespace `QuickFiler` while the file sits in
   `Interfaces\`, whereas `IMailItemActions`, `IQfcItemController`, and the rest of that folder use
   namespace `QuickFiler.Interfaces`. Changing the namespace would compile-break every consumer's
   `using` list. **Do not change.**

---

## 3. Existing Test Coverage (static analysis)

No numeric per-file coverage harness exists yet (F1, wave 0, has not delivered). The table below is
static analysis mapping each declared member to the existing `QuickFiler.Test/` test methods that
exercise it through an implementation or a test double, since the declarations themselves have no
bodies to execute.

| Member (line) | Executable lines | Exercised by (existing tests, by name/site) | Coverage effect on this file |
| --- | --- | --- | --- |
| `CounterEnter { get; set; }` (11) | 0 | `QuickFiler.Test\Controllers\QfcItemController.PropertiesTests.cs:86–87` (`controller.CounterEnter = 7; controller.CounterEnter.Should().Be(7);`); test-double implementation at `QuickFiler.Test\Helper Classes\QfcThemeHelperTests.cs:339` | None — declaration emits no IL |
| `CounterComboRight { get; set; }` (12) | 0 | `QfcItemController.PropertiesTests.cs:89–90`; test-double at `QfcThemeHelperTests.cs:340` | None |
| `RightKeyActions { get; }` (13) | 0 | `QuickFiler.Test\Controllers\QfcItemController.MailActionsTests.cs:107–120` (`RightKeyActions_Getter_ContainsExpectedMenuKeys`, cited at line 92 as "Cycle-2 Phase 5 (AC8) de-exemption coverage"); test-double at `QfcThemeHelperTests.cs:359–360` | None |

**Static-analysis conclusion:** all three declared members are reached through the `QfcItemController`
implementation (covered by the F10-owned test suite) and through a hand-written test double
(`FakeQfcItemController` in `QfcThemeHelperTests.cs:337–361`). No member is orphaned. The declaration
file itself has nothing to execute.

**Numeric measurement statement (required by the epic):** numeric per-file line coverage will be
measured at execution time with **F1's per-file coverage report harness**, derived from the Cobertura
output of `Invoke-MSTestWithCoverage.ps1`. Based on the empirical absence of a `<class>` entry in the
current Cobertura report, the expected harness output for this file is **N/A (0 measurable lines)**,
not `0%`. See the harness contract note in §4.

---

## 4. Coverage Gaps

**None.** Three declared members, zero executable lines, zero branches — therefore zero untested
members and zero untested branches. There is no genuine gap to target.

Per issue #136's mandate to target genuine gaps rather than duplicate existing tests, any test written
directly against `IItemControler` would assert only that the C# compiler emitted the members the
source declares. It would move no coverage number and must not be written.

### Harness contract note for F1 (0/0 division)

Same requirement as recorded in `09-IMailItemActions.md` §4, restated here because it applies to this
file independently and to roughly 24 interface-only files across the epic
(`docs/features/epics/quickfiler-per-file-coverage/epic.md:113`):

> A file producing **no `<class>` element** in the Cobertura report has 0 measurable lines. F1's
> per-file harness must report it as `N/A` / `not measured` and exclude it from the >= 80% gate
> arithmetic. Reporting `0%` would create ~24 permanently-failing false gate failures; dropping it
> silently would make F16's "all 121 compiled files accounted for" check unverifiable.

**Additional harness edge case specific to this file.** The type name `QuickFiler.IItemControler`
appears in the Cobertura report only as a **method-signature substring** on a consumer's method
(`coverage-final.cobertura.xml:5401`, `set_Controller` on `ItemViewer`). A harness that attributes
coverage by naive substring match on the type name rather than by the `<class>` element's `filename`
attribute would mis-attribute `ItemViewer.cs` lines to `IItemControler.cs`. F1's harness must key on
`filename`, not on name matching. Report as a defect to F1 if observed.

### Recommended ledger classification (for F1 to ratify)

**Recommended classification: `interface-only — zero executable lines — not in the coverage
denominator`.**

Deliberately **not** `ratified-exempt`, for the same load-bearing reason set out in
`09-IMailItemActions.md` §7:

| Bucket | Governing rule | Fits this file? |
| --- | --- | --- |
| `testable` | epic Shared Design §1 | No — 0 executable lines; a >= 80% target is 0/0 and unreachable by construction |
| `ratified-exempt` | `CLAUDE.md` § UT2 COM/VSTO/WinForms exemption | No — there is no executable remainder to accept, and no COM/WinForms type appears anywhere in the file |
| `interface-only` | `.claude/rules/general-unit-test.md:29` — "Type-only / interface-only modules with no executable behavior may be omitted from coverage measurement … C# interface-only files. Such modules legitimately report 0% executable coverage and may be excluded from measurement. This is a clarification only; it does not lower any coverage threshold." | **Yes** |

**Test against the epic's irreducible-remainder standard.** Shared Design §1 reads the CLAUDE.md
qualifier "without an injectable seam" as a live obligation: an exemption is legitimate only where a
refactor cannot produce a testable seam. Applying that standard asks what a refactor would extract.
The answer is nothing — the file declares three property signatures and contains no logic, no I/O, no
COM call, and no WinForms type. Notably, the file's dependency surface is entirely host-neutral
(`int`, `Dictionary<string, Action>`), so it is not even nominally within the COM/VSTO exemption's
subject matter. Classifying it `ratified-exempt` would misrecord "nothing to cover" as "an accepted
untestable remainder" and would inflate the epic's exempt-file count without cause.

**Consequence for the epic's leading indicator.** `epic.md:14` targets zero QuickFiler files carrying
`[ExcludeFromCodeCoverage]` on a testable seam. This file carries no attribute and needs none. It is
already compliant and requires no action from F3.

---

## 5. Seam Requirements

**Not applicable — no executable behavior.**

There is no body, branch, or dependency call to isolate. As with `IMailItemActions`, the seam-hierarchy
analysis in `.claude/rules/csharp.md:49–54` inverts: this file **is** a level-1 interface seam. Its
practical function in the F3 cluster is precisely that — it is the abstraction through which
`KeyboardHandler.cs` (this child's central 414-line file) reaches an item controller without a
compile-time dependency on the concrete `QfcItemController` or `EfcItemController`:

- `QuickFiler\Controllers\KeyboardHandler.cs:308` — `viewer.Controller.RightKeyActions`
- `QuickFiler\Controllers\KeyboardHandler.cs:354` — `cbo.GetAncestor<ItemViewer>().Controller.RightKeyActions`

Both reach `RightKeyActions` through `ItemViewer.Controller`, which is typed `IItemControler`
(`QuickFiler\Viewers\ItemViewer.cs:52–57`). **This interface is therefore the mock point that will make
those two `KeyboardHandler` paths testable.** That is directly relevant to this child's seam work, and
it is already available — no change to this file is required to use it. A test can supply a
`Mock<IItemControler>` whose `RightKeyActions` returns a prepared dictionary without constructing any
controller.

Constraint carried forward for the `KeyboardHandler` research: both call sites pass the dictionary
into `MyBox.ShowDialog(...)`, which is a popup. Per the epic's Shared Design §2 and this child's
constraints, a unit test must never show a popup, so `MyBox.ShowDialog` itself will need its own seam
(analysed in the `KeyboardHandler.cs` artifact, not here). `IItemControler` supplies the *other half*
of that seam pair and needs no modification.

---

## 6. Cross-Child Contract Impact

### Implementers (production)

| Implementer | Path | Members implemented | Owning child |
| --- | --- | --- | --- |
| `QfcItemController` | `QuickFiler\Controllers\QfcItemController.cs:25–29` (declares `: IQfcItemController, INotifyPropertyChanged, IItemControler`) | `CounterEnter` at `QfcItemController.cs:117–121`; `CounterComboRight` at `:124–128`; `RightKeyActions` at `QuickFiler\Controllers\QfcItemController.MailActions.cs:54` | **F10** (`quickfiler-item-controller-coverage`) |
| `EfcItemController` | `QuickFiler\Controllers\EfcItemController.cs:26` (`internal class EfcItemController : IItemControler`) | `CounterEnter` at `:425`; `CounterComboRight` at `:432`; `RightKeyActions` at `:1157` | **F9** (`quickfiler-efc-form-item-controller-coverage`) |

### Implementers (test doubles)

| Double | Path | Owning child |
| --- | --- | --- |
| `FakeQfcItemController` | `QuickFiler.Test\Helper Classes\QfcThemeHelperTests.cs:337–361` (implements `IQfcItemController`, which declares the same three member signatures) | F4 test code |

### Consumers

| Consumer | Path | Owning child |
| --- | --- | --- |
| `ItemViewer.Controller` property (`IItemControler _controller` + get/set) | `QuickFiler\Viewers\ItemViewer.cs:52–57` | **F14** (`quickfiler-itemviewer-coverage`) |
| `ItemViewerExpanded.Controller` property | `QuickFiler\Viewers\ItemViewerExpanded.cs:50–51` | **F14** |
| `IItemViewer.Controller { get; set; }` declaration | `QuickFiler\Viewers\IItemViewer.cs:17` | **F14** |
| `KeyboardHandler` — `viewer.Controller.RightKeyActions` | `QuickFiler\Controllers\KeyboardHandler.cs:308` | **F3 (this child)** |
| `KeyboardHandler` — `cbo.GetAncestor<ItemViewer>().Controller.RightKeyActions` | `QuickFiler\Controllers\KeyboardHandler.cs:354` | **F3 (this child)** |
| `QfcItemViewer.Controller` property | `QuickFiler\Viewers\QfcItemViewer.cs:47–48` | **Not compiled** — a grep of `QuickFiler.csproj` for `Viewers\QfcItemViewer.cs` returned no match; only `Viewers\QfcItemViewerExpanded.cs` (line 450) is registered. This file is outside the coverage denominator and outside the epic. |

The consumer set spans **F3, F9, F10, and F14**. This is the widest cross-child surface of the three
files in this cluster.

### Additive-vs-breaking determination

**Recommended change: none. The impact is therefore nil.**

Determinations if a change were contemplated, stated against the mandate that F3's changes remain
additive and that sibling-owned files are not edited:

| Contemplated change | Classification | Reason |
| --- | --- | --- |
| Rename `IItemControler` → `IItemController` | **BREAKING** | Requires edits to `QfcItemController.cs:28` (F10), `EfcItemController.cs:26` (F9), `ItemViewer.cs:52–53` (F14), `ItemViewerExpanded.cs:50–51` (F14), `IItemViewer.cs:17` (F14). Five sibling-owned files. **Prohibited.** |
| Change namespace `QuickFiler` → `QuickFiler.Interfaces` | **BREAKING** | Compile-breaks the `using` list of every consumer above. **Prohibited.** |
| Add a member to `IItemControler` | **BREAKING for implementers** | Forces an edit to `QfcItemController` (F10) and `EfcItemController` (F9) to satisfy the new member. No need exists. **Prohibited.** |
| Make `IQfcItemController : IItemControler` to remove the triplicated `CounterEnter`/`CounterComboRight`/`RightKeyActions` declarations (`IQfcItemController.cs:34,35,97` duplicate `IItemControler.cs:11,12,13`) | **Out of scope regardless** | The edit would land in `QuickFiler\Interfaces\IQfcItemController.cs`, which the epic assigns to **F10** (`epic.md:327`). F3 must not edit it. Record as a design observation for F10 or F16. |
| Remove unused `using` directives (lines 3–5) or the redundant `public` (line 13) | Non-breaking but **not recommended** | Zero coverage benefit; creates a merge-conflict surface on the integration branch during a 14-child parallel wave. |

**F3 leaves this file byte-identical.** That satisfies the additive mandate trivially and eliminates
this file from the epic's conflict surface.

---

## 7. Proposed Test Cases

**None — no executable behavior.**

Rationale against each required scenario category:

- *Positive path*: no path exists; all three members are declarations without bodies.
- *Invalid input*: no parameter lists, no bodies to reject input.
- *Boundary*: no state, no branch.
- *Error handling*: no statement that can throw.

Behavior is tested where it is implemented — `QfcItemController` (F10; e.g.
`QfcItemController.PropertiesTests.cs:86–90` and `QfcItemController.MailActionsTests.cs:107–120`) and
`EfcItemController` (F9). Writing a test class against the declarations would duplicate those and is
prohibited by issue #136's non-duplication mandate.

**Where this interface DOES appear in F3's proposed test work:** as a Moq mock type inside the
`KeyboardHandler.cs` test cases, to supply `RightKeyActions` at `KeyboardHandler.cs:308` and `:354`
without constructing a controller. Those tests belong to the `KeyboardHandler.cs` research artifact and
are counted there, not here. They exercise `KeyboardHandler.cs` lines, not `IItemControler.cs` lines.

---

## 8. Risks and Open Questions

| # | Item | Assessment |
| --- | --- | --- |
| R1 | **F1's ledger may classify this file `ratified-exempt` or `testable` instead of `interface-only`.** | F1's ledger is authoritative. If F1 offers no `interface-only` bucket, F3 accepts F1's classification and records this artifact's reasoning as a dissent note in `spec.md`. If F1 classifies it `testable` with a >= 80% target, that target is 0/0 and unreachable — escalate to the epic orchestrator rather than fabricate tests. |
| R2 | **F1's harness may report 0/0 files as `0%`,** producing a false gate failure here and for ~23 sibling interface files. | Raised in §4 as a consumer requirement on the upstream contract. Verify on first harness use; report a defect to F1 if observed. |
| R3 | **Harness mis-attribution via name matching.** `QuickFiler.IItemControler` appears in the Cobertura report only inside a consumer's `signature` attribute (`coverage-final.cobertura.xml:5401`). A substring-matching harness would attribute `ItemViewer.cs` lines to this file. | F1's harness must key on the `<class>` element's `filename` attribute. Verify and report. |
| R4 | **Temptation to "fix" the misspelling, namespace, unused usings, or redundant `public` while the file is open.** | All four are explicitly rejected in §2 and §6. The misspelling fix is BREAKING across five sibling-owned files; the others are pure conflict surface. The atomic plan should contain no task touching this file. |
| R5 | **Triplicated member declarations across `IItemControler` and `IQfcItemController`** mean a future signature change must be made in two interfaces plus two implementers. | Real design debt, but the consolidating edit lands in F10-owned `IQfcItemController.cs`. Out of F3's scope. Record as a cross-child observation in `spec.md` for F10 or the F16 capstone. |
| Q1 | Does `KeyboardHandler.cs` reach `IItemControler` anywhere other than lines 308 and 354? | A grep for `RightKeyActions` returned only those two `KeyboardHandler.cs` sites, and `CounterEnter`/`CounterComboRight` do not appear in `KeyboardHandler.cs` at all. The full `KeyboardHandler.cs` structural inventory belongs to that file's own artifact; this answer covers only the `IItemControler` surface. |
| Q2 | Is `QuickFiler\Viewers\QfcItemViewer.cs` (which also declares an `IItemControler Controller` property at lines 47–48) genuinely outside the denominator? | Yes, on current evidence: a grep of `QuickFiler.csproj` for `Viewers\QfcItemViewer.cs` returned no match, and the epic's file assignment (`epic.md:242–373`) does not list it under any child. F1's ledger should confirm it as not-compiled. If F1 finds otherwise, the consumer table in §6 gains a row and an owning child must be assigned. |

---

## 9. Sources

| Source | Lines cited |
| --- | --- |
| `QuickFiler\Interfaces\IItemControler.cs` | 1–15 (read in full) |
| `QuickFiler\Interfaces\IQfcItemController.cs` | 1–107 (read in full); esp. 34, 35, 95, 97 (duplicate member declarations) |
| `QuickFiler\QuickFiler.csproj` | 14 (`<LangVersion>preview</LangVersion>`), 358 (`<Compile Include="Interfaces\IItemControler.cs" />`), 392 (`Viewers\IItemViewer.cs`), 438/450 (`ItemViewerExpanded.cs` / `QfcItemViewerExpanded.cs`); grep for `Viewers\QfcItemViewer.cs` returned **no match** |
| `QuickFiler\Controllers\QfcItemController.cs` | 25–29 (implements `IItemControler`), 116–128 (`CounterEnter`, `CounterComboRight`) |
| `QuickFiler\Controllers\QfcItemController.MailActions.cs` | 54 (`RightKeyActions`), 72 (`RightKeyActionsAsync`) |
| `QuickFiler\Controllers\EfcItemController.cs` | 26 (implements `IItemControler`), 425, 432, 1157 |
| `QuickFiler\Controllers\KeyboardHandler.cs` | 292–315 (`BreadcrumbArrowFallThrough`, incl. `viewer.Controller.RightKeyActions` at 308 and `MyBox.ShowDialog` at 304), 343–357 (`DdOpen_KeyDownAsync` Right branch, incl. line 354) |
| `QuickFiler\Viewers\ItemViewer.cs` | 52–57 (`IItemControler Controller`) |
| `QuickFiler\Viewers\ItemViewerExpanded.cs` | 50–51 |
| `QuickFiler\Viewers\IItemViewer.cs` | 17 |
| `QuickFiler\Viewers\QfcItemViewer.cs` | 47–48 (not compiled) |
| `QuickFiler.Test\Controllers\QfcItemController.PropertiesTests.cs` | 86–90 |
| `QuickFiler.Test\Controllers\QfcItemController.MailActionsTests.cs` | 92, 107–120, 122–128 |
| `QuickFiler.Test\Helper Classes\QfcThemeHelperTests.cs` | 337–361 (`FakeQfcItemController` test double) |
| `docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\qa-gates\coverage-final.cobertura.xml` | 5401 (`set_Controller` signature reference); grep for an `IItemControler` `<class>` entry returned **no matches** |
| `docs\features\epics\quickfiler-per-file-coverage\epic.md` | 1–419 (read in full); esp. 14 (leading indicator), 113 (~24 interface-only files), 132–192 (Shared Design §1–§6), 267–274 (F3 assignment), 315–329 (F9/F10 assignments), 355–363 (F14 assignment) |
| `docs\features\active\2026-08-07-quickfiler-keyboard-actions-coverage-430\issue.md` | 1–95 (read in full); esp. 63–79 (Constraints & Risks) |
| `.claude\rules\general-unit-test.md` | 21–29 (Coverage Requirements incl. the interface-only clarification at line 29), 31–46 (Coverage Exclusion Policy), 48–57 (Scenario Completeness) |
| `.claude\rules\csharp.md` | 47–54 (DI seam hierarchy), 31–41 (Testing Standards) |
| `CLAUDE.md` | § UT2 COM/VSTO/WinForms coverage exemption; § CUT1–CUT3 |
