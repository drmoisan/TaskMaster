# Pre-Existing Metrics Tests Still Pass After the Guard (P1-T12)

Timestamp: 2026-09-01T12-45

Production file state: guarded — the P1-T5 fix is applied.

Command:
`& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce|FullyQualifiedName~WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting"`
EXIT_CODE: 0

## Verbatim Output

```
  Passed WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce [260 ms]
  Passed WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting [1 ms]

Test Run Successful.
Total tests: 2
     Passed: 2
 Total time: 1.3594 Seconds
```

## Acceptance

| Condition | Required | Observed | Met |
|---|---|---|---|
| `EXIT_CODE` | `0` | `0` | Yes |
| Printed summary shows `Passed:     2` | yes | `     Passed: 2` | Yes |

ACCEPTANCE: MET.

## Why These Two Tests Are the Correct Regression Check

Both tests exercise the same `WriteMetricsAsync` path the guard was inserted into, with
**non-empty** filtered arrays, so they are the tests most exposed to an over-broad guard:

- `WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce` supplies
  `{ "line-one", "line-two" }`. After filtering, `lines.Length == 2`, so the new guard must
  not fire and the writer must still be invoked exactly once. It passed, confirming the
  guard does not suppress legitimate writes.
- `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` supplies
  `{ "line-one", "   ", null, "line-two" }` — a *partially* null/whitespace array. After
  filtering, `lines.Length == 2`, so again the guard must not fire, and the writer must
  receive exactly `{ "line-one", "line-two" }`. It passed, confirming the guard did not
  turn the partial-filter case into an early return.

Together these bound the guard's behavior from the other side: the new test (P1-T9) proves
it fires when the filtered array is empty, and these two prove it does not fire when the
filtered array is non-empty, including when the unfiltered input contained null and
whitespace entries. The boundary between the two cases is exactly `lines.Length == 0`.

Both tests were genuinely executed (260 ms and 1 ms, with per-test `Passed` lines printed),
not skipped by the filter.

Combined with P1-T13, which shows the test-file diff contains zero removed lines, this is
the evidence backing AC5.
