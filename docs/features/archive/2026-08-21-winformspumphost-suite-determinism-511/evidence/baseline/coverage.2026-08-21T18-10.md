# Baseline — Numeric Coverage

Timestamp: 2026-08-22T09-47

Command:

```
pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput coverage\baseline.cobertura.xml
```

Run from the worktree root
`<repo-root>\.claude\worktrees\agent-ad37a256a0fb60243`. The script wraps the
same nine assemblies with `dotnet-coverage` and emits Cobertura XML; `vstest.console.exe
/EnableCodeCoverage` alone emits a binary `.coverage` file, not a percentage, which is why this
script is the source of numeric coverage in this plan.

EXIT_CODE: 0

Output Summary:

```
Discovered 9 test assemblies.
Test Run Successful.
Total tests: 6437
     Passed: 6437
Code coverage results: ...\coverage\baseline.cobertura.xml.
Done. Coverage artifact: ...\coverage\baseline.cobertura.xml
```

### The four required figures, as numeric percentages to two decimal places

| Figure | Cobertura attribute | Raw | Percent |
| --- | --- | --- | --- |
| Repository headline line rate | root `line-rate` | `0.855531` | **85.55%** |
| Repository headline branch rate | root `branch-rate` | `0.790312` | **79.03%** |
| `QuickFiler` package line rate | `QuickFiler` package `line-rate` | `0.8092566619915849` | **80.93%** |
| Changed-module rate (`QuickFiler\Controllers\QfcItemController*` classes, aggregated) | per-`<line>` count across the 10 matched classes | 1410 / 1633 | **86.34%** |

Supporting root counters: `lines-covered="53386"`, `lines-valid="62401"`,
`branches-covered="12547"`, `branches-valid="15876"`. The `QuickFiler` package branch rate is
`0.7491152182461659` (**74.91%**).

No coverage field above is empty and none carries the token `UNVERIFIED`.

### All nine packages

| Package | line-rate | Percent |
| --- | --- | --- |
| QuickFiler | 0.8092566619915849 | 80.93% |
| UtilitiesCS | 0.8955850144092219 | 89.56% |
| TaskVisualization | 0.8984326018808777 | 89.84% |
| SVGControl | 0.47303128371089537 | 47.30% |
| ToDoModel | 0.5731056563500534 | 57.31% |
| Tags | 0.9268929503916449 | 92.69% |
| TaskMaster | 0.7335945151811949 | 73.36% |
| TaskTree | 0.9548387096774194 | 95.48% |
| VBFunctions | 1 | 100.00% |

### Per-class figures for the changed module

Ten Cobertura classes have a `filename` beginning `QuickFiler\Controllers\QfcItemController`
(`MATCH_COUNT=10`). Filenames use backslashes because the script's Koverage post-processing rewrites
them; a forward-slash query matches nothing.

| Filename | line-rate | Percent | branch-rate |
| --- | --- | --- | --- |
| `QuickFiler\Controllers\QfcItemController.cs` | 1 | 100.00% | 0.7857142857142857 |
| `QuickFiler\Controllers\QfcItemController.Initialization.cs` | 0.949612 | 94.96% | 0.90625 |
| `QuickFiler\Controllers\QfcItemController.ViewerSetup.cs` | 0.850829 | 85.08% | 0.677419 |
| `QuickFiler\Controllers\QfcItemController.Conversation.cs` | 0.882353 | 88.24% | 0.944444 |
| `QuickFiler\Controllers\QfcItemController.FolderHandling.cs` | 0.952381 | 95.24% | 0.7 |
| `QuickFiler\Controllers\QfcItemController.EventWiring.cs` | 0.815182 | 81.52% | 0.65 |
| `QuickFiler\Controllers\QfcItemController.EventHandlers.cs` | 0.7865168539325843 | 78.65% | 0.6111111111111112 |
| `QuickFiler\Controllers\QfcItemController.Navigation.cs` | 0.90678 | 90.68% | 0.818182 |
| `QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs` | 0.793249 | 79.32% | 0.691176 |
| `QuickFiler\Controllers\QfcItemController.MailActions.cs` | 0.768 | 76.80% | 0.727273 |

### Counting method (must be reproduced exactly at post-change comparison)

The aggregate changed-module figure was computed by counting `<line>` elements inside each matched
`<class>`, deduplicated by line `number` **within** each class, and summing across the ten classes:
1,633 total lines, 1,410 covered, **86.34%**. Cobertura repeats line entries under `<method>` as well
as under the class-level `<lines>` element, so an all-descendant count without deduplication roughly
doubles the denominator and would fabricate a coverage delta. Any post-change comparison must use
this same per-class-deduplicated method against a **post-processed** XML produced by the same script.

## Acceptance conditions

1. **Artifact exists with all four fields** — met.
2. **The script reported exactly 9 discovered test assemblies** — met; the log line reads
   `Discovered 9 test assemblies.` This was independently corroborated before the run by replaying the
   script's own discovery filter (`*.Test.dll` under `\bin\Debug\`, excluding `\obj\` and `\ref\`),
   which returned exactly the nine canonical assemblies and nothing else.
3. **No coverage field contains `UNVERIFIED` or an empty value** — met.

Note on the `\.claude\` exclusion hazard: the script's filter (lines 296 through 302) does not exclude
`\.claude\`. All nine discovered paths do contain `\.claude\`, because this worktree itself lives
under `.claude\worktrees\`. That is expected and harmless: there is no nested `.claude/worktrees/`
inside this worktree (`ls -d .claude/worktrees` → no such file or directory), so no foreign or stale
agent-worktree assembly can be picked up. The count is exactly 9 and every path is one of the nine
canonical assemblies.

## Two earlier invocations, recorded for completeness and as pre-fix data

This figure came from the **third** invocation of the coverage script. The first two are recorded
rather than discarded, because the second carries directly relevant pre-fix evidence.

**Invocation 1 — discarded, wrong output path.** The `-CoverageOutput coverage\baseline.cobertura.xml`
argument was passed through the Bash tool, which consumed the backslash and produced
`coveragebaseline.cobertura.xml` at the worktree root instead of `coverage\baseline.cobertura.xml`.
The run itself was clean (`Test Run Successful. Total tests: 6437, Passed: 6437`). The misplaced file
was deleted and the invocation was repeated through a `pwsh` script file so no shell escaping applies.
`git status --porcelain` was re-checked afterwards and showed no stray entry.

**Invocation 2 — failed, and it is pre-fix evidence.** With the corrected path, the run reported
`Total tests: 6437, Passed: 6430, Failed: 7, Test Run Failed.` All seven failures were 60,000 ms
timeouts (`PumpTimeoutMs`), and all seven are WinFormsPumpHost-driven tests in the class this child
concerns:

```
Failed InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState [1 m]
Failed CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController [1 m 1 s]
Failed InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme [1 m]
Failed CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing [1 m]
Failed InitializeBool_ThroughThePumpHost_CompletesAndInitializesState [1 m]
Failed InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates [1 m]
Failed InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults [1 m]
```

**Both tests named by #511 and #571 are in that list.** The script threw at
`Invoke-MSTestWithCoverage.ps1:236` immediately after `dotnet-coverage` returned non-zero, which is
before the Koverage post-processing step, so that run's 17.6 MB XML was raw (forward-slash filenames,
unmerged `<class>` nodes) and not comparable to a post-processed measurement. It was therefore
overwritten rather than read.

The only environmental difference between invocation 2 and invocation 3 was machine load: 17 idle
MSBuild node-reuse processes left over from the P0-T13 and P0-T14 `/m` builds were still resident
during invocation 2 and were stopped before invocation 3. No stray `testhost`, `vstest.console`, or
`dotnet-coverage` process from any other agent was present at any point, so the failures were not
caused by a competing test runner.

**Disposition:** invocation 2 is treated as data about the race window, not as a reason to change the
remedy, exactly as the plan's Phase 1 instruction directs. It is carried forward into the P1-T6
intermittency analysis. It is **not** a pre-existing baseline failure in the sense of P0-T15: the
P0-T15 plain-`vstest` baseline recorded 6437/6437 with zero failures, and invocation 3 of this script
also recorded 6437/6437. The seven failures appear only intermittently and only under added load.
