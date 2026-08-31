# Feature Audit: Issue #637 rooted breadcrumb selection normalization

## Scope and Baseline

This review compares `a314228b9c3d9a4944a9e88e1a4eb4bd9c4b0f7b` to `main` at merge base `3be3f237a8551df3f27f83d9d1af2f26074fc93a`. The canonical PR-context artifacts were collected against `main`. The active feature folder is `docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`.

The full feature-vs-base diff comprises 95 changed files: 8 C#, 2 project files, and 85 Markdown files. Current-head C# QA passed: formatter and check, analyzer rebuild, nullable rebuild, and 6,894 passing MSTest tests with 85.3545% line coverage.

## Acceptance Criteria Inventory

The persisted work mode is `full-bug`; `spec.md` is the authoritative source. It contains 30 acceptance-criteria checkboxes, all currently marked delivered. AC21's deliberate specification-correction wording is retained unchanged.

## Acceptance Criteria Evaluation

| Criterion | Status | Evidence |
|---|---|---|
| AC1-AC5 | PASS | Named Issue #637 tests cover root-exact, rooted descendant, case/trailing separator, out-of-root, and separator-boundary behavior. |
| AC6-AC10 | PASS | Relative/trash pass-through, untouched hierarchy/commit paths, family shape, and `SelectFirstRow` reachability are covered by source review and P8 evidence. |
| AC11-AC14 | PASS | The new partial-file helper is pure, gated, normalized when eligible, and total/no-throw under documented cases. |
| AC15-AC17 | PASS | Existing archive-stem behavior, MoveToFolder family shape, and folder-opening/degradation paths are preserved. |
| AC18-AC21 | PASS | The Issue #439 assertion and wording were corrected while provider lookup remains rooted; AC21 remains explicitly an invariant-driven specification correction. |
| AC22-AC24 | PASS | Stale deferrals were corrected, guard behavior remains covered, and the SelectedFolderPath surface is preserved. |
| AC25 | PASS | The specification's stated no-growth disposition for the existing 694-line fixture is met; the separate policy audit identifies that repository policy supplies no exception for modifying it. |
| AC26-AC30 | PASS | Compile include, nullable, ordered toolchain, canonical evidence/coverage, and scope-boundary evidence pass. |

## Summary

All 30 product acceptance criteria are substantively met. The feature is not PR-ready because the policy audit has a blocker independent of product behavior: the modified Issue #439 test file is 694 lines, above the repository maximum of 500 for test code.

## Acceptance Criteria Check-off

No acceptance checkbox was changed by this review. Each was already checked and is evaluated PASS above. The policy blocker is separately recorded because AC25 documents a pre-existing-size/no-growth disposition, whereas repository policy provides no exception for modification of the oversized file.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637/spec.md`
- Total AC items: 30
- Checked off (delivered): 30
- Remaining (unchecked): 0
- Items remaining: none
