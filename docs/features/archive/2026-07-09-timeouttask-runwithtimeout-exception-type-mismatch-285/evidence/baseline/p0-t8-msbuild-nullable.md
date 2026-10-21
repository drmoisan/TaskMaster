# P0-T8 — Nullable / Type-Check Baseline

Timestamp: 2026-09-01T08-08

## Command (quoted verbatim)

```text
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

Invoked through the same vswhere-resolved MSBuild path recorded by P0-T7:
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`

The argument vector actually passed, recorded by the runner:

```text
TaskMaster.sln | /t:Rebuild | /m | /p:Configuration=Debug | /p:Platform=Any CPU | /p:TreatWarningsAsErrors=true
```

EXIT_CODE: 0

## Command-String Assertions

- The recorded command string **contains** `TreatWarningsAsErrors=true`. Confirmed: it appears as the
  final switch `/p:TreatWarningsAsErrors=true` in both the quoted command and the executed argument
  vector.
- The recorded command string **contains no occurrence of** `Nullable=enable`. Confirmed: neither the
  quoted command nor the executed argument vector contains that substring. `/p:Nullable=enable` was
  not added. This is character-for-character the command in `.github/workflows/ci.yml`, which omits
  it deliberately; adding it would conscript every file that has never adopted the `#nullable enable`
  pragma.
- `/t:Rebuild` was used, not `/t:Build`, so the compiler and its nullable-flow analysis genuinely ran
  rather than being skipped by MSBuild's incremental up-to-date check.

## Output Summary

MSBuild's trailing summary:

```text
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:12.35
```

**Baseline: 0 errors, 5 warnings.** The 5 warnings are the same pre-existing
`System.Reactive.PackagesConfigCheck.targets` `packages.config` warning enumerated in the P0-T7
artifact, emitted once per affected project. It is a build-targets warning from a restored NuGet
package rather than a compiler or nullable diagnostic, so `/p:TreatWarningsAsErrors=true` does not
promote it to an error and the build exits 0.

No `CS86xx` nullable diagnostic was emitted anywhere in the solution at baseline. This matters for
Phase 1: the plan's mandatory `?` on `Func<int, CancellationTokenSource>? timeoutSourceFactory = null`
is required precisely because `UtilitiesCS/Threading/TimeOutTask.cs` carries `#nullable enable`, and
the un-annotated form would produce a `CS8625` that this gate would promote to a build error against
a clean zero-error baseline.

Acceptance: met. `EXIT_CODE: 0`; the recorded command string contains `TreatWarningsAsErrors=true`;
and the recorded command string contains no occurrence of `Nullable=enable`.
