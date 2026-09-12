# Research: `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`

- Parent epic: #136 (`quickfiler-per-file-coverage`)
- Child feature: #431 F2 (`quickfiler-queue-admission-coverage`)
- File under research: `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` (191 lines, verified by direct read)
- Evidence basis: direct read of the file; direct read of
  `QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs`; grep for
  `ExcludeFromCodeCoverage` across `QuickFiler/Controllers` to locate the exact attribute site.

## Correction to the epic's file-level `[X]` marker

The epic (`docs/features/epics/quickfiler-per-file-coverage/epic.md`, F2 section) marks this file
`[X]` at the file level, implying the whole file is exempted. **That is imprecise.** A direct read
shows the file defines four types, and `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]`
is present on exactly **one** of them — the internal `FolderScoringService` adapter class (line 166) —
and nowhere else. The other three types (`QfcHighConfidencePreFilter` itself, the `QfcPreScoredItem`
struct, and the `IFolderScoringService` interface) carry no exemption attribute and are the file's
actual testable surface.

## Current structure

- `internal static class QfcHighConfidencePreFilter` — one public static method, `FilterAsync(IList<MailItem> items, IApplicationGlobals globals, double threshold, CancellationToken token, IFolderScoringService scoringService = null)`. Pure filtering/scoring-orchestration logic; the `scoringService` parameter is an **already-present interface seam** with a production default (`new FolderScoringService()`), injectable by tests.
- `public readonly struct QfcPreScoredItem` — a simple immutable data carrier (`MailItem`, `PredeterminedFolder`), no logic beyond the constructor's null-coalescing of `predeterminedFolder`.
- `internal interface IFolderScoringService` — the scoring seam contract: `Task<(long Score, string TopFolder)> ScoreAsync(MailItem, IApplicationGlobals, CancellationToken)`.
- `[ExcludeFromCodeCoverage] internal sealed class FolderScoringService : IFolderScoringService` — the **default, COM-bound adapter**. Its `ScoreAsync` body calls `MailItemHelper.FromMailItemAsync(mailItem, globals, token, false)` (COM-bound Outlook item materialization) followed by `new FolderPredictor(...)` + `InitAsync(...)` (live Bayesian classifier training/scoring against the configured account). This is a genuine "thinnest possible wiring" adapter over live Outlook COM and the classifier, reached only when a caller does not supply a `scoringService` override.
- `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` at file scope (line 11) — an existing convention in this codebase that lets Moq's dynamic-proxy generator mock the `internal IFolderScoringService` interface directly, without needing to make it `public`.
- Constructor-injected dependencies: none (static class); the seam is a method parameter, not a constructor.
- No direct dependency on `Microsoft.Office.Interop.Outlook.Application/Store/MAPIFolder` inside the testable surface (`FilterAsync`); the COM-bound calls live entirely inside the excluded `FolderScoringService.ScoreAsync`.
- Concurrency: `FilterAsync` scores every item in parallel via `items.Select(async (item, index) => ...)` + `Task.WhenAll`, then reorders survivors by original `index` to restore input order. No locks; no shared mutable state beyond the local `scoringTasks`/`scored` arrays.
- No RNG. No wall-clock usage.

## Existing test coverage

`QfcHighConfidencePreFilterTests.cs` (9 tests), all targeting `FilterAsync` through the injected
`IFolderScoringService` mock: `FilterAsync_ProbabilityDebugLog_IncludesCallerSubjectEntryIdScoreAndTopFolder`,
`FilterAsync_WithSingleAboveThresholdItem_ReturnsThatItem`, `FilterAsync_ExcludesItemsBelowCutoff`,
`FilterAsync_ExcludesZeroScoreNoSuggestion`, `FilterAsync_RetainsItemExactlyAtCutoff`,
`FilterAsync_SurvivorsCarryPredeterminedTopFolder`, `FilterAsync_NullItems_ReturnsEmpty`,
`FilterAsync_EmptyItems_ReturnsEmpty`, `FilterAsync_AllBelowThreshold_ReturnsEmpty`,
`FilterAsync_HonorsCancellation`.

This is already a comprehensive suite for the testable surface: null/empty short-circuits, the
inclusive-cutoff boundary (`score >= cutoff`), the `score > 0` zero-score exclusion, below-cutoff
exclusion, all-below-threshold exclusion, cancellation honored via `token.ThrowIfCancellationRequested()`
at entry, survivor order/`PredeterminedFolder` carry-through, and the debug log line's content.
`FolderScoringService` (the excluded adapter) is, correctly, not exercised by any of these tests.

## Coverage gap

Minimal, on the testable (non-excluded) surface only:

- `QfcPreScoredItem`'s constructor with a `null` `predeterminedFolder` (coercion to `string.Empty`) is
  exercised only indirectly through `FilterAsync`'s survivor construction; no test directly asserts the
  struct's own null-coalescing contract in isolation.
- The multi-item **parallel ordering** guarantee — that survivors preserve original input order even
  when scoring completions arrive out of order — is asserted only via `FilterAsync_SurvivorsCarryPredeterminedTopFolder`
  with a small fixed set; no test explicitly forces out-of-order task completion (e.g., via a scoring
  mock whose delay is inverse to index) to prove the `OrderBy(result => result.index)` line is actually
  load-bearing rather than incidentally correct because `Task.WhenAll` happened to preserve order in the
  existing tests' synchronous-mock setups.

On the excluded `FolderScoringService` adapter: no gap to close — see disposition below.

## `[ExcludeFromCodeCoverage]` disposition

**Two separate dispositions for the two parts of the file, not one:**

- `QfcHighConfidencePreFilter`, `QfcPreScoredItem`, `IFolderScoringService` — **carry no exemption
  today** and are already close to fully covered by the existing 9-test suite plus the two gap cases
  above. No action needed beyond closing the small gap; there is nothing to "remove" here because
  nothing is excluded.
- `FolderScoringService` (the adapter) — **carries the exemption and should very likely be ratified as
  irreducible**, not removed. Rationale, evaluated against the epic's irreducible-remainder test: (a)
  a seam already exists and is already used to isolate this exact dependency — `IFolderScoringService`
  — so this is not a case of "a seam can be introduced but wasn't"; the seam already exists at the
  precise boundary; (b) the adapter's body is COM-bound (`MailItemHelper.FromMailItemAsync`) plus a live
  Bayesian classifier scoring call (`FolderPredictor.InitAsync`/`RefreshSuggestions`), which cannot be
  exercised deterministically without a live Outlook mailbox and trained classifier state — exactly the
  CLAUDE.md § UT2 exemption class ("Outlook Interop event handler classes ... that directly depend on
  ... `MailItem` ... without an injectable seam" — here the seam exists one level up, and this class *is*
  the thin, irreducible remainder the seam was built to isolate); (c) the class is already documented
  in-code with exactly this reasoning (`FolderScoringService`'s XML `<remarks>` block explicitly states
  the rationale). F2's plan should record this file's disposition for F1's ledger as: **ratify
  `FolderScoringService`'s exemption as irreducible; no further seam extraction is warranted or
  recommended.** This should be flagged explicitly at execution time against F1's actual ledger entry
  (once it exists) rather than assumed — this research states F2's best-effort classification for
  planning purposes only, per the cross-cutting summary's guidance on ledger authority.

## Seam requirements

None beyond what already exists. The `IFolderScoringService` interface seam is already in place and
already used by the existing test suite; no further seam work is needed to close the small gap
identified above.

## Candidate test cases

| # | Case | Type | Notes |
|---|---|---|---|
| 1 | `QfcPreScoredItem` constructed with `predeterminedFolder = null` exposes `PredeterminedFolder == string.Empty` | Boundary | Direct struct-level test, independent of `FilterAsync` |
| 2 | `QfcPreScoredItem` constructed with a non-null `predeterminedFolder` exposes it unchanged, and `MailItem` is the same reference passed in | Positive | Direct struct-level test |
| 3 | `FilterAsync` preserves original input order among survivors even when the injected scoring mock completes out of index order (e.g., later-indexed items resolve first via reversed `TaskCompletionSource` releases) | Positive/concurrency | Proves the `OrderBy(result => result.index)` line is load-bearing, not incidental |

## Determinism constraints

`FilterAsync`'s existing tests already use synchronous or `Task.FromResult`-backed mocks for
`IFolderScoringService`, which is deterministic. The new out-of-order-completion test (case 3) should
use `TaskCompletionSource`-gated mock responses released in a controlled, deterministic sequence — never
`Task.Delay`/`Thread.Sleep` — to force genuine out-of-order completion without introducing flakiness. No
clock or RNG seam is needed anywhere in this file.
