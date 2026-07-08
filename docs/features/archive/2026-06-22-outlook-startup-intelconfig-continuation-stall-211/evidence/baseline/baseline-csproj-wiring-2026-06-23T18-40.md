# Baseline — csproj Wiring (#211 Phase 3.1)

Timestamp: 2026-06-23T18-40
Command: `grep -n "EngineInitTimingProbe.cs" TaskMaster/TaskMaster.csproj` and `grep -n "EngineInitTimingProbeTests.cs" TaskMaster.Test/TaskMaster.Test.csproj` (plus surrounding context reads)
EXIT_CODE: 0

Output Summary:
- Both `TaskMaster/TaskMaster.csproj` and `TaskMaster.Test/TaskMaster.Test.csproj` are legacy non-SDK projects using explicit `<Compile Include>` items (no wildcard/glob include). New `.cs` files MUST be wired with an explicit `<Compile Include>` entry or they will not compile into the assembly.

- `TaskMaster/TaskMaster.csproj` — existing sibling `AppGlobals` compile items (line 402 is the EngineInitTimingProbe anchor):
  - line 401: `<Compile Include="AppGlobals\AppItemEngines.cs" />`
  - line 402: `<Compile Include="AppGlobals\EngineInitTimingProbe.cs" />`  (insertion anchor)
  - line 403: `<Compile Include="AppGlobals\ApplicationGlobals.cs" />`
  - Insertion plan: add `<Compile Include="AppGlobals\StartupDiagnosticsProbe.cs" />` adjacent to line 402.

- `TaskMaster.Test/TaskMaster.Test.csproj` — existing sibling `AppGlobals` test compile items (line 265 is the EngineInitTimingProbeTests anchor):
  - line 264: `<Compile Include="AppGlobals\ContinuationProbeSequenceTests.cs" />`
  - line 265: `<Compile Include="AppGlobals\EngineInitTimingProbeTests.cs" />`  (insertion anchor)
  - line 266: `<Compile Include="AppGlobals\StartupTimingRecorderTests.cs" />`
  - Insertion plan: add `<Compile Include="AppGlobals\StartupDiagnosticsProbeTests.cs" />` adjacent to line 265.

Determination: Explicit-include style confirmed for both csproj files; insertion anchors identified.
