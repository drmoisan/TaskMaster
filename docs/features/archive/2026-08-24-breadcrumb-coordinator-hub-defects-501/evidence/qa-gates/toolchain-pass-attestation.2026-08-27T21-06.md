# Final QA — Toolchain Pass Attestation (P7-T7)

Timestamp: 2026-08-27T21-06

## The four commands of the final uninterrupted pass, in order

| # | Step | Command | Timestamp | EXIT_CODE |
| ---: | --- | --- | --- | ---: |
| 1 | Format | `dotnet tool run csharpier format .` | 2026-08-27T20-57 | **0** |
| 2 | Analyze | `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 2026-08-27T20-58 | **0** |
| 3 | Type-check | `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 2026-08-27T20-59 | **0** |
| 4 | Test | `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\postchange.cobertura.xml` | 2026-08-27T21-02 | **0** |

Exactly four command rows, in the order format, analyze, type-check, test, each with `EXIT_CODE: 0`.

The read-only verification `dotnet tool run csharpier check .` (P7-T2, `EXIT_CODE: 0`, zero files needing
formatting) is not counted as a fifth step; it is the confirmation that step 1 reached a fixed point.

## Restart count

**Restart count: 0.**

The four steps ran once each, in order, with no restart. No step failed, and no step after step 1
changed any file.

## No file changed after P7-T1 in the final pass

Step 1 rewrote exactly 3 files, all of them this feature's own, measured by before/after SHA-256
comparison (`FF/evidence/qa-gates/final-csharpier-format.2026-08-27T20-57.md`). After that:

- `csharpier check .` (P7-T2) reported zero files needing formatting, so the formatter was at a fixed
  point and a second `format` pass would rewrite nothing.
- Steps 2, 3 and 4 are a compile, a compile, and a test run. None writes source. Their only outputs are
  `bin`/`obj` build artifacts and the coverage XML, none of which is a source file and none of which is in
  the change set.

**No source file changed after step 1 in the final pass.** That is why the restart count is 0 rather than
the loop having to repeat.

## Steps whose ACCEPTANCE was not fully met, recorded here rather than omitted

All four toolchain commands exited 0, which is what this task attests. Two separate evidence tasks in
Phase 7 nonetheless have unmet acceptance conditions, and they are named here so this attestation is not
read as a blanket clearance:

- **P5-T8** (logging verification) — its third acceptance conjunct requires citing a factually false
  statement about `QuickFiler.Test/QuickFiler.Test.csproj`. Left unchecked. See
  `FF/evidence/qa-gates/logging-verification-501.2026-08-27T20-48.md`.
- **P7-T6** (coverage delta) — the repository line-rate delta is -0.00099 pp against a required
  at-or-above 0.00 pp, and the combined SR-1 split-pair delta is -1.13636 pp against a required
  at-or-above -0.50 pp. Left unchecked. See
  `FF/evidence/qa-gates/coverage-delta.2026-08-27T21-05.md`.

Neither is a toolchain command failure. Both are threshold or citation conditions on evidence artifacts,
and both are documented with exact figures and root cause in their own artifacts.

Acceptance: exactly four command rows in the order format, analyze, type-check, test, each with
`EXIT_CODE: 0`, and the restart count stated as a number (0). PASS.
