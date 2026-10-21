# CFN-4 Promotion — POSTING BLOCKED

Timestamp: 2026-08-26T11-32
Task: [P7-T4]
Command: `gh issue create --repo drmoisan/TaskMaster --title '...' --body-file '<scratchpad>/cfn4-body.md'`
EXIT_CODE: 1
PostedAs: unknown

## POSTING BLOCKED

CFN-4 was **not** promoted to a GitHub issue. The CFN-4 section of `spec.md` records
`PROMOTION BLOCKED` together with the path of this artifact, and AC-25 is left unchecked per
[P7-T33].

### Blocker

A repository policy hook rejected the `gh` invocation:

> PROMOTION_MCP_ONLY_BLOCKED: Direct GitHub issue creation via `gh` bypasses the approved
> drm-copilot MCP promotion path (`mcp__drm-copilot__new_potential_entry` ->
> `mcp__drm-copilot__potential_to_issue` -> `mcp__drm-copilot__new_active_feature_folder`). Use
> those MCP tools instead.

The three MCP tools that hook names are **not present in this executing agent's tool surface**. The
only drm-copilot MCP tools available here are the PoshQC ones
(`run_poshqc_format`, `run_poshqc_analyze`, `run_poshqc_test`, `run_poshqc_analyze_autofix`). The
approved promotion path is therefore unreachable from this session, and the unapproved path is
correctly refused by the hook.

This is not the literal condition [P7-T4] anticipated. That task's fallback is written for the case
where the `GH:` line of the [P0-T3] toolchain probe recorded `NOT_FOUND`. It did not: `gh` resolved
to `C:\Program Files\GitHub CLI\gh.exe` and `gh auth status` reports an authenticated account with
`drmoisan/TaskMaster` as the resolved repository. The obstruction is a policy restriction rather
than a missing tool, but the outcome is identical, so the fallback branch was taken. [P7-T4]
directs explicitly that this is not a reason to halt, and the plan was not halted.

### Required follow-up

An agent or operator holding the drm-copilot promotion MCP tools should run the lifecycle
`new_potential_entry` -> `potential_to_issue` using the exact title and body below, then write the
resulting issue number into the CFN-4 section of
`docs/features/active/quickfiler-home-controller-metrics-442/spec.md` in place of
`PROMOTION BLOCKED`, and check off AC-25.

## Exact issue title to be filed

```
QuickFiler session-metrics time field uses 12-hour "hh:mm" with no AM/PM designator
```

## Exact issue body to be filed

## Summary

The session-metrics CSV renders its time-of-day field with the format string `"hh:mm"`. In .NET,
lowercase `hh` is the 12-hour clock. With no `tt` designator in the format, 14:30 renders as
`02:30` and is indistinguishable from 02:30. Every row written since the format was introduced
carries an ambiguous time.

## Locations

All three sites are in QuickFiler:

- `QuickFiler/Controllers/QfcHomeController.Metrics.cs:31`
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs:110`
- `QuickFiler/Controllers/EfcHomeController.Metrics.cs:68`

Line numbers are as of the spec that raised this note; they shift slightly after the metrics work
described below.

## Why this was split out rather than fixed alongside the metrics work

This was identified as cross-feature note CFN-4 while delivering issues #442, #443, and #451
(feature `quickfiler-home-controller-metrics-442`). It was deliberately excluded from that scope
for three reasons:

1. It is a **content** defect, whereas that feature's remit was the row **shape**, the flush, and
   duration correctness.
2. Fixing it breaks three currently passing tests on their asserted literals:
   `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` (two clock-seam tests) and
   `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` (the formatted-row test). Those
   literals encode the 12-hour rendering and would each need updating.
3. No issue in that family lists it as an acceptance criterion.

Note that the sibling numeric-format defect at the same sites **was** fixed there: the six numeric
format calls now pass `CultureInfo.InvariantCulture`. The date and time format calls were left
untouched precisely so this defect could be tracked separately.

## Proposed fix

Change the three format strings from `"hh:mm"` to `"HH:mm"` (24-hour), and update the three
asserted literals in the two test files to match. `"HH:mm"` is preferred over `"hh:mm tt"` because
the adjacent `SentDate` field already renders as `"HH:mm:ss"`, so 24-hour is the file's existing
convention and keeps the row internally consistent. Consider passing
`CultureInfo.InvariantCulture` to these calls at the same time, matching what the numeric fields
now do.

## Acceptance criteria

- [ ] All three sites render the time-of-day field on a 24-hour clock.
- [ ] A repository search for the 12-hour format literal under `QuickFiler/` returns no match.
- [ ] The three affected test literals are updated and the full QuickFiler test suite is green.
- [ ] The change is stated in the PR body, since it alters the emitted CSV content.

## Backward compatibility

The session-metrics CSV has **no in-repo reader**: a repository-wide search for `EmailSession`
returns three settings-plumbing declarations and three writers, and no parser or schema consumer.
The artifact is write-only from the codebase's perspective. The residual risk is confined to a
human-maintained spreadsheet outside the repository.

## Provenance

Promoted from cross-feature note CFN-4 of
`docs/features/active/quickfiler-home-controller-metrics-442/spec.md`, per that feature's AC-25.
