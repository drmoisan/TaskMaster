# Scope and Anchor (P0-T2)

Timestamp: 2026-10-01T17-37
Task: P0-T2
Command: git rev-parse HEAD; git rev-parse --abbrev-ref HEAD; git diff --exit-code BASE-SHA -- TaskMaster TaskMaster.Test; git status --porcelain --untracked-files=all; pwsh -NoProfile -Command ISSUE-SHAPE payload (plan P0-T2)
EXIT_CODE: 0

Output Summary:
- BASE-SHA: 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85
- BRANCH: bug/engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947 (matches)
- Anchored diff `git diff --exit-code BASE-SHA -- TaskMaster TaskMaster.Test`: exit 0 (code trees equal BASE-SHA)
- Porcelain: no line names a path under TaskMaster/ or TaskMaster.Test/
- HEADINGS ac=45 logs=55
- WORK_MODE_LINES=1
- SCOPE_CONSOLIDATION_HEADINGS=1
- AC_OPEN=7 AC_DONE=0
- SPEC_PRESENT=False USER_STORY_PRESENT=False
- Result: all P0-T2 clauses hold; no stop condition triggered.

Note: BASE-SHA is the merge commit 2e6ce2cab that merged origin/main into this branch immediately before this delegation; that merge touched no Write Set file.

PRE-EXISTING-WORKTREE-PATHS:
```
 M docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/plan.2026-10-01T06-45.md
?? docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/phase0-instructions-read.md
```
(Both lines are this run's own P0-T1 output: the P0-T1 check-off mark and the P0-T1 artifact.)

Write Set code paths:
- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs (modify: edits E1 to E6)
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs (create)
- TaskMaster.Test/TaskMaster.Test.csproj (modify: one compile entry)

Must-not-touch paths (from the plan Write Set section):
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs (AC2: byte-identical)
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs
- TaskMaster/Ribbon/RibbonController.EngineCommands.cs
- TaskMaster/Ribbon/RibbonCommandBoundary.cs
- TaskMaster/TaskMaster.csproj
- TaskMaster.runsettings
- scripts/vscode/TaskMaster.cli.runsettings
- every file under scripts/, .claude/ (including .claude/agent-memory/), config/, artifacts/ and docs/features/potential/

## PHASE0-ARTIFACTS:

Appended by P0-T15 at 2026-10-01T17-43. Listing of FEATURE/evidence/baseline/ (FILE_COUNT: 15; no other file present):

| Artifact | Timestamp | Command | EXIT_CODE | ExpectedExitCode | Output Summary |
|---|---|---|---|---|---|
| anchor-production-shape.md | present | present | 0 | n/a | present |
| anchor-test-side.md | present | present | 0 | n/a | present |
| bootstrap-dotnet-coverage.md | present | present | 0 | n/a | present |
| bootstrap-nuget-restore.md | present | present | 0 | n/a | present |
| bootstrap-sdk.md | present | present | 0 | n/a | present |
| bootstrap-tool-restore.md | present | present | 0 | n/a | present |
| coordinator-tests-baseline.md | present | present | 0 | n/a | present |
| coverage-baseline.md | present | present | 0 | n/a | present |
| csharpier-check-baseline.md | present | present | 0 | n/a | present |
| file-line-counts-baseline.md | present | present | 0 | n/a | present |
| msbuild-analyzer-baseline.md | present | present | 0 | n/a | present |
| msbuild-nullable-baseline.md | present | present | 0 | n/a | present |
| phase0-instructions-read.md | present | not command-bearing | n/a | n/a | present (Policy Order: present) |
| scope-and-anchor.md | present | present | 0 | n/a | present |
| stall-probe.md | present | present | 1 | 1 | present |

Result: all fifteen Phase 0 artifacts of the Write Set are present; every command-bearing artifact carries Timestamp, Command, EXIT_CODE and Output Summary; the one non-zero EXIT_CODE (stall-probe.md, 1) carries ExpectedExitCode 1. No PHASE 0 EVIDENCE INCOMPLETE condition.
