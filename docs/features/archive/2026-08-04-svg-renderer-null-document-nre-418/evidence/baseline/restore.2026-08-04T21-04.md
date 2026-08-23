# [P0-T5] Baseline Restore State — re-capture on VSTO-enabled host

Timestamp: 2026-08-04T21-04

Issue: #418
Plan: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md`
Task: `[P0-T5]`
Branch: `bug/svg-renderer-null-document-nre-418`
HEAD: `a5695656e711f98a8ae6ad334115c0f8666c509f`
Base: `ce0c91e6` (PR #419 repository-wide NuGet package update)

## Command

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"
```

EXIT_CODE: 0

## Output Summary

Restore succeeded. `Build succeeded. 0 Warning(s) 0 Error(s)`. Elapsed 00:00:01.15.

- MSBuild used: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`,
  version `18.8.2+ce25c0108 for .NET Framework`.
- Solution configuration validated as `Debug|Any CPU`.
- `_GetAllRestoreProjectPathItems` determined the projects to restore; the `Restore` target completed
  without a package-resolution warning or error.
- Package-resolution warning text: **none**. No `NU1603`, `NU1605`, `NU1701`, or missing-package
  diagnostic was emitted.
- The only network activity was NuGet vulnerability-index retrieval
  (`api.nuget.org/v3/vulnerabilities/index.json`, plus the `2026.08.04.11.53.37` base and update
  vulnerability manifests), all returning `OK`. No vulnerability warning was raised for any pin.
- The short elapsed time reflects that `packages/` was already populated for the current
  post-`ce0c91e6` pin set; restore was a no-op confirmation rather than a download pass.

## Note on tree state at baseline capture

This baseline is captured at HEAD `a5695656`, which already contains the Phase 1 prerequisite
commits from the prior host (`0162567d docs(418): add feature folder and wire SVGControl.Test into
solution`). Consequently `SVGControl.Test` is already a member of `TaskMaster.sln` at the time of
this restore. This is recorded, not corrected — see
`svgcontrol-test-buildability.2026-08-04T21-04.md` for the full divergence account.
