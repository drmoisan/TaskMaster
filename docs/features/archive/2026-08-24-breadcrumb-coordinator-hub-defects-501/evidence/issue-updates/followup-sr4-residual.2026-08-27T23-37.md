# Issue Update Mirror — follow-up issue #656 (P9-T3)

Timestamp: 2026-08-27T23-37

PostedAs: body

Issue URL: https://github.com/drmoisan/TaskMaster/issues/656

IssueState: OPEN, confirmed with `gh issue view 656`

## Purpose

P9-T3 requires filing a follow-up issue for the SR-4 known limitation (`_closeCompleted` residual) against feature 488's host paths, explicitly out of scope for this feature.

## Exact text posted

The issue body below is the promoted content of the potential entry, reproduced verbatim. The promotion
tooling carried every section through, so no section was dropped and no supplementary comment was
needed.

```
- Work Mode: full-bug

## Summary
`BreadcrumbDropDownOpenCoordinator._closeCompleted` stays `true` when the drop-down host is reopened by
a path that reaches neither `RequestOpen` nor `Invalidate`, so a subsequent close is wrongly suppressed.
This is the known residual of the SR-4 two-flag close fix shipped for #462 under #501, recorded against
the host paths owned by feature #488.

## Environment
- OS/version: Windows 11, Outlook VSTO add-in host
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`
- Data source or fixture: `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` harness

## Steps to Reproduce
1. Open the breadcrumb drop-down host and close it through `CloseCore`, so `_closeCompleted` becomes `true`.
2. Reopen the host through a path that reaches neither `RequestOpen` nor `Invalidate`.
3. Request a close.

## Expected Behavior
The close request reaches `_host.Close`, because the host is genuinely open again.

## Actual Behavior
The coordinator still treats the host as already closed and suppresses the close. `_closeCompleted` was
never cleared, because it is cleared only on the `RequestOpen` and `Invalidate` paths.

## Logs / Screenshots
- [ ] Attached minimal logs or screenshot
- Snippet: no runtime log; the residual is established by source inspection of the flag-clearing paths.

## Impact / Severity
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: it requires a reopen path that bypasses both entry points, which the currently exercised UI
flows do not take. It is a latent correctness gap rather than an observed user-facing failure.

## Source
From: docs/features/potential/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate.md
```

## Route deviation recorded

The plan task names `gh issue create` as the mechanism. That call was DENIED by a repository PreToolUse
hook:

```
PROMOTION_MCP_ONLY_BLOCKED: Direct GitHub issue creation via `gh` bypasses the approved
drm-copilot MCP promotion path.
```

The issue was therefore filed through the approved MCP promotion lifecycle instead, in two steps:
`mcp__drm-copilot__new_potential_bug_entry` to create the potential entry, then `mcp__drm-copilot__potential_to_issue` with `promotion_type` `bug` to promote it.

No wording was altered to evade the hook; only the route changed, to the one the repository mandates.
The approved route is also strictly better for durability, because it leaves a permanent promoted
record at `docs/features/potential/promoted/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate.md` in addition to the GitHub issue.

The plan task's acceptance is met in full: this mirror artifact exists, carries a
`https://github.com/drmoisan/TaskMaster/issues/` URL, and records `PostedAs: body`.
