# Remediation Plan: Bayesian Email Sorter Unit Tests (#248)

- **Issue:** #248
- **Requirements Source:** `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/remediation-inputs.2026-07-06T19-09.md`
- **Feature Folder:** `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248`
- **Original Plan:** `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/plan.2026-07-06T18-07.md`
- **Review Artifacts:** `policy-audit.2026-07-06T19-09.md`, `code-review.2026-07-06T19-09.md`, `feature-audit.2026-07-06T19-09.md`
- **PR Context:** `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`
- **Planning Handoff Receipt:** `resolve_atomic_plan_prompt` returned success for this target file.
- **Status:** Ready for remediation executor preflight validation.

## Remediation Scope

This plan addresses the remediation findings recorded in `remediation-inputs.2026-07-06T19-09.md`:

- COV-1: repository-wide C# line coverage is 20.21%, below the required 80% floor.
- TOOL-1: the planned CSharpier command `dotnet tool run csharpier .` is incompatible with the pinned local CSharpier CLI, while `dotnet tool run csharpier format .` passes.

No production or test implementation edits are authorized by this review handoff. If the remediation executor determines that COV-1 cannot be resolved without broad test expansion outside issue #248, it must record a blocked disposition artifact rather than weakening policy or marking coverage as out of scope.

### Phase 0 — Remediation Baseline

- [x] [P0-T1] Record remediation policy-read evidence.
  - Files: `AGENTS.md`, `.agents/skills/atomic-plan-contract/SKILL.md`, `.agents/skills/acceptance-criteria-tracking/SKILL.md`, `.agents/skills/csharp/SKILL.md`, `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/remediation-inputs.2026-07-06T19-09.md`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/remediation-baseline/remediation-policy-read.2026-07-06T19-09.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command: policy/read`, `EXIT_CODE: 0`, and `Output Summary:` with the files read and policy order applied.

- [x] [P0-T2] Capture the COV-1 coverage deficit from existing evidence.
  - Files: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-coverage-comparison.2026-07-06T18-07.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-06T18-07.coveragexml`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/remediation-baseline/csharp-coverage-deficit.2026-07-06T19-09.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command: coverage deficit analysis from recorded evidence`, `EXIT_CODE: 0`, `Output Summary:`, final line coverage, required line coverage, numeric gap, and whether issue #248 changed production files.

- [x] [P0-T3] Capture the TOOL-1 formatter invocation baseline.
  - Files: `.config/dotnet-tools.json`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharpier-final.2026-07-06T18-07.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharpier-final.2026-07-06T18-07.attempt1.md`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/remediation-baseline/csharpier-command-baseline.2026-07-06T19-09.md`
  - Acceptance: Evidence contains `Timestamp:`, exact CSharpier command(s), numeric exit codes from recorded evidence or fresh check-only command execution, and an `Output Summary:` identifying the accepted formatter invocation.

### Phase 1 — Remediation Actions

- [x] [P1-T1] Resolve TOOL-1 without weakening formatting enforcement.
  - Files: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/remediation-baseline/csharpier-command-baseline.2026-07-06T19-09.md`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharpier-remediation.2026-07-06T19-09.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` showing a passing CSharpier invocation. If the exact planned command remains incompatible, the evidence must state the compatible invocation and identify the policy-command mismatch as unresolved for policy-owner follow-up.

- [x] [P1-T2] Produce the COV-1 automated coverage-floor disposition.
  - Files: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/remediation-baseline/csharp-coverage-deficit.2026-07-06T19-09.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/remediation-inputs.2026-07-06T19-09.md`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/remediation-baseline/coverage-floor-disposition.2026-07-06T19-09.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command: coverage floor disposition`, `EXIT_CODE: 0`, `Output Summary:`, the final line coverage value, the 80% requirement, the exact remediation feasibility finding, and one of these dispositions: `RESOLVED_BY_COVERAGE_EVIDENCE` or `BLOCKED_BY_REPOSITORY_WIDE_COVERAGE_DEBT`.

- [x] [P1-T3] Mirror the remediation disposition into local issue-update evidence.
  - Files: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/remediation-baseline/coverage-floor-disposition.2026-07-06T19-09.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharpier-remediation.2026-07-06T19-09.md`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/issue-updates/remediation-status.2026-07-06T19-09.md`
  - Acceptance: Evidence contains `Timestamp:`, `PostedAs: unknown` or a GitHub comment URL if posted, and a concise status summary for COV-1 and TOOL-1.

### Phase 2 — Final QA and Review Handoff

- [x] [P2-T1] Run the final formatter verification for the remediation cycle.
  - Files: C# files changed by the remediation cycle, or the issue #248 changed C# test files if no C# files changed during remediation.
  - Command: `dotnet tool run csharpier format .`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharpier-remediation-final.2026-07-06T19-09.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:`.

- [x] [P2-T2] Run the final analyzer build for the remediation cycle.
  - Files: `TaskMaster.sln`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-analyzers-remediation-final.2026-07-06T19-09.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:`.

- [x] [P2-T3] Run the final nullable build for the remediation cycle.
  - Files: `TaskMaster.sln`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-nullable-remediation-final.2026-07-06T19-09.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:`.

- [x] [P2-T4] Run the final MSTest coverage command for the remediation cycle.
  - Files: `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
  - Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-vstest-coverage-remediation-final.2026-07-06T19-09.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, `Output Summary:`, full-suite pass/fail counts, and numeric line coverage.

- [x] [P2-T5] Request a post-remediation feature review handoff.
  - Files: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/remediation-inputs.2026-07-06T19-09.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/remediation-plan.2026-07-06T19-09.md`, `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`
  - Evidence: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/remediation-review-handoff.2026-07-06T19-09.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command: feature-review handoff`, `EXIT_CODE: 0`, `Output Summary:`, and the exact artifacts to include in the post-remediation review context package.
