# P2-T3 — Test Project Build After the Fix

Timestamp: 2026-09-17T02-22

Command: `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU "/flp:LogFile=coverage\p2-t3.msbuild.log;Verbosity=normal"`
(MSBuild resolved through vswhere)

The project-file platform value is `AnyCPU` with no space. The solution builds use
`"/p:Platform=Any CPU"`; the project file defaults `Platform` to `AnyCPU` at line 12 and defines
`OutputPath` only under the four configuration groups `Debug|AnyCPU`, `Release|AnyCPU`, `Debug|x86`
and `Release|x86`, so a project-file build must pass the no-space spelling.

EXIT_CODE: 0

CHANNEL: COMMAND

## Output Summary

CSC_OUT_LINES: 2

ZERO_ERRORS_LINES: 1

DLL_ADVANCED: True

DLL_LASTWRITEUTC: 2026-09-17T06-22-23 (UTC)

## Acceptance

All three conditions hold.

- `EXIT_CODE: 0`.
- `CSC_OUT_LINES:` is at least 1 and `DLL_ADVANCED: True`. Together these prove the edited file was
  actually compiled into `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`. The two observations are
  complementary and neither alone is sufficient: the `/out:obj\Debug\QuickFiler.Test.dll` literal
  in the normal-verbosity log proves `CoreCompile` ran rather than being skipped by MSBuild's
  incremental up-to-date check, and the assembly's `LastWriteTimeUtc` advancing across the command
  proves the compiled output was copied to the location the test runner will load. This matters
  because `/t:Build` is used here rather than `/t:Rebuild`, and `vstest.console.exe` never compiles:
  a build that silently skipped would leave P2-T4 running the pre-fix assembly and reporting a green
  result that says nothing about the change.
- `ZERO_ERRORS_LINES:` is at least 1, the count of the literal ` 0 Error(s)` with its leading space.

## Build lock

This task ran inside a held shared build lock for item 900, acquired before the build and released
immediately after it completed.
