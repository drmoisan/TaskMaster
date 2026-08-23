## [P0-T3] Target Test File Line Count

- Timestamp: 2026-08-08T20-45
- Command: `pwsh -NoProfile -Command "(Get-Content QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs).Count ; exit $LASTEXITCODE"`
- EXIT_CODE: 0
- Output Summary: Line count = 345, matching D1's stated headroom assumption. File: `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs`.
