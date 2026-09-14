# Phase 1 compile gate: msbuild analyzer build after the five-file change — issue #877

Timestamp: 2026-09-13T10-50
Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation"; $o = & msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true 2>&1; $ec = $LASTEXITCODE; Write-Host "EXIT_CODE=$ec"; Write-Host "--- anchored summary ---"; $o | Where-Object { $_.ToString().Trim() -match "^(Build succeeded\.|Build FAILED\.|[0-9]+ Warning\(s\)|[0-9]+ Error\(s\))$" }; Write-Host "--- test assembly outputs ---"; $o | Where-Object { $_.ToString() -match "^\s+(QuickFiler\.Test|UtilitiesCS\.Test) -> " }; Write-Host "--- CS8370 count ---"; ($o | Where-Object { $_.ToString() -match "CS8370" }).Count'`
EXIT_CODE: 0
Output Summary: Observed exit code 0, equal to the [P0-T11] baseline of 0. Anchored summary lines captured: `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`. A line whose trimmed text is exactly `0 Error(s)` WAS present. `QuickFiler.Test` compiled: the build emitted `QuickFiler.Test -> <repo-root>\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`. `UtilitiesCS.Test` also compiled, emitting `UtilitiesCS.Test -> <repo-root>\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`. CS8370 occurrence count in the captured output: 0. Run under an acquired build lock, released immediately after the command returned.

## Language-version evidence

`QuickFiler.Test.csproj` declares no `<LangVersion>` element and targets `v4.8.1`, so it compiles at the C# 7.3 default. The successful compilation of `QuickFiler.Test` with zero CS8370 diagnostics is the evidence that the shared file's explicit null-test spelling, written in place of the C# 8 `??=` null-coalescing assignment, is accepted at that default language version. No `<LangVersion>` element was added to any project file.

## Baseline comparison

- [P0-T11] baseline exit code: 0.
- This run exit code: 0. Equal to baseline, and a trimmed-exact `0 Error(s)` line is present, which is what a green baseline requires.

## Assertion method

No assertion is made on any count of the bare substring `error`. The first attempt at this task used a broader `Where-Object` filter that matched the `csc.exe` command line emitted at diagnostic verbosity; that filter displaced the summary block from the tail of the captured output, so the command was re-run with a filter anchored to the whole trimmed line. Both runs returned exit code 0. The anchored form is the one recorded above.
