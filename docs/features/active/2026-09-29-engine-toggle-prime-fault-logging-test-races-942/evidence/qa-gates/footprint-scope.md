# Footprint scope (issue 942)

Timestamp: 2026-09-30T07-53
Task: P3-T12 (creates this file); P3-T14 appends.
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; $ext = @(".trx", ".xml", ".coverage", ".coveragexml", ".cobertura"); $added = @(git diff --name-only --diff-filter=A 231e1c0b55105aeb626bf5a6e8d0266a567cacad HEAD); ...; "RAW-DOCS-COMMITTED: ..."; $untracked = @(git status --porcelain --untracked-files=all --ignored -- docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942 | ...); "RAW-DOCS-UNTRACKED-IN-FEATURE: ..."' (the P3-T12 payload; the worktree path was composed inside the payload by concatenation)
EXIT_CODE: 0

Output Summary:

Committed additions since the anchor (name-listing diff):

- ADDED-PATH: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/bootstrap-dotnet-coverage.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/bootstrap-nuget-restore.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/bootstrap-sdk.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/bootstrap-tool-restore.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/coordinator-tests-baseline.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/coverage-baseline.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/csharpier-check-baseline.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/file-line-counts-baseline.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/msbuild-analyzer-baseline.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/msbuild-nullable-baseline.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/phase0-commit.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/phase0-instructions-read.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/scope-and-anchor.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/stall-probe.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/qa-gates/csproj-registration.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/qa-gates/harness-hook-edit-scope.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/qa-gates/implementation-commit.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/qa-gates/original-test-unchanged.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/qa-gates/production-reorder-scope.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/regression-testing/build-after-reorder.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/regression-testing/build-before-reorder.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/regression-testing/prime-fault-ordering-fail-before.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/regression-testing/prime-fault-ordering-pass-after.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/issue.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/plan.2026-09-29T23-07.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/research/2026-09-29T23-20-engine-toggle-prime-fault-race-research.md
- ADDED-PATH: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/spec.md
- ADDED-PATH: docs/features/potential/promoted/2026-09-29-engine-toggle-prime-fault-logging-test-races.md

- RAW-DOCS-COMMITTED: 0
- RAW-DOCS-UNTRACKED-IN-FEATURE: 0 (porcelain span over the feature folder with --untracked-files=all and --ignored, because .gitignore ignores trx and cobertura xml names repository-wide)
- Positive control: the ADDED-PATH list contains TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs, so the enumeration saw the committed additions.

## Change footprint (P3-T14)

Timestamp: 2026-09-30T07-54
Command: git diff --name-status 231e1c0b55105aeb626bf5a6e8d0266a567cacad HEAD; git status --porcelain --untracked-files=all
EXIT_CODE: 0

Output Summary:

INHERITED-AND-EXCLUDED:
- A docs/features/potential/promoted/2026-09-29-engine-toggle-prime-fault-logging-test-races.md (the promotion record the amended AC13 exempts; a member of INHERITED-COMMITTED from P0-T4, subtracted by rule D-8)

THIS-ITEM-FOOTPRINT:
- A TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs
- M TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs
- M TaskMaster.Test/TaskMaster.Test.csproj
- M TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/bootstrap-dotnet-coverage.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/bootstrap-nuget-restore.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/bootstrap-sdk.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/bootstrap-tool-restore.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/coordinator-tests-baseline.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/coverage-baseline.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/csharpier-check-baseline.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/file-line-counts-baseline.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/msbuild-analyzer-baseline.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/msbuild-nullable-baseline.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/phase0-commit.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/phase0-instructions-read.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/scope-and-anchor.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/stall-probe.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/qa-gates/csproj-registration.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/qa-gates/harness-hook-edit-scope.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/qa-gates/implementation-commit.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/qa-gates/original-test-unchanged.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/qa-gates/production-reorder-scope.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/regression-testing/build-after-reorder.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/regression-testing/build-before-reorder.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/regression-testing/prime-fault-ordering-fail-before.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/regression-testing/prime-fault-ordering-pass-after.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/issue.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/plan.2026-09-29T23-07.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/research/2026-09-29T23-20-engine-toggle-prime-fault-race-research.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/spec.md

Checks:
- Every THIS-ITEM-FOOTPRINT path is one of the four code paths or lies under the feature folder.
- All four code paths are present; the new partial carries status A.
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs, TaskMaster/Ribbon/RibbonController.EngineCommands.cs, TaskMaster.runsettings and scripts/vscode/TaskMaster.cli.runsettings are absent from the footprint.
- No diff path is outside the code paths, the feature folder and the promotion record: no FOOTPRINT OUTSIDE AC13.
- Porcelain composition (stated without a count): modified and untracked entries under the feature folder (evidence artifacts written or appended after the P2-T8 commit, and this plan file), and modified and untracked entries under .claude/agent-memory/ (uncommitted session memory, all members of PRE-EXISTING-WORKTREE-PATHS from P0-T4, never staged). No porcelain line names a path under TaskMaster/ or TaskMaster.Test/.
