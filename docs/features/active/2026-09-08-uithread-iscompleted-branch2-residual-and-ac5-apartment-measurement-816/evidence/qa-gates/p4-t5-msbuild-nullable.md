# P4-T5 — Type-check gate (nullable analysis, warnings as errors)

Timestamp: 2026-09-13T23-41

Command:

```
$msb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\logs\p4-t5-nullable.log;Verbosity=detailed"
```

EXIT_CODE: 0

Output Summary:

| Observation | Value | Required |
|---|---|---|
| Error count, read by `(^|[^0-9])[0-9]+ Error\(s\)` | `    0 Error(s)` — **0** | zero |
| `(Select-String -Path coverage\logs\p4-t5-nullable.log -Pattern 'Task \x22Csc\x22').Count` | **36** | greater than zero |
| `(Select-String -Path coverage\logs\p4-t5-nullable.log -Pattern 'Skipping target \x22CoreCompile\x22').Count` | **0** | zero |

Recorded for context only: the warning count line reads `    0 Warning(s)`. Under
`/p:TreatWarningsAsErrors=true` any warning would have been promoted to an error and counted in the
error line, so this zero is an invariant of the command form rather than an independent measurement.

The compile-task count of 36 is the non-vacuity observation.

The skipped-compile count is recorded because AC14 requires it, and it is stated plainly that under
`/t:Rebuild` that message cannot be emitted, so the count is **an invariant restatement rather than
a condition that can fail**.

The command carries no `/p:Nullable=enable`. Nullable enforcement in this repository is per-file
opt-in through the `#nullable enable` directive; the production file this delivery changes,
`UtilitiesCS/Threading/UiThread.cs`, carries `#nullable enable` at line 1 (verified by
`Select-String -Pattern '^#nullable'`, which reports exactly that one match), so its `CS86xx` diagnostics are
promoted to build errors by this command and the hardened condition is covered by this gate. The
added conjunct includes an explicit `_dispatcher is not null` test, which is exactly the shape
nullable flow analysis requires before the field is passed to `ReferenceEquals`.
