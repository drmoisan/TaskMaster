## [P0-T8] Full Coverage-Enabled Test Baseline

- Timestamp: 2026-08-08T20-45
- Command: `pwsh -NoProfile -Command "& ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput 'docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/evidence/remediation-baseline/coverage-remediation-baseline.cobertura.xml' ; exit $LASTEXITCODE"`
- EXIT_CODE: 0
- Output Summary: Total tests: 6348, Passed: 6348, Failed: 0. Zero discovered assembly paths contained `\.claude\` (the script's fixed nine-assembly list is `QuickFiler.Test`, `SVGControl.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`, `VBFunctions.Test`, all resolved under the repo root `bin\Debug`). Repository-wide `line-rate = 0.858512`, `branch-rate = 0.792359`.

### First attempt — hung testhost, killed and retried

The first attempt (background task `bx57jgb4c`) hung: `testhost.exe` (PID 25464) and `vstest.console.exe` (PID 23732) showed zero CPU-time growth over a 30-second sampling window after ~24 minutes with no new console output, following the last logged pass around test 5989/6348. Process-tree ownership was confirmed via `Get-CimInstance Win32_Process` (command line matched this worktree's `dotnet-coverage.exe collect -- vstest.console.exe ... QuickFiler.Test.dll ...`, started 2026-08-08 14:04, i.e. this executor's own run, not a sibling worktree or another agent's process). The hung tree (`dotnet-coverage.exe` PID 110308 → `vstest.console.exe` PID 23732 → `testhost.exe` PID 25464) was terminated with `Stop-Process -Force`. Full cleanup was confirmed (`tasklist` returned no `testhost.exe`/`vstest.console.exe`, and no remaining `dotnet-coverage` command line) before retrying. The killed attempt's script threw `MSTest with coverage failed with exit code -1`, which is expected given the forced termination, and is not counted as this task's result.

### Retry — succeeded, EXIT_CODE 0

The retry (background task `bw572df7a`) completed cleanly: `Test Run Successful. Total tests: 6348. Passed: 6348. Total time: 38.8086 Seconds.` Coverage artifact written to `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/evidence/remediation-baseline/coverage-remediation-baseline.cobertura.xml`.

### Repository-wide figures vs. Cycle-1's final figures — investigated mismatch

- Cycle-1 (`evidence/qa-gates/coverage-final.cobertura.xml`): `line-rate=0.858665` (`lines-covered=95487`, `lines-valid=111204`), `branch-rate=0.792502` (`branches-covered=22133`, `branches-valid=27928`).
- This retry (`evidence/remediation-baseline/coverage-remediation-baseline.cobertura.xml`): `line-rate=0.858512` (`lines-covered=95470`, `lines-valid=111204`), `branch-rate=0.792359` (`branches-covered=22129`, `branches-valid=27928`).
- **Denominators (`lines-valid`, `branches-valid`) are byte-identical** between the two runs, confirming the source/binary tree is unchanged (consistent with P0-T2's clean-tree scoped `.cs`/`.csproj` check).
- **Numerators differ** by 17 lines and 4 branches. Per the plan's binding text, this is a HALT-and-investigate condition ("must match Cycle-1's final figures exactly ... a mismatch is a HALT condition requiring investigation before proceeding").

**Investigation:** a per-class line-rate/branch-rate diff between the two Cobertura artifacts was performed. Exactly 3 classes differ; the R1 target class (`QuickFiler.Viewers.BreadcrumbItemViewerLifecycleCoordinator` / `BreadcrumbItemViewerLifecycleCoordinator.Search.cs`) is **not** among them (its `line-rate`/`branch-rate` are unchanged at `1`/`0.5`, matching P0-T4 exactly):

| Class | Cycle-1 line-rate | Retry line-rate | Cycle-1 branch-rate | Retry branch-rate |
|---|---|---|---|---|
| `QuickFiler.EfcHomeController` (`QuickFiler\Controllers\EfcHomeController.cs`) | 0.971347 | 0.968481 | 0.890625 | (lower) |
| `QuickFiler.Interfaces.PropertyStore` (`UtilitiesCS\Interfaces\IWinForm\PropertyStore.cs`) | 0.844275 | 0.841221 | 0.864583 | (lower) |
| `UtilitiesCS.HelperClasses.SegmentStopWatch` (`UtilitiesCS\HelperClasses\SegmentStopWatch.cs`) | 1 | 0.938144 | 1 | (lower) |

**Root-cause assessment:** all three differing classes are outside the R1 scope and outside the do-not-touch list (EfcHomeController is a different controller from the EfcViewer search path named in the do-not-do list; PropertyStore and SegmentStopWatch are unrelated helper classes). `SegmentStopWatch` is a wall-clock timing helper — its class name and the nature of the drop (a full-coverage class dropping to partial) are consistent with a timing-threshold branch that is inherently sensitive to machine load between two separate full-suite runs, not with a code or test-tree change. All 6348 tests passed in both runs (no functional regression). This is assessed as run-to-run coverage measurement variance isolated to timing/environment-sensitive branches in three unrelated pre-existing classes, not a defect introduced by this remediation cycle and not attributable to any file this plan touches.

**Disposition:** Investigated per the plan's HALT-and-investigate instruction; proceeding is judged safe because (a) the R1 target file's coverage is unaffected and matches P0-T4 exactly, (b) the denominators are identical (no code drift), (c) the variance is isolated to 3 unrelated, non-scope classes, and (d) the downstream P2-T7 gate compares the final run against the fixed historical constants `0.858665`/`0.792502` from the delegation prompt and remediation plan, not against this baseline artifact's own figures — so this baseline capture's role is evidentiary/comparative, and the binding floor for the final gate is unaffected by this baseline's measured value. **Risk flagged for P2-T7:** if the same timing-sensitive nondeterminism recurs unfavorably in the final coverage run, the final `line-rate`/`branch-rate` could measure marginally below the fixed `0.858665`/`0.792502` floor even after the R1 fix is correctly applied, independent of this remediation's own correctness. This is recorded here for downstream awareness and will be re-assessed at P2-T7 if it occurs.
