# Scope fences, remediation cycle 1 (P4-T13, #1002)

Timestamp: 2026-10-22T13-33
Command: git diff --name-only <merge base of origin/main and HEAD>; git status --porcelain; git diff -U0 <merge base> -- '*.cs' (added ExcludeFromCodeCoverage lines); git diff --stat <merge base> -- coverage.config
EXIT_CODE: 0
Output Summary:
- Cycle-1 working changes (git status --porcelain, uncommitted): modified QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs, QuickFiler/Controllers/EfcFormController.Actions.cs, QuickFiler/Controllers/EfcFormController.Breadcrumb.cs, UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererIssue1002Tests.cs, UtilitiesCS.Test/UtilitiesCS.Test.csproj, UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs, the remediation plan; untracked UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessagesIssue1002Tests.cs and this cycle's evidence files. None is on the spec's excluded list; none is under .claude/rules/.
- The merge-base `git diff --name-only` also lists files committed earlier on the branch by the original change (including the router, renderer and codec files and .claude/agent-memory notes); those are outside this cycle's edits and were not touched by it.
- Added ExcludeFromCodeCoverage lines in the merge-base diff over *.cs: 5, located on ActionOkAsync, ActionCancelAsync, CreateFolderAsync, RefreshSuggestionsAsync (P1-T10 recorded YES) and ConfigureBreadcrumbControl. No other member.
- `git diff --stat` of coverage.config against the merge base: empty (no change).
- No .trx, .coverage or Cobertura document appears in git status.
