# P0-T12 — Analyzer Baseline (Issue #751)

Timestamp: 2026-09-03T14-28

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0

Sanitization: applied. Placeholder tokens used: `<WORKTREE>` and `<USER>`.

MSBuild was resolved through `vswhere` exactly as the plan's Toolchain resolution convention specifies:

```powershell
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
```

The resolved MSBuild is the Visual Studio 18 Community `MSBuild\Current\Bin\MSBuild.exe`. The
`Invoke-VSBuild` helper was not used, because it rewrites `csproj` HintPaths.

## Output Summary

Final summary lines, sanitized and transcribed:

```
    18>Done Building Project "<WORKTREE>\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (Rebuild target(s)).
     1>Done Building Project "<WORKTREE>\TaskMaster.sln" (Rebuild target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:16.32
```

- Warning count: **0**
- Error count: **0**
- Build result: succeeded

## Acceptance

| Required | Observed | Result |
|---|---|---|
| `EXIT_CODE: 0` | 0 | PASS |
| Observed error count recorded | 0 | PASS |

## Notes

- `/t:Rebuild` was used and `/t:Build` was not. MSBuild's up-to-date check does not invalidate on a
  command-line `/p:` change, so a warm `/t:Build` would return exit 0 with `CoreCompile` skipped on every
  project and would run no analyzer. The build above rebuilt all 18 projects, so the analyzers actually ran.
- No `CS0006` or other missing-reference-assembly diagnostic was reported, so the P0-T8 remedy path
  (re-running `nuget restore TaskMaster.sln`) was not required. The `packages/` tree created by P0-T8
  resolved every `HintPath`.
