# P0-T8 — Analyzer baseline over TaskMaster.sln with /t:Rebuild

Timestamp: 2026-09-07T14-10
Task: [P0-T8]
Issue: #796
Channel used: A

RunStartedUtc: 2026-09-07T14:09:26.4332297Z

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults\796\p0-t8\analyzer-rebuild.log;Verbosity=detailed"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0

MSBuild version reported: 18.9.1+a81b43525 for .NET Framework.

## Build summary, verbatim

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:23.21
```

ANALYZER-BASELINE-WARNINGS: 0
ANALYZER-BASELINE-ERRORS: 0

These two totals are the baseline that the P1-T9 final analyzer gate is compared
against.

## Compiler-invocation counts read back from the detailed log

Raw log (gitignored, `.gitignore` line 84 ignores `*.log`):
TestResults/796/p0-t8/analyzer-rebuild.log

Command:

```
pwsh -NoProfile -Command '$log = "TestResults\796\p0-t8\analyzer-rebuild.log"; "CscTaskCount=" + (Select-String -Path $log -Pattern "Task .Csc.").Count; "CscToolCount=" + (Select-String -Path $log -SimpleMatch "csc.exe").Count; (Get-Item QuickFiler\bin\Debug\QuickFiler.dll).LastWriteTimeUtc.ToString("o"); (Get-Item QuickFiler.Test\bin\Debug\QuickFiler.Test.dll).LastWriteTimeUtc.ToString("o")'
```

CscTaskCount=36
CscToolCount=36

Both counts are greater than zero, so `CoreCompile` ran and the analyzers ran with
it. An exit code of 0 with both counts at zero would have been a FAILED gate.

## Assembly-freshness corroboration

| Assembly | LastWriteTimeUtc | At or later than RunStartedUtc |
|---|---|---|
| QuickFiler/bin/Debug/QuickFiler.dll | 2026-09-07T14:09:37.7556047Z | yes |
| QuickFiler.Test/bin/Debug/QuickFiler.Test.dll | 2026-09-07T14:09:42.4832403Z | yes |

Both assemblies were written after the run started, so neither of the two projects
this item touches was skipped.

## Remediation branch

Not entered. No `error CS0006` naming an analyzer assembly was emitted in any
project, so the second-attempt restore branch did not apply.

Output Summary: /t:Rebuild of TaskMaster.sln with EnableNETAnalyzers and
EnforceCodeStyleInBuild returned EXIT_CODE 0 with 0 warnings and 0 errors.
36 Csc task invocations and 36 csc.exe tool invocations recorded in the detailed
log, and both touched assemblies rebuilt after RunStartedUtc, so the gate is not
vacuous.
