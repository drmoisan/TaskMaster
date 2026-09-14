# Scoped pass-after — Init_CreatesTokenSourceBeforeAnyLoaderObservesIt — issue #839

Timestamp: 2026-09-13T05-54
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage | Out-Null; & $vstest QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation "/logger:console;verbosity=normal" "/TestCaseFilter:FullyQualifiedName=QuickFiler.Controllers.Tests.QfcHomeControllerTests.Init_CreatesTokenSourceBeforeAnyLoaderObservesIt" | Tee-Object -FilePath coverage/839-scoped-pass-tests.log; $code = $LASTEXITCODE; "VSTEST_EXIT=$code"; exit $code'
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; $log = Get-Content -LiteralPath coverage/839-scoped-pass-tests.log; foreach ($k in "Total tests:", "Passed:", "Failed:", "Skipped:") { $m = @($log | Select-String -Pattern ("^\s*" + [regex]::Escape($k) + "\s*(\d+)")); "$($k -replace ":") LINES=$($m.Count) VALUE=$(if ($m.Count -gt 0) { $m[-1].Matches[0].Groups[1].Value } else { 0 })" }; "RUN_SUCCESSFUL_LINES=$(@($log | Select-String -SimpleMatch "Test Run Successful.").Count)"; "RUN_FAILED_LINES=$(@($log | Select-String -SimpleMatch "Test Run Failed.").Count)"; foreach ($t in "Init_CreatesTokenSourceBeforeAnyLoaderObservesIt", "Init_InitializesCorrectly", "Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource") { "PASSED_$t=$(@($log | Select-String -Pattern ("^\s*Passed\s+" + $t + "\b")).Count) FAILED_$t=$(@($log | Select-String -Pattern ("^\s*Failed\s+" + $t + "\b")).Count)" }; "NOTNULL_MESSAGE_LINES=$(@($log | Select-String -SimpleMatch "not to be").Count)"'
EXIT_CODE: 0

## Output Summary

VSTEST_EXIT=0
Total tests LINES=1 VALUE=1
Passed LINES=1 VALUE=1
Failed LINES=0 VALUE=0
Skipped LINES=0 VALUE=0
RUN_SUCCESSFUL_LINES=1
RUN_FAILED_LINES=0
PASSED_Init_CreatesTokenSourceBeforeAnyLoaderObservesIt=1 FAILED_Init_CreatesTokenSourceBeforeAnyLoaderObservesIt=0
PASSED_Init_InitializesCorrectly=0 FAILED_Init_InitializesCorrectly=0
PASSED_Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource=0 FAILED_Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource=0
NOTNULL_MESSAGE_LINES=0

Console line: `Passed Init_CreatesTokenSourceBeforeAnyLoaderObservesIt [1 s]`, followed by `Test Run Successful.`

This run is the pass half of the fail-before/pass-after pair for the single regression test. It is the same command, the same assembly path and the same test-case filter as the fail-before run recorded in evidence/regression-testing/init-token-source-fail-before.md, and the only thing that changed between the two runs is the production file: the fail-before run was taken against the unfixed assembly built by [P1-T2], and this run against the fixed assembly built by [P2-T4]. All seven assertions now pass, and `NOTNULL_MESSAGE_LINES=0` confirms the not-null failure message that the fail-before run recorded is no longer emitted.

`PASSED_Init_InitializesCorrectly=0` and `PASSED_Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource=0` are expected here and are not regressions: the test-case filter admits one test by fully qualified name, so neither of those two tests ran in this scoped run. Both are observed in the assembly-wide run recorded by [P2-T6].

Raw disposition: the vstest console log lives under the gitignored coverage directory at the worktree root and is not committed.

## Command-transport adaptations, both semantics-preserving

1. `Set-Location -LiteralPath "REPO-ROOT";` prepended to both spans. Forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one.
2. In the summary span, the plan's `-replace ":", ""` was run as the single-operand `-replace ":"`, which replaces every match with the empty string and prints an identical label. The isolation probe for the forcing PowerShell parse defect is recorded in evidence/baseline/baseline-quickfiler-tests.md, and the same adaptation is applied identically wherever this plan cites CMD-TEST-SUMMARY.
