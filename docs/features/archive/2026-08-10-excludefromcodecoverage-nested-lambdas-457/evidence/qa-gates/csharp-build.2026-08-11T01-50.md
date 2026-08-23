# [P3-T6] C# rebuild for post-change coverage collection

Timestamp: 2026-08-11T01-50
Command (as written in the plan): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
Actual invoked form (this environment; `msbuild` is not on PATH and the Bash tool is git-bash, which
mangles MSBuild-style `/switch` arguments into filesystem paths):
`pwsh -NoProfile -Command '& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"'`
The plan's single-quoted-outer / double-quoted-inner quoting discipline is preserved verbatim.
EXIT_CODE: **0**

`/p:Nullable=enable` is deliberately absent per issue #522 and the plan's scope prohibitions. No C#
source is changed by this feature; this step exists solely to guarantee current `*.Test.dll`
assemblies for the `[P3-T7]` re-capture.

## Result

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.74
```

- Errors: **0**
- Warnings: **5** — the same five non-blocking `System.Reactive` `packages.config` advisories recorded
  at `[P0-T10]`, one per `packages.config` project. No new warning.

The 1.74-second elapsed time reflects an incremental build: nothing under any C# project changed
since `[P0-T10]`, so MSBuild's up-to-date check found every output current. That is the correct and
expected outcome for a PowerShell-only feature, and the `*.Test.dll` assemblies from `[P0-T10]` remain
valid and current.

## Restore status

Packages were restored by `[P0-T9]` and nothing in Phases 1 through 3 removed the `packages\` tree.
MSBuild reported no missing references, so the `[P0-T9]` re-run branch was not taken.

## Output Summary

`MSBuild.exe TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exited 0 with 0
errors and the same 5 pre-existing `System.Reactive` advisories as the `[P0-T10]` baseline. Test
assemblies are current. `[P3-T7]`'s precondition is satisfied.
