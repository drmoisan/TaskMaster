# QA Post-Commit Verification (P4-T25)

Timestamp: 2026-09-29T09-22
Command: git diff --name-status 177b6d78e1b2408e5aedbd794cef3aad6b7fb372 HEAD ; git status --porcelain --untracked-files=all ; git show --name-only --format= HEAD (each run as git -C <repo-root> ...)
EXIT_CODE: 0
Output Summary:
- HEAD: 544a5708fa89b7f661e47d7dc56c92050a8e0388 (the P4-T24 commit; informational)
- ANCHORED-DIFF: 48 paths. Every path is either in BASE-DIFF-PATHS (the two orchestrator agent-memory paths, issue.md, the plan, the two research records, spec.md) or a Write Set path. PRE-EXISTING-NON-WRITE-SET is NONE. No path lies outside those sets.
- Unconditional Write Set coverage: every unconditional Write Set path appears in the anchored diff except this artifact, which the P4-T26 commit adds. The conditional rerun artifact is absent because the D6 branch was not taken.
- GIT-SHOW (HEAD): 16 paths, all Write Set paths (14 qa-gates artifacts, the plan and spec.md); no agent-memory path and no path outside the Write Set.
- Two C# files: they are not in the HEAD listing because their final content was committed by earlier phase commits (7443cb262 for the fix, 19eda7f7e for the format) and has not changed since; P4-T7 shows the hashes equal the P4-T1 AFTER hashes. Both appear as `M` in the anchored diff above, so they are committed on the branch. The P4-T24 clause that the HEAD listing contains the two C# files cannot hold after mid-plan commits; this is recorded as a plan-shape deviation, not a missing change.
- PORCELAIN: only .claude/agent-memory/ entries (8, never staged) and the plan file (` M`, the P4-T24 check-off written after that commit). This artifact was written after the porcelain listing was taken. No other entry is inside or outside the feature folder.

git diff --name-status 177b6d78e1b2408e5aedbd794cef3aad6b7fb372 HEAD:
```
M	.claude/agent-memory/orchestrator/MEMORY.md
A	.claude/agent-memory/orchestrator/parallel-item-preparation-is-structurally-impossible.md
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
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/coverage-jacoco-projection.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/coverage-summary.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/mstest-test-result-summary.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-acceptance-criteria-status.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-analyzer-rebuild.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-coverage-comparison.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-coverage-test-run.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-csharpier-check.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-csharpier-format.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-footprint-scope.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-hygiene-scan.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-loop-closure.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-nullable-rebuild.md
A	docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-post-format-audit.md
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
```

git show --name-only --format= HEAD:
```
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/coverage-jacoco-projection.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/coverage-summary.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/mstest-test-result-summary.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-acceptance-criteria-status.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-analyzer-rebuild.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-coverage-comparison.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-coverage-test-run.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-csharpier-check.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-csharpier-format.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-footprint-scope.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-hygiene-scan.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-loop-closure.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-nullable-rebuild.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/qa-post-format-audit.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/plan.2026-09-13T18-24.md
docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/spec.md
```
