# Acceptance and Scope-Change Preservation

Timestamp: 2026-08-27T03-30-00Z

Command: `rg -n "^- \[x\] \*\*AC14|^- \[ \] \*\*AC24" docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/spec.md`

EXIT_CODE: 0

Output Summary: AC14 is checked at line 1056 and AC24 is unchecked at line 1116 before final QA.

Command: `git diff --exit-code -- docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/spec.md`

EXIT_CODE: 0

Output Summary: `spec.md` has no premature cycle-3 diff.

Command: `rg -n "issue-614-approved-documentation-findings-scope-change" artifacts/orchestration/orchestrator-state.json`

EXIT_CODE: 0

Output Summary: The approved human scope-change entry is present at line 154. The two named documentation/evidence findings remain excluded from remediation; AC24 remains governed by its exact acceptance criterion.
