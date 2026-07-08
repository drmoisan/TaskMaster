# Baseline — Contingency Precondition and Evidence-Location Invariant (Remediation Cycle 1, Issue #223)

Timestamp: 2026-06-28T21-46

## Bounded-local-attempt rule
- PATH-LOCAL is attempted exactly once (a single run of `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput artifacts/csharp/coverage.xml`). No retries, sleeps, or timing hacks are permitted (per PowerShell prohibited-behaviors policy and the plan's bounded-attempt rule).

## PATH-CI fallback trigger
- If the single bounded PATH-LOCAL run fails to produce a well-formed Cobertura `artifacts/csharp/coverage.xml` with a readable repo-wide `line-rate` (for example the known Moq binding-redirect failure during local full-assembly instrumentation), the decision task P1-T3 routes to PATH-CI.
- PATH-CI obtains the authoritative `.coverage` attachment from the green PR CI `quality-gates` run on head commit `e91927105abde2ceadd10a7011bc17d714108afd` and converts it to Cobertura at the canonical path. Instrumentation occurs on the CI runner, so the local binding-redirect failure does not block measurement.

## Single permitted non-evidence path
- The only output path permitted outside `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/evidence/<kind>/` is the canonical coverage artifact `artifacts/csharp/coverage.xml`, mandated by the coverage-verification contract.
- No `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` evidence path is used. No non-canonical evidence path was supplied by the caller, so no `EVIDENCE_LOCATION_OVERRIDE_REJECTED` entry is required.

Output Summary:
Contingency model recorded: one bounded PATH-LOCAL attempt; on failure, route to PATH-CI for the authoritative CI-produced measurement; the sole permitted non-evidence output path is `artifacts/csharp/coverage.xml`.
