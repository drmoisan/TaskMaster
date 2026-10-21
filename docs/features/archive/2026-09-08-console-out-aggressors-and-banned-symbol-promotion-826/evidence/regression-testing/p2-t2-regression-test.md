# Item-2 regression test file (issue #826, [P2-T2])

Timestamp: 2026-09-09T19-15

BRANCH: REACHABLE

The value above is identical to the one recorded in the [P1-T1] artifact
`<FEATURE>/evidence/other/item2-branch-reachability.2026-09-09T19-11.md`.

Command: the file was created with the Write tool, then the following ran as one
`pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard:

```
& $dotnet tool run csharpier format UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs
(Get-Content -LiteralPath $f).Count
@(Select-String -LiteralPath $f -CaseSensitive -SimpleMatch "[TestClass]").Count
@(Select-String -LiteralPath $f -CaseSensitive -SimpleMatch "[TestMethod]").Count
@(Select-String -LiteralPath $f -CaseSensitive -SimpleMatch "Thread.Sleep").Count
@(Select-String -LiteralPath $f -CaseSensitive -SimpleMatch "Task.Delay").Count
@(Select-String -LiteralPath $f -CaseSensitive -SimpleMatch "Console.").Count
@(Select-String -LiteralPath $f -CaseSensitive -SimpleMatch "new CancellationTokenSource(").Count
@(Select-String -LiteralPath $f -CaseSensitive -SimpleMatch "new CancellationTokenSource()").Count
```

EXIT_CODE: 0

## File

Path: `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs`

Line count: **235**, under the 500-line limit in `.claude/rules/general-code-change.md`.

## Fully qualified test method names

- `UtilitiesCS.Test.OutlookObjects.Table.OlTableExtensionsTimeoutDiagnosticsTests.GetTableInViewAsync_TimeoutSourceThrowsTimeout_EntersTimeoutCatchAndRetriesOnce`
- `UtilitiesCS.Test.OutlookObjects.Table.OlTableExtensionsTimeoutDiagnosticsTests.GetTableInViewAsync_TimeoutSourceThrowsTaskCanceled_EntersCancelCatchElseAndRetriesOnce`

Both are declared in the single `[TestClass]` type
`UtilitiesCS.Test.OutlookObjects.Table.OlTableExtensionsTimeoutDiagnosticsTests`.

## Gate figures

| Measure | Observed | Required |
|---|---|---|
| line count | 235 | under 500 |
| `[TestClass]` | 1 | exactly 1 |
| `[TestMethod]` | 2 | exactly 2 |
| `Thread.Sleep` | 0 | 0 |
| `Task.Delay` | 0 | 0 |
| `Console.` | 0 | 0 |
| `new CancellationTokenSource(` | 1 | equal to the next row, at least 1 |
| `new CancellationTokenSource()` | 1 | equal to the previous row, at least 1 |

The two `CancellationTokenSource` counts are equal at 1, which is the plan's mechanism for proving that
every construction in this file uses the **parameterless** constructor and that the file therefore adds
no new RS0030 call site. A parameterized construction would raise the first count above the second.

An earlier draft of this file failed the `Console.` gate at a count of 1. The single hit was in the class
doc comment, which read "asserts anything about Console.Out"; the prose was rewritten to "asserts
anything about the process-global console writer" and the count is now 0. Recorded because a
zero-occurrence gate that trips on a doc comment is easy to mis-read as a code defect.

## Design notes

- **Reflective invocation is mandatory, not stylistic.** `GetTableInViewAsync` returns
  `Task<Outlook.Table>` and Outlook types are embedded interop types, so a direct `await` of it from this
  assembly is rejected with CS1769. The private static helpers `SignatureTypes` and
  `InvokeGetTableInViewAsync` follow the reference implementation in
  `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs`. The parameter-Type array was
  re-derived from the current signature at
  `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` lines 36 to 43 rather than copied:
  `Outlook.Explorer`, `CancellationToken`, `int`, `int`, `Func<int, CancellationTokenSource>`,
  `TimeProvider`.
- **Only the two named helpers were reproduced.** The reference file also carries a third helper,
  `CreateArmingBarrier`, which this file does not need and does not contain.
- **Factory shape.** Per plan decision D4 the injected factory constructs a new
  `CancellationTokenSource` on every non-throwing invocation, using the parameterless constructor.
  `TimeOutTask.RunWithTimeout` holds the source in a `using var` and disposes it at the end of each
  attempt, so returning the same instance on the retry would make the recursion read `.Token` on a
  disposed source.
- **Determinism.** No `Thread.Sleep`, no `Task.Delay`, no wall-clock wait, no temporary file and no
  external service. Both tests are driven entirely by the injected factory throwing on its first
  invocation.
- **Why `GetTable` count is 1 and not 2.** The first attempt throws from the factory before `Task.Run`
  is reached, so the delegate never runs on that attempt. The factory invocation count of 2 is what
  proves the bounded retry occurred.

Output Summary: the file exists at the stated path, is 235 lines, contains exactly one `[TestClass]` and
exactly two `[TestMethod]` attributes with the two required method names, contains zero occurrences of
`Thread.Sleep`, `Task.Delay` and `Console.`, and its two `new CancellationTokenSource` counts are equal
at 1. All acceptance conditions for [P2-T2] hold.
