# Baseline NuGet Restore (pre-fix, Issue #354)

Timestamp: 2026-07-18T14:10:37Z

Command: `nuget restore TaskMaster.sln` (run from repo root on branch `bug/stale-app-config-binding-redirects-354`, pre-fix state, after `taskkill //F //IM MSBuild.exe //T` and `taskkill //F //IM VBCSCompiler.exe //T`)

EXIT_CODE: 0

Output Summary:
- Both taskkill commands reported "process not found" (safe no-op; no lingering build-server processes).
- MSBuild auto-detection used version 18.8.2.30814 from `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin`.
- "All packages listed in packages.config are already installed." — no packages needed to be downloaded.
- One pre-existing advisory: `NU1902: Package 'AngleSharp' 1.4.0 has a known moderate severity vulnerability` (not related to this fix; not a restore failure).
- Restore completed successfully with exit code 0.
