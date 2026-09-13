# Final QA step 4 of 4 — tests with coverage — issue #839

Timestamp: 2026-09-13T06-13
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; . ./scripts/vscode/Invoke-MSTestWithCoverage.ps1; $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage | Out-Null; $derived = ConvertTo-DerivedCoverageSettingsXml -CanonicalSettingsXml (Get-Content -LiteralPath coverage.config -Raw -Encoding UTF8); Set-Content -LiteralPath coverage/839-effective-coverage.config -Value $derived -Encoding UTF8 -NoNewline; & dotnet-coverage collect --output coverage/839-final.cobertura.xml --output-format cobertura --settings coverage/839-effective-coverage.config -- $vstest QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation "/logger:console;verbosity=normal" "/TestCaseFilter:TestCategory!=LiveOutlook" | Tee-Object -FilePath coverage/839-final-tests.log; $code = $LASTEXITCODE; "COVERAGE_RUN_EXIT=$code"; "COBERTURA_EXISTS=$(Test-Path -LiteralPath coverage/839-final.cobertura.xml)"; exit $code'
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; $log = Get-Content -LiteralPath coverage/839-final-tests.log; foreach ($k in "Total tests:", "Passed:", "Failed:", "Skipped:") { $m = @($log | Select-String -Pattern ("^\s*" + [regex]::Escape($k) + "\s*(\d+)")); "$($k -replace ":") LINES=$($m.Count) VALUE=$(if ($m.Count -gt 0) { $m[-1].Matches[0].Groups[1].Value } else { 0 })" }; "RUN_SUCCESSFUL_LINES=$(@($log | Select-String -SimpleMatch "Test Run Successful.").Count)"; "RUN_FAILED_LINES=$(@($log | Select-String -SimpleMatch "Test Run Failed.").Count)"; foreach ($t in "Init_CreatesTokenSourceBeforeAnyLoaderObservesIt", "Init_InitializesCorrectly", "Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource") { "PASSED_$t=$(@($log | Select-String -Pattern ("^\s*Passed\s+" + $t + "\b")).Count) FAILED_$t=$(@($log | Select-String -Pattern ("^\s*Failed\s+" + $t + "\b")).Count)" }; "NOTNULL_MESSAGE_LINES=$(@($log | Select-String -SimpleMatch "not to be").Count)"'
EXIT_CODE: 0

## Output Summary

COVERAGE_RUN_EXIT=0
COBERTURA_EXISTS=True
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

Counts under the [P0-T18] derivation rule: Total tests 1394 and Passed 1394 are read from their lines, whose `LINES=` values are 1. Failed and Skipped are recorded as 0 with the absence stated, both lines being absent while `RUN_SUCCESSFUL_LINES=1`.

Population arithmetic: 1394 equals the [P0-T18] baseline total of 1393 plus exactly one, the one test this item adds.

## The exit code is not the only evidence this run succeeded

The plan's span ends its native invocation in a pipeline into `Tee-Object` and then reads `$LASTEXITCODE`. That read is sound here because `Tee-Object` is a cmdlet and does not set `$LASTEXITCODE`, so the value carried forward is the exit code of `dotnet-coverage` itself rather than a cmdlet result. The gate does not rest on that reasoning alone: `RUN_SUCCESSFUL_LINES=1` and the zero Failed count are read from the console log by the second command and are independent of the exit code, and the coverage document is separately parsed and asserted over by [P3-T5] rather than merely checked for existence.

This run is also not exposed to the known false-pass mechanism in the repository's coverage runner. That script throws at its line 236 on a non-zero child exit, and line 236 precedes the Cobertura post-processing call at line 341, so under a failure the document would be left raw while `Test-Path` still succeeded. The script is dot-sourced here for one function only, `ConvertTo-DerivedCoverageSettingsXml`, and its entry point is never invoked: its guard at line 349, `if ($MyInvocation.InvocationName -ne '.')`, is false under a dot-source, so `Invoke-MSTestWithCoverageMain` does not run and line 236 is never reached. `dotnet-coverage` is invoked directly. The document this plan reads is therefore raw and un-post-processed by design, which is why its `filename` attributes carry absolute host paths and its package count before filtering is large. That is the expected input for CMD-COVERAGE-PARSE, not a symptom.

Raw disposition: the Cobertura document, the derived coverage settings file and the vstest console log live under the gitignored coverage directory at the worktree root and are not committed.

## Two command-transport adaptations, both semantics-preserving

1. `Set-Location -LiteralPath "REPO-ROOT";` prepended to both spans. Forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one.
2. In the summary span, the plan's `-replace ":", ""` was run as the single-operand `-replace ":"`. The isolation probe for the forcing PowerShell parse defect is recorded in evidence/baseline/baseline-quickfiler-tests.md, and the same adaptation is applied identically wherever this plan cites CMD-TEST-SUMMARY, including the baseline this run is compared against.
