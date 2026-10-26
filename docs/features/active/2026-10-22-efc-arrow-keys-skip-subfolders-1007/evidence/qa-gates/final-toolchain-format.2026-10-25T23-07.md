# Final Toolchain: Format (Issue #1007, P19-T1, clean pass)

Timestamp: 2026-10-25T23-07
Command: git status --porcelain; dotnet tool run csharpier format .; git status --porcelain; dotnet tool run csharpier check . (launched from a helper under SCRATCH with Set-Location to the worktree)
EXIT_CODE: 0
Output Summary: `csharpier format .` exited 0 (`Formatted 1691 files in 1455ms.`). The porcelain capture before the format and the capture after it are identical (62 lines each: 41 tracked modifications and 21 untracked entries), so the format rewrote nothing. `csharpier check .` exited 0 with `Checked 1691 files in 4640ms.`. No file outside the write-set fence was touched. CSharpier 1.2.6 through the tool manifest.

Loop history: the 23-02 to 23-04 pass was clean in all four steps, but its P19-T5 comparison found one changed-line shortfall in `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` (52 of 55 changed lines covered against 61 of 61 replaced lines). The fix is in two parts:
- a behaviour-preserving restructure of `CollapseRowLeaf`: the refused-toggle guard became a positive `if` block, which removes two unreachable statements;
- the test `ArrowRight_OnRowCollapsedAwayFromActiveSegment_RestoresChain` in `BreadcrumbBridgeRouterIssue1007Tests.DoubleClick.cs`, which reaches the R3 non-anchored ReExpand path through a `segmentActivate` click after a segment double-click.
The loop restarted here. This file and the 23-07 and 23-09 artifacts form the clean pass of record.
