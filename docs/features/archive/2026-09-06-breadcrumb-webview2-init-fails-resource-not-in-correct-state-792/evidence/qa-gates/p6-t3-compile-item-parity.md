# [P6-T3] AC-U8 compile-item parity

- Issue: #792
- Timestamp: 2026-09-17T20-54
- Command: CMD-BASE (binds `$BaseSha` from `p0-t7-git-base.md`), then `git diff --name-status $BaseSha HEAD -- '*.cs'`; `git diff $BaseSha HEAD -- QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj`; `git diff --numstat $BaseSha HEAD -- QuickFiler.Test/Controllers/EfcFormControllerTests.cs`; `git status --porcelain --untracked-files=all -- '*.cs' '*.csproj'`; all run from `coverage/plan792-helper.ps1` with the item worktree as the working directory (the helper's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` is the worktree proof and passed; HEAD `9ac987a969d1f4786d296fee25817c8e5dde9233`)
- EXIT_CODE: 0
- Output Summary: name-status lists 28 rows: `A-ROWS: 17`, `D-ROWS: 0`, `M-ROWS: 11`, every `A` and `M` path a write-set member; the two csproj diffs carry `CSPROJ-ADDED-LINES: 17`, all 17 bare self-closing `<Compile Include="...cs" />` elements, `CSPROJ-REMOVED-LINES: 0`; `INCLUDE-SET-EQUALS-A-SET: True` (17 = 17, no path on either side only); `EFCFORMCONTROLLERTESTS-DIFF: none`; `UNTRACKED-SOURCE: none`.

BASE-SHA: e7cbb57229c63a228e7fe0bcbcdbfbc06db8bcd3 (read by CMD-BASE from `$FEATURE/evidence/baseline/p0-t7-git-base.md`, itself the `git merge-base HEAD origin/main` after `git fetch origin`; bare local `main` was not used)

## `git diff --name-status $BaseSha HEAD -- '*.cs'` (verbatim, 28 rows)

```
A	QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue792Tests.cs
A	QuickFiler.Test/Controllers/BreadcrumbOutboundQueueIssue792Tests.cs
A	QuickFiler.Test/Controllers/EfcDataModelIssue792CarryTests.cs
A	QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs
A	QuickFiler.Test/Controllers/QfcCollectionControllerIssue792PopOutTests.cs
A	QuickFiler.Test/Helper Classes/EfcViewerQueueIssue792Tests.cs
A	QuickFiler.Test/Viewers/WebView2BreadcrumbHostIssue792Tests.cs
A	QuickFiler.Test/Viewers/WebView2EnvironmentContractTests.cs
M	QuickFiler/Controllers/BreadcrumbBridgeRouter.cs
M	QuickFiler/Controllers/BreadcrumbOutboundQueue.cs
A	QuickFiler/Controllers/EfcDataModel.Carry.cs
M	QuickFiler/Controllers/EfcDataModel.cs
A	QuickFiler/Controllers/EfcFormController.Actions.cs
A	QuickFiler/Controllers/EfcFormController.Breadcrumb.cs
A	QuickFiler/Controllers/EfcFormController.EventHandlers.cs
A	QuickFiler/Controllers/EfcFormController.Helpers.cs
A	QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs
M	QuickFiler/Controllers/EfcFormController.cs
M	QuickFiler/Controllers/EfcHomeController.cs
A	QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs
M	QuickFiler/Controllers/EfcItemController.cs
A	QuickFiler/Controllers/QfcCollectionController.PopOut.cs
M	QuickFiler/Controllers/QfcCollectionController.cs
M	QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
M	QuickFiler/Controllers/QfcItemController.cs
M	QuickFiler/Helper Classes/EfcViewerQueue.cs
M	QuickFiler/Viewers/WebView2BreadcrumbHost.cs
A	QuickFiler/Viewers/WebView2EnvironmentContract.cs
```

A-ROWS: 17 (the nine new production and eight new test files named in the Write set)
D-ROWS: 0
M-ROWS: 11
OTHER-STATUS-ROWS: 0
M-ROWS-OUTSIDE-WRITE-SET: 0
A-ROWS-OUTSIDE-WRITE-SET: 0

The eleven `M` rows are `BreadcrumbBridgeRouter.cs`, `BreadcrumbOutboundQueue.cs`, `EfcDataModel.cs`, `EfcFormController.cs`, `EfcHomeController.cs`, `EfcItemController.cs`, `QfcCollectionController.cs`, `QfcItemController.ViewerSetup.cs`, `QfcItemController.cs`, `EfcViewerQueue.cs`, `WebView2BreadcrumbHost.cs`; each is a write-set member. `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` (a write-set member) has no row, consistent with the deliberate zero-edit.

## csproj diffs (`git diff $BaseSha HEAD -- QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj`)

CSPROJ-ADDED-LINES: 17
CSPROJ-REMOVED-LINES: 0
CSPROJ-ADDED-BARE-COMPILE-LINES: 17 (each matches `^\+\s*<Compile Include="[^"]+\.cs" />$`; no metadata, no `DependentUpon`)
CSPROJ-ADDED-NON-BARE-LINES: 0

Added lines by project (verbatim `+` lines, hunk context omitted):

`QuickFiler.Test/QuickFiler.Test.csproj` (8):

```
+    <Compile Include="Controllers\BreadcrumbBridgeRouterIssue792Tests.cs" />
+    <Compile Include="Controllers\BreadcrumbOutboundQueueIssue792Tests.cs" />
+    <Compile Include="Controllers\EfcDataModelIssue792CarryTests.cs" />
+    <Compile Include="Controllers\EfcFormControllerIssue792Tests.cs" />
+    <Compile Include="Controllers\QfcCollectionControllerIssue792PopOutTests.cs" />
+    <Compile Include="Viewers\WebView2BreadcrumbHostIssue792Tests.cs" />
+    <Compile Include="Viewers\WebView2EnvironmentContractTests.cs" />
+    <Compile Include="Helper Classes\EfcViewerQueueIssue792Tests.cs" />
```

`QuickFiler/QuickFiler.csproj` (9):

```
+    <Compile Include="Controllers\EfcDataModel.Carry.cs" />
+    <Compile Include="Controllers\EfcFormController.Actions.cs" />
+    <Compile Include="Controllers\EfcFormController.Breadcrumb.cs" />
+    <Compile Include="Controllers\EfcFormController.EventHandlers.cs" />
+    <Compile Include="Controllers\EfcFormController.Helpers.cs" />
+    <Compile Include="Controllers\EfcFormController.SetupAndProperties.cs" />
+    <Compile Include="Controllers\EfcItemController.WebViewEnvironment.cs" />
+    <Compile Include="Controllers\QfcCollectionController.PopOut.cs" />
+    <Compile Include="Viewers\WebView2EnvironmentContract.cs" />
```

## Include set versus `A` set

Each `Include` value was normalised with `.Replace('\', '/')` and prefixed with its project directory (`QuickFiler/` or `QuickFiler.Test/`, taken from the `+++ b/` header of the hunk it appears in).

INCLUDE-SET-COUNT: 17
A-SET-COUNT: 17
ONLY-IN-INCLUDE-SET: 0
ONLY-IN-A-SET: 0
INCLUDE-SET-EQUALS-A-SET: True

## `EfcFormControllerTests.cs` zero-edit

EFCFORMCONTROLLERTESTS-DIFF: none (`git diff --numstat $BaseSha HEAD -- QuickFiler.Test/Controllers/EfcFormControllerTests.cs` printed nothing)

Positive control: the same numstat form against the edited write-set member `QuickFiler/Controllers/EfcHomeController.cs` printed `20	3	QuickFiler/Controllers/EfcHomeController.cs` (`CONTROL-NUMSTAT-EDITED-FILE`), so an empty numstat is a true zero-edit rather than a mis-scoped pathspec.

## Untracked-source companion

UNTRACKED-SOURCE: none (`git status --porcelain --untracked-files=all -- '*.cs' '*.csproj'` printed nothing)

This proves that every path the anchored name-status diff must enumerate is committed and therefore visible to it; an untracked new file would be invisible to `git diff $BaseSha HEAD` and would appear here instead.

Positive control: the unscoped `git status --porcelain --untracked-files=all` at the same moment listed 6 entries (`CONTROL-PORCELAIN-ALL-COUNT: 6`): the plan file and `MEMORY.md` as modified, and `p6-t1-ac-u6-structural-pass.md`, `p6-t2-line-counts-advisory.md`, `p5-t9-commit.md` and one `.claude/agent-memory/atomic-executor/` note as untracked — all docs, evidence or agent-memory paths (convention 9 expected-dirty). The scoped form's emptiness is therefore a property of the source pathspecs, not of a status command that reports nothing.
