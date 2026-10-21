# Issue update mirror — issue 882 (new H-LEAK issue, filed under ratification condition 4)

Timestamp: 2026-09-13T18-05
PostedAs: body (issue creation via MCP promotion lifecycle) and comment (recovery of dropped template sections)
URL: https://github.com/drmoisan/TaskMaster/issues/882
Comment URL: https://github.com/drmoisan/TaskMaster/issues/882#issuecomment-5656393710
IssueUpdatedAt: 2026-09-13T18-05
EXIT_CODE: 0

## Creation

Commands, in order, both MCP:

1. `mcp__drm-copilot__new_potential_bug_entry` with `short_name=quickfiler-transactiongate-permit-leak-unexcluded`. Receipt: created `docs/features/potential/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded.md`.
2. `mcp__drm-copilot__potential_to_issue` with `promotion_type=bug`, `work_mode=full-bug`, absolute `potential_path`. Receipt: `{"ok":true,"artifacts":["https://github.com/drmoisan/TaskMaster/issues/882"],"destination_path":".../docs/features/potential/promoted/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded.md","target_repository":"drmoisan/TaskMaster"}`.

No active feature folder was created for issue 882, as directed by the ratification condition. `mcp__drm-copilot__new_active_feature_folder` was not called.

The body text as promoted is the promoted record, retained in the repository at `docs/features/potential/promoted/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded.md`, minus the three sections named below.

## Fidelity verification of the promoted body

Measured against the live issue body after creation:

- Body length: 5763 characters.
- `(not provided in potential file)` placeholder count: **0**.
- Occurrences of `743`: 11.
- The sentence "never excluded, only never observed": present (1 occurrence).
- The correction C2 quotation ("changed the owner of the serialization"): **0 occurrences — dropped.**

Section headings present in the issue body: `Summary`, `Environment`, `Steps to Reproduce`, `Expected Behavior`, `Actual Behavior`, `Logs / Screenshots`, `Impact / Severity`, `Source`.

Three source sections were dropped because the bug-report issue template carries no matching heading: `Suspected Cause / Notes`, `Proposed Fix / Validation Ideas`, `Next Step`. The promotion tool maps section by section on heading name and silently drops a heading the template does not declare. The zero placeholder count is therefore not by itself sufficient evidence of fidelity: it proves only that no template section went unfilled, not that no source section was lost. Both checks are required.

## Recovery of the dropped sections

The three dropped sections carried the load-bearing evidence of the issue — the correction C2 quotation, the "do not treat a clean run as evidence of absence" trap warning, and the cross-reference rationale. They were reposted verbatim as a comment: https://github.com/drmoisan/TaskMaster/issues/882#issuecomment-5656393710.

The body file for the comment was written outside the repository tree so no untracked artifact entered it.

## Cross-reference discharge

- Issue 882 names issue 743: 11 occurrences in the body, plus the comment.
- Issue 743 names issue 882: in `evidence/other/maintainer-ratification-ac1.2026-09-13T18-00.md`, in the issue-743 comment mirrored at `evidence/issue-updates/issue-743.2026-09-13T18-10.md`, and in this item's pull-request body.
