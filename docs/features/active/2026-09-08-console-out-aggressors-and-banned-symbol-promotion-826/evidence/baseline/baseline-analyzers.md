# Baseline analyzer build (issue #826, [P0-T7], toolchain step 2)

Timestamp: 2026-09-09T19-04

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:LogFile=coverage/826-raw/p0-t7-analyzers.log;Verbosity=detailed"
```

resolved through `vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe"` and run as one
`pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard. `/v:m` was added to the
console channel only, so the transcribed console text is readable; the detailed-verbosity file logger
that the gate figures are read from is unaffected by it.

EXIT_CODE: 0

## Gate figures read from `coverage/826-raw/p0-t7-analyzers.log`

| Figure | Observed | Required |
|---|---|---|
| ` 0 Error(s)` (`-SimpleMatch`, leading space load-bearing) | 1 | at least 1 |
| `Skipping target "CoreCompile"` (`-Pattern`) | 0 | 0 |
| `Task "Csc"` (`-Pattern`) | 18 | at least 1 |

The `Task "Csc"` count of 18 is what makes the zero `Skipping target "CoreCompile"` non-vacuous: the log
records 18 compiler invocations, one per project in the solution, so the build demonstrably compiled
rather than skipped.

Output Summary: MSBuild 18.9.1 rebuilt all 18 projects and exited 0. All four acceptance figures hold.
The raw detailed log stays under the gitignored `coverage/826-raw/` directory and is not committed,
because it embeds absolute host paths.

---

Timestamp correction, recorded rather than absorbed: the `Timestamp:` value in this artifact was initially written as an extrapolated value rather than read from the clock. At 2026-09-09T19-10 the executor read the real clock, observed the discrepancy, and replaced the value with this artifact's own observed filesystem write time. The corrected value is an observation; the superseded value was not.
