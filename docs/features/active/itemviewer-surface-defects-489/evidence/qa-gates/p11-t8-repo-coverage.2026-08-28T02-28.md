# P11-T8 — Repository-wide coverage run, final QC (loop iteration 1)

Timestamp: 2026-08-28T02-28
Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\coverage.cobertura.xml
EXIT_CODE: 0
ExpectedExitCode: 0

FinalLineRate: 0.851567
FinalBranchRate: 0.792213
FinalLinesValid: 63901
FinalRepoPassed: 6741
FinalRepoFailed: 0
FinalRepoSkipped: 0

Loop iteration: **1**. The values above are from **run 2**, the re-execution clause (a) required and
the run whose document is the one now on disk at `coverage/coverage.cobertura.xml`. Both runs are
recorded in full below, as clause (a) requires.

## The document shape changed relative to the baseline, and that is the whole story of this gate

The P0-T14 baseline run **failed one test** and therefore threw at
`scripts/vscode/Invoke-MSTestWithCoverage.ps1:236`, before the post-processing step. Its
`coverage/coverage.cobertura.xml` held **raw `dotnet-coverage` output**: absolute
`<class filename>` attributes, no `<sources>` element, and every third-party assembly
`dotnet-coverage` instrumented at runtime still present as a `<package>`.

Both Phase 11 runs **passed every test**, so both reached
`ConvertTo-KoverageCoberturaXml` at `:340`, then `Assert-CoberturaLineCoverageThreshold` at `:341`,
then the `Set-Content` at `:343`. The document on disk is therefore the **Koverage-post-processed**
form. Verified directly on the file:

- `<sources><source>.</source></sources>` is present — injected by post-processing.
- `<class filename>` attributes are **repo-relative with backslash separators**, for example
  `QuickFiler\Controllers\EfcHomeController.cs`. This is what P11-T9 requires.
- Exactly **9** `<package>` elements remain, one per first-party project — `QuickFiler`,
  `UtilitiesCS`, `TaskVisualization`, `SVGControl`, `ToDoModel`, `Tags`, `TaskMaster`, `TaskTree`,
  `VBFunctions`. Third-party packages have been stripped.
- The console printed `Post-processing coverage XML for Koverage compatibility...` and
  `Done. Coverage artifact: …`, which are emitted only on the path through `:340`–`:343`.

`Assert-CoberturaLineCoverageThreshold` was therefore **reached** and **passed**: it asserts against
the post-processed content and the post-processed line rate is 85.16 percent, above the 80 percent
floor. `EXIT_CODE:` is `0`, so neither of the two non-zero paths P0-T14 records was taken and
`ExpectedExitCode: 0` is declared.

**The two documents have different denominators and are not directly comparable.** The raw
denominator counts vendored and third-party code compiled into or loaded by the first-party
assemblies; the post-processed denominator counts first-party packages only. Comparing 0.851567
against the baseline's 0.7051419519922018 would manufacture a false improvement of 14.6 points that
no code change produced.

## Clause (a) — denominator check, performed first

### Against the baseline as recorded (raw shape) — diverges, so the re-execution was performed

```
BaselineLinesValid (raw shape)   = 82070
FinalLinesValid    (processed)   = 63901
|63901 - 82070| / 82070          = 0.22138  ->  22.14 percent
```

That is greater than 5 percent, so the run was **re-executed once** exactly as clause (a) directs,
and both runs are recorded here before any line-rate comparison is made.

| Run | Command exit | Total / Passed / Failed | line-rate | branch-rate | lines-covered | lines-valid | branches-covered | branches-valid |
|---|---:|---|---|---|---:|---:|---:|---:|
| 1 | 0 | 6741 / 6741 / 0 | 0.851599 | 0.792151 | 54418 | 63901 | 12939 | 16334 |
| 2 | 0 | 6741 / 6741 / 0 | 0.851567 | 0.792213 | 54416 | 63901 | 12940 | 16334 |

The two runs land on the **identical denominator**, 63901, and their line rates differ by 0.000032 —
three thousandths of one percent, from two lines of `dotnet-coverage`'s own instrumentation
non-determinism. So the 22 percent gap against the baseline is **not** the parallelism-sensitive merge
instability clause (a) was written to catch. It is the document-shape change described above, and
re-executing cannot close it: any passing run produces the post-processed shape.

### Shape-matched denominator check — satisfied

To compare like for like, the baseline's own raw document — preserved from
`coverage/coverage.cobertura.xml` **before** this task's first run overwrote it, and still carrying
`line-rate="0.7051419519922018"` and `lines-valid="82070"` — was passed through the repository's own
`ConvertTo-KoverageCoberturaXml` from `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, with
`-RepoRoot` set to this worktree root. That is the identical function, from the identical file, that
the script applies at `:340`; no reimplementation and no hand arithmetic is involved. It yields the
baseline in the post-processed shape:

```
Baseline, post-processed shape:  line-rate 0.851185, branch-rate 0.7921,
                                 lines-covered 54395, lines-valid 63905,
                                 branches-covered 12935, branches-valid 16330,
                                 9 packages
```

Shape-matched denominator check:

```
|63901 - 63905| / 63905 = 0.0000626  ->  0.0063 percent   <=  5 percent   SATISFIED
```

Four lines of movement in a 63905-line denominator. The comparison in clause (b) is made against this
run, whose shape-matched denominator satisfies clause (a).

## Clause (b) — line rate not lower than baseline, like for like

```
Baseline line rate (post-processed shape) = 0.851185   (85.1185 percent)
Final    line rate (post-processed, run 2) = 0.851567  (85.1567 percent)
Final    line rate (post-processed, run 1) = 0.851599  (85.1599 percent)
```

**`FinalLineRate:` is not lower than `BaselineLineRate:` in the shape-matched comparison.** Run 2
exceeds the baseline by 0.000382 (0.038 percentage points) and run 1 by 0.000414; both are above it,
so the verdict does not depend on which run is taken. Branch rate likewise: 0.792213 and 0.792151
against 0.7921.

This satisfies the acceptance condition as a **relative no-regression** comparison, which is what the
criterion is. No absolute threshold is invented here. For completeness, the post-processed figure is
also above the repository's 80 percent floor, which is why
`Assert-CoberturaLineCoverageThreshold` did not throw.

### What is not obtainable, stated plainly

The **final run's raw-shape figure is not obtainable**. On a passing run the script overwrites the
raw document in place at `:343`, so the raw intermediate does not survive the run, and no switch in
the plan's prescribed command retains it. This artifact therefore reports the raw number for the
baseline and the post-processed number for both the baseline and the final runs, and makes its
verdict **only** in the post-processed shape where all three values exist. The two denominators are
named explicitly wherever a number appears; no delta is reported across shapes.

Summary of every number and the denominator it belongs to:

| Figure | Denominator shape | lines-valid | line-rate |
|---|---|---:|---|
| P0-T14 baseline, as recorded | raw `dotnet-coverage` merge | 82070 | 0.7051419519922018 |
| P0-T14 baseline, converted by the repo helper | Koverage post-processed | 63905 | 0.851185 |
| P11-T8 run 1 | Koverage post-processed | 63901 | 0.851599 |
| P11-T8 run 2 (on disk) | Koverage post-processed | 63901 | 0.851567 |
| P11-T8, raw shape | not obtainable — overwritten in place at `:343` | — | — |

## Clause (c) — failed count not greater than baseline

```
BaselineRepoFailed = 1
FinalRepoFailed    = 0        0 is not greater than 1.   SATISFIED
```

The single baseline failure was
`UtilitiesCS.Test.Threading.ProgressTrackerAsync_Tests.InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker`,
a load-sensitive STA-dispatcher `TaskCanceledException` in an assembly this feature is forbidden to
touch. **It passed in both Phase 11 runs.** Both reported `Test Run Successful.` with
`Total tests: 6741 / Passed: 6741` and no failure line at all, so every test in the search root
passed, that one included. Its passing is precisely why the script reached post-processing and the
document shape changed.

The total rose from 6719 to 6741, a delta of **+22**, which reconciles exactly with the +22
`QuickFiler.Test` delta P11-T7 recorded (1099 to 1121). No other assembly's test count moved.

## Clause (d) — skipped count equals baseline

```
BaselineRepoSkipped = 0
FinalRepoSkipped    = 0       equal.   SATISFIED
```

Both runs printed no `Skipped:` line, and the counters reconcile exactly as
`passed 6741 = total 6741`, leaving no room for a skipped test. This matches the mechanism P0-T14
recorded: the MSTest adapter filters the five live `[Ignore]` tests in `UtilitiesCS.Test` at
discovery rather than reporting them as skipped results. This feature adds and removes no `[Ignore]`
attribute.

## Assembly discovery

`Discovered 9 test assemblies.` / `A total of 9 test files matched the specified pattern.` — the same
nine as the baseline. Stripping the worktree-root prefix from each discovered path leaves no
remaining path segment equal to `.claude`, which is the satisfiable form of the confinement check
prescribed in § Execution conventions; a raw substring assertion would be unsatisfiable by
construction because this worktree root itself lies under `.claude\worktrees\`.

## Loop consequence

The stage passed and rewrote no tracked file. `coverage/coverage.cobertura.xml` is gitignored by the
`coverage` directory rule and is deliberately not an evidence artifact. No restart is triggered; the
loop proceeds to P11-T9, which reads the post-processed document run 2 left on disk.

Output Summary: The repository-wide coverage gate **passes** at loop iteration 1 with `EXIT_CODE: 0`,
`FinalLineRate: 0.851567`, `FinalBranchRate: 0.792213`, `FinalLinesValid: 63901`,
`FinalRepoPassed: 6741`, `FinalRepoFailed: 0`, `FinalRepoSkipped: 0`. Both Phase 11 runs passed every
test — including the environmental `UtilitiesCS.Test` STA test that was the baseline's single failure
— so both reached the Koverage post-processing step and produced a **different document shape** from
the baseline: repo-relative backslash `<class filename>` attributes, an injected `<sources>` element,
and third-party packages stripped to 9 first-party packages. Clause (a) against the recorded raw
baseline diverged by 22.14 percent (63901 against 82070), so the run was re-executed once as
directed; run 1 and run 2 landed on the identical 63901 denominator with line rates 0.851599 and
0.851567, proving the gap is the shape change and not merge instability. A shape-matched comparison
was then obtained by passing the retained baseline raw document through the repository's own
`ConvertTo-KoverageCoberturaXml`, giving a baseline of 0.851185 at `lines-valid=63905`: the
denominator check is then 0.0063 percent, well within 5 percent, and **0.851567 is not lower than
0.851185**, so the relative no-regression criterion is satisfied like-for-like. The final run's
raw-shape figure is **not obtainable**, because the script overwrites the raw document in place at
`:343` on a passing run; every number in this artifact is labelled with the denominator it belongs to
and no delta is reported across shapes. Clause (c) holds at 0 failures against a baseline of 1, and
clause (d) holds at 0 skipped equal to the baseline 0.
