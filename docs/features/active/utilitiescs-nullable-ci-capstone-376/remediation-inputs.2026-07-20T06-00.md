# Remediation Inputs — utilitiescs-nullable-ci-capstone (Issue #376)

- Timestamp: 2026-07-20T06-00
- Source artifacts: `policy-audit.2026-07-20T06-00.md`, `code-review.2026-07-20T06-00.md`, `feature-audit.2026-07-20T06-00.md`

## Status

No AC-level or code-quality defect requires remediation. All 7 acceptance criteria PASS with
on-disk evidence; no correctness or behavior-change finding was raised at Medium severity or
above. This document exists because two **procedural, pre-merge** obligations remain open and
must be closed before this branch can actually merge — neither is a defect in the feature's
delivered work product.

## Remediation-Required Items

### 1. `modified-workflow-needs-green-run` gate is unsatisfied (Blocking for merge, not for this review)

- **Finding:** `.github/workflows/ci.yml` is modified by this feature. Per
  `.claude/skills/feature-review-workflow/SKILL.md`'s `modified-workflow-needs-green-run` rule, a
  diff under a GitHub Actions workflow file is Blocking at merge time unless a green workflow run
  against the branch head is present in remediation inputs. No such run exists yet.
- **Correctly recorded, not falsely claimed satisfied:** the feature's own
  `evidence/other/green-run-requirement-recorded.2026-07-20T04-50.md` explicitly states "NOT
  SATISFIED BY THIS PLAN" and assigns the obligation to epic-orchestrator. This review confirms
  that disposition is accurate and was not overstated.
- **Required action:** epic-orchestrator (or whoever triggers the branch's CI) must capture a
  green GitHub Actions run — either a PR-triggered run or a `workflow_dispatch` run — whose head
  SHA matches this branch's committed head, and record it as remediation-input evidence before
  merge to the integration branch (or, later, `main`).
- **Owner:** epic-orchestrator / repository maintainer.

### 2. Canonical `artifacts/csharp/coverage.xml` is absent (procedural, non-blocking for this feature's own quality bar)

- **Finding:** The automated coverage-verification tooling's canonical lookup path
  (`artifacts/csharp/coverage.xml`) does not exist in this worktree. This review performed
  independent verification instead using the feature's own canonical evidence-folder Cobertura
  artifacts (`evidence/baseline/baseline-coverage.cobertura.xml`,
  `evidence/qa-gates/qc-coverage.cobertura.xml`), which are complete, internally consistent with
  their corresponding markdown summaries, and show no coverage regression.
- **Required action:** if repo-wide automated coverage gating depends on the
  `artifacts/csharp/coverage.xml` path specifically (as opposed to per-feature evidence), copy or
  regenerate a Cobertura report at that canonical path before/at merge time. This is a tooling-
  wiring gap, not a code or test defect.
- **Owner:** epic-orchestrator / CI maintainer, at merge time.

## Non-Remediation Items (recorded for completeness, no action required)

- Repo-wide C# line coverage (83.89%) is below the 85% uniform floor in `.claude/rules/quality-tiers.md`,
  but this is a **pre-existing condition** (baseline 83.88%, before this feature), not introduced or
  worsened here (delta is a marginal increase). No action required from this feature; tracked
  separately as a repo-wide condition.
- Analyzer-package-version drift (16 first-party `.csproj` files) and one residual pre-existing
  CS2002 warning are correctly flagged-only in `spec.md`'s Maintainer Decision Summary as
  pre-existing, unrelated conditions confirmed present on `origin/main`. No action required from
  this feature.
- This branch currently has zero commits beyond its base (`git log bfcdb394..HEAD` is empty); all
  reviewed work is uncommitted in the worktree. This must be committed before a PR can be opened,
  but is a mechanical step, not a remediation item against the work's content.
