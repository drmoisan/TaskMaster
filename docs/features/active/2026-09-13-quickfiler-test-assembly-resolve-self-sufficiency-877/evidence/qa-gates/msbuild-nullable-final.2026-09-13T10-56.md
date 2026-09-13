# Final QC step 4: msbuild nullable / warnings-as-errors gate — issue #877

Timestamp: 2026-09-13T10-56
Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation"; $o = & msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true 2>&1; $ec = $LASTEXITCODE; Write-Host "EXIT_CODE=$ec"; Write-Host "--- anchored summary ---"; $o | Where-Object { $_.ToString().Trim() -match "^(Build succeeded\.|Build FAILED\.|[0-9]+ Warning\(s\)|[0-9]+ Error\(s\))$" }; Write-Host "--- diagnostics count ---"; ($o | Where-Object { $_.ToString() -match "(warning|error) [A-Z]+[0-9]+" }).Count; Write-Host "QF_DLL=$(Test-Path -LiteralPath ''QuickFiler.Test\bin\Debug\QuickFiler.Test.dll'')"; Write-Host "UT_DLL=$(Test-Path -LiteralPath ''UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll'')"'`
EXIT_CODE: 0
Output Summary: Observed exit code 0, equal to the [P0-T12] baseline of 0. Anchored summary lines captured: `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`. A line whose trimmed text is exactly `0 Error(s)` WAS present. Count of output lines matching a compiler or analyzer diagnostic identifier pattern: 0. Test assembly existence after this build: `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` exists (True) and `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` exists (True), so both assemblies the Phase 2 vstest spans load were produced and execution proceeds to [P2-T7]. Run under an acquired build lock, released immediately after the command returned.

## Baseline comparison

- [P0-T12] baseline exit code: 0.
- This run exit code: 0. The baseline was green, so the required condition is exit code 0 together with a trimmed-exact `0 Error(s)` line. Both hold.

## Nullable scope note

Nullable enforcement in this repository is per-file opt-in through the `#nullable enable` pragma, and `/p:TreatWarningsAsErrors=true` promotes the `CS86xx` diagnostics of participating files to build errors. The new shared file deliberately carries no `#nullable` directive, matching the source it was lifted from, because the resolver method returns `null` on three paths. `/p:Nullable=enable` was not passed, in line with the `CLAUDE.md` command form.
