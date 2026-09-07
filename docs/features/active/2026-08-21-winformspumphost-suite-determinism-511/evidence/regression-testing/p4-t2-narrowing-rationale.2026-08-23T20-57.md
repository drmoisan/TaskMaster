# P4-T2 Narrowing Rationale — Why No Ten-Run Determinism Pass Is Re-Executed

Timestamp: 2026-08-23T19-06

## (a) The narrowed condition and the measured evidence of record

Remediation Finding F established that the original P4-T2 acceptance clause — each of ten TRX
recording an absolute zero failed count across all nine assemblies — spans an assembly this child
does not own. Task P2-T3 of `remediation-plan.2026-08-23T20-57.md` narrowed that clause to:

> each of those ten records zero failed tests within the `QuickFiler.Test` assembly, with both named
> end-to-end tests (`InitializeBool_ThroughThePumpHost_CompletesAndInitializesState`,
> `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates`) and both named
> regression tests (`BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread`,
> `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles`) recorded as passed in every TRX, and any
> failure in a sibling assembly recorded by fully qualified name and attributed to issue #594 rather
> than failing the task (narrowed 2026-08-23 per remediation Finding F)

The narrowed condition is satisfied by evidence already committed to this branch. The three distilled
records of record are:

| Record | What it establishes |
| --- | --- |
| `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/determinism-ten-runs.2026-08-21T18-10.md` | Nine of the ten runs report a suite-wide `failed=0`; run 5 reports `failed=1`. |
| `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/named-tests-ten-runs.2026-08-21T18-10.md` | Both named end-to-end tests recorded `Passed` in 10 of 10 runs. |
| `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/regression-tests-ten-runs.2026-08-21T18-10.md` | Both named regression tests recorded `Passed` in 10 of 10 runs. |

Run 5's single failure is
`UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`.
It sits in `UtilitiesCS.Test`, a sibling-owned assembly that this child's three-file
`QuickFiler.Test/` diff cannot reach, and it is one of the three pre-existing flakes tracked as
issue #594. Under the narrowed condition it is recorded and attributed, not treated as a gate
failure. The `QuickFiler.Test` failed count is zero in all ten runs.

## (b) The post-remediation source differs from the evidence-producing source by comment lines only

The ten-run determinism pass was executed against the source at commit `02983a70`
(`wip(511): preserve halted #511/#571 investigation`), which is the commit that introduced both the
fixture change and the distilled ten-run records. The only source edits since then are the two
comment-block corrections made by remediation tasks P1-T1 and P1-T2.

Verification command and output:

```
$ git diff --numstat 02983a70 -- \
    QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs \
    QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs \
    QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs
7       5       QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs
6       2       QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs
```

`QfcItemController.InitializationTests.Part3.cs` does not appear in the numstat output at all: it is
byte-identical to the source that produced the Phase 4 evidence.

Filtering the same diff to changed lines that are not `//` comment lines returns the empty set:

```
$ git diff 02983a70 -- <the two changed files> \
    | grep -E '^[+-]' | grep -vE '^(\+\+\+|---)' | grep -vE '^[+-]\s*//'
(no output)
```

The post-remediation source therefore differs from the source that produced the Phase 4 determinism
evidence by comment lines only. Every one of the 13 added and 7 removed lines is a `//` comment
line. No executable statement, no assertion, no timeout constant, and no test method signature
changed; in particular both `viewer.Handle` read statements are retained per orchestrator Decision 2.
A recompiled binary is behaviourally identical, so re-running the ten-run determinism pass could
produce no information the committed records do not already carry. No re-run of the ten-run
determinism pass is required, and none is performed by this cycle.

## (c) Faithfulness of the distilled record, and the raw-artifact deletion it licensed

Remediation-inputs Part 1 row 8 records that the committed distilled record
`determinism-ten-runs.2026-08-21T18-10.md` was compared against values re-derived directly from the
raw TRX (`ResultSummary/Counters` for the per-run pass and fail counts,
`UnitTestResult[@outcome='Failed']` for the failing test identity, and a per-test outcome scan for
the four owned named tests) and was confirmed identical. The distilled markdown is therefore a
faithful distillation and is the evidence of record.

That finding licensed the raw-artifact deletion already carried out. The raw ten-TRX directory
`docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2/`
**no longer exists**: its contents were deleted at maintainer instruction on 2026-08-23, together
with the other 56 raw `.trx` and 42 `.coverage` files (roughly 1,180.6 MB) and 188 empty scratch
directories. The recorded disposition is
`docs/features/active/winformspumphost-suite-determinism-511/evidence/other/raw-vstest-artifact-disposition.2026-08-23T21-40.md`.

Because that directory has been deleted, no task in this cycle asserts its existence, and it is
named in the past tense wherever it is referenced.
