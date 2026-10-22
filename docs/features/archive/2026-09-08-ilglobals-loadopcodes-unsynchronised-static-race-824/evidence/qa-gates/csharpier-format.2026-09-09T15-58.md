# QC loop step 1 — csharpier format (Issue #824, task P5-T1)

Timestamp: 2026-09-09T15-58

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; git add -N .; $before = (git diff HEAD | Out-String); dotnet tool run csharpier format . 2>&1 | Tee-Object -FilePath coverage/csharpier-format.log; $rc = $LASTEXITCODE; git add -N .; $after = (git diff HEAD | Out-String); Write-Output ("FORMAT_EXIT=" + $rc); Write-Output ("FORMAT_CHANGED_TREE=" + ($before -ne $after)); Write-Output "---NAMES---"; git diff --name-only HEAD; Write-Output "---PORCELAIN---"; git status --porcelain --untracked-files=all; exit $rc'`

The full-patch comparison is anchored on `HEAD` rather than on the merge base, per the adaptation
recorded in `evidence/other/executor-deviations.2026-09-09T15-28.md`. The anchor choice does not
affect the `FORMAT_CHANGED_TREE` observation, which compares the same anchor before and after the
formatter and is therefore a differential measurement.

EXIT_CODE: 0

## Two passes were required

| Pass | `FORMAT_EXIT` | `FORMAT_CHANGED_TREE` | Console line | Disposition |
|---|---|---|---|---|
| 1 | 0 | **True** | `Formatted 1622 files in 4429ms.` | formatter rewrote tracked files; loop restarted at P5-T1 |
| 2 | 0 | **False** | `Formatted 1622 files in 1578ms.` | final pass for this step |

Pass 1 rewrote both source files this feature modifies. `git diff --stat HEAD` for the two of them
immediately after that pass reported:

```
.../SDILReader/ILGlobals_Tests.cs                  | 169 +++++++++++++++++++--
 .../NewtonsoftHelpers/SDIL Reader/ILGlobals.cs     |  51 +++++--
 2 files changed, 194 insertions(+), 26 deletions(-)
```

Those figures are the cumulative diff of this feature's changes against `HEAD` after formatting, not
the size of the formatter's own edit. The formatter's edit was confined to the hand-written line
wrapping of the new tests and the new static constructor, which CSharpier reflowed to its own
layout. No file outside this feature's two source files was rewritten, which the pass-2 name listing
below confirms.

Per the Phase 5 preamble a formatter rewrite restarts the loop at P5-T1, which is what happened. The
count CSharpier prints is a processed-file count rather than a changed-file count, so `1622` on both
passes is not evidence that nothing changed; the `FORMAT_CHANGED_TREE` differential is.

## Final-pass observations

`FORMAT_EXIT=0` and `FORMAT_CHANGED_TREE=False`, which is what this task requires.

`PORCELAIN_COUNT=37`.

### `---NAMES---` listing, reproduced verbatim

```text
UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs
UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/base-anchor.2026-09-09T14-58.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/base-inertness.2026-09-09T15-19.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/coverage-baseline.2026-09-09T15-14.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/coverage-classes-baseline.2026-09-09T15-16.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/coverage-contingency.2026-09-09T15-15.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/csharpier-check-baseline.2026-09-09T15-06.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/file-line-counts-baseline.2026-09-09T15-17.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/msbuild-analyzers-baseline.2026-09-09T15-08.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/msbuild-nullable-baseline.2026-09-09T15-09.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/phase0-instructions-read.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-analyzer-paths.2026-09-09T15-03.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-dotnet-coverage.2026-09-09T15-05.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-restore.2026-09-09T15-00.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-sdk.2026-09-09T15-02.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-tool-restore.2026-09-09T15-04.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/other/executor-deviations.2026-09-09T15-28.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/other/p2t2-line-range-record.2026-09-09T15-34.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/other/phase3-completion-notes.2026-09-09T15-42.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/ac1-assignment-sweep.2026-09-09T15-46.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/ac10-build-file-discipline.2026-09-09T15-53.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/ac5-loadopcodes-retained.2026-09-09T15-48.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/ac6-nullable-and-comment.2026-09-09T15-49.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/ac7-test-rework.2026-09-09T15-50.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/ac8-parallelism-preserved.2026-09-09T15-51.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/ac9-no-synchronisation-primitive.2026-09-09T15-52.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/csharpier-format.2026-09-09T15-58.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/scope-gate-phase4.2026-09-09T15-55.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/ac2-ac3-pass-after.2026-09-09T15-38.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/ac2-fail-before.2026-09-09T15-25.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/ac3-fail-before.2026-09-09T15-30.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/build-p1t2.2026-09-09T15-24.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/build-p1t5.2026-09-09T15-29.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/build-p2t4.2026-09-09T15-36.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/build-p3t4.2026-09-09T15-43.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/ilglobals-tests-after-rework.2026-09-09T15-44.md
docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/plan.2026-09-08T23-51.md
```

### `---PORCELAIN---` listing, reproduced verbatim

```text
 M UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs
 M "UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs"
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/base-anchor.2026-09-09T14-58.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/base-inertness.2026-09-09T15-19.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/coverage-baseline.2026-09-09T15-14.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/coverage-classes-baseline.2026-09-09T15-16.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/coverage-contingency.2026-09-09T15-15.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/csharpier-check-baseline.2026-09-09T15-06.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/file-line-counts-baseline.2026-09-09T15-17.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/msbuild-analyzers-baseline.2026-09-09T15-08.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/msbuild-nullable-baseline.2026-09-09T15-09.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/phase0-instructions-read.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-analyzer-paths.2026-09-09T15-03.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-dotnet-coverage.2026-09-09T15-05.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-restore.2026-09-09T15-00.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-sdk.2026-09-09T15-02.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/baseline/toolchain-tool-restore.2026-09-09T15-04.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/other/executor-deviations.2026-09-09T15-28.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/other/p2t2-line-range-record.2026-09-09T15-34.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/other/phase3-completion-notes.2026-09-09T15-42.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/ac1-assignment-sweep.2026-09-09T15-46.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/ac10-build-file-discipline.2026-09-09T15-53.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/ac5-loadopcodes-retained.2026-09-09T15-48.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/ac6-nullable-and-comment.2026-09-09T15-49.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/ac7-test-rework.2026-09-09T15-50.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/ac8-parallelism-preserved.2026-09-09T15-51.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/ac9-no-synchronisation-primitive.2026-09-09T15-52.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/csharpier-format.2026-09-09T15-58.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/qa-gates/scope-gate-phase4.2026-09-09T15-55.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/ac2-ac3-pass-after.2026-09-09T15-38.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/ac2-fail-before.2026-09-09T15-25.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/ac3-fail-before.2026-09-09T15-30.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/build-p1t2.2026-09-09T15-24.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/build-p1t5.2026-09-09T15-29.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/build-p2t4.2026-09-09T15-36.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/build-p3t4.2026-09-09T15-43.md
 A docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/evidence/regression-testing/ilglobals-tests-after-rework.2026-09-09T15-44.md
 M docs/features/active/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race-824/plan.2026-09-08T23-51.md
```

P5-T3 consumes both listings and classifies every path in them against the D6 Owned Write Set and
against the P0-T8 pre-existing-drift list.
