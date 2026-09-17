# P5-T4 — Nullable and Type-Check Gate (loop iteration 2)

Timestamp: 2026-09-17T02-35

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\p5-t4.msbuild.log;Verbosity=normal"`
(MSBuild resolved through vswhere)

No `/p:Nullable=enable` and no `/t:Build`.

EXIT_CODE: 0

CHANNEL: COMMAND

## Output Summary

WARNINGS: 0

ERRORS: 0

ZERO_ERRORS_LINES: 1

CSC_OUT_LINES: 2

FILE-DIAGNOSTIC-LINES: 0

TEST-DLL-EXISTS: True

## Acceptance (D-8, baseline-relative)

All four conditions hold.

- `EXIT_CODE:` is 0 and equals `NULLABLE-BASELINE-EXIT:` from P0-T8, which is 0.
- `ERRORS:` is 0 and equals the P0-T8 `ERRORS:` value, which is 0.
- `FILE-DIAGNOSTIC-LINES: 0`.
- `CSC_OUT_LINES:` is 2, at least 1.

The rewritten test file introduces no nullable or type-check diagnostic. The values are identical to
iteration 1.

## Note on the assembly this gate leaves behind

This gate rebuilds the whole solution, so the `QuickFiler.Test.dll` that the P5-T5 coverage run
loads is the one produced here, not the one produced by P4-T1. Both are built from the same
committed source, which P3-T4 verified byte-identical to the fix commit, so the two runs observe the
same code.

## Loop context

Iteration 2. See `evidence/other/p5-t5-iteration-1-environmental-failure.2026-09-17T02-32.md`.

## Build lock

This task ran inside a held shared build lock for item 900.
