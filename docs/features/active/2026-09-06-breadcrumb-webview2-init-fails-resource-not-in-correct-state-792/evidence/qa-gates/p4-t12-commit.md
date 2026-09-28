# [P4-T12] Phase 4 commit

- Issue: #792
- Timestamp: 2026-09-17T20-12
- Command: `git add -- QuickFiler QuickFiler.Test docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` then `git commit -m "fix(792): converge WebView2 environment contract, bounded breadcrumb retry, explicit discard, pop-out carry"` (run from `coverage/plan792-helper.ps1` with the item worktree as the working directory, on branch `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792`; the session's attribution trailer lines were supplied through a second `-m`, so the subject line is the plan's text verbatim); then `git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'` and `git diff --name-only $BaseSha HEAD -- '*.cs' '*.csproj'` with `$BaseSha` bound by CMD-BASE
- EXIT_CODE: 0
- Output Summary: `[bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792 c2a55b23] fix(792): converge WebView2 environment contract, bounded breadcrumb retry, explicit discard, pop-out carry`; `14 files changed, 507 insertions(+), 94 deletions(-)`; 9 production files and 1 test file modified, 4 feature-folder paths (3 evidence files created, the plan file modified with the [P3-T8] and [P4-T1] through [P4-T11] check-offs); scoped porcelain printed nothing; the BASE-SHA-to-HEAD `.cs`/`.csproj` footprint lists exactly 30 paths.

COMMIT-SHA-OBSERVED: c2a55b23523636e9f4d47b169d95bf8b596085c9

PARENT-SHA: 494f71381790c1f2f57531c0269eabe79890ad63 (the [P3-T8] commit)

## Paths in the commit (`git show --stat --format= HEAD`, 14 paths)

- `QuickFiler.Test/Viewers/WebView2EnvironmentContractTests.cs` (+84)
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` (+32, 0 deletions)
- `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs`
- `QuickFiler/Controllers/EfcDataModel.Carry.cs`
- `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`
- `QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs`
- `QuickFiler/Controllers/QfcCollectionController.PopOut.cs`
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
- `QuickFiler/Helper Classes/EfcViewerQueue.cs`
- `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`
- `docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/regression-testing/p3-t8-commit.md` (the Phase 3 residual left uncommitted by construction after [P3-T8])
- `docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/regression-testing/p4-t4-site3-mutation.md`
- `docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/regression-testing/p4-t11-pass-after.md`
- `docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/plan.2026-09-17T07-30.md`

No `.csproj`, no `.runsettings`, no `.config` and no file outside `QuickFiler/`, `QuickFiler.Test/` or the feature folder is in the commit.

## Acceptance observations

`git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'` printed nothing.

`git diff --name-only e7cbb57229c63a228e7fe0bcbcdbfbc06db8bcd3 HEAD -- '*.cs' '*.csproj'` (BASE-SHA to HEAD), recorded verbatim, 30 paths:

```
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue792Tests.cs
QuickFiler.Test/Controllers/BreadcrumbOutboundQueueIssue792Tests.cs
QuickFiler.Test/Controllers/EfcDataModelIssue792CarryTests.cs
QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs
QuickFiler.Test/Controllers/QfcCollectionControllerIssue792PopOutTests.cs
QuickFiler.Test/Helper Classes/EfcViewerQueueIssue792Tests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/Viewers/WebView2BreadcrumbHostIssue792Tests.cs
QuickFiler.Test/Viewers/WebView2EnvironmentContractTests.cs
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs
QuickFiler/Controllers/BreadcrumbOutboundQueue.cs
QuickFiler/Controllers/EfcDataModel.Carry.cs
QuickFiler/Controllers/EfcDataModel.cs
QuickFiler/Controllers/EfcFormController.Actions.cs
QuickFiler/Controllers/EfcFormController.Breadcrumb.cs
QuickFiler/Controllers/EfcFormController.EventHandlers.cs
QuickFiler/Controllers/EfcFormController.Helpers.cs
QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs
QuickFiler/Controllers/EfcFormController.cs
QuickFiler/Controllers/EfcHomeController.cs
QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs
QuickFiler/Controllers/EfcItemController.cs
QuickFiler/Controllers/QfcCollectionController.PopOut.cs
QuickFiler/Controllers/QfcCollectionController.cs
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
QuickFiler/Controllers/QfcItemController.cs
QuickFiler/Helper Classes/EfcViewerQueue.cs
QuickFiler/QuickFiler.csproj
QuickFiler/Viewers/WebView2BreadcrumbHost.cs
QuickFiler/Viewers/WebView2EnvironmentContract.cs
```

PATH-COUNT: 30. The list is exactly the plan's 31-entry write set minus `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` (listed: 0), and nothing else.

## Residual (recorded, not an acceptance clause)

`git status --porcelain --untracked-files=all` immediately after the commit listed only `.claude/agent-memory/atomic-executor/MEMORY.md` (modified) and `.claude/agent-memory/atomic-executor/project_pwsh_param_name_case_collision_flattens_log_array.md` (untracked), both inherited from the Phase 1 executor and outside the plan's add pathspecs; they were deliberately left uncommitted.

This artifact and the [P4-T12] check-off in the plan file are written after the commit they describe, so they remain uncommitted feature-folder changes at the end of Phase 4 (convention 9 treats docs and evidence as expected-dirty). No source path is dirty. The Phase 5 commit will sweep them, as this commit swept the Phase 3 residual `p3-t8-commit.md`.

Git printed four `LF will be replaced by CRLF` warnings for Markdown files; this is the repository's autocrlf normalisation notice and does not affect the committed content.
