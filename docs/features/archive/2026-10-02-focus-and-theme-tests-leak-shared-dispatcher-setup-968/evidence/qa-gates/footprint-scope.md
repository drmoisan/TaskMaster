# Footprint gate (issue #968, task P8-T9)

Timestamp: 2026-10-03T03-32
Command: git -C WORKTREE diff --name-status 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD
Canonical command: git -C WORKTREE diff --name-status 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD; git -C WORKTREE status --porcelain --untracked-files=all; git -C WORKTREE rev-parse origin/main (separate Bash calls)
EXIT_CODE: 0
Output Summary:
- name-status diff: exit 0 (an additional pathspec-scoped run of the same diff over QuickFiler, QuickFiler.Test, scripts and docs/features/potential was also made by mistake before the unscoped run; it printed the same non-FEATURE lines)
- status porcelain: exit 0
- rev-parse origin/main: exit 0

THIS-ITEM-FOOTPRINT (name-status paths outside FEATURE, excluding the P0-T3 INHERITED-COMMITTED paths): exactly the fourteen Write Set code paths:
- M QuickFiler/Controllers/QfcDatamodel.cs
- M QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs
- M QuickFiler.Test/QuickFiler.Test.csproj
- M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
- M QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs
- M QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs
- M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
- M QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs
- M QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs
- M QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs
- M QuickFiler.Test/Controllers/QfcDatamodelTests.cs
- A QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs
- A QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs
- A QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs

The only paths under `QuickFiler/` are the two production paths AC20 names (`QuickFiler/Controllers/QfcDatamodel.cs`, `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`); every other footprint path is under `QuickFiler.Test/`. Every other name-status path is under FEATURE (docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/...).

INHERITED-AND-EXCLUDED:
- A docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md (one of the two promoted records)
- A docs/features/potential/promoted/2026-10-02-qfc-datamodel-950-review-residuals.md (one of the two promoted records)
- the FEATURE paths that P0-T3 listed in INHERITED-COMMITTED (all under FEATURE)

Porcelain: every line names a path under FEATURE (` M` the plan file and `??` twelve evidence files under FEATURE/evidence/qa-gates/); no porcelain line names a path under QuickFiler/, QuickFiler.Test/ or scripts/.

ORIGIN-MAIN-NOW: 993fdd01566dee82e5f37acb761a600feaaa1454
BASE-REF-MOVED: YES (origin/main differs from 94287369908cc920b21b0e3256314f988ad7d2f5; recorded, not a stop, because every gate names BASE explicitly; it equals the P0-T3 ORIGIN-MAIN-SHA)
