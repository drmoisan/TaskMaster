# P3-T4 — Nullable / Type-Check Gate (QC loop stage 3)

Timestamp: 2026-09-01T08-25

## Executed Command Line (quoted verbatim)

```text
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

Invoked through the vswhere-resolved MSBuild path recorded in P0-T7:
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`

Argument vector actually passed, recorded by the runner:

```text
TaskMaster.sln | /t:Rebuild | /m | /p:Configuration=Debug | /p:Platform=Any CPU | /p:TreatWarningsAsErrors=true
```

EXIT_CODE: 0

## Command-String Assertions

- The quoted command line **contains** `TreatWarningsAsErrors=true`. Confirmed: it is the final
  switch, `/p:TreatWarningsAsErrors=true`, in both the quoted command and the executed argument
  vector.
- The quoted command line **contains no occurrence of** `Nullable=enable`. Confirmed: `/p:Nullable=enable`
  was not added, in either the quoted command or the executed argument vector. This is
  character-for-character the command in `.github/workflows/ci.yml`, which omits that property
  deliberately; adding it would conscript every file that has never adopted the `#nullable enable`
  pragma and would fail wholesale on this repository.
- `/t:Rebuild` was used, not `/t:Build`, so the compiler and its nullable-flow analysis genuinely ran
  rather than being skipped by MSBuild's incremental up-to-date check.

## Output Summary

MSBuild's trailing summary:

```text
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:14.05
```

**0 Error(s).** The 5 warnings are unchanged from the P0-T8 baseline and are the same pre-existing
ID-less `System.Reactive.PackagesConfigCheck.targets` `packages.config` warning, one per affected
project. Being a build-targets warning rather than a compiler or nullable diagnostic, it is not
promoted to an error by `/p:TreatWarningsAsErrors=true`.

### Nullable outcome for the change

`UtilitiesCS/Threading/TimeOutTask.cs` carries `#nullable enable` at line 1, so it participates in
nullable analysis and any `CS86xx` diagnostic in it would be promoted to a build error by this gate.
**No `CS86xx` diagnostic was emitted.** In particular no **CS8625** arose, which confirms the
mandatory `?` annotation was written on both new parameter declarations:

```text
Func<int, CancellationTokenSource>? timeoutSourceFactory = null
```

on the public wrapper and on the private implementation. The un-annotated form would have assigned
the `null` literal to a non-nullable reference type and failed this gate against a clean zero-error
baseline.

This artifact is the evidence cited by the AC10 check-off at P4-T10.

Acceptance: met. `EXIT_CODE: 0`; `0 Error(s)`; the quoted command line contains
`TreatWarningsAsErrors=true`; and the quoted command line contains no occurrence of `Nullable=enable`.
