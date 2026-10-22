# Issue #608 remediation cycle 2 input

Timestamp: 2026-08-25T12-55
Trigger: Remediation cycle 1 stopped at `[P1-T4]`.

## Immutable prior evidence

- Preserve `remediation-plan.2026-08-25T12-33.md` unchanged as the cycle-1 plan of record.
- Preserve `evidence/qa-gates/r1-csharp-tests-coverage.2026-08-25T12-33.md` unchanged as the failed coverage receipt.
- Preserve the original plan and its failed global-nullable evidence. This cycle must not revive `/p:Nullable=enable`.

## Observed failure

The required command `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput "docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/qa-gates/r1-csharp-coverage.2026-08-25T12-33.cobertura.xml"` exited 1.

- Test total: 6,476.
- Passed: 6,475.
- Failed: 1.
- Reported line coverage: 70.1716% (57,239 / 81,570), below the 84.7835% provenance baseline.
- The wrapper receipt did not retain the failing test name or failure detail.

## Required recovery scope

1. Diagnose the single failing test and the coverage variance using read-only, deterministic diagnostics before considering any source or test change.
2. Determine whether the failure or coverage result is attributable to Issue #608. Do not infer that it is.
3. Preserve the existing one-production-file and one-test-file Issue #608 implementation scope unless evidence proves a directly necessary correction. Do not repair unrelated failures, add suppressions, or modify policy/project/configuration files.
4. Use the executable C# QA gate: CSharpier, analyzer rebuild, `TreatWarningsAsErrors=true` rebuild without `/p:Nullable=enable`, then coverage-enabled MSTest. The cycle must retain an evidence-based coverage comparison and all Issue #608 acceptance criteria.
5. If the diagnostic establishes an unrelated external or baseline failure that cannot be corrected within #608 scope, record it precisely and stop without claiming a pass. Otherwise, complete the remaining QA, review, PR, and CI continuation tasks.

## Compatibility constraints

- Preserve issue #233's fill-or-exhaust contract.
- Preserve issue #424's zero-accepted deadline behavior.
- Preserve the distinct issue #446 scope: its empty-result/source-exhaustion interpretation work must not be modified or broadened by #608.

