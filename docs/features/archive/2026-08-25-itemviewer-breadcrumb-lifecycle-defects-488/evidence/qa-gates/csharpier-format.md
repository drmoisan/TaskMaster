# Final QC Stage 1 — CSharpier Format ([P8-T1])

Timestamp: 2026-08-28T06-20

Command (from the worktree root, with the **seven owned file paths supplied explicitly** as arguments,
not a repository-wide dot argument):

```
dotnet tool run csharpier format QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs QuickFiler/Viewers/BreadcrumbDropDownHost.cs QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs
```

EXIT_CODE: 0

Tool output: `Formatted 7 files in 3104ms.`

## Why the scope is locked to seven paths

Decision D-6: a repository-wide mutating `csharpier format .` would rewrite files that were already
unformatted at the baseline and break the changed-file-set criterion `[P7-T10]` flips. The
repository-wide read-only `csharpier check .` remains the gate, and is run by `[P8-T2]`.

## Per-file SHA-256 comparison

The tool's own `Formatted 7 files` line is a count of files **processed**, not of files **changed**, so
it is not a substitute for this comparison. Each file's content hash was taken immediately before and
immediately after the command.

| # | File | SHA-256 before | SHA-256 after | Content changed? |
| --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | `6de6f17f…eceab75` | `04c2ae3d…98cf28` | **YES** |
| 2 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | `e50842ce…049a8d` | `e50842ce…049a8d` | no |
| 3 | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | `676bcb3a…4c45db` | `676bcb3a…4c45db` | no |
| 4 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `990f4f6a…01889e` | `990f4f6a…01889e` | no |
| 5 | `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | `c0ae85c1…0f9366` | `c0ae85c1…0f9366` | no |
| 6 | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | `ac78b80b…967799` | `ac78b80b…967799` | no |
| 7 | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | `fc5098d7…7e09ec` | `fc5098d7…7e09ec` | no |

**One of the seven changed: `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`.** The remaining six were
already in CSharpier's canonical form and are byte-identical across the command.

## What changed in the one file

A single reflow, in `InitializeBreadcrumbPipeline`, of the lambda argument `[P6-T7]` introduced:

```csharp
            BreadcrumbItemViewerLifecycleCoordinator lifecycle = EnsureBreadcrumbLifecycle(() =>
                operations
            );
```

CSharpier moved the `() =>` onto the invocation line. The change is purely cosmetic — no statement,
identifier, or argument order changed — and the file's line count is **425**, unchanged by the format
pass.

Three of the other six were already verified CSharpier-clean earlier in execution: the two coordinator
files by an interim read-only check recorded in `[P2-T7]`, and the new test file by a scoped format run
during the capacity-rule-3 compaction recorded in `[P6-T10]`, which is why its 480-line figure was
already a post-format figure.

## Loop position

This is **stage 1** of the four-stage final QC loop. Because this stage changed a file, the stages that
follow it run against the post-format content: `[P8-T2]` re-verifies formatting repository-wide,
`[P8-T3]` and `[P8-T4]` are the analyzer and nullable gates, and `[P8-T5]` is the test gate. No stage
after this one may change a file; `[P8-T11]` confirms that by re-taking these seven hashes after the
test run and comparing.

Output Summary: EXIT_CODE 0, `Formatted 7 files in 3104ms`. Per-file SHA-256 comparison shows **exactly
one** of the seven owned files changed — `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`, by a single
cosmetic lambda reflow with no line-count change — and the other **six** are byte-identical before and
after.
