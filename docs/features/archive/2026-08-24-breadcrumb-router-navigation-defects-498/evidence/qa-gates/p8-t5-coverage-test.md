# P8-T5 — Toolchain Step 4, Full-Suite Test with Coverage

Timestamp: 2026-08-26T11-25

Pass number: **3** — the final pass.

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-MSTestWithCoverage.ps1" -SearchRoot . -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

`Test Run Successful.` **6514 total, 6514 passed, 0 failed, 0 skipped. Repository-wide line coverage
84.83%.**

### Test counts

| Metric | Value |
|---|---:|
| Total tests | **6514** |
| Passed | **6514** |
| **Failed** | **0** |
| Skipped | 0 (none reported by the runner) |

Total test time 54.8732 seconds. The runner discovered **9 test assemblies** under `-SearchRoot .`
(`Discovered 9 test assemblies.`), the same nine as the `P0-T15` baseline. No sibling worktree's
assemblies were reachable: no other agent's worktree is nested inside this worktree, so the recursive
discovery cannot cross into one, and the discovery list contains no `\.claude\` path fragment.

`vstest.console.exe` ran with `/InIsolation` and the runner's standard
`/TestCaseFilter:TestCategory!=LiveOutlook`, both supplied by
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`. There was no test-host crash and no re-run per assembly
was needed.

Baseline comparison: `P0-T15` recorded 6482 total / 6482 passed / 0 failed. This run adds **32** tests
and keeps the failure count at zero. The 32 comprise the 29 regression and coverage tests authored by
Phases 2 through 6, plus the 3 added by the `P8-T7` remediation (pass-1 count was 6511).

### Failing-test identifiers

**The failing set is EMPTY.** There is no failing fully qualified identifier to list.

Acceptance conditions, both met absolutely:

- The failing identifier set is a SUBSET of the `P0-T15` `BASELINE_FAILURE_SET`. That baseline set is
  EMPTY, so the only satisfying run is one with zero failures — which this is.
- No failing identifier belongs to any of the six test classes this plan writes
  (`BreadcrumbBridgeRouterQueueTests`, `BreadcrumbBridgeRouterTests`,
  `OutlookFolderHierarchyProviderTests`, `FolderBreadcrumbBridgeRouterTests`, `BreadcrumbRowStateTests`,
  `BreadcrumbStateModelTests`) — vacuously met, since there is no failing identifier at all.

The four classes the task excludes from its second clause — `BreadcrumbBridgeRouterIssue439Tests`,
`FolderBreadcrumbAssetContractTests`, `BreadcrumbStateModelSelectorTests` and
`BreadcrumbStateModelSequenceTests` — were also all green, and each was independently confirmed by its
dedicated Phase 7 gate (`P7-T8` 10/10, `P7-T4` 15/15, `P7-T5` 9/9, `P7-T6` 32/32). Note the naming point
recorded in `p7-t6-ac22-400-residual.md`: `BreadcrumbStateModelSequenceTests` is a FILE name, not a type
name; its tests belong to the owned partial type `BreadcrumbStateModelTests` and are therefore governed
by the stricter of the two clauses, which is met.

### Repository-wide coverage — NUMERIC values

Read from the `line-rate` attribute of the root `<coverage>` element of the copied Cobertura file
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p8-t5-coverage.cobertura.xml`:

- `line-rate="0.84831"` → repository-wide line coverage **84.83%** (two decimal places)
- `lines-covered="53933"`, `lines-valid="63577"`
- `branch-rate="0.787866"` → **78.79%**; `branches-covered="12765"`, `branches-valid="16202"`
- `complexity="25122"`

| Metric | `P0-T15` baseline | This run | Delta |
|---|---:|---:|---:|
| Repository line rate | 84.78% (`0.847813`) | **84.83%** (`0.84831`) | **+0.05 pp** |
| Lines covered | 53770 | 53933 | +163 |
| Lines valid | 63422 | 63577 | +155 |
| Branch rate | 78.70% (`0.787`) | 78.79% (`0.787866`) | +0.09 pp |

No placeholder value appears above; every figure was measured in this run. The full delta analysis,
including the changed-line figure, is in `p8-t7-coverage-delta.md`.

### Artifact copied

`coverage/coverage.cobertura.xml` (gitignored) was copied to
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p8-t5-coverage.cobertura.xml`
(10,641,500 bytes), which is the only location this feature writes coverage to. No coverage file was
created under `artifacts/`.

### `ExpectedExitCode:` and degradation status

The observed exit code is 0, so no `ExpectedExitCode:` is declared. The `BASELINE_FAILURE_SET` is empty
and this run has zero failures, so no degradation branch was used; the gate met its primary condition.

### Pass history for this step

| Pass | Result |
|---:|---|
| 1 | `EXIT_CODE: 0`; 6511/6511 passed, 0 failed; line rate 84.81%. Superseded by the `P8-T7` remediation. |
| 2 | not reached — `P8-T1` rewrote a file and the loop restarted |
| 3 | `EXIT_CODE: 0`; 6514/6514 passed, 0 failed; line rate 84.83%. Terminal. |

### Invocation note

The recipe above is the plan's Coverage recipe verbatim and is what was executed. It was launched as a
DETACHED process from a wrapper held in the session scratchpad outside the repository, with its console
output captured to a scratchpad log, so that a foreground tool timeout could not orphan the vstest
runner mid-suite. No wrapper, script or log file was written anywhere under
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/`.
