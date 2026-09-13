# Coverage Post-Change (P5-T5)

- Timestamp: 2026-09-13T02-05
- Command: <resolved dotnet-coverage executable> collect --output <session-temp-file>
  --output-format cobertura --settings <dotnet-coverage module-exclude settings file> --
  <resolved vstest executable> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation
  /Settings:scripts\vscode\TaskMaster.cli.runsettings
  "/TestCaseFilter:TestCategory!=LiveOutlook"
- EXIT_CODE: 0

## Deviation note

Identical dotnet-coverage module-exclude settings file as used at P0-T8 was supplied, for the
same pre-existing Deedle/FSharp instrumentation reason recorded there.

## Test run result

```
Test Run Successful.
Total tests: 1394
     Passed: 1394
 Total time: 14.4300 Seconds
```

(1394 total vs. the P0-T8 baseline's 1393 — the one added test is the new
KeyEquals_ContainsMatchAtNonPrefixIndex_InvokesUpdateWithLastMatchedCharacter regression method.)

## Root coverage figures

- Root line-rate: 0.4300204268507431
- Root branch-rate: 0.24095967959333

## KaStringAsync.cs covered/total (post-change)

- Covered: 65
- Total: 65

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
| 109 | 1 |
| 112 | 1 |
| 113 | 1 |
| 114 | 1 |
| 117 | 1 |
| 118 | 1 |
| 119 | 1 |
| 120 | 1 |
| 121 | 1 |
| 122 | 1 |
| 123 | 1 |
| 124 | 1 |
| 127 | 1 |
| 128 | 1 |
| 129 | 1 |
| 130 | 1 |
| 131 | 1 |
| 132 | 1 |
| 133 | 1 |
| 134 | 1 |
| 135 | 1 |
| 136 | 1 |
| 138 | 1 |
| 139 | 1 |
| 140 | 1 |
| 141 | 1 |
| 142 | 1 |
| 143 | 1 |
| 144 | 1 |
| 145 | 1 |
| 146 | 1 |
| 147 | 1 |
| 148 | 1 |
| 149 | 1 |
| 150 | 1 |
| 151 | 1 |
| 152 | 1 |
| 157 | 1 |
| 158 | 1 |
| 164 | 1 |
| 165 | 1 |

## Raw output disposition

- Raw-output location (relative to the per-user temp directory root): p583-p5t5/coverage.cobertura.xml
- Prefix comparison (raw-output full path begins with repository root full path): False
- Post-deletion existence check of the raw file: False (absent)
- Directory listing of evidence/qa-gates/ after deletion (entry names only):
  - csharpier-check.md
  - csharpier-format.md
  - msbuild-analyzers.md
  - msbuild-nullable.md

## Output Summary

Exit code 0; inner test run "Test Run Successful." with 0 Failed (1394/1394 Passed); root
line-rate 0.4300204268507431, root branch-rate 0.24095967959333; KaStringAsync.cs covered/total
65/65; per-line hits projection holds 65 rows, no repeated line number; raw output confirmed
outside the repository root and deleted; evidence/qa-gates/ directory listing contains no .xml
entry.
