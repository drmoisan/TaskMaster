---
name: quickfiler-test-sta-and-ivt
description: QuickFiler grants InternalsVisibleTo to QuickFiler.Test, and QuickFiler.Test already has manual STA-thread infra (no MSTest.STAExtensions package needed) — corrects two earlier assumptions
metadata:
  type: project
---

Two facts verified 2026-08-07 during F11 (#454) research that correct earlier notes:

1. **`QuickFiler` DOES grant internals to `QuickFiler.Test`** — `QuickFiler/Properties/AssemblyInfo.cs:5`
   has `[assembly: InternalsVisibleTo("QuickFiler.Test")]`. So `internal` seams on QuickFiler types are
   directly testable. The separate epic-#136 constraint is unchanged and only concerns **UtilitiesCS**,
   which grants internals to `DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, and `ToDoModel.Test` only.

2. **`QuickFiler.Test` already has working STA infrastructure** — manual STA-thread helpers at
   `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:267-278` and `:302-317`
   (`StartRunningDispatcher`/`ShutdownDispatcher`), and
   `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:21-45`. There is NO
   `[STATestClass]`/`MSTest.STAExtensions` package. This supersedes the earlier
   [[qfc-helper-classes-f4-434]] note that "QuickFiler.Test has zero STA infra".

**Why:** both facts change whether a child needs a package addition or a public-surface widening to
reach a seam; getting them wrong inflates scope estimates and risks a `packages.config` edit that is
not required.

**How to apply:** before proposing a public seam "because internals are unreachable", check the
target assembly's own `AssemblyInfo.cs` — not just UtilitiesCS. Before declaring the STA
last-resort clause infeasible in QuickFiler.Test, reuse the manual STA-thread helper pattern instead
of adding a NuGet package.

Also proven-reusable in QuickFiler.Test: `FormatterServices.GetUninitializedObject` for WinForms
viewer types (`ViewerQueueStaticWrapperTests.cs:97,128,143`) and
`ItemViewerQueue.SetCoreForTesting`/`ResetCoreForTesting` (internal, `ItemViewerQueue.cs:69,77`) to
replace the static viewer queue — with `[DoNotParallelize]` plus a reset in `[TestCleanup]`.

Related: [[qfc-collection-controller-454]], [[qfc-helper-classes-f4-434]]
