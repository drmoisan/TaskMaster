# [P3-T8] Phase 3 commit

- Issue: #792
- Timestamp: 2026-09-17T19-52
- Command: `git add -- QuickFiler.Test docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` then `git commit -m "test(792): seam-dependent regression tests recorded failing before the fix"` (run with `git -C <repo-root>` against the item worktree on branch `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792`; the session's attribution trailer lines were supplied through a second `-m`, so the subject line is the plan's text verbatim)
- EXIT_CODE: 0
- Output Summary: `[bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792 494f7138] test(792): seam-dependent regression tests recorded failing before the fix`; `11 files changed, 1204 insertions(+), 8 deletions(-)`; 5 test files created, 2 test-project files modified, 4 feature-folder paths (3 evidence files created, the plan file modified with the [P3-T1] through [P3-T7] check-offs).

## Acceptance observations

`git show --name-only --format= HEAD` listed 11 paths, all under `QuickFiler.Test/` or the feature folder:

- Five new test files: `QuickFiler.Test/Viewers/WebView2EnvironmentContractTests.cs`, `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue792Tests.cs`, `BreadcrumbOutboundQueueIssue792Tests.cs`, `EfcDataModelIssue792CarryTests.cs`, `QfcCollectionControllerIssue792PopOutTests.cs`.
- Two modified test-project files: `QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs` (five tests and the scripted seam appended by [P3-T4]), `QuickFiler.Test/QuickFiler.Test.csproj` (five bare Compile items).
- Four paths under the feature folder: `plan.2026-09-17T07-30.md`, `evidence/regression-testing/p3-t7-fail-before.md`, and the two Phase 2 residuals left uncommitted after [P2-T15] by construction (`evidence/other/p2-t14-independent-confirmation.md`, `evidence/qa-gates/p2-t15-commit.md`).
- Nothing else.

`git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'` printed nothing.

`git status --porcelain --untracked-files=all` immediately after the commit listed only `.claude/agent-memory/atomic-executor/MEMORY.md` (modified) and `.claude/agent-memory/atomic-executor/project_pwsh_param_name_case_collision_flattens_log_array.md` (untracked), both inherited from the Phase 1 executor and outside the plan's add pathspecs.

COMMIT-SHA-OBSERVED: 494f71381790c1f2f57531c0269eabe79890ad63

PARENT-SHA: 11f5aa59816a73a0ee8bb5f6e9545b6b37962563 (the [P2-T15] commit)

## Residual (recorded, not an acceptance clause)

This artifact and the [P3-T8] check-off in the plan file are written after the commit they describe, so they remain uncommitted feature-folder changes at the end of Phase 3 (convention 9 treats docs and evidence as expected-dirty). No source path is dirty. The Phase 4 commit will sweep them, as this commit swept the Phase 2 residuals.

Git printed four `LF will be replaced by CRLF` warnings for Markdown files; this is the repository's autocrlf normalisation notice and does not affect the committed content.

No production `.cs` file, no `QuickFiler/QuickFiler.csproj` change, and no `.runsettings` change is in this commit: Phase 3 touched the test project only.
