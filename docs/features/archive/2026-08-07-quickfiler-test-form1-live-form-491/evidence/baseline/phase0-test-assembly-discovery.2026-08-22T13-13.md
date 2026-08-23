Timestamp: 2026-08-22T13-13
Command: pwsh -NoProfile -Command '$all = @(Get-ChildItem -Path . -Recurse -Filter *.Test.dll -File | ForEach-Object { Resolve-Path -Relative $_.FullName } | Where-Object { $_ -like "*\bin\Debug\*" -and $_ -notlike "*\obj\*" -and $_ -notlike "*\ref\*" }); $claude = @($all | Where-Object { $_ -like "*\.claude\*" }); $assemblies = @($all | Where-Object { $_ -notlike "*\.claude\*" }); "PREFILTER={0} CLAUDE={1} KEPT={2}" -f $all.Count, $claude.Count, $assemblies.Count; $assemblies'
EXIT_CODE: 0
Output Summary: PREFILTER=9, CLAUDE=0, KEPT=9. All 9 kept paths contain the segment `bin\Debug`. This run took place after P0-T15/P0-T16 rebuilds, so `bin\Debug` output existed for enumeration.

Kept assembly list:
```
.\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
.\SVGControl.Test\bin\Debug\SVGControl.Test.dll
.\Tags.Test\bin\Debug\Tags.Test.dll
.\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
.\TaskTree.Test\bin\Debug\TaskTree.Test.dll
.\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
.\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
.\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
.\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

The `.claude` count (0) is recorded from the pre-filter list as an observation about this worktree's location, not asserted to be zero as a pass condition (a post-filter zero is guaranteed by the filter itself).
