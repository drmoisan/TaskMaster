# P2-T7 — Numeric C# coverage baseline

Timestamp: 2026-09-19T15-40

Command: CMD-MSTEST-COVERAGE.

```
pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .
```

Invoked with an absolute script path and the execution worktree as the working directory.
`-SearchRoot .` is mandatory: the script's single-search-root defect otherwise discovers assemblies
from a sibling worktree. The script always appends `/TestCaseFilter:TestCategory!=LiveOutlook`, so
every figure below excludes that category, and it enforces its own floors of 0.80 line and 0.75
branch.

EXIT_CODE: 0

## This is the numeric C# coverage baseline

**This artifact is the numeric C# coverage baseline for the no-regression comparison at P9-T9.** It
supersedes the unmeasurable attempt recorded at
`evidence/baseline/p0-t14-mstest-coverage.2026-09-19T09-44.md`. The cause of that failure is
defect #898: the merge-base tree did not compile, because fifteen project files carried
`<Analyzer Include>` items naming `Meziantou.Analyzer.3.0.203`, a package folder the restore does
not produce, so `CSC` raised `CS0006` and no test assembly was built to measure. P1-T9 corrected
those items and P1-T14 confirmed the cold-restore build green, which is what makes a numeric
measurement possible at this point in the run and not earlier.

## Numeric coverage

One-line first-party coverage report, quoted verbatim as the runner printed it:

```
First-party coverage: lines 56486/65737 (85.93%), branches 13657/17052 (80.09%)
```

| Metric | Covered | Total | Percentage | Runner floor | Margin above floor |
|---|---|---|---|---|---|
| Line | 56486 | 65737 | **85.93%** (0.8593) | 0.80 | **+5.93 points** |
| Branch | 13657 | 17052 | **80.09%** (0.8009) | 0.75 | **+5.09 points** |

Both margins are recorded because they are what make an unrelated regression visible at P9-T9: a
change that costs more than 5.93 points of line coverage or more than 5.09 points of branch
coverage drops the run below the runner's own floor and fails it outright, while a smaller
regression is visible only by comparing against the figures above.

Both metrics also clear the repository policy floors, which gate rule 13 records as 80 percent
line and 75 percent branch for C#, per the execution worktree's `CLAUDE.md` under issue #563.

### Discrepancy against the preflight figures — reported, not absorbed

The plan records preflight measurements of line `0.820056` and branch `0.782406`. This run measures
`0.8593` and `0.8009`, which is **+3.92 points of line coverage and +2.65 points of branch
coverage above the preflight figures**.

The direction is upward and both preflight values were themselves above the runner's floors, so no
acceptance clause of this task is violated: the task asserts that both percentages are present and
that their margins are recorded, not that they equal the preflight values. The divergence is
recorded here rather than silently accepted because P9-T9 compares against this artifact, and a
later reader must not mistake the preflight pair for this baseline.

The most likely cause is the same defect the paragraph above describes. The preflight measurement
was taken on a tree in a different compile state, and a coverage denominator computed from a
partially-built solution is not the same denominator as one computed from all eighteen assemblies.
That is an explanation rather than a measurement, and it is labelled as such; the figures in the
table are the measured ones and are what P9-T9 must read.

## Test counts

| Count | Value |
|---|---|
| Total tests | **7343** |
| Passed | **7343** |
| Failed | **0** |
| Skipped | **0** |

`Test Run Successful.` A successful vstest run prints no `Failed:` and no `Skipped:` line at all,
so those two zeros are not read from absent output. They are read from the trx-derived summary the
runner wrote, quoted in full below, which states them explicitly.

## Permitted evidence forms copied into the evidence tree, per gate rule 12

The runner printed both path lines, so both copies are mandatory and both were made.

| Form | Source path printed by the run | Destination |
|---|---|---|
| Package-level JaCoCo projection | `coverage/coverage.cobertura.jacoco.xml` | `evidence/qa-gates/p2-t7-coverage-projection.2026-09-19T09-44.jacoco.xml` |
| Trx-derived test-result summary | `coverage/test-results/mstest-coverage-run.summary.txt` | `evidence/qa-gates/p2-t7-test-results.2026-09-19T09-44.summary.txt` |

The exact stdout lines that named them:

```
Coverage projection: <execution-worktree-root>\coverage\coverage.cobertura.jacoco.xml
Test-result summary: <execution-worktree-root>\coverage\test-results\mstest-coverage-run.summary.txt
```

**TEST-RESULT-SUMMARY: produced.** The `Test-result summary was not written:` warning did not
appear. The runner's line 425 sits inside `if ($runSummary)` behind a try/catch covering a missing,
unreadable, unparseable or summary-less trx, and that branch is live in this repository — but it
did not fire on this run, so the summary copy is mandatory and its presence in the P2-T8 commit is
required.

Both copies are text and carry no absolute host path. The projection is 1467 bytes of package-level
counters with no file-path attributes; the summary is 298 bytes of counts. `.csharpierignore` line
4 excludes `**/evidence/**`, so neither copy reaches the formatter — confirmed at P2-T4, where the
check reported no file under the evidence tree.

### Summary content, verbatim

```
Test run outcome: Completed
Total 7343, executed 7343, passed 7343, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
```

### Projection content, verbatim

```
<report name="TaskMaster">
  <package name="QuickFiler">        <counter type="LINE" missed="2293" covered="10461" /> <counter type="BRANCH" missed="699"  covered="2518" /></package>
  <package name="UtilitiesCS">       <counter type="LINE" missed="4207" covered="39217" /> <counter type="BRANCH" missed="1796" covered="9473" /></package>
  <package name="TaskVisualization"> <counter type="LINE" missed="143"  covered="1426" />  <counter type="BRANCH" missed="67"   covered="333" /></package>
  <package name="SVGControl">        <counter type="LINE" missed="977"  covered="877" />   <counter type="BRANCH" missed="338"  covered="300" /></package>
  <package name="ToDoModel">         <counter type="LINE" missed="762"  covered="1061" />  <counter type="BRANCH" missed="260"  covered="248" /></package>
  <package name="Tags">              <counter type="LINE" missed="56"   covered="702" />   <counter type="BRANCH" missed="16"   covered="174" /></package>
  <package name="TaskMaster">        <counter type="LINE" missed="802"  covered="2443" />  <counter type="BRANCH" missed="211"  covered="517" /></package>
  <package name="TaskTree">          <counter type="LINE" missed="11"   covered="295" />   <counter type="BRANCH" missed="8"    covered="94" /></package>
  <package name="VBFunctions">       <counter type="LINE" missed="0"    covered="4" />     <counter type="BRANCH" missed="0"    covered="0" /></package>
</report>
```

The block above is re-laid out for width; the committed copy at
`evidence/qa-gates/p2-t7-coverage-projection.2026-09-19T09-44.jacoco.xml` is the runner's byte-exact
output. The nine package line counters sum to 56486 covered and 9251 missed, totalling 65737, which
reconciles exactly with the one-line report. That reconciliation is the check that the copied
projection describes this run and not a stale document left by an earlier one.

## Why the copies are mandatory rather than optional

Gate rule 12's rationale is substitution, not prohibition alone: committing the projection and the
summary *in place of* the raw collector document loses no figure a reviewer needs. Producing them
into `coverage/` and committing nothing in their place leaves the prose figures above uncheckable,
because `coverage/` is gitignored and the originals are overwritten by the next run. The raw
`coverage/coverage.cobertura.xml` is **not** copied and **not** committed, which is the half of the
rule that is a prohibition.

Output Summary: CMD-MSTEST-COVERAGE returned EXIT_CODE 0 with `Test Run Successful.`, 7343 total
tests, 7343 passed, 0 failed, 0 skipped. First-party coverage is **85.93 percent line**
(56486/65737) and **80.09 percent branch** (13657/17052), clearing the runner's own 0.80 and 0.75
floors by 5.93 and 5.09 points respectively. This is the numeric C# baseline for the P9-T9
no-regression comparison, superseding P0-T14, which could not measure because defect #898 stopped
the merge-base tree compiling. Both permitted evidence forms were produced and copied into
`evidence/qa-gates/`; the trx-derived summary was produced, so `TEST-RESULT-SUMMARY: produced`. The
measured percentages run 3.92 and 2.65 points above the preflight figures the plan records; the
divergence is upward, breaches no clause, and is reported rather than absorbed.
