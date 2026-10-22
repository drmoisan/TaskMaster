# P4-T1 — Rebuild From the Reverted Source

Timestamp: 2026-09-17T02-26

Command: `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU "/flp:LogFile=coverage\p4-t1.msbuild.log;Verbosity=normal"`
(MSBuild resolved through vswhere)

EXIT_CODE: 0

CHANNEL: COMMAND

## Output Summary

CSC_OUT_LINES: 2

ZERO_ERRORS_LINES: 1

DLL_ADVANCED: True

DLL_LASTWRITEUTC: 2026-09-17T06-26-11

DLL_LASTWRITELOCAL: 2026-09-17T02-26-11

## Acceptance

All three conditions hold.

- `EXIT_CODE: 0`.
- `CSC_OUT_LINES:` is at least 1 and `DLL_ADVANCED: True`.
- The recorded `LastWriteTimeUtc` is later than the `Timestamp:` of the P3-T4 artifact. P3-T4 is
  stamped `2026-09-17T02-25` in local time; this assembly's write time is `2026-09-17T02-26-11`
  local, equivalently `2026-09-17T06-26-11` UTC. Both spellings are recorded so the comparison
  against the artifact timestamps, which are local, is unambiguous.

## Why this rebuild is mandatory rather than defensive

`vstest.console.exe` never compiles. The assembly on disk before this task was the one P3-T3 built
from the M2-mutated source, in which `RunOnDedicatedWorkerThread` invoked the delegate inline. P3-T4
reverted the source but could not change the assembly. Without this rebuild, P4-T2 would have loaded
the M2 assembly and reported two failures against a source tree that no longer contains the
mutation, and the pass-after evidence for AC6 would have been measured against the wrong binary.

The `DLL_ADVANCED: True` observation is what proves the rebuild actually replaced that assembly
rather than being skipped by MSBuild incrementality.

## Build lock

This task ran inside a held shared build lock for item 900, acquired before the build and released
immediately after it completed.
