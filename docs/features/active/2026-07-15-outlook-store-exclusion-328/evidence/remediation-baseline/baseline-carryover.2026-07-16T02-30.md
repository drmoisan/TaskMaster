# Remediation Baseline — Carryover Statement (Issue #328)

Timestamp: 2026-07-16T02-30
Command: N/A — carryover, no toolchain re-run
EXIT_CODE: 0 (carryover)

## Basis for carryover

This remediation modifies only Markdown documents (R3: `spec.md`, `user-story.md`), one Markdown
disposition note (R2), one Markdown evidence note plus the JaCoCo coverage-gate input file
`artifacts/csharp/coverage.xml` (R1), and canonical evidence artifacts. No C# source file, no C# test
file, and no `.csproj`/`.props`/`.targets` file is changed. No compilable file compiles differently.

Therefore the four-stage C# toolchain loop (csharpier -> analyzer build -> nullable/TreatWarningsAsErrors
build -> vstest with coverage) is NOT triggered and is NOT re-run. The authoritative toolchain and
coverage baseline is the delivered plan's executed Phase 4 evidence, carried over unchanged.

## Carryover artifacts (delivered plan Phase 4)

- `evidence/qa-gates/final-csharpier.2026-07-15T18-45.md`
- `evidence/qa-gates/final-analyzer-build.2026-07-15T18-45.md`
- `evidence/qa-gates/final-nullable-build.2026-07-15T18-45.md`
- `evidence/qa-gates/final-vstest.2026-07-15T18-45.md`
- `evidence/qa-gates/final-coverage.2026-07-15T18-45.cobertura.xml`
- `evidence/qa-gates/coverage-delta.2026-07-15T18-45.md`
- `evidence/qa-gates/file-size-check.2026-07-15T18-45.md`

All seven files are present on disk (verified 2026-07-16T02-30).

## Output Summary

Toolchain (carried over, not re-run): csharpier PASS; analyzer build EXIT 0; nullable/TWAE build
EXIT 0; vstest 4611/4611 passing without coverage instrumentation. csharpier/analyzers/nullable/vstest
are not re-run because no compilable file changes in this remediation.

Numeric carryover coverage headline values (per-class, from
`final-coverage.2026-07-15T18-45.cobertura.xml` / `coverage-delta.2026-07-15T18-45.md`):

- StoreFilterAttribution: line 100.00% / branch 96.88%
- StoresWrapper:          line  98.42% / branch 89.13%
- StoreWrapper:           line  95.31% / branch 64.81%  (branch below the 75% floor — pre-existing; see R2 disposition)
- StoreWrapperController: line  95.89% / branch 85.38%

New/changed-line coverage on the touched non-exempt first-party classes is >= 95% line with both arms
of every new branch covered (coverage-delta evidence). No first-party regression is introduced by the
delivered feature; this remediation does not alter that.
