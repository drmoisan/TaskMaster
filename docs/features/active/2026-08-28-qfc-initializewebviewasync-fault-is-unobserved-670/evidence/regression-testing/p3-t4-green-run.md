# P3-T4 — Discriminating pair, green half

Timestamp: 2026-09-01T20-00
Command:

    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
    & $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/TestCaseFilter:FullyQualifiedName~InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault' /Logger:trx '/ResultsDirectory:coverage\testresults\p3-t4'

The resolved test runner is recorded in the placeholder form the plan's section 0 prescribes: `<vs-install>\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.

EXIT_CODE: 0

## Output Summary

    VSTest version 18.9.0 (x64)

    Starting test execution, please wait...
    A total of 1 test files matched the specified pattern.
    Test Parallelization enabled for <repo-root>\.claude\worktrees\agent-<id>\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll (Workers: 24, Scope: ClassLevel)
      Passed InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault [221 ms]
    Results File: <repo-root>\.claude\worktrees\agent-<id>\coverage\testresults\p3-t4\<trx-name>.trx

    Test Run Successful.
    Total tests: 1
         Passed: 1
     Total time: 1.3253 Seconds

`/ResultsDirectory` is supplied because `/Logger:trx` otherwise writes into a `TestResults` directory relative to the working directory, which would collide across the three runs of this identical command in P3-T4, P3-T5 and P3-T6.

## The pass is not vacuous

An exit code of 0 from a filtered vstest run is also what a filter matching **no test** produces, so the exit code alone cannot distinguish a real pass from an empty selection. Two independent observations exclude that case:

- The generated `.trx` contains **3** fixed-string hits for `InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault`, so the named test is genuinely recorded in the result document.
- The `.trx` result summary reads `outcome=Completed`, `total=1`, `passed=1`, `failed=0`. A filter matching nothing would have produced `total=0`.

The console output independently names the test on a `Passed` line with a 221 ms duration.

## Artifact

The `.trx` was copied to `evidence/regression-testing/p3-t4-green.trx`. The generated filename produced by the runner embeds the account name and the machine name; copying to a fixed evidence name removes both from the path. The document's own contents still carry host identifiers and are sanitised in place by P3-T14 before the Phase 3 commit stages anything.

## Role in the discriminating pair

This is the green half of the substantive red step. P3-T5 removes the sink invocation from the guard's `catch (Exception ex)` arm, rebuilds, and re-runs the **identical** command against the **same** assembly path with only the results directory changed. The pair of exit codes — 0 here, non-zero there — is the evidence that the test actually observes the fix. A test that passed in both states would prove it does not observe the fix at all, and its green result here would be worthless.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
