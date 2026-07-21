# Remediation Inputs — Issue #283 (liveoutlook-harness-construction-scoped-skip)

- Timestamp: 2026-07-08T14-34
- Base: `main` @ `930467f4`; Head @ `143da1ed`
- Source audits: `policy-audit.2026-07-08T14-34.md`, `code-review.2026-07-08T14-34.md`, `feature-audit.2026-07-08T14-34.md`
- Overall verdict: NO-GO pending remediation. Acceptance criteria AC1–AC7 PASS; three policy-level Blocking findings remain.

## Remediation-Required Findings

### R1 — modified-workflow-needs-green-run (Blocking)

- Trigger: `.github/workflows/ci.yml` is modified (adds `/TestCaseFilter:"TestCategory!=LiveOutlook"` to the vstest invocation).
- Gap: no green workflow run (PR-context or `workflow_dispatch`) against branch head `143da1ed` is recorded.
- Required action: run the affected CI workflow against head `143da1ed` (a `workflow_dispatch` green run satisfies the rule per `feature-review-workflow` SKILL), confirm conclusion = success, and record the run URL + head SHA in the feature `evidence/qa-gates/` folder as green-run evidence.
- Acceptance: a recorded workflow run whose head SHA equals `143da1ed` and whose conclusion is success for the modified workflow.
- Artifact to add: `docs/features/active/2026-07-08-liveoutlook-harness-construction-scoped-skip-283/evidence/qa-gates/ci-green-run.<timestamp>.md`.

### R2 — C# coverage not machine-verifiable (Blocking, coverage FAIL)

- Gap: canonical C# coverage artifact `artifacts/csharp/coverage.xml` is absent; no scratch Cobertura or `TestResults/` coverage file remains, so the reviewer cannot machine-verify the executor-reported new-seam 100% or the overall no-regression claim. Additionally, `ac-verification-summary.md` (90.0%) conflicts with `coverage-delta.md`/`csharp-test-coverage-final.md` (100.0%).
- Required action: regenerate C# coverage (`dotnet test --collect:"XPlat Code Coverage"` or the repo `dotnet-coverage` Cobertura flow with `/TestCaseFilter:TestCategory!=LiveOutlook`) and persist the report to the canonical artifact path consumed by the coverage gate; reconcile the seam figure to the final value (100.0% if confirmed).
- Acceptance: a canonical C# coverage artifact exists; the new seam file reports >= 90% line coverage; no regression on changed lines; the evidence dossiers agree on a single figure.

### R3 — PowerShell coverage below floor and not machine-verifiable (Blocking, coverage FAIL)

- Gap: canonical PowerShell coverage artifact `artifacts/pester/powershell-coverage.xml` is absent; the executor-reported QC-script line coverage is 77.06% (109/84), below the 85% line floor. Changed lines did not regress (the appended token is on already-covered return-array lines).
- Required action: persist a canonical Pester coverage artifact at `artifacts/pester/powershell-coverage.xml`, and either raise `Invoke-MSTest.ps1` and `Invoke-MSTestWithCoverage.ps1` line coverage to >= 85% (the uncovered lines are the top-level host-status/wrapper-invocation blocks) or record a maintainer-ratified exemption for those host-bound lines with rationale.
- Acceptance: a canonical PowerShell coverage artifact exists and either the two scripts clear the 85% line floor or a ratified exemption is documented; changed-line no-regression is preserved.

## Non-Blocking Items (carry forward, not gating)

- Seam invariant hardening (Low): consider factory methods or a `Debug.Assert` to enforce mutual exclusivity of `HarnessOutcome.Captured` / `SkipReason`. Optional.

## Handoff

Route R1–R3 to `atomic-planner` per `remediation-handoff-atomic-planner` to produce a remediation
plan. The three findings are small and independent; R2 and R3 are evidence-persistence plus a
coverage-floor decision, and R1 is a CI run against the branch head. No source-behavior change to
the seam is required.
