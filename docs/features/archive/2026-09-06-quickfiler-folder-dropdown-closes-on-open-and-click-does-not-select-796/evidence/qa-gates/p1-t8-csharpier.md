# P1-T8 — CSharpier over QuickFiler and QuickFiler.Test

Timestamp: 2026-09-07T14-22
Task: [P1-T8]
Issue: #796
Channel used: A

Commands, in the order run:

1. `pwsh -NoProfile -Command 'git status --porcelain --untracked-files=all'` (before)
2. `pwsh -NoProfile -Command 'dotnet tool run csharpier format QuickFiler QuickFiler.Test; "EXIT_CODE=$LASTEXITCODE"'`
3. `pwsh -NoProfile -Command 'git status --porcelain --untracked-files=all'` (after)
4. `pwsh -NoProfile -Command 'dotnet tool run csharpier check QuickFiler QuickFiler.Test; "EXIT_CODE=$LASTEXITCODE"'`

FORMAT EXIT_CODE: 0 (not the acceptance evidence; see below)
CHECK EXIT_CODE: 0

Format stdout: `Formatted 326 files in 3198ms.`
Check stdout: `Checked 326 files in 2561ms.` with no file listed as unformatted.

## Porcelain capture BEFORE the format run

```
 M QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs
 M QuickFiler.Test/QuickFiler.Test.csproj
 M QuickFiler/Controllers/QfcFormController.Deactivate.cs
 M QuickFiler/Controllers/QfcItemController.EventHandlers.cs
 M QuickFiler/QuickFiler.csproj
 M QuickFiler/Viewers/BreadcrumbDropDownHost.cs
 M <FEATURE>/plan.2026-09-06T21-59.md
?? QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs
?? QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs
?? <FEATURE>/evidence/baseline/p0-t10-quickfiler-test-baseline.md
?? <FEATURE>/evidence/baseline/p0-t11-coverage-baseline.md
?? <FEATURE>/evidence/baseline/p0-t12-file-size-baseline.md
?? <FEATURE>/evidence/baseline/p0-t13-mcp-validator-probe.md
?? <FEATURE>/evidence/baseline/p0-t14-scope-baseline.md
?? <FEATURE>/evidence/baseline/p0-t2-dotnet-sdk-install.md
?? <FEATURE>/evidence/baseline/p0-t3-nuget-restore.md
?? <FEATURE>/evidence/baseline/p0-t4-analyzer-version-skew.md
?? <FEATURE>/evidence/baseline/p0-t5-dotnet-tool-restore.md
?? <FEATURE>/evidence/baseline/p0-t6-dotnet-coverage-probe.md
?? <FEATURE>/evidence/baseline/p0-t7-csharpier-check-baseline.md
?? <FEATURE>/evidence/baseline/p0-t8-analyzer-rebuild-baseline.md
?? <FEATURE>/evidence/baseline/p0-t9-nullable-rebuild-baseline.md
?? <FEATURE>/evidence/baseline/phase0-instructions-read.md
?? <FEATURE>/evidence/qa-gates/p1-t2-host-line-count.md
?? <FEATURE>/evidence/qa-gates/p1-t3-compile-entry.md
?? <FEATURE>/evidence/qa-gates/p1-t6-compile-entry.md
?? <FEATURE>/evidence/qa-gates/p1-t7-deactivate-suite-count.md
```

28 entries. `<FEATURE>` abbreviates
docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796
for width only; the captured text carried the full path.

## Porcelain capture AFTER the format run

Byte-identical to the capture above: the same 28 entries, in the same order, with the
same two-character status codes. No path changed status, none was added, and none was
removed.

## What the two captures do and do not establish

The exit code of the write-mode format invocation is identical on a clean run and on
a repairing one, which is why the before-and-after tree observation is the acceptance
evidence and the format exit code is not.

Stated precisely, because the distinction matters: porcelain status cannot detect a
rewrite of a file that was ALREADY reported as `M`, since such a file stays `M`
whether or not the formatter touched it. Six of the seven `M` entries are Phase 1
edits and were already `M` before the format run. What the identical captures do
establish is that the format run introduced no NEW modified path and left every
untracked path untracked — in particular it did not reformat any file outside the
Phase 1 write set into a modified state.

The complementary evidence for the six already-modified files is the read-only check
invocation in step 4, which returned EXIT_CODE 0 over the same 326 files with no file
listed. A file the formatter had left unformatted would have been listed there.

## Line counts after formatting

| Path | Physical lines |
|---|---|
| QuickFiler/Viewers/BreadcrumbDropDownHost.cs | 485 |
| QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs | 79 |
| QuickFiler/Controllers/QfcFormController.Deactivate.cs | 132 |
| QuickFiler/Controllers/QfcItemController.EventHandlers.cs | 278 |
| QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs | 274 |
| QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs | 49 |

Measured with the idiom recorded on the `LINE-COUNT-IDIOM:` line of
evidence/baseline/p0-t12-file-size-baseline.md. Every file is under the 500-line
ceiling, and BreadcrumbDropDownHost.cs is unchanged at 485, so the P1-T2 band result
survives formatting.

Output Summary: CSharpier check over QuickFiler and QuickFiler.Test returned
EXIT_CODE 0 across 326 files with none listed as unformatted. The porcelain captures
before and after the format run are identical.
