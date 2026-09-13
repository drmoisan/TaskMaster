# Phase 0 re-anchor record

Timestamp: 2026-09-13T15-03
ReAnchoredAt: 2026-09-13T15-03
ReAnchorReason: merge commit 8213826f695439e86e3ed34faa575de493a11ec7 brought origin/main into this
branch, superseding every measured Phase 0 baseline. Structural citations were re-verified and none
moved; only the measured figures required re-derivation.
Command: (this record interprets the re-measurements recorded in the five artifacts it names; each
carries its own Command line)
EXIT_CODE: 0

## Why a re-anchor was required

This item halted at P0-T12 when the coverage runner produced a raw rather than post-processed Cobertura
document. The cause was three failing tests under that runner, traced to an unsatisfiable bind on
netstandard version 2.1.0.0 entering through an FSharp.Core redirect. The fix was delivered upstream as
issue 877, pull request 880, merged to origin/main at a5622ab9123a88bfa3ec5b8fccfdc613e74c4df5 and
merged into this branch as 8213826f695439e86e3ed34faa575de493a11ec7. It installs a process-wide
`AssemblyResolve` fallback in the QuickFiler test assembly's own assembly initializer through the new
shared source file TestSupport/TestAssemblyResolver.cs.

The merge brought 91 files onto this branch. Every measured Phase 0 baseline was therefore evidence
about a tree that no longer exists, and the diff anchor named a commit that is no longer this branch's
tip. Retaining either would have made the plan's later gates read the merged-in upstream work as this
item's own output.

## Superseded and re-derived values

| Figure | Superseded | Re-derived | Moved |
|---|---|---|---|
| BASE_SHA | d44b51932c6094dd54dc60e7c0f8d5f14d9e088d | 8213826f695439e86e3ed34faa575de493a11ec7 | yes |
| BASELINE_TEST_TOTAL | 1394 | 1395 | yes, by +1 |
| Analyzer error count | 0 | 0 | no |
| Analyzer warning count | 0 | 0 | no |
| Nullable error count | 0 | 0 | no |
| Nullable warning count | 0 | 0 | no |
| PreExistingDriftFiles | NONE | NONE | no |
| DriftInsideWriteSet | NONE | NONE | no |
| CSharpier files checked | 1626 | 1627 | yes, by +1 |
| PreExistingWorktreePaths | empty | empty | no |

Two figures moved, and each movement is accounted for by a specific property of the merge rather than
inferred.

The test total rose by exactly one because the merge added exactly one `[TestMethod]` to the QuickFiler
test assembly: `Init_CreatesTokenSourceBeforeAnyLoaderObservesIt` in
QuickFiler.Test/Controllers/QfcHomeControllerTests.cs, a regression test for issue 839 delivered in a
sibling change. An anchored diff of that file between the superseded anchor and the merge commit shows
a single added block of 71 lines carrying that one test method and no other. Every later `total`
comparison in this plan — P1-T7, P2-T9, P3-T11 and the arithmetic identity in P4-T24 — now reads 1395.

The CSharpier inspected-file count rose by exactly one because the merge added exactly one new tracked
C# file, TestSupport/TestAssemblyResolver.cs, confirmed by an addition-filtered anchored diff restricted
to C# files. Both drift figures stayed at `NONE`, so the merged-in file arrived CSharpier-clean and this
item inherits no pre-existing drift. The consequence recorded by P0-T8 for P5-T1, P6-T6 and P7-T22 is
therefore unchanged, and acceptance criterion AC22 remains not-at-risk on the strength of this record.

## Artifacts re-measured and overwritten in place

Each keeps its mandated filename, which encodes this plan's artifact-naming contract rather than a claim
about when the command ran, and each carries `ReAnchoredAt:` and `ReAnchorReason:` lines:

- p0-t2-diff-anchor.2026-09-12T10-25.md — new BASE_SHA, fresh porcelain capture, `SupersededBaseSha:` retained.
- p0-t8-csharpier-baseline.2026-09-12T10-25.md — CMD-CHECK re-run.
- p0-t9-analyzer-baseline.2026-09-12T10-25.md — CMD-ANALYZE re-run.
- p0-t10-nullable-baseline.2026-09-12T10-25.md — CMD-NULLABLE re-run.
- p0-t11-test-baseline.2026-09-12T10-25.md — CMD-VSTEST and CMD-TRXCOUNTERS re-run, new BASELINE_TEST_TOTAL.

## Structural citations re-verified as unmoved

Every structural citation the plan makes was re-derived against the post-merge tree in this pass. None
moved, so no Phase 1 range, no project-file placement citation and no line-count assertion needed
revision.

- `QuickFiler/Controllers/QfcQueue.cs` measures exactly 507 physical lines, the figure P0-T13 asserts.
- `QuickFiler/Controllers/QfcQueue.Enqueue.cs` measures exactly 200, the other figure P0-T13 asserts.
- The Tlp Manipulation region opens at line 230 and closes at line 453, the range P1-T1 names.
- The Helper Methods region opens at line 472 and closes at line 505, the range P1-T2 names.
- The two QfcQueue Compile items in `QuickFiler/QuickFiler.csproj` sit at lines 348 and 349, the
  placement P1-T3 names.
- The Interfaces Compile block in that project begins at line 363, the placement P2-T4 names.
- The three QfcQueue test Compile items in `QuickFiler.Test/QuickFiler.Test.csproj` sit at lines 119,
  120 and 215, the placement P4-T2 names. These are unmoved even though the merge added three lines to
  that project file, because those lines landed after line 215.

An anchored diff confirms the merge touched none of the five production Write Set code paths. Within the
Write Set it touched only `QuickFiler.Test/QuickFiler.Test.csproj`.

## P0-T3 through P0-T7 were not re-run, and why

Those five tasks provision the build environment. A merge changes tracked files in the worktree; it
cannot uninstall an SDK, empty a package cache or remove a globally installed tool, so no merge can
invalidate them. Re-running them would consume the shared build lock to re-observe an unchanged fact.
Each was instead verified present:

- P0-T3, repo-local SDK: the .dotnet-sdk/sdk/8.0.205 directory is present.
- P0-T4, tool manifest restore: CSharpier resolved and ran from the manifest in this pass, in the P0-T8
  re-measurement, which is a stronger observation than the version print because it required the
  restored tool to execute.
- P0-T5, NuGet package graph: the packages directory is present, and both solution rebuilds in this pass
  completed with zero errors, which they could not do against an unrestored graph.
- P0-T6, analyzer include paths: a missing analyzer path is compiler error CS0006, so the two zero-error
  solution rebuilds in this pass are positive evidence that every analyzer include still resolves.
- P0-T7, dotnet-coverage global tool: the coverage runner throws before running anything when the tool
  is absent; it ran to completion and produced a post-processed document in P0-T12, so the tool is
  present.

Four of the five are evidenced by a command that ran in this pass and would have failed had the
provisioning been absent, which is stronger than re-running the provisioning step and observing it
report success.

## Checklist state

P0-T1 through P0-T11 remain checked. Each of the five re-measured baselines was checked against its own
acceptance condition after re-measurement and every one still passes: both rebuilds exited 0 with zero
errors and zero warnings, CMD-CHECK exited 0 naming no file, and the test run exited 0 with `failed=0`.
No re-measurement failed its acceptance condition, so no task required un-checking.

Output Summary: Phase 0 re-anchored onto merge commit 8213826f. Two measured figures moved and both are
accounted for exactly: BASELINE_TEST_TOTAL 1394 to 1395 from one added test method, and CSharpier
inspected files 1626 to 1627 from one added source file. The analyzer, nullable and drift figures are
unchanged at zero, zero and NONE. Every structural citation in the plan was re-derived and none moved,
so no Phase 1 range or project-file placement required revision. P0-T3 through P0-T7 were not re-run
because environment provisioning is not invalidated by a merge, and each is evidenced present by a
command that ran in this pass.
