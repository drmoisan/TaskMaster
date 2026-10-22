# [P3-T1] CSharpier format pass

Timestamp: 2026-09-07T07-49

Command: dotnet tool run csharpier format .
(preceded in the same block by `git add --intent-to-add -- '*.cs' '*.csproj'`, the before capture of
`git status --porcelain --untracked-files=all` and `git diff --stat <BASE-SHA>`, and followed by the same two
captures again)

EXIT_CODE: 0

ExpectedExitCode: 0

## Verbatim printed line

```
Formatted 1601 files in 8171ms.
```

`format` is a write-mode command: it rewrites tracked source and still exits 0 after rewriting, so the exit code
alone cannot distinguish a clean run from a repairing one. The distinguishing observations are the two derived
comparison lines below, captured before and after the run in the same shell.

## Derived comparison lines

PATH_SETS_IDENTICAL: False
DIFFSTAT_IDENTICAL: False

Both lines are recorded with their values, which is what this task's acceptance requires. `False` on both is the
truthful observation: the formatter rewrote files, so the porcelain path set went from empty to nine entries and
the anchored diffstat changed. It is not a failure signal. The gate on formatting cleanliness is [P3-T2], whose
read-only `check` exit code is the actual pass/fail.

## Before: `git status --porcelain --untracked-files=all`

Empty. Phase 2 was committed at `f50fb727`, so the worktree was clean at the start of this task and the
`--intent-to-add` companion had nothing new to stage.

## After: `git status --porcelain --untracked-files=all`

```
 M QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs
 M QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs
 M UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs
 M UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs
 M UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs
 M UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs
 M UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs
 M UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs
 M UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs
```

## Scope check on the touched set

Nine files appear in the post-run porcelain output. All nine are members of this plan's twenty-path Write Set. NO
file outside the Write Set appears, so no revert was required and the repository-wide pass did not widen the scope
boundary [P3-T11] asserts.

Of those nine, only FIVE carry a content change. The other four were rewritten byte-identically — CSharpier
updated their modification time without changing their bytes — and `git status` reported them modified from the
refreshed stat cache before any content comparison had been made. This was measured at staging time rather than
inferred: `git diff --cached --numstat` returns zero lines for each of the four, and each file's line endings are
uniformly CRLF (CRLF count equals total LF count), so no line-ending rewrite occurred either:

```
STAGED_NUMSTAT_LINES=0 CRLF=41  LF_TOTAL=41   QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs
STAGED_NUMSTAT_LINES=0 CRLF=217 LF_TOTAL=217  UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs
STAGED_NUMSTAT_LINES=0 CRLF=176 LF_TOTAL=176  UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs
STAGED_NUMSTAT_LINES=0 CRLF=63  LF_TOTAL=63   UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs
```

Those four files were therefore already CSharpier-clean when this pass began, which is consistent with [P3-T2]
finding no drift immediately afterwards.

## Anchored diffstat delta (before vs after), by file

Derived by comparing the two `git diff --stat <BASE-SHA>` captures. Only five files moved; the insertion totals
went from 3305 to 3347, a net +42.

| Path | before | after | delta |
|---|---|---|---|
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs` | 425 | 463 | +38 |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs` | 344 | 346 | +2 |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | 179 | 181 | +2 |
| `UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs` | 92 | 93 | +1 |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs` | 213 | 212 | -1 |
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` | 176 | 176 | 0, no content change |
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs` | 217 | 217 | 0, no content change |
| `UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs` | 63 | 63 | 0, no content change |
| `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs` | 41 | 41 | 0, no content change |

## The three D11-budgeted files were NOT rewritten

The [P2-T18] interim measurement left one line of headroom on two files, so a formatter rewrite of either would
have breached its budget. The formatter did not touch any of the three:

- `QuickFiler/Controllers/EfcFormController.cs` — diffstat unchanged at `3 +/-` before and after; absent from the
  rewritten set.
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` — diffstat unchanged at `41 +/-`; absent from the
  rewritten set.
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` — diffstat unchanged at `33 --`; absent from the
  rewritten set.

[P3-T10] is the gating re-measurement and confirms the resulting counts.

## Checked-file count

The formatter reports 1601 files, which is the [P0-T8] baseline of 1593 plus the eight new `.cs` files this plan
adds (three production, five test). [P3-T2] records the same delta from the read-only `check`.

Output Summary: The repository-wide CSharpier pass exited 0 and printed `Formatted 1601 files in 8171ms.`. Nine
files appear in the post-run porcelain output, every one of them inside this plan's Write Set, so nothing was
reverted and the scope boundary is unchanged; five of the nine carry a content change and the other four were
rewritten byte-identically, measured at staging time. Both required derived comparison lines are recorded:
`PATH_SETS_IDENTICAL: False` and `DIFFSTAT_IDENTICAL: False`, which is the truthful before/after observation for a
run that rewrote files. The three D11-budgeted files were not among the touched set, so the one line of headroom
[P2-T18] recorded on `EfcFormController.cs` and `FolderPredictor.cs` was not consumed by formatting.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact.
