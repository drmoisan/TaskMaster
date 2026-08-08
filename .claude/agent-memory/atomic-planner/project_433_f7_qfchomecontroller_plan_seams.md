---
name: project-433-f7-qfchomecontroller-plan-seams
description: F7 #433 QfcHomeController coverage plan — mandatory partial split before seams, the :133/:136 invisible viewer/scheduler coupling, five frozen #424 test files, QuickFiler.Test.csproj wiring
metadata:
  type: project
---

Planning seams for epic #136 child **F7** (`quickfiler-qfc-home-controller-coverage`, issue #433). Load-bearing facts a future planner or reviewer in this territory needs:

- **The partial split is a hard sequencing constraint, not a cleanup.** `QuickFiler/Controllers/QfcHomeController.cs` is 487 lines against a hard 500 limit; the minimum seam set (S1 `ShowUserMessage` + S2 `MetricsFileWriter`) adds ~15. The `#region Public Properties` block (lines 406-485) moves to `QfcHomeController.Properties.cs` *before* any seam task, or an intermediate state breaches the limit. The alternative split (relocating the metrics block, lines 353-386) was rejected: it moves 22 *uncovered* lines into a partial already at ~65%.
- **`QfcHomeController.cs:133` and `:136` are invisibly coupled.** `TaskScheduler.FromCurrentSynchronizationContext()` at :136 succeeds today only because :133 constructs a live `QfcFormViewer` that auto-installs a `WindowsFormsSynchronizationContext`. Any viewer seam must ship with a scheduler seam in the same task or `InitAsync_InitializesCorrectly` turns into an `InvalidOperationException`. This is why F7's plan declines Tier C (S4/S5a/S5b) outright.
- **Five test files are FROZEN byte-unmodified by #424 AC 12:** `QfcHomeControllerIterationTests.cs`, `QfcInitEmailQueueZeroBatchTests.cs`, `QfcHighConfidencePreFilterTests.cs`, `QfcFormControllerTests.cs`, `QfcHomeControllerIssue218Tests.cs`. The first is the largest process risk — it is F7's own primary iteration suite and sits beside the new files. Name the new file explicitly in every test task and hash all five in Phase 0.
- **`QuickFiler.Test/QuickFiler.Test.csproj` needs explicit `<Compile Include>` wiring** for every new test file (legacy `packages.config` project; home-controller entries at lines 125-131). Specs that enumerate the diff as "new test files" omit this; without it the tests never compile. `QuickFiler/QuickFiler.csproj` compile entries for `QfcHomeController*` are at lines 325-327 and are a wave-1 merge hotspot shared with F9/F11/F13.
- **Two files named `IQfcHomeController.cs` exist.** Only `QuickFiler/Controllers/IQfcHomeController.cs` is compiled; `QuickFiler/Interfaces/IQfcHomeController.cs` is an orphan surviving in `QuickFiler.csproj.bak`. Always use full paths.
- **`IFilerHomeController` is also implemented by F8-owned `EfcHomeController`** — any member addition is CS0535 on a sibling-owned file, so all seams are `internal` on the class (`InternalsVisibleTo("QuickFiler.Test")` at `QfcHomeController.cs:18`).
- **Out-of-scope defects to leave alone:** #442 (metrics never flushed), #443 (metrics duration misread), #446 (empty-batch inference closes the UI queue irreversibly), #447 (dead `Iterate`/`Iterate2` removal, sequenced after F6). Tests pinning them must be labelled CHARACTERIZATION naming the issue.

Plan: `docs/features/active/2026-08-07-quickfiler-qfc-home-controller-coverage-433/plan.2026-08-07T20-41.md` (8 phases, 120 tasks).

Related: [[project-136-wave1-nonhalting-f1-dependency]], [[project-424-quickfiler-deadline-plan-seams]], [[project-legacy-csproj-explicit-compile-include]].
