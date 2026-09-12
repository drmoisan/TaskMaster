# Research: `QuickFiler/Controllers/EmailSorter.cs`

- Parent epic: #136 (`quickfiler-per-file-coverage`)
- Child feature: #431 F2 (`quickfiler-queue-admission-coverage`)
- File under research: `QuickFiler/Controllers/EmailSorter.cs` (85 lines, verified by direct read)
- Evidence basis: direct read of the file; direct read of `QuickFiler.Test/Controllers/EmailSorterTests.cs`.

## Current structure

- Two types in one file: `internal class EmailSorter` and `public interface IEmailSortInfo`.
- `EmailSorter` public surface: two constructors (`EmailSorter()`, `EmailSorter(SortOptionsEnum options)`),
  `Options` (get/set property), `GetSortKey(string triage, DateTime dateTime)`, `GetDateKey(DateTime dateTime)`.
- No constructor-injected dependencies beyond the plain `SortOptionsEnum` value type; the class has no
  external collaborators at all — it is pure logic with two private lookup dictionaries as fixed data.
- No dependency on `Microsoft.Office.Interop.Outlook.*`.
- No concurrency, no RNG. `GetDateKey` reads `DateTime` from its **parameter**, not from `DateTime.Now`
  or any ambient clock — the caller supplies the timestamp, so this file has no wall-clock read of its
  own.
- `IEmailSortInfo` is a plain data-shape interface (six read-only properties) with no implementing class
  in this file and no logic — declaration-only.
- Dead field: `_triageImportantFirst` is initialized but never read anywhere in the class; `GetSortKey`
  always reads `_triageImportantLast` regardless of any option flag. This is a pre-existing code
  characteristic, not introduced by this research; noted because it affects which dictionary a
  triage-key test should target (always `_triageImportantLast`, matching the current, unchanged
  behavior — this is a coverage/testing note only, not a behavior-change recommendation, per the
  "no behavior change" constraint on this child).

## Existing test coverage

`EmailSorterTests.cs` (6 tests): `Constructor_Default_UsesDefaultSortOptions`,
`Constructor_WithOptions_UsesProvidedSortOptions`, `GetDateKey_WithKnownDate_ReturnsSortableTimestampKey`,
`GetSortKey_WithSupportedTriage_ReturnsExpectedCompositeKey` (a `[DataRow]`-parameterized test covering
triage values `A`/`B`/`C`/`Z`), `GetSortKey_WithUnsupportedTriage_PropagatesKeyNotFoundException`.

Covered: both constructors; `GetDateKey`; `GetSortKey`'s success path for all four dictionary keys under
`TriageImportantFirst | DateRecentFirst`; the `KeyNotFoundException` propagation for an unsupported
triage value under the same flag combination.

## Coverage gap

- **`GetSortKey`'s `return -1` fallback branch** — reached whenever the caller's `_options` does not have
  **both** `TriageImportantFirst` and `DateRecentFirst` set (e.g., the default constructor's
  `SortOptionsEnum.Default`, or either flag alone). No existing test calls `GetSortKey` with any option
  combination other than `TriageImportantFirst | DateRecentFirst`; this early-return branch is entirely
  unexercised.
- **`Options` property setter** — only the constructor-driven value is asserted (via the getter); no
  test calls the setter directly after construction and re-reads `Options` to confirm the value changed.

## `[ExcludeFromCodeCoverage]` disposition

Not applicable — this file carries no such attribute.

## Seam requirements

None. The class has no external collaborators; every gap above is closed with plain unit tests against
the existing public surface.

## Candidate test cases

| # | Case | Type | Notes |
|---|---|---|---|
| 1 | `GetSortKey` with `Options == SortOptionsEnum.Default` returns `-1` for any triage/date input | Boundary/negative | Exercises the fallback `return -1;` branch via the default constructor |
| 2 | `GetSortKey` with only `TriageImportantFirst` set (no `DateRecentFirst`) returns `-1` | Boundary | Confirms the `&&` requires both flags, not either |
| 3 | `GetSortKey` with only `DateRecentFirst` set (no `TriageImportantFirst`) returns `-1` | Boundary | Confirms the `&&` requires both flags, not either |
| 4 | `Options` setter changes the property value, observable via a subsequent `GetSortKey` call whose outcome differs from the pre-set value (e.g., set to `Default` after constructing with both flags, then observe the `-1` fallback) | Positive/state-transition | Exercises the setter directly rather than only via constructor |

## Determinism constraints

None required. `GetDateKey`/`GetSortKey` take their `DateTime` as a parameter; tests should continue to
supply fixed literal `DateTime` values (as the existing suite already does) rather than reading
`DateTime.Now`. No RNG is used in this file.
