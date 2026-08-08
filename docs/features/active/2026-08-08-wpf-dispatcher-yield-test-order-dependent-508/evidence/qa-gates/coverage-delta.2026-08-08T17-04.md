# Repository-Wide Coverage Delta

Timestamp: 2026-08-08T17-04

Task: [P2-T11]

AC served: AC9 (repository-wide line coverage does not regress).

Sources:

- Baseline: `<FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml` (P0-T10, 6293/6293)
- Post-change: `<FEATURE>/evidence/qa-gates/coverage-postchange.cobertura.xml` (P2-T5 pass 4, 6295/6295)

Both were produced by the identical command
(`Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`), over the identical set of 9
discovered test assemblies, with the identical `coverage.config`, so the comparison is
like-for-like.

## Root `<coverage>` elements

Baseline:

```xml
<coverage line-rate="0.858162" branch-rate="0.792118" complexity="24646" version="1.9"
          timestamp="1786220438" lines-covered="95274" lines-valid="111021"
          branches-covered="22070" branches-valid="27862">
```

Post-change:

```xml
<coverage line-rate="0.858328" branch-rate="0.792318" complexity="24661" version="1.9"
          timestamp="1786222160" lines-covered="95325" lines-valid="111059"
          branches-covered="22093" branches-valid="27884">
```

## Delta table

| Metric | Baseline | Post-change | Signed delta |
|---|---|---|---|
| **line-rate** | **0.858162** | **0.858328** | **+0.000166** |
| branch-rate | 0.792118 | 0.792318 | +0.000200 |
| lines-covered | 95274 | 95325 | +51 |
| lines-valid | 111021 | 111059 | +38 |
| branches-covered | 22070 | 22093 | +23 |
| branches-valid | 27862 | 27884 | +22 |
| complexity | 24646 | 24661 | +15 |

## Gate: non-negative line-rate delta — PASS

Signed line-rate delta is **+0.000166** (85.8162% -> 85.8328%). This is non-negative, so the gate
required by the task text is met. Branch-rate also improved (+0.000200).

## Why the denominator grew, and why the rate still rose

`lines-valid` increased by 38. This is the expected and intended consequence of P1-T7 removing
`[ExcludeFromCodeCoverage]` from `WpfDispatcherYield`.

P0-T11 measured that the attribute **is** honored in this configuration: the class was entirely
absent from the baseline report (0 matched `<class>` elements, 0 substring occurrences of
`WpfDispatcherYield`), while 20+ peer classes from the same directory were present. Removing the
attribute admits the class into the denominator for the first time.

The escalation condition named in the execution directive — "removing `[ExcludeFromCodeCoverage]`
from `WpfDispatcherYield` moves the repo-wide figure materially" — did **not** trigger:

- The movement is +0.000166 in line-rate, i.e. **+0.0166 percentage points** on a base of 85.8162%.
- The class contributes 38 lines against `lines-valid = 111059`, roughly **0.034%** of the
  denominator.
- The movement is upward, not downward.

The rate rose because the newly-admitted class is covered well above the repository average
(aggregated 97.37% per P2-T12, versus a repo-wide 85.83%), so adding it lifts the mean. The +51
covered lines against +38 valid lines also reflects the four in-scope tests exercising previously
uncovered paths in already-instrumented code.

## Policy floor

`.claude/rules/csharp.md` requires repository-wide line coverage `>= 80%`. Both figures clear it:
baseline 85.8162%, post-change 85.8328%. No pre-existing repository-wide coverage shortfall exists,
so the escalation condition on that point does not apply.

## Test-count context

| Run | Total | Passed | Failed |
|---|---|---|---|
| Baseline | 6293 | 6293 | 0 |
| Post-change | 6295 | 6295 | 0 |

Both source runs were fully green, so neither figure is depressed by unexecuted tests. The +2 total
is exactly the two tests added by P1-T10 and P1-T11.

Output Summary: PASS. Repository-wide line-rate moved from 0.858162 to 0.858328, a signed delta of
**+0.000166** (non-negative, gate satisfied); branch-rate moved +0.000200. `lines-valid` grew by 38
because P1-T7 removed a genuinely-honored `[ExcludeFromCodeCoverage]`, admitting
`WpfDispatcherYield` to the denominator for the first time — yet the rate still rose because the
class is covered at 97.37% against a repo average of 85.83%. The movement is +0.0166 percentage
points on ~0.034% of the denominator and is upward, so the "material movement" escalation condition
does not trigger. Both figures clear the 80% policy floor.
