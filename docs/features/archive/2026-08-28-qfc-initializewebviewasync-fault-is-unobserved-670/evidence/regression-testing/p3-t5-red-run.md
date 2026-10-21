# P3-T5 [expect-fail] — Discriminating pair, red half

Timestamp: 2026-09-01T20-01
Command: the **identical** vstest invocation P3-T4 ran, differing only in the results directory:

    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
    & $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/TestCaseFilter:FullyQualifiedName~InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault' /Logger:trx '/ResultsDirectory:coverage\testresults\p3-t5'

The resolved test runner is recorded as `<vs-install>\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.

EXIT_CODE: 1
ExpectedExitCode: 1

## The mutation

Inside `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs`, in the guard's `catch (Exception ex)` arm, this single statement:

    WebViewInitializationErrorSink("WebView2 initialization failed.", ex);

was replaced by this single statement:

    _ = ex;

A discard assignment was used rather than an empty arm so that `ex` remains used and no unused-variable diagnostic appears, which keeps the mutation a one-line, exactly reversible substitution that changes only the behaviour under test. Nothing else in the tree was altered.

## The rebuild was clean and the assembly is not stale

    Build succeeded.
        5 Warning(s)
        0 Error(s)

    Time Elapsed 00:00:12.24

Zero coded diagnostics (`: error [A-Z]+[0-9]+:` returns 0; `: warning [A-Z]+[0-9]+:` returns 0). The mutation compiles cleanly, so the red result below is a behavioural failure rather than a compilation failure.

The rebuilt test assembly is demonstrably the one that ran:

    BUILD_START_UTC = 2026-09-02T00:01:21.9946953Z
    DLL_WRITE_UTC   = 2026-09-02T00:01:30.1676065Z
    DLL_NEWER       = True

`QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` was written after the rebuild started, so a stale assembly carrying the unmutated guard cannot be the source of this result. Without this check the red result would be equally consistent with the test failing for an unrelated reason against an old binary.

## Output Summary and the exact failure message

      Failed InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault [315 ms]
      Error Message:
       Expected captured to be QuickFiler.Controllers.Tests.QfcItemController_InitializationTests+WebViewSentinelException
       because the sink must receive the exact fault raised at the mocked seam, but found <null>.

    Total tests: 1
         Failed: 1
    Test Run Failed.
     Total time: 1.4306 Seconds

The `.trx` was copied to `evidence/regression-testing/p3-t5-red.trx`. It contains **4** fixed-string hits for the test name and its result summary reads `outcome=Failed`, `total=1`, `passed=0`, `failed=1`, so the run genuinely executed the named test and failed it rather than matching nothing.

## Why this failure message is the right one

The test carries two assertions. The first, `await act.Should().NotThrowAsync(...)`, still **passes** under the mutation: the guard still catches the exception, so the returned task still does not fault. The second, `captured.Should().BeOfType<WebViewSentinelException>(...)`, is the one that fails, and it fails with `but found <null>` — the sink was never invoked, so the capture variable was never assigned.

That is the precise discrimination this step exists to establish. The test is sensitive to the **sink invocation specifically**, not merely to the presence of a `try`/`catch`. A weaker test that only asserted no-throw would have passed under this mutation and would therefore have provided no evidence that the observation behaviour works at all.

## The discriminating pair

| Run | Guard's `catch (Exception ex)` arm | vstest EXIT_CODE | trx outcome |
| --- | --- | --- | --- |
| P3-T4 | `WebViewInitializationErrorSink("WebView2 initialization failed.", ex);` | 0 | Completed, 1 passed |
| P3-T5 | `_ = ex;` | 1 | Failed, 1 failed |

Same command, same filter, same assembly path, same runsettings, same working directory. The only difference between the two runs is the presence of the sink invocation. This pair is the substantive red step for the bugfix workflow: the literal "write a failing test first" step cannot be applied to this defect, because a test authored before the fix would reference members that do not exist and would fail to **compile**, and a non-compiling test assembly reports nothing about the defect.

P3-T6 restores the sink invocation and re-runs the identical command, closing the demonstration.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
