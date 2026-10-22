# Supplementary Evidence — Ten-Run Confirmation Under MSBuild Node Contention

Timestamp: 2026-08-22T15-03

## This artifact is supplementary

It is **not** a plan task. It satisfies no acceptance condition in
`plan.2026-08-21T18-10.md`, changes no task's acceptance condition, and licenses no edit to the
plan. It is additive evidence recorded to close a gap between two conditions:

- **The condition the plan mandates.** P4-T1 mandates a CPU load generator, and P4-T2's ten runs
  execute under it.
- **The condition empirically observed to reproduce the defect.** The only genuine pre-fix failure
  seen anywhere in this execution was the second P0-T16 coverage invocation, which reported
  `Total tests: 6437, Passed: 6430, Failed: 7` with all seven failures being 60,000 ms
  `PumpTimeoutMs` expiries and both named tests among them. That run differed from the passing runs
  immediately either side of it in exactly one respect: **17 idle MSBuild node-reuse processes were
  resident**. Clearing them restored 6437 / 6437.

The ten mandated P4-T2 runs executed with an MSBuild node count of **0** throughout (the 17 nodes
left by the P2-T6 and P3-T3 rebuilds reached their idle timeout and exited before the completed
window began). Ten green runs at a zero node count would not have exercised the only condition known
to reproduce the defect, so the determinism gate would be weaker than it looks. These two
supplementary passes exercise it directly.

## Method

Two ten-run passes were executed with the same nine-assembly command as P4-T2, **without** the CPU
load generator (which had already been stopped by P4-T3), and with MSBuild node-reuse processes
deliberately present. Nodes exit on an idle timeout, so a single preceding build does not keep them
resident across ten runs; the runner therefore re-established them immediately before each run.

```
# before each run, if the node count had decayed below the pass's floor:
MSBuild.exe <target> /t:Build /m /p:Configuration=Debug /p:Platform=<platform> /v:q /nologo

vstest.console.exe <the nine assemblies> /EnableCodeCoverage /InIsolation /Logger:trx `
  /ResultsDirectory:<pass directory> /TestCaseFilter:"TestCategory!=LiveOutlook"
```

| Pass | Node-spawning build | Node floor | Observed node count | Results directory |
| --- | --- | --- | --- | --- |
| A | `QuickFiler.Test\QuickFiler.Test.csproj` | 8 | **3** per run | `evidence/regression-testing/supplementary-node-contention/` |
| B | `TaskMaster.sln` | 15 | **17** per run | `evidence/regression-testing/supplementary-node-contention-b/` |

Pass A was run first and reached only 3 nodes, because a single-project `/m` build spawns far fewer
nodes than a solution build. Rather than discard it, it is recorded as executed and pass B was added
to reach a node count matching the 17 observed at reproduction. Both passes are reported; neither is
a re-run of the other to obtain a better result.

Each pass directory holds exactly ten `.trx` files.

EXIT_CODE: 0 (pass A, all ten runs); 1 on two of ten runs in pass B (see the table)

## Output Summary — Pass A, 3 MSBuild nodes

| # | Total | Passed | Failed | Duration (s) | Nodes before / after | Four tracked tests |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 6439 | 6439 | 0 | 56.7 | 3 / 3 | all Passed |
| 2 | 6439 | 6439 | 0 | 76.6 | 3 / 3 | all Passed |
| 3 | 6439 | 6439 | 0 | 54.6 | 3 / 3 | all Passed |
| 4 | 6439 | 6439 | 0 | 57.0 | 3 / 3 | all Passed |
| 5 | 6439 | 6439 | 0 | 55.9 | 3 / 3 | all Passed |
| 6 | 6439 | 6439 | 0 | 54.4 | 3 / 3 | all Passed |
| 7 | 6439 | 6439 | 0 | 59.5 | 3 / 3 | all Passed |
| 8 | 6439 | 6439 | 0 | 54.3 | 3 / 3 | all Passed |
| 9 | 6439 | 6439 | 0 | 54.9 | 3 / 3 | all Passed |
| 10 | 6439 | 6439 | 0 | 54.8 | 3 / 3 | all Passed |

Ten of ten green, suite-wide.

## Output Summary — Pass B, 17 MSBuild nodes (the reproduction condition)

| # | Total | Passed | Failed | Duration (s) | Nodes before / after | Four tracked tests |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 6439 | 6438 | **1** | 57.2 | 17 / 17 | all Passed |
| 2 | 6439 | 6439 | 0 | 55.1 | 17 / 17 | all Passed |
| 3 | 6439 | 6439 | 0 | 56.9 | 17 / 17 | all Passed |
| 4 | 6439 | 6439 | 0 | 56.2 | 17 / 17 | all Passed |
| 5 | 6439 | 6439 | 0 | 56.3 | 17 / 17 | all Passed |
| 6 | 6439 | 6439 | 0 | 56.8 | 17 / 17 | all Passed |
| 7 | 6439 | 6439 | 0 | 54.3 | 17 / 17 | all Passed |
| 8 | 6439 | 6439 | 0 | 58.4 | 17 / 17 | all Passed |
| 9 | 6439 | 6438 | **1** | 55.5 | 17 / 17 | all Passed |
| 10 | 6439 | 6439 | 0 | 56.8 | 17 / 17 | all Passed |

The four tracked tests are
`InitializeBool_ThroughThePumpHost_CompletesAndInitializesState`,
`InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates`,
`BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` and
`BuildPumpHarness_DoesNotCreateTheWebViewChildHandles`. Every one of them passed in **all twenty**
supplementary runs, including both runs that recorded a suite-wide failure.

## The two pass-B failures

| Run | Failing test | Message |
| --- | --- | --- |
| 1 | `UtilitiesCS.Test.OutlookObjects.FilterDASL.DASLFilterParserTests.PrintTree_WritesIndentedTreeToConsole` | `Expected writer.ToString() "" to contain "AND".` |
| 9 | `UtilitiesCS.Test.ReusableTypeClasses.StackGeek_Tests.Main_RunsSampleScenarioWithoutThrowing` | `Expected writer.ToString() "" to contain "Middle Element :".` |

Both are in `UtilitiesCS.Test`, both assert on the contents of a redirected `Console.Out` writer,
and in both the writer was **empty**. That is the signature of a shared-`Console.Out` race between
parallel test classes: one test's redirection is displaced by another's before the assertion reads
it. Neither is a `[Timeout]` expiry, neither is a pump-harness test, and neither is reachable from
this change's diff, which is confined to three files under `QuickFiler.Test/Controllers/`.

## Finding: three independent pre-existing flaky tests in `UtilitiesCS.Test`

Across the mandated P4-T2 window and these two supplementary passes, three distinct
`UtilitiesCS.Test` tests failed intermittently, none of them related to #511:

1. `Extensions.DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`
   — `System.NullReferenceException`, in P4-T2 run 5 (under 100% CPU load).
2. `OutlookObjects.FilterDASL.DASLFilterParserTests.PrintTree_WritesIndentedTreeToConsole`
   — empty redirected `Console.Out`, in supplementary pass B run 1.
3. `ReusableTypeClasses.StackGeek_Tests.Main_RunsSampleScenarioWithoutThrowing`
   — empty redirected `Console.Out`, in supplementary pass B run 9.

Items 2 and 3 share a root cause class (shared `Console.Out` redirection under class-level
parallelization). All three are pre-existing, out-of-scope for this child issue, and are reported to
the caller for promotion rather than fixed here.

## Conclusion, stated no more strongly than the evidence supports

Under the exact condition observed to reproduce the #511 failure — 17 resident MSBuild node-reuse
processes — the two named tests and the two regression tests passed in **10 of 10** runs, and did so
again in the 3-node pass A and in the 10 mandated 100%-CPU-load runs of P4-T2. That is 30 of 30
post-fix runs with all four tracked tests green, spanning zero-node, 3-node, 17-node and
CPU-saturated conditions.

This is not a fail-before / pass-after proof, and it is not presented as one: the pre-fix
measurement in P1-T5 recorded no failure of the named tests across its twenty runs, so there is no
measured pre-fix failure rate for these conditions to be compared against. What the 30 runs
establish is that the fixture change holds under the reproduction condition, and that the residual
suite-level instability observed in this execution is located in `UtilitiesCS.Test`, not in the
pump harness.
