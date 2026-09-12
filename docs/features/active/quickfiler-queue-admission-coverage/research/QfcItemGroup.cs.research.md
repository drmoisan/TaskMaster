# Research: `QuickFiler/Controllers/QfcItemGroup.cs`

- Parent epic: #136 (`quickfiler-per-file-coverage`)
- Child feature: #431 F2 (`quickfiler-queue-admission-coverage`)
- File under research: `QuickFiler/Controllers/QfcItemGroup.cs` (52 lines, verified by direct read)
- Evidence basis: direct read of the file; grep for `QfcItemGroup(` and `new QfcItemGroup` usage across
  `QuickFiler.Test`; direct read of the constructing helper in `QfcQueueCoverageExpansionTests.cs`.

## Current structure

- `public class QfcItemGroup` — a plain data-carrier/DTO with no behavior beyond property get/set.
  Two constructors: parameterless `QfcItemGroup()` and `QfcItemGroup(MailItem mailItem)`.
- Properties: `MailItem` (internal get/set, backed by `_mailItem`), `ItemViewer` (internal get/set,
  backed by `_itemViewer`, typed as the concrete `QuickFiler.Viewers.ItemViewer`), `ItemController`
  (internal get/set, backed by `_itemController`, typed as `IQfcItemController` — already an interface),
  `PredeterminedFolder` (internal get/set auto-property, added for issue #171's high-confidence carrier
  path).
- No constructor-injected dependencies beyond the optional `MailItem` parameter (a data value, not a
  live-COM construction — the type is the mockable COM interface, and every test in this codebase
  already constructs it via `new Mock<MailItem>()`).
- No dependency on `Microsoft.Office.Interop.Outlook.Application/Store/MAPIFolder`. `MailItem` is a
  plain data field.
- No concurrency, no RNG, no wall-clock usage. The class has no logic branches at all — every member is
  a field, a property accessor, or a constructor assignment.
- Unused `using static QuickFiler.Controllers.QfcCollectionController;` import (line 12) — a static-using
  directive with no visible reference inside this file's body; harmless but worth flagging as removable
  in a future cleanup pass (out of scope for a coverage-only child per the "no behavior change" rule,
  since removing an unused `using` has no coverage or behavior effect and is not required to reach the
  file's coverage target).

## Existing test coverage

No dedicated test file exists (`QfcItemGroupTests.cs` is absent from `QuickFiler.Test/Controllers/`).
`QfcItemGroup` instances are constructed pervasively as **test fixtures** for other files' tests — most
directly in `QfcQueueCoverageExpansionTests.cs`, whose `NewGroup(mailItem, controller)` helper
(`return new QfcItemGroup(mailItem) { ItemController = controller };`) is used by seven tests in that
file, and in `QfcQueueTests.cs`/`QfcCollectionControllerTests.cs`/`QfcFormControllerTests.cs`/
`QfcCollectionControllerDarkModeTests.cs`. Through this incidental usage, the `QfcItemGroup(MailItem)`
constructor, the `MailItem` getter, and the `ItemController` get/set are exercised as a side effect of
those files' own tests (which is legitimate coverage — line-coverage tooling does not distinguish
"exercised directly" from "exercised as a fixture").

`QfcQueueCoverageExpansionTests.Dequeue_WithHighConfidenceCarrier_PreservesPredeterminedFolder` (line
194) directly sets and re-reads `PredeterminedFolder`, so that property's get/set is also exercised as a
side effect of a `QfcQueue`-focused test.

## Coverage gap

- The **parameterless constructor** `QfcItemGroup()` is never called anywhere in the current test suite
  (every fixture helper uses `QfcItemGroup(MailItem)`). Its own line is unexercised.
- The **`ItemViewer` property get/set** (backed by `_itemViewer`, typed as the concrete `ItemViewer`) is
  never exercised anywhere in `QuickFiler.Test` today — no fixture helper sets it, because doing so would
  require constructing a real `ItemViewer` (a `UserControl`, owned by F14's file set), which the existing
  `QfcQueue`-focused fixtures deliberately avoid.
- No test constructs `QfcItemGroup` and asserts on the `MailItem` getter or the `PredeterminedFolder`
  default value (`null`) in isolation, independent of another file's test fixture.

## `[ExcludeFromCodeCoverage]` disposition

Not applicable — this file carries no such attribute.

## Seam requirements

None. Every property is already a plain, mockable/settable member; no COM object is constructed inside
this file. The only friction is that a *direct, isolated* test of the `ItemViewer` property would need
either a real (never-shown) `ItemViewer` instance — feasible per the #227 headless-construction precedent
cited in the `QfcQueue.cs` research for this child — or simply assigning `null`/a placeholder value,
since `QfcItemGroup` itself does not validate or use the `ItemViewer` value in any way (it is a pure
property, so a null or dummy `ItemViewer` reference is sufficient to exercise the getter/setter line
without needing a fully-initialized control).

## Candidate test cases

| # | Case | Type | Notes |
|---|---|---|---|
| 1 | `QfcItemGroup()` parameterless constructor leaves `MailItem` as its default (`null`) | Positive/boundary | Closes the sole uncalled constructor |
| 2 | `QfcItemGroup(MailItem)` constructor stores the supplied `MailItem` reference, retrievable via the getter | Positive | Direct, isolated assertion (currently only exercised as a side effect of other files' fixtures) |
| 3 | `ItemViewer` property setter/getter round-trips a reference without transformation | Positive | Use a dummy/no-op reference (e.g., `null`, or a real never-shown `ItemViewer` if the atomic-planner prefers exercising the concrete type) — no COM/live-Outlook dependency either way |
| 4 | `ItemController` property setter/getter round-trips an `IQfcItemController` mock reference | Positive | Already implied by existing fixtures, but not asserted directly against a freshly constructed `QfcItemGroup` outside another file's test — add one direct assertion to close the gap cleanly |
| 5 | `PredeterminedFolder` defaults to `null` on a freshly constructed instance and round-trips any assigned string, including `string.Empty` | Positive/boundary | Direct, isolated assertion |

## Determinism constraints

None required. The class has no clock, RNG, or concurrency surface.
