# [P3-T7] Post-change repository coverage re-capture

Timestamp: 2026-08-11T01-56
Command: identical to `[P0-T11]` —
`pwsh -NoProfile -Command '& { & "<repo-root>\scripts\vscode\Invoke-MSTestWithCoverage.ps1" -SearchRoot . -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml" } | ForEach-Object { "{0:o} {1}" -f [datetime]::UtcNow, $_ }'`
`<repo-root>` resolved at run time (`git rev-parse --show-toplevel`) =
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a`. Not hard-coded. The
single-quoted-outer / double-quoted-inner quoting is mandatory and was used verbatim; the timestamped
stdout form is mandatory because the runner emits its two post-processing markers with no timestamps
and overwrites the raw Cobertura file in place.
EXIT_CODE: 1 (test-run exit; see below). The post-processing step that produces every figure here
completed with EXIT_CODE 0.

## Deviation — identical in kind and handling to [P0-T11]

The canonical runner again completed instrumentation, wrote the raw Cobertura artifact, and threw at
its own test-exit-code check (`Invoke-MSTestWithCoverage.ps1:236`) before reaching its post-processing
block, because of the same two pre-existing `QuickFiler.Test` failures:

```
Total tests: 6435
     Passed: 6433
     Failed: 2
```

`InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` and
`InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates`, both
`System.InvalidOperationException: Invoke or BeginInvoke cannot be called on a control until the
window handle has been created`. Counts are identical to both `[P0-T11]` attempts (6435 / 6433 / 2),
confirming this feature introduced no new failure and fixed none — it is PowerShell-only and
scope prohibition 3 forbids modifying C# source.

The runner's own post-processing block was then executed verbatim against the raw artifact, using the
same scratchpad script, the same production function, the same `$repoRoot` derivation and the same
timestamping wrapper as `[P0-T11]`. **The identical procedure on both sides means the before/after
delta is attributable solely to this feature's filter**, which is what `[P3-T8]`'s substantive gate
depends on.

## Repository headline values (from the post-processed document element)

| Attribute | Value |
|---|---|
| `lines-covered` | **53375** |
| `lines-valid` | **62401** |
| `line-rate` | **0.855355** (85.5355%) |
| `branches-covered` | **12541** |
| `branches-valid` | **15872** |
| `branch-rate` | **0.790134** (79.0134%) |
| `complexity` | 24765 |
| `version` | 1.9 |
| `timestamp` | 1786424753 |

## Per-file figures

### `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`

| Measure | Value |
|---|---|
| `<class>` node count for this filename | 1 |
| class `name` | `QuickFiler.Viewers.BreadcrumbPopupUiOperations` |
| class `line-rate` | **0.991453** |
| class `branch-rate` | **0.896552** |
| count of class-level `<line>` elements | **234** |
| count of class-level `<line>` elements with `hits` > 0 | **232** |
| count of `<methods>/<method>` elements | 28 |
| count of method-level `<line>` elements | 82 |
| class `complexity` | 124 (was 131) |

Class open tag (verbatim):

```xml
<class line-rate="0.991453" branch-rate="0.896552" complexity="124" name="QuickFiler.Viewers.BreadcrumbPopupUiOperations" filename="QuickFiler\Viewers\BreadcrumbPopupUiOperations.cs">
```

### `TaskVisualization/FlagTasks.cs`

**absent (all classes removed)**

`SelectNodes('//class[@filename="TaskVisualization\FlagTasks.cs"]')` returns a node count of **0**,
and a direct `SelectSingleNode` for the same filename returns `(absent)`. This is the expected and
correct measured outcome, not a missing measurement: every member of the type is attributed, so the
sole class present at baseline was the closure class `TaskVisualization.FlagTasks.<>c` whose only
method `<InitializeToDoList>b__13_0` resolves to a declaring member absent from the presence set. The
filter dropped that method, retained zero methods, removed the `<class>` element, and the filename
consequently disappeared from the report entirely — the correct semantic for a wholly exempt file.

## Post-processing wall-clock duration

Verbatim source timestamps from the timestamped stdout:

```
2026-08-11T05:06:14.2192468Z Post-processing coverage XML for Koverage compatibility...
2026-08-11T05:06:33.3568363Z Done. Coverage artifact: C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a\coverage\coverage.cobertura.xml
```

Derived duration: **19.1376 seconds** (05:06:33.3568363 − 05:06:14.2192468).

Baseline `[P0-T11]` duration was 18.6413 s. The filter's additional linear pass over the DOM costs
**+0.4963 s**, about 2.7%. `spec.md` § Performance constraints sets no explicit latency budget and
requires only that observed wall-clock post-processing time be recorded before and after; the observed
cost is immaterial against the roughly 60-second C# test run that produces the report.

## Test-assembly discovery (P0-T11 rule applied by reference)

- Runner's printed count (verbatim): `Discovered 9 test assemblies.`
- Executing repository root resolved at run time:
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
hard-coded.

## Artifact copy rule

Post-processed `coverage\coverage.cobertura.xml` size: **10,446,316 bytes (9.962 MB)**, above the 5 MB
threshold. The full dump was **omitted for size** and was NOT copied to
`<FEATURE>/evidence/qa-gates/coverage-final.cobertura.xml`. The extract artifact was written instead:

`docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/qa-gates/coverage-final-extract.2026-08-11T01-56.md`

## Output Summary

Post-change repository figures: `lines-covered` 53375, `lines-valid` 62401, `line-rate` 0.855355,
`branches-covered` 12541, `branches-valid` 15872, `branch-rate` 0.790134.
`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` at `line-rate` 0.991453, 234 lines, 232 covered.
`TaskVisualization/FlagTasks.cs` absent (all classes removed). Post-processing wall-clock 19.1376 s
(+0.4963 s versus baseline). 9 test assemblies, count reproduced, none from a nested worktree. Full
dump omitted for size; extract artifact written. The same two pre-existing `QuickFiler.Test` failures
as the baseline, with identical counts.
