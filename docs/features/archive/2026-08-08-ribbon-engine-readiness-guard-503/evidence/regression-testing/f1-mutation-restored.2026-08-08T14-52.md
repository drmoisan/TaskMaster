# F1 — Temporary Mutation Restored (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P1-T8]
Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; git checkout -- TaskMaster/Ribbon/RibbonExplorer.xml; git status --porcelain -- TaskMaster/Ribbon/RibbonExplorer.xml; (Get-Content 'TaskMaster\Ribbon\RibbonExplorer.xml' | Measure-Object -Line).Lines; (Select-String -Path 'TaskMaster\Ribbon\RibbonExplorer.xml' -Pattern 'getEnabled=\"EngineCommand_GetEnabled\"' -AllMatches | Measure-Object).Count"`
EXIT_CODE: 0

## Why `git checkout --` is the restoration mechanism

It is exact and deterministic. Phase 1 made no other change to `RibbonExplorer.xml`, and Phase 2 has not yet run, so restoring the path from the index reproduces the pre-mutation bytes without any opportunity to introduce a whitespace, ordering, or encoding difference. Hand re-insertion of the deleted line would be a second opportunity for exactly that class of error.

## Output Summary

```text
539
8
EXIT_CODE=0
```

| Measurement | Value | Required |
|---|---|---|
| `git status --porcelain -- TaskMaster/Ribbon/RibbonExplorer.xml` | **(empty — no output line)** | empty |
| Line count | **539** | 539 |
| `getEnabled="EngineCommand_GetEnabled"` occurrences | **8** | 8 |

The porcelain query emitted **no output line** for the path. Its absence between the command invocation and the `539` line count is the empty result: git reports nothing for a path that matches HEAD. The file is byte-identical to its pre-mutation state.

## Mutation window closed

The window opened by P1-T5 and governed by plan section 3 rule 8 is now **closed**. The permanent tree retains no part of the mutation. The Phase 4 commit gate (P4-T3) — which may execute only after this artifact records an empty porcelain for `TaskMaster/Ribbon/RibbonExplorer.xml` — is satisfied by this record.

Binary outcome satisfied on all three conditions: empty porcelain, 539 lines, 8 occurrences.
