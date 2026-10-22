# POSTING BLOCKED — R-1 Follow-Up Promotion (P5-T12)

Timestamp: 2026-08-27T12-13
Task: [P5-T12]
Command: (not executed — the required MCP promotion tools are not available in this session; see Reason)
EXIT_CODE: BLOCKED
Output Summary: The R-1 follow-up bug could not be promoted. The two `drm-copilot` MCP promotion
tools the task requires are not exposed in this executor session's tool set, and the direct `gh`
fallback is denied by a `PreToolUse` hook. The complete intended issue body is preserved verbatim
below so the orchestrator can complete the promotion without re-deriving it. No issue was created and
no receipt payload exists.

PostedAs: not posted
IssueUrl: (none — not created)
IssueNumber: (none — not created)

## Reason

The task directs: "Use the MCP promotion path instead — create the potential bug entry with the
`drm-copilot` potential-bug-entry tool, then promote it to an issue with the `drm-copilot`
issue-promotion tool, passing an absolute `potential_path`."

This executor session exposes exactly four `drm-copilot` MCP tools:

- `mcp__drm-copilot__run_poshqc_format`
- `mcp__drm-copilot__run_poshqc_analyze`
- `mcp__drm-copilot__run_poshqc_test`
- `mcp__drm-copilot__run_poshqc_analyze_autofix`

Neither promotion tool is present. The hook
`.claude/hooks/enforce-promotion-mcp-only.ps1:36` names the required chain explicitly as
`mcp__drm-copilot__new_potential_entry` -> `mcp__drm-copilot__potential_to_issue` ->
`mcp__drm-copilot__new_active_feature_folder`; none of those three is callable here.

The `gh issue create` fallback was **not attempted**, for two reasons. The task text forbids it
outright, and the same hook denies it: `.claude/hooks/enforce-promotion-mcp-only.ps1:99` treats
`gh issue create` and `gh issue new` as direct bypasses of the MCP path and returns
`PROMOTION_MCP_ONLY_BLOCKED`. Attempting it would have been both a plan violation and a hook denial.

Writing a potential-entry file directly under `docs/features/potential/` was also not attempted: that
path is outside this plan's § Scope Lock, which permits writes only to the five named source paths
and to `<FEATURE>` and its evidence tree.

## Required orchestrator action

Run the two-step MCP promotion with the body below, then replace this artifact — or add a sibling
artifact — carrying `PostedAs: body`, the new issue's URL and number, and the raw receipt payload
returned by each of the two promotion calls, as the task's acceptance condition requires.

## Intended issue title

`Bug: route WpfUiDispatcherTests static swap through the shared UiThreadDispatcherFixture`

## Intended issue body, verbatim

### Summary

`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` swaps the process-wide static
`UtilitiesCS.UiThread._dispatcher` to a *running* dispatcher by raw reflection, restores it in a plain
`finally`, and participates in neither of the two locks introduced by #493. After #493 lands it
remains an ungated mutator of the same static and can still lose an update against a transaction held
by the QuickFiler pump fixtures.

### Affected file

`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` — the swap and its `finally` restore.

### Why this is a separate issue rather than part of #493

The file is not in #493's owned file set. #493 was scoped to
`QfcItemController.TestSupport.cs` and `QfcItemController.InitializationTests.Part2.cs` plus two new
files, and was recorded as accepted residual risk **R-1** in
`docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md` § Risks & Mitigations, with
§ Rollout & Follow-up item 3 asking that it be promoted as its own small issue once the shared fixture
exists. It now exists.

### Proposed fix

Route the swap through the shared fixture that #493 created:

- Replace the raw reflection swap with
  `await UiThreadDispatcherFixture.BeginTransactionAsync()` followed by
  `transaction.Install(<the running dispatcher>)`.
- Replace the `finally` restore with `transaction.Dispose()`, which restores conditionally
  (`ReferenceEquals` compare-then-write) and then releases the gate, in that order.
- Do not reintroduce a second reflection lookup; `UiThreadDispatcherFixture` is intended to be the
  single owner of every mutation of that static made from this assembly's owned files, and #493's
  AC-4 gates that uniqueness.

`UiThreadDispatcherFixture` and `UiThreadDispatcherTransaction` live in
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`. Both are `internal` to
`QuickFiler.Test`, so no new grant or reference is needed.

### Risk and priority

Low. That assembly runs sequentially in CI
(`.github/workflows/_mstest-coverage.yml` supplies no `/Settings:`), so the race is dormant there; it
is reachable only under the repo runsettings, which force `<Scope>ClassLevel</Scope>` with
`Workers=0`. The swap is single-class and short-lived. This is a small, bounded change.

### Out of scope

The cross-assembly mutators in `UtilitiesCS.Test` — `ProgressTracker_Tests.cs`,
`ProgressTrackerAsync_Tests.cs`, and `IdleAsyncQueue_Tests.cs` — mutate the same process-wide static
and are **not** covered by this issue. No test-side lock inside `QuickFiler.Test` can reach them.
They are accepted residual risk R-2 of #493, and they overlap #584.

### References

- Motivating fix: #493
- Adjacent open issue on the same static: #584
- Originating defect report: #230
