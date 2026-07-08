# P2-T4 — Consolidated Repo-Wide Coverage Measurement (Remediation Cycle 1, Issue #223)

Timestamp: 2026-06-28T21-50
Command: scripts/vscode/Invoke-MSTestWithCoverage.ps1 (PATH-LOCAL); [xml] parse + per-`<line>` summation of artifacts/csharp/coverage.xml
EXIT_CODE: 0

## Canonical artifact
- `artifacts/csharp/coverage.xml` (well-formed Cobertura, ~8.97 MB, nine first-party packages, `.Test` stripped).

## Acquisition path
- PATH-LOCAL (single bounded run of the repo coverage script; the known Moq binding-redirect failure did not occur this cycle).

## Repo-wide first-party testable-denominator figure (Finding 1 measurement sub-claim RESOLVED)
- Authoritative (#197 per-`<line>`, vendored included): 73.35% (39585/53969).
- Cobertura root aggregate: 74.11% (71654/96685).
- Vendored-excluded per-`<line>` (transparency): 76.08% (38607/50745).

## Floor decision
- FLOOR-BELOW: 73.35% / 74.11% < 80%.

## Test result
- 4566 / 4566 first-party tests passed (repo-wide), no test removed or weakened (G3 honored).

Output Summary:
The canonical Cobertura artifact `artifacts/csharp/coverage.xml` was acquired via PATH-LOCAL and the repo-wide first-party testable-denominator figure is 73.35% (authoritative) / 74.11% (root). FLOOR-BELOW: below the `>= 80%` floor. Finding 1 (artifact absent + figure unmeasured) is resolved as to artifact existence and measurement; the floor confirmation is FLOOR-BELOW, routed to orchestrator escalation per P2-T5. The gate is not weakened and the cycle does not silently pass.
