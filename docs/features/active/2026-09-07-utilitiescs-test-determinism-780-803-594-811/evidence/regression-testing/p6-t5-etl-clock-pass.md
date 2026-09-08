# P6-T5 — ETL clock tests and tolerance retirement

Timestamp: 2026-09-08T10-04
Task: [P6-T5]
Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:q "/flp:LogFile=coverage/msbuild-p6-t5.log;Verbosity=normal" ; then <vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p6-t5.trx" /ResultsDirectory:coverage/trx/p6-t5 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None "/TestCaseFilter:FullyQualifiedName~OlTableExtensions_Tests|FullyQualifiedName~OlTableExtensionsEtlClockTests"
EXIT_CODE: 0

## Build

| Observation | Value |
|---|---|
| Build exit code | `0` |
| Count of lines exactly equal to `    0 Error(s)` | `1` |
| Warning count | `0` |

```
    0 Warning(s)
    0 Error(s)
```

## Test run

| Observation | Value |
|---|---|
| Test run exit code | `0` |

```
total=85 executed=85 passed=85 failed=0 error=0 timeout=0 aborted=0 notExecuted=0
```

`failed` = 0. No result carried an outcome other than `Passed`.

## Named outcomes

| Class | Method | Outcome |
|---|---|---|
| `OlTableExtensionsEtlClockTests` | `EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource` | `Passed` |
| `OlTableExtensionsEtlClockTests` | `EtlAsync_ClockNeverAdvances_ReturnsTransformedRows` | `Passed` |
| `OlTableExtensions_Tests` | `EtlAsync_WithBinaryAndObjectFieldsAndProgress_ReturnsTransformedData` | `Passed` |
| `OlTableExtensions_Tests` | `EtlByRowAsync_PrivateHelper_ReturnsConvertedRows` | `Passed` |

None returned `ABSENT`, so the newly registered file was compiled into the assembly and both of
its tests were discovered.

The third row is the test whose `Returns(120)` timing tolerance P6-T1 retired. It now runs with
the natural row count of 1, so its deadline is the real 250 ms rather than 30 000 ms, and it is
kept deterministic by an un-advanced `FakeTimeProvider` instead of by a mock value chosen to
outrun the deadline. That is the AC5-compliant replacement: the deadline is not widened, it is
placed under the test's control.

The fourth row is the reflection-binding test named in P6-T2 (its method is
`EtlByRowAsync_PrivateHelper_ReturnsConvertedRows`, and its `InvokeStaticAsync` call now sits at
lines 1122-1135). Its passing run is the behavioural confirmation of the static check P6-T2
recorded: the four-type array still resolves the unchanged four-parameter overload.

## The two new tests

`EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource` documents the surviving `EtlAsync`
contract rather than changing it: on expiry the `TimeoutException` is swallowed, `data` comes back
null through the null-forgiving suppression, and `tokenSource` is cancelled. This is the first
test in the repository to cover the `catch (TimeoutException)` block, which P0-T11 measured as
entirely uncovered at baseline (lines 117-123 of the pre-change file).

`EtlAsync_ClockNeverAdvances_ReturnsTransformedRows` is the paired green path on an un-advanced
clock, asserting `columnInfo["Store"]` is 1 and `data[0, 1]` is the injected binary string.

## Blame hang guard

SEQUENCE_FILES: 0

No `Sequence` file under `coverage/trx/p6-t5`, so the 4-minute blame hang timeout did not fire.
The gated deadline test reached its assertions rather than hanging, which confirms the timer was
armed before the clock advanced.

## Acceptance evaluation

- Build exit 0 with `    0 Error(s)`. PASS
- Test `EXIT_CODE: 0`. PASS
- `failed` = 0. PASS
- `Read-TrxOutcome` is `Passed` for both `OlTableExtensionsEtlClockTests` methods, for
  `EtlAsync_WithBinaryAndObjectFieldsAndProgress_ReturnsTransformedData`, and for the reflection
  test, whose method name is recorded above as `EtlByRowAsync_PrivateHelper_ReturnsConvertedRows`.
  PASS

## Output Summary

85 tests across the two table classes, all passed, exit 0. The `Returns(120)` timing tolerance is
retired and replaced by a controlled clock, and the previously uncovered ETL deadline-expiry path
now has a deterministic test.
