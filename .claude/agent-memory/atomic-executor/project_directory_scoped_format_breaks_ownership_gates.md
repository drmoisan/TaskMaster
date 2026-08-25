---
name: directory-scoped-format-breaks-ownership-gates
description: A final-QC csharpier pass scoped to whole DIRECTORIES can rewrite must-not-write files, contradicting the same plan's unmodified-file assertions; scope the mutating pass to explicit file paths.
metadata:
  type: project
---

A plan that correctly refuses a repo-wide `dotnet tool run csharpier format .` (because concurrent
sibling epic children share the branch) can still defeat itself by scoping the MUTATING pass to
directories rather than to explicit file paths. Observed on plan
`docs/features/active/breadcrumb-router-navigation-defects-498/plan.2026-08-24T09-39.md` (P7-T1):
the scoped list named 6 files plus `QuickFiler.Test/Controllers`, `QuickFiler.Test/Viewers`, and
`UtilitiesCS.Test/OutlookObjects/Folder` — 181 `.cs` files, of which the plan writes about 7.

**Why:** those directories contain files the same plan lists as MUST NOT WRITE and asserts are
unmodified via `git status --porcelain -- <path>` producing no output
(`QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`,
`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSelectorTests.cs`,
`BreadcrumbStateModelSequenceTests.cs`). If csharpier rewrites any of them the plan fails its own
ownership acceptance criteria. Whether it actually rewrites them depends on the pre-existing
baseline formatting state, which the plan captures only later — so the gate is a coin flip, not a
guarantee.

**How to apply:** at preflight, whenever a plan scopes the mutating format pass, check that every
entry is a FILE path, not a directory, and cross-check each entry against the plan's must-not-write
list and against every task whose acceptance is "`git status --porcelain -- <path>` produces no
output". Flag a bare directory as a Blocking revision. New files created mid-plan must be added to
the list by path, which the plan should say explicitly. See
[[project_csharpier_pipefiles_nonenforcing_gate]] and
[[project_count_idiom_pitfalls_csharpier_and_measureobject]] for the companion traps in verifying
that the format step actually did something.
