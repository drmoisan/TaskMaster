# Issue #608 R1 MSTest and coverage gate

Timestamp: 2026-08-25T12-54
Command: `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput "docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/qa-gates/r1-csharp-coverage.2026-08-25T12-33.cobertura.xml"`
EXIT_CODE: 1
Output Summary: MSTest with coverage executed and persisted the named Cobertura report, but failed: 6,475 passed and 1 failed of 6,476 total tests. The report records 57,239 covered of 81,570 valid lines, a 70.1716% repository line rate. This does not meet the passing acceptance condition and is remediation-required.

Observed result:

```text
Test Run Failed. Total tests: 6476
Passed: 6475
Failed: 1
Code coverage results: .../r1-csharp-coverage.2026-08-25T12-33.cobertura.xml
MSTest with coverage failed with exit code 1
```

Cobertura verification: the named file exists with `line-rate="0.7017163172735074"`, `lines-covered="57239"`, and `lines-valid="81570"`.

Gate Decision: REMEDIATION_REQUIRED. No hook failure occurred; `hook-failures.md` was not changed.
