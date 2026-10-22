# P0-T9 — Nullable baseline over TaskMaster.sln with /t:Rebuild

Timestamp: 2026-09-07T14-11
Task: [P0-T9]
Issue: #796
Channel used: A

RunStartedUtc: 2026-09-07T14:10:48.3258170Z

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=TestResults\796\p0-t9\nullable-rebuild.log;Verbosity=detailed"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0

## No `/p:Nullable=enable` token

The command line reproduced above contains no `/p:Nullable=enable` token. That is
confirmed by inspection of the command text and corroborated by a search of the
run's console output for the substring `Nullable=enable`, which returned 0 matches.

No project in this repository carries a `<Nullable>` element, and neither
Directory.Build.props nor Directory.Build.targets sets a nullable property, so
nullable analysis is reached only through the per-file `#nullable enable` pragma.

## Build summary, verbatim

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

NULLABLE-BASELINE-WARNINGS: 0
NULLABLE-BASELINE-ERRORS: 0

These two totals are the baseline that the P1-T10 final nullable gate is compared
against.

## Compiler-invocation counts read back from the detailed log

Raw log (gitignored): TestResults/796/p0-t9/nullable-rebuild.log

CscTaskCount=36
CscToolCount=36

Both counts are greater than zero, so `CoreCompile` ran on every project and the
compiler and nullable-flow diagnostics actually executed. An exit code of 0 with
both counts at zero would have been a FAILED gate.

## Assembly-freshness corroboration

| Assembly | LastWriteTimeUtc | At or later than RunStartedUtc |
|---|---|---|
| QuickFiler/bin/Debug/QuickFiler.dll | 2026-09-07T14:10:59.0796692Z | yes |
| QuickFiler.Test/bin/Debug/QuickFiler.Test.dll | 2026-09-07T14:11:04.9739118Z | yes |

Output Summary: /t:Rebuild of TaskMaster.sln with TreatWarningsAsErrors returned
EXIT_CODE 0 with 0 warnings and 0 errors. 36 Csc task invocations and 36 csc.exe
tool invocations recorded, both touched assemblies rebuilt after RunStartedUtc, and
the command line carries no `/p:Nullable=enable` token.
