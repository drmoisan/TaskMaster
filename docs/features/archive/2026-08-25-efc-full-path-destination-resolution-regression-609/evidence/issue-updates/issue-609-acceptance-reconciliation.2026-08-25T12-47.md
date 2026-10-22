Timestamp: 2026-08-25T12-47
Command: Reconcile spec.md acceptance criteria against P2-T2, P2-T3, and P3-T1 through P3-T5 evidence
EXIT_CODE: 0
Output Summary: Spec.md is the authoritative full-bug AC source. Eight criteria total: seven proven and checked, one remains unchecked.

# Acceptance reconciliation

Source: `docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609/spec.md`

- Checked criteria 1-4: P2-T2 proves full lookup, relative direct/ancestor/child selections, and single-prefix configuration output.
- Checked criterion 6: P0-T2 scope evidence and the no-production-edit result retain the prohibited `@` parsing and `Store.FilePath` boundaries.
- Checked criterion 7: P1-T6 passed before production edits; no correction was made outside the router boundary.
- Checked criterion 8: P3-T1 through P3-T4 passed; P3-T5 coverage comparison is recorded in canonical evidence locations.
- Unchecked criterion 5: `Existing banner, trash, root-boundary, and relative search/suggestion behavior remains covered and unchanged.` P2-T3 verifies existing router and configuration boundary tests, but does not directly prove the user-reported upstream initial potential-folder-row/suggestion-generation path. That scope is not covered or fixed by this plan evidence.

Total AC items: 8
Checked off: 7
Remaining: 1
Items remaining: Existing banner, trash, root-boundary, and relative search/suggestion behavior remains covered and unchanged.
