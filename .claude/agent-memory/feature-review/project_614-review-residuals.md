---
name: 614-review-residuals
description: "#614 EFC store-root leak: cycle-2 exit re-audit is NO-GO/1 blocking (RC-1, the CR-2 remedy widened the filing guard without normalizing, so the archive-root-exact suggestion row now crashes post-Hide); CR-1 closed; residuals unchanged"
metadata:
  type: project
---

Branch `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`, base `main`
@ `c279d40b`.

## Cycle-1 exit re-audit (head `b45e2a2d`, artifacts `*.2026-08-26T22-12.md`) — **NO-GO, 1 blocking**

**CR-1 closed.** `IsValidFilingSelection` lost `Length >= 3`; the rule moved to the new
`IsValidCreationSelection`, which is now the sole delegate of the `IsValidSelection` property.
Verified at all three call sites.

**CR-2 NOT closed — superseded by blocking finding RC-1.** The remedy widened
`IsValidFilingSelection(selection, archiveRoot)` to accept any rooted value that passes
`TryMakeArchiveRelative`, but normalized nothing. `SelectedFolder` is carried verbatim through
`ExecuteMovesCoreAsync` -> `EfcDataModel.cs:286` (`DestinationOlStem = folderpath`) ->
`EmailFilerConfig.ResolvePaths` -> `RequireArchiveRelativeStem`, which **throws** on any rooted
value. Not caught; `ButtonOK_Click` is `async void` and **rethrows** -> unhandled, and it happens
*after* `_formViewer.Hide()`. Before the cycle the same value produced a benign "Please select a
valid folder." dialog.

**Reachability (traced, not assumed):** `BreadcrumbRowBuilder` sets `FilingTarget = presentedText`
in all three branches. `FolderPredictor.ProjectSuggestionPath` (`:845-858`) strips the archive
prefix ONLY when `folderPath.Length > archivePrefix.Length`, so a suggestion whose folder **is** the
archive root is returned as a full rooted path verbatim -> `SelectRow` admits -> OK guard admits ->
boundary throws. Search results (`GetOlSubpath`) and recents (`DestinationOlStem`) are stems, so the
exact-root suggestion is the reachable case.

**Three guards, three answers for `\<root>`:** `SelectHierarchyPath` rejects (`stem.Length == 0`),
`SelectRow` admits verbatim, `IsValidFilingSelection` admits (pinned by
`IsValidFilingSelection_ArchiveRootExactTarget_IsAccepted`, whose own comment calls it a "CR-2
recorded consequence"), `RequireArchiveRelativeStem` throws.

**Fix direction (in `remediation-inputs.2026-08-26T22-12.md`):** normalize at the producer —
`SelectRow` should `CommitSelection(row, stem)` and reject the empty-stem case; then restore the
filing guard's rootedness rejection and drop the `archiveRoot` parameter and
`ResolveArchiveRootOrEmpty` with it; update `BreadcrumbBridgeRouterIssue439Tests.cs:165`; delete the
two tests pinning the current behaviour; add a **composition** test (a value the predicate accepts
must survive `RequireArchiveRelativeStem`) — its absence is why 100%-covered unit tests missed this.

## Other cycle-2 findings

- **RC-2.** spec AC16 ("share one predicate"; "OK rejects a non-relative selection") is now false on both clauses and was never amended; still checked `[x]`. AC16 evaluated **PARTIAL**.
- **RC-3.** `ResolveArchiveRootOrEmpty` guards 1 of 9 `ArchiveRootPath` reads reachable from `EfcFormController`; `:777`/`:787` run after `Hide()`. It is only effective at all because `BindBreadcrumbRowsAsync` swallows the earlier failure, leaving no rows to select.
- **RC-4.** `EmailFilerConfig.GetStem`'s new ternary (`:252`) has an untested out-of-ancestor arm — file branch coverage 70.0% -> 60.0%, the only branch decrease attributable to added code on the whole branch.
- **AC26 downgraded PASS -> PARTIAL:** the cycle added new OK-path behaviour with no new manual-validation record; the existing artifact predates it.

## Unchanged residuals (re-verified at `b45e2a2d`)

`FolderConverter` alternative-folder-name cluster is unreachable dead code; `ArchiveRootPath` /
`LoadFolders` fail-fast throws (intended, AC13/AC14); inert
`AppFileSystemFolderPaths(Func<string,string>)` seam; `SortEmail.ResolvePaths` unmigrated;
`FolderConverter.cs:265` `nameof(fsPath)` names a local, not a parameter.

## Measurements (this head)

- Repo-wide line **84.8790%** (54000/63620) -> below the 85% hook floor -> policy audit MUST carry an explicit FAIL row (non-blocking; merge base was 84.7797%, so +0.0993). Branch **78.8523%** (12752/16172), clears 75%.
- New files: `ArchiveStemContract` 100/100, `EfcSelectionGuard` 100/100 (31 instrumented lines, up from 9), `ArchiveRootPathGuard` 100 line / 90 branch.
- Modified files below the 85% line floor: `EfcDataModel` 52.7%, `EfcFormController` 11.3%, `AppOlObjects` 32.7%, `AppFileSystemFolderPaths` 69.0% — all sub-floor at the merge base too, all improved except `AppOlObjects` (covered count unchanged at 71; 4 instrumented lines added).
- Toolchain re-run green: csharpier 1530 files, analyzer rebuild 0, nullable rebuild 0, 6111/6111 tests across the 3 changed assemblies (was 6093 at `02092504`; +18 guard tests).
- File sizes: `EfcFormController.cs` 1079 (merge base 1084), `BreadcrumbBridgeRouter.cs` 596, `BreadcrumbBridgeRouterIssue439Tests.cs` 694 — all pre-existing, none grown.

## Process notes

- **PR context artifacts were STALE, not absent** this time (head ref `02092504`, four commits behind). Always compare `git rev-parse HEAD` against the summary's `Head ref`. Regenerated by hand from `git diff --numstat` in the `- <path> (+N/-N)` shape; the prior summary also miscounted `.cs` files as 22 (actual 21).
- The `<report>` JaCoCo-summary shape of `artifacts/csharp/coverage.xml` has package-level LINE counters only, so **per-file** figures must come from the gitignored `coverage/coverage.cobertura.filtered.*.xml` pair (`p0-t9` = merge base, `p5-t4` = head). Dedup `<class>` nodes by `filename`, max hits per line, max condition-coverage numerator per line — summing them double-counts.
- Long heredocs fail with `ENAMETOOLONG` / unmatched-quote errors through the Bash tool; use the Write tool for multi-page artifacts and avoid double backslashes in the content.
