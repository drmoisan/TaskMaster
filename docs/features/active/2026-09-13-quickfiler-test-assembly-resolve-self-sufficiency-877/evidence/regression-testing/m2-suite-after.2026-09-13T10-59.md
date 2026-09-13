# M2 suite after the fix — REGRESSION CHECK ONLY — issue #877

Timestamp: 2026-09-13T10-59
Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation"; $vstest = & (Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe") -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; Write-Host "VSTEST=$vstest"; & $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/ResultsDirectory:<scratch>/877/m2-post" "/Logger:trx;LogFileName=m2-post.trx" 2>&1 | Tee-Object -Variable out | Out-Null; Write-Host "EXIT_CODE=$LASTEXITCODE"; $out | Select-Object -Last 20'`
EXIT_CODE: 0
Output Summary: Read from `m2-post.trx`: total 1395, passed 1395, failed 0, executed 1395. Console banner: `Test Run Successful.`, `Total tests: 1395`, `Passed: 1395`, total time 13.0146 seconds. The runsettings in force were `Workers=0` and `Scope=ClassLevel`, read from the unmodified `scripts/vscode/TaskMaster.cli.runsettings` at lines 4 to 7, where the `<Parallelize>` block declares `<Workers>0</Workers>` and `<Scope>ClassLevel</Scope>`. This is a regression check only and non-probative. Resolved vstest executable: `<VS-install-root>\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`. Run under an acquired build lock, released immediately after the command returned. No re-run was needed: the first attempt exited 0.

## Why this run proves nothing about the fix

This run shape is order-dependent. The M-matrix in `issue.md` records that this exact command shape was observed once at 1392 passed with 3 FAILED and once at 1395 passed with 0 failed, both selecting the identical total of 1395 tests. A suite run lets any earlier class install the resolver and rescue the bind invisibly, so a green result here is consistent both with the defect being fixed and with the defect still being present and merely masked by scheduling order. A red result here would likewise not have been evidence of a regression introduced by this change.

The discriminating evidence for this fix is the M3 single-class run with no runsettings, recorded at `evidence/regression-testing/m3-pass-after-summary.2026-09-13T10-58.md` and its three per-run artifacts.

## Parallelization configuration

`scripts/vscode/TaskMaster.cli.runsettings` was not modified. `/Settings:` was passed and not dropped or bypassed. `Workers` was not changed, the `<Parallelize>` block was not removed or weakened, and no `[DoNotParallelize]` attribute was introduced.

## Evidence hygiene

The raw `m2-post.trx` was written under the gitignored `TestResults/877/` scratch tree and is not committed; only this projection is. Absolute host paths are redacted.
