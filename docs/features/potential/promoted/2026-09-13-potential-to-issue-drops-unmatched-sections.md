# potential-to-issue-drops-unmatched-sections (Issue #887)

- Date captured: 2026-09-13
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/potential-to-issue-drops-unmatched-sections/ (Issue #887)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #887
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/887
- Last Updated: 2026-09-13
## Summary

`mcp__drm-copilot__potential_to_issue` silently drops source sections whose headings the bug-issue template has no matching slot for, even when those headings are the scaffold's own canonical headings. When it created issue #882, it dropped three sections — `## Suspected Cause / Notes`, `## Proposed Fix / Validation Ideas`, and `## Next Step` — from the promoted potential entry. A placeholder count of zero `(not provided in potential file)` markers in the resulting issue proves only that no template section went unfilled; it is not evidence that no source section was lost.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable; this is the `drm-copilot` MCP `potential_to_issue` promotion tool
- Command/flags used: `mcp__drm-copilot__potential_to_issue` invoked with `potential_path` = `docs/features/potential/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded.md`, `promotion_type=bug`, `work_mode=full-bug`
- Data source or fixture: promoted source at `docs/features/potential/promoted/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded.md` versus the created issue #882 body (`gh issue view 882 --json body`)

## Steps to Reproduce

1. Author a potential-bug entry using the standard scaffold from `new_potential_bug_entry`, filling every one of its ten canonical headings (`## Summary`, `## Environment`, `## Steps to Reproduce`, `## Expected Behavior`, `## Actual Behavior`, `## Logs / Screenshots`, `## Impact / Severity`, `## Suspected Cause / Notes`, `## Proposed Fix / Validation Ideas`, `## Next Step`) with real content.
2. Promote it with `potential_to_issue`.
3. Fetch the created issue body (`gh issue view <N> --json body -q .body`).
4. Diff the issue body against the promoted source file.
5. Observe that `## Suspected Cause / Notes`, `## Proposed Fix / Validation Ideas`, and `## Next Step` are entirely absent from the issue body — not present as empty sections, not present as `(not provided in potential file)` placeholders, simply not in the output at all. The issue body ends after `## Impact / Severity` followed by a `## Source` line pointing back at the potential file.

## Expected Behavior

Every canonical heading present and filled in the source potential-bug entry should either (a) survive into the created issue body with its content intact, or (b) if the tool's bug-issue template genuinely has no matching section, the tool should fail loudly or clearly flag the omission (e.g. an explicit "N sections dropped: ..." line in its return payload) rather than silently omitting the content with no signal in either the returned receipt or a placeholder marker.

## Actual Behavior

For issue #882, three sections were silently dropped with no signal: `## Suspected Cause / Notes` (containing a verbatim quotation of spec correction C2 from issue 743's spec and an explanation of its significance), `## Proposed Fix / Validation Ideas` (containing concrete verification-scenario guidance and an explicit "trap to avoid" — a statement that a clean run is not evidence of absence, load-bearing for the issue's argument), and `## Next Step`. The tool's return payload reported a normal success and a `destination_path`; nothing in the receipt indicated any content had been dropped. A placeholder count of zero `(not provided in potential file)` occurrences in the issue body is not evidence of fidelity — it proves only that the template's own sections were all filled, and says nothing about source sections the template has no slot for.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: promoted source `docs/features/potential/promoted/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded.md` lines 58-82 contain `## Suspected Cause / Notes`, `## Proposed Fix / Validation Ideas`, and `## Next Step` with full content (verified by direct read on 2026-09-13). Issue #882's body (`gh issue view 882 --json body -q .body`, read 2026-09-13) ends at `## Impact / Severity` followed immediately by a `## Source` line; none of the three sections appear anywhere in the body.

## Impact / Severity

- [x] High
- [ ] Blocker
- [ ] Medium
- [ ] Low

High: load-bearing content — including a verbatim quotation supporting the issue's central claim and an explicit trap warning meant to prevent a future reader from repeating a documented error — is lost silently on promotion, with no signal to the filer that anything was dropped. An existing agent-memory note (`.claude/agent-memory/orchestrator/potential-to-issue-keeps-only-summary-section.md`) claims these same three headings "landed in the issue body with full content" for a different promotion (issue #644, verified 2026-08-27); that claim does not hold for issue #882's promotion under the same bug template and is therefore stale and should be corrected.

## Suspected Cause / Notes

The promotion tool maps source sections onto the target GitHub issue template by heading-name match; a heading with no corresponding template slot is dropped rather than folded into a catch-all section or flagged. The bug-report template apparently does not always carry slots for `Suspected Cause / Notes`, `Proposed Fix / Validation Ideas`, and `Next Step` even though the potential-bug scaffold itself presents them as canonical, automation-mapped headings (the scaffold's own automation note reads: "Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template" — implying they should all map, which for issue #882 they did not).

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: add either (a) a catch-all "Additional Notes" section in the bug-issue template that folds in any unmatched scaffold heading verbatim, or (b) an explicit fail-loud/warn-loud path when the tool detects a source heading it has no template slot for, surfaced in the tool's return payload (not just silently proceeding to a normal-looking success).
- [x] Integration scenario to retest: re-promote a fully-filled scaffold (all ten headings) and assert byte-for-byte that every heading's content appears somewhere in the resulting issue body, not merely that the placeholder count is zero.
- [x] Manual verification notes: until the tool is fixed, every promotion must be followed by a manual diff of the promoted source against the created issue body, and any dropped section must be reposted as a comment on the issue (durable, no repository write required).

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Cross-reference: this defect was discovered while filing this very batch of four issues (per the coordinator's brief) and reproduced live during that filing; see the accompanying issue-comment reposts on each affected issue in this batch, if applicable.
