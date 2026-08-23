Timestamp: 2026-08-10T22-31

Command: `pwsh -NoProfile -File "docs\features\active\2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394\evidence\baseline\duplicate-sweep.ps1"`

The sweep script (`duplicate-sweep.ps1`, stored alongside this artifact for reproducibility) XML-parses `UtilitiesCS.Test\UtilitiesCS.Test.csproj` and groups `Include` attribute values by item type (`Compile`, `EmbeddedResource`, `None`, `Reference`, `ProjectReference`, `BootstrapperPackage`, `Analyzer`, `AdditionalFiles`), and separately parses `UtilitiesCS.Test\packages.config` and groups `<package>` `id` attribute values.

EXIT_CODE: 0

Raw output:
```
ItemType=Compile Total=452 DuplicateIncludeValues=2
  DUPLICATE: 'OutlookObjects\Folder\PercentageFormatterTests.cs' x2
ItemType=EmbeddedResource Total=1 DuplicateIncludeValues=0
ItemType=None Total=7 DuplicateIncludeValues=0
ItemType=Reference Total=126 DuplicateIncludeValues=0
ItemType=ProjectReference Total=2 DuplicateIncludeValues=0
ItemType=BootstrapperPackage Total=2 DuplicateIncludeValues=0
ItemType=Analyzer Total=11 DuplicateIncludeValues=0
ItemType=AdditionalFiles Total=1 DuplicateIncludeValues=0
packages.config Total=105 DuplicateIds=0
```

Output Summary: The sweep confirms exactly one duplicate `Include` value in the entire project file: `Compile` / `OutlookObjects\Folder\PercentageFormatterTests.cs`, appearing twice (lines 304 and 356, per P0-T7). Zero duplicates were found in every other item type (`EmbeddedResource`, `None`, `Reference`, `ProjectReference`, `BootstrapperPackage`, `Analyzer`, `AdditionalFiles`) and zero duplicate `id` values in `packages.config`.

Note on the `DuplicateIncludeValues=2` label for `Compile`: this is a PowerShell array-unwrapping artifact, not a second duplicate group. When `Where-Object` returns exactly one matching `Group-Object` result, PowerShell unwraps the single-element array to a scalar `GroupInfo` object, so `.Count` on the (would-be) array instead returns that group's own `.Count` property — the occurrence count of the one duplicate value (2), not the number of distinct duplicate values (1). The enumerated `DUPLICATE:` lines are the authoritative per-group finding and confirm exactly one duplicate `Include` value in `Compile` and none elsewhere; `Total=452` for `Compile` matches the plan's stated baseline `<Compile Include=` count. Verified independently: for item types with zero duplicates, PowerShell's `$null.Count` evaluates to `0`, which is why those lines correctly read `DuplicateIncludeValues=0`.

This confirms the plan's acceptance criterion for P0-T8: exactly one duplicate (`Compile` / `PercentageFormatterTests.cs`) and zero duplicates in every other item type and in `packages.config`.
