# Research: `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs`

- Parent epic: #136 (`quickfiler-per-file-coverage`)
- Child feature: #431 F2 (`quickfiler-queue-admission-coverage`)
- File under research: `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` (48 lines, verified by direct read)
- Evidence basis: direct read of the file; direct read of `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`
  lines 1-260 (the file's only consumer of this type in tests); the issue #424 spec's confirmation that
  "admission never scores" (issue #233 contract) is untouched and test-pinned.

## Current structure

- `internal sealed class QfcRemainingQueueAdmission` — no public surface; only reachable within `QuickFiler` (consumed by `QfcDatamodel`, which is F5's file, not F2's — F2 owns only the admission type itself).
- Constructor-injected exclusively: `IApplicationGlobals globals` (interface, unused inside the body beyond validating `scoreLoader` is non-null — actually `globals` itself has **no** null-guard, see gap below), `Func<MailItem, CancellationToken, Task<long>> scoreLoader` (delegate, validated non-null but never invoked inside this class — its only use is the null-check, confirming admission genuinely never scores), `Action<MailItem> addToQueue`, `Action<MailItem, Action<MailItem>> hookItem`, `Action<MailItem> removeFromQueue`. This is already a fully injectable-delegate design — no interface seam is needed because every collaborator is already a delegate.
- Single behavior method: `TryQueueAsync(MailItem mailItem, CancellationToken cancel)`.
- No direct construction of `Microsoft.Office.Interop.Outlook.Application/Store/MAPIFolder`. `MailItem` is a parameter/data type only, already mocked throughout the test suite (`new Mock<MailItem>().Object`).
- No concurrency primitives, no wall-clock or RNG usage. Fully synchronous logic wrapped in `Task.FromResult` (the method is `async`-shaped in signature only — it never actually awaits).

## Existing test coverage

No dedicated test file exists (`QfcRemainingQueueAdmissionTests.cs` does not exist under `QuickFiler.Test/Controllers/`). Coverage instead comes from a private helper (`CreateQueueAdmission`) inside `QfcDatamodelTests.cs` (lines 21-46), used by five tests in that file:

- `TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsAndHooksWithoutScoring`
- `TryQueueRemainingMailItemAsync_HighConfidenceEnabled_IgnoresThresholdAtAdmission`
- `TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsBelowThresholdCandidate`
- `TryQueueRemainingMailItemAsync_HighConfidenceDisabled_AddsAndHooksWithoutScoring`
- `TryQueueRemainingMailItemAsync_NullMailItem_DoesNotScoreAddOrHook`

Together these cover: the high-confidence-enabled success path (adds + hooks, never scores — three separate tests reinforcing the "admission never scores" issue #233/#424 contract from different angles, which is intentional pinning rather than duplication, per the spec's explicit callout that these tests must not change); the high-confidence-disabled success path; and the null-`mailItem` guard (`queued` is `false`, nothing added or hooked).

## Coverage gap

Not exercised by any existing test:

- Constructor guard clauses for `addToQueue`, `hookItem`, and `removeFromQueue` (`ArgumentNullException` for each). `scoreLoader`'s null-guard is exercised only implicitly (every test supplies a non-null delegate); no test asserts that a null `scoreLoader` throws.
- `TryQueueAsync`'s cancellation guard: `cancel.ThrowIfCancellationRequested()` at the top of the method, when called with an already-cancelled token, is never exercised.
- The constructor's `globals` parameter has **no** null-guard in the current source (`QfcRemainingQueueAdmission.cs:15-32` assigns nothing from `globals` and never checks it for null) — this is intentional dead-parameter behavior worth flagging to the atomic-planner as a documentation/behavior question, not a test gap per se (there is no branch to cover; `globals` is simply unused inside the constructor body once the delegates are captured).

## Seam requirements

None. Every collaborator is already an injected delegate (the top tier below "interface seam" in the hierarchy, and arguably preferable here since there is exactly one call site per collaborator with no need for a fuller interface). No further seam extraction is needed to close the gap; the remaining gap is pure test-writing, not seam-writing.

## Candidate test cases

| # | Case | Type | Notes |
|---|---|---|---|
| 1 | Constructor with `scoreLoader == null` throws `ArgumentNullException` naming `scoreLoader` | Negative | |
| 2 | Constructor with `addToQueue == null` throws `ArgumentNullException` naming `addToQueue` | Negative | |
| 3 | Constructor with `hookItem == null` throws `ArgumentNullException` naming `hookItem` | Negative | |
| 4 | Constructor with `removeFromQueue == null` throws `ArgumentNullException` naming `removeFromQueue` | Negative | |
| 5 | `TryQueueAsync` with an already-cancelled token throws `OperationCanceledException` before invoking `addToQueue`/`hookItem` | Negative/error-handling | Assert the delegates are never invoked (e.g. via a delegate that throws `AssertFailedException` if called, matching the existing style in `QfcDatamodelTests`) |
| 6 | `hookItem`'s second argument (`removeFromQueue`, passed through as the hook callback) is exactly the constructor's `removeFromQueue` delegate reference | Positive | Currently implied but not asserted directly; verifies the wiring rather than just the outcome |

## Determinism constraints

None required. The class has no clock, RNG, or real concurrency; `Task.FromResult` makes `TryQueueAsync` complete synchronously.
