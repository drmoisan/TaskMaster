# Feature Audit: Issue #637 rooted breadcrumb selection normalization

## Scope and Baseline

This post-remediation feature audit compares HEAD `952a760fb19ff9c10007fe2ebb42f8cadd49a886` with `main` at merge base `3be3f237a8551df3f27f83d9d1af2f26074fc93a`. Fresh `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` identify the same resolved base, head, and active feature folder: `docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`.

The feature range changes C#, project, Markdown, and agent-memory documents. The executable implementation scope is the ten Issue #637 QuickFiler and QuickFiler.Test paths recorded in `evidence/qa-gates/p8-t30-scope-boundary.md`. The final remediation commit adds the partial test fixture and its project include; it leaves `spec.md`, including AC21, unchanged.

## Acceptance Criteria Inventory

The persisted work mode in `issue.md` is `full-bug`, so `spec.md` is the authoritative acceptance-criteria source. Its scoped acceptance-criteria section contains 30 checkbox items: 30 checked and zero unchecked, independently confirmed by `p8-t31-ac-reconciliation.md`.

## Acceptance Criteria Evaluation

| Criteria | Status | Evidence |
|---|---|---|
| AC1-AC5 | PASS | Issue #637 tests and source review verify root-exact non-selection, under-root stem commitment, full-path-only nesting, no-bound-root pass-through, and Trash preservation. |
| AC6-AC10 | PASS | Out-of-root and boundary rejection, value-free root-exact diagnostic, unchanged hierarchy behavior, selection-family shape, and common `SelectRow` reachability are covered by tests and Phase 8 evidence. |
| AC11-AC14 | PASS | `EfcDataModel.FilingStem.cs` supplies the internal pure helper; Issue #637 helper tests cover rooted, verbatim, and total/no-throw cases. |
| AC15-AC17 | PASS | Existing archive-stem behavior, MoveToFolder family shape, and the excluded folder-opening paths remain unchanged. |
| AC18-AC21 | PASS | The corrected Issue #439 selection assertion retains the rooted provider lookup. `p8-t21-spec-correction-record.md` and unchanged `spec.md` preserve the deliberate, invariant-driven specification-correction statement. |
| AC22-AC24 | PASS | The three stale deferral records were corrected while guard behavior and the selected-folder-path surface remain unchanged. |
| AC25 | PASS | Remediation split the modified Issue #439 fixture into 455- and 253-line partial files, both within the repository maximum. |
| AC26-AC30 | PASS | Each partial appears once in the test project; nullable, ordered toolchain, coverage, and full scope boundary evidence pass. |

## Summary

All 30 product acceptance criteria are met, and the post-remediation audit found no policy or code blocker. The fixture split resolves the prior policy failure while preserving the original assertions, test identities, and AC21 deliberate specification-correction language.

## Acceptance Criteria Check-off

No acceptance checkbox was changed by this review. All authoritative `spec.md` acceptance criteria were already checked and each evaluated PASS.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637/spec.md`
- Total AC items: 30
- Checked off (delivered): 30
- Remaining (unchecked): 0
- Items remaining: none
