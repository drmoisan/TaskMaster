# Baseline — DOC-ANALYZE run cold ([P0-T9])

Timestamp: 2026-08-10T22-48
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:m /fl "/flp:logfile=coverage/baseline-doc-analyze-cold.log;verbosity=normal"`
EXIT_CODE: 0

`MSBUILD` = `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe`,
invoked via `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-doc-analyze-cold.ps1` per the
plan's command conventions (the Bash tool mangles MSBuild switches and is not used for MSBuild).

This is the **defective documented analyzer form** (DOC-ANALYZE), measured only. It is not adopted.

## Cold-state precondition

Immediately before this run, the tree carried no build outputs:

```
$ ls -d */bin/Debug   -> (no matches)
$ ls -d */obj         -> (no matches)
```

The only prior steps were [P0-T5] (`dotnet tool restore`), [P0-T6] (NuGet restore) and the CSharpier
runs in [P0-T7] and [P0-T8], none of which compiles. This run is therefore genuinely cold.

## Measurements

| Metric | Value |
|---|---|
| `EXIT_CODE` | 0 |
| Elapsed | 27.8 s |
| `Skipping target "CoreCompile"` count | **0** |
| MSBuild summary | `0 Error(s)` |
| MSBuild summary | `6 Warning(s)` |

## Non-vacuity assertion and its recorded deviation

The non-vacuity mechanism is the count of the literal string `Skipping target "CoreCompile"` in the
`/fl` log. Zero is the pass condition for a genuine compile. This is a **recorded deviation from
AC2's parenthetical**, which names a `csc.exe` invocation count: measurement shows that count is zero
at `verbosity=normal` even for genuine compiles, so the parenthetical as literally written is not
satisfiable by the described log. `CoreCompile:` header lines are also not counted, because they
print even when the target is skipped. The zero-skip assertion is strictly more discriminating and is
recorded in `spec.md` § "The non-vacuity assertion mechanism".

Skip count is **0**, so this cold run compiled genuinely.

## Output Summary

DOC-ANALYZE run cold returns `EXIT_CODE: 0` in 27.8 s with a zero `Skipping target "CoreCompile"`
count and `0 Error(s)`. Cold, the documented analyzer command does perform real work — which is why
the defect is invisible on a CI runner and visible only in a warm local tree. [P0-T10] re-runs the
identical command immediately to expose Defect C.
