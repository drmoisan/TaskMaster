# Phase 0 — Baseline Test Run (P0-T11)

Timestamp: 2026-09-08T06-48

Command: `vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll QuickFiler.Test/bin/Debug/QuickFiler.Test.dll TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /EnableCodeCoverage /InIsolation /ResultsDirectory:coverage/plan812/p0-t11 "/Logger:trx;LogFileName=p0-t11.trx" /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests"`

`vstest.console.exe` was resolved per D5 through `vswhere.exe -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`; neither executable is on `PATH`. The three assemblies are named explicitly rather than discovered by directory scan, which is what keeps `.claude/worktrees/**` copies out of the run.

EXIT_CODE: 1

ExpectedExitCode: 1

The expectation is declared because the plan accepts a red baseline whose failing set is a non-empty subset of the two-member carve-out set defined by D7 and D18. The exit code of 1 is a truthful record of that baseline, and the failure it reports is analysed below.

Output Summary:

Counters read from `coverage/plan812/p0-t11/p0-t11.trx` rather than from console text:

- Total: 6659
- Passed: 6658
- Failed: 1
- Skipped (`notExecuted`): 0
- Additionally `error`, `timeout`, `aborted`, and `inconclusive` are each 0.

BASELINE-FAILING-TESTS: UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue

The sole failing test is the D18 carve-out member `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`, so the failing set is a non-empty subset of the two-member carve-out set and the plan proceeds. D18 records the mechanism — the fixed `linkedTS.CancelAfter(500)` deadline at `UtilitiesCS/Extensions/DictionaryExtensions.cs:177`, in a file outside this plan's Write Set — and records the failure as pre-existing on this branch's base. P6-T5 covers it under the issue-780 protocol. The following diagnostics characterise it.

- Failure detail from the `.trx`: `System.Threading.Tasks.TaskCanceledException: A task was canceled.` thrown out of `UtilitiesCS.DictionaryExtensions.TryAddValuesAsync` at `UtilitiesCS/Extensions/DictionaryExtensions.cs:179`. The recorded duration of the failing execution is `00:00:04.0054213`, which is a four-second cancellation deadline elapsing rather than an assertion failing.
- Isolated re-run: the same test executed under the scoped filter `FullyQualifiedName~DictionaryExtensions_Tests&TestCategory!=LiveOutlook` over `UtilitiesCS.Test.dll` alone passed in 2 ms, with all 14 tests of that class passing and an exit code of 0.
- Second full-suite observation: the same three assemblies under the same D6 filter, run without `/EnableCodeCoverage`, produced Total 6659, Passed 6658, Failed 1, with the identical single failing test name. The failure therefore reproduced 2 out of 2 full-suite attempts on this workstation and is load-dependent rather than randomly intermittent.

The failure is pre-existing. No task of this plan has modified any source file at the time this baseline was captured; the branch carries only the preparation commit, the merge of `origin/main`, and one plan-correction commit. The test does not touch `FolderPredictor`, `StoreWrapperController`, or any other path in this plan's Write Set.

BASELINE-COVERAGE-HEADLINE: the root Cobertura figures produced by P0-T12 from this run's coverage attachments are `line-rate` 0.7363901154028049 (73.639 percent), `lines-covered` 166609, and `lines-valid` 226251.
