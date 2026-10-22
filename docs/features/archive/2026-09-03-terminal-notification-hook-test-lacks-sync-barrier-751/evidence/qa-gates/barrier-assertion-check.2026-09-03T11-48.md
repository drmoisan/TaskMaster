# Barrier Assertion Checks (Issue #751)

This artifact records the body-join acceptance checks for the two edits made to
`TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs`. Each task appends its own section.
The joined method body contains no host path, so no sanitization is required.

## P2-T1

Timestamp: 2026-09-03T14-38

Command:

```powershell
$p='TaskMaster.Test\AppGlobals\AppOlObjectsFolderTreeServiceTests.cs'; $l=Get-Content -LiteralPath $p; $s=($l|Select-String -SimpleMatch 'TerminalNotificationHookFailure_DoesNotReplaceDispatchFault'|Select-Object -First 1).LineNumber; $b=($l[($s-1)..($s+24)] -join ' '); $b -match 'GetExceptionAsync\(\s*await\s+run\.Terminal\s*\)' -and $b -notmatch 'sut\.NextTerminal'
```

EXIT_CODE: 0

Output Summary:

Result: **True**

The method declaration was found at line 102, so the 26-line join window spans lines 102 through 127. The
joined body the check evaluated:

```
        public async Task TerminalNotificationHookFailure_DoesNotReplaceDispatchFault()         {             var fault = new InvalidOperationException("hook containment fault");             var dispatcher = new ControlledUiDispatcher(DispatchMode.Pending, fault: fault);             var sut = CreateSut(dispatcher, throwFromTerminalHook: true);             var run = await StartWorkerAsync(sut, dispatcher);             try             {                 dispatcher.Complete(run.Operation, DispatchMode.Faulted);                 (await GetExceptionAsync(run.Worker)).Should().BeSameAs(fault);                 await run.Operation.ReleaseAsync();                 (await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);                 sut.LoadCount.Should().Be(0);                 sut.InvokedTerminalHookCount.Should().Be(1);             }             finally             {                 await CleanupAsync(sut, run.Operation, run.Worker);             }         }          private static async Task VerifyTerminalDispatchAsync(             DispatchMode terminalMode,             bool pending         )         {
```

Both conjuncts hold:

- `GetExceptionAsync\(\s*await\s+run\.Terminal\s*\)` matched, at character offset 668 of the joined body.
- `sut\.NextTerminal` did not match, confirming the inserted statement uses the captured tuple member
  `run.Terminal` and not a fresh read of `sut.NextTerminal`. A fresh read would bind to the never-completed
  signal that the fixture swaps in via `Interlocked.Exchange` at
  `AppOlObjectsFolderTreeServiceLifecycleTests.cs:201`, and the test would hang.

The statement inserted was exactly:

```csharp
(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);
```

placed immediately after the existing `await run.Operation.ReleaseAsync();` and immediately before the
existing `sut.LoadCount.Should().Be(0);`. Nothing else was inserted: no field, no local, no helper, no type,
and no `using` directive. `using System.Threading;` was already present at line 2 of this file, and
`GetExceptionAsync` already existed as an internal static helper in the same class.

Window sufficiency, as recorded by the plan and confirmed here: the declaration is at line 102 and the
method's closing brace is at line 120 after the edit, so the 26-line window reaching line 127 retains
margin beyond the method's extent. The window is a fixed span rather than a parse of the method's actual
extent.

## P2-T2

Timestamp: 2026-09-03T14-39

Command:

```powershell
$p='TaskMaster.Test\AppGlobals\AppOlObjectsFolderTreeServiceTests.cs'; $l=Get-Content -LiteralPath $p; $s=($l|Select-String -SimpleMatch 'TerminalNotificationHookFailure_DoesNotReplaceDispatchFault'|Select-Object -First 1).LineNumber; $b=($l[($s-1)..($s+24)] -join ' '); $b -match 'Volatile\.Read\(\s*ref\s+sut\.InvokedTerminalHookCount\s*\)\s*\.Should\(\)\s*\.Be\(1\)'
```

EXIT_CODE: 0

Output Summary:

Result: **True**

The joined body the check evaluated, after this edit:

```
        public async Task TerminalNotificationHookFailure_DoesNotReplaceDispatchFault()         {             var fault = new InvalidOperationException("hook containment fault");             var dispatcher = new ControlledUiDispatcher(DispatchMode.Pending, fault: fault);             var sut = CreateSut(dispatcher, throwFromTerminalHook: true);             var run = await StartWorkerAsync(sut, dispatcher);             try             {                 dispatcher.Complete(run.Operation, DispatchMode.Faulted);                 (await GetExceptionAsync(run.Worker)).Should().BeSameAs(fault);                 await run.Operation.ReleaseAsync();                 (await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);                 sut.LoadCount.Should().Be(0);                 Volatile.Read(ref sut.InvokedTerminalHookCount).Should().Be(1);             }             finally             {                 await CleanupAsync(sut, run.Operation, run.Worker);             }         }          private static async Task VerifyTerminalDispatchAsync(             DispatchMode terminalMode,             bool pending         )         {
```

The statement now reads exactly:

```csharp
Volatile.Read(ref sut.InvokedTerminalHookCount).Should().Be(1);
```

The expected value remains **1**. It was not relaxed, not widened to a range, and not deleted. This is the
same `Volatile.Read(ref ...)` assertion shape already used in the sibling fixture file, for example at
`AppOlObjectsFolderTreeServiceLifecycleTests.cs:312`.

Ordering evidence: within the joined body, the barrier assertion matches at character offset **668** and the
`Volatile.Read` assertion matches at character offset **795**, so the barrier assertion precedes the counter
assertion. The counter assertion is therefore reached only after the barrier assertion.

Window note: `InvokedTerminalHookCount` occurs exactly once in this file, so the window's overrun past the
method's closing brace cannot produce a match drawn from a neighbouring method. That single-occurrence
property is re-derived mechanically by P2-T4.

## P5-T1

Timestamp: 2026-09-03T14-49

Command:

```powershell
$p='TaskMaster.Test\AppGlobals\AppOlObjectsFolderTreeServiceTests.cs'; $l=Get-Content -LiteralPath $p; $s=($l|Select-String -SimpleMatch 'TerminalNotificationHookFailure_DoesNotReplaceDispatchFault'|Select-Object -First 1).LineNumber; $b=($l[($s-1)..($s+24)] -join ' '); $b -match 'GetExceptionAsync\(\s*await\s+run\.Terminal\s*\)' -and $b -notmatch 'sut\.NextTerminal'
```

EXIT_CODE: 0

Output Summary:

Result: **True** — reproduced after the final clean Phase 4 toolchain pass.

This is the AC1 check-off re-run. It was executed against the tree as it stands after P4-T1's formatter run
and after the P4-T3 through P4-T5 gates, so the result is recorded against the final tree rather than
against the mid-plan state P2-T1 observed. The declaration remains at line 102, the barrier assertion is
present at character offset 668 of the joined body, and `sut.NextTerminal` still does not appear.

## P5-T2

Timestamp: 2026-09-03T14-49

Command:

```powershell
$p='TaskMaster.Test\AppGlobals\AppOlObjectsFolderTreeServiceTests.cs'; $l=Get-Content -LiteralPath $p; $s=($l|Select-String -SimpleMatch 'TerminalNotificationHookFailure_DoesNotReplaceDispatchFault'|Select-Object -First 1).LineNumber; $b=($l[($s-1)..($s+24)] -join ' '); $b -match 'Volatile\.Read\(\s*ref\s+sut\.InvokedTerminalHookCount\s*\)\s*\.Should\(\)\s*\.Be\(1\)'
```

EXIT_CODE: 0

Output Summary:

Result: **True** — reproduced after the final clean Phase 4 toolchain pass.

Ordering re-derived at the same time: the barrier assertion matches at character offset **668** and the
`Volatile.Read` assertion matches at character offset **795**, so the barrier match sits at a lower
character offset in the joined body than the `Volatile.Read` match. The counter assertion is therefore
reached only after the barrier assertion. The expected value remains `1`.

The joined body evaluated by both re-runs above:

```
        public async Task TerminalNotificationHookFailure_DoesNotReplaceDispatchFault()         {             var fault = new InvalidOperationException("hook containment fault");             var dispatcher = new ControlledUiDispatcher(DispatchMode.Pending, fault: fault);             var sut = CreateSut(dispatcher, throwFromTerminalHook: true);             var run = await StartWorkerAsync(sut, dispatcher);             try             {                 dispatcher.Complete(run.Operation, DispatchMode.Faulted);                 (await GetExceptionAsync(run.Worker)).Should().BeSameAs(fault);                 await run.Operation.ReleaseAsync();                 (await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);                 sut.LoadCount.Should().Be(0);                 Volatile.Read(ref sut.InvokedTerminalHookCount).Should().Be(1);             }             finally             {                 await CleanupAsync(sut, run.Operation, run.Worker);             }         }          private static async Task VerifyTerminalDispatchAsync(             DispatchMode terminalMode,             bool pending         )         {
```
