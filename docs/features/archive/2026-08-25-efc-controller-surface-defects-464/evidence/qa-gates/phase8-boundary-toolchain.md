# Phase 8 boundary — full toolchain verification

Timestamp: 2026-08-28T01-40
Task: Phase 8 boundary verification (orchestrator-authorized batch end; not a numbered plan task)
EXIT_CODE: 0 for gates 1-3; 1 for gate 4, entirely from pre-existing failures analysed below

The authorized delegation covers Phases 5 through 8. This artifact records the full toolchain at that
boundary. The plan's own final QC phase is Phase 10 and is **not** executed here.

## Gate 1 — format

Command: `dotnet tool run csharpier check .` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 0

```
Checked 1549 files in 4990ms.
```

Zero unformatted files, repository-wide.

## Gate 2 — analyzers

Command: `& "<resolved MSBuild.exe>" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:n`
EXIT_CODE: 0

```
    5 Warning(s)
    0 Error(s)
```

**Non-vacuity:** a count of the literal `Skipping target "CoreCompile"` in the build log returns **0**,
so every project actually compiled and the analyzers actually ran.

| Metric | Phase 0 baseline | Phase 8 boundary | Verdict |
|---|---|---|---|
| Errors | 0 | 0 | unchanged |
| Warnings | 5 | 5 | unchanged |

## Gate 3 — nullable / type-check

Command: `& "<resolved MSBuild.exe>" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo /v:n`
EXIT_CODE: 0

```
    5 Warning(s)
    0 Error(s)
```

**Non-vacuity:** zero `Skipping target "CoreCompile"` lines. `/p:Nullable=enable` was not added, per
decision D2.

| Metric | Phase 0 baseline | Phase 8 boundary | Verdict |
|---|---|---|---|
| Errors | 0 | 0 | unchanged |

## Gate 4 — tests

Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation`
EXIT_CODE: 1

```
Total tests: 1169
     Passed: 1155
     Failed: 14
```

### Arithmetic against the post-merge measurement

| Metric | Post-merge (before Phase 5) | Phase 8 boundary | Delta |
|---|---|---|---|
| Total executed | 1137 | **1169** | **+32** |
| Passed | 1123 | **1155** | **+32** |
| Failed | 14 | **14** | **0** |

**+32 is exactly the count of test results this batch added**: 13 in Phase 5 (5 form-side, 8 item-side),
8 in Phase 6 (five `[DataRow]` boundary results, the default-sink test, the `PopulateFolderCombobox`
fault test, the stack-trace test), 6 in Phase 7, and 5 in Phase 8. Every one of them passes.

**The failed count did not move.** The 14 failures are the identical set recorded at the post-merge
measurement in `postmerge-quickfiler-test.md`, by name: the same eleven `QfcItemController.*`
initialization and seam-factory tests and the same three `QfcItemController.UiThreadDispatcherFixture`
tests, each reporting a duration of approximately one minute, which is the WinFormsPumpHost /
dispatcher-fixture timeout rather than an assertion failure. All three owning files
(`QfcItemController.InitializationTests.Part3.cs`, `QfcItemController.SeamFactoryTests.cs`,
`QfcItemController.UiThreadDispatcherFixtureTests.cs`) are outside this feature's owned set, are
forbidden to it by constraint C1, and are LIVE under sibling #489. All 15 of those tests passed in the
isolated scoped re-run recorded in `postmerge-quickfiler-test.md` (EXIT_CODE 0, 3.1 s).

**Classification: BASE-INTRODUCED, unchanged by this batch, not remediated here.**

## Owned-file sizes at the Phase 8 boundary

| File | Lines | Gate | Verdict |
|---|---|---|---|
| `QuickFiler/Controllers/EfcFormController.cs` | 1189 | at most 1193 | PASS |
| `QuickFiler/Controllers/EfcItemController.cs` | 1117 | strictly fewer than 1170 | PASS |
| `QuickFiler/Viewers/EfcViewer.cs` | 169 | at most 500 | PASS |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | 485 | at most 500 | PASS |
| `QuickFiler.Test/Controllers/EfcItemControllerTests.cs` | 470 | at most 500 | PASS |
| `QuickFiler.Test/Controllers/EfcItemController.CleanupTests.cs` | 260 | at most 500 | PASS |
| `QuickFiler.Test/Controllers/EfcViewerTests.cs` | 164 | at most 500 | PASS |

Output Summary: PASS at the Phase 8 boundary. Format 0 diffs; analyzers 0 errors / 5 warnings matching
baseline with zero CoreCompile skips; nullable 0 errors with zero CoreCompile skips; tests 1169 executed
with 1155 passed and the same 14 pre-existing base-introduced failures as the post-merge measurement, a
failed-count delta of 0 and a passed-count delta of exactly +32, matching the 32 results this batch
added. All owned files are within their size gates.
