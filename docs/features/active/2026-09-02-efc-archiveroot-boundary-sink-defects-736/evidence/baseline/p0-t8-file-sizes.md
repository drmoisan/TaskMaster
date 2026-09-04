# P0-T8 — Pre-change file sizes and the `OlAncestor` occurrence facts

Timestamp: 2026-09-03T23-36

Command: `Get-Content -LiteralPath <path> | Measure` line counting and `Select-String -SimpleMatch`
over `QuickFiler\Controllers\EfcDataModel.cs`, run from the worktree root.

EXIT_CODE: 0

## Pre-change line counts of the eight already-existing Write Set files

| Write Set path | Pre-change line count |
|---|---|
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 494 |
| `TaskMaster/TaskMaster.csproj` | 585 |
| `QuickFiler/Controllers/EfcFormController.cs` | 1216 |
| `QuickFiler/Controllers/EfcDataModel.cs` | 485 |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | 485 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 522 |
| `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` | 389 |
| `TaskMaster.Test/TaskMaster.Test.csproj` | 396 |

The three files named by this task's acceptance carry the required values:
`QuickFiler/Controllers/EfcFormController.cs` = 1216, `TaskMaster/AppGlobals/AppOlObjects.cs` = 494,
`QuickFiler.Test/Controllers/EfcFormControllerTests.cs` = 485, and
`QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` = 389.

The three remaining Write Set paths — `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs`,
`QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs`, and
`TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootComGuardTests.cs` — do not yet exist and are
created by this plan, so they carry no pre-change count.

## The `OlAncestor = olAncestor,` occurrences in `QuickFiler/Controllers/EfcDataModel.cs`

Exactly **three** lines match the quoted source literal. Their pre-change line numbers and enclosing
members, in file order:

| # | Line | Enclosing member | Member declaration line |
|---|---|---|---|
| 1 | 339 | five-parameter `MoveToFolderAsync` overload | 303 |
| 2 | 366 | `OpenOlFolderAsync` | 349 |
| 3 | 390 | `OpenFsFolderAsync` | 374 |

**Line 339 — the first in file order, inside the five-parameter `MoveToFolderAsync` overload — is the
one occurrence that finding 6's remedy must keep covered and that P6-T10 measures.** The other two
sit in entry points outside this item's findings and are not touched.

`MoveToFolderAsync` is an overload pair. The five-parameter `string`-first overload declared at
EfcDataModel.cs:303 contains the occurrence at 339. The six-parameter `MAPIFolder`-first overload
declared at EfcDataModel.cs:398 contains none and is not edited by this plan.

## The `new EmailFiler(` occurrences in the same file

Exactly **three** lines match the fixed string, at line numbers **343**, **370**, and **394**. Each
sits immediately after the corresponding `EmailFilerConfig` object initializer above, and each reads
`var sorter = new EmailFiler(config);`.

## D7 ceilings, restated

- `QuickFiler/Controllers/EfcFormController.cs` — budgeted ceiling of **1330** lines after the final
  formatting pass. A shrink assertion on this file would be unsatisfiable for a correct
  implementation; the file is 1216 lines at the merge base, is `internal class` rather than
  `partial`, and splitting it is an explicit non-goal of spec.md.
- `TaskMaster/AppGlobals/AppOlObjects.cs` — the repository's **500**-line ceiling. The D3 edit stays
  under it because the getter body shrinks.
- `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` — **exactly 485** lines. It gains only the
  `partial` keyword.
- Every other file in the Write Set — the plain **500**-line ceiling.

Output Summary: eight pre-existing Write Set files measured; the four acceptance-pinned counts are
1216, 494, 485, and 389 as required. Exactly three `OlAncestor = olAncestor,` lines exist in
`QuickFiler/Controllers/EfcDataModel.cs`, at 339, 366, and 390, enclosed in file order by the
five-parameter `MoveToFolderAsync` overload (declared at 303), `OpenOlFolderAsync` (349), and
`OpenFsFolderAsync` (374); line 339 is the one finding 6 must keep covered. Exactly three
`new EmailFiler(` lines exist, at 343, 370, and 394. D7's ceilings are restated above.
