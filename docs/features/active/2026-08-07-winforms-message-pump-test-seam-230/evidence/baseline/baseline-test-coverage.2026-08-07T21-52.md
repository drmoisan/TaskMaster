# P0-T6 — Full-Suite Test and Coverage Baseline

Issue: #230
Task: [P0-T6]

- Timestamp: 2026-08-07T21-52
- Command: `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/baseline/coverage-baseline.cobertura.xml`
- EXIT_CODE: 0
- Output Summary: **Total tests: 6272 — Passed: 6272, Failed: 0, Skipped: 0.**
  9 test assemblies discovered. Run completed with no hang and no manual
  intervention. Cobertura root `<coverage>` element reports
  **`line-rate="0.856453"` (85.6453%)** and **`branch-rate="0.790039"`
  (79.0039%)**, with `lines-covered="94937"`, `lines-valid="110849"`,
  `branches-covered="22001"`, `branches-valid="27848"`.

## Baseline coverage figures (authoritative for the D5 gate (a) comparison)

| Metric | Value |
|---|---|
| Repo-wide line-rate | 0.856453 (85.6453%) |
| Repo-wide branch-rate | 0.790039 (79.0039%) |
| lines-covered | 94937 |
| lines-valid (denominator) | 110849 |
| branches-covered | 22001 |
| branches-valid (denominator) | 27848 |
| `QuickFiler` package line-rate | 0.8001906577693041 (80.019%) |
| `QuickFiler` package branch-rate | 0.7371154614462645 (73.712%) |

## Artifact

- Cobertura XML: `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/baseline/coverage-baseline.cobertura.xml`
  (10,398,171 bytes; exists at the stated path).

## Run configuration

- Harness: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which resolves
  `vstest.console.exe` via `vswhere` and wraps it in `dotnet-coverage collect`.
- Resolved vstest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
- Inner vstest arguments include `/Settings:scripts\vscode\TaskMaster.cli.runsettings`,
  `/InIsolation` (mandatory for the Moq assemblies per D6), and
  `/TestCaseFilter:TestCategory!=LiveOutlook`.
- Instrumentation excludes come from repo-root `coverage.config` plus the derived
  `.*\.Test\.dll$` module exclusion the script injects, so test assemblies are not
  in the denominator.

## Notes for the Phase 8 comparison (D5)

Removing 8 `[ExcludeFromCodeCoverage]` attributes moves previously-uninstrumented
members into the denominator. The post-change raw `line-rate` must therefore be
reported **both raw and denominator-adjusted** (P8-T6); a raw-only comparison is
not denominator-stable and must not be read as a regression.
