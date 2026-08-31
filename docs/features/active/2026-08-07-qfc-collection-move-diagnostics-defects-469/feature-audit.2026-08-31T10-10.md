# Feature Audit: qfc-collection-move-diagnostics-defects (#469) evidence-only remediation re-review

## Scope and Baseline

- **Range:** `origin/main...HEAD` (`6191c74f3be6e37ecd82816902df9c3832bfc9af...d69a572b2f1ce3d65866fd9e09c8028b55545ee7`).
- **Primary PR-context input:** refreshed `artifacts/pr_context.summary.txt`.
- **Secondary PR-context input:** refreshed `artifacts/pr_context.appendix.txt`.
- **Feature folder:** `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469`, selected by the exact #469 feature-document match in the refreshed PR context.
- **Work mode and authoritative requirements source:** `full-bug`; `spec.md` only.
- **Additional inputs:** nine current-head P1 command-evidence reconciliation records at `evidence/qa-gates/*command-evidence-reconciliation.2026-08-31T10-10.md`.

## Acceptance Criteria Inventory

The authoritative `spec.md` contains 13 checked acceptance criteria: stale rationale removal; interface-contract rationale retention; test-comment correction; named filter guard; production defect-number alignment; test label alignment; documentation-only C# delta; file-size ceiling; unchanged test count; C# toolchain; CFN-2 resolution; #629 scope boundary; and pre/post test-count evidence.

## Acceptance Criteria Evaluation

| # | Criterion | Status | Current review evidence |
|---:|---|---|---|
| AC1 | Stale production token absent | PASS | Existing targeted evidence and the refreshed complete-range context remain consistent. |
| AC2 | Interface-contract rationale and filter retained | PASS | P1-T3 records both required filter-token counts as 1. |
| AC3 | Stale test token absent | PASS | Existing targeted evidence is present in refreshed PR context. |
| AC4 | Filter guard passes | PASS | Recorded named guard run: 4/4 passed. |
| AC5 | Production defect labels align | PASS | P1-T5 confirms documentation-only classification; existing targeted evidence is present. |
| AC6 | Test labels align and bodies unchanged | PASS | P1-T5 confirms test deltas are XML documentation/diagnostic text only. |
| AC7 | No executable C# delta | PASS | P1-T5 classifies all 28 C# diff lines as comments, XML documentation, or `because:` strings. |
| AC8 | File-size constraint | PASS | P1-T4 records 2,446 lines for `QfcCollectionController.cs`, with no increase attributed to #469. |
| AC9 | Test count unchanged | PASS | P1-T6 test-method counts and recorded 1,254/1,254 test run support the invariant. |
| AC10 | Full C# toolchain | PARTIAL | Analyzer, nullable, tests, and coverage pass by recorded evidence; the current full-tree CSharpier check exits 1 for 35 baseline-equivalent configuration paths, and CI format-check remains red. |
| AC11 | CFN-2 resolved | PASS | Existing CFN-2 evidence is present in the refreshed PR context. |
| AC12 | #629 scope boundary | PASS | P1-T1 excludes the protected file; P1-T2 records `StackMovedItems` count 2; P1-T8 finds no #469-attributable configuration/project path. |
| AC13 | Pre/post test-count evidence | PASS | Required baseline, post-change, and comparison artifacts are present. |

## Summary

The nine command-metadata gaps are cleared by current-head corroboration records and are not a remaining feature blocker. Twelve acceptance criteria pass. AC10 remains partial only because the independent full-tree CSharpier command and GitHub CI format-check are red; `p2-t2-csharpier-set-comparison.2026-08-31T10-15.md` shows that all 35 reported paths predate or are otherwise equal to the retained baseline and none is a #469 C# path. This distinction is material: the metadata reconciliation is complete, while the CI gate is not green.

## Acceptance Criteria Check-off

No authoritative requirements source was modified. The 13 `spec.md` checkboxes were already checked before this assigned evidence-only review. The review evaluates AC10 as PARTIAL for PR-readiness because the independent CI gate remains red; this does not change the historical execution check-off.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/spec.md`
- Total AC items: 13
- Evaluated PASS: 12
- Evaluated PARTIAL: 1 (AC10)
- Source checkboxes: 13 checked, 0 unchecked; unchanged by assigned scope.
- Remaining release condition: resolve or receive an authorized disposition for the red CI format-check.
