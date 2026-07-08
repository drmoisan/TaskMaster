Timestamp: 2026-07-06T15-24
Command: pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/remediation-csharp-coverage-rerun-all.2026-07-06T15-24.cobertura.xml
EXIT_CODE: 0

Output Summary:
- `-SearchRoot TaskMaster.Test` was retried first and failed before test execution with `The property 'Count' cannot be found on this object. Verify that the property exists.` because the script discovered a single test assembly.
- The successful rerun used `-SearchRoot .`, discovered 7 test assemblies, and ran the full test set under coverage.
- Tests: 4,972 total, 4,972 passed, 0 failed.
- `artifacts/csharp/coverage.xml`: exists and parses as Cobertura XML.
- Canonical remediation copy: `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/remediation-csharp-coverage.2026-07-06T12-29.cobertura.xml`.
- Baseline repository line coverage: 79.9234%.
- Final repository line coverage: 79.9920%.
- Final counters: 78,309 lines covered, 97,896 lines valid.
- Changed executable line coverage from prior issue #243 evidence: 100.0000%.
- Coverage policy threshold: 80.0000%.
- No-regression result versus baseline: PASS.
- Repository-wide threshold result: FAIL.
- Changed-code coverage result: PASS.

Result:
- FAIL. The required coverage artifact path exists, parses, and no longer regresses against the 79.9234% baseline, but repository-wide line coverage remains below the 80.0000% policy threshold by 0.0080 percentage points.
