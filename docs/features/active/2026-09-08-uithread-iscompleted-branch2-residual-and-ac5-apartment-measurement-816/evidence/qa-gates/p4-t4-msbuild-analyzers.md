# P4-T4 — Analyzer gate (lint step of the final QC toolchain)

Timestamp: 2026-09-13T23-40

Command:

```
$msb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\logs\p4-t4-analyzers.log;Verbosity=detailed"
```

EXIT_CODE: 0

Output Summary:

| Observation | Value | Required |
|---|---|---|
| Error count, read by `(^|[^0-9])[0-9]+ Error\(s\)` | `    0 Error(s)` — **0** | zero |
| `(Select-String -Path coverage\logs\p4-t4-analyzers.log -Pattern 'Task \x22Csc\x22').Count` | **36** | greater than zero |
| `(Select-String -Path coverage\logs\p4-t4-analyzers.log -Pattern 'Skipping target \x22CoreCompile\x22').Count` | **0** | zero |

Recorded for context only: the warning count line reads `    0 Warning(s)`.

The compile-task count of 36 is the non-vacuity observation: compilation actually ran on every
project, so the analyzers actually ran.

The skipped-compile count is recorded because AC14 requires it. It is stated plainly that this count
is **an invariant restatement rather than a condition that can fail**: the target is `/t:Rebuild`,
under which MSBuild cannot emit the skipped-`CoreCompile` message at all. The observation that can
fail is the compile-task count above, and it is the one this artifact relies on.

The target is `Rebuild` and never `Build`. MSBuild's incremental up-to-date check does not
invalidate on a command-line property change, so a warm `/t:Build` returns exit code zero having
skipped compilation on every project, and the gate could not fail.

No `/p:Nullable=enable` was added.
