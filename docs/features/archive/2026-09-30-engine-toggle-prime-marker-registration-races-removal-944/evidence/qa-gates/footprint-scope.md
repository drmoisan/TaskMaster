# Footprint Scope (P3-T12, P3-T14)

Timestamp: 2026-09-30T15-16
Command: git diff --name-only --diff-filter=A MAIN-MERGE-SHA HEAD with the raw-document extension filter; git status --porcelain --untracked-files=all --ignored -- docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944 (MAIN-MERGE-SHA 66afa6372fd82fc1ffd7c81f85a1ad65eebc5817 substituted for ANCHOR-SHA per the Phase 3 pass-2 anchor paragraph, because an unscoped diff against ANCHOR-SHA would list the merged item-929 paths)
EXIT_CODE: 0
Output Summary: RAW-DOCS-COMMITTED: 0; RAW-DOCS-UNTRACKED-IN-FEATURE: 0. The ADDED-PATH list (42 paths) contains the positive control TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs. Every P3-T12 clause holds. Substitution: interpolated output strings rewritten as string concatenation; the git commands, extension list and counting expressions are unchanged.

## P3-T12 Details

Extension list: .trx, .xml, .coverage, .coveragexml, .cobertura.

```
ADDED-PATH: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/anchor-edit-regions.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/anchor-merge.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/anchor-production-shape.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/anchor-test-side.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/bootstrap-dotnet-coverage.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/bootstrap-nuget-restore.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/bootstrap-sdk.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/bootstrap-tool-restore.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/coordinator-tests-baseline.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/coverage-baseline.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/csharpier-check-baseline.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/file-line-counts-baseline.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/msbuild-analyzer-baseline.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/msbuild-nullable-baseline.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/phase0-commit.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/phase0-instructions-read.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/pre-merge-docs-commit.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/scope-and-anchor.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/stall-probe.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/upstream-942-check.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/coverage-summary.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/csharpier-check-final.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/csharpier-format.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/csproj-registration.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/file-line-counts.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/implementation-commit.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/msbuild-analyzer-final.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/msbuild-nullable-final.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/production-edit-scope.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/protected-regions-unchanged.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/toolchain-final-pass.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/regression-testing/build-after-fix.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/regression-testing/build-before-fix.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/regression-testing/prime-registration-fail-before.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/regression-testing/prime-registration-partial-tokens.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/regression-testing/prime-registration-pass-after.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/issue.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/plan.2026-09-30T07-20.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/research/2026-09-30T08-00-engine-toggle-prime-marker-registration-research.md
ADDED-PATH: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/spec.md
ADDED-PATH: docs/features/potential/promoted/2026-09-30-engine-toggle-prime-marker-registration-races-removal.md
RAW-DOCS-COMMITTED: 0
RAW-DOCS-UNTRACKED-IN-FEATURE: 0
```

Paths the porcelain span listed for the feature folder (untracked, ignored or modified; the counting expression strips the status columns): evidence/qa-gates/coverage-summary.md, plan.2026-09-30T07-20.md and evidence/qa-gates/determinism-tokens.md, all Markdown. No raw test-result or coverage document is in the feature folder.

## P3-T14 — Change footprint

Timestamp: 2026-09-30T15-18
Command: git diff --name-status MAIN-MERGE-SHA HEAD and git status --porcelain --untracked-files=all (MAIN-MERGE-SHA 66afa6372fd82fc1ffd7c81f85a1ad65eebc5817 substituted for ANCHOR-SHA per the Phase 3 pass-2 anchor paragraph; HEAD 594c3eb9b)
EXIT_CODE: 0
Output Summary: INHERITED-AND-EXCLUDED: the promotion record only; THIS-ITEM-FOOTPRINT: the three code paths plus feature-folder paths only; no FOOTPRINT OUTSIDE AC17 path; no .claude/ or artifacts/ path in the diff; no porcelain line under TaskMaster/ or TaskMaster.Test/. Every P3-T14 clause holds.

INHERITED-AND-EXCLUDED: A docs/features/potential/promoted/2026-09-30-engine-toggle-prime-marker-registration-races-removal.md

THIS-ITEM-FOOTPRINT:

- A TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs (code path; the new partial, status A)
- M TaskMaster.Test/TaskMaster.Test.csproj (code path)
- M TaskMaster/Ribbon/EngineToggleStateCoordinator.cs (code path)
- A, for each of the following under docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/: evidence/baseline/anchor-edit-regions.md, evidence/baseline/anchor-merge.md, evidence/baseline/anchor-production-shape.md, evidence/baseline/anchor-test-side.md, evidence/baseline/bootstrap-dotnet-coverage.md, evidence/baseline/bootstrap-nuget-restore.md, evidence/baseline/bootstrap-sdk.md, evidence/baseline/bootstrap-tool-restore.md, evidence/baseline/coordinator-tests-baseline.md, evidence/baseline/coverage-baseline.md, evidence/baseline/csharpier-check-baseline.md, evidence/baseline/file-line-counts-baseline.md, evidence/baseline/msbuild-analyzer-baseline.md, evidence/baseline/msbuild-nullable-baseline.md, evidence/baseline/phase0-commit.md, evidence/baseline/phase0-instructions-read.md, evidence/baseline/pre-merge-docs-commit.md, evidence/baseline/scope-and-anchor.md, evidence/baseline/stall-probe.md, evidence/baseline/upstream-942-check.md, evidence/qa-gates/coverage-summary.md, evidence/qa-gates/csharpier-check-final.md, evidence/qa-gates/csharpier-format.md, evidence/qa-gates/csproj-registration.md, evidence/qa-gates/file-line-counts.md, evidence/qa-gates/implementation-commit.md, evidence/qa-gates/msbuild-analyzer-final.md, evidence/qa-gates/msbuild-nullable-final.md, evidence/qa-gates/production-edit-scope.md, evidence/qa-gates/protected-regions-unchanged.md, evidence/qa-gates/toolchain-final-pass.md, evidence/regression-testing/build-after-fix.md, evidence/regression-testing/build-before-fix.md, evidence/regression-testing/prime-registration-fail-before.md, evidence/regression-testing/prime-registration-partial-tokens.md, evidence/regression-testing/prime-registration-pass-after.md, issue.md, plan.2026-09-30T07-20.md, research/2026-09-30T08-00-engine-toggle-prime-marker-registration-research.md, spec.md

Clause checks:

- The three code paths are all present, and the new partial has status A.
- Absent from the footprint: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs, TaskMaster/Ribbon/RibbonController.EngineCommands.cs, TaskMaster/TaskMaster.csproj, TaskMaster.runsettings and scripts/vscode/TaskMaster.cli.runsettings.
- No path under .claude/ or artifacts/ is in the footprint.
- FOOTPRINT OUTSIDE AC17: none.

Porcelain companion (git status --porcelain --untracked-files=all), verbatim:

```
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/orchestrator/MEMORY.md
 M .claude/agent-memory/task-researcher/MEMORY.md
 M docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/coverage-summary.md
 M docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/plan.2026-09-30T07-20.md
?? .claude/agent-memory/atomic-planner/project_944_prime_marker_registration_plan_seams.md
?? .claude/agent-memory/orchestrator/isolated-child-liveness-wait-and-delegation-target-lines.md
?? .claude/agent-memory/task-researcher/project_prime_marker_register_before_start_944.md
?? docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/determinism-tokens.md
?? docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/evidence-hygiene.md
?? docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/footprint-scope.md
```

Composition: no porcelain line names a path under TaskMaster/ or TaskMaster.Test/; the remaining lines are either under the feature folder (the uncommitted Phase 3 artifacts and the plan check-offs) or under .claude/agent-memory/ (uncommitted session memory, never staged, and matching the PRE-EXISTING-WORKTREE-PATHS set recorded in evidence/baseline/anchor-merge.md).
