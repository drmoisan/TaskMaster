Timestamp: 2026-07-12T15-57
Command: vstest.console.exe TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll Tags.Test\bin\Debug\Tags.Test.dll /Settings:docs\features\active\2026-07-12-people-tag-window-autotag-322\evidence\baseline\coverage-322.runsettings /EnableCodeCoverage /InIsolation
EXIT_CODE: 0
Output Summary: `Total tests: 225`, `Passed: 225`, `Failed: 0`. Total time ~4.1s. Cobertura coverage
output archived at `evidence/baseline/baseline-coverage.cobertura.xml`.

Numeric baseline line-coverage percentages (Cobertura `<package>` `line-rate`, production-only,
`*.Test.dll` modules excluded via runsettings):
- `TaskVisualization.dll`: 89.72% (line-rate 0.897178683...)
- `Tags.dll`: 92.63% (line-rate 0.926315789...)
- Combined (both packages, overall `<coverage>` element): 90.66% (2135/2355 lines covered)

Runsettings used: `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/baseline/coverage-322.runsettings`
(Cobertura format, `TaskVisualization.dll`/`Tags.dll` module include, `*.Test.dll` exclude,
`[ExcludeFromCodeCoverage]`/`DebuggerHidden`/`DebuggerNonUserCode`/`GeneratedCode` attribute
excludes, MSTest 4-worker class-level parallelism per repo determinism precedent).
