# Final QC — Formatting (P7-T1)

Timestamp: 2026-07-18T10-20

Command: pwsh -NoProfile -Command "cd '<worktree>'; & \"$env:USERPROFILE\.dotnet\tools\csharpier.exe\" format ." followed by "... check ."
EXIT_CODE: 0
Output Summary: First `format` pass reformatted 9 files (all files authored by this feature: BreadcrumbBridgeCoordinator.cs, BreadcrumbBridgeCoordinatorTests.cs, IWebViewMessenger.cs, ItemViewer.Breadcrumb.cs, ItemViewer.FolderSearch.cs, WebView2Messenger.cs, BreadcrumbSelectionMapTests.cs, Theme.cs, Theme.Rendering.cs); per loop rules the toolchain restarted from P7-T1. Second `format` pass: 0 additional changes; `csharpier check .` — `Checked 1386 files`, EXIT_CODE 0, zero unformatted files on the final pass. Note: the plan text names `dotnet tool run csharpier .`; the binding orchestrator toolchain override substitutes the direct global-tool executable (manifest location makes `dotnet tool run` fail in this worktree).
