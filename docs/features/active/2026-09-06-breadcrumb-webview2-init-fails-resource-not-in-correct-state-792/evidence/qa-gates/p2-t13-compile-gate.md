# [P2-T13] Phase 2 compile gate

- Issue: #792
- Timestamp: 2026-09-17T19-24
- Command: CMD-OUTLOOK (`Get-Process -Name OUTLOOK`, printed `OUTLOOK-CLOSED: true`), then CMD-BUILD-ANALYZE (`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`), then CMD-BUILD-NULLABLE (`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`), then `dotnet tool run csharpier check QuickFiler/Controllers QuickFiler/Viewers "QuickFiler/Helper Classes" QuickFiler.Test/Controllers QuickFiler.Test/Viewers "QuickFiler.Test/Helper Classes"` (all run from `coverage/plan792-helper.ps1` with the item worktree as the working directory; msbuild console output captured to the gitignored `coverage/p2-t13-analyze.log` and `coverage/p2-t13-nullable.log`, formatter output to `coverage/p2-t13-csharpier.log`)
- EXIT_CODE: 0
- Output Summary:
  - `OUTLOOK-CLOSED: true` (printed first; no `HALT:` line)
  - CMD-BUILD-ANALYZE: `EXIT_CODE: 0`; `Build succeeded.`; `    0 Warning(s)`; `    0 Error(s)` (exact-line match `^\s+0 Error\(s\)$` = true); 16 s; 11871 log lines
  - CMD-BUILD-NULLABLE: `EXIT_CODE: 0`; `Build succeeded.`; `    0 Warning(s)`; `    0 Error(s)` (exact-line match = true); 17 s; 11708 log lines
  - csharpier scoped check: `EXIT_CODE: 0`; `Checked 308 files in 2385ms.` (the exit-0 branch of the acceptance held; no reference to `BASELINE-DRIFT-SET` was needed)

## Non-vacuity of both Rebuilds (from the captured logs)

| Gate | CSC-INVOCATIONS | CORECOMPILE-SKIPPED | PROJECTS-DONE-REBUILD | QuickFiler.dll rewritten | QuickFiler.Test.dll rewritten |
|---|---|---|---|---|---|
| analyze | 36 | 0 | 18 | 19:24:14 | 19:24:17 |
| nullable | 36 | 0 | 18 | 19:24:31 | 19:24:34 |

msbuild was resolved from `PATH` (`MSBUILD-ON-PATH: true`).

## Earlier attempts inside this task (recorded, not counted)

1. Attempt 1 (19:21): CMD-BUILD-ANALYZE failed with `1 Error(s)`: `QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs(47,13): error CS1061: 'SynchronizationContext' does not contain a definition for 'GetAwaiter'`. Cause: the moved `InitializeWebViewAsync` body awaits `_itemViewer.UiSyncContext`, which resolves through the extension `UiThread.GetAwaiter(this SynchronizationContext)` in namespace `UtilitiesCS` (`UtilitiesCS/Threading/UiThread.cs:211`); the retained `EfcItemController.cs` imports `UtilitiesCS` (line 21) but the using list [P2-T3] names for the new part does not. Repair: added `using UtilitiesCS;` to the new part (now 70 lines, ceiling 80; CRLF, no BOM; formatter-clean). Deviation from the plan's stated using list, recorded here.
2. Attempt 2 (19:23): discarded as vacuous. The helper's build function named its parameter `$args`, which PowerShell's automatic `$args` shadows inside a function, so msbuild ran with no arguments (default `/t:Build`, no properties): `CORECOMPILE-SKIPPED: 13`, `PROJECTS-DONE-REBUILD: 0`, and the "nullable" pass ran in 1 s with 0 `csc` invocations. The parameter was renamed and a guard added (`throw 'build arguments missing'` when fewer than five arguments are bound). This attempt's scoped formatter check was, however, a genuine observation and reported two unformatted files (below).
3. Attempt 3 (19:24): the run recorded above.

## Formatter repair inside this task (new files only, per the task's repair clause)

The attempt-2 scoped check reported `Was not formatted` for two Phase 1 test files, both created new in Phase 1 and neither in `BASELINE-DRIFT-SET` (empty). Each was repaired with `dotnet tool run csharpier format <that file>` and re-checked:

| File | SHA-256 before | SHA-256 after | Lines after | Ceiling |
|---|---|---|---|---|
| `QuickFiler.Test/Viewers/WebView2BreadcrumbHostIssue792Tests.cs` | A56A4298214DE585DB52E6F4AB2F264C5D2DD14A87D59D207A63C94A3BC1FCC3 | 961366098F99D4C8FD0D70D2A6BBEADD9CCDBAFF4380AAF606F7451433045543 | 153 | 200 |
| `QuickFiler.Test/Helper Classes/EfcViewerQueueIssue792Tests.cs` | B333546067323A370FBB2CD4BB8DE7338C41F3156FEE907F2443A1EA8E4FBCC2 | 8F319773F59B7CE351C72973E5C396DEDC9078FC9A991728BC5EED128A2C7422 | 40 | 120 |

Both files remained CRLF with no BOM after the rewrite. The re-check over the three test directories printed `Checked 173 files` with exit 0, and the full scoped check in attempt 3 printed `Checked 308 files` with exit 0. Every Phase 2 file (nine new production parts, twelve edited files) had already passed a per-file read-only check before the gate ran.

## Phase 2 line counts at this gate (`(Get-Content -LiteralPath <path>).Count`)

| File | Lines | Bound |
|---|---|---|
| `QuickFiler/Controllers/EfcFormController.cs` | 266 | 400 |
| `QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs` | 243 | 400 |
| `QuickFiler/Controllers/EfcFormController.EventHandlers.cs` | 383 | 400 |
| `QuickFiler/Controllers/EfcFormController.Actions.cs` | 184 | 400 |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | 125 | 160 |
| `QuickFiler/Controllers/EfcFormController.Helpers.cs` | 270 | 400 |
| `QuickFiler/Viewers/WebView2EnvironmentContract.cs` | 53 | 60 |
| `QuickFiler/Controllers/EfcItemController.cs` | 1076 | 1076 or 1077 |
| `QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs` | 70 | 80 |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2306 | 2306 or 2307 |
| `QuickFiler/Controllers/QfcCollectionController.PopOut.cs` | 93 | 120 |
| `QuickFiler/Controllers/EfcDataModel.cs` | 464 | 464 or 465 |
| `QuickFiler/Controllers/EfcDataModel.Carry.cs` | 62 | 90 |
| `QuickFiler/Controllers/QfcItemController.cs` | 340 | 345 |
| `QuickFiler/Controllers/EfcHomeController.cs` | 464 | 470 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 421 | (edited; numstat 14 0) |
| `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs` | 73 | (edited; numstat 6 0) |
| `QuickFiler/Helper Classes/EfcViewerQueue.cs` | 108 | (edited; numstat 7 0) |

Compile items added to `QuickFiler/QuickFiler.csproj` (numstat `9 0` against BASE-SHA; no trailing newline, matching the base file): `Controllers\EfcDataModel.Carry.cs`, `Controllers\EfcFormController.Actions.cs`, `Controllers\EfcFormController.Breadcrumb.cs`, `Controllers\EfcFormController.EventHandlers.cs`, `Controllers\EfcFormController.Helpers.cs`, `Controllers\EfcFormController.SetupAndProperties.cs`, `Controllers\EfcItemController.WebViewEnvironment.cs`, `Controllers\QfcCollectionController.PopOut.cs`, `Viewers\WebView2EnvironmentContract.cs`; each a bare self-closing element immediately after the neighbour its task names.

## Conservation gates (Phase 2 pure moves)

| Task | Type | Before multiset | After multiset | CONSERVATION-DIFF-COUNT | Positive control (a result file omitted) |
|---|---|---|---|---|---|
| [P2-T1] | `EfcFormController` | 875 | 875 | 0 | 56 (without `Breadcrumb.cs`) |
| [P2-T3] | `EfcItemController` | 739 | 739 | 0 | 37 (retained file only) |
| [P2-T4] | `QfcCollectionController` | 1533 | 1533 | 0 | 15 (retained file only) |
| [P2-T5] | `EfcDataModel` | 320 | 320 | 0 | 24 (retained file only) |

Instrument correction, recorded: the plan's gate reads the base file through `git show`, and the console decodes that byte stream as code page 437 by default (`[Console]::OutputEncoding` = `ibm437`) while `Get-Content` decodes the result files as UTF-8; the base file's UTF-8 BOM also arrives as U+FEFF on its first line, which `Trim()` does not remove. Run literally, the [P2-T1] gate printed `CONSERVATION-DIFF-COUNT: 3` whose three entries were one mis-decoded em-dash (U+2014 read as `0393 00C7 00F6`) on each side and the BOM-prefixed `using System;` line; the files on disk were verified byte-correct (`git diff --numstat` for the retained file is `1 1056`, i.e. only the declaration line changed). The gate was therefore run with `[Console]::OutputEncoding` set to UTF-8 for the `git show` read and with a leading U+FEFF stripped from the first line; the multiset semantics are unchanged. The uncorrected instrument was re-run once for the record (`UNCORRECTED-INSTRUMENT-DIFF-COUNT: 3`).
