# P3-T2 — AC2 pass-after evidence

Timestamp: 2026-09-08T09-48
Task: [P3-T2]
Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:q "/flp:LogFile=coverage/msbuild-p3-t2.log;Verbosity=normal" ; then <vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p3-t2.trx" /ResultsDirectory:coverage/trx/p3-t2 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None "/TestCaseFilter:FullyQualifiedName~DfDeedleEtlTimeoutTests|FullyQualifiedName~DfDeedle_COM_Tests"
EXIT_CODE: 0

Paired fail-before record: `p2-t4-ac2-fail-before.md`.

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
total=28 executed=28 passed=28 failed=0 error=0 timeout=0 aborted=0 notExecuted=0
```

`failed` = 0. No result carried an outcome other than `Passed`.

## Named outcomes

| Class | Method | P2-T4 (before) | P3-T2 (after) |
|---|---|---|---|
| `DfDeedleEtlTimeoutTests` | `GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder` | `Failed` | `Passed` |
| `DfDeedleEtlTimeoutTests` | `GetEmailDataInViewAsync_ClockNeverAdvances_ReturnsOneRowFrame` | `Passed` | `Passed` |
| `DfDeedle_COM_Tests` | `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` | not in scope of that run | `Passed` |

The first row is the RED-to-GREEN transition AC2 requires. The only change between the two runs is
P3-T1's guard: no test text, no mock, and no deadline value was altered. The test still asserts the
exception type explicitly and the folder name through `.WithMessage("*Inbox*")`, so a regression to
`NullReferenceException`, or to an exception that does not name the folder, would fail it rather
than pass a generic assertion.

The third row is the #803 sentinel, which now runs with an un-advanced `FakeTimeProvider`, so every
deadline on its path is armed on a clock that does not move and cannot fire under host load.

## Note on the P3-T1 `#nullable enable` acceptance clause

P3-T1's acceptance lists `#nullable enable`=1 for `DfDeedle.cs`. The measured substring count is 2,
and it was also 2 at the base commit `bb1c7d4b` (verified with
`git grep -c -F '#nullable enable' bb1c7d4b -- UtilitiesCS/Extensions/DfDeedle.cs`, which returns
2). The second hit is line 157, a pre-existing #798 comment reading
"file's #nullable enable while the validator parameter is a non-nullable string." — it is prose,
not a directive, and this change does not touch it.

The clause was therefore resolved on its discriminator rather than on the stated number: the
`#nullable enable` directive itself occurs on exactly one line (a whole-line regex match returns
1, at line 23), and the substring count is identical before and after this change, so the edit
altered the file's nullable state in no way. Satisfying the literal count of 1 would have required
deleting an unrelated pre-existing comment, which is outside the write set's intent and is not
requested anywhere in the plan. This is recorded as a plan-citation defect for the orchestrator;
it changes no acceptance criterion and no other gate depends on the figure. P0-T6 did not assert
this count, so the halt gate had no opportunity to catch it.

## Acceptance evaluation

- Build `EXIT_CODE: 0` with `    0 Error(s)`. PASS
- Test `EXIT_CODE: 0`. PASS
- `failed` = 0. PASS
- `Read-TrxOutcome` is `Passed` for both `DfDeedleEtlTimeoutTests` methods and for
  `DfDeedle_COM_Tests` / `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`.
  PASS
- The paired fail-before record is named: `p2-t4-ac2-fail-before.md`. PASS

## Output Summary

28 tests across the two `DfDeedle` classes, all passed, exit 0. The AC2 deadline-expiry test moved
from `Failed` with `NullReferenceException` to `Passed` with the descriptive
`InvalidOperationException` naming the folder, with the guard as the only intervening change.
