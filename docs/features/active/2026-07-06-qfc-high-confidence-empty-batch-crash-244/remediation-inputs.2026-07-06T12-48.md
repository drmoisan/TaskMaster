# Remediation Inputs — Issue #244 (qfc-high-confidence-empty-batch-crash)

- Component/Feature: `2026-07-06-qfc-high-confidence-empty-batch-crash-244`
- Date: 2026-07-06
- Entry timestamp: 2026-07-06T12-48
- Source audit artifacts: `policy-audit.2026-07-06T12-48.md`, `code-review.2026-07-06T12-48.md`, `feature-audit.2026-07-06T12-48.md`
- Blocking finding count: **1**

## Blocking Finding

### 1. C# canonical coverage artifact absent (`artifacts/csharp/coverage.xml`)

- **File/path affected**: `artifacts/csharp/coverage.xml` (repository-relative canonical coverage artifact location; currently does not exist).
- **Expected behavior**: A canonical, repo-wide C# coverage artifact must exist at `artifacts/csharp/coverage.xml` whenever C# files have changed on the branch, per this review's mandatory Coverage Verification procedure. Its repo-wide line-rate and branch-rate must be evaluated against the uniform tier thresholds (line >= 85%, branch >= 75% per `.claude/rules/quality-tiers.md`; CLAUDE.md's embedded C# Unit Test Policy separately states repo-wide >= 80%).
- **Actual behavior**: `test -f artifacts/csharp/coverage.xml` returns not-found. C# has three changed files on this branch (`QuickFiler/Controllers/QfcDatamodel.cs`, `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`), so this is an unconditional FAIL per the reviewer's coverage-verification contract, independent of the strong feature-scoped Cobertura evidence already captured in `evidence/qa-gates/qc-coverage.md` (which is scoped to the `QuickFiler` package via a `QuickFiler.Test`-only run, not a repo-wide canonical artifact).
- **Verification command (for whichever remediation path is chosen)**: `test -f artifacts/csharp/coverage.xml && echo present` should print `present` after remediation.

## Do Not Do

- Do not modify AC1-AC5 in `issue.md`; all five are correctly checked off and independently re-verified as PASS in `feature-audit.2026-07-06T12-48.md`. This blocking finding is a review-policy artifact gap, not an unmet acceptance criterion.
- Do not modify the production fix in `QuickFiler/Controllers/QfcDatamodel.cs` or the guard logic; the fix itself has no identified defect.
- Do not weaken the coverage-verification rule, add a scope-narrowing note (e.g. "informational only," "not applicable," "N/A," "out of scope") to the coverage row, or otherwise suppress the FAIL verdict to force a pass; per this repo's `.claude/hooks/validate-feature-review-coverage.ps1` and the reviewer's own operating contract, such narrowing phrases on a coverage row for a changed language are themselves a violation.
- Do not silently re-run coverage generation and merge results without recording provenance; if remediation path 1 (below) is chosen, the generating command and its exit code must be captured as evidence per `evidence-and-timestamp-conventions`.
- Do not widen the scope of this remediation to the diagnosis artifact's optional Option 2 refactor (extracting `SelectFirstBatchRows` into a COM-free helper); that is a separate, non-blocking follow-up recommendation in `code-review.2026-07-06T12-48.md`, not required to close this finding.

## Available Remediation Paths (pick one; both are precedented in this repository's review history)

### Path 1 — Generate the canonical artifact

Generate a repo-wide C# coverage artifact at `artifacts/csharp/coverage.xml` via the repository's documented coverage procedure (for example `dotnet test --collect:"XPlat Code Coverage"` merged to Cobertura, or the multi-assembly `vstest.console.exe ... /EnableCodeCoverage` + `dotnet-coverage merge -f cobertura` procedure referenced in `.claude/agent-memory/feature-review/project_csharp-repowide-coverage-below-80.md`), then re-run feature-review so the policy audit can evaluate the real repo-wide line-rate and branch-rate against the uniform thresholds.

- **Known risk (documented precedent in this repo)**: a full-repository, 7-assembly Cobertura run previously measured repo-wide C# line coverage at approximately 58.9%, below both the 80% and 85% thresholds, as a pre-existing condition unrelated to any single small feature (`.claude/agent-memory/feature-review/project_csharp-repowide-coverage-below-80.md`). If this path is chosen and the real number remains below threshold, the correct outcome is a FAIL recorded with the pre-existing-condition context — not a reinterpretation of the threshold to changed-code-only.
- **Known local blocker**: a local, full-assembly C# coverage run has previously failed on a Moq binding-redirect issue in this repository (`.claude/agent-memory/feature-review/project_csharp-local-fullsuite-coverage-blocked.md`); the reliable path for a genuine repo-wide root may be the PR/CI run rather than a local one.

### Path 2 — Authority-recorded, PR-scoped coverage exception

If Path 1 is judged out of proportion to this minimal bugfix (repo-wide coverage is a pre-existing condition, and this feature's own changed/new-code and no-regression gates already pass within the feature-scoped evidence), the repository owner may record an authority-scoped policy exception limiting the coverage gate to changed/new code for this PR, following the precedent recorded in `.claude/agent-memory/orchestrator/feedback_repowide_coverage_authority_exception.md` (issue #171 precedent: PASS with a documented pre-existing-condition justification; issue #185 precedent: authority-recorded, PR-scoped exception).

- The exception must be authored by the repository owner/authority, not by an orchestrator or worker agent.
- Record it as a governance artifact in this feature folder, e.g. `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/coverage-policy-exception.md` with a stable ID such as `244-COV-001`.
- The exception must modify no policy document (`.claude/rules/*.md`); it scopes the gate for this PR only.
- After the exception is recorded, re-run feature-review so the coverage row can be judged PASS-with-exception, citing the exception artifact as evidence, without using any of the narrowing phrases prohibited above.

## Pointer to Audit Artifacts

- `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/policy-audit.2026-07-06T12-48.md` (§5 Test Coverage Detail, §8 Gaps and Exceptions, §10 Compliance Verdict)
- `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/code-review.2026-07-06T12-48.md`
- `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/feature-audit.2026-07-06T12-48.md` (AC5 row)
