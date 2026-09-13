# Baseline: QuickFiler.Test under coverage — issue #839 — GREEN

Timestamp: 2026-09-13T05-35
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; . ./scripts/vscode/Invoke-MSTestWithCoverage.ps1; $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage | Out-Null; $derived = ConvertTo-DerivedCoverageSettingsXml -CanonicalSettingsXml (Get-Content -LiteralPath coverage.config -Raw -Encoding UTF8); Set-Content -LiteralPath coverage/839-effective-coverage.config -Value $derived -Encoding UTF8 -NoNewline; & dotnet-coverage collect --output coverage/839-baseline.cobertura.xml --output-format cobertura --settings coverage/839-effective-coverage.config -- $vstest QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation "/logger:console;verbosity=normal" "/TestCaseFilter:TestCategory!=LiveOutlook" | Tee-Object -FilePath coverage/839-baseline-tests.log; $code = $LASTEXITCODE; "COVERAGE_RUN_EXIT=$code"; "COBERTURA_EXISTS=$(Test-Path -LiteralPath coverage/839-baseline.cobertura.xml)"; exit $code'
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; $log = Get-Content -LiteralPath coverage/839-baseline-tests.log; foreach ($k in "Total tests:", "Passed:", "Failed:", "Skipped:") { $m = @($log | Select-String -Pattern ("^\s*" + [regex]::Escape($k) + "\s*(\d+)")); "$($k -replace ":") LINES=$($m.Count) VALUE=$(if ($m.Count -gt 0) { $m[-1].Matches[0].Groups[1].Value } else { 0 })" }; "RUN_SUCCESSFUL_LINES=$(@($log | Select-String -SimpleMatch "Test Run Successful.").Count)"; "RUN_FAILED_LINES=$(@($log | Select-String -SimpleMatch "Test Run Failed.").Count)"; foreach ($t in "Init_CreatesTokenSourceBeforeAnyLoaderObservesIt", "Init_InitializesCorrectly", "Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource") { "PASSED_$t=$(@($log | Select-String -Pattern ("^\s*Passed\s+" + $t + "\b")).Count) FAILED_$t=$(@($log | Select-String -Pattern ("^\s*Failed\s+" + $t + "\b")).Count)" }; "NOTNULL_MESSAGE_LINES=$(@($log | Select-String -SimpleMatch "not to be").Count)"'
EXIT_CODE: 0

## Output Summary

COVERAGE_RUN_EXIT=0
COBERTURA_EXISTS=True
Total tests LINES=1 VALUE=1393
Passed LINES=1 VALUE=1393
Failed LINES=0 VALUE=0
Skipped LINES=0 VALUE=0
RUN_SUCCESSFUL_LINES=1
RUN_FAILED_LINES=0
PASSED_Init_CreatesTokenSourceBeforeAnyLoaderObservesIt=0 FAILED_Init_CreatesTokenSourceBeforeAnyLoaderObservesIt=0
PASSED_Init_InitializesCorrectly=1 FAILED_Init_InitializesCorrectly=0
PASSED_Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource=1 FAILED_Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource=0
NOTNULL_MESSAGE_LINES=0

Counts under the [P0-T18] derivation rule: Total tests 1393 and Passed 1393 are read from their lines, whose `LINES=` values are 1. Failed is recorded as 0 and Skipped as 0 with the absence stated: neither line is present in the log (`LINES=0`) and `RUN_SUCCESSFUL_LINES=1`, which is the condition the rule requires before reading an absent line as 0. The console printed `Test Run Successful.` and `Total time: 14.5036 Seconds`.

BASELINE-TOTAL-TESTS: 1393. This is the figure the [P2-T6] and [P3-T4] acceptance conditions compare against; both require the post-change total to equal 1394.

## Two command-transport adaptations, both semantics-preserving

1. `Set-Location -LiteralPath "REPO-ROOT";` was prepended to both spans. Forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one; every path in both spans is worktree-relative and the plan assumes the current directory is the worktree root. The prefix supplies that current directory and changes no command semantics.
2. In the CMD-TEST-SUMMARY span, the plan's `-replace ":", ""` was run as the single-operand `-replace ":"`. Forced, not preferred: the two-operand form cannot be parsed by PowerShell in this position. Inside an expandable string, a `$()` subexpression containing the empty nested string `""` is read by the parser as an escaped quote rather than as an empty string literal, so the span terminates early and the command exits 1 having printed nothing. This was isolated by probe: `"$($k -replace "X", "Y") OK"` prints `PassedY OK`, while `"$($k -replace "X", "") OK"` exits 1 with no output, so the empty replacement string alone is the cause and neither the colon nor the Bash transport is. The single-operand `-replace ":"` replaces every match with the empty string by definition, so the printed label is identical. The same adaptation is applied identically wherever this plan cites CMD-TEST-SUMMARY, so the method is uniform across the baseline and every post-change run and no before-and-after comparison is skewed.

## Relationship to the superseded RED record this artifact replaces

This artifact overwrites, in place, a committed record of a RED baseline (Total 1393, Passed 1390, Failed 3) taken before the Decision D16 correction. That measurement is superseded, not amended, and the reason is recorded here because the earlier record's stated root cause was wrong in a way worth naming.

The three failures were `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`, `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` and `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`, all on `QfcInitEmailQueueZeroBatchTests`, each carrying a `TypeInitializationException` for `Deedle.Reflection` resolving to `FileNotFoundException` for netstandard 2.1.0.0. The superseded record attributed them to an unconditional packaging gap in QuickFiler.Test and concluded that AC5 was unsatisfiable.

That conclusion does not survive this measurement. The present run uses the same assembly, the same derived coverage settings, the same isolation switch and the same test-case filter, and differs from the superseded run in exactly one respect: per Decision D16 it does not pass the CLI runsettings file whose entire content is an MSTest class-level parallelisation element. Under that one change the three tests pass and the assembly is green at 1393 of 1393 with exit 0.

The superseded record's isolating diagnostic removed `dotnet-coverage` and still saw the three failures, and concluded from that that instrumentation was not the cause. That inference was sound as far as it went, but it did not isolate the actual cause, because the runsettings file was present in that diagnostic too. Both the instrumented and the uninstrumented run in that round carried class-level parallelism, so the variable that this run changed was never varied. The failure is therefore a defect of the run configuration, reproducible under parallelism with or without instrumentation, and not a property of the tree under test.

Whether the parallelisation element should be removed at source, so the repository's own runner stops diverging from the CI workflow (which passes no settings file to vstest at all), is out of scope for this item and is reported to the caller as a candidate follow-up.

Raw disposition: the Cobertura document, the derived coverage settings file and the vstest console log live under the gitignored coverage directory at the worktree root and are not committed. Only this projection is committed.
