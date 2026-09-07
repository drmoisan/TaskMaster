# P5-T3 — AC3 fail-before run (expect-fail)

Timestamp: 2026-09-07T14-24
Task: [P5-T3] [expect-fail]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbDropDownCloseOrderingTests" /ResultsDirectory:TestResults\796\p5-t3 "/Logger:trx;LogFileName=p5-t3.trx"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 1
ExpectedExitCode: 1

Raw results (gitignored, never committed): TestResults/796/p5-t3/p5-t3.trx

## Run summary

```
Total tests: 3
     Passed: 2
     Failed: 1
```

## Per-test results

| Test | Result |
|---|---|
| FormatDropDownClosedDiagnostics_IncludesEveryDiscriminatingField | Passed |
| NativeCloseWhileCommitPending_DoesNotCancelSelection | Failed |
| NativeCloseWithNoCommitPending_StillCancelsSelection | Passed |

## Carve-out

- NativeCloseWhileCommitPending_DoesNotCancelSelection is Failed, which this gate requires.
- FormatDropDownClosedDiagnostics_IncludesEveryDiscriminatingField, landed by task P1-T5, is
  Passed, which this gate requires.
- NativeCloseWithNoCommitPending_StillCancelsSelection is recorded PASSED. The gate admits either
  Passed or Failed for this one test, because it asserts the behaviour the unfixed code already has
  for the clear-latch case; the observed value is recorded here rather than assumed.
- No test other than the two named ones is Failed. In fact only one of the two is Failed, so the
  carve-out is satisfied strictly.

## Failure detail (the assertion that matters)

```
Expected harness.CancelCount to be 0 because a close racing an in-flight commit must not cancel the selection, but found 1 (difference of 1).
```

That is the defect under repair: with the latch declared and cleared at open time but not yet
consulted by `FinishClose`, a native-reason close cancels the selection even while a commit has been
requested for that popup lifetime.

The failure is a runtime assertion failure, not a compile or arrange failure. The harness's own
pre-act assertions — that the open task completed, that the host reports open, that the show
delegate ran exactly once, and that the cancel delegate had not yet run — all passed, so the
headless host really reached the open state before the close was handed to it.

Output Summary: 3 total, 2 passed, 1 failed; the one failure is the named expect-fail test and the
scoping companion passed at this gate.
