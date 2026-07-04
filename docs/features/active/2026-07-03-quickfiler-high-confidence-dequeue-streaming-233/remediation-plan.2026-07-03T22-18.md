# Remediation Plan: QuickFiler High-Confidence Dequeue Streaming (#233)

- **Issue:** #233
- **Timestamp:** 2026-07-03T22-18
- **Status:** Draft for executor preflight
- **Primary Requirements Source:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/remediation-inputs.2026-07-03T22-18.md`
- **Feature Folder:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233`
- **Base Branch:** `main`
- **Merge Base:** `ec4af1f0924b175a725fe50a5d2a61f7d27a3318`
- **Reviewed Head:** `787bb46198df1a29189077cd450943c23fbb4a1a`

## Context Package

- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/remediation-inputs.2026-07-03T22-18.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/policy-audit.2026-07-03T22-18.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/code-review.2026-07-03T22-18.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/feature-audit.2026-07-03T22-18.md`
- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/plan.2026-07-03T16-57.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`

## Evidence Contract

All new evidence from executing this plan must be written under:

- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/`

Each command-bearing evidence artifact must include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

### Phase 0 — Remediation Baseline

- [x] [P0-T1] Read `AGENTS.md`, `.agents/skills/feature-review/SKILL.md`, `.agents/skills/atomic-plan-contract/SKILL.md`, `.agents/skills/csharp/SKILL.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/remediation-inputs.2026-07-03T22-18.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`, and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/phase0-22-18-instructions-read.md` with `Timestamp:`, `Policy Order:`, and exact files read.
- [x] [P0-T2] Capture the current remediation baseline by running `git status --short --branch --untracked-files=all`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/remediation-22-18-git-status-baseline.md` with the command, exit code, and output summary.
- [x] [P0-T3] Capture the current whitespace failure by running `git diff --check ec4af1f0924b175a725fe50a5d2a61f7d27a3318...HEAD`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/remediation-22-18-git-diff-check-baseline.md` with `EXIT_CODE: 1` and the listed trailing-whitespace paths.
- [x] [P0-T4] Reconfirm the AC10 coverage blocker by reading `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/r4-final-coverage-comparison.md` and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/r4-ac10-blocker.md`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/remediation-22-18-ac10-baseline.md`.

### Phase 1 — Correct Markdown Whitespace

- [x] [P1-T1] Remove only trailing whitespace from the issue #233 markdown artifacts listed in `remediation-inputs.2026-07-03T22-18.md`; do not alter policy meaning, acceptance-criteria text, validation summaries, or historical audit conclusions.
- [x] [P1-T2] Run `git diff --check HEAD` to verify the current uncommitted worktree delta after the issue #233 markdown whitespace edits; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-worktree-git-diff-check.md` with `Timestamp:`, `Command: git diff --check HEAD`, `EXIT_CODE: 0`, and `Output Summary:` confirming no whitespace diagnostics.
- [x] [P1-T3] Run `git diff --name-only HEAD` and `git ls-files --others --exclude-standard`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-22-18-whitespace-file-list.md` with `Timestamp:`, both `Command:` values, `EXIT_CODE:` values, and `Output Summary:` confirming all tracked and untracked remediation paths are issue #233 markdown/evidence artifacts unless a later task explicitly authorizes more.

### Phase 2 — Resolve AC10 Coverage Disposition

- [x] [P2-T1] Determine the AC10 route by reading `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/r4-final-coverage-comparison.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/r4-ac10-blocker.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/remediation-inputs.2026-07-03T22-18.md`, and any existing `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/*ac10*exception*.md` approval artifacts; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-22-18-ac10-route.md` with exactly one route value: `ROUTE: COVERAGE_IMPROVEMENT` when in-scope tests can raise repository-path coverage to the 80% floor, `ROUTE: APPROVED_EXCEPTION` when a valid pre-existing approved exception artifact explicitly authorizes AC10 disposition, or `ROUTE: FAIL_CLOSED` when neither condition is satisfied.
- [x] [P2-T2] Execute the coverage-improvement branch from `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-22-18-ac10-route.md`: if `ROUTE: COVERAGE_IMPROVEMENT`, keep changes scoped to issue #233 behavior and touched-code coverage gaps, update or add MSTest tests using Moq and FluentAssertions, and write targeted regression evidence under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/`; if the route is not `COVERAGE_IMPROVEMENT`, write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-22-18-coverage-branch-not-selected.md` with `Timestamp:`, `SelectedRoute:`, and `Output Summary:` confirming no coverage-improvement test changes were made by this branch task.
- [x] [P2-T3] Execute the non-coverage AC10 disposition branch from `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-22-18-ac10-route.md`: if `ROUTE: APPROVED_EXCEPTION`, write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-22-18-ac10-approved-exception.md` summarizing the approval artifact and leave AC10 unchecked unless the accepted exception explicitly authorizes check-off; if `ROUTE: FAIL_CLOSED`, write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-22-18-ac10-no-approved-exception.md` and leave AC10 unchecked; if `ROUTE: COVERAGE_IMPROVEMENT`, write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-22-18-noncoverage-branch-not-selected.md` with `Timestamp:`, `SelectedRoute:`, and `Output Summary:` confirming the non-coverage branch was not selected.

### Phase 3 — Final C# QA Loop

- [x] [P3-T1] Run `dotnet tool run csharpier -- check .`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-csharpier-check.md`. If it fails or changes are needed, fix only in-scope issues and restart Phase 3 at P3-T1.
- [x] [P3-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-msbuild-analyzers.md`. If it fails, fix only in-scope diagnostics and restart Phase 3 at P3-T1.
- [x] [P3-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-msbuild-nullable.md`. If it fails, fix only in-scope diagnostics and restart Phase 3 at P3-T1.
- [x] [P3-T4] Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\remediation-22-18-vstest-results`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-vstest.md`. If it fails, fix only in-scope failures and restart Phase 3 at P3-T1.
- [x] [P3-T5] Convert the newest `.coverage` attachment from P3-T4 with `dotnet-coverage merge <coverage-file> -o docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\remediation-22-18-vstest.cobertura.xml -f cobertura`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-coverage-conversion.md`.
- [x] [P3-T6] Compare coverage against the existing baseline and AC10 threshold; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-coverage-comparison.md` with repository-path coverage, changed/new-code coverage, and explicit PASS or FAIL for AC10.

### Phase 4 — Acceptance Tracking and PR Context

- [x] [P4-T1] Update only AC10 checkbox state in `spec.md` if Phase 3 evidence or an approved exception satisfies AC10; otherwise leave AC10 unchecked and record the reason in `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-22-18-ac10-status.md`.
- [x] [P4-T2] Apply the same AC10 decision to `user-story.md`, preserving criterion text exactly.
- [x] [P4-T3] Refresh PR context by running `mcp__drm_copilot.collect_pr_context` with base `main`; verify `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` reflect the remediated head.
- [x] [P4-T4] Produce fresh timestamped `policy-audit`, `code-review`, and `feature-audit` artifacts for the remediated head and validate each with `mcp__drm_copilot.validate_orchestration_artifacts`.

### Phase 5 — Final Validation

- [ ] [P5-T1] After the orchestrator creates the pre-R4 remediation commit, run `git diff --check ec4af1f0924b175a725fe50a5d2a61f7d27a3318...HEAD`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-post-commit-git-diff-check.md` with `Timestamp:`, `Command: git diff --check ec4af1f0924b175a725fe50a5d2a61f7d27a3318...HEAD`, `EXIT_CODE: 0`, and `Output Summary:` confirming the committed base-to-head delta has no whitespace diagnostics.
- [ ] [P5-T2] Validate this remediation plan with `mcp__drm_copilot.validate_orchestration_artifacts` using `artifact_type: "plan"` and this exact file path.
- [ ] [P5-T3] Confirm all command-bearing evidence artifacts contain `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [ ] [P5-T4] Report whether remediation leaves `REVIEW_STATUS: PASS` or `REVIEW_STATUS: REMEDIATION_REQUIRED`, using the feature-review final status fields.

## Do Not Do

- Do not modify repository policy files.
- Do not broaden production C# behavior while fixing markdown whitespace.
- Do not check off AC10 without qualifying evidence or approved exception evidence.
- Do not delete prior audit artifacts to remove failures from history.
- Do not write evidence outside the canonical feature-folder evidence paths.
- Do not treat unavailable GitHub CLI status as passing CI evidence.
