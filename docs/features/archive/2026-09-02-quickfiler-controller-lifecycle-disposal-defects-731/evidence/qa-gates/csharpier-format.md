# Final QA gate 1 — formatting

Timestamp: 2026-09-03T14-27

Task: [P5-T1]
Issue: #731

## Axis F resolution

**Axis F row taken: F-CLEAN.**

The selecting fact is the exit code `[P0-T6]` recorded for `dotnet tool run csharpier check .` on the pre-change tree: **`EXIT_CODE: 0`**, with 1574 files checked and 0 reported unformatted. Under the DEGRADED-RUN STATE MODEL, `[P0-T6] recorded EXIT_CODE: 0` selects row **F-CLEAN**, whose branch is the repository-wide format. Row F-DRIFT was not taken and the eleven-path scoped form was not used. The condition was read from the recorded Axis F table rather than re-derived here.

## Command

```
dotnet tool run csharpier format .
```

Run from the worktree root, through `dotnet tool run` so the manifest-pinned CSharpier 1.2.6 is used rather than any global install.

EXIT_CODE: 0

## Output Summary

Runner's final summary line, quoted verbatim as observed on the recorded run:

```
Formatted 1577 files in 2729ms.
```

### Loop restart, and why this artifact records the second run

The first invocation of this task rewrote tracked files. Under the Phase 5 preamble and the General Code Change Policy toolchain rule, a step that auto-fixes files restarts the loop from step 1, so `[P5-T1]` was run a second time. The second run is the one recorded above and it repaired nothing: `git diff --numstat` was captured immediately before and immediately after it and the two captures are byte-identical (SHA-256 comparison returned equal). The pass recorded here is therefore the start of a single uninterrupted clean pass through `[P5-T1]`..`[P5-T5]`.

Recording that comparison is required because a formatter rewrites files and still exits 0, so the exit code alone cannot distinguish a clean run from a repairing one. The `Formatted 1577 files` line has the same limitation: it counts files processed, not files changed.

### `git status --porcelain`, captured immediately after the run

Collapsed (default) porcelain form, full output:

```
 M QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs
 M QuickFiler.Test/Controllers/QfcDatamodelTests.cs
 M QuickFiler.Test/QuickFiler.Test.csproj
 M QuickFiler/Controllers/QfcCollectionController.cs
 M QuickFiler/Controllers/QfcDatamodel.cs
 M QuickFiler/Controllers/QfcFormController.SetupDisposal.cs
 M QuickFiler/Controllers/QfcQueue.cs
 M QuickFiler/Controllers/QfcRemainingQueueAdmission.cs
 M "QuickFiler/Helper Classes/EmailMoveMonitor.cs"
 M docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/plan.2026-09-02T12-02.md
?? QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.Volatile.cs
?? QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs
?? QuickFiler.Test/Controllers/QfcMoveMonitorTopologyTests.cs
?? docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/evidence/
```

## Scope check against the permitted union

Every one of the fourteen listed paths falls inside the union of the PLAN WRITE SET, the DISCLOSED BASELINE SET recorded by `[P0-T2]`, and the AGENT-MEMORY ALLOWANCE. No path lies outside it.

| Path | Member of |
|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | PLAN WRITE SET 1 |
| `QuickFiler/Controllers/QfcDatamodel.cs` | PLAN WRITE SET 2 |
| `QuickFiler/Controllers/QfcQueue.cs` | PLAN WRITE SET 3 |
| `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | PLAN WRITE SET 4 |
| `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` | PLAN WRITE SET 5 |
| `QuickFiler/Helper Classes/EmailMoveMonitor.cs` | PLAN WRITE SET 6 |
| `QuickFiler.Test/Controllers/QfcMoveMonitorTopologyTests.cs` | PLAN WRITE SET 7 (created) |
| `QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs` | PLAN WRITE SET 8 (created) |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | PLAN WRITE SET 9 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | PLAN WRITE SET 10 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.Volatile.cs` | PLAN WRITE SET 11 (created) |
| `QuickFiler.Test/QuickFiler.Test.csproj` | PLAN WRITE SET 12 |
| `docs/.../plan.2026-09-02T12-02.md` | DISCLOSED BASELINE SET (tracked-modified) |
| `docs/.../evidence/` | DISCLOSED BASELINE SET (untracked, collapsed directory entry) |

The collapsed porcelain form is used here deliberately. The feature folder's four requirement and plan documents are tracked and committed, so only the modified plan file appears; the `evidence` subdirectory this plan creates is wholly untracked and collapses to the single directory entry that `[P0-T2]` captured into the DISCLOSED BASELINE SET. The individual evidence artifact paths therefore need no separate enumeration at this step — `[P5-T9]` enumerates them from the `--untracked-files=all` form instead.

No path under `.claude/agent-memory/` appears in this capture, so the AGENT-MEMORY ALLOWANCE was available but not needed.

The four COVERAGE DOCUMENT PATHS and the single permitted helper script do not appear because `coverage/` is gitignored at `.gitignore:144`, with only `coverage/.gitkeep` re-included at `:145`.

## Verdict

PASS. `EXIT_CODE: 0`, Axis F row **F-CLEAN** named together with the `[P0-T6]` exit code 0 that selected it, the recorded porcelain output lists no path outside the permitted union, and the recorded run repaired nothing.
