---
name: 959-review-residuals
description: '#959 (SortEmail latent logic defects L1-L4 plus folded #966 residuals, EfcDataModel finally, ToDoModel duplicate) full-bug review 2026-10-06T15-30 PASS 25/27 AC (AC6/AC27 pending the PR body, no class given), 0 blocking, 7 non-blocking; sync/async core pairs need both ternary arms pinned; pre-existing sub-floor modified file disposed on the no-regression limb; validator artifact_path must be workspace-relative'
metadata:
  type: project
---

Full-bug review (parallel cohort bugs-2026-09-28, item worktree `repos/TaskMaster/.claude/worktrees/agent-ae9faf6e1bf21ac17`, head
`0fab75ed6`, merge base `942873699` = origin/main at cut, no main merge yet): PASS, 25/27 AC, AC6 and AC27 PENDING on the PR body
(caller said "UNVERIFIED/pending, not FAIL" with NO remediability class, so PASS and no remediation-inputs, the #950 shape rather than
the #968 AWAITING_CI shape), 0 blocking, CR-1..CR-7 non-blocking, G-1..G-7, U-1..U-3. Caller forbade Bash; all three artifacts validated
first try. Advertised the five-`..` path form from the TaskMaster-wt session cwd as at #968.

**Reusable verification points:**
- `mcp__drm-copilot__validate_orchestration_artifacts` JOINS `workspace_root` and `artifact_path`: an absolute artifact_path fails with
  ENOENT on a doubled path. Pass `docs/features/active/<feature>/<stem>.<ts>.md` relative to the worktree root.
- A seamed sync/async core PAIR with the same `IsImage ? pictures : attachments` ternary: the async core had image and document tests,
  the sync core's three tests all used `report.pdf`, so the sync ternary read `condition-coverage="50% (1/2)"` while the file read 100%
  lines. Read the `<class>` node for every new core, not just the per-member line percentages the executor reports (CR-1, one-test fix).
- A new `throw;` inside an async `catch` reads `branch="True" condition-coverage="50% (1/2)"` (the unreachable fall-through of the
  state machine's rethrow) and the brace after it `hits="0"`: the same shape as a pre-existing `else { throw; }`. Branch-rate drops
  (92.86% -> 90.00%) from such arms are not regressions; say which arm and cite the pre-existing analogue line.
- Modified file below every floor at baseline (EfcDataModel.cs 75.69/73.08 -> 76.34/73.08, changed lines all hit, QuickFiler interop
  controller within CLAUDE.md UT2 exemption (c)'s named assemblies): FAIL on the per-file threshold limb, PASS on the no-regression limb,
  non-blocking, filed as unrelated coverage debt. Cite the Bugfix Workflow minimal-fix clause for why uplift is out of the item.
- Same-root-cause call site in an UNCOMPILED file (`QuickFiler/Legacy/QfcController.cs:792`, no `Compile Include` under `Legacy\`,
  calls a `SortEmail.Run` that does not exist): report as Minor with two dispositions (delete under the D12 uncompiled-duplicate precedent,
  or file a folder cleanup) and leave the choice to the orchestrator; no in-item behavioural fix is possible.
- Promotion-lifecycle record under `docs/features/potential/promoted/` shows in the branch diff but outside the AC's write-set list;
  the footprint gate subtracts it under its inherited clause. Evaluate the write-set AC on intent and disclose the record.
- Clock without a shell: worktree reflog `repos/TaskMaster/.git/worktrees/<wt>/logs/HEAD`; choose the review label strictly after the
  last epoch and say it is not a clock read. Cobertura root `timestamp=` sat 4 min before the executor's 12-46 label and 283 s before the
  P6-T7 commit epoch; the 2026-10-06 labels matched their commit epochs to the minute.
- Hook-safe policy-audit: the TS/PS/Python 1.2.1 bullets and metrics rows may carry `N/A` because they contain no C#/csharp/.NET/dotnet
  label; every line that has BOTH a C# label (including `dotnet-coverage`, `artifacts/csharp/`) AND `coverage` must stay free of
  N/A / UNVERIFIED / out of scope. Grep the finished audit for the hook's narrowing regex and inspect each hit's labels.
- AC wording drift to evaluate on intent: "has exactly one catch clause" (the outer try) and "gains one DataRow row" (two were added
  under a planner correction) — record as Informational, not FAIL.

**Follow-ups owed to the orchestrator:** CR-1 sync-core image-arm test (related, in-item); CR-2 QfcController.cs decision; CR-3 unused
`using System;` in SortEmail_SaveCase_Tests.cs; G-1/U-1 EfcDataModel coverage uplift (file); U-2 `QuickFiler/Legacy/` uncompiled folder
(file); G-5/U-3 `quality-tiers.yml` absent (recurring); AC6 and AC27 close at the pr-author step (UT5 call-out and closing references).
