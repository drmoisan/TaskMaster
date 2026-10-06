Timestamp: 2026-10-02T05-17
Command: git -C <worktree-root> check-ignore -v -- TaskMaster.sln.bak TaskTree/TaskTree.vbproj.bak TaskVisualization/TaskVisualization.vbproj.bak
EXIT_CODE: 0
Output Summary: Three lines, one per path, each beginning `.gitignore:259:*.bak` (followed by the queried path). The same shape with --no-index printed nothing in P0-T10.
