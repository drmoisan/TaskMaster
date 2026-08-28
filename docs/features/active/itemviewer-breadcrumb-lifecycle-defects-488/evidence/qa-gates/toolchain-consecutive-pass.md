# Four Toolchain Stages — One Consecutive Clean Pass ([P8-T11])

Timestamp: 2026-08-28T06-32

Command: comparison of the ordered stage timestamps recorded by `[P8-T1]` through `[P8-T5]`, plus a
re-take of the SHA-256 of all seven owned files after the `[P8-T5]` test run, compared against the
post-format hashes `[P8-T1]` recorded.
EXIT_CODE: 0

## Ordered stage timestamps of the final pass

| Stage | Task | Command | Timestamp (UTC) | Result |
| --- | --- | --- | --- | --- |
| 1. Format | `[P8-T1]` | `csharpier format` over the seven owned paths | **2026-08-28T06-20** | EXIT 0 |
| 1v. Format verify | `[P8-T2]` | `csharpier check .` | **2026-08-28T06-20** | EXIT 0, zero unformatted of 1554 |
| 2. Lint | `[P8-T3]` | msbuild `/t:Rebuild` with analyzers | **2026-08-28T06-21** | EXIT 0, 0 errors |
| 3. Type-check | `[P8-T4]` | msbuild `/t:Rebuild` with `TreatWarningsAsErrors` | **2026-08-28T06-22** | EXIT 0, 0 errors |
| 4. Test | `[P8-T5]` | vstest with `/EnableCodeCoverage /InIsolation` | **2026-08-28T06-25** | EXIT 0, 1201/1201 passed |

The timestamps are monotonically non-decreasing and span five minutes, so the four stages ran **in
order** and **consecutively**, with no other work interleaved.

## No owned file's SHA-256 changed between the format pass and the test run

| # | File | SHA-256 after `[P8-T1]` format | SHA-256 after `[P8-T5]` test | Changed? |
| --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | `04c2ae3d…98cf28` | `04c2ae3d…98cf28` | **no** |
| 2 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | `e50842ce…049a8d` | `e50842ce…049a8d` | **no** |
| 3 | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | `676bcb3a…4c45db` | `676bcb3a…4c45db` | **no** |
| 4 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `990f4f6a…01889e` | `990f4f6a…01889e` | **no** |
| 5 | `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | `c0ae85c1…0f9366` | `c0ae85c1…0f9366` | **no** |
| 6 | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | `ac78b80b…967799` | `ac78b80b…967799` | **no** |
| 7 | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | `fc5098d7…7e09ec` | `fc5098d7…7e09ec` | **no** |

**All seven are byte-identical across the pass.** No stage after the format stage modified any owned
file, so stages 2, 3, and 4 all ran against the same content that stage 1 produced and that stage 1v
verified as canonically formatted.

## Number of loop restarts before the final pass: 0

The four stages ran **once each**, in order, and every one passed on its first execution:

- Stage 1 exited 0.
- Stage 1's verification, stage 2, stage 3, and stage 4 all exited 0.
- No stage failed, and no stage after stage 1 changed a file.

**Zero restarts were required.** The recorded pass is therefore the first and only execution of the
Phase 8 loop.

### Note on the one file the format stage rewrote

Stage 1 changed `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`, by a single cosmetic lambda reflow. That
is not a restart trigger: rewriting files is the format stage's function, and the restart rule targets a
**later** stage failing or changing files. The loop's own verification of that rewrite is stage 1v,
`csharpier check .`, which ran immediately afterward and reported zero unformatted files across all
1554 files checked. The six other owned files were already canonical and stage 1 left them untouched.

### Note on defects found before the final pass

Two defects were found and fixed during earlier phases, both well before Phase 8 began, so neither
caused a restart of this loop:

- A **CS8604** nullable warning introduced by D2's first guard form, found by an intermediate build in
  `[P2-T5]` and fixed there. `[P8-T4]` confirms zero `CS86xx` diagnostics remain.
- The new test file exceeding its 480-line cap at 521 lines, found in `[P6-T2]` and compacted there.
  `[P8-T8]` confirms it is delivered at 480.

Output Summary: The four stages ran **in order and consecutively** at 06-20, 06-20, 06-21, 06-22, and
06-25 UTC, each exiting 0 on its first execution. **All seven owned files' SHA-256 values are identical
between the `[P8-T1]` format pass and the `[P8-T5]` test run**, so no file changed during the pass.
**Zero loop restarts** occurred before the recorded final pass.
