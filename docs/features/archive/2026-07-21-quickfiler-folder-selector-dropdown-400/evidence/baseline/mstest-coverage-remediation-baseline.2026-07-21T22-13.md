Timestamp: 2026-07-21T22-13Z
Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/baseline/coverage-remediation-baseline.2026-07-21T22-13.cobertura.xml`, executed while temporarily adding `<ModulePath>.*\.Test\.dll$</ModulePath>` only to instrumentation exclusions and restoring the original `coverage.config` bytes in `finally`
EXIT_CODE: 0
Output Summary: The canonical wrapper discovered all 8 first-party test assemblies and passed all 5,849 tests. Failed: 0. Skipped: 0. Zero-test detection: false. Repository line coverage is 89,240/106,048 = 84.1506%. The temporary instrumentation filter did not omit test execution and `coverage.config` was restored byte-for-byte and verified against `HEAD`.

- Test assemblies: 8
- Total tests: 5,849
- Passed: 5,849
- Failed: 0
- Skipped: 0
- Test time: 53.8855 seconds
- Lines covered: 89,240
- Lines valid: 106,048
- Repository line rate: 0.841506
- Repository line coverage: 84.1506%
- Cobertura path: `evidence/baseline/coverage-remediation-baseline.2026-07-21T22-13.cobertura.xml`
- Cobertura bytes: 9,932,026
- Cobertura SHA-256: `4BD88CD3B9786FA8E8142A39A84EEBEF44581C321D8146F6A8EB46A58D2D8FE8`

## Instrumentation integrity

- `coverage.config` SHA-256 before: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`
- `coverage.config` SHA-256 after: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`
- Working Git blob before/after: `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`
- `HEAD` blob: `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`
- `git diff --exit-code -- coverage.config`: 0

The earlier 22:10 attempt executed all 5,849 tests but had one transient failure. It is superseded as the current baseline by this clean rerun and is not cited for numeric completion gates.
