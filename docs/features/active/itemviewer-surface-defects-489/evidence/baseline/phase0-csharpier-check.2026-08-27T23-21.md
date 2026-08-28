# Phase 0 — CSharpier Format Baseline on the Untouched Worktree (P0-T9)

Timestamp: 2026-08-27T23-21
Command: dotnet tool run csharpier check .
EXIT_CODE: 0
ExpectedExitCode: 0

BaselineUnformattedSet:
(empty — `dotnet tool run csharpier check .` reported no unformatted file)

Output Summary:
- The run reported `Checked 1543 files in 4729ms.` and listed **no** file as unformatted. The
  `BaselineUnformattedSet:` block above is therefore empty, and `ExpectedExitCode:` is recorded as `0`
  per the P0-T9 branch rule (`0` when the reported set is empty, `1` when it is non-empty). The
  observed exit code was `0`, so the artifact normalizes to `pass`.
- **Does `QuickFiler/Viewers/ItemViewer.Designer.cs` appear in the set? No.**
- **Does `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` appear in the set? No.**
  Neither appears, because the set is empty; no file at all was reported.
- The absence of both `.Designer.cs` paths is not merely a consequence of the empty set. Two
  read-only single-file checks were run to establish *why* they are absent:
  `dotnet tool run csharpier check "QuickFiler/Viewers/ItemViewer.Designer.cs"` reported
  `Checked 0 files in 44ms.` with exit `0`, and
  `dotnet tool run csharpier check "QuickFiler/Viewers/ItemViewerExpanded.Designer.cs"` reported
  `Checked 0 files in 45ms.` with exit `0`. A processed-but-already-formatted file would report
  `Checked 1 files`. `Checked 0 files` means CSharpier declined to process either file.
- That distinction is falsifiable and is the whole point of this baseline. `.csharpierignore` is
  14 lines and its eight exclusion patterns are `**/evidence/**`, `*.cobertura.xml`, `*.coverage`,
  `*.coveragexml`, `*.trx`, `*.csproj`, `*.props`, `*.targets`. It does **not** exclude
  `*.Designer.cs`, and there is no `.csharpierrc` anywhere in the repository, so the print width is
  CSharpier's 100-column default. `ItemViewer.Designer.cs:256` measures **111** columns and
  `ItemViewerExpanded.Designer.cs:274` measures **110** columns. Had either file been processed, both
  lines would have been re-wrapped and both files reported as unformatted. They were not, so
  CSharpier 1.2.6 is skipping them through its built-in generated-file detection on the
  `*.Designer.cs` filename.
