# P7-T2 — Determinism and Temp-File Compliance Audit

Issue: #230
Task: [P7-T2]

## Scope — files added or modified by this feature

Enumerated from `git status --porcelain -uall`:

| File | State |
|---|---|
| `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` | added |
| `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` | added |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | added |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | added |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` | modified |
| `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` | modified |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | modified |
| `QuickFiler.Test/QuickFiler.Test.csproj` | modified |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` | modified |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | modified |

## Step 1 — Banned-API scan

- Timestamp: 2026-08-07T23-35
- Command:
  `Select-String -Pattern 'Thread\.Sleep|Task\.Delay|Date\.now|DateTime\.Now|GetTempFileName|GetTempPath|Path\.GetRandomFileName' -Path <the nine .cs files above>`
- EXIT_CODE: 0 (derived from `$?` = `True` per D14)
- Output Summary: **`BANNED_HITS=0`.** Zero occurrences of `Thread.Sleep`,
  `Task.Delay`, `Date.now`, `DateTime.Now`, `GetTempFileName`, `GetTempPath`, or
  `Path.GetRandomFileName` anywhere in feature-added or feature-modified code.

## Step 2 — Supplementary filesystem and polling scan

- Timestamp: 2026-08-07T23-35
- Command:
  `Select-String -Pattern 'File\.|Directory\.|StreamWriter|StreamReader|FileStream|Path\.Combine' -Path <the seven test .cs files>`
  and
  `Select-String -Pattern 'SpinWait|while \(true\)|Stopwatch|Environment\.TickCount|DateTime\.UtcNow' -Path <same>`
- EXIT_CODE: 0 (derived from `$?`)
- Output Summary: `IO_HITS=7`, **all seven are substring false positives** — six
  matches of `autoFile.` and one of `capturedBlFile.` against the `File\.` pattern.
  No `File`, `Directory`, `StreamWriter`, `StreamReader`, `FileStream`, or
  `Path.Combine` call exists in any feature-added test file. `SPIN_HITS=0`: no
  `SpinWait`, no `while (true)` loop, no `Stopwatch`, no `Environment.TickCount`,
  and no `DateTime.UtcNow`.

**No test added or modified by this feature creates or uses a temporary file.**

## Step 3 — MSTest `[Timeout]` coverage on the new tests

- Timestamp: 2026-08-07T23-35
- Command: per-file counts of `\[TestMethod\]` versus `\[Timeout\(`
- EXIT_CODE: 0 (derived from `$?`)
- Output Summary:

| File | `[TestMethod]` | `[Timeout(...)]` | Feature-added tests |
|---|---|---|---|
| `TestSupport/WinFormsPumpHostTests.cs` | 13 | 13 | 13 of 13 — all carry `[Timeout(30000)]` |
| `Controllers/QfcItemController.InitializationTests.Part3.cs` | 5 | 5 | 5 of 5 — all carry `[Timeout(60000)]` |
| `Controllers/QfcItemController.SeamFactoryTests.cs` | 9 | 2 | the 2 feature-added tests carry `[Timeout(60000)]`; the 7 pre-existing tests are unchanged |
| `Controllers/QfcItemController.ViewerSetupTests.cs` | 10 | 1 | the 1 feature-added test carries `[Timeout(60000)]`; the 9 pre-existing tests are unchanged |
| `Controllers/QfcItemController.InitializationTests.cs` | 4 | 0 | no feature-added tests (only the shared `PumpTimeoutMs` constant); the 4 pre-existing tests are unchanged |
| `Controllers/QfcItemController.InitializationTests.Part2.cs` | 0 | 0 | fixture only, no tests |

**Every one of the 21 tests added by this feature carries an MSTest `[Timeout]`
attribute.** Pre-existing tests were deliberately not modified.

## Coordination primitives used (all deterministic)

- `ManualResetEventSlim` — pump readiness handshake (`WinFormsPumpHost` constructor).
- `TaskCompletionSource<T>` with `TaskCreationOptions.RunContinuationsAsynchronously`
  — per-call completion and the loop-stopped signal.
- Awaiting the member's own returned `Task`.
- `Task.Yield()` in two self-tests — a scheduling primitive that yields to the
  captured `SynchronizationContext`, not a wall-clock wait.
- `Thread.Join()` after the deterministic `_stopped` signal — the established
  in-repo pattern (`WpfUiDispatcherTests.cs`, `QfcItemController.TestSupport.cs`).

The `[Timeout]` attributes are a harness bound (in-repo precedent
`TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs`), not a wall-clock wait in
test logic: they exist only to convert a genuine deadlock in production code into a
test failure instead of a CI hang.

## Result

- Zero banned-API hits in feature-added code.
- Zero temporary files created or used.
- Zero polling or wall-clock-dependent waits.
- Every feature-added test carries an MSTest `[Timeout]`.
