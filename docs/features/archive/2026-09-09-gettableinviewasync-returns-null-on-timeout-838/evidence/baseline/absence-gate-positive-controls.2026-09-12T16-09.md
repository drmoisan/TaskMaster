# P0-T21 — Baseline positive controls for every later absence gate

Timestamp: 2026-09-13T02-46

Command: a single pwsh payload using the plan's fixed search-gate form, `@(Select-String -LiteralPath <file> -SimpleMatch -CaseSensitive -Pattern "<literal>").Count`, against eight named file-and-literal pairs. The one pattern gate uses `-CaseSensitive` without `-SimpleMatch` and its regular expression was echoed before use as `new CancellationTokenSource\([^)]`, matching the plan's stated pattern exactly.

EXIT_CODE: 0

| Token | File | Literal or pattern | Count | Planner's measurement |
|---|---|---|---|---|
| `CTRL_LATENT` | `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` | `pre-existing latent` | 1 | 1 |
| `CTRL_RETURN_BANG` | same file | `return table!;` | 1 | 1 |
| `CTRL_CATCH_TCE` | same file | `catch (TaskCanceledException` | 3 | 3 |
| `CTRL_MAKING_NULL` | `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs` | `making the returned table null` | 1 | 1 |
| `CTRL_CANCELAFTER` | `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs` | `.CancelAfter(` | 1 | 1 |
| `CTRL_SLEEP` | `UtilitiesCS/Threading/ThreadMonitor.cs` | `Thread.Sleep(` | 2 | 2 |
| `CTRL_DELAY` | `UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs` | `Task.Delay(` | 3 | 3 |
| `CTRL_TIMED_CTOR` | `UtilitiesCS/Threading/TimeOutTask.cs` | `new CancellationTokenSource\([^)]` | 10 | 10 |

Output Summary: every one of the eight counts is at least 1 and every one reproduces the planner's measured value exactly, so no later zero-hit gate is incapable of failing. Each control is read through the same file-reading search form as the gate it controls; no control is an in-payload string, so the file-reading half of the mechanism is proven as well as the matching half. The four control files named for the timing and construction gates — the QuickFiler queue coverage test, the thread monitor, the additional time-out task tests and the time-out task itself — are read only and are edited by no task in this plan. The exit code is 0 and no `ExpectedExitCode:` field applies, which is the plan's convention for every search gate.
