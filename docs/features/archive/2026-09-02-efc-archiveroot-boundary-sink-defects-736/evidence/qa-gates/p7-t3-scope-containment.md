# P7-T3 — Scope containment against the merge base

Timestamp: 2026-09-04T02-33

Command:

```
git add -A
git status --porcelain
git diff --name-only origin/main...HEAD -- . ":(exclude)docs/**" ":(exclude).claude/**"
```

EXIT_CODE: 0

## Anchored name-only diff

```
QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs
QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs
QuickFiler.Test/Controllers/EfcFormControllerTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/Controllers/EfcDataModel.cs
QuickFiler/Controllers/EfcFormController.cs
TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootComGuardTests.cs
TaskMaster.Test/TaskMaster.Test.csproj
TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs
TaskMaster/AppGlobals/AppOlObjects.cs
TaskMaster/TaskMaster.csproj
```

**Eleven paths. Sorted, this list is byte-equal to the sorted eleven-path ratified Write Set** stated
at the top of the plan and in spec.md's `## Write Set` section, row for row:

| # | Write Set path (sorted) | Present in the diff |
|---|---|---|
| 1 | `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` | yes |
| 2 | `QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs` | yes |
| 3 | `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | yes |
| 4 | `QuickFiler.Test/QuickFiler.Test.csproj` | yes |
| 5 | `QuickFiler/Controllers/EfcDataModel.cs` | yes |
| 6 | `QuickFiler/Controllers/EfcFormController.cs` | yes |
| 7 | `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootComGuardTests.cs` | yes |
| 8 | `TaskMaster.Test/TaskMaster.Test.csproj` | yes |
| 9 | `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` | yes |
| 10 | `TaskMaster/AppGlobals/AppOlObjects.cs` | yes |
| 11 | `TaskMaster/TaskMaster.csproj` | yes |

No extra path appears and no Write Set path is missing.

## `git status --porcelain` span

```
A  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/other/p7-t2-commit.md
M  docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/plan.2026-09-02T12-02.md
```

**Every line names a path under this feature folder.** No line names a path under `TaskMaster/`,
`TaskMaster.Test/`, `QuickFiler/`, or `QuickFiler.Test/`, and no line names a path under
`.claude/agent-memory/` either, though such a line would have been permitted.

The porcelain span cannot be asserted empty here, and an earlier draft that asserted it empty was
unsatisfiable. One category of path is necessarily present when this command runs: the evidence
artifact P7-T2 wrote after its own commit, which is by construction uncommitted — that is the first
line above. The plan file's checkbox update is the second. A second category may be present without
being required: a `.claude/agent-memory/` path an agent wrote during this run after P7-T2's commit;
none is present here. The agent-memory edits this run's earlier agents wrote were committed to this
branch before Phase 0, so they do not appear in this span at all, which is why the clause is written
as a universal over whatever lines the span prints rather than as an expectation that any particular
line is present.

**The assertion that carries the actual weight is the negative one** — no path under the four code
trees appears — because that is what proves every code change this item made reached the commit the
anchored diff reads, which is the only property the empty-span form was ever standing in for.

## Why the documentation and Claude trees are excluded by pathspec

The pathspec carries `":(exclude)docs/**"` and `":(exclude).claude/**"` because **both trees are
tracked and both legitimately receive this item's own artifacts**: `docs/features/active/...` holds
every evidence artifact and this plan file, and `.claude/agent-memory/` holds the memory files this
run's agents wrote. An unscoped diff would report all of them as scope violations they are not. The
`.claude/agent-memory/` paths removed from view here are each named individually in the P7-T2
artifact, so nothing is silently dropped.

Output Summary: the anchored merge-base diff, excluding the documentation and Claude trees, names
exactly the eleven ratified Write Set paths and nothing else. The `git status --porcelain` span
carries two lines, both under this feature folder, and no line names a path under any of the four
code trees, which establishes that no uncommitted code edit is hiding a change from the anchored
diff.
