# Post-Merge Toolchain Pass: Re-Run of the Phase 4 Final QC Gates

Timestamp: 2026-09-29T20-01
HEAD: 55a50e9226173d39d3c169b0dc90400b430af59b (merge commit 55a50e922, origin/main merged into bug/tests-depend-on-uncontrolled-environment-931)
Command: reconciliation of the six post-merge step artifacts listed below. Each step reuses the command, runsettings file, isolation switch and test-case filter recorded in the pre-merge Phase 4 evidence.
EXIT_CODE: 0

Output Summary:

Steps, in CLAUDE.md toolchain order:
- 0 Bootstrap: dotnet tool restore, then pwsh -NoProfile -File scripts\vscode\Invoke-Restore.ps1. Exit 0 and 0. packages\MSTest.Analyzers.4.4.1 and packages\Meziantou.Analyzer.3.0.290 both exist. Evidence: post-merge-bootstrap.2026-09-29T19-53.md
- 1 Format: dotnet tool run csharpier check . Exit 0, printed "Checked 1625 files in 8418ms.", no file reported, so no format run and no restart. Evidence: post-merge-csharpier-check.2026-09-29T19-53.md
- 2 Analyze: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (plus /nodeReuse:false and a file logger). Exit 0, 0 errors, 0 warnings, SKIP_CORECOMPILE_LINES 0. Evidence: post-merge-msbuild-analyzers.2026-09-29T19-53.md
- 3 Type-check: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (plus /nodeReuse:false and a file logger). Exit 0, 0 errors, 0 warnings, SKIP_CORECOMPILE_LINES 0. Evidence: post-merge-msbuild-nullable.2026-09-29T19-54.md
- 4a QuickFiler.Test full assembly under TaskMaster.runsettings with /InIsolation and no filter. Exit 0, total 1469, failed 0. The four required names are executed and Passed. Evidence: post-merge-parallel-suite-quickfiler-test.md
- 4b UtilitiesCS.Test full assembly under TaskMaster.runsettings with /InIsolation and the P0-T9 shell-icon filter only. Exit 0, total 4924, failed 0. All eight FileInfoWrapper_Tests methods are executed and Passed. Evidence: post-merge-parallel-suite-utilitiescs-test.md
- 5 Coverage: COVERAGE-ROUTE DIRECT, which dot-sources scripts\vscode\Invoke-MSTestWithCoverage.ps1 and applies the P0-T9 filter; the substitution is recorded in the artifact. Exit 0 in both measurements, total 7323, failed 0, LINE-FLOOR MET and BRANCH-FLOOR MET. First-party lines are 56079/65736 and 56080/65736 (85.31%); branches are 13597/17054 (79.73%). Evidence: post-merge-coverage-final.md

Coverage comparison against evidence/baseline/coverage-baseline.md (MEASUREMENT 2):

| Package | Counter | Baseline | Post-merge | Not lower |
| --- | --- | --- | --- | --- |
| UtilitiesCS | LINE | 38816/43424 (0.893884) | 38811/43423 (0.893789) | False |
| UtilitiesCS | BRANCH | 9411/11269 (0.835123) | 9413/11271 (0.835152) | True |
| QuickFiler | LINE | 10461/12754 (0.820213) | 10461/12754 (0.820213) | True |
| QuickFiler | BRANCH | 2518/3217 (0.782717) | 2518/3217 (0.782717) | True |

MEASUREMENT 1 read QuickFiler LINE as 10460/12754; post-merge-coverage-final.md attributes this to the same one-line collector variance recorded in coverage-final.md.

FINDING: the UtilitiesCS LINE rate is lower than the baseline in both measurements. Per-file analysis (post-merge-coverage-final.md) splits the drop into two parts:
- Reproducible: the production edits that origin/main brought in, ILGlobals.cs (-2 lines, -2 covered) and UiThread.cs (+1 line, +1 covered). Together they give 38815/43423 (0.893881).
- Variance: 4 covered lines missing in a different unchanged file on each run.
No part of the drop comes from this item's test-only changes. Nothing was fixed.

Tree state: git status --porcelain for the feature folder was empty before these artifacts were written. No .cs, .csproj, plan checkbox or spec acceptance-criteria line was edited. No .trx, .xml or .coverage file was staged.

LOOP: CLEAN PASS on gates 1 to 4 (no step failed and no step changed a file). The coverage step completed with exit 0; its package comparison carries the UtilitiesCS LINE finding above.
