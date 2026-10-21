# Pre-existing evidence index — issue #877

Timestamp: 2026-09-13T10-42
Command: read-only verification, no command executed
EXIT_CODE: 0
Output Summary: Four already-committed evidence artifacts were read in this session and each was found schema-valid. None was re-run. The M3 fail-before half of the fail-before/pass-after pair is satisfied by citation of `evidence/regression-testing/m3-fail-before.2026-09-13T09-14.md` and is NOT re-run in this execution.

## Cited artifacts

### 1. `evidence/regression-testing/m3-fail-before.2026-09-13T09-14.md`

- Read in this session; found schema-valid.
- Carries `Timestamp: 2026-09-13T09-14`, a `Command:` row, `EXIT_CODE: 1`, `ExpectedExitCode: 1` and an `Output Summary:` row.
- Records Total 3, Failed 3, Passed 0, at head `c9590a8b7`, before any fix, with the three failures all at Deedle static-initializer time on `FileNotFoundException: netstandard, Version=2.1.0.0`.
- Because the declared expectation equals the observed exit code, the artifact normalizes to pass.

### 2. `evidence/baseline/m2-suite-before.2026-09-13T09-15.md`

- Read in this session; found schema-valid.
- Carries `Timestamp: 2026-09-13T09-15`, a `Command:` row, `EXIT_CODE: 0` and an `Output Summary:` row. It carries no `ExpectedExitCode:` row, and must not acquire one.
- Records Total 1395, Passed 1395, Failed 0, with `Workers=0` and `Scope=ClassLevel` in force, at head `c9590a8b7`.

### 3. `evidence/other/m6-order-control.2026-09-13T09-16.md`

- Read in this session; found schema-valid.
- Carries `Timestamp: 2026-09-13T09-16`, a `Command:` row, `EXIT_CODE: 1`, `ExpectedExitCode: 1` and an `Output Summary:` row.
- Records Total 13, Passed 10, Failed 3, at head `c9590a8b7`, with the zero-batch class executing first and failing.

### 4. `evidence/baseline/mechanism-static-facts.2026-09-13T09-17.md`

- Read in this session; found schema-valid.
- Carries `Timestamp: 2026-09-13T09-17`, a `Command:` row, `EXIT_CODE: 0` and an `Output Summary:` row.
- Records the measured structural links: Deedle references FSharp.Core 4.5.0.0 and netstandard 2.0.0.0; the deployed FSharp.Core is 11.0.0.0 and references netstandard 2.1.0.0; netstandard 2.1.0.0 is absent from `bin\Debug` and from the GAC; no repository config file mentions netstandard; the repository holds exactly two first-party `AssemblyResolve` handlers.

## Fail-before status

The fail-before half of the fail-before/pass-after pair required by the primary guard is satisfied by citation of artifact 1 above. That run is NOT re-run in this execution. The pass-after half is produced by the three post-fix M3 runs in Phase 2.
