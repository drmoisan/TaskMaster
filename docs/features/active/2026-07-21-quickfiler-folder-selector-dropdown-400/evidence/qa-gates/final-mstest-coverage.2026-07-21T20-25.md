# Final MSTest Coverage

Timestamp: 2026-07-21T20-25Z
Run Identity: `final-pass-2026-07-21T20-25Z`
Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final.2026-07-21T20-25.cobertura.xml`
EXIT_CODE: 0
Output Summary: The canonical repository wrapper discovered all eight first-party test assemblies, passed 5,841 of 5,841 tests with zero failures or skips, and produced post-processed Cobertura coverage of 89,224/106,048 lines, or 84.1355%.

## Test Results

- Discovered test assemblies: 8
- Total tests: 5,841
- Passed: 5,841
- Failed: 0
- Skipped: 0
- Test time: 57.6619 seconds

## Numeric Coverage

- Lines covered: 89,224
- Lines valid: 106,048
- Repository line rate: 0.841355
- Repository line coverage: 84.1355%
- First-party production packages: 9
- Test packages in final Cobertura: 0

Artifact:

- Path: `evidence/qa-gates/coverage-final.2026-07-21T20-25.cobertura.xml`
- Bytes: 9,932,006
- SHA-256: `63eb63566fb106bf7fcdca03b40873dc91a4e1b4874af6fc5abc5b2325aa0809`

## Instrumentation Stability Protocol

Prior repository evidence records profiler/test-host instability when test binaries themselves are instrumented. Before P5-T1, `<ModulePath>.*\.Test\.dll$</ModulePath>` was added temporarily to the instrumentation exclusions. All eight test assemblies were still discovered and executed; only test-code instrumentation was excluded, as required by repository coverage policy. After the successful wrapper returned, `coverage.config` was restored immediately:

- Working blob: `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`
- Tracked HEAD blob: `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`
- `git diff --exit-code -- coverage.config`: 0

The failed 20:20 zero-hit diagnostic artifact is invalid and is not cited as final coverage evidence.

P5-T4 result: PASS.
