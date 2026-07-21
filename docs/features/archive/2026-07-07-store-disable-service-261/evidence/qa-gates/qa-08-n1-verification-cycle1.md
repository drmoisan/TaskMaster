# QA Gate 8 — N1 Fix Verification (Remediation Cycle 1)

- Timestamp: 2026-07-08T00-56
- Source: P2-T4 vstest run output (`evidence/qa-gates/qa-04-mstest-cycle1.md` and the underlying
  run log)

## Verification

Both N1-affected test methods appear as individually passed results in the P2-T4 MSTest run
output (not silently skipped as fire-and-forget async calls):

```
Passed Writes_ThrowArgumentException_ForSentinelIdentity [6 ms]
Passed Writes_ThrowInvalidOperation_WhenModelIsNull [3 ms]
```

Both methods are now `async Task` (changed from `public void`) with `await` immediately preceding
their `ReenableAsync` `.Should().ThrowAsync<...>()` assertions (per P1-T9/P1-T10). Because vstest
reports an explicit timed `Passed` result with a non-zero elapsed time for each method, and MSTest
awaits the returned `Task` from an `async Task` test method before recording its outcome, this
confirms the `ReenableAsync` guard-path assertions genuinely execute and are verified as part of
the test run, rather than being fire-and-forget calls whose exceptions (or lack thereof) would
never be observed by the test framework.
