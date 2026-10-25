# Baseline Toolchain Bootstrap (Issue #1007)

Timestamp: 2026-10-23T13-39
Command: dotnet --version; dotnet tool run csharpier --version; packages.config restore check against packages/; git status --porcelain (helper script under SCRATCH, read-only, no restore re-run)
EXIT_CODE: 0
Output Summary:
- global.json pins SDK 8.0.205; `.dotnet-sdk/sdk` holds 8.0.205; `dotnet --version` reports 8.0.205.
- `dotnet tool run csharpier --version` reports 1.2.6.
- packages/ holds 173 package folders; every package id and version listed in a first-party packages.config resolves to a packages/ folder (missing=0).
- MSBuild resolved through vswhere (Visual Studio 18 Community, MSBuild\Current\Bin\MSBuild.exe); dotnet-coverage resolved from the user tool path; node v24.14.0.
- `git status --porcelain`: no tracked source change. The only entries are the plan checkbox for P0-T1 and the untracked evidence/baseline folder written by Phase 0.
