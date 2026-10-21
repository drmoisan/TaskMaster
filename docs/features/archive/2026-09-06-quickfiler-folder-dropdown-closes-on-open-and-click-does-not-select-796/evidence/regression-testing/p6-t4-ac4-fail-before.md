# P6-T4 — AC4 fail-before run (expect-fail)

Timestamp: 2026-09-07T14-40
Task: [P6-T4] [expect-fail]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~QfcItemController_SearchLeaveLatchTests" /ResultsDirectory:TestResults\796\p6-t4 "/Logger:trx;LogFileName=p6-t4.trx"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 1
ExpectedExitCode: 1

Raw results (gitignored, never committed): TestResults/796/p6-t4/p6-t4.trx

## Run summary

```
Total tests: 2
     Passed: 1
     Failed: 1
```

Total is 2 and not 0, so the compile entry task P6-T3 added took effect and this gate is not passing
on an empty population.

## Per-test results

| Test | Result |
|---|---|
| SearchLeaveAfterMouseDrivenOpen_DoesNotCloseDropDown | Failed |
| SearchLeaveAfterSearchDrivenOpen_ClosesDropDown | Passed |

## Carve-out

The single Failed test is exactly SearchLeaveAfterMouseDrivenOpen_DoesNotCloseDropDown, landed by
task P6-T2 as a deliberately-failing regression test and made to pass by task P6-T5. The paired
positive test is Passed. No test other than that exact named test is Failed.

The filter names the new class exactly and does not match QfcItemController_EventHandlersTests: the
run reports a Total of 2, which is the count of `[TestMethod]` members in the new class alone, and
none of the other class's fourteen tests appears in the result lines.

## Failure detail (the assertion that matters)

```
Moq.MockException:
Expected invocation on the mock should never have been performed, but was 1 times: v => v.SetFolderDroppedDown(False)
```

That is the AC4 gap: with the latch declared but not consulted, `TextBoxSearch_Leave` dismisses a
drop-down that a mouse gesture opened and the search box never owned. The recorded invocation list
shows the handler read `IsFolderDropDownOpen` and then dismissed, which is the unconditional
ownership the fix removes.

## Note on unrelated console output

The run's standard output carries a FluentAssertions licensing notice emitted by the assertion
library on first use. It is not a test result and has no bearing on this gate; it is recorded here
only so a later reader does not mistake it for a diagnostic from the code under test.

Output Summary: 2 total, 1 passed, 1 failed; the one failure is the named expect-fail test and the
paired positive test passed.
