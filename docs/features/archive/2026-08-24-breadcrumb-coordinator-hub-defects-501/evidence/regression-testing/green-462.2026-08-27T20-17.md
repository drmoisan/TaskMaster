# GREEN AFTER THE FIX — #462 (P1-T6)

Timestamp: 2026-08-27T20-17

## Step 1 — analyzer build

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0
Output Summary: `5 Warning(s)`, `0 Error(s)`, `Time Elapsed 00:00:19.74`. The five warnings are the same
pre-existing `System.Reactive` `packages.config` advisory recorded at baseline.
Count of lines matching `Skipping target "CoreCompile"`: **0** — the gate is non-vacuous.

## Step 2 — scoped test run

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~RequestOpen_AfterSuccessfulCloseAndHostReopen_ReachesHostOpenAsync|FullyQualifiedName~CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce'
    '/Logger:trx;LogFileName=p1-t6.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p1-t6'
```

EXIT_CODE: 0

Output Summary:

```
  Passed RequestOpen_AfterSuccessfulCloseAndHostReopen_ReachesHostOpenAsync [69 ms]
  Passed CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce [< 1 ms]

Test Run Successful.
Total tests: 2
     Passed: 2
 Total time: 1.3809 Seconds
```

| Metric | Value |
| --- | ---: |
| Tests run | 2 |
| Passed | 2 |
| Failed | 0 |
| Skipped | 0 |

## Red-to-green transition

| Test | P1-T2 / P1-T4 (before the fix) | P1-T6 (after the fix) |
| --- | --- | --- |
| `RequestOpen_AfterSuccessfulCloseAndHostReopen_ReachesHostOpenAsync` | FAILED — observed `Requests` count 1 | PASSED |
| `CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce` | PASSED | PASSED |

The reopen test moved from RED to GREEN, and the idempotent-close guard held across the change. That
pair is the discriminating evidence: the fix delivered I-462.2 without sacrificing I-462.3, which is
exactly what rules out research section 6.1 option A.

TRX artifact: `FF/evidence/regression-testing/trx/p1-t6/p1-t6.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: `EXIT_CODE: 0` with 2 passed, 0 failed, 0 skipped. PASS.
