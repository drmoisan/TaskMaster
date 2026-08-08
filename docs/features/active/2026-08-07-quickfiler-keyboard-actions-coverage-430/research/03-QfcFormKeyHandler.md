# Per-File Coverage Research — `QuickFiler/Controllers/QfcFormKeyHandler.cs`

Timestamp: 2026-08-07T21-55
Feature: `quickfiler-keyboard-actions-coverage` (issue #430, epic child F3, wave 1)
Epic: `quickfiler-per-file-coverage` (issue #136)
Branch: `feature/quickfiler-keyboard-actions-coverage`

---

## 1. File Under Research

| Attribute | Value |
| --- | --- |
| Path | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aafcc2531072ca96b\QuickFiler\Controllers\QfcFormKeyHandler.cs` |
| Line count | 20 |
| Type | `internal static class QfcFormKeyHandler` (line 10), namespace `QuickFiler.Controllers` |
| Compiled | Yes — `QuickFiler/QuickFiler.csproj:321` `<Compile Include="Controllers\QfcFormKeyHandler.cs" />` |
| `[ExcludeFromCodeCoverage]` | **Absent.** No attribute on the file or the type. |
| Existing tests | **Yes** — `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` (67 lines, 4 `[TestMethod]`s) |
| Test-assembly access | `QuickFiler/Properties/AssemblyInfo.cs:5` — `[assembly: InternalsVisibleTo("QuickFiler.Test")]`. The `internal static` member is called directly with no reflection (`QfcFormKeyHandlerTests.cs:22`). |
| Exemption-status authority | **F1's `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`.** Recommended classification: `testable`, already at 100% line coverage. |
| Per-file coverage measurement | Numeric per-file line coverage will be measured at execution time with F1's harness (derived from the Cobertura output of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`). Static analysis (§3) predicts 100% line and 100% branch. |

### Origin

The file is the product of a prior seam extraction. Its own XML documentation (lines 5–9) records the rationale: *"Pure routing predicates extracted from the QuickFiler form variants' `ProcessCmdKey` overrides so the key-command logic can be unit tested without a live `Form` window handle."* The existing test class documents it as "Seam A" (`QfcFormKeyHandlerTests.cs:10`). This file is therefore the in-cluster precedent for the exact refactor pattern epic child F3 must apply to `KeyboardHandler.cs`: pull the host-neutral predicate out of the `Form`-derived host, leave the thinnest possible wiring behind.

---

## 2. Structural Inventory

| Lines | Declaration | Kind | Dependencies | Seam-isolatable? |
| --- | --- | --- | --- | --- |
| 1 | `using System.Windows.Forms;` | using | WinForms — for the `Keys` **enum only** (a value type; no control, no handle, no UI thread) | n/a |
| 3 | `namespace QuickFiler.Controllers` | namespace | — | n/a |
| 5–9 | XML `<summary>` on the type | doc comment | — | n/a |
| 10 | `internal static class QfcFormKeyHandler` | type declaration | — | n/a |
| 12–17 | XML `<summary>`/`<param>`/`<returns>` on the method | doc comment | — | n/a |
| 18 | `internal static bool IsAltKeyCommand(Keys keyData) => keyData.HasFlag(Keys.Alt);` | expression-bodied static method | `System.Enum.HasFlag` (BCL, pure), `Keys.Alt` (`0x00040000`) | **Already directly testable. No seam needed.** |
| 19–20 | closing braces | — | — | n/a |

**Totals: 1 type, 1 method, 0 constructors, 0 fields, 0 properties, 0 events, 0 nested types, 1 executable statement.**

### Dependency assessment

- **Outlook Interop:** none.
- **WinForms controls / forms / handles:** none. `Keys` is an `[Flags]` enum in `System.Windows.Forms`; referencing it requires no window, no message pump, no STA apartment.
- **UI thread:** none.
- **Static or mutable global state:** none. The method is a pure function of its single argument.
- **Other QuickFiler controllers:** none.
- **I/O, clock, RNG:** none.

The method is total (defined for every `Keys` value), deterministic, side-effect-free, and allocation-free. It is the cleanest testable surface in the entire F3 file set.

### Branch analysis

`Enum.HasFlag(Keys.Alt)` compiles to a single call with no branch inside `QfcFormKeyHandler`'s own IL. The expression body has **1 sequence point and 0 decision points**, so line coverage and branch coverage are the same measurement for this file: either the one statement runs or it does not.

---

## 3. Existing Test Coverage (static analysis)

Test file: `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` — `[TestClass] public class QfcFormKeyHandlerTests` (line 13), MSTest + FluentAssertions, Arrange–Act–Assert throughout, no mocks needed, no external dependency, no temporary files, no sleeps.

| Member / branch (line) | Exercised by | Input | Expected | Status |
| --- | --- | --- | --- | --- |
| `IsAltKeyCommand` (18) — statement | `IsAltKeyCommand_WithAltKey_ReturnsTrue` (`QfcFormKeyHandlerTests.cs:15–26`) | `Keys.Alt` | `true` | covered |
| `IsAltKeyCommand` (18) — combined-modifier input | `IsAltKeyCommand_WithAltPlusOtherKey_ReturnsTrue` (`:28–39`) | `Keys.Alt \| Keys.Left` | `true` | covered |
| `IsAltKeyCommand` (18) — non-Alt modifier | `IsAltKeyCommand_WithControlKey_ReturnsFalse` (`:41–52`) | `Keys.Control` | `false` | covered |
| `IsAltKeyCommand` (18) — zero input | `IsAltKeyCommand_WithNone_ReturnsFalse` (`:54–65`) | `Keys.None` | `false` | covered |
| type declaration (10) | — | — | — | no IL emitted for a static class with no static ctor and no fields |

**Static-analysis conclusion: `QfcFormKeyHandler.cs` is already at 100% line coverage and 100% branch coverage.** The single executable statement is exercised by four independent test methods, covering `true` from two distinct input shapes and `false` from two distinct input shapes.

Each of the four tests already satisfies the General Unit Test Policy checklist: descriptive name, explicit Arrange/Act/Assert comment sections, a FluentAssertions `because` string on every assertion, no shared state, no ordering dependency.

---

## 4. Coverage Gaps

**No line-coverage gap.** The file is already at the epic's 80% floor and above the 90% new-code floor.

Two *scenario-completeness* gaps remain, however, and they matter more than the coverage number. `.claude/rules/general-unit-test.md` § Scenario Completeness requires positive flows, negative flows, **edge cases and boundary conditions**, and error handling. Both gaps are boundary conditions arising from the actual production call sites, and neither is currently pinned:

### Gap G1 — `Keys.Menu` (the raw ALT key, value `18`) is untested and returns `false`

This is the highest-value missing case. `Keys.Alt` is the **modifier flag** `0x00040000`; `Keys.Menu` is the **key code** for the physical ALT key, value `0x12` (18). `Keys.Menu.HasFlag(Keys.Alt)` is `18 & 0x40000 != 0` → **`false`**.

This distinction is load-bearing at the production call sites. `ProcessCmdKey` reports `keyData` as key-code-plus-modifiers, so pressing and holding ALT alone typically surfaces as `Keys.Menu` or `Keys.Menu | Keys.Alt` depending on the message, while ALT+X surfaces as `Keys.X | Keys.Alt`. Nothing currently documents which of those the predicate is intended to catch. A test pinning `Keys.Menu → false` and `Keys.Menu | Keys.Alt → true` turns an implicit assumption into an executable specification and would catch a future "simplification" from `HasFlag(Keys.Alt)` to a `Keys.Menu` comparison (or vice versa).

### Gap G2 — combined-modifier boundaries are only half-covered

`Keys.Alt | Keys.Left` is covered; the neighbouring shapes are not:
- `Keys.Control | Keys.Shift` (multiple non-Alt modifiers) → `false`
- `Keys.Control | Keys.Alt` (Alt present alongside another modifier) → `true`
- `Keys.Alt | Keys.Control | Keys.Shift | Keys.A` (saturated) → `true`

`HasFlag` semantics make these predictable, but the production call sites are `ProcessCmdKey` overrides that receive arbitrary modifier combinations from the OS, so pinning them costs four cheap tests and removes an entire class of future regression.

### Not a gap

- **Null input:** impossible. `Keys` is a non-nullable value type and the parameter is not `Keys?`.
- **Error handling:** the method cannot throw. `Enum.HasFlag` on a same-typed argument raises no exception, and there is no allocation, cast, or dereference.
- **Concurrency / state transitions:** not applicable. The method is stateless and pure.
- **Determinism infrastructure (clock, RNG):** not applicable. No time or randomness is involved.

---

## 5. Seam Requirements

**None required. The file is directly testable and already tested.**

Assessment against the seam hierarchy in `.claude/rules/csharp.md` § DI Seams and `epic.md` Shared Design §2:

| Hierarchy level | Applicable? | Reason |
| --- | --- | --- |
| 1 — Interface seam | **No** | There is no boundary call to extract. The method touches no process, HTTP, filesystem, clock, COM object, or UI surface. |
| 2 — Injectable delegate | **No** | There is no single call path to redirect; the entire body is one BCL pure call. |
| 3 — Adapter for static/third-party API | **No** | `Enum.HasFlag` is a deterministic BCL pure function requiring no isolation. Wrapping it would be the "heavy abstraction without need" that `.claude/rules/csharp.md` § Prohibited Behaviors forbids. |

**This file already *is* the seam.** Its purpose (lines 5–9) is to be the host-neutral extraction that lets three `Form`-derived viewers be reasoned about without a window handle. Adding another indirection layer would be a regression in simplicity against the General Code Change Policy § Design Principles ("Simplicity first — avoid cleverness and deep indirection").

### F1 ledger classification recommendation

`testable` — currently at 100% line coverage. Explicitly **not** a candidate for `[ExcludeFromCodeCoverage]` and **not** an exemption candidate. This file should be cited in the F1 ledger as the positive exemplar of the refactor pattern the epic mandates.

---

## 6. Cross-Child Contract Impact

### 6.1 Determination

**ADDITIVE — because no production change is proposed at all.** F3's work on this file is test-only: adding boundary test cases to the existing `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`. No signature, no visibility, no behavior, and no file-level attribute changes.

### 6.2 Call sites (exhaustive)

Verified by repo-wide grep for `QfcFormKeyHandler` and `IsAltKeyCommand` across all `*.cs`. There are exactly three production call sites, all in `ProcessCmdKey` overrides on `Form`-derived viewers.

| # | File : line | Owning child | Call shape and surrounding behavior |
| --- | --- | --- | --- |
| S1 | `QuickFiler/Viewers/QfcFormViewer.cs:56–73` | **F15** (`quickfiler-form-viewers-bayesian-coverage`; the file carries `[ExcludeFromCodeCoverage]` per `epic.md:366`) | `if ((_keyboardHandler is not null) && Controllers.QfcFormKeyHandler.IsAltKeyCommand(keyData))` → sets the sync context, builds a `KeyEventArgs`, sets `e.Handled = true`, fires `_ = _keyboardHandler.ToggleKeyboardDialogAsync()` (fire-and-forget), returns `true`. Note the **null guard on `_keyboardHandler` precedes the predicate**. |
| S2 | `QuickFiler/Viewers/QfcFormViewerExpanded.cs:41–53` (predicate at line 43) | **unassigned in `epic.md`** — see §7 R-2 | `if (QfcFormKeyHandler.IsAltKeyCommand(keyData))` → `_keyboardHandler.KeyboardHandler_KeyDown(sender, e)`, returns `true`. **No null guard on `_keyboardHandler`.** |
| S3 | `QuickFiler/Viewers/QfcFormViewerDark.cs:41–53` (predicate at line 43) | **unassigned in `epic.md`** — see §7 R-2 | Identical to S2: `if (QfcFormKeyHandler.IsAltKeyCommand(keyData))` → `_keyboardHandler.KeyboardHandler_KeyDown(sender, e)`, returns `true`. **No null guard.** |

Test-side call sites: `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs:22, 35, 48, 61` — four direct static invocations.

**No sibling-owned file is touched by F3's work on this file.** The three `ProcessCmdKey` overrides are `Form`-derived host wiring belonging to F15 and (pending correction) to no child; F3 neither reads nor modifies them.

### 6.3 Changes explicitly REJECTED

| Tempting change | Determination | Rationale |
| --- | --- | --- |
| Widen `IsAltKeyCommand` from `internal` to `public` | **Rejected** — unnecessary. `InternalsVisibleTo("QuickFiler.Test")` already grants test access (`QuickFiler/Properties/AssemblyInfo.cs:5`), and `.claude/rules/csharp.md` § Coding Standards says "Prefer `internal` for non-public APIs." All three call sites are in the same assembly. |
| Add a second predicate (e.g. `IsAltKeyOnly`, `IsNavigationKey`) to unify S1/S2/S3, which currently diverge — S1 fires `ToggleKeyboardDialogAsync()`, S2/S3 fire `KeyboardHandler_KeyDown` | **Rejected as out of scope.** Unifying them would change observable behavior in F15-owned and unassigned viewer files, violating issue #430's "No behavior change to observable QuickFiler keyboard flows" and the epic's additive mandate. Promote as a follow-up issue instead (§7 R-3). |
| Move the null-guard from S1 into `IsAltKeyCommand` | **Rejected.** The guard is on `_keyboardHandler`, a field of the viewer, not on `keyData`. It cannot move into a static predicate over `Keys`. Recording the S1-vs-S2/S3 asymmetry as a latent-defect follow-up is the correct response (§7 R-3). |
| Add `[ExcludeFromCodeCoverage]` | **Rejected.** The file is at 100%. Adding the attribute here would be exactly the unratified-exemption pattern the epic exists to remove. |

---

## 7. Proposed Test Cases

All new cases are additions to the **existing** `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` (currently 67 lines; projected ~180 lines after these additions — comfortably under the 500-line ceiling). No new test file is required. Per the epic's per-file mandate, each case below becomes its own atomic plan task.

Conventions for every case: MSTest `[TestMethod]`, Arrange–Act–Assert with explicit section comments matching the existing four tests, FluentAssertions with a `because` string, **no mocks and no seams required** (the SUT is a pure static function).

| # | Test method | Arrange | Act | Assert | Category | Seam/mock |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | `IsAltKeyCommand_WithMenuKeyCode_ReturnsFalse` | `var keyData = Keys.Menu;` | `QfcFormKeyHandler.IsAltKeyCommand(keyData)` | `Should().BeFalse("Keys.Menu is the ALT key code (0x12), not the Keys.Alt modifier flag (0x40000)")` | **boundary — Gap G1** | none |
| 2 | `IsAltKeyCommand_WithMenuKeyCodePlusAltModifier_ReturnsTrue` | `var keyData = Keys.Menu \| Keys.Alt;` | as above | `Should().BeTrue("holding ALT reports the Menu key code together with the Alt modifier flag")` | **boundary — Gap G1** | none |
| 3 | `IsAltKeyCommand_WithControlAndShiftModifiers_ReturnsFalse` | `var keyData = Keys.Control \| Keys.Shift;` | as above | `Should().BeFalse("no Alt flag is present regardless of how many other modifiers are set")` | negative / Gap G2 | none |
| 4 | `IsAltKeyCommand_WithControlPlusAltModifiers_ReturnsTrue` | `var keyData = Keys.Control \| Keys.Alt;` | as above | `Should().BeTrue("the Alt flag is set even alongside another modifier")` | boundary / Gap G2 | none |
| 5 | `IsAltKeyCommand_WithAllModifiersAndLetterKey_ReturnsTrue` | `var keyData = Keys.Alt \| Keys.Control \| Keys.Shift \| Keys.A;` | as above | `Should().BeTrue("the predicate tests only the Alt flag and ignores every other bit")` | boundary / Gap G2 | none |
| 6 | `IsAltKeyCommand_WithShiftModifierOnly_ReturnsFalse` | `var keyData = Keys.Shift;` | as above | `Should().BeFalse("the Shift modifier is not an Alt-key command")` | negative | none |
| 7 | `IsAltKeyCommand_WithArrowKeysUsedByFormNavigation_ReturnsFalse` | `[DataRow(Keys.Up)] [DataRow(Keys.Down)] [DataRow(Keys.Left)] [DataRow(Keys.Right)]` on a `[DataTestMethod]` | as above | `Should().BeFalse(...)` for each — pins that unmodified arrows fall through to `base.ProcessCmdKey` at all three call sites | negative, drawn directly from the commented intent at `QfcFormViewerDark.cs:45` | none |
| 8 | `IsAltKeyCommand_WithKeyCodeValueMask_IsUnaffectedByKeyCodeBits` | `var keyData = (Keys)0x0000FFFF;` (all key-code bits set, no modifier bits) | as above | `Should().BeFalse("the key-code region of the Keys bitfield carries no Alt information")` | boundary — bitfield partition | none |

**Total: 8 new discrete test cases** (case 7 is a single `[DataTestMethod]` with four `[DataRow]`s; if the planner prefers one atomic task per assertion it expands to 4, giving 11).

### Coverage effect

Line and branch coverage remain at 100% — they are already there. **The value of these cases is scenario completeness and regression protection, not the coverage number.** The plan and `spec.md` should state that explicitly so a reviewer does not read "8 new tests, 0% coverage delta" as wasted effort. This is also the honest answer to issue #430's acceptance criterion *"Coverage per file spans the positive path plus invalid-input, boundary, and error-handling behavior"*: the existing four tests satisfy positive and negative flows; cases 1–8 close the boundary dimension. Invalid-input and error-handling are structurally not applicable here (§4) and that should be recorded as such rather than manufactured.

### Sequencing note for the planner

These 8 cases are the cheapest, lowest-risk tasks in child F3 and depend on nothing — no seam, no new production file, no F1 artifact beyond the classification. They are good Phase 1 candidates that establish the test-file conventions before the `KeyboardHandler.cs` seam work in `01-KeyboardHandler.md` begins.

---

## 8. Risks and Open Questions

| # | Risk / question | Assessment | Proposed handling |
| --- | --- | --- | --- |
| R-1 | **A reviewer may read "no coverage delta" as "no value"** and challenge the 8 proposed tests. | Low. | State the rationale explicitly in `spec.md` and in the PR body: the file is already at 100%, and these cases discharge the *boundary* limb of the acceptance criterion, not the coverage limb. Cite §4 Gap G1 as the concrete regression the `Keys.Menu` case prevents. |
| R-2 | **`QfcFormViewerExpanded.cs` and `QfcFormViewerDark.cs` (call sites S2 and S3) appear in no child's file assignment** in `epic.md` § Feature File Assignments, yet both consume `QfcFormKeyHandler` and `IQfcKeyboardHandler`. | Low for F3 (nothing to edit), but a real gap in the epic's "every one of the 121 compiled files is assigned to exactly one child" claim. | Report to the epic orchestrator and to the F16 capstone. Confirm against `QuickFiler/QuickFiler.csproj` whether both are `<Compile Include>`d; if so, the assignment table needs correcting. **Out of scope for F3 to fix.** Same finding is recorded in `01-KeyboardHandler.md` §10 R-4 and `02-IQfcKeyboardHandler.md` §8 R-3. |
| R-3 | **The three call sites diverge behaviorally on the same predicate.** S1 (`QfcFormViewer.cs:56–73`) null-guards `_keyboardHandler` and fires the fire-and-forget `ToggleKeyboardDialogAsync()`; S2 and S3 (`QfcFormViewerExpanded.cs:41–53`, `QfcFormViewerDark.cs:41–53`) do **not** null-guard and fire the synchronous `KeyboardHandler_KeyDown(sender, e)` instead. S2/S3 therefore carry a latent `NullReferenceException` if ALT is pressed before `SetKeyboardHandler` has been called. Separately, `KeyboardHandler_KeyDown` is the member whose only other production wiring is commented out (`EfcItemController.cs:651`), so S2/S3 may be the sole live consumers of the synchronous path. | Medium as a latent defect; **out of scope** as a fix (F15-owned and unassigned files; would be a behavior change). | Promote as its own GitHub issue through the MCP promotion lifecycle so the finding survives the feature-folder merge. Do **not** fix it in F3. |
| R-4 | **Ambiguity about which key shape `IsAltKeyCommand` is meant to catch** (`Keys.Alt` modifier vs `Keys.Menu` key code) is currently undocumented, which is what makes Gap G1 a live risk rather than a theoretical one. | Medium. | Proposed cases 1 and 2 convert the assumption into an executable specification. Additionally, extend the XML `<returns>` doc at `QfcFormKeyHandler.cs:17` with one clarifying sentence distinguishing `Keys.Alt` (modifier flag `0x40000`) from `Keys.Menu` (key code `0x12`). That is a comment-only change to an F3-owned file — additive and safe. |
| R-5 | **The file is 20 lines and could be seen as a candidate for inlining back into the viewers**, undoing the seam. | Very low, but worth pinning. | The XML doc at lines 5–9 already records why the extraction exists. No action; noted so a future reviewer does not "simplify" the seam away. |
| R-6 | **Rebase collisions.** In-flight features #400 and #424 touch QuickFiler. Neither touches `QfcFormKeyHandler.cs` or `QfcFormKeyHandlerTests.cs`. | Very low. | None required. |

---

## 9. Sources

All paths relative to `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aafcc2531072ca96b\`.

**Policy**
- `CLAUDE.md` — § UT2 (coverage floors, testable denominator, COM/VSTO exemption), § CUT1–CUT3 (MSTest/Moq/FluentAssertions, toolchain commands)
- `.claude/rules/general-unit-test.md` — § Core Principles, § Scenario Completeness, § Coverage Requirements, § Test File Location
- `.claude/rules/csharp.md:47–53` (DI seam hierarchy), `:21–29` (Coding Standards — prefer `internal`), `:89–96` (Prohibited Behaviors)
- `.claude/rules/general-code-change.md` — § Design Principles ("Simplicity first"), § File Size Limit

**Feature / epic**
- `docs/features/epics/quickfiler-per-file-coverage/epic.md:132–192` (Shared Design §§1–6), `:267–275` (F3 assignment), `:364–373` (F15 assignment, `QfcFormViewer.cs` marked `[X]`), `:405–418` (Known Conflict Risks)
- `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/issue.md`

**File under research**
- `QuickFiler/Controllers/QfcFormKeyHandler.cs:1–20` (read in full)
- `QuickFiler/QuickFiler.csproj:321`

**Existing tests**
- `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs:1–67` (read in full; 4 `[TestMethod]`s at lines 15, 28, 41, 54)

**Call sites**
- `QuickFiler/Viewers/QfcFormViewer.cs:51–73` (S1 — `virtual SetKeyboardHandler`, `ProcessCmdKey` with null guard and `ToggleKeyboardDialogAsync()`)
- `QuickFiler/Viewers/QfcFormViewerExpanded.cs:36–53` (S2)
- `QuickFiler/Viewers/QfcFormViewerDark.cs:36–53` (S3)

**Access and related contracts**
- `QuickFiler/Properties/AssemblyInfo.cs:5` (`InternalsVisibleTo("QuickFiler.Test")`)
- `QuickFiler/Interfaces/IQfcKeyboardHandler.cs:14, 15, 17` (`ToggleKeyboardDialogAsync`, `KeyboardHandler_KeyDown` — the two members the three call sites dispatch to)
- `QuickFiler/Controllers/EfcItemController.cs:651` (the commented-out `KeyboardHandler_KeyDown` wiring, evidence for R-3)

**Related research**
- `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/research/01-KeyboardHandler.md`
- `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/research/02-IQfcKeyboardHandler.md`

**Tooling**
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (F1 harness input)
- `TaskMaster.runsettings:1–30`
