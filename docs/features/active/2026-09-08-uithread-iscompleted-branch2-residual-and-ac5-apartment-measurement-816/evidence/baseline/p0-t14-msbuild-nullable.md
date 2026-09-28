# P0-T14 — Nullable and type-check baseline (baseline)

Timestamp: 2026-09-13T23-07

Command:

```
$msb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\logs\p0-t14-nullable.log;Verbosity=detailed"
```

EXIT_CODE: 0

Output Summary:

- Exit code: **0**
- Error count, read by the pattern `(^|[^0-9])[0-9]+ Error\(s\)`: `    0 Error(s)` — **0**
- `(Select-String -Path coverage\logs\p0-t14-nullable.log -Pattern 'Task \x22Csc\x22').Count`: **36**

The compile-task count of 36 is greater than zero, satisfying the non-vacuity requirement.

Recorded for context only, not a required field: the warning count line reads `    0 Warning(s)`.
Under `/p:TreatWarningsAsErrors=true` a warning would have been promoted to an error and counted in
the error line instead, so the zero warning count is an invariant of this command form rather than
an independent measurement.

The command carries no `/p:Nullable=enable`, per CLAUDE.md. Nullable enforcement in this repository
is per-file opt-in through the `#nullable enable` directive, and this command promotes the `CS86xx`
diagnostics of files that have opted in to build errors.

The analyzer provisioning step described in the P0-T13 artifact was already in place when this
command ran, which is why it did not reproduce the CS0006 failure recorded there.
