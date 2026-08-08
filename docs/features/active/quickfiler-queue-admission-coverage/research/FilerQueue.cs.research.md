# Research: `QuickFiler/Controllers/FilerQueue.cs`

- Parent epic: #136 (`quickfiler-per-file-coverage`)
- Child feature: #431 F2 (`quickfiler-queue-admission-coverage`)
- File under research: `QuickFiler/Controllers/FilerQueue.cs` (83 lines, verified by direct read)
- Evidence basis: direct read of the file; direct read of `QuickFiler.Test/Controllers/FilerQueueTests.cs`;
  grep/read of `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` for the `SortAsync` seam.

## Current structure

- Two types in one file: `public class FilerQueue` and `public class FilerQueueItem`.
- `FilerQueue` public surface: `Enqueue(FilerQueueItem)`, `Enqueue(EmailFiler, IList<MailItemHelper>)`, `Consumer` (public get-only `Task` property), `ConsumeAsync()`.
- Dependencies: `BlockingCollection<FilerQueueItem> Queue` (newed up as a field initializer, `internal` get, not injected); `ThreadSafeSingleShotGuard guard` (newed up field, from `UtilitiesCS.Threading`, not injected); `log4net.ILog logger` (static, standard repo logging pattern).
- No direct dependency on `Microsoft.Office.Interop.Outlook.Application/MailItem/Store/MAPIFolder`. `MailItemHelper` (a `UtilitiesCS` wrapper type, not the raw COM interface) is only a data-carrier parameter.
- Concurrency: `BlockingCollection<FilerQueueItem>` (thread-safe producer/consumer collection); `Task.Run(async () => ...)` inside `ConsumeAsync`; `ThreadSafeSingleShotGuard.CheckAndSetFirstCall` gates whether `Enqueue` starts a new `ConsumeAsync()` task, and the guard is replaced (`guard = new ThreadSafeSingleShotGuard();`) once the consumer loop drains — a single-shot restart pattern with no explicit lock around the field reassignment (relies on the guard's own internal thread-safety).
- No wall-clock or RNG usage.
- `FilerQueueItem`'s constructor validates `filer`/`helpers` via `.ThrowIfNull()` and an explicit `Any(h => h is null)` guard, throwing `ArgumentNullException` for both cases.

## Existing test coverage

`FilerQueueTests.cs` (89 lines, 5 tests), explicitly scoped by its own doc comment to "the pure, Outlook-free surface": `FilerQueueItem_Constructor_StoresFilerAndHelpers`, `FilerQueueItem_Constructor_NullFiler_ThrowsArgumentNullException`, `FilerQueueItem_Constructor_NullHelpers_ThrowsArgumentNullException`, `FilerQueueItem_Constructor_HelpersContainingNull_ThrowsArgumentNullException`, `FilerQueue_NewInstance_HasCompletedConsumerByDefault`.

Covered: the entire `FilerQueueItem` constructor contract (three guard clauses and the success path); `FilerQueue`'s default `Consumer` state (`Task.CompletedTask`).

The test file's own doc comment records a deliberate, reasoned exclusion: "The FilerQueue.Enqueue/ConsumeAsync path is intentionally NOT exercised because it dispatches to `EmailFiler.SortAsync` on a background task (Outlook-bound and non-deterministic)."

## Coverage gap

Not exercised by any existing test:

- `Enqueue(FilerQueueItem)` and `Enqueue(EmailFiler, IList<MailItemHelper>)` — neither overload's queueing behavior nor the `guard.CheckAndSetFirstCall` first-call gating is exercised.
- `ConsumeAsync()` — the entire drain loop is uncovered: the `while (Queue.TryTake(out var item))` loop, the success call to `item.Filer.SortAsync(item.Helpers)`, the `catch (Exception e)` branch that logs `item.Helpers.First()` details, and the terminal `guard = new ThreadSafeSingleShotGuard();` reset.
- `Queue` property getter (trivially covered as a side effect of any `Enqueue` test, but not independently asserted today).

## `[ExcludeFromCodeCoverage]` disposition

Not applicable — this file carries no such attribute.

## Seam requirements

`EmailFiler.SortAsync()` (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:137`) is declared `public virtual async Task<bool> SortAsync()`, and the two-argument overload (`SortAsync(IList<MailItemHelper> mailHelpers)`, line 128) delegates to it. Because it is `virtual`, a minimal, in-test subclass of `EmailFiler` can override `SortAsync()` to return a deterministic result or throw, without touching live Outlook COM and without any production change to `FilerQueue.cs` or `EmailFiler.cs`. This is the seam to use for `ConsumeAsync` coverage:

- No interface seam exists for `EmailFiler` (it is a concrete class, not behind an `IEmailFiler` abstraction), so the ideal "interface seam" tier is not available without a change to `UtilitiesCS` — out of scope for this child (`EmailFiler.cs` is not in F2's file list and is owned elsewhere).
- The virtual-method override is the next-best "injectable delegate"-equivalent seam already available in the codebase and requires zero production changes in `FilerQueue.cs`.
- `Queue` and `guard` remain field-initialized (not constructor-injected); no test today needs to substitute either, and introducing constructor injection for them would be a larger, unnecessary change given `BlockingCollection<T>` and `ThreadSafeSingleShotGuard` are already deterministic, in-memory, side-effect-free collaborators.

## Candidate test cases

| # | Case | Type | Notes |
|---|---|---|---|
| 1 | `Enqueue(FilerQueueItem)` adds the item to `Queue` and starts `Consumer` on the first call | Positive | Assert `Queue.Count` transiently or drain via a controllable `EmailFiler` subclass with a gated `TaskCompletionSource` |
| 2 | `Enqueue(FilerQueueItem)` on a second call while the consumer is still running does not start a second consumer task (guard already tripped) | Positive/concurrency | Assert `guard.CheckAndSetFirstCall` is false on the second call via the existing `Consumer` reference identity |
| 3 | `Enqueue(EmailFiler, IList<MailItemHelper>)` overload constructs a `FilerQueueItem` internally and enqueues it | Positive | |
| 4 | `ConsumeAsync` drains all queued items by invoking `SortAsync` on each, in enqueue order | Positive/state-transition | Test subclass of `EmailFiler` records the order it was invoked |
| 5 | `ConsumeAsync` continues draining subsequent items after one item's `SortAsync` throws | Error-handling | Test subclass throws for one item only; assert the log call is reached (inject/verify via the existing `log4net` pattern or assert no unhandled exception escapes `ConsumeAsync`) and that later items are still processed |
| 6 | `ConsumeAsync` resets `guard` to a fresh `ThreadSafeSingleShotGuard` once the queue is drained, allowing a subsequent `Enqueue` to start a new consumer task | State-transition | Enqueue → drain → enqueue again → assert a new `Consumer` task starts |
| 7 | `FilerQueue` with an empty queue completes `ConsumeAsync` immediately without invoking any `SortAsync` | Boundary | |

## Determinism constraints

- `ConsumeAsync` wraps its loop in `Task.Run(...)`; tests must await the returned `Consumer`/`ConsumeAsync()` task rather than sleeping, and should use a `TaskCompletionSource`-backed `EmailFiler` subclass to control completion ordering deterministically instead of any real delay.
- No RNG or wall-clock reads exist in this file; no seeded-RNG or injected-clock requirement.
