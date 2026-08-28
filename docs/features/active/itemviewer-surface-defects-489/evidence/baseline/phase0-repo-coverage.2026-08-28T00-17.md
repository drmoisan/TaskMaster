# Phase 0 — Repository-wide Coverage Baseline (P0-T14) — re-run, supersedes 2026-08-27T23-30

Timestamp: 2026-08-28T00-17
Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\coverage.cobertura.xml
EXIT_CODE: 1
ExpectedExitCode: 1

BaselineLineRate: 0.7051419519922018
BaselineBranchRate: 0.5921834815970319
BaselineLinesValid: 82070
BaselineLinesCovered: 57871
BaselineBranchesValid: 23719
BaselineBranchesCovered: 14046
BaselineRepoPassed: 6718
BaselineRepoFailed: 1
BaselineRepoSkipped: 0

## Supersession

This artifact supersedes `evidence/baseline/phase0-repo-coverage.2026-08-27T23-30.md`, which
recorded `BaselineLineRate: 0.13296151701059677` at `BaselineLinesValid: 8965` from a run that
discovered **one** test assembly, because the inherited `CS0006` analyzer version skew left eight
of the nine assemblies absent after the `/t:Rebuild` clean. Those figures were not a
repository-wide baseline and the superseded artifact said so. With the skew cleared for this
worktree without changing any tracked file, this run discovered **all nine** assemblies. The
superseded artifact is retained as the audit record of the blocked first attempt.

## DiscoveredAssemblies:

`Discovered 9 test assemblies.` / `A total of 9 test files matched the specified pattern.` Each
path below has the worktree-root prefix replaced by `<repo-root>`.

```
<repo-root>\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
<repo-root>\SVGControl.Test\bin\Debug\SVGControl.Test.dll
<repo-root>\Tags.Test\bin\Debug\Tags.Test.dll
<repo-root>\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
<repo-root>\TaskTree.Test\bin\Debug\TaskTree.Test.dll
<repo-root>\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
<repo-root>\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
<repo-root>\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
<repo-root>\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

**No entry contains a path segment equal to `.claude` after the prefix substitution.** This is the
satisfiable form the plan's § Execution conventions prescribes: the worktree root itself lies under
`.claude\worktrees\`, so a raw substring assertion against a `.claude` path fragment would be
unsatisfiable by construction. The script resolves its repo root from its own directory, which is
this worktree root, so discovery never reaches a nested worktree.

## Why EXIT_CODE is 1, and which of the two paths produced it

The non-zero exit came from the **failing-test** path, not from the coverage-threshold path.

`scripts/vscode/Invoke-MSTestWithCoverage.ps1:236` threw
`MSTest with coverage failed with exit code 1` after the run reported
`Total tests: 6719 / Passed: 6718 / Failed: 1 / Test Run Failed.` in 38.53 seconds.
`Assert-CoberturaLineCoverageThreshold` at `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-489`
was **not** reached: the throw at `:236` precedes the assert call at `:341`.

Consequence for the artifact shape, recorded so Phase 11 is not surprised by it: because the
script terminated at `:236`, the Koverage post-processing `Set-Content` at `:343` did not run
either, so `coverage/coverage.cobertura.xml` holds **raw dotnet-coverage output**. Its
`<class filename>` attributes are absolute paths rather than the repo-relative backslash form
P11-T9 matches. Any Phase 11 run that reaches the post-processing step will produce a
differently-shaped document, and the P11-T8 clause (a) denominator check exists precisely to
detect a denominator shift of that kind before any line-rate comparison is made.

### The one failing test

```
UtilitiesCS.Test.Threading.ProgressTrackerAsync_Tests.InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker
```

Failure message: `Expected threadException to be <null> because the STA thread must not throw, but
it threw: System.Threading.Tasks.TaskCanceledException: A task was canceled.` The stack originates
in `UtilitiesCS/Threading/ProgressTrackerAsync.cs:35` and the assertion is at
`UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:164`.

It is in `UtilitiesCS.Test`, an assembly this feature does not touch and may not touch: the plan's
P10-T9 asserts that **no** `UtilitiesCS` file appears in this feature's diff. The failure is a
load-sensitive STA-dispatcher timeout in a 24-worker parallel run, not a QuickFiler defect. The
scoped `QuickFiler.Test` run recorded by P0-T13 minutes earlier reported **1099 passed, 0 failed**.
This baseline records the failure rather than suppressing it; P11-T8 clause (c) is the relative
gate `FinalRepoFailed:` not greater than `BaselineRepoFailed:`, which this value of `1` sets.

## BaselineRepoSkipped is 0, not the non-zero value P0-T14 anticipated

P0-T14 expects a non-zero skip count from three files carrying a live `[Ignore(...)]`. Those
attributes are present and were re-verified on this branch head — five of them, not three:
`UtilitiesCS.Test/InputBox_Test.cs:11`, `UtilitiesCS.Test/ResourceTests.cs:17`, `:25` and `:108`,
and `UtilitiesCS.Test/YesNoToAll_Test.cs:10`. The two further occurrences the plan names are
commented out and contribute nothing, as the plan states:
`ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs:112` and
`ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs:231` both read
`//[Ignore("ProductionBugSuspected")]`.

The measured skip count is nevertheless `0`, because the MSTest adapter filters `[Ignore]`-marked
tests at discovery rather than reporting them as skipped results: the run counters reconcile
exactly as `passed 6718 + failed 1 = total 6719`, leaving no room for a skipped test, and the
console printed no `Skipped:` line at all.

P0-T14 states that whatever the run actually measured governs and asserts no repo-wide
zero-skipped gate, so `BaselineRepoSkipped: 0` is the recorded baseline. P11-T8 clause (d) asserts
**equality** against it, which is satisfiable: the same adapter behaviour will report `0` again
unless an `[Ignore]` attribute is added or removed, and this feature adds none.

## Acceptance

- `BaselineLineRate:` and `BaselineBranchRate:` are recorded as decimals; `BaselineLinesValid:`,
  `BaselineRepoPassed:`, `BaselineRepoFailed:` and `BaselineRepoSkipped:` are recorded as integers.
- `BaselineLinesValid: 82070` is a **positive** integer.
- No `DiscoveredAssemblies:` entry contains a path segment equal to `.claude` after the prefix
  substitution.
- `EXIT_CODE:` is not asserted to be `0`; `ExpectedExitCode: 1` is declared because the run exited
  non-zero, and `Output Summary:` states which of the two paths produced it.

All values were read from the root `<coverage>` element of `coverage/coverage.cobertura.xml`:
`line-rate="0.7051419519922018" branch-rate="0.5921834815970319" lines-covered="57871"
lines-valid="82070" branches-covered="14046" branches-valid="23719" complexity="25254"`.
`coverage/` is gitignored by a `coverage` directory rule and the raw XML is deliberately not an
evidence artifact.

Output Summary: The repository-wide coverage baseline is **captured across all nine test
assemblies**. Line rate **0.7051419519922018 (70.51%)** at `lines-valid=82070` /
`lines-covered=57871`; branch rate **0.5921834815970319 (59.22%)** at `branches-valid=23719`. The
denominator is the raw dotnet-coverage merge over every assembly the script discovered,
**including vendored and third-party code compiled into the first-party assemblies**, because the
script threw before its Koverage post-processing step; it is not a first-party-only denominator and
must not be compared against one. Run totals: 6718 passed, 1 failed, 0 skipped out of 6719 in 38.53
seconds. `EXIT_CODE: 1` came from the **failing-test** path at `Invoke-MSTestWithCoverage.ps1:236`,
**not** from `Assert-CoberturaLineCoverageThreshold`, which was never reached; `ExpectedExitCode: 1`
is declared accordingly. The single failure is
`UtilitiesCS.Test.Threading.ProgressTrackerAsync_Tests.InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker`,
a load-sensitive STA-dispatcher `TaskCanceledException` in an assembly this feature is forbidden to
touch; the scoped `QuickFiler.Test` run in P0-T13 was 1099 passed / 0 failed. `BaselineRepoSkipped:`
is `0` rather than the anticipated non-zero value because the MSTest adapter filters the five live
`[Ignore]` tests at discovery instead of reporting them as skipped. This run supersedes the
2026-08-27T23-30 artifact, whose 13.296% at `lines-valid=8965` came from a single discovered
assembly.
