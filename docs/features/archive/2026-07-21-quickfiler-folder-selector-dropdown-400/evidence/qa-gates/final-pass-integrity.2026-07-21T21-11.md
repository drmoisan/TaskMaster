# Final Pass Integrity

Timestamp: 2026-07-21T21-11Z
Run Identity: `final-pass-2026-07-21T21-07Z`
Command: Exact-field inspection of the four current final-pass artifacts, C# state-hash comparison, coverage artifact validation, and `git diff --exit-code -- coverage.config`
EXIT_CODE: 0
Output Summary: The current final-pass artifacts record one uninterrupted format, analyzer, nullable, and coverage sequence. Every command exited zero, no source file changed, all four artifacts use the same run identity, and no superseded artifact is cited.

| Order | Task | Artifact | Exit | State |
|---:|---|---|---:|---|
| 1 | P5-T1 format | `final-csharpier.2026-07-21T21-07.md` | 0 | 1,432 files; C# state unchanged |
| 2 | P5-T2 analyzers | `final-analyzers.2026-07-21T21-07.md` | 0 | Build succeeded; 5 known package warnings; 0 errors; C# state unchanged |
| 3 | P5-T3 nullable | `final-nullable.2026-07-21T21-07.md` | 0 | Build succeeded; 0 compiler/nullable warnings; 0 errors; C# state unchanged |
| 4 | P5-T4 coverage | `final-mstest-coverage.2026-07-21T21-09.md` | 0 | 5,849/5,849 passed; 89,255/106,048 lines; 84.1647% |

Integrity checks:

- Run identity occurrence in each artifact: exactly 1.
- `EXIT_CODE: 0` occurrence in each artifact: exactly 1.
- Exact `Command:` field occurrence in each artifact: exactly 1.
- Formatter, analyzer, and nullable C# state hash: `99ef4c6bde5f33d7dbd20cddf1df5ad2167ff34ad860339c7c14ee0ac625763b` before and after.
- Final Cobertura artifact: `coverage-final.2026-07-21T21-09.cobertura.xml`, SHA-256 `6d44e4ba3cf9c5fbc3d37b2bf43ffc540c618309955861b55aa2b09a6177c1f0`.
- Restored `coverage.config` working and `HEAD` Git blobs: `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`.
- `git diff --exit-code -- coverage.config`: 0.
- Superseded 20:25 and 20:38 final-pass artifacts cited as current evidence: No.

P5-T5 result: PASS.
