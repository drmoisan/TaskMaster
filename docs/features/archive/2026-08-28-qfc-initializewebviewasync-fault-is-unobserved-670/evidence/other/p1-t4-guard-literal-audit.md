# P1-T4 — Guard literal audit

Timestamp: 2026-09-01T19-54
Command: `Select-String -Path 'QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs' -SimpleMatch '<literal>'` for each of the four literals below, plus `Select-String -Path 'QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs' -SimpleMatch -CaseSensitive 'throw'`
EXIT_CODE: 0

## The four required literals, with observed match counts

| Literal | Required | Observed |
| --- | --- | --- |
| `internal async Task InitializeWebViewGuardedAsync()` | exactly 1 | 1 |
| `await InitializeWebViewAsync();` | exactly 1 | 1 |
| `catch (OperationCanceledException)` | exactly 1 | 1 |
| `WebViewInitializationErrorSink("WebView2 initialization failed.", ex);` | exactly 1 | 1 |

All four conditions hold.

## The rethrow exclusion, and why the case-sensitive form is required

    Select-String -SimpleMatch -CaseSensitive 'throw'   →  0 matches
    Select-String -SimpleMatch 'throw'                  →  1 match

The case-sensitive search returns **zero**, which is the acceptance condition: the guard contains no rethrow, so the task it returns cannot transition to `Faulted` and the fault is genuinely contained rather than deferred.

The case-insensitive search returns **one**. That match is not a rethrow. It is the word `Throw` inside `Token.ThrowIfCancellationRequested()`, which appears in the explanatory comment on the `catch (OperationCanceledException)` arm that this same task was required to write. The contrast between the two counts is recorded here because it demonstrates the condition is discriminating in the intended direction rather than passing by accident: had the plan specified the default case-insensitive form, the acceptance condition would have been unsatisfiable against the exact body the plan itself dictates, since a comment the task must write already matches it.

A genuine rethrow in C# is spelled `throw;` or `throw ex;` in lower case, so the case-sensitive form still excludes the construct this condition exists to exclude. A rethrow added to either catch arm would raise the case-sensitive count to one and fail the gate.

## Observed member shape

The authored member is:

    internal async Task InitializeWebViewGuardedAsync()
    {
        try
        {
            await InitializeWebViewAsync();
        }
        catch (OperationCanceledException)
        {
            // Cooperative cancellation during QuickFiler teardown is expected and is not a
            // fault: InitializeWebViewAsync opens with Token.ThrowIfCancellationRequested().
        }
        catch (Exception ex)
        {
            WebViewInitializationErrorSink("WebView2 initialization failed.", ex);
        }
    }

The broad `catch (Exception ex)` is deliberate and is permitted by `.claude/rules/csharp.md`, which allows it at a defined boundary with added context. `InitializeWebViewGuardedAsync` is that boundary, and the sink call supplies the context: a message identifying WebView2 initialization plus the exception instance.

The `catch (OperationCanceledException)` arm precedes the broad arm, which is required for it to be reachable: C# selects the first matching arm, and `OperationCanceledException` derives from `Exception`. Cooperative cancellation during QuickFiler teardown is expected rather than a fault, so it is swallowed without reaching the sink; `InitializeWebViewAsync` opens with `Token.ThrowIfCancellationRequested()` before any seam call, which is what makes that arm deterministically reachable from a pre-cancelled token.

Output Summary: All four required literals are present exactly once. The case-sensitive `throw` search returns zero, so the guard does not rethrow and its returned task never transitions to `Faulted`. The file measures 41 lines at this point, within the 60-line ceiling P1-T7 applies.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
