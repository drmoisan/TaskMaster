# UtilitiesCS.Test suite after the fix — REGRESSION CHECK — issue #877

Timestamp: 2026-09-13T11-00
Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation"; $vstest = & (Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe") -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; Write-Host "VSTEST=$vstest"; & $vstest "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/ResultsDirectory:<scratch>/877/utilities-post" "/Logger:trx;LogFileName=utilities-post.trx" 2>&1 | Tee-Object -Variable out | Out-Null; Write-Host "EXIT_CODE=$LASTEXITCODE"; $out | Select-Object -Last 25'`
EXIT_CODE: 0
Output Summary: Read from `utilities-post.trx`: total 4926, passed 4926, failed 0, executed 4926. Console banner: `Test Run Successful.`, `Total tests: 4926`, `Passed: 4926`, total time 29.8676 seconds. The run completed on the FIRST attempt with exit code 0, so no re-run was performed and no `ExpectedExitCode:` row is declared. Resolved vstest executable: `<VS-install-root>\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`. Run under an acquired build lock, released immediately after the command returned.

## Purpose

This run proves that the resolver logic moved out of `UtilitiesCS.Test/TestAssemblyInitializer.cs` and into the shared `TestSupport/TestAssemblyResolver.cs` still behaves identically in its original home. `UtilitiesCS.Test` continues to install the same resolver from its own `[AssemblyInitialize]`, now by calling `global::TaskMaster.TestSupport.TestAssemblyResolver.Install()`, and the resolution semantics are unchanged.

## Known caveats that did not materialise on this run

- **Shell-icon stall.** The `ShellUtilities_Tests` and `ShellUtilitiesStatic_Tests` classes have previously stalled vstest on this machine through `SHGetFileInfo`, a behaviour that reproduces on `main`. This run did not stall, so the authorised `FullyQualifiedName!~ShellUtilities` filter substitution was NOT applied. The `Command:` row above records the filter actually used, `TestCategory!=LiveOutlook`, with no exclusion.
- **Intermittent failures tracked as issue #811.** That issue consolidates #780 (`TryAddValuesAsync_UpdatesExistingValue`), #803, #594 (`DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`) and two `Console.Out` races, all of which reproduce under parallel class workers on `main`. None of them failed on this run, so no pre-existing-failure attribution was required.

No `UtilitiesCS.Test` test file was modified to work around either caveat.

## Probative status

This is a regression check. Like the M2 suite run, it is not the discriminator for the fix; the discriminator is the M3 single-class run recorded at `evidence/regression-testing/m3-pass-after-summary.2026-09-13T10-58.md`.

## Evidence hygiene

The raw `utilities-post.trx` was written under the gitignored `TestResults/877/` scratch tree and is not committed; only this projection is. Absolute host paths are redacted.
