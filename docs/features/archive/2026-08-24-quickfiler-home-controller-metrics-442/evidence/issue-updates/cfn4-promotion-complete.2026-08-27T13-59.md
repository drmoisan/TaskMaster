# CFN-4 Promotion — COMPLETE (supersedes the 2026-08-26T11-32 blocker)

Timestamp: 2026-08-27T13-59
Task: [P7-T4]
Command: `mcp__drm-copilot__new_potential_bug_entry` then `mcp__drm-copilot__potential_to_issue`
EXIT_CODE: 0
PostedAs: body
IssueUpdatedAt: 2026-08-27T13-58

## Result

CFN-4 is promoted to **issue #645**: https://github.com/drmoisan/TaskMaster/issues/645

- Title: `Bug: quickfiler-session-metrics-twelve-hour-time-format`
- State: `OPEN`, verified with `gh issue view 645 --repo drmoisan/TaskMaster --json number,title,state,url,body`
- Body length: 4190 characters; every section of the potential entry was mapped into the issue body,
  including the acceptance criteria, the proposed fix, and the backward-compatibility statement.
- Work-mode marker persisted in the issue body: `- Work Mode: full-bug`

The CFN-4 section of `docs/features/active/quickfiler-home-controller-metrics-442/spec.md` now
carries the issue number in place of the literal `PROMOTION BLOCKED`, which satisfies the
[P7-T4] acceptance condition and unblocks AC-25 as far as CFN-4 is concerned.

## Why the earlier blocker artifact is superseded, not contradicted

`evidence/issue-updates/cfn4-promotion-blocked.2026-08-26T11-32.md` recorded
`PROMOTION BLOCKED` because the executing session's tool surface carried only the PoshQC
drm-copilot MCP tools, and the repository policy hook `PROMOTION_MCP_ONLY_BLOCKED` correctly
refused the direct `gh issue create` fallback. That record was accurate for that session. The
resuming session carries the promotion MCP tools, so the approved lifecycle was reachable and was
run. Both artifacts are retained: the blocker documents why the first attempt could not proceed,
this one documents the completed promotion.

## Promotion lifecycle receipts

`new_potential_bug_entry`:

```json
{
  "ok": true,
  "tool": "new_potential_bug_entry",
  "short_name": "quickfiler-session-metrics-twelve-hour-time-format",
  "artifacts": [
    "<session-checkout>/docs/features/potential/2026-08-27-quickfiler-session-metrics-twelve-hour-time-format.md"
  ]
}
```

`potential_to_issue`:

```json
{
  "ok": true,
  "tool": "potential_to_issue",
  "promotion_type": "bug",
  "work_mode": "full-bug",
  "artifacts": ["https://github.com/drmoisan/TaskMaster/issues/645"],
  "destination_path": "<session-checkout>/docs/features/potential/promoted/2026-08-27-quickfiler-session-metrics-twelve-hour-time-format.md",
  "target_repository": "drmoisan/TaskMaster"
}
```

`new_active_feature_folder` was deliberately **not** run. CFN-4 is a follow-up defect that this
feature does not deliver; creating an active feature folder for it would seed an implementation
lifecycle nobody is executing. The promotion requirement in AC-25 is satisfied by the issue.

## Where the lifecycle records live, and why they are not committed on this branch

Both the potential entry and the promoted record were created in the **session checkout**, not in
this feature's worktree:

- created: `docs/features/potential/2026-08-27-quickfiler-session-metrics-twelve-hour-time-format.md`
- moved to: `docs/features/potential/promoted/2026-08-27-quickfiler-session-metrics-twelve-hour-time-format.md`

That location was chosen so this child branch's diff stays inside its declared ownership boundary.
AC-19 and [P7-T8] require every changed path to be one of the five owned production files, one of
the two owned test files, or a path under
`docs/features/active/quickfiler-home-controller-metrics-442/`. A `docs/features/potential/**` path
is none of those. Committing the record here would have broken a gate this feature is required to
pass, in order to store a local copy of content that already exists verbatim in issue #645.

The durable record is therefore the GitHub issue. The full text of the promoted record is mirrored
below so the audit trail inside this feature folder is complete without widening the diff.

`SearchScope:` `docs/features/potential/` and `docs/features/potential/promoted/` in the session
checkout.
`SearchPatterns:` `2026-08-27-quickfiler-session-metrics-twelve-hour-time-format.md`
`SearchResult:` the pre-promotion path is absent (the tool moves rather than copies it); the
promoted path is present at 6156 bytes.

## Mirrored promoted record

```markdown
# quickfiler-session-metrics-twelve-hour-time-format (Potential Bug)

- Work Mode: full-bug
- Date captured: 2026-08-27
- Status: Promoted to issue #645

## Summary

The QuickFiler session-metrics CSV renders its time-of-day field with the .NET format string
"hh:mm". Lowercase hh is the 12-hour clock, and the format carries no tt designator, so 14:30
renders as 02:30 and is indistinguishable from 02:30. Every row written since the format was
introduced carries an ambiguous time.

Three sites are affected, all in QuickFiler:

- QuickFiler/Controllers/QfcHomeController.Metrics.cs:31
- QuickFiler/Controllers/QfcHomeController.Metrics.cs:110
- QuickFiler/Controllers/EfcHomeController.Metrics.cs:68

Line numbers are as of the spec that raised this note; they shift slightly after the metrics work
described below.

Why this was split out rather than fixed alongside the metrics work. This was identified as
cross-feature note CFN-4 while delivering issues #442, #443 and #451 (feature
quickfiler-home-controller-metrics-442). It was deliberately excluded from that scope for three
reasons: it is a content defect whereas that feature's remit was the row shape, the flush, and
duration correctness; fixing it breaks three currently passing tests on their asserted literals
(QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs, two clock-seam tests, and
QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs, the formatted-row test), each of
which encodes the 12-hour rendering; and no issue in that family lists it as an acceptance
criterion. The sibling numeric-format defect at the same sites was fixed there: the six numeric
format calls now pass CultureInfo.InvariantCulture. The date and time format calls were left
untouched precisely so this defect could be tracked separately.

Proposed fix. Change the three format strings from "hh:mm" to "HH:mm" (24-hour) and update the
three asserted test literals to match. "HH:mm" is preferred over "hh:mm tt" because the adjacent
SentDate field already renders as "HH:mm:ss", so 24-hour is the file's existing convention and
keeps the row internally consistent. Consider passing CultureInfo.InvariantCulture to these calls
at the same time, matching what the numeric fields now do.

Backward compatibility. The session-metrics CSV has no in-repo reader: a repository-wide search
for EmailSession returns three settings-plumbing declarations and three writers, and no parser or
schema consumer. The artifact is write-only from the codebase's perspective. The residual risk is
confined to a human-maintained spreadsheet outside the repository.

## Impact / Severity

Medium: the emitted data is silently wrong rather than absent, and the file has no in-repo reader,
so nothing in the product misbehaves. The cost is borne by whoever analyses the CSV outside the
repository.

## Acceptance criteria carried into issue #645

- All three sites render the time-of-day field on a 24-hour clock.
- A repository search for the 12-hour format literal under QuickFiler/ returns no match.
- The three affected test literals are updated and the full QuickFiler test suite is green.
- The change is stated in the PR body, since it alters the emitted CSV content.
```

## Effect on AC-25

AC-25 has two halves. CFN-4's half is now satisfied: it is promoted to its own GitHub issue via the
promotion lifecycle and the number is written back into the CFN-4 section. The CFN-1 / CFN-2 /
CFN-3 half is satisfied by
`evidence/issue-updates/cross-feature-notes-handoff.2026-08-26T11-32.md`, which records each note's
owning sibling feature and states that none of the three is fixed in this feature's diff.
