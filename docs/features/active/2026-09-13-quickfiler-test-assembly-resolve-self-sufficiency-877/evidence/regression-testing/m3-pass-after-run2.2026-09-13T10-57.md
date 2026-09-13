# M3 pass-after, run 2 of 3: QfcInitEmailQueueZeroBatchTests alone, no runsettings — issue #877

Timestamp: 2026-09-13T10-57
Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation"; $vstest = & (Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe") -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; Write-Host "VSTEST=$vstest"; & $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" "/TestCaseFilter:FullyQualifiedName~QfcInitEmailQueueZeroBatchTests" /InIsolation "/ResultsDirectory:<scratch>/877/m3-post-2" "/Logger:trx;LogFileName=m3-post-2.trx" 2>&1 | Tee-Object -Variable out | Out-Null; Write-Host "EXIT_CODE=$LASTEXITCODE"; $out | Select-Object -Last 18'`
EXIT_CODE: 0
Output Summary: The captured console output contains the token `Test Run Successful.` Read from `m3-post-2.trx`: total 3, passed 3, failed 0, executed 3. Per-test outcomes read from the same file: `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` Passed; `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` Passed; `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` Passed. Console total time 1.7168 seconds, VSTest version 18.10.0 (x64). Resolved vstest executable: `<VS-install-root>\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`, located through vswhere with `-latest -products * -find`. Run under an acquired build lock, released immediately after the command returned.

## Run shape

Identical to run 1 except for the `/ResultsDirectory:` leaf `m3-post-2` and the `LogFileName=m3-post-2.trx` value. No `/Settings:` file is passed and none was added.

## Evidence hygiene

The raw `m3-post-2.trx` was written under the gitignored `TestResults/877/` scratch tree and is not committed; only this projection is. Absolute host paths are redacted.
