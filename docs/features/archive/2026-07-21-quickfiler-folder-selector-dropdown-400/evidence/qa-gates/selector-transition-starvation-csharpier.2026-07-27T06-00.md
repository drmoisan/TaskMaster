# P8-T78 selector transition CSharpier gate (restarted)

Commands:

```powershell
csharpier format QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs
csharpier check QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs
```

Both commands exited `0`. The stable format output SHA-256 is `F5DA51EDDBCF6C514679178BD55D65FEA4806AFB46F990FF771D4F655B4E3A2F`; the subsequent check made no change. The file contains 434 physical lines.

The current task diff is limited to `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs`: it removes the scheduler-dependent helper, adds direct reflected router-lock observations, and uses cumulative observations so either of the two post callbacks preserves a lock-held result. No production, configuration, coverage, filter, parallelism, or test-count file was changed.
