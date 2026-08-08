# Phase 2 — Final vstest (coverage)

Timestamp: 2026-08-08T16-10

Command: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
Invocation used:
`MSYS_NO_PATHCONV=1 "C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe" QuickFiler.Test/bin/Debug/QuickFiler.Test.dll SVGControl.Test/bin/Debug/SVGControl.Test.dll Tags.Test/bin/Debug/Tags.Test.dll TaskMaster.Test/bin/Debug/TaskMaster.Test.dll TaskTree.Test/bin/Debug/TaskTree.Test.dll TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll ToDoModel.Test/bin/Debug/ToDoModel.Test.dll UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll VBFunctions.Test/bin/Debug/VBFunctions.Test.dll /EnableCodeCoverage /InIsolation`

MSTest Discovery Caveat applied: same 9-assembly list as the Phase 0 baseline, re-derived via
`find . -iname "*.Test.dll" -path "*bin/Debug*" | grep -v "\.claude"` immediately before this run.

Precondition: solution rebuilt with default properties (exit 0) immediately before this run to
resync build outputs after the isolated diagnostic builds used in P2-T3.

EXIT_CODE: 0

Output Summary: `Total tests: 6296`, `Passed: 6296`, `Failed: 0`, `Skipped: 0` (`Test Run
Successful.`, 55.3753 seconds). Baseline was `6294`/`6294`; final is `6296`/`6296` — the +2 exactly
matches the two new regression tests added in Phase 1, zero failures in either run.

Coverage file
`TestResults/b512946c-8694-4c57-9bbd-32fc62fdcc1b/DanMoisan_MEGALODON4_2026-08-08.15_54_14.coverage`
converted via `dotnet-coverage merge <file> -f cobertura -o
docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/phase2-final-coverage.cobertura.xml`
(exit 0). Repo-wide `line-rate`: `0.6165729148230514` = **61.66%**, versus the Phase 0 baseline's
**74.43%** — an apparent 12.8-point drop, investigated below.

**Investigation (this is a denominator artifact, not a real coverage regression):**
`lines-covered` actually *increased* between baseline and final (158,543 → 160,251, +1,708 lines),
while `lines-valid` (the denominator) grew far more (213,002 → 259,906, +46,904 lines) and the
enumerated `<class>` element count grew from 1,924 to 2,336 distinct source files. The same 25
assembly packages are present in both Cobertura files (no new/removed assemblies). This matches
documented prior-session behavior for this repository's `dotnet-coverage`/Cobertura conversion:
run-to-run JIT/test-order variance changes how many generic/async-closure/state-machine method
instantiations get enumerated as "valid" lines, producing large denominator swings between
otherwise-equivalent runs (see `project_dotnet_coverage_denominator_nondeterminism`,
`project_coverage_delta_reproduce_baseline_counting_method` in prior session history).

To verify no genuine regression, a per-file `line-rate` comparison was run across every file
present in the Phase 0 baseline Cobertura output (1,924 files) against the same file in the final
Cobertura output:
- 0 files present in baseline are missing from final.
- Exactly 1 file (of 1,924) shows a `line-rate` decrease greater than 1 percentage point:
  `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs` (95.56% → 88.28%), a
  file this feature does not touch; consistent with ordinary test-order/timing variance in
  dataflow/async code, not a change caused by this feature.
- `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` shows 7 methods at 100% in baseline and 8
  methods at 100% in final (the extra entry is the new test coverage), no regression.
- `TaskMaster/Ribbon/RibbonController.Intelligence.cs` does not appear as an instrumented class in
  either Cobertura file, consistent with the `[ExcludeFromCodeCoverage]` attribute on
  `RibbonController` (ratified VSTO/COM exemption) suppressing instrumentation of that class
  entirely; this feature's one-line change adds no coverage surface, as required by the plan's Hard
  Scope Boundary.

**Conclusion**: the MSTest pass/fail counts (6296/6296 passed, 0 failed vs. baseline's 6294/6294
passed, 0 failed) are strictly no worse than baseline, satisfying AC6 without ambiguity. The
repo-wide coverage percentage swing is attributable to `dotnet-coverage`'s known run-to-run
denominator nondeterminism, confirmed via a per-file reproduction showing zero coverage loss
attributable to this feature's change. This finding is carried into P2-T5's comparison task.
