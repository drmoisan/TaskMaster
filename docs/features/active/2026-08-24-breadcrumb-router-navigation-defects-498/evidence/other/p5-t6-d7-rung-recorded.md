# P5-T6 — Decision D7 Rung Recorded in the Spec

Timestamp: 2026-08-26T10-48

Command: `grep -c -F "to be recorded during execution" docs/features/active/breadcrumb-router-navigation-defects-498/spec.md`

EXIT_CODE: 1

## Output Summary

The RISK-1 placeholder has been replaced and the literal is gone from the spec.

**Fixed-string search result: `0` matches.** `grep -c -F` prints `0` and exits `1` when a
fixed-string pattern is absent, which is the required outcome here: the phrase
`to be recorded during execution` no longer appears anywhere in
`docs/features/active/breadcrumb-router-navigation-defects-498/spec.md`. Before the edit the same
search matched exactly one line, `spec.md:1164`, in the RISK-1 entry.

**What the RISK-1 entry now records** (`spec.md:1164` onward):

- The rung number: `Rung taken: RUNG 1 (PREFERRED), delivered.`
- The read-only evidence that selected it: `BreadcrumbSelectionMap.RowValue` (`:109`) reads the
  leaf chain segment's `FolderPath`; that segment is constructed by the OWNED
  `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` through the public immutable
  `FolderBreadcrumbSegment` constructor (`FolderBreadcrumbSegment.cs:29-40`), which separates `Key`
  from `FolderPath`; and the only other readers of `BreadcrumbStateRow.Chain` read `DisplayName`
  (`BreadcrumbRenderProjection.cs:177`) or `Key` (`FolderBreadcrumbBridgeRouter.cs:416`).
- The path of the `P4-T1` artifact:
  `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/other/p4-t1-d7-rung-verification.md`,
  cited by name and recorded as carrying the line `D7 RUNG SELECTED: 1`.
- How rung 1 was delivered, and the fail-before / pass-after evidence pair
  (`evidence/regression-testing/p5-t2-d7-red.md`,
  `evidence/regression-testing/p5-t3-d7-rung1-green.md`), together with the confirmation that
  `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs` is unmodified.
- That rungs 2 and 3 do not apply, with pointers to their NOT-APPLICABLE artifacts.

Satisfies the AC-14 recording clause.
