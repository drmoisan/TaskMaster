# QA Gate — Format Apply (Issue #656)

Timestamp: 2026-09-01T14-49
Task: [P4-T1] (toolchain loop pass 1, step 1)

Command:
```
dotnet tool run csharpier format .
git status --porcelain -- QuickFiler QuickFiler.Test
```

EXIT_CODE: 0

Command output: `Formatted 1566 files in 4759ms.`

## Porcelain After Format:

```
 M QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs
 M QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs
```

The output contains exactly the two lines naming
`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` and
`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`, and no other path. Both
carry the ` M` status, meaning modified-in-worktree and not staged. No untracked file and no
additional modified file appears under either production tree.

## Why the tree observation is recorded in addition to the exit code

`csharpier format` is a write-mode command: it exits 0 whether or not it rewrote a file, and its
summary line reports the number of files *checked*, not the number changed. The exit code alone
therefore cannot distinguish a clean run from a repairing one. The porcelain span is the
observation that carries the real signal, and it confirms the change footprint is still the two
authorized files after the formatter ran across the whole tree.

The porcelain span is scoped by pathspec to `QuickFiler` and `QuickFiler.Test` because
`.claude/agent-memory` is tracked in this repository and `artifacts/orchestration/orchestrator-state.json`
is tracked despite the `artifacts/` entry in `.gitignore`. An unscoped porcelain assertion would be
unsatisfiable for reasons unrelated to this item's change set.

Output Summary: Format applied across 1566 files, exit 0. The scoped porcelain output lists exactly
the two authorized files and nothing else.
