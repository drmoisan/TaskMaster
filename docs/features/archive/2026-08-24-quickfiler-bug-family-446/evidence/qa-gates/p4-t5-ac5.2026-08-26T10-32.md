# [P4-T5] AC5 Verification — #427-A Datamodel Scorer Returns the Real Top Folder

Timestamp: 2026-08-26T10-32

Task: [P4-T5]
Acceptance criterion: AC5
Feature: docs/features/active/quickfiler-bug-family-446
Merge base (`<mb>`): `61edc19befcf6c4e95b5acd32542f2dcdab41b78`

## AC5 text (spec.md:879)

> AC5 — #427-A: `ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder` is present in
> `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` and fails against the current
> `return score.Score;` at `QuickFiler/Controllers/QfcDatamodel.cs:376`.

## 1. Presence

Command: `grep -n "ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder" "QuickFiler.Test/Controllers/QfcDatamodelTests.cs"`
EXIT_CODE: 0
Output: `327:        public async Task ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder()`

## 2. Fail-before / pass-after pairing

| State | TRX path | Outcome |
| --- | --- | --- |
| Pre-fix (`[P1-T12]`, red step) | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t12/p1-t12.trx` | Failed |
| Post-fix (`[P2-T4]`) | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t4/p2-t4.trx` | Passed |

Recorded pre-fix failure message, verbatim from `p1-t12.trx`:

```
Expected result.TopFolder to be "Inbox\Projects\Alpha" with a length of 20 because the top-ranked folder the scorer already computed must reach the caller instead of being discarded and re-derived downstream, but "" has a length of 0, differs near "" (index 0).
```

The pre-fix observed value is the empty string, which is exactly the symptom of the discarding
`return score.Score;` form AC5 names: the score reached the caller and the folder did not. The
message is a FluentAssertions assertion failure, not a compile error or a timeout.

Corroborating later runs: `Passed` in `evidence/regression-testing/p2-t8/p2-t8.trx` and
`evidence/regression-testing/p3-t8/p3-t8.trx`.

## Output Summary

AC5 holds. `ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder` is present at
`QuickFiler.Test/Controllers/QfcDatamodelTests.cs:327`, is recorded `Failed` pre-fix in
`evidence/regression-testing/p1-t12/p1-t12.trx` with the folder observed as the empty string, and
is recorded `Passed` post-fix in `evidence/regression-testing/p2-t4/p2-t4.trx`. The AC5 checkbox
in `spec.md` is checked.

EXIT_CODE: 0
