# [P4-T1] — QA Step 1 of 4: Formatting

Timestamp: 2026-08-27T20-49

Command:
```
dotnet tool run csharpier format QuickFiler/Viewers/WebView2BreadcrumbHost.cs QuickFiler/Viewers/WebView2CoreInitializer.cs QuickFiler/Viewers/IWebViewCoreInitializer.cs QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs
dotnet tool run csharpier check .
```
(both run from the workspace root, through the manifest-pinned CSharpier 1.2.6)

EXIT_CODE: 0

## Deliberate deviation from `csharpier format .`

This task applies the formatter **file-scoped** rather than repository-wide, and verifies
**repository-wide** with the read-only `check .`. The ground is Decisions Record item 9: a
repository-wide apply would rewrite any pre-existing formatting deviation in a file on this feature's
forbidden list — `QuickFiler/Viewers/WebView2Messenger.cs`,
`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`, `QuickFiler/Controllers/EfcFormController.cs` and the
rest — producing a scope violation manufactured by the toolchain rather than by the change. The
gate CI actually enforces is the read-only `check .`, which is run here at full repository scope.
`[P5-T38]` records this reconciliation at check-off rather than checking the toolchain criterion off
silently.

## Scoped format output

First pass:

```
Formatted 6 files in 2718ms.
```

The first pass rewrote four of the six files. `git diff --stat` after it:

```
 QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs        |  4 +++-
 QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs     | 14 ++++++++------
 QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs             |  8 ++++----
 QuickFiler/Viewers/WebView2BreadcrumbHost.cs                       |  7 +++----
```

Per the phase's loop discipline, a rewrite restarts the phase from this task, so the formatter was
run again:

```
Formatted 6 files in 2523ms.
```

Second pass rewrote **nothing**: the `git diff --stat` output captured immediately before and
immediately after the second pass is byte-identical. "Formatted 6 files" is CSharpier's count of
files **processed**, not files changed, so the diffstat comparison rather than that line is the
evidence of idempotence. The four toolchain steps that follow therefore run over a formatter-stable
tree.

## Repository-wide verification

```
Checked 1542 files in 5241ms.
```

Exit code 0. **No file was reported as unformatted.**

## Two-part acceptance

1. The `check .` output names **none** of the six files this feature touches. It names no file at
   all.
2. It names no file that was not already recorded in the Phase 0 baseline artifact
   `baseline-1-csharpier-check.2026-08-27T19-59.md`. That baseline recorded an **empty** reported-file
   list, and this run also reports an empty list, so the comparison holds trivially and in the
   strongest direction: the feature introduced no formatting debt and no pre-existing debt was
   silently rewritten.

The file count rose from 1540 at baseline to 1542 here, accounted for exactly by the two new test
files this feature adds.

## Phase restart at 2026-08-27T20-54 — this step re-run and re-verified

`[P4-T4]` failed on its first attempt with one unrelated flaky test, so the phase was restarted from
this task per the loop discipline. This step was re-run and is recorded here rather than in a second
artifact, so there remains exactly one artifact per QC step.

```
Formatted 6 files in 2445ms.
```

`git diff --stat` captured immediately before and immediately after this run is byte-identical:
**no file was rewritten**, so the tree the restarted pass runs over is the same tree the first pass
verified.

```
Checked 1542 files in 6264ms.
CHECK_EXIT=0
```

Repository-wide `check .` again reports no file. Both parts of the acceptance continue to hold: none
of the six touched files is named, and no file is named that was not already in the Phase 0 baseline's
(empty) reported-file list.
