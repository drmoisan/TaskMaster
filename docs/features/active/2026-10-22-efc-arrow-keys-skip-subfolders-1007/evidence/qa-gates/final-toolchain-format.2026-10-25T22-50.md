# Final Toolchain: Format (Issue #1007, P19-T1)

Timestamp: 2026-10-25T22-50
Command: git status --porcelain; dotnet tool run csharpier format .; git status --porcelain; dotnet tool run csharpier check . (launched from a helper under SCRATCH with Set-Location to the worktree)
EXIT_CODE: 0
Output Summary: `csharpier format .` exited 0 (`Formatted 1691 files in 1532ms.`). The porcelain capture before the format and the capture after it are identical (62 lines each), so the format rewrote nothing. `csharpier check .` exited 0 with `Checked 1691 files in 5424ms.`. No file outside the write-set fence was touched. CSharpier 1.2.6 through the tool manifest.

Porcelain capture (identical before and after the format; 62 lines in each):
- 41 tracked modifications (` M`): the 21 PROD-MODIFY files (`QuickFiler/Resources/FolderBreadcrumb.js` included), the 14 Disposition Register test and script files, the 4 legacy project files, the browser harness, and the plan file;
- 21 untracked entries (`??`): the 5 PROD-CREATE files, the 12 TEST-CREATE files, and 4 untracked evidence entries under `<FEATURE>/evidence/` (git collapses the untracked evidence directories).
The full path list is the P18-T1 scope-fences artifact (scope-fences.2026-10-25T22-44.md). No path was added between that artifact and this step other than Markdown evidence under `<FEATURE>/evidence/`.
