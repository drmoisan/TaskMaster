# P9-T4 — Final nullable gate over TaskMaster.sln with /t:Rebuild

Timestamp: 2026-09-07T15-55
Task: [P9-T4]
Issue: #796
Channel used: A

Command, the P0-T9 command form with the log path
`TestResults\796\p9-t4\nullable-rebuild.log`:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=TestResults\796\p9-t4\nullable-rebuild.log;Verbosity=detailed"; "EXIT_CODE=$LASTEXITCODE"'
```

RunStartedUtc: 2026-09-07T19:55:29.7661680Z

EXIT_CODE: 0

## No `/p:Nullable=enable` token

The command line reproduced above contains no `/p:Nullable=enable` token. That is
confirmed by inspection of the command text and corroborated mechanically against the
detailed log, which records the full compiler command line for every project:

```
pwsh -NoProfile -Command '$log = "TestResults\796\p9-t4\nullable-rebuild.log"; "NullableEnableTokenCount=" + (Select-String -Path $log -SimpleMatch "Nullable=enable").Count'
```

NullableEnableTokenCount=0

The corroboration is meaningful rather than circular: a detailed-verbosity MSBuild log
reproduces the csc command line for each project, so a solution-wide nullable opt-in
introduced by any route — a command-line property, a project element, or a
Directory.Build file — would appear in it. It does not.

## Build summary, verbatim

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:16.88
```

## Compiler-invocation counts read back from the detailed log

Raw log (gitignored, `.gitignore` line 84 ignores `*.log`):
TestResults/796/p9-t4/nullable-rebuild.log

CscTaskCount=36
CscToolCount=36

Both counts are greater than zero and both match the P0-T9 baseline exactly, so
`CoreCompile` ran on the same thirty-six projects and the nullable-flow diagnostics
actually executed. An exit code of 0 with both counts at zero would have been a FAILED
gate.

## Assembly-freshness corroboration

| Assembly | LastWriteTimeUtc | At or later than RunStartedUtc |
|---|---|---|
| QuickFiler/bin/Debug/QuickFiler.dll | 2026-09-07T19:55:37.7388044Z | yes |
| QuickFiler.Test/bin/Debug/QuickFiler.Test.dll | 2026-09-07T19:55:41.1478330Z | yes |

Both assemblies were rewritten after the run started, so neither of the two projects
this item touches was skipped.

## Comparison against the P0-T9 baseline

Baseline read from evidence/baseline/p0-t9-nullable-rebuild-baseline.md.

| Total | P0-T9 baseline | P9-T4 | Verdict |
|---|---|---|---|
| Warnings | 0 | 0 | no greater than baseline |
| Errors | 0 | 0 | no greater than baseline |

## Acceptance clause by clause

| Clause | Observed | Met |
|---|---|---|
| `EXIT_CODE: 0` | 0 | yes |
| at least one of `CscTaskCount` and `CscToolCount` greater than zero | both 36 | yes |
| both touched assemblies carry LastWriteTimeUtc at or later than `RunStartedUtc:` | 19:55:37.74Z and 19:55:41.15Z against 19:55:29.77Z | yes |
| warning and error totals no greater than the P0-T9 baseline | 0 and 0 against 0 and 0 | yes |
| the recorded command line contains no `/p:Nullable=enable` token | absent from the command text, and 0 occurrences in the detailed log | yes |

## Environmental note

The MSB3061 file-lock warnings that blocked P9-T3 attempt 1 are absent here. No Outlook
process was running when this gate executed, and none was started for it.

Output Summary: /t:Rebuild of TaskMaster.sln with TreatWarningsAsErrors returned
EXIT_CODE 0 with 0 warnings and 0 errors, matching the P0-T9 baseline exactly. 36 Csc
task invocations and 36 csc.exe tool invocations were recorded in the detailed log, both
touched assemblies were rewritten after RunStartedUtc, and neither the command line nor
the log carries a `Nullable=enable` token. All five acceptance clauses are met.
