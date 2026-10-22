# QA Gate — Final uninterrupted toolchain pass attestation (P7-T7 re-run)

Timestamp: 2026-08-27T23-31

Command: the four steps listed below, in this order

EXIT_CODE: 0 (all four)

Output Summary: all four toolchain steps passed in one uninterrupted final pass on the merged tree.
No file changed after the formatting step of that pass.

| # | Step | Command | EXIT_CODE |
| --- | --- | --- | --- |
| 1 | Format | `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` | 0 |
| 2 | Analyze | `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 |
| 3 | Type-check | `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 |
| 4 | Test | `pwsh -NoProfile -File ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/postchange.cobertura.xml` | 0 |

## Restart count

**Restarts in this resumed run: 1.**

The loop was restarted from step 1 once, after the `AddItemsCore` seam and its test were added to
remediate the P7-T6 coverage shortfall. That is the only restart. The pass tabulated above is the one
that followed it.

Separately, step 4 of that final pass was executed twice on a byte-identical tree because of an
authorized `PumpTimeoutMs` environmental expiry in sibling-owned pump-host tests. That re-run is
permitted by the Phase 7 preamble without restarting the loop, and is documented in
`post-merge-test-coverage.2026-08-27T23-31.md`. No file changed between the two executions, so the pass remains
uninterrupted in the sense the policy requires.
