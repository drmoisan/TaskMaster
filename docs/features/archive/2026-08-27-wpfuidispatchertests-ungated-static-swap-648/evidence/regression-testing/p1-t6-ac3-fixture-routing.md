# P1-T6 — AC-3 Verified by Measurement (Swap Routed Through the Shared Fixture)

Timestamp: 2026-09-01T14-10

Command:
```
grep -n -F -e 'UiThreadDispatcherFixture.BeginTransactionAsync' -e 'UiThreadDispatcherTransaction' -e 'transaction.Install' -e 'transaction.Dispose();' -e 'async Task Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread' -e 'private const int GateTimeoutMs = 60000;' -e '[Timeout(GateTimeoutMs)]' -e 'ShutdownDispatcher' QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs
```

EXIT_CODE: 0

Output Summary:

All seven tokens match at least one line of
`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`. Matching line numbers:

| Token | Matching line(s) |
|---|---|
| `UiThreadDispatcherFixture.BeginTransactionAsync` | `:59` |
| `UiThreadDispatcherTransaction` | `:44`, `:58`, `:60` |
| `transaction.Install` | `:63` |
| `transaction.Dispose();` | `:95` |
| `async Task Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread` | `:50` |
| `private const int GateTimeoutMs = 60000;` | `:21` |
| `[Timeout(GateTimeoutMs)]` | `:49` |

## Ordering condition

`transaction.Dispose();` at `:95` lies inside the inner `finally` block and is textually above the
line matching `ShutdownDispatcher`, which is `:100`. The nesting, read from the file:

```
 92                }
 93                finally
 94                {
 95                    transaction.Dispose();
 96                }
 97            }
 98            finally
 99            {
100                QfcItemControllerTestSupport.ShutdownDispatcher(dispatcher);
101            }
```

The inner `finally` at `:93-96` disposes the transaction; the outer `finally` at `:98-101` shuts the
dispatcher down. That is the order P1-T2 directs, and it means the gate is released before the
dispatcher the transaction installed is torn down.

## Formatting-driven shaping of the acquisition statement

The gate acquisition is written as two statements rather than one:

```
 58                Task<UiThreadDispatcherTransaction> gate =
 59                    UiThreadDispatcherFixture.BeginTransactionAsync();
 60                UiThreadDispatcherTransaction transaction = await gate.ConfigureAwait(false);
```

The single-expression form P1-T2 states,
`await UiThreadDispatcherFixture.BeginTransactionAsync().ConfigureAwait(false)`, is 121 characters
before indentation, so at this statement's 16-column indent it exceeds CSharpier's 100-column print
width. CSharpier then breaks it as a three-line member chain, exactly as it has already done at the
sibling call site `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:47-49`:

```
                UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
```

In that form the qualified call `UiThreadDispatcherFixture.BeginTransactionAsync` is split across two
lines and matches no single line, so this task's first token would return zero matches whatever the
executor did. That was confirmed empirically rather than inferred: the single-expression form was
written first and `dotnet tool run csharpier check` on the file reported the only difference as line
endings, which establishes that the three-line chain break is CSharpier's own output for that shape
and cannot be avoided by hand-formatting.

The two-statement form is semantically identical — the same method is called, the same task is
awaited, and `ConfigureAwait(false)` is still applied to it — and it keeps the qualified call
contiguous on `:59`. A confirming `csharpier check` on the reshaped file again reported only a
line-ending difference, so this shape is CSharpier-stable. The choice is recorded here because it is
a deviation from the literal expression text P1-T2 quotes, made so that P1-T2's own stated acceptance
(that P1-T4 through P1-T7 pass) can be met.

AC-3 holds: the test obtains its gate from `UiThreadDispatcherFixture.BeginTransactionAsync()`,
installs the running dispatcher through the returned `UiThreadDispatcherTransaction`, restores by
disposing that transaction rather than by writing the field, and is declared `async Task` because the
gate is awaited.
