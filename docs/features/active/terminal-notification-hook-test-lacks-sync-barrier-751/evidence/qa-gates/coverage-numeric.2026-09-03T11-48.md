# P4-T12 — Numeric Post-Change Coverage Capture and Comparison (Issue #751)

Timestamp: 2026-09-03T14-48

Rung: 3
Rung recorded by P0-T17: 3

Command: `Get-ChildItem -Path 'coverage\trx\P4-T5' -Recurse -Filter '*.coverage'`

EXIT_CODE: 0

The recorded `Command:` is the locate command, because rung 3 was reached on the attachment count and no
conversion command was attempted. The output directory was created first, as the task requires:
`New-Item -ItemType Directory -Path 'coverage\numeric' -Force | Out-Null`.

## Output Summary

```
COVERAGE_CAPTURE_BLOCKED
```

**Recorded outcome: Blocked outcome.**

Observed `.coverage` file count under `coverage\trx\P4-T5`: **2**

The task requires the locate search to return exactly one file. It returned two, both of byte length
21356197. Per the rung-3 rule, an arbitrary member of the set was **not** converted and **no coverage number
is stated**. No error text is transcribed, because rung 3 was reached on the file count rather than on a
command failure: no conversion command was run, so no command produced an error.

The two file paths are not transcribed, because the vstest results directory names embed the account name.
Their structural relationship, recorded without paths: one is the attachment published into the run's
results directory, and the other is the in-run copy that vstest retains under its `In\<machine>\` subtree.
vstest produced both from the single P4-T5 run; they are not two separate collections. This is the same
situation P0-T14 produced.

## Baseline values restated

| Figure | Baseline (P0-T17) | Post-change (P4-T12) |
|---|---|---|
| `lines-covered` | not recorded — `COVERAGE_CAPTURE_BLOCKED` | not recorded — `COVERAGE_CAPTURE_BLOCKED` |
| `lines-valid` | not recorded — `COVERAGE_CAPTURE_BLOCKED` | not recorded — `COVERAGE_CAPTURE_BLOCKED` |
| Line-coverage percentage | not recorded — `COVERAGE_CAPTURE_BLOCKED` | not recorded — `COVERAGE_CAPTURE_BLOCKED` |
| Rung | 3 | 3 |
| `.coverage` file count observed | 2 | 2 |

## Outcome selection

The task defines exactly three acceptance outcomes. The selection is determined as follows:

- **Numeric outcome — not applicable.** It requires both artifacts to carry real numeric values. Neither
  does.
- **Denominator-shift outcome — not applicable.** It also requires both artifacts to carry real numeric
  values, differing only in `lines-valid` or in the rung used. Neither artifact carries numeric values, so
  there is nothing to compare. `COVERAGE_METHOD_MISMATCH` does not apply either: both artifacts record
  `Rung: 3`, so the rungs agree; they simply produced no figures.
- **Blocked outcome — applicable and recorded.** Both this artifact and the P0-T17 artifact carry the header
  `COVERAGE_CAPTURE_BLOCKED`, each with the observed `.coverage` file count where rung 3 was reached because
  that count was not exactly one.

**Consequence, as the task requires:** the completion report states the coverage criterion as
**remediation-required**, not PASS.

## What is and is not established

This blocked outcome means no repository-wide numeric coverage pair was produced by this plan. It does
**not** mean the change is unassessed for coverage regression. The applicable obligation for a change of
this shape is the no-regression obligation, and the evidence for it is P4-T11's observation, re-derived
against the actual post-change branch diff, that the number of changed production lines on this branch is
**zero**: every row of the unscoped `git diff --numstat f8414ee9..HEAD` names either one of the two
`TaskMaster.Test/AppGlobals/` source files or a feature-folder Markdown path. The changed-line no-regression
requirement therefore has an empty subject and cannot be violated.

No absolute coverage floor is asserted by this plan, per the recorded decision in P0-T16. The repository
floors remain standing obligations that this plan neither raises, lowers, nor supersedes.

## Note on the blocking condition

The blocking condition is a property of how vstest lays out its results directory — it retains both the
published attachment and an in-run copy — rather than a property of this change. It reproduced identically
on the pre-change P0-T14 run and the post-change P4-T5 run. Resolving it is outside this plan's authorized
scope: the plan states the locate command and the exactly-one requirement explicitly, and explicitly forbids
converting an arbitrary member of the set. No waiver is granted within this plan and no figure is
fabricated.
