# Phase 0 — Requirement Sources Read and Write Set Recorded (P0-T2)

Timestamp: 2026-09-03T01-13
Task: [P0-T2]
Command: Read tool over `spec.md` (199 lines) and `research/2026-09-02T09-15-ribbon-engine-toggle-defects-research.md` (887 lines), both in full; checkbox counts measured with `^- \[ \] ` and `^- \[x\] ` regex counts over `spec.md`.
EXIT_CODE: 0

## Requirement sources read in full

1. `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/spec.md` — the sole
   acceptance-criteria source for this cycle (Work Mode: full-bug).
2. `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/research/2026-09-02T09-15-ribbon-engine-toggle-defects-research.md`
   — the design record. Its section 0 decision table, sections 2 through 5, section 7 write set and
   section 9 toolchain expectations are binding on this execution.

`issue.md` is NOT an acceptance-criteria source for this cycle. It carries the work-mode marker only.

## Acceptance-criteria checkbox census in spec.md

- Unchecked (`- [ ] `) items: 25
- Checked (`- [x] `) items: 0
- Total: 25

All 25 lie under the `## Acceptance Criteria` heading, partitioned by the spec's four subheadings as
Finding 1 (7), Finding 2 (8), Finding 3 (6), Cross-cutting (4). This matches the plan's AC identity
table exactly: F1-AC1..F1-AC7, F2-AC1..F2-AC8, F3-AC1..F3-AC6, X-AC1..X-AC4.

## Write set — the only ten paths this change may create or modify outside the feature folder

1. `TaskMaster/Ribbon/RibbonExplorer.xml`
2. `TaskMaster/Ribbon/RibbonController.Intelligence.cs`
3. `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`
4. `TaskMaster/Ribbon/SpamManagerResetGate.cs`
5. `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`
6. `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs`
7. `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs`
8. `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs`
9. `TaskMaster/TaskMaster.csproj`
10. `TaskMaster.Test/TaskMaster.Test.csproj`

The plan's P4-T3 branch B contingency is the single authorized extension of this set, and it must be
reported as a scope amendment in its own artifact if taken.

## Prohibited paths — fail closed rather than infer

- `TaskMaster/AppGlobals/AppOlObjects.cs` — PROHIBITED. Owned by a different concurrent work item in
  the same parallel run. Not opened during research and not to be opened here.
- `TaskMaster/AppGlobals/NonBlockingDelay.cs` — PROHIBITED. Same reason.

Two further read-only constraints carried from the plan's write-set section:

- `TaskMaster/Ribbon/RibbonViewer.cs` is read-only for this change. No callback method is added,
  renamed or removed on that type. F1-AC2 depends on this remaining true.
- `TaskMaster.Test/Ribbon/RibbonViewerEngineCallbackShapeTests.cs` is deliberately not modified. The
  two new XML-consistency tests go in `RibbonExplorerXmlTests.cs` instead, at the cost of one
  duplicated ribbon-control type-name constant.

Output Summary: Both requirement sources read in full. The spec's acceptance section holds 25
unchecked checkbox items and zero checked items. The ten write-set paths are recorded above, and
`TaskMaster/AppGlobals/AppOlObjects.cs` and `TaskMaster/AppGlobals/NonBlockingDelay.cs` are recorded
as prohibited.
