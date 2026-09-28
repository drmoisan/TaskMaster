# P0-T14 — C# Test and Coverage Baseline (unmeasurable at merge-base)

Timestamp: 2026-09-19T22-58

Command:

```
pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .
```

Invoked with `-WorkingDirectory` set to the execution worktree and an absolute script path.
`pwsh -File` resolves a relative script path against the caller's own working directory before
`-WorkingDirectory` takes effect, and the first attempt with the relative form ran the **session**
worktree's copy of the script and threw `No test assemblies found`. The absolute form runs the same
script with the same `-SearchRoot .` argument against the correct checkout.

EXIT_CODE: 1

## Blocking diagnostic, verbatim

Absolute worktree prefixes are replaced by `<repo-root>` per the repository's
no-absolute-host-paths rule.

```
Exception: <repo-root>\scripts\vscode\Invoke-MSTestWithCoverage.Threshold.ps1:54:9
Line |
  54 |          throw "Cobertura line coverage $formattedPercentage% is below .
     |          ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
     | Cobertura line coverage 3.4321% is below the required 80% threshold.
```

## What the run did produce

| Measurement | Value |
|---|---|
| Test run result | `Test Run Successful.` |
| Total tests | 141 |
| Passed | 141 |
| Failed | 0 |
| Skipped | 0 |
| Total time | 3.3702 seconds |
| Cobertura line coverage | 3.4321 percent |
| Cobertura branch coverage | not printed |
| Coverage document | `coverage/coverage.cobertura.xml` |
| Test-result document | `coverage/test-results/mstest-coverage-run.trx` |

## coverage unmeasurable at merge-base; cause: Cobertura line coverage 3.4321% is below the required 80% threshold.

The 3.4321 percent figure is not the repository's coverage. It is an artefact of the merge-base
tree not compiling. P0-T11 and P0-T12 both ran `/t:Rebuild` against the solution and both failed at
analyzer reference resolution for defect #898, and `Rebuild` cleans before it builds, so the
`bin/Debug` output of every project downstream of `VBFunctions` and `UtilitiesCS` was deleted and
never rebuilt. The coverage runner therefore discovered only the small subset of test assemblies
whose projects do not depend on those two, ran their 141 tests, and measured them against the
first-party denominator of the whole repository. The denominator is complete; the numerator covers
a fraction of it.

Two consequences follow, and both are recorded rather than worked around:

- **No branch-coverage percentage exists for this run at all.** The runner's threshold gate throws
  on the line figure at `Invoke-MSTestWithCoverage.Threshold.ps1:54`, which executes before the
  branch check, before the Koverage post-processing completes, and before the projection and
  test-result summary are written. `coverage/` after the run contains the raw
  `coverage.cobertura.xml` and the trx and neither a `.jacoco.xml` projection nor a summary file.
  The `Output Summary:` therefore takes the second of this task's two permitted forms — naming the
  blocking diagnostic verbatim — because the first form requires both percentages and only one was
  printed.
- **No permitted evidence form was produced, so none is copied.** Gate rule 12 prohibits committing
  the raw collector document and the raw test-platform document in any form, including under a
  feature folder's evidence tree, and permits the projection, the one-line first-party summary and
  the trx-derived test-result summary in their place. This run produced none of the three. Both
  documents it did produce stay in `coverage/`, which `.gitignore:144` ignores, and neither is
  copied into the evidence tree or named in any commit pathspec.

## Numeric successor

**P2-T7 is the numeric C# coverage baseline for this change.** It is the first point in the plan at
which the solution compiles — after P1-T9 corrects the 15 stranded `<Analyzer Include>` items and
P1-T14 confirms a green analyzer rebuild from a cold restore — so it is the first point at which the
runner discovers the full test-assembly set and produces both percentages. The no-regression
comparison at P9-T9 reads P2-T7's figures, not this task's. This artifact records no numeric
baseline and must not be cited as one.

## Acceptance evaluation

- The artifact records `EXIT_CODE:` as returned — **1**. PASS.
- The `Output Summary:` names the blocking diagnostic verbatim and states
  `coverage unmeasurable at merge-base; cause: <diagnostic>`. PASS.
- The artifact names P2-T7 as its numeric successor. PASS.

Output Summary: CMD-MSTEST-COVERAGE returned EXIT_CODE 1. 141 of 141 discovered tests passed in
3.3702 seconds, but the run threw before completing coverage post-processing with the verbatim
diagnostic `Cobertura line coverage 3.4321% is below the required 80% threshold.` and printed no
branch-coverage percentage at all. coverage unmeasurable at merge-base; cause: Cobertura line
coverage 3.4321% is below the required 80% threshold. The cause is that the merge-base tree does
not compile — defect #898 fails the analyzer reference resolution of `VBFunctions` and
`UtilitiesCS`, and the preceding `/t:Rebuild` runs cleaned every dependent project's `bin/Debug`
output — so only a fraction of the test assemblies existed to be discovered. The numeric C#
coverage baseline is captured instead at **P2-T7**.
