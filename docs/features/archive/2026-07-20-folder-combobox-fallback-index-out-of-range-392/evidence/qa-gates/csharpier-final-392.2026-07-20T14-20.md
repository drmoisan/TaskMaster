Timestamp: 2026-07-20T14-20
Command: `csharpier format .` (equivalent to plan's `dotnet tool run csharpier .`), followed by `csharpier check .` (non-mutating re-verification)
EXIT_CODE: 0 (format run); 1 (subsequent check run — see explanation below; this is the pre-existing baseline formatting-noise exit code, not a failure introduced by this plan)
Output Summary:
- `csharpier format .` reported "Formatted 1406 files in 2969ms" and exited 0. `git status` after this
  run showed it had rewritten 31 pre-existing, out-of-scope `app.config`/`packages.config` files
  across unrelated projects (SVGControl, Tags, Tags.Test, TaskMaster, TaskMaster.Test, TaskTree,
  TaskTree.Test, TaskVisualization, TaskVisualization.Test, ToDoModel, ToDoModel.Test, UtilitiesCS,
  UtilitiesCS.Test, VBFunctions.Test, plus QuickFiler/QuickFiler.Test's own `app.config`/
  `packages.config`) — exactly the same 32 pre-existing formatting errors already documented at
  baseline in `evidence/baseline/csharpier-baseline.2026-07-20T13-15.md`. This conflicts with this
  plan's own Scope-Lock ("No other file may be changed by this plan"; only
  `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` and
  `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` are authorized for
  modification). Per this plan's Scope-Lock (the more specific, overriding constraint for this
  minor-audit bugfix), the out-of-scope config-file changes were reverted via
  `git checkout -- <path>` for each of the 31 affected non-authorized files, leaving only the two
  Scope-Lock-authorized files (already csharpier-clean from earlier per-file formatting during
  Phase 1) as the sole tracked modifications.
- A subsequent non-mutating `csharpier check .` (run after the revert) confirms: 32 pre-existing
  errors remain (unchanged from the P0-T9 baseline count — no regression, no improvement, no new
  unformatted files), and neither `QfcItemController.FolderHandling.cs` nor
  `QfcItemController.FolderHandlingTests.cs` appears in the error list. Both in-scope files are
  fully CSharpier-formatted. "Checked 1406 files in 2540ms."
- No further Phase 2 restart is triggered: this command's only *authorized* file-changing effect
  (formatting the two in-scope files) had already converged to a stable, clean state before this
  step ran (they were formatted incrementally during Phase 1 edits), so no in-scope file changed as
  a result of this step.
