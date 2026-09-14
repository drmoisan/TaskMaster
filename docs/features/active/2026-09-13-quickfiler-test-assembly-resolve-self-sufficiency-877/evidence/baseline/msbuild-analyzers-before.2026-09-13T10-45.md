# Baseline: msbuild analyzer gate — issue #877

Timestamp: 2026-09-13T10-45
Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation"; $o = & msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true 2>&1; $ec = $LASTEXITCODE; Write-Host "EXIT_CODE=$ec"; $o | Where-Object { $_ -match "Warning\(s\)|Error\(s\)|Build succeeded|Build FAILED|Time Elapsed" } | Select-Object -Last 12'`
EXIT_CODE: 0
Head: 21450791681f2af6f1124e067301602ab7978984
Output Summary: Observed exit code 0. Warnings reported on the msbuild summary line: 0. A line whose trimmed text is exactly `0 Error(s)` WAS present. Summary block captured verbatim: `Build succeeded.` / `0 Warning(s)` / `0 Error(s)` / `Time Elapsed 00:00:18.35`. Run under an acquired build lock, released immediately after the command returned.

## Baseline colour and its consequence

The analyzer baseline is GREEN at exit 0. Under [P1-T8] and [P2-T5] the corresponding builds must therefore also be 0, and a line whose trimmed text is exactly `0 Error(s)` must be present in each. No `ExpectedExitCode:` row is declared here.

## Assertion method

Per the plan's msbuild assertion rule, no assertion is made on any count of the bare substring `error`. A successful msbuild run in this repository prints that substring many times in package paths, target names and `ErrorText` properties. The assertion is on the exit code together with an anchored match for a line whose trimmed text is exactly `0 Error(s)`, which excludes a substring match against `10 Error(s)`.

The `Head:` row above is informational only. No later task in this plan is gated on that SHA value.
