# MSBuild Analyzer Gate — Issue #503 (P6-T4)

Timestamp: 2026-08-08T14-35

Command:
```
pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; Write-Host \"EXIT_CODE=$LASTEXITCODE\""
```

EXIT_CODE: **0**

## Output Summary

- Result: `Build succeeded.`
- Error count: **0**
- Warning count: **6**

### Reconciliation against the P0-T7 merge-base baseline

| Diagnostic | Baseline (P0-T7) | Now (P6-T4) | New? |
|---|---|---|---|
| `System.Reactive.PackagesConfigCheck.targets(31,5): warning : ... packages.config ... not supported by System.Reactive v7.0` | 5 (`QuickFiler`, `TaskMaster`, `ToDoModel`, `UtilitiesCS`, `UtilitiesCS.Test`) | 5 (same five projects) | No — pre-existing, unchanged |
| `CSC : warning CS2002: Source file '...\UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times [UtilitiesCS.Test.csproj]` | 0 (build was incrementally up to date and skipped `CoreCompile` for `UtilitiesCS.Test`) | 1 | No — **pre-existing and out of scope**, established in `<FEATURE>\evidence\other\phase2-build.2026-08-08T13-30.md` |

`CS2002` originates from a duplicate `<Compile Include>` item that already exists at the merge-base (`git show <MERGE_BASE>:UtilitiesCS.Test/UtilitiesCS.Test.csproj | grep -c PercentageFormatterTests.cs` returns 2), in a project this branch has not touched (`git diff --name-only <MERGE_BASE>..HEAD -- UtilitiesCS.Test/` returns 0 paths). It is outside the plan's section 4 scope lock and is therefore recorded rather than fixed.

### Analyzer rule diagnostics

A scan of the full build log for analyzer rule IDs returns exactly two occurrences, both the same `CS2002` compiler warning (emitted once in the per-project block and once in the summary block). There are **zero** occurrences of any of the following across the entire log:

- `CA####` (.NET analyzers)
- `S####` (SonarAnalyzer.CSharp)
- `MA####` (Meziantou.Analyzer)
- `RCS####` (Roslynator.Analyzers)
- `AsyncFixer##` (AsyncFixer)
- `RS0030` (BannedApiAnalyzers banned-symbol rule)

The absence of `RS0030` is the `BannedSymbols.txt` result referenced by AC28: no new or changed file in this branch calls `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, or `Task.Delay`.

Binary outcome: **PASS** — zero errors and no new analyzer diagnostic relative to the P0-T7 baseline. This satisfies the AC22 clause "completes with zero errors and no new analyzer diagnostics".

This task mutated no source file.
