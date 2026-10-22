# P1-T10 — Nullable gate over TaskMaster.sln with /t:Rebuild

Timestamp: 2026-09-07T14-25
Task: [P1-T10]
Issue: #796
Channel used: A

RunStartedUtc: 2026-09-07T14:24:20.9418220Z

Command: the P0-T9 command form with the log path
`TestResults\796\p1-t10\nullable-rebuild.log`:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=TestResults\796\p1-t10\nullable-rebuild.log;Verbosity=detailed"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0

## No `/p:Nullable=enable` token

The recorded command line contains no `/p:Nullable=enable` token. Corroborated by a
search of the run's console output for the substring `Nullable=enable`, which
returned 0 matches.

## Build summary, verbatim

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Comparison against the P0-T9 baseline

| Total | P0-T9 baseline | P1-T10 | Verdict |
|---|---|---|---|
| Warnings | 0 | 0 | no greater than baseline |
| Errors | 0 | 0 | no greater than baseline |

The new file QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs carries
`#nullable enable` on line 1, so it participates in nullable analysis and its
`CS86xx` diagnostics would have been promoted to build errors under
`TreatWarningsAsErrors`. It produced none.

## Compiler-invocation counts read back from the detailed log

Raw log (gitignored): TestResults/796/p1-t10/nullable-rebuild.log

CscTaskCount=36
CscToolCount=36

Both greater than zero.

## Assembly-freshness corroboration

| Assembly | LastWriteTimeUtc | At or later than RunStartedUtc |
|---|---|---|
| QuickFiler/bin/Debug/QuickFiler.dll | 2026-09-07T14:24:31.6560560Z | yes |
| QuickFiler.Test/bin/Debug/QuickFiler.Test.dll | 2026-09-07T14:24:36.5646003Z | yes |

Output Summary: EXIT_CODE 0 with 0 warnings and 0 errors, equal to the P0-T9
baseline; 36 Csc task and 36 csc.exe tool invocations; both touched assemblies
rebuilt after RunStartedUtc; no `/p:Nullable=enable` token on the command line.
