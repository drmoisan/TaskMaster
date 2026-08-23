Timestamp: 2026-07-21T22-06Z
Command: Inspect the 19 `spec.md` acceptance-criteria lines, compute SHA-256 over each criterion after removing only its checkbox marker, change only the required checkbox markers, and recompute the inventory and wording hash.
EXIT_CODE: 0
Output Summary: The authoritative specification now records 6 review-supported checked criteria and 13 open remediation criteria. Criterion wording and order are unchanged.

- Checked: AC-2, AC-3, AC-4, AC-9, AC-17, AC-18
- Open: AC-1, AC-5, AC-6, AC-7, AC-8, AC-10, AC-11, AC-12, AC-13, AC-14, AC-15, AC-16, AC-19
- Wording/order SHA-256 before: `85F08730E24A6A4BED0092802FA173D94DDE86F20007B92847B23ED73A8F7EB3`
- Wording/order SHA-256 after: `85F08730E24A6A4BED0092802FA173D94DDE86F20007B92847B23ED73A8F7EB3`
- Checked count: 6
- Open count: 13

`evidence/qa-gates/spec-checkbox-reconciliation.2026-07-21T21-23.md` is superseded and invalid as a current 19-PASS reconciliation source because the fresh 21:27 review found 13 failed criteria.
