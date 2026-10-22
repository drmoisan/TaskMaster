# Scope and Anchor (issue 942)

Timestamp: 2026-09-30T07-23
Task: P0-T2 (creates this file); P0-T4 and P0-T15 append.

## Sources read in full

- docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/spec.md
- docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/issue.md
- docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/research/2026-09-29T23-20-engine-toggle-prime-fault-race-research.md

## Write Set (code paths, verbatim)

- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs (new)
- TaskMaster.Test/TaskMaster.Test.csproj

## Prohibited paths and trees (from the plan's Write Set section)

1. TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs
2. TaskMaster/Ribbon/RibbonController.EngineCommands.cs
3. TaskMaster.runsettings
4. scripts/vscode/TaskMaster.cli.runsettings
5. every file under scripts/vscode/
6. every file under .claude/ except .claude/agent-memory/ (session memory, never staged)
7. every file under config/
8. every file under docs/features/potential/ (including the inherited promotion record docs/features/potential/promoted/2026-09-29-engine-toggle-prime-fault-logging-test-races.md)

## Work mode and acceptance-criteria counts

- issue.md line 12 reads: `- Work Mode: full-bug`
- spec.md acceptance section, counted from the file: 14 lines beginning `- [ ] AC`, 0 lines beginning `- [x] AC`.

## Anchor and pre-change tree state (P0-T4)

Timestamp: 2026-09-30T07-24
Command: git rev-parse HEAD; git merge-base --is-ancestor 231e1c0b55105aeb626bf5a6e8d0266a567cacad HEAD; git merge-base origin/main HEAD; git diff --name-status 231e1c0b55105aeb626bf5a6e8d0266a567cacad HEAD; git status --porcelain --untracked-files=all
EXIT_CODE: 0

Output Summary:
- HEAD-SHA: d5e57c26bfd6dee1fcc3e0bf61356e27cd8ac8b5
- Ancestor check (231e1c0b55105aeb626bf5a6e8d0266a567cacad is an ancestor of HEAD): exit 0
- git merge-base origin/main HEAD printed: 231e1c0b55105aeb626bf5a6e8d0266a567cacad (equals the anchor; no BASE-SHA MISMATCH)

INHERITED-COMMITTED:
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/issue.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/plan.2026-09-29T23-07.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/research/2026-09-29T23-20-engine-toggle-prime-fault-race-research.md
- A docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/spec.md
- A docs/features/potential/promoted/2026-09-29-engine-toggle-prime-fault-logging-test-races.md

Every inherited path is under the feature folder or is exactly the promotion record the amended AC13 exempts. No INHERITED SET EXCEEDS AC13 EXEMPTION.

PRE-EXISTING-WORKTREE-PATHS:
```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/prd-feature/feedback_ac_gates_verify_satisfiability.md
 M .claude/agent-memory/task-researcher/MEMORY.md
 M docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/plan.2026-09-29T23-07.md
?? .claude/agent-memory/atomic-executor/index_artifact_hygiene_and_misc.md
?? .claude/agent-memory/atomic-executor/index_build_toolchain.md
?? .claude/agent-memory/atomic-executor/index_csharp_and_components.md
?? .claude/agent-memory/atomic-executor/index_test_execution_and_coverage.md
?? .claude/agent-memory/atomic-executor/project_code_commit_before_final_format_pass_orphans_rewrites.md
?? .claude/agent-memory/atomic-executor/project_plan_column_widths_may_include_markdown_indent.md
?? .claude/agent-memory/atomic-planner/project_942_prime_fault_report_then_clear_plan_seams.md
?? .claude/agent-memory/task-researcher/project_engine_toggle_prime_fault_log_order_942.md
?? docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/bootstrap-sdk.md
?? docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/phase0-instructions-read.md
?? docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/scope-and-anchor.md
```

No porcelain line names a path under TaskMaster/ or TaskMaster.Test/ (no CODE TREE DIRTY AT ANCHOR).

## PHASE0-ARTIFACTS:

Timestamp: 2026-09-30T07-32 (P0-T15; listing of evidence/baseline/, field check by a line-anchored search for the four schema fields and ExpectedExitCode)

| Artifact (evidence/baseline/) | Task | Command-bearing | Timestamp / Command / EXIT_CODE / Output Summary | EXIT_CODE | ExpectedExitCode |
|---|---|---|---|---|---|
| phase0-instructions-read.md | P0-T1 | no | Timestamp and Policy Order present | n/a | n/a |
| scope-and-anchor.md | P0-T2, P0-T4 | yes (P0-T4 section) | all four present | 0 | not required |
| bootstrap-sdk.md | P0-T3 | yes | all four present | 0 | not required |
| bootstrap-tool-restore.md | P0-T5 | yes | all four present | 0 | not required |
| bootstrap-nuget-restore.md | P0-T6 | yes | all four present | 0 | not required |
| bootstrap-dotnet-coverage.md | P0-T7 | yes | all four present | 0 | not required |
| csharpier-check-baseline.md | P0-T8 | yes | all four present | 0 | not required |
| msbuild-analyzer-baseline.md | P0-T9 | yes | all four present | 0 | not required |
| msbuild-nullable-baseline.md | P0-T10 | yes | all four present | 0 | not required |
| file-line-counts-baseline.md | P0-T11 | yes | all four present | 0 | not required |
| coordinator-tests-baseline.md | P0-T12 | yes | all four present | 0 | not required |
| stall-probe.md | P0-T13 | yes | all four present | 1 | 1 (matches) |
| coverage-baseline.md | P0-T14 | yes | all four present | 0 | not required |

Every artifact named by P0-T1 through P0-T14 exists at its exact path; every non-zero EXIT_CODE carries a matching ExpectedExitCode.
