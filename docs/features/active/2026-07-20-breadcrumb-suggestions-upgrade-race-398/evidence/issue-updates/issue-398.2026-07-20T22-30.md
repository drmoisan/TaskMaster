# Issue #398 Update Mirror — AC-5 Coverage Sub-Clause Confirmation (P2-T8)

Timestamp: 2026-07-20T23-17

PostedAs: body (local feature issue.md annotation under `## Acceptance Criteria` AC-5). Not pushed to
GitHub in this session per the "Do NOT commit" execution constraint; the update is mirrored into the
local feature issue.md at
docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/issue.md.

## Exact annotation text added under AC-5

> Coverage sub-clause confirmed (remediation 2026-07-20T22-30): the canonical HEAD JaCoCo artifact was
> regenerated at `artifacts/csharp/coverage.xml` (first-party denominator UtilitiesCS + QuickFiler).
> Verified via the gate hook functions `Get-JacocoRepoCoverage` / `Get-JacocoBranchCoverage`:
> line 86.54% (>= 85%), branch 80.85% (>= 75%). Full suite 5061/5061 passing; CSharpier/analyzer/nullable
> gates green. Test-only remediation (R1 partial-class splits), so production coverage is unchanged and
> the prior fix's new-code coverage (100%) is unaffected.

## Confirmed values

- Canonical coverage artifact path: artifacts/csharp/coverage.xml (JaCoCo format, hook-parseable).
- First-party line coverage: 86.54% (43143/49851) — >= 85% floor.
- First-party branch coverage: 80.85% (9331/11541) — >= 75% floor.
- Full MSTest suite: 5061 / 5061 passing (0 failed).
- Toolchain: CSharpier check clean, analyzer build 0 errors, nullable build 0 errors.

AC-5 (including its coverage sub-clause) is confirmed. R2 procedural FAIL resolved.
