Timestamp: 2026-08-31T10:59:40-04:00
Command: pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\p7-t5-postchange.cobertura.xml
EXIT_CODE: 0
COVERAGE_FLOOR_THROW: no
Output Summary: Discovered 9 test assemblies, matching the baseline. Test Run Successful: Total tests 6894; Passed 6894; Failed 0. The baseline failure set is empty, so no baseline failure remains. Coverage attributes: line-rate=0.853327; branch-rate=0.793089; lines-covered=54822; lines-valid=64245; branches-covered=13059; branches-valid=16466. Derived line coverage: 85.3327%. Derived branch coverage: 79.3089%.

Baseline comparison:
- Discovered assemblies: baseline 9; post-change 9.
- Baseline failure set: empty.
- Post-change failures: 0.
- Baseline failures still failing: none.

Coverage headline read-only substep:
- line-rate=0.853327
- branch-rate=0.793089
- lines-covered=54822
- lines-valid=64245
- branches-covered=13059
- branches-valid=16466
- Derived line coverage: 85.3327%
- Derived branch coverage: 79.3089%
