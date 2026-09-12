# Per-File Coverage Research — `QuickFiler/Interfaces/IMailItemActions.cs`

Timestamp: 2026-08-07T22-00
Feature: `quickfiler-keyboard-actions-coverage` (child F3, issue #430)
Parent epic: `quickfiler-per-file-coverage` (issue #136)
Branch: `feature/quickfiler-keyboard-actions-coverage`

---

## 1. File Under Research

| Attribute | Value |
| --- | --- |
| Path | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aafcc2531072ca96b\QuickFiler\Interfaces\IMailItemActions.cs` |
| Line count | 35 lines of source (file ends at line 35; a trailing newline yields 36 in some counters) |
| Compiled | Yes — `QuickFiler/QuickFiler.csproj:367` `<Compile Include="Interfaces\IMailItemActions.cs" />` |
| `[ExcludeFromCodeCoverage]` status | **Absent.** A grep for `ExcludeFromCodeCoverage` across `QuickFiler\Interfaces\` returned no matches. No attribute exists on the file, the interface, or any member. |
| Namespace / type | `QuickFiler.Interfaces.IMailItemActions` (public interface) |
| Exemption-status authority | **F1's `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`** is the sole authority on the final `testable` vs `ratified-exempt` classification. F1 has not yet produced the ledger; this artifact records the recommended classification and its evidence for F1 to ratify. |

### Executable-behavior determination (the central question)

**Determination: the file contains ZERO executable behavior.** Verified by reading the whole file:

- No default interface members (no member has a body; every declaration terminates in `;`).
- No `static` members of any kind.
- No constants, and therefore no constant initializers.
- No nested types.
- No extension class, no partial class, and no second type declared in the file.
- Only one `using` directive (`Microsoft.Office.Interop.Outlook`, line 1) supporting the `MailItem` return types; a `using` emits no IL.

Note on language capability: `QuickFiler/QuickFiler.csproj:14` sets `<LangVersion>preview</LangVersion>`, so default interface members would be syntactically permitted. None are present. This determination is by direct reading, not by language restriction.

**Empirical corroboration.** The most recent committed Cobertura report in the repository —
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml` —
contains **no `<class>` element whose `name` is `QuickFiler.Interfaces.IMailItemActions`**. A grep for
`name="QuickFiler\.(Interfaces\.IMailItemActions|IItemControler)"` against that file returned no
matches. The instrumenter emits no class entry because the type compiles to no IL bodies. The file
therefore contributes **zero lines to both the numerator and the denominator** of any line-coverage
metric derived from that report.

---

## 2. Structural Inventory

| Lines | Member | Kind | Body? | Dependencies |
| --- | --- | --- | --- | --- |
| 1 | `using Microsoft.Office.Interop.Outlook;` | using directive | n/a | Outlook PIA |
| 3 | `namespace QuickFiler.Interfaces` | namespace | n/a | — |
| 5–11 | XML doc comment on the interface | comment | n/a | references `MailItemActionsAdapter`, `QfcItemController` |
| 12 | `public interface IMailItemActions` | type declaration | no | — |
| 15 | `MailItem Reply();` | method decl | no | `Microsoft.Office.Interop.Outlook.MailItem` |
| 18 | `MailItem ReplyAll();` | method decl | no | `MailItem` |
| 21 | `MailItem Forward();` | method decl | no | `MailItem` |
| 24 | `void Display();` | method decl | no | — |
| 27 | `bool UnRead { get; set; }` | property decl (auto-shape, no body) | no | — |
| 30 | `void Save();` | method decl | no | — |
| 33 | `string EntryID { get; }` | property decl (get-only, no body) | no | — |

Total: 7 declared members (5 methods, 2 properties), **0 method bodies**, **0 branches**, **0 IL-emitting lines**.

### Design purpose (from the file's own XML doc, lines 5–11)

The interface is the narrow Outlook COM seam scoped to exactly the operations `QfcItemController`
performs on its underlying `MailItem`. It exists so that previously COM-bound controller methods
become unit-testable with a `Mock<IMailItemActions>`. This is a **seam-enabling type**: its value to
the coverage metric is entirely indirect — it is the mechanism by which other files become testable,
not itself a source of covered or uncovered lines.

---

## 3. Existing Test Coverage (static analysis)

No numeric per-file coverage harness exists yet (F1, wave 0, has not delivered). The table below is
static analysis: each declared member is mapped to the existing `QuickFiler.Test/` test methods that
exercise **the declaration** (via a mock or an implementation), since the declaration itself has no
body to execute.

| Member (line) | Executable lines | Exercised by (existing test methods) | Coverage effect |
| --- | --- | --- | --- |
| `Reply()` (15) | 0 | `MailItemActionsAdapterTests.Reply_ForwardsToUnderlyingMailItem` (implementation); `QfcItemController.SeamCoreTests` / `.SeamDispatcherTests` / `.NavigationTests` via `Mock<IMailItemActions>` | None — declaration emits no IL |
| `ReplyAll()` (18) | 0 | `MailItemActionsAdapterTests.ReplyAll_ForwardsToUnderlyingMailItem`; `QfcItemController.NavigationTests` via mock | None |
| `Forward()` (21) | 0 | `MailItemActionsAdapterTests.Forward_ForwardsToUnderlyingMailItem`; `QfcItemController.NavigationTests` via mock | None |
| `Display()` (24) | 0 | `MailItemActionsAdapterTests.Display_ForwardsToUnderlyingMailItem`; `QuickFiler.Test\Controllers\QfcItemController.SeamCoreTests.cs:161` (`mailActions.Verify(m => m.Display(), Times.Once())`) | None |
| `UnRead { get; set; }` (27) | 0 | `MailItemActionsAdapterTests.UnRead_GetAndSet_ForwardToUnderlyingMailItem`; consumed at `QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs:322` and covered by the FocusAndTheme tests via mock | None |
| `Save()` (30) | 0 | `MailItemActionsAdapterTests.Save_ForwardsToUnderlyingMailItem`; consumed at `QfcItemController.FocusAndTheme.cs:323` via mock | None |
| `EntryID { get; }` (33) | 0 | `MailItemActionsAdapterTests.EntryID_ForwardsToUnderlyingMailItem`; `QuickFiler.Test\Controllers\QfcItemController.MailActionsTests.cs:160–165` (`Mock<IMailItemActions>` EntryID seam) | None |

**Static-analysis conclusion:** every declared member is already reached through at least two
independent routes (the concrete `MailItemActionsAdapter` and Moq-generated proxies in the
`QfcItemController` seam tests). No declared member is orphaned or unreferenced.

**Numeric measurement statement (required by the epic):** the numeric per-file line coverage for this
file will be measured at execution time with **F1's per-file coverage report harness**, derived from
the Cobertura output of `Invoke-MSTestWithCoverage.ps1`. Based on the empirical absence of a
`<class>` entry in the current Cobertura report, the expected harness output for this file is
**N/A (0 measurable lines)**, not `0%`.

---

## 4. Coverage Gaps

**None.** There are zero executable members and zero branches, therefore zero untested members and
zero untested branches. There is no genuine gap to target.

Restating this against issue #136's mandate to "target the genuine gaps rather than duplicating
existing tests": any test written directly against `IMailItemActions` would be a test of the C#
compiler (that an interface declares the members it declares), would move no coverage number, and
would therefore be pure duplication. It must not be written.

### Harness contract note for F1 (0/0 division)

This file surfaces a real requirement on F1's harness, and it applies to the roughly 24 interface-only
files identified in the epic scope statement (`epic.md:113`):

> A file that produces **no `<class>` element** in the Cobertura report has 0 measurable lines. The
> per-file harness must report it as `N/A` / `not measured` and exclude it from the >= 80% gate
> arithmetic. Reporting it as `0%` would create 24 permanently-failing false gate failures across the
> epic; silently dropping it without a marker would make the "every one of the 121 compiled files is
> accounted for" capstone check (F16) unverifiable.

This is stated as a consumer requirement on the upstream contract, not as an F3 deliverable.

---

## 5. Seam Requirements

**Not applicable — no executable behavior.**

Seam extraction has nothing to attach to: there is no body, no branch, no dependency call to isolate.
The seam-hierarchy analysis in `.claude/rules/csharp.md` (interface seam > injectable delegate >
adapter) is inverted here — this file **is** the level-1 interface seam. It is the highest-priority
seam form the repository prescribes, already applied. Its production realization is
`MailItemActionsAdapter` (level-3 adapter seam), analysed in `10-MailItemActionsAdapter.md`.

---

## 6. Cross-Child Contract Impact

### Implementers

| Implementer | Path | Owning child |
| --- | --- | --- |
| `MailItemActionsAdapter` | `QuickFiler\Interfaces\MailItemActionsAdapter.cs:12` | **F3 (this child)** |
| Moq-generated proxies | `QuickFiler.Test\Controllers\QfcItemController.SeamCoreTests.cs:34,40,87,155`; `.SeamDispatcherTests.cs:161,333`; `.MailActionsTests.cs:165` | test-only |

There is exactly one production implementer, and it is owned by this child.

### Consumers (all production call sites)

| Call site | Path | Owning child |
| --- | --- | --- |
| Field declaration `_mailActions` | `QuickFiler\Controllers\QfcItemController.cs:68` | **F10** |
| Optional ctor parameter | `QuickFiler\Controllers\QfcItemController.Initialization.cs:40`, assignment at `:59` | **F10** |
| Production default construction | `QuickFiler\Controllers\QfcItemController.Initialization.cs:392–394` | **F10** |
| `_mailActions.Display()` | `QuickFiler\Controllers\QfcItemController.EventHandlers.cs:135` | **F10** |
| `_mailActions.UnRead = false;` / `.Save()` | `QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs:322–323` | **F10** |
| `_mailActions.EntryID` | `QuickFiler\Controllers\QfcItemController.MailActions.cs:32,43` | **F10** |
| `_mailActions.Reply()` / `.ReplyAll()` / `.Forward()` | `QuickFiler\Controllers\QfcItemController.Navigation.cs:90,96,102` | **F10** |

Every consumer is an `QfcItemController` partial owned by sibling child **F10
(`quickfiler-item-controller-coverage`)**. No other assembly or project references the interface.

### Additive-vs-breaking determination

**Recommended change: none. Therefore the impact is neither additive nor breaking — it is nil.**

For completeness, the determination if a change were contemplated:

- **Adding a member** to `IMailItemActions` would be **BREAKING** for implementers: it would force an
  edit to `MailItemActionsAdapter.cs` (F3-owned, permitted) and would compile-break nothing in F10
  (Moq auto-implements new members; F10's production code only *consumes* the interface). It would,
  however, add an uncovered forward to `MailItemActionsAdapter` and would require a genuine consumer
  in F10-owned code to justify it. No such consumer need exists. **Do not do this.**
- **Removing or renaming a member** would be **BREAKING** for F10-owned call sites listed above and is
  explicitly prohibited by the mandate not to edit sibling-owned files.
- **Reordering members or editing XML doc comments** would be non-breaking but delivers no coverage
  value and creates a needless merge-conflict surface on the integration branch. **Do not do this.**

The mandate that F3's changes remain additive with respect to F6/F10-owned files is satisfied
trivially by leaving this file byte-identical.

---

## 7. Proposed Test Cases

**None — no executable behavior.**

Rationale, stated against each of the four scenario categories the policy requires:

- *Positive path*: there is no path. Every member is a declaration with no body.
- *Invalid input*: there is no parameter list to validate and no body to reject input.
- *Boundary*: there is no state and no branch.
- *Error handling*: there is no statement that can throw.

The behavior of the seam is tested where it is realized (`MailItemActionsAdapter`, see
`10-MailItemActionsAdapter.md`, existing 7 test methods plus proposed additions) and where it is
consumed (`QfcItemController.*` seam tests, F10-owned). Writing a separate test class against the
interface would duplicate both and is prohibited by issue #136's non-duplication mandate.

### Recommended ledger classification (for F1 to ratify)

**Recommended classification: `interface-only — zero executable lines — not in the coverage
denominator`.**

This is deliberately **not** `ratified-exempt`, and the distinction is load-bearing:

| Bucket | Governing rule | Meaning | Fits this file? |
| --- | --- | --- | --- |
| `testable` | epic Shared Design §1 | has executable lines reachable by a deterministic unit test; must reach >= 80% | No — 0 executable lines |
| `ratified-exempt` | `CLAUDE.md` § UT2 COM/VSTO/WinForms exemption | has executable lines that **cannot** be reached without a live host, after refactor has been attempted; an irreducible remainder is accepted | No — there is no remainder to accept |
| `interface-only` | `.claude/rules/general-unit-test.md` § Coverage Requirements, line 29: "Type-only / interface-only modules with no executable behavior may be omitted from coverage measurement … C# interface-only files. Such modules legitimately report 0% executable coverage and may be excluded from measurement. This is a clarification only; it does not lower any coverage threshold." | zero executable lines; a measurement-scope statement, not a waiver | **Yes** |

**Test against the epic's irreducible-remainder standard.** The epic's Shared Design §1 reconciliation
reads the CLAUDE.md qualifier "without an injectable seam" as a live obligation: exemption is only
legitimate where refactoring cannot produce a testable seam. Applying that standard here asks "what
would a refactor extract?" The answer is nothing — there is no logic in the file to extract, and the
file is itself the product of exactly the refactor the standard demands (it is the level-1 interface
seam that de-COM-bound seven `QfcItemController` call sites). Classifying it `ratified-exempt` would
misrecord a completed refactor as an accepted defeat, and would inflate the epic's exempt-file count
with 24 interface files that carry no risk. Classifying it `interface-only` records the truth: there
is nothing to cover.

**Consequence for the epic's leading indicator.** `epic.md:14` targets "the count of QuickFiler files
carrying `[ExcludeFromCodeCoverage]` on a testable seam falls to zero." This file carries no attribute
and needs none. It is already compliant and requires no action from F3.

---

## 8. Risks and Open Questions

| # | Item | Assessment |
| --- | --- | --- |
| R1 | **F1's ledger may classify this file `ratified-exempt` or `testable` instead of `interface-only`.** | F1's ledger is the authority. If F1 introduces no `interface-only` bucket, F3 must accept F1's classification and record this artifact's reasoning as a dissent note in `spec.md`. If F1 classifies it `testable` with an >= 80% target, that target is unreachable (0/0) and F3 must escalate to the epic orchestrator rather than fabricate tests. |
| R2 | **F1's harness may report 0/0 files as `0%`.** | Would produce a false gate failure for this file and ~23 siblings. Raised in §4 as a consumer requirement on the upstream contract. F3 should verify the harness's 0/0 handling on first use and report a defect to F1 if it reports `0%`. |
| R3 | **`[ExcludeFromCodeCoverage]` could be added by a well-meaning later change** to "silence" the file in a report. | Would be a Blocking finding under `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy only if it excluded production runtime code; here it would be inert but misleading. Recommendation: do not add it. The file needs no attribute because it generates no class entry. |
| Q1 | Should `IMailItemActions` and `MailItemActionsAdapter` be merged into one file, or the interface moved next to its consumers? | **No.** Both are already under the 500-line ceiling by two orders of magnitude, the current split matches the repository convention (`QuickFiler\Interfaces\` holds `IQfcItemController.cs`, `IQfcCollectionController.cs`, `IQfcKeyboardHandler.cs`, etc.), and any move creates merge conflicts on the integration branch for zero coverage benefit. |
| Q2 | Is `Microsoft.Office.Interop.Outlook.MailItem` in the public signature a portability liability for the long-term VSTO exit? | Yes in principle — `Reply()`, `ReplyAll()`, and `Forward()` return the COM type. Out of scope for a coverage child, and changing it would break F10-owned call sites at `QfcItemController.Navigation.cs:90,96,102`. Record as a migration observation only; do not act in F3. |

---

## 9. Sources

Files read in full or at the cited ranges during this research:

| Source | Lines cited |
| --- | --- |
| `QuickFiler\Interfaces\IMailItemActions.cs` | 1–35 (read in full) |
| `QuickFiler\Interfaces\MailItemActionsAdapter.cs` | 1–47 (read in full) — sole implementer |
| `QuickFiler\QuickFiler.csproj` | 14 (`<LangVersion>preview</LangVersion>`), 367 (`<Compile Include="Interfaces\IMailItemActions.cs" />`) |
| `QuickFiler.Test\Controllers\MailItemActionsAdapterTests.cs` | 1–96 (read in full) |
| `QuickFiler.Test\Controllers\QfcItemController.SeamCoreTests.cs` | 15, 17, 34, 40, 87, 155, 161 (grep) |
| `QuickFiler.Test\Controllers\QfcItemController.SeamDispatcherTests.cs` | 161, 333 (grep) |
| `QuickFiler.Test\Controllers\QfcItemController.MailActionsTests.cs` | 140, 160, 165 (grep) |
| `QuickFiler\Controllers\QfcItemController.cs` | 25–29 (type declaration), 68 (`_mailActions` field) |
| `QuickFiler\Controllers\QfcItemController.Initialization.cs` | 40, 59, 375–398 (production default construction) |
| `QuickFiler\Controllers\QfcItemController.EventHandlers.cs` | 135 (grep) |
| `QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs` | 322–323 (grep) |
| `QuickFiler\Controllers\QfcItemController.MailActions.cs` | 32, 43 (grep) |
| `QuickFiler\Controllers\QfcItemController.Navigation.cs` | 90, 96, 102 (grep) |
| `docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\qa-gates\coverage-final.cobertura.xml` | 14448–14513 (adapter class entry); grep for `IMailItemActions` class entry returned **no matches** |
| `docs\features\epics\quickfiler-per-file-coverage\epic.md` | 1–419 (read in full); esp. 113 (~24 interface-only files), 132–192 (Shared Design §1–§6), 267–274 (F3 assignment), 405–419 (Known Conflict Risks) |
| `docs\features\active\2026-08-07-quickfiler-keyboard-actions-coverage-430\issue.md` | 1–95 (read in full) |
| `.claude\rules\general-unit-test.md` | 23–29 (Coverage Requirements incl. the interface-only clarification), 31–46 (Coverage Exclusion Policy), 48–57 (Scenario Completeness), 76–80 (Test File Location) |
| `.claude\rules\csharp.md` | 47–54 (DI seam hierarchy), 31–41 (Testing Standards) |
| `CLAUDE.md` | § UT2 COM/VSTO/WinForms coverage exemption; § CUT1–CUT3 C# test framework and toolchain |
