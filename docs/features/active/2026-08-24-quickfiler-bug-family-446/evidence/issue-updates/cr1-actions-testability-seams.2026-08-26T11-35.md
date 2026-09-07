# CR-1 Follow-up Issue Mirror - QfcFormController.Actions.cs Testability Seams

Timestamp: 2026-08-26T11-35

Origin: feature review finding CR-1 / feature-audit residual item 3, not a plan task.
Feature: docs/features/active/quickfiler-bug-family-446
Promotion type: refactor
Work mode: minor-audit

## Why this was filed

The `2026-08-26T11-29` feature review accepted the `QfcFormController.Actions.cs` coverage carve-out
as legitimate, but recorded one gap: no promoted document routed the resulting testability-seam
debt. A finding that lives only as prose inside an active feature folder is lost when that folder is
archived at epic close, so the debt was promoted to a real issue.

## MCP Calls

Tool: mcp__drm-copilot__new_potential_entry
Result: ok=true
Potential-entry path returned: `docs/features/potential/2026-08-26-qfcformcontroller-actions-testability-seams.md`

Tool: mcp__drm-copilot__potential_to_issue
Result: ok=true
Arguments: promotion_type=refactor, work_mode=minor-audit
Promoted record retained at: `docs/features/potential/promoted/2026-08-26-qfcformcontroller-actions-testability-seams.md`

## GitHub Issue

- Issue number: 624
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/624
- Source of the number and URL: the `artifacts` field of the promotion result payload.

Both payloads embedded the absolute worktree path in `workspace_root`, `artifacts` and
`destination_path`. Those paths are omitted here rather than reproduced, because the repository
artifact-hygiene rule forbids absolute host paths in any committed artifact. Every other field is as
returned.

## Substance Recorded on the Issue

The uncovered set in `QuickFiler/Controllers/QfcFormController.Actions.cs` is three blocks:

| Lines | Block | Why untestable today |
| --- | --- | --- |
| 29-160 | `LoadItems` / `LoadItemsAsync` overloads | bound to `TableLayoutPanel` and Outlook COM with no seam |
| 241-258 | `ProcessUndoItemAsync` | COM-and-dispatcher take branch |
| 267-306 | `UndoDialog` | three modal `MessageBox.Show` calls at :225, :238, :248 |

The issue carries one correction to the original #446 carve-out rationale, established by the
review's independent re-derivation of the uncovered-line map: the carve-out named only the
`MessageBox.Show` calls, which understates the problem. A dialog seam alone would lift the file to
roughly 67%, not past 90%, because the COM-bound loader overloads at 29-160 dominate. A future plan
that seams only the dialog will miss the target. Recording that correction is the main reason this
mirror exists.

The issue also records that `[ExcludeFromCodeCoverage]` is not an acceptable route here, since the
repository's policy direction disfavors coverage exemptions where the real answer is a seam.

EXIT_CODE: 0

Output Summary: The CR-1 testability-seam debt was promoted to refactor issue #624 in minor-audit
mode, with the promoted record retained. The issue carries the corrected uncovered-line analysis
showing that a dialog-only seam reaches roughly 67% and is insufficient.
