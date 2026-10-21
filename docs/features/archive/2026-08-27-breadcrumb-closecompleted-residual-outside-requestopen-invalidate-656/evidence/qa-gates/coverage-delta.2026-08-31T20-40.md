# QA Gate — Changed-Line Coverage and Coverage Delta (Issue #656)

Timestamp: 2026-09-01T14-53
Task: [P4-T8]

Command:
```
$A = (Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -SimpleMatch 'bool hostOpen = _host.IsOpen;').LineNumber
$B = (Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -SimpleMatch 'if (_closeCompleted && !hostOpen)').LineNumber
[xml]$c = Get-Content -LiteralPath coverage\coverage.cobertura.xml -Raw -Encoding UTF8
# hits of the ./lines/line nodes with number = $A and $B, under the class node whose
# filename = 'QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs'
```

EXIT_CODE: 0

## Changed-line coverage

Both line numbers are derived mechanically from the file itself rather than asserted, so a later
reformat cannot invalidate them.

- Changed Line A: **326** — `bool hostOpen = _host.IsOpen;`
- Changed Line A Hits: **1**
- Changed Line B: **333** — `if (_closeCompleted && !hostOpen)`
- Changed Line B Hits: **1**

Both hit counts are greater than or equal to 1, which is the acceptance condition. Exactly one
`line` node matched each number under the coordinator's single `class` node, so neither figure is an
artifact of duplicate node selection. Changed-line coverage is therefore 100 percent: both lines
this change introduced are executed by the suite.

Both outcomes of the new conjunct are exercised, not merely both lines: `!hostOpen == true`
(suppression retained) by the standing guards
`PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose`,
`SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired` and
`CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce`; and `!hostOpen == false` (suppression
released) by the new regression test
`CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain`. All four passed in P3-T4 and
P3-T2.

## Coverage delta

| Metric | Baseline (P0-T11) | Post-change (P4-T7) | Direction |
|---|---|---|---|
| Baseline Repo Line Rate | 0.853792 | — | — |
| Post-Change Repo Line Rate | — | 0.853732 | -0.000060 |
| Baseline Coordinator Line Rate | 0.983122 | — | — |
| Post-Change Coordinator Line Rate | — | 0.983193 | +0.000071 |

Acceptance conditions:

- Post-change repo line rate **0.853732 >= 0.80**: satisfied, with substantial margin. It also
  remains above the 0.85 floor in `.claude/rules/general-unit-test.md`.
- Post-change coordinator line rate **0.983193 >= baseline 0.983122**: satisfied. The coordinator
  rate rose, so **PASS is recorded on the first measurement** and the conditional single repeat run
  authorized by this task was **not** executed. `TestResults\p4-t8-repeat\` remains empty and no
  `Repeat Coordinator Line Rate:` value exists, because no second measurement was needed.

The repository rate moved down by 6.0e-5, which is six thousandths of one percentage point. This is
within the per-run nondeterminism band this repository exhibits for `lines-covered` and is not a
coverage regression on the changed lines: both changed lines are covered, as recorded above. The
gate that this task actually applies to the repository figure is the 0.80 floor, which is met.

## Lines-valid delta — the deterministic quantity

- Baseline Coordinator Lines Valid: **237**
- Post-Change Coordinator Lines Valid: **238**

The post value equals the baseline value plus **exactly one**, which is the required relation. This
is the deterministic measurement and it is what a genuine instrumented-size change would move: the
change adds exactly one executable statement to the coordinator, `bool hostOpen = _host.IsOpen;`.
The narrowed guard replaced an existing statement in place and the two `remarks` blocks are XML
documentation, so neither adds an instrumented line. The observed difference is exactly one, so the
`LINES-VALID DELTA UNEXPECTED` branch of this task was not taken.

Per-file `lines-covered` is not deterministic in this repository between two runs against the same
tree, while `lines-valid` is; the measurement supporting that is recorded in
`.claude/agent-memory/orchestrator/coverage-lines-covered-is-nondeterministic.md`, where two
Cobertura documents of the same tree carry identical `lines-valid` for all 550 files while per-file
`lines-covered` moves by up to four lines. The coordinator-rate comparison above is therefore
treated as a candidate signal rather than a hard gate on a single measurement, exactly as this task
specifies; it happened to pass on the first measurement, so that distinction did not need to be
exercised.

Coordinator covered lines moved from 233 to 234, consistent with the one added statement being
covered.

Output Summary: Both changed lines are covered (hits 1 and 1, at lines 326 and 333). Post-change
repository line rate 0.853732 is above the 0.80 floor. Post-change coordinator line rate 0.983193 is
at or above the baseline 0.983122, so PASS was recorded on the first measurement and no repeat run
was executed. Coordinator `lines-valid` moved from 237 to 238, exactly the expected delta of one.
