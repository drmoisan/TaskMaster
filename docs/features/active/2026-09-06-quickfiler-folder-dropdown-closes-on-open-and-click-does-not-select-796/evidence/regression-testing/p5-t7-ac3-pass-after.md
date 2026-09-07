# P5-T7 — AC3 pass-after run

Timestamp: 2026-09-07T14-30
Task: [P5-T7]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbDropDownCloseOrderingTests|FullyQualifiedName~BreadcrumbPendingOpenCloseTests" /ResultsDirectory:TestResults\796\p5-t7 "/Logger:trx;LogFileName=p5-t7.trx"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0

Raw results (gitignored, never committed): TestResults/796/p5-t7/p5-t7.trx

## Run summary

```
Test Run Successful.
Total tests: 9
     Passed: 9
```

No test in either class is recorded as Failed. vstest.console.exe prints no `Failed:` line on a
successful run, so the Failed count is 0 with the note NOT PRINTED ON A PASSING RUN.

## Per-test results

| Test | Class | Result |
|---|---|---|
| FormatDropDownClosedDiagnostics_IncludesEveryDiscriminatingField | BreadcrumbDropDownCloseOrderingTests | Passed |
| NativeCloseWhileCommitPending_DoesNotCancelSelection | BreadcrumbDropDownCloseOrderingTests | Passed |
| NativeCloseWithNoCommitPending_StillCancelsSelection | BreadcrumbDropDownCloseOrderingTests | Passed |
| CloseWhileFactoryPending_InvalidatesOpenAndRepeatedCloseIsIdempotent | BreadcrumbPendingOpenCloseTests | Passed |
| CloseWhileReadinessPending_RejectsLateReadyAttachShowAndFocus | BreadcrumbPendingOpenCloseTests | Passed |
| CloseCanceledFactory_AllowsOneFreshReopenWithoutLateMutation | BreadcrumbPendingOpenCloseTests | Passed |
| CloseWhilePendingOpenAndCommitPending_DoesNotCancelSelection | BreadcrumbPendingOpenCloseTests | Passed |
| ToggleAndEscapeWhileOpenIsPending_EachClosesHostExactlyOnce | BreadcrumbPendingOpenCloseTests | Passed |
| AutomaticSelectorCloseWhileOpenIsPending_ClosesHostExactlyOnce | BreadcrumbPendingOpenCloseTests | Passed |

vstest.console.exe prints one combined Total for a multi-class filter and no per-class subtotal, so
the class column above was derived by attributing each recorded per-test result line to the class
that declares it, not read from the runner.

## The proof that the suppression did not become global

Both named scoping tests are Passed:

- CloseWhileFactoryPending_InvalidatesOpenAndRepeatedCloseIsIdempotent — Passed. It carries the
  retained `CancelCount.Should().Be(1)` assertion at line 48.
- CloseWhileReadinessPending_RejectsLateReadyAttachShowAndFocus — Passed. It carries the retained
  `CancelCount.Should().Be(1)` assertion at line 79.

Neither was driven to zero by the fix, so a close with no commit in flight still cancels.

## Fail-before to pass-after transition

| Test | P5-T3 (fail-before) | P5-T7 (pass-after) |
|---|---|---|
| NativeCloseWhileCommitPending_DoesNotCancelSelection | Failed | Passed |
| NativeCloseWithNoCommitPending_StillCancelsSelection | Passed | Passed |

The only expect-fail tests in these classes were landed by P5-T2 and made to pass by P5-T4, both of
which precede this gate.

Output Summary: 9 total, 9 passed, 0 failed; the AC3 expect-fail test transitioned Failed to Passed
and both retained scoping assertions still hold.
