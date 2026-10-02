# QA Footprint and Scope Gate, Pre-Commit (P4-T9)

Timestamp: 2026-09-29T09-19
Command: git diff --name-status 177b6d78e1b2408e5aedbd794cef3aad6b7fb372 ; git status --porcelain --untracked-files=all ; git diff --numstat 177b6d78e1b2408e5aedbd794cef3aad6b7fb372 -- "*.csproj" (each run as git -C <repo-root> ...)
EXIT_CODE: 0
Output Summary:
- BASE-SHA: 177b6d78e1b2408e5aedbd794cef3aad6b7fb372 (from evidence/baseline/base-anchor.md)
- PROJECT-FILE-NUMSTAT: empty (no .csproj anywhere changed)
- FOOTPRINT = union of the two listings minus BASE-DIFF-PATHS (7 paths) minus PRE-EXISTING-NON-WRITE-SET (NONE).
- Every FOOTPRINT path is a Write Set path or lies under .claude/agent-memory/ (listed below as EXCLUDED-AGENT-MEMORY; never staged).
- No FOOTPRINT path lies under QuickFiler.Test/ other than the two Write Set C# files.
- No FOOTPRINT path ends in .trx, .coverage, .coveragexml, .cobertura.xml or .jacoco.xml, and none is named coverage.xml.
- Unconditional Write Set paths present in the union: the two C# files, spec.md and the plan (both tracked; spec.md shows as `A` and the plan as `A` in the anchored diff and ` M` in porcelain), all 18 baseline artifacts, all 6 regression-testing artifacts (the dossier resolves RUN-TS to 2026-09-29T09-06), and the 11 qa-gates artifacts written by P4-T1 to P4-T8.
- WRITE-SET-PATHS-NOT-YET-IN-UNION: qa-footprint-scope.md (this artifact, written by this task), qa-acceptance-criteria-status.md (P4-T21), qa-hygiene-scan.md (P4-T22) and qa-post-commit-verification.md (P4-T25). All four are written at or after this task, so no pre-commit listing can contain them. P4-T25 re-checks the committed set.
- Conditional path qa-coverage-test-run-rerun.md: absent, as expected (the D6 re-run branch was not taken).

git diff --name-status 177b6d78e1b2408e5aedbd794cef3aad6b7fb372:
```
M	.claude/agent-memory/atomic-executor/MEMORY.md
M	.claude/agent-memory/atomic-planner/MEMORY.md
M	.claude/agent-memory/orchestrator/MEMORY.md
A	.claude/agent-memory/orchestrator/parallel-item-preparation-is-structurally-impossible.md
M	.claude/agent-memory/prd-feature/project_671_projections_only_evidence.md
M	.claude/agent-memory/task-researcher/MEMORY.md
M	.claude/agent-memory/task-researcher/project_pump_timeout_743.md
M	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
M	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/base-anchor.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/baseline-analyzer-rebuild.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/baseline-coverage-test-run.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/baseline-csharpier-check.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/baseline-nullable-rebuild.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/baseline-source-facts.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/bootstrap-analyzer-paths.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/bootstrap-channel-probe.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/bootstrap-dotnet-sdk.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/bootstrap-package-restore.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/bootstrap-tool-resolution.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/bootstrap-tool-restore.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/coverage-jacoco-projection.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/coverage-summary.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/helper-script-record.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/mstest-test-result-summary.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/phase0-instructions-read.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/worktree-identity.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/regression-testing/build-after-fix.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/regression-testing/fail-before-exception.2026-09-29T09-06.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/regression-testing/fixture-structure-gates.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/regression-testing/pass-after-scoped-run.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/regression-testing/scoped-format-after-fix.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/regression-testing/test-structure-gates.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/issue.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/plan.2026-09-13T18-24.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/research/2026-09-13T19-00-transactiongate-bounded-acquisition-research.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/research/2026-09-28T00-10-transactiongate-research-refresh-research.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/spec.md
```

git status --porcelain --untracked-files=all:
```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/prd-feature/project_671_projections_only_evidence.md
 M .claude/agent-memory/task-researcher/MEMORY.md
 M .claude/agent-memory/task-researcher/project_pump_timeout_743.md
 M docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/plan.2026-09-13T18-24.md
?? .claude/agent-memory/atomic-executor/project_preimplementation_gate_reads_session_root_checkpoint_not_item_worktree.md
?? .claude/agent-memory/atomic-planner/project_882_transactiongate_bounded_acquisition_plan_seams.md
?? .claude/agent-memory/task-researcher/project_transactiongate_parallel_safe_probe_882.md
?? docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/coverage-jacoco-projection.md
?? docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/coverage-summary.md
?? docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/mstest-test-result-summary.md
?? docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-analyzer-rebuild.md
?? docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-coverage-comparison.md
?? docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-coverage-test-run.md
?? docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-csharpier-check.md
?? docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-csharpier-format.md
?? docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-loop-closure.md
?? docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-nullable-rebuild.md
?? docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-post-format-audit.md
```

EXCLUDED-AGENT-MEMORY (never staged):
```
.claude/agent-memory/atomic-executor/MEMORY.md
.claude/agent-memory/atomic-executor/project_preimplementation_gate_reads_session_root_checkpoint_not_item_worktree.md
.claude/agent-memory/atomic-planner/MEMORY.md
.claude/agent-memory/atomic-planner/project_882_transactiongate_bounded_acquisition_plan_seams.md
.claude/agent-memory/prd-feature/project_671_projections_only_evidence.md
.claude/agent-memory/task-researcher/MEMORY.md
.claude/agent-memory/task-researcher/project_pump_timeout_743.md
.claude/agent-memory/task-researcher/project_transactiongate_parallel_safe_probe_882.md
```

FOOTPRINT (excluding the agent-memory paths above):
```
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/base-anchor.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/baseline-analyzer-rebuild.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/baseline-coverage-test-run.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/baseline-csharpier-check.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/baseline-nullable-rebuild.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/baseline-source-facts.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/bootstrap-analyzer-paths.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/bootstrap-channel-probe.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/bootstrap-dotnet-sdk.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/bootstrap-package-restore.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/bootstrap-tool-resolution.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/bootstrap-tool-restore.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/coverage-jacoco-projection.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/coverage-summary.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/helper-script-record.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/mstest-test-result-summary.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/phase0-instructions-read.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/worktree-identity.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/regression-testing/build-after-fix.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/regression-testing/fail-before-exception.2026-09-29T09-06.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/regression-testing/fixture-structure-gates.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/regression-testing/pass-after-scoped-run.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/regression-testing/scoped-format-after-fix.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/regression-testing/test-structure-gates.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/coverage-jacoco-projection.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/coverage-summary.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/mstest-test-result-summary.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-analyzer-rebuild.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-coverage-comparison.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-coverage-test-run.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-csharpier-check.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-csharpier-format.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-loop-closure.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-nullable-rebuild.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-post-format-audit.md
```
