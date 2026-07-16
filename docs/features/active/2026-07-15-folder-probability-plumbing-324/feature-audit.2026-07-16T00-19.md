# Feature Audit — folder-probability-plumbing (#324)

- Timestamp: 2026-07-16T00-19
- Feature branch: feature/folder-probability-plumbing-324 @ d9bfe081
- Base: origin/epic/folder-tree-percentage-ui-integration
- Work mode: full-feature
- AC sources: spec.md (`## Acceptance Criteria`, 13 items + 3 seeded test conditions) and
  user-story.md (`## Acceptance Criteria / Done When`, 8 items)

## Scope and Baseline

Baseline is the epic base branch origin/epic/folder-tree-percentage-ui-integration (6d4535c6). The
audit evaluates the full branch diff. This feature adds the folder-probability contract (child 9001)
and preserves all existing scoring outputs byte-for-byte, verified structurally and by regression
tests.

## Acceptance Criteria Inventory

- spec.md `## Acceptance Criteria`: AC1..AC13 (13 items).
- spec.md `## Seeded Test Conditions`: 3 items.
- user-story.md `## Acceptance Criteria / Done When`: US1..US8 (8 items).
- All items were already checked off `[x]` by the executor. This audit independently verifies each
  and confirms the check-off; no PASS item required a state change, and no item needed to be reverted.

## Acceptance Criteria Evaluation

### spec.md — Acceptance Criteria

| AC | Criterion (abbrev.) | Status | Evidence |
|---|---|---|---|
| AC1 | FolderScore readonly struct (net48-safe, get-only FolderPath/Score/Probability + ctor) | PASS | FolderScore.cs; compiles under nullable/TreatWarningsAsErrors. |
| AC2 | ToScoredArray() / ToScoredArray(int) return FolderScore[] | PASS | FolderScorer.cs diff. |
| AC3 | ToScoredArray ordering == ToArray incl. tie | PASS | FolderScorerRegressionTests.ToScoredArray_FolderPathOrdering_EqualsToArrayOrdering, ToScoredArray_WithTie_PreservesIdenticalOrdinalTieBreakAsToArray. |
| AC4 | ToArray()/ToArray(int) unchanged byte-for-byte | PASS | Shared OrderedScores(); golden tests ToArray_WithPopulatedScorer_*, ToArrayTopN_*. |
| AC5 | FolderArray / FindFolder unchanged byte-for-byte | PASS | FolderRowTests Text-parity + FolderRowArray_DoesNotAlterFolderArrayOutput; structural non-mutation verified against source. |
| AC6 | Probability max-normalized [0,1] with zero-guard | PASS | BuildScoredArray zero-guard; empty-scorer and all-zero-seed tests. |
| AC7 | Scored projection across Bayesian/conversation/word-sequence + mixed; no COM Bayesian path | PASS | FolderScoreTests (per-source + mixed-source accumulation). |
| AC8 | FolderRow + FolderRowKind + FolderRowArray + FindFolderRows; Text/Kind/Score correct | PASS | FolderRow.cs; FolderPredictor.cs; FolderRowTests Kind + Score-only-on-Suggestion. |
| AC9 | "Error" sentinel never in scored contract | PASS | FolderScorerRegressionTests.AddSuggestion_WithErrorSentinel_*, AddArray_WhenFirstElementIsErrorSentinel_* (verified by reading the tests). |
| AC10 | Downstream sufficiency documented | PASS | evidence/other/downstream-sufficiency.md. |
| AC11 | Probability XML doc = relative display value, not calibrated posterior | PASS | FolderScore.cs XML doc on Probability. |
| AC12 | New/changed code meets stricter coverage regime | PASS | evidence/qa-gates/coverage-delta.md; new members >= 90% line, branch paths covered, no changed-line regression, no exclusions. |
| AC13 | Full C# toolchain green | PASS | qc-csharpier (EXIT 0), qc-analyzer-build (0/0), qc-nullable-build (0 feature errors), qc-vstest-coverage (18/18 new tests pass). |

### spec.md — Seeded Test Conditions

| Item | Status | Evidence |
|---|---|---|
| Unit coverage of contract projection for Bayesian/conversation/word-sequence | PASS | FolderScoreTests per-source scale tests. |
| Regression tests proving ToArray/FolderArray ordering + content unchanged | PASS | FolderScorerRegressionTests golden baseline; FolderRowTests parity. |
| Edge cases: empty scorer, ties, "Error" sentinel, separator rows | PASS | FolderScoreTests empty/all-zero; regression tie + Error tests; FolderRowTests separator Kind tagging. |

### user-story.md — Acceptance Criteria / Done When

| US | Criterion (abbrev.) | Status | Maps to |
|---|---|---|---|
| US1 | FolderScore value type available from scoring layer | PASS | AC1 |
| US2 | ToScoredArray ordering matches ToArray incl. ties | PASS | AC2/AC3 |
| US3 | ToArray/ToArray(int)/FolderArray/FindFolder unchanged | PASS | AC4/AC5 |
| US4 | Probability max-normalized + zero-guard + documented as relative | PASS | AC6/AC11 |
| US5 | Verified across 3 sources + mixed, no COM Bayesian path | PASS | AC7 |
| US6 | FolderRow model + Kind + Score-only-on-Suggestion | PASS | AC8 |
| US7 | "Error" sentinel absent from scored contract | PASS | AC9 |
| US8 | Contract documented sufficient for 9002/9003, single normalization point | PASS | AC10 |
| (coverage + toolchain green) | Both PASS | AC12/AC13 |

## Acceptance Criteria Check-off

All 16 spec.md items (AC1-AC13 + 3 seeded) and all 8 user-story.md items are `[x]` in their source
files and independently verified PASS by this audit. No item was newly checked or reverted by the
reviewer. No PARTIAL, FAIL, or UNVERIFIED item exists.

### Acceptance Criteria Status
- Source: spec.md, user-story.md
- Total AC items: 24 (spec 13 + spec seeded 3 + user-story 8)
- Checked off (delivered): 24
- Remaining (unchecked): 0
- Items remaining: none

## Summary

Feature audit: PASS. Every acceptance criterion across both source files is delivered and verified
against the baseline. The additive contract preserves all protected outputs byte-for-byte and is
sufficient for downstream features 9002 and 9003 without a second plumbing pass. Blocking findings: 0.
