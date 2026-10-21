# Terminal Evidence — Final Commit

Timestamp: 2026-09-13T15-51
Task: [P2-T33]

HeadSha: 7c10e3bdaf8c526f39dded464aa6ec4812a6cd71

That is the head SHA produced by the P2-T32 commit, whose message is
`fix(872): assert scan-bound log, add progress package source ownership, remove dormant tracker`. It
recorded 22 files changed, 1265 insertions and 48 deletions. The base commit for the whole delivery,
recorded by P0-T2, is `430e2a11db0fa7069d02d42e18df46d21f7db7b5`.

## Write Set Final State

| Path | Final state |
|---|---|
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs | modified |
| UtilitiesCS/Threading/ProgressPackage.cs | modified |
| UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs | modified |
| UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs | modified |
| UtilitiesCS/Threading/ProgressTrackerAsync.cs | deleted |
| UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs | deleted |
| UtilitiesCS/UtilitiesCS.csproj | modified |
| UtilitiesCS.Test/UtilitiesCS.Test.csproj | modified |

No file outside the Write Set was modified by this delivery, apart from this plan's own bookkeeping and
evidence under the feature folder, which the Write Set section of the plan declares separately.

## Note On The Deletion Staging Span

The P2-T32 span `git add -A -- UtilitiesCS/Threading/ProgressTrackerAsync.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`
exited 128 with `fatal: pathspec 'UtilitiesCS/Threading/ProgressTrackerAsync.cs' did not match any files`.
That is recorded rather than suppressed. Its cause is that both deletions were already committed by the
Phase 1 commit, so neither path existed in the worktree nor differed from the index when the span ran,
and `git add` treats a pathspec matching nothing at all as an error. The span had nothing left to stage
because its work was already recorded in the branch history; it is not a case of a deletion failing to
be captured. P2-T11 verifies both deletions independently against the base commit, and its anchored
name-status diff printed exactly two lines each beginning with the deletion status letter D. The
subsequent commit succeeded and the porcelain span produced zero output lines.

## Gate Outcome

No hook blocked either commit. The pre-implementation commit gate that D13 anticipates did not fire on
the P2-T32 commit, which stages production source, nor on this one. Nothing was restructured, no commit
was split into exempt pathspecs, and no shared orchestration state file was read or written.

## Loop Restart Accounting

The Phase 2 toolchain loop ran two passes of P2-T1 and one pass of every later stage.

- Pass 1 of P2-T1 recorded `ChangedFileCount: 1`: the formatter rewrote
  `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs`, whose anchored insertion figure moved from 96
  to 100 and whose line count moved from 216 to 220. That is the stated restart trigger and the loop
  restarted from P2-T1.
- Pass 2 of P2-T1 recorded `ChangedFileCount: 0` and the loop advanced.
- P2-T2 through P2-T7 each ran once, each passed, and none rewrote a file, so no further restart
  occurred. All four toolchain stages therefore completed in a single clean pass.

The superseded pass is recorded inside `evidence/qa-gates/qc-csharpier-format.md` alongside the
completing pass, so the restart is auditable from the artifact of the task that triggered it.

## Evidence Index

**evidence/baseline/** (Phase 0, 14 artifacts)

- phase0-instructions-read.md — P0-T1
- base-commit.md — P0-T2
- dotnet-tool-restore.md — P0-T3
- restore.md — P0-T4
- csharpier-check.md — P0-T5
- build-analyzers.md — P0-T6
- build-nullable.md — P0-T7
- tests-utilitiescs.md — P0-T8
- tests-quickfiler.md — P0-T9
- coverage-baseline.md — P0-T10
- coverage-baseline-progresspackage.md — P0-T11
- compile-item-counts.md — P0-T12
- pinned-source-facts.md — P0-T13
- phase0-gate.md — P0-T14, recording `PHASE0_GATE: GREEN`

**evidence/other/**

- implementation-handoff.md — P1-T1
- p1-t14-interim-build.md — P1-T14
- final-commit.md — P2-T33, this artifact

**evidence/regression-testing/**

- p1-t15-utilitiescs-scoped.md — P1-T15
- p1-t16-quickfiler-scoped.md — P1-T16
- fail-before-exception.2026-09-12T10-26.md — P1-T17

**evidence/qa-gates/** (Phase 2, 16 artifacts)

- qc-csharpier-format.md — P2-T1
- qc-csharpier-check.md — P2-T2, AC8
- qc-build-analyzers.md — P2-T3, AC9
- qc-build-nullable.md — P2-T4, AC10
- qc-tests-utilitiescs.md — P2-T5
- qc-tests-quickfiler.md — P2-T6
- qc-coverage-postchange.md — P2-T7
- ac11-test-count-delta.md — P2-T8, AC11
- ac12-progresspackage-coverage.md — P2-T9, AC12
- ac7-compile-item-counts.md — P2-T10, AC7
- ac6-deletions.md — P2-T11, AC6
- ac5-using-declaration.md — P2-T12, AC5
- ac3-ownership-structure.md — P2-T13, AC3
- ac4-disposal-tests.md — P2-T14, AC4
- ac1-scan-cap-log.md — P2-T15, AC1
- ac2-ceiling-log.md — P2-T16, AC2
- file-size-audit.md — P2-T17
- ac-reconciliation.md — P2-T31

**evidence/issue-updates/**

- ac-status-summary.md — P2-T30

## Evidence Hygiene

No TRX file, no MSBuild log and no raw Cobertura XML is committed by either commit task, per D10. The
MSBuild file logs remain at `TestResults/msbuild/`, the vstest TRX files at `TestResults/vstest/`, and
the two raw coverage documents at `TestResults/coverage/`, all under the git-ignored results directory
class. The prohibited path artifacts/csharp/coverage.xml was checked and does not exist; it is named
here in prose rather than in path formatting so that no extractor reads this line as a write claim. No
absolute host path appears in any committed artifact.

## Final Toolchain Results, In CLAUDE.md Order

1. `dotnet tool run csharpier format .` — exit 0, `Formatted 1625 files in 1779ms.`, `ChangedFileCount: 0`
2. `dotnet tool run csharpier check .` — exit 0, `Checked 1625 files in 5142ms.`, no unformatted file
3. MSBuild `/t:Rebuild` with EnableNETAnalyzers and EnforceCodeStyleInBuild — exit 0, `Build succeeded.`,
   0 warnings, 0 errors, compilation confirmed non-vacuous
4. MSBuild `/t:Rebuild` with TreatWarningsAsErrors and no Nullable property — exit 0, `Build succeeded.`,
   0 warnings, 0 errors, compilation confirmed non-vacuous
5. vstest over UtilitiesCS.Test — exit 0, 4897 of 4897 passed
6. vstest over QuickFiler.Test — exit 0, 1397 of 1397 passed
7. Coverage runner — exit 0, 7218 of 7218 passed, first-party lines 85.72 percent, branches 79.89 percent

All stages passed in the single pass that completed the phase.
