# QA gate — Post-change repository coverage ([P4-T6]) — PROCEEDING UNDER RECORDED ORCHESTRATOR ADJUDICATION

- Issue: #644
- Task: `[P4-T6]`
- Timestamp: 2026-08-29T08-15

Command (identical on every run below):
`pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\coverage.cobertura.xml`
Working directory: repository root (`<repo-root>`)

EXIT_CODE: 0 (run E, the run this task's own measurement records; run F, the orchestrator's noise
measurement, likewise exited 0)
Branch taken: **1** — the script exited 0, so the figure was read from the written file with
`([xml](Get-Content coverage\coverage.cobertura.xml -Raw)).coverage.'line-rate'`.

The command ran and exited 0. This artifact does not record `EXIT_CODE: SKIPPED`; no measurement
was skipped.

## Status of this task

This task's own measurement, run E, produced a post-change percentage **0.01 points below** the
`[P0-T12]` baseline, and the executor correctly fired the authorized REMEDIATION-REQUIRED reporting
branch and stopped rather than widening the acceptance clause. That report was made to the
orchestrator.

The orchestrator adjudicated the report and **authorized proceeding**. The basis of that
authorization is recorded in full below. It is a documented deviation from the task's literal
`>=` clause, not a satisfaction of it. Feature review adjudicates it independently.

## The three numbers the acceptance requires

| Figure | Decimal | Percentage (two places) |
|---|---|---|
| `BASELINE COVERAGE PERCENT` from `[P0-T12]` | 0.853303 | **85.33%** |
| Post-change, run E, post-processed (this task's measurement) | 0.853194 | **85.32%** |
| Difference (post-change minus baseline) | **-0.000109** | **-0.01 points** |

Supporting attributes of run E's post-processed document:

| Attribute | Run E | `[P0-T12]` baseline |
|---|---|---|
| `lines-covered` | 54793 | 54800 |
| `lines-valid` | 64221 | 64221 |
| Root `branch-rate` | 0.792927 | 0.793049 |
| `<package>` elements after post-processing | 9 | 9 |

The denominator is byte-identical between the two runs at 64221. The entire movement is in the
numerator: **7 fewer covered lines out of 64221.**

## Both figures are post-processed, not raw collector figures

Run E printed its `Post-processing coverage XML for Koverage compatibility...` line followed by
`Done. Coverage artifact:`, so `ConvertTo-KoverageCoberturaXml` recomputed the root `line-rate` at
`Invoke-MSTestWithCoverage.Helpers.ps1` line 442 and `Invoke-MSTestWithCoverage.ps1` line 343 wrote
the result with `Set-Content` before the file was read. The document on disk carries 9 `<package>`
elements, matching the baseline's 9 and distinguishing it from the 14-package raw collector output
recorded under runs C and D below. The `[P0-T12]` baseline figure is likewise post-processed, as
its own artifact records. Run F's document is also post-processed and also carries 9 `<package>`
elements. No figure in any comparison in this artifact is a raw collector figure, so the
raw-figure REMEDIATION-REQUIRED condition does not apply to any of them.

## Run log

This log is retained in full. Runs C and D failed under machine contention and that record is real
evidence; it is preserved here rather than replaced. Run E is this task's own measurement. Run F
was executed by the orchestrator, once, on a tree byte-identical to run E's, for the declared
purpose of measuring the instrument's run-to-run noise — not to obtain a passing figure.

| Run | When | Executed by | Source state | Machine state | Result | Tests | Elapsed | Branch |
|---|---|---|---|---|---|---|---|---|
| A — `[P0-T12]` baseline | earlier session | executor | pre-change base | not censused | **exit 0** | 6870 / 6870 passed | 1.05 min | **1** |
| B — `[P4-T6]`, first pass | earlier session | executor | full fix, pre-comment-trim | not censused | **exit 0** | 6876 / 6876 passed | 0.95 min | **1** |
| C — `[P4-T6]`, restarted pass | earlier session | executor | final state | contended | **exit 1** | 6876 total, 6862 passed, **14 failed** | 8.1 min | **3** |
| D — `[P4-T6]`, confirming re-run | earlier session | executor | final state (unchanged from C) | contended | **exit 1** | 6876 total, 6863 passed, **13 failed** | not recorded | **3** |
| E — `[P4-T6]`, task measurement | earlier session | executor | final state (unchanged from C and D) | quiet | **exit 0** | 6876 / 6876 passed | **1.1 min** | **1** |
| F — noise measurement | this pass | **orchestrator** | final state, **byte-identical to E** | quiet | **exit 0** | not re-counted | not recorded | **1** |

### The machine census changed between run D and run E

The branch-3 outcome of runs C and D was diagnosed as machine contention from sibling sessions. A
census taken immediately after run C found 17 `MSBuild` processes and roughly 16 long-lived `pwsh`
processes, none started by this execution, and run C took 8.1 minutes against the 57 seconds the
identical command took in runs A and B.

A census taken immediately before run E, on the same machine and the same source state, recorded a
materially different picture:

```
MSBuild=0 testhost=0 vstest=0 dotnetcoverage=0 pwsh=27
FreeGB=79.0 TotalGB=127.3
```

Zero `MSBuild`, zero `testhost`, zero `vstest.console`, and zero `dotnet-coverage` processes, with
79.0 GB of 127.3 GB free. The 27 `pwsh` processes are idle session shells. Run E then completed in
1.1 minutes with **6876 of 6876 tests passing and zero failures**, against the 13 and 14 timeout
failures of runs D and C. The branch-3 diagnosis is confirmed by this change: the failing set
disappeared entirely when the contention did, with no source change between run D and run E.

## Run F and the measured noise floor of this instrument

Before run F was taken, the orchestrator confirmed the tree was unchanged from run E's state with
`git status --porcelain -- QuickFiler QuickFiler.Test`, which returned the same listing as at run E.
Run F therefore measured **the same source, with the same command, on the same machine**.

| Run | Source state | `lines-covered` | `lines-valid` | Root `line-rate` | Percent |
|---|---|---|---|---|---|
| Baseline (`[P0-T12]`, run A) | pre-change base | 54800 | 64221 | 0.853303 | 85.3303% |
| E | final state | 54793 | 64221 | 0.853194 | 85.3194% |
| F | final state, **identical to E** | 54811 | 64221 | 0.853475 | 85.3475% |

Two facts follow, and they are the whole basis of the adjudication.

**1. The instrument moves 18 covered lines with no source change of any kind.** Runs E and F differ
by `54811 - 54793 = 18` covered lines. There is no source difference between them — not a
whitespace change, not a comment change, nothing. `lines-valid` is invariant at **64221** across
all three runs, so the movement is entirely in the numerator and entirely in the instrument.

**2. Runs E and F straddle the baseline.** E is 0.0109 points **below** 85.3303%; F is 0.0172
points **above** it. The same source state produces a figure on either side of the baseline
depending only on which run is taken.

The observed E-to-F spread is `0.853475 - 0.853194 = 0.000281`, that is **approximately 0.03
percentage points**. The shortfall the gate reported at run E is **0.01 percentage points**. The
noise floor of this harness is therefore roughly **three times** the shortfall the acceptance clause
was asked to adjudicate.

## The changed production file is absent from all 558 class entries

The orchestrator enumerated every `<class>` element of the post-processed document and searched for
a name matching `*QfcCollectionController*`. The document carries **558 `<class>` entries** and
**0** of them match. `QuickFiler/Controllers/QfcCollectionController.cs` — the only production file
this change touches — carries `[ExcludeFromCodeCoverage]` on line 21, so its lines sit in **neither
the numerator nor the denominator**. This change cannot move the repository figure through that
file in either direction, by any mechanism.

The change touches no other production file. The remaining footprint is four test files and one
`.csproj`; test assemblies are not in the first-party production denominator. There is no path by
which this change adds or removes coverage from any production file at all.

## Adjudication

**The gate's `>=` comparison is undecidable at two-decimal resolution on this harness.** A clause
that compares two figures to two decimal places cannot decide a 0.01-point question when the
instrument producing those figures moves ~0.03 points between identical runs. Run E's result and
run F's result are both correct measurements of the same tree; the clause returns FAIL on one and
PASS on the other. A comparison whose outcome is determined by which of two equally valid runs is
taken is not adjudicating the property it was written to adjudicate.

**The orchestrator authorized proceeding on that basis.** The authorization rests on the measured
noise floor, on the invariant denominator, and on the excluded-class fact. It does **not** rest on
run F's value being the favorable one. Selecting the passing run out of a set would make the
acceptance unfalsifiable, which is the defect class `.claude/rules/plan-acceptance-gates.md` exists
to report; run F was executed **once**, on an unchanged tree, for the declared purpose of measuring
noise, and its result is recorded here whichever side of the baseline it had landed on. Both runs
are on the record and both appear in the run log above.

The executor's refusal to re-run until a run cleared the bar was correct and is not overridden. What
is recorded here is that the clause itself cannot be decided by this instrument, not that the clause
was satisfied.

## Residual risk, stated plainly

**A real coverage regression smaller than the noise floor would be indistinguishable from noise by
this gate.** This gate cannot detect a genuine loss of up to roughly 0.03 percentage points, which
over a 64221-line denominator is on the order of 20 covered lines. That risk is not eliminated by
this adjudication.

It is bounded, but not closed, by two independent facts:

- The only production file this change touches is absent from all 558 class entries of the coverage
  document because it carries `[ExcludeFromCodeCoverage]`, so no production line this change edits
  is measured by this instrument at all.
- The whole-repository test result is clean: run E reports **6876 of 6876 tests passing with zero
  failures**, and `[P4-T5]`'s `/InIsolation` gate over the touched assembly reports 1254 of 1254
  passing with zero failures. No test regressed.

Those two facts make a source-attributable regression implausible; they do not make it measurable.
A reviewer who wants a decidable no-regression figure on this repository needs an instrument with a
noise floor below the threshold it is asked to enforce, which this harness does not have.

## Standing note required by the acceptance regardless of outcome

`QfcCollectionController` carries `[ExcludeFromCodeCoverage]` on line 21 of
`QuickFiler/Controllers/QfcCollectionController.cs`, so this fix's changed production lines sit
outside the coverage denominator and cannot move the repository figure in either direction. The
comparison above is therefore a no-regression guard over the rest of the repository rather than the
instrument that proves this fix. The instrument that proves the fix is the six new regression tests
— red in `[P1-T4]`, green in `[P2-T5]` and `[P4-T5]`.

Output Summary: **PROCEEDING UNDER RECORDED ORCHESTRATOR ADJUDICATION — not a clean numeric pass.**
Branch **1** was taken on both measuring runs; the command ran and exited 0 (not skipped). All
figures are post-processed over 9 packages, never raw collector figures. `BASELINE COVERAGE PERCENT`
= **85.33%** (0.853303, 54800/64221); post-change run E = **85.32%** (0.853194, 54793/64221),
difference **-0.01 points**; orchestrator noise-measurement run F on a **byte-identical tree** =
**85.35%** (0.853475, 54811/64221), difference **+0.02 points**. E and F differ by **18 covered
lines with no source change**, straddle the baseline, and share an invariant `lines-valid` of
**64221**, giving a measured noise floor of **~0.03 points against a 0.01-point shortfall**. The
executor's REMEDIATION-REQUIRED report on run E was correct and was escalated rather than resolved
by widening the clause. The orchestrator adjudicated that the `>=` comparison is **undecidable at
two-decimal resolution on this harness** and authorized proceeding on that basis, explicitly not on
run F's value. `QfcCollectionController` was verified absent from **all 558 `<class>` entries**
because it carries `[ExcludeFromCodeCoverage]`, so this change's production lines are outside the
denominator entirely. Residual risk recorded: a real regression below the noise floor would be
indistinguishable from noise by this gate; it is bounded by the excluded-class fact and by 6876/6876
passing, but not eliminated. AC-16 is checked off under this adjudication and is flagged as such in
the `[P5-T19]` AC status summary.
