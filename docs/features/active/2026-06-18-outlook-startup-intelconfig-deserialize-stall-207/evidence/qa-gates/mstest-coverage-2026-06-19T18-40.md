# Final QC — MSTest with Coverage (Issue #207, increment 2)

Timestamp: 2026-06-19T23-35

Command:
- `dotnet-coverage collect --output coverage/postchange.cobertura.xml --output-format cobertura --settings coverage.config -- <vstest.console.exe> UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation`

EXIT_CODE: 0

Output Summary:
- Tests: 3916 passed, 0 failed (Test Run Successful). Baseline was 3915; one net additional test method (the new read-versus-deserialize split test; the AC3 behavior-preservation test was extended in place).
- New/extended tests all pass:
  - ReadConfigurationAsync_RecordsReadSeparatelyFromDeserialize_SplitIsVisible [Passed]
  - ReadConfigurationAsync_IsBehaviorPreserving_ConfigKeysMatchNonNullFixtures [Passed] (extended with Config.Count pin)
  - ReadConfigurationAsync_WithFixtureResources_ProducesBreakdownRowPerEntry [Passed] (increment-1 test, still green)
- Repository-wide line coverage (raw Cobertura @line-rate): 71.65% (lines-covered 87329 / lines-valid 121880).
- Repository-wide first-party line coverage (excluding vendored Swordfish/SVGControl, #197 denominator): 72.73% (covered 85126 / total 117036).
- Targeted module — UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs: 90.12% (146 covered / 162 total).
- New/changed executable lines in IntelligenceConfig.cs (read Stopwatch block lines 91-95; FormatResourceTimingBreakdown call line 148; readLine + deserializeTable + return lines 205, 222, 227): 9/9 covered = 100%.
- Measurement scope: identical single-assembly (UtilitiesCS.Test) basis as the P0-T7 baseline, so the comparison is apples-to-apples.
- Coverage artifact: coverage/postchange.cobertura.xml
