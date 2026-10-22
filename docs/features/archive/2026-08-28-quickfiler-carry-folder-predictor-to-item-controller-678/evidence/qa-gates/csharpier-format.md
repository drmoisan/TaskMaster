# P2-T1 — CSharpier format (apply)

Timestamp: 2026-09-01T22-42

Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0

The command was run unconditionally.

## Output Summary

The run printed exactly one summary line, reproduced verbatim:

```
Formatted 1574 files in 2107ms.
```

**That line does not distinguish a clean run from a repairing one.** CSharpier prints a
**processed**-file count, not a rewritten-file count, so the same sentence shape appears whether it
rewrote every file or none. The exit code is likewise 0 in both cases, because `format` is a
write-mode command. Neither observation is sufficient on its own.

## Tree observation, which does distinguish them

`git status --porcelain` was taken immediately before and immediately after the command.

**Before:**

```
 M docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/plan.2026-08-31T21-12.md
?? docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/
```

**After:**

```
 M docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/plan.2026-08-31T21-12.md
?? docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/
```

The two outputs are **identical**. Both entries are the plan file's own checklist edits and the
untracked evidence directory, both of which predate the command and are unaffected by a C#
formatter.

**Rewritten paths: none.** No path appears in the after-state that was absent from the before-state,
and no tracked `.cs`, `.csproj`, `.props`, `.targets`, `.xml` or `packages.config` file changed
status. The whole tree was already CSharpier-clean when this task ran, because formatting was
applied and verified after each Phase 1 task.

## Restoration clause

The clause requires any path rewritten **outside** the `QuickFiler/` and `QuickFiler.Test/` prefixes
to be restored to its base-ref content with `git checkout <base-ref> --` followed by that path,
because AC23 forbids a change outside those prefixes.

**No restoration was needed or performed.** The clause's trigger is a rewritten path outside those
prefixes; the before-and-after comparison shows no rewritten path at all, inside or outside them.

Consequently the Phase 2 restart rule's carve-out is not engaged either: this task produced **no net
change under `QuickFiler/` or `QuickFiler.Test/`**, and no restored path exists to list.

## Non-vacuity note

The `Formatted 1574 files` count is recorded alongside the tree observation for a second reason: a
run that processed zero files would also exit 0 and would also leave the tree unchanged. The count
of 1574 confirms the command actually walked the tree and, together with the 1574 reported by the
P2-T2 `check` run, confirms both commands saw the same file set.
