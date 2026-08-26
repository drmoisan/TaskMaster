# [P5-T4] Type-Check / Nullable Gate

Timestamp: 2026-08-26T10-56

Task: [P5-T4]
Feature: docs/features/active/quickfiler-bug-family-446

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

Executed through `pwsh -NoProfile`. `$msbuild` was resolved at run time with the plan's vswhere
prelude to the Visual Studio 18 Community full-framework MSBuild under
`<program-files>\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`.

EXIT_CODE: 0

## Command-text assertions required by this task

- The recorded command text contains `/t:Rebuild` and **does not contain** `/t:Build`.
- The recorded command text **does not contain** `Nullable=enable`. This is character-for-character
  the command in `.github/workflows/ci.yml` (step "Build with nullable warnings treated as errors").
  Per D-Plan-6, adding `/p:Nullable=enable` would conscript every file that never adopted the
  `#nullable enable` pragma; none of the owned production files carries that pragma and none gains
  it here.

## MSBuild Summary Counts

- Error count: `0`
- Warning count: `5`
- `Build succeeded.`, Time Elapsed `00:00:19.30`

## Non-vacuity of the gate

Counts taken over the full build log:

- Occurrences of `Skipping target "CoreCompile"`: **0**
- Lines mentioning `CoreCompile`: 73
- Lines mentioning `csc.exe`: 36

Every project was recompiled under `/p:TreatWarningsAsErrors=true`, so the gate was live and
could have failed.

## Nullable diagnostics

Occurrences of `CS86` in the build log: **0**. No nullable-flow diagnostic was produced by any
file, which matches the `[P0-T11]` baseline.

## Warning detail

The five warnings are the same pre-existing
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`
packages.config diagnostic recorded by `[P0-T10]`, `[P0-T11]` and `[P5-T3]`. Because it is emitted
with a bare `warning` category rather than a `CS` or analyzer code,
`/p:TreatWarningsAsErrors=true` does not promote it and the build stays green. The warning count
is identical to the `[P0-T11]` baseline, so the change set introduced no new warning.

## Build-output lock check

Occurrences of `being used by another process`, `MSB3021`, `MSB3023`, `MSB3026` and `MSB3027`:
**0**. No `obj/` or `bin/` output was locked and no build failure was caused by a lock.

## Output Summary

Type-check gate passed: `EXIT_CODE: 0`, `0 Error(s)`, `5 Warning(s)` all pre-existing and equal in
number to the `[P0-T11]` baseline, zero `CS86xx` nullable diagnostics. Non-vacuity proven by zero
`Skipping target "CoreCompile"` occurrences. No locked build output.
