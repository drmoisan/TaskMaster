# Baseline — csproj Wiring (#211 Phase 3)

Timestamp: 2026-06-23T14-30
Command: `grep -n 'AppItemEngines.cs' TaskMaster/TaskMaster.csproj` ; `grep -n 'ContinuationProbeSequenceTests.cs' TaskMaster.Test/TaskMaster.Test.csproj` ; glob check `grep -n 'Include="**/*.cs"' <both csproj>`
EXIT_CODE: 0

Output Summary:
- Both `TaskMaster/TaskMaster.csproj` and `TaskMaster.Test/TaskMaster.Test.csproj` are legacy non-SDK MSBuild projects (`<Project ToolsVersion="17.0"/"15.0" ... xmlns=".../developer/msbuild/2003">`) using explicit `<Compile Include="...">` items. No wildcard/glob `<Compile Include="**/*.cs" />` exists in either project (glob grep returned no matches).
- TaskMaster.csproj insertion anchor (sibling existing include): line 401 — `<Compile Include="AppGlobals\AppItemEngines.cs" />`. New file `EngineInitTimingProbe.cs` will be added as `<Compile Include="AppGlobals\EngineInitTimingProbe.cs" />` adjacent to this anchor.
- TaskMaster.Test.csproj insertion anchor (sibling existing include): line 264 — `<Compile Include="AppGlobals\ContinuationProbeSequenceTests.cs" />`. New test file `EngineInitTimingProbeTests.cs` will be added as `<Compile Include="AppGlobals\EngineInitTimingProbeTests.cs" />` adjacent to this anchor.

Conclusion: explicit-include style confirmed; explicit `<Compile Include>` entries are required for each new file to compile into its assembly.
