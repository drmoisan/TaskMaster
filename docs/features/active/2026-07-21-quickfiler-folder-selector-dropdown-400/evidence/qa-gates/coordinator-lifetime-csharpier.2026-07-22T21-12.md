# Coordinator lifetime CSharpier gate

Timestamp: `2026-07-22T21:12:00-04:00`

Command: `C:\Users\DanMoisan\.dotnet\tools\csharpier.exe format <three P6 batch-A production files and three approved tests>` repeated twice, followed by `csharpier.exe check` over the same six files.

Result: PASS. Both format commands and the final check returned exit code `0`. The final files and SHA-256 hashes were:

- `BreadcrumbCoordinatorUpgradeLifetime.cs`: 309 lines, `BABCE0A23AFEDB38D38F5512D40BD9A82B18F6144522AB06A82FB80C30D888C9`
- `BreadcrumbBridgeCoordinator.cs`: 496 lines, `4C4A61C8F1B908BA26A0057373693A23E60F710EFABDBBF90BB9FB49154DA5C7`
- `ItemViewer.Breadcrumb.cs`: 399 lines, `9AA9E43EAD363D7A6B390754008EDE341099E144B216CF791B566E97BAD56EFB`
- `BreadcrumbCoordinatorLifecycleTests.cs`: 459 lines, `F9BFC5037AEE5B5F0C0306827B6B504AB3E8DBA6E81375874063F8CD0322469B`
- `BreadcrumbCollapsedSurfaceReadinessTests.cs`: 488 lines, `807B9469DB77208B033580C4B88DC312F2F4B46F6FF84DA88A83BAB47C20EE40`
- `BreadcrumbMessengerHubTests.cs`: 414 lines, `0B0EAC5A57FCE56900083B46E0FCE13EDEDB0B67EA1BC33676D3E310AA7EDF11`

All six files remain within the repository's 500-line limit.
