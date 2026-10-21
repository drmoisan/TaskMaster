# Final QA Gate 7 — Post-Format File-Size and Untracked-File Audit (P5-T7)

Timestamp: 2026-08-28T16-11
Command: `(Get-Content <file>).Count` per file; `git ls-files --others --exclude-standard -- '*.cs'`
EXIT_CODE: 0

All counts were taken **after** the P5-T1 repo-wide CSharpier format pass, so they are the
formatter-final line counts.

## Every production and test file touched or created by this plan

| Lines | File | Status | <= 500 |
|---:|---|---|:--:|
| 498 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | modified (P2-T1) | yes |
| 78 | `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | modified (P2-T2) | yes |
| 449 | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | modified (P2-T3) | yes |
| 72 | `QuickFiler/Interfaces/IQfcFormViewer.cs` | modified (P3-T1) | yes |
| 296 | `QuickFiler/Viewers/QfcFormViewer.cs` | modified (P3-T2) | yes |
| 180 | `QuickFiler/Viewers/IItemViewer.cs` | modified (P3-T3) | yes |
| 85 | `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | modified (P3-T4) | yes |
| 113 | `QuickFiler/Interfaces/IQfcItemController.cs` | modified (P3-T5) | yes |
| 239 | `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | modified (P3-T6) | yes |
| 60 | `QuickFiler/Controllers/QfcFormController.Deactivate.cs` | **new** (P3-T8) | yes |
| 234 | `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | modified (P3-T9) | yes |
| 379 | `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs` | **new** (P1-T1) | yes |
| 248 | `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` | **new** (P1-T2) | yes |
| 70 | `QuickFiler.Test/Controllers/QfcItemController.CancelBreadcrumbSelectorTests.cs` | **new** (P1-T3) | yes |
| 467 | `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs` | modified (P3-T7) | yes |

**Maximum: 498 lines** (`BreadcrumbDropDownHost.cs`). Every file is at or under the repository's
500-line ceiling. The decision-D13 relocation remedy for `BreadcrumbDropDownHost.cs` (moving the
new private `FocusAnchorIfPermitted` member to the `BreadcrumbDropDownHost.Open.cs` partial) was
**not needed** and was not applied.

Two project files were also edited and are not subject to the 500-line ceiling for source files;
recorded here for completeness: `QuickFiler/QuickFiler.csproj` (one additive `<Compile Include>`)
and `QuickFiler.Test/QuickFiler.Test.csproj` (three additive `<Compile Include>` items).

## Untracked-file scan

Command: `git ls-files --others --exclude-standard -- '*.cs'`

Before the intent-to-add, the scan returned exactly three paths, all of them this plan's own new
test files:

```
QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs
QuickFiler.Test/Controllers/QfcItemController.CancelBreadcrumbSelectorTests.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs
```

No stray or unexpected `.cs` file exists anywhere in the tree.
`QuickFiler/Controllers/QfcFormController.Deactivate.cs` was already absent from this list because
P5-T6 had recorded its intent-to-add first.

`git add -N` was then run on the three files so every new `.cs` file is visible to diff-based
checks. The rescan afterwards returns **empty output**: all four new C# files are now tracked
intent-to-add, and no untracked `.cs` file remains.
