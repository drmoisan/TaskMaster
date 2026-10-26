# Final Toolchain: Format (Issue #1007, P19-T1, clean pass)

Timestamp: 2026-10-25T23-02
Command: git status --porcelain; dotnet tool run csharpier format .; git status --porcelain; dotnet tool run csharpier check . (launched from a helper under SCRATCH with Set-Location to the worktree)
EXIT_CODE: 0
Output Summary: `csharpier format .` exited 0 (`Formatted 1691 files in 1810ms.`). The porcelain capture before the format and the capture after it are identical (62 lines each: 41 tracked modifications and 21 untracked entries), so the format rewrote nothing. `csharpier check .` exited 0 with `Checked 1691 files in 4845ms.`. No file outside the write-set fence was touched. CSharpier 1.2.6 through the tool manifest.

Loop history: the pass recorded at 22-50 to 22-52 had a clean format, analyzer and type-check run. Its P19-T5 comparison found two PROD-CREATE files below 90 percent line coverage and a repository branch figure below the baseline. Tests were added only to TEST-CREATE and Disposition Register test files (`BreadcrumbSelectionSessionTreeIssue1007Tests.cs`, `FolderBreadcrumbBridgeRouterIssue1007Tests.cs`, `BreadcrumbBridgeRouterIssue1007Tests.DoubleClick.cs`, `BreadcrumbSelectorMessagesTests.cs`). The loop then restarted here, after one settling `csharpier format .` that laid out the new test text. This file and the 23-03 and 23-04 artifacts form the clean pass.
