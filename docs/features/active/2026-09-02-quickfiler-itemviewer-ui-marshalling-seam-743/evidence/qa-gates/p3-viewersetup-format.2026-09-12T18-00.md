# Phase 3 — ViewerSetup format and size check (P3-T4)

Task: [P3-T4]
Every command below was run from the item worktree root via Set-Location inside one pwsh invocation; the csharpier commands were each run while holding the shared machine build lock for item 743 (acquired immediately before and released immediately after each command). Inner quoting of the plan spans was inverted to single quotes where wrapped; semantics identical.

The format/check pair was run twice. The first pass (03:06-03:07) measured 480 lines, exactly at the ceiling; the four-line explanatory comment above the P3-T2 null block (the executor's own wording, not a plan-mandated literal) was then trimmed to two lines so the concurrently-editing sibling item keeps headroom, and the pair was re-run. The P3-T1, P3-T2 and P3-T3 gate literals were re-counted after the trim and are unchanged (`dispatcher is null` = 1). The figures below are from the final pass.

## Command 1 — csharpier format (write-mode)

Timestamp: 2026-09-13T03-15
Command: `pwsh -Command 'dotnet tool run csharpier format QuickFiler\Controllers\QfcItemController.ViewerSetup.cs'`
EXIT_CODE: 0
Output Summary:
- `Formatted 1 files in 937ms.` (a processed count, not a changed count)
- Observation beyond the exit code: `git diff -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` against the Phase 2 commit shows only the P3-T1 member replacements (two lines), the P3-T2 marshal conversion (three-line marshal replaced by the two-line comment, `var dispatcher = _uiDispatcher;`, the `if (dispatcher is null)` block and the one-line `await dispatcher.InvokeAsync(() => AssignControls(itemInfo, viewerPosition));`) and the four-line P3-T3 comment; the formatter reflowed nothing else and kept the replacement marshal on one line.

## Command 2 — csharpier check (read-only)

Timestamp: 2026-09-13T03-15
Command: `pwsh -Command 'dotnet tool run csharpier check QuickFiler\Controllers\QfcItemController.ViewerSetup.cs'`
EXIT_CODE: 0
Output Summary:
- `Checked 1 files in 378ms.`; the file was not reported as unformatted.

## Command 3 — line count

Timestamp: 2026-09-13T03-15
Command: `pwsh -Command '(Get-Content QuickFiler\Controllers\QfcItemController.ViewerSetup.cs).Count'`
EXIT_CODE: 0
Output Summary:
- `478` (was 467 at P0-T8; at most 480 required; 22 of headroom to the 500-line repository limit, 2 to the plan ceiling)

## Observation beyond the exit code — `git status --porcelain -- QuickFiler` (verbatim)

```
 M QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
```

(The IItemViewer.cs and ItemViewer.cs edits were committed at the Phase 2 boundary and are therefore absent from the porcelain output.)
