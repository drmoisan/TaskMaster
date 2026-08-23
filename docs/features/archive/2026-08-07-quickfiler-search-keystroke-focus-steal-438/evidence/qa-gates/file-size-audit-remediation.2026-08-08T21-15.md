## [P2-T4] Post-Format File-Size Audit

- Timestamp: 2026-08-08T21-15
- Command: `pwsh -NoProfile -Command "(Get-Content QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs).Count ; exit $LASTEXITCODE"`
- EXIT_CODE: 0
- Output Summary: Line count = 382, within the 500-line ceiling (expected ~380; 345 baseline + 37 added lines). File: `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs`.
