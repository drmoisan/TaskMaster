# P0-T13 — Pre-Change Changed-Line Coverage

Timestamp: 2026-09-01T08-12

Command: an XML query over `coverage\p0-t10.cobertura.xml` (produced by P0-T10), applying the
plan's stated lookup rule — read every `<line>` element whose `number` attribute equals the target
line, across all `<class>` elements whose `filename` attribute ends with `TimeOutTask.cs`; record the
count of matching elements and the `hits` value of each; the recorded hit count is the maximum over
those elements.

EXIT_CODE: 0

## Why the Multi-Element Rule Was Necessary

`RunWithTimeout<T1, TResult>` is `async`, so the compiler emits nested state-machine types. The
report carries **28 `<class>` elements** whose `filename` ends with `TimeOutTask.cs`. Their `name`
attributes include the outer type `UtilitiesCS.TimeOutTask`, closure display classes such as
`UtilitiesCS.TimeOutTask.<>c__DisplayClass6_0<T1, TResult>`, and one async state machine per
overload, for example `UtilitiesCS.TimeOutTask.<RunWithTimeout>d__6<T1, TResult>`. Several of these
carry overlapping `<line number=...>` entries for the same physical source line. Reading only the
first matching element would have produced an arbitrary result; the maximum-over-elements rule is
what makes the figure well-defined.

## Distinct Cobertura `filename` Values Matched

Exactly **one** distinct `filename` value was matched by all 28 `<class>` elements:

```text
<worktree-root>\UtilitiesCS\Threading\TimeOutTask.cs
```

(The report stores an absolute path. The machine- and account-specific prefix is written here as
`<worktree-root>` so this artifact carries no host path; the path segment that identifies the file,
`UtilitiesCS\Threading\TimeOutTask.cs`, is reproduced verbatim. There is exactly one distinct value,
so no second entry exists to enumerate.)

## Target Line 189 — the timeout-source construction being replaced

Source at line 189 (pre-change):
`using var timeoutSource = new CancellationTokenSource(milliseconds);`

| Property | Value |
| --- | --- |
| Present in report | **Yes** |
| Matching `<line>` element count | **2** |
| `hits` of each matching element | **1, 1** |
| **RECORDED HIT COUNT (maximum)** | **1** |

## Target Line 202 — first statement inside the `catch (TimeoutException)` clause at line 200

Source at line 202 (pre-change): `token.ThrowIfCancellationRequested();`

| Property | Value |
| --- | --- |
| Present in report | **Yes** |
| Matching `<line>` element count | **2** |
| `hits` of each matching element | **1, 1** |
| **RECORDED HIT COUNT (maximum)** | **1** |

Line 202 already shows a non-zero baseline hit count. This is consistent with the spec's root-cause
analysis: the clause at line 200 is dead only for a *timer-driven* timeout. The existing test
`RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException` fakes a timeout by throwing
`TimeoutException` directly from the delegate, which does match the clause and does execute its body.
The baseline therefore already covers this line by the `TimeoutException` path, and the fix adds the
`TaskCanceledException` path to the same body rather than covering a previously unreached line.

## Presence Statement

Both target lines are **present in the report**. Neither required the literal
`NOT PRESENT IN REPORT`.

## Reported Repository-Wide Figure

Root `<coverage>` `line-rate`: `0.7082975641163215`, that is 70.83%. Recorded as a reported figure
only; no threshold is asserted by this plan.

Output Summary: Baseline recorded hit counts are **line 189 = 1** and **line 202 = 1**. Each was
derived from 2 matching `<line>` elements with `hits` values 1 and 1, across 28 `<class>` elements
sharing a single distinct `filename` value ending `UtilitiesCS\Threading\TimeOutTask.cs`. Both lines
are present in the report. P3-T7 compares its post-change figures against these two values to show
that no changed line moved from covered to uncovered.

Acceptance: met. Two explicit recorded hit counts (1 and 1), each a non-negative integer rather than
`NOT PRESENT IN REPORT`; for each of the two lines the number of matching `<line>` elements (2) and
the `hits` value of every one of them (1, 1) are recorded; and every distinct Cobertura `filename`
value matched is enumerated (there is exactly one).
