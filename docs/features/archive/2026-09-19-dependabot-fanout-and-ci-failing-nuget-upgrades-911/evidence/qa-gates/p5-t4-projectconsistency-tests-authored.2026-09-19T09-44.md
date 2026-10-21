# P5-T4 — ProjectConsistency acceptance suite authored

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $p = "<execution-worktree-root>\tests\scripts\dependencies\ProjectConsistency.Tests.ps1"; $lines = [System.IO.File]::ReadAllLines($p); ... per-token It-name counts, Describe and Context AC\d count, temporary-file idiom count'
```

EXIT_CODE: 0

## Output Summary

```
LINES=375
IT_TOTAL=13
COUNT_AC8-=2
COUNT_AC11-=4
COUNT_AC14-=2
COUNT_AC16-=2
COUNT_AC21-=1
COUNT_AC23-=2
BLOCKS=7
BLOCKS_WITH_ACDIGIT=0
TEMPFILE_HITS=0
```

`BLOCKS` is the count of `Describe` and `Context` declarations; `BLOCKS_WITH_ACDIGIT` is
how many of those names match the regex `AC\d`. The prohibited token is `AC` followed by a
digit and not the bare two letters: PowerShell matching is case-insensitive, so a bare `AC`
prohibition would fire on `Package`, `exact`, `track` and `character` and could never be
satisfied. `TEMPFILE_HITS` counts occurrences of `New-TemporaryFile`, `GetTempPath`,
`$env:TEMP` and `Out-File`.

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| File at most 500 lines | <= 500 | 375 |
| No temporary file created | 0 idioms | 0 |
| `Describe` and `Context` names matching `AC\d` | exactly 0 | 0 |
| `It` names beginning `AC11-` | exactly 4 | 4 |
| `It` names beginning `AC14-` | exactly 2 | 2 |
| `It` names beginning `AC16-` | exactly 2 | 2 |
| `It` names beginning `AC21-` | exactly 1 | 1 |
| `It` names beginning `AC8-` | at least 2 | 2 |
| `It` names beginning `AC23-` | at least 2 | 2 |

The per-token counts are pinned here because P5-T15, P5-T16, P5-T18 and P5-T20 assert exact
`Total` values against these same filtered populations. A suite authored with a different
case count would fail those tasks for an authoring reason rather than a behavioural one.

### Re-measured after the P6-T3 coverage cases were added

P6-T3 measured `scripts/dependencies/ProjectConsistency.psm1` at 86.36 percent line
coverage, below the at-least-90 clause, and four cases were added to this suite to reach
100.00. They sit in a new `Context` named `Guard clauses and explicit overrides in the
reconciliation surface` and cover the two guard clauses and the two else-branches that had
no test. The re-measurement:

| Clause | Required | Measured |
|---|---|---|
| File at most 500 lines | <= 500 | 453 |
| No temporary file created | 0 idioms | 0 |
| `Describe` and `Context` names matching `AC\d` | exactly 0 | 0 |
| `It` names beginning `AC11-` | exactly 4 | 4 |
| `It` names beginning `AC14-` | exactly 2 | 2 |
| `It` names beginning `AC16-` | exactly 2 | 2 |
| `It` names beginning `AC21-` | exactly 1 | 1 |
| `It` names beginning `AC8-` | at least 2 | 2 |
| `It` names beginning `AC23-` | at least 2 | 2 |
| `It` blocks in total | not pinned | 17, up from 13 |

**Every pinned per-token count is unchanged.** No added `It` name begins with an `AC<N>-`
token and the new `Context` name does not match `AC\d`, so none of the four enters a
criterion-filtered population and the exact `Total` assertions at P5-T15, P5-T16, P5-T18
and P5-T20 are unaffected. The total `It` count was never pinned by this task's acceptance,
which is stated per token precisely so that a non-criterion case can be added without
disturbing a criterion population.

## Fixtures

Every fixture is an in-memory string or scriptblock. No temporary file is created and no
project file on disk is read, so the suite is independent of the state of the working tree.
Both `scripts/dependencies/ProjectConsistency.psm1` and
`scripts/dependencies/ConsistencyVerifier.psm1` are imported, per Scope Decision 5.

The AC21 fixture reproduces the #908 three-way divergence in one project: the in-memory
manifest declares `3.0.235`, the `<Import>` and `<Error>` guards name `3.0.259`, and the
`<Analyzer Include>` names `3.0.203`. The injected listing offers `roslyn4.14`, `roslyn5.0`
and `roslyn5.9`, so `roslyn5.0` is neither the first nor the highest offered folder and a
selection implementation fails the post-repair assertion.

## State of the tree at authoring time

All three modules are declared pass-throughs at this point, so this suite is expected to
fail. P5-T5 captures the AC21 case failing against that tree as the AC22 red-before
control; P5-T6 through P5-T12 implement the behaviour; P5-T15 through P5-T20 capture the
green runs.
