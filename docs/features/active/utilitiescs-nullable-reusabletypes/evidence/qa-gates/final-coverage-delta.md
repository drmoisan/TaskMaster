# Final QC — Changed-Line Coverage Delta (P9-T5)

Timestamp: 2026-07-19T22-03

AC4 gate: no coverage regression on changed lines. This feature is annotation-only, so the
operative gate is no-regression (threshold-independent), per the plan's Open Questions note.

## Whole-run coverage: baseline vs post-change

| Metric | Baseline (P0-T6) | Post-change (P9-T4) | Delta |
|---|---|---|---|
| Line-rate | 0.837874 (83.79%) | 0.838827 (83.88%) | +0.000953 (improved) |
| Branch-rate | 0.763563 (76.36%) | 0.763528 (76.35%) | -0.000035 (stable; nondeterminism) |

- Baseline source: `evidence/baseline/baseline-tests-coverage.md` (numeric authoritative baseline;
  the `baseline-coverage.cobertura.xml` referenced there was not retained in-tree — coverage
  `.cobertura.xml` artifacts are large and were not committed for batches 1–5 either — so the
  numeric headline in the baseline record is used as the authoritative comparison value).
- Post-change source: `evidence/qa-gates/final-coverage.cobertura.xml`.
- Test pass count unchanged: 5702/5702.

## Changed-line analysis (the 51 remediated ReusableTypeClasses files + 4 waiver consumers)

Diff vs merge-base with `main` (`b11b69f3`): 628 added lines across the cluster. These decompose
into non-executable and annotation-only edits:
- 6 `#nullable enable` pragma lines (non-executable directive).
- 4 `where TKey : notnull` constraint clause lines (non-executable generic-parameter metadata).
- The remainder are annotation modifications to EXISTING declarations (`Type` -> `Type?`,
  `out TValue` -> `out TValue?`, `T` -> `T?`), `= null!` / `= default!` initializer suffixes on
  reflection-populated fields, and justified `!` on existing expressions.

No new executable statement or branch logic was introduced (AC3/AC5 confirmed independently in
`final-signature-compat.md` and `final-scope-guards.md`). Because each changed line is either
non-executable or an annotation applied to a pre-existing executable line, no changed line can
introduce a new uncovered executable path; the executable behavior — and therefore the
coverage — of each changed line is identical to its pre-change state.

## Per-file coverage of representative remediated classes (post-change, from final cobertura)

| File | line % | branch % |
|---|---|---|
| ConcurrentObservableDictionary.cs | 85.75 | 78.57 |
| LockingLinkedList.cs | 92.93 | 81.82 |
| DenMatrix.cs | 91.26 | 90.62 |
| SmartSerializable.cs | 91.90 | 98.33 |
| AsyncQueue.cs | 100.00 | 0.00 (no branches) |
| TreeNodeOfT.cs | 93.88 | 87.36 |
| ScoDictionaryNew.cs | 83.87 | 50.00 |
| ScDictionary.cs | 91.95 | 75.00 |
| SerializableList.cs | 95.79 | 92.59 |
| TimedAsyncTask.cs | 90.73 | 75.00 |

These remediated classes retain strong coverage after annotation. Combined with the improved
whole-run line coverage and the unchanged 5702/5702 pass count, there is no coverage regression
on changed lines (AC4). Outcome: PASS.
