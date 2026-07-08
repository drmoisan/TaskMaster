# Issue #218 Changed-Line Coverage Gap — Cycle 2, Issue #218

Timestamp: 2026-06-28T17-31

Command: Map the issue #218 production diff vs merge-base `1b8536b6` (`git diff 1b8536b6 -- QuickFiler/Controllers/*.cs`) to the line hits in `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/coverage-final-218.cobertura.xml`, supplemented by source-level branch analysis of the #218 admission seam against the existing focused tests (`QfcDatamodelTests.cs`). Class-mention grep over the Cobertura: `QfcRemainingQueueAdmission` = 0, `QfcHomeController` = 50, `QfcDatamodel` = 10, `EmailSorter` = 4.

EXIT_CODE: 0

## Denominator dispositions of the #218 admission / initial-load code

1. **QfcDatamodel admission/initial-load methods** (`TryQueueRemainingMailItemAsync`/scoring helpers, `LoadRemainingEmailsToQueueAsync`, `InitEmailQueueAsync`): the `QfcDatamodel` partial class carries `[ExcludeFromCodeCoverage]` (QfcDatamodel.cs:24) and is COM-host-bound; all its partials inherit this disposition. These lines are EXCLUDED from the testable coverage denominator per the CLAUDE.md COM/VSTO exemption — they are NOT "uncovered." They are nonetheless exercised behaviorally by the focused `QfcDatamodelTests` and the home-controller initial-load tests.
2. **QfcHomeController initial-load (`RunAsync`)**: the cycle-1 changed-line evidence recorded the single changed home-controller line (then line 277) as COVERED. QfcHomeController is not `[ExcludeFromCodeCoverage]`. Reconfirmed numerically in P5-T7 against the regenerated final Cobertura.
3. **QfcRemainingQueueAdmission seam (`TryQueueAsync`)**: this `internal sealed class` (NOT excluded) is the testable #218 admission seam and is entirely NEW (extracted by commit `7905efa1`). It is ABSENT from `coverage-final-218.cobertura.xml` (0 mentions) because that Cobertura predates the extraction. Authoritative numeric coverage is measured in P5-T7 against the regenerated final Cobertura. Source-level branch analysis vs the 4 existing focused tests follows.

## QfcRemainingQueueAdmission.TryQueueAsync branch coverage vs existing focused tests

The 4 existing focused tests (`QfcDatamodelTests.TryQueueRemainingMailItemAsync_*`) call `admission.TryQueueAsync` directly and cover:
- line 35 `cancel.ThrowIfCancellationRequested()` — covered (all 4).
- line 42 high-confidence branch + lines 44-46 cutoff/score — covered (enabled / equal / below).
- line 49 `return false` (score < cutoff) — covered (below-threshold test).
- lines 53-55 `_addToQueue` / `_hookItem` / `return true` — covered (enabled / equal / disabled).
- line 37 `if (mailItem is null)` — evaluated (false) in all tests.

## Uncovered issue #218 admission/initial-load line

- **QuickFiler/Controllers/QfcRemainingQueueAdmission.cs:39** — `return false;` inside the `if (mailItem is null)` guard. This new, in-denominator admission line is NOT exercised by any existing test (every existing test passes a non-null Mock<MailItem>). This is the smallest, single uncovered admission line in the #218 testable seam.

## P4-T2 disposition — minimal test added

Added the smallest deterministic MSTest covering `QfcRemainingQueueAdmission.cs:39`:
- Test: `QuickFiler.Controllers.Tests.QfcDatamodelTests.TryQueueRemainingMailItemAsync_NullMailItem_DoesNotScoreAddOrHook`.
- File: `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` (already wired into `QuickFiler.Test/QuickFiler.Test.csproj` at the existing `Controllers\QfcDatamodelTests.cs` include — no new csproj entry required).
- Design: MSTest + Moq + FluentAssertions, Arrange-Act-Assert. Passes `null` to `TryQueueAsync`; asserts the result is `false` and that nothing was added or hooked. The score-loader throws `AssertFailedException` if invoked, proving the null item is rejected before scoring (covers the early-return null guard). No live Outlook COM (the mail item is null), no temporary files, deterministic.
- This test targets issue #218 changed code (the new admission seam) directly; it does NOT add out-of-scope coverage to raise the repo-wide figure.

Final numeric changed-line coverage for the #218 production diff is confirmed in P5-T7 against the regenerated final Cobertura (which will, unlike the cycle-1 artifact, contain QfcRemainingQueueAdmission).

Output Summary: Uncovered issue #218 admission/initial-load lines (in-denominator): exactly ONE — `QfcRemainingQueueAdmission.cs:39` (null-mailItem guard `return false;`). QfcDatamodel #218 methods are `[ExcludeFromCodeCoverage]` (excluded, not uncovered); QfcHomeController initial-load is covered. P4-T2 added one minimal deterministic MSTest (`TryQueueRemainingMailItemAsync_NullMailItem_DoesNotScoreAddOrHook`) for that line; final numeric changed-line coverage confirmed in P5-T7.
