# P5-T10 ToDoModel Duplicate Deletion (D12, PD-13)

Timestamp: 2026-10-03T12-30
Command: CMD-DELETE with PATH ToDoModel/Email Utilities/SortItemsToExistingFolder.cs (five lines substituted, no other change; run by the coordinator through its PowerShell tool under the maintainer bypass of enforce-epic-worktree-removal-gate.ps1, payload unchanged, at HEAD 284a44dc74659a3c30c1d75f58baad9bcb25f094); then git status --porcelain -- ToDoModel ToDoModel.Test; then git diff --name-only 94287369908cc920b21b0e3256314f988ad7d2f5 -- ToDoModel ToDoModel.Test (each issued as git -C WORKTREE, before this task's commit)
EXIT_CODE: 0 (scoped to the CMD-DELETE payload, its process exit code, as reported by the coordinator)
Output Summary: The uncompiled ToDoModel duplicate was deleted (EXISTS-BEFORE True, EXISTS-AFTER False, payload PORCELAIN line shows ` D`); the working-tree porcelain under ToDoModel and ToDoModel.Test is exactly one ` D` row for that file and the anchored name-only diff lists exactly that path. P5-T10 acceptance met.

- HEAD-AT-HANDOFF: 284a44dc74659a3c30c1d75f58baad9bcb25f094

## Substituted payload (as handed to the coordinator)

```
Set-Location -LiteralPath "WORKTREE"
Write-Output ("EXISTS-BEFORE: " + (Test-Path -LiteralPath "ToDoModel/Email Utilities/SortItemsToExistingFolder.cs"))
if (Test-Path -LiteralPath "ToDoModel/Email Utilities/SortItemsToExistingFolder.cs") { Remove-Item -LiteralPath "ToDoModel/Email Utilities/SortItemsToExistingFolder.cs" -Force }
Write-Output ("EXISTS-AFTER: " + (Test-Path -LiteralPath "ToDoModel/Email Utilities/SortItemsToExistingFolder.cs"))
Write-Output ("PORCELAIN: " + (@(git status --porcelain -- "ToDoModel/Email Utilities/SortItemsToExistingFolder.cs") -join " | "))
```

## Channel note

At P5-T10 the executor stopped before the deletion and reported DELETE HANDOFF TO COORDINATOR with HEAD-AT-HANDOFF 284a44dc74659a3c30c1d75f58baad9bcb25f094. The maintainer's bypass of enforce-epic-worktree-removal-gate.ps1 covers the CMD-DELETE payload at P4-T5 and P5-T10 only. The coordinator ran the exact, unchanged P5-T10 CMD-DELETE payload through its PowerShell tool in the item worktree at HEAD 284a44dc74659a3c30c1d75f58baad9bcb25f094, which equals HEAD-AT-HANDOFF. The executor did not run CMD-DELETE in any form. The four lines below are the coordinator's output, transcribed verbatim (form of the P4-T5 ITERATION: 2 record, p4-t5-legacy-deletion.2026-10-03T10-17.md).

## CMD-DELETE output (coordinator-run)

- EXISTS-BEFORE: True
- EXISTS-AFTER: False
- PORCELAIN:  D "ToDoModel/Email Utilities/SortItemsToExistingFolder.cs"
- EXIT_CODE of the payload: 0

## Porcelain (git status --porcelain -- ToDoModel ToDoModel.Test)

```
 D "ToDoModel/Email Utilities/SortItemsToExistingFolder.cs"
```

## Name-only diff (git diff --name-only 94287369908cc920b21b0e3256314f988ad7d2f5 -- ToDoModel ToDoModel.Test)

```
ToDoModel/Email Utilities/SortItemsToExistingFolder.cs
```

## Acceptance (P5-T10, all three required)

1. EXISTS-BEFORE: True and EXISTS-AFTER: False: met.
2. The porcelain output is exactly one ` D` row for the deleted file and the name-only diff lists exactly that path (ToDoModel.csproj and both ToDoModel.Test source files unchanged): met.
3. The PORCELAIN: line of the payload shows ` D`: met.

HEAD-AT-HANDOFF equals the HEAD named in the channel note, so DELETE CHANNEL REFUSED does not apply.
