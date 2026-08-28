# Post-merge C# toolchain re-run (orchestrator)

Timestamp: 2026-08-27T20-41
Command: see the four gate rows below
EXIT_CODE: 0
Output Summary: full four-step C# toolchain re-run after merging epic integration tip `13a22ade`
(#493 fan-in). All four gates pass in a single loop; no gate rewrote a file, so no restart was
triggered. 6719 of 6719 tests pass. Line coverage 85.1326 percent, branch coverage 79.2162 percent.

## Why this re-run was required

Repository policy requires the full toolchain in order after every merge. This re-run was also
substantively necessary rather than ceremonial: the #493 fan-in modified
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` and
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`, which are partial-class
siblings of this feature's `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs`. A
compile-level or fixture-level conflict was a real possibility and had to be ruled out by running the
gates, not by inspection.

## Gate results

| # | Gate | Command | EXIT | Non-vacuity proof |
| --- | --- | --- | --- | --- |
| 1 | Format verify | `dotnet tool run csharpier check .` | 0 | 1543 files checked, 0 unformatted. File count rose from 1541 to 1543, the two files #493 added. |
| 2 | Lint / analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | `Skipping target "CoreCompile"` count **0**; 15 `CoreCompile:` target executions; 53 csc invocation lines; analyzers genuinely loaded, e.g. `/analyzer:..\packages\Roslynator.Analyzers.4.16.0\...\Roslynator.CSharp.Analyzers.dll`; both `KbdActions.cs` and `QfcCollectionController.cs` appear in the compile item lists. 0 errors, 5 warnings. |
| 3 | Type-check / nullable | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 | `Skipping target "CoreCompile"` count **0**; 11 `CoreCompile:` target executions; 0 `error CS`/`error CA` lines. |
| 4 | Test + coverage | `pwsh -NoProfile -File ./scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 0 | Test Run Successful; total 6719, passed 6719, failed 0; 1.0092 minutes. |

`/t:Rebuild` was used for both msbuild gates, never `/t:Build`. `/p:Nullable=enable` was not added to
any command.

### On the `Task "Csc"` count

A `Task "Csc"` grep returns 0 in both msbuild logs. That is **not** evidence of a vacuous gate: at
this log verbosity the compiler task is not announced under that name. The authoritative
non-vacuity signal is the zero count of `Skipping target "CoreCompile"` together with the positive
count of executed `CoreCompile:` targets, both recorded above.

### Warning accounting (gate 2)

All 5 warnings are the pre-existing `System.Reactive 7.0.0` `packages.config` diagnostic emitted by
`System.Reactive.PackagesConfigCheck.targets`, one per consuming project. This is identical to the
Phase 0 baseline set and is unrelated to this feature's diff.

## Coverage

Unfiltered whole-run denominator, the same wrapper and denominator as the Phase 0 baseline, which is
what makes the figures comparable.

| Figure | Baseline | Pre-merge final | Post-merge final |
| --- | --- | --- | --- |
| Repository-wide line coverage | 85.04% | 85.13% | **85.1326%** |
| Repository-wide branch coverage | 79.12% | 79.21% | **79.2162%** |
| `lines-valid` | 63921 | 63905 | 63905 |
| `lines-covered` | — | — | 54404 |
| `KbdActions.cs` line-rate | 0.93976 | 0.98980 | **0.98980** |
| `QfcItemController.Navigation.cs` line-rate | 0.90678 | 0.92126 | **0.92126** |

Both repository figures clear both readings of the repository's two coverage policies: the
`CLAUDE.md` §UT2 floor of `>= 80%` line, and the `.claude/rules/general-unit-test.md` /
`quality-tiers.md` floors of `>= 85%` line and `>= 75%` branch. No interpretation was needed to
declare a pass. That policy conflict is pre-existing and is recorded, unresolved, in
`evidence/other/p5-t9-pr-body-inputs.2026-08-27T20-13.md` Item 3.

`QuickFiler/Controllers/QfcCollectionController.cs` reports no line-rate because it carries
`[ExcludeFromCodeCoverage]`, which is pre-existing at the base and not introduced here.

`artifacts/csharp/coverage.xml` was deliberately NOT created. The generated
`coverage/coverage.cobertura.xml` is ignored by `.gitignore:144` (`coverage/*`) and is therefore not
committed.

## Nullable-participation disclosure

None of this feature's three production files carries a `#nullable enable` directive, so none of them
participates in nullable flow analysis. Gate 3 therefore passes without substantively constraining
this feature's diff. This is disclosed rather than presented as a stronger result than it is. Adding
the pragma was not done: nullable enforcement in this repository is per-file opt-in, and conscripting
a 2437-line pre-existing file into nullable analysis would be scope widening well beyond three
keyboard-registration defects, and is forbidden to this feature by the epic's ownership rules.

## Merge and deletion invariant

- Merged integration tip: `13a22ade` (`Merge pull request #653 from ... bug/quickfiler-test-uithread-dispatcher-493`).
- Behind after merge: **0**. Ahead: 16.
- `git diff --numstat origin/epic/quickfiler-bug-family-integration..HEAD | awk '$1==0 && $2>0'` returned **zero rows**, so no file loses content the base gained.
- Shared project file: `QuickFiler.Test.csproj` retains #493's added includes
  `Controllers\QfcItemController.UiThreadDispatcherFixture.cs` and
  `Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs` alongside this feature's single
  added include `Controllers\QfcCollectionControllerNavigationDigitsTests.cs`. This feature's diff
  against the merged base is exactly one added line in that file.
  `QuickFiler/QuickFiler.csproj` is untouched.

Note for the epic: the ownership regions as declared overlap. This feature was assigned
`Controllers\Qfc*`, but #493's two added includes also match `Controllers\Qfc*`. No conflict arose
and nothing was lost, but the region declarations are not disjoint and would benefit from being
narrowed to a per-file list in future waves.
