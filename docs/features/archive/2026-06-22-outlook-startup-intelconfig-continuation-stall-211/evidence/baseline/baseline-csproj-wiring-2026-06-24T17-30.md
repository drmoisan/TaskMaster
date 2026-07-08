# Baseline — csproj Wiring (AC10, issue #211)

Timestamp: 2026-06-24T19-06
Command: grep -n "AppOlObjects" TaskMaster/TaskMaster.csproj; grep -c "Compile Include" (both projects); grep glob patterns
EXIT_CODE: 0

Output Summary:
- TaskMaster/TaskMaster.csproj uses explicit `<Compile Include>` items, NO glob:
  - :407 `<Compile Include="AppGlobals\AppOlObjects.cs" />`
  - :408 `<Compile Include="AppGlobals\AppOlObjects.JunkFolders.cs" />`
  - 30 total `<Compile Include>` entries; no `**` glob present.
- TaskMaster.Test/TaskMaster.Test.csproj uses explicit `<Compile Include>` items, NO glob:
  - 27 total `<Compile Include>` entries; no `**` glob present.
- Conclusion: every new .cs file (JunkFolderPathNavigator.cs in TaskMaster, JunkFolderPathNavigatorTests.cs in TaskMaster.Test) requires an explicit `<Compile Include>` entry or it will not compile (P1-T3, P2-T2).
