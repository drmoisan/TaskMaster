---
epic: winforms-testability-refactor
integration_branch: epic/winforms-testability-refactor-integration
created_at: 2026-07-09T20:08:22Z
intent:
  epic_type: enabler
  business_outcome_hypothesis: Making the Tags, TaskTree, and TaskVisualization UI projects unit-testable (>= 80% line coverage each, no live forms) reduces regression escapes in the WinForms/Outlook-Interop UI layer and enables autonomous agentic maintenance of that layer.
  leading_indicators:
    - Per-project line coverage reported by vstest /EnableCodeCoverage rises to >= 80% for Tags, TaskTree, and TaskVisualization.
    - Zero production files over 500 lines remain in the three projects.
    - Unit test runs remain deterministic (no live forms, no popups, no UI thread dependence in tests).
  nfrs:
    - No behavior change to end-user UI flows (refactor preserves observable behavior).
    - Full C# toolchain (csharpier, analyzers, nullable, MSTest) green for every child feature.
    - No [ExcludeFromCodeCoverage] on testable seams; exemptions limited to irreducible WinForms/COM wiring and maintainer-ratified.
features:
  - issue_num: 293
    feature_folder: 2026-07-09-tagcontroller-testability-refactor-293
    depends_on: []
  - issue_num: 296
    feature_folder: 2026-07-09-tasktree-testability-refactor-296
    depends_on: []
  - issue_num: 297
    feature_folder: 2026-07-09-taskvisualization-core-testability-refactor-297
    depends_on: []
  - issue_num: 298
    feature_folder: 2026-07-09-taskvisualization-secondary-testability-298
    depends_on: [297]
---

# Epic: WinForms Testability Refactor (#295)

- Epic issue: https://github.com/drmoisan/TaskMaster/issues/295
- Integration branch: `epic/winforms-testability-refactor-integration`
- Status: Design phase COMPLETE (2026-07-09) — all four children have research, spec, atomic plan, and `PREFLIGHT: ALL CLEAR`; execution awaits maintainer signal.

## Goal

Refactor the three WinForms/Outlook-Interop UI projects — `Tags`, `TaskTree`, and
`TaskVisualization` — so their controller logic is unit-testable without live UI, and
bring each project to at least 80% line coverage.

## Shared Design Pattern (applies to every child)

1. **Viewer interfaces.** For each viewer/form a controller consumes, create an
   interface deriving from `UtilitiesCS.Interfaces.IWinForm.IForm`
   (`ITagViewer` for `TagViewer`, `ITaskTreeForm` for `TaskTreeForm`, `ITaskViewer`
   for `TaskViewer`, `IEditFilterViewer` for `EditFilterViewer`,
   `IManageFiltersViewer` for `ManageFilters`). The concrete form implements the
   interface; the controller depends only on the interface.
2. **File-size compliance.** Refactor along logical divisions so no production file
   exceeds 500 lines (`TagController.cs` 877, `TaskTreeController.cs` 546,
   `TaskController.cs` 1861 are currently over).
3. **COM/logic separation.** Extract host-neutral business logic into separate files
   from COM/WinForms interaction; minimize methods that mix COM calls with pure logic.
4. **Seams over UI-thread execution.** Use seams (interface seam > injectable
   delegate > adapter, per `.claude/rules/csharp.md`) so unit tests never construct
   live forms or windows and never show popups (a popup requiring human interaction
   is a unit-test-policy violation). Running COM elements on the UI thread is a
   production-only last resort where no seam alternative exists — never in tests.

   **Maintainer-ratified refinement (2026-07-09, last-resort STA controls):**
   In-memory, never-shown WinForms **controls** (e.g., `TableLayoutPanel`, `Label`,
   `Panel`, `CheckBox`) MAY be constructed in unit tests on an STA thread, strictly
   as a LAST RESORT where no seam can isolate the logic. Conditions:
   - (a) Seams remain the required first approach; each STA-bound test documents
     why no seam is feasible for the covered logic.
   - (b) All STA-bound tests live in separate, dedicated test files (suffix
     `*.StaTests.cs`, marked `[STATestClass]`/`[STATestMethod]` or equivalent
     runsettings scoping) so the STA surface is limited to the essential.
   - (c) Never `Show()`/`ShowDialog()`; no message-pump reliance (no `PostMessage`
     round-trip assertions, no `DoEvents`, no timers); all controls disposed per
     test; popups remain a policy violation.
   - (d) `Form`-derived types remain prohibited in tests even when unshown.
5. **Coverage.** MSTest + Moq + FluentAssertions tests bring each project to
   >= 80% line coverage. `TaskTree.Test` must be created (it does not exist).

## Decomposition and Waves

| Wave | Issue | Feature folder | Scope |
|---|---|---|---|
| 0 | #293 | `2026-07-09-tagcontroller-testability-refactor-293` | Tags: `TagController`/`TagViewer` → `ITagViewer`; split 877-line controller; Tags project → 80% |
| 0 | #296 | `2026-07-09-tasktree-testability-refactor-296` | TaskTree: `TaskTreeController`/`TaskTreeForm` → `ITaskTreeForm`; split 546-line controller; create `TaskTree.Test`; project → 80% |
| 0 | #297 | `2026-07-09-taskvisualization-core-testability-refactor-297` | TaskVisualization: `TaskController` (1861 lines) decomposition; `TaskViewer` → `ITaskViewer`; core → 80% |
| 1 | #298 | `2026-07-09-taskvisualization-secondary-testability-298` | TaskVisualization: `EditFilterController`/`EditFilterViewer` → `IEditFilterViewer`; `ManageFilters` → `IManageFiltersViewer`; Flag*/AutoCreate/AutoAssign helpers; project-wide → 80% |

Dependency rationale: #298 depends on #297 because both modify
`TaskVisualization.csproj` and `TaskVisualization.Test`; serializing them avoids
integration-branch merge conflicts. #293, #296, #297 touch disjoint projects and run
in parallel in wave 0.

## Non-Goals

- No behavior or UX changes to the forms themselves.
- No migration off WinForms/VSTO (separate No-COM architecture effort).
- No new production dependencies.

## Design-Phase Deliverables (per child, before execution)

1. Research artifact under `<feature-folder>/research/` (member-level interface
   inventory, seam signatures, file decomposition, caller updates, per-method test plan,
   `## Automation Feasibility` section).
2. Completed `spec.md` (user-story.md is not applicable to these refactor children;
   each spec must state this explicitly).
3. Atomic plan at the canonical `plan.*.md` path in the feature folder.
4. `PREFLIGHT: ALL CLEAR` from atomic-executor preflight-only validation.

Execution (worktrees, integration branch, wave scheduling, PRs) starts only on
maintainer signal, via `epic-orchestrator` per `.claude/skills/epic-orchestrate/SKILL.md`.
