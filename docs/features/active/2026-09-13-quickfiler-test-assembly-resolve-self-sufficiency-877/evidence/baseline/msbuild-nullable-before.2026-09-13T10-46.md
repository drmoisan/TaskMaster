# Baseline: msbuild nullable / warnings-as-errors gate — issue #877

Timestamp: 2026-09-13T10-46
Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation"; $o = & msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true 2>&1; $ec = $LASTEXITCODE; Write-Host "EXIT_CODE=$ec"; $o | Where-Object { $_ -match "Warning\(s\)|Error\(s\)|Build succeeded|Build FAILED|Time Elapsed" } | Select-Object -Last 12'`
EXIT_CODE: 0
Output Summary: Observed exit code 0. A line whose trimmed text is exactly `0 Error(s)` WAS present. Summary block captured verbatim: `Build succeeded.` / `0 Warning(s)` / `0 Error(s)` / `Time Elapsed 00:00:16.62`. Run under an acquired build lock, released immediately after the command returned.

## Baseline colour and its consequence

The nullable baseline is GREEN at exit 0. Under [P2-T6] the corresponding build must therefore also be 0, and a line whose trimmed text is exactly `0 Error(s)` must be present. No `ExpectedExitCode:` row is declared here.

## Command form

The command is the character-for-character `CLAUDE.md` form. `/p:Nullable=enable` is deliberately absent: no project in this repository carries a `<Nullable>` element, so passing it would conscript every file that has never adopted the per-file `#nullable enable` pragma. `/t:Rebuild` is used rather than `/t:Build`, because MSBuild's up-to-date check does not invalidate on a command-line `/p:` change and a warm `/t:Build` would return a vacuous exit 0 with `CoreCompile` skipped.
