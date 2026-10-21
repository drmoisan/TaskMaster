# [P7-T11] Scope-Boundary Check

Timestamp: 2026-09-08T10-29
Command: `git diff --name-status origin/main` paired with `git status --porcelain --untracked-files=all`
EXIT_CODE: 0
Output Summary: The union of the two spans is 85 paths. Every one of them is either a Write Set member or an inherited path under clause A or clause B. Nothing escaped.

OUT-OF-WRITE-SET: NONE

The porcelain span is paired with the name-listing diff because a name-listing diff cannot report a path that is still untracked, and 54 of the 85 paths are evidence artifacts most of which are untracked at the time of this check.

## CHANGED-PATHS

The union contains 85 paths. 54 of them are evidence artifacts under `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/`, each named on a task line of this plan and therefore a Write Set member under its "Evidence artifacts: every path named on a task line below" clause. They are counted rather than listed individually here; the remaining 31 are listed in full.

### Production source and project files (10)

```
QuickFiler/Controllers/QfcFormController.Deactivate.cs
QuickFiler/Controllers/QfcFormController.EventHandlers.cs
QuickFiler/Controllers/QfcFormController.SetupDisposal.cs
QuickFiler/Controllers/QfcHomeController.cs
QuickFiler/Controllers/QfcItemController.EventHandlers.cs
QuickFiler/QuickFiler.csproj
QuickFiler/Viewers/BreadcrumbDropDownHost.cs
QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs
QuickFiler/Viewers/QfcFormViewer.cs
```

All ten are Write Set production members. `BreadcrumbPopupOwnerRegistry.cs` is the new file the Write Set marks as such.

### Test source and project files (6)

```
QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs
QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs
QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs
```

All six are Write Set test members. `BreadcrumbPopupOwnerRegistryTests.cs` is the new file the Write Set marks as such.

### Feature documents (4)

```
docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/issue.md
docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/plan.2026-09-07T21-59.md
docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/research/research.2026-09-07T22-10.md
docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/spec.md
```

Three are Write Set document members and all four are also clause-A inherited.

### Agent-memory paths (11)

```
.claude/agent-memory/atomic-executor/MEMORY.md
.claude/agent-memory/atomic-executor/project_tool_results_inject_bash_read_edit_instruction.md
.claude/agent-memory/atomic-planner/MEMORY.md
.claude/agent-memory/atomic-planner/project_810_teardown_dropdown_residuals_plan_seams.md
.claude/agent-memory/orchestrator/MEMORY.md
.claude/agent-memory/orchestrator/get-blastradius-overincludes-citations-omits-gitignored-writes.md
.claude/agent-memory/orchestrator/new-active-feature-folder-date-prefix.md
.claude/agent-memory/orchestrator/preparation-child-cwd-is-session-root-not-item-worktree.md
.claude/agent-memory/orchestrator/worktree-isolation-blocks-pwsh-per-agent-type.md
.claude/agent-memory/task-researcher/MEMORY.md
.claude/agent-memory/task-researcher/project_qfc810_teardown_dropdown_residuals.md
```

## INHERITED-PATHS

16 paths satisfy at least one inherited clause, with the clause letters recorded beside each. Clause A is membership in the `INHERITED-CLAUSE-A:` block captured by [P0-T2]; clause B is a path under `.claude/agent-memory/`.

```
.claude/agent-memory/atomic-executor/MEMORY.md [A,B]
.claude/agent-memory/atomic-executor/project_tool_results_inject_bash_read_edit_instruction.md [A,B]
.claude/agent-memory/atomic-planner/MEMORY.md [A,B]
.claude/agent-memory/atomic-planner/project_810_teardown_dropdown_residuals_plan_seams.md [A,B]
.claude/agent-memory/orchestrator/MEMORY.md [A,B]
.claude/agent-memory/orchestrator/get-blastradius-overincludes-citations-omits-gitignored-writes.md [A,B]
.claude/agent-memory/orchestrator/new-active-feature-folder-date-prefix.md [A,B]
.claude/agent-memory/orchestrator/preparation-child-cwd-is-session-root-not-item-worktree.md [A,B]
.claude/agent-memory/orchestrator/worktree-isolation-blocks-pwsh-per-agent-type.md [A,B]
.claude/agent-memory/task-researcher/MEMORY.md [A,B]
.claude/agent-memory/task-researcher/project_qfc810_teardown_dropdown_residuals.md [A,B]
docs/features/active/.../evidence/baseline/phase0-instructions-read.md [A]
docs/features/active/.../issue.md [A]
docs/features/active/.../plan.2026-09-07T21-59.md [A]
docs/features/active/.../research/research.2026-09-07T22-10.md [A]
docs/features/active/.../spec.md [A]
```

Feature-folder paths are abbreviated with `.../` above for line length; each resolves under `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/`.

## The subtraction and why it is not a waiver

`OUT-OF-WRITE-SET` is evaluated over `CHANGED-PATHS` minus `INHERITED-PATHS` minus the Write Set, and the result is empty.

Clause A covers paths already changed relative to `origin/main` before [P0-T1] ran, which no task here touches; an unsubtracted comparison would report all sixteen of them on a tree in which every task behaved correctly. Clause B covers `.claude/agent-memory/`, a tracked tree this plan does not own and into which the executing agent writes its own persistent memory entries during execution; those cannot be enumerated at authoring time, which is why the subtraction is stated as a rule over a path prefix rather than as a list.

Neither clause subtracts any path the Write Set names, so a Write Set path that changed unexpectedly would still be reported. The 16 production and test source files above are all Write Set members and all are accounted for; none is inherited.

Four of the eleven clause-B paths were not in the [P0-T2] clause-A capture at authoring time but are present in that capture as executed, which is the drift the mechanical capture exists to absorb. No agent-memory path outside the captured set appears in this union.

## D8 and D9 fences

UNCHANGED: `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` — absent from the union of both spans (D8, owned by a concurrent run on issue 809).
UNCHANGED: `TaskMaster/ThisAddIn.cs` — absent from the union of both spans (D8).
UNCHANGED: `UtilitiesCS/Threading/UiThread.cs` — absent from the union of both spans (D8).
UNCHANGED: `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` — absent from the union of both spans (D9, the AC2 fence). [P7-T10] independently confirms it byte-unmodified with both a scoped name-listing diff and a scoped porcelain status, each reporting zero lines.

No file under `CLAUDE.md`, `.claude/rules/` or `.github/` appears in the union.
