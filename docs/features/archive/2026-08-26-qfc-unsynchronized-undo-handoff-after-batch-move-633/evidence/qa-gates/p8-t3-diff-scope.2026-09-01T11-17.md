# Diff scope and change footprint (P8-T3)

Timestamp: 2026-09-01T11-17
Task: [P8-T3]
Working directory: WORKTREE

## Command 1

Command: `git diff --name-only origin/main...HEAD`
EXIT_CODE: 0
Total paths listed: 72.

### Classification of the 72 paths

| Bucket | Count |
|---|---|
| Under `QuickFiler/` | **2** |
| Under `QuickFiler.Test/` | 4 |
| Under `docs/` | 66 |
| Anything else | **0** |

The two paths under `QuickFiler/` are exactly the two named production files:

```
QuickFiler/Controllers/FilerQueue.cs
QuickFiler/Controllers/QfcFormController.EventHandlers.cs
```

The four paths under `QuickFiler.Test/` are exactly the four authorized test and project files:

```
QuickFiler.Test/Controllers/FilerQueueTests.cs
QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs
QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
```

The 66 paths under `docs/` are all inside
`docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/`: this plan file,
`issue.md`, `spec.md`, the research record, and the evidence tree. The first three of those and the
research record were untracked before P2-T7 committed the feature folder, which is why they appear as
added.

Every listed path is therefore under `QuickFiler/`, `QuickFiler.Test/`, or `docs/`. No path lies under
`.claude/agent-memory/`, and `artifacts/orchestration/orchestrator-state.json` does not appear; both are
permitted by the acceptance condition but neither is present in the diff. That file is tracked but
carries `git update-index --skip-worktree`, so it is orchestrator-owned state and was neither staged nor
committed by this execution.

## Command 2

Command: `git status --porcelain`
EXIT_CODE: 0
Verbatim output:

```
 M .claude/agent-memory/orchestrator/MEMORY.md
 M docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/plan.2026-08-31T19-35.md
?? .claude/agent-memory/orchestrator/preimplementation-gate-needs-lifecycle-ready-bool.md
```

The porcelain companion is required because a name-listing diff enumerates tracked changes only and is
blind to untracked files.

The porcelain output contains **no path under `QuickFiler/` or `QuickFiler.Test/`**: all source, test,
and project work is committed. Every other path it lists is either under the feature folder — this plan
file, dirtied by the P8-T2 check-off — or under `.claude/agent-memory/`.

### The two `.claude/agent-memory/` paths

```
.claude/agent-memory/orchestrator/MEMORY.md
.claude/agent-memory/orchestrator/preimplementation-gate-needs-lifecycle-ready-bool.md
```

Neither is production or test source. That directory is tracked in this repository and holds agent
infrastructure: an index file and a single memory note written by the orchestrator during planning. They
do not widen the production diff that AC16 constrains, and a reviewer can confirm from the two names
above that neither is a `.cs`, `.csproj`, `.props`, or `.targets` file. P8-T26 commits them.

An unrestricted "porcelain output is empty" condition would be unsatisfiable at the point this task runs,
which is why the acceptance condition is written with these three exceptions rather than as a bare
clean-tree gate.

Output Summary: The production diff touches no file other than `QuickFiler/Controllers/FilerQueue.cs`
and `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`. The authorized blast radius of six
files was respected exactly: two production files, three test files, one project file. Nothing outside
`QuickFiler/`, `QuickFiler.Test/`, and `docs/` appears in the diff at all, and no file under
`.claude/rules/`, `.claude/skills/`, or `CLAUDE.md` was edited.

This artifact supplies the evidence for the AC16 check-off in P8-T20.
