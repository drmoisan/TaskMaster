# Final MSTest Coverage

Timestamp: 2026-07-21T20-38Z
Run Identity: `final-pass-2026-07-21T20-38Z`
Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final.2026-07-21T20-38.cobertura.xml`
EXIT_CODE: 0
Output Summary: The canonical repository wrapper discovered all eight first-party test assemblies, passed 5,842 of 5,842 tests with zero failures or skips, and produced post-processed Cobertura coverage of 89,234/106,048 lines, or 84.1449%.

## Test Results

- Discovered test assemblies: 8
- Total tests: 5,842
- Passed: 5,842
- Failed: 0
- Skipped: 0
- Test time: 52.6412 seconds

## Numeric Coverage

- Lines covered: 89,234
- Lines valid: 106,048
- Repository line rate: 0.841449
- Repository line coverage: 84.1449%
- First-party production packages: 9
- Test packages in final Cobertura: 0

Artifact:

- Path: `evidence/qa-gates/coverage-final.2026-07-21T20-38.cobertura.xml`
- Bytes: 9,932,009
- SHA-256: `bbc488a75fa2d475622993cbd4ef98368c85f6fafd7ee254f2ac1858c889b684`

## Instrumentation Stability Protocol

Prior repository evidence records profiler/test-host instability when test binaries themselves are instrumented. Before P5-T1, `<ModulePath>.*\.Test\.dll$</ModulePath>` was added temporarily to the instrumentation exclusions. All eight test assemblies were still discovered and executed; only test-code instrumentation was excluded, consistent with repository coverage policy. After the successful wrapper returned, `coverage.config` was restored immediately:

- Working blob: `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`
- Tracked HEAD blob: `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`
- `git diff --exit-code -- coverage.config`: 0

The 20:20 crash artifact and superseded 20:25 clean-pass artifact are not cited as final coverage evidence.

P5-T4 result: PASS.
