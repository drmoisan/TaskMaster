# P8-T78 selector transition CSharpier gate

Commands:

```powershell
csharpier format QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs
csharpier check QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs
```

Both commands exited `0`. The format command changed the initial edit to CSharpier output; the subsequent check made no change. The final SHA-256 is `B8439DD93647B0ACC34D275AF323646378EEF218BC6DB3E1B774450502AA21E3` and the file contains 434 physical lines.

`git diff -- QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` shows the task scope only: remove `System.Threading.Tasks` and `AssertRouterAvailable`, add `System.Threading`, reflect `_router` and `_sync` in the existing test, record callback-local `Monitor.IsEntered` observations, and retain the existing post and selection counters. No production, project, configuration, filter, coverage, or test-count file was changed by this task.
