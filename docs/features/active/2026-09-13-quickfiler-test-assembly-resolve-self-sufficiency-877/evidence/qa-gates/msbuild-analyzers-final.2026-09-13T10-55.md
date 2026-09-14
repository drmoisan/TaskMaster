# Final QC step 3: msbuild analyzer gate — issue #877

Timestamp: 2026-09-13T10-55
Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation"; $o = & msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true 2>&1; $ec = $LASTEXITCODE; Write-Host "EXIT_CODE=$ec"; Write-Host "--- anchored summary ---"; $o | Where-Object { $_.ToString().Trim() -match "^(Build succeeded\.|Build FAILED\.|[0-9]+ Warning\(s\)|[0-9]+ Error\(s\))$" }; Write-Host "--- write-set diagnostics ---"; ($o | Where-Object { $_.ToString() -match "(warning|error) [A-Z]+[0-9]+" }).Count'`
EXIT_CODE: 0
Output Summary: Observed exit code 0, equal to the [P0-T11] baseline of 0. Anchored summary lines captured: `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`. A line whose trimmed text is exactly `0 Error(s)` WAS present. Count of output lines matching a compiler or analyzer diagnostic identifier pattern: 0, so no diagnostic names any write-set path or any other path. Run under an acquired build lock, released immediately after the command returned.

## Baseline comparison

- [P0-T11] baseline exit code: 0.
- This run exit code: 0. The baseline was green, so the required condition is exit code 0 together with a trimmed-exact `0 Error(s)` line. Both hold.

## Assertion method

No assertion is made on any count of the bare substring `error`. A successful msbuild run in this repository prints that substring many times in package paths, target names and `ErrorText` properties. The assertion is on the exit code together with a match anchored to the whole trimmed line.
