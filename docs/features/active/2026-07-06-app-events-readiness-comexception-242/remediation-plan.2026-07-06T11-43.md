# Remediation Plan: app-events-readiness-comexception-242 (#242)

- **Issue:** #242
- **Owner:** remediation executor
- **Last Updated:** 2026-07-06T11-43
- **Status:** Draft for execution
- **Work Mode:** minor-audit remediation
- **Requirements Source:** `docs/features/active/2026-07-06-app-events-readiness-comexception-242/remediation-inputs.2026-07-06T11-43.md`
- **Context Package:** `artifacts/pr_context.summary.txt`; `artifacts/pr_context.appendix.txt`; `docs/features/active/2026-07-06-app-events-readiness-comexception-242/policy-audit.2026-07-06T11-43.md`; `docs/features/active/2026-07-06-app-events-readiness-comexception-242/code-review.2026-07-06T11-43.md`; `docs/features/active/2026-07-06-app-events-readiness-comexception-242/feature-audit.2026-07-06T11-43.md`; `docs/features/active/2026-07-06-app-events-readiness-comexception-242/plan.2026-07-06T10-42.md`; `docs/features/active/2026-07-06-app-events-readiness-comexception-242/issue.md`

This remediation plan addresses the review findings for issue #242. It does not authorize policy changes, unrelated implementation changes, or replacement of canonical PR-context artifacts.

### Phase 0 — Remediation Baseline

- [x] [P0-T1] Read the repository instructions and remediation context in this order: `AGENTS.md`, `.agents/skills/policy-compliance-order/SKILL.md`, `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`, `.agents/skills/atomic-plan-contract/SKILL.md`, `.agents/skills/acceptance-criteria-tracking/SKILL.md`, `docs/features/active/2026-07-06-app-events-readiness-comexception-242/remediation-inputs.2026-07-06T11-43.md`, `artifacts/pr_context.summary.txt`, and `artifacts/pr_context.appendix.txt`; write `docs/features/active/2026-07-06-app-events-readiness-comexception-242/evidence/remediation-baseline/remediation-phase0-instructions-read.2026-07-06T11-43.md` with `Timestamp:`, `Policy Order:`, and the explicit files read.
- [x] [P0-T2] Capture the current whitespace baseline by running `git diff --check origin/main..HEAD` from `C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36`; write `docs/features/active/2026-07-06-app-events-readiness-comexception-242/evidence/remediation-baseline/remediation-baseline-diff-check.2026-07-06T11-43.md` with `Timestamp:`, `Command: git diff --check origin/main..HEAD`, `EXIT_CODE:`, and `Output Summary:` listing the exact failing files or confirming no failures.
- [x] [P0-T3] Capture the current coverage-floor baseline by reading `docs/features/active/2026-07-06-app-events-readiness-comexception-242/evidence/qa-gates/final-coverage-comparison.2026-07-06T10-44.md`; write `docs/features/active/2026-07-06-app-events-readiness-comexception-242/evidence/remediation-baseline/remediation-baseline-coverage-floor.2026-07-06T11-43.md` with `Timestamp:`, `Command: coverage comparison artifact inspection`, `EXIT_CODE: 0`, and `Output Summary:` containing repo-wide coverage, changed-code coverage, and whether the 80% repo-wide floor is met.

### Phase 1 — Evidence Whitespace Remediation

- [x] [P1-T1] Remove trailing whitespace only from the evidence Markdown files reported by the Phase 0 diff-check baseline; do not change implementation code, tests, issue text, acceptance criteria text, or policy documents.
- [x] [P1-T2] Verify the uncommitted whitespace remediation before committing by rerunning the working-tree-aware command `git diff --check origin/main`; write `docs/features/active/2026-07-06-app-events-readiness-comexception-242/evidence/qa-gates/remediation-diff-check.2026-07-06T11-43.md` with `Timestamp:`, `Command: git diff --check origin/main`, `EXIT_CODE:`, and `Output Summary:`; acceptance criterion: exit code 0.

### Phase 2 — Coverage and Test-Command Disposition

- [x] [P2-T1] Inspect whether remediation can meet the repository-wide C# 80% coverage floor without unrelated scope expansion; write `docs/features/active/2026-07-06-app-events-readiness-comexception-242/evidence/qa-gates/remediation-coverage-floor-disposition.2026-07-06T11-43.md` with `Timestamp:`, `Command: coverage floor disposition from review and coverage artifacts`, `EXIT_CODE: 0`, and `Output Summary:` stating either `COVERAGE_FLOOR_MET` with numeric evidence or `COVERAGE_FLOOR_REMAINS_BLOCKED` with the exact current percentage and required next authority.
- [x] [P2-T2] Run the approved C# verification sequence in order: `dotnet tool run csharpier check .`, `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`, and `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage`; write one evidence artifact per command under `docs/features/active/2026-07-06-app-events-readiness-comexception-242/evidence/qa-gates/` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P2-T3] Run the diagnostic VSTest command without `/EnableCodeCoverage` only to classify the dependency behavior; write `docs/features/active/2026-07-06-app-events-readiness-comexception-242/evidence/other/remediation-vstest-noncoverage-diagnostic.2026-07-06T11-43.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; acceptance criterion: the artifact clearly states whether the command now passes or still fails due `System.Threading.Tasks.Extensions`.

### Phase 3 — Review Artifact Refresh and Final Gate

- [x] [P3-T1] Update the existing review artifacts in place or create new timestamped post-remediation review artifacts in `docs/features/active/2026-07-06-app-events-readiness-comexception-242/` so they reflect the remediation evidence; do not delete the original review artifacts.
- [x] [P3-T2] Validate each review artifact with `validate_orchestration_artifacts` using artifact types `policy-audit`, `code-review`, and `feature-audit`; write `docs/features/active/2026-07-06-app-events-readiness-comexception-242/evidence/qa-gates/remediation-review-validator.2026-07-06T11-43.md` with `Timestamp:`, `Command: validate_orchestration_artifacts for policy-audit/code-review/feature-audit`, `EXIT_CODE:`, and `Output Summary:`.
- [ ] [P3-T3] Mark remediation complete only if, after the remediation commit is created, the branch comparison command `git diff --check origin/main..HEAD` exits 0, the approved C# verification sequence passes, the review artifacts validate, and the coverage floor is either met or an approved exception is recorded without modifying policy documents; otherwise record remediation as blocked with the exact unmet gate.
