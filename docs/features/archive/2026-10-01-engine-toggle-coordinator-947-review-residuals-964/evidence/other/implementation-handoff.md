# Implementation Handoff (P1-T1)

Timestamp: 2026-10-03T07-42
Task: P1-T1
Plan: docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/plan.2026-10-02T05-20.md (version 1.5)

Delegated executor role: atomic-executor, small-path implementation (Phase 1 executed task by task from the approved plan; no redesign).

Write Set code paths (verbatim):
- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (modify: split, then fix)
- `TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs` (create)
- `TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs` (create)
- `TaskMaster/TaskMaster.csproj` (modify: two compile items)
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` (modify: the `OnNotify` harness member)
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` (create)
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` (modify: remark only, D-7a)
- `TaskMaster.Test/TaskMaster.Test.csproj` (modify: one compile item)

Implementation-completion criteria (all must be met):
- P1-T24 pass-after gate: the coordinator fixture run against the fixed coordinator reports 43 total, 43 passed, 0 failed, every NEW-NAMES-964 and INVARIANT-NAMES entry Passed.
- P1-T22 census gate: every STRIPPED, PHRASE and SPAN-HASH value equals its required final value.
- P1-T25 unchanged-partials gate: the four untouched partials are byte-equal to BASE-SHA; Race.cs has no non-documentation change; the primary fixture removes exactly one line.
- P1-T26 size gate: every production file at most 450 lines, every test partial at most 500 lines, csproj registrations complete.

Edit-route rule (D-11): every `.cs` and `.csproj` change is made with the Edit or Write tool, never through a shell write, so the repository hooks see it. A hook denial of any Edit or Write is reported verbatim and stops that step; the executor does not retry with a rephrased edit or another tool.
