# P4-T7 — Repository-wide test and coverage refresh (Phase 4, loop iteration 1)

Timestamp: 2026-08-28T04-16
Task: [P4-T7]
LoopIteration: 1
Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\coverage.cobertura.xml
EXIT_CODE: 0

FinalLineRate: 0.851617
FinalBranchRate: 0.792335
FinalLinesCovered: 54420
FinalLinesValid: 63902
FinalRepoPassed: 6742
FinalRepoFailed: 0
FinalRepoSkipped: 0

This is the canonical runner. Its output document is Koverage-post-processed on a passing run —
repo-relative backslash `<class filename>` attributes, first-party packages only — which is the same
shape as the P0-T5 baseline figures, so every comparison below is like-for-like.
`coverage/coverage.cobertura.xml` is gitignored (`.gitignore:144`, `coverage/*`) and is **not** an
evidence artifact; only the figures read from it are recorded here.

## Runs performed

Two executions of the command were performed. **The gate is judged on the second**, which is the only
one with a captured exit code, and both are recorded in full with their counters as the plan requires.

| Run | Exit code | Total / Passed / Failed / Skipped | line-rate | branch-rate | lines-covered | lines-valid | branches-covered | branches-valid | line 481 hits |
|---|---|---|---|---|---:|---:|---:|---:|---:|
| 1 | not captured | 6742 / 6742 / 0 / 0 | 0.85157 | 0.792213 | 54417 | 63902 | 12940 | 16334 | 1 |
| **2 (gate)** | **0** | **6742 / 6742 / 0 / 0** | **0.851617** | **0.792335** | **54420** | **63902** | **12942** | **16334** | **1** |

**Why there are two runs, stated plainly.** Run 1 was launched detached, so its process exit code was
not capturable after the fact. An acceptance condition that requires `EXIT_CODE: 0` cannot be
satisfied by a run whose exit code was never observed, so run 1 is treated as an operational false
start and the gate run was executed in the foreground with its exit code captured. **This is not the
jitter branch.** The plan's single-re-execution allowance is scoped to the case where a run reports a
line rate *below* the baseline; no run here did, so that allowance is unused and remains available.
Run 1's figures are recorded above as corroboration, not as the judged run.

The two runs differ by 3 covered lines out of 63902 — a line-rate spread of 0.000047, consistent with
the ±2-covered-line instrumentation non-determinism the P0-T5 baseline measured on this same runner
(±0.000032). Both runs are above the baseline, so the verdict does not depend on which is taken.

## (a) Exit code, failures and skips

`EXIT_CODE: 0`. `Test Run Successful.`, `Total tests: 6742`, `Passed: 6742`. The runner's output
contains **zero** `Failed`, `Skipped` or `Not Run` result lines, so `FinalRepoFailed: 0` and
`FinalRepoSkipped: 0`.

`Discovered 9 test assemblies.` and `A total of 9 test files matched the specified pattern.` — the
same nine as the P0-T5 baseline, so the run scope has not narrowed.

The repository-wide total moved from the baseline's **6741** to **6742**: exactly the one test this
remediation adds, and nothing else.

## (b) Changed-line no-regression proof

The class with `filename="QuickFiler\Controllers\QfcItemController.EventWiring.cs"` is present in the
post-processed document (1 matching class, `QuickFiler.Controllers.QfcItemController`). The line this
remediation added is file line **481**,
`_itemViewer.PicturesChanged -= this.CbxPictures_CheckedChanged;`.

**Line 481 reports `hits="1"`.** It is covered.

The line is exercised by both `UnwireIntentEvents_DetachesPicturesChanged` and the sibling
`UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions`, since both call `UnwireIntentEvents()` on a
controller whose `_itemViewer` field is a live mock, so the statement executes on both paths.

This satisfies the changed-line no-regression requirement directly: the only production line this
remediation adds is covered, so coverage cannot have regressed on a changed line.

## (c) Denominator, and the shape it is measured in

| | Value |
|---|---:|
| `BaselineLinesValid:` (P0-T5) | 63901 |
| Production lines added by this remediation | 1 |
| Expected | 63902 |
| `FinalLinesValid:` observed | **63902** |

Exactly as predicted. The added detachment is one coverable statement, so the denominator grows by
exactly one. Test files are excluded from the denominator by the runner's `.*\.Test\.dll$` module
exclude, which is why the 24 lines added to `QfcItemController.EventWiringTests.Part2.cs` do not appear
in it. No explanation of a delta is required because there is no unexplained delta.

Both runs landed on the identical denominator of 63902, as the two baseline runs landed on an identical
63901 — the denominator is stable across runs; only the numerator jitters.

**Shape.** The document is in the Koverage-post-processed shape: 9 first-party packages
(`QuickFiler`, `SVGControl`, `Tags`, `TaskMaster`, `TaskTree`, `TaskVisualization`, `ToDoModel`,
`UtilitiesCS`, `VBFunctions`) and repo-relative backslash `<class filename>` attributes. That is the
same shape as the adopted baseline, so the comparison in (d) is like-for-like. The raw-shape figure
`line-rate 0.7051` at `lines-valid 82070` is **not** a comparable basis and is not used anywhere here.

## (d) Line rate not below the baseline

```
BaselineLineRate (P0-T5, post-processed shape) = 0.851567
FinalLineRate    (gate run, post-processed)    = 0.851617

0.851617 >= 0.851567    SATISFIED, by +0.000050 (0.005 percentage points)
```

Run 1 likewise: `0.85157 >= 0.851567`. Both runs clear the floor, so the verdict does not depend on
which run is taken.

Branch rate moved the same way and is recorded for completeness, though it is not a gate here:
`FinalBranchRate: 0.792335` against the baseline's `0.792213`.

No re-execution was required under the jitter clause, because no run reported a line rate below
`0.851567`. A numeric `FinalLineRate:` is on record, so this gate is not being reported PASS without
one.

## Tree hygiene

`git status --porcelain` after both runs shows **no** entry outside the feature folder. A first,
discarded launch of this command had its `-CoverageOutput` argument mangled by shell backslash
handling into a repo-root path, which produced a stray
`coveragecoverage.cobertura.xml.effective-coverage.config` at the repository root; that run was killed,
its process chain confirmed gone, and the stray file deleted before either recorded run. The argument
was quoted correctly for both recorded runs, and the runner echoed the intended
`...\coverage\coverage.cobertura.xml` destination each time.

## Acceptance

| P4-T7 condition | Result |
|---|---|
| (a) `EXIT_CODE: 0`, `FinalRepoFailed: 0`, `FinalRepoSkipped: 0` | **Yes** — 0, 0, 0 |
| (b) the added detachment line reports at least 1 hit | **Yes** — line 481 `hits="1"` |
| (c) `FinalLinesValid:` is 63902 | **Yes** — 63902, exactly baseline + 1 |
| (d) `FinalLineRate:` not lower than 0.851567 | **Yes** — 0.851617 |

Output Summary: The repository-wide gate **passes** on all four clauses. `EXIT_CODE: 0`,
`Test Run Successful.`, `FinalRepoPassed: 6742` (the baseline 6741 plus this remediation's one test)
with `FinalRepoFailed: 0` and `FinalRepoSkipped: 0` across the same nine discovered assemblies.
`FinalLinesValid: 63902` — exactly the baseline 63901 plus the one added production line, test
assemblies being excluded from the denominator — and `FinalLineRate: 0.851617`, which is **not lower
than** the P0-T5 baseline `0.851567`, measured like-for-like in the Koverage-post-processed shape (9
first-party packages, backslash filename attributes); `FinalBranchRate: 0.792335` against 0.792213.
The changed-line no-regression proof is direct: the added detachment at line 481 of
`QuickFiler\Controllers\QfcItemController.EventWiring.cs` reports `hits="1"`, executed by both the new
test and the sibling balance test. Two runs were performed and both are recorded in full; the gate is
judged on the second, the only one with a captured exit code, and the plan's jitter re-execution
allowance was not used because no run fell below the baseline.
