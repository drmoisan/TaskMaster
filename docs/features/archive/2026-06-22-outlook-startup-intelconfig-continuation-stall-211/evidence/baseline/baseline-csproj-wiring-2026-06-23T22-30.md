# Baseline — csproj Wiring (#211 Phase 3.2)

Timestamp: 2026-06-23T22-30
Command: `Select-String -Path TaskMaster.Test/TaskMaster.Test.csproj -Pattern "StartupDiagnosticsProbeTests.cs"`
EXIT_CODE: 0

Output Summary:
- TaskMaster.Test/TaskMaster.Test.csproj line 266: `<Compile Include="AppGlobals\StartupDiagnosticsProbeTests.cs" />`
- The test file is already referenced by an explicit `<Compile Include>` item. This project uses packages.config with explicit Compile items (no glob). No new test file is planned for this increment, so no new `<Compile Include>` is required.
