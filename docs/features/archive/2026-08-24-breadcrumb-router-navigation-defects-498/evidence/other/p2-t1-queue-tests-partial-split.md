# P2-T1 — Partial-Class Split of BreadcrumbBridgeRouterQueueTests

Timestamp: 2026-08-26T09-15

Command: `pwsh -NoProfile -Command '$vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Rebuild /m "/p:Configuration=Debug" "/p:Platform=Any CPU" "/p:EnableNETAnalyzers=true" "/p:EnforceCodeStyleInBuild=true"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

The decision-D8 partial split of the queue test class is in place and the solution rebuilds clean
under the analyzer gate: `5 Warning(s)`, `0 Error(s)`, `EXIT_CODE: 0`. All five warnings are the
pre-existing `System.Reactive.PackagesConfigCheck` `packages.config` advisory emitted once per
`packages.config` project (`ToDoModel`, `QuickFiler`, `TaskMaster`, `UtilitiesCS.Test` and
`UtilitiesCS`). None names a file on this plan's written-file list. The count matches the
`P0-T13` baseline, which recorded the same advisory set at `EXIT_CODE: 0`.

### Acceptance checks

| # | Condition | Observed | Result |
|---:|---|---|:--:|
| 1 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` at or under 500 lines | 462 lines (unchanged) | PASS |
| 2 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs` exists | exists, 10 lines | PASS |
| 3 | csproj carries `Controllers\BreadcrumbBridgeRouterQueueTests.Part2.cs` on the line immediately following `Controllers\BreadcrumbBridgeRouterQueueTests.cs` | line 58 is the `.cs` entry, line 59 is the `.Part2.cs` entry | PASS |
| 4 | Analyzer Rebuild recipe returns `EXIT_CODE: 0` | 0 | PASS |

### Changes made

- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs:23` — `public class` became
  `public partial class`. That is the only change to the file; the `[TestClass]` attribute at `:22`
  and the shared `[TestInitialize] Setup()` block at `:34-74` stay in the primary file, so the new
  part carries no attribute of its own.
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs` — new file declaring
  `public partial class BreadcrumbBridgeRouterQueueTests` in namespace `QuickFiler.Test.Controllers`.
  It is created empty of test methods; `P2-T2`, `P2-T6`, `P3-T1`, `P3-T5` and `P3-T6` add the twelve
  planned methods to it. It carries no `using` directives yet, so that no unused-directive
  diagnostic is introduced ahead of the code that needs them.
- `QuickFiler.Test/QuickFiler.Test.csproj:59` — one new `Compile Include` entry, inserted
  immediately after the existing entry for the file it splits. The item group was not re-sorted.

### CRLF preservation

The binding CRLF rule was observed. The project file measured 477 CRLF-terminated lines of 477
before the edit and 478 of 478 after, and `git diff --stat` reports the project file as
`1 insertion(+)` with no deletions, confirming that no other line's terminator was rewritten. The
insertion was performed with an explicit `\r\n` rewrite rather than `sed -i`. The new `.cs` file is
CRLF-terminated and BOM-free, matching the file it splits.
