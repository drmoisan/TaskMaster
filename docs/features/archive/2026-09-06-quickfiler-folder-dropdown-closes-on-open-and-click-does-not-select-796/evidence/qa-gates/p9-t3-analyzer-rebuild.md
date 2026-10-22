# P9-T3 — Final analyzer gate over TaskMaster.sln with /t:Rebuild

Timestamp: 2026-09-07T15-07
Task: [P9-T3]
Issue: #796
Channel used: A

Command: the P0-T8 command form with the log path
`TestResults\796\p9-t3\analyzer-rebuild.log`.

## Attempt 1 — GATE NOT MET

RunStartedUtc: 2026-09-07T19:05:46.6776541Z

EXIT_CODE: 0

Build summary, verbatim:

```
    3 Warning(s)
    0 Error(s)

Time Elapsed 00:00:20.95
```

CscTaskCount=36
CscToolCount=36

| Assembly | LastWriteTimeUtc | At or later than RunStartedUtc |
|---|---|---|
| QuickFiler/bin/Debug/QuickFiler.dll | 2026-09-07T19:05:56.8789568Z | yes |
| QuickFiler.Test/bin/Debug/QuickFiler.Test.dll | 2026-09-07T19:06:01.3438632Z | yes |

Comparison against the P0-T8 baseline recorded in
evidence/baseline/p0-t8-analyzer-rebuild-baseline.md:

| Total | P0-T8 baseline | P9-T3 attempt 1 | Verdict |
|---|---|---|---|
| Warnings | 0 | 3 | GREATER THAN BASELINE — gate not met |
| Errors | 0 | 0 | no greater than baseline |

Three of the four acceptance clauses are met — exit code 0, both compiler-invocation
counts at 36, and both touched assemblies rebuilt after RunStartedUtc. The warning-total
clause is not, so this attempt does not satisfy the task.

### The three warnings, and why they are not this item's

All three are the same diagnostic, `MSB3061`, raised by the `CoreClean` target of
TaskMaster/TaskMaster.csproj. Each reports that a file under TaskMaster/bin/Debug could
not be deleted because a running Microsoft Outlook process holds it open. The three
files are `runtimes/win-x64/native/WebView2Loader.dll`, `x64/leptonica-1.82.0.dll` and
`x64/tesseract50.dll`. None is an analyzer diagnostic, none names a source file, none
arises in QuickFiler or QuickFiler.Test, and none is reachable from this item's diff.

The interference began during execution and is timestamped. The Outlook process that
holds the locks reports `StartTime` 2026-09-07 14:59:58 local, which is
2026-09-07T18:59:58 UTC. That instant falls between two runs of the identical command
form:

| Run | RunStartedUtc | Warnings |
|---|---|---|
| P8-T1 rebuild | 2026-09-07T18:59:16.8282314Z | 0 |
| Outlook process 47952 starts | 2026-09-07T18:59:58 | — |
| P8-T2 rebuild | 2026-09-07T19:00:52.2216349Z | 3 |
| P9-T3 attempt 1 | 2026-09-07T19:05:46.6776541Z | 3 |

The last clean run of this command form preceded the Outlook start by 42 seconds and the
first warning-bearing run followed it by 54 seconds, with no change to any tracked file
in between. That places the cause outside this item's change with a measured boundary
rather than an assertion.

The gate was not weakened to accommodate this. `MSB3061` is a file-lock warning rather
than an analyzer diagnostic, but the acceptance clause compares build-summary totals, and
the build-summary total is 3. Execution therefore paused here and the condition was
reported for resolution rather than reinterpreted.

## Attempt 2 — GATE MET

Timestamp: 2026-09-07T15-54
Channel used: A

The environmental cause recorded above was cleared by the maintainer: the Outlook
process that held the three files under TaskMaster/bin/Debug was closed. That was
verified before this attempt rather than assumed, with
`pwsh -NoProfile -Command "@(Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue).Count"`,
which printed `0`. No Outlook process was started by this attempt, and none is required
by it.

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults\796\p9-t3\analyzer-rebuild.log;Verbosity=detailed"; "EXIT_CODE=$LASTEXITCODE"'
```

RunStartedUtc: 2026-09-07T19:54:22.1490275Z

EXIT_CODE: 0

Build summary, verbatim:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:17.92
```

Compiler-invocation counts read back from the detailed log with the P0-T8 read-back
command form, against the raw log (gitignored, `.gitignore` line 84 ignores `*.log`)
TestResults/796/p9-t3/analyzer-rebuild.log:

CscTaskCount=36
CscToolCount=36

| Assembly | LastWriteTimeUtc | At or later than RunStartedUtc |
|---|---|---|
| QuickFiler/bin/Debug/QuickFiler.dll | 2026-09-07T19:54:31.8208498Z | yes |
| QuickFiler.Test/bin/Debug/QuickFiler.Test.dll | 2026-09-07T19:54:35.2016372Z | yes |

Comparison against the P0-T8 baseline recorded in
evidence/baseline/p0-t8-analyzer-rebuild-baseline.md:

| Total | P0-T8 baseline | P9-T3 attempt 2 | Verdict |
|---|---|---|---|
| Warnings | 0 | 0 | no greater than baseline |
| Errors | 0 | 0 | no greater than baseline |

Acceptance clause by clause:

| Clause | Observed | Met |
|---|---|---|
| `EXIT_CODE: 0` | 0 | yes |
| at least one of `CscTaskCount` and `CscToolCount` greater than zero | both 36 | yes |
| both touched assemblies carry LastWriteTimeUtc at or later than `RunStartedUtc:` | 19:54:31.82Z and 19:54:35.20Z against 19:54:22.15Z | yes |
| analyzer warning and error totals no greater than the P0-T8 baseline | 0 and 0 against 0 and 0 | yes |

The compiler-invocation counts are identical to the P0-T8 baseline at 36 and 36, so the
same thirty-six projects compiled and the gate is not vacuous.

Output Summary: attempt 2 of the /t:Rebuild analyzer gate returned EXIT_CODE 0 with 0
warnings and 0 errors, matching the P0-T8 baseline exactly. 36 Csc task invocations and
36 csc.exe tool invocations were recorded in the detailed log, and both touched
assemblies were rewritten after RunStartedUtc. All four acceptance clauses are met. The
attempt 1 record above is retained as the audit trail of the environmental file-lock
condition and its resolution.
