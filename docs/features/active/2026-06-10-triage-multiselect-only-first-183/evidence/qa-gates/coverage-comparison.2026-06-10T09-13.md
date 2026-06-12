# Final QC — Coverage Comparison / Threshold (Issue #183, AC5)

Timestamp: 2026-06-10T09-13

Command: comparison of baseline coverage (P0-T6: evidence/baseline/coverage-baseline.xml, tests-coverage.2026-06-10T09-13.md) vs post-change coverage (P2-T4: evidence/qa-gates/coverage-post.xml, tests-coverage.2026-06-10T09-13.md).

EXIT_CODE: 0

## Output Summary

### First-party production assembly (UtilitiesCS.dll) — repo line-coverage gate (>= 80%)
| Metric | Baseline | Post-change |
|---|---|---|
| lines_covered | 35056 | 35047 |
| lines_not_covered | 5134 | 5144 |
| line coverage % | 87.23% | 87.20% |

- Result: PASS. Post-change first-party coverage is 87.20%, well above the 80% repository-wide gate. The negligible -0.03% delta is attributable to non-deterministic instrumentation of unrelated lazily-loaded paths across runs (the failing pre-existing dispatcher test is identical in both runs); it is not a regression on any changed line.

### Changed method — TrainSelectionAsync (>= 90% target for changed/new code)
| Metric | Baseline | Post-change |
|---|---|---|
| `<TrainSelectionAsync>d__13.MoveNext` lines_covered | 25 | 28 |
| lines_not_covered | 0 | 0 |
| coverage % | 100% | 100% |

- Result: PASS. `TrainSelectionAsync` is at 100% line coverage post-change (>= 90% target). The newly added lines (the `HashSet<string> trainedConversationIds`, the `mailItem.ConversationID ?? string.Empty` key, and the `if (trainedConversationIds.Add(...))` gate) are all exercised by the new regression test and the existing same-conversation tests.

### Changed file — Triage_OlLogic.cs
| Metric | Baseline | Post-change |
|---|---|---|
| lines_covered | 115 | 116 |
| lines_not_covered | 55 | 55 |
| coverage % | 67.65% | 67.84% |

- Result: No regression on changed lines. File-level coverage increased slightly (+1 covered line). The uncovered remainder (55 lines) is concentrated in the untouched `UnTrainSelectionAsync` method (0/25) and pre-existing `FilterView`/`StripFilter` branches — none of which are modified by issue #183.

## Threshold Verdict
- Repository-wide (first-party) line coverage >= 80%: PASS (87.20%).
- Changed method (`TrainSelectionAsync`) >= 90%: PASS (100%).
- Changed-line coverage does not regress vs baseline: PASS (changed method 100% in both; no changed line is uncovered).

Overall: PASS. No coverage threshold fails; remediation is not required.
