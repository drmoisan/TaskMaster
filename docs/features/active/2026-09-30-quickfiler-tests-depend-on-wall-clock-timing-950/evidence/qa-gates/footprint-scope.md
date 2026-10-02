# Footprint gate (P6-T10)

Timestamp: 2026-10-02T01-24
Command: git -C WORKTREE diff --name-status 34c2ed88cbb009f2f231453db87bc64d45a9bd51 HEAD -- . ":(exclude)docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950"; git -C WORKTREE status --porcelain --untracked-files=all (separate calls). The exclude pathspec removes the FEATURE paths at source; this is the same set the plan derives by filtering the unscoped name-status (P4-T7's full list is recorded in FEATURE/evidence/qa-gates/implementation-commit.md).
EXIT_CODE: 0

Output Summary:
Name-status BASE..HEAD outside FEATURE:
M	QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs
M	QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs
M	QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs
M	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
M	QuickFiler/Controllers/QfcDatamodel.cs
A	docs/features/potential/promoted/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing.md

THIS-ITEM-FOOTPRINT: exactly the five CODE5 paths, status M
INHERITED-AND-EXCLUDED: docs/features/potential/promoted/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing.md (status A; listed in P0-T3 INHERITED-COMMITTED)
No path ends in .csproj. No status A outside FEATURE other than the inherited promotion record.

Porcelain (all paths under FEATURE: the plan file and the uncommitted Phase 6 qa-gates artifacts):
 M docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/plan.2026-10-01T07-11.md
?? FEATURE/evidence/qa-gates/coverage-comparison.md, coverage-post-change.md, csharpier-check-final.md, csharpier-format.md, msbuild-analyzer-final.md, msbuild-nullable-final.md, prohibited-constructs.md, toolchain-final-pass.md, wall-clock-tokens.md
No porcelain line names a path under QuickFiler/, QuickFiler.Test/ or scripts/.
