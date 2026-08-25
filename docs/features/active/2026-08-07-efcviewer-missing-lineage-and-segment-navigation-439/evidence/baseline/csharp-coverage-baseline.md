Timestamp: 2026-08-24T18:14:02.0000000-04:00
Command: pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/baseline/issue-439-baseline.cobertura.xml
EXIT_CODE: 1
Output Summary: Coverage baseline was not collected because no Debug test assemblies were found. The coverage XML was not created.
Diagnostic: No test assemblies found under `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-24T17-27-23\.` for configuration `Debug`. Build first.
REMEDIATION_REQUIRED: Restore the repository's required packages and produce Debug test assemblies before rerunning P0-T5.

---
Timestamp: 2026-08-24T18:20:55-04:00
Command: pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/baseline/issue-439-baseline.cobertura.xml
EXIT_CODE: 0
Output Summary: Coverage baseline retry discovered nine Debug test assemblies and produced the requested Cobertura XML. Repository line coverage: 85.58% (line-rate 0.855756); source classes: 546.
