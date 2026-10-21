# Pass-after — whole QuickFiler.Test assembly on the fixed tree — issue #839

Timestamp: 2026-09-13T05-56
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage | Out-Null; & $vstest QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation "/logger:console;verbosity=normal" "/TestCaseFilter:TestCategory!=LiveOutlook" | Tee-Object -FilePath coverage/839-pass-after-tests.log; $code = $LASTEXITCODE; "VSTEST_EXIT=$code"; exit $code'
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; $log = Get-Content -LiteralPath coverage/839-pass-after-tests.log; foreach ($k in "Total tests:", "Passed:", "Failed:", "Skipped:") { $m = @($log | Select-String -Pattern ("^\s*" + [regex]::Escape($k) + "\s*(\d+)")); "$($k -replace ":") LINES=$($m.Count) VALUE=$(if ($m.Count -gt 0) { $m[-1].Matches[0].Groups[1].Value } else { 0 })" }; "RUN_SUCCESSFUL_LINES=$(@($log | Select-String -SimpleMatch "Test Run Successful.").Count)"; "RUN_FAILED_LINES=$(@($log | Select-String -SimpleMatch "Test Run Failed.").Count)"; foreach ($t in "Init_CreatesTokenSourceBeforeAnyLoaderObservesIt", "Init_InitializesCorrectly", "Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource") { "PASSED_$t=$(@($log | Select-String -Pattern ("^\s*Passed\s+" + $t + "\b")).Count) FAILED_$t=$(@($log | Select-String -Pattern ("^\s*Failed\s+" + $t + "\b")).Count)" }; "NOTNULL_MESSAGE_LINES=$(@($log | Select-String -SimpleMatch "not to be").Count)"'
EXIT_CODE: 0

## Output Summary

VSTEST_EXIT=0
Total tests LINES=1 VALUE=1394
Passed LINES=1 VALUE=1394
Failed LINES=0 VALUE=0
Skipped LINES=0 VALUE=0
RUN_SUCCESSFUL_LINES=1
RUN_FAILED_LINES=0
PASSED_Init_CreatesTokenSourceBeforeAnyLoaderObservesIt=1 FAILED_Init_CreatesTokenSourceBeforeAnyLoaderObservesIt=0
PASSED_Init_InitializesCorrectly=1 FAILED_Init_InitializesCorrectly=0
PASSED_Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource=1 FAILED_Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource=0
NOTNULL_MESSAGE_LINES=0

Counts under the [P0-T18] derivation rule: Total tests 1394 and Passed 1394 are read from their lines, whose `LINES=` values are 1. Failed is recorded as 0 and Skipped as 0 with the absence stated: neither line is present in the log (`LINES=0`) and `RUN_SUCCESSFUL_LINES=1`, which is the condition the rule requires before reading an absent line as 0. The zero-failure result is a confirming count taken on the post-change tree, not a subset argument from the baseline.

Population arithmetic: the [P0-T18] baseline total was 1393 and this run reports 1394, which is the baseline total plus exactly one. The one added test is the regression test this item introduces. No test was removed, renamed or filtered out.

Passed among them, all three named by the plan's gates:

    Init_CreatesTokenSourceBeforeAnyLoaderObservesIt
    Init_InitializesCorrectly
    Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource

The three `QfcInitEmailQueueZeroBatchTests` tests that were red before the Decision D16 correction to the vstest invocation are green in this run, as they were in the [P0-T18] baseline. Both sides of every before-and-after comparison in this plan are therefore measured under one identical method.

Raw disposition: the vstest console log lives under the gitignored coverage directory at the worktree root and is not committed.

## Command-transport adaptations, both semantics-preserving

1. `Set-Location -LiteralPath "REPO-ROOT";` prepended to both spans. Forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one.
2. In the summary span, the plan's `-replace ":", ""` was run as the single-operand `-replace ":"`, which replaces every match with the empty string and prints an identical label. The isolation probe for the forcing PowerShell parse defect is recorded in evidence/baseline/baseline-quickfiler-tests.md, and the same adaptation is applied identically wherever this plan cites CMD-TEST-SUMMARY, including the [P0-T18] baseline this run is compared against.
