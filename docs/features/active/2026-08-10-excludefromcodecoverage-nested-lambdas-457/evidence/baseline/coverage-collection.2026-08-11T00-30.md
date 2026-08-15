# [P0-T11] Repository coverage baseline

Timestamp: 2026-08-11T00-30
Command: `pwsh -NoProfile -Command '& { & "<repo-root>\scripts\vscode\Invoke-MSTestWithCoverage.ps1" -SearchRoot . -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml" } | ForEach-Object { "{0:o} {1}" -f [datetime]::UtcNow, $_ }'`
`<repo-root>` resolved at run time with `git rev-parse --show-toplevel` =
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a`. Not hard-coded.
The single-quoted-outer / double-quoted-inner quoting form mandated by Conventions was used verbatim
and executed correctly, emitting timestamped stdout lines.
EXIT_CODE: 1 (both attempts) — see "Deviation" below. The post-processing step that produces every
figure in this artifact completed with EXIT_CODE 0.

## Deviation from the plan's expectation, recorded not papered over

The canonical runner completed instrumentation and wrote the raw Cobertura artifact, then threw at
its own test-exit-code check (`Invoke-MSTestWithCoverage.ps1:236`,
`throw "MSTest with coverage failed with exit code $coverageExitCode"`) **before** reaching its
post-processing block at lines 333-343. Two pre-existing C# test failures cause that throw.

Two full attempts were made with the identical command. Both produced the same two failures with the
same exception, so this is deterministic on this branch head, not the known load-flake pattern:

| Run | Total | Passed | Failed | Duration |
|---|---|---|---|---|
| Attempt 1 (04:12 UTC) | 6435 | 6433 | 2 | 1.0204 min |
| Attempt 2 (04:14 UTC) | 6435 | 6433 | 2 | ~0.9 min |

Failing tests (identical in both runs), in `QuickFiler.Test`:

- `QuickFiler.Controllers.Tests.QfcItemController_InitializationTests.InitializeBool_ThroughThePumpHost_CompletesAndInitializesState`
- `QuickFiler.Controllers.Tests.QfcItemController_InitializationTests.InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates`

Both fail with
`System.InvalidOperationException: Invoke or BeginInvoke cannot be called on a control until the window handle has been created.`
at `QfcItemController.InvokeBeginInvoke` -> `QfcItemController.ToggleTips` ->
`QfcItemController.Initialize`, driven from `QuickFiler.Test.TestSupport.WinFormsPumpHost`. The debug
trace also carries repeated
`SvgRenderer load 'QuickFiler.resources': System.IO.FileNotFoundException`.

**This is a pre-existing condition on the branch head.** At the time of this measurement this
feature had modified no production file, no test file and no C# source of any kind — only evidence
artifacts and plan checkboxes (confirmed by the `[P0-T4]` `git status --porcelain` baseline). The
failures are in `QuickFiler.Test`, which this PowerShell-only feature does not touch, and scope
prohibition 3 forbids modifying any C# source file. The condition is therefore recorded and handed
up, not repaired here.

**How the required figures were nonetheless obtained, without weakening the measurement.** The
runner's own post-processing block was executed verbatim against the raw artifact the canonical
runner had just written, using the production function with the production arguments and the same
`$repoRoot` derivation `(Resolve-Path (Join-Path $ScriptRoot '..\..')).Path`:

```powershell
. (Join-Path $scriptRoot 'Invoke-MSTestWithCoverage.Helpers.ps1')
$repoRoot = (Resolve-Path (Join-Path $scriptRoot '..\..')).Path
$resolvedOutputPath = Join-Path $repoRoot 'coverage\coverage.cobertura.xml'
& {
    Write-Output 'Post-processing coverage XML for Koverage compatibility...'
    $xmlContent = Get-Content $resolvedOutputPath -Raw -Encoding UTF8
    $processedXmlContent = ConvertTo-KoverageCoberturaXml -XmlContent $xmlContent -RepoRoot $repoRoot
    Set-Content -Path $resolvedOutputPath -Value $processedXmlContent -Encoding UTF8 -NoNewline
    Write-Output "Done. Coverage artifact: $resolvedOutputPath"
} | ForEach-Object { '{0:o} {1}' -f [datetime]::UtcNow, $_ }
```

That block is a line-for-line copy of `Invoke-MSTestWithCoverageMain` lines 338-343. It is the same
transform, on the same document, through the same production code path, and it is stamped through the
same `ForEach-Object` timestamping wrapper the plan mandates. The identical procedure is applied at
`[P3-T7]`, so the before/after delta remains attributable solely to this feature's filter.
Post-processing EXIT_CODE: 0.

## Repository headline values (from the post-processed document element)

| Attribute | Value |
|---|---|
| `lines-covered` | **53663** |
| `lines-valid` | **62873** |
| `line-rate` | **0.853514** (85.3514%) |
| `branches-covered` | **12609** |
| `branches-valid` | **15956** |
| `branch-rate` | **0.790236** (79.0236%) |
| `complexity` | 24765 |
| `version` | 1.9 |
| `timestamp` | 1786421720 |

## Per-file figures

### `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`

| Measure | Value |
|---|---|
| `<class>` node count for this filename | 1 |
| class `name` | `QuickFiler.Viewers.BreadcrumbPopupUiOperations` |
| class `line-rate` | **0.906977** |
| class `branch-rate` | **0.883333** |
| count of class-level `<line>` elements | **258** |
| count of class-level `<line>` elements with `hits` > 0 | **234** |
| count of `<methods>/<method>` elements | 28 |
| count of method-level `<line>` elements | 82 |

258 matches the figure recorded in `issue.md` § Impact / Severity, confirming the post-#441
denominator for this file.

### `TaskVisualization/FlagTasks.cs`

| Measure | Value |
|---|---|
| `<class>` node count for this filename | 1 |
| class `name` | `TaskVisualization.FlagTasks.<>c` |
| class `line-rate` | **0** |
| class `branch-rate` | **0** |
| count of class-level `<line>` elements | **10** |
| count of class-level `<line>` elements with `hits` > 0 | **0** |
| count of `<methods>/<method>` elements | 1 |

The surviving merged class is named `TaskVisualization.FlagTasks.<>c` — a closure class. No
non-closure primary existed for this filename, confirming research §5.3's finding that no
`TaskVisualization.FlagTasks` class element exists for this file. `[P3-T7]` expects this file to
disappear from the report entirely; `[P3-T8]`'s substantive gate for it is therefore live, because
`[P0-T11]` records it as PRESENT.

## Post-processing wall-clock duration

Verbatim source timestamps from the timestamped stdout:

```
2026-08-11T04:16:27.2901433Z Post-processing coverage XML for Koverage compatibility...
2026-08-11T04:16:45.9314107Z Done. Coverage artifact: C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a\coverage\coverage.cobertura.xml
```

Derived duration: **18.6413 seconds** (04:16:45.9314107 − 04:16:27.2901433).

Recorded per `spec.md` § Performance constraints, which sets no explicit latency budget and requires
only that observed wall-clock post-processing time be recorded before and after.

## Test-assembly discovery

- Runner's printed count (verbatim): `Discovered 9 test assemblies.`
- Executing repository root resolved at run time (`git rev-parse --show-toplevel`, equivalently
  `(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path` from `scripts/vscode/`):
  `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a`
- Independently reproduced count using
  `Get-ChildItem -Path <resolvedSearchRoot> -Recurse -Filter '*.Test.dll'` filtered to `\bin\Debug\`
  and excluding `\obj\` and `\ref\`: **9**
- Reproduced count equals the runner's printed `N`: **yes** (9 = 9)

| # | Path with the resolved-root prefix stripped | begins with resolved root (state) | remainder contains `\.claude\worktrees\` (gate) |
|---|---|---|---|
| 1 | `\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` | True | False |
| 2 | `\SVGControl.Test\bin\Debug\SVGControl.Test.dll` | True | False |
| 3 | `\Tags.Test\bin\Debug\Tags.Test.dll` | True | False |
| 4 | `\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` | True | False |
| 5 | `\TaskTree.Test\bin\Debug\TaskTree.Test.dll` | True | False |
| 6 | `\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll` | True | False |
| 7 | `\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll` | True | False |
| 8 | `\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` | True | False |
| 9 | `\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll` | True | False |

GATE RESULT: zero discovered paths contain a `\.claude\worktrees\` segment after the resolved-root
prefix is stripped. No nested agent worktree's stale assemblies entered the run. No absolute path is
hard-coded in this measurement.

## Artifact copy rule

Post-processed `coverage\coverage.cobertura.xml` size: **10,482,935 bytes (9.997 MB)**. This exceeds
the 5 MB threshold, so the full dump was **omitted for size** and was NOT copied to
`<FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml`. The extract artifact required by the
plan's alternative branch was written instead:

`docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/coverage-baseline-extract.2026-08-11T00-30.md`

## Output Summary

Repository baseline measured against the post-#441 arithmetic: `lines-covered` 53663, `lines-valid`
62873, `line-rate` 0.853514, `branches-covered` 12609, `branches-valid` 15956, `branch-rate` 0.790236.
`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` at `line-rate` 0.906977, 258 lines, 234 covered.
`TaskVisualization/FlagTasks.cs` present as `TaskVisualization.FlagTasks.<>c`, `line-rate` 0, 10 lines,
0 covered. Post-processing wall-clock 18.6413 s. 9 test assemblies discovered, count reproduced
independently, none from a nested worktree. Full dump omitted for size (9.997 MB); extract artifact
written. Two pre-existing `QuickFiler.Test` WinForms pump-host failures (deterministic across two
runs) prevented the runner from reaching its own post-processing block; that block was executed
verbatim and the condition is recorded as a pre-existing, out-of-scope finding.
