# Scoped build and run of the new test class (issue #826, [P2-T4])

Timestamp: 2026-09-09T19-18

This is an interim scoped check, not a toolchain-loop pass. Its purpose is to surface a compile or
registration failure here rather than in the Phase 7 final QA loop.

Commands, run as one `pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard:

```
msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /fl "/flp:LogFile=coverage/826-raw/p2-t4-testproj.log;Verbosity=detailed"
<vstest.console.exe> "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation "/Logger:trx;LogFileName=p2-t4.trx" /ResultsDirectory:coverage/826-raw/p2-t4 /TestCaseFilter:FullyQualifiedName~OlTableExtensionsTimeoutDiagnosticsTests
```

`/v:q` was added to the msbuild console channel only; the detailed-verbosity file logger is unaffected.

EXIT_CODE: 0 (msbuild) and 0 (vstest) — both exit codes are recorded as the acceptance condition requires.

## TRX counters

Read from `coverage/826-raw/p2-t4/p2-t4.trx`, `ResultSummary/Counters`:

| Counter | Value | Required |
|---|---|---|
| total | 2 | — |
| executed | 2 | 2 |
| passed | 2 | 2 |
| failed | 0 | 0 |
| notExecuted | 0 | — |

An `executed` count of 0 would have proved the `<Compile Include>` entry did not take effect. The
observed count of 2 proves the new file is compiled into `UtilitiesCS.Test.dll` and that both test
methods are discovered and run.

## Test methods present in the TRX, matched on `TestMethod/@name`

- `GetTableInViewAsync_TimeoutSourceThrowsTimeout_EntersTimeoutCatchAndRetriesOnce`
- `GetTableInViewAsync_TimeoutSourceThrowsTaskCanceled_EntersCancelCatchElseAndRetriesOnce`

Both names match the two recorded by [P2-T2] exactly.

Console outcome lines from the run:

```
  Passed GetTableInViewAsync_TimeoutSourceThrowsTimeout_EntersTimeoutCatchAndRetriesOnce [173 ms]
  Passed GetTableInViewAsync_TimeoutSourceThrowsTaskCanceled_EntersCancelCatchElseAndRetriesOnce [1 ms]
Test Run Successful.
Total tests: 2
     Passed: 2
```

Output Summary: the `UtilitiesCS.Test` project rebuilt clean, the new class was discovered, and both test
methods passed. The sub-second durations confirm neither test waits on wall time. All acceptance
conditions for [P2-T4] hold.

The raw TRX and msbuild log stay under the gitignored `coverage/826-raw/` directory and are not
committed, because a TRX records the account name in `runUser=` and the machine name in `computerName=`.
