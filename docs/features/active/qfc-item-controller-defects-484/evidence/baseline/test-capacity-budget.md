# Phase 0 — Test-Capacity Budget

Timestamp: 2026-08-26T08-40
Task: [P0-T17]

Command: derived from the `[P0-T15]` measured line counts and the plan's constraint C2 assignment tables.
EXIT_CODE: 0

## Per-file headroom (from `[P0-T15]`)

| File | Baseline lines | Headroom to 500 |
|---|---|---|
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 326 | 174 |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 391 | 109 |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 430 | 70 |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | 224 | 276 |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 497 | 3 |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 374 | 126 |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 474 | 26 |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | 184 | 316 |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 365 | 135 |

## Planned home and approximate size of each test group (constraint C2)

| Test group | Planned home | Approx. added lines |
|---|---|---|
| #480 assertion tightening (in place) | `QfcItemController.FocusAndThemeTests.cs` | 0 |
| #480 `async: true` exact-count test | `QfcItemController.MailActionsTests.cs` | 26 |
| #481 intent-detach test (16 `VerifyRemove`) | `QfcItemController.MailActionsTests.cs` | 48 |
| #481 control-tree unwire test | `QfcItemController.EventWiringTests.cs` | 80 |
| #481 teardown robustness test | `QfcItemController.EventWiringTests.cs` | 26 |
| #483 error-handling and cancellation tests | `QfcItemController.MailActionsTests.cs` | 96 |
| #484 T1 timer-disposal test | `QfcItemController.MailActionsTests.cs` | 14 |
| #484 T2 callback-inertness test | `QfcItemController.MailActionsTests.cs` | 16 |
| #484 `_mailActions` rebind test | `QfcItemController.MailActionsTests.cs` | 26 |
| #485 `TryResolveCidResource` tests | `QfcItemController.MailActionsTests.cs` | 68 |
| Shared arrange helpers (all groups) | `QfcItemController.TestSupport.cs` (helpers only) | 88 |

## Per-file projection

| File | Baseline | Planned additions | Projected |
|---|---|---|---|
| `QfcItemController.FocusAndThemeTests.cs` | 497 | 0 | 497 |
| `QfcItemController.EventWiringTests.cs` | 374 | 106 | 480 |
| `QfcItemController.ViewerSetupTests.cs` | 474 | 0 | 474 |
| `QfcItemController.MailActionsTests.cs` | 184 | 294 | 478 |
| `QfcItemController.TestSupport.cs` | 365 | 88 | 453 |

## Aggregate arithmetic

- **Aggregate headroom across the four original test files:** 3 + 126 + 26 + 316 = **471 lines**, of which
  the 3 lines in `QfcItemController.FocusAndThemeTests.cs` are unusable for a new test, leaving
  **468 usable lines**.
- **Aggregate planned addition to those four files:** 0 + 106 + 0 + 294 = **400 lines**.
- **Resulting margin against the 468 usable lines:** **68 lines**.
- Including `QfcItemController.TestSupport.cs` (135 headroom, 88 planned), the total planned addition
  across all five owned test files is **488 lines**.
- No test file receiving added lines is projected above **480**, leaving a **20-line safety margin** under
  the 500 ceiling for CSharpier reflow.

## The seven numbered capacity rules of constraint C2

1. Every test-adding task's acceptance includes: all five owned test files are at most 500 lines. Where an
   individual task's own acceptance text names four test files, this rule extends that check to include
   `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`.
2. Compaction techniques the executor must use before considering relocation: one shared private arrange
   helper per test group instead of repeated arrange blocks; a `[DataTestMethod]` with one `[DataRow]` per
   case where every case asserts the same outcome shape; folding a second assertion into an existing test
   rather than adding a near-duplicate test method.
3. If a planned home would exceed 500 lines after compaction, the test group may be relocated to a
   different owned test file (never `QfcItemController.TestSupport.cs`, which receives shared arrange
   helpers only), with a header comment naming the issue number. Relocation is permitted; file creation is
   not.
4. If no allocation across the five owned test files fits, the executor must stop, write a blocker artifact
   to `docs/features/active/qfc-item-controller-defects-484/evidence/other/capacity-blocker.md`, and
   report. It must not edit a `.csproj`, create a new file, write a forbidden file, or leave any file above
   500 lines.
5. The two pre-existing headless real-`ItemViewer` tests in
   `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` (at lines 229-309 and 319-372) must
   not be refactored, renamed, or shortened. Preserving them keeps the real-`ItemViewer` construction count
   arithmetic in `[P7-T10]` deterministic.
6. In `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs`, `System` is not imported and
   `Microsoft.Office.Interop.Outlook` is, so a bare `Action` silently binds
   `Microsoft.Office.Interop.Outlook.Action` and a bare `Exception` silently binds
   `Microsoft.Office.Interop.Outlook.Exception`. Do not add `using System;` to that file, and do not add
   any other `System` namespace import to it. Instead write every `System`-namespace type fully qualified
   there. This is a general rule, not a three-name list: besides `System.Action`, `System.Func<Task>`, and
   `System.Exception`, the test groups routed to this file need whichever of `System.Threading.Timer`,
   `System.Threading.Timeout`, `System.Threading.CancellationToken`,
   `System.Threading.CancellationTokenSource`, `System.ObjectDisposedException`,
   `System.InvalidOperationException`, `System.OperationCanceledException`, `System.EventHandler`,
   `System.Delegate`, `System.IAsyncResult`, `System.Uri`, `System.UriKind`, and
   `System.Windows.Forms.KeyEventHandler` their arrange and assert blocks actually use. The generic arities
   differ from the non-generic Outlook types, so `Action<string>` and `Func<Task>` are unambiguous in the
   production file `QuickFiler/Controllers/QfcItemController.MailActions.cs`, which does import `System`;
   that file nonetheless already writes `System.Action` (`:54`) and `System.Exception` (`:115`) for the
   arity-zero names and new code there must follow the same convention.
   `EventWiringTests.cs` and `TestSupport.cs` have no such constraint.
7. **Append-below-the-citation rule.** Every member added to
   `QuickFiler/Controllers/QfcItemController.EventWiring.cs` and every test method added to
   `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` must be appended **after** the last
   existing member of the containing type, never inserted above it, so that the two `spec.md` citations
   `QuickFiler/Controllers/QfcItemController.EventWiring.cs:50` and
   `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs:229-309` still resolve to the same
   source in the delivered file. The same rule applies to
   `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`, whose members are enumerated against
   `<BASE_SHA>` in `[P7-T9]`. This rule preserves exactly those two citations and is the only rule that
   preserves any citation; every other `spec.md` citation into a file this plan edits is a pre-change
   locator anchored to `<BASE_SHA>` and must not be renumbered, deleted, or corrected.

Output Summary: Aggregate usable headroom across the four original test files is 468 lines; the aggregate
planned addition to them is 400 lines, leaving a 68-line margin. Including `TestSupport.cs`, 488 lines are
planned in total. No file receiving added lines projects above 480. All seven numbered capacity rules of
constraint C2 are restated above.
