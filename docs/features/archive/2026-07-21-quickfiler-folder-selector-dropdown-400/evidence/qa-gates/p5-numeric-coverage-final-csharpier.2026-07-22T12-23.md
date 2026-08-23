# P5 numeric-coverage final CSharpier (replacement pass) — BLOCKED

Timestamp: `2026-07-22T12-23`

Command: `csharpier.exe check <10 files>` (authoritative verification); `csharpier.exe format <10 files>` (writing pass, then reverted); `(Get-Content).Count` per file; `sha256sum`. CSharpier version `1.3.0`.

EXIT_CODE: `1`

Output Summary: FAIL / BLOCKED. `csharpier check` on the committed state returns exit 1: 8 of the 10 named files are reported "Was not formatted". Applying `csharpier format` in place produces a stable, idempotent result, but two of the required test files then exceed the hard 500-line file-size limit: `BreadcrumbDropDownOpenCoordinatorTests.cs` grows 395 -> 514 and `BreadcrumbPopupBoundaryCoverageTests.cs` grows 479 -> 562. P5-T154 authorizes only formatting the ten named files (no file split, no new file or include), and the plan's fixed rules require every new/modified test file to remain at most 480/500 lines. These two requirements are mutually unsatisfiable for those two files without a file split, which is a new outcome outside this task's scope. P5-T154 is left unchecked; the ten files have been reverted to committed state.

## Root cause

Prior P5 CSharpier gates in this plan invoked `csharpier pipe-files` with an absolute-path stdin list. `pipe-files` writes formatted output to stdout and never modifies the file on disk; the evidence then re-hashed the (unmodified) file and reported "stable". That verification is trivially satisfied and does not detect genuine formatting deltas. The committed P5 test and production files were therefore never actually CSharpier-formatted.

## Authoritative verification (committed state, non-writing)

`csharpier check` (exit code 1, "Checked 10 files") reported the following as "Was not formatted":

- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`
- `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`
- `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs`
- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs`
- `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs`
- `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs`
- `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs`

The two already-clean files were `BreadcrumbDropDownLifecycleCoverageTests.cs` and `BreadcrumbMessengerHubCoverageTests.cs`.

## Post-format line counts (writing pass, stable/idempotent, then reverted)

| File | Committed lines | CSharpier-formatted lines | Limit | Verdict |
|---|---:|---:|---|---|
| `ItemViewer.Breadcrumb.cs` | 396 | 398 | <=460 | OK |
| `BreadcrumbDropDownOpenCoordinator.cs` | 277 | 272 | <=480 | OK |
| `BreadcrumbWebViewSurfaceFactory.cs` | 226 | 225 | <=500 | OK |
| `BreadcrumbDropDownHost.cs` | 472 | 472 | <=480 | OK |
| `ItemViewerBreadcrumbDropDownContractTests.cs` | 132 | 132 | <=480 | OK |
| `BreadcrumbDropDownOpenCoordinatorTests.cs` | 395 | 514 | <=480/500 | VIOLATION |
| `BreadcrumbDropDownIntegrationTests.cs` | 500 | 500 | exactly 500 | OK |
| `BreadcrumbPopupBoundaryCoverageTests.cs` | 479 | 562 | <=480/500 | VIOLATION |
| `BreadcrumbDropDownLifecycleCoverageTests.cs` | 468 | 468 | <=480 | OK |
| `BreadcrumbMessengerHubCoverageTests.cs` | 478 | 478 | <=480 | OK |

The re-run of `csharpier format` on the two oversized files kept them at 514 and 562 (stable/idempotent), confirming these are the genuine formatted sizes, not a transient state. No `.csharpierrc` exists in the repository; only `.editorconfig` is present, so no custom print width applies (CSharpier default width 100).

## Why this blocks and what a revision must authorize

P5-T154 scope: run CSharpier on exactly the ten named files and reach a stable formatted state. It does not authorize creating a new file, adding a `Compile` include, or splitting a class. The plan's fixed rules require every new/modified test file to remain within the 480/500-line limits. Genuine CSharpier formatting of `BreadcrumbDropDownOpenCoordinatorTests.cs` (514) and `BreadcrumbPopupBoundaryCoverageTests.cs` (562) violates that limit. The only compliant resolution is to split each of those two test classes into a partial-class pair (or otherwise reduce them) so that each resulting CSharpier-clean file is at most 480 lines, with one adjacent `QuickFiler.Test.csproj` include per new partial file. That is a new independent outcome requiring an atomic-planner in-place plan revision before editing.

## Working-tree state

All ten files were reverted to committed state (`git checkout --`). The only working-tree changes remaining are the P5-T153 evidence artifact and this artifact, plus the P5-T153 checkbox in the remediation plan. No production or test file content was left modified. No commit was created.
