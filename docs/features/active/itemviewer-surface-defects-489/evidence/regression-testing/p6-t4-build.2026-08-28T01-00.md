# P6-T4 — Solution analyzer rebuild after the #489 guard and `UiScheduler` deletions

Timestamp: 2026-08-28T01-00
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

`EXIT_CODE: 0`. `Build succeeded.` with `5 Warning(s)` and `0 Error(s)` across 18 projects.

WarningCount: 5
BaselineAnalyzerWarningCount: 5
ErrorCount: 0

The warning count is **not greater than** `BaselineAnalyzerWarningCount:` from P0-T11 — it is equal
to it. All five are the identical pre-existing `System.Reactive` `packages.config` advisory raised by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`, one each
for `QuickFiler`, `TaskMaster`, `ToDoModel`, `UtilitiesCS` and `UtilitiesCS.Test`. A count of lines
matching `: (warning|error) CS[0-9]+` returns **0**, so there is no `CS` diagnostic and no analyzer
diagnostic of any kind.

This is a solution-level build and keeps the spaced platform spelling `"/p:Platform=Any CPU"`
verbatim; the project-level substitution recorded at P5-T4 does not apply. `/v:normal` is appended so
the non-vacuity count below can be taken, which changes verbosity only.

## The gate is non-vacuous

SkippingCoreCompileCount: 0
CoreCompileInvocations: 103
ProjectsBuilt: 18

The literal `Skipping target "CoreCompile"` occurs **0** times in the `/v:normal` log, proving the
analyzers actually ran over the recompiled sources rather than being skipped by an incremental
up-to-date check.

## No `using` directive was removed by this task

The plan is explicit that this task removes no `using` directive, and none was removed. The measured
justification holds on this build: a count of lines mentioning `IDE0005` or `CS8019` in the full
`/v:normal` log returns **0**.

`IDE0005` cannot be emitted in this repository's non-SDK projects — `QuickFiler/QuickFiler.csproj`
wires only Meziantou.Analyzer, Roslynator.Analyzers, AsyncFixer,
Microsoft.CodeAnalysis.BannedApiAnalyzers and SonarAnalyzer.CSharp, none of which produces it; there
is no `.globalconfig`; and `.editorconfig` configures no `IDE0005` severity. The compiler's `CS8019`
is hidden severity, which `/p:TreatWarningsAsErrors=true` does not promote.

Consequently `QuickFiler/Viewers/ItemViewer.cs:10`, `QuickFiler/Viewers/IItemViewer.cs:4` and
`QuickFiler/Viewers/ItemViewerExpanded.cs:1` all stay in place even though this plan's deletions
leave some of them without a consumer. Leaving `ItemViewer.cs:10` is load-bearing: it sits above
`ItemViewer.cs:20`, and P10-T17 together with the plan's § Intra-file shift section depend on that
`[ExcludeFromCodeCoverage]` attribute keeping line number 20 for the whole plan. Line 10 was verified
to still read `using System.Threading.Tasks;` and line 20 to still read `[ExcludeFromCodeCoverage]`
after the P6-T3 deletions.

## What changed in this phase

- `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` — P6-T1 added the `InvokeRequired`
  guard to `HtmlDarkConverter`, mirroring the `NavigateToString` pair at
  `QfcItemController.EventWiring.cs:140-147`. The diff against `BASELINE_SHA` is a single hunk at
  `@@ -289,12 +289,32 @@`, wholly inside that member; no other member is touched and the file is 358
  lines, under the 500-line ceiling.
- `QuickFiler/Viewers/IItemViewer.cs` — P6-T2 removed the `UiScheduler` declaration (0 added,
  1 deleted).
- `QuickFiler/Viewers/ItemViewer.cs` — P6-T3 removed the `_uiScheduler` capture and the `UiScheduler`
  property.

A zero-error build is itself evidence that removing the interface member broke no implementer and no
consumer: `ItemViewer` is the only type that both implements `IItemViewer` and declared the member,
and every remaining `_itemViewer.UiScheduler` reference in the tree is commented out
(`EfcItemController.cs:918`, `:927`, `QfcItemController.ViewerSetup.cs:397`).

Output Summary: `TaskMaster.sln` rebuilds at `EXIT_CODE: 0` with `Build succeeded.`, `5 Warning(s)`
and `0 Error(s)` across 18 projects — equal to, and therefore not greater than, the P0-T11 baseline
warning count. All five are the pre-existing `System.Reactive` advisory and there is no `CS` or
analyzer diagnostic. The gate is non-vacuous: `Skipping target "CoreCompile"` occurs 0 times with 103
`CoreCompile` invocations. No `using` directive was removed, and a repo-wide count of `IDE0005` and
`CS8019` occurrences in the log returns 0, confirming the analyzer set cannot report the now-unused
directives this plan's deletions leave behind.
