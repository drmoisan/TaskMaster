# Fail-before — Init_CreatesTokenSourceBeforeAnyLoaderObservesIt — issue #839

Timestamp: 2026-09-13T05-47
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage | Out-Null; & $vstest QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation "/logger:console;verbosity=normal" "/TestCaseFilter:FullyQualifiedName=QuickFiler.Controllers.Tests.QfcHomeControllerTests.Init_CreatesTokenSourceBeforeAnyLoaderObservesIt" | Tee-Object -FilePath coverage/839-fail-before-tests.log; $code = $LASTEXITCODE; "VSTEST_EXIT=$code"; exit $code'
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; $log = Get-Content -LiteralPath coverage/839-fail-before-tests.log; foreach ($k in "Total tests:", "Passed:", "Failed:", "Skipped:") { $m = @($log | Select-String -Pattern ("^\s*" + [regex]::Escape($k) + "\s*(\d+)")); "$($k -replace ":") LINES=$($m.Count) VALUE=$(if ($m.Count -gt 0) { $m[-1].Matches[0].Groups[1].Value } else { 0 })" }; "RUN_SUCCESSFUL_LINES=$(@($log | Select-String -SimpleMatch "Test Run Successful.").Count)"; "RUN_FAILED_LINES=$(@($log | Select-String -SimpleMatch "Test Run Failed.").Count)"; foreach ($t in "Init_CreatesTokenSourceBeforeAnyLoaderObservesIt", "Init_InitializesCorrectly", "Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource") { "PASSED_$t=$(@($log | Select-String -Pattern ("^\s*Passed\s+" + $t + "\b")).Count) FAILED_$t=$(@($log | Select-String -Pattern ("^\s*Failed\s+" + $t + "\b")).Count)" }; "NOTNULL_MESSAGE_LINES=$(@($log | Select-String -SimpleMatch "not to be").Count)"'
EXIT_CODE: 1
ExpectedExitCode: 1

## Output Summary

VSTEST_EXIT=1
Total tests LINES=1 VALUE=1
Passed LINES=0 VALUE=0
Failed LINES=1 VALUE=1
Skipped LINES=0 VALUE=0
RUN_SUCCESSFUL_LINES=0
RUN_FAILED_LINES=0
PASSED_Init_CreatesTokenSourceBeforeAnyLoaderObservesIt=0 FAILED_Init_CreatesTokenSourceBeforeAnyLoaderObservesIt=1
PASSED_Init_InitializesCorrectly=0 FAILED_Init_InitializesCorrectly=0
PASSED_Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource=0 FAILED_Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource=0
NOTNULL_MESSAGE_LINES=1

`Init_CreatesTokenSourceBeforeAnyLoaderObservesIt` failed on the not-null assertion, which is the first of the test's seven assertions and the one the plan predicts fails first against the unfixed production file. The test-case filter matched exactly one test, so this is a real observation of the named test and not a zero-match run reported as a pass.

First line of the error message, transcribed verbatim:

    Expected capturedSource not to be <null>.

The subject name the message carries is `capturedSource`, the local the form-controller loader lambda assigns. The plan deliberately does not assert the subject name, only the `not to be` substring, and that substring is present once.

Stack frame naming the assertion site, host prefix replaced: `QuickFiler.Controllers.Tests.QfcHomeControllerTests.Init_CreatesTokenSourceBeforeAnyLoaderObservesIt()` in REPO-ROOT\QuickFiler.Test\Controllers\QfcHomeControllerTests.cs line 224, which is the `capturedSource.Should().NotBeNull();` line of the inserted method.

This is the defect reproduced: `Init()` passes `this._tokenSource` to the form-controller loader before anything has assigned it, so the captured source is null and the datamodel and queue tokens are `default(CancellationToken)`.

## RUN_FAILED_LINES=0 is expected on a failing run and gates nothing

The console printed `Test Run Failed.` but the log records `RUN_FAILED_LINES=0`, because vstest writes that terminal line to the error stream while `Tee-Object` captures the output stream only. The per-test `Failed` line and the `Total tests:` and `Failed:` count lines are on the output stream and were captured, so every value this task's acceptance reads is present. No acceptance condition in this plan reads `RUN_FAILED_LINES`. The complementary condition `RUN_SUCCESSFUL_LINES=1`, which the pass-after gates in [P2-T6] and [P3-T4] do read, is written to the output stream and was captured on the green [P0-T18] baseline run, so that gate is not affected by this stream split.

## No .trx was produced by this run

`git status --porcelain --untracked-files=all` prints three lines and none ends in .trx:

     M QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
     M docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/plan.2026-09-12T22-14.md
    ?? docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/regression-testing/p1-build.md

No task in this plan passes a TRX logger switch, and this run's only logger switch is the console logger, so no .trx can have been produced. That was confirmed against the filesystem rather than inferred from the porcelain span alone, and the confirmation surfaced a pre-existing repository condition worth recording accurately:

- `TRX_FILES_ANYWHERE=332` and `TESTRESULTS_DIR_EXISTS=True`.
- Every one of those 332 files is a tracked file committed under a different feature folder's evidence tree, for example the issue 501 breadcrumb feature folder's evidence/baseline/trx and evidence/regression-testing/trx directories. They arrived with the branch checkout, not with this run.
- Their newest write time is 2026-09-13T02-19, which precedes this session's first command at 05:34, so none was written by any run recorded in this feature folder.
- Because they are tracked and unmodified they do not appear in the porcelain span, and because they are unchanged from the base commit they will not appear in the anchored diff that [P3-T13] gates. This item neither adds nor removes any of them.

That other feature folders committed raw .trx contrary to the issue 671 projections-only decision is a pre-existing condition outside this item's Write Set. It is reported, not corrected here.

Raw disposition: the vstest console log lives under the gitignored coverage directory at the worktree root and is not committed. Only this projection is committed.

## Command-transport adaptations, both semantics-preserving

1. `Set-Location -LiteralPath "REPO-ROOT";` prepended to both spans. Forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one; all paths in both spans are worktree-relative.
2. In the summary span, the plan's `-replace ":", ""` was run as the single-operand `-replace ":"`, which replaces every match with the empty string and prints an identical label. Forced: inside an expandable string a `$()` subexpression containing the empty nested string `""` is read by the PowerShell parser as an escaped quote, so the two-operand form exits 1 having printed nothing. The full isolation probe is recorded in evidence/baseline/baseline-quickfiler-tests.md, and the same adaptation is applied identically wherever this plan cites CMD-TEST-SUMMARY.
