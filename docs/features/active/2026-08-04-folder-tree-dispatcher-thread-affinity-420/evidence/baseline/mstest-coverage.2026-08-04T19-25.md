Timestamp: 2026-08-04T19-25
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/evidence/baseline/coverage-baseline.cobertura.xml
EXIT_CODE: 1
Output Summary: MSTest ran 6,075 tests: 6,074 passed and 1 failed. Cobertura coverage was emitted. Repository line coverage was 69.2280% (54,785 / 79,137) and branch coverage was 56.8900% (12,984 / 22,823). The initial repository coverage is below the 80% policy threshold.

The failing baseline test and below-threshold repository coverage predate this issue's implementation. The produced Cobertura artifact is retained as the baseline comparison input.
