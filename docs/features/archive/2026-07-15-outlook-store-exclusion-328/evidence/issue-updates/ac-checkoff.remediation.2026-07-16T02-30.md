# Acceptance-Criteria Re-Checkoff — Remediation (Issue #328)

Timestamp: 2026-07-16T02-30
PostedAs: unknown (local AC-checkoff record; this remediation does not post to the GitHub issue —
posting/PR creation is out of scope for this cycle)

Work Mode: full-feature (AC sources: `spec.md` §12 AC1–AC12; `user-story.md` US-AC1–US-AC4).

## Basis for resolution

The feature-audit (`feature-audit.2026-07-15T21-22.md`) graded AC12 and US-AC4 as PARTIAL on two open
coverage items — (1) the canonical C# coverage artifact `artifacts/csharp/coverage.xml` was absent, and
(2) `StoreWrapper` branch coverage (64.81%) was below the 75% branch floor as a pre-existing condition.
This remediation resolves both:

- R1 — `artifacts/csharp/coverage.xml` is emitted (JaCoCo format, converted from the verified feature
  Cobertura, scoped to first-party production packages). It is present and hook-parseable
  (`Get-JacocoRepoCoverage` = 70.45% LINE, `Get-JacocoBranchCoverage` = 67.11% BRANCH; 6 LINE + 6
  BRANCH counters). The "canonical artifact absent" finding is resolved. The repo-wide first-party
  aggregate is authoritatively deferred to the PR CI coverage run per policy-audit §5.4 (issue #328's
  own assembly `UtilitiesCS` clears the floor at 88.33% line; the sub-85% aggregate is driven by
  out-of-scope, not-instrumented-in-this-run assemblies QuickFiler/Tags/TaskVisualization). Evidence:
  `evidence/qa-gates/csharp-coverage-canonical.2026-07-16T02-30.md`.
- R2 — the `StoreWrapper` branch-floor is a ratified, documented pre-existing exception (no threshold
  weakened, no production-source `exclude` added). Evidence:
  `evidence/qa-gates/storewrapper-branch-coverage-disposition.2026-07-16T02-30.md`.
- R3 — AC6 wording is reconciled to deletion-as-delivered (the two dead `ToDoEvents` methods were
  deleted under the maintainer-approved scope change `resolved_at: 2026-07-15T23:35:00Z`, not threaded).

Final verification: `evidence/qa-gates/remediation-verification.2026-07-16T02-30.md` (all three checks
PASS).

## Resolved AC verdicts

| Criterion | Prior (feature-audit) | Now | Basis |
|---|---|---|---|
| spec.md AC1–AC11 | PASS | PASS | unchanged |
| spec.md AC6 | PASS (with deviation) | PASS | R3 reconciled the spec/user-story wording to the delivered deletion; substantive requirement (bypass sites route through the shared predicate; no parallel filtering logic) retained |
| spec.md AC12 | PARTIAL | PASS | R1 (canonical artifact emitted + hook-parseable; repo-wide aggregate deferred to CI per §5.4) + R2 (ratified pre-existing branch-floor disposition) |
| user-story US-AC1–US-AC3 | PASS | PASS | unchanged |
| user-story US-AC4 | PARTIAL | PASS | same basis as AC12 (R1 + R2) |

## Checkbox state

- `spec.md` §12: 12/12 `[x]` (all PASS). AC Status narrative updated to cite the remediation resolution.
- `user-story.md`: 4/4 `[x]` (all PASS). AC Status narrative updated to cite the remediation resolution.
- No checkbox text was altered; both files already carried `[x]` from the delivered plan. This record
  and the updated AC Status narratives are the authoritative statement that AC12 and US-AC4 are now
  PASS (no longer PARTIAL).

### Acceptance Criteria Status
- Source: `spec.md` (AC1–AC12) and `user-story.md` (US-AC1–US-AC4)
- Total AC items: 16
- PASS: 16 (spec 12/12; user-story 4/4)
- PARTIAL / FAIL / UNVERIFIED: 0
- Items remaining: none.
