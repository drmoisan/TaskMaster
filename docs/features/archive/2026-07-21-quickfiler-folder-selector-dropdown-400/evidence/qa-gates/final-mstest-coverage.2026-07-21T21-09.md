# Final MSTest Coverage

Timestamp: 2026-07-21T21-09Z
Run Identity: `final-pass-2026-07-21T21-07Z`
Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final.2026-07-21T21-09.cobertura.xml`
EXIT_CODE: 0
Output Summary: The canonical repository wrapper discovered all eight first-party test assemblies, passed 5,849 of 5,849 tests with zero failures or skips, and produced post-processed Cobertura coverage of 89,255/106,048 lines, or 84.1647%.

## Test Results

- Discovered test assemblies: 8
- Total tests: 5,849
- Passed: 5,849
- Failed: 0
- Skipped: 0
- Test time: 52.4323 seconds

## Numeric Coverage

- Lines covered: 89,255
- Lines valid: 106,048
- Repository line rate: 0.841647
- Repository line coverage: 84.1647%
- First-party production packages: 9
- Test packages in final Cobertura: 0

Artifact:

- Path: `evidence/qa-gates/coverage-final.2026-07-21T21-09.cobertura.xml`
- Bytes: 9,932,023
- SHA-256: `6d44e4ba3cf9c5fbc3d37b2bf43ffc540c618309955861b55aa2b09a6177c1f0`

## Instrumentation Stability Protocol

Prior repository evidence records profiler/test-host instability when test binaries themselves are instrumented. Before P5-T1, `<ModulePath>.*\.Test\.dll$</ModulePath>` was added temporarily to the instrumentation exclusions. All eight test assemblies were still discovered and executed; only test-code instrumentation was excluded, consistent with repository coverage policy. Immediately after the successful wrapper returned, `coverage.config` was restored and verified against `HEAD`:

- Working Git blob: `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`
- Tracked `HEAD` blob: `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`
- `git diff --exit-code -- coverage.config`: 0

The superseded 20:25 and 20:38 artifacts are not cited as final coverage evidence.

P5-T4 result: PASS.
