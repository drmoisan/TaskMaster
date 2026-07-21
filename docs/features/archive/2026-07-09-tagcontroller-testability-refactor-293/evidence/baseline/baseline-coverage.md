# Baseline — Test Coverage (P0-T10)

Timestamp: 2026-07-09T22-01

Command: `vstest.console.exe Tags.Test/bin/Debug/Tags.Test.dll /EnableCodeCoverage /Settings:docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/evidence/qa-gates/tags-coverage.runsettings`
(VS18 vstest.console 18.7.0; Cobertura runsettings scoped to `Tags.dll`, honoring `[ExcludeFromCodeCoverage]`)
EXIT_CODE: 0

Raw coverage XML copied to: `artifacts/csharp/coverage.xml`

Output Summary:
- Total tests: 13, Passed: 13, Failed: 0.
- Baseline `Tags` project (Tags.dll) line coverage: **67.28%** (516 / 767 lines).
- Per-class baseline: `Tags.TagController` 56.7%, `Tags.PrefixItem` 68.2%, `Tags.TagViewer` 100%.
- `Tags.TagLauncher` and `Tags.CheckBoxController` are `[ExcludeFromCodeCoverage]` at baseline
  and therefore excluded from the denominator (they will re-enter partially after the refactor
  narrows their exemptions).
- Baseline is below the 80% project target; the refactor adds `TagSelectionModel`,
  `LauncherAutoAssign`, and expanded controller/decision tests to reach >= 80%.
