---
name: qfc-efc-metrics-442
description: "#442/#443/#451 metrics research: QFC MoveAndIterate stopwatch race is unfixable in owned files; legacy csproj blocks any new .cs; session-metrics CSV has zero in-repo readers"
metadata:
  type: project
---

Research findings for epic child `quickfiler-home-controller-metrics` (#442 flush, #443 duration
misread, #451 EFC inert duration), 2026-08-24. Owned files were the five
`Qfc/EfcHomeController*.cs` partials.

**Four non-obvious constraints that cost real time to establish:**

1. **`QuickFiler.csproj` and `QuickFiler.Test.csproj` are legacy non-SDK with explicit
   `<Compile Include=...>`.** Neither was in the owned-file list, so **no new production or test
   `.cs` file could be created at all**. Every design had to land in an existing partial. This
   invalidates the usual "extract to a new file to stay under 500 lines" escape hatch — check the
   csproj style and its ownership before proposing any new file in QuickFiler.

2. **The QFC `MoveAndIterate` stopwatch race cannot be fixed from the home controller.**
   `SwapStopWatch()` lives in `QfcHomeController.Iteration.cs` and all three call sites are in
   `QfcFormController.EventHandlers.cs` — both owned by feature 446. On the queue-non-empty branch
   the swap (via `LoadUiFromQueue`) races the metrics write (via a non-awaited `BackGroundMoveAsync`
   task). Three owned-file workarounds were evaluated and all fail: a property-setter snapshot fixes
   *which value* not *when*; self-swapping from the writer creates a second race; capturing at
   `CacheMoveObjects()` needs two forbidden files. The end-of-database branch IS deterministic and
   IS fixable with one line. Do not re-derive this.

3. **The session metrics CSV (`Globals.FS.Filenames.EmailSession`) has zero readers in the
   repository** — a code-file-type grep returns only three writers plus the settings plumbing. Any
   column-shape change is therefore low-risk, which is what unblocked the EFC missing-separator fix.

4. **`TimeProvider.GetTimestamp()` / `GetElapsedTime()` DO work on net481 here**, proven
   behaviourally by compiling production code in `QfcStreamingDequeueConfidenceGate.cs`, not by
   reading a manifest (the `packages/` dir is unrestored in agent worktrees). But `Stopwatch` is not
   TimeProvider-driven and abstracting it would require editing `IFilerHomeController` — out of
   reach.

**Why:** the epic split ownership file-by-file across siblings 446/464/468, so several "obvious"
fixes sat one file outside scope. Establishing *which* half of a defect is reachable was the
expensive part of the research, not finding the defect.

**How to apply:** when researching another child of this epic, first map the call graph across the
ownership boundary and state decisively which half is reachable — that is the finding the
orchestrator actually needs. See [[winforms-testability-epic-298]] for the sibling-coordination
pattern.
