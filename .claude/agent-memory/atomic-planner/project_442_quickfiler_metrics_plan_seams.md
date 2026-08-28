---
name: project-442-quickfiler-metrics-plan-seams
description: "#442/#443/#451 QuickFiler home-controller metrics plan seams: commented-out code defeats zero-hit grep gates, an AC conjunct that is already green, a seam-first ordering so red tests compile, and the repo coverage runner that throws below 80%"
metadata:
  type: project
---

Planning seams found while authoring the atomic plan for epic-child bug #442
(`docs/features/active/quickfiler-home-controller-metrics-442`). Each cost a real
correction and generalizes past this feature.

**Why:** the spec's own acceptance criteria contained two constructions that would have
shipped an unsatisfiable gate and a no-op gate respectively, and the red-before ordering
was not derivable from the spec's rollout list alone.

**How to apply:** check these four seams on any C# bugfix plan in this repo.

1. **A commented-out occurrence defeats a "returns no match" gate.**
   `QfcHomeController.Metrics.cs:120` is `//Duration = _stopWatchMoved.Elapsed.Seconds;`
   sitting directly above the live read on `:121`. AC-7 asserts a search for
   `Elapsed.Seconds` under `QuickFiler/Controllers/` returns no match. Fixing only the
   live line leaves the comment and the gate can never pass. Always grep the target
   pattern BEFORE writing a zero-hit gate and add an explicit deletion task for every
   commented, XML-doc, and string-literal hit. Complements
   [[zero-hit-grep-gates-need-carveouts]].

2. **Read every AC conjunct for "already true at branch head".** AC-7's second conjunct —
   `BuildQuickFileMetricLines` with `elapsedSeconds = 90` renders `90` not `30` — is green
   on the pre-fix source, because the 0-59 truncation lives at
   `EfcHomeController.Metrics.cs:23` where the `TimeSpan` component is read, not inside the
   pure function. Keep the test as a declared pin, tag it NOT `[expect-fail]`, and say in
   the plan which conjunct carries the falsifiability. See
   [[acceptance-edits-must-be-false-before-true-after]].

3. **Declare the injectable seam in its own task BEFORE the red tests.** The #442 flush
   tests assert an injected `MetricsFileWriter`. If the seam does not exist yet the tests do
   not COMPILE, and a non-compiling suite is not a clean red state. Order: (a) add the seam
   property only, production call path unchanged, solution compiles; (b) author the tests,
   which now compile and fail because the writer is never invoked; (c) rewire
   `WriteMetricsAsync`. Same shape applies to any writer/clock/delegate seam introduced by a
   bugfix.

4. **`scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws below a repo-wide 80% line
   floor.** `Assert-CoberturaLineCoverageThreshold` (in
   `Invoke-MSTestWithCoverage.Helpers.ps1`) throws BEFORE the Koverage `Set-Content`, so a
   sub-80% run leaves the RAW dotnet-coverage cobertura at
   `coverage\coverage.cobertura.xml` (absolute paths, third-party packages still present)
   and exits non-zero. Repo baseline is ~70%, so both the baseline and the final coverage
   task WILL exit non-zero. Plan for it: record `EXIT_CODE:` as observed, extract `line-rate`
   from the file as written, label it raw-aggregate, and gate on the change-scoped per-file
   figures instead. Extends [[project_494_threshold_reconciliation_plan_seams]].

Ownership context worth keeping: this epic child owns five `QuickFiler/Controllers/*.cs`
production files plus two `QuickFiler.Test/Controllers/*MetricsTests.cs` files. Both
`.csproj` files are unowned legacy non-SDK projects, so NO new `.cs` file is possible; and
`QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` is NOT owned, which makes the
`int` to `double` widening's effect on it a Phase 0 verification task rather than something
the plan can fix. `QfcHomeControllerMetricsTests.cs` starts at 421 lines against the 500 cap
— deleting the orphaned `NonBlockingProducer_DelaySeam_*` test is the plan's line budget.
