# P0-T15 — Baseline Full-Suite Test and Coverage Run

Timestamp: 2026-08-26T08-48

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-MSTestWithCoverage.ps1" -SearchRoot . -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

Observed exit code: **0**. `Test Run Successful.`

### Test counts (measured in this execution worktree)

| Metric | Value |
|---|---:|
| Total tests | **6482** |
| Passed | **6482** |
| Failed | **0** |
| Skipped | **0** |

Total test time 2.6686 minutes. The runner discovered **9 test assemblies** under `-SearchRoot .`:
`QuickFiler.Test`, `SVGControl.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`,
`TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`, `VBFunctions.Test` — each from its
`bin\Debug` output. No other agent's worktree is nested inside this worktree, so the recursive
discovery could not reach a sibling worktree's assemblies; a `\.claude\` path-fragment exclusion was
verified unnecessary rather than assumed (the discovery list contains no such path).

`vstest.console.exe` ran with `/InIsolation` and with the runner's standard
`/TestCaseFilter:TestCategory!=LiveOutlook`, both supplied by
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`. The runner configures no TRX logger, so this run
produced no `.trx` file; the counts above are read from the vstest console summary. No results file
named after the machine or the user account was created.

### BASELINE_FAILURE_SET

```
(empty — zero failing tests at baseline)
```

The set is EMPTY. Consequences that bind the rest of the plan:

- `P8-T5`'s subset condition is satisfied only by a run with **zero** failing identifiers.
- The DEGRADATION branches of `P6-T17`, `P7-T4`, `P7-T5`, `P7-T6` and `P7-T8` are all unavailable,
  because each is gated on `BASELINE_FAILURE_SET` containing an identifier in the relevant class. Each
  of those five gates therefore stands at its primary condition, `failed 0`.
- `P7-T8`'s passed count equals its total count, since no baseline-failing identifier exists in
  `BreadcrumbBridgeRouterIssue439Tests` to subtract.

### Repository-wide coverage

Read from the `line-rate` attribute of the root `<coverage>` element of the copied Cobertura file
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/baseline/p0-t15-coverage.cobertura.xml`:

- `line-rate="0.847813"` → repository-wide line coverage **84.78%** (two decimal places).
- Supporting attributes on the same element: `lines-covered="53770"`, `lines-valid="63422"`,
  `branch-rate="0.787"`, `branches-covered="12677"`, `branches-valid="16108"`.

This 84.78% figure is the baseline that `P8-T7` compares the post-change repository-wide line rate
against. Every value in this artifact was MEASURED in this execution worktree in this run. No figure is
inherited from version 1.0 of this plan or from any other feature folder, which matters because pull
request #605 changed the coverage denominator by removing an `[ExcludeFromCodeCoverage]` attribute from
the unowned `QuickFiler/Controllers/EfcFormController.cs` (1084 lines). No placeholder values appear
above.

The runner's own `Assert-CoberturaLineCoverageThreshold` check (an 80% floor applied to this same root
`line-rate`) passed, which is why the Koverage post-processing step completed and the artifact was
written.

### Artifact copied

`coverage/coverage.cobertura.xml` (gitignored) was copied to
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/baseline/p0-t15-coverage.cobertura.xml`
(10,602,259 bytes) as the plan requires.

### Invocation note

The recipe above is the plan's Coverage recipe verbatim and is what was executed. It was launched as a
DETACHED process from a wrapper in the system temporary directory, and its console output was captured
to a temporary log, so that a foreground tool timeout could not orphan the vstest runner mid-suite. No
wrapper or log file was written anywhere under `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/`.

### Prerequisite that made this run possible

An earlier attempt at this task was correctly refused because the solution did not build in this
worktree and only one of the nine test assemblies existed. That build failure was a worktree
provisioning gap in the gitignored `packages/` directory, now corrected and documented in
`p0-t13-analyzer-rebuild.md`. With the solution building (`P0-T13` `EXIT_CODE: 0`), all nine assemblies
were present and the full baseline could be measured rather than approximated.
