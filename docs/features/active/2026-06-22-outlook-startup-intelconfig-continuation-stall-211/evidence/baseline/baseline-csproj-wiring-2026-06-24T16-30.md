# Phase 0 — Baseline csproj Wiring (issue #211)

Timestamp: 2026-06-24T16-30
Command: `grep -nE "StoreWrapperInitClock|StoreWrapperInitProbe|PhaseNetProbeTests|TestableApplicationGlobals\.cs" <4 csprojs>` and `grep -c '<Compile Include=' <4 csprojs>` and `grep 'Compile Include="\*\*'`
EXIT_CODE: 0

Output Summary:
- All four target projects use explicit `<Compile Include="...">` items with NO wildcard/glob include:
  - `UtilitiesCS/UtilitiesCS.csproj`: 403 explicit `<Compile Include>` items.
  - `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: 350 explicit `<Compile Include>` items.
  - `TaskMaster/TaskMaster.csproj`: 30 explicit `<Compile Include>` items.
  - `TaskMaster.Test/TaskMaster.Test.csproj`: 25 explicit `<Compile Include>` items.
  - Wildcard `<Compile Include="**...">` search across all four returned no matches (exit 1 = no glob).
- Planned new files are NOT yet wired (grep for `StoreWrapperInitClock`, `StoreWrapperInitProbe`, `PhaseNetProbeTests`, `TestableApplicationGlobals.cs` returned no matches; exit 1):
  - `StoreWrapperInitClock.cs` — not wired.
  - `StoreWrapperInitProbe.cs` — not wired.
  - `StoreWrapperInitClockTests.cs` — not wired.
  - `StoreWrapperInitProbeTests.cs` — not wired.
  - `PhaseNetProbeTests.cs` — not wired.
  - `TestableApplicationGlobals.cs` (new extracted file) — not wired.

Conclusion: Explicit-include wiring confirmed for all four projects; every new file will require an explicit `<Compile Include>` entry.
