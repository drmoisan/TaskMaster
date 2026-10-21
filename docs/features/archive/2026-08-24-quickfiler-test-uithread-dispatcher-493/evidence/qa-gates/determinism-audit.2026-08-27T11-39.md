# Determinism Audit (P4-T5)

Timestamp: 2026-08-27T11-39
Task: [P4-T5]
Command: `Select-String -SimpleMatch -Pattern <token> -Path <path>` for each of the five tokens against each of the four in-scope C# paths — twenty combinations
EXIT_CODE: 0
Output Summary: All twenty token-and-path combinations return **0** matches. Combination count 20,
non-zero results 0.

## Full twenty-combination matrix

| Token | `…UiThreadDispatcherFixture.cs` | `…UiThreadDispatcherFixtureTests.cs` | `…TestSupport.cs` | `…InitializationTests.Part2.cs` |
| --- | --- | --- | --- | --- |
| `Thread.Sleep` | 0 | 0 | 0 | 0 |
| `Task.Delay` | 0 | 0 | 0 | 0 |
| `Path.GetTempFileName` | 0 | 0 | 0 | 0 |
| `Path.GetTempPath` | 0 | 0 | 0 | 0 |
| `Path.GetRandomFileName` | 0 | 0 | 0 | 0 |

Full paths, as listed in `P4-T3`:

- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`
- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`

## Token-set note

`Path.GetRandomFileName` is audited in place of `Directory.CreateTempSubdirectory`, which is a
.NET 7+ API that cannot exist in a `v4.8.1` assembly and would therefore name a search no executor
behaviour could ever make match — an unfalsifiable condition. `Path.GetTempFileName` and
`Path.GetTempPath` are the two temporary-file entry points that are reachable on net481.

## What the audited files use instead

All cross-thread coordination in the new and modified files uses:

- `ManualResetEventSlim` — in `GetParkedDispatcher` (to observe the parked STA thread's dispatcher
  becoming available) and in R4 (to signal that the second caller has started).
- Awaited `Task` completion — `await UiThreadDispatcherFixture.BeginTransactionAsync()`,
  `await waiter`, and the existing `await host.InvokeAsync(...)` in the pump fixture.
- `SemaphoreSlim.WaitAsync()` — the transaction gate, released by the preceding holder's `Dispose`
  and never by elapsed time.

No temporary file is created anywhere in the four files. The `[Timeout(GateTimeoutMs)]` attribute on
each of the six regression tests is not a wall-clock wait in the audited sense: it converts a genuine
deadlock into a test failure rather than a hung run, matching the precedent and stated rationale at
`QfcItemController.SeamFactoryTests.cs:288-293`.
