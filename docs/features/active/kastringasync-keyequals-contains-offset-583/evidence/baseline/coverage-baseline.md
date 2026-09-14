# Coverage Baseline (P0-T8)

- Timestamp: 2026-09-13T01-00
- Command: <resolved dotnet-coverage executable> collect --output <session-temp-file>
  --output-format cobertura --settings <dotnet-coverage module-exclude settings file> --
  <resolved vstest executable> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation
  /Settings:scripts\vscode\TaskMaster.cli.runsettings
  "/TestCaseFilter:TestCategory!=LiveOutlook"
- EXIT_CODE: 0

## Deviation note — dotnet-coverage module-exclude settings

The first attempt at this capture, using only the TaskMaster CLI runsettings file with no
dotnet-coverage module excludes, failed 3 of 1393 tests
(InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing,
InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker,
InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop) with a
System.TypeInitializationException on Deedle.Reflection / netstandard, Version=2.1.0.0. This is
a known, pre-existing, environment-level failure mode of dotnet-coverage's own instrumentation
against Deedle/F# assemblies, unrelated to KaStringAsync and unrelated to this branch's diff
(confirmed no source diff from origin/main at the time of this capture). The repository's own
TaskMaster.runsettings already carries a matching Deedle/FSharp module-exclude block for the
built-in Code Coverage collector; dotnet-coverage's own instrumentation does not read that file,
so an equivalent exclude was supplied to dotnet-coverage collect via its own --settings file
(ModulePaths/Exclude for .*Deedle.*, .*FSharp.*, .*Castle\.Core.*, .*FluentAssertions.*,
.*Moq.*, .*Microsoft\.Testing.*, .*MSTest.*), written to a temp-directory file, never added to
the repository. With that settings file supplied, the same capture reported "Test Run
Successful.", 1393/1393 Passed.

## Test run result

```
Test Run Successful.
Total tests: 1393
     Passed: 1393
 Total time: 15.6062 Seconds
```

## Root coverage figures

- Root line-rate: 0.4298999577286177
- Root branch-rate: 0.24095967959333

## KaStringAsync.cs covered/total

- Covered: 60
- Total: 60

## Per-line hits projection

| Line | Hits |
|---|---|
| 12 | 1 |
| 14 | 1 |
| 15 | 1 |
| 16 | 1 |
| 17 | 1 |
| 18 | 1 |
| 19 | 1 |
| 20 | 1 |
| 21 | 1 |
| 22 | 1 |
| 23 | 1 |
| 24 | 1 |
| 25 | 1 |
| 26 | 1 |
| 27 | 1 |
| 32 | 1 |
| 33 | 1 |
| 39 | 1 |
| 40 | 1 |
| 46 | 1 |
| 47 | 1 |
| 50 | 1 |
| 53 | 1 |
| 54 | 1 |
| 107 | 1 |
| 110 | 1 |
| 111 | 1 |
| 112 | 1 |
| 115 | 1 |
| 116 | 1 |
| 117 | 1 |
| 118 | 1 |
| 119 | 1 |
| 120 | 1 |
| 121 | 1 |
| 122 | 1 |
| 125 | 1 |
| 126 | 1 |
| 127 | 1 |
| 128 | 1 |
| 129 | 1 |
| 131 | 1 |
| 132 | 1 |
| 133 | 1 |
| 134 | 1 |
| 135 | 1 |
| 136 | 1 |
| 137 | 1 |
| 138 | 1 |
| 139 | 1 |
| 140 | 1 |
| 141 | 1 |
| 142 | 1 |
| 143 | 1 |
| 144 | 1 |
| 145 | 1 |
| 150 | 1 |
| 151 | 1 |
| 157 | 1 |
| 158 | 1 |

## Raw output disposition

- Raw-output location (relative to the per-user temp directory root): p583-p0t8b/coverage.cobertura.xml
- Prefix comparison (raw-output full path begins with repository root full path): False
- Post-deletion existence check of the raw file: False (absent)
- Directory listing of evidence/baseline/ after deletion (entry names only):
  - coverage-tool-probe.md
  - csharpier-check.md
  - dotnet-bootstrap.md
  - msbuild-analyzers.md
  - msbuild-nullable.md
  - nuget-restore.md
  - phase0-instructions-read.md

## Output Summary

Exit code 0; inner test run "Test Run Successful." with 0 Failed (1393/1393 Passed, after
supplying a dotnet-coverage module-exclude settings file to work around a known pre-existing
Deedle/FSharp instrumentation crash unrelated to this change); root line-rate
0.4298999577286177, root branch-rate 0.24095967959333; KaStringAsync.cs covered/total 60/60;
per-line hits projection holds 60 rows, no repeated line number; raw output confirmed outside
the repository root and deleted; evidence/baseline/ directory listing contains no .xml entry.
