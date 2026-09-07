# [P0-T20] Baseline full-suite test and coverage run

Timestamp: 2026-08-27T09-45
Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\coverage.cobertura.baseline.xml`
EXIT_CODE: 0

The raw Cobertura document is written to the gitignored `coverage` directory. Only the extracted
figures below are committed.

## Discovered test assemblies (paths expressed relative to `WS`)

The script reports `Discovered 9 test assemblies.` The nine, listed relative to `WS`:

```
.\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
.\SVGControl.Test\bin\Debug\SVGControl.Test.dll
.\Tags.Test\bin\Debug\Tags.Test.dll
.\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
.\TaskTree.Test\bin\Debug\TaskTree.Test.dll
.\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
.\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
.\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
.\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

**Discovery-contamination branch NOT taken.** `Invoke-MSTestWithCoverage.ps1` discovers assemblies by
absolute `FullName` and filters only `\bin\<Configuration>\`, `\obj\`, and `\ref\`, applying no
agent-worktree exclusion. This execution worktree itself sits under a `.claude` segment, so every
absolute `FullName` contains `\.claude\`; the plan's condition is therefore evaluated against the path
**relative to `WS`**, and **no relative path contains a `.claude` segment**. No nested worktree exists
under `WS` (`.claude/worktrees` does not exist inside this worktree), so no sibling's stale binaries
could be swept in. `CONTAMINATED-DISCOVERY:` is not recorded.

## Test result summary (verbatim)

```
Test Run Successful.
Total tests: 6686
     Passed: 6686
 Total time: 38.6436 Seconds
```

| Measure | Value |
| --- | --- |
| Total | 6686 |
| Passed | 6686 |
| Failed | 0 |
| Skipped | 0 |

```
BaselineFailureSet = none
```

## Repository-wide coverage figures

Root `<coverage>` element as written by the post-processed document:

```xml
<coverage line-rate="0.850393" branch-rate="0.791192" complexity="25249" version="1.9"
          lines-covered="54358" lines-valid="63921"
          branches-covered="12917" branches-valid="16326">
```

```
BaselineLineCoveragePercent   = 85.04
BaselineBranchCoveragePercent = 79.12
BaselineMeasurableLines       = 63921   (lines-valid)
```

**Threshold branch NOT taken.** `Assert-CoberturaLineCoverageThreshold` throws when the
repository-wide Cobertura `line-rate` is below 80 percent. The observed rate is 85.04 percent, so the
assertion passed and the post-processed document was written back normally.
`COVERAGE-THRESHOLD-THROW` is not recorded.

Against the repository policy floors, the baseline **already meets** both: line 85.04 percent is at or
above the `.claude/rules/general-unit-test.md` / `quality-tiers.md` floor of 85 percent, and it is
above `CLAUDE.md` §UT2's 80 percent. Branch 79.12 percent is above the 75 percent floor. This is
recorded as an observed fact; the binding condition for this feature is no regression against these
figures, evaluated by `[P4-T11]`.

## Per-file line rates for the two changed files `[P4-T11]` compares against

| File | `<class>` `line-rate` | `<class>` `branch-rate` |
| --- | --- | --- |
| `QuickFiler\Controllers\QfcItemController.Navigation.cs` | 0.90678 | 0.818182 |
| `QuickFiler\Controllers\KbdActions.cs` | 0.9397590361445783 | 1 |

No figure is recorded for `QuickFiler\Controllers\QfcCollectionController.cs`: that class carries
`[ExcludeFromCodeCoverage]` at its declaration (line 21, confirmed by `[P0-T13]`), so its lines are
outside every coverage denominator (decision D-P4).

## Evidence-location note

No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` line is written. Per decision D-P2, `spec.md`'s AC-QA-10
names the canonical `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/baseline/`
folder, which is a canonical `<kind>` under
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. No override occurred, so recording one
would place a rejection that did not happen into a committed artifact.

## Environmental note: two aborted attempts before this run

This baseline required three attempts. The first two are recorded here because they are an
environmental condition a later reader would otherwise have to rediscover, not a property of this
feature's diff.

| Attempt | Outcome | Evidence |
| --- | --- | --- |
| 1 | Died silently mid-`QuickFiler.Test` after 1060 lines of passing output. No Cobertura document written; empty stderr; the script's `finally` block did not run, leaving `coverage.cobertura.baseline.xml.effective-coverage.config` behind, which indicates abrupt process termination rather than a thrown exception. | — |
| 2 | Deadlocked at the same point. `testhost.exe` CPU time was flat at 24.03 seconds across three samples 5 seconds apart, with 34-35 threads alive: zero CPU consumption, so a deadlock rather than slow progress. Terminated deliberately. | — |
| 3 | **Succeeded.** Figures above. | this artifact |

During attempts 1 and 2 a sibling agent worktree (`agent-a0d0b74b…`) was running the same
`Invoke-MSTestWithCoverage.ps1` script concurrently against its own checkout, with its own
`dotnet-coverage collect` session and `vstest.console.exe` on the same machine. Attempt 3 was launched
only after that sibling's runner and all `testhost.exe`, `vstest.console.exe`, and
`dotnet-coverage.exe` processes had exited, and it completed in 38.6 seconds of test time with 6686 of
6686 tests passing. Two concurrent `dotnet-coverage collect` sessions on one machine are the probable
cause; no test-level defect was observed in either aborted attempt (every line of both partial logs
reads `Passed`). Nothing outside this worktree was deleted; only this worktree's own deadlocked
process chain was terminated.

## Acceptance evaluation

- `BaselineLineCoveragePercent` (85.04) and `BaselineBranchCoveragePercent` (79.12) are both numeric. PASS.
- `BaselineFailureSet` is explicitly present (`none`). PASS.
- No discovered assembly path expressed relative to `WS` contains a `.claude` segment. PASS.

Output Summary: 6686 of 6686 tests passed, 0 failed, 0 skipped; repo-wide line coverage 85.04 percent,
branch 79.12 percent, `lines-valid` 63921; `KbdActions.cs` 0.9398 line-rate,
`QfcItemController.Navigation.cs` 0.90678 line-rate; no coverage-threshold throw; no discovery
contamination; succeeded on the third attempt after two concurrency-induced aborts.
