# Baseline — MSTest with Coverage (Issue #207, increment 2)

Timestamp: 2026-06-19T23-35

Command:
- `dotnet-coverage collect --output coverage/baseline.cobertura.xml --output-format cobertura --settings coverage.config -- <vstest.console.exe> UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation`
- (repo-standard coverage mechanism: dotnet-coverage wrapping vstest.console.exe with /InIsolation and coverage.config instrumentation excludes, producing Cobertura XML — equivalent to the policy `vstest.console.exe /EnableCodeCoverage` step.)

EXIT_CODE: 0

Output Summary:
- Tests: 3915 passed, 0 failed (Total tests: 3915; Test Run Successful).
- Repository-wide line coverage (raw Cobertura `coverage/@line-rate`): 71.64% (lines-covered 87269 / lines-valid 121820). Raw figure includes vendored Swordfish/SVGControl packages.
- Repository-wide first-party line coverage (excluding vendored Swordfish/SVGControl, per #197 denominator method, deduped per-line): 72.72% (covered 85066 / total 116976).
- Targeted module — UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs line coverage: 89.04% (130 covered / 146 total).
- Measurement scope note: this baseline instruments the UtilitiesCS.Test assembly only (per plan P0-T7 scope), so packages exercised by other test assemblies (e.g. TaskMaster.Test, QuickFiler.Test) appear uncovered in this single-assembly run and deflate the repo-wide figure. The post-change run (P2-T4) uses the identical single-assembly basis so the baseline-vs-post comparison is apples-to-apples.
- Coverage artifact: coverage/baseline.cobertura.xml
