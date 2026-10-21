# Phase 2 — Seam test file format and assembly build (P2-T8)

Task: [P2-T8]
Every command below was run from the item worktree root via Set-Location inside one pwsh invocation, each while holding the shared machine build lock for item 743 (acquired immediately before and released immediately after each command). Inner quoting of the plan spans was inverted to single quotes where wrapped; semantics identical. Outlook was closed.

## Command 1 — csharpier format (write-mode)

Timestamp: 2026-09-13T03-02
Command: `pwsh -Command 'dotnet tool run csharpier format QuickFiler.Test\Controllers\QfcItemController.SeamMarshallingTests.cs'`
EXIT_CODE: 0
Output Summary:
- `Formatted 1 files in 937ms.` (a processed count, not a changed count)
- Observation beyond the exit code: the formatter reflowed the file from 296 authored lines to 295 (one multi-line `new List<Label> { ... }` initializer collapsed); no semantic change.

## Command 2 — csharpier check (read-only)

Timestamp: 2026-09-13T03-02
Command: `pwsh -Command 'dotnet tool run csharpier check QuickFiler.Test\Controllers\QfcItemController.SeamMarshallingTests.cs'`
EXIT_CODE: 0
Output Summary:
- `Checked 1 files in 358ms.`; the file was not reported as unformatted.

## Command 3 — assembly build (not a gate)

Timestamp: 2026-09-13T03-03
Command: `pwsh -Command '& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"'` (Command Reference tool resolution prepended; console output redirected to the ignored path `coverage\p2-t8-build.log`)
EXIT_CODE: 0
Output Summary:
- `Build succeeded.` / `    0 Warning(s)` / `    0 Error(s)` / `Time Elapsed 00:00:03.52`
- This is an incremental `/t:Build`; the log shows `CoreCompile` executed (not skipped) for project 11, `QuickFiler.Test -> ...\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, and the assembly's LastWriteTime is 2026-09-13T03:03:08, so the new test file (P2-T7 Compile entry) was compiled into the assembly. No analyzer or nullable claim is made from this build.

## Post-format line count

- `QuickFiler.Test\Controllers\QfcItemController.SeamMarshallingTests.cs` = 295 (at most 400 required)

## Observation beyond the exit code — `git status --porcelain --untracked-files=all -- QuickFiler.Test` (verbatim)

```
 M QuickFiler.Test/QuickFiler.Test.csproj
?? QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs
```

(The two fixture files edited in Phase 1 are committed and therefore absent; the new test file is untracked and the csproj carries the P2-T7 Compile entry.)
