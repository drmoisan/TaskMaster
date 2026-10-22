# [P4-T7] AC7 Verification — All Four Gate Exits Return a Stop Reason

Timestamp: 2026-08-26T10-33

Task: [P4-T7]
Acceptance criterion: AC7
Feature: docs/features/active/quickfiler-bug-family-446
Merge base (`<mb>`): `61edc19befcf6c4e95b5acd32542f2dcdab41b78`

## AC7 text (spec.md:884)

> AC7 — `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` returns a stop reason from
> all four exits, mapped `:100`/`:154` -> `QuantitySatisfied`, `:119` -> `DeadlineExpired`,
> `:128` -> `SourceExhausted`. Verified by AC1.

The line numbers quoted in the AC text are the pre-change positions. The post-change positions are
recorded below; the mapping, which is what AC7 asserts, is unchanged.

## 1. The four post-change return statements

Command: `grep -n "return \|QfcDequeueStop\." "QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs"`
EXIT_CODE: 0

The command returns exactly four lines. Every `return` in the file is one of these four, so the
census is complete rather than a sample.

| # | Post-change line | Return statement (verbatim) | Stop value | Exit condition |
| --- | --- | --- | --- | --- |
| 1 | `:149` | `return new QfcGateBatch(accepted, QfcDequeueStop.QuantitySatisfied, scanned);` | `QuantitySatisfied` | degenerate request, `quantity <= 0` guard at `:147` |
| 2 | `:167` | `return new QfcGateBatch(accepted, QfcDequeueStop.DeadlineExpired, scanned);` | `DeadlineExpired` | first-batch deadline elapsed with zero accepted, guard at `:160-164` |
| 3 | `:176` | `return new QfcGateBatch(accepted, QfcDequeueStop.SourceExhausted, scanned);` | `SourceExhausted` | take delegate returned null and the producer is no longer active, guard at `:173` |
| 4 | `:222` | `return new QfcGateBatch(accepted, QfcDequeueStop.QuantitySatisfied, scanned);` | `QuantitySatisfied` | normal loop exit, `while (accepted.Count < quantity)` at `:156` fell through |

The mapping in document order is therefore `QuantitySatisfied`, `DeadlineExpired`,
`SourceExhausted`, `QuantitySatisfied`, exactly as AC7 and `[P4-T7]` state. Each of the four sites
carries an explicit `QfcDequeueStop` value in the constructor call; none defaults, and no exit
returns a bare collection.

## 2. Behavioural corroboration — the `[P2-T1]` TRX

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t1/p2-t1.trx`

| Test | Outcome |
| --- | --- |
| `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop` | Passed |
| `DequeueAsync_SourceDrained_ReportsSourceExhaustedStop` | Passed |

Total 2, passed 2, failed 0. These are the AC1 tests, which exercise return sites 2 and 3 and
discriminate them from the `QuantitySatisfied` default that the pre-fix gate reported for both
(see `evidence/qa-gates/p4-t1-ac1.*.md` for the pre-fix messages, both of which read
`but found QfcDequeueStop.QuantitySatisfied {value: 0}`).

## Output Summary

AC7 holds. `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` has exactly four return
statements, at `:149`, `:167`, `:176` and `:222`, each constructing a `QfcGateBatch` with an
explicit stop value in the order `QuantitySatisfied`, `DeadlineExpired`, `SourceExhausted`,
`QuantitySatisfied`. The `[P2-T1]` TRX records both discriminating stop-reason tests as Passed.
The AC7 checkbox in `spec.md` is checked.

EXIT_CODE: 0
