# QA Gate: Change Footprint Against BASE-SHA (P2-T13)

Timestamp: 2026-10-01T18-11
Task: P2-T13
Command: git diff --name-status 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85; git status --porcelain --untracked-files=all; pwsh feature-folder raw-document scan (git status --porcelain --untracked-files=all --ignored -- FEATURE)
EXIT_CODE: 0

Output Summary:
- DIFF-FOOTPRINT: 29 name-status lines; the code paths are `M TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, `M TaskMaster.Test/TaskMaster.Test.csproj` and `A TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs`; every other line lies under the feature folder.
- PORCELAIN-FOOTPRINT: 14 lines, all under the feature folder; no `??` line under TaskMaster/; no line under TaskMaster.Test/.
- FOREIGN-MEMORY-PATHS: none (no .claude/agent-memory/ path in either list).
- No path under scripts/, config/, artifacts/, docs/features/potential/ or any .claude/ subtree appears in either list.
- FEATURE-STATUS-LINES: 14; RAW-DOCS-IN-FEATURE: 0; NON-MD-IN-FEATURE: 0.
- Supplementary: all 38 tracked or untracked files under the feature folder are Markdown; 0 ignored files are present there (RAW-DOCS-ALL-FEATURE-FILES: 0, NON-MD-ALL-FEATURE-FILES: 0).
- Result: no FOOTPRINT OUTSIDE WRITE SET; three literal clauses diverge for the reason recorded below.

LITERAL-CLAUSE-DIVERGENCE (the plan was written for an uncommitted run; Phases 0 and 1 were committed at 433d5c2e2 and caeb82c40 before this phase):
- Clause "every listed diff path is one of the two code paths, a feature-folder path, or .claude/agent-memory/": the diff also lists `A TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs`. That file is the third Write Set code path, and the diff can see it only because it is now tracked. No path outside the Write Set appears.
- Clause "exactly one `??` line lies under TaskMaster.Test/Ribbon and names the partial": observed 0 such lines, because the partial is committed. The paired diff lists it as `A`, which is the same observation.
- Clause "FEATURE-STATUS-LINES at least 34": observed 14. The 25 Phase 0 and Phase 1 evidence files are committed, and 21 of them are unmodified since, so the porcelain span does not list those 21 (the other 4 appear as ` M` because Phase 2 appended sections to them); the diff lists all 25 as `A`. The positive control (untracked evidence files are visible to the span) is still observed: the span lists the 9 untracked Phase 2 artifacts as `??`. The supplementary scan counted 38 feature-folder files.
- No criterion was weakened; the literal values are recorded and reported to the orchestrator.

PRE-EXISTING-WORKTREE-PATHS (P0-T2) reconciliation: both P0-T2 lines name feature-folder paths (the plan and phase0-instructions-read.md); both are now committed.

## DIFF-FOOTPRINT:

```
A	TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs
M	TaskMaster.Test/TaskMaster.Test.csproj
M	TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/anchor-production-shape.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/anchor-test-side.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/bootstrap-dotnet-coverage.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/bootstrap-nuget-restore.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/bootstrap-sdk.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/bootstrap-tool-restore.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/coordinator-tests-baseline.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/coverage-baseline.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/csharpier-check-baseline.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/file-line-counts-baseline.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/msbuild-analyzer-baseline.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/msbuild-nullable-baseline.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/phase0-instructions-read.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/scope-and-anchor.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/stall-probe.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/csproj-registration.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/production-edit-scope.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/production-format.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/protected-regions-unchanged.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/build-after-fix.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/build-before-fix.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/throwing-sink-fail-before.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/throwing-sink-partial-format.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/throwing-sink-partial-tokens.md
A	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/throwing-sink-pass-after.md
M	docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/plan.2026-10-01T06-45.md
```

(git also printed a line-ending warning for the plan file on stderr; it is not a name-status line.)

## PORCELAIN-FOOTPRINT:

```
 M docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/production-edit-scope.md
 M docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/protected-regions-unchanged.md
 M docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/throwing-sink-partial-tokens.md
 M docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/throwing-sink-pass-after.md
 M docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/plan.2026-10-01T06-45.md
?? docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/coverage-summary.md
?? docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/csharpier-check-final.md
?? docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/csharpier-format.md
?? docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/determinism-tokens.md
?? docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/file-line-counts.md
?? docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/msbuild-analyzer-final.md
?? docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/msbuild-nullable-final.md
?? docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/prime-fault-ordering-identity.md
?? docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/toolchain-final-pass.md
```

## Feature-folder scan

```
FEATURE-STATUS-LINES: 14
RAW-DOCS-IN-FEATURE: 0
NON-MD-IN-FEATURE: 0
FEATURE-FILES-TRACKED-OR-UNTRACKED: 38 IGNORED-IN-FEATURE: 0
RAW-DOCS-ALL-FEATURE-FILES: 0
NON-MD-ALL-FEATURE-FILES: 0
```
