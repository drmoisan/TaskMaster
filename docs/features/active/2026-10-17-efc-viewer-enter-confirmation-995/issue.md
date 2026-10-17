# EFC Viewer: Enter does not confirm the highlighted destination folder (Issue #995)

- Date captured: 2026-10-17
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/ (Issue #995)

- Issue: #995
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/995
- Last Updated: 2026-10-17
- Work Mode: full-bug

## Summary

Pressing Enter after highlighting a destination folder in EFC Viewer did not invoke OK. A deterministic renderer regression failed before the fix and passes with the new confirmation message and existing OK boundary.

## Environment

- TaskMaster QuickFiler EFC Viewer on Windows.
- Exact deployed version was not supplied.

## Steps to Reproduce

1. Open EFC Viewer to sort an email into a folder.
2. Highlight the destination folder.
3. Press Enter.

## Expected Behavior

Enter invokes the same confirmation action as clicking OK for the highlighted valid destination.

## Actual Behavior

Nothing happens; the OK action is not invoked.

## Logs / Screenshots

No logs or screenshots supplied.

## Impact / Severity

- [x] Medium
- Keyboard confirmation of a selected destination is unavailable.

## Suspected Cause / Notes

The document previously consumed Enter and posted selection-only `rowSelected`. Plain, nonrepeated Enter now sends `rowConfirmed`; the router validates current row identity and retained destination, then requests `ButtonOK_Click`. Ancestor/child destinations survive. Cleanup and reconfiguration detach the subscription. Modified/repeated Enter and ordinary selection do not confirm.

Fail-before: `evidence/regression-testing/issue-995-enter-red.2026-10-17T23-03.md`. Pass-after: `evidence/regression-testing/issue-995-csharp-green.2026-10-17T23-09.md` (232 tests). All 49 routing/attestation tests pass; native MCP `1.1.21` validates current receipts in `evidence/qa-gates/issue-995-mcp-compatibility.2026-10-17T23-09.md`. Final full QA, coverage comparison and independent review remain pending.

The user also authorized repair of the model-routing workflow that prevented this fix from proceeding. The expanded scope includes the inherited Codex profiles, routing configuration, PowerShell resolver and attestation hook, supporting skill references, and the MCP dependency pin. The authoritative requirements and acceptance criteria for this `full-bug` work mode are in `spec.md`.

## Proposed Fix / Validation Ideas

- Add a deterministic regression test before fixing the behavior.
- Route Enter through the existing OK action while retaining invalid-selection guards.
- Verify keyboard modifiers and existing cancellation behavior.
- Run the required C# formatting, analyzers, nullable checks, and tests.
- Verify the complete inherited workflow-repair diff with routing/profile contract tests and the required PowerShell quality gates; retain mandatory validator and review gates.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch

## Acceptance Criteria

These original intake criteria are retained for traceability. `spec.md` is the acceptance-criteria source for the persisted `full-bug` work mode.

- [ ] Pressing Enter with a valid highlighted destination invokes the same action as OK exactly once.
- [ ] Invalid or absent destinations remain guarded by existing confirmation validation.
- [ ] Existing navigation, modifier-key, and cancellation behavior remains unchanged.
- [ ] A deterministic regression test fails before the fix and passes afterward.
- [ ] Required C# quality gates pass with recorded evidence.
