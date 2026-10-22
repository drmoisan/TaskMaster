# P4-T4 — AC1 pass-after evidence

Timestamp: 2026-09-08T09-52
Task: [P4-T4]
Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:q "/flp:LogFile=coverage/msbuild-p4-t4.log;Verbosity=normal" ; then <vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p4-t4.trx" /ResultsDirectory:coverage/trx/p4-t4 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None "/TestCaseFilter:FullyQualifiedName~DictionaryExtensions_Tests"
EXIT_CODE: 0

Paired fail-before record: `fail-before-exception.2026-09-08T09-50.md` (AC1 has no deterministic
failing run; see that dossier for why, and for the structural alternative proof).

## Build

| Observation | Value |
|---|---|
| Build exit code | `0` |
| Count of lines exactly equal to `    0 Error(s)` | `1` |

```
    0 Warning(s)
    0 Error(s)
```

## Test run

| Observation | Value |
|---|---|
| Test run exit code | `0` |

```
total=15 executed=15 passed=15 failed=0 error=0 timeout=0 aborted=0 notExecuted=0
```

`failed` = 0.

## Named outcomes

| Method | Outcome |
|---|---|
| `TryAddValuesAsync_UpdatesExistingValue` | `Passed` |
| `TryAddValuesAsync_PreCancelledToken_ThrowsTaskCanceledAndLeavesValueUnchanged` | `Passed` |

The first is the #780 test, retained unchanged: no assertion was weakened, no tolerance added, and
no attribute applied to serialise it. It now passes because the production defect it was tripping
over is gone, not because the test was adjusted.

The second is the new contract-lock test, which is what keeps the deletion honest: removing a
cancellation window could in principle have removed cancellation altogether, and this test fails if
a pre-cancelled caller token ever stops producing `TaskCanceledException` or if the value is
mutated despite cancellation.

## Duration of the #780 test

DURATION_UPDATES: `00:00:00.0027633`

Read from that result's `UnitTestResult/@duration`. 2.76 ms, which is under one second as the
acceptance requires, and is consistent with the roughly 2 ms the issue reports for an isolated
run. The failing runs recorded in #780 took about 20 s for this same test, because the 500 ms
window fired and the `await` observed a cancellation after the pool eventually scheduled the work.
With the window deleted there is no wall-clock deadline left in the method for load to exceed.

## Acceptance evaluation

- Build exit 0 with `    0 Error(s)`. PASS
- Test `EXIT_CODE: 0`. PASS
- `failed` = 0. PASS
- `Read-TrxOutcome` is `Passed` for both named methods. PASS
- The duration of `TryAddValuesAsync_UpdatesExistingValue` read from `@duration` is under one
  second (2.76 ms). PASS

## Output Summary

15 tests in `DictionaryExtensions_Tests`, all passed, exit 0. The retained #780 test runs in
2.76 ms and the new pre-cancelled-token contract test passes, so the cancellation semantics that
survive the deletion are locked by an assertion rather than assumed. AC1's "passes deterministically
under 24-worker parallel coverage runs" clause is evidenced separately by the P8-T6 ten-run gate.
