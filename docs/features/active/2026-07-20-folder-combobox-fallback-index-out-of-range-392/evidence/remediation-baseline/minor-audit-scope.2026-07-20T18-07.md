Timestamp: 2026-07-20T18-07

## Verification of minor-audit Scope-Lock boundary for remediation cycle 1 (issue #392)

Directory listing of `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/`
confirms the following `.md` files exist: `issue.md`, `plan.2026-07-20T12-59.md`,
`policy-audit.2026-07-20T18-00.md`, `code-review.2026-07-20T18-00.md`,
`feature-audit.2026-07-20T18-00.md`, `remediation-inputs.2026-07-20T18-00.md`,
`remediation-plan.2026-07-20T18-00.md`. **No `spec.md` and no `user-story.md`** are present —
confirms `issue.md` remains the sole AC source for this minor-audit cycle, unchanged from the
original cycle.

`issue.md`'s `Work Mode: minor-audit` marker (line 12) and its `## Acceptance Criteria` section
(AC-1 through AC-5) are unchanged from the original cycle; all five ACs are already `- [x]` checked
and are not reopened by this remediation cycle (per the remediation plan's Work Mode note).

Scope-Lock file list for this remediation cycle (unchanged from the original fix scope):
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` (R1: coverage-only addition, no new
  production code path, no behavior change)
- `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` (R1: 1-2 new `[TestMethod]`
  additions only, no new file)

No other production file may be changed by this cycle. This matches the remediation plan's
Scope-Lock section verbatim.

Command: `ls docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/*.md`
EXIT_CODE: 0
Output Summary: 7 markdown files present in the feature folder; neither `spec.md` nor `user-story.md`
among them. Scope-Lock boundary confirmed unchanged from the original cycle.
