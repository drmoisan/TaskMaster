# Baseline — TYPECHECK (the corrected form): pre-change positive control and build-output restoration ([P0-T14])

Timestamp: 2026-08-10T22-59
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo /v:m /fl "/flp:logfile=coverage/baseline-typecheck-rebuild.log;verbosity=normal"`
EXIT_CODE: 0

This is **TYPECHECK**, the corrected type-check command this feature adopts. It is
`.github/workflows/ci.yml`'s command character-for-character modulo the solution token, and it
deliberately omits `/p:Nullable=enable`.

Invoked via `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-typecheck-rebuild.ps1 -LogName baseline-typecheck-rebuild`.

This task serves two purposes: it is the **pre-change positive control**, and it is the **mandatory
build-output restoration** after the failing `/t:Rebuild` in [P0-T13], which issued `Clean` to every
project before aborting.

## Measurements

| Metric | Value | Acceptance |
|---|---|---|
| `EXIT_CODE` | **0** | required 0 — PASS |
| MSBuild summary | **`0 Error(s)`** | required `0 Error(s)` — PASS |
| MSBuild summary | `6 Warning(s)` | not gated |
| `Skipping target "CoreCompile"` count | **0** | required 0 — PASS |
| Node-prefixed `error CS` count | 0 | corroborates `0 Error(s)` |
| Elapsed | 18.4 s | recorded |
| `CoreCompile:` header-line count | 47 | informational only (see below) |

The positive control is **green**. A non-zero exit here would have invalidated the central design
assumption and required a stop-and-report; it did not occur.

## Build-output restoration confirmed

```
$ ls -d */bin/Debug | wc -l
18
$ ls UtilitiesCS/bin/Debug/UtilitiesCS.dll
UtilitiesCS/bin/Debug/UtilitiesCS.dll*
```

All 18 projects have a `bin/Debug` output directory again and `UtilitiesCS.dll` is present. The
`bin`/`obj` deletion caused by [P0-T13]'s failing `/t:Rebuild` is fully repaired.

## Non-vacuity assertion and its recorded deviation

The pass condition is a **zero** count of `Skipping target "CoreCompile"` in the `/fl` log; the
measured count is **0**. This is a recorded deviation from AC2's `csc.exe` parenthetical, whose count
is zero at `verbosity=normal` even for genuine compiles. The `CoreCompile:` header count is recorded
as **informational only** and is deliberately **not** the assertion: it varies between otherwise
equivalent full-rebuild runs (73 in [P0-T12]'s ANALYZE rebuild, 47 here) and, decisively, it prints
even when the target is skipped. That instability is precisely why `spec.md` rejects it as a counting
mechanism in favour of the skip count, which is emitted only when the target is actually skipped.

## Comparison with the defective documented form

| Run | Command | EXIT | Elapsed | Skip count | Errors |
|---|---|---|---|---|---|
| [P0-T11] | DOC-TYPECHECK warm (`/t:Build` + `/p:Nullable=enable`) | 0 | 1.8 s | **18** | 0 (nothing compiled) |
| [P0-T13] | DEBT-PROBE (`/t:Rebuild` + `/p:Nullable=enable`) | 1 | 4.3 s | 0 | **195** |
| [P0-T14] (this run) | **TYPECHECK** (`/t:Rebuild`, no `/p:Nullable=enable`) | **0** | 18.4 s | **0** | **0** |

## Output Summary

The corrected type-check command is **green and non-vacuous before any edit**: `EXIT_CODE: 0`,
`0 Error(s)`, zero `CoreCompile` skips, 18.4 s. The measured toolchain cost of adopting `/t:Rebuild`
for both MSBuild steps is 17.5 s + 18.4 s = 35.9 s per pass, against 2.8 s + 1.8 s = 4.6 s of vacuous
warm builds — an added ~31 s per pass, consistent with the ~36 s the spec records and accepts. Build
outputs for all 18 projects are restored.
